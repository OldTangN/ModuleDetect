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
using MySql.Data.MySqlClient;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace mkjcApp
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            //此程序只允许一个实例
            Process pCurrent = Process.GetCurrentProcess();
            Process[] pList = Process.GetProcessesByName(pCurrent.ProcessName);
            if (pList.Length >= 2)
            {
                this.Close();
            }

        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            string username = loginUser.Text;
            string passwd = loginPassword.Password;
            if (username == "" || passwd == "")
            {
                MessageBox.Show("用户名、密码不能为空！");
                return;
            }
            lblWait.Visibility = Visibility.Visible;
            Button_Login.IsEnabled = false;
            //TODO:使用webservice登录
            Thread thread = new Thread((obj)=> {
                Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow.CurrentLogin = username;
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }), null);
            });
            thread.IsBackground = true;
            thread.Start();
            //string username = loginUser.Text.ToString();
            //string number_id = loginNumber.Text.ToString();
            //string passwd = loginPassword.Password;

            //if(username == "" || number_id == "" || passwd == "")
            //{
            //    MessageBox.Show("有未填项！");
            //    return;
            //}

            //List<String> returnList = new List<string> { };
            //string sqlstring = "select name, number, password, granted from operator";
            //returnList = MySQLHelper.SQLselect(sqlstring, 4);
            //bool flag = false;
            //int i = 0;
            //bool user_exist = false;
            //while (returnList.Count != 0 && flag == false)//遍历表中数据
            //{
            //    if (returnList[i] == username && returnList[i+1] == number_id && returnList[i+2] == passwd)
            //    {
            //        //MessageBox.Show("登陆成功！");
            //        flag = true;
            //        MainWindow.CurrentLogin = returnList[i];
            //        ModifyWindow.muser = returnList[i];
            //        ModifyWindow.mnumber = returnList[i + 1];
            //        ModifyWindow.opassword = returnList[i + 2];
            //        switch (returnList[i+3])
            //        {
            //            case "1":
            //                MainWindow.RootAuth = 1;
            //                break;
            //            case "2":
            //                MainWindow.ConfigAuth = 1;
            //                break;
            //            case "3":
            //                MainWindow.LogAuth = 1;
            //                break;
            //        }
            //        break;
            //    }
            //    else if (returnList[i] == username)
            //    {
            //        user_exist = true;
            //    }

            //    i = i + 4;
            //    if (returnList.Count == i)
            //        break;
            //}

            //if (flag == false)
            //{
            //    if (user_exist == false)
            //    {
            //        MessageBox.Show("用户名不存在，请注册！");
            //    }
            //    else if (user_exist == true)
            //    {
            //        MessageBox.Show("密码或工号错误，请重试！");
            //    }
            //}
            //else
            //{//登录成功
            //    MainWindow mainWindow = new MainWindow();
            //    mainWindow.Show();
            //    this.Close();
            //}
        }

        private void Button_Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }
    }
}
