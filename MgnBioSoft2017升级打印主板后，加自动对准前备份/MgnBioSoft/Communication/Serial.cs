using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace MGNBIO.Communication
{
    public partial class Serial
    {
        private SerialPort comm = new SerialPort();
        public StringBuilder builder = new StringBuilder();//避免在事件处理方法中反复的创建，定义到外面。接收到的数据
        private long receive_count = 0;//接收计数
        private long send_count = 0;//发送计数
        private bool saveflag = false;//保存报文标志
        public bool HexFlag = false;
        private byte[] buffer = new byte[1024];
        //private string[] ports;
        //private int receive
        public Serial()
        {
            //初始化SerialPort对象
            comm.NewLine = "\r\n";
            comm.RtsEnable = true;//根据实际情况吧。
            receive_count = 0;
            send_count = 0;
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            //添加事件注册
            comm.DataReceived += comm_DataReceived;
        }

        void comm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int n = comm.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
            receive_count += n;
            byte[] buf = new byte[n];//声明一个临时数组存储当前来的串口数据
            receive_count += n;//增加接收计数
            comm.Read(buf, 0, n);//读取缓冲数据
                                 //builder.Clear();//清除字符串构造器的内容
            if (HexFlag)
            {
                foreach (byte b in buf)
                {
                    //依次的拼接出16进制字符串
                    builder.Append(b.ToString("X2") + "");
                    //Console.WriteLine(b.ToString("X2") + "");
                }
            }
            else
            {
                //直接按ASCII规则转换成字符串
                builder.Append(Encoding.ASCII.GetString(buf));
            }
        }

        public void removestr(int len)
        {
            builder.Remove(0, len);
        }

        public void Close()
        {
            //根据当前串口对象，来判断操作
            if (comm.IsOpen)
            {
                //打开时点击，则关闭串口
                comm.Close();
            }
        }
        public void Open(string PortName, string BaudRate)
        {
            comm.PortName = PortName;
            comm.BaudRate = int.Parse(BaudRate);
            try
            {
                comm.Open();
            }
            catch (Exception ex)
            {
                //捕获到异常信息，创建一个新的comm对象，之前的不能用了。
                //comm = new SerialPort();
                //现实异常信息给客户。
                writeLogFile(ex.Message);
            }
        }

        public int Send(string sendstr, bool HexFlag)
        {
            int n = 0;
            if (!comm.IsOpen)
                return 0;
            byte[] buf = new byte[sendstr.Length / 2];
            if (HexFlag)
            {
                buf = HexStringToByteArray(sendstr);
                //转换列表为数组后发送
                comm.Write(buf, 0, buf.Length);
                //记录发送的字节数
                n = buf.Length;
            }
            else
            {
                comm.Write(sendstr);
                n = sendstr.Length;
                /*
                byte[] buff = new byte[sendstr.Length];//
                buff = Encoding.ASCII.GetBytes(sendstr);
                comm.Write(buff, 0, buff.Length);
                n = sendstr.Length;
                */
            }

            return n;
        }

        private byte[] HexStringToByteArray(string strHexString)
        {
            strHexString.Replace(" ", "");
            int len = strHexString.Length;
            if ((len % 2) != 0)
                writeLogFile("");

            int byteLen = len / 2;
            byte[] bytes = new byte[byteLen];
            for (int i = 0; i < byteLen; i++)
            {
                bytes[i] = Convert.ToByte(strHexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }


        #region 写log文件
        public int writeLogFile(string str)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\run.log";
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 2000000)
                    File.Delete(filePath);
            }

            FileStream aFile = new FileStream(filePath, FileMode.OpenOrCreate | FileMode.Append);
            StreamWriter sw = new StreamWriter(aFile);
            DateTime tt = DateTime.Now;
            str = "[" + tt.ToString() + "] " + str;
            sw.WriteLine(str);
            sw.Close();
            aFile.Close();
            return 1;
        }
        #endregion
    }
}