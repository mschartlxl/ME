using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
using ME.BaseCore;
using ME.BaseCore.Instrument;

namespace ME.ControlLibrary.View
{
    /// <summary>
    /// CommonModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PumpDeviceInfoWindow
    {


        public PumpDeviceInfoWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var list = ListConfig.GetInstance().ListPumpNumber;
            cbNumberList.ItemsSource = list;
            cbNumberList.SelectedItem = list.FirstOrDefault();
            cbNumberList.DisplayMemberPath = "Name";
        }
        public ComBoxItem SelectNumber { get; set; }
        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(numDistance.Value.ToString()))
            {
                MessageBox.Show("距离不能为空!");
                return;
            }
            var disvalue =Convert.ToInt32( numDistance.Value);
            if (!(disvalue >= 0 && disvalue <= 12000)) 
            {
                MessageBox.Show("距离取值范围:0-12000!");
                return;
            }
            
            SelectNumber = cbNumberList.SelectedItem as ComBoxItem;
            DialogResult = true;
        }
    }
}
