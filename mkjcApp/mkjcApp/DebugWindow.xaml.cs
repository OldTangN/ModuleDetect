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
    /// DebugWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)//打开电源
        {
            MainWindow.IVYTProtocol.IVYTControl(7);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//关闭电源
        {
            MainWindow.IVYTProtocol.IVYTControl(8);
        }
    }
}
