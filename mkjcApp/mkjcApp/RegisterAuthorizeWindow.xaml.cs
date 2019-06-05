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

namespace mkjcApp
{
    /// <summary>
    /// RegisterAuthorizeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterAuthorizeWindow : Window
    {
        public static int modify_user = 0;
        public static string opassword = "";
        public static int delete_index = 0;
        public static string UserName = "";
        public static bool newName = false;

        public RegisterAuthorizeWindow()
        {
            InitializeComponent();
            
        }

        private void SignInNew_OnClick(object sender, RoutedEventArgs e)
        {
            string username = signinUser.Text.ToString();
            string number_id = signinNumber.Text.ToString();
            string passwd = signinPassword.Password;
            string confirm = signinConfim.Password;
            int granted = AuthList.SelectedIndex;

            if (username == "" || number_id == "" || passwd == "" || confirm == "")
            {
                MessageBox.Show("有未填项！");
                return;
            }
            if (passwd != confirm)
            {
                MessageBox.Show("两次密码输入不一致");
                return;
            }




            string sqlstring = "select name, number from operator";

            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 2);
            int i = 0;
            bool flag = false;
            while (returnList.Count != 0 && flag == false)//遍历表中数据
            {
                //MessageBox.Show(reader[0].ToString() + "、" + reader[1].ToString());
                if (returnList[i] == username || returnList[i+1] == number_id)
                {
                    MessageBox.Show("用户名或者工号与他人重复，试试直接登录！");
                    flag = true;
                    break;
                }
                i = i+2;
                if (returnList.Count == i)
                    break;
            }
            if (flag == false)
            {
                sqlstring = "insert into operator(name, number, password, granted) values('" + username + "', " + "'" + number_id + "', " + "'" + passwd + "', " + granted + ")";
                //MessageBox.Show(sqlstring);
                MySQLHelper.SQLinsert(sqlstring);
                MessageBox.Show("注册成功！");
                UserName = username;
                newName = true;

                signinUser.Text = "";
                signinNumber.Text = "";
                signinPassword.Password = "";
                signinConfim.Password = "";
                AuthList.SelectedIndex = 0;
            }
        }

        private void Modified_OnClick(object sender, RoutedEventArgs e)
        {
            int user_index = userList.SelectedIndex;
            int auth_index = authList.SelectedIndex;


            string sqlstring = "select id,granted from operator";
            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 2);
            int i = 0, j=0;
            bool flag = false;
            int user_id = 0;
            while (returnList.Count != 0)
            {
                if(user_index == 1)
                {
                    MessageBox.Show("是不是傻？ROOT权限不能修改！");
                    break;
                }
                else if (j == user_index)
                {
                    //MessageBox.Show(sqlstring);
                    if (user_index != 0 && returnList[2*j-1] != auth_index.ToString() && auth_index != 0)
                    {
                        //MessageBox.Show("可以更改");
                        int.TryParse(returnList[2*(j-1)], out user_id);
                        flag = true;
                    }
                    else if (returnList[2 * j - 1] == auth_index.ToString())
                    {
                        MessageBox.Show("权限一致，不需修改");
                    }
                    else if (auth_index == 0)
                    {
                        MessageBox.Show("请选择需要更改的配置");
                    }
                    break;
                }
                i = i+2; j++;
                if (returnList.Count == i)
                    break;
            }
            if (flag == true)
            {
                sqlstring = "update operator set granted=" + auth_index + " where id=" + user_id;

                MySQLHelper.SQLinsert(sqlstring);
                MessageBox.Show("更改成功！");
            }
        }


        private void userList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int user_index = userList.SelectedIndex;
            string sqlstring = "select granted from operator";
            

            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 1);

            int i = 0, j=0;
            while (returnList.Count != 0)//遍历表中数据
            {
                
                if (i == user_index && user_index != 0)
                {
                    switch (returnList[i-1])
                    {
                        case "1":
                            currentAuth.Content = "一级：超级";
                            authList.SelectedIndex = 0;
                            break;
                        case "2":
                            currentAuth.Content = "二级：配置";
                            authList.SelectedIndex = 0;
                            break;
                        case "3":
                            currentAuth.Content = "三级：查看";
                            authList.SelectedIndex = 0;
                            break;
                    }
                    break;
                }
                j++; i++;
                if (returnList.Count == j)
                    break;
            }


        }


        private void Finish_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ModifyUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modify_user = ModifyUserList.SelectedIndex;
            MPassword.Password = "";
            MNew.Password = "";
            MConfim.Password = "";
        }

        private void ModifyUser_OnClick(object sender, RoutedEventArgs e)
        {
            string mpassword = MPassword.Password;//输入的原密码
            string mnew = MNew.Password;//输入的新密码
            string mconfirm = MConfim.Password;//输入的确认密码

            int i = 0;
            string sqlstring = "select id, password from operator";
            
            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 2);
            while(returnList.Count != 0)
            {
                if (modify_user == i)
                    break;
                i = i + 1;
                if (i == returnList.Count)
                    break;

            }

            if (returnList[2*i-1] != mpassword)
            {
                MessageBox.Show("原密码输入不正确！");
            }
            else if (mnew != mconfirm)
            {
                MessageBox.Show("新密码与确认密码不一致！");
            }
            else if (mpassword == mnew)
            {
                MessageBox.Show("原密码与新密码一致！");
            }
            else
            {
                sqlstring = "update operator set password='" + mnew + "' where id='" + returnList[2*(i-1)] + "'";
                MySQLHelper.SQLinsert(sqlstring);
                MessageBox.Show("更改成功！");
            }
        }

        //读用户列表
        private List<String> ReadUserList()
        {
            string sqlstring = "select * from operator";
            List<String> returnList = new List<string> { };
            returnList = MySQLHelper.SQLselect(sqlstring, 5);
            return returnList;
        }

        private void deleteUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            delete_index = deleteUserList.SelectedIndex;
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (delete_index == 1)
                MessageBox.Show("ROOT用户不能删！");
            else
            {
                string sqlstring = "select id from operator";
                List<String> returnList = new List<string> { };
                returnList = MySQLHelper.SQLselect(sqlstring, 1);
                int i = 2;
                while (returnList.Count != 0)
                {
                    if (i == delete_index)
                        break;
                    i++;
                    if (returnList.Count == i)
                        break;
                }
                sqlstring = "delete from operator where id=" + returnList[i-1];
                int result =MySQLHelper.SQLdelete(sqlstring);
                if (result == 1)
                {
                    deleteUserList.Items.RemoveAt(delete_index);
                    deleteUserList.SelectedIndex = 0;
                    MessageBox.Show("注销成功！");
                }
                    
            }
        }

        private void Register_Checked(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Visible;
            DeleteGrid.Visibility = Visibility.Hidden;
            AuthGrid.Visibility = Visibility.Hidden;
            ModifyGrid.Visibility = Visibility.Hidden;
            AllGrid.Visibility = Visibility.Hidden;
            AboutGrid.Visibility = Visibility.Hidden;
        }

        private void Delete_Checked(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Hidden;
            DeleteGrid.Visibility = Visibility.Visible;
            AuthGrid.Visibility = Visibility.Hidden;
            ModifyGrid.Visibility = Visibility.Hidden;
            AllGrid.Visibility = Visibility.Hidden;
            AboutGrid.Visibility = Visibility.Hidden;

            List<String> returnList = new List<string> { };
            returnList = ReadUserList();

            deleteUserList.Items.Clear();
            deleteUserList.Items.Insert(0, "---请选择---");
            deleteUserList.SelectedIndex = 0;

            int i = 0;
            int j = 1;

            while (returnList.Count != 0)//遍历表中数据
            {
                deleteUserList.Items.Insert(j, returnList[i+1]);
                i = i+5;
                j++;
                if (returnList.Count == i)
                    break;
            }
        }

        private void ModifyAuthorize_Checked(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Hidden;
            DeleteGrid.Visibility = Visibility.Hidden;
            AuthGrid.Visibility = Visibility.Visible;
            ModifyGrid.Visibility = Visibility.Hidden;
            AllGrid.Visibility = Visibility.Hidden;
            AboutGrid.Visibility = Visibility.Hidden;

            List<String> returnList = new List<string> { };
            returnList = ReadUserList();

            userList.Items.Clear();
            userList.Items.Insert(0, "---请选择---");
            userList.SelectedIndex = 0;
            authList.SelectedIndex = 0;

            int i = 0;
            int j = 1;

            while (returnList.Count != 0)//遍历表中数据
            {
                userList.Items.Insert(j, returnList[i+1]);
                i = i + 5;
                j++;
                if (returnList.Count == i)
                    break;
            }
        }

        private void ModifyPassword_Checked(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Hidden;
            DeleteGrid.Visibility = Visibility.Hidden;
            AuthGrid.Visibility = Visibility.Hidden;
            ModifyGrid.Visibility = Visibility.Visible;
            AllGrid.Visibility = Visibility.Hidden;
            AboutGrid.Visibility = Visibility.Hidden;

            List<String> returnList = new List<string> { };
            returnList = ReadUserList();

            ModifyUserList.Items.Clear();
            ModifyUserList.Items.Insert(0, "---请选择---");
            ModifyUserList.SelectedIndex = 0;

            int i = 0;
            int j = 1;

            while (returnList.Count != 0)//遍历表中数据
            {
                ModifyUserList.Items.Insert(j, returnList[i+1]);
                i = i + 5;
                j++;
                if (returnList.Count == i)
                    break;
            }
        }

        private void All_Checked(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Hidden;
            DeleteGrid.Visibility = Visibility.Hidden;
            AuthGrid.Visibility = Visibility.Hidden;
            ModifyGrid.Visibility = Visibility.Hidden;
            AllGrid.Visibility = Visibility.Visible;
            AboutGrid.Visibility = Visibility.Hidden;

            showUser.Items.Clear();

            string yonghuming = "";
            string gonghao = "";
            string mima = "";
            string quanxian = "";

            List<String> returnList = new List<string> { };
            returnList = ReadUserList();
            int j = 0;
            while (returnList.Count != 0)//遍历表中数据
            {
                yonghuming = returnList[j + 1];
                gonghao = returnList[j + 2];
                mima = returnList[j + 3];
                quanxian = returnList[j + 4];
                switch (quanxian)
                {
                    case "1":
                        quanxian = "一级：超级";
                        break;
                    case "2":
                        quanxian = "二级：配置";
                        break;
                    case "3":
                        quanxian = "三级：查看";
                        break;
                }
                //界面动态显示解析内容
                showUser.Items.Add(
                    new
                    {
                        yhm = yonghuming,
                        gh = gonghao,
                        mm = mima,
                        qx = quanxian
                    }
                );
                j = j + 5;
                if (returnList.Count == j)
                    break;
            }
        }

        private void About_Checked(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Hidden;
            DeleteGrid.Visibility = Visibility.Hidden;
            AuthGrid.Visibility = Visibility.Hidden;
            ModifyGrid.Visibility = Visibility.Hidden;
            AllGrid.Visibility = Visibility.Hidden;
            AboutGrid.Visibility = Visibility.Visible;
        }
    }
}
