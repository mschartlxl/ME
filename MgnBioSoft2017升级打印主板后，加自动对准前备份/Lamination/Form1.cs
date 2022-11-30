using HZH_Controls.Forms;
using MGNBIO.Communication;
using MGNBIO.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lamination
{
    
    public partial class Form1 : Form
    {
        public static SerialPort serialPortvalve_Send;
        public static List<GetOrderClsSimple> task_list_g;
        public static List<GetOrderClsSimple> task_list_query_g;
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
        private void getPortDeviceName()
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
        private void ucBtnImg24_BtnClick(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                //MessageBox.Show("请选择串口");
                FrmDialog.ShowDialog(this, "请选择串口", "提示");
                return;
            }
            if (serialPortvalve_Send.IsOpen)
            {
                FrmDialog.ShowDialog(this, "已连接设置，无需再次连接", "提示");
            }

            string com_number = comboBox1.SelectedItem.ToString();

            //string com_number = "COM4 USB Serial Port";

            string[] com_number_array = com_number.Split(' ');
            if (com_number_array.Length > 0)
            {
                if (!serialPortvalve_Send.IsOpen)
                {
                    try
                    {
                        serialPortvalve_Send.BaudRate = 9600;
                        serialPortvalve_Send.PortName = com_number_array[0];
                        //serialPort_Send.DataBits = 8;
                        serialPortvalve_Send.Open();//打开串口
                        //serialPort1.Write(buf, 0, buf.Length);
                        serialPortvalve_Send.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.OnDataReceivedValve);
                        //serialPort_Send.WriteBufferSize = 1024;
                        //serialPort_Send.ReadBufferSize = 1024;
                        //serialPort1.ReadBufferSize = 1024;
                        serialPortvalve_Send.DataBits = 8;
                        serialPortvalve_Send.Parity = (Parity)0;
                        //serialPort1.StopBits =1;
                        serialPortvalve_Send.ReadTimeout = 2000;
                        serialPortvalve_Send.RtsEnable = true;
                        serialPortvalve_Send.DtrEnable = true;
                        serialPortvalve_Send.ReceivedBytesThreshold = 1;
                        //串口默认的ReceivedBytesThreshold是1
                        //https://www.cnblogs.com/haofaner/p/3402307.html
                        //ucBtnImg27.Enabled = false;
                        //ucBtnImg27.FillColor = Color.Gray;
                    }
                    catch
                    {
                        //ucBtnImg27.Enabled = true;
                        //ucBtnImg27.FillColor = Color.Black;
                        FrmDialog.ShowDialog(this, "连接失败，端口已经被别的程序占用", "提示");
                    }
                }
            }
        }
        private List<byte> buffer = new List<byte>(4096);
        /// <summary>
        /// 是否接收数据
        /// </summary>
        public static bool is_receive_data_g = true;

        private void OnDataReceivedValve(object sender, SerialDataReceivedEventArgs e)
        {
            if (is_receive_data_g)
            {
                byte[] readBuffer = null;
                int n = serialPortvalve_Send.BytesToRead;
                byte[] buf = new byte[n];
                serialPortvalve_Send.Read(buf, 0, n);
                //1.缓存数据           
                buffer.AddRange(buf);
                if (buf.Length >= 7)
                {
                    //处理Z返回
                    DealZReturn(buf);
                }
            }
        }
        /// <summary>
        /// 发送指令
        /// </summary>
        List<SendOffset> send_offset_list_g = new List<SendOffset>();
        public void DealZReturn(byte[] receive_data)
        {
            UInt16 length = (UInt16)(receive_data.Length - 2);
            UInt16 crc = FreeModbus.MB_CRC16(receive_data, length);
            if (receive_data.Length == 9 && receive_data[1] == 4 && receive_data[2] == 4)
            {
                byte a = (byte)crc;                     /* crc 低字节 */
                byte b = (byte)(crc >> 8);              /* crc 高字节 */
                //进行CRC检验
                if (receive_data[7] == a && receive_data[8] == b)
                {
                    //int c = 0;
                    Console.WriteLine("----------------------");
                    if (send_offset_list_g.Count > 0)
                    {
                        byte[] offset_array = new byte[4];
                        //receive_data.CopyTo(offset_array, 10);//
                        Array.Copy(receive_data, 3, offset_array, 0, 4);
                        if (send_offset_list_g.Find(o => o.Status == 2) != null)
                        {
                            int id1 = send_offset_list_g.Find(o => o.Status == 2).Id;
                            int offset1 = send_offset_list_g.Find(o => o.Status == 2).Offset;
                            int offset2 = bytes44ToInt(offset_array, 0);

                            if (offset2 == offset1)
                            {
                                //send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id1));
                                send_offset_list_g.Find(o => o.Id == id1).Status = 3;
                                send_control_flage_pause = false;
                                //loop_send_query_z_axis_position = false;
                                //send_wait_flage = false;
                            }
                        }
                    }
                }
            }
        }


        public void SendCammandString(string buf)
        {
            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes(buf);
            SendCammand(decBytes, "");
        }
        /// <summary>
        /// 两个字转成int 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int bytesToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8));
            //| ((src[offset + 2] & 0xFF) << 16)
            //| ((src[offset + 3] & 0xFF) << 24));
            return value;
        }
        /// <summary>
        /// 4个字转成int 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int bytes4ToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8))
                    | ((src[offset + 2] & 0xFF) << 16)
                    | ((src[offset + 3] & 0xFF) << 24);
            return value;
        }
        /// <summary>
        /// byte 数据转成 int 值
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int bytes44ToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8)
                    | ((src[offset + 2] & 0xFF) << 16)
                    | ((src[offset + 3] & 0xFF) << 24));

            int value1 = (src[3] & 0xFF);
            int value2 = ((src[2] & 0xFF) << 8);
            int value3 = ((src[1] & 0xFF) << 16);
            int value4 = ((src[0] & 0xFF) << 24);
            value = (value3 >> 16) | (value4 >> 16 & 0xff00) | (value1 << 16) | (value2 << 16);
            return value;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            serialPortvalve_Send = new SerialPort();
            getPortDeviceName();
            dataGridView2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(82)))), ((int)(((byte)(168)))));
            task_list_g = new List<GetOrderClsSimple>();
            task_list_query_g = new List<GetOrderClsSimple>();

            Task.Factory.StartNew(() => { SendRunCammand(); });

            Task.Factory.StartNew(() => { SendQueryZAxisPositionloop(); });


            send_offset_list_g = new List<SendOffset>();
            string strReadFilePath = Application.StartupPath + "\\runinfo.txt";
            if (File.Exists(strReadFilePath))
            {
                //存在 
                StreamReader srReadFile = new StreamReader(strReadFilePath);
                string strReadLine = "";
                // 读取流直至文件末尾结束
                while (!srReadFile.EndOfStream)
                {
                    strReadLine = srReadFile.ReadLine(); //读取每行数据 
                    Console.WriteLine(strReadLine);
                    string[] str_array = strReadLine.Split(',');
                    SendOffset order_cls = new SendOffset();
                    order_cls.Id = int.Parse(str_array[0]);
                    order_cls.Name = str_array[1];
                    order_cls.Offset = int.Parse(str_array[2]);
                    order_cls.Type = int.Parse(str_array[3]);
                    order_cls.Status = 1;
                    send_offset_list_g.Add(order_cls);
                }
                // 关闭读取流文件
                srReadFile.Close();
                //return strReadLine;
            }
            else
            {
                //不存在 
                //return "No TXT";
            }


            UpdateListUi();
        }

        private void ucBtnImg36_BtnClick(object sender, EventArgs e)
        {

            if (!serialPortvalve_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBox1.Checked)
            {
                SendZAxisMotorFindZero(1);
            }
            Thread.Sleep(150);
            if (checkBox2.Checked)
            {
                SendZAxisMotorFindZero(2);
            }
        }

        /// <summary>
        /// Z轴电机找零
        /// </summary>
        /// <param name="device_id"></param>
        private void SendZAxisMotorFindZero(byte device_id)
        {
            byte[] order_content_byte_array = new byte[8];
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Find_Zero(device_id, 1, 0, 0, 0x0601);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + 1000);
        }

        public static void SendCammand(byte[] buf_array, string order_content)
        {
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";
            //Console.Write("Time:" + log_time + "发送指令：" + string.Join(" ", buf_array) + " 序号：" + send_order_number_g + "\r\n");
            string send_log = "";
            //send_log = "Time:" + log_time + "发送指令：" + byteToHexStr(buf_array) + " 序号：" + send_order_number_g + "";

            //LogHelper.WriteLog(typeof(FormMain), send_log);


            //var sw = new System.IO.StreamWriter("RunLog.txt", true);
            //sw.WriteLine(
            //    send_log
            //     + "" + ""
            //    + "");
            //sw.Close();
            //send_order_number_g++;
            string[] order_content_array = order_content.Split(':');
            //判断是否有开关指令
            if (order_content_array.Length == 2 && order_content_array[0].Equals("Switch"))
            {
                serialPortvalve_Send.Write(buf_array, 0, buf_array.Length);
            }

        }

        private void ucBtnImg25_BtnClick(object sender, EventArgs e)
        {

            if (!serialPortvalve_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }
            int a = int.Parse(textBox_A_Offset.Text);
            if (checkBox1.Checked)
            {
                SendZAxismotorMove(1, a);
            }

            Thread.Sleep(150);
            a = int.Parse(textBox1.Text);
            if (checkBox2.Checked)
            {
                SendZAxismotorMove(2, a);
            }
        }

        /// <summary>
        /// Z轴电机位移，绝对位置
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="offset"></param>
        private void SendZAxismotorMove(byte device_id, int offset)
        {
            byte[] order_content_byte_array = new byte[8];
            int a = offset;// int.Parse(textBox_A_Offset.Text);
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH10(device_id, 2, 0, 1, 0x0300, 4, a);//填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg37_BtnClick(object sender, EventArgs e)
        {

            if (!serialPortvalve_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            byte[] order_content_byte_array = new byte[8];
            float a = float.Parse(textBox_A_Offset.Text);
            //添加开动作
            if (checkBox1.Checked)
            {
                order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(1, 2, 0, 3, 0x0300, 4, a);//
                SendCammand(order_content_byte_array, "Switch:" + a);
            }

            if (checkBox2.Checked)
            {
                a = int.Parse(textBox1.Text);
                order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(2, 2, 0, 3, 0x0300, 4, a);//
                SendCammand(order_content_byte_array, "Switch:" + a);
            }




        }

        private void ucBtnImg34_BtnClick(object sender, EventArgs e)
        {
            if (!serialPortvalve_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }


            byte[] order_content_byte_array = new byte[8];
            int a = 1000;
            //添加开动作
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_1(1, 1, 0, 0, 0x0101);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            //Tx:084-01 06 00 00 01 01 49 9A
            SendCammand(order_content_byte_array, "Switch:" + a);
        }


        public void UpdateListUi()
        {
            getDataFromList();
        }
        //添加一行
        public void getDataFromList()
        {
            if (send_offset_list_g == null)
                return;
            if (send_offset_list_g.Count == 0)
            {
                return;
            }
            this.dataGridView2.DataSource = null;
            this.dataGridView2.Rows.Clear();
            for (int index = 0; index < send_offset_list_g.Count; index++)
            {
                if (send_offset_list_g.Count > 0)
                {
                    index = this.dataGridView2.Rows.Add();
                    this.dataGridView2.Rows[index].Cells[0].Value = index;
                    this.dataGridView2.Rows[index].Cells[0].Value = send_offset_list_g[index].Id; //电机编号
                    this.dataGridView2.Rows[index].Cells[1].Value = string.Join(" ", send_offset_list_g[index].Name); //指令内容
                    this.dataGridView2.Rows[index].Cells[2].Value = string.Join(" ", send_offset_list_g[index].Offset); //指令内容
                }
            }
        }
        //添加一行
        public void getDataFromList(DataGridView dataGridViewX, List<OrderModelClsLS> order_list)
        {
            dataGridViewX.DataSource = null;
            dataGridViewX.Rows.Clear();
            for (int index = 0; index < order_list.Count; index++)
            {
                if (order_list.Count > 0)
                {
                    index = dataGridViewX.Rows.Add();
                    dataGridViewX.Rows[index].Cells[0].Value = index;
                    dataGridViewX.Rows[index].Cells[0].Value = send_offset_list_g[index].Id; //电机编号
                    dataGridViewX.Rows[index].Cells[1].Value = string.Join(" ", send_offset_list_g[index].Name); //指令内容
                    dataGridViewX.Rows[index].Cells[2].Value = string.Join(" ", send_offset_list_g[index].Offset); //指令内容
                    //dataGridViewX.Rows[index].Cells[3].Value = string.Join(" ", order_list[index].Speed); //指令内容
                    //dataGridViewX.Rows[index].Cells[4].Value = string.Join(" ", order_list[index].Offset);
                    //dataGridViewX.Rows[index].Cells[5].Value = string.Join(" ", order_list[index].Step_name);
                    //dataGridViewX.Rows[index].Cells[6].Value = order_list[index].OrderState;

                }
            }
        }

        private void ucBtnImg2_BtnClick(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                SendOffset order_cls = new SendOffset();
                int i = 0;
                if (send_offset_list_g == null)
                {
                    i = 0;
                } else
                {
                    i = send_offset_list_g.Count;
                }
                i++;
                order_cls.Id = i;
                order_cls.Name = "Z1归零";
                order_cls.Offset = 0;
                order_cls.Status = 1;
                order_cls.Type = 1;
                send_offset_list_g.Add(order_cls);
            }

            //Thread.Sleep(150);
            if (checkBox2.Checked)
            {
                SendOffset order_cls = new SendOffset();
                int i = 0;
                if (send_offset_list_g == null)
                {
                    i = 0;
                }
                else
                {
                    i = send_offset_list_g.Count;
                }
                i++;
                order_cls.Id = i;
                order_cls.Name = "Z2归零";
                order_cls.Offset = 0;
                order_cls.Status = 1;
                order_cls.Type = 2;
                send_offset_list_g.Add(order_cls);
            }
            UpdateListUi();
        }

        private void ucBtnImg3_BtnClick(object sender, EventArgs e)
        {
            int a = int.Parse(textBox_A_Offset.Text);
            if (checkBox1.Checked)
            {
                SendOffset order_cls = new SendOffset();
                int i = 0;
                if (send_offset_list_g == null)
                {
                    i = 0;
                }
                else
                {
                    i = send_offset_list_g.Count;
                }
                i++;
                order_cls.Id = i;
                order_cls.Name = "Z1运动";
                order_cls.Offset = a;
                order_cls.Status = 1;
                order_cls.Type = 1;
                send_offset_list_g.Add(order_cls);
            }
            //Thread.Sleep(150);
            a = int.Parse(textBox1.Text);
            if (checkBox2.Checked)
            {
                SendOffset order_cls = new SendOffset();
                int i = 0;
                if (send_offset_list_g == null)
                {
                    i = 0;
                }
                else
                {
                    i = send_offset_list_g.Count;
                }
                i++;
                order_cls.Id = i;
                order_cls.Name = "Z2运动";
                order_cls.Offset =  a;
                order_cls.Status = 1;
                order_cls.Type = 2;
                send_offset_list_g.Add(order_cls);
                //SendZAxismotorMove(2, a);
            }
            UpdateListUi();
        }

        private void ucBtnImg4_BtnClick(object sender, EventArgs e)
        {
            var sw = new System.IO.StreamWriter("runinfo.txt", false);
            for (int i = 0; i < send_offset_list_g.Count; i++)
            {
                string line = send_offset_list_g[i].Id+ "," + send_offset_list_g[i].Name + "," + send_offset_list_g[i].Offset+","+ send_offset_list_g[i].Type;
                sw.WriteLine(""+ ""+ line);
            }
            FrmDialog.ShowDialog(this, "保存成功", "提示");
            sw.Close();
        }

        private void ucBtnImg1_BtnClick(object sender, EventArgs e)
        {
            if (!serialPortvalve_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            send_offset_list_g = new List<SendOffset>();
            string strReadFilePath = Application.StartupPath + "\\runinfo.txt";
            if (File.Exists(strReadFilePath))
            {
                //存在 
                StreamReader srReadFile = new StreamReader(strReadFilePath);
                string strReadLine = "";
                // 读取流直至文件末尾结束
                while (!srReadFile.EndOfStream)
                {
                    strReadLine = srReadFile.ReadLine(); //读取每行数据 
                    Console.WriteLine(strReadLine);
                    string[] str_array = strReadLine.Split(',');
                    SendOffset order_cls = new SendOffset();
                    order_cls.Id = int.Parse(str_array[0]);
                    order_cls.Name = str_array[1];
                    order_cls.Offset = int.Parse(str_array[2]);
                    order_cls.Type = int.Parse(str_array[3]);
                    order_cls.Status = 1;
                    send_offset_list_g.Add(order_cls);
                }
                // 关闭读取流文件
                srReadFile.Close();
                //return strReadLine;
            }
            else
            {
                //不存在 
                //return "No TXT";
            }


            UpdateListUi();

            send_control_flage = true;
            loop_send_query_z_axis_position = true;
            send_control_flage_pause = false;
            xunh_number = int.Parse(textBox2.Text);
            delay_time_g = int.Parse(textBox3.Text);


        }
        /// <summary>
        /// 只要为true，就一直去查，直到查到的和发送的位置一样，就置为false
        /// </summary>
        public bool loop_send_query_z_axis_position = false;
        /// <summary>
        /// 发送查询Z轴位置指令
        /// </summary>
        public void SendQueryZAxisPositionloop()
        {
            while (true)
            {
                while (loop_send_query_z_axis_position)
                {
                    if (send_offset_list_g.Count > 0)
                    {
                        if (send_offset_list_g.Find(o => o.Status == 2) != null)
                        {
                            int offset = send_offset_list_g.Find(o => o.Status == 2).Offset;
                            int type = send_offset_list_g.Find(o => o.Status == 2).Type;
                            int id = send_offset_list_g.Find(o => o.Status == 2).Id;
                            switch (type)
                            {
                                case 1:
                                    //发送查询Z位置指令
                                    SendReadZAXisPosition(1);
                                    break;
                                case 2:
                                    //发送查询Z位置指令
                                    SendReadZAXisPosition(2);
                                    break;
                            }
                            Thread.Sleep(150);
                        }

                    }
                    Thread.Sleep(150);
                }
                Thread.Sleep(150);
            }
        }
        public void SendReadZAXisPosition(byte z_index)
        {
            byte[] order_content_byte_array = new byte[8];
            order_content_byte_array = ContentTrasforByteOrderCls.ReadInputReg_04H((byte)z_index, 0x0002, 2);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + 1000);
        }

        public static bool send_flage = true;
        public static bool send_control_flage = false;
        public static bool send_control_flage_pause = false;
        public static int xunh_number = 0;
        public static int delay_time_g = 0;
        public void SendRunCammand()
        {
            while (send_flage)
            {
                if (send_control_flage)
                {
                    //send_offset_list_g
                    for (int j = 0; j < xunh_number; j++)
                    {
                        if (j == xunh_number - 1)
                        {
                            xunh_number = 0;
                        }
                        else
                        {
                            for (int i = 0; i < send_offset_list_g.Count; i++)
                            {
                                send_offset_list_g[i].Status = 1;
                            }
                        }
                    }
                    //send_offset_list_g.Find(o => o.Id == id1).Status = 3;
                    List<SendOffset> tmp1 =send_offset_list_g.FindAll(o => o.Status == 1);
                    if (tmp1 != null)
                    {
                        for (int i = 0; i < tmp1.Count; i++)
                        {
                            if (tmp1[i].Status == 1)
                            {
                                int id = tmp1[i].Id;
                                while (send_control_flage_pause)
                                {
                                    Thread.Sleep(1);
                                }

                                SendRunCammand1(tmp1[i].Name, tmp1[i].Type, tmp1[i].Offset);
                                send_offset_list_g.Find(o => o.Id == id).Status = 2;
                                send_control_flage_pause = true;

                                if (tmp1[i].Name.Equals("Z1运动") && tmp1[i].Offset!=0)
                                {
                                    Thread.Sleep(delay_time_g);
                                }

                                Thread.Sleep(200);
                            }
                        }

                    }
                    Thread.Sleep(1000);

                }
                Thread.Sleep(1000);
            }
        }
        public void SendRunCammand1(string name,int type,int offset)
        {
            if (name.Equals("Z1归零"))
            {
                SendZAxisMotorFindZero(1);
            }
            else if (name.Equals("Z2归零"))
            {
                SendZAxisMotorFindZero(2);
            }
            else if (name.Equals("Z2运动"))
            {
                SendZAxismotorMove(2, offset);
            }
            else if (name.Equals("Z1运动"))
            {
                SendZAxismotorMove(1, offset);
            }
        }

        private void ucBtnImg5_BtnClick(object sender, EventArgs e)
        {
            //var sr = new System.IO.StreamReader("runinfo.txt", true);
            //string strLine;
            //while ((strLine = sr.ReadLine()) != null)
            //{
            //    //this.listBox1.Items.Add(strLine);
            //    Console.WriteLine(strLine);
            //}
            send_offset_list_g = new List<SendOffset>();
            string strReadFilePath = Application.StartupPath + "\\runinfo.txt";
            if (File.Exists(strReadFilePath))
            {
                //存在 
                StreamReader srReadFile = new StreamReader(strReadFilePath);
                string strReadLine = "";
                // 读取流直至文件末尾结束
                while (!srReadFile.EndOfStream)
                {
                    strReadLine = srReadFile.ReadLine(); //读取每行数据 
                    Console.WriteLine(strReadLine);
                    string[] str_array = strReadLine.Split(',');
                    SendOffset order_cls = new SendOffset();
                    order_cls.Id = int.Parse(str_array[0]);
                    order_cls.Name = str_array[1];
                    order_cls.Offset = int.Parse(str_array[2]);
                    order_cls.Type = int.Parse(str_array[3]);
                    order_cls.Status = 1;
                    send_offset_list_g.Add(order_cls);
                }
                // 关闭读取流文件
                srReadFile.Close();
                //return strReadLine;
            }
            else
            {
                //不存在 
                //return "No TXT";
            }


            UpdateListUi();

        }
        
        private void DataGridViewListCellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //判断双击的是否为标题
            if (e.RowIndex >= 0)
            {
                //DataTable table = (DataTable)dataGridView2.DataSource;//数据源
                //string id = table.Rows[e.RowIndex]["Id"].ToString();
                //测试
                //MessageBox.Show("id:"+id);
                //当前日期
                send_offset_list_g.Remove(send_offset_list_g[e.RowIndex]);
                //DateTime data = System.DateTime.Today;

                UpdateListUi();
            }


        }
    }
}
