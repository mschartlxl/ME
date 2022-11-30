using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using LinqToDB.Common;
using ME.BaseCore.Instrument;
using ME.BioSoft.Model;
using ME.ControlLibrary.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
namespace ME.BioSoft.ViewModel
{
    public class ChipPrintViewModel : ObservableRecipient, IRecipient<string>
    {
        private string platform = ConfigurationManager.AppSettings["SystemPlatform"].ToString();
        gclib gclib = new gclib();
        bool platStatus = false;
        public static int timespan = 120;
        public ChipPrintViewModel()
        {
            InitPlatForm();
            InitZAxis();
            this.IsActive = true;
        }
        private void InitPlatForm()
        {
            PlatForm();
            PlatFormFiled();
        }
        private void PlatFormFiled ()
        {
            SendSportCmd = new RelayCommand(SendSport);
            FindOriginCmd = new RelayCommand(FindOrigin);
            BreakEnableCmd = new RelayCommand(BreakEnable);
            EmergencyStopCmd = new RelayCommand(EmergencyStop);
            EnableCmd = new RelayCommand(Enable);
            ConnectCmd = new RelayCommand(Connect);
        }
        #region 平台调试
        private void PlatForm()
        {
            if (connectText == "连接")
            {
                StartPlatForm();
            }
            else
            {
                EndPlatForm();
            }
        }
        /// <summary>
        /// 启动位移平台服务
        /// </summary>
        public void StartPlatForm()
        {

            if (ConnectPlatForm())
            {
                ConnectText = "断开";
                platStatus = true;
            }
            else
            {
                //  MessageBox.Show("连接异常，设备为上电或是，通讯异常，请查看故障灯及设备是否正常运行");
            }
        }
        public void EndPlatForm()
        {
            connectText = "连接";
            gclib.GClose();
            platStatus = false;
        }
        public bool ConnectPlatForm()
        {
            try
            {
                gclib.GOpen("192.168.0.101 -d"); //通过控制器IP地址连接控制器
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"位移平台连接失败，请检查！原因:{ex.Message}", "提示", 1);
                return false;
            }
        }
        private string connectText = "连接";

        public string ConnectText
        {
            get { return connectText; }
            set
            {
                SetProperty(ref connectText, value);
            }
        }
        private bool xAxisIsCheck = true;

        public bool XAxisIsCheck
        {
            get { return xAxisIsCheck; }
            set
            {
                SetProperty(ref xAxisIsCheck, value);
            }
        }
        private bool yAxisIsCheck = true;

        public bool YAxisIsCheck
        {
            get { return yAxisIsCheck; }
            set
            {
                SetProperty(ref yAxisIsCheck, value);
            }
        }

        private bool rAxisIsCheck;

        public bool RAxisIsCheck
        {
            get { return rAxisIsCheck; }
            set
            {
                SetProperty(ref rAxisIsCheck, value);
            }
        }
        private int plusSpeed = 1000000;
        /// <summary>
        /// 加速度
        /// </summary>
        public int PlusSpeed
        {
            get { return plusSpeed; }
            set
            {
                SetProperty(ref plusSpeed, value);
            }
        }
        private int reduceSpeed = 1000000;
        /// <summary>
        /// 减速度
        /// </summary>
        public int ReduceSpeed
        {
            get { return reduceSpeed; }
            set
            {
                SetProperty(ref reduceSpeed, value);
            }
        }
        private int speed = 16666;
        /// <summary>
        /// 速度
        /// </summary>
        public int Speed
        {
            get { return speed; }
            set
            {
                SetProperty(ref speed, value);
            }
        }
        private int length = 10000;
        /// <summary>
        /// 点动长度
        /// </summary>
        public int Length
        {
            get { return length; }
            set
            {
                SetProperty(ref length, value);
            }
        }
        private int pulse = 1;
        /// <summary>
        /// 脉冲当量
        /// </summary>
        public int Pulse
        {
            get { return pulse; }
            set
            {
                SetProperty(ref pulse, value);
            }
        }
        public ICommand SendSportCmd { get; private set; }
        public ICommand FindOriginCmd { get; private set; }
        public ICommand BreakEnableCmd { get; private set; }
        public ICommand EmergencyStopCmd { get; private set; }
        public ICommand EnableCmd { get; private set; }
        public ICommand ConnectCmd { get; private set; }

        private bool findOriginEnable = true;

        public bool FindOriginEnable
        {
            get { return findOriginEnable; }
            set
            {
                SetProperty(ref findOriginEnable, value);
            }
        }

        private void SendSport()
        {
            if (!platStatus)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            string move_offset = "PA";
            string move_offset_end = "";
            string moveexpression = $"ACX={PlusSpeed * Pulse};DCX={ReduceSpeed * Pulse};SPX={Speed * Pulse};";
            if (XAxisIsCheck)
            {
                string strText = String.Format("{0}", Length * Pulse);
                move_offset += strText;
                move_offset_end += "X";
            }
            move_offset += ",";
            if (YAxisIsCheck)
            {
                string strText = String.Format("{0}", Length * Pulse);
                move_offset += strText;
                move_offset_end += "Y";
            }
            move_offset += ",";
            if (RAxisIsCheck)
            {
                string strText = String.Format("{0}", Length * Pulse);
                move_offset += strText;
                move_offset_end += "Z";
            }
            move_offset += ";";
            move_offset += "BG" + move_offset_end;
            move_offset = moveexpression + move_offset;
            gclib.GCommand(move_offset);
        }
        private void FindOrigin()
        {
            FindOriginEnable = false;
            try
            {
                if (platform == "1")
                {
                    string cmd = $"AC{PlusSpeed * Pulse * 10};DC{ReduceSpeed * Pulse * 10};SP{Speed * Pulse}";
                    gclib.GCommand(cmd);
                    string findzeroend = "";
                    string findzero = "SH;";
                    if (XAxisIsCheck)
                    {
                        findzero += "HMX;";
                        findzeroend += "X";
                    }
                    if (YAxisIsCheck)
                    {
                        findzero += "HMY;";
                        findzeroend += "Y";
                    }
                    if (RAxisIsCheck)
                    {
                        findzero += "FIZ;";
                        findzeroend += "Z";
                    }
                    findzero += "BG" + findzeroend;
                    gclib.GCommand(findzero);
                    //手动清零
                    gclib.GCommand("DP0,0,0");
                }
            }
            catch (Exception er)
            {
            }
            finally
            {
                FindOriginEnable = true;
            }


        }
        private void BreakEnable()
        {
            if (!platStatus)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            if (XAxisIsCheck)
            {
                Thread.Sleep(timespan);
                gclib.GCommand("STX;MOX");
            }

            if (YAxisIsCheck)
            {
                Thread.Sleep(timespan);
                gclib.GCommand("STY;MOY");
            }

            if (RAxisIsCheck)
            {
                Thread.Sleep(timespan);
                gclib.GCommand("STZ;MOZ");
            }
        }
        private void EmergencyStop()
        {
            if (!platStatus)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            gclib.GCommand("AB");
        }
        private void Enable()
        {
            if (!platStatus)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            if (XAxisIsCheck)
            {
                Thread.Sleep(timespan);
                gclib.GCommand("SHX");
            }
            if (YAxisIsCheck)
            {
                Thread.Sleep(timespan);
                gclib.GCommand("SHY");
            }
            if (RAxisIsCheck)
            {
                Thread.Sleep(timespan);
                gclib.GCommand("SHZ");
            }

        }
        private void Connect()
        {
            PlatForm();
        }
        #endregion
        #region Z轴调试
        private ObservableCollection<ZAxisItem> zAxisItems;
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<ZAxisItem> ZAxisItems
        {
            get => zAxisItems;
            set
            {
                zAxisItems = value;
                SetProperty(ref zAxisItems, value);
            }
        }
        private void InitZAxis()
        {
            ZAxisFiled();
        }
        private void ZAxisFiled() 
        {
            ZAxisItems = new ObservableCollection<ZAxisItem>();
           var zlist= ListConfig.GetInstance().ListZAxisNumber;
            foreach (var z in zlist) 
            {
                ZAxisItems.Add(new ZAxisItem() { IsCheck=true,CkContent=$"Z{z.Type}"});
            }
        }
        #endregion
        public void Receive(string message)
        {

        }
    }
}
