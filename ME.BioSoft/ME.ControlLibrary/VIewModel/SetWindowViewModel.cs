using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ME.BaseCore;
using ME.BaseCore.Instrument;
using ME.ControlLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
namespace ME.ControlLibrary.VIewModel
{
    public class SetWindowViewModel : ObservableRecipient, IRecipient<string>
    {
        public SetWindowViewModel()
        {
            ReadCmd = new RelayCommand(Read);
            EditCmd = new RelayCommand(Edit);
        }
        private int readNumber;

        public int ReadNumber
        {
            get { return readNumber; }
            set
            {
                SetProperty(ref readNumber, value);
            }
        }
        private int editNumber;

        public int EditNumber
        {
            get { return editNumber; }
            set
            {
                SetProperty(ref editNumber, value);
            }
        }
        public ICommand ReadCmd { get; private set; }
        public ICommand EditCmd { get; private set; }
        public void Read()
        {
            System.Func<bool> cancelFun = () =>
            {
                return false;
            };
            byte[] senddata = InstructionConfig.cmdPumpReadAddr;
            var senddatanew = CRC.GetNewCrcArray(senddata);
            var temp = senddatanew.Clone() as byte[];
            var tasksingle = Task.Run(() =>
            {
                return UtilsFun._AbtInstrument.Send_16(cancelFun, temp, true, UtilsFun._AbtInstrument.SerialPump, 5);
            });
            tasksingle.Wait();
           var  result = tasksingle.Result;
            if (result != null && result[0]== temp[0]) 
            {
                ReadNumber = Convert.ToInt16(result[1]);
            }
            else 
            {
                MessageBox.Show("查询失败！", "提示", 1);
            }
        }
        public void Edit()
        {
            System.Func<bool> cancelFun = () =>
            {
                return false;
            };
            byte[] senddata = InstructionConfig.cmdPumpWriteAddr;
            senddata[1] = Convert.ToByte(ReadNumber.ToString("X2"), 16);
            senddata[7]= Convert.ToByte(EditNumber.ToString("X2"), 16);
            var senddatanew = CRC.GetNewCrcArray(senddata);
            var temp = senddatanew.Clone() as byte[];
            var tasksingle = Task.Run(() =>
            {
                return UtilsFun._AbtInstrument.Send_16(cancelFun, temp, true, UtilsFun._AbtInstrument.SerialPump, 5);
            });
            tasksingle.Wait();
            var result = tasksingle.Result;
            if (result != null && result[1] ==EditNumber)
            {
                MessageBox.Show("修改成功！", "提示", 1);
            }
            else
            {
                MessageBox.Show("修改失败！", "提示", 1);
            }
        }

        public void Receive(string message)
        {

        }
    }
}
