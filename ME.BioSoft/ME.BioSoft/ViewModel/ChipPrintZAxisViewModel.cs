using CommunityToolkit.Mvvm.Input;
using ME.BaseCore.Instrument;
using ME.BaseCore;
using ME.BioSoft.Model;
using ME.ControlLibrary.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
namespace ME.BioSoft.ViewModel
{
    public partial class ChipPrintViewModel
    {
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
                //return;
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
                Messages.Insert(0, new MessageCmd()
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
                Messages.Insert(0, new MessageCmd()
                {
                    TimeCmd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSendCmd = "收",
                    ContentCmd = taskresult == null ? "" : BitConverter.ToString(taskresult)
                });
            });
            return taskresult;
        }
        #endregion
    }
}
