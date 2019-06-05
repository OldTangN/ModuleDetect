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

namespace mkjcApp
{
    /// <summary>
    /// ModifyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ModifyWindow : Window
    {
        public static string muser = "";
        public static string mnumber = "";
        public static string opassword = "";

        public ModifyWindow()
        {
            InitializeComponent();

            MUser.Content = muser;
            MNumber.Content = mnumber;
        }

        private void Modify_OnClick(object sender, RoutedEventArgs e)
        {
            string mpassword = MPassword.Password;//输入的原密码
            string mnew = MNew.Password;//输入的新密码
            string mconfirm = MConfim.Password;//输入的确认密码

            if(opassword != mpassword)
            {
                MessageBox.Show("原密码输入不正确！");
            }
            else if(mnew != mconfirm)
            {
                MessageBox.Show("新密码与确认密码不一致！");
            }
            else if(mpassword == mnew)
            {
                MessageBox.Show("原密码与新密码一致！");
            }
            else
            {
                string sqlstring = "update operator set password=" + mnew + " where name='" + muser + "' and id>0";
                MySQLHelper.SQLinsert(sqlstring);
                MessageBox.Show("更改成功！");
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
