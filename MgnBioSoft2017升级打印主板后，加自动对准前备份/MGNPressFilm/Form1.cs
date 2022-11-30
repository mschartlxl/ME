using HZH_Controls.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MGNPressFilm
{
    public partial class Form1 : Form
    {
        public static SerialPort serialport_valve_send_g;
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取串口完整名字（包括驱动名字）
        /// 如果找不到类，需要添加System.Management引用，添加引用->程序集->System.Management
        /// </summary>
        Dictionary<String, String> coms = new Dictionary<String, String>();
        /// <summary>
        /// 获得电脑串口列表
        /// </summary>
        private void GetPortDeviceName()
        {
            coms.Clear();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher
            ("select * from Win32_PnPEntity where Name like '%(COM%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        string deviceName = hardInfo.Properties["Name"].Value.ToString();
                        int startIndex = deviceName.IndexOf("(");
                        int endIndex = deviceName.IndexOf(")");
                        string key = deviceName.Substring(startIndex + 1, deviceName.Length - startIndex - 2);
                        string name = deviceName.Substring(0, startIndex - 1);
                        Console.WriteLine("key:" + key + ",name:" + name + ",deviceName:" + deviceName);
                        coms.Add(key, name);
                    }
                }
                //创建一个用来更新UI的委托 (主线程更新)
                this.Invoke(
                     new Action(() =>
                     {
                         comboBox1.Items.Clear();
                         foreach (KeyValuePair<string, string> kvp in coms)
                         {
                             comboBox1.Items.Add(kvp.Key + " " + kvp.Value);//更新下拉列表中的串口
                         }
                     })
                 );
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetPortDeviceName();
        }

        private void ucBtnImg27_BtnClick(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                //MessageBox.Show("请选择串口");
                FrmDialog.ShowDialog(this, "请选择串口", "提示");
                return;
            }
            if (serialport_valve_send_g.IsOpen)
            {
                FrmDialog.ShowDialog(this, "已连接设置，无需再次连接", "提示");
            }
            string com_number = comboBox1.SelectedItem.ToString();
            //string com_number = "COM4 USB Serial Port";
            string[] com_number_array = com_number.Split(' ');
            if (com_number_array.Length > 0)
            {
                if (!serialport_valve_send_g.IsOpen)
                {
                    try
                    {
                        serialport_valve_send_g.BaudRate = 9600;
                        serialport_valve_send_g.PortName = com_number_array[0];
                        //serialPort_Send.DataBits = 8;
                        serialport_valve_send_g.Open();//打开串口
                        //serialPort1.Write(buf, 0, buf.Length);
                        serialport_valve_send_g.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.OnDataReceivedValve);
                        //serialPort_Send.WriteBufferSize = 1024;
                        //serialPort_Send.ReadBufferSize = 1024;
                        //serialPort1.ReadBufferSize = 1024;
                        serialport_valve_send_g.DataBits = 8;
                        serialport_valve_send_g.Parity = (Parity)0;
                        //serialPort1.StopBits =1;
                        serialport_valve_send_g.ReadTimeout = 2000;
                        serialport_valve_send_g.RtsEnable = true;
                        serialport_valve_send_g.DtrEnable = true;
                        serialport_valve_send_g.ReceivedBytesThreshold = 1;
                        //串口默认的ReceivedBytesThreshold是1
                        //https://www.cnblogs.com/haofaner/p/3402307.html
                        ucBtnImg27.Enabled = false;
                        ucBtnImg27.FillColor = Color.Gray;
                    }
                    catch
                    {
                        ucBtnImg27.Enabled = true;
                        ucBtnImg27.FillColor = Color.Black;
                        FrmDialog.ShowDialog(this, "连接失败，端口已经被别的程序占用", "提示");
                    }
                }
            }
        }
        private void OnDataReceivedValve(object sender, SerialDataReceivedEventArgs e)
        {

        }
    }
}
