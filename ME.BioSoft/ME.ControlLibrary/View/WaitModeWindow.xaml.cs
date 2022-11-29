using System;
using System.Windows;
using System.Collections.Generic;
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
using ME.BaseCore;

namespace ME.ControlLibrary.View
{
    /// <summary>
    /// WaitModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WaitModeWindow
    {
        WaitMode _WaitMode = null;

        public WaitModeWindow()
        {
            InitializeComponent();

            waitTime.Focus();
        }
        
        public void Init(WaitMode waitMode)
        {
            if (waitMode == null)
            {
                waitMode = new WaitMode();
            }
            _WaitMode = waitMode;

            nodeName.Text = _WaitMode.NodeName;
            waitTime.Value = _WaitMode.WaitTime;
        }

        public WaitMode GetInfo()
        {
            _WaitMode.NodeName = nodeName.Text;
            _WaitMode.WaitTime = Convert.ToInt32(waitTime.Value);
            return _WaitMode;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
