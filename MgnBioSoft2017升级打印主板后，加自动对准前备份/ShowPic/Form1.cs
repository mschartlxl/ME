using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShowPic
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            try {
                System.Windows.Forms.Screen[] sc;
                sc = System.Windows.Forms.Screen.AllScreens;
                if (sc.Length > 1)
                {
                    this.Left = sc[1].Bounds.Left;
                    this.Top = sc[1].Bounds.Top;
                    this.Height = sc[1].Bounds.Height;
                    this.Width = sc[1].Bounds.Width;
                }
                else
                {


                    this.Left = sc[0].Bounds.Width;
                    this.Top = sc[0].Bounds.Top;
                    this.Height = sc[1].Bounds.Height;
                    this.Width = sc[1].Bounds.Width;
                }
            }
            catch
            {

            }

        }
        /// <summary>
        /// 全屏显示
        /// </summary>
        public void ShowAllScreen()
        {
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            Rectangle ret = Screen.GetWorkingArea(this);

            this.pictureBox1.ClientSize = new Size(ret.Width, ret.Height);
            this.pictureBox1.Dock = DockStyle.Fill;
            this.pictureBox1.BringToFront();


            //if (sc.length > 1)
            //{
            //    this.Left = sc[0].Bounds.Width;
            //    this.Top = sc[0].Bounds.Top;
            //    this.Height = sc[1].Bounds.Height;
            //    this.Width = sc[1].Bounds.Width;
            //}
            //else
            //{
            //    this.Left = sc[0].Bounds.Left;
            //    this.Top = sc[0].Bounds.Top;
            //    this.Height = sc[0].Bounds.Height;
            //    this.Width = sc[0].Bounds.Width;
            //}


        }
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)] public string lpData;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT1
        {
            public int dwData;
            public int cbData;
            public IntPtr lpData;
        }

        const int WM_Lbutton = 0x201; //定义了鼠标的左键点击消息
        public const int USER = 0x0400; // 是windows系统定义的用户消息
        const int WM_MYMSG = 10000; //定义了鼠标的左键点击消息
        const int WM_COPYDATA = 0x004A;
        //[DllImport("User32.dll", EntryPoint = "SendMessage")]
        //private static extern int SendMessage(
        //    int hWnd,　　　// handle to destination window
        //    int Msg,　　　 // message
        //    int wParam,　// first message parameter
        //    int lParam // second message parameter
        //);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("User32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 发送完成
        /// </summary>
        public void SendMessageToQT(string msg_type)
        {
            String strSent = msg_type;//"发送完成";
            //如果为实时模式的，那么发送  strSent = "实时模式发送完成";

            int WINDOW_HANDLE = FindWindow(null, "HumablotPro");
            if (WINDOW_HANDLE != 0)
            {
                //byte[] arr = System.Text.Encoding.Default.GetBytes(strSent);
                //int len = arr.Length;
                //COPYDATASTRUCT cdata;
                //cdata.dwData = (IntPtr)100;
                //cdata.lpData = strSent;
                //cdata.cbData = len + 1;
                //SendMessage(WINDOW_HANDLE, WM_COPYDATA, 0, ref cdata);
                byte[] sarr = System.Text.Encoding.Default.GetBytes(strSent);
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)Convert.ToInt16(strSent.Length);//可以是任意值
                cds.cbData = len + 1;//指定lpData内存区域的字节数
                cds.lpData = strSent;//发送给目标窗口所在进程的数据
                SendMessage(WINDOW_HANDLE, WM_COPYDATA, 0, ref cds);
                SendMessage(WINDOW_HANDLE, WM_MYMSG, 0, ref cds);
            }
        }

        public void ShowPic(string pic_path)
        {
            try {

                pictureBox1.Load(pic_path);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                Rectangle ret = Screen.GetWorkingArea(this);
                this.pictureBox1.ClientSize = new Size(ret.Width, ret.Height);
                this.pictureBox1.Dock = DockStyle.Fill;
                this.pictureBox1.BringToFront();
            }
            catch(Exception e)
            {

            }

        }

        private const long WM_GETMINMAXINFO = 0x24;
        public struct POINTAPI
        {
            public int x;
            public int y;
        }
        public struct MINMAXINFO
        {
            public POINTAPI ptReserved;
            public POINTAPI ptMaxSize;
            public POINTAPI ptMaxPosition;
            public POINTAPI ptMinTrackSize;
            public POINTAPI ptMaxTrackSize;
        }


        protected override void WndProc(ref System.Windows.Forms.Message message)
        {
            const int WM_CREATE = 0x0001;
            const int WM_SYSCOMMAND = 0x0112;//定义要截获的消息类型
            const int SC_CLOSE = 0xF060;//关闭按钮对应的消息值
            if (message.Msg == WM_CREATE)
            {
                ShowAllScreen();
                ShowPic("CamFile/Cam_2022-06-25-02-35-41-648.jpg");
            }

            if (message.Msg == WM_GETMINMAXINFO)
            {
                MINMAXINFO mmi = (MINMAXINFO)message.GetLParam(typeof(MINMAXINFO));
                mmi.ptMinTrackSize.x = this.MinimumSize.Width;
                mmi.ptMinTrackSize.y = this.MinimumSize.Height;
                if (this.MaximumSize.Width != 0 || this.MaximumSize.Height != 0)
                {
                    mmi.ptMaxTrackSize.x = this.MaximumSize.Width;
                    mmi.ptMaxTrackSize.y = this.MaximumSize.Height;
                }
                mmi.ptMaxPosition.x = 1;
                mmi.ptMaxPosition.y = 1;
                System.Runtime.InteropServices.Marshal.StructureToPtr(mmi, message.LParam, true);

                this.pictureBox1.ClientSize = new Size(this.MaximumSize.Width, this.MinimumSize.Height);
                //this.pictureBox1.Dock = DockStyle.Fill;
                //this.pictureBox1.BringToFront();
                Update();

            }






            if (message.Msg == WM_SYSCOMMAND && (int)message.WParam == SC_CLOSE)
            {
                // 屏蔽传入的消息事件
                this.WindowState = FormWindowState.Minimized;
                //Application.Exit();
                System.Environment.Exit(0);
                return;
            }

            switch (message.Msg)
            {
                case WM_Lbutton:
                    ///string与MFC中的CString的Format函数的使用方法有所不同
                    break;
                case WM_MYMSG:
                    break;
                case WM_COPYDATA:
                    //test_id_list = new List<string>();
                    COPYDATASTRUCT1 CD = (COPYDATASTRUCT1)message.GetLParam(typeof(COPYDATASTRUCT1));
                    byte[] B = new byte[CD.cbData];
                    IntPtr lpData = CD.lpData;
                    Marshal.Copy(lpData, B, 0, CD.cbData);
                    string strData = Encoding.Default.GetString(B);
                    //request:aaa,bbb,ccc,
                    if (strData.Length > 0)
                    {
                        string[] str_data_array = strData.Split(':');
                        string path = "";
                        path = strData.Replace("\0", "");
                        label62.Text = path;
                        ShowPic(path);
                    }
                    break;
                default:
                    base.DefWndProc(ref message);///调用基类函数处理非自定义消息。
                    break;
            }
        }
    }
}
