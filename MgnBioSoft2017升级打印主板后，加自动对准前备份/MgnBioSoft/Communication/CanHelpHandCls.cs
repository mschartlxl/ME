using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices; 
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using XmlHelper; 

namespace PrjExecutiveOrder
{
    public class CanHelpHandCls
    {
        #region 属性
        /// <summary>
        /// 静态路径.
        /// </summary>
        public static string strFilesPath = "";
        #endregion

        #region 定义
        /// <summary>
        ///  发送错误信息.
        /// </summary>
        public static Int32 iSendErrorCount = 0;

        /// <summary>
        /// 回收错误的次数.
        /// </summary>
        public static Int32 iErrCountResive = 0;

        /// <summary>
        /// Can对应的信息.
        /// </summary>
        public static CanOrderMsgModel canModelmsg = new CanOrderMsgModel();

        /// <summary>
        /// 其他参数
        /// </summary>
        public static Parameter_Other_Model _ParameterOther = new Parameter_Other_Model();


        //usb-e-u 波特率.
        const UInt32 STATUS_OK = 1;
        static UInt32[] GCanBrTab = new UInt32[10]{
	                0x060003, 0x060004, 0x060007,
		                0x1C0008, 0x3AC011, 0x160023,
		                0x1C002C, 0x1600B3, 0x1C00E0,
		                0x1C01C1
                };
        static UInt32[] GCanBrTabValue = new UInt32[10]{
            1000,
            800,
            500,
            250,
            125,
            100,
            50,
            20,
            10,
            5	 
        };
        #endregion

        #region 方法

        /// <summary>
        /// 字符串不足两位的补齐.
        /// </summary>
        /// <param name="ss">需要处理的字符串</param>
        /// <returns>返回的字符串</returns>
        private static string strfor(string ss)
        {
            if (ss.Length < 2)
            {
                ss = "0" + ss;
            }
            return ss;
        }

        /// <summary>
        /// 返回对应的数组
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        public static string[] strArray16(string ss)
        {
            if (ss.Length < 16)
            {
                ss=ss.PadRight('0');
            }
            string[] strretunvalue = new string[8];
            for (int i = 0; i < 8; i++)
            {
                strretunvalue[i] = ss.Substring(i * 2, 2);
            }
            return strretunvalue;
        }

        /// <summary>
        /// 此函数用以清空指定缓冲区。.
        /// </summary>
        public static void ClearBuffer()
        {
            VCI_ClearBuffer(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
        }
        /// <summary>
        /// 接收返回信息.
        /// </summary>
        /// <returns>返回信息的字符串</returns>
        public static string[] GetSignBack()
        {
            string[] rtnSign = new string[1];
            //rtnSign[0] = "0";
            //rtnSign[1] = "";
            IList<string[]> ggg = GetSignBackLst(3);
            if (ggg != null)
            {
                if (ggg.Count > 0)
                {
                    if (ggg[0] != null)
                    {
                        if (ggg[0].Length >= 2)
                        {
                            rtnSign = new string[2];
                            rtnSign[0] = ggg[0][0];
                            rtnSign[1] = ggg[0][1];
                        }
                    }
                }
            }

            return rtnSign;
        }

        #region 批量回收数据 2013-10-11

        /// <summary>
        /// 接收返回信息:新增批量返回.
        /// </summary>
        /// <returns>返回信息的字符串</returns>
        public static IList<string[]> GetSignBackPL()
        {
            IList<string[]> ggg = GetSignBackLst(50);
            return ggg; 
        }

        /// <summary>
        /// 批量回收数据，一次性回收N条数据，同一处理.
        /// </summary>
        /// <param name="ilength">返回长度：1 只返回一条数据</param>
        /// <returns>返回回收后的数据</returns>
        public static IList<string[]> GetSignBackLst(UInt32 ilength)
        {
            //OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            IList<string[]> rtnSign = new List<string[]>();
             
            #region CAN 连接使用
            try
            {
                DateTime dtbegin = DateTime.Now;
                if (iErrCountResive < 1000)
                {
                    iErrCountResive = iErrCountResive + 1;
                }
                #region 回收
                UInt32 res = new UInt32();

                //res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                //res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);


                if (_ParameterOther.CommunicationModuleType.Equals("2"))
                {

                    res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                }
                else
                {
                    res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                }



                for (Int32 i = 0; i < 200; i++)
                {
                    if (res > 0)
                    {
                        break;
                    } 



                    //res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                    ////add NET receiveNum
                    //res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);


                    if (_ParameterOther.CommunicationModuleType.Equals("2"))
                    {
                     
                        res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                    }
                    else
                    {
                        res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                    }




                }
                if (res == 0)
                {
                    return null;
                } 
                /////////////////////////////////////
                UInt32 con_maxlen = ilength;
                IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ)) * (Int32)con_maxlen);


                //res = VCI_Receive(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, pt, con_maxlen, 100);
                //res = NetCanHelpHandCls.VCI_Receive(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, pt, con_maxlen, 100);


                if (_ParameterOther.CommunicationModuleType.Equals("2"))
                {
                    res = VCI_Receive(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, pt, con_maxlen, 100);
                }
                else
                {
                    //res = NetCanHelpHandCls.VCI_Receive(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, pt, con_maxlen, 100);
                }


                ////////////////////////////////////////////////////////

                for (UInt32 i = 0; i < res; i++)
                {
                    PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ obj = (PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ))), typeof(PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ));
                     
                    String str = "";
                    if (obj.RemoteFlag == 0)
                    { 
                        byte lens = (byte)(obj.DataLen % 9);
                        byte ji = 0;
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[0], 16));
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[1], 16));
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[2], 16));
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[3], 16));
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[4], 16));
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[5], 16));
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[6], 16));
                        if (ji++ < lens)
                            str += "" + strfor(System.Convert.ToString(obj.Data[7], 16));

                    }
                    string[] straddlst = new string[2];
                    straddlst[0] = obj.ID.ToString();
                    straddlst[1] = str;
                    rtnSign.Add(straddlst);

                    IList<string> sWriteMsg = new List<string>();//写入日志计算数据
                    DateTime dtend = DateTime.Now;
                    string strMsg = getMsgForOrder(straddlst[0], str, dtbegin, dtend);

                    sWriteMsg.Add(strMsg + " 【错误次数：" + iErrCountResive.ToString() + "次】");
                    iReadLog(sWriteMsg);
                    iErrCountResive = 0;
                }

                Marshal.FreeHGlobal(pt);
                #endregion

            }
            catch (Exception ee)
            {
                iReadLog("回收异常");
            }
            return rtnSign;
            #endregion
        }

        /// <summary>
        /// 批量回收数据，一次性回收N条数据，同一处理.
        /// </summary>
        /// <param name="ilength">返回长度：1 只返回一条数据</param>
        /// <returns>返回回收后的数据</returns>
        public static Int32 GetSignBackLst(ref IList<string[]> rtValue, ref IList<Int32> rtID)
        {
            //OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            rtValue = new List<string[]>();
            rtID = new List<Int32>();
             

            #region CAN 连接使用
            try
            {
                DateTime dtbegin = DateTime.Now;
                if (iErrCountResive < 1000)
                {
                    iErrCountResive = iErrCountResive + 1;
                }
                #region 回收
                UInt32 res = new UInt32();


                //res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                ////linshi
                //res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);


                if (_ParameterOther.CommunicationModuleType.Equals("2"))
                {

                    res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                }
                else
                {
                    res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                }


                for (Int32 i = 0; i < 200; i++)
                {
                    if (res > 0)
                    {
                        break;
                    }


                    //iGetOuttimes(1);
                    //res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                    ////linshi
                    //res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);

                    if (_ParameterOther.CommunicationModuleType.Equals("2"))
                    {
                        res = NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);

                    }
                    else
                    {
                        res = VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
                    }




                }
                if (res == 0)
                {
                    return 0;
                }
                //res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0],50, 100);
                /////////////////////////////////////
                UInt32 con_maxlen = res;
                IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ)) * (Int32)con_maxlen);


                if (_ParameterOther.CommunicationModuleType.Equals("1"))
                {
                    res = VCI_Receive(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, pt, con_maxlen, 100);
                }
                else
                {
                    ////增加了NETCAN接收数据
                    //res = NetCanHelpHandCls.VCI_Receive(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, pt, con_maxlen, 100);
                }


                ////////////////////////////////////////////////////////
                if (res > 0)
                {
                    for (UInt32 i = 0; i < res; i++)
                    {
                        PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ obj = (PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ))), typeof(PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ));

                        string[] strReturnValue = new string[obj.Data.Length];
                        string str = "";
                        if (obj.RemoteFlag == 0)
                        {
                            if (obj.Data != null)
                            {
                                for (int k = 0; k < obj.Data.Length; k++)
                                {
                                    if (k < obj.Data.Length)
                                    {
                                        strReturnValue[k] = strfor(System.Convert.ToString(obj.Data[k], 16));
                                        str = str + strReturnValue[k];
                                    }
                                }
                            }

                            #region 确定
                            //byte lens = (byte)(obj.DataLen % 9);
                            // byte ji = 0;
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[0], 16));
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[1], 16));
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[2], 16));
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[3], 16));
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[4], 16));
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[5], 16));
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[6], 16));
                            // if (ji++ < lens)
                            //     str += "" + strfor(System.Convert.ToString(obj.Data[7], 16));
                            #endregion
                        }
                        if (obj.ID > 0 || str != "")
                        {
                            rtValue.Add(strReturnValue);
                            rtID.Add(ParameterHelper.ConvertTypeCls.GetInt32(obj.ID));
                        }
                        IList<string> sWriteMsg = new List<string>();//写入日志计算数据
                        DateTime dtend = DateTime.Now;
                        string strMsg = getMsgForOrder(obj.ID.ToString(), str, dtbegin, dtend);

                        sWriteMsg.Add(strMsg + " 【错误次数：" + iErrCountResive.ToString() + "次】");
                        iReadLog(sWriteMsg);
                        iErrCountResive = 0;
                    }
                }

                Marshal.FreeHGlobal(pt);
                #endregion

            }
            catch (Exception ee)
            {
                iReadLog("回收异常");
            }
            return 1;
            #endregion
        }

        #endregion

        /// <summary>
        /// 根据Order返回信息.
        /// </summary>
        /// <param name="OrderID">接收的指令ID</param>
        /// <param name="OrderMsg">接收的指令内容</param>
        /// <returns>返回信息</returns>
        private static string getMsgForOrder(string OrderID, string OrderMsg)
        {
            string str_ReturnValue = "";
            try
            {
                IList<Int32> iFlg = new List<Int32>();
                iFlg.Add(1);
                iFlg.Add(2);
                iFlg.Add(3);
                iFlg.Add(4);
                iFlg.Add(5);
                iFlg.Add(6);
                iFlg.Add(7);
                string str_Time = ParameterHelper.ConvertTypeCls.GetStringForDate(DateTime.Now, iFlg);
                str_ReturnValue = "【接收指令】   【时间：" + str_Time + "】   【指令ID：" + OrderID + "】  【指令内容：" + OrderMsg + "】";
            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80025", "根据Order返回信息异常：" + ee.Message, 0, 2);
                return "";
            }
            return str_ReturnValue;
        }

        /// <summary>
        /// 根据Order返回信息.
        /// </summary>
        /// <param name="OrderID">接收的指令ID</param>
        /// <param name="OrderMsg">接收的指令内容</param>
        /// <returns>返回信息</returns>
        private static string getMsgForOrder(string OrderID, string OrderMsg, DateTime dtbegin, DateTime dtend)
        {
            string str_ReturnValue = "";
            try
            {
                IList<Int32> iFlg = new List<Int32>();
                iFlg.Add(3);
                iFlg.Add(4);
                iFlg.Add(5);
                iFlg.Add(6);
                iFlg.Add(7);
                string str_Time = ParameterHelper.ConvertTypeCls.GetStringForDate_G(DateTime.Now, iFlg);
                iFlg.Clear();
                iFlg.Add(4);
                iFlg.Add(5);
                iFlg.Add(6);
                iFlg.Add(7);

                string str_Time_B = ParameterHelper.ConvertTypeCls.GetStringForDate_G(dtbegin, iFlg);

                string str_Time_E = ParameterHelper.ConvertTypeCls.GetStringForDate_G(dtend, iFlg);

                str_ReturnValue = "【接收指令】   【时间:[" + str_Time + " (" + str_Time_B + "——" + str_Time_E + ")】   【指令ID：" + OrderID + "】  【指令内容：" + OrderMsg + "】";
            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80025", "根据Order返回信息异常：" + ee.Message, 0, 2);
                return "";
            }
            return str_ReturnValue;
        }

        /// <summary>
        /// 根据Order返回信息.
        /// </summary>
        /// <param name="OrderID">接收的指令ID</param>
        /// <param name="OrderMsg">接收的指令内容</param>
        /// <returns>返回信息</returns>
        private static string getMsgForDatetime(DateTime dt)
        {
            string str_ReturnValue = "";
            try
            {
                IList<Int32> iFlg = new List<Int32>();
                iFlg.Add(1);
                iFlg.Add(2);
                iFlg.Add(3);
                iFlg.Add(4);
                iFlg.Add(5);
                iFlg.Add(6);
                iFlg.Add(7);
                string str_Time =ParameterHelper.ConvertTypeCls.GetStringForDate(dt, iFlg);
                str_ReturnValue = str_Time;
            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80025", "根据Order返回信息异常：" + ee.Message, 0, 2);
                return "";
            }
            return str_ReturnValue;
        }

        /// <summary>
        /// 字符串数组与字符数组的转化.
        /// </summary>
        /// <param name="orderbefore">字符串数组</param>
        /// <returns>字符数组</returns>
        private static byte[] OrderReplace(string[] orderbefore)
        {
            byte[] orderafter = new byte[canModelmsg.IDataLen];
            for (int i = 0; i < orderbefore.Length; i++)
            {
                orderafter[i] = Convert.ToByte(orderbefore[i], 16);
            }
            return orderafter;
        }

        //发送时只读一次 0 未为读 1为已经读。
        public static int readOtherParameterFlage = 0;


        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="i_ID">指令ID</param>
        /// <param name="str_Data">指令内容</param>
        /// <returns>发送是否成功标志</returns>
        public static uint SendSignInfo(int i_ID, string[] str_Data)
        {
            uint returnflag = 0;
             
            //在这里打开一次配置文件
            if (readOtherParameterFlage == 0)
            {
                readParameterOtherModelCommunicationType();
            }


            #region CAN 使用
            try
            {
                PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ vco = new PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ();
                vco.ExternFlag = 1;
                vco.SendType = (byte)canModelmsg.Sendtype;
                vco.ID = (uint)i_ID;
                vco.DataLen = (byte)str_Data.Length;
                //vco.DataLen = (byte)canModelmsg.IDataLen;
                vco.ExternFlag = (byte)canModelmsg.IExternFlag;
                vco.RemoteFlag = (byte)canModelmsg.IRemoteFlag;
                vco.Init();
                vco.Data = new byte[str_Data.Length];
                vco.Reserved = new byte[3];
                vco.Data = OrderReplace(str_Data);

                if (_ParameterOther.CommunicationModuleType.Equals("1"))
                {
                    returnflag = VCI_Transmit(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref vco, (uint)1);
                }
                else
                {
                    canModelmsg.Devtype = 17;
                    returnflag = NetCanHelpHandCls.VCI_TransmitNet(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, vco, (uint)1);
                }

                iSendErrorCount = 0;
                 
                if (returnflag == 0)
                {
                    iSendErrorCount = iSendErrorCount + 1;
                    iReadError();
                    Thread.Sleep(1);
                    //returnflag = VCI_Transmit(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref vco, (uint)1);
                }
            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80000", "发送数据异常（ID：" + i_ID + " Data：" + str_Data + ") :" + ee.Message, 0, 2);
            }

            return returnflag;
            #endregion
        }

        public static void readParameterOtherModelCommunicationType()
        {
            #region 读取系统路径
            //读取路径
            string strshangyijibul = System.Windows.Forms.Application.StartupPath;
            System.IO.DirectoryInfo topdir = System.IO.Directory.GetParent(strshangyijibul);
            _ParameterOther = Read_Parameter_Other_Model_Data(strshangyijibul);
            //参数读取完成。
            if (_ParameterOther != null)
                readOtherParameterFlage = 1;
            #endregion
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="i_ID">指令ID</param>
        /// <param name="str_Data">指令内容</param>
        /// <param name="iLoopCount">需要循环发送的次数</param>
        /// <returns>发送是否成功标志</returns>
        public static uint SendSignInfo(int i_ID, string[] str_Data, int iLoopCount)
        {
            uint returnflag = 0;
            //在这里打开一次配置文件
            if (readOtherParameterFlage == 0)
            {
                readParameterOtherModelCommunicationType();
            }

            #region CAN 连接使用
            try
            {
                //把下面代码抽到一个方法里面去。
                PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ vco = new PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ();
                vco.ExternFlag = 1;
                vco.SendType = (byte)canModelmsg.Sendtype;
                vco.ID = (uint)i_ID;
                vco.DataLen = (byte)str_Data.Length;
                //vco.DataLen = (byte)canModelmsg.IDataLen;
                vco.ExternFlag = (byte)canModelmsg.IExternFlag;
                vco.RemoteFlag = (byte)canModelmsg.IRemoteFlag;
                vco.Init();
                vco.Data = new byte[str_Data.Length];
                vco.Reserved = new byte[3];
                vco.Data = OrderReplace(str_Data);


                if (_ParameterOther.CommunicationModuleType.Equals("1"))
                {
                    returnflag = VCI_Transmit(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref vco, (uint)1);
                }
                else
                {
                    canModelmsg.Devtype = 17;
                    returnflag = NetCanHelpHandCls.VCI_TransmitNet(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, vco, (uint)1);
                }

                iSendErrorCount = 0;
                //这个重发需要修改 
                if (returnflag == 0)
                {
                    iSendErrorCount = iSendErrorCount + 1;
                    iReadError();
                    Thread.Sleep(1);

                    if (_ParameterOther.CommunicationModuleType.Equals("1"))
                    {
                        returnflag = VCI_Transmit(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref vco, (uint)1);
                    }
                    else
                    {
                        canModelmsg.Devtype = 17;
                        returnflag = NetCanHelpHandCls.VCI_TransmitNet(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, vco, (uint)1);
                    }
                }
            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80000", "发送数据异常（ID：" + i_ID + " Data：" + str_Data + ") :" + ee.Message, 0, 2);
            }
            return returnflag;
            #endregion
        }

        /// <summary>
        /// 重新启动CAN.
        /// </summary>
        /// <returns></returns>
        private static Int32 CQCan(int iflg)
        {
            //先调用关闭指令

            close_conn();
            iReadLog("关闭CAN，并继续发送数据");
            Int32 iretfgl = OpenDeviceAndStart();
            iReadLog("重新启动CAN，并继续发送数据");
            return iretfgl;
        }

        /// <summary>
        /// 关闭连接.
        /// </summary>
        public static void closeconn()
        {
            //#region UIBCAN 连接使用
            //if (canModelmsg.I_Select_Can_Type == 1)
            //{
            //    Can9100HelpHandCls.close_conn();
            //}
            //#endregion


            #region CAN 连接使用
            if (canModelmsg.Mconnect == 1)
            {
                if (_ParameterOther.CommunicationModuleType.Equals("1"))
                {

                    VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL
                    
                }
                else
                {
                    ////增加了NETCAN
                    NetCanHelpHandCls.VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);
                }
                canModelmsg.Mconnect = 0;
                //  System.Windows.Forms.MessageBox.Show("断开成功");
            }
            #endregion

        }

        /// <summary>
        /// 关闭连接.
        /// </summary>
        public static int close_conn()
        {
            //#region UIBCAN 连接使用
            //if (canModelmsg.I_Select_Can_Type == 1)
            //{
            //    return Can9100HelpHandCls.close_conn();
            //}
            //#endregion
            #region CAN 连接使用
            try
            {
                int iFlg = 0;
                //if (canModelmsg.Mconnect == 1)
                //{

                //iFlg = (int)VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL
                if (_ParameterOther.CommunicationModuleType.Equals("1"))
                {
                    iFlg = (int)VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL
                }
                else
                {
                    ////增加了NETCAN
                    iFlg = (int)NetCanHelpHandCls.VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL
                }

                canModelmsg.Mconnect = 0;
                //  System.Windows.Forms.MessageBox.Show("断开成功");
                //}
                return iFlg;
            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80001", "关闭连接异常 :" + ee.Message, 0, 2);
                return 0;
            }
            #endregion
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static string[] iReadError()
        {
            IList<Int32> iFlg = new List<Int32>();
            iFlg.Add(3);
            iFlg.Add(4);
            iFlg.Add(5);
            iFlg.Add(6);
            iFlg.Add(7);
            string str_Time = ParameterHelper.ConvertTypeCls.GetStringForDate_G(DateTime.Now, iFlg);

            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            IList<string> sWriteMsg = new List<string>();//写入日志计算数据

            sWriteMsg.Add("【重发】 【时间：" + str_Time + "】 【第 " + iSendErrorCount.ToString() + "次】");
            string strrv = "";
            iReadCanStatus(ref strrv);
            sWriteMsg.Add("【读取CAN状态】 【时间：" + str_Time + "】 【" + strrv + "】");

            string[] sreturn = new string[10];
            PrjExecutiveOrder.CanModelCls.VCI_ERR_INFO vcERR = new PrjExecutiveOrder.CanModelCls.VCI_ERR_INFO();

            int ireturn = 0;

            if (_ParameterOther.CommunicationModuleType.Equals("1"))
            {
                    ireturn = (int)VCI_ReadErrInfo(canModelmsg.Devtype, canModelmsg.Devind, 0, ref vcERR);
            }
            else
            {
                ////增加了NETCAN
                ireturn = (int)NetCanHelpHandCls.VCI_ReadErrInfo(canModelmsg.Devtype, canModelmsg.Devind, 0, ref vcERR);
            }

            if (ireturn == 1)
            {
                sreturn[0] = vcERR.ErrCode.ToString();
                sreturn[1] = vcERR.ArLost_ErrData.ToString();
                sreturn[2] = vcERR.Passive_ErrData1.ToString();
                sreturn[3] = vcERR.Passive_ErrData2.ToString();
                sreturn[4] = vcERR.Passive_ErrData3.ToString();

                string strMsg = "【读取CAN异常信息0】  【时间：" + str_Time + "】 ";

                strMsg = strMsg + "【ErrCode=" + sreturn[0] + "】;";
                strMsg = strMsg + "【ArLost_ErrData=" + sreturn[1] + "】;";
                strMsg = strMsg + "【Passive_ErrData1=" + sreturn[2] + "】;";
                strMsg = strMsg + "【Passive_ErrData2=" + sreturn[3] + "】;";
                strMsg = strMsg + "【Passive_ErrData3=" + sreturn[4] + "】;";
                sWriteMsg.Add(strMsg);

            }
            PrjExecutiveOrder.CanModelCls.VCI_ERR_INFO vcERR1 = new PrjExecutiveOrder.CanModelCls.VCI_ERR_INFO();

            //int ireturn1 = (int)VCI_ReadErrInfo(canModelmsg.Devtype, canModelmsg.Devind, -1, ref vcERR1);

            int ireturn1 = 0;

            if (_ParameterOther.CommunicationModuleType.Equals("1"))
            {
                ireturn1 = (int)VCI_ReadErrInfo(canModelmsg.Devtype, canModelmsg.Devind, -1, ref vcERR1);
            }
            else
            {
                ////增加了NETCAN
                ireturn1 = (int)NetCanHelpHandCls.VCI_ReadErrInfo(canModelmsg.Devtype, canModelmsg.Devind, -1, ref vcERR1);
                //ireturn1 = (int)NetCanHelpHandCls.VCI_ReadErrInfo(canModelmsg.Devtype, canModelmsg.Devind, -1, ref vcERR1);
            }


            if (ireturn1 == 1)
            {
                sreturn[5] = vcERR.ErrCode.ToString();
                sreturn[6] = vcERR.ArLost_ErrData.ToString();
                sreturn[7] = vcERR.Passive_ErrData1.ToString();
                sreturn[8] = vcERR.Passive_ErrData2.ToString();
                sreturn[9] = vcERR.Passive_ErrData3.ToString();

                string strMsg = "【读取CAN异常信息-1】  【时间：" + str_Time + "】 ";

                strMsg = strMsg + "【ErrCode=" + sreturn[5] + "】;";
                strMsg = strMsg + "【ArLost_ErrData=" + sreturn[6] + "】;";
                strMsg = strMsg + "【Passive_ErrData1=" + sreturn[7] + "】;";
                strMsg = strMsg + "【Passive_ErrData2=" + sreturn[8] + "】;";
                strMsg = strMsg + "【Passive_ErrData3=" + sreturn[9] + "】;";

                sWriteMsg.Add(strMsg);
            }

            #region  写日志

            if (sWriteMsg != null)
            {
                if (sWriteMsg.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData(sWriteMsg);
                }
            }
            #endregion
            return sreturn;
        }

        /// <summary>
        /// 获取缓冲区尚未读取到的帧数.
        /// </summary>
        /// <returns></returns>
        public static uint GetReceiveNum()
        {


            if (_ParameterOther.CommunicationModuleType.Equals("1"))
            {
                return VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
            }
            else
            {
                canModelmsg.Devtype = 17;
                return NetCanHelpHandCls.Net_VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
            }
            //return VCI_GetReceiveNum(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid);
        }


        //发送列表
        List<OrderModelCls> sendOrderlist = new List<OrderModelCls>();
        //接收列表
        List<OrderModelCls> receiveOrderlist = new List<OrderModelCls>();

        //创建一个发送线程

        //创建一个接收数据的线程


        /// <summary>
        /// 连接Can总线(125kbps)　检查当前CAN设备是否连接 并且开启CAN 返回开启CAN的状态 1为开启成功 0为开启失败.
        /// </summary>
        /// <returns>连接标示</returns>
        public static int OpenDeviceAndStart()
        { 

            #region CAN-E-U 连接使用
            if (canModelmsg.Devtype >= 20)
            {
                return OpenDeviceAndStartEU();
            }
            #endregion

            #region CAN-1 连接使用
            GetTime01();
            string timing0 = canModelmsg.S16Timing0;// "03";
            string timing1 = canModelmsg.S16Timing1;
            try
            {
                if (canModelmsg.Mconnect == 1)
                {
                    //VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL 关闭CAN


                    if (_ParameterOther.CommunicationModuleType.Equals("1"))
                    {
                        VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL 关闭CAN
                    }
                    else
                    {
                        ////增加了NETCAN
                        NetCanHelpHandCls.VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL 关闭CAN
                    }


                    canModelmsg.Mconnect = 0;//设置该变量为0
                    //   System.Windows.Forms.MessageBox.Show("断开成功");
                    return canModelmsg.Mconnect;
                }
                PrjExecutiveOrder.CanModelCls.VCI_INIT_CONFIG InitConfig = new PrjExecutiveOrder.CanModelCls.VCI_INIT_CONFIG();//定义CAN初始化配置结构

                InitConfig.AccCode = System.Convert.ToUInt32(canModelmsg.S16AccCode, 16);
                InitConfig.AccMask = System.Convert.ToUInt32(canModelmsg.S16AccMask, 16);
                InitConfig.Filter = (byte)canModelmsg.IFilter;//设定滤波
                InitConfig.Mode = (byte)canModelmsg.IMode;//
                InitConfig.Timing0 = System.Convert.ToByte("0x" + timing0, 16);//设定定时器1
                InitConfig.Timing1 = System.Convert.ToByte("0x" + timing1, 16);//设定定时器2


                if (VCI_OpenDevice(canModelmsg.Devtype, canModelmsg.Devind, 0) != 1)//reserverd
                {
                    uint i = VCI_OpenDevice(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.IBaudrate);
                    //      System.Windows.Forms.MessageBox.Show("连接失败！！请确认电脑是否已连接机器的USB线");
                    canModelmsg.Mconnect = 0;
                    return 0;
                }
                if (VCI_InitCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref InitConfig) == 1)//canid此函数用以初始化指定的CAN。
                {
                    if (VCI_StartCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid) == 1)//canid此函数用以启动CAN。
                    {
                        canModelmsg.Mconnect = 1; 
                    }
                }
                else
                { 
                    canModelmsg.Mconnect = 0;
                }


            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80002", "连接Can总线异常 :" + ee.Message, 0, 2);
            }
            return canModelmsg.Mconnect;
            #endregion
        }

        /// <summary>
        /// 连接Can总线(125kbps)　检查当前CAN设备是否连接 并且开启CAN 返回开启CAN的状态 1为开启成功 0为开启失败.
        /// </summary>
        /// <returns>连接标示</returns>
        public static int OpenDeviceAndStart(out string[] strvalue)
        { 
            #region CAN 连接使用
            strvalue = null;

            GetTime01();
            string timing0 = canModelmsg.S16Timing0;// "03";
            string timing1 = canModelmsg.S16Timing1;
            try
            { 
                PrjExecutiveOrder.CanModelCls.VCI_INIT_CONFIG InitConfig = new PrjExecutiveOrder.CanModelCls.VCI_INIT_CONFIG();//定义CAN初始化配置结构

                InitConfig.AccCode = System.Convert.ToUInt32(canModelmsg.S16AccCode, 16);
                InitConfig.AccMask = System.Convert.ToUInt32(canModelmsg.S16AccMask, 16);
                InitConfig.Filter = (byte)canModelmsg.IFilter;//设定滤波
                InitConfig.Mode = (byte)canModelmsg.IMode;//
                InitConfig.Timing0 = System.Convert.ToByte("0x" + timing0, 16);//设定定时器1
                InitConfig.Timing1 = System.Convert.ToByte("0x" + timing1, 16);//设定定时器2
                if (VCI_OpenDevice(canModelmsg.Devtype, canModelmsg.Devind, 0) != 1)//reserverd
                {
                    uint i = VCI_OpenDevice(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.IBaudrate); // 9600 
                    canModelmsg.Mconnect = 0;
                    strvalue = iReadError();
                    return 0;
                }
                if (VCI_InitCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref InitConfig) == 1)//canid此函数用以初始化指定的CAN。
                {
                    if (VCI_StartCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid) == 1)//canid此函数用以启动CAN。
                    {
                        canModelmsg.Mconnect = 1; 
                    }
                }
                else
                { 
                    canModelmsg.Mconnect = 0;
                    strvalue = iReadError();
                }
            }
            catch (Exception ee)
            {
                //PrjSystemMessageCls.PubSysMsgCls msg = new PrjSystemMessageCls.PubSysMsgCls();
                //msg.ShowMessage("80003", "连接Can总线异常 :" + ee.Message, 0, 2);
            }
            return canModelmsg.Mconnect;
            #endregion
        }

        #region EU处理
        /// <summary>
        /// 获取EU波特率.
        /// </summary>
        /// <param name="ibaudrate"></param>
        /// <returns></returns>
        private static uint getEuValueForBaudrate(uint ibaudrate)
        {
            uint irtvalue = 0;
            if (GCanBrTabValue != null)
            {
                if (GCanBrTabValue.Length > 0)
                {
                    for (uint i = 0; i < GCanBrTabValue.Length; i++)
                    {
                        if (GCanBrTabValue[i] == ibaudrate)
                        {
                            irtvalue = i;
                            break;
                        }
                    }
                }
            }
            return irtvalue;
        }
        /// <summary>
        /// 连接Can总线(125kbps)　检查当前CAN设备是否连接 并且开启CAN 返回开启CAN的状态 1为开启成功 0为开启失败.
        /// </summary>
        /// <returns>连接标示</returns>
        unsafe public static int OpenDeviceAndStartEU()
        {
            canModelmsg.IBaudrateEU = getEuValueForBaudrate(canModelmsg.IBaudrate);
            GetTime01();
            string timing0 = canModelmsg.S16Timing0;// "03";
            string timing1 = canModelmsg.S16Timing1;
            try
            {
                if (canModelmsg.Mconnect == 1)
                {
                    VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);//调用DLL 关闭CAN
                    canModelmsg.Mconnect = 0;//设置该变量为0
                    //   System.Windows.Forms.MessageBox.Show("断开成功");
                    return canModelmsg.Mconnect;
                }
                CanModelCls.VCI_INIT_CONFIG InitConfig = new CanModelCls.VCI_INIT_CONFIG();//定义CAN初始化配置结构

                InitConfig.AccCode = System.Convert.ToUInt32(canModelmsg.S16AccCode, 16);
                InitConfig.AccMask = System.Convert.ToUInt32(canModelmsg.S16AccMask, 16);
                InitConfig.Filter = (byte)canModelmsg.IFilter;//设定滤波
                InitConfig.Mode = (byte)canModelmsg.IMode;//
                InitConfig.Timing0 = System.Convert.ToByte("0x" + timing0, 16);//设定定时器1
                InitConfig.Timing1 = System.Convert.ToByte("0x" + timing1, 16);//设定定时器2
                if (VCI_OpenDevice(canModelmsg.Devtype, canModelmsg.Devind, 0) != 1)//reserverd
                {
                    uint i = VCI_OpenDevice(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.IBaudrateEU);
                    //      System.Windows.Forms.MessageBox.Show("连接失败！！请确认电脑是否已连接机器的USB线");
                    canModelmsg.Mconnect = 0;
                    return 0;
                }
                //USB-E-U 代码 
                UInt32 baud;
                baud = GCanBrTab[canModelmsg.IBaudrateEU];
                //baud = 0;
                if (VCI_SetReference(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, 0, (byte*)&baud) != STATUS_OK)
                {
                    //MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);
                    return 0;
                }
                //设置超时
                uint iouttimes = 3000;
                if (CanHelpHandCls.canModelmsg.ISendOutTimes > 0)
                {
                    iouttimes = CanHelpHandCls.canModelmsg.ISendOutTimes;
                }
                if (VCI_SetReference(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, 4, (byte*)&iouttimes) != STATUS_OK)
                {
                    //MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);
                    return 0;
                }
                if (VCI_InitCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref InitConfig) == 1)//canid此函数用以初始化指定的CAN。
                {
                    if (VCI_StartCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid) == 1)//canid此函数用以启动CAN。
                    {
                        canModelmsg.Mconnect = 1;
                    }
                }
                else
                {
                    canModelmsg.Mconnect = 0;
                    VCI_CloseDevice(canModelmsg.Devtype, canModelmsg.Devind);
                    Thread.Sleep(5000);
                    if (VCI_InitCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref InitConfig) == 1)//canid此函数用以初始化指定的CAN。
                    {
                        if (VCI_StartCAN(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid) == 1)//canid此函数用以启动CAN。
                        {
                            canModelmsg.Mconnect = 1;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
            }
            return canModelmsg.Mconnect;
        }
        #endregion

        /// <summary>
        /// 检测CAN状态.
        /// </summary>
        /// <param name="strValue">检测状态</param>
        /// <returns></returns>
        private Int32 iReadCanStatusss(ref string strValue)
        {
            try
            {
                PrjExecutiveOrder.CanModelCls.VCI_CAN_STATUS canstatus = new CanModelCls.VCI_CAN_STATUS();
                if (VCI_ReadCANStatus(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref canstatus) == 1)
                {
                    strValue = "";//canstatus.Reserved
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.ErrInterrupt, 16)) + ";";
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.regMode, 16)) + ";";
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.regStatus, 16)) + ";";
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.regALCapture, 16)) + ";";
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.regECCapture, 16)) + ";";
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.regEWLimit, 16)) + ";";
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.regRECounter, 16)) + ";";
                    strValue = strValue + strfor(System.Convert.ToString(canstatus.regTECounter, 16)) + ";";
                    if (canstatus.Reserved != null)
                    {
                        if (canstatus.Reserved.Length > 0)
                        {
                            for (int i = 0; i < canstatus.Reserved.Length; i++)
                            {
                                strValue = strValue + canstatus.Reserved[i].ToString() + ";";
                            }
                        }
                    }
                }
                else
                {
                    return 0;
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 检测CAN状态.
        /// </summary>
        /// <param name="strValue">检测状态</param>
        /// <returns></returns>
        public static Int32 iReadCanStatus(ref string strValue)
        {
            try
            {
                PrjExecutiveOrder.CanModelCls.VCI_CAN_STATUS canstatus = new CanModelCls.VCI_CAN_STATUS();

                uint returnValue = 0 ;
 
                if (_ParameterOther.CommunicationModuleType.Equals("1"))
                {
                    returnValue = VCI_ReadCANStatus(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref canstatus);
                }
                else
                {
                    ////增加了NETCAN
                    returnValue = NetCanHelpHandCls.VCI_ReadCANStatus(canModelmsg.Devtype, canModelmsg.Devind, canModelmsg.Canid, ref canstatus);
                }

                if (returnValue == 1)
                {
                    strValue = "";//canstatus.Reserved
                    strValue = strValue + "【ErrInterrupt=" + strfor(System.Convert.ToString(canstatus.ErrInterrupt, 16)) + "】;";
                    strValue = strValue + "【regMode=" + strfor(System.Convert.ToString(canstatus.regMode, 16)) + "】;";
                    strValue = strValue + "【regStatus=" + strfor(System.Convert.ToString(canstatus.regStatus, 16)) + "】;";
                    strValue = strValue + "【regALCapture=" + strfor(System.Convert.ToString(canstatus.regALCapture, 16)) + "】;";
                    strValue = strValue + "【regECCapture=" + strfor(System.Convert.ToString(canstatus.regECCapture, 16)) + "】;";
                    strValue = strValue + "【regEWLimit=" + strfor(System.Convert.ToString(canstatus.regEWLimit, 16)) + "】;";
                    strValue = strValue + "【regRECounter=" + strfor(System.Convert.ToString(canstatus.regRECounter, 16)) + "】;";
                    strValue = strValue + "【regTECounter=" + strfor(System.Convert.ToString(canstatus.regTECounter, 16)) + "】;";
                    if (canstatus.Reserved != null)
                    {
                        if (canstatus.Reserved.Length > 0)
                        {
                            for (int i = 0; i < canstatus.Reserved.Length; i++)
                            {
                                strValue = strValue + canstatus.Reserved[i].ToString() + ";";
                            }
                        }
                    }
                }
                else
                {
                    return 0;
                }
                return 1;



            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 备用参数
        /// </summary>
        public static string Parameter_Other_Model_Name = @"\Parameter_Other_Model.xml";
        /// <summary>
        /// 保存的信息
        /// </summary>
        private static string strDataSet = @"\Configer";
        /// <summary>
        /// 读取Parameter_Other_Model类信息.
        /// </summary> 
        /// <param name="strFilesName">读取文件的路径</param>
        /// <returns></returns>
        public static Parameter_Other_Model Read_Parameter_Other_Model_Data(string strFilePath)
        {
            try
            {
                string strfilesName = strFilePath + strDataSet + Parameter_Other_Model_Name;
                XmlDocument xmldoc = new XmlDocument();

                //文件夹名称 
                if (!File.Exists(strfilesName))
                {
                    return null;
                }

                //装载Xml
                xmldoc.Load(strfilesName);
                string _innerXml = xmldoc.InnerXml;

                //反序列化
                Parameter_Other_Model _resetListObj = XmlOperation.Deserialize(typeof(Parameter_Other_Model), _innerXml) as Parameter_Other_Model;


                return _resetListObj;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取定时器.
        /// </summary>
        public static void GetTime01()
        {
            switch (canModelmsg.IBaudrate)
            {
                case 5:
                    canModelmsg.S16Timing0 = "BF";
                    canModelmsg.S16Timing1 = "FF";
                    break;
                case 10:
                    canModelmsg.S16Timing0 = "31";
                    canModelmsg.S16Timing1 = "1C";
                    break;
                case 20:
                    canModelmsg.S16Timing0 = "18";
                    canModelmsg.S16Timing1 = "1C";
                    break;
                case 40:
                    canModelmsg.S16Timing0 = "87";
                    canModelmsg.S16Timing1 = "FF";
                    break;
                case 50:
                    canModelmsg.S16Timing0 = "09";
                    canModelmsg.S16Timing1 = "1C";
                    break;
                case 80:
                    canModelmsg.S16Timing0 = "83";
                    canModelmsg.S16Timing1 = "FF";
                    break;
                case 100:
                    canModelmsg.S16Timing0 = "04";
                    canModelmsg.S16Timing1 = "1C";
                    break;
                case 125:
                    canModelmsg.S16Timing0 = "03";
                    canModelmsg.S16Timing1 = "1C";
                    break;
                case 200:
                    canModelmsg.S16Timing0 = "81";
                    canModelmsg.S16Timing1 = "FA";
                    break;
                case 250:
                    canModelmsg.S16Timing0 = "01";
                    canModelmsg.S16Timing1 = "1C";
                    break;
                case 400:
                    canModelmsg.S16Timing0 = "80";
                    canModelmsg.S16Timing1 = "FA";
                    break;
                case 500:
                    canModelmsg.S16Timing0 = "00";
                    canModelmsg.S16Timing1 = "1C";
                    break;
                case 666:
                    canModelmsg.S16Timing0 = "80";
                    canModelmsg.S16Timing1 = "B6";
                    break;
                case 800:
                    canModelmsg.S16Timing0 = "00";
                    canModelmsg.S16Timing1 = "16";
                    break;
                case 1000:
                    canModelmsg.S16Timing0 = "00";
                    canModelmsg.S16Timing1 = "14";
                    break;
            }

        }
        #endregion

        #region 辅助参数
        /// <summary>
        /// 处理等待时间.
        /// </summary>
        /// <param name="iHMValue"></param>
        /// <returns></returns>
        private static Int32 iGetOuttimes(Int32 iHMValue)
        {
            Int32 irtflg = 0;

            DateTime dt = DateTime.Now.AddMilliseconds(iHMValue);
            for (; true; )
            {
                if (dt <= DateTime.Now)
                {
                    break;
                }
            }
            irtflg = 1;

            return irtflg;
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static Int32 iReadLog(string strvalue)
        {
            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            IList<string> sWriteMsg = new List<string>();//写入日志计算数据

            sWriteMsg.Add(strvalue);

            #region  写日志

            if (sWriteMsg != null)
            {
                if (sWriteMsg.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData(sWriteMsg);
                }
            }
            #endregion
            return 1;
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static Int32 iReadLog(IList<string> strvalue)
        {
            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls(); 
            #region  写日志

            if (strvalue != null)
            {
                if (strvalue.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData(strvalue);
                }
            }
            #endregion
            return 1;
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static Int32 iReadLog_2016(string strvalue, Int32 iflg)
        {
            if (iflg == 0)
            {
                return 1;
            }
            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            ordpub.FilesPath = strFilesPath;
            IList<string> sWriteMsg = new List<string>();//写入日志计算数据 
            DateTime dtnow = DateTime.Now;
            string strmsg = strvalue + "[" + dtnow.ToLongTimeString() + " " + dtnow.Millisecond.ToString().PadLeft(4, '0') + "]";
            sWriteMsg.Add(strmsg);

            #region  写日志

            if (sWriteMsg != null)
            {
                if (sWriteMsg.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData_2016(sWriteMsg);
                }
            }
            #endregion
            return 1;
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static Int32 iReadLog_2016(string strvalue, Int32 iflg,string strName)
        {
            if (iflg == 0)
            {
                return 1;
            }
            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            ordpub.FilesPath = strFilesPath;
            IList<string> sWriteMsg = new List<string>();//写入日志计算数据 
            DateTime dtnow = DateTime.Now;
            string strmsg = strvalue + "[" + dtnow.ToLongTimeString() + " " + dtnow.Millisecond.ToString().PadLeft(4, '0') + "]";
            sWriteMsg.Add(strmsg);

            #region  写日志

            if (sWriteMsg != null)
            {
                if (sWriteMsg.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData_2016(sWriteMsg,strName);
                }
            }
            #endregion
            return 1;
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static Int32 iReadLog_Time(string strvalue, Int32 iflg, string strName)
        {
            if (iflg == 0)
            {
                return 1;
            }
            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            ordpub.FilesPath = strFilesPath;
            IList<string> sWriteMsg = new List<string>();//写入日志计算数据 
            DateTime dtnow = DateTime.Now;
            string strmsg = strvalue + "[" + dtnow.ToLongTimeString() + " " + dtnow.Millisecond.ToString().PadLeft(4, '0') + "]";
            sWriteMsg.Add(strmsg);

            #region  写日志

            if (sWriteMsg != null)
            {
                if (sWriteMsg.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData_Time(sWriteMsg, strName);
                }
            }
            #endregion
            return 1;
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static Int32 iReadLog(string strvalue,Int32 iflg)
        {
            if (iflg == 0)
            {
                return 1;
            }
            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            ordpub.FilesPath = strFilesPath;
            IList<string> sWriteMsg = new List<string>();//写入日志计算数据
            DateTime dtnow = DateTime.Now;
            string strmsg = strvalue + "[" + dtnow.ToLongTimeString() + " " + dtnow .Millisecond .ToString ().PadLeft (4,'0')+ "]";
            sWriteMsg.Add(strmsg);

            #region  写日志

            if (sWriteMsg != null)
            {
                if (sWriteMsg.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData(sWriteMsg);
                }
            }
            #endregion
            return 1;
        }

        /// <summary>
        /// 获取错误信息.
        /// </summary>
        /// <returns></returns>
        public static Int32 iReadLog(IList<string> strvalue, Int32 iflg)
        {
            if (iflg == 0)
            {
                return 1;
            }
            OrderPublicFunctionCls ordpub = new OrderPublicFunctionCls();
            #region  写日志

            if (strvalue != null)
            {
                if (strvalue.Count > 0)
                {
                    ordpub.SaveLogForSendandResiveData(strvalue);
                }
            }
            #endregion
            return 1;
        }
        #endregion

        #region USBCAN调用的DLL文件中包含的方法
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref PrjExecutiveOrder.CanModelCls.VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref PrjExecutiveOrder.CanModelCls.VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, Int32 CANInd, ref PrjExecutiveOrder.CanModelCls.VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref PrjExecutiveOrder.CanModelCls.VCI_CAN_STATUS pCANStatus);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        //static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        unsafe static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, byte* pData);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ pSend, UInt32 Len);
        //[DllImport("controlcan.dll")]
        //static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ[] pSend, UInt32 Len);

        #region 20131011 修改前注释

        //[DllImport("controlcan.dll")]
        //static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref PrjExecutiveOrder.CanModelCls.VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        #endregion

        #region 20131011 修改
        [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        #endregion

        #endregion

    }
}
