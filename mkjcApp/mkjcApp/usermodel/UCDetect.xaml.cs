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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO.Ports;
using System.Threading;

namespace mkjcApp
{
    /// <summary>
    /// UCDetect.xaml 的交互逻辑
    /// </summary>
    public partial class UCDetect : UserControl
    {
        public UCDetect()
        {
            InitializeComponent();
        }

        public void testc()
        {
            // MessageBox.Show("ok！");
            ;
        }
        /// <summary>
        /// 静态功耗测试步骤：
        /// 向测试板发送指令，测试仪控制投入，返回数据解析成功或者失败
        /// 读取功率仪的实时功耗，返回数据解析功耗值
        /// 静态功耗值显示在界面上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_static_power_Click(object sender, RoutedEventArgs e)
        {
           

        }
        /// <summary>
        /// 动态功耗测试步骤
        /// 向功率仪发送命令，最大值锁存，返回数据
        /// 通过抄控器通信读取电量，返回值解析电量
        /// 读取功率仪最大功耗
        /// 功率仪解除锁存状态
        /// 动态功耗显示在界面上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_dynamic_power_Click(object sender, RoutedEventArgs e)
        {
        
        }
    }
}
