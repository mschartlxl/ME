using ME.BaseCore;
using ME.BaseCore.Instrument;
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
    /// ElectValueDeviceInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SwitchDeviceInfoWindow
    {
        public ComBoxItem SelectNumber { get; set; }
        public SwitchDeviceInfoWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Init();
        }
        private void Init() 
        {
            rbtnClose.IsChecked = true;
            var list = ListConfig.GetInstance().ListSwitchNumber;
            cbNumberList.ItemsSource = list;
            cbNumberList.SelectedItem = list.FirstOrDefault();
            cbNumberList.DisplayMemberPath = "Name";
        }
        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectNumber = cbNumberList.SelectedItem as ComBoxItem;
            DialogResult = true;
        }
    }
}
