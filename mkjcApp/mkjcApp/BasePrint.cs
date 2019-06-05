using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace test214
{
    public class RETURN_CODE
    {
        public const string num_1 = "固件解析器忙！";
        public const string _1 = "固件解析器闲置！";
        public const string num_2 = "纸张报错！";
        public const string _2 = "纸张已准备好！";
        public const string num_4 = "墨带报错！";
        public const string _4 = "墨带已准备好！";
        public const string num_8 = "打印批处理文件！";
        public const string _8 = "其它！";
        public const string num_16 = "打印机正在打印！";
        public const string _16 = "打印机不在打印状态！";
        public const string num_32 = "打印机暂停，等待下一次任务！";
        public const string _32 = "打印机没有停顿！";
        public const string num_64 = "有标签！";
        public const string _64 = "没有标签！";
        public const string num_128 = "应该为0，不可能为1！";
        public const string _128 = "应该为0，正常！";
    }

    public enum FjSelect
    {
        HG = 0,//合格
        DWX = 1, //修理
        DPF = 2,  //赔付
        DBF = 3, //报废
        DJD = 4  //捡定  
    }
    public class BasePrint
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nBaudRate">   
        ///1 -> 110     9 -> 19200
        ///2 -> 300    10 -> 38400
        ///3 -> 600    11 -> 56000
        ///4 -> 1200   12 -> 57600
        ///5 -> 2400   13 -> 115200
        ///6 -> 4800   14 -> 128000
        ///7 -> 9600   15 -> 256000
        ///8 -> 14400   0 -> 9600
        /// /param>
        /// <param name="nByteSize">       0 -> 7-bit data      7 -> 7-bit data      8 -> 8-bit data</param>
        /// <param name="nParity">  0 -> none parity      1 -> even parity      2 -> odd parity</param>
        /// <param name="nStopBits"> 0 -> 1 stop bit      1 -> 1 stop bit      2 -> 2 stop bits</param>
        /// <param name="nDsr">    1 -> DTR CONTROL HANDSHAKE;      0 -> DTR CONTROL ENABLE;</param>
        /// <param name="nCts"> 1 -> RTS CONTROL HANDSHAKE;      0 -> RTS CONTROL ENABLE;</param>
        /// <param name="nXonXoff">   0 -> Enable;      1 -> Disable;</param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Set_Prncomport_PC(int nBaudRate, int nByteSize, int nParity, int nStopBits, int nDsr, int nCts, int nXonXoff);
        /// <summary>
        /// Clear Flash memory.
        /// </summary>
        [DllImport("Winppla.dll")]
        public static extern void A_Clear_Memory();

        /// <summary>
        /// Stop printer operation.
        /// </summary>
        [DllImport("Winppla.dll")]
        public static extern void A_ClosePrn();

        /// <summary>
        /// Start printer opreation.
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_CreatePrn(int selection, string filename);

        /// <summary>
        /// Clean the stored "graphic data" in RAM or Flash memory.
        /// </summary>
        /// <param name="mem_mode"></param>
        /// <param name="graphic"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Del_Graphic(int mem_mode, string graphic);

        /// <summary>
        /// Create a "box" object.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="top"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Draw_Box(char mode, int x, int y, int width, int height,
            int top, int side);

        /// <summary>
        /// Get or show this verison for library.
        /// </summary>
        /// <param name="nShowMessage"></param>
        /// <returns></returns>


        [DllImport("Winppla.dll")]        
        public static extern IntPtr A_Get_DLL_Version(int nShowMessage);

        /// <summary>
        /// Used to immediately send data out,or is temporarily written to the buffer.
        /// </summary>
        /// <param name="IsImmediate"></param>
        /// <param name="pbuf"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_WriteData(int IsImmediate, byte[] pbuf, int length);

        /// <summary>
        /// Perform printing function.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="copies"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Print_Out(int width, int height, int copies, int amount);

        /// <summary>
        /// Create a "True Type Font" text object.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="FSize"></param>
        /// <param name="FType"></param>
        /// <param name="Fspin"></param>
        /// <param name="FWeight"></param>
        /// <param name="FItalic"></param>
        /// <param name="FUnline"></param>
        /// <param name="FStrikeOut"></param>
        /// <param name="id_name"></param>
        /// <param name="data"></param>
        /// <param name="mem_mode"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Prn_Text_TrueType(int x, int y, int FSize, string FType,
            int Fspin, int FWeight, int FItalic, int FUnline, int FStrikeOut, string id_name,
            string data, int mem_mode);

        /// <summary>
        /// Setup the "darkness" function (heating level).
        /// </summary>
        /// <param name="heat"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Set_Darkness(int heat);

        /// <summary>
        /// 设置回纸
        /// </summary>
        /// <param name="back"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Set_Backfeed(int back);

        /// <summary>
        /// Enable Debug Message Dialog.
        /// </summary>
        /// <param name="nEnable"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Set_DebugDialog(int nEnable);

        /// <summary>
        /// Setup the "sensoring" mode (gap, black mark, continuous).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="continuous"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Set_Sensor_Mode(char type, int continuous);

        /// <summary>
        /// Other function setup e.g. printing type, cutter and dispenser configuration, label length, slash zero mark, pause function....
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns> 
        [DllImport("Winppla.dll")]
        public static extern int A_Set_Syssetting(int transfer, int cut_peel, int length,
            int zero, int pause);

        /// <summary>
        /// etup measurement unit (metric or inches).
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Set_Unit(char unit);

        /// <summary>
        /// Get USB port data length.
        /// </summary>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_GetUSBBufferLen();

        /// <summary>
        /// Enum USB port.
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_EnumUSB(byte[] buf);

        /// <summary>
        /// Open USB port.
        /// </summary>
        /// <param name="nPort"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_CreateUSBPort(int nPort);

        /// <summary>
        ///  Create the 2D barcode object - QR (Automatic format).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="ori"></param>
        /// <param name="mult"></param>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        /// <param name="numeric"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_Bar2d_QR_A(int x, int y, int ori, char mult, int value,
            char mode, int numeric, string data);

        /// <summary>
        /// Get USB port that device name and device path.
        /// </summary>
        /// <param name="nPort"></param>
        /// <param name="pDeviceName"></param>
        /// <param name="pDeviceNameLen"></param>
        /// <param name="pDevicePath"></param>
        /// <param name="pDevicePathLen"></param>
        /// <returns></returns>
        [DllImport("Winppla.dll")]
        public static extern int A_GetUSBDeviceInfo(int nPort, byte[] pDeviceName,
            out int pDeviceNameLen, byte[] pDevicePath, out int pDevicePathLen);
        /// <summary>
        /// Create an "line" object.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>

        [DllImport("Winppla.dll")]
        public static extern int A_Draw_Line(char mode, int x, int y, int width, int height);

        [DllImport("Winppla.dll")]
        public static extern int A_ReadData(byte[] pbuf, int length, int dwTimeoutms);
    }
}
