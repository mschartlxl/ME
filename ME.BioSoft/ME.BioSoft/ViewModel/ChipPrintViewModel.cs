using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using LinqToDB.Common;
using ME.BaseCore;
using ME.BaseCore.Instrument;
using ME.BioSoft.AutoMapper;
using ME.BioSoft.Model;
using ME.ControlLibrary.Model;
using ME.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        /// <summary>
        /// 对象映射
        /// </summary>
        public IMapper mapper;
        public static int timespan = 120;
        public ChipPrintViewModel()
        {
            InitPlatForm();
            InitZAxis();
            InitDataGrid();
            this.IsActive = true;
        }
        private void InitPlatForm()
        {
            PlatForm();
            PlatFormCmd();
        }
        private void PlatFormCmd()
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
                SetProperty(ref zAxisItems, value);
            }
        }
        private int zAxisDistance;

        public int ZAxisDistance
        {
            get { return zAxisDistance; }
            set
            {
                SetProperty(ref zAxisDistance, value);
            }
        }
        private int zAxisPos;

        public int ZAxisPos
        {
            get { return zAxisPos; }
            set
            {
                SetProperty(ref zAxisPos, value);
            }
        }
        private float zAxisSpeed;

        public float ZAxisSpeed
        {
            get { return zAxisSpeed; }
            set
            {
                SetProperty(ref zAxisSpeed, value);
            }
        }
        public ICommand ZAxisFindZeroCmd { get; private set; }
        public ICommand ZAxisSetSpeedCmd { get; private set; }
        public ICommand ZAxisReadSpeedCmd { get; private set; }
        public ICommand ZAxisReadPosCmd { get; private set; }
        public ICommand ZAxisEnabledCmd { get; private set; }
        public ICommand ZAxisNoEnabledCmd { get; private set; }

        public ICommand ZAxisMoveCmd { get; private set; }
        public ICommand ZAxisStopCmd { get; private set; }
        private void InitZAxis()
        {
            ZAxisFiled();
            InitZAxisCmd();
        }
        private void InitZAxisCmd()
        {
            ZAxisFindZeroCmd = new RelayCommand(ZAxisFindZero);
            ZAxisSetSpeedCmd = new RelayCommand(ZAxisSetSpeed);
            ZAxisReadSpeedCmd = new RelayCommand(ZAxisReadSpeed);
            ZAxisReadPosCmd = new RelayCommand(ZAxisReadPos);
            ZAxisEnabledCmd = new RelayCommand(ZAxisEnabled);
            ZAxisNoEnabledCmd = new RelayCommand(ZAxisNoEnabled);
            ZAxisMoveCmd = new RelayCommand(ZAxisMove);
            ZAxisStopCmd = new RelayCommand(ZAxisStop);

        }
        private void ZAxisFiled()
        {
            ZAxisItems = new ObservableCollection<ZAxisItem>();
            var zlist = ListConfig.GetInstance().ListZAxisNumber;
            foreach (var z in zlist)
            {
                ZAxisItems.Add(new ZAxisItem() { IsCheck = true, CkContent = $"Z{z.Type}", Id = z.Type });
            }
        }
        private void ZAxisFindZero()
        {
            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisReset;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);
                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);

                }
            }

        }
        private void ZAxisReadSpeed()
        {
            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisReadSpeed;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);

                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    var result = Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);
                    var byter = new List<byte>();
                    byter.Add(result[4]);
                    byter.Add(result[3]);
                    byter.Add(result[6]);
                    byter.Add(result[5]);
                    ZAxisSpeed = BitConverter.ToSingle(byter.ToArray(), 0);
                }
            }
        }
        private void ZAxisSetSpeed()
        {
            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisSetSpeed;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);
                    var speedx16 = ABTInstrument.float2String(ZAxisSpeed).ToByteArray(16);
                    senddata[7] = speedx16[1];
                    senddata[8] = speedx16[0];
                    senddata[9] = speedx16[3];
                    senddata[10] = speedx16[2];
                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);
                }
            }
        }
        private void ZAxisReadPos()
        {
            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisPos;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);
                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    var result = Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);
                    if (result != null)
                    {
                        var str16 = CRC.byteToHexStr(result.ToArray());
                        var strlist = str16.Split(' ');
                        ZAxisDistance = Convert.ToInt32(strlist[5] + strlist[6] + strlist[3] + strlist[4], 16);
                    }

                }
            }
        }
        private void ZAxisEnabled()
        {
            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisEnable;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);
                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);
                }
            }
        }
        private void ZAxisNoEnabled()
        {
            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisNoEnable;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);
                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);
                }
            }
        }
        private void ZAxisMove()
        {
            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisMove;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);
                    var x16 = ZAxisDistance.ToString("X8");
                    var data = x16.ToByteArray(16);
                    senddata[7] = data[2];
                    senddata[8] = data[3];

                    senddata[9] = data[0];
                    senddata[10] = data[1];

                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);
                }
            }
        }
        private void ZAxisStop()
        {

            if (!UtilsFun._AbtInstrument.SerialSwitch.IsOpen)
            {
                MessageBox.Show($"请连接设备", "提示", 1);
                return;
            }
            foreach (var item in ZAxisItems)
            {
                if (item.IsCheck)
                {
                    //SendZAxisMotorFindZero(1);
                    byte[] senddata = InstructionConfig.cmdZAxisStop;
                    senddata[0] = Convert.ToByte(item.Id.ToString("X2"), 16);
                    var crcdata = CRC.CRC16(senddata);
                    var senddatanew = senddata.ToList();
                    senddatanew.AddRange(crcdata.Reverse());
                    var temps = senddatanew.ToArray().Clone() as byte[];
                    Send(() => { return false; }, temps, UtilsFun._AbtInstrument.SerialSwitch);
                }
            }
        }
        private byte[] Send(Func<bool> cancelFun, byte[] temp, SerialPort serialPort)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CommandMessage.Instance.Messages.Insert(0, new MessageCmd()
                {
                    TimeCmd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSendCmd = "发",
                    ContentCmd = BitConverter.ToString(temp)
                });
            });

            var tasksingle = Task.Run(() =>
            {
                return UtilsFun._AbtInstrument.Send_16(cancelFun, temp, true, serialPort, 5);
            });
            tasksingle.Wait();
            var taskresult = tasksingle.Result;
            App.Current.Dispatcher.Invoke(() =>
            {
                CommandMessage.Instance.Messages.Insert(0, new MessageCmd()
                {
                    TimeCmd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSendCmd = "收",
                    ContentCmd = taskresult == null ? "" : BitConverter.ToString(taskresult)
                });
            });
            return taskresult;
        }
        #endregion
        #region
        private void InitDataGrid()
        {
            InitDataGridField();
            InitData();
        }
        private void InitDataGridField()
        {
            PlatformActionList = new ObservableCollection<PlatformActionDTO>();
            AllCheckedCmd = new RelayCommand<bool>(AllChecked);
            DataGridAddCmd = new RelayCommand(DataGridAdd);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrginalProfile>();
            });
            mapper = config.CreateMapper();
        }
        private void InitData()
        {
            var alldata = PlatformActionDAL.Instance.SearchMany(null);
            
            foreach (var data in alldata)
            {
                var model = mapper.Map<PlatformActionDTO>(data);
                PlatformActionList.Add(model);
            }
        }
        public ICommand AllCheckedCmd { get; private set; }
        public ICommand DataGridAddCmd { get; private set; }
        private ObservableCollection<PlatformActionDTO> platformActionList;
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<PlatformActionDTO> PlatformActionList
        {
            get => platformActionList;
            set
            {
                SetProperty(ref platformActionList, value);
            }
        }
        private PlatformActionDTO currentDataGridItem=new PlatformActionDTO ();
        public PlatformActionDTO CurrentDataGridItem 
        {
            get => currentDataGridItem;
            set
            {
                SetProperty(ref currentDataGridItem, value);
            }
        }
        public void AllChecked(bool flag)
        {
            foreach (var item in PlatformActionList)
            {
                item.IsChecked = flag;
            }

        }
        private void DataGridAdd()
        {
            var z = "";
            foreach (var item in ZAxisItems)
            {
               z+=  $"{item.CkContent}:{item.CkTxt},";
            }
            CurrentDataGridItem.Z= z.TrimEnd(',');
            PlatformActionList.Add(CurrentDataGridItem);
        }
        public void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
        #endregion
        public void Receive(string message)
        {

        }
    }
}
