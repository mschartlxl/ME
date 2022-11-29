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

namespace ME.ControlLibrary.View
{
    /// <summary>
    /// SelectModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectModeWindow
    {
        public SelectModeWindow()
        {
            InitializeComponent();
        }

        private void RBtn_Checked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
