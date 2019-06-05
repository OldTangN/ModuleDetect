using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// 打印机调用代码
//using System.Runtime.InteropServices;
//[DllImport("kernel32.dll")]
//static extern IntPtr LoadLibrary(string lpFileName);
//int feedback = 300;
//string result = "合格";
//string reason = "读数据成功";
//string opername = "张三";
//Print214 print214 = new Print214();
//print214.PrintResult(Convert.ToInt32(feedback), result, reason, opername);
/// </summary>

namespace test214
{
    public class Print214
    {
        public bool PrintResult(int feedback, string result, string reason, string opername)
        {

            //SECSLogHelper.WriteLog("打印");
            //OS-214Plus  "OS314P"

            // open port.
            int ret;
            byte[] pbuf = new byte[128];
            string strmsg;
            IntPtr ver;
            // dll version.
            try
            {
                ver = BasePrint.A_Get_DLL_Version(0);
            }
            catch
            {
                //SECSLogHelper.WriteLog("打印机DLL文件读取失败！");
                //return false;
            }
            // search port.
            int nLen = BasePrint.A_GetUSBBufferLen() + 1;
            strmsg = "DLL:";
            // strmsg += Marshal.PtrToStringAnsi(ver);
            strmsg += "\r\n";
            if (nLen > 1)
            {
                byte[] buf1, buf2;
                int len1 = 128, len2 = 128;
                buf1 = new byte[len1];
                buf2 = new byte[len2];
                BasePrint.A_EnumUSB(pbuf);
                BasePrint.A_GetUSBDeviceInfo(1, buf1, out len1, buf2, out len2);
                int sw = 1;
                if (1 == sw)
                {
                    ret = BasePrint.A_CreatePrn(12, Encoding.ASCII.GetString(buf2, 0, len2));//must call A_GetUSBBufferLen() function fisrt.open COM1
                }
                else
                {
                    ret = BasePrint.A_CreateUSBPort(1);//open usb.
                }
                if (0 != ret)
                {
                    //SECSLogHelper.WriteLog("打印机操作：打开USB失败!");
                    return false;
                }
                else
                {
                    strmsg += "Open USB:\r\nDevice name: ";
                    strmsg += Encoding.ASCII.GetString(buf1, 0, len1);
                    strmsg += "\r\nDevice path: ";
                    strmsg += Encoding.ASCII.GetString(buf2, 0, len2);
                    //sw = 2;
                    if (2 == sw)
                    {
                        //get printer status.
                        pbuf[0] = 0x01;
                        pbuf[1] = 0x41;
                        pbuf[2] = 0x0D;
                        pbuf[3] = 0x0A;
                        BasePrint.A_WriteData(1, pbuf, 4);//<SOH>F
                        ret = BasePrint.A_ReadData(pbuf, 8, 1000);

                        // MessageBox.Show("status=" + encAscII.GetString(pbuf));
                        BasePrint.A_ClosePrn();
                        return false;
                    }
                }
            }
            else
            {
                const string szSavePath = "C:\\ArgoxPrn";
                const string szSaveFile = "C:\\ArgoxPrn\\PPLA_Example.Prn";
                const string sznop1 = "nop_front\r\n";
                const string sznop2 = "nop_middle\r\n";

                System.IO.Directory.CreateDirectory(szSavePath);
                ret = BasePrint.A_CreatePrn(4, szSaveFile);// open file.
                strmsg += "打开 ";
                strmsg += szSaveFile;
                if (0 != ret)
                {
                    strmsg += " 文件失败!";
                    //SECSLogHelper.WriteLog("打印机操作：打印机内部错误！");
                    return false;
                }
                else
                {
                    strmsg += " 文件成功!";
                    BasePrint.A_Set_Prncomport_PC(13, 8, 0, 1, 0, 0, 0);
                }

            }
            BasePrint.A_Set_DebugDialog(1);
            BasePrint.A_Set_Unit('n');
            BasePrint.A_Set_Syssetting(1, 0, 0, 0, 0);
            // BasePrint.A_Set_Darkness(Convert.ToInt32(printConfig.ParamValue));
            BasePrint.A_Set_Backfeed(feedback);
            BasePrint.A_Del_Graphic(1, "*");// delete all picture.
            BasePrint.A_Clear_Memory();// clear memory.

            const int margin1 = 10;//通用1栏左边距
            const int margin2 = 150;//通用2栏左边距
            const int fsize = 24;//字号
            const int fweight = 600;//字体粗细
            const int down = 30;//底部 20 
                                //   const int down = 80;//底部 20 
            const string ftype = "Arial";//字体类型

            BasePrint.A_Prn_Text_TrueType(margin1, down, fsize, ftype, 1, fweight, 0, 0, 0, "AA", result, 1);
            BasePrint.A_Prn_Text_TrueType(margin1, down - 13, fsize, ftype, 1, fweight, 0, 0, 0, "AB", "结果:" + reason +" / " + DateTime.Now.ToString("yyMMdd"), 1);
            BasePrint.A_Prn_Text_TrueType(margin1, down - 25, fsize, ftype, 1, fweight, 0, 0, 0, "AC", "操作员:" + opername, 1);
            // BasePrint.A_Prn_Text_TrueType(margin1, 25*5, fsize, ftype, 1, fweight, 0, 0, 0, "AD","" , 1);
            int intret = BasePrint.A_Print_Out(1, 1, 1, 1);
            BasePrint.A_ClosePrn();
            return true;
        }

    }
}