using ME.BaseCore.Instrument;
using ME.ControlLibrary.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace ME.BioSoft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var flag = AppContext._stepInfo;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mainUcTabItem.Content = new MainUC(this);

        }

        /// <summary>
        /// 显示蒙板
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowMask(bool isShow)
        {
            if (isShow == true)
            {
                Mask.Visibility = Visibility.Visible;
                loadText.Text = "正在加载";
            }
            else
            {
                Mask.Visibility = Visibility.Collapsed;
                loadGrid.Visibility = Visibility.Collapsed;
                loadText.Text = "";
            }
        }
    }
}
