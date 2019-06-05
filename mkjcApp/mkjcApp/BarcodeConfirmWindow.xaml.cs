using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using test214;

namespace mkjcApp
{
    /// <summary>
    /// BarcodeConfirmWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BarcodeConfirmWindow : Window
    {
        public BarcodeConfirmWindow()
        {
            InitializeComponent();
        }
        private void UpdateErrorItem(string MoKuaiTiaoMa, string itemid)
        {
            string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sqlstr = $"select * from plc_scheme.test_result where module_barcode='{MoKuaiTiaoMa}' and item='{itemid}';";//这里定义17是模块不上电
            DataTable dt = MySQLHelper.SQLSelect(sqlstr);
            string sqlup = "";
            if (dt == null || dt.Rows.Count == 0)
            {
                //insert
                sqlup = $"insert into plc_scheme.test_result(result,result_data,station_barcode,module_barcode, date, task_id, item,test_operator,verify_operator,temperature,humidity) values(" +
                                             $"2," +     //成功还是失败
                                             $"0," +     //结果值
                                             $"'0'," +  //外观 和 不上电 不用工位的id
                                             $"'{MoKuaiTiaoMa}'," +
                                             $"'{datetime}'," +
                                             $"'{MainWindow._Select_task_id}'," +
                                             $"'{itemid}'," +
                                             $"'{MainWindow.CurrentLogin}'," +
                                             $"''," +  //复检人
                                             $"''," +//温度
                                             $"''" +//湿度
                                             $");";
            }
            else
            {
                //update
                sqlup = $"update plc_scheme.test_result set result=-1 ,date={datetime}," +
                        $"test_operator={MainWindow.CurrentLogin}" +
                        $" where  module_barcode='{MoKuaiTiaoMa}' and item='{itemid}'";
            }
            if (sqlup != "")
                MySQLHelper.SQLinsert(sqlup);
        }
        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            bool MoKuaiBuShangDian = false;
            bool WaiGuanJianCeShiBai = false;
            string MoKuaiTiaoMa = "";

            if(mokuaibushangdian.IsChecked == true)
            {
                MoKuaiBuShangDian = true;
            }
            if(waiguanjiance.IsChecked == true)
            {
                WaiGuanJianCeShiBai = true;
            }
            MoKuaiTiaoMa = barcodeConfirmText.Text.ToString();

            if((MoKuaiBuShangDian || WaiGuanJianCeShiBai) && (MoKuaiTiaoMa != ""))
            {
                //查询表中是否有此barcode
                string sql = $"select * from plc_scheme.mt_detect_out_equip where BAR_CODE='{MoKuaiTiaoMa}'";
                DataTable ds = MySQLHelper.SQLSelect(sql);
                if (ds == null || ds.Rows.Count == 0)
                {
                    MessageBox.Show("无此待检条码！");
                    return;
                }
              
                string itemid="";
                Print214 print = new Print214();
                MoKuaiTiaoMa = Convert.ToInt64(MoKuaiTiaoMa.Trim().Substring(12, 10)).ToString();
                //插入数据库
                if (MoKuaiBuShangDian)
                {
                    itemid = "17";
                    UpdateErrorItem(MoKuaiTiaoMa, itemid);
                    print.PrintResult(Convert.ToInt32("330"), MoKuaiTiaoMa, itemid, MainWindow.CurrentLogin);
                }
                if(WaiGuanJianCeShiBai)
                {
                    itemid = "18";
                    UpdateErrorItem(MoKuaiTiaoMa, itemid);
                    print.PrintResult(Convert.ToInt32("330"), MoKuaiTiaoMa, itemid, MainWindow.CurrentLogin);
                }
                string errorinfo = "";
                if (!Properties.Settings.Default.LineOff) //如果不是离线 将结果更新倒临时表
                {
                    lock (MainWindow.obj)
                    {
                        MainWindow.mds.UpdataRsltForBar(out errorinfo);
                    }
                }


            }
            else
            {
                if (MoKuaiTiaoMa == "")
                    MessageBox.Show("条码不能为空！");
                if(MoKuaiBuShangDian && WaiGuanJianCeShiBai)
                    MessageBox.Show("外观检测失败、模块不上电：必须选一项！");
            }
            this.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
