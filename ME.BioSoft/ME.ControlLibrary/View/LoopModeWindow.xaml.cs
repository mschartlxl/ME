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
    /// LoopModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoopModeWindow
    {
        LoopMode _LoopMode = null;

        public LoopModeWindow()
        {
            InitializeComponent();

            nodeName.Focus();
        }

        public void Init(LoopMode loopMode)
        {
            if (loopMode == null)
            {
                loopMode = new LoopMode();
            }
            _LoopMode = loopMode;

            nodeName.Text = _LoopMode.NodeName;
            loopNum.Value = _LoopMode.CycleNumber;
        }

        public LoopMode GetInfo()
        {
            _LoopMode.NodeName = nodeName.Text;
            _LoopMode.CycleNumber = Convert.ToInt32(loopNum.Value);
            return _LoopMode;
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
