using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using mkjcApp.usermodel;
using MySql.Data.MySqlClient;
using Visifire.Charts;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace mkjcApp
{
    /// <summary>
    /// ShowHistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShowHistoryWindow2 : Window
    {
        public ShowHistoryWindow2()
        {
            InitializeComponent();
        }


        public static int currentYear = DateTime.Now.Year;

        List<EquipModel> _SearchedEqs = new List<EquipModel>();

        //检测总量统计
        private void showCount_Click(object sender, RoutedEventArgs e)
        {
            //数据库查找数据
            //string sqlstring = "SELECT * FROM plc_scheme.mt_detect_out_equip where 1=1 ";
            string sqlstring = "select * from (SELECT module_barcode,max(date) as date,max(test_operator) as test_operator FROM plc_scheme.test_result group by  module_barcode ) as b  "
                + "left join  plc_scheme.mt_detect_out_equip as e on b.module_barcode = e.BAR_CODE where 1=1 ";

            if (!string.IsNullOrEmpty(txtTaskNo.Text))
            {
                sqlstring += $" and DETECT_TASK_NO like '%{txtTaskNo.Text}%' ";
            }
            if (!string.IsNullOrEmpty(txtBarcode.Text))
            {
                sqlstring += $" and BAR_CODE like '%{txtBarcode.Text}%' ";
            }
            if (FromDate.SelectedDate.HasValue)
            {
                sqlstring += $" and date >= '{FromDate.SelectedDate.Value.ToString("yyyy-MM-dd")}' ";//date(YQ_date)
            }
            if (ToDate.SelectedDate.HasValue)
            {
                sqlstring += $" and date <= '{ToDate.SelectedDate.Value.ToString("yyyy-MM-dd")}' ";// date(YQ_date)
            }

            DataTable dtbl = MySQLHelper.SQLSelect(sqlstring);
            _SearchedEqs = new List<EquipModel>();
            if (dtbl != null && dtbl.Rows.Count > 0)
            {
                foreach (DataRow dr in dtbl.Rows)
                {
                    EquipModel eq = new EquipModel()
                    {
                        BAR_CODE = dr["module_barcode"]?.ToString(),
                        DETECT_TASK_NO = dr["DETECT_TASK_NO"]?.ToString(),
                        HPLC_CERT_CONC_CODE = dr["HPLC_CERT_CONC_CODE"]?.ToString(),
                        POSITION_NO = dr["POSITION_NO"]?.ToString(),
                        YQ_date = dr["date"]?.ToString(),
                        YQ_FINISH_FLAG = dr["YQ_FINISH_FLAG"]?.ToString(),
                        YQ_RSLT_FLAG = dr["YQ_RSLT_FLAG"]?.ToString(),
                        YQ_TEST_OPERATOR = dr["test_operator"]?.ToString(),
                    };
                    if (eq.HPLC_CERT_CONC_CODE == "01")
                        eq.HPLC_CERT_CONC_CODE = "成功";
                    else if (eq.HPLC_CERT_CONC_CODE == "02")
                        eq.HPLC_CERT_CONC_CODE = "失败";
                    else if (eq.HPLC_CERT_CONC_CODE == "03")
                        eq.HPLC_CERT_CONC_CODE = "未检测";

                    if (eq.YQ_RSLT_FLAG == "01")
                        eq.YQ_RSLT_FLAG = "合格";
                    else if (eq.YQ_RSLT_FLAG == "02")
                        eq.YQ_RSLT_FLAG = "合格";
                    else if (eq.YQ_RSLT_FLAG == "03")
                        eq.YQ_RSLT_FLAG = "未检测";
                    _SearchedEqs.Add(eq);
                }
            }
            gdEquip.ItemsSource = _SearchedEqs;
            ZongLiang.Content = "当前查询条件下的总量是：" + _SearchedEqs.Count;
        }

        private void CSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (_SearchedEqs.Count == 0)
            {
                return;
            }
            if (gdEquip.SelectedIndex == -1)
            {
                MessageBox.Show("请选择【有任务号】的记录！");
                return;
            }
            EquipModel selEq = gdEquip.Items[gdEquip.SelectedIndex] as EquipModel;
            if (selEq == null || string.IsNullOrEmpty(selEq.DETECT_TASK_NO))
            {
                MessageBox.Show("请选择【有任务号】的记录！");
                return;
            }

            string taskNo = selEq.DETECT_TASK_NO;
            //查询检测项名称，构造数据列
            string strSearchName = "SELECT t.term FROM plc_scheme.test_result r inner join plc_scheme.test_term t  on r.item = t.id "
                                + " where r.module_barcode in (select e.BAR_CODE from plc_scheme.mt_detect_out_equip e where e.DETECT_TASK_NO = '" + taskNo + "')  "
                                + " group by t.id,t.term order by t.id ";
            var dtblColName = MySQLHelper.SQLSelect(strSearchName);
            if (dtblColName == null && dtblColName.Rows.Count == 0)
            {
                MessageBox.Show("无检测数据！");
                return;
            }
            DataTable dtblCVS = new DataTable();
            dtblCVS.Columns.Add("行号", typeof(string));
            dtblCVS.Columns.Add("任务号", typeof(string));
            dtblCVS.Columns.Add("条码", typeof(string));
            dtblCVS.Columns.Add("检测人员", typeof(string));
            dtblCVS.Columns.Add("检测结论", typeof(string));
            dtblCVS.Columns.Add("检测时间", typeof(string));
            dtblCVS.Columns.Add("工位", typeof(string));
            foreach (DataRow dataRow in dtblColName.Rows)//添加检测项列名
            {
                dtblCVS.Columns.Add(dataRow[0].ToString(), typeof(string));
            }
            int lineNo = 0;
            foreach (var eq in _SearchedEqs)
            {
                if (eq.DETECT_TASK_NO != selEq.DETECT_TASK_NO)
                {
                    continue;
                }
                lineNo++;
                DataRow dr = dtblCVS.NewRow();
                dtblCVS.Rows.Add(dr);

                dr["行号"] = "\t" + lineNo;
                dr["任务号"] = "\t" + eq.DETECT_TASK_NO;
                dr["条码"] = "\t" + eq.BAR_CODE;
                dr["检测人员"] = "\t" + eq.YQ_TEST_OPERATOR;
                dr["检测结论"] = "\t" + eq.YQ_RSLT_FLAG;
                dr["检测时间"] = "\t" + eq.YQ_date;
                dr["工位"] = "\t" + eq.POSITION_NO;
                try
                {
                    string strSearchRlt = "SELECT t.term,r.result_data,r.result FROM plc_scheme.test_result r inner join plc_scheme.test_term t  on r.item = t.id "
                                   + " where r.module_barcode = '" + eq.BAR_CODE + "' ";
                    var dtblRlt = MySQLHelper.SQLSelect(strSearchRlt);
                    if (dtblRlt == null || dtblRlt.Rows.Count == 0)
                    {
                        continue;
                    }
                    foreach (DataRow drRlt in dtblRlt.Rows)
                    {
                        if (drRlt["term"].ToString().Contains("功耗"))
                        {
                            dr[drRlt["term"].ToString()] = "\t" + drRlt["result_data"].ToString() + " W  " + (drRlt["result"].ToString() == "1" ? "通过" : "不通过");
                        }
                        else if (drRlt["term"].ToString().Contains("HPLC"))
                        {
                            dr[drRlt["term"].ToString()] = "\t" + drRlt["result_data"].ToString() + " " + (drRlt["result"].ToString() == "1" ? "通过" : "不通过");
                        }
                        else
                        {
                            dr[drRlt["term"].ToString()] = "\t" + drRlt["result_data"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyLogLib.MyLog.WriteLog(ex);
                }
            }
            //taskNo = taskNo.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("<", "").Replace(">", "").Replace("|", "").Replace("*", "");
            SaveCSV(dtblCVS, AppDomain.CurrentDomain.BaseDirectory + @"history\" + taskNo + ".csv");
            return;

            string sqlstring = "select test_result.id, scheme_name, result_data, date, test_operator, verify_operator, temperature, humidity, module_barcode, station_barcode, result, item from test_result, test_scheme where test_result.task_id = test_scheme.task_id";
            List<String> returnList = new List<string> { };
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("行号", typeof(int));
            dataTable.Columns.Add("方案名称", typeof(String));
            dataTable.Columns.Add("结果数据", typeof(String));
            dataTable.Columns.Add("日期", typeof(String));
            dataTable.Columns.Add("检测人员", typeof(String));
            dataTable.Columns.Add("核验人员", typeof(String));
            dataTable.Columns.Add("温度", typeof(String));
            dataTable.Columns.Add("湿度", typeof(String));
            dataTable.Columns.Add("模块条码", typeof(String));
            dataTable.Columns.Add("工装条码", typeof(String));
            dataTable.Columns.Add("是否合格", typeof(String));
            dataTable.Columns.Add("检测项", typeof(String));

            returnList = MySQLHelper.SQLselect(sqlstring, 12);
            int i = 0;
            int k = 1;
            string yesorno = "";
            while (returnList.Count != 0)
            {
                if (returnList[10 + i] == "2")
                    yesorno = "不合格";
                else
                    yesorno = "合格";
                dataTable.Rows.Add(k, returnList[1 + i], returnList[2 + i], returnList[3 + i], returnList[4 + i],
                    returnList[5 + i], returnList[6 + i], returnList[7 + i], returnList[8 + i], returnList[9 + i],
                    yesorno, returnList[11 + i]);
                i = i + 12;
                k++;
                if (returnList.Count == i)
                    break;
            }
            SaveCSV(dataTable, AppDomain.CurrentDomain.BaseDirectory + @"history\history.csv");
        }

        public static void SaveCSV(DataTable dt, string fullPath)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            string data = "";
            //写出列名称
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);
            //写出各行数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string str = dt.Rows[i][j].ToString();
                    str = str.Replace("\"", "\"\"");
                    if (str.Contains(',') || str.Contains('"')
                        || str.Contains('\r') || str.Contains('\n'))
                    {
                        str = string.Format("\"{0}\"", str);
                    }
                    data += str;
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();

            string csvpath = fullPath;// @"D:\tiaoshi\mkjcApp_V01.02\mkjcApp\history\history.csv";
            MessageBox.Show("CSV文件保存成功，存储路径为：\n" + csvpath);
        }

        private void GdEquip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gdEquip.SelectedIndex != -1)
            {
                EquipModel eq = gdEquip.Items[gdEquip.SelectedIndex] as EquipModel;
                if (eq == null)
                {
                    return;
                }
                string sqlstr = "SELECT r.*,i.term,s.scheme_name FROM plc_scheme.test_result r left join plc_scheme.test_scheme s on r.task_id = s.task_id "
                    + $" left join plc_scheme.test_term i on r.item = i.id where r.module_barcode = '{eq.BAR_CODE}'";
                DataTable dtbl = MySQLHelper.SQLSelect(sqlstr);
                List<TestResultModel> lstRlts = new List<TestResultModel>();
                if (dtbl != null && dtbl.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtbl.Rows)
                    {
                        TestResultModel rlt = new TestResultModel()
                        {
                            scheme_name = dr["scheme_name"].ToString(),
                            module_barcode = dr["module_barcode"].ToString(),
                            result = dr["result"].ToString(),
                            result_data = dr["result_data"].ToString(),
                            term = dr["term"].ToString(),
                        };
                        if (rlt.result == "1")
                            rlt.result = "合格";
                        else if (rlt.result == "2")
                            rlt.result = "不合格";
                        else
                            rlt.result = "未检测";
                        if (rlt.term.Contains("功耗") && rlt.result_data != "不通过")
                        {
                            rlt.result_data += " W";
                        }
                        lstRlts.Add(rlt);
                    }
                }
                gdTestRlt.ItemsSource = lstRlts;
            }
        }
    }
}
