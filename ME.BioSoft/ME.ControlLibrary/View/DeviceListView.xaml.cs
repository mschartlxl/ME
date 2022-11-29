
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
using CommunityToolkit.Mvvm.ComponentModel;
using ME.BaseCore;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
namespace ME.ControlLibrary.View
{
    /// <summary>
    /// ExportCommand.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceListView : Window
    {
        public ObservableCollection<ComBoxItem> DeviceTypes = new ObservableCollection<ComBoxItem>();
        #region Private Fields

        #endregion

        #region Properties
        public string CommandInfoFileName { get; set; }
        public Device SelectDevice { get; set; } = new Device();
        #endregion

        #region Events

        #endregion

        #region Constructors

        public DeviceListView()
        {
            InitializeComponent();
            InitDeviceInfos();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        private void InitDeviceInfos()
        {
            DeviceTypes.Add(new ComBoxItem() { Name = "注射泵", Type = 1 });
            DeviceTypes.Add(new ComBoxItem() { Name = "旋切阀", Type = 2 });
            DeviceTypes.Add(new ComBoxItem() { Name = "电磁阀", Type = 3 });
            cbDeviceList.ItemsSource = DeviceTypes;
            cbDeviceList.SelectedItem = DeviceTypes.FirstOrDefault();
            cbDeviceList.DisplayMemberPath = "Name";
        }
        #endregion

        #region Methods

        #endregion

        private void Confirm(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("名称不能为空!");
                return;
            }
            DialogResult = true;
            if (cbDeviceList.SelectedItem != null)
            {
                var selectItem = cbDeviceList.SelectedItem as ComBoxItem;
                if (selectItem != null)
                {
                    if (selectItem.Type == 1)
                    {
                        PumpDeviceInfoWindow pumpDeviceInfoWin = new PumpDeviceInfoWindow();
                        pumpDeviceInfoWin.ShowDialog();
                        if (pumpDeviceInfoWin.DialogResult == true)
                        {
                            SelectDevice.Name = txtName.Text;
                            SelectDevice.Type = selectItem.Type;
                            SelectDevice.Distance = pumpDeviceInfoWin.numDistance.Value.ToString();
                            SelectDevice.Number = pumpDeviceInfoWin.SelectNumber.Type;
                            SelectDevice.Speed =Convert.ToInt32( pumpDeviceInfoWin.numSpeed.Value);
                            //SelectDevice.IsChecked=
                        }
                    }
                    else if (selectItem.Type == 2)
                    {
                        ReCircleDeviceInfoWindow reCircleDeviceInfoWin = new ReCircleDeviceInfoWindow();
                        reCircleDeviceInfoWin.ShowDialog();
                        if (reCircleDeviceInfoWin.DialogResult == true)
                        {
                            SelectDevice.Name = txtName.Text;
                            SelectDevice.Number = reCircleDeviceInfoWin.SelectNumber.Type;
                            SelectDevice.Type = selectItem.Type;
                            SelectDevice.Hole = reCircleDeviceInfoWin.SelectItem.Type.ToString();
                        }
                    }
                    else if (selectItem.Type == 3)
                    {
                        SwitchDeviceInfoWindow eleValueDeviceInfoWin = new SwitchDeviceInfoWindow();
                        eleValueDeviceInfoWin.ShowDialog();
                        if (eleValueDeviceInfoWin.DialogResult == true)
                        {
                            SelectDevice.Name = txtName.Text;
                            SelectDevice.Type = selectItem.Type;
                            SelectDevice.Flag = eleValueDeviceInfoWin.rbtnOpen.IsChecked.Value == true ? 1 : 0;
                            SelectDevice.Number = eleValueDeviceInfoWin.SelectNumber.Type;
                        }
                    }
                }
            }
        }
    }
  
    public class Device
    {
        public bool IsChecked { get; set; }
        public int Number { get; set; }
        public int Speed { get; set; }
        public string Name { get; set; }
        public string Distance { get; set; }
        public int Type { get; set; }
        public string Hole { get; set; }
        public int Flag { get; set; }
    }

}
