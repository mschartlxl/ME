using System;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace ME.ControlLibrary.View
{
    /// <summary>
    /// UMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class UMessageBox
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern int MessageBoxTimeout(IntPtr hwnd, string txt, string caption, int wtype, int wlange, int dwtimeout);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        const int WM_CLOSE = 0x10;

        static UMessageBox UMsgBox = null;

        public UMessageBox()
        {
            InitializeComponent();

            UMsgBox = this;
        }

        /// <summary>
        /// 弹窗显示的内容
        /// </summary>
        public string MsgContent
        {
            get { return TxtMessage.Text; }
            set { TxtMessage.Text = value; }
        }

        /// <summary>
        /// UMessageBox.Show
        /// </summary>
        /// <param name="message">弹窗内容</param>
        /// <param name="caption">弹窗标题</param>
        /// <param name="showCancel">是否显示取消按钮</param>
        /// <param name="owner">父窗体</param>
        /// <returns>是否点击确定按钮</returns>
        public static bool Show(string message, string caption, bool showCancel, Window owner = null)
        {
            var msgBox = new UMessageBox
            {
                MsgContent = message,
                Title = caption,
                Owner = owner
            };
            if (owner != null)
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            else
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msgBox.btnCancel.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;
            bool? result = msgBox.ShowDialog();
            if (result == true)
                return true;
            else
                return false;
        }

        /// <summary>
        /// UMessageBox.Show
        /// </summary>
        /// <param name="message">弹窗内容</param>
        /// <param name="caption">弹窗标题</param>
        /// <param name="time">定时关闭时长(可选)，单位:秒</param>
        /// <param name="showCancel">不显示取消按钮</param>
        /// <returns>确定</returns>
        public static bool Show(string message, string caption = "", double time = -1)
        {
            var msgBox = new UMessageBox
            {
                MsgContent = message,
                Title = caption
            };
            msgBox.btnCancel.Visibility = Visibility.Collapsed;
            msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (time != -1)
            {
                msgBox.operateArea.Visibility = Visibility.Collapsed; //弹窗自动关闭时，隐藏按钮
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(time)
                };
                timer.Tick += TimerTick;
                timer.Start();
            }
            bool? result = msgBox.ShowDialog();
            if (result == true)
                return true;
            else
                return false;
        }

        private static void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            UMsgBox.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
