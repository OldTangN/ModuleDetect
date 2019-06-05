using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using System.IO.Ports;
using MySql.Data.MySqlClient;

namespace mkjcApp
{
    /// <summary>
    /// CfgWindow.xaml 的交互逻辑
    /// </summary>

    public partial class CfgWindow : Window
    {
        public decimal PWBaseValue = 0;
        public CfgWindow()
        {
            InitializeComponent();

            string sqlstring = "select module_style from module_style";
            sql_module_style_select(SW1, sqlstring);
            //sql_module_style_select(SW2, sqlstring);
            //sql_module_style_select(SW3, sqlstring);
            //sql_module_style_select(SW4, sqlstring);
            //sql_module_style_select(SW5, sqlstring);
            //sql_module_style_select(SW6, sqlstring);
            //sql_module_style_select(SW7, sqlstring);
            //sql_module_style_select(SW8, sqlstring);
            //sql_module_style_select(SW9, sqlstring);
            //sql_module_style_select(SW10, sqlstring);

            sqlstring = "select scheme_name from test_scheme";
            sql_module_style_select(DefaultConfig, sqlstring);


            sqlstring = "select id from test_scheme";
            List<String> returnList = new List<string> { };

            returnList = MySQLHelper.SQLselect(sqlstring, 1);
            int i = 0;
            while (returnList.Count != 0)//遍历表中数据
            {
                //MessageBox.Show(reader[0].ToString() + "、" + reader[1].ToString());
                string id = returnList[i];
                int.TryParse(id, out MainWindow._Task_id);
                i++;
                if (returnList.Count == i)
                    break;
            }

            SW1.Items.Insert(0, "---请选择---");
            SW1.SelectedIndex = 0;
            //SW2.Items.Insert(0, "---请选择---");
            //SW2.SelectedIndex = 0;
            //SW3.Items.Insert(0, "---请选择---");
            //SW3.SelectedIndex = 0;
            //SW4.Items.Insert(0, "---请选择---");
            //SW4.SelectedIndex = 0;
            //SW5.Items.Insert(0, "---请选择---");
            //SW5.SelectedIndex = 0;
            //SW6.Items.Insert(0, "---请选择---");
            //SW6.SelectedIndex = 0;
            //SW7.Items.Insert(0, "---请选择---");
            //SW7.SelectedIndex = 0;
            //SW8.Items.Insert(0, "---请选择---");
            //SW8.SelectedIndex = 0;
            //SW9.Items.Insert(0, "---请选择---");
            //SW9.SelectedIndex = 0;
            //SW10.Items.Insert(0, "---请选择---");
            //SW10.SelectedIndex = 0;


            for (i = 1; i < 6; i++)
            {
                ChaoKqPort.Items.Add(i);
            }
            ChaoKqPort.Items.Insert(0, "---请选择---");
            ChaoKqPort.SelectedIndex = 0;

            SpecialModelType.Items.Insert(0, "---默认---");
            SpecialModelType.Items.Insert(1, "---I型HPLC---");
            SpecialModelType.Items.Insert(2, "---东软I型HPLC---");
            SpecialModelType.Items.Insert(3, "---东软采集器HPLC模块---");
            SpecialModelType.Items.Insert(4, "---单相窄带698模块---");
            SpecialModelType.Items.Insert(5, "---晓程窄带模块---");
            SpecialModelType.SelectedIndex = 0;

            PowerTypeCombo.Items.Insert(0, "---HPLC功耗---");
            PowerTypeCombo.Items.Insert(1, "---窄带功耗---");
            PowerTypeCombo.SelectedIndex = 0;

            //功耗补偿值选择
            PWBase.Items.Insert(0, "---默认0W---");
            PWBase.Items.Insert(1, "---0.100W---");
            PWBase.Items.Insert(2, "---0.200W---");
            PWBase.Items.Insert(3, "---0.300W---");
            PWBase.Items.Insert(4, "---0.400W---");
            PWBase.Items.Insert(5, "---0.500W---");
            PWBase.SelectedIndex = 0;

            DefaultConfig.Items.Insert(0, "---默认配置选择---");
            DefaultConfig.SelectedIndex = 0;
        }

        //数据库查询模块类型，显示
        public void sql_module_style_select(ComboBox comboBox, string sqlstring)
        {
            List<String> returnList = new List<string> { };

            returnList = MySQLHelper.SQLselect(sqlstring, 1);
            int i = 0;
            while (returnList.Count != 0)//遍历表中数据
            {
                //MessageBox.Show(reader[0].ToString() + "、" + reader[1].ToString());
                comboBox.Items.Add(returnList[i]);
                i++;
                if (returnList.Count == i)
                    break;
            }
        }

        //数据库查询模块类型的检测项，显示
        public void sql_test_item_select(StackPanel stackPanel, string sqlstring)
        {

            //先清空控件，不能用下面的foreach，删不干净
            DeletePanel(stackPanel);

#if false
            //先清空控件
            foreach (UIElement c in stackPanel.Children)
            {
                if (c is CheckBox)
                    stackPanel.Children.Remove(c);
                else if (c is Button)
                    stackPanel.Children.Remove(c);
            }
#endif

            Button all = new Button();
            all.Content = "全选";
            all.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009696"));
            all.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DD150808"));
            all.FontSize = 16;
            stackPanel.Children.Add(all);//动态添加一个按钮
            all.Click += new RoutedEventHandler(all_click);//给按钮指定点击事件

            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 2);
            int i = 0;

            while (returnList.Count != 0)//遍历表中数据
            {
                //MessageBox.Show(reader[0].ToString() + "、" + reader[1].ToString());
                //label.Text += reader[0].ToString() + "、";

                //动态添加复选框，可自定义具体的检测项
                CheckBox checkBox = new CheckBox();
                checkBox.Height = 35;
                checkBox.FontSize = 16;
                checkBox.Name = "checkBox" + returnList[i];
                checkBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                checkBox.VerticalContentAlignment = VerticalAlignment.Center;
                checkBox.Content = returnList[i + 1];
                stackPanel.Children.Add(checkBox);

                i = i + 2;
                if (returnList.Count == i)
                    break;
            }
        }

        private void all_click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;//获取被点击的控件按钮

            //判断点击的是哪个panel生成的按钮，遍历该panel，取出其中所有的复选框，并设置选中

            if (btn.Parent == Panel1)
            {
                foreach (Control c in Panel1.Children)
                {
                    if (c is CheckBox)
                        ((CheckBox)c).IsChecked = true;
                }
            }
            //else if (btn.Parent == Panel2)
            //{
            //    foreach (Control c in Panel2.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel3)
            //{
            //    foreach (Control c in Panel3.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel4)
            //{
            //    foreach (Control c in Panel4.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel5)
            //{
            //    foreach (Control c in Panel5.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel6)
            //{
            //    foreach (Control c in Panel6.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel7)
            //{
            //    foreach (Control c in Panel7.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel8)
            //{
            //    foreach (Control c in Panel8.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel9)
            //{
            //    foreach (Control c in Panel9.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
            //else if (btn.Parent == Panel10)
            //{
            //    foreach (Control c in Panel10.Children)
            //    {
            //        if (c is CheckBox)
            //            ((CheckBox)c).IsChecked = true;
            //    }
            //}
        }

        void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            NewSchemeWindow newSchemeWindow = new NewSchemeWindow();
            newSchemeWindow.ShowDialog();
            string scheme_name = newSchemeWindow.newSchemeName.Text.ToString();
            if (newSchemeWindow.DialogResult == true)
            {

                string selectStr = "select * from plc_scheme.test_scheme where  scheme_name='" + scheme_name + "'";
                if (MySQLHelper.SQLselect(selectStr, 3).Count > 0 || scheme_name == "")
                {
                    MessageBox.Show("方案名字在数据表中已存在或名称不为空");
                    newSchemeWindow.Close();
                    return;
                }
                if (scheme_name != "")
                {
                    MainWindow._Task_id++;
                    MainWindow._Select_task_id = MainWindow._Task_id;
                    MainWindow.newschemeflag = 1;
                    string sqlstring = "insert into plc_scheme.test_scheme(scheme_name, task_id) values("
                    + "'" + scheme_name + "', " + MainWindow._Task_id + ")";
                    //MessageBox.Show(sqlstring);
                    MySQLHelper.SQLinsert(sqlstring);
                }
                DefaultConfig.Items.Add(scheme_name);
            }

            //获取本次之前的检测量
#if false
            int i = 0;
            string sqlUpdate = "";
            string sqlSelect = "";
            int actual_count = 0;
            for (i=1;i<=5;i++)
            {
                sqlSelect = "select actual_count from count where style_id=" + i;
                sqlSelect = mkjcApp.MainWindow.SQLselect(sqlSelect);
                int.TryParse(sqlSelect, out actual_count);
                if (i == 1)
                    mkjcApp.MainWindow.DanXiangZaiBo = actual_count;
                else if (i == 2)
                    mkjcApp.MainWindow.SanXiangZaiBo = actual_count;
                else if (i == 3)
                    mkjcApp.MainWindow.IXingJiZhongQiZaiBo = actual_count;
                else if (i == 4)
                    mkjcApp.MainWindow.IXingJiZhongQiGPRS = actual_count;
                else
                    mkjcApp.MainWindow.IIXingJiZhongQiGPRS = actual_count;
            }
#endif

            //获取窗体上所有的设置，并保存至数据库
            string location = "";
            string module_style = "";
            string sqlUpdate = "";

#if false
            try
            {
                string sqlstring = "truncate table task";
                MySqlCommand cmd = new MySqlCommand(sqlstring, mkjcApp.MainWindow.conn);
                cmd.ExecuteNonQuery();//执行上述cmd命令
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "未知错误");
            }
#endif
            //遍历控件，查找Panel里的CheckBox
            foreach (UIElement p in this.grid.Children)
            {
                if (p is StackPanel)
                {
                    string item = "";
                    location = (p as StackPanel).Name.ToString();

                    //解析
                    int workplace_id = 0;
                    int item_id = 0;
                    switch (location)
                    {
                        case "Panel1":
                            workplace_id = 1;
                            break;
                        case "Panel2":
                            workplace_id = 2;
                            break;
                        case "Panel3":
                            workplace_id = 3;
                            break;
                        case "Panel4":
                            workplace_id = 4;
                            break;
                        case "Panel5":
                            workplace_id = 5;
                            break;
                        case "Panel6":
                            workplace_id = 6;
                            break;
                        case "Panel7":
                            workplace_id = 7;
                            break;
                        case "Panel8":
                            workplace_id = 8;
                            break;
                        case "Panel9":
                            workplace_id = 9;
                            break;
                        case "Panel10":
                            workplace_id = 10;
                            break;
                    }

                    foreach (UIElement c in (p as StackPanel).Children)
                    {
                        if (c is ComboBox)
                        {
                            module_style = ((ComboBox)c).SelectedIndex.ToString();
                            switch (module_style)
                            {
                                case "1":
                                    MainWindow.DanXiangZaiBo += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.DanXiangZaiBo + " where style_id= 1";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "2":
                                    MainWindow.SanXiangZaiBo += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.SanXiangZaiBo + " where style_id= 2";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "3":
                                    MainWindow.IXingJiZhongQiZaiBo += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.IXingJiZhongQiZaiBo + " where style_id= 3";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "4":
                                    MainWindow.IXingJiZhongQiGPRS += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.IXingJiZhongQiGPRS + " where style_id= 4";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "5":
                                    MainWindow.IIXingJiZhongQiGPRS += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.IIXingJiZhongQiGPRS + " where style_id= 5";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                            }
                        }
                        else if (c is CheckBox)
                        {
                            if (((CheckBox)c).IsChecked == true)
                            {
                                item = ((CheckBox)c).Content.ToString();
                                switch (item)
                                {
                                    case "载波通信":
                                        item_id = 0;
                                        break;
                                    case "载波衰减":
                                        item_id = 1;
                                        break;
                                    case "静态功耗":
                                        item_id = 2;
                                        break;
                                    case "动态功耗":
                                        item_id = 3;
                                        break;
                                    case "电源波动":
                                        item_id = 4;
                                        break;
                                    case "网口测试":
                                        item_id = 5;
                                        break;
                                    case "GPRS测试":
                                        item_id = 6;
                                        break;
                                    case "RESET管脚":
                                        item_id = 7;
                                        break;
                                    case "/SET管脚":
                                        item_id = 8;
                                        break;
                                    case "STA管脚":
                                        item_id = 9;
                                        break;
                                    case "/STATE测试":
                                        item_id = 10;
                                        break;
                                    case "EVENT管脚":
                                        item_id = 11;
                                        break;
                                    case "加热控制":
                                        item_id = 12;
                                        break;
                                    case "ON/OFF":
                                        item_id = 13;
                                        break;
                                    case "工位重启":
                                        item_id = 14;
                                        break;
                                    case "下行通信":
                                        item_id = 15;
                                        break;
                                    case "HPLC ID":
                                        item_id = 16;
                                        break;
                                }
                                //将新的配置内容插入到task表中

                                string sqlstring = "insert into plc_scheme.task(workplace_id, module_id, test_term_id, task_id) values("
                                        + workplace_id + ", " + module_style + ", " + item_id + ", " + mkjcApp.MainWindow._Task_id + ")";
                                MySQLHelper.SQLinsert(sqlstring);

                            }
                        }
                    }
                }
            }
            //this.Close();
            //this.DialogResult = true;
            DefaultConfig.SelectedIndex = 0;


        }


        void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            //test214.Print214 print = new test214.Print214();
            //print.PrintResult(Convert.ToInt32("330"), "0802787098", "16", MainWindow.CurrentLogin);
            //this.Close();
            this.DialogResult = false;
        }

        //清空panel上的控件
        public void DeletePanel(Panel panel)
        {
            List<UIElement> remove_list = new List<UIElement>();
            for (int i = 0; i < panel.Children.Count; i++)
            {
                UIElement c = panel.Children[i];
                if (c is CheckBox)
                {
                    remove_list.Add(c);
                }
                else if (c is Button)
                {
                    remove_list.Add(c);
                }
            }
            remove_list.ForEach(ctl => panel.Children.Remove(ctl));
        }

        //显示检测项
        public void Show_Item(int chosen, StackPanel stackPanel)
        {
            string sqlstring = "select t.id, t.term from plc_scheme.test_term t, plc_scheme.module_style m, plc_scheme.style_ref_item s where s.style_id = m.id and s.item_id = t.id and m.id = ";
            switch (chosen)
            {
                case 1:
                    sql_test_item_select(stackPanel, sqlstring + "1");
                    break;
                case 2:
                    sql_test_item_select(stackPanel, sqlstring + "1");
                    break;
                case 3:
                    sql_test_item_select(stackPanel, sqlstring + "3");
                    break;
                case 4:
                    sql_test_item_select(stackPanel, sqlstring + "4");
                    break;
                case 5:
                    sql_test_item_select(stackPanel, sqlstring + "4");
                    break;
            }
        }

        private void SW1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int module_style_index = SW1.SelectedIndex;

            if (module_style_index == 0)
            {
                DeletePanel(Panel1);
            }
            else
            {
                Show_Item(module_style_index, Panel1);
            }

        }

        //private void SW2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW2.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel2);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel2);
        //    }

        //}

        //private void SW3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW3.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel3);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel3);
        //    }
        //}

        //private void SW4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW4.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel4);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel4);
        //    }
        //}

        //private void SW5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW5.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel5);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel5);
        //    }
        //}
        //private void SW6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW6.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel6);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel6);
        //    }
        //}

        //private void SW7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW7.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel7);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel7);
        //    }
        //}

        //private void SW8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW8.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel8);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel8);
        //    }
        //}

        //private void SW9_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW9.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel9);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel9);
        //    }
        //}

        //private void SW10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int module_style_index = SW10.SelectedIndex;

        //    if (module_style_index == 0)
        //    {
        //        DeletePanel(Panel10);
        //    }
        //    else
        //    {
        //        Show_Item(module_style_index, Panel10);
        //    }
        //}

        public void SelectedCombobox(int index1, string index2, string index3)
        {
            switch (index2)
            {
                case "1":
                    SW1.SelectedIndex = index1;
                    SelectedCheckbox(Panel1, index3);
                    break;
                //case "2":
                //    SW2.SelectedIndex = index1;
                //    SelectedCheckbox(Panel2, index3);
                //    break;
                //case "3":
                //    SW3.SelectedIndex = index1;
                //    SelectedCheckbox(Panel3, index3);
                //    break;
                //case "4":
                //    SW4.SelectedIndex = index1;
                //    SelectedCheckbox(Panel4, index3);
                //    break;
                //case "5":
                //    SW5.SelectedIndex = index1;
                //    SelectedCheckbox(Panel5, index3);
                //    break;
                //case "6":
                //    SW6.SelectedIndex = index1;
                //    SelectedCheckbox(Panel6, index3);
                //    break;
                //case "7":
                //    SW7.SelectedIndex = index1;
                //    SelectedCheckbox(Panel7, index3);
                //    break;
                //case "8":
                //    SW8.SelectedIndex = index1;
                //    SelectedCheckbox(Panel8, index3);
                //    break;
                //case "9":
                //    SW9.SelectedIndex = index1;
                //    SelectedCheckbox(Panel9, index3);
                //    break;
                //case "10":
                //    SW10.SelectedIndex = index1;
                //    SelectedCheckbox(Panel10, index3);
                //    break;
            }
        }

        public void SelectedCheckbox(StackPanel stackPanel, string index)
        {
            foreach (UIElement c in stackPanel.Children)
            {
                if (c is CheckBox)
                {
                    if (((CheckBox)c).Name == "checkBox" + index)
                    {
                        ((CheckBox)c).IsChecked = true;
                    }
                }
            }
        }
        /// <summary>
        /// 选择下拉框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Default_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int default_config = DefaultConfig.SelectedIndex;
            string sqlstring = "select scheme_name, id from test_scheme";
            string schemename_selected = DefaultConfig.SelectedValue.ToString();

            List<String> returnList = new List<string> { };
            int i = 0;
            returnList = MySQLHelper.SQLselect(sqlstring, 2);
            while (returnList.Count != 0)
            {
                if (returnList[i] == schemename_selected)
                {
                    int.TryParse(returnList[i + 1], out default_config);
                    break;
                }
                i = i + 2;
                if (returnList.Count == i)
                    break;
            }

            DeletePanel(Panel1);
            //DeletePanel(Panel2);
            //DeletePanel(Panel3);
            //DeletePanel(Panel4);
            //DeletePanel(Panel5);
            //DeletePanel(Panel6);
            //DeletePanel(Panel7);
            //DeletePanel(Panel8);
            //DeletePanel(Panel9);
            //DeletePanel(Panel10);

            SW1.SelectedIndex = 0;
            //SW2.SelectedIndex = 0;
            //SW3.SelectedIndex = 0;
            //SW4.SelectedIndex = 0;
            //SW5.SelectedIndex = 0;
            //SW6.SelectedIndex = 0;
            //SW7.SelectedIndex = 0;
            //SW8.SelectedIndex = 0;
            //SW9.SelectedIndex = 0;
            //SW10.SelectedIndex = 0;

            sqlstring = "select t.workplace_id, t.module_id, t.test_term_id " +
                "from plc_scheme.task t, plc_scheme.test_scheme s where " +
                "t.task_id = s.task_id and s.id = " + default_config;

            returnList.Clear();
            returnList = MySQLHelper.SQLselect(sqlstring, 3);
            i = 0;

            while (returnList.Count != 0)
            {
                //MessageBox.Show(reader[0].ToString() + "、" + reader[1].ToString() + "、" + reader[2].ToString());
                switch (returnList[i + 1])
                {
                    case "1":
                        SelectedCombobox(1, returnList[i], returnList[i + 2]);
                        break;
                    case "2":
                        SelectedCombobox(2, returnList[i], returnList[i + 2]);
                        break;
                    case "3":
                        SelectedCombobox(3, returnList[i], returnList[i + 2]);
                        break;
                    case "4":
                        SelectedCombobox(4, returnList[i], returnList[i + 2]);
                        break;
                    case "5":
                        SelectedCombobox(5, returnList[i], returnList[i + 2]);
                        break;
                    default:
                        SelectedCombobox(MainWindow._Task_id, returnList[i], returnList[i + 2]);
                        break;
                }
                i = i + 3;
                if (returnList.Count == i)
                    break;
            }
        }
        /// <summary>
        /// 确认方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            //根据该索引，查找数据库的检测项，赋值给相关数组
            MainWindow._Select_task_id = DefaultConfig.SelectedIndex;

            PWBaseValue = (decimal)0.1 * PWBase.SelectedIndex;
            if (MainWindow._Select_task_id != 0)
            {
                if ((ChaoKqPort.SelectedIndex != 0) && (ChaoKqPort.SelectedIndex != -1) && (SpecialModelType.SelectedIndex != -1))
                {
                    int ChaoKqPortId = Convert.ToInt32(ChaoKqPort.SelectedValue);
                    int SpecialModelTypeId = SpecialModelType.SelectedIndex;
                    for (int i = 0; i < 10; i++)
                    {
                        MainWindow.myScheme.gzChaoKongType[i] = ChaoKqPortId - 1;
                        MainWindow.myScheme.gzProductId[i] = SpecialModelTypeId;
                    }
                    this.DialogResult = true;

                    if (PowerTypeCombo.SelectedIndex == 0)//HPLC功耗
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            MainWindow.myScheme.ModuleStaticPW[i] = MainWindow.myScheme.KDModuleStaticPW[i] + PWBaseValue;
                            MainWindow.myScheme.ModuleDynamicPw[i] = MainWindow.myScheme.KDModuleDynamicPw[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            MainWindow.myScheme.ModuleStaticPW[i] = MainWindow.myScheme.ZDModuleStaticPW[i] + PWBaseValue;
                            MainWindow.myScheme.ModuleDynamicPw[i] = MainWindow.myScheme.ZDModuleDynamicPw[i];
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请选择抄控器端口");
                }
            }
            else
            {
                MessageBox.Show("请选择默认方案");
            }
        }
        /// <summary>
        /// 删除方案（根据下拉框名称）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            string schemeName = DefaultConfig.SelectedValue.ToString();
            if (DefaultConfig.SelectedIndex == 0)
            {
                MessageBox.Show("请在下拉表中选择要删除的方案");
            }
            else
            {
                MessageBox.Show("是否要删除" + schemeName + "方案");
                string deleteStr = "DELETE FROM plc_scheme.test_scheme WHERE scheme_name='" + schemeName + "'";
                MySQLHelper.SQLdelete(deleteStr);
            }
            DefaultConfig.SelectedIndex = 0;
            DefaultConfig.Items.Remove(schemeName);


        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            string schemeName = DefaultConfig.SelectedValue.ToString();
            if (DefaultConfig.SelectedIndex == 0)
            {
                MessageBox.Show("请在下拉框内选择方案修改后再点击修改按钮");
                return;
            }
            string selectStr = "select task_id from plc_scheme.test_scheme where scheme_name='" + schemeName + "'";
            List<String> idList = new List<string>();
            idList = MySQLHelper.SQLselect(selectStr, 1);
            MainWindow._Task_id = int.Parse(idList[0]);
            string deleteStr = "delete from plc_scheme.task where  task_id='" + int.Parse(idList[0]) + "'";
            MySQLHelper.SQLdelete(deleteStr);

            //获取窗体上所有的设置，并保存至数据库
            string location = "";
            string module_style = "";
            string sqlUpdate = "";

            //遍历控件，查找Panel里的CheckBox
            foreach (UIElement p in this.grid.Children)
            {
                if (p is StackPanel)
                {
                    string item = "";
                    location = (p as StackPanel).Name.ToString();

                    //解析
                    int workplace_id = 0;
                    int item_id = 0;
                    switch (location)
                    {
                        case "Panel1":
                            workplace_id = 1;
                            break;
                            //case "Panel2":
                            //    workplace_id = 2;
                            //    break;
                            //case "Panel3":
                            //    workplace_id = 3;
                            //    break;
                            //case "Panel4":
                            //    workplace_id = 4;
                            //    break;
                            //case "Panel5":
                            //    workplace_id = 5;
                            //    break;
                            //case "Panel6":
                            //    workplace_id = 6;
                            //    break;
                            //case "Panel7":
                            //    workplace_id = 7;
                            //    break;
                            //case "Panel8":
                            //    workplace_id = 8;
                            //    break;
                            //case "Panel9":
                            //    workplace_id = 9;
                            //    break;
                            //case "Panel10":
                            //    workplace_id = 10;
                            //    break;
                    }

                    foreach (UIElement c in (p as StackPanel).Children)
                    {
                        if (c is ComboBox)
                        {
                            module_style = ((ComboBox)c).SelectedIndex.ToString();
                            switch (module_style)
                            {
                                case "1":
                                    MainWindow.DanXiangZaiBo += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.DanXiangZaiBo + " where style_id= 1";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "2":
                                    MainWindow.SanXiangZaiBo += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.SanXiangZaiBo + " where style_id= 2";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "3":
                                    MainWindow.IXingJiZhongQiZaiBo += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.IXingJiZhongQiZaiBo + " where style_id= 3";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "4":
                                    MainWindow.IXingJiZhongQiGPRS += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.IXingJiZhongQiGPRS + " where style_id= 4";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                                case "5":
                                    MainWindow.IIXingJiZhongQiGPRS += 1;
                                    sqlUpdate = "update count set actual_count=" + MainWindow.IIXingJiZhongQiGPRS + " where style_id= 5";
                                    MySQLHelper.SQLinsert(sqlUpdate);
                                    break;
                            }
                        }
                        else if (c is CheckBox)
                        {
                            if (((CheckBox)c).IsChecked == true)
                            {
                                item = ((CheckBox)c).Content.ToString();
                                switch (item)
                                {
                                    case "载波通信":
                                        item_id = 0;
                                        break;
                                    case "载波衰减":
                                        item_id = 1;
                                        break;
                                    case "静态功耗":
                                        item_id = 2;
                                        break;
                                    case "动态功耗":
                                        item_id = 3;
                                        break;
                                    case "电源波动":
                                        item_id = 4;
                                        break;
                                    case "网口测试":
                                        item_id = 5;
                                        break;
                                    case "GPRS测试":
                                        item_id = 6;
                                        break;
                                    case "RESET管脚":
                                        item_id = 7;
                                        break;
                                    case "/SET管脚":
                                        item_id = 8;
                                        break;
                                    case "STA管脚":
                                        item_id = 9;
                                        break;
                                    case "/STATE测试":
                                        item_id = 10;
                                        break;
                                    case "EVENT管脚":
                                        item_id = 11;
                                        break;
                                    case "加热控制":
                                        item_id = 12;
                                        break;
                                    case "ON/OFF":
                                        item_id = 13;
                                        break;
                                    case "工位重启":
                                        item_id = 14;
                                        break;
                                    case "下行通信":
                                        item_id = 15;
                                        break;
                                    case "HPLC ID":
                                        item_id = 16;
                                        break;
                                }
                                //将新的配置内容插入到task表中

                                for (int i = 1; i <= 10; i++)//10个工位同样的设置
                                {
                                    workplace_id = i;
                                    string sqlstring = "insert into plc_scheme.task(workplace_id, module_id, test_term_id, task_id) values("
                                      + workplace_id + ", " + module_style + ", " + item_id + ", " + mkjcApp.MainWindow._Task_id + ")";
                                    MySQLHelper.SQLinsert(sqlstring);
                                }

                            }
                        }
                    }

                }

            }
            DefaultConfig.SelectedIndex = 0;
            //this.Close();
            //this.DialogResult = true;

        }
    }
}
