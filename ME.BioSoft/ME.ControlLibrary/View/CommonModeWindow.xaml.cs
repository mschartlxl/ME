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
using ME.ControlLibrary.Model;
using ME.BaseCore;

namespace ME.ControlLibrary.View
{
    /// <summary>
    /// CommonModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CommonModeWindow
    {
        CommonMode _CommonMode = null;
        public ObservableCollection<CmdParameter> CommandParaList { get; set; } = new ObservableCollection<CmdParameter>();

        public CommonModeWindow()
        {
            InitializeComponent();
            nodeName.Focus();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Loaded += CommonModeWindow_Loaded;
            oveerTime.Text = "15";
        }

        private void CommonModeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            datagrid.ItemsSource = CommandParaList;
        }
        public void Init(CommonMode cmdMode)
        {
            if (cmdMode == null)
            {
                cmdMode = new CommonMode();
            }
            _CommonMode = cmdMode;
            nodeName.Text = _CommonMode.NodeName;
            oveerTime.Text = _CommonMode.Overtime.ToString();
            foreach (var item in _CommonMode.CommandParas)
            {
                CommandParaList.Add(new CmdParameter()
                {
                    ParaName = item.ParaName,
                    ParaType=item.ParaType
                });

            }

        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DeviceListView view = new DeviceListView();
            if (view.ShowDialog() != true)
            {
                return;
            }
            if (view.SelectDevice.Type == 1) //注射泵
            {
                CommandParaList.Add(new CmdParameter()
                {
                    ParaName = $"序号:{view.SelectDevice.Number},名称:{view.SelectDevice.Name},距离:{view.SelectDevice.Distance},速度:{view.SelectDevice.Speed}",
                    ParaType = view.SelectDevice.Type
                });
            }
            else if (view.SelectDevice.Type == 2)//旋切阀
            {
                CommandParaList.Add(new CmdParameter()
                {
                    ParaName = $"序号:{view.SelectDevice.Number},名称:{view.SelectDevice.Name},孔:{view.SelectDevice.Hole}",
                    ParaType = view.SelectDevice.Type
                });
            }
            else if (view.SelectDevice.Type == 3)//电磁阀
            {
                CommandParaList.Add(new CmdParameter()
                {
                    ParaName = $"序号:{view.SelectDevice.Number},名称:{view.SelectDevice.Name},开关:{view.SelectDevice.Flag}",
                    ParaType = view.SelectDevice.Type
                });
            }

        }
        public CommonMode GetInfo(ObservableCollection<TreeItem> Children)
        {
            if (_CommonMode == null)
            {
                _CommonMode = new CommonMode();
            }
            _CommonMode.NodeName = nodeName.Text;
            _CommonMode.Overtime = Convert.ToInt32(oveerTime.Text);
            List<CommandPara> listCommandPara = new List<CommandPara>();
            foreach (var item in CommandParaList)
            {
                listCommandPara.Add(new CommandPara()
                {
                    ParaName = item.ParaName,
                    ParaType=item.ParaType
                });
            }
            _CommonMode.CommandParas = listCommandPara;
            return _CommonMode;
        }
        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void datagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            CmdParameter item = datagrid.SelectedItem as CmdParameter;
            CommandParaList.Remove(item);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
