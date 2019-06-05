using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using MdsLibrary;
using test214;
using MessageBox = System.Windows.MessageBox;
using MyLogLib;

namespace mkjcApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //Function define
        private const byte FUN_READ_STATE = 1;//
        private const byte FUN_READ_ID = 2;//
        private const byte FUN_SETUP_SCHEME = 3;//
        private const byte FUN_INIT = 4;//
        private const byte FUN_PM_CTL = 5;//
        private const byte FUN_STATIC_TEST = 6;//
        private const byte FUN_DYNAMIC_TEST = 7;//
        private const byte FUN_ATTENUATION_TEST = 8;//
        private const byte FUN_DCPOWER_TEST = 9;//
        private const byte FUN_ACTIVE_RPT = 10;//
        private const byte FUN_SPECIALL_TEST = 11;//
        private const byte FUN_COMM_TEST = 12;//
        private const byte FUN_RESULT_TEST = 13;//

        //状态字节索引定义
        private const byte S_SCHEME_INI = 1;//
        private const byte S_MODULE_INSERT = 2;//
        private const byte S_PM_INPUT = 3;//
        private const byte S_MODULE_MATCH = 4;//
        private const byte S_I_COMM_RESULT = 5;//
        private const byte S_I_COMM_ATTENUATION = 6;//
        private const byte S_NETPORT_TEST = 7;//
        private const byte S_GPRS_TEST = 8;//
        private const byte S_DCPOWER_TESY = 9;//
        private const byte S_STATE_TEST = 10;//
        private const byte S_RESET_TEST = 11;//
        private const byte S_SET_TEST = 12;//
        private const byte S_STA_TEST = 13;//
        private const byte S_EVENT_TEST = 14;//
        private const byte S_HEATING_TEST = 15;//
        private const byte S_ONOFF_TEST = 16;//
        private const byte S_SYS_RST = 17;//

        //系统相关宏定义
        private const byte SYSTEM_GW_NUM = 10;//
        private const Int16 INDIVIDUL_TIMEOUT = 1000;
        private UCDetect[] gzmodelhandle = new UCDetect[10];
        public static int SystemSrarted = 0;
        public static int PortOpened = 0;
        private static myprotocol[] gzprotocol = new myprotocol[10];
        private static myprotocol GPMProtocol = new myprotocol();
        public static myprotocol IVYTProtocol = new myprotocol();
        private static myprotocol[] Dlt645Protocol = new myprotocol[5];
        private static myprotocol NET16IOProtocol = new myprotocol();
        private static myprotocol NET4IOProtocol = new myprotocol();

        public static int _Task_id = 0;
        public static int _Select_task_id = 0;
        public static int newschemeflag = 0;

        //模块以类型统计数量
        public static int DanXiangZaiBo = 0;
        public static int SanXiangZaiBo = 0;
        public static int IXingJiZhongQiZaiBo = 0;
        public static int IXingJiZhongQiGPRS = 0;
        public static int IIXingJiZhongQiGPRS = 0;

        public static int RootAuth = 0;
        public static int ConfigAuth = 0;
        public static int LogAuth = 0;
        public static string CurrentLogin = "00020088";//测试时固定账号


        public static int buff = 0;
        public static string Scheme_Name = "";

        public static string[] GzList = new string[] { "181101001", "181101002", "181101003", "181101004", "181101005", "181101006",
                                                 "181101007", "181101008", "181101009", "181101010" };

        public static Scheme myScheme = new Scheme();
        private int CurrentGwIndex = -1;//范围0-9
        private int ResetCheckProc = 0;
        private int InitRunning = 0;
        private decimal GPMPReadValue = 0;

        public static int CheckTypeNum = 17; //myScheme.CheckTypeName.Count

        private ushort PcbPut, PcbGet;
        private int[] PcbFiFo = new int[CheckTypeNum];
        private void PutFiFo(int Pcb)
        {
            PcbFiFo[PcbPut] = Pcb;
            PcbPut = (ushort)((PcbPut + 1) % CheckTypeNum);
        }
        private int GetFiFo()
        {
            int rtn;
            if (PcbPut == PcbGet) return -1;
            rtn = PcbFiFo[PcbGet];
            PcbGet = (ushort)((PcbGet + 1) % CheckTypeNum);
            return rtn;
        }

        public static MdsClass mds = null;// mds系统交互
        public static object obj = new object();

        private ScanerHook bargunHook = new ScanerHook();

        //最后一次扫描的工位号
        private static int id_Latest = -1;
        private void Listener_ScanerEvent(ScanerHook.ScanerCodes codes)
        {
            //id_Latest = -1;
            bool isFinded = false;
            for (int i = 0; i < GzList.Length; i++)
            {
                if (codes.Result == GzList[i])
                {
                    isFinded = true;
                    break;
                }
            }
            if (isFinded)
                id_Latest = JudgeBarCode(codes.Result.Trim());
            else
                FillStationUI(id_Latest, codes.Result.Trim());
        }
        //刷新相应的工位面板界面
        private void FillStationUI(int ID_Latest, string barcode)
        {
            if (ID_Latest == -1)
                return;
            gzmodelhandle[ID_Latest - 1].barcode.Background = new SolidColorBrush(Colors.Green);
            gzmodelhandle[ID_Latest - 1].barcode.Content = barcode;
            return;
        }
        //刷新工位条码显示条
        private int JudgeBarCode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return -1;

            /*gzmodel1.GzTiaoma.Background = new SolidColorBrush(Colors.Transparent);*/

            int id_station = Convert.ToInt32(barcode.Trim().Substring(7, 2));
            gzmodelhandle[id_station - 1].GzTiaoma.Content = barcode.Trim();
            gzmodelhandle[id_station - 1].GzTiaoma.Background = new SolidColorBrush(Colors.Green);

            return id_station;
        }


        public MainWindow()
        {
            //string ip = System.Configuration.ConfigurationManager.AppSettings["gprsip"];
            //int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["gprsport"]);
            //GPRSServer gprs = new GPRSServer(ip, port);
            //if (!gprs.Start())
            //{
            //    MessageBox.Show("GPRS主站启动失败！");
            //}
            //GPRSServer gprs2 = new GPRSServer("192.168.8.100", 8899);
            //if (!gprs2.Start())
            //{
            //    MessageBox.Show("GPRS主站启动失败2！");
            //}
            //add by sun 2019-2-27
            if (!Properties.Settings.Default.LineOff) //如果不是离线
            {
                mds = new MdsClass();
                string errorstr = "";
                bool t = mds.InitIni(out errorstr);
                if (t == false)
                {
                    MessageBox.Show(errorstr);
                }
                t = mds.InitDB(out errorstr);
                if (t == false)
                {
                    MessageBox.Show(errorstr);
                }
                t = mds.InitMySqlDB(out errorstr);
                if (t == false)
                {
                    MessageBox.Show(errorstr);
                }
                t = mds.IniWebService(out errorstr);
                if (t == false)
                {
                    MessageBox.Show(errorstr);
                }
            }
            //add end
            InitializeComponent();

            string taskno = "";
            string totalmodule = "";
            string finishcnt = "";
            string errinfo = "";
            Thread thd = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (!Properties.Settings.Default.LineOff) //如果不是离线
                    {
                        //下拉任务
                        lock (obj)
                        {
                            // mds.DownLoadTask(out errinfo);
                            //获取目前检定的状态信息检定的完成数目
                            mds.GetMdsInfo(out taskno, out totalmodule, out finishcnt, out errinfo);
                        }
                    }
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        RenWuHao.Content = taskno;
                        CaoZuoYuan.Content = CurrentLogin;
                        DaiJianZongLiang.Content = totalmodule;
                        YiJianShuLiang.Content = finishcnt;
                        QueXianShuLiang.Content = errinfo;
                    }));
                }
            });
            thd.IsBackground = true;
            thd.Start();
            //只有root用户能注册和修改权限
            if (RootAuth == 1)
            {
                UserBtn.IsEnabled = true;
                HistoryBtn.IsEnabled = true;
                ConfigBtn.IsEnabled = true;
            }
            if (ConfigAuth == 1)
            {
                UserBtn.IsEnabled = true;
                HistoryBtn.IsEnabled = true;
                ConfigBtn.IsEnabled = true;
            }
            if (LogAuth == 1)
            {
                UserBtn.IsEnabled = true;
                HistoryBtn.IsEnabled = true;
                ConfigBtn.IsEnabled = false;
            }

            currentLogin.Content = CurrentLogin;

            bargunHook.ScanerEvent += Listener_ScanerEvent; //注册条码枪钩子

            gzmodelhandle[0] = gzmodel1;
            gzmodelhandle[1] = gzmodel2;
            gzmodelhandle[2] = gzmodel3;
            gzmodelhandle[3] = gzmodel4;
            gzmodelhandle[4] = gzmodel5;
            gzmodelhandle[5] = gzmodel6;
            gzmodelhandle[6] = gzmodel7;
            gzmodelhandle[7] = gzmodel8;
            gzmodelhandle[8] = gzmodel9;
            gzmodelhandle[9] = gzmodel10;
        }

        public static string ByteToHexStr(byte[] bytes)
        {
            string returnstr = "";
            if (bytes == null) return "";
            for (int i = 0; i < bytes.Length; i++)
            {
                returnstr += bytes[i].ToString("X2");
            }
            return returnstr;
        }
        public int initPlc()//要根据测试方案，打开
        {
            int i, rtn;
            int[] Temp = new int[CheckTypeNum];
            PcbPut = 0;
            PcbGet = 0;
            SetupgwCheckItem();

            //if (PortOpened == 0)
            {
                for (i = 0; i < SYSTEM_GW_NUM; i++)
                {
                    gzprotocol[i]?.ClosePort();
                    gzprotocol[i] = new myprotocol();
                    gzprotocol[i].DataReceived += DataReceived;
                    gzprotocol[i].InitializePorts(myScheme.gzmoduleUartNum[i]);
                }
                for (i = 0; i < 5; i++)
                {
                    Dlt645Protocol[i]?.ClosePort();
                    Dlt645Protocol[i] = new myprotocol();
                    Dlt645Protocol[i].DataReceived = DataReceived;
                    rtn = Dlt645Protocol[i].InitializePorts(myScheme.ChaoKongPortNum[i]);
                    if (rtn > 0)
                    {
                        //这里返回成功打开的端口号，显示打开的抄控器。
                        //gzmodelhandle[0].ItemLabel1.Content = "串口打开成功";
                    }
                    else
                    {
                        //这里应显示打开失败的超控器;
                        MessageBox.Show("抄控器端口失败");
                    }
                }
                GPMProtocol.ClosePort();
                rtn = GPMProtocol.InitializePorts(2);
                if (rtn > 0)
                {
                    GPMProtocol.DataReceived = DataReceived;
                    //这里返回成功打开的端口号，显示功耗测试仪端口正常。
                }
                else
                {
                    return -1;
                }
                IVYTProtocol.ClosePort();
                rtn = IVYTProtocol.InitializePorts(1);
                if (rtn > 0)
                {
                    IVYTProtocol.DataReceived = DataReceived;
                    //这里返回成功打开的端口号，显示直流源端口正常。
                }
                else
                {
                    return -1;
                }
                if (NET16IOProtocol.InitializeNet16() >= 0)
                {
                    NET16IOProtocol.DataReceived = DataReceived;
                }
                else
                {
                    return -1;
                }
                if (NET4IOProtocol.InitializeNet4() >= 0)
                {
                    NET4IOProtocol.DataReceived = DataReceived;
                }
                else
                {
                    return -1;
                }
                PortOpened = 1;
            }

            for (i = 0; i < SYSTEM_GW_NUM; i++)
            {
                for (int j = 0; j < CheckTypeNum; j++)
                {
                    Temp[j] = myScheme.SchemeCheckItem[myScheme.gzmoduleType[i], j];
                }

                gzprotocol[i].locModuleObj.SetSchemeCheckItem(Temp, CheckTypeNum);
                gzprotocol[i].locModuleObj.InitSchemeOk = 0;
                rtn = 1; //gzprotocol[i].InitializePorts(myScheme.gzmoduleUartNum[i]);
                if (rtn > 0)
                {
                    gzmodelhandle[i].gwId.Content = "工位" + (i + 1);
                    //gzmodelhandle[i].GzTiaoma.Content = GzList[i];
                    //if (myScheme.gzmoduleType[i] != 0)
                    //{
                    gzmodelhandle[i].SchemeName.Content = Scheme_Name;
                    // }
                    for (int j = 0; j < CheckTypeNum; j++)
                    {
                        int item = gzprotocol[i].locModuleObj.CheckItemToViewIndex[j];
                        switch (item)
                        {
                            case 0:
                                gzmodelhandle[i].ItemLabel1.Content = myScheme.CheckTypeName[j];
                                break;
                            case 1:
                                gzmodelhandle[i].ItemLabel2.Content = myScheme.CheckTypeName[j];
                                break;
                            case 2:
                                gzmodelhandle[i].ItemLabel3.Content = myScheme.CheckTypeName[j];
                                break;
                            case 3:
                                gzmodelhandle[i].ItemLabel4.Content = myScheme.CheckTypeName[j];
                                break;
                            case 4:
                                gzmodelhandle[i].ItemLabel5.Content = myScheme.CheckTypeName[j];
                                break;
                            case 5:
                                gzmodelhandle[i].ItemLabel6.Content = myScheme.CheckTypeName[j];
                                break;
                            case 6:
                                gzmodelhandle[i].ItemLabel7.Content = myScheme.CheckTypeName[j];
                                break;
                            case 7:
                                gzmodelhandle[i].ItemLabel8.Content = myScheme.CheckTypeName[j];
                                break;
                            default:
                                break;
                        }
                    }
                    //这里返回成功打开的端口号，显示打开的工位。
                    //gzmodelhandle[0].ItemLabel1.Content = "串口打开成功";
                }
                else
                {
                    //这里应显示没有打开的工位;
                }
            }
            return 0;
        }

        /// <summary>
        /// 所有的逻辑主循环
        /// </summary>
        public void PlcProcess()
        {
            int i, gwIndex = 0;
            while (true)
            {
                Thread.Sleep(50);
                try
                {
                    AutoMachine();
                    for (i = 0; i < SYSTEM_GW_NUM; i++)
                    {
                        gzprotocol[i].mkjcProtocol();
                    }
                    for (i = 0; i < 5; i++)
                    {
                        Dlt645Protocol[i].DLT645Protocol();
                    }
                    GPMProtocol.GPMProtocol();
                    IVYTProtocol.IVYTProtocol();
                    NET16IOProtocol.NET16IOProtocol();
                    NET4IOProtocol.NET4IOProtocol();
                    CheckFinishedProc(gwIndex);
                    gwIndex++;
                    if (gwIndex > SYSTEM_GW_NUM - 1) gwIndex = 0;
                }
                catch (Exception ex)
                {
                    MyLog.WriteLog(ex);
                }
            }
        }

        /// <summary>
        /// 检测特定工位是否投入HPLCID检测项，大于0表示投入
        /// </summary>
        /// <param name="gwIndex"></param>
        /// <returns></returns>
        private int HPLCEnableCheck(int gwIndex)
        {
            try
            {
                for (int i = 0; i < gzprotocol[gwIndex].locModuleObj.checktypenum; i++)
                {
                    if (gzprotocol[gwIndex].locModuleObj.SchemeCheckItem[i] == 16)//如果发现HPLID检测项
                        return 1;
                }
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("gwIndex:" + gwIndex, ex);
            }
            return -1;
        }

        /// <summary>
        /// hplcid读取完毕，离线状态直接保存数据，在线状态验证hplcid后保存数据
        /// </summary>
        /// <param name="Index"></param>
        public void CheckFinishedProc(int Index)
        {
            int TotalResult, gwIndex;
            gwIndex = Index;
            if (myScheme.gzmoduleType[gwIndex] != 0)
            {
                if (gzprotocol[gwIndex].locModuleObj.TotalResult != 0)//如果工位检测完成
                {
                    TotalResult = gzprotocol[gwIndex].locModuleObj.TotalResult;
                    //如果工位条码和模块条码完整，则存储
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        string gw_barcode = "";
                        string module_barcode = "";
                        gw_barcode = gzmodelhandle[gwIndex].GzTiaoma.Content.ToString();
                        module_barcode = gzmodelhandle[gwIndex].barcode.Content.ToString();
                        // module_barcode = "631423000242";
                        if ((module_barcode != "") && (gw_barcode != ""))
                        {
                            string rest = "";
                            string errstr = "";
                            if (HPLCEnableCheck(gwIndex) > 0)//如果芯片ID功能有效
                            {
                                Thread hplcCheckThread = new Thread(() =>
                                {
                                    gzprotocol[gwIndex].locModuleObj.TotalResult = 0;
                                    string hplcid = gzprotocol[gwIndex].HPLC_ID_String;
                                    //  hplcid = "01029C01C1FB024646463000000123B69B8F18C1ED75B59A";
                                    //hplcid = "2018080600102";

                                    if (gzprotocol[gwIndex].HPLC_ID_String != "" && !gzprotocol[gwIndex].CheckingHPLC_ID)
                                    {
                                        if (!Properties.Settings.Default.LineOff) //如果不是离线
                                        {
                                            gzprotocol[gwIndex].CheckingHPLC_ID = true;
                                            //lock (obj)
                                            {
                                                //Thread.Sleep(20 * 1000);
                                                MyLog.WriteLog("HPLCCheck:" + hplcid + " ; " + module_barcode);
                                                mds.HPLCCheck(hplcid, module_barcode, out rest, out errstr);//rest "1"=合法 "0"=不合法
                                                MyLog.WriteLog("HPLCCheck result:" + rest + " ; errinfo:" + errstr);
                                            }
                                            gzprotocol[gwIndex].CheckingHPLC_ID = false;
                                            if (rest != "1")
                                            {
                                                TotalResult = 2;
                                            }
                                            else
                                            {
                                                TotalResult = 1;
                                            }

                                        }
                                        SaveResult(TotalResult, gwIndex, gw_barcode, module_barcode, rest);
                                    }
                                });
                                hplcCheckThread.IsBackground = true;
                                hplcCheckThread.Start();
                            }
                            else
                            {
                                SaveResult(TotalResult, gwIndex, gw_barcode, module_barcode, rest);
                            }
                        }
                    }));
                }
            }
        }

        private void SaveResult(int TotalResult, int gwIndex, string gw_barcode, string module_barcode, string rest)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                bool printflag = false;
                if (TotalResult == 1)//测试通过
                {
                    gzmodelhandle[gwIndex].GzBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                    gzprotocol[gwIndex].SendMstCmd(13, 1, 0);
                }
                else
                {
                    gzmodelhandle[gwIndex].GzBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                    gzprotocol[gwIndex].SendMstCmd(13, 0, 0);
                }
                gzprotocol[gwIndex].locModuleObj.TotalResult = 0;
                //resu1t>0表示检测通过，检测数据参见下方代码 myScheme.SchemeCheckItem[style, i]
                string printitemid = "";
                for (int i = 0; i < CheckTypeNum; i++)
                {
                    int everyResult = gzprotocol[gwIndex].locModuleObj.checkresult[i];
                    if (i == 16)//HPLCID 判断是否有效 1是通过 2是不通过
                    {
                        if (rest != "1")
                            everyResult = 2;
                        else
                            everyResult = 1;
                    }
                    decimal everyValue = gzprotocol[gwIndex].locModuleObj.resultValue[i];
                    //数据库存储接口处
                    int a = gzprotocol[gwIndex].locModuleObj.ModuleType;
                    bool ValidItem = false;
                    for (int j = 0; j < CheckTypeNum; j++)//验证检测项是否在方案中投入检测
                    {
                        if (myScheme.SchemeCheckItem[a, j] == i)
                        {
                            ValidItem = true;
                            break;
                        }
                    }

                    if (ValidItem == true)//方案中有设置检测，则保存数据库
                    {
                        string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        string sqlstring = $"select * from plc_scheme.test_result where module_barcode='{module_barcode}' and item='{i.ToString()}'";

                        DataTable dt = MySQLHelper.SQLSelect(sqlstring);
                        string result_data = "";
                        result_data = i == 16 ? gzprotocol[gwIndex].HPLC_ID_String : everyValue.ToString(); //hplcid太长(48位)，超过mds中间库长度(16位)
                        if (result_data == "0")//有数值的保存数值，没数值的保存通过不通过
                        {
                            result_data = everyResult == 1 ? "通过" : "不通过";
                        }
                        //else
                        //{
                        //    result_data = everyValue.ToString();
                        //}

                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                string upsqlstr = $"update plc_scheme.test_result set result='{everyResult}',result_data='{result_data}',date='{datetime}',station_barcode='{gw_barcode}'" +
                                                                 $" where  module_barcode='{module_barcode}' and item='{i.ToString()}'";
                                MySQLHelper.SQLinsert(upsqlstr);
                            }
                            else
                            {
                                //  sqlstring = "insert into plc_scheme.test_result(result, result_data, station_barcode, module_barcode, date, task_id, item) values(" + everyResult + ", " + everyValue.ToString() + ", " + gw_barcode + ", " + module_barcode + ", " + datetime + ", " + MainWindow.select_task_id + ", '" + i.ToString() + "')";
                                sqlstring = $"insert into plc_scheme.test_result(result,result_data,station_barcode,module_barcode, date, task_id, item,test_operator,verify_operator,temperature,humidity) values(" +
                                 $"'{everyResult}'," +
                                 $"'{result_data}'," +
                                 $"'{gw_barcode}'," +
                                 $"'{module_barcode}'," +
                                 $"'{datetime}'," +
                                 $"'{MainWindow._Select_task_id}'," +
                                 $"'{i.ToString()}'," +
                                 $"'{MainWindow.CurrentLogin}'," +
                                 $"''," +
                                 $"''," +
                                 $"''" +
                                 $");";
                                MySQLHelper.SQLinsert(sqlstring);
                            }
                        }

                        if (everyResult == 2)
                        {
                            printitemid += i.ToString() + " ";
                            printflag = true;
                        }
                    }
                }
                string errorinfo = "";
                if (!Properties.Settings.Default.LineOff) //如果不是离线
                {
                    lock (obj)
                    {
                        mds.UpdataRsltForMdsBar(out errorinfo);//替换为 根据任务号更新结论
                    }
                }
                if (printflag == true)
                {
                    try
                    {
                        Print214 print = new Print214();
                        module_barcode = Convert.ToInt64(module_barcode.Trim().Substring(12, 10)).ToString();
                        print.PrintResult(Convert.ToInt32("330"), module_barcode, printitemid, currentLogin.Content.ToString());
                    }
                    catch
                    {

                    }
                }
            }));
        }

        private int dispatchStep = 1;
        private int WaitingTimeout = 0;
        private int[] RtnMsgValue = new int[25];

        public void AutoMachine()
        {
            /********************************************************
             * 初始化步骤1-8：
             * 1.直流源初始化,设置直流源为12V，电流为5A
             * 2.打开直流源
             * 3.功率测试仪初始化
             * 4.功率测试仪HOLD OFF
             * 5.抄控器投入，衰减器退出（如果需要）
             * 6.设置工位方案，确定工位工作正常，或检测工装基础功耗（如需要）
             * 7 提示操作员整备就绪
 
             *轮训步骤
             * 1.接收主动上报信息，对信息进行缓存及显示；
             * 2.将准备就绪（特定的信息或超时）的模块对应工位加入检测队列，采用先进先出策略；
             * 3.针对就绪的工位步骤：
             *    单相:
             * （1）功率测试仪HOLD OFF，读取功率值；（2）将指定工位切入测试仪，读取功率值；（3）功率测试仪HOLD ON
             * （4）通过抄控器发送指令（5）读取功率值，工位退出；（6）投入衰减器，发送抄读指令；（7）电源升到13V，维持2秒，期间发送抄读指令；
             * （8）电源降到11V，维持2秒，期间发送抄读指令；（9）恢复到12V
             * 
             *    三相：
             *    与单相的区别是在（3）（5）之间加入相序切换步骤：切换至A相，通信；切换至B相，通信；切换至C相，通信。
             *    
             *    I型载波：
             * （1）功率测试仪HOLD OFF，读取功率值；（2）将指定工位切入测试仪，读取功率值；（3）功率测试仪HOLD ON
             * （4）向工位发送动态功耗测试指令，等待通信完成信息（5）读取功率值，工位退出；（6）投入衰减器，发送衰减测试指令；（7）发送电源波动测试指令，电源升到13V，维持2秒；
             * （8）电源降到11V，维持2秒，等待结果上报；（9）恢复到12V
             *    
             *    GPRS模块：
             * （1）功率测试仪HOLD OFF，读取功率值；（2）将指定工位切入测试仪；（3）发送静态功耗测试指令，等待2秒后读取功率值；（4）功率测试仪HOLD ON
             * （5）向工位发送动态功耗测试指令，等待通信完成信息（6）读取功率值，工位退出；（7）发送电源波动测试指令，电源升到13V，维持2秒；
             * （8）电源降到11V，维持2秒，等待结果上报；（9）恢复到12V
             * 
             * 
             *设计说明：
             * 1.为每个工位准备一个对应的数据对象，保存模块状态及ID号、已检项目、待检项目、整体时钟、单项时钟；
             * 2.在检测电源波动测试时，向就绪队列中的所有工位发送波动测试命令。其他测试项均按照顺序执行；
             * 3.单个工位测试完成后，发送指示灯控制（红灯或绿灯）；
             * 4.工位红灯点亮后，若错误信息被纠正，则关闭红灯，点亮绿灯；
             * 5.若在测试过程中收到工装复位信息，则需清除该工位所有已测内容，若频繁出现则需停机整改；
             * 
             * 异常处理：
             * 1.工装均无通信——检查12V电源
             * 2.功耗过低——检查功率切入继电器是否正常
             * 3.步骤1功率过高（应接近0）——前面工位未退出
             * 4.静态功率过高——检查测试仪是否被锁存
             * 
             * 意见及建议：
             * 1.为提高测试效率，对于进行动态测试的设备，不必投入通信测试功能和衰减检测功能。
             * 2.对于流水线测试，单三相模块插入可通过模块地址请求指令完成，不必检测STA脚稳定时间。
             *******************************************************/
            int i, ViewVal;
            if (ResetCheckProc != 0)
            {
                dispatchStep = 1;
                ResetCheckProc = 0;
                CurrentGwIndex = -1;

            }
            try
            {
                switch (dispatchStep)
                {
                    case 1://打开直流源
                        PowerControl(1);
                        dispatchStep = 2;
                        break;
                    case 2://功率仪初始化
                        GPMPControl();
                        dispatchStep = 4;
                        break;
                    case 4://220V A相控制
                        for (i = 0; i < SYSTEM_GW_NUM; i++)
                        {
                            RtnMsgValue[i] = 0;
                            if (myScheme.gzmoduleType[i] != 0)
                            {
                                if (myScheme.gzmoduleType[i] == 1) break;//是否是单相模块
                            }
                        }
                        if (i < 10)//说明有单相载波通信模块，则闭合A相，之后不再控制，提高测试效率
                            PhaseABCControl(1, 0, 0);
                        else
                            PhaseABCControl(0, 0, 0);
                        dispatchStep = 5;
                        break;
                    case 5://复归16口继电器，使其均为分
                        Reset16IOController();
                        ChaokongqiControl(0, 1);
                        ChaokongqiControl(1, 1);
                        ChaokongqiControl(2, 1);
                        ChaokongqiControl(3, 1);
                        NET16IOProtocol.RelayClose(14);//点亮黄灯
                        Thread.Sleep(500);
                        dispatchStep = 6;
                        break;
                    case 6://
                        NET16IOProtocol.RelayClose(15);//点亮绿灯
                        dispatchStep = 13;
                        break;
                    case 106://等待网络继电器响应
                        if (RtnMsgValue[23] != 0)
                        {
                            dispatchStep = 13;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("抄控器投入成功！");
                                G4.Visibility = Visibility.Visible;
                                R4.Visibility = Visibility.Hidden;
                            }
                            ));
                            break;
                        }
                        WaitingTimeout--;
                        if (WaitingTimeout <= 0)
                        {
                            dispatchStep = 6;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("抄控器投入失败！");
                                R4.Visibility = Visibility.Visible;
                                G4.Visibility = Visibility.Hidden;
                            }));
                        }
                        break;
                    case 13://工位检测过程，包括工位方案初始化
                        GwCheckDispatch();
                        break;
                    case 20://基础功耗更新
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
            }
        }

        private int PowerControl(int CtlType)
        {
            int locStep = 1;
            int locWaitingTimeout = 100;
            while (true)
            {
                Thread.Sleep(50);
                IVYTProtocol.IVYTProtocol();
                switch (locStep)
                {
                    case 1://直流源初始化
                        RtnMsgValue[12] = 0;
                        IVYTProtocol.IVYTControl(1);
                        locStep = 101;
                        locWaitingTimeout = 100;
                        break;
                    case 101://等待直流源响应
                        if (RtnMsgValue[12] != 0)
                        {
                            locStep = 2;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("直流源初始化成功！");
                                G1.Visibility = Visibility.Visible;
                                R1.Visibility = Visibility.Hidden;
                            }
                            ));
                            break;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 1;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("直流源初始化失败！");
                                R1.Visibility = Visibility.Visible;
                                G1.Visibility = Visibility.Hidden;
                                zly_dy.Text = "";
                                zly_dl.Text = "";
                                zly_ycms.Text = "";
                            }
                            ));
                        }
                        break;
                    case 2://打开直流源
                        RtnMsgValue[12] = 0;
                        IVYTProtocol.IVYTControl(7);
                        locStep = 102;
                        locWaitingTimeout = 100;
                        break;
                    case 102://等待直流源响应
                        if (RtnMsgValue[12] != 0)
                        {
                            locStep = 3;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("直流源输出成功！");
                                G2.Visibility = Visibility.Visible;
                                R2.Visibility = Visibility.Hidden;
                                zly_ycms.Text = "REM";
                            }
                            ));
                            return 0;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 2;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("直流源输出失败！");
                                R2.Visibility = Visibility.Visible;
                                G2.Visibility = Visibility.Hidden;
                                zly_dl.Text = "";
                                zly_dy.Text = "";
                                zly_ycms.Text = "";
                            }
                            ));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private int GPMPControl()
        {
            int locStep = 1;
            int locWaitingTimeout = 100;
            while (true)
            {
                Thread.Sleep(50);
                GPMProtocol.GPMProtocol();
                switch (locStep)
                {
                    case 1://初始化功率测试仪
                        RtnMsgValue[11] = 0;
                        GPMProtocol.GPMControl(1);
                        locStep = 101;
                        locWaitingTimeout = 100;
                        break;
                    case 101://等待功率仪响应
                        if (RtnMsgValue[11] != 0)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("功率仪初始化成功！");
                                G3.Visibility = Visibility.Visible;
                                R3.Visibility = Visibility.Hidden;
                            }
                            ));
                            return 0;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 1;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("功率仪初始化失败！");
                                R3.Visibility = Visibility.Visible;
                                G3.Visibility = Visibility.Hidden;
                            }
                            ));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private int PhaseABCControl(int AStation, int BStation, int CStation)
        {
            int locStep = 1;
            int locWaitingTimeout = 100;
            while (true)
            {
                Thread.Sleep(50);
                NET4IOProtocol.NET4IOProtocol();
                switch (locStep)
                {
                    case 1://220V A相控制
                        locStep = 2;
                        RtnMsgValue[24] = 0;
                        locWaitingTimeout = 100;
                        if (AStation != 0)
                            NET4IOProtocol.RelayClose1(1);
                        else
                            NET4IOProtocol.RelayTrip1(1);
                        break;
                    case 2://等待A相操作响应
                        if (RtnMsgValue[24] != 0)
                        {
                            locStep = 3;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("A相操作成功！");
                                G9.Visibility = Visibility.Visible;
                                R9.Visibility = Visibility.Hidden;
                            }
                            ));

                            break;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 1;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("A相操作失败！");
                                R9.Visibility = Visibility.Visible;
                                G9.Visibility = Visibility.Hidden;
                            }
                            ));
                        }
                        break;
                    case 3://220V B相控制
                        locStep = 4;
                        RtnMsgValue[24] = 0;
                        locWaitingTimeout = 100;
                        if (BStation != 0)
                            NET4IOProtocol.RelayClose1(2);
                        else
                            NET4IOProtocol.RelayTrip1(2);
                        break;
                    case 4://等待B相操作响应
                        if (RtnMsgValue[24] != 0)
                        {
                            locStep = 5;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("B相操作成功！");
                                G10.Visibility = Visibility.Visible;
                                R10.Visibility = Visibility.Hidden;
                            }
                            ));

                            break;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 3;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("B相操作失败！");
                                R10.Visibility = Visibility.Visible;
                                G10.Visibility = Visibility.Hidden;
                            }
                            ));
                        }
                        break;
                    case 5://220V C相控制
                        locStep = 6;
                        RtnMsgValue[24] = 0;
                        locWaitingTimeout = 100;
                        if (CStation != 0)
                            NET4IOProtocol.RelayClose1(3);
                        else
                            NET4IOProtocol.RelayTrip1(3);
                        break;
                    case 6://等待C相操作响应
                        if (RtnMsgValue[24] != 0)
                        {
                            locStep = 7;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("C相操作成功！");
                                G11.Visibility = Visibility.Visible;
                                R11.Visibility = Visibility.Hidden;
                            }
                            ));
                            return 0;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 5;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("C相操作失败！");
                                R11.Visibility = Visibility.Visible;
                                G11.Visibility = Visibility.Hidden;
                            }
                            ));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private int ChaokongqiControl(int ChaoKongIndex, int join)
        {
            int locStep = 1;
            int locWaitingTimeout = 100;
            int ChaoKongIOIndex = myScheme.ChaoKongIOIndex[ChaoKongIndex];
            if (ChaoKongIOIndex <= 0) return -1;
            int sjqIOIndex = ChaoKongIOIndex - 1;
            while (true)
            {
                Thread.Sleep(50);
                NET16IOProtocol.NET16IOProtocol();
                switch (locStep)
                {
                    case 1:
                        RtnMsgValue[23] = 0;
                        locWaitingTimeout = 100;
                        locStep = 2;
                        if (join != 0)//抄控器投入
                            NET16IOProtocol.RelayClose(ChaoKongIOIndex);
                        else
                            NET16IOProtocol.RelayClose(sjqIOIndex);
                        break;
                    case 2:
                        if (RtnMsgValue[23] != 0)
                        {
                            locStep = 3;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                if (join != 0)
                                {
                                    ShowSysMsgLabel("抄控器投入成功！");
                                    G4.Visibility = Visibility.Visible;
                                    R4.Visibility = Visibility.Hidden;
                                }

                                else
                                {
                                    ShowSysMsgLabel("衰减器投入成功！");
                                    G6.Visibility = Visibility.Visible;
                                    R6.Visibility = Visibility.Hidden;
                                }
                            }
                            ));
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 1;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                if (join != 0)
                                {
                                    ShowSysMsgLabel("抄控器投入失败！");
                                    R4.Visibility = Visibility.Visible;
                                    G4.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    ShowSysMsgLabel("衰减器投入失败！");
                                    R6.Visibility = Visibility.Visible;
                                    G6.Visibility = Visibility.Hidden;
                                }
                            }
                            ));
                        }
                        break;
                    case 3:
                        RtnMsgValue[23] = 0;
                        locWaitingTimeout = 100;
                        locStep = 4;
                        if (join != 0)//衰减器退出
                            NET16IOProtocol.RelayTrip(sjqIOIndex);
                        else
                            NET16IOProtocol.RelayTrip(ChaoKongIOIndex);
                        break;
                    case 4:
                        if (RtnMsgValue[23] != 0)
                        {
                            locStep = 5;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                if (join != 0)
                                {
                                    ShowSysMsgLabel("衰减器退出成功！");
                                    G7.Visibility = Visibility.Visible;
                                    R7.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    ShowSysMsgLabel("抄控器退出成功！");
                                    G5.Visibility = Visibility.Visible;
                                    R5.Visibility = Visibility.Hidden;
                                }
                            }
                            ));
                            return 0;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 3;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                if (join != 0)
                                {
                                    ShowSysMsgLabel("衰减器退出失败！");
                                    R7.Visibility = Visibility.Visible;
                                    G7.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    ShowSysMsgLabel("抄控器退出失败！");
                                    R5.Visibility = Visibility.Visible;
                                    G5.Visibility = Visibility.Hidden;
                                }
                            }
                            ));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private int Reset16IOController()
        {
            int locStep = 1;
            int locWaitingTimeout = 100;
            int IoIndex = 1;
            while (true)
            {
                Thread.Sleep(50);
                NET16IOProtocol.NET16IOProtocol();
                if (locStep == 1)
                {
                    locStep = 2;
                    RtnMsgValue[23] = 0;
                    locWaitingTimeout = 100;
                    NET16IOProtocol.RelayTrip(IoIndex);
                }
                else
                {
                    if (RtnMsgValue[23] != 0)
                    {
                        locStep = 1;
                        if (IoIndex >= 10)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("16口继电器复归成功！");
                                G8.Visibility = Visibility.Visible;
                                R8.Visibility = Visibility.Hidden;
                            }
                            ));
                            return 0;
                        }
                        IoIndex++;
                    }
                    locWaitingTimeout--;
                    if (locWaitingTimeout <= 0)
                    {
                        locStep = 1;//等待超时，重复当前步骤。
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("16口IO继电器复归失败！");
                            R8.Visibility = Visibility.Visible;
                            G8.Visibility = Visibility.Hidden;
                        }
                        ));
                    }
                }
            }
        }

        private void InitGwPWValue()
        {
            int locStep = 1;
            int locWaitingTimeout = 100;
            int locGwIndex = 0;
            while (true)
            {
                Thread.Sleep(50);
                for (int i = 0; i < SYSTEM_GW_NUM; i++)
                {
                    gzprotocol[i].mkjcProtocol();
                }
                GPMProtocol.GPMProtocol();
                switch (locStep)
                {
                    case 1://功率测试仪HOLD OFF
                        RtnMsgValue[11] = 0;
                        GPMProtocol.GPMControl(7);
                        locWaitingTimeout = 20;
                        locStep = 2;
                        break;
                    case 2://等待功率仪状态确认
                        if (RtnMsgValue[11] == 7)
                        {
                            locStep = 3;
                            break;
                        }
                        locWaitingTimeout--;

                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 1;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                SysMsgLabel.Content = "功率仪控制失败";
                            }
                            ));
                        }
                        break;
                    case 3://读取功率测试仪
                        RtnMsgValue[11] = 0;
                        GPMProtocol.GPMControl(10);
                        locWaitingTimeout = 20;
                        locStep = 4;
                        break;
                    case 4://等待功率仪读数确认
                        if (RtnMsgValue[11] == 10)
                        {
                            if (GPMPReadValue < (decimal)0.05)
                            {
                                locStep = 5;
                            }
                            else
                            {
                                locStep = 3;
                                for (int i = 0; i < SYSTEM_GW_NUM; i++)
                                {
                                    if (myScheme.gzmoduleType[i] != 0)
                                    {
                                        gzprotocol[i].SendMstCmd(5, 0, 0);
                                    }
                                }
                                Thread.Sleep(500);
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    String str = "发现工位没有退出功率测量！";
                                    ShowSysMsgLabel(str);
                                }
                            ));
                            }

                            break;
                        }
                        locWaitingTimeout--;

                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 3;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                SysMsgLabel.Content = "功率仪读取失败1";
                            }
                            ));
                        }
                        break;
                    case 5://将工位投入功率测量回路
                        RtnMsgValue[locGwIndex] = 0;
                        gzprotocol[locGwIndex].SendMstCmd(5, 1, 0);
                        locWaitingTimeout = 40;
                        locStep = 6;
                        break;
                    case 6://等待工位投入确认
                        if (RtnMsgValue[locGwIndex] == 5)
                        {
                            locStep = 20;
                            locWaitingTimeout = 10;
                            Thread.Sleep(500);
                            break;
                        }
                        locWaitingTimeout--;

                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 5;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                String str = "工位投入失败！";
                                ShowSysMsgLabel(str);
                            }
                            ));
                        }
                        break;
                    case 20://延时
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 7;
                        }
                        break;
                    case 7:
                        RtnMsgValue[11] = 0;
                        GPMProtocol.GPMControl(10);
                        locWaitingTimeout = 20;
                        locStep = 8;
                        break;
                    case 8://等待功率仪读数
                        if (RtnMsgValue[11] == 10)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                if ((GPMPReadValue > 0.05M) && (GPMPReadValue < 0.2M))
                                    SetGwBorderGreen(locGwIndex);
                                else
                                    SetGwBorderRed(locGwIndex);
                            }
                            ));

                            myScheme.GwPWValue[locGwIndex] = GPMPReadValue;//保存工位基础功耗
                            locStep = 13;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                String str = "工位静态功耗值：" + GPMPReadValue;
                                ShowSysMsgLabel(str);
                            }
                            ));
                            break;
                        }
                        locWaitingTimeout--;

                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 7;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("功率仪读取失败2");
                            }
                            ));
                        }
                        break;
                    case 13://将工位退出功率测量回路
                        RtnMsgValue[locGwIndex] = 0;
                        gzprotocol[locGwIndex].SendMstCmd(5, 0, 0);
                        locWaitingTimeout = 40;
                        locStep = 14;
                        break;
                    case 14://等待工位退出确认
                        if (RtnMsgValue[locGwIndex] == 5)
                        {
                            locStep = 1;
                            locGwIndex++;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("工位" + locGwIndex + "测试完成！");
                            }
                            ));
                            if (locGwIndex == SYSTEM_GW_NUM)
                            {
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ShowSysMsgLabel("工位基础功耗测试完成！");
                                }
                                ));
                                return;
                            }
                            break;
                        }
                        locWaitingTimeout--;

                        if (locWaitingTimeout <= 0)
                        {
                            locStep = 13;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("工位退出失败！");
                            }
                            ));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void InitGwAddress()
        {
            int i;
            int locStep = 1;
            int locWaitingTimeout = 100;
            while (true)
            {
                Thread.Sleep(50);
                for (i = 0; i < SYSTEM_GW_NUM; i++)
                {
                    gzprotocol[i].mkjcProtocol();
                }
                GPMProtocol.GPMProtocol();
                switch (locStep)
                {
                    case 1://设置工位地址
                        for (i = 0; i < SYSTEM_GW_NUM; i++)
                        {
                            RtnMsgValue[i] = 0;
                            if (myScheme.gzmoduleType[i] != 0)
                            {
                                if (i < 9)
                                    gzprotocol[i].SendMstCmd(14, i + 1, 0);
                                else
                                    gzprotocol[i].SendMstCmd(14, 0x10, 0);
                            }
                        }
                        locStep = 2;
                        locWaitingTimeout = 50;
                        break;
                    case 2://等待地址设置确认
                        for (i = 0; i < SYSTEM_GW_NUM; i++)
                        {
                            if (myScheme.gzmoduleType[i] != 0)
                            {
                                if (RtnMsgValue[i] != 14)
                                {
                                    break;
                                }

                            }
                            if (i == SYSTEM_GW_NUM - 1)
                            {
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ShowSysMsgLabel("工位地址初始化成功！");
                                }
                                ));
                                return;
                            }
                        }

                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            dispatchStep = 1;//等待超时，重复当前步骤。
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                for (i = 0; i < SYSTEM_GW_NUM; i++)
                                {
                                    if (myScheme.gzmoduleType[i] != 0)
                                    {
                                        if (RtnMsgValue[i] != 14)
                                            gzmodelhandle[i].GzBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                                    }
                                }
                                ShowSysMsgLabel("工位地址初始化失败！");
                            }
                            ));
                        }
                        break;
                }
            }
        }

        private int InitGWScheme(int locGwIndex)//locGwIndex范围0-9
        {
            int locStep = 1;
            int ViewVal, locWaitingTimeout = 100;
            if (gzprotocol[locGwIndex].locModuleObj.InitSchemeOk != 0) return 0;
            while (true)
            {
                Thread.Sleep(50);
                gzprotocol[locGwIndex].mkjcProtocol();
                switch (locStep)
                {
                    case 1://设置工位方案，需通过工位管理类进行信息管理
                        gzprotocol[locGwIndex].SendMstCmd(3, myScheme.gzmoduleType[locGwIndex], myScheme.gzProductId[locGwIndex]);
                        gzprotocol[locGwIndex].locModuleObj.ModuleType = myScheme.gzmoduleType[locGwIndex];
                        locStep = 2;
                        locWaitingTimeout = 100;
                        break;
                    case 2://等待工位响应
                        if (RtnMsgValue[locGwIndex] == 3)
                        {
                            gzprotocol[locGwIndex].locModuleObj.InitSchemeOk = 1;//置初始化成功
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                gzmodelhandle[locGwIndex].GzBorder.BorderBrush = new SolidColorBrush(Colors.Black);
                                ShowSysMsgLabel("工位" + (locGwIndex + 1) + "方案初始化成功!");
                                G12.Visibility = Visibility.Visible;
                                R12.Visibility = Visibility.Hidden;
                            }
                            ));
                            return 0;
                        }
                        locWaitingTimeout--;
                        if (locWaitingTimeout <= 0)
                        {
                            ViewVal = locGwIndex;
                            if (RtnMsgValue[locGwIndex] != 3)
                            {
                                gzprotocol[locGwIndex].SendMstCmd(3, myScheme.gzmoduleType[locGwIndex], myScheme.gzProductId[locGwIndex]);
                                WaitingTimeout = 100;
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    gzmodelhandle[ViewVal].GzBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                                    ShowSysMsgLabel("工位" + (locGwIndex + 1) + "方案初始化失败!");
                                    R12.Visibility = Visibility.Visible;
                                    G12.Visibility = Visibility.Hidden;
                                }
                                ));
                            }
                            return -1;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void GwCheckDispatch()
        {
            /********************************************************************************************************
             * 单相 / 三相：
             * （1）功率测试仪HOLD OFF，读取功率值；（2）将指定工位切入测试仪，读取功率值；（3）功率测试仪HOLD ON
             * （4）通过抄控器发送指令（5）读取功率值，工位退出；（6）投入衰减器，发送抄读指令；
             * （7）电源升到13V，维持2秒，期间发送抄读指令；
             * （8）电源降到11V，维持2秒，期间发送抄读指令；（9）恢复到12V
             * ********************************************************************************************************/
            int TempIntValue;
            //判断工位是否需要初始化方案，如果需要，优先进行工位的方案初始化
            for (int i = 0; i < SYSTEM_GW_NUM; i++)
            {
                if (gzprotocol[i].locModuleObj.InitSchemeOk == 0)//如果发现有需要初始化的工位
                {
                    if (myScheme.gzmoduleType[i] != 0)
                    {
                        InitGWScheme(i);
                        gzprotocol[i].locModuleObj.InitSchemeOk = 2;
                    }
                    /*for (int j = 0; j < SYSTEM_GW_NUM; j++)//如果所有工位都已初始化，则显示初始化完成
                    {
                        if (gzprotocol[j].locModuleObj.InitSchemeOk == 0)
                        {
                            return;
                        }
                        if (j == SYSTEM_GW_NUM-1)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("系统初始化完成!");
                            }
                            ));
                        }
                    }*/
                }
            }

            //找出准备就绪的工位，写入队列，确保不重复写入
            for (int i = 0; i < SYSTEM_GW_NUM; i++)
            {
                if (myScheme.gzmoduleType[i] != 0)
                {
                    if (gzprotocol[i].locModuleObj.InFiFo == 0)//如果工位不在队列中
                    {
                        if (gzprotocol[i].locModuleObj.ModuleReady == 1)//如果工位准备就绪，则加入队列
                        {
                            gzprotocol[i].locModuleObj.InFiFo = 1;
                            PutFiFo(i);
                        }
                    }
                }
            }
            if (CurrentGwIndex < 0)//若当前检测空闲，则读取待检队列工位号
            {
                CurrentGwIndex = GetFiFo();
                Dispatcher.Invoke(new Action(delegate
                {
                    ShowCurrentGwIndex(CurrentGwIndex + 1);
                }));
            }
            else
            {
                //判断需要检测项,如果当前无检测项，则取出未检测项
                if (gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem < 0)
                {
                    for (int i = 0; i < CheckTypeNum; i++)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.SchemeCheckItem[i];
                        if (CheckItem < 0) break;//若没有检测项则跳出
                        if ((gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem] >= 0) &&
                            (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] == 0))
                        {
                            //注意，这里是惟一的检测项设置位置，如其他地方存在，请删除。
                            gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = CheckItem;
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 1;//如果是新的检测项，则将检测步骤置复归。
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckItemTaskLock = 1;//默认加锁
                            gzprotocol[CurrentGwIndex].locModuleObj.IndividualTimeOut = INDIVIDUL_TIMEOUT;
                            int TempCurrentGwIndex = CurrentGwIndex;
                            Dispatcher.Invoke(new Action(delegate
                            {
                                CurrentCheckName.Content = myScheme.CheckTypeName[gzprotocol[TempCurrentGwIndex].locModuleObj.CurrentCheckItem];
                            }
                            ));
                            break;
                        }
                    }
                    //若没有新的待检项，则将该工位置完成标志，并由检测队列中清除，
                    if (gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem < 0)
                    {
                        int tempCurrentGwIndex = CurrentGwIndex;
                        gzprotocol[CurrentGwIndex].locModuleObj.TotalResult = 0;
                        for (int i = 0; i < CheckTypeNum; i++)
                        {
                            if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[i] == 2)
                            {
                                gzprotocol[CurrentGwIndex].locModuleObj.TotalResult = 2;//模块故障
                            }
                        }

                        if (gzprotocol[CurrentGwIndex].locModuleObj.TotalResult == 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.TotalResult = 1;//模块检测通过
                            //gzprotocol[CurrentGwIndex].SendMstCmd(13, 1, 0);
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                CheckFinishedFun(tempCurrentGwIndex, 1);
                            }
                            ));
                        }
                        else
                        {
                            //gzprotocol[CurrentGwIndex].SendMstCmd(13, 0, 0);
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                CheckFinishedFun(tempCurrentGwIndex, -1);
                            }
                            ));
                        }
                        //Thread.Sleep(100);
                        gzprotocol[CurrentGwIndex].locModuleObj.InFiFo = 0;//清除队列标志
                        gzprotocol[CurrentGwIndex].locModuleObj.ModuleReady = 0;//清除模块就绪标志
                        CurrentGwIndex = GetFiFo();//检测新的模块

                        if (CurrentGwIndex >= 0)
                        {   //添加模块切换显示
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ShowCurrentGwIndex(CurrentGwIndex + 1);
                            }));
                            gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;//初始化检测项
                        }
                    }
                }
                else
                {//超时判断
                    TempIntValue = CurrentGwIndex;
                    if (gzprotocol[CurrentGwIndex].locModuleObj.IndividualTimeOut > 0)
                        gzprotocol[CurrentGwIndex].locModuleObj.IndividualTimeOut--;
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        TheCheckTimeOut.Content = gzprotocol[TempIntValue].locModuleObj.IndividualTimeOut;
                    }
                    ));
                    int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                    if (gzprotocol[CurrentGwIndex].locModuleObj.IndividualTimeOut <= 0)
                    {
                        int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];
                        if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] == 0)
                        {
                            SetItemLabelRed(TempIntValue, Viewitem);
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 2;
                        }
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                    }
                    else
                    {
                        if (gzprotocol[CurrentGwIndex].locModuleObj.CheckItemTaskLock == 0)//如果任务未闭锁，则可终止当前任务
                        {
                            if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] != 0)
                            {
                                gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                            }
                        }
                    }
                }
                if ((CurrentGwIndex < 0) || (CurrentGwIndex > 9)) return;//工位号错误，返回。
                if (gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem < 0) return;//无检测项，返回。

                switch (gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem)
                {
                    case 0://载波通信
                        ZBCommTest();
                        break;
                    case 1://载波衰减
                        ZBSJCommTest();
                        break;
                    case 2://静态功耗
                        StaticPWTest();
                        break;
                    case 3://动态功耗
                        DynamicPWTest();
                        break;
                    case 4://电源波动
                        DCPowerTest();
                        break;
                    case 5://网口测试
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckItemTaskLock = 0;
                        break;
                    case 6://GPRS通信测试
                        //ZBCommTest();
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckItemTaskLock = 0;
                        break;
                    case 12://GPRS加热测试
                        JIARETest();
                        break;
                    case 16://宽带ID读取
                        HPLCIDRead();
                        break;
                    default:
                        break;
                }
            }
        }

        private void HPLCIDRead()//读取单三相模块ID
        {
            int chaokongqiindex = myScheme.gzChaoKongType[CurrentGwIndex];
            switch (gzprotocol[CurrentGwIndex].locModuleObj.CheckStep)
            {
                case 1://发送ID读取指令
                    //I型集中器载波
                    if (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                    {
                        gzprotocol[CurrentGwIndex].SendMstCmd(2, 0, 0);
                        WaitingTimeout = 150;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                        break;
                    }

                    //单三相，通过抄控器发送645指令
                    RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] = 0;
                    Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].MasterAskID645(CurrentGwIndex + 1);

                    WaitingTimeout = 250;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                    break;
                case 2://等待通信结果
                    //I型集中器载波
                    if (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                    {
                        if (RtnMsgValue[CurrentGwIndex] == 2)//ID Function
                        {
                            int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                            int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];
                            SetItemLabelGreen(CurrentGwIndex, Viewitem);
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                            gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                            break;
                        }
                        WaitingTimeout--;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            String str = "ID读取超时:" + WaitingTimeout;
                            ShowSysMsgLabel(str);
                        }
                        ));
                        if (WaitingTimeout <= 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 1;
                        }
                        break;
                    }
                    if (RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] == 0xFF)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];
                        SetItemLabelGreen(CurrentGwIndex, Viewitem);
                        gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        break;
                    }
                    WaitingTimeout--;
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        String str = "ID读取超时:" + WaitingTimeout;
                        ShowSysMsgLabel(str);
                    }
                    ));
                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 1;
                    }
                    break;
            }
        }

        private void ZBCommTest()
        {
            int chaokongqiindex = myScheme.gzChaoKongType[CurrentGwIndex];
            switch (gzprotocol[CurrentGwIndex].locModuleObj.CheckStep)
            {
                case 1:
                    //ChaokongqiControl(chaokongqiindex, 1);
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                    break;
                case 2://发送载波通信指令
                    //I型集中器载波
                    if (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                    {
                        gzprotocol[CurrentGwIndex].SendMstCmd(12, 0, 0);
                        WaitingTimeout = 150;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        break;
                    }

                    //单三相，通过抄控器发送645指令
                    RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] = 0;
                    if (myScheme.gzProductId[CurrentGwIndex] == 4)//698协议模块
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend698Num(CurrentGwIndex + 1, 3);
                    }
                    else
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend645Num(CurrentGwIndex + 1, 3);
                    }
                    WaitingTimeout = 250;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                    break;
                case 3://等待通信结果
                    //I型集中器载波
                    if (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] != 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                            break;
                        }
                        WaitingTimeout--;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            String str = "载波通信超时:" + WaitingTimeout;
                            ShowSysMsgLabel(str);
                        }
                        ));
                        if (WaitingTimeout <= 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                        }
                        break;
                    }
                    int RtnVal = RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]];
                    if ((RtnVal >= 2) && (RtnVal != 0xFF))
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];
                        SetItemLabelGreen(CurrentGwIndex, Viewitem);
                        gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        break;
                    }
                    WaitingTimeout--;
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        String str = "载波通信超时:" + WaitingTimeout;
                        ShowSysMsgLabel(str);
                    }
                    ));
                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                    }
                    break;
            }
        }

        private void ZBSJCommTest()//载波衰减测试
        {
            int chaokongqiindex = myScheme.gzChaoKongType[CurrentGwIndex];
            switch (gzprotocol[CurrentGwIndex].locModuleObj.CheckStep)
            {
                case 1:
                    ChaokongqiControl(chaokongqiindex, 1);
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                    break;
                case 2://发送载波通信指令
                    if (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                    {
                        gzprotocol[CurrentGwIndex].SendMstCmd(8, 0, 0);
                        WaitingTimeout = 150;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        break;
                    }

                    //单三相，通过抄控器发送645指令
                    RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] = 0;
                    Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend645Num(CurrentGwIndex + 1, 3);
                    WaitingTimeout = 250;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                    break;
                case 3://等待通信结果
                    if (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] != 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("衰减测试成功！");
                            }
                            ));
                            break;
                        }
                        WaitingTimeout--;

                        if (WaitingTimeout <= 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("衰减测试失败！");
                            }
                            ));
                        }
                        break;
                    }
                    if (RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] >= 2)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];
                        SetItemLabelGreen(CurrentGwIndex, Viewitem);
                        gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("衰减测试成功！");
                        }
                        ));
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("衰减测试失败！");
                        }
                        ));
                    }
                    break;
                default:
                    break;
            }
        }

        private void StaticPWTest()//静态功耗测试
        {
            switch (gzprotocol[CurrentGwIndex].locModuleObj.CheckStep)
            {
                case 1:
                    if (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 1)
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend645Num(CurrentGwIndex + 1, 0);
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 20;
                    break;
                case 20://功率测试仪HOLD OFF
                    RtnMsgValue[11] = 0;
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        GLPanelView(3, "", 0);
                    }));
                    GPMProtocol.GPMControl(7);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;

                    break;
                case 2://等待功率仪状态确认
                    if (RtnMsgValue[11] == 7)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 20;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            SysMsgLabel.Content = "功率仪控制失败";
                        }
                        ));
                    }
                    break;
                case 3://读取功率测试仪
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(10);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 4;
                    break;
                case 4://等待功率仪读数确认
                    if (RtnMsgValue[11] == 10)
                    {
                        if (GPMPReadValue < (decimal)0.05)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                        }
                        else
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                            for (int i = 0; i < SYSTEM_GW_NUM; i++)
                            {
                                if (myScheme.gzmoduleType[i] != 0)
                                {
                                    gzprotocol[i].SendMstCmd(5, 0, 0);
                                }
                            }
                            Thread.Sleep(500);
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                String str = "发现工位没有退出功率测量！";
                                ShowSysMsgLabel(str);
                            }
                        ));
                        }

                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            SysMsgLabel.Content = "功率仪读取失败3";
                        }
                        ));
                    }
                    break;
                case 5://将工位投入功率测量回路
                    RtnMsgValue[CurrentGwIndex] = 0;
                    gzprotocol[CurrentGwIndex].SendMstCmd(5, 1, 0);
                    WaitingTimeout = 40;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 6;
                    break;
                case 6://等待工位投入确认
                    if (RtnMsgValue[CurrentGwIndex] == 5)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                        Thread.Sleep(500);
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            String str = "工位投入失败！";
                            ShowSysMsgLabel(str);
                        }
                        ));
                    }
                    break;
                case 7:
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(10);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 8;
                    break;
                case 8://等待功率仪读数
                    if (RtnMsgValue[11] == 10)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];
                        decimal Threshold = myScheme.ModuleStaticPW[gzprotocol[CurrentGwIndex].locModuleObj.ModuleType];
                        if (GPMPReadValue < myScheme.GwPWValue[CurrentGwIndex])
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("工位" + CurrentGwIndex + "功率异常！");
                            }
                            ));
                        }
                        GPMPReadValue = GPMPReadValue - myScheme.GwPWValue[CurrentGwIndex];
                        if (GPMPReadValue < Threshold)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                            SetItemLabelGreen(CurrentGwIndex, Viewitem);
                        }
                        else
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 2;
                            SetItemLabelRed(CurrentGwIndex, Viewitem);

                        }
                        gzprotocol[CurrentGwIndex].locModuleObj.resultValue[CheckItem] = GPMPReadValue;
                        //gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 13;
                        decimal TempGPMPReadValue = GPMPReadValue;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            GLPanelView(3, "", TempGPMPReadValue);
                        }));
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪读取失败4");
                        }
                        ));
                    }
                    break;
                case 13://将工位退出功率测量回路
                    RtnMsgValue[CurrentGwIndex] = 0;
                    gzprotocol[CurrentGwIndex].SendMstCmd(5, 0, 0);
                    WaitingTimeout = 40;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 14;
                    break;
                case 14://等待工位退出确认
                    if (RtnMsgValue[CurrentGwIndex] == 5)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位退出成功！");
                        }
                        ));
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 13;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位退出失败！");
                        }
                        ));
                    }
                    break;
                default:
                    break;
            }
        }

        private void DynamicPWTest()//动态功耗测试
        {
            switch (gzprotocol[CurrentGwIndex].locModuleObj.CheckStep)
            {
                case 1://功率测试仪HOLD OFF
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(7);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        GLPanelView(4, "", 0);
                    }));
                    break;
                case 2://等待功率仪状态确认
                    if (RtnMsgValue[11] == 7)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            SysMsgLabel.Content = "功率仪控制失败";
                        }
                        ));
                    }
                    break;
                case 3://读取功率测试仪
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(10);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 4;
                    break;
                case 4://等待功率仪读数确认
                    if (RtnMsgValue[11] == 10)
                    {
                        if (GPMPReadValue < (decimal)0.05)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                        }
                        else
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                            for (int i = 0; i < SYSTEM_GW_NUM; i++)
                            {
                                if (myScheme.gzmoduleType[i] != 0)
                                {
                                    gzprotocol[i].SendMstCmd(5, 0, 0);
                                }
                            }
                            Thread.Sleep(500);
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("发现工位没有退出功率测量！");
                            }
                        ));
                        }

                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪读取失败5");
                        }
                        ));
                    }
                    break;
                case 5://将工位投入功率测量回路
                    RtnMsgValue[CurrentGwIndex] = 0;
                    gzprotocol[CurrentGwIndex].SendMstCmd(5, 1, 0);
                    WaitingTimeout = 40;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 6;
                    break;
                case 6://等待工位投入确认
                    if (RtnMsgValue[CurrentGwIndex] == 5)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                        Thread.Sleep(500);
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位投入失败！");
                        }
                        ));
                    }
                    break;
                case 7://功率测试仪HOLD ON
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(8);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 8;
                    break;
                case 8://等待功率仪状态确认
                    if (RtnMsgValue[11] == 8)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 9;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪控制失败！");
                        }
                        ));
                    }
                    break;
                case 9://发送载波通信指令
                    if ((gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 4)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 5))
                    {
                        gzprotocol[CurrentGwIndex].SendMstCmd(7, 0, 0);
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 10;
                        WaitingTimeout = 200;
                        break;
                    }
                    RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] = 0;

                    if (myScheme.gzProductId[CurrentGwIndex] == 4)//698协议模块
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend698Num(CurrentGwIndex + 1, 3);
                    }
                    else
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend645Num(CurrentGwIndex + 1, 3);
                    }
                    WaitingTimeout = 200;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 10;
                    break;
                case 10://等待通信结果
                    if ((gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 4)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 5))
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] != 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                            break;
                        }
                        WaitingTimeout--;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            String str = "通信超时:" + WaitingTimeout;
                            ShowSysMsgLabel(str);
                        }
                            ));
                        if (WaitingTimeout <= 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 9;
                        }
                        break;
                    }
                    if (RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] >= 1)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                        break;
                    }
                    WaitingTimeout--;
                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 9;
                    }
                    break;
                case 11:
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(10);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 12;
                    break;
                case 12://等待功率仪读数
                    if (RtnMsgValue[11] == 10)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];

                        decimal Threshold = myScheme.ModuleDynamicPw[gzprotocol[CurrentGwIndex].locModuleObj.ModuleType];
                        if (GPMPReadValue < myScheme.GwPWValue[CurrentGwIndex])
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("工位" + CurrentGwIndex + "功率异常！");
                            }
                            ));
                        }
                        GPMPReadValue = GPMPReadValue - myScheme.GwPWValue[CurrentGwIndex];
                        if (GPMPReadValue < Threshold)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                            SetItemLabelGreen(CurrentGwIndex, Viewitem);
                        }
                        else
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 2;
                            SetItemLabelRed(CurrentGwIndex, Viewitem);

                        }
                        gzprotocol[CurrentGwIndex].locModuleObj.resultValue[CheckItem] = GPMPReadValue;
                        //gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 13;
                        decimal TempGPMPReadValue = GPMPReadValue;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            GLPanelView(4, "", TempGPMPReadValue);
                        }));
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪读取失败6");
                        }
                        ));
                    }
                    break;
                case 13://将工位退出功率测量回路
                    RtnMsgValue[CurrentGwIndex] = 0;
                    gzprotocol[CurrentGwIndex].SendMstCmd(5, 0, 0);
                    WaitingTimeout = 40;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 14;
                    break;
                case 14://等待工位退出确认
                    if (RtnMsgValue[CurrentGwIndex] == 5)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位退出成功！");
                        }
                        ));
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 13;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位退出失败！");
                        }
                        ));
                    }
                    break;
                default:
                    break;
            }
        }

        private void DCPowerTest()
        {
            switch (gzprotocol[CurrentGwIndex].locModuleObj.CheckStep)
            {
                case 1://直流源升压
                    gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation = 1;
                    RtnMsgValue[12] = 0;
                    IVYTProtocol.IVYTControl(4);
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                    WaitingTimeout = 100;
                    break;
                case 2://等待直流源确认
                    if (RtnMsgValue[12] == 13)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        WaitingTimeout = 20;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("直流源控制失败！");
                        }
                        ));
                    }
                    break;
                case 3://延时1秒
                    WaitingTimeout--;
                    if (WaitingTimeout <= 0)
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 4;
                    break;

                case 4://发送载波通信指令
                    if ((gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 4)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 5))
                    {
                        gzprotocol[CurrentGwIndex].SendMstCmd(9, 0, 0);
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                        WaitingTimeout = 250;
                        break;
                    }
                    RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] = 0;
                    Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend645Num(CurrentGwIndex + 1, 3);
                    if (myScheme.gzProductId[CurrentGwIndex] == 4)//698协议模块
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend698Num(CurrentGwIndex + 1, 3);
                    }
                    else
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend645Num(CurrentGwIndex + 1, 3);
                    }
                    WaitingTimeout = 500;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                    break;
                case 5://等待通信结果
                    if ((gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 4)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 5))
                    {
                        int CheckItem1 = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem1] != 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem1] = 0;
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 6;
                            break;
                        }
                        WaitingTimeout--;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            String str = "通信超时:" + WaitingTimeout;
                            ShowSysMsgLabel(str);
                        }
                        ));
                        if (WaitingTimeout <= 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation = 13;
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;//升压测试未通过，直接恢复电压后退出
                        }
                        break;
                    }
                    if (RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] >= 2)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 6;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("电源升压测试通过！");
                        }
                        ));
                        break;
                    }
                    WaitingTimeout--;
                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation = 13;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;//升压测试未通过，直接恢复电压后退出
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("电源升压测试失败！");
                        }
                        ));
                    }
                    break;
                case 6://直流源降压
                    RtnMsgValue[12] = 0;
                    gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation = 2;
                    IVYTProtocol.IVYTControl(2);
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                    WaitingTimeout = 500;
                    break;
                case 7://等待直流源确认
                    if (RtnMsgValue[12] == 11)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 8;
                        WaitingTimeout = 20;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 6;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("直流源控制失败！");
                        }
                        ));
                    }
                    break;
                case 8://延时1秒
                    WaitingTimeout--;
                    if (WaitingTimeout <= 0)
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 9;
                    break;

                case 9://发送通信指令
                    if ((gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 4)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 5))
                    {
                        gzprotocol[CurrentGwIndex].SendMstCmd(9, 0, 0);
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 10;
                        WaitingTimeout = 400;
                        break;
                    }
                    RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] = 0;
                    if (myScheme.gzProductId[CurrentGwIndex] == 4)
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend698Num(CurrentGwIndex + 1, 3);
                    }
                    else
                    {
                        Dlt645Protocol[myScheme.gzChaoKongType[CurrentGwIndex]].SetSend645Num(CurrentGwIndex + 1, 3);
                    }

                    WaitingTimeout = 500;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 10;
                    break;
                case 10://等待通信结果
                    if ((gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 3)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 4)
                        || (gzprotocol[CurrentGwIndex].locModuleObj.ModuleType == 5))
                    {
                        int CheckItem1 = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        if (gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem1] != 0)
                        {
                            //gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                            break;
                        }
                        WaitingTimeout--;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            String str = "通信超时:" + WaitingTimeout;
                            ShowSysMsgLabel(str);
                        }
                        ));
                        if (WaitingTimeout <= 0)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation = 11;
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                        }
                        break;
                    }
                    if (RtnMsgValue[13 + myScheme.gzChaoKongType[CurrentGwIndex]] >= 2)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("电源降压测试通过！");
                        }
                        ));
                        break;
                    }
                    WaitingTimeout--;
                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation = 11;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("电源降压测试失败！");
                        }
                        ));
                    }
                    break;
                case 11://恢复电压12V
                    RtnMsgValue[12] = 0;
                    IVYTProtocol.IVYTControl(3);
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 12;
                    WaitingTimeout = 100;
                    break;
                case 12://等待直流源确认
                    if (RtnMsgValue[12] == 12)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 13;
                        WaitingTimeout = 20;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("直流源控制失败！");
                        }
                        ));
                    }
                    break;
                case 13://处理结果
                    int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                    int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];
                    if (gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation <= 2)
                    {
                        SetItemLabelGreen(CurrentGwIndex, Viewitem);
                        gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("电源检测通过！");
                        }
                        ));
                    }
                    else
                    {
                        SetItemLabelRed(CurrentGwIndex, Viewitem);
                        gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 2;
                        gzprotocol[CurrentGwIndex].locModuleObj.resultValue[CheckItem] = gzprotocol[CurrentGwIndex].locModuleObj.DCPowerCheckStation;
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("电源检测失败！");
                        }
                        ));
                    }
                    break;
                default:
                    break;
            }
        }

        private void JIARETest()
        {
            switch (gzprotocol[CurrentGwIndex].locModuleObj.CheckStep)
            {
                case 1://功率测试仪HOLD OFF
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(7);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 2;
                    break;
                case 2://等待功率仪状态确认
                    if (RtnMsgValue[11] == 7)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪控制失败！");
                        }
                        ));
                    }
                    break;
                case 3://读取功率测试仪
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(10);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 4;
                    break;
                case 4://等待功率仪读数确认
                    if (RtnMsgValue[11] == 10)
                    {
                        if (GPMPReadValue < (decimal)0.05)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                        }
                        else
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                            for (int i = 0; i < SYSTEM_GW_NUM; i++)
                            {
                                if (myScheme.gzmoduleType[i] != 0)
                                {
                                    gzprotocol[i].SendMstCmd(5, 0, 0);
                                }
                            }
                            Thread.Sleep(500);
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("发现工位没有退出功率测量！");
                            }
                        ));
                        }

                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 3;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪读取失败7");
                        }
                        ));
                    }
                    break;
                case 5://将工位投入功率测量回路
                    RtnMsgValue[CurrentGwIndex] = 0;
                    gzprotocol[CurrentGwIndex].SendMstCmd(5, 1, 0);
                    WaitingTimeout = 40;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 6;
                    break;
                case 6://等待工位投入确认
                    if (RtnMsgValue[CurrentGwIndex] == 5)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                        Thread.Sleep(500);
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 5;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位投入失败！");
                        }
                        ));
                    }
                    break;

                case 7://功率测试仪HOLD ON
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(8);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 8;
                    break;
                case 8://等待功率仪状态确认
                    if (RtnMsgValue[11] == 8)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 9;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪控制失败！");
                        }
                        ));
                    }
                    break;
                case 9://发送加热指令
                    RtnMsgValue[CurrentGwIndex] = 0;
                    gzprotocol[CurrentGwIndex].SendMstCmd(11, 1, 0);
                    WaitingTimeout = 40;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 10;
                    break;
                case 10://加热确认
                    if (RtnMsgValue[CurrentGwIndex] == 11)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 20;
                        WaitingTimeout = 20;
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 9;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("加热控制失败！");
                        }
                        ));
                    }
                    break;
                case 20://延时
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                    }
                    break;
                case 11://读取功率值
                    RtnMsgValue[11] = 0;
                    GPMProtocol.GPMControl(10);
                    WaitingTimeout = 20;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 12;
                    break;
                case 12://等待功率仪读数
                    if (RtnMsgValue[11] == 10)
                    {
                        int CheckItem = gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem;
                        int Viewitem = gzprotocol[CurrentGwIndex].locModuleObj.CheckItemToViewIndex[CheckItem];

                        decimal Threshold = 0.5M;
                        if (GPMPReadValue < myScheme.GwPWValue[CurrentGwIndex])
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 7;
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowSysMsgLabel("工位" + CurrentGwIndex + "功率异常！");
                            }
                            ));
                        }
                        GPMPReadValue = GPMPReadValue - myScheme.GwPWValue[CurrentGwIndex];
                        if (GPMPReadValue > Threshold)
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 1;
                            SetItemLabelGreen(CurrentGwIndex, Viewitem);
                        }
                        else
                        {
                            gzprotocol[CurrentGwIndex].locModuleObj.checkresult[CheckItem] = 2;
                            SetItemLabelRed(CurrentGwIndex, Viewitem);

                        }
                        gzprotocol[CurrentGwIndex].locModuleObj.resultValue[CheckItem] = GPMPReadValue;
                        //gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 13;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("加热测试完成！");
                        }
                        ));
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 11;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("功率仪读取失败8");
                        }
                        ));
                    }
                    break;
                case 13://将工位退出功率测量回路
                    RtnMsgValue[CurrentGwIndex] = 0;
                    gzprotocol[CurrentGwIndex].SendMstCmd(5, 0, 0);
                    WaitingTimeout = 40;
                    gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 14;
                    break;
                case 14://等待工位退出确认
                    if (RtnMsgValue[CurrentGwIndex] == 5)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CurrentCheckItem = -1;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位退出成功！");
                        }
                        ));
                        break;
                    }
                    WaitingTimeout--;

                    if (WaitingTimeout <= 0)
                    {
                        gzprotocol[CurrentGwIndex].locModuleObj.CheckStep = 13;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowSysMsgLabel("工位退出失败！");
                        }
                        ));
                    }
                    break;
                default:
                    break;
            }
        }

        private void DataReceived(int Uart, int Function, byte[] msg)
        {
            //TODO:根据通信协议解析报文
            if (Uart == 23)//16口网络继电器
            {
                RtnMsgValue[23] = Function;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowSysMsgLabel("NET16继电器遥控成功！");
                }
                ));
            }
            if (Uart == 24)//4口网络继电器
            {
                RtnMsgValue[24] = Function;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowSysMsgLabel("NET4继电器遥控成功！");
                }
                ));
            }
            if (Uart > 22) return;

            int ObjType = myScheme.SchemeUart[Uart];

            if (ObjType == 11)//功率测试仪端口
            {
                switch (Function)
                {
                    case 6://初始化结束
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            GLPanelView(1, "已开启", 0);

                        }));
                        break;
                    case 7://锁存取消
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            GLPanelView(2, "锁存取消", 0);
                        }));
                        break;
                    case 8://锁存开启
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            GLPanelView(2, "锁存开启", 0);
                        }));
                        break;
                    case 10://返回静态测量值
                        GPMPReadValue = pubclass.FromBytes(msg);
                        //GPMPReadValue = result / 1000;
                        /*Dispatcher.BeginInvoke(new Action(delegate
                        {
                            GLPanelView(3, "", GPMPReadValue);
                        }));*/
                        break;
                    case 11://返回动态测量值
                        GPMPReadValue = pubclass.FromBytes(msg);
                        //GPMPReadValue = result / 1000;
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            GLPanelView(4, "", GPMPReadValue);
                        }));
                        break;

                    default:
                        break;
                }
                RtnMsgValue[ObjType] = Function;
            }
            if (ObjType == 12)//直流源端口
            {
                decimal result = pubclass.FromBytes(msg);
                RtnMsgValue[ObjType] = (int)result;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    DCPWPanelView(2, "", result);
                }));
            }
            if ((ObjType >= 13) && (ObjType <= 17))//抄控器端口
            {
                RtnMsgValue[ObjType] = Function;
                if (Function == 0xFF)//宽带载波模拟抄控器读取单三相模块芯片ID
                {
                    //msg[1]为工位号，注意如果值为0x10，表示10
                    int gwindex = msg[1];
                    if (gwindex > SYSTEM_GW_NUM) gwindex = SYSTEM_GW_NUM;
                    gwindex = gwindex - 1;
                    //msg[14]开始，为ID首地址
                    byte[] byteArray = new byte[24];
                    for (int i = 0; i < 24; i++)
                        byteArray[i] = msg[37 - i];
                    gzprotocol[gwindex].HPLC_ID_String = ByteToHexStr(byteArray);
                }

                Dispatcher.BeginInvoke(new Action(delegate
                {
                    String str = "抄控器端口：" + Function;
                    ShowSysMsgLabel(str);
                }));
            }
            if ((ObjType >= 1) && (ObjType <= 10))//工装端口
            {
                gzrtnHandle(ObjType - 1, Function, msg);
            }
            return;
        }

        private void gzrtnHandle(int gzIndex1, int Function, byte[] msg)
        {
            int gzIndex = gzIndex1;
            switch (Function)
            {
                case FUN_ACTIVE_RPT:
                    byte status_no = msg[3];
                    byte status = msg[4];
                    //解析主动上报的工装状态
                    switch (status_no)
                    {
                        case 0x01://终端方案状态
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                GwSchemeStation(gzIndex, status);
                            }
                            ));
                            break;
                        case 0x02://模块插入
                            if (gzprotocol[gzIndex].locModuleObj.CurrentCheckItem == 4)//电源波动测试中
                            {
                                if (gzprotocol[gzIndex].locModuleObj.DCPowerCheckStation == 1)
                                    gzprotocol[gzIndex].locModuleObj.DCPowerCheckStation = 13;
                                if (gzprotocol[gzIndex].locModuleObj.DCPowerCheckStation == 2)
                                    gzprotocol[gzIndex].locModuleObj.DCPowerCheckStation = 11;
                                break;
                            }

                            if (status == 0xFF)
                            {
                                //int moduleType1 = myScheme.gzmoduleType[gzIndex];

                                //gzprotocol[gzIndex].locModuleObj.ModuleReady = 1;
                                int moduleType1 = myScheme.gzmoduleType[gzIndex];
                                switch (moduleType1)
                                {
                                    case 1:
                                    case 2:
                                        break;
                                    case 3:
                                        gzprotocol[gzIndex].locModuleObj.ModuleReady = 1;
                                        break;
                                    case 4:
                                    case 5:
                                        gzprotocol[gzIndex].locModuleObj.ModuleReady = 1;
                                        break;
                                    default:
                                        break;
                                }
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ModuleInsert(gzIndex);
                                }
                                ));
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ResetItemLabelColor(gzIndex);
                                }
                                ));
                                if (CurrentGwIndex == gzIndex)
                                    CurrentGwIndex = -1;
                                gzprotocol[gzIndex].locModuleObj.IniModuleObj();
                            }

                            break;
                        case 0x03://功率仪投入状态
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                GLOnlineStation(gzIndex, status);
                            }
                            ));
                            break;
                        case 0x04://"模块类型不匹配",暂无效;
                            break;
                        //case 0x07://
                        //break;
                        case 0x0F://加热状态
                            int Viewitem1 = gzprotocol[gzIndex].locModuleObj.CheckItemToViewIndex[myScheme.StationCode[status_no]];
                            int CheckItem1 = myScheme.StationCode[status_no];

                            if (status == 0xFF)
                            {
                                gzprotocol[gzIndex].locModuleObj.checkresult[CheckItem1] = 1;
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    SetItemLabelGreen(gzIndex, Viewitem1);
                                }
                                ));
                            }
                            break;
                        case 0x11://"工位重启动"
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                GwRestartErr(gzIndex);
                                ResetItemLabelColor(gzIndex);
                            }
                            ));
                            if (CurrentGwIndex == gzIndex)
                                CurrentGwIndex = -1;
                            gzprotocol[gzIndex].locModuleObj.IniModuleObj();

                            if (myScheme.gzmoduleType[gzIndex] != 0)
                            {
                                gzprotocol[gzIndex].locModuleObj.InitSchemeOk = 0;//置方案未初始化标志
                            }
                            break;
                        //case 0x12:

                        //break;
                        default:
                            //MessageBox.Show("状态类型："+ status_no);
                            if (myScheme.StationCode[status_no] > 15) break;
                            //通过工位号，得到模块类型
                            int moduleType = myScheme.gzmoduleType[gzIndex];
                            switch (moduleType)
                            {
                                case 1:
                                case 2:
                                    //if (status_no == S_EVENT_TEST)S_RESET_TEST)
                                    if (status_no == S_RESET_TEST)
                                    //if (status_no == S_MODULE_INSERT)
                                    {
                                        if (status == 0xFF) gzprotocol[gzIndex].locModuleObj.ModuleReady = 1;
                                    }
                                    break;
                                case 3:
                                    //if (status_no == S_RESET_TEST)
                                    if (status_no == S_MODULE_INSERT)
                                    {
                                        if (status == 0xFF) gzprotocol[gzIndex].locModuleObj.ModuleReady = 1;
                                    }
                                    break;
                                case 4:
                                case 5:
                                    /*if (status_no == S_ONOFF_TEST)
                                    //if (status_no == S_MODULE_INSERT)
                                    {
                                        if (status == 0xFF) gzprotocol[gzIndex].locModuleObj.ModuleReady = 1;
                                    }*/
                                    break;
                                default:
                                    break;

                            }
                            int CheckItem = myScheme.StationCode[status_no];
                            int Viewitem = gzprotocol[gzIndex].locModuleObj.CheckItemToViewIndex[CheckItem];

                            if (status_no == 0x12)
                            {
                                CheckItem = myScheme.StationCode[status_no];
                            }
                            if (status_no == 9)
                            {
                                CheckItem = myScheme.StationCode[status_no];
                            }
                            if (status == 0xFF)
                            {
                                if (status_no == 7)
                                {
                                    gzprotocol[gzIndex].locModuleObj.checkresult[CheckItem] = 1;
                                }
                                gzprotocol[gzIndex].locModuleObj.checkresult[CheckItem] = 1;
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    SetItemLabelGreen(gzIndex, Viewitem);
                                }
                                ));
                            }
                            else
                            {
                                gzprotocol[gzIndex].locModuleObj.checkresult[CheckItem] = 2;
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    SetItemLabelRed(gzIndex, Viewitem);
                                }
                                ));
                            }
                            break;
                    }
                    break;
                case FUN_SETUP_SCHEME:
                    RtnMsgValue[gzIndex] = Function;
                    gzprotocol[gzIndex].locModuleObj.SoftVersion = msg[7] + "." + msg[6];
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ShowGwSoftVersion(gzIndex, gzprotocol[gzIndex].locModuleObj.SoftVersion);
                    }
                    ));
                    break;
                case FUN_READ_ID://工装读取I型集中器模块芯片ID
                    RtnMsgValue[gzIndex] = Function;

                    byte[] byteArray = new byte[24];
                    for (int i = 0; i < 24; i++)
                        byteArray[i] = msg[27 - i];
                    gzprotocol[gzIndex].HPLC_ID_String = ByteToHexStr(byteArray);
                    //msg[4];//24个字节的ID
                    break;
                default:
                    RtnMsgValue[gzIndex] = Function;
                    break;
            }

        }
        /***************************************************按钮区*************************************************/

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow win1 = new LoginWindow();
            win1.ShowDialog();
            StartBtn.IsEnabled = true;
            SchemeBtn.IsEnabled = true;
        }

        private void SchemeBtn_Click(object sender, RoutedEventArgs e)//基础初始化
        {
            if (InitRunning != 0)
            {
                MessageBox.Show("基础信息更新过程进行中！");
                return;
            }

            InitRunning = 1;

            int rtn = initPlc();
            if (rtn < 0)
            {
                MessageBox.Show("因端口原因，初始化失败！");
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                ResetItemLabelColor(i);
            }
            Thread thread = new Thread(() =>
            {
                for (int i = 0; i < SYSTEM_GW_NUM; i++)
                {
                    myScheme.gzmoduleType[i] = i + 1;
                    gzprotocol[i].locModuleObj.InitSchemeOk = 0;
                }
                PowerControl(1);//打开电源
                GPMPControl();
                InitGwAddress();
                InitGwPWValue();
                InitRunning = 0;
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private Thread mainThread;
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow._Select_task_id <= 0)
            {
                MessageBox.Show("请选择方案!");
                return;
            }
            if (SystemSrarted == 0)
            {
                mainThread = new Thread(() =>
                {
                    PlcProcess();
                });
                ConfigBtn.IsEnabled = false;
                StartBtn.Header = "停止检测";
                //启动条码枪监听
                bargunHook.Start();

                int rtn = initPlc();
                if (rtn < 0)
                {
                    MessageBox.Show("因端口原因，初始化失败！");
                    return;
                }

                for (int i = 0; i < SYSTEM_GW_NUM; i++)
                {
                    gzprotocol[i].locModuleObj.InitSchemeOk = 0;
                }
                mainThread.IsBackground = true;
                mainThread.Start();
                SystemSrarted = 1;
                ResetCheckProc = 1;
            }
            else//关闭直流源，串口等设备
            {
                ConfigBtn.IsEnabled = true;
                //直流源输出关闭
                byte[] OUTP0 = new byte[] { 0x3a, 0x0a, 0x4f, 0x55, 0x54, 0x50, 0x20, 0x30, 0x0a };
                NET16IOProtocol.RelayTrip(15);//关闭绿灯
                NET16IOProtocol.NET16IOProtocol();//强制发送plc命令
                Thread.Sleep(500);
                IVYTProtocol.InitializePorts(myScheme.SchemeUart[12]);
                IVYTProtocol.WritePorts(myScheme.SchemeUart[12], OUTP0);

                try { mainThread.Abort(); } catch { }
                StartBtn.Header = "开始检测";
                SystemSrarted = 0;
            }
        }
        //直流源上电
        private void DCPW_BTN_Click(object sender, RoutedEventArgs e)
        {
            IVYTProtocol.IVYTControl(7);
        }
        //直流源断电
        private void GongLv_Btn_Click(object sender, RoutedEventArgs e)
        {
            IVYTProtocol.IVYTControl(8);
        }
        //全部重新检测
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Dlt645Protocol.SetSendNum(5);

            if (SystemSrarted == 0) return;
            ResetCheckProc = 1;
            Dispatcher.BeginInvoke(new Action(delegate
            {
                for (int i = 0; i < SYSTEM_GW_NUM; i++)
                {
                    RtnMsgValue[i] = 0;
                    if (myScheme.gzmoduleType[i] != 0)
                    {
                        gzprotocol[i].SendMstCmd(4, 0, 0);

                        ResetItemLabelColor(i);
                    }
                    gzprotocol[i].locModuleObj.IniModuleObj();
                    gzprotocol[i].CheckingHPLC_ID = false;
                }
            }
            ));
        }

        private void TripBtn1_Click(object sender, RoutedEventArgs e)
        {
            NET16IOProtocol.RelayTrip(1);
        }

        private void CloseBtn1_Click(object sender, RoutedEventArgs e)
        {
            NET16IOProtocol.RelayClose(1);
        }

        /***************************************************接口代码区*************************************************/
        //显示当前被检测工位
        private void ShowCurrentGwIndex(int GwIndex)
        {
            if (GwIndexLable.Content.ToString() != GwIndex.ToString())
            {
                GwIndexLable.Content = GwIndex;
            }
        }


        private void ShowGwSoftVersion(int gwIndex, String Version)
        {
            gzmodelhandle[gwIndex].VerLable.Content = Version;
        }
        private void SetupgwCheckItem()
        {
            //初始化myScheme.SchemeCheckItem二维表,进行检测方案初始化
            int i = 1;
            string sqlstring = "select task_id, scheme_name from test_scheme";
            List<String> returnList = new List<String> { };
#if true
            returnList = MySQLHelper.SQLselect(sqlstring, 2);
            while (returnList.Count != 0)//遍历表中数据
            {
                if (_Select_task_id != i)
                    i++;
                else
                {
                    if (newschemeflag == 0)
                    {
                        int.TryParse(returnList[2 * i - 2], out _Select_task_id);
                        Scheme_Name = returnList[2 * i - 1];
                    }
                    else
                    {
                        int.TryParse(returnList[returnList.Count - 2], out _Select_task_id);
                        Scheme_Name = returnList[returnList.Count - 1];
                    }
                    break;
                }
                if (returnList.Count == i)
                    break;
            }

#endif

            i = 0;
            int j = 0;
            sqlstring = "select * from task where task_id = " + _Select_task_id;

            returnList.Clear();
            returnList = MySQLHelper.SQLselect(sqlstring, 5);

            for (int k = 0; k < 10; k++)
            {
                myScheme.gzmoduleType[k] = 0;
            }

            while (returnList.Count != 0)//遍历表中数据
            {
                //MessageBox.Show(reader[1].ToString() + "---" + reader[2].ToString() + "---" + reader[3].ToString());
                //将读到的类型写入变量
                int id = 0;
                int.TryParse(returnList[j + 3], out id);
                int style = 0;
                int.TryParse(returnList[j + 1], out style);

                myScheme.gzmoduleType[id - 1] = style;

                //将读到的检测项写入变量
                int item = 0;
                int.TryParse(returnList[j + 2], out item);


                int flag = 0;
                for (int index = 0; index < CheckTypeNum; index++)
                {
                    if (myScheme.SchemeCheckItem[style, index] == item)
                        flag = 1;
                }
                if (flag == 0)
                {
                    myScheme.SchemeCheckItem[style, i] = item;
                    i++;
                }



                /*
                buff = item;
                if (item == buff)
                {
                    //buff = style;
                    i = 0;
                    myScheme.SchemeCheckItem[style, i] = item;
                }
                else
                {
                    myScheme.SchemeCheckItem[style, i] = item;
                }
                */
                j = j + 5;
                if (returnList.Count == j)
                    break;
            }


        }


        //检测完成接口
        private void CheckFinishedFun(int gwIndex, int resu1t)
        {
            //判断模块条形码是否已经扫描，没有则阻塞直至扫描完成
            if (gzmodelhandle[gwIndex].GzTiaoma.Content.ToString() == "")
            {
                gzmodelhandle[gwIndex].GzTiaoma.Background = new SolidColorBrush(Colors.Red);
            }
            if (gzmodelhandle[gwIndex].barcode.Content.ToString() == "")
            {
                gzmodelhandle[gwIndex].barcode.Background = new SolidColorBrush(Colors.Red);
            }
        }

        //设置工位边框为红色
        private void SetGwBorderRed(int gwIndex)
        {
            gzmodelhandle[gwIndex].GzBorder.BorderBrush = new SolidColorBrush(Colors.Red);
        }
        //设置工位边框为绿色
        private void SetGwBorderGreen(int gwIndex)
        {
            gzmodelhandle[gwIndex].GzBorder.BorderBrush = new SolidColorBrush(Colors.Green);
        }

        //功率仪状态接口
        private void GLPanelView(int ViewType, String ViewStr, decimal Value)
        {
            switch (ViewType)
            {
                case 1://功率仪状态
                    GLStation.Content = ViewStr;
                    break;
                case 2://功率仪锁存状态
                    GLLockStation.Content = ViewStr;
                    break;
                case 3://功率仪读数
                    JTGLValue.Content = Value;
                    break;
                case 4://功率仪读数
                    DTGLValue.Content = Value;
                    break;
                default:
                    break;
            }
        }
        //直流源状态接口
        private void DCPWPanelView(int ViewType, String ViewStr, decimal Value)
        {
            switch (ViewType)
            {
                case 1://直流源状态

                    break;
                case 2://直流源输出
                    DCPVValue.Content = Value;
                    break;
                default:
                    break;
            }
        }

        //工位方案显示
        private void GwSchemeStation(int gzIndex, byte status)
        {
            if (status == 0x00)
            {
                gzmodelhandle[gzIndex].ItemLabel1.Content = "方案没有初始化";
                gzmodelhandle[gzIndex].ItemLabel1.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                if ((status >= 1) && (status <= 5))
                    gzmodelhandle[gzIndex].SchemeName.Content = myScheme.CommModelName[status];
                else
                    gzmodelhandle[gzIndex].SchemeName.Content = "未知类型";
            }
        }
        //工位功率源切入状态
        private void GLOnlineStation(int gzIndex, byte stn)
        {
            if (stn == 0xFF)
                gzmodelhandle[gzIndex].GLOnline.Content = "在线";
            else
                gzmodelhandle[gzIndex].GLOnline.Content = "离线";
        }
        //工位模块插入状态
        private void ModuleInsert(int gzIndex)
        {
            ResetItemLabelColor(gzIndex);
            gzmodelhandle[gzIndex].GzBorder.BorderBrush = new SolidColorBrush(Colors.Yellow);
        }

        private void GwRestartErr(int gzIndex)//工位重启处理
        {
        }
        //系统信息监视窗口
        private void ShowSysMsgLabel(String str)
        {
            SysMsgLabel.Content = str;
        }

        //复归工位状态
        private void ResetItemLabelColor(int gzIndex)
        {
            gzmodelhandle[gzIndex].GzBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            gzmodelhandle[gzIndex].ItemLabel1.Background = null;
            gzmodelhandle[gzIndex].ItemLabel2.Background = null;
            gzmodelhandle[gzIndex].ItemLabel3.Background = null;
            gzmodelhandle[gzIndex].ItemLabel4.Background = null;
            gzmodelhandle[gzIndex].ItemLabel5.Background = null;
            gzmodelhandle[gzIndex].ItemLabel6.Background = null;
            gzmodelhandle[gzIndex].ItemLabel7.Background = null;
            gzmodelhandle[gzIndex].ItemLabel8.Background = null;


            gzmodelhandle[gzIndex].GzTiaoma.Content = "";
            gzmodelhandle[gzIndex].barcode.Content = "";
            gzmodelhandle[gzIndex].GzTiaoma.Background = null;
            gzmodelhandle[gzIndex].barcode.Background = null;
            //gzmodelhandle[gzIndex].GzTiaoma.Background = new SolidColorBrush(Colors.Transparent);
            //gzmodelhandle[gzIndex].barcode.Background = new SolidColorBrush(Colors.Transparent);
        }
        //设置工位相关检测项为绿色
        private void SetItemLabelGreen(int gzIndex, int index)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                switch (index)
                {
                    case 0:
                        gzmodelhandle[gzIndex].ItemLabel1.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case 1:
                        gzmodelhandle[gzIndex].ItemLabel2.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case 2:
                        gzmodelhandle[gzIndex].ItemLabel3.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case 3:
                        gzmodelhandle[gzIndex].ItemLabel4.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case 4:
                        gzmodelhandle[gzIndex].ItemLabel5.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case 5:
                        gzmodelhandle[gzIndex].ItemLabel6.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case 6:
                        gzmodelhandle[gzIndex].ItemLabel7.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case 7:
                        gzmodelhandle[gzIndex].ItemLabel8.Background = new SolidColorBrush(Colors.Green);
                        break;
                    default:
                        break;
                }
            }
            ));
        }

        private void SchemeButton_Click(object sender, RoutedEventArgs e)
        {
            CfgWindow cfg = new CfgWindow();
            cfg.ShowDialog();

            if (cfg.DialogResult == true)
            {
                SchemeBtn.IsEnabled = false;
                //SchemeBtn.Content = "设置完成";
                StartBtn.IsEnabled = true;

                //从数据库中取出配置好的方案，并显示
                string sqlstring = "select workplace_id, module_id, test_term_id from task";

                List<String> returnList = new List<string> { };
                int i = 0;
                returnList = MySQLHelper.SQLselect(sqlstring, 3);
                while (returnList.Count != 0)//遍历表中数据
                {
                    //MessageBox.Show(reader[0].ToString() + "、" + reader[1].ToString() + "、" + reader[2].ToString());
                    switch (returnList[i])
                    {
                        case "1":
                            parse_style_use(gzmodel1, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "2":
                            parse_style_use(gzmodel2, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "3":
                            parse_style_use(gzmodel3, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "4":
                            parse_style_use(gzmodel4, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "5":
                            parse_style_use(gzmodel5, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "6":
                            parse_style_use(gzmodel6, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "7":
                            parse_style_use(gzmodel7, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "8":
                            parse_style_use(gzmodel8, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "9":
                            parse_style_use(gzmodel9, returnList[i + 1], returnList[i + 2]);
                            break;
                        case "10":
                            parse_style_use(gzmodel10, returnList[i + 1], returnList[i + 2]);
                            break;
                    }
                    i = i + 3;
                    if (returnList.Count == i)
                        break;
                }
            }
            else if (cfg.DialogResult == false)
            {
                SchemeBtn.IsEnabled = true;
                StartBtn.IsEnabled = false;
            }

#if false
            if (SystemSrarted != 0)
            {
                MessageBox.Show("确定重新设置方案吗？");
            }
            for (int i = 0; i < 10; i++)
            {
                gzprotocol[i].SendMstCmd(3);
                gzmodelhandle[i].gwId.Content = "工位" + (i+1);
                gzmodelhandle[i].SchemeName.Content = "未知";
                for (int j = 0; j < 16; j++)
                {
                    int item = myScheme.ViewIndex[j];
                    switch (item)
                    {
                        case 1:
                            gzmodelhandle[i].ItemLabel1.Content = myScheme.CheckTypeName[j];
                            break;
                        case 2:
                            gzmodelhandle[i].ItemLabel2.Content = myScheme.CheckTypeName[j];
                            break;
                        case 3:
                            gzmodelhandle[i].ItemLabel3.Content = myScheme.CheckTypeName[j];
                            break;
                        case 4:
                            gzmodelhandle[i].ItemLabel4.Content = myScheme.CheckTypeName[j];
                            break;
                        case 5:
                            gzmodelhandle[i].ItemLabel5.Content = myScheme.CheckTypeName[j];
                            break;
                        case 6:
                            gzmodelhandle[i].ItemLabel6.Content = myScheme.CheckTypeName[j];
                            break;
                        case 7:
                            gzmodelhandle[i].ItemLabel7.Content = myScheme.CheckTypeName[j];
                            break;
                        case 8:
                            gzmodelhandle[i].ItemLabel8.Content = myScheme.CheckTypeName[j];
                            break;
                        default:
                            break;
                    }
                }
            }
#endif
        }



        public void parse_style_use(UCDetect ucDetect, string module, string item)
        {
#if false
            switch (module)
            {
                case "1":
                    switch (item)
                    {
                        case "1":
                            ucDetect.label1.Content = "载波通信";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "2":
                            ucDetect.label1.Content = "载波通信";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "3":
                            ucDetect.label2.Content = "载波衰减";
                            ucDetect.label2.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "6":
                            ucDetect.label3.Content = "静态功耗测试";
                            ucDetect.label3.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "7":
                            ucDetect.label4.Content = "动态功耗测试";
                            ucDetect.label4.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "8":
                            ucDetect.label5.Content = "电源波动测试";
                            ucDetect.label5.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "10":
                            //ucDetect.label7.Content = "插入测试";
                            //ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "11":
                            ucDetect.label6.Content = "RESET管脚";
                            ucDetect.label6.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "12":
                            //ucDetect.label9.Content = "/SET管脚";
                            //ucDetect.label9.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "13":
                            //ucDetect.label10.Content = "STA管脚";
                            //ucDetect.label10.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "14":
                            ucDetect.label7.Content = "EVENT管脚";
                            ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                    }
                    break;
                case "2":
                    switch (item)
                    {
                        case "1":
                            ucDetect.label1.Content = "载波通信";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "2":
                            ucDetect.label1.Content = "载波通信";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "3":
                            ucDetect.label2.Content = "载波衰减";
                            ucDetect.label2.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "6":
                            ucDetect.label3.Content = "静态功耗测试";
                            ucDetect.label3.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "7":
                            ucDetect.label4.Content = "动态功耗测试";
                            ucDetect.label4.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "8":
                            ucDetect.label5.Content = "电源波动测试";
                            ucDetect.label5.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "10":
                            //ucDetect.label7.Content = "插入测试";
                            //ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "11":
                            ucDetect.label6.Content = "RESET管脚";
                            ucDetect.label6.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "12":
                            //ucDetect.label9.Content = "/SET管脚";
                            //ucDetect.label9.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "13":
                            //ucDetect.label10.Content = "STA管脚";
                            //ucDetect.label10.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "14":
                            ucDetect.label7.Content = "EVENT管脚";
                            ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                    }
                    break;
                case "3":
                    switch (item)
                    {
                        case "1":
                            ucDetect.label1.Content = "载波通信";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "2":
                            ucDetect.label1.Content = "载波通信";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "3":
                            ucDetect.label2.Content = "载波衰减";
                            ucDetect.label2.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "6":
                            ucDetect.label3.Content = "静态功耗测试";
                            ucDetect.label3.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "7":
                            ucDetect.label4.Content = "动态功耗测试";
                            ucDetect.label4.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "8":
                            ucDetect.label5.Content = "电源波动测试";
                            ucDetect.label5.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "9":
                            //ucDetect.label7.Content = "STATE测试";
                            //ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "11":
                            ucDetect.label6.Content = "RESET管脚";
                            ucDetect.label6.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                    }
                    break;
                case "4":
                    switch (item)
                    {
                        case "4":
                            ucDetect.label1.Content = "网口测试";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "5":
                            ucDetect.label2.Content = "GPRS测试";
                            ucDetect.label2.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "6":
                            ucDetect.label3.Content = "静态功耗测试";
                            ucDetect.label3.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "7":
                            ucDetect.label4.Content = "动态功耗测试";
                            ucDetect.label4.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "8":
                            ucDetect.label5.Content = "电源波动测试";
                            ucDetect.label5.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "9":
                            //ucDetect.label6.Content = "STATE测试";
                            //ucDetect.label6.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "12":
                            //ucDetect.label7.Content = "/SET管脚";
                            //ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "15":
                            ucDetect.label6.Content = "加热控制";
                            ucDetect.label6.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "16":
                            ucDetect.label7.Content = "ON/OFF控制";
                            ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                    }
                    break;
                case "5":
                    switch (item)
                    {
                        case "4":
                            ucDetect.label1.Content = "网口测试";
                            ucDetect.label1.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "5":
                            ucDetect.label2.Content = "GPRS测试";
                            ucDetect.label2.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "6":
                            ucDetect.label3.Content = "静态功耗测试";
                            ucDetect.label3.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "7":
                            ucDetect.label4.Content = "动态功耗测试";
                            ucDetect.label4.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "8":
                            ucDetect.label5.Content = "电源波动测试";
                            ucDetect.label5.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "9":
                            //ucDetect.label6.Content = "STATE测试";
                            //ucDetect.label6.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "12":
                            //ucDetect.label7.Content = "/SET管脚";
                            //ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "15":
                            ucDetect.label6.Content = "加热控制";
                            ucDetect.label6.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "16":
                            ucDetect.label7.Content = "ON/OFF控制";
                            ucDetect.label7.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                    }
                    break;
            }
#endif
        }


        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            //条码枪终止工作
            bargunHook.Stop();
        }

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowHistoryWindow2 showHistoryWindow = new ShowHistoryWindow2();
            showHistoryWindow.Owner = this;
            showHistoryWindow.ShowDialog();
        }

        private void UserBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RootAuth == 1)
            {
                RegisterAuthorizeWindow registerAuthorizeWindow = new RegisterAuthorizeWindow();
                registerAuthorizeWindow.Owner = this;
                registerAuthorizeWindow.ShowDialog();
            }
            else
            {
                ModifyWindow modifyWindow = new ModifyWindow();
                modifyWindow.Owner = this;
                modifyWindow.ShowDialog();
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();

            RootAuth = 0;
            ConfigAuth = 0;
            LogAuth = 0;
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void SystemBtn_Click(object sender, RoutedEventArgs e)
        {
            SystemConfigWindow systemConfigWindow = new SystemConfigWindow();
            systemConfigWindow.Owner = this;
            systemConfigWindow.ShowDialog();
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO
            //MessageBox.Show("TEST");
            /*string seletsqlstr = $"select * from plc_scheme.test_result where module_barcode='633005410127180000787'";
            DataTable dt = MySQLHelper.SQLSelect(seletsqlstr);
            if (dt.Rows.Count > 0) 
            {

            }*/
            DebugWindow debugWin = new DebugWindow();
            debugWin.Owner = this;
            debugWin.ShowDialog();

        }

        private void ResultBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确定提交？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                ResultBtn.IsEnabled = false;
                Thread thread = new Thread(() =>
                {
                    string errinfo = "";
                    bool updateflag = false;
                    lock (obj)
                    {
                        //   updateflag = mds.UpdateMDSData(out errinfo);
                    }
                    Thread.Sleep(2000);
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ResultBtn.IsEnabled = true;
                    }
                    ));
                    if (updateflag == true)
                    {
                        MessageBox.Show("结果上传成功！");
                    }
                    else
                    {
                        MessageBox.Show("结果上传失败！" + errinfo);
                    }
                });
                thread.IsBackground = true;
                thread.Start();

            }
            else
            {
                //Nothing to do!
            }
        }

        private void InputBtn_Click(object sender, RoutedEventArgs e)
        {
            BarcodeConfirmWindow barcodeConfirmWindow = new BarcodeConfirmWindow();
            barcodeConfirmWindow.Owner = this;
            barcodeConfirmWindow.ShowDialog();
        }

        private void ControlBox_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                NET16IOProtocol.RelayTrip(15);//关闭绿灯
                NET16IOProtocol.NET16IOProtocol();//强制发送plc命令
                Thread.Sleep(500);
            }
            catch (Exception)
            {
            }
        }

        //设置工位相关检测项为红色
        private void SetItemLabelRed(int gzIndex, int index)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                switch (index)
                {
                    case 0:
                        gzmodelhandle[gzIndex].ItemLabel1.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case 1:
                        gzmodelhandle[gzIndex].ItemLabel2.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case 2:
                        gzmodelhandle[gzIndex].ItemLabel3.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case 3:
                        gzmodelhandle[gzIndex].ItemLabel4.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case 4:
                        gzmodelhandle[gzIndex].ItemLabel5.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case 5:
                        gzmodelhandle[gzIndex].ItemLabel6.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case 6:
                        gzmodelhandle[gzIndex].ItemLabel7.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case 7:
                        gzmodelhandle[gzIndex].ItemLabel8.Background = new SolidColorBrush(Colors.Red);
                        break;
                    default:
                        break;
                }
            }));
        }
    }
}
