using ME.BaseCore;
using ME.BaseCore.Instrument;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MessageBox = ME.ControlLibrary.View.UMessageBox;
namespace ME.ControlLibrary.View
{
    /// <summary>
    /// ReCircleDeviceInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ReCircleDeviceInfoWindow
    {
        public ObservableCollection<ComBoxItem> DeviceTypes = new ObservableCollection<ComBoxItem>();
        public ComBoxItem SelectItem { get; set; } = new ComBoxItem();
        public ComBoxItem SelectNumber { get; set; }
        public ReCircleDeviceInfoWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitInfos();
        }
        private void InitInfos()
        {
            var list = ListConfig.GetInstance().ListReCircleNumber;
            cbNumberList.ItemsSource = list;
            cbNumberList.SelectedItem = list.FirstOrDefault();
            cbNumberList.DisplayMemberPath = "Name";
            DeviceTypes.Add(new ComBoxItem() { Name = "关", Type = 0 });
            DeviceTypes.Add(new ComBoxItem() { Name = "1", Type = 1 });
            DeviceTypes.Add(new ComBoxItem() { Name = "2", Type = 2 });
            DeviceTypes.Add(new ComBoxItem() { Name = "3", Type = 3 });
            DeviceTypes.Add(new ComBoxItem() { Name = "4", Type = 4 });
            DeviceTypes.Add(new ComBoxItem() { Name = "5", Type = 5 });
            DeviceTypes.Add(new ComBoxItem() { Name = "6", Type = 6 });
            DeviceTypes.Add(new ComBoxItem() { Name = "7", Type = 7 });
            DeviceTypes.Add(new ComBoxItem() { Name = "8", Type = 8 });
            cbHoleList.ItemsSource = DeviceTypes;
            cbHoleList.SelectedItem = DeviceTypes.LastOrDefault();
            cbHoleList.DisplayMemberPath = "Name";
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (cbHoleList.SelectedValue != null) 
            {
                SelectItem= cbHoleList.SelectedValue as ComBoxItem;
            }
            SelectNumber = cbNumberList.SelectedItem as ComBoxItem;
            DialogResult = true;
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
