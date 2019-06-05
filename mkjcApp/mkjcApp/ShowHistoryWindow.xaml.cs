using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Visifire.Charts;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Forms.Application;
using System.Text;

namespace mkjcApp
{
    /// <summary>
    /// ShowHistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShowHistoryWindow : Window
    {
        public ShowHistoryWindow()
        {
            InitializeComponent();

            AnRenWu.Items.Clear();
            AnRenWu.Items.Insert(0, "按任务查找");

            string sqlstring = "select scheme_name from test_scheme";
            int i = 1;

            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 1);
            int j = 0;

            while (returnList.Count != 0)
            {
                AnRenWu.Items.Insert(i, returnList[j]);
                i++;
                j++;
                if (returnList.Count == j)
                    break;
            }
            AnRenWu.SelectedIndex = 0;
        }

        public static int hege_index = 0;
        public static int gongwei_index = 0;
        public static int renwu_index = 0;
        public static int riqi_index = 0;

        public static int currentYear = DateTime.Now.Year;

        //检测总量统计
        private void showCount_Click(object sender, RoutedEventArgs e)
        {
            List<string> wheres = new List<string>();

            int i = 0;
            int j = 0;
            //数据库查找数据
            string sqlstring = "select test_result.id, scheme_name, result_data, date, test_operator, verify_operator, temperature, humidity, module_barcode, station_barcode, result, term from test_result, test_scheme, test_term ";
            //多条件查找，用List组SQL语句
            if (hege_index != 0)//按合格与不合格查找选中
            {
                //j++;
                wheres.Add(" result = " + (hege_index - 1).ToString());
            }
            if (gongwei_index != 0)//按工位查找选中
            {
                //j++;
                wheres.Add(" station_barcode = " + "'" + MainWindow.GzList[gongwei_index - 1] + "'");
            }
            if (renwu_index != 0)//按任务查找选中
            {
                //j++;
                wheres.Add(" test_result.task_id =  " + renwu_index.ToString());
            }
            if (FromDate.SelectedDate.ToString() != "")//按日期查找选中
            {
                //select * from test_result where date >= "2018/02/05 00:00:00" AND date <= "2018/12/06 23:59:59";
                string fromdate = FromDate.SelectedDate.ToString().Split(' ')[0] + " 00:00:00";
                wheres.Add(" date >= '" + fromdate + "'");
            }
            if (ToDate.SelectedDate.ToString() != "")//按日期查找选中
            {
                //select * from test_result where date >= "2018/02/05 00:00:00" AND date <= "2018/12/06 23:59:59";
                string todate = ToDate.SelectedDate.ToString().Split(' ')[0] + " 23:59:59";
                wheres.Add(" date <= '" + todate + "'");
            }

            wheres.Add("test_result.task_id = test_scheme.task_id and test_term.id = test_result.item ");

            //条件之间加上where和and
            if (wheres.Count != 0)
            {
                sqlstring += " where ";
                for (i = 0; i < wheres.Count; i++)
                {
                    sqlstring += wheres[i];
                    if(i < wheres.Count-1)
                        sqlstring += " and ";
                }
            }
            //MessageBox.Show(sqlstring);

            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 12);
            i = 0;
            j = 0;
            showData.Items.Clear();
            if (returnList.Count == 0)
            {
                ZongLiang.Content = "当前查询条件下的总量是：0" ;
                MessageBox.Show("当前查询条件下无数据，请重新选择！");
            }
            else
            {
                while (returnList.Count != 0)//遍历表中数据
                {
                    j++;//统计数量

                    //读数据库存储的结果数据，界面显示
                    string station_barcode = "";
                    string task_name = "";
                    string module_barcode = "";
                    string item = "";
                    string result_data = "";
                    string result = "";
                    string date = "";
                    string test_operator = "";
                    string verify_operator = "";
                    string temperature = "";
                    string humidity = "";


                    station_barcode = returnList[i+9];
                    task_name = returnList[i + 1];
                    module_barcode = returnList[i + 8];
                    item = returnList[i + 11];
                    result_data = returnList[i + 2];
                    result = returnList[i + 10];
                    if (result == "2")
                        result = "不合格";
                    else if (result == "1")
                        result = "合格";
                    date = returnList[i + 3];
                    test_operator = returnList[i + 4];
                    verify_operator = returnList[i + 5];
                    temperature = returnList[i + 6];
                    humidity = returnList[i + 7];
                    //界面动态显示解析内容
                    
                    showData.Items.Add(
                        new
                        {
                            gwtm = station_barcode,
                            jcrw = task_name,
                            mktm = module_barcode,
                            jctm = item,
                            jcjg = result_data,
                            jcjl = result,
                            jcsj = date,
                            jcry = test_operator,
                            hyry = verify_operator,
                            jcwd = temperature,
                            jcsd = humidity
                        }
                    );
                    i = i + 12;
                    if (returnList.Count == i)
                        break;
                }
                ZongLiang.Content = "当前查询条件下的总量是：" + j.ToString();
            }
        }

        private void AnHeGe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            hege_index = AnHeGe.SelectedIndex;
        }


        private void CSVButton_Click(object sender, RoutedEventArgs e)
        {
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
            SaveCSV(dataTable, @"D:\tiaoshi\mkjcApp_V01.02\mkjcApp\history\history.csv");
        }
        
        public static void SaveCSV(DataTable dt, string fullPath)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
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

            string csvpath = @"D:\tiaoshi\mkjcApp_V01.02\mkjcApp\history\history.csv";
            MessageBox.Show("CSV文件保存成功，存储路径为：\n" + csvpath);
        }



        private void AnGongWei_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gongwei_index = AnGongWei.SelectedIndex;
        }

        private void AnRenWu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            renwu_index = AnRenWu.SelectedIndex;
            if(renwu_index >0)
            {
                string sqlstring = "select task_id from test_scheme";
                List<String> returnList = new List<string> { };
                returnList = MySQLHelper.SQLselect(sqlstring, 1);
                int.TryParse(returnList[renwu_index - 1], out renwu_index);
            }
            
        }

        

#if false
        private void Count_Checked(object sender, RoutedEventArgs e)
        {
           
            //先清空，再动态添加
            AnRiQi.Items.Clear();
            AnRenWu.Items.Clear();

            AnRiQi.Items.Insert(0, "按年查找");
            AnRiQi.Items.Insert(1, (currentYear - 0).ToString() + "年");
            AnRiQi.Items.Insert(2, (currentYear - 1).ToString() + "年");
            AnRiQi.Items.Insert(3, (currentYear - 2).ToString() + "年");
            AnRiQi.Items.Insert(4, (currentYear - 3).ToString() + "年");
            AnRiQi.Items.Insert(5, (currentYear - 4).ToString() + "年");
            AnRiQi.SelectedIndex = 0;
            
            AnRenWu.Items.Insert(0, "按任务查找");
            
            string sqlstring = "select scheme_name from test_scheme";
            int i = 1;

            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 1);
            int j = 0;

            while (returnList.Count != 0)
            {
                AnRenWu.Items.Insert(i, returnList[j]);
                i++;
                j++;
                if (returnList.Count == j)
                    break;
            }
            AnRenWu.SelectedIndex = 0;
        }
#endif

        private void Log_Checked(object sender, RoutedEventArgs e)
        {
          

            //读数据库存储的结果数据，界面显示
            string station_barcode = "";
            string task_name = "";
            string module_barcode = "";
            string result_data  = "";
            string result = "";
            string date = "";
            string test_operator = "";
            string verify_operator = "";
            string temperature = "";
            string humidity = "";

            string sqlstring = "select test_result.station_barcode, " +
                "test_scheme.scheme_name, " +
                "test_result.module_barcode, " +
                "test_result.result_data, " +
                "test_result.result, " +
                "test_result.date, " +
                "test_result.test_operator, " +
                "test_result.verify_operator, " +
                "test_result.temperature, " +
                "test_result.humidity " +
                "from test_result, test_scheme " +
                "where test_result.task_id = test_scheme.task_id";
            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 10);
            int i = 0;
            while (returnList.Count != 0)//遍历表中数据
            {
                station_barcode = returnList[i];
                task_name = returnList[i + 1] ;
                module_barcode = returnList[i+2];
                result_data = returnList[i+3];
                result = returnList[i+4];
                if (result == "2")
                    result = "不合格";
                else if (result == "1")
                    result = "合格";
                date = returnList[i+5];
                test_operator = returnList[i+6];
                verify_operator = returnList[i+7];
                temperature = returnList[i+8];
                humidity = returnList[i+9];
                //界面动态显示解析内容
                showData.Items.Add(
                    new
                    {
                        gwtm = station_barcode,
                        jcrw = task_name,
                        mktm = module_barcode,
                        jcjg = result_data,
                        jcjl = result,
                        jcsj = date,
                        jcry = test_operator,
                        hyry = verify_operator,
                        jcwd = temperature,
                        jcsd = humidity
                    }
                );
                i = i + 10;
                if (returnList.Count == i)
                    break;
            }
        }
    }
}
