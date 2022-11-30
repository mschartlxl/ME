using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.IO.Ports;
using System.Threading;
using MGNBIO.Model;
using BenNHControl;
using MGNBIO.Control;
using HZH_Controls.Forms;
using Ewin.Client.Frame.UcGrid;
using MGNBIO.Communication;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections;
using TestLog4Net;
using System.Runtime.InteropServices;
using MGNChipMatrix;
using Jyt.Sdk.PrintEngine;

namespace MGNBIO
{
    public partial class FormMain : Form
    {

        public enum DeviceListInfo
        {
            泵001 = 1,
            泵002 = 2,
            阀003 = 3,
            阀004 = 4,
            电磁阀005 = 5,
            电磁阀006 = 6,
            电磁阀007 = 7,
            电磁阀008 = 8
        }

        public FormMain()
        {

            InitializeComponent();
            mainFrm = this;
            //自已绘制  
            //this.treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            //this.treeView1.DrawNode += new DrawTreeNodeEventHandler(treeView1_DrawNode);  
        }
        public void OpenCom()
        {
 
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public byte[] TranPackageAddHeadEnd(byte[] buf_array,string head,string end)
        {
            byte[] byteArrayHead = System.Text.Encoding.Default.GetBytes (head);
            byte[] byteArrayEnd = System.Text.Encoding.Default.GetBytes (end);
            byte[] new_buf_array = new byte[buf_array.Length + 2];
            //new_buf_array.
            return new_buf_array;
        }

        //字节数组转16进制字符串
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
        public static FormMain mainFrm;
        public static int send_order_number_g = 0;
        public static void SendCammand(byte[] buf_array,string order_content)
        {
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";
            //Console.Write("Time:" + log_time + "发送指令：" + string.Join(" ", buf_array) + " 序号：" + send_order_number_g + "\r\n");
            string send_log = "";
            send_log = "Time:" + log_time + "发送指令：" + byteToHexStr(buf_array) + " 序号：" + send_order_number_g + "";

            //LogHelper.WriteLog(typeof(FormMain), send_log);
            FormMain.mainFrm.Invoke((EventHandler)(delegate
            {
                SaveSendOrderLog(buf_array, order_content);
                FormMain.mainFrm.textBox2.AppendText(send_log + "\r\n");
            }));

            //var sw = new System.IO.StreamWriter("RunLog.txt", true);
            //sw.WriteLine(
            //    send_log
            //     + "" + ""
            //    + "");
            //sw.Close();
            send_order_number_g++;
            string[] order_content_array = order_content.Split(':');
            //判断是否有开关指令
            if (order_content_array.Length == 2 && order_content_array[0].Equals("Switch"))
            {
                serialPortvalve_Send.Write(buf_array, 0, buf_array.Length);
            }
            else
            {
                if (order_content_array.Length != 2)
                {
                    serialPort_Send.Write(buf_array, 0, buf_array.Length);
                }
            }
        }

        public static void SendCammandSwitch(byte[] buf_array)
        {
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";
            Console.Write("Time:" + log_time + "发送指令：" + string.Join(" ", buf_array) + " 序号：" + send_order_number_g + "\r\n");
            string send_log = "";
            send_log = "Time:" + log_time + "发送指令：" + byteToHexStr(buf_array) + " 序号：" + send_order_number_g + "";

            LogHelper.WriteLog(typeof(FormMain), send_log);

            FormMain.mainFrm.Invoke((EventHandler)(delegate
            {
                FormMain.mainFrm.textBox2.AppendText(send_log + "\r\n");
            }));

            //var sw = new System.IO.StreamWriter("RunLog.txt", true);
            //sw.WriteLine(
            //    send_log
            //     + "" + ""
            //    + "");
            //sw.Close();
            send_order_number_g++;
            serialPortvalve_Send.Write(buf_array, 0, buf_array.Length);
        }

        public void SendCammandString(string buf)
        {
            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes(buf);
            SendCammand(decBytes,"");
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 获取串口完整名字（包括驱动名字）
        /// 如果找不到类，需要添加System.Management引用，添加引用->程序集->System.Management
        /// </summary>
        Dictionary<String, String> coms = new Dictionary<String, String>();
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

        private List<byte> buffer = new List<byte>(4096);
        public byte[] g_RS232DataBuff;
        public int g_i_DataBuffLen = 0;
        public int g_l_ReceivedCount = 0;
        public static string read_s = "";

        public void PrintBuffer(byte[] readBuffer)
        {
            string recev_data = "";
            //拼成一个完整指令
            if (readBuffer.Length > 1)
            {
                recev_data += string.Join(" ", readBuffer);
            }
            else
            {
                recev_data += string.Join(" ", readBuffer);
            }
            byte[] recev_ds = StringToByteArray(recev_data);
            //byte[] recev_ds = StringToByteArray(recev_data);
            //string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】 ";
            //Console.Write(" time:" + log_time + "收到指令：" + recev_data + "\r\n");
            //处理返回结果
            try
            {
                DealReceiveData(recev_ds);
            }
            catch (Exception eee)
            {
                var st = new StackTrace(eee, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                var sw = new System.IO.StreamWriter("Exception.txt", true);

                sw.WriteLine(
                    DateTime.Now.ToString() + "\r\n"
                     + "DealReceiveData:" + "\r\n"
                    + eee.Message + "\r\n"
                    + eee.InnerException + "\r\n"
                    + eee.Source + "\r\n"
                    + frame + "\r\n"
                    + line);
                sw.Close();
            }
        }
        /// <summary>
        /// 是否接收数据
        /// </summary>
        public static bool is_receive_data_g = true;

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (is_receive_data_g)
            {
                byte[] readBuffer = null;
                int n = serialPort_Send.BytesToRead;
                byte[] buf = new byte[n];
                serialPort_Send.Read(buf, 0, n);
                //1.缓存数据           
                buffer.AddRange(buf);
                if (buf.Length >= 7)
                {
                    PrintBuffer(buf);
                }
            }
        }

        private void OnDataReceivedValve(object sender, SerialDataReceivedEventArgs e)
        {

        }

        public byte[] StringToByteArray(string rec)
        {
            string[] rec_s = rec.Split(' ');
            byte[] rec_byte_array = new byte[rec_s.Length];
            for (int i = 0; i < rec_s.Length; i++)
            {
                //int num = 255;
                if (!rec_s[i].Equals(""))
                {
                    byte b = Convert.ToByte(int.Parse(rec_s[i]));
                    rec_byte_array[i] = b;//Convert.ToByte(rec_s[i], 16);
                }
            }
            return rec_byte_array;
        }
        /// <summary>
        /// 保存发送指令
        /// </summary>
        /// <param name="buf_array"></param>
        /// <param name="order_content"></param>
        public static void SaveSendOrderLog(byte[] buf_array, string order_content)
        {
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";
            //Console.Write("Time:" + log_time + "发送指令：" + string.Join(" ", buf_array) + "-" + order_content + " 序号：" + send_order_number_g + "\r\n");
            string send_log = "";
            send_log = "Time:" + log_time + " 发送指令：" + byteToHexStr(buf_array) + "-" + order_content + " 序号：" + send_order_number_g + "";
            LogHelper.WriteLog(typeof(FormMain), send_log);
        }
        /// <summary>
        /// 保存收到记录日志
        /// </summary>
        /// <param name="receive_data"></param>
        /// <param name="task_id"></param>
        /// <param name="thread_id"></param>
        /// <param name="set_value"></param>
        /// <param name="receive_value"></param>
        public void SaveReceiveLog(byte[] receive_data, int task_id, int thread_id, int set_value, int receive_value, int order_id)
        {
            string recev_data = "";
            string recev_data1 = "";
            //拼成一个完整指令
            if (receive_data.Length > 1)
            {
                recev_data += string.Join(" ", receive_data);
                recev_data1 += byteToHexStr(receive_data);
            }
            else
            {
                recev_data += string.Join(" ", receive_data);
                recev_data1 += byteToHexStr(receive_data);
            }
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";

            //Console.Write("-------------------------Time:" + log_time + "收到指令：" + recev_data + "\r\n");

            string receive = "Time:" + log_time + " 收到指令：" + recev_data1 + "\r\n";

            FormMain.mainFrm.Invoke((EventHandler)(delegate
            {
                FormMain.mainFrm.textBox2.AppendText(receive);
            }));

            LogHelper.WriteLog(typeof(FormMain), "Time:" + log_time + " 收到指令：" + recev_data1 + " task_id:" + task_id + " thread_id:" + thread_id + " set_value:" + set_value + " receive_value:" + receive_value);

            if (!set_value.Equals(receive_value))
            {
                ReSendRunOrder(task_id, thread_id, set_value, receive_value, order_id);
            }
        }


        /// <summary>
        /// 多少次不相等就重发指令,如果收到的相等了，就把他置为0
        /// </summary>
        public static int not_equal_error_number = 0;
        /// <summary>
        ///  如果还一直不相等，重发多少次。
        /// </summary>
        public static int error_resend_number = 0;
        /// <summary>
        /// 重发脚本
        /// </summary>
        /// <param name="task_id"></param>
        /// <param name="thread_id"></param>
        /// <param name="set_value"></param>
        /// <param name="receive_value"></param>
        public void ReSendRunOrder(int task_id, int thread_id, int set_value, int receive_value, int order_id)
        {
            if (task_list_g.Count > 0)
            {
                not_equal_error_number++;
                if (not_equal_error_number == 15)
                {
                    not_equal_error_number = 0;
                    Console.Write(".....................................................................\r\n");
                    GetOrderClsSimple error_order = task_list_g.Find(o => o.Status == 2 && o.Task_id.Equals(task_id + "") && o.Thread_id.Equals(thread_id + "") && o.Id.Equals(order_id));
                    if (error_order != null)
                    {
                        byte[] error_order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(error_order.Order_content);
                        SendCammand(error_order_content_byte_array, error_order.Order_content);
                        Thread.Sleep(120);

                        FormMain.mainFrm.Invoke((EventHandler)(delegate
                        {
                            FormMain.mainFrm.textBox2.AppendText("重发\r\n");
                            LogHelper.WriteLog(typeof(FormMain), "重发" + error_order.Order_content + "task_id" + task_id + "thread_id" + thread_id + "order_id" + order_id);
                            error_resend_number++;
                            if (error_resend_number == 5)
                            {
                                error_resend_number = 0;
                                not_equal_error_number = 0;
                                //电机异常，运行结束
                                Stop();
                                FormMain.mainFrm.textBox2.AppendText("电机异常，运行结束\r\n");
                            }
                        }));
                    }
                }
            }
        }
        /// <summary>
        /// 解析接收到的数据
        /// </summary>
        /// <param name="receive_data"></param>
        public void DealReceiveData(byte[] receive_data)
        {
            string recev_data = string.Join(" ", receive_data);
            //Console.Write(recev_data);
            //阀控制    //泵控制   if返回失败，重发三次，还失败，指令从发送队列中移去
            //泵返回
            //如果没有查询指令返回
            QueryValveHole(receive_data);
            if (query_task_list.Count == 0)
                return;

            //如果返回数据包不够长度返回
            if (receive_data.Length < 5)
                return;

            if (receive_data[0] == 0xFF && receive_data[1] == 0x2F && receive_data[2] == 0x30 && receive_data[3] == 0x60)
            {
                int length = receive_data.Length;
                if (receive_data[length - 3] == 0x03)
                {
                    Console.Write("收到完整的数据包");
                }
                //查询指令处理，如果是运行类指令，那么需要结合查询指令来判断下一次指令是否下发。
                //if()
                //非查询类指令处理
                if (receive_data.Length == 7)
                {
                    Console.Write("应答指令\r\n");
                }
                try
                {
                    if (receive_data.Length > 7)
                    {
                        string result = Encoding.ASCII.GetString(receive_data, 0, receive_data.Length);
                        byte[] tt = FindFirst3Position(receive_data);
                        if (tt.Length == 1)
                        {
                            int a = tt[0];
                            a = a - 48;
                            result = a + ""; 
                        }
                        else
                        {
                            result = Encoding.ASCII.GetString(tt, 0, tt.Length);
                        }
                        if (result != null)
                        {
                            Console.Write("value:" + QueryValue(query_task_list[0].Order_content) + "--" + result + "\r\n");
                        }
                        int value = QueryValue(query_task_list[0].Order_content);
                        int value_return = int.Parse(result);
                        Console.Write("valus_g:" + value + "offset:" + result + "\r\n");
                        //怎么知道是哪一个PUMP的
                        SaveReceiveLog(receive_data, int.Parse(query_task_list[0].Task_id), int.Parse(query_task_list[0].Thread_id), value, value_return, query_task_list[0].Id);

                        if (query_task_list.Count > 0)
                        {
                            ContinueRun(query_task_list[0].Task_id, int.Parse(query_task_list[0].Thread_id), value, value_return, query_task_list[0].Id, receive_data);
                        }

                        //if ((query_task_list[0]+ "").Equals(result))
                        //{
                        //    Console.Write("一致\r\n");
                        //}
                    }

                }
                catch { 
                }
            }

            //阀返回
            if ((receive_data[0] == 0xCC && receive_data[2] == 0xFE))
            {
                Console.Write("阀返回\r\n");
                //RemoveSendList(receive_data);
            }

            //阀查询返回
            if ((receive_data[0] == 0xCC && receive_data[2] == 0x00))
            {
                int receive_value = 0;
                int receive_device_id = receive_data[1];
                if (receive_device_id < 3)
                {
                    receive_value = receive_data[3];
                }
                else
                {
                    byte[] tt = new byte[2];
                    tt[0] = receive_data[3];
                    tt[1] = receive_data[4];
                    receive_value = bytesToInt(tt, 0);
                }

                //string result = Encoding.ASCII.GetString(tt, 0, tt.Length);
                int value = QueryValue(query_task_list[0].Order_content);
                //Console.Write("valus_g:" + value + "hole:" + hole + "\r\n");
                SaveReceiveLog(receive_data, int.Parse(query_task_list[0].Task_id), int.Parse(query_task_list[0].Thread_id), value, receive_value, query_task_list[0].Id);

                if (query_task_list.Count > 0)
                {
                    ContinueRun(query_task_list[0].Task_id, int.Parse(query_task_list[0].Thread_id), value, receive_value, query_task_list[0].Id, receive_data);
                }


                //RemoveSendList(receive_data);
            }
        }

        public int bytesToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8));
                    //| ((src[offset + 2] & 0xFF) << 16)
                    //| ((src[offset + 3] & 0xFF) << 24));
            return value;
        }

        //查询任务完成
        public static bool thread1_query_finish = false;
        public static bool thread2_query_finish = false;
        public void ContinueRun(string task_id, int thread_id, int set_value, int query_value, int id, byte[] receive_data)
        {
            //Console.Write("***************************************************id:" + id + "\r\n");

            if(set_value.Equals(query_value))
            {
                not_equal_error_number = 0;

                var model_query = query_task_list.Where(c => c.Status1 == 0 && c.Task_id.Equals(task_id) && c.Thread_id.Equals(thread_id + "") && c.Id==id).FirstOrDefault();
                if (model_query != null)
                {
                    model_query.Status1 = 3;
                    for (int ii = 0; ii < query_task_list.Count; ii++)
                    {
                        if (query_task_list[ii].Id == id)
                        {
                            query_task_list.Remove(query_task_list.Find(o=>o.Id==id));
                        }
                    }
                    if (thread_id == 0)
                    {
                        sub0_pause_flage = false;
                    }

                    TreeNode node = new TreeNode();//= treeView1.SelectedNode;
                    FormMain.mainFrm.Invoke((EventHandler)(delegate
                    {
                        node = treeView1.SelectedNode;
                    }));

                    if (thread_id == 1)
                    {
                        //sub1_pause_flage = false;
                        if (node.Text.IndexOf("并行") == -1 && node.Parent!=null)
                        {
                            sub1_pause_flage = false;
                        }
                        else
                        {
                            thread1_query_finish = true;
                        }
                    }
                    if (thread_id == 2)
                    {
                        //sub2_pause_flage = false;
                        //thread2_query_finish = true;
                        if (node.Text.IndexOf("并行") == -1 && node.Parent != null)
                        {
                            sub2_pause_flage = false;
                        }
                        else
                        {
                            thread2_query_finish = true;
                        }
                    }

                    if (thread1_query_finish && thread2_query_finish && query_task_list.Count==0)
                    {
                        thread1_query_finish = false;
                        thread2_query_finish = false;
                        sub1_pause_flage = false;
                        Thread.Sleep(120);
                        sub2_pause_flage = false;
                    }
                }
                var model_run = task_list_g.Where(c => c.Status == 2 && c.Task_id.Equals(task_id) && c.Thread_id.Equals(thread_id + "") && c.Id==id).FirstOrDefault();
                if (model_run != null)
                {
                    model_run.Status = 3;
                }
            }
        }

        public byte[] FindFirst3Position(byte[] receive_data)
        {
            byte[] tt = new byte[1];
            switch (receive_data.Length)
            {
                case 11:
                    tt = receive_data.Skip(4).Take(4).ToArray();
                    break;
                case 10:
                    tt = receive_data.Skip(4).Take(3).ToArray();
                    break;
                case 9:
                    tt = receive_data.Skip(4).Take(2).ToArray();
                    break;
                case 8:
                    tt = receive_data.Skip(4).Take(1).ToArray();
                    break;
            }
            return tt;
        }
        /// <summary>
        /// 解析接收到的数据
        /// </summary>
        /// <param name="receive_data"></param>
        public void DealReceiveDataOld(byte[] receive_data)
        {
            string recev_data = string.Join(" ", receive_data);
            //Console.Write(recev_data);
            //Invoke(new EventHandler(delegate
            //{
            //}));

            //阀控制    //泵控制   if返回失败，重发三次，还失败，指令从发送队列中移去
            if ((receive_data[0] == 0xCC && receive_data[2] == 0xFE) || receive_data[0] == 0xFF && receive_data[1] == 0x2F)
            {
                //查询指令处理，如果是运行类指令，那么需要结合查询指令来判断下一次指令是否下发。
                //if()
                //非查询类指令处理
                if (receive_data.Length == 7)
                {
                    Console.Write("应答指令");
                }

                RemoveSendList(receive_data);

            }
            else if (receive_data.Length > 2)
            {
                continue_send_next_receive_flage = false;
                send_order_list_pump1[0].Re_send_number += 1;
                if (send_order_list_pump1[0].Re_send_number == 3)
                {
                    RemoveSendList(receive_data);
                }
            }
        }

        public void RemoveSendList(byte[] receive_data)
        {
            Console.Write("指令发送成功\r\n");
            send_order_list_pump1[0].Receive_data = receive_data;
            send_order_list_log.Add(send_order_list_pump1[0]);
            send_order_list_pump1[0].OrderState = 9;
            send_order_list_pump1.Remove(send_order_list_pump1[0]);
            continue_send_next_receive_flage = false;
            this.Invoke(new EventHandler(delegate
            {
                getDataFromList();
            }));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);

            //System.Environment.Exit(System.Environment.ExitCode);
            //serialPort_Send.Close();
            //try
            //{
            //    System.Environment.Exit(System.Environment.ExitCode);
            //    this.Dispose();
            //    this.Close();
            //}
            //catch (Exception ee)
            //{
            //    Console.Write("" + ee.ToString());
            //}
        }

        public static DeviceInfo device_info = new DeviceInfo();

        public static SerialPort serialPort_Send ;//= new SerialPort(;

        public static SerialPort serialPortvalve_Send;


        public static FrmPrintATCG frm_print_atcg;

        private void Form1_Load(object sender, EventArgs e)
        {
            getPortDeviceName();
            Valve valve1 = new Valve();
            Valve valve2 = new Valve();
            valve1.id = "03";
            valve2.id = "04";
            ValvePump valve_pump1 = new ValvePump();
            valve_pump1.id = "01";
            ValvePump valve_pump2 = new ValvePump();
            valve_pump2.id = "02";
            // 添加两个阀
            device_info.Valve_list.Add(valve1);
            device_info.Valve_list.Add(valve2);
            //添加两个注射泵
            device_info.Valve_pump_list.Add(valve_pump1);
            device_info.Valve_pump_list.Add(valve_pump2);
            tabControlExt1.Select();// = true;
            tabControlExt1.SelectedTab = tabPage1;
            tabControlExt1.TabPages.Remove(tabPage4);
            tabControlExt1.TabPages.Remove(tabPage2);
            tabControlExt1.TabPages.Remove(tabPage7);
            tabControlExt1.TabPages.Remove(tabPage6);
            dataGridView2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(82)))), ((int)(((byte)(168)))));
            serialPort_Send = new SerialPort();
            serialPortvalve_Send = new SerialPort();
            //添加一个根节点
            //var tn = new TreeNode("Root");
            //treeViewEx1.Nodes.Add(tn);
            //treeView1.Nodes.Add("RunRoot");
            //treeView1.
            this.treeView1.HideSelection = false;
            label13.Text = "";
            txtNodeName.Enabled = false;
            try
            {
                //StartThreadPool();
            }
            catch (Exception eee)
            {

                var st = new StackTrace(eee, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                var sw = new System.IO.StreamWriter("Exception.txt", true);

                sw.WriteLine(
                    DateTime.Now.ToString() + "\r\n"
                     + "StartThreadPool:" + "\r\n"
                    + eee.Message + "\r\n"
                    + eee.InnerException + "\r\n"
                    + eee.Source + "\r\n"
                    + frame + "\r\n"
                    + line);
                sw.Close();

            }


            ucBtnImg13.Enabled = false;
            ucBtnImg15.Enabled = false;
            ucBtnImg13.FillColor = Color.Gray;
            ucBtnImg15.FillColor = Color.Gray;


            ucBtnImg14.Enabled = false;
            ucBtnImg14.FillColor = Color.Gray;

            ucBtnImg7.Enabled = false;
            ucBtnImg7.FillColor = Color.Gray;

            FormChipMatrix form = new FormChipMatrix();
            form.TopLevel = false;      //设置为非顶级控件
            tabPage3.Controls.Add(form);
            form.Show();               //让窗体form显示出来

            frm_print_atcg = new FrmPrintATCG();
            frm_print_atcg.TopLevel = false;
            tabPage1.Controls.Add(frm_print_atcg);
            frm_print_atcg.Show();

            FormMoveControl frm_move_control = new FormMoveControl();
            frm_move_control.TopLevel = false;
            tabPage4.Controls.Add(frm_move_control);
            frm_move_control.Show();
            //OpenCommuniction();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] package_content = new byte[] { 0xcc, 0x03, 0x01, 0x00, 0x00,0xdd };
            byte[] package_content_crc = CRC.GetNewCrcArray(package_content);//CRC.CRC16(package_content);
            //CRC.GetKey(package_content);
            TranPackageAddHeadEnd(package_content,0xcc,3,0,0,0,0,0xDD);
            //TranPackageAddHeadEnd();
        }
        //            if (checkBox4.Checked)
        //    {
        //        string s = "/3ZR" + "\r";
        //        byte[] buf = Encoding.ASCII.GetBytes(s);
        //        SendCammand(buf);
        //    }

        //    //CC 03 01 00 00 DD AD 01 

        //}
        public byte[] TranPackageAddHeadEnd(byte[] buf_array, int head, int motor_id,  int cammad_id, int speed, int offset,int position,int end)
        {
            byte[] byteArray_head = intToBytes(head);
            byte[] byteArray_id = intToBytes(motor_id);
            byte[] byteArray_cammad = intToBytes(cammad_id);
            byte[] byteArray_speed = intToBytes(speed);
            byte[] byteArray_position = intToBytes(position);
            byte[] byteArray_end = intToBytes(end);
            byte[] new_buf_array = new byte[buf_array.Length + 2];
            return new_buf_array;
        }

        public byte[] intToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);//高8位
            src[0] = (byte)(value & 0xFF);//低位
            return src;
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public void InitializeAll()
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请先连接设备", "提示");
                return;
            }

            ucBtnImg23.Enabled = false;
            ucBtnImg23.FillColor = Color.Gray;
            //InitializePump(1);
            //Thread.Sleep(30);
            //InitializePump(2);
            //Thread.Sleep(30);
            //InitializeValve(3);
            //Thread.Sleep(30);
            //InitializeValve(4);

            ReSetPump(0x03);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x04);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x05);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x06);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x07);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x08);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x9);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(16);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(17);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(18);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(19);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(20);
            Thread.Sleep(send_delay_time_g);

            byte[] package_content1 = new byte[] { 0xcc, 0x01, 0x45, 0x00, 0x00, 0xdd };
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
            Thread.Sleep(send_delay_time_g);

            byte[] package_content2 = new byte[] { 0xcc, 0x02, 0x45, 0x00, 0x00, 0xdd };
            byte[] package_content_crc2 = CRC.GetNewCrcArray(package_content2);
            SendCammand(package_content_crc2, "");

            initial_flage_g = 1;

            Thread.Sleep(10000);

            ucBtnImg14.Enabled = true;
            ucBtnImg14.FillColor = Color.Black;

            ucBtnImg7.Enabled = true;
            ucBtnImg7.FillColor = Color.Black;

            sub0_pause_flage = false;
            sub1_pause_flage = false;
            sub2_pause_flage = false;

            ucBtnImg13.Enabled = true;
            ucBtnImg15.Enabled = true;
            ucBtnImg13.FillColor = Color.Black;
            ucBtnImg15.FillColor = Color.Black;

            ucBtnImg23.Enabled = true;
            ucBtnImg23.FillColor = Color.Black;


        }

        public void OpenCommuniction() {

            if (comboBox1.SelectedItem == null)
            {
                //MessageBox.Show("请选择串口");
                FrmDialog.ShowDialog(this, "请选择串口", "提示");
                return;
            }

            if (serialPort_Send.IsOpen)
            {
                FrmDialog.ShowDialog(this, "已连接设置，无需再次连接", "提示");
            }

            string com_number = comboBox1.SelectedItem.ToString();
            //string com_number = "COM4 USB Serial Port";
            string[] com_number_array = com_number.Split(' ');
            if (com_number_array.Length > 0)
            {
                if (!serialPort_Send.IsOpen)
                {
                    try
                    {
                        serialPort_Send.BaudRate = 9600;
                        serialPort_Send.PortName = com_number_array[0];
                        //serialPort_Send.DataBits = 8;
                        serialPort_Send.Open();//打开串口
                        //serialPort1.Write(buf, 0, buf.Length);
                        serialPort_Send.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.OnDataReceived);
                        //serialPort_Send.WriteBufferSize = 1024;
                        //serialPort_Send.ReadBufferSize = 1024;
                        //serialPort1.ReadBufferSize = 1024;
                        serialPort_Send.DataBits = 8;
                        serialPort_Send.Parity = (Parity)0;
                        //serialPort1.StopBits =1;
                        serialPort_Send.ReadTimeout = 2000;
                        serialPort_Send.RtsEnable = true;
                        serialPort_Send.DtrEnable = true;
                        serialPort_Send.ReceivedBytesThreshold = 1;
                        //串口默认的ReceivedBytesThreshold是1
                        //https://www.cnblogs.com/haofaner/p/3402307.html
                        ucBtnImg2.Enabled = false;
                        ucBtnImg2.FillColor = Color.Gray;
                    }
                    catch
                    {
                        ucBtnImg2.Enabled = true;
                        ucBtnImg2.FillColor = Color.Black;
                        FrmDialog.ShowDialog(this, "连接失败，端口已经被别的程序占用", "提示");
                    }
                }
            }
        }

        private void ucBtnImg2_BtnClick(object sender, EventArgs e)
        {
        }

        //private static bool thread_order_flage_continue_send = false;
        private static bool continue_send_next_receive_flage = false;
        public static bool thread_send_order_flage_pump2 = true;
        //private static bool thread_order_flage_continue_send_pump2 = false;
        //private static bool continue_send_next_receive_flage_pump2 = false;
        public void AddRunOrderList(string content,int device_id)
        {
            switch (device_id)
            {
                case 1:
                    AddPumpOrder(content, 1);
                    break;
                case 2:
                    AddPumpOrder(content, 2);
                    break;
                case 3:
                    int target_hole = -1;
                    string[] order_sz_array = content.Split(',');
                    if (order_sz_array[1].Length > 1)
                    {
                        target_hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                    }
                    AddValveOrder(3,target_hole);
                    break;
                case 31:
                    AddValueOrderInfo(content,31);
                    break;
                case 32:
                    AddValueOrderInfo(content, 32);
                    break;
                case 33:
                    AddValueOrderInfo(content, 33);
                    break;
                case 34:
                    AddValueOrderInfo(content, 34);
                    break;
                case 1000:
                    AddValueOrderInfo(content, 1000);
                    break;
                default:
                    break;
            }
        }

        public void AddValueOrderInfo(string switch_status,ushort device_id)
        {
            if (switch_status.Equals("1"))
            {
                CoilON(device_id);
            }
            if (switch_status.Equals("0"))
            {
                CoilOFF(device_id);
            }
            //添加延时动作
            if (device_id == 1000)
            {
                byte[] Send_order = new byte[] { 1, 1, 1 };
                string step_name = string.Format("延时", device_id, 1);
                AddRunList(device_id, step_name, int.Parse(switch_status), 0, 0, Send_order);
            }
        }

        public void MakeDelayOrder(string switch_status, ushort device_id)
        {
            if (switch_status.Equals("1"))
            {
                CoilON(device_id);
            }
            if (switch_status.Equals("0"))
            {
                CoilOFF(device_id);
            }
            //添加延时动作
            if (device_id == 1000)
            {
                byte[] Send_order = new byte[] { 1, 1, 1 };
                string step_name = string.Format("延时", device_id, 1);
                AddRunList(device_id, step_name, int.Parse(switch_status), 0, 0, Send_order);
            }
        }

        /// <summary>
        /// 查询阀状态
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="target_hole"></param>
        /// <returns></returns>
        public static byte[] QueryValveStatusOrder(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x3E, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            return package_content_crc1;
        }

        /// <summary>
        /// 查询泵状态
        /// </summary>
        /// <param name="device_id"></param>
        /// <returns></returns>
        public static byte[] QueryPumpStatusOrder(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x66, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            return package_content_crc1;
        }

        /// <summary>
        /// 查询泵状态
        /// </summary>
        /// <param name="device_id"></param>
        /// <returns></returns>
        public static byte[] QueryPumpStatusOrder6(int device_id)
        {
            string s = string.Format("/{0:D}", device_id);
            s += "?6\r";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            //查询时不需要加   "\r"; 
            //string aa = "/1O3V1000A3000";
            //aa = "/1Z";
            //aa += "R\r";

            //string aa = "/1O3V1000A3000";
            //aa = "/1Z";
            //aa += "R\r";
            //byte[] buf = Encoding.ASCII.GetBytes(aa);
            //SendCammand(buf);
            return buf;
        }
        /// <summary>
        /// 初始化泵 /1ZR
        /// </summary>
        /// <param name="device_id"></param>
        public void InitializePump(int device_id)
        {
            string s = string.Format("/{0:D}", device_id);
            s += "ZR\r";
            //复位时,泵1目标孔位在第6
            //if (device_id == 1)
            //{
            // s += "Z0,2,3R\r";
            //}
            ////复位时,泵2目标孔位在第6
            //if (device_id == 2)
            //{
            //    s += "Z0,5,6R\r";
            //}
            byte[] buf = Encoding.ASCII.GetBytes(s);
            //查询时不需要加   "\r"; 
            //string aa = "/1O3V1000A3000";
            //aa = "/1Z";
            //aa += "R\r";
            SendCammand(buf,"");
        }

        public void InitializeValve(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x45, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1,"");
        }
        /// <summary>
        ///     CC 04 45 00 00 DD F2 01 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="device_id"></param>
        public void AddValveOrder(int device_id, int target_hole)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0xA4, 0x00, 0x00, 0xdd };
            //int target_hole = int.Parse(comboBoxFH.SelectedItem.ToString());
            int up_target_hole = target_hole - 1;
            if (target_hole == 0)
            {
                up_target_hole = target_hole + 1;
            }
            package_content1[3] = (byte)target_hole;
            package_content1[4] = (byte)up_target_hole;

            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            string step_name = string.Format("阀{0:D},开{1:D}", device_id, target_hole);
            AddRunList(device_id, step_name, 0, 0, 0, package_content_crc1);
        }

        public void AddPumpOrder(string content, int device_id)
        {
            string[] array1 = content.Split(',');
            string s = string.Format("/{0:D}", device_id);
            string hole = array1[1];//"O" + comboBox_H.SelectedItem.ToString();
            s += hole;
            string speed = array1[2];//"V" + textBox_V.Text.ToString();
            s += speed;
            string offset = array1[3]; //"A" + textBox_A.Text.ToString();
            s += offset;
            //string position = "";
            s += "R\r";
            //device_info.add_valve_pump_send_cammand_list("01", s);
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string step_name = string.Format("泵{0:D}", device_id);
            AddRunList(device_id, step_name, 0, 0, 0, buf);
        }

        private void ucBtnImg5_BtnClick(object sender, EventArgs e)
        {
            //MessageBoxEX.Show("自定义", "Hello");
            //new FrmOKCancel2Test().ShowDialog(this);
            if (FrmDialog.ShowDialog(this, "带确定和取消按钮的弹出提示框？", "模式窗体", true) == System.Windows.Forms.DialogResult.OK)
            {
                FrmDialog.ShowDialog(this, "没有取消按钮的提示框", "模式窗体");
            }
        }
        //发送列表
        public static List<OrderModelClsLS> send_order_list_pump1 = new List<OrderModelClsLS>();
        //public static List<OrderModelCls> send_order_list = new List<OrderModelCls>();
        public static List<OrderModelClsLS> send_order_list_log = new List<OrderModelClsLS>();
        public void ReFreshDataGridView()
        {

        }
        public void AddRunList(int motor_id, string step_name, int offset, int speed,int hole,byte[] send_order_111)
        {
            //if (send_order_list_pump1.Count > 0)
            //{
            //    byte[] aaaa = send_order_list_pump1[send_order_list_pump1.Count - 1].Send_order_a;
            //}
            //如果motor_id=1000,并且TimeLate!=0时，要执行延时后动作

            OrderModelClsLS order_tmp = new OrderModelClsLS();
            order_tmp.Motor_id = motor_id;
            order_tmp.Step_name = step_name;
            order_tmp.Offset = offset;
            order_tmp.Speed = speed;
            order_tmp.Send_order_a = send_order_111;
            order_tmp.OrderState = 1;
            order_tmp.MarkID += 1;
            order_tmp.TimeLate = offset;
            send_order_list_pump1.Add(order_tmp);

            UpdateListUi();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iOrderId"></param>
        /// <param name="iOffset"></param>
        /// <param name="iSpeed"></param>
        /// <param name="iMotorId"></param>
        /// <param name="iOutTime"></param>
        /// <param name="iBoardId"></param>
        /// <param name="stepName"></param>
        /// <param name="iThreshold"></param>
        /// <param name="iThresholdAD"></param>
        public void AddRunList(Int32 iOrderId, Int32 iOffset, Int32 iSpeed, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId, string stepName, Int32 iThreshold, Int32 iThresholdAD)
        {
                GetOrderCls getorder = new GetOrderCls();
                PrjExecutiveOrder.OrderModelCls sendOrderModel = getorder.get_OrderAkso(iOrderId, iOffset, iSpeed, iMotorId, 5000, iBoardId, stepName, iThreshold, iThresholdAD);
                //sendOrderModel.OrderState = 1;
                //增加到一个发送列表中去，用于生成自定义流程
                OrderModelClsLS order_tmp = new OrderModelClsLS();
                order_tmp.OrderID = sendOrderModel.OrderID;
                order_tmp.Send_Data = sendOrderModel.Send_Data;
                order_tmp.OrderInfo = sendOrderModel.OrderInfo;
                order_tmp.OrderName = sendOrderModel.OrderName;
                order_tmp.OrderState = sendOrderModel.OrderState;
                send_order_list_pump1.Add(order_tmp);
                UpdateListUi();
        }
        public void UpdateListUi()
        {
            getDataFromList();
        }
        //添加一行
        public void getDataFromList()
        {
            if (task_list_g == null)
                return;
            if (task_list_g.Count == 0)
            {
                return;
            }
            this.dataGridView2.DataSource = null;
            this.dataGridView2.Rows.Clear();
            for (int index = 0; index < task_list_g.Count; index++)
            {
                if (task_list_g.Count > 0)
                {
                    index = this.dataGridView2.Rows.Add();
                    this.dataGridView2.Rows[index].Cells[0].Value = index;
                    this.dataGridView2.Rows[index].Cells[0].Value = task_list_g[index].Id; //电机编号
                    this.dataGridView2.Rows[index].Cells[1].Value = string.Join(" ", task_list_g[index].Order_content); //指令内容
                    this.dataGridView2.Rows[index].Cells[2].Value = string.Join(" ", task_list_g[index].Task_id); //指令内容
                    this.dataGridView2.Rows[index].Cells[3].Value = string.Join(" ", task_list_g[index].Thread_id); //指令内容

                    switch (task_list_g[index].Status)
                    {
                        case 0:
                            this.dataGridView2.Rows[index].Cells[4].Value = "默认";//task_list[index].Status;
                            break;
                        case 1: 
                            this.dataGridView2.Rows[index].Cells[4].Value = "等待发送";//task_list[index].Status;
                            break;
                        case 2: 
                            this.dataGridView2.Rows[index].Cells[4].Value = "发送完成";//task_list[index].Status;
                            break;
                        case 3: 
                            this.dataGridView2.Rows[index].Cells[4].Value = "执行完成";//task_list[index].Status;
                            break;
                    }


                }
            }
        }

        //添加一行
        public void getDataFromList(DataGridView dataGridViewX,List<OrderModelClsLS> order_list)
        {
            dataGridViewX.DataSource = null;
            dataGridViewX.Rows.Clear();
            for (int index = 0; index < order_list.Count; index++)
            {
                if (order_list.Count > 0)
                {
                    index = dataGridViewX.Rows.Add();
                    dataGridViewX.Rows[index].Cells[0].Value = index;
                    dataGridViewX.Rows[index].Cells[0].Value = order_list[index].Motor_id; //电机编号
                    dataGridViewX.Rows[index].Cells[1].Value = string.Join(" ", order_list[index].Send_order_a); //指令内容
                    dataGridViewX.Rows[index].Cells[2].Value = string.Join(" ", order_list[index].Receive_data); //指令内容
                    //dataGridViewX.Rows[index].Cells[3].Value = string.Join(" ", order_list[index].Speed); //指令内容
                    //dataGridViewX.Rows[index].Cells[4].Value = string.Join(" ", order_list[index].Offset);
                    //dataGridViewX.Rows[index].Cells[5].Value = string.Join(" ", order_list[index].Step_name);
                    //dataGridViewX.Rows[index].Cells[6].Value = order_list[index].OrderState;

                }
            }
        }
        private void ucBtnImg6_BtnClick(object sender, EventArgs e)
        {
            //FreeModbus fmb = new FreeModbus();

            FreeModbus.WriteCoil_0FH(4, 6, 6);//填充写单个线圈寄存器

            //FreeModbus.WriteCoil_05H(4, 31, FreeModbus.Coil_OFF);
            //fmb.WriteCoil_05H(4, 1, 0xff00);//填充写单个线圈寄存器
            serialPort_Send.Write(FreeModbus.Tx_Buf, 0, FreeModbus.TxCount);//串口发送数据
            /*
            for (int j = 0; j < 10; j++)
            {
                for (ushort i = 31; i < 34; i++)
                {
                    //打开1号电磁阀
                    FreeModbus.WriteCoil_05H(4, i, FreeModbus.Coil_ON);//填充写单个线圈寄存器
                    //fmb.WriteCoil_05H(4, 1, 0xff00);//填充写单个线圈寄存器
                    serialPort_Send.Write(FreeModbus.Tx_Buf, 0, FreeModbus.TxCount);//串口发送数据
                    //CoilON(i);
                    //Thread.Sleep(500);
                    //关闭1号电磁阀
                    //FreeModbus.WriteCoil_05H(4, i, FreeModbus.Coil_OFF);//填充写单个线圈寄存器
                    //serialPort_Send.Write(FreeModbus.Tx_Buf, 0, FreeModbus.TxCount);//串口发送数据
                    CoilOFF(i);
                    Thread.Sleep(500);
                }
            }

            */
            //注释：方法WriteCoil_05H 见FreeModbus.cs文件


            //thread_order_flage_continue_send = true;
        }

        public void CoilOFF(ushort device_id)
        {
            byte[] Send_order443345 = WriteCoil_05H(4, device_id, FreeModbus.Coil_OFF);//填充写单个线圈寄存器
            //serialPort_Send.Write(Send_order443345, 0, Send_order443345.Length);//串口发送数据
            //byte[] Send_order = FreeModbus.Tx_Buf;
            string step_name = string.Format("阀{0:D},关{1:D}", device_id, 1);
            AddRunList(device_id, step_name, 0, 0, 0, Send_order443345);
        }

        public byte[] WriteCoil_05H(byte _addr, UInt16 _reg, UInt16 _sta)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[8];

            Tx_Buf[TxCount++] = _addr;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x05;                   /* 功能码 */
            Tx_Buf[TxCount++] = (byte)(_reg >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(_reg);           /* 寄存器地址 低字节 */
            Tx_Buf[TxCount++] = (byte)(_sta >> 8);      /* 线圈(bit)个数 高字节 */
            Tx_Buf[TxCount++] = (byte)(_sta);           /* 线圈(bit)个数 低字节 */

            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }

        public void CoilON(ushort device_id)
        {
            byte[] Send_order12 = WriteCoil_05H(4, device_id, FreeModbus.Coil_ON);//填充写单个线圈寄存器

            //serialPort_Send.Write(Send_order12, 0, Send_order12.Length);//串口发送数据
            //byte[] Send_order = FreeModbus.Tx_Buf;

            string step_name = string.Format("阀{0:D},开{1:D}", device_id, 1);
            AddRunList(device_id, step_name, 0, 0, 0, Send_order12);
        }

        private void ucBtnImg9_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }
        }

        private void ucBtnImg2_BtnClick_1(object sender, EventArgs e)
        {
            OpenCommuniction();
        }

        private void ucBtnImg7_BtnClick(object sender, EventArgs e)
        {
        }



        private void ucBtnImg12_BtnClick(object sender, EventArgs e)
        {
            //添加一个根节点
            //var tn = new TreeNode("Root");
            //treeViewEx1.Nodes.Add(tn);

            //要添加的节点名称为空，即文本框是否为空
            //添加子节点
            if (string.IsNullOrEmpty(txtNodeName.Text.Trim()))
            {
                //MessageBox.Show("要添加的节点内容不能为空！");
                FrmDialog.ShowDialog(this, "要添加的节点内容不能为空", "提示");
                return;
            }
            if (treeView1.SelectedNode == null)
            {
                //MessageBox.Show("请选择要添加子节点的节点！");
                FrmDialog.ShowDialog(this, "请选择要添加子节点的节点", "提示");
                return;
            }

            if(txtNodeName.Equals(""))
            {
                //MessageBoxEX.Show("节点内容不能为空");
                FrmDialog.ShowDialog(this, "节点内容不能为空", "提示");
                return;
            }

            if (txtNodeName.Text == "")
            {
                //MessageBoxEX.Show("脚本信息不能为空");
                FrmDialog.ShowDialog(this, "脚本信息不能为空", "提示");
                return;
            }

            string selected_content = txtNodeName.Text.ToString();

            if (comboBox5.Text.Equals("电磁阀"))
            {
                selected_content = "Switch:" + selected_content;
            }

            if (comboBox5.Text.Equals("延时"))
            {
                selected_content = "iDelay:" + selected_content;
            }
            SetSelectedTreeView(treeView1, double_node_Name, selected_content);
            //string[] node_content_array = txtNodeName.Text.ToString().Split('|');

            //if (node_content_array.Length == 3)
            //{
            //    int a = int.Parse(node_content_array[0]);
            //    int b = int.Parse(node_content_array[1]);
            //    int c = int.Parse(node_content_array[2]);
            //    string fa_open_sz = Convert.ToString(a, 2);
            //    fa_open_sz = new string(fa_open_sz.ToCharArray().Reverse<char>().ToArray<char>());
            //    int delay_time = b;
            //    string fa_close_sz = Convert.ToString(c, 2);
            //    fa_close_sz = new string(fa_close_sz.ToCharArray().Reverse<char>().ToArray<char>());
            //}
            node_content = "";
            txtNodeName.Text = node_content;

        }

        static string str = string.Empty;
        /// <summary>
        /// 10进制转2进制
        /// </summary>
        /// <param name="a"></param>
        public static void GetBinary(int a)
        {
            if (a % 2 == 0)
            {
                str += "0";
            }
            else
            {
                str += "1";
            }
            if (a / 2 < 1)
            {
                return;
            }
            GetBinary(a / 2);
        }　　

        /// <summary>
        /// 设置TreeView选中节点
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="selectStr">选中节点文本</param>
        private void SetSelectedTreeView(TreeView treeView, string selectStr,string new_str)
        {
            treeView.Focus();
            treeView1.SelectedNode.Text = new_str;
            for (int i = 0; i < treeView.Nodes.Count; i++)
            {
                for (int j = 0; j < treeView.Nodes[i].Nodes.Count; j++)
                {
                    if (treeView.Nodes[i].Nodes[j].Text == selectStr)
                    {
                        treeView.Nodes[i].Nodes[j].Name = new_str;
                        double_node_Name = new_str;
                        treeView1.SelectedNode = treeView.Nodes[i].Nodes[j];//选中
                        //treeView.Nodes[i].Nodes[j].Checked = true;
                        treeView.Nodes[i].Expand();//展开父级
                        return;
                    }
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Checked)
                {
                    //TreeNode node1 = e.Node;
                    //Console.Write(" node1.Index:" + node1.Index);
                }
                e.Node.EndEdit(true);
            }
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                int x = e.X;
                int y = e.Y;
                TreeNode CurrentNode = treeView1.GetNodeAt(ClickPoint);
                if (CurrentNode is TreeNode)//判断你点的是不是一个节点
                {
                    treeView1.SelectedNode = CurrentNode;
                    CurrentNode.ContextMenuStrip = this.contextMenuStrip1;
                    contextMenuStrip1.Show(MousePosition);
                }
                else
                {
                    treeView1.ContextMenuStrip = this.contextMenuStrip2;
                    contextMenuStrip2.Show(MousePosition);
                }
            }
            pi = new Point(e.X, e.Y);
        }

        private void 添加根节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmAddNodeName f4 = new FrmAddNodeName();
            if (f4.ShowDialog() == DialogResult.OK)
            {
                treeView1.Nodes.Add(f4.nodeName);
            }
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
        }

        private void 添加子节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            a_g++;
            //FrmAddNodeName f5 = new FrmAddNodeName();
            //if (f5.ShowDialog() == DialogResult.OK)
            //{
            //    treeView1.SelectedNode.Nodes.Add(f5.nodeName);
            //}
            TreeNode add_node = new TreeNode();
            string add_script_tmp = "Tmp" + ",T" + a_g;
            add_node.Name = add_script_tmp;
            add_node.Tag = a_g;
            add_node.Text = add_script_tmp;

            TreeNode node = treeView1.SelectedNode;
            string node_txt = node.Text;
            if (node_txt.IndexOf("并行") != -1 && treeView1.SelectedNode.Nodes.Count==2)
            {
                FrmDialog.ShowDialog(this, "并行节点，系统目前只支持两个子节点的任务列表并发", "提示");
                return;
            }

            if (treeView1.SelectedNode.Text.IndexOf("并行") != -1)
            {
                if (treeView1.SelectedNode.Nodes.Count == 0)
                {
                    //string script_tmp = "Script_Tmp" + ",T1";
                    //treeView1.SelectedNode.Nodes.Add(script_tmp);

                    treeView1.SelectedNode.Nodes.Add(add_node);

                }
                else if (treeView1.SelectedNode.Nodes.Count==1)
                {
                    //string script_tmp = "Script_Tmp" + ",T2";
                    //treeView1.SelectedNode.Nodes.Add(script_tmp);
                    treeView1.SelectedNode.Nodes.Add(add_node);
                }
            }
            else
            {
                //string script_tmp = "Script_Tmp" + ",T0";
                //treeView1.SelectedNode.Nodes.Add(script_tmp);
                treeView1.SelectedNode.Nodes.Add(add_node);
            }

            //int count = treeView1.SelectedNode.Nodes.Count;

        }

        private void 删除子节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            a_g--;
            treeView1.SelectedNode.Remove();
        }

        //在绘制节点事件中，按自已想的绘制  
        //private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        //{
        //    e.DrawDefault = true; //我这里用默认颜色即可，只需要在TreeView失去焦点时选中节点仍然突显  
        //    return;

        //    if ((e.State & TreeNodeStates.Selected) != 0)
        //    {
        //        //演示为绿底白字  
        //        e.Graphics.FillRectangle(Brushes.DarkBlue, e.Node.Bounds);

        //        Font nodeFont = e.Node.NodeFont;
        //        if (nodeFont == null) nodeFont = ((TreeView)sender).Font;
        //        e.Graphics.DrawString(e.Node.Text, nodeFont, Brushes.White, Rectangle.Inflate(e.Bounds, 2, 0));
        //    }
        //    else
        //    {
        //        e.DrawDefault = true;
        //    }

        //    if ((e.State & TreeNodeStates.Focused) != 0)
        //    {
        //        using (Pen focusPen = new Pen(Color.Black))
        //        {
        //            focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        //            Rectangle focusBounds = e.Node.Bounds;
        //            focusBounds.Size = new Size(focusBounds.Width - 1,
        //            focusBounds.Height - 1);
        //            e.Graphics.DrawRectangle(focusPen, focusBounds);
        //        }
        //    }
        //}

        private Point pi;
        public static string double_node_Name = "";
        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.GetNodeAt(pi);
            double_node_Name = node.Text;
            if (pi.X < node.Bounds.Left || pi.X > node.Bounds.Right)
            {
                txtNodeName.Text = "";     //不触发事件   
                return;
            }
            else
            {
                //txtNodeName.Text = "ggg";     //触发事件   
            }   
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Console.Write(treeView1.SelectedNode.Text);
            string selected_node_content = treeView1.SelectedNode.Text;
            string[] node_sz1 = selected_node_content.Split(':');
            //开关和延时
            if (node_sz1.Length == 2)
            {
                if (node_sz1[0].Equals("iDelay"))
                {
                    comboBox5.SelectedItem = "延时";
                }

                if (node_sz1[0].Equals("Switch"))
                {
                    comboBox5.SelectedItem = "电磁阀";
                }

                txtNodeName.Text = node_sz1[1] + "";
                txtNodeName.Enabled = true;
                ucTextBoxExSpeed.Visible = false;
                ucTextBoxExoffset.Visible = false;
                comboBoxHole.Visible = false;

                label7.Visible = false;
                label10.Visible = false;
                label8.Visible = false;
            }
            else
            {
                txtNodeName.Text = selected_node_content;
            }

            //D1,O5,V1000,A4000    阀和泵
            string[] node_sz2 = selected_node_content.Split(',');
            if (node_sz2.Length == 4)
            {
                int device_id = int.Parse(node_sz2[0].Substring(1, node_sz2[0].Length - 1));
                int hole = 0;
                if (node_sz2[1].Length > 1)
                {
                    int.Parse(node_sz2[1].Substring(1, node_sz2[1].Length - 1));

                    comboBoxHole.SelectedItem = node_sz2[1].Substring(1, node_sz2[1].Length - 1);
                }


                //int speed = 0;
                int offset = 0;

                if (node_sz2[2].Length > 1)
                {
                    //speed = int.Parse(node_sz2[2].Substring(1, node_sz2[2].Length - 1));
                    ucTextBoxExSpeed.Visible = true;
                    ucTextBoxExoffset.Visible = true;
                }
                else
                {
                    //speed = -1;
                    ucTextBoxExSpeed.Visible = false;
                    ucTextBoxExoffset.Visible = false;
                }

                if (node_sz2[3].Length > 1)
                {
                    offset = int.Parse(node_sz2[3].Substring(1, node_sz2[3].Length - 1));
                    ucTextBoxExSpeed.Visible = false;
                    comboBoxHole.Visible = false;
                    ucTextBoxExoffset.Visible = true;
                    ucTextBoxExoffset.InputText = ""+offset;
                }
                else
                {
                    offset = -1;
                    ucTextBoxExSpeed.Visible = false;
                    ucTextBoxExoffset.Visible = false;
                }



                comboBox5.Visible = true;
                //comboBoxHole.Visible = true;

                string device_id_sz = "";

                switch (device_id)
                {
                     case 3:
                        device_id_sz = "泵003";
                        break;
                    case 4:
                        device_id_sz = "泵004";
                        break;
                    case 5:
                        device_id_sz = "泵005";
                        break;
                    case 6:
                        device_id_sz = "泵006";
                        break;
                    case 7:
                        device_id_sz = "泵007";
                        break;
                    case 8:
                        device_id_sz = "泵008";
                        break;
                    case 9:
                        device_id_sz = "泵009";
                        break;
                    case 16:
                        device_id_sz = "泵010";
                        break;
                    case 17:
                        device_id_sz = "泵011";
                        break;
                    case 18:
                        device_id_sz = "泵012";
                        break;
                    case 19:
                        device_id_sz = "泵013";
                        break;
                    case 20:
                        device_id_sz = "泵014";
                        break;
                    case 1:
                        device_id_sz = "阀001";
                        //value = 1;
                        break;
                    case 2:
                        device_id_sz = "阀002";
                        break;

                }



                comboBox5.SelectedItem = device_id_sz;
                
                comboBoxHole.SelectedItem = hole + "";

                //if (speed != -1 || offset != -1)
                //{
                //    ucTextBoxExSpeed.InputText = speed+"";
                //    ucTextBoxExoffset.InputText = offset + "";
                //}
            }



            //{
                //Console.Write(e.Node.Name);
                //if (e.Node.Index == 0)
                //{
                //    MessageBox.Show("不可以修改根节点名字");
                //}
                //e.Node.Text = "xxxxxxxxxxxxxxxxxxxxx";
                //if (e.Node.Name == "添加项目")
                //{
                //    MessageBox.Show("添加项目");
                //}
            //}
        }

        private void ucBtnImg11_BtnClick(object sender, EventArgs e)
        {
            //exportToXml(treeView1, "treeView1.xml");
            //SaveXml_Click(sender, e);
            //UpdateScriptXml();
            SaveScriptContent();
        }
        //function: 将dataGridView导出到csv
        private bool SaveScriptContent()
        {
            //if (xml_content.Equals(""))
            //{
            //    MessageBox.Show("没有数据可保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return false;
            //}

            //if (xml_content.Length == 0)
            //{
            //    MessageBox.Show("没有数据可保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return false;
            //}
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SCR files (*.scr)|*.scr";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.CreatePrompt = true;
            saveFileDialog.FileName = null;
            saveFileDialog.Title = "保存";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string file_name = saveFileDialog.FileName.ToString().Split('.')[0];
                    RefreshScriptXml(file_name);
                    //SavePicFileName(saveFileDialog.FileName.ToString().Split('.')[0] + ".jpg");
                    //MessageBox.Show("脚本被保存到：" + saveFileDialog.FileName.ToString(), "保存完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MessageBox.Show("保存完成", "保存完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "保存错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            return true;
        }

        XmlDocument doc = new XmlDocument();
        StringBuilder sb = new StringBuilder();
        //XML每行的内容
        private string xmlLine = "";
        //保存
        private void SaveXml_Click(object sender, EventArgs e)
        {

        }
        public void RefreshScriptXml(string file_name)
        {
            sb = new StringBuilder();
            try
            {
                //写文件头部内容
                //下面是生成RSS的OPML文件
                sb = GetScriptXmlContent();
                StreamWriter sr = new StreamWriter(file_name + ".scr", false, System.Text.Encoding.UTF8);
                sr.Write(sb.ToString());
                sr.Close();
                //toolStripStatusLabel1.Text = "保存成功";
                MessageBox.Show("更新完成", "更新完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //toolStripStatusLabel1.Text = ex.Message;
                Console.Write(ex.ToString());
            }
        }
        public StringBuilder GetScriptXmlContent()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<Tree>");
            //遍历根节点
            foreach (TreeNode node in treeView1.Nodes)
            {
                xmlLine = GetRSSText(node);
                sb.Append(xmlLine);
                //递归遍历节点
                parseNode(node, sb);
                sb.Append("</Node>");
            }
            sb.Append("</Tree>");

            return sb;
        }

        //递归遍历节点内容,最关键的函数
        private void parseNode(TreeNode tn, StringBuilder sb)
        {
            IEnumerator ie = tn.Nodes.GetEnumerator();

            while (ie.MoveNext())
            {
                TreeNode ctn = (TreeNode)ie.Current;
                xmlLine = GetRSSText(ctn);
                sb.Append(xmlLine);
                //如果还有子节点则继续遍历
                if (ctn.Nodes.Count > 0)
                {
                    parseNode(ctn, sb);
                }
                sb.Append("</Node>");
            }
        }
        //成生RSS节点的XML文本行
        private string GetRSSText(TreeNode node)
        {
            //根据Node属性生成XML文本
            string rssText = "<Node Name=\"" + node.Name + "\" Text=\"" + node.Text + "\" >";

            return rssText;
        }

        private void ucBtnImg16_BtnClick(object sender, EventArgs e)
        {

            a_g = 0;
            //System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            //if (folder.ShowDialog() == DialogResult.OK)
            //{
            //    string path = folder.SelectedPath;
            //    Console.Write(path);
            //}
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            //fileDialog.Filter="所有文件(*.*)|*.*";
            fileDialog.Filter = "SCR files (*.scr)|*.scr";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file_path = fileDialog.FileName;
                string file_name = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
                label13.Text = file_name;
                //MessageBox.Show("已选择文件:" + file,"选择文件提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                doc.Load(file_path); //我是把xml放到debug里面了.你的路径就随便啦.不过这样方便一些.
                //如果节点上没有数据，那么

                treeView1.Nodes.Clear();

                if (treeView1.Nodes.Count > 0)
                {
                    RecursionTreeControl1(doc.DocumentElement, treeView1.Nodes);
                }
                else
                {
                    RecursionTreeControl(doc.DocumentElement, treeView1.Nodes);//将加载完成的XML文件显示在TreeView控件中
                }

                //treeView1.ExpandAll();//展开TreeView控件中的所有项
            }
            //treeView1.SelectedNode = treeView1.Nodes[0].Nodes[0].Parent;//选中
            //doc.Load("TreeXml.xml"); //我是把xml放到debug里面了.你的路径就随便啦.不过这样方便一些.
            //RecursionTreeControl(doc.DocumentElement, treeView1.Nodes);//将加载完成的XML文件显示在TreeView控件中
            //treeView1.ExpandAll();//展开TreeView控件中的所有项
            //a_g = 0;


        }
       
        public void LoopOpenFile()
        {
            string a_t_c_g_list = textBox1.Text;
            if (a_t_c_g_list.Equals(""))
            {
                FrmDialog.ShowDialog(this, "请输入合成序列", "提示");
                return;
            }
            //判断是否有不是ATCG的
            int a_number = 0;
            int g_number = 0;
            int c_number = 0;
            int t_number = 0;
            a_number = a_t_c_g_list.ToArray().Where(o => o=='A').Count();
            g_number = a_t_c_g_list.ToArray().Where(o => o=='G').Count();
            c_number = a_t_c_g_list.ToArray().Where(o => o=='C').Count();
            t_number = a_t_c_g_list.ToArray().Where(o => o=='T').Count();
            if (a_t_c_g_list.Length != (a_number + t_number + c_number + g_number))
            {
                FrmDialog.ShowDialog(this, "合成序列输入格式不对，请检查", "提示");
                return;
            }

            for (int i = 0; i < a_t_c_g_list.Length; i++)
            {
                Console.Write("" + a_t_c_g_list[i] + "\r\n");
                string file_name1 = a_t_c_g_list[i] + ".scr";
                //string file_path = fileDialog.FileName;
                //string file_name = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
                //label13.Text = file_name;
                //MessageBox.Show("已选择文件:" + file,"选择文件提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                doc.Load(file_name1); //我是把xml放到debug里面了.你的路径就随便啦.不过这样方便一些.
                //如果节点上没有数据，那么
                if (treeView1.Nodes.Count > 0)
                {
                    RecursionTreeControl1(doc.DocumentElement, treeView1.Nodes);
                }
                else
                {
                    RecursionTreeControl(doc.DocumentElement, treeView1.Nodes);//将加载完成的XML文件显示在TreeView控件中
                }
                treeView1.SelectedNode = treeView1.Nodes[0].Nodes[0].Parent;//选中
                a_g = 0;
            }
        }

        /// <summary>
        /// RecursionTreeControl:表示将XML文件的内容显示在TreeView控件中
        /// </summary>
        /// <param name="xmlNode">将要加载的XML文件中的节点元素</param>
        /// <param name="nodes">将要加载的XML文件中的节点集合</param>
        private void RecursionTreeControl(XmlNode xmlNode, TreeNodeCollection nodes)
        {
            Console.Write("a_g" + a_g + "\r\n");
            a_g++;

            foreach (XmlNode node in xmlNode.ChildNodes)//循环遍历当前元素的子元素集合
            {
                string value = node.Attributes["Text"].Value;
                //if (value.Equals("Root") && nodes.Count > 0)
                //{
                //    //new_child.Text = value;
                //}
                //else
                //{
                //    Console.Write("value:" + value + "\r\n");
                //}

                TreeNode new_child = new TreeNode();//定义一个TreeNode节点对象
                new_child.Name = node.Attributes["Name"].Value;
                new_child.Text = value;
                new_child.Tag = a_g;

                nodes.Add(new_child);//向当前TreeNodeCollection集合中添加当前节点
                RecursionTreeControl(node, new_child.Nodes);//调用本方法进行递归
            }

            //for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            //{
            //    string value = xmlNode.ChildNodes[i].Attributes["Text"].Value;
            //    //if (value.Equals("Root") && nodes.Count > 0)
            //    //{
            //    //    //new_child.Text = value;
            //    //    i++;
            //    //}
            //    //else
            //    //{
            //    Console.Write("value:" + value + "\r\n");
            //    TreeNode new_child = new TreeNode();//定义一个TreeNode节点对象
            //    new_child.Name = xmlNode.ChildNodes[i].Attributes["Name"].Value;
            //    new_child.Text = value;
            //    nodes.Add(new_child);//向当前TreeNodeCollection集合中添加当前节点
            //    RecursionTreeControl(xmlNode.ChildNodes[i], new_child.Nodes);//调用本方法进行递归
            //    //}
            //}

        }
        public static int a_g = 0;
        private void RecursionTreeControl1(XmlNode xmlNode, TreeNodeCollection nodes)
        {
            Console.Write("a_g" + a_g + "\r\n");
            a_g++;
            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                string value = xmlNode.ChildNodes[i].Attributes["Text"].Value;
                //if (value.Equals("Root") && nodes.Count > 0)
                //{
                //    //new_child.Text = value;
                //    i++;
                //}
                //else
                //{
                //Console.Write("value:" + value + "\r\n");
                TreeNode new_child = new TreeNode();//定义一个TreeNode节点对象
                new_child.Name = xmlNode.ChildNodes[i].Attributes["Name"].Value;
                new_child.Text = value;
                new_child.Tag = a_g;
                if(a_g==1)
                {
                    new_child = treeView1.SelectedNode;
                }else
                {
                    nodes.Add(new_child);//向当前TreeNodeCollection集合中添加当前节点
                }
                RecursionTreeControl1(xmlNode.ChildNodes[i], new_child.Nodes);//调用本方法进行递归
                //a_g = 0;
                //}
            }
        }

        public int DeviceNameToId(string device_name)
        {

            //泵003
            //泵004
            //泵005
            //泵006
            //泵007
            //泵008
            //泵009
            //泵010
            //泵011
            //泵012
            //泵013
            //泵014
            //阀001
            //阀002
            //电磁阀
            //延时



            int value = 0;
            switch (device_name)
            {
                case "泵003":
                    value = 3;
                    break;
                case "泵004":
                    value = 4;
                    break;
                case "泵005":
                    value = 5;
                    break;
                case "泵006":
                    value = 6;
                    break;
                case "泵007":
                    value = 7;
                    break;
                case "泵008":
                    value = 8;
                    break;
                case "泵009":
                    value = 9;
                    break;
                case "泵010":
                    value = 16;
                    break;
                case "泵011":
                    value = 17;
                    break;
                case "泵012":
                    value = 18;
                    break;
                case "泵013":
                    value = 19;
                    break;
                case "泵014":
                    value = 20;
                    break;
                case "阀001":
                    value = 1;
                    break;
                case "阀002":
                    value = 2;
                    break;
                case "电磁阀":
                    value = 31;
                    break;
                case "延时":
                    value = 32;
                    break;
                default:
                    break;
            }
            return value;
        }



        public static string node_content = "";
        private void ucBtnImg18_BtnClick(object sender, EventArgs e)
        {
            int index = node_content.IndexOf("O");
            if (index>0)
            {
                if (FrmDialog.ShowDialog(this, "当前脚本已经包含动作信息，是否要更新当前脚本？", "脚本编辑", true) == System.Windows.Forms.DialogResult.OK)
                {
                    //FrmDialog.ShowDialog(this, "没有取消按钮的提示框", "模式窗体");
                    node_content = "";
                }
                else
                {
                    return;
                }
            }

            int device_id = 1;
            string device_id_sz =  comboBox5.Text;
            if(device_id_sz.Equals(""))
            {
                FrmDialog.ShowDialog(this, "请选择设备", "请选择设备");
                return;
            }

            if (device_id_sz.Equals("阀001") || device_id_sz.Equals("阀002"))
            {
                string hole = comboBoxHole.Text;
                if (hole.Equals(""))
                {
                    FrmDialog.ShowDialog(this, "请选择开阀孔", "提示");
                    return;
                }
            }

            if (device_id_sz.Equals("泵001") || device_id_sz.Equals("泵002"))
            {

                string hole = comboBoxHole.Text;
                if (hole.Equals(""))
                {
                    FrmDialog.ShowDialog(this, "请选择开阀孔", "提示");
                    return;
                }

                string speed = ucTextBoxExSpeed.InputText.ToString();
                string offset = ucTextBoxExoffset.InputText.ToString();
                if (speed.Equals("") || offset.Equals("") || speed.Equals("-") || offset.Equals("-"))
                {
                    FrmDialog.ShowDialog(this, "速度和距离的格式不对或是未填写", "提示");
                    return;
                }
            }

            device_id = DeviceNameToId(device_id_sz);
            node_content += "D" + device_id;
            node_content += ",O" + comboBoxHole.Text;
            node_content += ",V" + ucTextBoxExSpeed.InputText.ToString();
            node_content += ",A" + ucTextBoxExoffset.InputText.ToString();

            //if (comboBoxSWitch.Text.Equals("开"))
            //{
            //    node_content += ",S1";// +comboBoxSWitch.Text;
            //}
            //else
            //{
 
            //}

            //if (comboBoxSWitch.Text.Equals("关"))
            //{
            //    node_content += ",S0";// +comboBoxSWitch.Text;
            //}

            /*

            if (!node_content.Equals(""))
            {
                //string s = "/1";
                //string hole = "O" + comboBox_H.SelectedItem.ToString();
                //s += hole;
                //string speed = "V" + textBox_V.Text.ToString();
                //s += speed;
                //string offset = "A" + textBox_A.Text.ToString();
                //s += offset;
                ////string position = "";
                //s += "R\r";
                node_content += "|";
                node_content += "/" + comboBox5.Text;
                node_content += ",O" + comboBoxHole.Text;
                node_content += ",V" + ucTextBoxExSpeed.InputText.ToString();
                node_content += ",A" + ucTextBoxExoffset.InputText.ToString();
                node_content += "," + comboBoxSWitch.Text;
                //node_content += "," + comboBoxLoopType.Text;//
                //node_content += ",G" + ucTextBoxExLoopNumber.InputText.ToString();
            }
            else
            {
                node_content += "/" + comboBox5.Text;
                node_content += ",O" + comboBoxHole.Text;
                node_content += ",V" + ucTextBoxExSpeed.InputText.ToString();
                node_content += ",A" + ucTextBoxExoffset.InputText.ToString();
                node_content += "," + comboBoxSWitch.Text;
                //node_content += "," + comboBoxLoopType.Text;//
                //node_content += ",G" + ucTextBoxExLoopNumber.InputText.ToString();
                //node_content += "," + comboBox5.Text;
                //node_content += "," + comboBoxSWitch.Text;
                //node_content += "," + comboBoxHole.Text;
                //node_content += "," + ucTextBoxExSpeed.InputText.ToString();
                //node_content += "," + ucTextBoxExoffset.InputText.ToString();
                //node_content += "," + comboBoxLoopType.Text;//
                //node_content += "," + ucTextBoxExLoopNumber.InputText.ToString();
            }

           */
            txtNodeName.Text = node_content;
            Console.Write(node_content);
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

            Console.Write(comboBox5.Text + "\r\n;");

            string select_sz = comboBox5.Text;

            if (select_sz.Equals(""))
                return;

            if (select_sz.IndexOf('泵') == 0)
            {
                txtNodeName.Enabled = false;
                ucBtnImg18.Visible = true;
                ShowInputControl(true, false, false, false);
            }

            if (select_sz.IndexOf('阀') == 0)
            {
                ucBtnImg18.Visible = true;
                ShowInputControl(false, false, true, false);
                txtNodeName.Enabled = false;
            }

            if (select_sz.IndexOf('阀') == 2)
            {
                ucBtnImg18.Visible = false;
                ShowInputControl(false, false, false, false);
                txtNodeName.Enabled = true;
            }

            if (select_sz.IndexOf('延') == 0)
            {
                ucBtnImg18.Visible = false;
                ShowInputControl(false, false, false, false);
                txtNodeName.Enabled = true;
            }


            //Console.Write(comboBox5.Text+"\r\n;");

            //switch (comboBox5.Text)
            //{
            //    case "泵001":
            //        txtNodeName.Enabled = false;
            //        ucBtnImg18.Visible = true;
            //        ShowInputControl(true, true, true, false);
            //        break;
            //    case "泵002":
            //        txtNodeName.Enabled = false;
            //        ucBtnImg18.Visible = true;
            //        ShowInputControl(true, true, true, false);
            //        break;
            //    case "阀003":
            //        txtNodeName.Enabled = false;
            //        ucBtnImg18.Visible = true;
            //        ShowInputControl(false, false, true, false);
            //        break;
            //    case "阀004":
            //        ucBtnImg18.Visible = true;
            //        ShowInputControl(false, false, true, false);
            //        txtNodeName.Enabled = false;
            //        break;
            //    case "电磁阀":
            //        ucBtnImg18.Visible = false;
            //        ShowInputControl(false, false, false, false);
            //        txtNodeName.Enabled = true;
            //        break;
            //    case "电磁阀2":
            //        //ShowInputControl(false, false, false, false);
            //        break;
            //    case "空气阀3":
            //        break;
            //    case "延时":
            //        ucBtnImg18.Visible = false;
            //        ShowInputControl(false, false, false, false);
            //        txtNodeName.Enabled = true;
            //        break;
            //        //ShowInputControl(false, false, false, false);

            //    //case "电磁阀008":
            //    //    ShowInputControl(false, false, false, true);
            //    //    break;
            //    default:
            //        break;
            //}

            
        }
        public void ShowInputControl(bool offset_b,bool speed_b,bool hole_b,bool switch_b)
        {

            label8.Visible = offset_b;
            ucTextBoxExoffset.Visible = offset_b;


            label10.Visible = speed_b;
            ucTextBoxExSpeed.Visible = speed_b;

            label7.Visible = hole_b;
            comboBoxHole.Visible = hole_b;

            if (!speed_b)
            {
                ucTextBoxExSpeed.InputText = "";
            }


            if (!offset_b)
            {
                ucTextBoxExoffset.InputText = "";
            }

            //comboBoxSWitch.Visible = switch_b;
        }

        public void GetInputControlValue()
        {
            //textBoxOffset.Visible = offset_b;
            //label8.Visible = offset_b;
            //textBoxSpeed.Visible = speed_b;
            //label10.Visible = speed_b;
            //comboBoxHole.Visible = hole_b;
            //label7.Visible = hole_b;
            //comboBoxSWitch.Visible = switch_b;
        }

        log4net.ILog myLogger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private void ucBtnImg17_BtnClick(object sender, EventArgs e)
        {
            //myLogger.Warn("这是一个警告日志");
            //myLogger.Info("单击了按钮");
            //myLogger.Debug("用Log4Net写入数据库日志");
            //myLogger.Error("这是一个错误日志");
            //myLogger.Fatal("这是一个致命的错误日志");
            node_content = "";
            txtNodeName.Text = node_content;
        }

        private void ucBtnImg12_BtnClick_1(object sender, EventArgs e)
        {

            if (treeView1.SelectedNode == null)
            {
                //MessageBox.Show("请选择要添加子节点的节点！");
                FrmDialog.ShowDialog(this, "请选择要添加或是选择节点", "提示");
                return;
            }

            string loop_number = ucTextBoxExLoopNumber.InputText.ToString();
            string loop_type = comboBoxLoopType.Text;
            if (loop_number.Equals("") || loop_type.Equals("") || loop_number.Equals("-"))
            {
                FrmDialog.ShowDialog(this, "格式不对或未选择类型和次数", "提示");
                return;
            }

            string loop_parameter = "";
            loop_parameter += "G" + loop_number;
            loop_parameter += "," + loop_type;//

            if (treeView1.Nodes.Count == 0)
            {
                return;
            }
            //G2,串行,1
            //G2,串行

            //string thread_id ;
            if (JudgeCX_Or_BXNode(treeView1.SelectedNode.Parent.Text) == 2)
            {
                //如果长度为2
                int length = treeView1.SelectedNode.Text.Split(',').Length;

                if (length == 2)
                {

                    TreeNode node_near_node = treeView1.SelectedNode.PrevNode;
                    TreeNode node_selected = treeView1.SelectedNode;
                    if (node_near_node == null)
                    {
                        node_near_node = treeView1.SelectedNode.NextNode;
                    }

                    if (node_near_node.Text.Split(',').Length == 3)
                    {
                        string thread_id = node_near_node.Text.Split(',')[2];
                        if (thread_id.Equals("1"))
                        {
                            loop_parameter += "," + "2";//
                        }

                        if (thread_id.Equals("2"))
                        {
                            loop_parameter += "," + "1";//
                        }
                    }
                    else
                    {
                        loop_parameter += "," + "1";//
                    }

                    //
                    SetSelectedTreeView(treeView1, double_node_Name, loop_parameter);
                }

                if (length == 3)
                {
                    //检查另外一个在什么任务列表里面，如果他在任务列表1中，那么，当前设置为2，否则设置为1
                    TreeNode node_near_node = treeView1.SelectedNode.PrevNode;
                    TreeNode node_selected = treeView1.SelectedNode;
                    if (node_near_node == null)
                    {
                        node_near_node = treeView1.SelectedNode.NextNode;
                    }


                    if (node_near_node.Text.Split(',').Length == 3)
                    {
                        string thread_id = node_near_node.Text.Split(',')[2];
                        if (thread_id.Equals("1"))
                        {
                            loop_parameter += "," + "2";//
                        }

                        if (thread_id.Equals("2"))
                        {
                            loop_parameter += "," + "1";//
                        }
                    }

                    if (node_near_node.Text.Split(',').Length == 2)
                    {
                        loop_parameter += "," + "1";//
                    }

                    SetSelectedTreeView(treeView1, double_node_Name, loop_parameter);
                }

                if (length > 3)
                {
 
                }
            }
            else
            {
                SetSelectedTreeView(treeView1, double_node_Name, loop_parameter);
            }

        }

        /// <summary>
        /// 判断串行  0，1(串行)，2（并行）
        /// </summary>
        /// <param name="node_text"></param>
        /// <returns></returns>
        public int JudgeCX_Or_BXNode(string node_text)
        { 
            int value = 0;
            if (node_text.IndexOf("串行") != -1)
            {
                value = 1;
            }
            if (node_text.IndexOf("并行") != -1)
            {
                value = 2;
            }
            return value; 
        }



        private void ucBtnImg10_BtnClick(object sender, EventArgs e)
        {
            string file_name = label13.Text;
            RefreshScriptXml(file_name);
        }
        public static string copy_content_g = "";
        public static TreeNode copy_node_g;
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.Focus();
            //copy_content_g = treeView1.SelectedNode.Text;//= new_str;
            copy_node_g = (TreeNode)treeView1.SelectedNode.Clone();
            Console.Write(copy_content_g);
            粘贴ToolStripMenuItem.Enabled = true;
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!copy_node_g.Text.Equals(""))
            {
                //treeView1.SelectedNode.Text = copy_content_g;
                try
                {
                    if (treeView1.SelectedNode.Text.IndexOf("并行") != -1)
                    {
                        int a = treeView1.SelectedNode.Nodes.Count;
                        if (a > 1)
                        {
                            return;
                        }
                        else if (a == 1)
                        {
                            string thread_id = treeView1.SelectedNode.Nodes[0].Text.Split(',')[2];
                            if (thread_id.Equals("1"))
                            {
                                //copy_node_g

                                if (treeView1.SelectedNode.Text.IndexOf("并行") != -1)
                                {

                                    copy_node_g.Text = copy_node_g.Text + ",2";

                                    //if (copy_node_g.Text.Split(',').Length < 3)
                                    //{
                                    //    copy_node_g.Text = copy_node_g.Text + ",1";
                                    //}

                                    //if (copy_node_g.Text.Split(',').Length == 3)
                                    //{

                                    //    copy_node_g.Text = copy_node_g.Text + ",1";
                                    //}

                                }
                            }
                            else
                            {
                                copy_node_g.Text = copy_node_g.Text + ",1";
                            }
                        }
                        else if (a == 0)
                        {
                            if (treeView1.SelectedNode.Text.IndexOf("并行") != -1)
                            {
                                if (copy_node_g.Text.Split(',').Length < 3)
                                {
                                    copy_node_g.Text = copy_node_g.Text + ",1";
                                }
                            }
                        }

                    }
                    treeView1.SelectedNode.Nodes.Add(copy_node_g);
                }
                catch (Exception eee)
                {
                    copy_node_g.Text = ""+treeView1.SelectedNode.Nodes.Count+1;
                    treeView1.SelectedNode.Nodes.Add(copy_node_g);
                    Console.Write(eee.ToString());
                }
            }
            粘贴ToolStripMenuItem.Enabled = false;
        }

        public void DealTreeNode(string node_text)
        {
            if (node_text.Split(',').Length > 1)
            {
                if (node_text.Split(',')[1].Equals("串行"))
                {

                }
                if (node_text.Split(',')[1].Equals("串行"))
                {

                }
            }

            int cx_index = node_text.IndexOf("串行");
            string node2_text = node_text;
            int loop_number2 = 1;

            if (cx_index != -1)
            {
                string[] node2_text_array = node2_text.Split(',');
                loop_number2 = int.Parse(node2_text_array[0].Substring(1, node2_text_array[0].Length - 1));
            }
        }



        /// <summary>
        /// 得到线程编号
        /// </summary>
        /// <param name="sz"></param>
        /// <returns></returns>
        public string GetThread_id(string sz)
        {

            //tn.Parent.Parent:aaaa--tn.Parent.Text:G1,串行--tn.Text:D1,O1,V1200,A1000
            //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,1--tn.Text:Switch:1
            //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,1--tn.Text:Switch:2
            //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,2--tn.Text:Switch:2
            string value = "0";
            string[] sz_array = sz.Split(',');
            if (sz_array.Length == 3)
            {
                value = sz_array[2];
            }
            return value;


        }
        /// <summary>
        /// 第一层循环
        /// </summary>
        public int GetILoop_Number(string sz)
        {
            int value = 1;
            string[] sz_array = sz.Split(',');
            if (sz_array.Length > 1)
            {
                value = int.Parse(sz_array[0].Substring(1, sz_array[0].Length - 1));
            }
            return value;
        }

        /// <summary>
        /// 第二层循环
        /// </summary>
        public int GetILoopNext_Number(string sz)
        {
            int value = 1;
            string[] sz_array = sz.Split(',');
            if (sz_array.Length > 1)
            {
                value = int.Parse(sz_array[0].Substring(1, sz_array[0].Length - 1));
            }
            return value;
        }

        public static bool can_add_iloop_1 = false;

        private void DiGui1(TreeNode tn, ref List<GetOrderClsSimple> simlpe_order_list)
        {
            int level = tn.Level;
            Console.Write(level);

            //1.将当前节点显示到lable上
            //label1.Text += "aaa" + "    " + tn.Text + "\r\n";
            string tn_text = tn.Text;

            if (level == 1 && tn.Text.IndexOf("串行") != -1)
            {
                //Console.Write("一次循环开始");

                if (can_add_iloop_1)
                {
                    iloop++;
                    can_add_iloop_1 = false;
                }
            }

            if (level == 1 && tn.Text.IndexOf("并行") != -1)
            {
                //Console.Write("可以加1了");
                can_add_iloop_1 = true;

            }


            //int cx_level = tn.Level;
            //int cx_index = tn.Text.IndexOf("串行");
            ////int bx_index = tn.Text.IndexOf("并行");
            //if (cx_index != -1 )//|| bx_index!=-1)
            //{
            //    if (ilevel_first == 0 && cx_index != -1)
            //    {
            //        ilevel_first = cx_level;
            //    }
            //    if (ilevel_first == cx_level)
            //    {
            //        //Console.Write("*******************************" + tn.Text + "---level" + tn.Level + "*******************************\r\n");
            //        iloop++;
            //        //parent_text = "";
            //    }
            //    //再次出现串行，循环次数+1;
            //}




            if (tn_text.IndexOf("D") != -1 || tn_text.IndexOf("Switch") != -1 || tn_text.IndexOf("iDelay") != -1 )
            {
                //Console.Write(tn.Level + tn.FullPath + tn.Parent.Tag + "\r\n");
                //Console.Write("tn.Parent.Parent:" + tn.Parent.Parent.Text + "--tn.Parent.Text:" + tn.Parent.Text + "--tn.Text:" + tn.Text + "" + "\r\n");
                //tn.Parent.Parent:aaaa--tn.Parent.Text:G1,串行--tn.Text:D1,O1,V1200,A1000
                //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,1--tn.Text:Switch:1
                //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,1--tn.Text:Switch:2
                //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,2--tn.Text:Switch:2
                GetOrderClsSimple simple_order = new GetOrderClsSimple();
                simple_order.Task_id = iloop + "";
                simple_order.Thread_id = GetThread_id(tn.Parent.Text);
                simple_order.ILoop = GetILoop_Number(tn.Parent.Parent.Text);
                simple_order.ILoopNext = GetILoopNext_Number(tn.Parent.Text);
                simple_order.Order_content = tn.Text;
               

                //Console.Write(simple_order.Task_id + "-" + simple_order.Thread_id + "" + "-" + simple_order.ILoop + "-" + simple_order.ILoopNext + "-" + simple_order.Order_content + "\r\n");
                simlpe_order_list.Add(simple_order);
            }



            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                TreeNode node = tn.Nodes[i];
                DiGui1(node, ref simlpe_order_list);
            }
        }

        private void DiGuiSingleTask(TreeNode tn, ref List<GetOrderClsSimple> simlpe_order_list)
        {
            //1.将当前节点显示到lable上
            //label1.Text += "aaa" + "    " + tn.Text + "\r\n";
            string tn_text = tn.Text;

            int cx_level = tn.Level;
            int cx_index = tn.Text.IndexOf("串行");
            //int bx_index = tn.Text.IndexOf("并行");
            if (cx_index != -1)//|| bx_index!=-1)
            {
                if (ilevel_first == 0 && cx_index != -1)
                {
                    ilevel_first = cx_level;
                }
                if (ilevel_first == cx_level)
                {
                    //Console.Write("*******************************" + tn.Text + "---level" + tn.Level + "*******************************\r\n");
                    //iloop++;
                    //parent_text = "";
                }
                //再次出现串行，循环次数+1;
            }

            if (tn_text.IndexOf("D") != -1 || tn_text.IndexOf("Switch") != -1 || tn_text.IndexOf("iDelay") != -1)
            {
                //Console.Write(tn.Level + tn.FullPath + tn.Parent.Tag + "\r\n");
                //Console.Write("tn.Parent.Parent:" + tn.Parent.Parent.Text + "--tn.Parent.Text:" + tn.Parent.Text + "--tn.Text:" + tn.Text + "" + "\r\n");

                GetOrderClsSimple simple_order = new GetOrderClsSimple();
                simple_order.Task_id = iloop + "";
                simple_order.Thread_id = GetThread_id(tn.Parent.Text);
                simple_order.ILoop = GetILoop_Number(tn.Parent.Parent.Text);
                simple_order.ILoopNext = GetILoopNext_Number(tn.Parent.Text);
                simple_order.Order_content = tn.Text;
                //Console.Write(simple_order.Task_id + "-" + simple_order.Thread_id + "" + "-" + simple_order.ILoop + "-" + simple_order.ILoopNext + "-" + simple_order.Order_content + "\r\n");
                simlpe_order_list.Add(simple_order);
            }



            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                TreeNode node = tn.Nodes[i];
                DiGuiSingleTask(node, ref simlpe_order_list);
            }
        }

        public static List<GetOrderClsSimple> task_list_g;
        public static List<GetOrderClsSimple> task_list_new_g;

        public static List<GetOrderClsSimpleQuery> query_task_list = new List<GetOrderClsSimpleQuery>();
        //public static List<GetOrderClsSimple> query_task_list1 = new List<GetOrderClsSimple>();
        //public static List<GetOrderClsSimple> query_task_list2 = new List<GetOrderClsSimple>();

        public class tb_SensorRecordModel
        {
            public int ID { get; set; }
            public decimal Value1 { get; set; }
        }

        public static List<tb_SensorRecordModel> list = new List<tb_SensorRecordModel>();
        public static int order_count_index_g = 0;

        public static string start_time_g = "";
        public static string current_time_g = "";

        private void ucBtnImg14_BtnClick(object sender, EventArgs e)
        {
            //if (initial_flage_g == 0)
            //{
            //    FrmDialog.ShowDialog(this, "软件第一次打开，请先初始化设备", "提示");
            //    return;
            //}
            //取消树的选中状态

            if (treeView1.Nodes.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }

            treeView1.SelectedNode = treeView1.Nodes[0];
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }
            string a_t_c_g_list = textBox1.Text;
            if (!a_t_c_g_list.Equals(""))
            {
                treeView1.Nodes.Clear();
                LoopOpenFile();
                Thread.Sleep(50);
            }

            if (treeView1.Nodes.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }

            //OpenFile_Preload_Clean("Preload", treeView_preload);
            //ActionConversionOrder(treeView_preload);

            iloop = 0;
            list_index = 0;
            List<GetOrderClsSimple> node_name_list = new List<GetOrderClsSimple>();
            parent_text = "";

            if (treeView1.Nodes.Count > 1)
            {
                return;
            }

            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list_g = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();
            List<GetOrderClsSimple> task_listxxx = new List<GetOrderClsSimple>();
            //TreeNode node_selected = treeView1.SelectedNode;
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                TreeNode node = treeView1.Nodes[i];
                DiGui1(node, ref node_name_list);
            }

            var groupList = node_name_list.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
            //分组循环
            for (int i = 0; i < groupList.Count; i++)
            {
                Int32 sam_count = groupList[i].samcount;
                string task_id = groupList[i].Keys.Task_id;
                List<GetOrderClsSimple> node_name_list_by_task_id = new List<GetOrderClsSimple>();
                node_name_list_by_task_id = node_name_list.FindAll(o => o.Task_id.Equals(task_id));
                //再找出线程ID相同的
                var groupList_by_thread_id = node_name_list_by_task_id.GroupBy(x => new { x.Thread_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
                for (int jj = 0; jj < groupList_by_thread_id.Count; jj++)
                {
                    string thread_id = groupList_by_thread_id[jj].Keys.Thread_id;
                    List<GetOrderClsSimple> node_name_by_loop_number_list = new List<GetOrderClsSimple>();
                    node_name_by_loop_number_list = node_name_list_by_task_id.FindAll(o => o.Thread_id.Equals(thread_id));
                    if (node_name_by_loop_number_list.Count > 0)
                    {
                        int aaa = node_name_by_loop_number_list[0].ILoop;
                        int bbb = node_name_by_loop_number_list[0].ILoopNext;
                        for (int ILoop = 0; ILoop < aaa; ILoop++)
                        {
                            for (int ILoopNext = 0; ILoopNext < bbb; ILoopNext++)
                            {
                                for (int aii = 0; aii < node_name_by_loop_number_list.Count; aii++)
                                {
                                    //Console.Write(node_name_by_loop_number_list[aii].Order_content + "--" +node_name_by_loop_number_list[aii].Task_id + "\r\n");
                                    //GetOrderClsSimple clss = new GetOrderClsSimple();
                                    //clss = node_name_by_loop_number_list[aii];
                                    //clss.Id = task_list.Count + 1;
                                    //task_list.Add(clss);

                                    string[] tmp_array = node_name_by_loop_number_list[aii].Order_content.Split(',');
                                    //如果是阀和泵一起的指令那么拆开来，否则还按原来的方式
                                    if (tmp_array.Length == 4 && tmp_array[3].Length > 1)
                                    {
                                        //one order to two order,one valve,two pump
                                        string aii_content = node_name_by_loop_number_list[aii].Order_content;
                                        //valve
                                        GetOrderClsSimple clss = new GetOrderClsSimple();
                                        //node_name_by_loop_number_list[aii].Order_content = tmp_array[0] + "," + tmp_array[1] + "" + "V,A";
                                        //clss = node_name_by_loop_number_list[aii];
                                        clss.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss.Order_content = node_name_by_loop_number_list[aii].Order_content;//tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss);

                                        //pump
                                        //GetOrderClsSimple clss1 = new GetOrderClsSimple();
                                        ////clss = node_name_by_loop_number_list[aii];
                                        //clss1.Order_content = tmp_array[0] + ",O-1," + tmp_array[2] + "," + tmp_array[3];
                                        //clss1.Id = task_list.Count + 1;
                                        //clss1.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        //clss1.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        //task_listxxx.Add(clss1);

                                    }
                                    else
                                    {
                                        //GetOrderClsSimple clss = new GetOrderClsSimple();
                                        //clss = node_name_by_loop_number_list[aii];
                                        //clss.Id = task_list.Count + 1;
                                        //task_listxxx.Add(clss);
                                        string aii_content1 = node_name_by_loop_number_list[aii].Order_content;
                                        GetOrderClsSimple clss4 = new GetOrderClsSimple();
                                        //node_name_by_loop_number_list[aii].Order_content = tmp_array[0] + "," + tmp_array[1] + "" + "V,A";
                                        //clss4 = node_name_by_loop_number_list[aii];
                                        clss4.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss4.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss4.Order_content = aii_content1;// tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss4.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss4);
                                    }
                                }
                            }
                        }
                    }
                }
                //Console.Write("---------------------------------------------------------------------------" + "\r\n");
            }

            //MultTask();
            for (int jja = 0; jja < task_listxxx.Count; jja++)
            {
                GetOrderClsSimple clss = new GetOrderClsSimple();
                clss.Id = jja;
                clss.ILoop = task_listxxx[jja].ILoop;
                clss.ILoopNext = task_listxxx[jja].ILoopNext;
                clss.Order_content = task_listxxx[jja].Order_content;
                clss.Status = task_listxxx[jja].Status;
                clss.Task_id = task_listxxx[jja].Task_id;
                clss.Thread_id = task_listxxx[jja].Thread_id;
                task_list_new_g.Add(clss);
            }

            task_list_g = new List<GetOrderClsSimple>();
            task_list_g = task_list_new_g;
            var groupList1 = task_list_g.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();

            //重新生成编号

            for (int i = 0; i < groupList1.Count; i++)
            {
                string task_id = groupList1[i].Keys.Task_id;
                int number = groupList1[i].samcount;
                task_id_list.Add(task_id);
                task_id_number_list.Add(number);
            }



            //OpenFile_Preload_Clean("Clean", treeView_clean);
            //ActionConversionOrder(treeView_clean);


            start_time_g = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            label19.Text = "运行时间：" + "00:00:00";
            ucBtnImg13.Enabled = true;
            ucBtnImg15.Enabled = true;
            ucBtnImg13.FillColor = Color.Black;
            ucBtnImg15.FillColor = Color.Black;

            ucBtnImg23.Enabled = false;
            //ucBtnImg24.Enabled = false;
            ucBtnImg23.FillColor = Color.Gray;
            //ucBtnImg24.FillColor = Color.Gray;

            query_sub0_pause_flage = false;

            sub01_pause_flage = true;
            sub11_pause_flage = true;
            sub21_pause_flage = true;


            buffer = new List<byte>(4096);

            sub0_pause_flage = false;
            sub1_pause_flage = true;
            sub2_pause_flage = true;



            task_scheduling_pause_flage = false;

            is_receive_data_g = true;

            show_run_finish_flage_g = true;

            number_current_thread0_status1 = 0;



            ucBtnImg14.Enabled = false;
            ucBtnImg14.FillColor = Color.Gray;


            ucBtnImg7.Enabled = false;
            ucBtnImg7.FillColor = Color.Gray;



            not_equal_error_number = 0;

            //UpdateListUi();
            //var model = task_list.Where(c => c.Task_id.Equals("1") && c.Status.Equals("0"));//.FirstOrDefault();
            //var model = task_list.Where(c => c.Status == 2 && c.Task_id.Equals("0") && c.Thread_id.Equals(0)).FirstOrDefault();
            //model.Status = 3;
            //启动时，线程号2，暂时停状态，只有线程1启动了，

            //thread_order_flage_continue_send = true;
        }

        public static List<GetOrderClsSimple> thread0_orderlist_g = new List<GetOrderClsSimple>();
        public static List<GetOrderClsSimple> thread1_orderlist_g = new List<GetOrderClsSimple>();
        public static List<GetOrderClsSimple> thread2_orderlist_g = new List<GetOrderClsSimple>();

        //开始
        public static bool sub0_flage = true;
        public static bool sub1_flage = true;
        public static bool sub2_flage = true;
        
        //停止
        public static bool sub0_stop_flage = true;
        public static bool sub1_stop_flage = true;
        public static bool sub2_stop_flage = true;
        public static bool query_sub0_stop_flage = false;
        public static bool task_scheduling_stop_flage = false;

        /// <summary>
        /// 查询线程
        /// </summary>
        public static bool query_sub0_flage = true;
        public static bool query_sub1_flage = true;
        public static bool query_sub2_flage = true;

        //查询暂停
        public static bool query_sub0_pause_flage = false;
        public static bool query_sub1_pause_flage = true;
        public static bool query_sub2_pause_flage = true;

        //暂停
        public static bool sub0_pause_flage = false;
        public static bool sub1_pause_flage = true;
        public static bool sub2_pause_flage = true;


        public static bool sub01_pause_flage = false;
        public static bool sub11_pause_flage = false;
        public static bool sub21_pause_flage = false;

        //总调度
        public static bool task_scheduling_flage = true;
        public static bool task_scheduling_pause_flage = false;
        public static string task_id_g = "1";


        public static bool refresh_gridview_flage_g = false;

        /// <summary>
        /// 显示运行完成窗口
        /// </summary>
        public static bool show_run_finish_flage_g = true;


        /// <summary>
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.ToString("c").Substring(0,8);//   00:00:07
            //return ts3.TotalMilliseconds.ToString();
        }

        //public static bool refresh_ui_flage = false;

        public void RefreshUiInfo()
        {
            while (true)
            {
                if (task_list_g != null)
                {
                    if (task_list_g.Count>0)
                    {

                        //while (refresh_ui_flage)
                        //{
                        //    Thread.Sleep(1);
                        //}


                        FormMain.mainFrm.Invoke((EventHandler)(delegate
                        {
                            int total_number = task_list_g.Count;
                            this.progressBar1.Maximum = total_number;//设置进度条最大值


                            int finish_number = task_list_g.FindAll(o => o.Status == 3).Count;
                            this.progressBar1.Value = finish_number;//设置进度条当前值

                            current_time_g = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                            if (!start_time_g.Equals("") && !current_time_g.Equals(""))
                            {

                                DateTime dt1 = Convert.ToDateTime(start_time_g);
                                DateTime dt2 = Convert.ToDateTime(current_time_g);

                                string time_run = "";
                                time_run = ExecDateDiff(dt1, dt2);

      
     
                                label19.Text = "运行时间：" + time_run;
                            }


                            if (finish_number == total_number)
                            {
                                if (show_run_finish_flage_g)
                                {
                                    FrmDialog.ShowDialog(this, "运行完成", "提示");
                                    show_run_finish_flage_g = false;
                                    start_time_g = "";

                                    ucBtnImg14.Enabled = true;
                                    ucBtnImg14.FillColor = Color.Black;


                                    ucBtnImg7.Enabled = true;
                                    ucBtnImg7.FillColor = Color.Black;


                                    ucBtnImg13.Enabled = false;
                                    ucBtnImg15.Enabled = false;
                                    ucBtnImg13.FillColor = Color.Gray;
                                    ucBtnImg15.FillColor = Color.Gray;

                                    ucBtnImg23.Enabled = true;
                                    //ucBtnImg24.Enabled = true;
                                    ucBtnImg23.FillColor = Color.Black;
                                    //ucBtnImg24.FillColor = Color.Black;
                                }
                            }
                            label18.Text = "共" + total_number + "个脚本，当前：" + finish_number;

                            Console.Write("total_number:" + total_number + " finish_number:" + finish_number + "\r\n");
                        }));


                    }
                }

                while (refresh_gridview_flage_g)
                {

                    FormMain.mainFrm.Invoke((EventHandler)(delegate
                    {
                        UpdateListUi();
                    }));

                    refresh_gridview_flage_g = false;
                    Thread.Sleep(1000);
                }
                Thread.Sleep(500);
            }
 
        }



        public void TaskScheduling()
        {
            while (task_scheduling_flage)
            {
                //while (task_scheduling_stop_flage)
                //{
                    if (task_list_g != null)
                    {


                        //暂停
                        while (task_scheduling_pause_flage)
                        {
                            Thread.Sleep(1);
                        }
                    A:
                        //分组循环,一组组的发送
                        //string task_id = task_id_list[0];//task_id_list[0].Keys.Task_id;
                        if (task_id_list.Count > 0)
                        {
                            task_id_g = task_id_list[0];//task_id_list[0].Keys.Task_id;
                        }

                        for (int jj = 0; jj < task_list_g.Count; jj++)
                        {
                            //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                            string id = task_list_g[jj].Task_id;
                            string status = task_list_g[jj].Status + "";
                            if (task_id_g.Equals(id) && status.Equals("0"))
                            {
                                task_list_g[jj].Status = 1;
                                //sub0_pause_flage = false;
                            }
                        }

                        for (int jjj = 0; jjj < task_id_number_list.Count; jjj++)
                        {
                            string task_id_sz = task_id_list[jjj];
                            int task_id_number = task_id_number_list[jjj];
                            int task_status3_number = task_list_g.FindAll(o => o.Task_id.Equals(task_id_sz) && o.Status == 3).Count;
                            int task_status1_number = task_list_g.FindAll(o => o.Task_id.Equals(task_id_sz) && o.Status == 1).Count;
                            if (task_status3_number == task_id_number)
                            {
                                if (task_id_list.Count > 0)
                                {
                                    sub1_pause_flage = true;
                                    sub2_pause_flage = true;
                                    sub0_pause_flage = true;
                                    task_id_list.Remove(task_id_list[0]);
                                    task_id_number_list.Remove(task_id_number_list[0]);
                                    goto A;
                                }
                            }

                            //第二个任务开始时
                            if (task_status1_number == task_id_number)
                            {
                                //第一个任务开始为0
                                if (!task_id_sz.Equals("0"))
                                {
                                    if (query_task_list.Count == 0)
                                    {
                                        sub1_pause_flage = true;
                                        sub2_pause_flage = true;
                                        sub0_pause_flage = false;
                                    }
                                }
                            }
                        }
                        //先计算出为这个队列中
                        //Console.Write("sub2_flage");
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(20);
                //}

            }
        }

        public void StartThreadPool()
        {
            bool pool = ThreadPool.SetMaxThreads(9, 9);
            if (pool)
            {
                //分发任务
                ThreadPool.QueueUserWorkItem(o =>
                {
                    TaskScheduling();
                });

                //0
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderSub0();
                });

                ////1
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderSub1();
                });

                ////2
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderSub2();
                });

                //增加三个查询状态的线程，如果不是开关阀，发送完指令后要进行查询
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderQuerySub0();
                });


                ThreadPool.QueueUserWorkItem(o =>
                {
                    RefreshUiInfo();
                });
                


                ////1
                //ThreadPool.QueueUserWorkItem(o =>
                //{
                //    SendOrderQuerySub1();
                //});
                ////2
                //ThreadPool.QueueUserWorkItem(o =>
                //{
                //    SendOrderQuerySub2();
                //});
            };
        }


        /// <summary>
        /// 用于与查询结果做比较，如果数据相等，那移除发送队列中发送查询的指令，把自己重置为-1，并且把队列中发送的暂停标识改为false可以进行下一条指令的发送，
        /// </summary>
        //public static int valus_g = -1;
        //public static int valus1_g = -1;
        //public static int valus2_g = -1;

        public List<GetRecevieData> set_value_list_g = new List<GetRecevieData>();

        /// <summary>
        /// 分别三个查询队列，查询完成后，从队列中移除掉
        /// </summary>
        /// <param name="order_query"></param>
        public void AddQueryOrder(GetOrderClsSimple order_query)
        {
            string order_content = order_query.Order_content;

            int index = -1;
            index = order_content.IndexOf("D");

            if (index == -1)
                return;
            //order_query.Status = 0;
            GetOrderClsSimpleQuery order_q = new GetOrderClsSimpleQuery();
            order_q.Order_content = order_query.Order_content;
            order_q.Status1 = 0;
            order_q.Task_id = order_query.Task_id;
            order_q.Thread_id = order_query.Thread_id;
            order_q.Id = order_query.Id;
            switch (order_query.Thread_id)
            {
                case "0":
                    query_task_list.Add(order_q);
                    break;
                case "1":
                    query_task_list.Add(order_q);
                    break;
                case "2":
                    query_task_list.Add(order_q);
                    break;
            }

            //var model_query = query_task_list.Where(c => c.Status1 == 0 && c.Task_id.Equals("1") && c.Thread_id.Equals("0")).FirstOrDefault();
            //model_query.Status1 = 2;



        }
        /// <summary>
        /// 得到要查询的值，如果为阀，那么值是目标孔，如果为泵，那么值为目标注液泵和行程
        /// </summary>
        /// <param name="order_content"></param>
        /// <param name="thread_id">0->query_value</param>
        /// <returns></returns>
        public int QueryValue(string order_content)
        {
            int value = -1;
            int device_id = -1;
            int hole = -1;
            //int speed = -1;
            int offset = -1;

            string[] order_sz_array_switch = order_content.Split(':');
            if (order_sz_array_switch.Length == 2)
            {

            }
            else
            {
                string[] order_sz_array = order_content.Split(',');
                if (order_sz_array[0].Length > 1)
                {
                    device_id = int.Parse(order_sz_array[0].Substring(1, order_sz_array[0].Length - 1));
                }

                if (order_sz_array[1].Length > 1)
                {
                    hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                }

                if (order_sz_array[3].Length > 1)
                {
                    offset = int.Parse(order_sz_array[3].Substring(1, order_sz_array[3].Length - 1));
                }
                //泵
                if (device_id > 2 && device_id < 21)
                {
                    //生成泵的查询指令
                    if (order_sz_array[2].Equals("V-1"))
                    {
                        value = hole;
                    }
                    else
                    {
                        value = offset;
                    }



                }
                //生成阀的查询指令
                //if (device_id > 2 && device_id < 5)
                if (device_id < 3)
                {
                    value = hole;
                }
            }
            return value;
        }

        public static byte[] QueryContentTrasforByteOrder(string Order_content)
        {
            byte[] order_content_byte_array = new byte[8];
            string order_sz = Order_content;
            int device_id = -1;
            int hole = -1;
            int speed = -1;
            int offset = -1;
            if (order_sz.Length > 0)
            {
                string[] order_sz_array_switch = order_sz.Split(':');
                if (order_sz_array_switch.Length == 2)
                {

                }
                else
                {
                    string[] order_sz_array = order_sz.Split(',');
                    if (order_sz_array[0].Length > 1)
                    {
                        device_id = int.Parse(order_sz_array[0].Substring(1, order_sz_array[0].Length - 1));
                    }

                    if (order_sz_array[1].Length > 1)
                    {
                        hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                    }

                    if (order_sz_array[2].Length > 1)
                    {
                        speed = int.Parse(order_sz_array[2].Substring(1, order_sz_array[2].Length - 1));
                    }

                    if (order_sz_array[3].Length > 1)
                    {
                        offset = int.Parse(order_sz_array[3].Substring(1, order_sz_array[3].Length - 1));
                    }
                    //泵
                    if (device_id > 2 && device_id < 21)
                    {
                        //pump
                        order_content_byte_array = QueryPumpStatusOrder(device_id);
                    }
                    //CC 08 66 00 00 DD 17 02 
                    //生成阀的查询指令
                    //if (device_id > 2 && device_id < 5)
                    if (device_id < 3)
                    {
                        order_content_byte_array = QueryValveStatusOrder(device_id);
                    }
                }
            }
            return order_content_byte_array;
        }
        /// <summary>
        /// 线程0中当前状态为1的脚本数量
        /// </summary>
        public static int number_current_thread0_status1 = 0;
        public void SendOrderSub0()
        {
            while (sub0_flage)
            {
                if (task_list_g != null)
                {

                    while (sub01_pause_flage)
                    {
                        List<GetOrderClsSimple> tmp_list = task_list_g.FindAll(o => o.Status == 1);
                        for (int jj = 0; jj < tmp_list.Count; jj++)
                        {
                            //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                            int pkid = tmp_list[jj].Id;
                            string id = tmp_list[jj].Task_id;
                            string status = tmp_list[jj].Status + "";
                            string thread_id = tmp_list[jj].Thread_id;
                            //暂停
                            while (sub0_pause_flage)
                            {
                                Thread.Sleep(1);
                            }

                            if (task_list_g.Count > 0)
                            {
                                //算出总数
                                if (number_current_thread0_status1 == 0)
                                {
                                    number_current_thread0_status1 = tmp_list.FindAll(o => o.Status.Equals(1) && o.Thread_id.Equals("0")).Count;
                                }
                                //一开始统计一下当前线程，并且状态为1的指令一共有多少条
                                int total_status_3 = tmp_list.FindAll(o => o.Status.Equals(3) && o.Thread_id.Equals("0")).Count;
                                if (number_current_thread0_status1 == total_status_3)
                                {
                                    Console.Write("--------------------------------------------------------------------" + "\r\n");
                                    sub1_pause_flage = false;
                                    Thread.Sleep(120);
                                    sub2_pause_flage = false;
                                    number_current_thread0_status1 = 0;
                                    sub0_pause_flage = true;
                                }
                                else
                                {
                                    Console.Write("total_status_1:" + number_current_thread0_status1 + "total_status_3" + total_status_3 + "\r\n");
                                }

                                if (status.Equals("1") && thread_id.Equals("0"))
                                {
                                    //todo
                                    byte[] order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(tmp_list[jj].Order_content);
                                    SendCammand(order_content_byte_array, tmp_list[jj].Order_content);
                                    //task_list[jj].Status = 3;
                                    //发送指令
                                    //task_list[jj].Status = 2;
                                    if (task_list_g.Count > 0)
                                    {
                                        var model_run2 = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                        model_run2.Status = 2;
                                    }

                                    if (JudgeSwitchOrDelay(tmp_list[jj].Order_content))
                                    {
                                        if (task_list_g.Count > 0)
                                        {
                                            var model_run = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                            model_run.Status = 3;
                                        }

                                        int delay_time = GetDelayTime(tmp_list[jj].Order_content);
                                        if (delay_time > 0)
                                        {
                                            Thread.Sleep(delay_time);
                                        }
                                    }

                                    //如果有开关，有延时的指令那么不用等待返回就可以继续发一下条指令
                                    string[] order_sz_array_switch = tmp_list[jj].Order_content.Split(':');
                                    if (order_sz_array_switch.Length == 2)
                                    {
                                        Thread.Sleep(50);
                                    }
                                    else
                                    {
                                        //Thread.Sleep(200);
                                        AddQueryOrder(tmp_list[jj]);
                                        sub0_pause_flage = true;
                                    }
                                    //如果下一条指令为并行，那么当前指令执行完成后，暂停当前线程，，跳到并发线程1和并发线程2，在线程1中查找对应的指令下发，在线程2中去列表查找对应的指令下发，所有
                                    //并发指令都执行完成后，查看队列中状态为1，是否还有别的指令未发送，如果为串行，则去掉本方法的暂停标识，继续发送并行后的串行命令
                                    //如果当前串行指令执行完成了。
                                    Thread.Sleep(10);
                            
                            }

                            
                            }




                        }
                        Thread.Sleep(100);
                        Thread.Sleep(120);
                    }


                }

            }
        }
        /// <summary>
        /// 得到延时时间
        /// </summary>
        /// <param name="sz"></param>
        /// <returns></returns>
        public int GetDelayTime(string sz)
        {
            int result = 0;
            string[] sz_array = sz.Split(':');
            if (sz_array.Length == 2)
            {
                if (sz_array[0].Equals("iDelay"))
                {
                    result = int.Parse(sz_array[1]);
                }
            }
            return result;
        }
        //判断是否为开关或是延时，如果是的话不需要查询指令
        public bool JudgeSwitchOrDelay(string sz)
        {
            bool result = false;
            string[] sz_array = sz.Split(':');
            if (sz_array.Length == 2)
            {
                result = true;
            }
            return result;
        }
  
        public void SendOrderSub1()
        {
            while (sub1_flage)
            {
                if (task_list_g != null)
                {

                    while (sub11_pause_flage)
                    {


                        List<GetOrderClsSimple> tmp_list = task_list_g.FindAll(o => o.Status == 1 && o.Thread_id.Equals("1"));
                        for (int jj = 0; jj < tmp_list.Count; jj++)
                        {
                            //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                            int pkid = tmp_list[jj].Id;
                            string id = tmp_list[jj].Task_id;
                            string status = tmp_list[jj].Status + "";
                            string thread_id = tmp_list[jj].Thread_id;
                            //暂停
                            while (sub1_pause_flage)
                            {
                                Thread.Sleep(1);
                            }

                            if (task_list_g.Count > 0)
                            {
                                if (status.Equals("1") && thread_id.Equals("1"))
                                {
                                    //todo
                                    //如果是泵的指令，要先查询孔位是不是已经
                                    byte[] order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(tmp_list[jj].Order_content);
                                    SendCammand(order_content_byte_array, tmp_list[jj].Order_content);
                                    //task_list[jj].Status = 3;
                                    //发送指令
                                    //task_list[jj].Status = 2;
                                    if (task_list_g.Count > 0)
                                    {
                                        var model_run2 = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                        model_run2.Status = 2;

                                    }
                                    if (JudgeSwitchOrDelay(tmp_list[jj].Order_content))
                                    {
                                        if (task_list_g.Count > 0)
                                        {
                                            var model_run = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                            model_run.Status = 3;
                                        }
                                        int delay_time = GetDelayTime(tmp_list[jj].Order_content);
                                        if (delay_time > 0)
                                        {
                                            Thread.Sleep(delay_time);
                                        }
                                    }

                                    //如果有开关，有延时的指令那么不用等待返回就可以继续发一下条指令
                                    string[] order_sz_array_switch = tmp_list[jj].Order_content.Split(':');
                                    if (order_sz_array_switch.Length == 2)
                                    {
                                        Thread.Sleep(50);
                                    }
                                    else
                                    {
                                        //Thread.Sleep(200);
                                        AddQueryOrder(tmp_list[jj]);
                                        sub1_pause_flage = true;
                                    }

                                    //如果下一条指令为并行，那么当前指令执行完成后，暂停当前线程，，跳到并发线程1和并发线程2，在线程1中查找对应的指令下发，在线程2中去列表查找对应的指令下发，所有
                                    //并发指令都执行完成后，查看队列中状态为1，是否还有别的指令未发送，如果为串行，则去掉本方法的暂停标识，继续发送并行后的串行命令
                                    //如果当前串行指令执行完成了。
                                    Thread.Sleep(50);
                                }
                            }
                        }

                        Thread.Sleep(120);
                    }

                }

   
            }
        }

        public void SendOrderSub2()
        {
            while (sub2_flage)
            {
                if (task_list_g != null)
                {

                    while (sub21_pause_flage)
                    {
                        List<GetOrderClsSimple> tmp_list = task_list_g.FindAll(o => o.Status == 1 && o.Thread_id.Equals("2"));
                        for (int jj = 0; jj < tmp_list.Count; jj++)
                        {
                            //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                            int pkid = tmp_list[jj].Id;
                            string id = tmp_list[jj].Task_id;
                            string status = tmp_list[jj].Status + "";
                            string thread_id = tmp_list[jj].Thread_id;
                            //暂停
                            while (sub2_pause_flage)
                            {
                                Thread.Sleep(1);
                            }
                            if (task_list_g.Count > 0)
                            {
                                if (status.Equals("1") && thread_id.Equals("2"))
                                {
                                    //todo
                                    byte[] order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(tmp_list[jj].Order_content);
                                    SendCammand(order_content_byte_array, tmp_list[jj].Order_content);
                                    //task_list[jj].Status = 3;
                                    //发送指令
                                    //task_list[jj].Status = 2;
                                    if (task_list_g.Count > 0)
                                    {
                                        var model_run2 = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                        model_run2.Status = 2;
                                    }
                                    if (JudgeSwitchOrDelay(tmp_list[jj].Order_content))
                                    {
                                        if (task_list_g.Count > 0)
                                        {
                                            //修改列表中某一个元素
                                            var model_run = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                            model_run.Status = 3;
                                        }
                                        int delay_time = GetDelayTime(tmp_list[jj].Order_content);
                                        if (delay_time > 0)
                                        {
                                            Thread.Sleep(delay_time);
                                        }
                                    }

                                    //如果有开关，有延时的指令那么不用等待返回就可以继续发一下条指令
                                    string[] order_sz_array_switch = tmp_list[jj].Order_content.Split(':');
                                    if (order_sz_array_switch.Length == 2)
                                    {
                                        Thread.Sleep(50);
                                    }
                                    else
                                    {
                                        Thread.Sleep(220);
                                        AddQueryOrder(tmp_list[jj]);
                                        sub2_pause_flage = true;
                                    }

                                    //如果下一条指令为并行，那么当前指令执行完成后，暂停当前线程，，跳到并发线程1和并发线程2，在线程1中查找对应的指令下发，在线程2中去列表查找对应的指令下发，所有
                                    //并发指令都执行完成后，查看队列中状态为1，是否还有别的指令未发送，如果为串行，则去掉本方法的暂停标识，继续发送并行后的串行命令
                                    //如果当前串行指令执行完成了。
                                    Thread.Sleep(50);
                                }
                            
                            }

                        }
                        Thread.Sleep(120);
                    }


                }
            }
        }

        public void SendOrderQuerySub0()
        {
            while (query_sub0_flage)
            {
                if (query_task_list.Count > 0)
                {

                    ///暂停
                    while (query_sub0_pause_flage)
                    {
                        Thread.Sleep(1);
                    }

                    List<GetOrderClsSimpleQuery> query_task_tmp = query_task_list.FindAll(o => o.Status1 != 3);
                    if (query_task_tmp.Count > 0)
                    {
                        byte[] order_content_byte_array = QueryContentTrasforByteOrder(query_task_tmp[0].Order_content);
                        //Console.Write("SendOrderQuerySub0 ----query_task_tmp[0]" + query_task_tmp[0].Status1 + "\r\n");
                        Thread.Sleep(140);
                        SendCammand(order_content_byte_array, query_task_tmp[0].Order_content);
                        Thread.Sleep(140);

                    }

                    if (query_task_tmp.Count > 1)
                    {
                        byte[] order_content_byte_array1 = QueryContentTrasforByteOrder(query_task_tmp[1].Order_content);
                        //Console.Write("SendOrderQuerySub0 ----query_task_tmp[1]" + query_task_tmp[1].Status1 + "\r\n");
                        Thread.Sleep(140);
                        SendCammand(order_content_byte_array1, query_task_tmp[1].Order_content);
                        Thread.Sleep(140);
                    }



                        //暂停
                        //while (query_sub0_pause_flage)
                        //{
                        //    Thread.Sleep(1);
                        //}
                        ////找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                        //string id = query_task_list[jj].Task_id;
                        //string status = query_task_list[jj].Status1 + "";
                        //string thread_id = query_task_list[jj].Thread_id;
                        //if (status.Equals("0") && thread_id.Equals("0"))
                        //{
                        //    //todo
                        //    //发送指令
                        //    //如果查询到的值与设定值一致了，那么。。。
                        //    int valus = QueryValue(query_task_list[jj].Order_content);
                        //    GetRecevieData recev_data = new GetRecevieData();
                        //    recev_data.Task_id = id;
                        //    recev_data.Thread_id = thread_id;
                        //    recev_data.Need_return_value = valus;

                        //    //如果没有处理完的相同任务，相同线程查询，那么不往里面加新的查询。
                        //    int num = 0;
                        //    if (set_value_list_g.Count > 0)
                        //    {
                        //        num = set_value_list_g.FindAll(o => o.Task_id.Equals(id) && o.Thread_id.Equals(thread_id) && o.Need_return_value.Equals(valus)).Count;
                        //    }

                        //    if (num == 0)
                        //    {
                        //        set_value_list_g.Add(recev_data);
                        //    }
                        //    byte[] order_content_byte_array = QueryContentTrasforByteOrder(query_task_list[jj].Order_content);
                        //    SendCammand(order_content_byte_array);
               
                        //    //需要查询到的值是不是和目标值是不是一致，如果是，query_sub0_pause_flage = true
                        //}
                    //}
                }
                Thread.Sleep(100);
            }
        }

        /*
        public void SendOrderQuerySub1()
        {
            while (query_sub1_flage)
            {

                if (query_task_list1.Count > 0)
                {
                    for (int jj = 0; jj < query_task_list1.Count; jj++)
                    {

                        //暂停
                        while (query_sub1_pause_flage)
                        {
                            Thread.Sleep(1);
                        }

                        //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                        string id = query_task_list1[jj].Task_id;
                        string status = query_task_list1[jj].Status + "";
                        string thread_id = query_task_list1[jj].Thread_id;
                        if (status.Equals("1") && thread_id.Equals("1"))
                        {
                            //todo
                            //发送指令
                            //如果查询到的值与设定值一致了，那么。。。
                            valus_g = QueryValue(query_task_list1[jj].Order_content);
                            byte[] order_content_byte_array = QueryContentTrasforByteOrder(query_task_list1[jj].Order_content);
                            //SendCammand(order_content_byte_array);
                            //需要查询到的值是不是和目标值是不是一致，如果是，query_sub0_pause_flage = true
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        public void SendOrderQuerySub2()
        {
            while (query_sub2_flage)
            {
                if (query_task_list2.Count > 0)
                {
                    for (int jj = 0; jj < query_task_list2.Count; jj++)
                    {
                        //暂停
                        while (query_sub2_pause_flage)
                        {
                            Thread.Sleep(1);
                        }
                        //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                        string id = query_task_list2[jj].Task_id;
                        string status = query_task_list2[jj].Status + "";
                        string thread_id = query_task_list2[jj].Thread_id;
                        if (status.Equals("1") && thread_id.Equals("2"))
                        {
                            //todo
                            //发送指令
                            //如果查询到的值与设定值一致了，那么。。。
                            valus_g = QueryValue(query_task_list2[jj].Order_content);
                            byte[] order_content_byte_array = QueryContentTrasforByteOrder(query_task_list2[jj].Order_content);
                            //SendCammand(order_content_byte_array);
                            //需要查询到的值是不是和目标值是不是一致，如果是，query_sub0_pause_flage = true
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        */


        public static List<string> task_id_list = new List<string>();

        public static List<int> task_id_number_list = new List<int>();
        /// <summary>
        /// 分发任务到各个线程列表
        /// </summary>
        public void MultTask()
        {
            
        }

        public void AddToOrderList(GetOrderClsSimple order)
        {
            string order_sz = order.Order_content;
            int device_id = -1;
            int hole = -1;
            int speed = -1;
            int offset = -1;
            //int switch_s = -1;
            if (order_sz.Length > 0)
            {
                string[] order_sz_array_switch = order_sz.Split(':');

                if (order_sz_array_switch.Length == 2)
                {
                    //电磁阀
                    int a = int.Parse(order_sz_array_switch[1]);
                    string b = order_sz_array_switch[0];
                    int delay_time = a;
                    if (b.Equals("iDelay"))
                    {
                        //添加延时动作
                        AddRunOrderList(delay_time + "", 1000);
                        device_id = 1000;
                    }
                    else
                    {
                        string fa_open_sz = Convert.ToString(a, 2);
                        fa_open_sz = new string(fa_open_sz.ToCharArray().Reverse<char>().ToArray<char>());
                        //string fa_close_sz = Convert.ToString(c, 2);
                        //fa_close_sz = new string(fa_close_sz.ToCharArray().Reverse<char>().ToArray<char>());
                        if (fa_open_sz.Length == 3)
                        {
                            fa_open_sz = "0" + fa_open_sz;
                        }
                        if (fa_open_sz.Length == 2)
                        {
                            fa_open_sz = "00" + fa_open_sz;
                        }
                        if (fa_open_sz.Length == 1)
                        {
                            fa_open_sz = "000" + fa_open_sz;
                        }
                        char[] switch_on_motor_list = fa_open_sz.ToCharArray();
                        //char[] switch_off_motor_list = fa_close_sz.ToCharArray();
                        //添加开动作
                        for (int ii = 0; ii < 4; ii++)
                        {
                            switch (ii)
                            {
                                case 0:
                                    //AddRunOrderList(switch_on_motor_list[0] + "", 34);
                                    break;
                                case 1:
                                    AddRunOrderList(switch_on_motor_list[1] + "", 33);
                                    device_id = 33;
                                    break;
                                case 2:
                                    AddRunOrderList(switch_on_motor_list[2] + "", 32);
                                    device_id = 32;
                                    break;
                                case 3:
                                    AddRunOrderList(switch_on_motor_list[3] + "", 31);
                                    device_id = 31;
                                    break;
                            }
                        }
                    }

                }
                else
                {
                    string[] order_sz_array = order_sz.Split(',');

                    if (order_sz_array[0].Length > 1)
                    {
                        device_id = int.Parse(order_sz_array[0].Substring(1, order_sz_array[0].Length - 1));
                    }

                    if (order_sz_array[1].Length > 1)
                    {
                        hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                    }

                    if (order_sz_array[2].Length > 1)
                    {
                        speed = int.Parse(order_sz_array[2].Substring(1, order_sz_array[2].Length - 1));
                    }

                    if (order_sz_array[3].Length > 1)
                    {
                        offset = int.Parse(order_sz_array[3].Substring(1, order_sz_array[3].Length - 1));
                    }

                    //if (order_sz_array[4].Length > 1)
                    //{
                    //    switch_s = int.Parse(order_sz_array[4].Substring(1, order_sz_array[4].Length - 1));
                    //}
                }

                //泵
                //if (device_id < 3)
                if (device_id > 2 && device_id < 21)
                {
                    AddRunOrderList(order_sz, device_id);
                }

                //阀
                //if (device_id > 2 && device_id < 5)
                if (device_id < 3)
                {
                    AddRunOrderList(order_sz, device_id);
                }
            }
        }


        /// <summary>
        /// 简单转换
        /// </summary>
        /// <param name="simple_order_list"></param>
        /// <returns></returns>
        public List<GetOrderCls> OrderClsSimpleToOrderCls(List<GetOrderClsSimple> simple_order_list)
        {
            List<GetOrderCls> order_cls_list = new List<GetOrderCls>();
            for (int i = 0; i < simple_order_list.Count; i++)
            {
                string order_sz = simple_order_list[i].Order_content;
                int device_id = -1;
                int hole = -1;
                int speed = -1;
                int offset = -1;
                //int switch_s = -1;
                if (order_sz.Length > 0)
                {
                    string[] order_sz_array_switch = order_sz.Split(':');
                    if (order_sz_array_switch.Length == 2)
                    {
                        //电磁阀
                        int a = int.Parse(order_sz_array_switch[1]);
                        string b = order_sz_array_switch[0];
                        int delay_time = a;
                        if (b.Equals("iDelay"))
                        {
                            //添加延时动作
                            AddRunOrderList(delay_time + "", 1000);
                            device_id = 1000;
                        }
                        else
                        {
                            string fa_open_sz = Convert.ToString(a, 2);
                            fa_open_sz = new string(fa_open_sz.ToCharArray().Reverse<char>().ToArray<char>());
                            //string fa_close_sz = Convert.ToString(c, 2);
                            //fa_close_sz = new string(fa_close_sz.ToCharArray().Reverse<char>().ToArray<char>());
                            if (fa_open_sz.Length == 3)
                            {
                                fa_open_sz = "0" + fa_open_sz;
                            }
                            if (fa_open_sz.Length == 2)
                            {
                                fa_open_sz = "00" + fa_open_sz;
                            }
                            if (fa_open_sz.Length == 1)
                            {
                                fa_open_sz = "000" + fa_open_sz;
                            }
                            char[] switch_on_motor_list = fa_open_sz.ToCharArray();
                            //char[] switch_off_motor_list = fa_close_sz.ToCharArray();
                            //添加开动作
                            for (int ii = 0; ii < 4; ii++)
                            {
                                switch (ii)
                                {
                                    case 0:
                                        //AddRunOrderList(switch_on_motor_list[0] + "", 34);
                                        break;
                                    case 1:
                                        AddRunOrderList(switch_on_motor_list[1] + "", 33);
                                        device_id = 33;
                                        break;
                                    case 2:
                                        AddRunOrderList(switch_on_motor_list[2] + "", 32);
                                        device_id = 32;
                                        break;
                                    case 3:
                                        AddRunOrderList(switch_on_motor_list[3] + "", 31);
                                        device_id = 31;
                                        break;
                                }
                            }
                        }

                    }
                    else
                    {
                        string[] order_sz_array = order_sz.Split(',');

                        if (order_sz_array[0].Length > 1)
                        {
                            device_id = int.Parse(order_sz_array[0].Substring(1, order_sz_array[0].Length - 1));
                        }

                        if (order_sz_array[1].Length > 1)
                        {
                            hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                        }

                        if (order_sz_array[2].Length > 1)
                        {
                            speed = int.Parse(order_sz_array[2].Substring(1, order_sz_array[2].Length - 1));
                        }

                        if (order_sz_array[3].Length > 1)
                        {
                            offset = int.Parse(order_sz_array[3].Substring(1, order_sz_array[3].Length - 1));
                        }

                        //if (order_sz_array[4].Length > 1)
                        //{
                        //    switch_s = int.Parse(order_sz_array[4].Substring(1, order_sz_array[4].Length - 1));
                        //}
                    }

                    int data_transf_type = simple_order_list[i].Data_transf_type;
                    //泵
                    if (device_id > 2 && device_id < 21)
                    {
                        AddRunOrderList(order_sz, device_id);
                    }

                    //阀
                    //if (device_id > 2 && device_id < 5)
                    if (device_id < 3)
                    {
                        AddRunOrderList(order_sz, device_id);
                    }
                    }
            }
            return order_cls_list;
        }

        public static TreeNode tn_parent;
        private void DiGui(TreeNode tn, ref List<GetOrderClsSimple> simlpe_order_list)
        {
            //1.将当前节点显示到lable上
            //label1.Text += "aaa" + "    " + tn.Text + "\r\n";
            string node_text = tn.Text;
            int parents_node_index = tn.Level;
            string[] order_sz_array = node_text.Split(',');
            string loop_flag = order_sz_array[0].Substring(0, 1);
            string loop_number = order_sz_array[0].Substring(1, order_sz_array[0].Length - 1);
            if (tn.Level > 0 && tn.Nodes.Count==0)
            {
                try {
                    GetParentsText(tn, tn.Level);

                    string[] a_array = parent_text.Split(' ');
                    int[] array1 = new int[a_array.Length - 1];
                    for (int i = 0; i < a_array.Length - 1; i++)
                    {
                        array1[i] = int.Parse(a_array[i]);
                    }
                    if (tn_parent != tn.Parent)
                    {
                        tn_parent = tn.Parent;

                        Loop(array1, tn, ref simlpe_order_list);
                    }
                    parent_text = "";
                    //MessageBoxEX.Show("请编辑脚本信息" + parent_text);
                }
                catch (Exception e){
                    MessageBoxEX.Show("请编辑脚本信息" + e.ToString());
                }
            }
            //Console.Write("*******************************" + tn.Text + "---level" + tn.Level + "*******************************\r\n");
            int cx_level = tn.Level;
            int cx_index = tn.Text.IndexOf("串行");
            if (cx_index != -1)
            {
                if (ilevel_first == 0 && cx_index != -1)
                {
                    ilevel_first = cx_level;
                }
                if (ilevel_first == cx_level)
                {
                    Console.Write("*******************************" + tn.Text + "---level" + tn.Level + "*******************************\r\n");
                    //parent_text = "";
                }

                //再次出现串行，循环次数+1;
            }
            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                TreeNode node = tn.Nodes[i];
                DiGui(node, ref simlpe_order_list);
            }
        }



        public static int iloop = 0;
        public static int ilevel = 0;
        public static int ilevel_first = 0;
        public static int list_index = 0;

        private void Loop(int[] array, TreeNode tn,ref List<GetOrderClsSimple> simlpe_order_list)
        {
            Console.Write("-----------------------------iloop:" + iloop + "--text" + tn.Text + "--level" + tn.Level + "--tag" + tn.Tag + "\r\n");
            //int cx_level = tn.Parent.Level;
            //int cx_index = tn.Parent.Text.IndexOf("串行");
            //if (cx_index != -1)
            //{
            //}
            //iloop++;
            for (int i = 0; i < array[0]; i++)
            {
                int array_length = array.Length;
                if (array_length > 1)
                {
                    int[] array_new = array.Skip(1).ToArray();
                    //if (array_new[0] > 1)
                    //{
 
                    //}
                    Loop(array_new, tn, ref simlpe_order_list);
                }
                else
                {
                    //增加一个串行次数
                    string[] order_sz_array = tn.Parent.Text.Split(',');
                    string loop_flag = order_sz_array[0].Substring(0, 1);
                    string loop_number = order_sz_array[0].Substring(1, order_sz_array[0].Length - 1) + " ";
                    //Console.Write("***********i:"+i+"\r\n");
                    for (int jjj = 0; jjj < int.Parse(loop_number); jjj++)
                    {
                        list_index++;
                        for (int jj = 0; jj < tn.Parent.Nodes.Count; jj++)
                        {
                            //Console.Write(jj);

                            TreeNode node = tn.Parent.Nodes[jj];
                            string node_sz = node.Text;
                            if (node_sz.Substring(0, 1).Equals("G"))
                            {
                                string a = node_sz.Substring(0, 1);
                            }
                            string[] node_content_array = node_sz.Split(':');
                            if (node_content_array.Length == 2)
                            {
                                GetOrderClsSimple simple_order = new GetOrderClsSimple();
                                string parent_text = tn.Parent.Parent.Text;

                                int index = parent_text.IndexOf("串行");

                                if (index != -1)
                                {
                                    simple_order.Data_transf_type = 0;
                                }
                                index = parent_text.IndexOf("并行");
                                if (index != -1)
                                {
                                    simple_order.Data_transf_type = 1;
                                }

                                simple_order.Id = i;
                                simple_order.Tag = "" + list_index;

                                simple_order.Order_content = node_sz;
                                simlpe_order_list.Add(simple_order);

                            }

                            if (!node_sz.Substring(0, 1).Equals("G") && node_sz.Substring(0, 1).Equals("D"))
                            {
                                GetOrderClsSimple simple_order = new GetOrderClsSimple();
                                string parent_text = tn.Parent.Text;
                                int index = parent_text.IndexOf("串行");
                                if (index != -1)
                                {
                                    simple_order.Data_transf_type = 0;
                                }
                                index = parent_text.IndexOf("并行");
                                if (index != -1)
                                {
                                    simple_order.Data_transf_type = 1;
                                }
                                simple_order.Id = i;
                                simple_order.Tag = "" + list_index;
                                simple_order.Order_content = node.Text;
                                simlpe_order_list.Add(simple_order);
                                //Console.Write(node.Text);
                            }
                        }
                        ///Console.Write("i:" + i + "\r\n");
                    }



                }
            }
        }

        public static string parent_text = "";
        private string GetParentsText(TreeNode tn,int level)
        {
            string parent_text1 = "";// tn.Parent.Text;
            if (level != 1)
            {
                string[] order_sz_array = tn.Parent.Text.Split(',');
                string loop_flag = order_sz_array[0].Substring(0, 1);
                string loop_number = order_sz_array[0].Substring(1, order_sz_array[0].Length - 1)+" ";

                parent_text += loop_number;//tn.Parent.Text;

                parent_text1 = tn.Parent.Text;
                GetParentsText(tn.Parent, tn.Parent.Level);
            }
            return parent_text1;
        }

        private void GetParentsName(TreeNode Node, ref List<String> NameList)
        {
            NameList.Add(Node.Text);
            if (Node.Parent != null)
            {
                //递归
                GetParentsName(Node.Parent, ref NameList);
            }
        }

        private void 复制内容ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.Focus();
            copy_content_g = treeView1.SelectedNode.Text;//= new_str;
            粘贴内容ToolStripMenuItem.Enabled = true;
        }

        private void 粘贴内容ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!copy_content_g.Equals(""))
            {
                treeView1.SelectedNode.Text = copy_content_g;
                粘贴内容ToolStripMenuItem.Enabled = false;
            }
        }
        public void OpenFileATCG(string file_name)
        {
            treeView1.Nodes.Clear();
            doc.Load(file_name + ".scr"); //我是把xml放到debug里面了.你的路径就随便啦.不过这样方便一些.
            RecursionTreeControl(doc.DocumentElement, treeView1.Nodes);//将加载完成的XML文件显示在TreeView控件中
            treeView1.ExpandAll();//展开TreeView控件中的所有项
            label13.Text = file_name;
        }


        public void OpenFile_Preload_Clean(string file_name,TreeView tv)
        {
            tv.Nodes.Clear();
            doc.Load(file_name + ".scr"); //我是把xml放到debug里面了.你的路径就随便啦.不过这样方便一些.
            RecursionTreeControl(doc.DocumentElement, tv.Nodes);//将加载完成的XML文件显示在TreeView控件中
            tv.ExpandAll();//展开TreeView控件中的所有项
            //label13.Text = file_name;
        }

        private void ucBtnImg19_BtnClick(object sender, EventArgs e)
        {
            //OpenFileATCG("A");
            OpenFileATCG("A");
        }

        private void ucBtnImg20_BtnClick(object sender, EventArgs e)
        {
            OpenFileATCG("T");
        }

        private void ucBtnImg21_BtnClick(object sender, EventArgs e)
        {
            OpenFileATCG("C");
        }

        private void ucBtnImg22_BtnClick(object sender, EventArgs e)
        {
            OpenFileATCG("G");
        }


        public void Pause()
        {


            //int query_order_list_number = query_task_list.Count;

            //if (query_order_list_number > 2)
            //{
            //    query_task_list.Remove(query_task_list[0]);
            //    query_task_list.Remove(query_task_list[1]);
            //}


            //sub0_pause_flage = true;
            //sub1_pause_flage = true;
            //sub2_pause_flage = true;

            is_receive_data_g = false;
            task_scheduling_pause_flage = true;
            query_sub0_pause_flage = true;

            sub01_pause_flage = false;
            sub11_pause_flage = false;
            sub21_pause_flage = false;

            ucBtnImg13.BtnText = "继续";
        }

        public void Continue()
        {
            is_receive_data_g = true;
            task_scheduling_pause_flage = false;
            query_sub0_pause_flage = false;

            sub01_pause_flage = true;
            sub11_pause_flage = true;
            sub21_pause_flage = true;


            //sub0_pause_flage = false;
            //sub1_pause_flage = false;
            //Thread.Sleep(100);
            //sub2_pause_flage = false;
            ucBtnImg13.BtnText = "暂停";
        }

        public void Stop()
        {



            query_task_list = new List<GetOrderClsSimpleQuery>();
            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list_g = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();


            sub01_pause_flage = false;
            sub11_pause_flage = false;
            sub21_pause_flage = false;


            sub0_pause_flage = true;
            sub1_pause_flage = true;
            sub2_pause_flage = true;


            //先停止接收数据，查询进入暂停状态，并且停止接收数据
            is_receive_data_g = false;

            task_scheduling_pause_flage = true;

            query_sub0_pause_flage = true;

            ucBtnImg13.Enabled = false;
            ucBtnImg13.FillColor = Color.Gray;

            ucBtnImg15.Enabled = false;
            ucBtnImg15.FillColor = Color.Gray;

            ucBtnImg14.Enabled = false;
            ucBtnImg14.FillColor = Color.Gray;


            ucBtnImg7.Enabled = false;
            ucBtnImg7.FillColor = Color.Gray;

            ucBtnImg23.Enabled = true;
            //ucBtnImg24.Enabled = true;
            ucBtnImg23.FillColor = Color.Black;
            //ucBtnImg24.FillColor = Color.Black;




        }


        private void ucBtnImg13_BtnClick(object sender, EventArgs e)
        {
            if (ucBtnImg13.BtnText.Equals("暂停"))
            {
                Pause();
            }
            else
            {
                Continue();
            }

            //FreeModbus.WriteCoil_05H(4, 31, FreeModbus.Coil_OFF);
            //serialPort_Send.Write(FreeModbus.Tx_Buf, 0, FreeModbus.TxCount);//串口发送数据
            //byte[] order_array = ContentTrasforByteOrderCls.WriteCoil_05H(4, 31, FreeModbus.Coil_ON);//填充写单个线圈寄存器
            //byte[] order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 15);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 0);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(3000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 0);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 1);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 0);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 3);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 0);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 7);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 0);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 15);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //order_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, 0);//填充写单个线圈寄存器
            //serialPort_Send.Write(order_array, 0, order_array.Length);//串口发送数据
            //Thread.Sleep(1000);

        }

        private void ucBtnImg23_BtnClick(object sender, EventArgs e)
        {
            InitializeAll();
        }

        public int initial_flage_g = 0;

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            e.CancelEdit = true;
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
            //treeView1.SelectedNode.EndEdit(true);
        }

        private void ucBtnImg2_BtnClick_2(object sender, EventArgs e)
        {
            OpenCommuniction();
        }

        //private void ucBtnImg24_BtnClick(object sender, EventArgs e)
        //{
        //    if (!serialPort_Send.IsOpen)
        //    {
        //        //MessageBox.Show("请连接设备");
        //        FrmDialog.ShowDialog(this, "请连接设备", "提示");
        //        return;
        //    }
        //    if (checkBox14.Checked)
        //    {
        //        //string s = "/1ZR" + "\r";

        //        //device_info.add_valve_pump_send_cammand_list("01", s);
        //        //device_info.Ht_send_cammand_list.Add("1", "/1ZR\r");
        //        //byte[] buf = Encoding.ASCII.GetBytes(s);
        //        //SendCammand(buf);
        //        //AddRunList(1, "泵1复位", 0, 0, 0, buf);
        //        //SendCammand(buf);
        //        InitializePump(1);
        //    }

        //    if (checkBox16.Checked)
        //    {
        //        //string s = "/2ZR" + "\r";
        //        ////device_info.add_valve_pump_send_cammand_list("02", s);
        //        ////device_info.Ht_send_cammand_list.Add("2", "/2ZR\r");
        //        //byte[] buf = Encoding.ASCII.GetBytes(s);
        //        //SendCammand(buf);
        //        ////AddRunList(2, "泵2复位", 0, 0, 0, buf);
        //        ////SendCammand(buf);
        //        InitializePump(2);
        //    }
        //    if (checkBox15.Checked)
        //    {
        //        //byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x45, 0x00, 0x00, 0xdd };
        //        //byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
        //        //string str = System.Text.Encoding.UTF8.GetString(package_content_crc1);
        //        ////device_info.add_valve_send_cammand_list("03", str);
        //        //AddRunList(3, "阀1复位", 0, 0, 0, package_content_crc1);
        //        ////SendCammand(package_content_crc1);
        //        InitializeValve(3);
        //    }
        //    Thread.Sleep(10);
        //    if (checkBox13.Checked)
        //    {
        //        //byte[] package_content = new byte[] { 0xcc, 0x04, 0x45, 0x00, 0x00, 0xdd };
        //        //byte[] package_content_crc = CRC.GetNewCrcArray(package_content);
        //        //string str = System.Text.Encoding.UTF8.GetString(package_content_crc);
        //        //device_info.add_valve_send_cammand_list("04", str);
        //        //AddRunList(4, "阀2复位", 0, 0, 0, package_content_crc);
        //        //SendCammand(package_content_crc);
        //        InitializeValve(4);
        //    }
        //}

        public bool ChangeNodeId(ref GetOrderClsSimple seleted_node, ref GetOrderClsSimple new_node)
        {
            string selected_node_sz = seleted_node.Tag;
            seleted_node.Tag = new_node.Tag;
            new_node.Tag = selected_node_sz;
            return true;
 
        }
        /// <summary>
        /// 上移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbMoveUp_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("请选择需要移动的的节点！");
                this.Focus();
                return;
            }
            GetOrderClsSimple model = new GetOrderClsSimple();
            TreeNode node = treeView1.SelectedNode;     //获得选中节点的值
            model.Tag = node.Tag.ToString();
            model.Order_content = node.Name.ToString();
            if (node.PrevNode == null)      //选中节点的上一个节点为Null则返回
            {
                return;
            }
            GetOrderClsSimple upModel = new GetOrderClsSimple();
            TreeNode upNode = node.PrevNode;            //获得选中节点的上一个节点
            upModel.Tag = upNode.Tag.ToString();
            upModel.Order_content = upNode.Name;
            bool flag = ChangeNodeId(ref model, ref upModel);       //将两个节点的排序编号互换
            if (flag)       //如果执行成功
            {
                string theOrder = node.Name;
                node.Name = upNode.Name;
                upNode.Name = theOrder;
                TreeNode newNode = (TreeNode)node.Clone();      //克隆选中的节点
                if (node.Parent == null)
                {
                    treeView1.Nodes.Insert(upNode.Index, newNode);      //在选中节点的上一个节点的地方插入本节点
                }
                else
                {
                    node.Parent.Nodes.Insert(upNode.Index, newNode);
                }
                node.Remove();
                treeView1.SelectedNode = newNode;
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// 下移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbMoveDown_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("请选择需要移动的的节点！");
                this.Focus();
                return;
            }
            GetOrderClsSimple model = new GetOrderClsSimple();
            TreeNode node = treeView1.SelectedNode;     //选中的节点
            model.Tag = node.Tag.ToString();
            model.Order_content = node.Name.ToString();
            if (node.NextNode == null)      //下一个节点为Null时返回
            {
                return;
            }
            GetOrderClsSimple downModel = new GetOrderClsSimple();
            TreeNode dowNode = node.NextNode;
            downModel.Tag = dowNode.Tag.ToString();
            downModel.Order_content = dowNode.Name;
            bool flag = ChangeNodeId(ref model, ref downModel);       //将两个节点的排序编号互换
            if (flag)
            {
                string theOrder = node.Name;
                node.Name = dowNode.Name;
                dowNode.Name = theOrder;
                TreeNode newNode = (TreeNode)dowNode.Clone();       //克隆下一个节点及其所有的子节点
                if (node.Parent == null)
                {
                    treeView1.Nodes.Insert(node.Index, newNode);    //在原节点的位置插入下一个节点的值
                }
                else
                {
                    node.Parent.Nodes.Insert(node.Index, newNode);
                }
                dowNode.Remove();           //删除掉选中节点的下一个节点
                treeView1.SelectedNode = node;  //重新选中
            }
            else
            {
                return;
            }
        }

        private void PartRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            string a_t_c_g_list = textBox1.Text;
            if (!a_t_c_g_list.Equals(""))
            {
                treeView1.Nodes.Clear();
                LoopOpenFile();
                Thread.Sleep(50);
            }

            if (treeView1.Nodes.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }

            iloop = 0;
            list_index = 0;
            List<GetOrderClsSimple> node_name_list = new List<GetOrderClsSimple>();
            parent_text = "";

            if (treeView1.Nodes.Count > 1)
            {
                return;
            }

            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list_g = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();
            List<GetOrderClsSimple> task_listxxx = new List<GetOrderClsSimple>();

            TreeNode node_selected = treeView1.SelectedNode;
            DiGuiSingleTask(node_selected, ref node_name_list);

            var groupList = node_name_list.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
            //分组循环
            for (int i = 0; i < groupList.Count; i++)
            {
                Int32 sam_count = groupList[i].samcount;
                string task_id = groupList[i].Keys.Task_id;
                List<GetOrderClsSimple> node_name_list_by_task_id = new List<GetOrderClsSimple>();
                node_name_list_by_task_id = node_name_list.FindAll(o => o.Task_id.Equals(task_id));
                //再找出线程ID相同的
                var groupList_by_thread_id = node_name_list_by_task_id.GroupBy(x => new { x.Thread_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
                for (int jj = 0; jj < groupList_by_thread_id.Count; jj++)
                {
                    string thread_id = groupList_by_thread_id[jj].Keys.Thread_id;
                    List<GetOrderClsSimple> node_name_by_loop_number_list = new List<GetOrderClsSimple>();
                    node_name_by_loop_number_list = node_name_list_by_task_id.FindAll(o => o.Thread_id.Equals(thread_id));
                    if (node_name_by_loop_number_list.Count > 0)
                    {
                        int aaa = node_name_by_loop_number_list[0].ILoop;
                        int bbb = node_name_by_loop_number_list[0].ILoopNext;
                        for (int ILoop = 0; ILoop < aaa; ILoop++)
                        {
                            for (int ILoopNext = 0; ILoopNext < bbb; ILoopNext++)
                            {
                                for (int aii = 0; aii < node_name_by_loop_number_list.Count; aii++)
                                {
                                    string[] tmp_array = node_name_by_loop_number_list[aii].Order_content.Split(',');
                                    //如果是阀和泵一起的指令那么拆开来，否则还按原来的方式
                                    if (tmp_array.Length == 4 && tmp_array[3].Length > 1)
                                    {
                                        //one order to two order,one valve,two pump
                                        string aii_content = node_name_by_loop_number_list[aii].Order_content;
                                        //valve
                                        GetOrderClsSimple clss = new GetOrderClsSimple();
                                        clss.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss.Order_content = aii_content;//tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss);
                                    }
                                    else
                                    {
                                        string aii_content1 = node_name_by_loop_number_list[aii].Order_content;
                                        GetOrderClsSimple clss4 = new GetOrderClsSimple();
                                        clss4.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss4.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss4.Order_content = aii_content1;// tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss4.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss4);
                                    }
                                }
                            }
                        }
                    }
                }
                //Console.Write("---------------------------------------------------------------------------" + "\r\n");
            }


            //MultTask();
            for (int jja = 0; jja < task_listxxx.Count; jja++)
            {
                GetOrderClsSimple clss = new GetOrderClsSimple();
                clss.Id = jja;
                clss.ILoop = task_listxxx[jja].ILoop;
                clss.ILoopNext = task_listxxx[jja].ILoopNext;
                clss.Order_content = task_listxxx[jja].Order_content;
                clss.Status = task_listxxx[jja].Status;
                clss.Task_id = task_listxxx[jja].Task_id;
                clss.Thread_id = task_listxxx[jja].Thread_id;
                task_list_new_g.Add(clss);
            }

            task_list_g = new List<GetOrderClsSimple>();
            task_list_g = task_list_new_g;
            var groupList1 = task_list_g.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();

            //重新生成编号
            for (int i = 0; i < groupList1.Count; i++)
            {
                string task_id = groupList1[i].Keys.Task_id;
                int number = groupList1[i].samcount;
                task_id_list.Add(task_id);
                task_id_number_list.Add(number);
            }

            //var model = task_list.Where(c => c.Task_id.Equals("1") && c.Status.Equals("0"));//.FirstOrDefault();
            //var model = task_list.Where(c => c.Status == 2 && c.Task_id.Equals("0") && c.Thread_id.Equals(0)).FirstOrDefault();
            //model.Status = 3;

            start_time_g = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            label19.Text = "运行时间：" + "00:00:00";


            ucBtnImg13.Enabled = true;
            ucBtnImg15.Enabled = true;
            ucBtnImg13.FillColor = Color.Black;
            ucBtnImg15.FillColor = Color.Black;


            ucBtnImg23.Enabled = false;
            //ucBtnImg24.Enabled = false;
            ucBtnImg23.FillColor = Color.Gray;
            //ucBtnImg24.FillColor = Color.Gray;

            query_sub0_pause_flage = false;

            sub01_pause_flage = true;
            sub11_pause_flage = true;
            sub21_pause_flage = true;

            buffer = new List<byte>(4096);

            sub0_pause_flage = false;
            sub1_pause_flage = true;
            sub2_pause_flage = true;

            task_scheduling_pause_flage = false;

            is_receive_data_g = true;

            show_run_finish_flage_g = true;

            number_current_thread0_status1 = 0;

            ucBtnImg14.Enabled = false;
            ucBtnImg14.FillColor = Color.Gray;

            ucBtnImg7.Enabled = false;
            ucBtnImg7.FillColor = Color.Gray;

            not_equal_error_number = 0;

            //启动时，线程号2，暂时停状态，只有线程1启动了，
            //task_scheduling_stop_flage = true;
            //sub0_pause_flage = false;
            //sub1_pause_flage = false;
            //sub2_pause_flage = false;
            //thread_order_flage_continue_send = true;



            /*
            //if (initial_flage_g == 0)
            //{
            //    FrmDialog.ShowDialog(this, "软件第一次打开，请先初始化设备", "提示");
            //    return;
            //}

            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }


            string a_t_c_g_list = textBox1.Text;
            if (!a_t_c_g_list.Equals(""))
            {
                treeView1.Nodes.Clear();
                LoopOpenFile();
                Thread.Sleep(50);
            }

            if (treeView1.Nodes.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }

            iloop = 0;
            list_index = 0;
            List<GetOrderClsSimple> node_name_list = new List<GetOrderClsSimple>();
            parent_text = "";

            if (treeView1.Nodes.Count > 1)
            {
                return;
            }

            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();

            TreeNode node_selected = treeView1.SelectedNode;
            DiGuiSingleTask(node_selected, ref node_name_list);

            var groupList = node_name_list.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
            //分组循环
            for (int i = 0; i < groupList.Count; i++)
            {
                Int32 sam_count = groupList[i].samcount;
                string task_id = groupList[i].Keys.Task_id;
                List<GetOrderClsSimple> node_name_list_by_task_id = new List<GetOrderClsSimple>();
                node_name_list_by_task_id = node_name_list.FindAll(o => o.Task_id.Equals(task_id));
                //再找出线程ID相同的
                var groupList_by_thread_id = node_name_list_by_task_id.GroupBy(x => new { x.Thread_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
                for (int jj = 0; jj < groupList_by_thread_id.Count; jj++)
                {
                    string thread_id = groupList_by_thread_id[jj].Keys.Thread_id;
                    List<GetOrderClsSimple> node_name_by_loop_number_list = new List<GetOrderClsSimple>();
                    node_name_by_loop_number_list = node_name_list_by_task_id.FindAll(o => o.Thread_id.Equals(thread_id));
                    if (node_name_by_loop_number_list.Count > 0)
                    {
                        int aaa = node_name_by_loop_number_list[0].ILoop;
                        int bbb = node_name_by_loop_number_list[0].ILoopNext;
                        for (int ILoop = 0; ILoop < aaa; ILoop++)
                        {
                            for (int ILoopNext = 0; ILoopNext < bbb; ILoopNext++)
                            {
                                for (int aii = 0; aii < node_name_by_loop_number_list.Count; aii++)
                                {
                                    //Console.Write(node_name_by_loop_number_list[aii].Order_content + "--" +node_name_by_loop_number_list[aii].Task_id + "\r\n");
                                    string[] tmp_array = node_name_by_loop_number_list[aii].Order_content.Split(',');

                                    //如果是阀和泵一起的指令那么拆开来，否则还按原来的方式
                                    if (tmp_array.Length == 4 && tmp_array[3].Length > 1)
                                    {
                                        //one order to two order,one valve,two pump
                                        string aii_content = node_name_by_loop_number_list[aii].Order_content;
                                        
                                        //valve
                                        GetOrderClsSimple clss = new GetOrderClsSimple();
                                        //node_name_by_loop_number_list[aii].Order_content = tmp_array[0] + "," + tmp_array[1] + "" + "V,A";
                                        clss = node_name_by_loop_number_list[aii];
                                        clss.Order_content = tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss.Id = task_list.Count + 1;
                                        task_list.Add(clss);

                                        //pump
                                        GetOrderClsSimple clss1 = new GetOrderClsSimple();
                                        //clss = node_name_by_loop_number_list[aii];
                                        clss1.Order_content = tmp_array[0] + ",O-1," + tmp_array[2] + "," + tmp_array[3];
                                        clss1.Id = task_list.Count + 1;
                                        clss1.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss1.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        task_list.Add(clss1);


                                    }
                                    else
                                    {
                                        GetOrderClsSimple clss = new GetOrderClsSimple();
                                        clss = node_name_by_loop_number_list[aii];
                                        clss.Id = task_list.Count + 1;
                                        task_list.Add(clss);
                                    }


                                }
                            }
                        }
                    }
                }
                //Console.Write("---------------------------------------------------------------------------" + "\r\n");
            }

            //MultTask();
            for (int jja = 0; jja < task_list.Count; jja++)
            {
                GetOrderClsSimple clss = new GetOrderClsSimple();
                clss.Id = jja;
                clss.ILoop = task_list[jja].ILoop;
                clss.ILoopNext = task_list[jja].ILoopNext;
                clss.Order_content = task_list[jja].Order_content;
                clss.Status = task_list[jja].Status;
                clss.Task_id = task_list[jja].Task_id;
                clss.Thread_id = task_list[jja].Thread_id;
                task_list_new_g.Add(clss);
            }

            task_list = new List<GetOrderClsSimple>();
            task_list = task_list_new_g;

            var groupList1 = task_list.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();

            //重新生成编号

            for (int i = 0; i < groupList1.Count; i++)
            {
                string task_id = groupList1[i].Keys.Task_id;
                int number = groupList1[i].samcount;
                task_id_list.Add(task_id);
                task_id_number_list.Add(number);
            }

            //var model = task_list.Where(c => c.Task_id.Equals("1") && c.Status.Equals("0"));//.FirstOrDefault();
            //var model = task_list.Where(c => c.Status == 2 && c.Task_id.Equals("0") && c.Thread_id.Equals(0)).FirstOrDefault();
            //model.Status = 3;

            //启动时，线程号2，暂时停状态，只有线程1启动了，
            sub0_pause_flage = false;
            //thread_order_flage_continue_send = true;

            */
        }

        /// <summary>
        /// 动作转换为指令
        /// </summary>
        /// <param name="tv"></param>
        public void ActionConversionOrder(TreeView tv)
        {
            iloop = 0;
            list_index = 0;
            List<GetOrderClsSimple> node_name_list = new List<GetOrderClsSimple>();
            parent_text = "";

            if (tv.Nodes.Count > 1)
            {
                return;
            }

            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list_g = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();
            List<GetOrderClsSimple> task_listxxx = new List<GetOrderClsSimple>();

            tv.SelectedNode = tv.Nodes[0];

            TreeNode node_selected = tv.SelectedNode;
            DiGuiSingleTask(node_selected, ref node_name_list);

            var groupList = node_name_list.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
            //分组循环
            for (int i = 0; i < groupList.Count; i++)
            {
                Int32 sam_count = groupList[i].samcount;
                string task_id = groupList[i].Keys.Task_id;
                List<GetOrderClsSimple> node_name_list_by_task_id = new List<GetOrderClsSimple>();
                node_name_list_by_task_id = node_name_list.FindAll(o => o.Task_id.Equals(task_id));
                //再找出线程ID相同的
                var groupList_by_thread_id = node_name_list_by_task_id.GroupBy(x => new { x.Thread_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
                for (int jj = 0; jj < groupList_by_thread_id.Count; jj++)
                {
                    string thread_id = groupList_by_thread_id[jj].Keys.Thread_id;
                    List<GetOrderClsSimple> node_name_by_loop_number_list = new List<GetOrderClsSimple>();
                    node_name_by_loop_number_list = node_name_list_by_task_id.FindAll(o => o.Thread_id.Equals(thread_id));
                    if (node_name_by_loop_number_list.Count > 0)
                    {
                        int aaa = node_name_by_loop_number_list[0].ILoop;
                        int bbb = node_name_by_loop_number_list[0].ILoopNext;
                        for (int ILoop = 0; ILoop < aaa; ILoop++)
                        {
                            for (int ILoopNext = 0; ILoopNext < bbb; ILoopNext++)
                            {
                                for (int aii = 0; aii < node_name_by_loop_number_list.Count; aii++)
                                {
                                    string[] tmp_array = node_name_by_loop_number_list[aii].Order_content.Split(',');
                                    //如果是阀和泵一起的指令那么拆开来，否则还按原来的方式
                                    if (tmp_array.Length == 4 && tmp_array[3].Length > 1)
                                    {
                                        //one order to two order,one valve,two pump
                                        string aii_content = node_name_by_loop_number_list[aii].Order_content;
                                        //valve
                                        GetOrderClsSimple clss = new GetOrderClsSimple();
                                        clss.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss.Order_content = aii_content;//tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss);
                                    }
                                    else
                                    {
                                        string aii_content1 = node_name_by_loop_number_list[aii].Order_content;
                                        GetOrderClsSimple clss4 = new GetOrderClsSimple();
                                        clss4.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss4.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss4.Order_content = aii_content1;// tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss4.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss4);
                                    }
                                }
                            }
                        }
                    }
                }
                //Console.Write("---------------------------------------------------------------------------" + "\r\n");
            }
            //MultTask();
            for (int jja = 0; jja < task_listxxx.Count; jja++)
            {
                GetOrderClsSimple clss = new GetOrderClsSimple();
                clss.Id = jja;
                clss.ILoop = task_listxxx[jja].ILoop;
                clss.ILoopNext = task_listxxx[jja].ILoopNext;
                clss.Order_content = task_listxxx[jja].Order_content;
                clss.Status = task_listxxx[jja].Status;
                clss.Task_id = task_listxxx[jja].Task_id;
                clss.Thread_id = task_listxxx[jja].Thread_id;
                task_list_new_g.Add(clss);
            }

            task_list_g = new List<GetOrderClsSimple>();
            task_list_g = task_list_new_g;
            var groupList1 = task_list_g.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();

            //重新生成编号
            for (int i = 0; i < groupList1.Count; i++)
            {
                string task_id = groupList1[i].Keys.Task_id;
                int number = groupList1[i].samcount;
                task_id_list.Add(task_id);
                task_id_number_list.Add(number);
            }
        }

        private void ucBtnImg27_BtnClick(object sender, EventArgs e)
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

        private void ucBtnImg28_BtnClick(object sender, EventArgs e)
        {
            refresh_gridview_flage_g = true;
        }

        private void ucBtnImg15_BtnClick(object sender, EventArgs e)
        {
            Stop();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label17.Text = "序列数量：" + textBox1.Text.Length;
        }

        private void ucBtnImg4_BtnClick(object sender, EventArgs e)
        {

        }

        private void ucBtnImg1_BtnClick(object sender, EventArgs e)
        {

        }
        public static int send_delay_time_g = 120;



        private void ucBtnImg1_BtnClick_1(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBoxPump1.Checked)
            {
                ReSetPump(0x03);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump2.Checked)
            {
                ReSetPump(0x04);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump3.Checked)
            {
                ReSetPump(0x05);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump4.Checked)
            {
                ReSetPump(0x06);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump5.Checked)
            {
                ReSetPump(0x07);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump6.Checked)
            {
                ReSetPump(0x08);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump7.Checked)
            {
                ReSetPump(0x9);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump8.Checked)
            {
                ReSetPump(16);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump9.Checked)
            {
                ReSetPump(17);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump10.Checked)
            {
                ReSetPump(18);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump11.Checked)
            {
                ReSetPump(19);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump12.Checked)
            {
                ReSetPump(20);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBox_Valve1Reset.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x01, 0x45, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1,"");
            }

            Thread.Sleep(100);

            if (checkBox_Valve2Reset.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x02, 0x45, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1, "");
            }

        }


        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="device_id"></param>
        public void ReSetPump(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x45, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        /// <summary>
        /// 注液
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="device_id"></param>
        /// <param name="order_id"></param>
        public void LiquidOut_Or_In(int offset, int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x4E, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] tmp_array = ConvertHex2(offset);
            package_content1[3] = tmp_array[0];
            package_content1[4] = tmp_array[1];
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        public void SetPumpSpeed(int speed, int device_id)
        {
            //byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x4E, 0x00, 0x00, 0xdd };
            byte[] package_content1 = new byte[] { 0xCC, 0x13, 0x07, 0xFF, 0xEE, 0xBB, 0xAA, 0x64, 0x00, 0x00, 0x00, 0xDD};
            package_content1[1] = (byte)device_id;
            byte[] tmp_array = ConvertHex2(speed);
            package_content1[7] = tmp_array[0];
            package_content1[8] = tmp_array[1];
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="device_id"></param>
        public void LiquidIn(int offset, int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x4d, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        private byte[] ConvertHex2(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[2];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            //hex[2] = (byte)((velocity >> 16) & 0xff);
            //hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }

        private StringBuilder ConvertHex1(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[4];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);
            hex[2] = (byte)((velocity >> 16) & 0xff);
            hex[3] = (byte)((velocity >> 24) & 0xff);
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < hex.Length - 1; i++)
            {
                tmp.Append(hex[i].ToString("x2"));  //转为16进制，当只有一个字符时补0
            }
            return tmp;
        }

        private byte[] ConvertHex4(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[4];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            hex[2] = (byte)((velocity >> 16) & 0xff);
            hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }

        private void ucBtnImg30_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            ReSetPump(0x03);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x04);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x05);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x06);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x07);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x08);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x9);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(16);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(17);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(18);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(19);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(20);
            Thread.Sleep(send_delay_time_g);

            byte[] package_content1 = new byte[] { 0xcc, 0x01, 0x45, 0x00, 0x00, 0xdd };
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1,"");
            Thread.Sleep(send_delay_time_g);

            byte[] package_content2 = new byte[] { 0xcc, 0x02, 0x45, 0x00, 0x00, 0xdd };
            byte[] package_content_crc2 = CRC.GetNewCrcArray(package_content2);
            SendCammand(package_content_crc2,"");
        }

        private void ucBtnImg4_BtnClick_1(object sender, EventArgs e)
        {
            //if (!serialPort_Send.IsOpen)
            //{
            //    //MessageBox.Show("请连接设备");
            //    FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
            //    return;
            //}

            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBoxPump1.Checked)
            {
                //ReSetPump(0x03);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump2.Checked)
            {
                //ReSetPump(0x4);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x04);
            }

            if (checkBoxPump3.Checked)
            {
                //ReSetPump(0x05);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x05);
            }

            if (checkBoxPump4.Checked)
            {
                //ReSetPump(0x6);
                Thread.Sleep(120);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x06);
            }
            if (checkBoxPump5.Checked)
            {
                //ReSetPump(0x07);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x07);
            }

            if (checkBoxPump6.Checked)
            {
                //ReSetPump(0x08);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x08);
            }
            if (checkBoxPump7.Checked)
            {
                //ReSetPump(0x09);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x09);
            }

            if (checkBoxPump8.Checked)
            {
                //ReSetPump(0x10);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 16);
            }

            if (checkBoxPump9.Checked)
            {
                //ReSetPump(0x11);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 17);
            }

            if (checkBoxPump10.Checked)
            {
                //ReSetPump(0x12);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 18);
            }

            if (checkBoxPump11.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 19);
            }

            if (checkBoxPump12.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 20);
            }

        }

        private void ucBtnImg31_BtnClick(object sender, EventArgs e)
        {
            //if (!serialPort_Send.IsOpen)
            //{
            //    //MessageBox.Show("请连接设备");
            //    FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
            //    return;
            //}

            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }


            Thread.Sleep(send_delay_time_g);
            string offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x03);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x04);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x05);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x06);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x07);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x08);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x09);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 16);


            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 17);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 18);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 19);


            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 20);
        }

        private void ucBtnImg29_BtnClick(object sender, EventArgs e)
        {
            if (checkBoxValve1.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x01, 0x3E, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1,"");
            }

            Thread.Sleep(100);
            if (checkBoxValve2.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x02, 0x3E, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1,"");
            }
        }

        private void ucBtnImg6_BtnClick_1(object sender, EventArgs e)
        {
            if (checkBoxValve1.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x01, 0xA4, 0x00, 0x00, 0xdd };
                int target_hole = int.Parse(comboBox_Hole.SelectedItem.ToString());
                int up_target_hole = target_hole - 1;
                if (target_hole == 0)
                {
                    up_target_hole = 1;
                }
                package_content1[3] = (byte)target_hole;
                package_content1[4] = (byte)up_target_hole;
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1,"");
            }

            Thread.Sleep(100);

            if (checkBoxValve2.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x02, 0xA4, 0x00, 0x00, 0xdd };
                int target_hole = int.Parse(comboBox_Hole1.SelectedItem.ToString());
                int up_target_hole = target_hole - 1;
                if (target_hole == 0)
                {
                    up_target_hole = 1;
                }
                package_content1[3] = (byte)target_hole;
                package_content1[4] = (byte)up_target_hole;
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1,"");
            }
        }

        private void ucBtnImg3_BtnClick(object sender, EventArgs e)
        {
            //todo
            if (textBox3.Text.Equals(""))
            {
                return;
            }
            //string switch_sz = "Switch:" + textBox3.Text;
            int a = int.Parse(textBox3.Text);
            byte[] a_array = ConvertHex4(a);
            byte[] order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH_New1(4, 31, 4, a_array);
            SendCammand(order_content_byte_array, "Switch:" + a);

            //byte[] order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, a);
            //SendCammand(order_content_byte_array);


            //byte[] order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, a);
            //SendCammand(order_content_byte_array);


            //            使用示例
            //打开全部电磁阀
            //            Byte[] data = new Byte[4];
            //            //电磁阀全部打开
            //            data[0] = 0xff;
            //            data[1] = 0xff;
            //            data[2] = 0xff;
            //            data[3] = 0xff;
            //            freeModebus.WriteCoil_0FH(4, 31, 4,data);//填充写多个线圈寄存器
            //            serial485Port.Write(freeModebus.Tx_Buf, 0, freeModebus.TxCount);
            //关闭全部电磁阀：
            //            Byte[] data = new Byte[4];
            //            //电磁阀全部关闭
            //            data[0] = 0;
            //            data[1] = 0;
            //            data[2] = 0;
            //            data[3] = 0;
            //            freeModebus.WriteCoil_0FH(4, 31, 4, data);//填充写多个线圈寄存器
            //            serial485Port.Write(freeModebus.Tx_Buf, 0, freeModebus.TxCount);
        }

        /// <summary>
        /// 查询阀当前孔位置
        /// </summary>
        /// <param name="receive_data"></param>
        public void QueryValveHole(byte[] receive_data)
        {
            //
            //CC 02 00 05 00 DD B0 01 
            //CC 02 00 01 08 DD B4 01 
            if (receive_data.Length == 8)
            {
                //孔1 复位后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x01 && receive_data[3] == 0x01 && receive_data[4] == 0x08 && receive_data[5] == 0xDD)
                {
                    FormMain.mainFrm.Invoke((EventHandler)(delegate
                    {
                        comboBox_Hole.SelectedItem = 8 + "";
                    }));
                }

                //孔1 运动后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x01 && receive_data[4] == 0x00 && receive_data[5] == 0xDD)
                {
                    FormMain.mainFrm.Invoke((EventHandler)(delegate
                    {
                        string hole = "" + receive_data[3];
                        comboBox_Hole.SelectedItem = hole;
                    }));
                }

                //孔2 复位后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x02 && receive_data[3] == 0x01 && receive_data[4] == 0x08 && receive_data[5] == 0xDD)
                {
                    FormMain.mainFrm.Invoke((EventHandler)(delegate
                    {
                        comboBox_Hole1.SelectedItem = 8 + "";
                    }));
                }


                //孔2 运动后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x02 && receive_data[4] == 0x00 && receive_data[5] == 0xDD)
                {
                    FormMain.mainFrm.Invoke((EventHandler)(delegate
                    {
                        string hole = "" + receive_data[3];
                        comboBox_Hole1.SelectedItem = hole;
                    }));
                }
            }
        }

        private void ucBtnImg7_BtnClick_1(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }

            treeView1.SelectedNode = treeView1.Nodes[0];
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            string a_t_c_g_list = textBox1.Text;
            if (!a_t_c_g_list.Equals(""))
            {
                treeView1.Nodes.Clear();
                LoopOpenFile();
                Thread.Sleep(50);
            }

            if (treeView1.Nodes.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }

            OpenFile_Preload_Clean("Preload", treeView_preload);
            ActionConversionOrder(treeView_preload);

            iloop = 0;
            list_index = 0;
            List<GetOrderClsSimple> node_name_list = new List<GetOrderClsSimple>();
            parent_text = "";

            if (treeView1.Nodes.Count > 1)
            {
                return;
            }

            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list_g = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();
            List<GetOrderClsSimple> task_listxxx = new List<GetOrderClsSimple>();
            //TreeNode node_selected = treeView1.SelectedNode;
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                TreeNode node = treeView1.Nodes[i];
                DiGui1(node, ref node_name_list);
            }

            var groupList = node_name_list.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
            //分组循环
            for (int i = 0; i < groupList.Count; i++)
            {
                Int32 sam_count = groupList[i].samcount;
                string task_id = groupList[i].Keys.Task_id;
                List<GetOrderClsSimple> node_name_list_by_task_id = new List<GetOrderClsSimple>();
                node_name_list_by_task_id = node_name_list.FindAll(o => o.Task_id.Equals(task_id));
                //再找出线程ID相同的
                var groupList_by_thread_id = node_name_list_by_task_id.GroupBy(x => new { x.Thread_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
                for (int jj = 0; jj < groupList_by_thread_id.Count; jj++)
                {
                    string thread_id = groupList_by_thread_id[jj].Keys.Thread_id;
                    List<GetOrderClsSimple> node_name_by_loop_number_list = new List<GetOrderClsSimple>();
                    node_name_by_loop_number_list = node_name_list_by_task_id.FindAll(o => o.Thread_id.Equals(thread_id));
                    if (node_name_by_loop_number_list.Count > 0)
                    {
                        int aaa = node_name_by_loop_number_list[0].ILoop;
                        int bbb = node_name_by_loop_number_list[0].ILoopNext;
                        for (int ILoop = 0; ILoop < aaa; ILoop++)
                        {
                            for (int ILoopNext = 0; ILoopNext < bbb; ILoopNext++)
                            {
                                for (int aii = 0; aii < node_name_by_loop_number_list.Count; aii++)
                                {
                                    //Console.Write(node_name_by_loop_number_list[aii].Order_content + "--" +node_name_by_loop_number_list[aii].Task_id + "\r\n");
                                    //GetOrderClsSimple clss = new GetOrderClsSimple();
                                    //clss = node_name_by_loop_number_list[aii];
                                    //clss.Id = task_list.Count + 1;
                                    //task_list.Add(clss);
                                    string[] tmp_array = node_name_by_loop_number_list[aii].Order_content.Split(',');
                                    //如果是阀和泵一起的指令那么拆开来，否则还按原来的方式
                                    if (tmp_array.Length == 4 && tmp_array[3].Length > 1)
                                    {
                                        //one order to two order,one valve,two pump
                                        string aii_content = node_name_by_loop_number_list[aii].Order_content;
                                        //valve
                                        GetOrderClsSimple clss = new GetOrderClsSimple();
                                        //node_name_by_loop_number_list[aii].Order_content = tmp_array[0] + "," + tmp_array[1] + "" + "V,A";
                                        //clss = node_name_by_loop_number_list[aii];
                                        clss.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss.Order_content = node_name_by_loop_number_list[aii].Order_content;//tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss);
                                        //pump
                                        //GetOrderClsSimple clss1 = new GetOrderClsSimple();
                                        ////clss = node_name_by_loop_number_list[aii];
                                        //clss1.Order_content = tmp_array[0] + ",O-1," + tmp_array[2] + "," + tmp_array[3];
                                        //clss1.Id = task_list.Count + 1;
                                        //clss1.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        //clss1.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        //task_listxxx.Add(clss1);

                                    }
                                    else
                                    {
                                        //GetOrderClsSimple clss = new GetOrderClsSimple();
                                        //clss = node_name_by_loop_number_list[aii];
                                        //clss.Id = task_list.Count + 1;
                                        //task_listxxx.Add(clss);
                                        string aii_content1 = node_name_by_loop_number_list[aii].Order_content;
                                        GetOrderClsSimple clss4 = new GetOrderClsSimple();
                                        //node_name_by_loop_number_list[aii].Order_content = tmp_array[0] + "," + tmp_array[1] + "" + "V,A";
                                        //clss4 = node_name_by_loop_number_list[aii];
                                        clss4.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss4.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss4.Order_content = aii_content1;// tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss4.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss4);
                                    }
                                }
                            }
                        }
                    }
                }
                //Console.Write("---------------------------------------------------------------------------" + "\r\n");
            }

            //MultTask();
            for (int jja = 0; jja < task_listxxx.Count; jja++)
            {
                GetOrderClsSimple clss = new GetOrderClsSimple();
                clss.Id = jja;
                clss.ILoop = task_listxxx[jja].ILoop;
                clss.ILoopNext = task_listxxx[jja].ILoopNext;
                clss.Order_content = task_listxxx[jja].Order_content;
                clss.Status = task_listxxx[jja].Status;
                clss.Task_id = task_listxxx[jja].Task_id;
                clss.Thread_id = task_listxxx[jja].Thread_id;
                task_list_new_g.Add(clss);
            }

            task_list_g = new List<GetOrderClsSimple>();
            task_list_g = task_list_new_g;
            var groupList1 = task_list_g.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();

            //重新生成编号

            for (int i = 0; i < groupList1.Count; i++)
            {
                string task_id = groupList1[i].Keys.Task_id;
                int number = groupList1[i].samcount;
                task_id_list.Add(task_id);
                task_id_number_list.Add(number);
            }

            OpenFile_Preload_Clean("Clean", treeView_clean);
            ActionConversionOrder(treeView_clean);

            start_time_g = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            label19.Text = "运行时间：" + "00:00:00";
            ucBtnImg13.Enabled = true;
            ucBtnImg15.Enabled = true;
            ucBtnImg13.FillColor = Color.Black;
            ucBtnImg15.FillColor = Color.Black;

            ucBtnImg23.Enabled = false;
            //ucBtnImg24.Enabled = false;
            ucBtnImg23.FillColor = Color.Gray;
            //ucBtnImg24.FillColor = Color.Gray;

            query_sub0_pause_flage = false;

            sub01_pause_flage = true;
            sub11_pause_flage = true;
            sub21_pause_flage = true;

            buffer = new List<byte>(4096);

            sub0_pause_flage = false;
            sub1_pause_flage = true;
            sub2_pause_flage = true;

            task_scheduling_pause_flage = false;

            is_receive_data_g = true;

            show_run_finish_flage_g = true;

            number_current_thread0_status1 = 0;

            ucBtnImg14.Enabled = false;
            ucBtnImg14.FillColor = Color.Gray;

            ucBtnImg7.Enabled = false;
            ucBtnImg7.FillColor = Color.Gray;

            not_equal_error_number = 0;
        }

        private void ucBtnImg8_BtnClick(object sender, EventArgs e)
        {


            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBoxPump1.Checked)
            {
                //ReSetPump(0x03);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump2.Checked)
            {
                //ReSetPump(0x4);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x04);
            }

            if (checkBoxPump3.Checked)
            {
                //ReSetPump(0x05);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x05);
            }

            if (checkBoxPump4.Checked)
            {
                //ReSetPump(0x6);
                Thread.Sleep(120);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x06);
            }
            if (checkBoxPump5.Checked)
            {
                //ReSetPump(0x07);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x07);
            }

            if (checkBoxPump6.Checked)
            {
                //ReSetPump(0x08);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x08);
            }
            if (checkBoxPump7.Checked)
            {
                //ReSetPump(0x09);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x09);
            }

            if (checkBoxPump8.Checked)
            {
                //ReSetPump(0x10);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 16);
            }

            if (checkBoxPump9.Checked)
            {
                //ReSetPump(0x11);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 17);
            }

            if (checkBoxPump10.Checked)
            {
                //ReSetPump(0x12);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 18);
            }

            if (checkBoxPump11.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 19);
            }

            if (checkBoxPump12.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 20);
            }




        }

        private void ucBtnImg32_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }


            Thread.Sleep(send_delay_time_g);
            string offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x03);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x04);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x05);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x06);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x07);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x08);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x09);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 16);


            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 17);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 18);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 19);


            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 20);

        }

        private void ucBtnImg33_BtnClick(object sender, EventArgs e)
        {
            //JytPrintEngineWrapper jprint = new JytPrintEngineWrapper(Jyt.Sdk.PrintEngine.MainBoardType.Eth_Gen5_4H, 1, Jyt.Sdk.PrintEngine.HeadType.KY300);
            JytPrintEngineWrapper jprint = new JytPrintEngineWrapper(Jyt.Sdk.PrintEngine.MainBoardType.Eth_Gen5_4H, 1, Jyt.Sdk.PrintEngine.HeadType.KY300);



        }

        private void ucBtnImg33_BtnClick_1(object sender, EventArgs e)
        {
            //byte[] order_content_byte_array = new byte[8];
            //int a = int.Parse(textBox_A_Offset.Text);
            ////添加开动作
            ////01 10 00 01   00 02     04   88 B8 00 00   99 E6
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH10(1, 2, 0, 1, 0x0300, 4, a);//填充写单个线圈寄存器//CoilOFFAndON(33, a);
            ////01 10 00 01 00 02 04 13 88 00 00 B6 CD
            //SendCammand(order_content_byte_array, "Switch:" + a);

            int a = int.Parse(textBox_A_Offset.Text);
            SendZAxismotorMove(1, a);
            Thread.Sleep(100);
            SendZAxismotorMove(2, a);
            Thread.Sleep(100);
            SendZAxismotorMove(3, a);
            Thread.Sleep(100);


        }

        private void ucBtnImg34_BtnClick(object sender, EventArgs e)
        {
            byte[] order_content_byte_array = new byte[8];
            int a = 1000;
            //添加开动作
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_1(1, 1,0,0,0x0101);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            //Tx:01 06 00 00 01 01 49 9A

            //Tx:084-01 06 00 00 01 01 49 9A

            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg35_BtnClick(object sender, EventArgs e)
        {
            byte[] order_content_byte_array = new byte[8];
            int a = 1000;
            //添加开动作
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_1(1, 1, 0, 0, 0x0400);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg36_BtnClick(object sender, EventArgs e)
        {
            //byte[] order_content_byte_array = new byte[8];
            //int a = 1000;
            ////添加开动作
            ////添加开动作
            ////order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_1(1, 1, 0, 0x18, 0x0001);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            ////Tx:01 06 00 00 01 01 49 9A

            ////Tx:084-01 06 00 00 01 01 49 9A

            ////SendCammand(order_content_byte_array, "Switch:" + a);

            ////Thread.Sleep(100);
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Find_Zero(1, 1, 0, 0, 0x0601);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            ////Tx:01 06 00 00 01 01 49 9A
            ////Tx:084-01 06 00 00 01 01 49 9A
            ////       01 06 00 00 06 01 4B AA
            //SendCammand(order_content_byte_array, "Switch:" + a);


            //int a = int.Parse(textBox_A_Offset.Text);
            SendZAxisMotorFindZero(1);
            Thread.Sleep(100);
            SendZAxisMotorFindZero(2);
            Thread.Sleep(100);
            //SendZAxisMotorFindZero(3);
            //Thread.Sleep(100);



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
        /// <summary>
        /// Z轴电机位移，绝对位置
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="offset"></param>
        private void SendZAxismotorMove(byte device_id,int offset)
        {
            byte[] order_content_byte_array = new byte[8];
            int a = offset;// int.Parse(textBox_A_Offset.Text);
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH10(device_id, 2, 0, 1, 0x0300, 4, a);//填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg37_BtnClick(object sender, EventArgs e)
        {
            byte[] order_content_byte_array = new byte[8];
            float a = float.Parse(textBox_A_Offset.Text);
            //添加开动作
            //01 10 00 03   00 02   04  40 00 46 1C   95 D3

            //01 06 00 03 72 10 5C A6

            //Tx:057-01 06 00 03 7D 00 58 9A
            //Tx:072-01 06 00 05 7D 00 B8 9B
            //          01 06 00 00 06 01 4B AA
            //Tx: 329 - 01 10 00 03 00 02 04 00 00 47 7A 01 A9
            //Tx:291-   01 10 00 03   00 02     04   00 00 47 7A   01 A9
            //          01 10 00 01   00 02     04   88 B8 00 00   99 E6  
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH10(1, 2, 0, 1, 0x0300, 4, a);//填充写单个线圈寄存器//CoilOFFAndON(33, a);

            //Tx: 379 - 01 10 00 03 00 02 04  00 00 45 C8  80 BC   //6400
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(1, 2, 0, 3, 0x0300, 4, a);//
            SendCammand(order_content_byte_array, "Switch:" + a);

            //Thread.Sleep(100);
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(1, 2, 0, 5, 0x0300, 4, a * 10);//
            //SendCammand(order_content_byte_array, "Switch:" + a);

            //Thread.Sleep(100);
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(1, 2, 0, 7, 0x0300, 4, a * 10);//
            //SendCammand(order_content_byte_array, "Switch:" + a);

        }

        private void ucBtnImg38_BtnClick(object sender, EventArgs e)
        {
            //OpenFile_Preload_Clean("Preload", treeView1);

            OpenFile_Preload_Clean("预装试剂", treeView1);
            label13.Text = "预装试剂";

            //ActionConversionOrder(treeView_preload);
        }

        private void ucBtnImg39_BtnClick(object sender, EventArgs e)
        {
            //OpenFile_Preload_Clean("Clean", treeView1);

            OpenFile_Preload_Clean("WASH", treeView1);
            label13.Text = "WASH";
            //ActionConversionOrder(treeView_clean);
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ucBtnImg24_BtnClick(object sender, EventArgs e)
        {
            OpenFile_Preload_Clean("TCA", treeView1);
            label13.Text = "TCA";
        }

        private void ucBtnImg40_BtnClick(object sender, EventArgs e)
        {
            OpenFile_Preload_Clean("WASHCAPACAPBOXI", treeView1);
            label13.Text = "WASHCAPACAPBOXI";
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            frm_print_atcg.CloseCam();
            //System.Threading.Thread.CurrentThread.Abort();
            Process.GetCurrentProcess().Kill();
            Application.ExitThread();
            //System.Environment.Exit(0);
            Environment.Exit(0);
            //Application.Exit();
            //Application.ExitThread();
            //System.Environment.Exit(System.Environment.ExitCode); 
        }

        private static void ReStart()
        {
            Application.Exit();
            System.Diagnostics.Process.Start(Application.ExecutablePath);
        }
    }
}
