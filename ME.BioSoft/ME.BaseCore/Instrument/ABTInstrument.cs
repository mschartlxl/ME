using LogHelperLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.IO.Ports;
using System.Runtime.InteropServices;

using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;
using System.IO;
using ME.Language;
using System.Runtime.Remoting.Channels;
using ME.BaseCore.Models.Enums;

namespace ME.BaseCore.Instrument
{
    public class ABTInstrument
    {

        public ABTInstrument()
        {
            ReadSerialConfig();
        }
        public SerialPort GetElement(XElement instrumentEE)
        {
            SerialPort serialcfg = new SerialPort();
            List<XElement> paramXES = instrumentEE.Elements().ToList();
            foreach (XElement param in paramXES)
            {
                if (param.Name == "SerialPort")
                {
                    serialcfg.PortName = param.Value;
                }
                else if (param.Name == "BautRate")
                {
                    serialcfg.BaudRate = Convert.ToInt32(param.Value);
                }
                else if (param.Name == "Parity")
                {
                    serialcfg.Parity = (Parity)System.Enum.Parse(typeof(Parity), param.Value);
                }
                else if (param.Name == "DataBits")
                {
                    serialcfg.DataBits = Convert.ToInt32(param.Value);
                }
                else if (param.Name == "StopBits")
                {
                    serialcfg.StopBits = (StopBits)System.Enum.Parse(typeof(StopBits), param.Value);
                }

            }
            return serialcfg;
        }
        public SerialPort SerialPump { get; set; } = new SerialPort();
        public SerialPort SerialSwitch { get; set; } = new SerialPort();
        /// <summary>
        public void ReadSerialConfig()
        {
            var serialcfg = AppDomain.CurrentDomain.BaseDirectory + "Config/SerialConfig.xml";
            if (System.IO.File.Exists(serialcfg))
            {
                XElement instrumentEEs = XElement.Load(serialcfg);
                List<XElement> instrumentXES = instrumentEEs.Elements().ToList();
                foreach (XElement instrumentEE in instrumentXES)
                {
                    var paramNum = XMLHelper.GetAttributeStringValue(instrumentEE, "Name");
                    if (paramNum == "Pump")
                    {
                        SerialPump = GetElement(instrumentEE);
                    }
                    if (paramNum == "Switch")
                    {
                        SerialSwitch = GetElement(instrumentEE);
                    }

                }
            }
        }
        public bool IsOpen(SerialPort serialPort)
        {
            return serialPort.IsOpen;
        }


        public bool OpenIfRequirement(SerialPort serialPort)
        {
            Monitor.Enter(serialPort);
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            
            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.Open();

                    return true;
                }
                catch (Exception ex)
                {
                    //OnMeasureShowHint(LangHelper.LangRes("portNotFound"));
                    //throw new Exception("打开端口错误,端口地址为：COM" + config.PortAddress.ToString() + "\n" + ex.Message.ToString());
                }
            }
            Monitor.Exit(serialPort);
            //ABTGlobal.SerialPortStatus = Channel.DriverStatusEnum.FailedToConnecte;
            return false;
        
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancelFun"></param>
        /// <param name="sendData"></param>
        /// <param name="isReadData"></param>
        /// <param name="overtime">单位是s</param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public byte[] Send_16(Func<bool> cancelFun, byte[] sendData, bool isReadData, SerialPort newSerial, decimal overtime = 0)
        {
            byte[] readD = null;
            try
            {
                if (sendData?.Length > 0)
                {
                    newSerial.DiscardInBuffer();
                    //LOGHelper.Info("发送指令:" + BitConverter.ToString(sendData));
                    newSerial.Write(sendData, 0, sendData.Length);
                    if (isReadData)
                    {
                        byte MB_MOTOR = sendData[1];
                        byte stateValue = 0;
                        int readNullCount = 0;
                        //当前指令发送之后，下位机在时间范围内或者下位机故障未给上位机发送数据则进行三次重发
                        while (readNullCount < 3)
                        {
                            readD = ReadAnything_16(cancelFun, MB_MOTOR, overtime, newSerial, out stateValue);
                            if (readD == null || readD.Count() <= 3)
                            {
                                newSerial.DiscardInBuffer();
                                newSerial.Write(sendData, 0, sendData.Length);
                                stateValue = 0;
                                readNullCount++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (readNullCount >= 3)
                        {
                            //ABTGlobal.SerialPortStatus = Channel.DriverStatusEnum.FailedToConnecte;
                            //ABTGlobal.IsStop = true;
                            //ABTGlobal.IsDevicePower = 3;
                            return null;
                        }
                        if (stateValue != 0)
                        {
                            bool isSaveEroor = true;
                            //QPCR.Model.AlarmInfo warnInfo = new QPCR.Model.AlarmInfo();
                            //warnInfo.CMDCode = MB_MOTOR.ToString("X2");

                            if (isSaveEroor)
                            {
                                //OnWarningShowHint(warnInfo);
                            }
                        }
                        return readD;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {

                ABTGlobal.IsStop = true;
                if (ex.Message.IndexOf("端口") >= 0)
                {
                    if (ex.Message.IndexOf("被关闭") >= 0)
                    {
                        ABTGlobal.SerialPortStatus = DriverStatusEnum.FailedToConnecte;
                    }
                    if (ex.Message.IndexOf("访问被拒绝") >= 0)
                    {
                        ABTGlobal.SerialPortStatus = DriverStatusEnum.FailedToConnecte;
                    }
                }
                return null;
            }
        }
        static int CNum = 0;
        public static void ClearMemory10()
        {
            CNum++;
            if (CNum >= 10)
            {
                ClearMemory();
                CNum = 0;
            }
        }
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        DateTime overdateTime = DateTime.Now;
        List<byte> _buffer;//数据缓冲
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancelFun"></param>
        /// <param name="MB_MOTOR"></param>
        /// <param name="overTime">单位是s</param>
        /// <returns></returns>
        public byte[] ReadAnything_16(Func<bool> cancelFun, byte MB_MOTOR, decimal overTime, SerialPort serialPort, out byte state)
        {
            state = 0;
            //overdateTime = DateTime.Now.AddSeconds(overTime);
            overdateTime = DateTime.Now.AddMilliseconds((double)(overTime * 1000));
            _buffer = null;
            ClearMemory10();
            int n = 0;
            while (cancelFun != null ? !cancelFun() : true)
            {
                n = serialPort.BytesToRead;
                if (n > 0)
                {
                    var data = (byte)serialPort.ReadByte();
                    if (null == _buffer)
                        _buffer = new List<byte>();
                    if (null != _buffer)
                    {
                        if (_buffer.Count <= 8)
                        {
                            _buffer.Add(data);
                        }
                        else
                        {
                            if (_buffer.Count < _buffer[3])
                            {
                                _buffer.Add(data);
                            }
                            if (_buffer.Count >= _buffer[3])
                            {
                                //if (_buffer[1] == (byte)(MB_MOTOR | 0x80))
                                //{ }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (_buffer == null)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
                if (overTime > 0)
                {
                    if (overdateTime < DateTime.Now)
                    {
                        LogHelper.SystemError(string.Format("读值超时:{0}s", overdateTime));
                        break;
                    }
                }
            }

            if (_buffer != null)
            {
                LogHelper.SystemError("返回指令:" + BitConverter.ToString(_buffer?.ToArray()));
                ABTGlobal.IsDevicePower = 2;
            }
            return _buffer?.ToArray();
        }
        ///<summary>
        /// 将浮点数转ASCII格式十六进制字符串（符合IEEE-754标准（32））
        /// <paramname="data">浮点数值</param>
        /// <returns>十六进制字符串</returns>
        /// </summary>
        public static string float2String(float data)
        {
            byte[] intBuffer = BitConverter.GetBytes(data);
            string stringBuffer = "";
            for (int i = 0; i < intBuffer.Length; i++)
            {
                stringBuffer += toHexString(intBuffer[i] & 0xff, 2);
            }
            var tss = byteToHexStr(intBuffer);
            return stringBuffer.ToString();
        }
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 将二进制值转ASCII格式十六进制字符串
        /// </summary>
        /// <paramname="data">二进制值</param>
        /// <paramname="length">定长度的二进制</param>
        /// <returns>ASCII格式十六进制字符串</returns>
        public static string toHexString(int data, int length)
        {
            string result = "";
            if (data > 0)
                result = Convert.ToString(data, 16).ToUpper();
            if (result.Length < length)
            {
                // 位数不够补0
                StringBuilder msg = new StringBuilder(0);
                msg.Length = 0;
                msg.Append(result);
                for (; msg.Length < length; msg.Insert(0, "0")) ;
                result = msg.ToString();
            }
            return result;
        }
        public static byte[] ConvertHex2(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[2];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            //hex[2] = (byte)((velocity >> 16) & 0xff);
            //hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }

    }


}