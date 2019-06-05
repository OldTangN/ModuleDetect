using System;
using System.Collections.Generic;
using System.Threading;
using System.IO.Ports;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;
namespace mkjcApp
{
    public class myprotocol
    {
        private myprotocol instance;
        private static readonly object lockobj = new object();//线程锁，简单的避免多线程操作同一串口
        public ModuleObj locModuleObj = new ModuleObj();//工装调度信息类
        public bool CheckingHPLC_ID = false;//正在校验HPLC_ID
        public myprotocol Instance => GetInstance();

        public Action<int, int, byte[]> DataReceived;

        public myprotocol()
        {
            ;
        }
        public myprotocol GetInstance()
        {
            if (instance == null)
            {
                lock (lockobj)
                {
                    if (instance == null)
                    {
                        instance = new myprotocol();
                    }
                }
            }
            return instance;
        }
        /**********************************************************************************
         *                   串行口操作接口函数
        ***********************************************************************************/
        const ushort APPUART_NUM = 23;
        const ushort RECBUFSIZE = 256;
        const ushort TXBUFFSIZE = 256;

        const ushort _True = 1;
        const ushort _False = 0;
        const short Ok = 0;

        ushort[] Rput = new ushort[APPUART_NUM] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        ushort[] Rget = new ushort[APPUART_NUM] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        byte[,] DmaRxBuffer = new byte[APPUART_NUM, RECBUFSIZE];

        public String HPLC_ID_String;

        public int OpenPort(SerialPort serialPort, string portName, string parityBit)
        {
            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.PortName = portName;
                    serialPort.BaudRate = 9600;

                    if (parityBit == "Even")
                        serialPort.Parity = Parity.Even;
                    else if (parityBit == "Odd")
                        serialPort.Parity = Parity.Odd;
                    else
                        serialPort.Parity = Parity.None;

                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;
                    serialPort.ReceivedBytesThreshold = 1;
                    serialPort.ReadBufferSize = 1024;
                    serialPort.ReadTimeout = 1000;
                    serialPort.WriteTimeout = 1000;



                    //serialPort.Handshake = Handshake.XOnXOff;
                    //serialPort.DtrEnable = true;



                    serialPort.RtsEnable = true;

                    serialPort.Open();//打开串口
                    return 0;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "串口未知错误");
                    return -1;
                }
            }
            else
            {
                return 0;
            }
        }

        //关闭串口
        public void ClosePort()
        {
            try
            {
                if (localSerialPort.IsOpen)
                    localSerialPort.Close();//关闭串口
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "串口未知错误");
            }
        }


        public int WritePort(SerialPort serialPort, string portName, byte[] data)
        {
            //serialPort.PortName = portName;

            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Write(data, 0, data.Length);//关闭串口
                    return 0;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "串口未知错误");
                    return -1;
                }
            }
            else
            {
                return 0;
            }
        }


        //初始化所有串口，赋值
        public int InitializePorts(int uart)
        {
            string parity = "";
            localSerialPortNum = uart;
            if (localSerialPortNum == 0)
            {
                //MessageBox.Show("串口未设置！");
                return 1;
            }
            string uartname = string.Format("COM{0:G}", localSerialPortNum);

            if (myScheme.SchemeUartCheck[uart] == 2)
                parity = "Even";
            else if (myScheme.SchemeUartCheck[uart] == 1)
                parity = "Odd";
            else
                parity = "None";
            int rtn = OpenPort(localSerialPort, uartname, parity);//注意：如果是网络映射串口，这里的奇偶校验设置不起作用，需要通过映射工具设置
            if (rtn == 0) return localSerialPortNum;
            return -1;
        }


        //写端口
        public void WritePorts(int uart, byte[] data)
        {
            localSerialPortNum = uart;
            if (localSerialPortNum == 0)
            {
                //MessageBox.Show("串口未设置！");
            }
            string uartname = string.Format("COM{0:G}", localSerialPortNum);
            WritePort(localSerialPort, uartname, data);
        }

        void WriteDataToRxbuffer(int nUart)
        {
            int i, nNum;

            if (nUart >= APPUART_NUM) return;

            nNum = localSerialPort.BytesToRead;
            if (nNum > 250) nNum = 250;
            if (nNum > 0)
            {
                localSerialPort.Read(TmpRxBuffer, 0, nNum);
                //添加到缓冲列表
                for (i = 0; i < nNum; i++)
                {
                    DmaRxBuffer[nUart, Rput[nUart]] = TmpRxBuffer[i];
                    Rput[nUart] = (ushort)((Rput[nUart] + 1) % RECBUFSIZE);
                    if (Rput[nUart] == Rget[nUart]) Rget[nUart] = (ushort)((Rget[nUart] + 1) % RECBUFSIZE);
                }
            }
        }

        int WriteComm(int nUart, byte[] pBuf, int nLen)
        {
            localSerialPort.Write(pBuf, 0, nLen);
            return 0;
        }

        ushort GetRxDataNum(int nUart)
        {
            ushort len;

            if (Rput[nUart] >= Rget[nUart])
            {
                len = (ushort)(Rput[nUart] - Rget[nUart]);
            }
            else
            {
                len = (ushort)(RECBUFSIZE - Rget[nUart] + Rput[nUart]);
            }

            return len;
        }

        ushort GetRxBuf(int nUart, byte[] Buffer, ushort Len)
        {
            ushort i, Rtnlen;
            Rtnlen = GetRxDataNum(nUart);
            if (Rtnlen > Len) Rtnlen = Len;

            for (i = 0; i < Rtnlen; i++)
            {
                Buffer[i] = DmaRxBuffer[nUart, Rget[nUart]];

                Rget[nUart] = (ushort)((Rget[nUart] + 1) % RECBUFSIZE);
            }

            return Rtnlen;
        }
        /**********************************************************************************
         *                   工装模块自定义协议
        ***********************************************************************************/
        //general constant
        //Get comm data step
        const byte S_RCV_HEAD = 0;
        const byte S_RCV_ALL_DATA = 1;
        const byte S_RCV_TIME_OUT = 2;

        const ushort MKJC_FRAME_HEADER_LEN = 7;

        //Function define
        const byte FUN_READ_STATE = 1;//
        const byte FUN_READ_ID = 2;//
        const byte FUN_SETUP_SCHEME = 3;//
        const byte FUN_INIT = 4;//
        const byte FUN_PM_CTL = 5;//
        const byte FUN_STATIC_TEST = 6;//
        const byte FUN_DYNAMIC_TEST = 7;//
        const byte FUN_ATTENUATION_TEST = 8;//
        const byte FUN_DCPOWER_TEST = 9;//
        const byte FUN_ACTIVE_RPT = 10;//
        const byte FUN_SPECIALL_TEST = 11;//
        const byte FUN_COMM_TEST = 12;//
        const byte FUN_RESULT_TEST = 13;//

        //状态字节索引定义
        const byte S_SCHEME_INI = 1;//
        const byte S_MODULE_INSERT = 2;//
        const byte S_PM_INPUT = 3;//
        const byte S_MODULE_MATCH = 4;//
        const byte S_I_COMM_RESULT = 5;//
        const byte S_I_COMM_ATTENUATION = 6;//
        const byte S_NETPORT_TEST = 7;//
        const byte S_GPRS_TEST = 8;//
        const byte S_DCPOWER_TESY = 9;//
        const byte S_STATE_TEST = 10;//
        const byte S_RESET_TEST = 11;//
        const byte S_SET_TEST = 12;//
        const byte S_STA_TEST = 13;//
        const byte S_EVENT_TEST = 14;//
        const byte S_HEATING_TEST = 15;//
        const byte S_ONOFF_TEST = 16;//
        const byte S_SYS_RST = 17;//

        ulong[] BitMap = new ulong[32] {
        0x00000001,0x00000002,0x00000004,0x00000008,
        0x00000010,0x00000020,0x00000040,0x00000080,
        0x00000100,0x00000200,0x00000400,0x00000800,
        0x00001000,0x00002000,0x00004000,0x00008000,
        0x00010000,0x00020000,0x00040000,0x00080000,
        0x00100000,0x00200000,0x00400000,0x00800000,
        0x01000000,0x02000000,0x04000000,0x08000000,
        0x10000000,0x20000000,0x40000000,0x80000000};

        ulong[] BitMask = new ulong[32]  {
        0xFFFFFFFE,0xFFFFFFFD,0xFFFFFFFB,0xFFFFFFF7,
        0xFFFFFFEF,0xFFFFFFDF,0xFFFFFFBF,0xFFFFFF7F,
        0xFFFFFEFF,0xFFFFFDFF,0xFFFFFBFF,0xFFFFF7FF,
        0xFFFFEFFF,0xFFFFDFFF,0xFFFFBFFF,0xFFFF7FFF,
        0xFFFEFFFF,0xFFFDFFFF,0xFFFBFFFF,0xFFF7FFFF,
        0xFFEFFFFF,0xFFDFFFFF,0xFFBFFFFF,0xFF7FFFFF,
        0xFEFFFFFF,0xFDFFFFFF,0xFBFFFFFF,0xF7FFFFFF,
        0xEFFFFFFF,0xDFFFFFFF,0xBFFFFFFF,0x7FFFFFFF};

        byte RecStatus;

        ushort SlaveAddr = 1;
        ushort RcvTimeoutMs = 0;
        ushort RcvNum = 0;
        ushort PreRcvNum = 0;

        int localSerialPortNum = 0;
        int MsgSize, WaitLen;
        ulong sys_second, lastsecond;

        byte[] ComRxBuf = new byte[RECBUFSIZE];
        byte[] ComTxBuf = new byte[TXBUFFSIZE];
        byte[] TmpRxBuffer = new byte[TXBUFFSIZE];


        ushort PtlReadComm = _True;
        ushort PtlWriteComm = _False;
        ushort SchemeType;

        Scheme myScheme = new Scheme();
        SerialPort localSerialPort = new SerialPort();

        public void mkjcProtocol()
        {
            //***********RECV MSG *******************
            if (localSerialPortNum == 0) return;
            WriteDataToRxbuffer(localSerialPortNum);
            if (PtlReadComm == _True)
            {
                PreRcvNum = RcvNum;
                while ((RcvNum = GetRxDataNum(localSerialPortNum)) >= WaitLen)
                {
                    switch (RecStatus)
                    {
                        case S_RCV_HEAD:
                            SearchFrameHeader(localSerialPortNum);
                            break;
                        case S_RCV_ALL_DATA:
                            RecAllFrameData(localSerialPortNum);
                            break;
                        default:
                            RecStatus = S_RCV_HEAD;
                            WaitLen = MKJC_FRAME_HEADER_LEN;
                            break;
                    }
                }

                if (RcvNum != 0)
                {
                    if (RcvNum != PreRcvNum)
                    {
                        PreRcvNum = RcvNum;
                        RcvTimeoutMs = 0;
                    }
                    else
                    {
                        RcvTimeoutMs++;
                        if (RcvTimeoutMs > 3)
                        {
                            RecStatus = S_RCV_HEAD;
                            WaitLen = MKJC_FRAME_HEADER_LEN;
                            //debug rs232
                            MsgSize = GetRxBuf(localSerialPortNum, ComRxBuf, 256);
                            RcvTimeoutMs = 0;
                        }
                    }
                }
                else
                {
                    if (sys_second != lastsecond)
                    {
                        lastsecond = sys_second;
                    }
                }
            }

            //*********TRANSMIT MSG ****************
            if (PtlWriteComm == _True) //not active yet
            {
                {
                    WriteComm(localSerialPortNum, ComTxBuf, MsgSize);

                    //SetUartStatus(PORT_MODBUS, IDLE);
                    PtlWriteComm = _False;
                    MsgSize = 0;
                }
            }
            return;
        }

        short locStartRx()
        {
            RecStatus = S_RCV_HEAD;
            WaitLen = MKJC_FRAME_HEADER_LEN;
            return Ok;
        }

        byte SUM8(byte[] buf, ushort DataLen)
        {
            int i;
            byte sum = 0;

            for (i = 0; i < DataLen; i++)
            {
                sum += (byte)buf[i];
            }
            return sum;
        }

        short ParSlaverDataFunc(int uart)//解析终端协议数据
        {
            byte ctl, Function, sumdata, datelen, StationIndex;
            ushort i, TmlAddr;
            byte[] sumbuffer = new byte[64];
            byte[] Msgbuffer = new byte[64];

            ctl = DmaRxBuffer[uart, ((Rget[uart] + 6) % RECBUFSIZE)];
            Function = (byte)(ctl & 0x0F);
            datelen = DmaRxBuffer[uart, ((Rget[uart] + 1) % RECBUFSIZE)];
            TmlAddr = DmaRxBuffer[uart, ((Rget[uart] + 5) % RECBUFSIZE)];
            TmlAddr <<= 8;
            TmlAddr |= DmaRxBuffer[uart, ((Rget[uart] + 4) % RECBUFSIZE)];

            if ((ctl & 0x80) != 0) return -1;//数据不是来自终端

            if ((ctl & 0x40) == 0)//响应帧
            {
                //返回功能码，由master处理
                for (i = 0; i < datelen; i++)
                {
                    Msgbuffer[i] = DmaRxBuffer[uart, ((Rget[uart] + i + 4) % RECBUFSIZE)];
                }
                DataReceived(uart, Function, Msgbuffer);
                if (Function == 3)
                {
                    int mm = 0;
                }
                if (Function == 14)
                {
                    int mmm = 0;
                }
                return Function;
            }

            //下面代码是对工装主动上报数据的处理
            if (Function != FUN_ACTIVE_RPT) return -1;
            StationIndex = DmaRxBuffer[uart, ((Rget[uart] + 7) % RECBUFSIZE)];


            switch (StationIndex)
            {
                case 100:

                    break;
                default:
                    for (i = 0; i < datelen; i++)
                    {
                        Msgbuffer[i] = DmaRxBuffer[uart, ((Rget[uart] + i + 4) % RECBUFSIZE)];
                    }

                    for (i = 0; i < datelen + 6; i++)
                    {
                        ComTxBuf[i] = DmaRxBuffer[uart, ((Rget[uart] + i) % RECBUFSIZE)];
                    }
                    ComTxBuf[6] = 0xAA;

                    break;
            }

            for (i = 0; i < datelen; i++)
            {
                sumbuffer[i] = ComTxBuf[i + 4];
            }

            sumdata = (byte)SUM8(sumbuffer, datelen);
            ComTxBuf[datelen + 4] = sumdata;
            ComTxBuf[datelen + 5] = 0x16;
            MsgSize = (ushort)(datelen + 6);
            /*List<byte> tempFrame = new List<byte>();
            byte[] tempbuf = tempFrame.ToArray();
            Array.Copy(tempbuf, ComTxBuf, MsgSize);*/
            PtlWriteComm = _True;
            DataReceived(uart, Function, Msgbuffer);

            return 0;
        }

        short locCheckData(int uart)
        {
            byte i, rcv_sum;
            byte local_sum = 0x00;
            byte[] sumbuffer = new byte[64];
            ushort rcvaddr;

            for (i = 0; i < WaitLen; i++)
            {
                ComRxBuf[i] = DmaRxBuffer[uart, (Rget[uart] + i) % RECBUFSIZE];
            }
            for (i = 0; i < WaitLen - 6; i++)
            {
                sumbuffer[i] = ComRxBuf[i + 4];
            }

            local_sum = (byte)SUM8(sumbuffer, (ushort)(WaitLen - 6));
            rcv_sum = (byte)((DmaRxBuffer[uart, (Rget[uart] + WaitLen - 2) % RECBUFSIZE]) & 0xFF);

            if (rcv_sum != local_sum) return -1;

            rcvaddr = DmaRxBuffer[uart, (Rget[uart] + 5) % RECBUFSIZE];
            rcvaddr <<= 8;
            rcvaddr |= DmaRxBuffer[uart, (Rget[uart] + 4) % RECBUFSIZE];
            //if (rcvaddr == 0xFFFF) return Ok;
            //if (SlaveAddr != rcvaddr) return -1;//如果地址不一致，说明初始化存在问题，或工位复位，但必须接收并处理。

            return Ok;
        }

        short SearchFrameHeader(int uart)
        {
            byte Function, bval1, bval2, bval3, bval4;

            bval1 = DmaRxBuffer[uart, Rget[uart] % RECBUFSIZE];
            bval2 = DmaRxBuffer[uart, (Rget[uart] + 1) % RECBUFSIZE];
            bval3 = DmaRxBuffer[uart, (Rget[uart] + 2) % RECBUFSIZE];
            bval4 = DmaRxBuffer[uart, (Rget[uart] + 3) % RECBUFSIZE];
            if ((bval1 == 0x68) && (bval4 == 0x68) && (bval2 == bval3))
            {
                RecStatus = S_RCV_ALL_DATA;
                WaitLen = (ushort)(bval2 + 6);
            }
            else
            {
                Rget[uart] = (ushort)((Rget[uart] + 1) % RECBUFSIZE);
                locStartRx();
            }
            return Ok;
        }

        short RecAllFrameData(int uart)
        {
            if (locCheckData(uart) == Ok)
            {
                ParSlaverDataFunc(uart);
                Rget[uart] = (ushort)((Rget[uart] + WaitLen) % RECBUFSIZE);
            }
            else
            {
                Rget[uart] = (ushort)((Rget[uart] + 1) % RECBUFSIZE);
            }
            RecStatus = S_RCV_HEAD;
            WaitLen = MKJC_FRAME_HEADER_LEN;
            return Ok;
        }

        public short SendMstCmd(int CmdType, int CmdMsg, int CmdMsg1)//解析终端协议数据
        {
            byte sumdata, datelen;
            ushort i, TmlAddr = 1;
            byte[] sumbuffer = new byte[64];
            byte[] SetNormalCmd = new byte[] { 0x68, 0x03, 0x03, 0x68, 0x01, 0x00, 0xC8, 0xC9, 0x16 };

            byte[] ReadIDCmd = new byte[] { 0x68, 0x04, 0x04, 0x68, 0x01, 0x00, 0xC2, 0x00, 0xC3, 0x16 };
            byte[] SetSchemeCmd = new byte[] { 0x68, 0x06, 0x06, 0x68, 0x01, 0x00, 0xC3, 0x01, 0x00, 0x00, 0xC5, 0x16 };
            byte[] SetGPMPCmd = new byte[] { 0x68, 0x04, 0x04, 0x68, 0x01, 0x00, 0xC5, 0x01, 0xC7, 0x16 };
            byte[] SetIzbsjCommCmd = new byte[] { 0x68, 0x03, 0x03, 0x68, 0x01, 0x00, 0xC8, 0xC9, 0x16 };
            byte[] SetIzbCommCmd = new byte[] { 0x68, 0x04, 0x04, 0x68, 0x01, 0x00, 0xCC, 0x08, 0xD5, 0x16 };
            byte[] JIARECmd = new byte[] { 0x68, 0x04, 0x04, 0x68, 0x01, 0x00, 0xCB, 0x08, 0xD4, 0x16 };
            byte[] SetGwAddrCmd = new byte[] { 0x68, 0x05, 0x05, 0x68, 0xFF, 0xFF, 0xCE, 0x01, 0x00, 0xD0, 0x16 };
            byte[] SetLedCmd = new byte[] { 0x68, 0x04, 0x04, 0x68, 0x01, 0x00, 0xCD, 0x00, 0xCE, 0x16 };

            int zwIndex = myScheme.SchemeUart[localSerialPortNum];
            SetNormalCmd[6] = (byte)(CmdType | 0xC0);
            switch (CmdType)
            {
                case 1://读取状态
                    break;
                case 2:
                    for (i = 0; i < ReadIDCmd.Length; i++)
                    {
                        ComTxBuf[i] = ReadIDCmd[i];
                    }
                    break;
                case 3://设置方案
                    SetSchemeCmd[7] = (byte)CmdMsg;
                    SetSchemeCmd[8] = (byte)(CmdMsg1 & 0xFF);
                    SetSchemeCmd[9] = (byte)(CmdMsg1 >> 8);
                    for (i = 0; i < SetSchemeCmd.Length; i++)
                    {
                        ComTxBuf[i] = SetSchemeCmd[i];
                    }
                    break;
                case 5://功率测试仪控制
                    SetGPMPCmd[7] = (byte)CmdMsg;
                    for (i = 0; i < SetGPMPCmd.Length; i++)
                    {
                        ComTxBuf[i] = SetGPMPCmd[i];
                    }
                    break;
                case 4://测试初始化

                case 6://静态功耗测试通知 
                case 7://动态功耗测试通知
                case 8://衰减测试指令  
                case 9://电源波动测试通知
                    for (i = 0; i < SetNormalCmd.Length; i++)
                    {
                        ComTxBuf[i] = SetNormalCmd[i];
                    }
                    break;
                case 11://加热测试
                    for (i = 0; i < JIARECmd.Length; i++)
                    {
                        ComTxBuf[i] = JIARECmd[i];
                    }
                    break;
                case 12://通信测试
                    for (i = 0; i < SetIzbCommCmd.Length; i++)
                    {
                        ComTxBuf[i] = SetIzbCommCmd[i];
                    }
                    break;
                case 13://控制终端状态灯
                    SetLedCmd[7] = (byte)CmdMsg;
                    for (i = 0; i < SetLedCmd.Length; i++)
                    {
                        ComTxBuf[i] = SetLedCmd[i];
                    }
                    break;
                case 14://工位地址设置
                    SlaveAddr = (ushort)CmdMsg;
                    SetGwAddrCmd[7] = (byte)(SlaveAddr & 0xFF);
                    SetGwAddrCmd[8] = (byte)(SlaveAddr >> 8);
                    for (i = 0; i < SetGwAddrCmd.Length; i++)
                    {
                        ComTxBuf[i] = SetGwAddrCmd[i];
                    }
                    break;
                default:
                    return 0;
            }

            //ComTxBuf[4] = (byte)(SlaveAddr & 0xFF);
            //ComTxBuf[5] = (byte)(SlaveAddr >> 8);
            ComTxBuf[4] = (byte)(TmlAddr & 0xFF);
            ComTxBuf[5] = (byte)(TmlAddr >> 8);

            if (CmdType == 14)
            {
                ComTxBuf[4] = 0xFF;
                ComTxBuf[5] = 0xFF;
            }

            datelen = ComTxBuf[1];
            for (i = 0; i < datelen; i++)
            {
                sumbuffer[i] = ComTxBuf[i + 4];
            }

            sumdata = (byte)SUM8(sumbuffer, datelen);
            ComTxBuf[datelen + 4] = sumdata;
            ComTxBuf[datelen + 5] = 0x16;
            MsgSize = (ushort)(datelen + 6);
            PtlWriteComm = _True;
            return 0;
        }

        public short ObjSetup(int uart)//用于协议运行参数设置
        {
            return 0;
        }

        /**********************************************************************************
         *                   功率测试仪控制协议
         *    操作步骤如下：
         *    初始化过程：
         *    1.主进程启动设置远程模式，读取通信模式，确认远程模式设置成功
         *    2.自动设置电压量程，读取电压量程，确认电压量程设置成功
         *    3.自动设置电流量程，读取电流量程，确认电流量程设置成功
         *    
         *    静态测试过程
         *    1.主进程启动设置锁存方式为OFF，读取锁存方式，确认为OFF
         *    2.主进程读取测量值
         *    
         *    动态测试过程
         *    1.主进程启动设置锁存方式为ON，读取锁存方式，确认为ON
         *    2.主站读取冻结值
         *    3.主站启动设置锁存方式为OFF，读取锁存方式，确认为OFF
        ***********************************************************************************/
        const byte GPMP_NULL = 0;//
        const byte GPMP_INIT_STEP = 1;//
        const byte GPMP_READ_RMT_STEP = 2;//
        const byte GPMP_SET_VOLTAGE_STEP = 3;//
        const byte GPMP_READ_VOLTAGE_STEP = 4;//
        const byte GPMP_SET_CURRENT_STEP = 5;//
        const byte GPMP_READ_CURRENT_STEP = 6;//

        const byte GPMP_SET_HOLDOFF_STEP = 7;//
        const byte GPMP_SET_HOLDON_STEP = 8;//
        const byte GPMP_READ_HOLD_STEP = 9;//
        const byte GPMP_READ_VALUE_STEP = 10;//
        const byte GPMP_SET_MODE_STEP = 11;//

        private int GPMPStep = 0;
        private int GPMPWaittimes = 0;
        public void GPMProtocol()
        {
            if (GPMPWaittimes != 0) GPMPWaittimes--;
            //***********RECV MSG *******************
            if (localSerialPortNum == 0) return;
            WriteDataToRxbuffer(localSerialPortNum);
            if (PtlReadComm == _True)
            {
                PreRcvNum = RcvNum;
                RcvNum = GetRxDataNum(localSerialPortNum);
                if (RcvNum != 0)
                {
                    if (RcvNum != PreRcvNum)
                    {
                        PreRcvNum = RcvNum;
                        RcvTimeoutMs = 0;
                    }
                    else
                    {
                        RcvTimeoutMs++;
                        if (RcvTimeoutMs > 3)
                        {
                            //debug rs232
                            MsgSize = GetRxBuf(localSerialPortNum, ComRxBuf, 256);
                            string str = Convert.ToBase64String(ComRxBuf);
                            GPMPParRtnMsgProc(ComRxBuf, MsgSize);
                            RcvTimeoutMs = 0;
                        }
                    }
                }
                else
                {
                    GPMPMasterSendProc();
                }
            }

            //*********TRANSMIT MSG ****************
            if (PtlWriteComm == _True) //not active yet
            {
                {
                    WriteComm(localSerialPortNum, ComTxBuf, MsgSize);
                    PtlWriteComm = _False;
                    MsgSize = 0;
                }
            }
            return;
        }

        public void GPMControl(int cmd)
        {
            GPMPStep = cmd;
        }
        void GPMPMasterSendProc()
        {
            String cmdstr;
            byte[] byteArray;
            if (GPMPWaittimes > 0) return;
            switch (GPMPStep)
            {
                case GPMP_INIT_STEP:
                    cmdstr = ":COMMUNICATE:REMote ON\r\n";
                    GPMPWaittimes = 10;
                    GPMPStep = GPMP_SET_MODE_STEP;
                    break;
                case GPMP_SET_MODE_STEP:
                    cmdstr = ":INPUT:MODE DC\r\n";
                    GPMPWaittimes = 10;
                    GPMPStep = GPMP_READ_RMT_STEP;
                    break;
                case GPMP_READ_RMT_STEP:
                    cmdstr = ":COMMUNICATE:REMote?\r\n";
                    GPMPWaittimes = 10;
                    break;
                case GPMP_SET_VOLTAGE_STEP:
                    cmdstr = ":INPUT:VOLTAGE:RANGE 15V\r\n";
                    GPMPWaittimes = 10;
                    GPMPStep = GPMP_READ_VOLTAGE_STEP;
                    break;
                case GPMP_READ_VOLTAGE_STEP:
                    cmdstr = ":INPut:VOLTage:RANGe?\r\n";
                    GPMPWaittimes = 10;
                    break;
                case GPMP_SET_CURRENT_STEP:
                    cmdstr = ":INPUT:CURRENT:RANGE 0.5A\r\n";
                    GPMPWaittimes = 10;
                    GPMPStep = GPMP_READ_CURRENT_STEP;
                    break;
                case GPMP_READ_CURRENT_STEP:
                    cmdstr = ":INPUT:CURRENT:RANGE?\r\n";
                    GPMPWaittimes = 10;
                    break;
                case GPMP_SET_HOLDOFF_STEP:
                    cmdstr = ":MEASURE:MHOLD OFF\r\n";
                    GPMPWaittimes = 10;
                    GPMPStep = GPMP_READ_HOLD_STEP;
                    break;
                case GPMP_SET_HOLDON_STEP:
                    cmdstr = ":MEASURE:MHOLD ON\r\n";
                    GPMPWaittimes = 10;
                    GPMPStep = GPMP_READ_HOLD_STEP;
                    break;
                case GPMP_READ_HOLD_STEP:
                    cmdstr = ":MEASURE:MHOLD?\r\n";
                    GPMPWaittimes = 10;
                    break;
                case GPMP_READ_VALUE_STEP:
                    cmdstr = ":NUMERIC:NORMAL:VALUE?\r\n";
                    GPMPWaittimes = 10;
                    break;

                default:
                    return;
            }
            byteArray = System.Text.Encoding.Default.GetBytes(cmdstr);
            Array.Copy(byteArray, ComTxBuf, byteArray.Length);
            MsgSize = byteArray.Length;
            PtlWriteComm = _True;
            return;
        }
        void GPMPParRtnMsgProc(byte[] RcvBuf, int RcvLen)
        {
            int RtnOk = -1;
            String rcvstr;
            rcvstr = System.Text.Encoding.Default.GetString(RcvBuf);
            switch (GPMPStep)
            {
                case GPMP_READ_RMT_STEP:
                    RtnOk = rcvstr.IndexOf(":COMM:REM 1");
                    if (RtnOk >= 0)
                        GPMPStep = GPMP_SET_VOLTAGE_STEP;
                    break;
                case GPMP_READ_VOLTAGE_STEP:
                    RtnOk = rcvstr.IndexOf(":VOLT:RANG 15");
                    if (RtnOk >= 0)
                        GPMPStep = GPMP_SET_CURRENT_STEP;
                    break;
                case GPMP_READ_CURRENT_STEP:
                    RtnOk = rcvstr.IndexOf(":CURR:RANG");
                    if (RtnOk >= 0)
                    {
                        GPMPStep = GPMP_NULL;
                        DataReceived(localSerialPortNum, GPMP_READ_CURRENT_STEP, null);
                    }
                    break;
                case GPMP_READ_HOLD_STEP:
                    RtnOk = rcvstr.IndexOf(":MEAS:MHOL 1");
                    if (RtnOk >= 0)
                    {
                        GPMPStep = GPMP_NULL;
                        DataReceived(localSerialPortNum, GPMP_SET_HOLDON_STEP, null);
                    }
                    RtnOk = rcvstr.IndexOf(":MEAS:MHOL 0");
                    if (RtnOk >= 0)
                    {
                        GPMPStep = GPMP_NULL;
                        DataReceived(localSerialPortNum, GPMP_SET_HOLDOFF_STEP, null);
                    }
                    break;
                case GPMP_READ_VALUE_STEP:
                    String[] arr = rcvstr.Split(',');
                    if (arr.Length < 3) break;
                    //String []item1 = arr[2].Split('E');
                    //String str = System.Text.RegularExpressions.Regex.Replace(item1[0], @"[^\d.\d]", "");
                    // 如果是数字，则转换为decimal类型
                    GPMPStep = GPMP_NULL;
                    decimal result = ChangeDataToDec(arr[2]);
                    byte[] tembyte = pubclass.ToBytes(result);
                    //if(locModuleObj.CurrentCheckItem == 2)
                    DataReceived(localSerialPortNum, GPMP_READ_VALUE_STEP, tembyte);//静态功耗
                                                                                    // else
                                                                                    // DataReceived(localSerialPortNum, 11, tembyte);//d
                    break;

                default:
                    return;
            }
            //if (RtnOk >= 0) GPMPWaittimes = 0;
            return;
        }
        private Decimal ChangeDataToDec(string strData)
        {
            Decimal dData = 0.0M;
            if (strData.Contains("E"))
            {
                dData = Decimal.Parse(strData, System.Globalization.NumberStyles.Float);
            }
            return dData;
        }
        /**********************************************************************************
         *                   直流源控制协议
        ***********************************************************************************/
        const byte IVYT_NULL = 0;//
        const byte IVYT_SET_REM_STEP = 1;//
        const byte IVYT_SET_11V_STEP = 2;//
        const byte IVYT_SET_12V_STEP = 3;//
        const byte IVYT_SET_13V_STEP = 4;//
        const byte IVYT_READ_CFGVOLT_STEP = 5;//

        const byte IVYT_READ_MEASVOLT_STEP = 6;//
        const byte IVYT_PW_ON_STEP = 7;//
        const byte IVYT_PW_OFF_STEP = 8;//

        const byte IVYT_READ_CFGVOLT_STEP1 = 9;//
        const byte IVYT_SETCFG_12V_STEP = 10;//

        int WaitCfmVolValue = 0;
        int IVYTWaittimes = 0;
        public void IVYTProtocol()
        {
            if (IVYTWaittimes != 0) IVYTWaittimes--;
            if (localSerialPortNum == 0) return;
            WriteDataToRxbuffer(localSerialPortNum);
            if (PtlReadComm == _True)
            {
                PreRcvNum = RcvNum;
                RcvNum = GetRxDataNum(localSerialPortNum);
                if (RcvNum != 0)
                {
                    if (RcvNum != PreRcvNum)
                    {
                        PreRcvNum = RcvNum;
                        RcvTimeoutMs = 0;
                    }
                    else
                    {
                        RcvTimeoutMs++;
                        if (RcvTimeoutMs > 3)
                        {
                            //debug rs232
                            MsgSize = GetRxBuf(localSerialPortNum, ComRxBuf, 256);
                            IVYTParRtnMsgProc(ComRxBuf, MsgSize);
                            RcvTimeoutMs = 0;
                        }
                    }
                }
                else
                {
                    IVYTMasterSendProc();
                }
            }

            //*********TRANSMIT MSG ****************
            if (PtlWriteComm == _True) //not active yet
            {
                {
                    WriteComm(localSerialPortNum, ComTxBuf, MsgSize);
                    PtlWriteComm = _False;
                    MsgSize = 0;
                }
            }
            return;
        }
        public void IVYTControl(int cmd)
        {
            GPMPStep = cmd;
        }

        void IVYTMasterSendProc()
        {
            String cmdstr;
            byte[] byteArray;
            if (IVYTWaittimes > 0) return;
            switch (GPMPStep)
            {
                case IVYT_SET_REM_STEP:
                    cmdstr = "[:]\r\nSYST:REM\r\n";
                    IVYTWaittimes = 20;
                    GPMPStep = IVYT_SETCFG_12V_STEP;
                    break;
                case IVYT_SET_11V_STEP:
                    cmdstr = "[:]\r\nVOLT 11.000\r\n";
                    WaitCfmVolValue = 11;
                    IVYTWaittimes = 20;
                    GPMPStep = IVYT_READ_MEASVOLT_STEP;
                    break;
                case IVYT_SET_12V_STEP:
                    cmdstr = "[:]\r\nVOLT 12.000\r\n";
                    WaitCfmVolValue = 12;
                    IVYTWaittimes = 20;
                    GPMPStep = IVYT_READ_MEASVOLT_STEP;
                    break;
                case IVYT_SET_13V_STEP:
                    cmdstr = "[:]\r\nVOLT 13.000\r\n";
                    WaitCfmVolValue = 13;
                    IVYTWaittimes = 20;
                    GPMPStep = IVYT_READ_MEASVOLT_STEP;
                    break;
                case IVYT_READ_CFGVOLT_STEP:
                    cmdstr = ":\r\nVOLT?\r\n";
                    IVYTWaittimes = 50;
                    //GPMPStep = IVYT_READ_CFGVOLT_STEP1;
                    break;
                case IVYT_READ_CFGVOLT_STEP1:
                    cmdstr = "VOLT?\r\n";
                    IVYTWaittimes = 50;
                    //GPMPStep = IVYT_READ_CFGVOLT_STEP;
                    break;
                case IVYT_READ_MEASVOLT_STEP:
                    cmdstr = "[:]\r\nMEAS:VOLT?\r\n";
                    IVYTWaittimes = 20;
                    break;
                case IVYT_PW_ON_STEP:
                    cmdstr = "[:]\r\nOUTP 1\r\n";
                    IVYTWaittimes = 20;
                    GPMPStep = IVYT_READ_MEASVOLT_STEP;
                    break;
                case IVYT_PW_OFF_STEP:
                    cmdstr = "[:]\r\nOUTP 0\r\n";
                    IVYTWaittimes = 20;
                    GPMPStep = IVYT_READ_MEASVOLT_STEP;
                    break;
                case IVYT_SETCFG_12V_STEP:
                    cmdstr = "[:]\r\nVOLT 12.000\r\n";
                    WaitCfmVolValue = 12;
                    IVYTWaittimes = 20;
                    GPMPStep = IVYT_READ_CFGVOLT_STEP;
                    break;
                default:
                    return;
            }
            byteArray = System.Text.Encoding.Default.GetBytes(cmdstr);
            Array.Copy(byteArray, ComTxBuf, byteArray.Length);
            MsgSize = byteArray.Length;
            PtlWriteComm = _True;
            return;
        }
        void IVYTParRtnMsgProc(byte[] RcvBuf, int RcvLen)
        {
            int RtnOk = -1;
            String rcvstr, str;
            rcvstr = System.Text.Encoding.Default.GetString(RcvBuf);
            switch (GPMPStep)
            {
                case IVYT_READ_CFGVOLT_STEP:
                    str = System.Text.RegularExpressions.Regex.Replace(rcvstr, @"[^\d.\d]", "");
                    if (str == "") break;
                    if (System.Text.RegularExpressions.Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                    {
                        decimal result = decimal.Parse(str);
                        GPMPStep = IVYT_NULL;
                        byte[] tembyte = pubclass.ToBytes(result);
                        DataReceived(localSerialPortNum, IVYT_READ_CFGVOLT_STEP, tembyte);
                    }
                    break;
                case IVYT_READ_MEASVOLT_STEP:
                    str = System.Text.RegularExpressions.Regex.Replace(rcvstr, @"[^\d.\d]", "");
                    if (str == "") break;
                    if (System.Text.RegularExpressions.Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                    {
                        decimal result = decimal.Parse(str);
                        GPMPStep = IVYT_NULL;
                        byte[] tembyte = pubclass.ToBytes(result);
                        DataReceived(localSerialPortNum, IVYT_READ_MEASVOLT_STEP, tembyte);
                    }
                    break;
                default:
                    return;
            }

            if (RtnOk >= 0) IVYTWaittimes = 0;
            return;
        }

        /**********************************************************************************
         *                   DLT645协议  集中器读取电能表数据
        ***********************************************************************************/
        private int Send645CmdNum = 0;
        private int Send698CmdNum = 0;
        private int ProtocolType = 0;//1=645  2=698
        private int RcvCmdNum = 0;
        private int Waittimes = 0;
        private int CommgwIndex = 0;
        public void DLT645Protocol()
        {
            //***********RECV MSG *******************
            if (Waittimes != 0) Waittimes--;
            if (localSerialPortNum == 0) return;
            WriteDataToRxbuffer(localSerialPortNum);
            if (PtlReadComm == _True)
            {
                PreRcvNum = RcvNum;
                while ((RcvNum = GetRxDataNum(localSerialPortNum)) >= WaitLen)
                {
                    switch (RecStatus)
                    {
                        case S_RCV_HEAD:
                            SearchFrameHeader645(localSerialPortNum);
                            break;
                        case S_RCV_ALL_DATA:
                            RecAllFrameData645(localSerialPortNum);
                            break;
                        default:
                            RecStatus = S_RCV_HEAD;
                            WaitLen = MKJC_FRAME_HEADER_LEN;
                            break;
                    }
                }

                if (RcvNum != 0)
                {
                    if (RcvNum != PreRcvNum)
                    {
                        PreRcvNum = RcvNum;
                        RcvTimeoutMs = 0;
                    }
                    else
                    {
                        RcvTimeoutMs++;
                        if (RcvTimeoutMs > 3)
                        {
                            RecStatus = S_RCV_HEAD;
                            WaitLen = MKJC_FRAME_HEADER_LEN;

                            //debug rs232
                            MsgSize = GetRxBuf(localSerialPortNum, ComRxBuf, 256);

                            RcvTimeoutMs = 0;
                        }
                    }
                }
                else
                {
                    if ((Waittimes == 0) && (Send645CmdNum > 0))
                    {
                        Send645CmdNum--;
                        Waittimes = 100;//大约1s
                        MasterAskAcc645(localSerialPortNum);
                    }

                    if ((Waittimes == 0) && (Send698CmdNum > 0))
                    {
                        Send698CmdNum--;
                        Waittimes = 100;//大约1s
                        MasterAskAcc698(localSerialPortNum);
                    }
                }
            }

            //*********TRANSMIT MSG ****************
            if (PtlWriteComm == _True) //not active yet
            {
                {
                    WriteComm(localSerialPortNum, ComTxBuf, MsgSize);

                    //SetUartStatus(PORT_MODBUS, IDLE);
                    PtlWriteComm = _False;
                    MsgSize = 0;
                }
            }
            return;
        }
        public void SetSend645Num(int gwIndex, int num)
        {
            if (gwIndex < 10)
                CommgwIndex = gwIndex;
            else
                CommgwIndex = 0x10;
            Send645CmdNum = num;
            RcvCmdNum = 0;
            Waittimes = 0;
            ProtocolType = 1;
        }

        public void SetSend698Num(int gwIndex, int num)
        {
            if (gwIndex < 10)
                CommgwIndex = gwIndex;
            else
                CommgwIndex = 0x10;
            Send698CmdNum = num;
            RcvCmdNum = 0;
            Waittimes = 0;
            ProtocolType = 2;
        }

        int locStartRx645()
        {
            RecStatus = S_RCV_HEAD;
            WaitLen = 10;
            return Ok;
        }

        int locCheckData645(int uart)
        {
            int i;
            byte rcv_sum;
            byte local_sum = 0x00;

            for (i = 0; i < WaitLen; i++)
            {
                ComRxBuf[i] = DmaRxBuffer[uart, (Rget[uart] + i) % RECBUFSIZE];
            }
            local_sum = SUM8(ComRxBuf, (ushort)(WaitLen - 2));
            rcv_sum = DmaRxBuffer[uart, (Rget[uart] + WaitLen - 2) % RECBUFSIZE];
            //debugprintf((char *)ComRxBuf, WaitLen);
            if (rcv_sum == local_sum) return Ok;
            else return 1;
        }

        int SearchFrameHeader645(int uart)
        {
            if ((DmaRxBuffer[uart, Rget[uart] % RECBUFSIZE] == 0x68) && (DmaRxBuffer[uart, (Rget[uart] + 7) % RECBUFSIZE] == 0x68))
            {
                RecStatus = S_RCV_ALL_DATA;
                WaitLen = DmaRxBuffer[uart, (Rget[uart] + 9) % RECBUFSIZE] + 12;
            }
            else
            {
                Rget[uart] = (ushort)((Rget[uart] + 1) % RECBUFSIZE);
                locStartRx645();
            }
            return Ok;
        }

        int RecAllFrameData645(int uart)
        {
            if (locCheckData645(uart) == Ok)
            {
                if (ProtocolType == 2)
                    ParSlaverFunc698(uart);
                else
                    ParSlaverFunc645(uart);

                Rget[uart] = (ushort)((Rget[uart] + WaitLen) % RECBUFSIZE);
            }
            else
            {
                Rget[uart] = (ushort)((Rget[uart] + 1) % RECBUFSIZE);
            }
            locStartRx645();
            return Ok;
        }
        void ParSlaverFunc645(int uart)
        {
            byte Datalen, DI0, DI1, DI2, DI3, SlaverAddr;
            byte[] Msgbuffer = new byte[64];

            SlaverAddr = DmaRxBuffer[uart, (Rget[uart] + 1) % RECBUFSIZE];
            Datalen = (byte)(DmaRxBuffer[uart, (Rget[uart] + 9) % RECBUFSIZE] + 12);

            DI0 = (byte)(DmaRxBuffer[uart, (Rget[uart] + 10) % RECBUFSIZE] - 0x33);
            DI1 = (byte)(DmaRxBuffer[uart, (Rget[uart] + 11) % RECBUFSIZE] - 0x33);
            DI2 = (byte)(DmaRxBuffer[uart, (Rget[uart] + 12) % RECBUFSIZE] - 0x33);
            DI3 = (byte)(DmaRxBuffer[uart, (Rget[uart] + 13) % RECBUFSIZE] - 0x33);

            if ((DI3 == 0) && (DI2 == 1) && (DI1 == 0) && (DI0 == 0))
            {
                RcvCmdNum++;
                DataReceived(uart, RcvCmdNum, null);
            }
            if ((DI3 == 1) && (DI2 == 1) && (DI1 == 1) && (DI0 == 1))
            {//获取单三相模块内部ID
                if (Datalen >= 28)
                {
                    for (int i = 0; i < WaitLen; i++)
                    {
                        Msgbuffer[i] = DmaRxBuffer[uart, (Rget[uart] + i) % RECBUFSIZE];
                    }
                    DataReceived(uart, 0xFF, Msgbuffer);
                }
            }
            return;
        }

        void ParSlaverFunc698(int uart)
        {
            byte Datalen, DI0, DI1, DI2, DI3, SlaverAddr;
            byte[] Msgbuffer = new byte[64];

            SlaverAddr = DmaRxBuffer[uart, (Rget[uart] + 1) % RECBUFSIZE];
            Datalen = (byte)(DmaRxBuffer[uart, (Rget[uart] + 9) % RECBUFSIZE] + 12);

            DI0 = (byte)(DmaRxBuffer[uart, (Rget[uart] + 10) % RECBUFSIZE] - 0x33);
            DI1 = (byte)(DmaRxBuffer[uart, (Rget[uart] + 11) % RECBUFSIZE] - 0x33);

            if ((DI1 == 0x90) && (DI0 == 0x10))
            {
                RcvCmdNum++;
                DataReceived(uart, RcvCmdNum, null);
            }

            return;
        }

        void MasterAskAcc645(int uart)
        {
            int i;
            byte sumdata, datelen;
            //byte[] myAddr = new byte[6] { 0x78, 0x56, 0, 0, 0, 0 };
            byte[] myAddr = new byte[6] { 0, 0, 0, 0, 0, 0 };

            //byte addr = (byte)myScheme.SchemeUart[uart];
            myAddr[0] = (byte)CommgwIndex;

            ComTxBuf[0] = 0x68;
            for (i = 0; i < 6; i++)
            {
                ComTxBuf[i + 1] = myAddr[i];
            }
            ComTxBuf[7] = 0x68;
            ComTxBuf[8] = 0x11;
            ComTxBuf[9] = 0x04;
            ComTxBuf[10] = 0x33;
            ComTxBuf[11] = 0x33;
            ComTxBuf[12] = 0x34;
            ComTxBuf[13] = 0x33;

            datelen = (byte)(ComTxBuf[9] + 10);
            sumdata = (byte)SUM8(ComTxBuf, datelen);
            ComTxBuf[14] = sumdata;
            ComTxBuf[15] = 0x16;
            MsgSize = (ushort)(datelen + 2);//----------------
            PtlWriteComm = _True;
            return;
        }

        void MasterAskAcc698(int uart)
        {
            int i;
            byte sumdata, datelen;
            //byte[] myAddr = new byte[6] { 0x78, 0x56, 0, 0, 0, 0 };
            byte[] myAddr = new byte[6] { 0, 0, 0, 0, 0, 0 };

            //byte addr = (byte)myScheme.SchemeUart[uart];
            myAddr[0] = (byte)CommgwIndex;

            ComTxBuf[0] = 0x68;
            for (i = 0; i < 6; i++)
            {
                ComTxBuf[i + 1] = myAddr[i];
            }
            ComTxBuf[7] = 0x68;
            ComTxBuf[8] = 0x01;
            ComTxBuf[9] = 0x02;
            ComTxBuf[10] = 0x43;
            ComTxBuf[11] = 0xC3;

            datelen = (byte)(ComTxBuf[9] + 10);
            sumdata = (byte)SUM8(ComTxBuf, datelen);
            ComTxBuf[12] = sumdata;
            ComTxBuf[13] = 0x16;
            MsgSize = (ushort)(datelen + 2);//----------------
            PtlWriteComm = _True;
            return;
        }

        public void MasterAskID645(int gwIndex)
        {
            int i;
            byte sumdata, datelen;
            //byte[] myAddr = new byte[6] { 0x78, 0x56, 0, 0, 0, 0 };
            byte[] myAddr = new byte[6] { 0, 0, 0, 0, 0, 0 };

            if (gwIndex < 10)
                myAddr[0] = (byte)gwIndex;
            else
                myAddr[0] = 0x10;

            ComTxBuf[0] = 0x68;
            for (i = 0; i < 6; i++)
            {
                ComTxBuf[i + 1] = myAddr[i];
            }
            ComTxBuf[7] = 0x68;
            ComTxBuf[8] = 0x11;
            ComTxBuf[9] = 0x04;
            ComTxBuf[10] = 0x34;
            ComTxBuf[11] = 0x34;
            ComTxBuf[12] = 0x34;
            ComTxBuf[13] = 0x34;

            datelen = (byte)(ComTxBuf[9] + 10);
            sumdata = (byte)SUM8(ComTxBuf, datelen);
            ComTxBuf[14] = sumdata;
            ComTxBuf[15] = 0x16;
            MsgSize = (ushort)(datelen + 2);//----------------
            PtlWriteComm = _True;
            return;
        }

        /**********************************************************************************
         *                   16路网络继电器控制
        ***********************************************************************************/
        public string IP = "192.168.0.150";
        public int PORT = 150;
        string command = "";
        TcpClient client = new TcpClient();//控制继电器的TCP客户端连接
        NetworkStream networkStream;

        public void NET16IOProtocol()
        {
            byte[] readstream = new byte[64];
            if (PtlWriteComm == _True)
            {
                byte[] stream = Encoding.UTF8.GetBytes(command);
                networkStream.Write(stream, 0, stream.Length);
                PtlWriteComm = _False;
            }

            if (networkStream.DataAvailable)
            {
                int size = readstream.Length;
                networkStream.Read(readstream, 0, size);
                string str = Encoding.UTF8.GetString(readstream, 0, readstream.Length);
                int RtnOk = str.IndexOf("ok");
                //if (str.Split(':')[1].Split(';')[0] == "ok")
                if (RtnOk >= 0)
                {
                    DataReceived(23, 1, null);
                }
            }
        }
        public int InitializeNet16()
        {
            try
            {
                if (!client.Connected)
                {
                    client.Connect(IPAddress.Parse(IP), PORT);
                }
                networkStream = client.GetStream();
            }
            catch(Exception ex)
            {
                return -1;
            }
            return 0;
        }
        public int RelayClose(int RelayIndex)
        {
            PtlWriteComm = _True;
            command = "write-relay:relay" + RelayIndex + "=1;";
            return 0;
        }
        public int RelayTrip(int RelayIndex)
        {
            PtlWriteComm = _True;
            command = "write-relay:relay" + RelayIndex + "=0;";
            return 0;
        }

        /**********************************************************************************
         *                   4路网络继电器控制
        ***********************************************************************************/
        public string IP1 = "192.168.0.151";
        public int PORT1 = 151;
        string command1 = "";
        TcpClient client1 = new TcpClient();//控制继电器的TCP客户端连接
        NetworkStream networkStream1;

        public void NET4IOProtocol()
        {
            byte[] readstream = new byte[64];
            if (PtlWriteComm == _True)
            {
                byte[] stream = Encoding.UTF8.GetBytes(command1);
                networkStream1.Write(stream, 0, stream.Length);
                PtlWriteComm = _False;
            }

            if (networkStream1.DataAvailable)
            {
                int size = readstream.Length;
                networkStream1.Read(readstream, 0, size);
                string str = Encoding.UTF8.GetString(readstream, 0, readstream.Length);
                int RtnOk = str.IndexOf("ok");
                //if (str.Split(':')[1].Split(';')[0] == "ok")
                if (RtnOk >= 0)
                {
                    DataReceived(24, 1, null);
                }
            }
        }
        public int InitializeNet4()
        {
            if (!client1.Connected)
            {
                client1.Connect(IPAddress.Parse(IP1), PORT1);
            }
            networkStream1 = client1.GetStream();
            return 0;
        }
        public int RelayClose1(int RelayIndex)
        {
            PtlWriteComm = _True;
            command1 = "write-relay:relay" + RelayIndex + "=1;";
            return 0;
        }
        public int RelayTrip1(int RelayIndex)
        {
            PtlWriteComm = _True;
            command1 = "write-relay:relay" + RelayIndex + "=0;";
            return 0;
        }
    }
}

