using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Communication
{
    /// <summary>
    /// 获取指令集合
    /// </summary>
    public class GetOrderCls
    {
        #region 基础指令
        /// <summary>
        /// 根据ID、数据、长度、超时、名称  获取指令.
        /// 修改时间：20180115
        /// 修改原因：指令集合由串口修改为CAN.
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="iID">ID</param>
        /// <param name="iData">数据内容</param>
        /// <param name="iDataLength">长度</param>
        /// <param name="outtime">超时时间</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls getOrderModel(string strName, Int32 iID, Int32[] iData, Int32[] iDataLength, Int32[] iDataReciveLength, Int32 outtime)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32 sendLength = 0;
            sendLength = 0;// 长度为数据长度内容.
            for (int i = 0; i < iDataLength.Length; i++)
            {
                sendLength = sendLength + iDataLength[i];
            }
            byte[] sendbyte = new byte[sendLength];
            Int32 icount = 0;

            //数据
            #region 数据
            for (int i = 0; i < iDataLength.Length; i++)
            {
                if (iData.Length > i)
                {
                    if (iDataLength[i] == 1)
                    {
                        sendbyte[icount] = get1byteforint(iData[i]);
                        icount = icount + 1;
                    }
                    if (iDataLength[i] == 2)
                    {
                        //byte[] b2byte = get2byteforint(iData[i]);换成有符号整型
                        byte[] b2byte = get2byteforint16(iData[i]);

                        for (int j = 0; j < 2; j++)
                        {
                            sendbyte[icount] = b2byte[j];
                            icount = icount + 1;
                        }
                    }
                    if (iDataLength[i] == 3)
                    {
                        byte[] b3byte = get3byteforint(iData[i]);
                        for (int j = 0; j < 3; j++)
                        {
                            sendbyte[icount] = b3byte[j];
                            icount = icount + 1;
                        }
                    }
                    if (iDataLength[i] == 4)
                    {
                        //byte[] b4byte = get3byteforint(iData[i]);换成有符号整型
                        byte[] b4byte = get4byteforint32(iData[i]);
                        for (int j = 0; j < 4; j++)
                        {
                            sendbyte[icount] = b4byte[j];
                            icount = icount + 1;
                        }
                    }
                }
            }
            if (icount < 1)
            {
                return null;//没有要发送的指令.
            }
            #endregion

            //赋值
            rtnvalue.ChkResiveOrderMsg = "";
            rtnvalue.ILoopCount = 1;
            rtnvalue.IntOrderFlag = 0;
            rtnvalue.LockFlg = 0;
            rtnvalue.MarkID = 0;
            rtnvalue.MoveToPlace = 0;
            rtnvalue.OrderID = iID;
            rtnvalue.OrderInfo = new string[sendbyte.Length];// = sendbyte;
            for (int i = 0; i < sendbyte.Length; i++)
            {
                rtnvalue.OrderInfo[i] = get16StringForByte(sendbyte[i]);
            }
            rtnvalue.OrderKZZFlg = 0;
            rtnvalue.OrderMemo = "";
            rtnvalue.OrderName = strName;
            rtnvalue.OrderResiveID = iID;
            rtnvalue.OrderResiveInfo = new string[8];
            rtnvalue.OrderState = 0;
            rtnvalue.OrderSubordinate = "";
            rtnvalue.Recive_Data = new Int32[iDataReciveLength.Length];
            rtnvalue.Recive_Data_Length = iDataReciveLength;
            rtnvalue.RetMsg = "";
            rtnvalue.Savetime = 10;
            rtnvalue.SpanTime = outtime;
            rtnvalue.StrOrderFlag = "";
            rtnvalue.StrShowMessage = "";
            rtnvalue.TimeLate = 10;
            rtnvalue.Send_Data = iData;
            rtnvalue.Send_Data_Length = iDataLength;

            return rtnvalue;
        }


        /// <summary>
        /// 根据ID、数据、长度、超时、名称  获取指令.
        /// 修改时间：20180115
        /// 修改原因：指令集合由串口修改为CAN.
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="iID">ID</param>
        /// <param name="iData">数据内容</param>
        /// <param name="iDataLength">长度</param>
        /// <param name="outtime">超时时间</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls getOrderModelAkso_NoData(string strName, Int32 iID, Int32[] iData, Int32[] iDataLength, Int32[] iDataReciveLength, Int32 outtime)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32 sendLength = 0;
            sendLength = 0;// 长度为数据长度内容.
            for (int i = 0; i < iDataLength.Length; i++)
            {
                sendLength = sendLength + iDataLength[i];
            }
            byte[] sendbyte = new byte[sendLength];
            Int32 icount = 0;

            //数据
            #region 数据
            for (int i = 0; i < iDataLength.Length; i++)
            {
                if (iData.Length > i)
                {
                    if (iDataLength[i] == 1)
                    {
                        sendbyte[icount] = get1byteforint(iData[i]);
                        icount = icount + 1;
                    }
                    if (iDataLength[i] == 2)
                    {
                        //byte[] b2byte = get2byteforint(iData[i]);换成有符号整型
                        byte[] b2byte = get2byteforint16(iData[i]);

                        for (int j = 0; j < 2; j++)
                        {
                            sendbyte[icount] = b2byte[j];
                            icount = icount + 1;
                        }
                    }
                    if (iDataLength[i] == 3)
                    {
                        byte[] b3byte = get3byteforint(iData[i]);
                        for (int j = 0; j < 3; j++)
                        {
                            sendbyte[icount] = b3byte[j];
                            icount = icount + 1;
                        }
                    }
                    if (iDataLength[i] == 4)
                    {
                        //byte[] b4byte = get3byteforint(iData[i]);换成有符号整型
                        byte[] b4byte = get4byteforint32(iData[i]);
                        for (int j = 0; j < 4; j++)
                        {
                            sendbyte[icount] = b4byte[j];
                            icount = icount + 1;
                        }
                    }
                }
            }
            if (icount < 1)
            {
                return null;//没有要发送的指令.
            }
            #endregion

            //赋值
            rtnvalue.ChkResiveOrderMsg = "";
            rtnvalue.ILoopCount = 1;
            rtnvalue.IntOrderFlag = 0;
            rtnvalue.LockFlg = 0;
            rtnvalue.MarkID = 0;
            rtnvalue.MoveToPlace = 0;
            rtnvalue.OrderID = iID;
            rtnvalue.OrderInfo = new string[sendbyte.Length];// = sendbyte;
            for (int i = 0; i < sendbyte.Length; i++)
            {
                rtnvalue.OrderInfo[i] = get16StringForByte(sendbyte[i]);
            }
            rtnvalue.OrderKZZFlg = 0;
            rtnvalue.OrderMemo = "";
            rtnvalue.OrderName = strName;
            rtnvalue.OrderResiveID = iID;
            rtnvalue.OrderResiveInfo = new string[8];
            rtnvalue.OrderState = 0;
            rtnvalue.OrderSubordinate = "";
            rtnvalue.Recive_Data = new Int32[iDataReciveLength.Length];
            rtnvalue.Recive_Data_Length = iDataReciveLength;
            rtnvalue.RetMsg = "";
            rtnvalue.Savetime = 10;
            rtnvalue.SpanTime = outtime;
            rtnvalue.StrOrderFlag = "";
            rtnvalue.StrShowMessage = "";
            rtnvalue.TimeLate = 10;
            rtnvalue.Send_Data = iData;
            rtnvalue.Send_Data_Length = iDataLength;

            return rtnvalue;
        }


        #endregion

        #region 运动控制

        /// <summary>
        /// 返回运动中：快门开关、查询和查询版本.
        /// </summary>
        /// <param name="iFlag">标记：0 开门；1 关门；2 查询；3 查询版本</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Move_OCCC_Order(Int32 iFlag)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[1];
            Int32[] i_Data_Length = new Int32[1];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 1000;
            Int32 iid = 0x02;
            string strname = "";
            i_Data_Length[0] = 1;

            switch (iFlag)
            {
                case 0:
                    strname = "0x11";//A07PublicFunctionCls.GetLanguageCls.getLanguage("K1247", "开门");//  "开门";
                    i_Data[0] = 0x11;
                    break;
                case 1:
                    strname = "0x21";//A07PublicFunctionCls.GetLanguageCls.getLanguage("K1248", "关门");//"关门";
                    i_Data[0] = 0x21;
                    break;
                case 2:
                    strname = "0x12";//A07PublicFunctionCls.GetLanguageCls.getLanguage("K1002", "查询");// "查询";
                    i_Data[0] = 0x12;
                    break;
                case 3:
                    strname = "0xF2";//A07PublicFunctionCls.GetLanguageCls.getLanguage("K1256", "查询");//"查询版本";
                    i_Data[0] = 0xF2;
                    i_Data_Length_Recive = new Int32[8];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    i_Data_Length_Recive[5] = 1;
                    i_Data_Length_Recive[6] = 1;
                    i_Data_Length_Recive[7] = 1;
                    break;
            }
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="iFlag">1 X复位；2 Y复位；3 TIP复位；4 Pump复位；5 PMT复位;6 磁套；7 磁棒</param>
        /// <param name="iOffset">复位偏移</param>
        /// <param name="iSpeed">速度</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Move_Reset_OrderAkso(Int32 iFlag, Int32 iOffset, Int32 iSpeed, Int32 iMotorId)
        {

            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[4];

            Int32 outime = 3000;
            Int32 iBoardId = 0x01;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[4];
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[7];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            //old
            //1 X复位；2 Y复位；3 TIP复位；4 Pump复位；5 PMT复位;6 磁套；7 磁棒
            //X复位	0x15
            //Y复位	0x25
            //TIP复位	0x35
            //护套复位	0x45
            //磁棒复位	0xC5
            //Pump复位	0x55
            //PMT复位	0x65
            //new
            //1-PMT电机
            //2-X轴电机
            //3-转盘电机
            //4-X臂电机
            //5-Z臂电机
            //6-移液泵电机
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            i_Data[0] = 0x15;
            i_Data[1] = iMotorId;
            i_Data[2] = iOffset;
            i_Data[3] = iSpeed;
            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;


            //PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            //Int32[] i_Data = new Int32[3];
            //Int32[] i_Data_Length = new Int32[3];

            //Int32[] i_Data_Length_Recive = new Int32[2];
            //i_Data_Length_Recive[0] = 1;
            //i_Data_Length_Recive[1] = 1;

            //Int32 outime = 3000;
            //Int32 iid = 0x02;
            //string strname = "";
            //i_Data_Length[0] = 1;
            //i_Data_Length[1] = 2;
            //i_Data_Length[2] = 1;

            //switch (iFlag)
            //{
            //    case 1:
            //        strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "X复位");//"X复位";
            //        i_Data[0] = 0x15;
            //        i_Data_Length_Recive = new Int32[3];
            //        i_Data_Length_Recive[0] = 1;
            //        i_Data_Length_Recive[1] = 1;
            //        i_Data_Length_Recive[2] = 2;
            //        break;
            //    case 2:
            //        strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1474", "Y复位");// "Y复位";
            //        i_Data[0] = 0x25;
            //        i_Data_Length_Recive = new Int32[3];
            //        i_Data_Length_Recive[0] = 1;
            //        i_Data_Length_Recive[1] = 1;
            //        i_Data_Length_Recive[2] = 2;
            //        break;
            //    case 3:
            //        strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1415", "TIP复位");//"TIP复位";
            //        i_Data[0] = 0x35;
            //        break;
            //    case 4:
            //        strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1416", "Pump复位");//"Pump复位";
            //        i_Data[0] = 0x55;
            //        break;
            //    case 5:
            //        strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1417", "PMT复位");// "PMT复位";
            //        i_Data[0] = 0x65;
            //        break;
            //    case 6:
            //        strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1413", "磁套复位");//"磁套复位";
            //        i_Data[0] = 0x45;
            //        break;
            //    case 7:
            //        strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1414", "磁棒复位");// "磁棒复位";
            //        i_Data[0] = 0xC5;
            //        break;
            //}
            //i_Data[1] = iOffset;
            //i_Data[2] = iSpeed;
            //rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            //return rtnvalue;
        }

        //

        public PrjExecutiveOrder.OrderModelCls get_OrderAkso4Parameter(Int32 iOrderId, Int32 iOffset, Int32 iSpeed, Int32 iThreshold, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[5];
            Int32 outime = iOutTime;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[5];
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            i_Data_Length[4] = 1;
            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[7];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "位移");//"位移";
            i_Data[0] = iOrderId;
            i_Data[1] = iMotorId;
            i_Data[2] = iOffset;
            i_Data[3] = iSpeed;
            i_Data[4] = iThreshold;
            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }


        public PrjExecutiveOrder.OrderModelCls get_OrderAkso(Int32 iOrderId, Int32 iOffset, Int32 iSpeed, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[4];
            Int32 outime = iOutTime;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[4];
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[7];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            i_Data[0] = iOrderId;
            i_Data[1] = iMotorId;
            i_Data[2] = iOffset;
            i_Data[3] = iSpeed;
            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        public PrjExecutiveOrder.OrderModelCls get_OrderAkso(Int32 iOrderId, Int32 iOffset, Int32 iSpeed, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId, string stepName)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[4];
            Int32 outime = iOutTime;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[4];
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[7];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            //strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            strname = stepName;
            i_Data[0] = iOrderId;
            i_Data[1] = iMotorId;
            i_Data[2] = iOffset;
            i_Data[3] = iSpeed;
            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }
        //运行
        /// <summary>
        /// 添加指令到发送列表
        /// </summary>
        /// <param name="iOrderId">指令ID</param>
        /// <param name="iOffset">偏移量</param>
        /// <param name="iSpeed">速度</param>
        /// <param name="iMotorId">电机ID</param>
        /// <param name="iOutTime">超时时间</param>
        /// <param name="iBoardId">主板ID</param>
        /// <param name="stepName">步骤名称</param>
        /// <param name="iThreshold">运行阀值</param>
        /// <param name="iThresholdAD">AD阀值</param>
        public PrjExecutiveOrder.OrderModelCls get_OrderAkso(Int32 iOrderId, Int32 iOffset, Int32 iSpeed, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId, string stepName, Int32 iThreshold, Int32 iThresholdAD)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[6];
            Int32 outime = iOutTime;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[6];
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            i_Data_Length[4] = 1;
            i_Data_Length[5] = 1;
            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[7];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            //strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            strname = stepName;
            i_Data[0] = iOrderId;
            i_Data[1] = iMotorId;
            i_Data[2] = iOffset;
            i_Data[3] = iSpeed;
            i_Data[4] = iThreshold;
            i_Data[5] = iThresholdAD;
            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }


        public PrjExecutiveOrder.OrderModelCls get_OrderAksoReadCode(Int32 iOrderId, Int32 iOffset, Int32 iSpeed, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId, string stepName, Int32 iThreshold, Int32 iThresholdAD)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[3];
            Int32 outime = iOutTime;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[3];
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 1;

            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[7];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            //strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            strname = stepName;
            i_Data[0] = iOrderId;
            i_Data[1] = iMotorId;
            i_Data[2] = iOffset;

            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iOrderId"></param>
        /// <param name="iMotorId"></param>
        /// <param name="iOutTime"></param>
        /// <param name="iBoardId"></param>
        /// <returns></returns>

        public PrjExecutiveOrder.OrderModelCls get_OrderAkso_NoOffset_NoSpeed(Int32 iOrderId, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[2];
            Int32 outime = iOutTime;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[4];
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[8];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            i_Data_Length_Recive[5] = 1;
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            i_Data[0] = iOrderId;
            i_Data[1] = iMotorId;
            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }


        public PrjExecutiveOrder.OrderModelCls get_OrderAkso_NoOffset_NoSpeed(Int32[] i_SendData, Int32[] i_Data_Length, Int32[] i_Data_Length_Recive, Int32 iOutTime, Int32 iBoardId)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            //Int32[] i_Data = new Int32[2];
            Int32 outime = iOutTime;
            string strname = "";
            ////发送的数据长度格式
            //Int32[] i_Data_Length = new Int32[4];
            //i_Data_Length[0] = 1;
            //i_Data_Length[1] = 1;
            //i_Data_Length[2] = 3;
            //i_Data_Length[3] = 1;
            ////接收的数据长度格式
            //Int32[] i_Data_Length_Recive = new Int32[8];
            //i_Data_Length_Recive[0] = 1;
            //i_Data_Length_Recive[1] = 1;
            //i_Data_Length_Recive[2] = 1;
            ////3-5当前位置，用三个字节来存放。
            //i_Data_Length_Recive[3] = 3;
            //i_Data_Length_Recive[4] = 1;
            //i_Data_Length_Recive[5] = 1;
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            //i_Data[0] = iOrderId;
            //i_Data[1] = iMotorId;
            rtnvalue = getOrderModel(strname, iBoardId, i_SendData, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }


        public PrjExecutiveOrder.OrderModelCls get_OrderAkso_NoOffset_NoSpeed(Int32 iOrderId, Int32[] i_SendData, Int32[] i_Data_Length, Int32[] i_Data_Length_Recive, Int32 iMotorId, Int32 iOutTime, Int32 iBoardId)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            //Int32[] i_Data = new Int32[2];
            Int32 outime = iOutTime;
            string strname = "";
            ////发送的数据长度格式
            //Int32[] i_Data_Length = new Int32[4];
            //i_Data_Length[0] = 1;
            //i_Data_Length[1] = 1;
            //i_Data_Length[2] = 3;
            //i_Data_Length[3] = 1;
            ////接收的数据长度格式
            //Int32[] i_Data_Length_Recive = new Int32[8];
            //i_Data_Length_Recive[0] = 1;
            //i_Data_Length_Recive[1] = 1;
            //i_Data_Length_Recive[2] = 1;
            ////3-5当前位置，用三个字节来存放。
            //i_Data_Length_Recive[3] = 3;
            //i_Data_Length_Recive[4] = 1;
            //i_Data_Length_Recive[5] = 1;
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            //i_Data[0] = iOrderId;
            //i_Data[1] = iMotorId;
            rtnvalue = getOrderModel(strname, iBoardId, i_SendData, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        public PrjExecutiveOrder.OrderModelCls get_OrderAkso_NoMotor_NoOffset_NoSpeed(Int32 iOrderId, Int32 iOutTime, Int32 iBoardId)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[1];
            Int32 outime = iOutTime;
            string strname = "";
            //发送的数据长度格式
            Int32[] i_Data_Length = new Int32[1];
            i_Data_Length[0] = 1;
            //接收的数据长度格式
            Int32[] i_Data_Length_Recive = new Int32[7];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            //3-5当前位置，用三个字节来存放。
            i_Data_Length_Recive[3] = 3;
            i_Data_Length_Recive[4] = 1;
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "复位");//"X复位";
            i_Data[0] = iOrderId;
            rtnvalue = getOrderModel(strname, iBoardId, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="iFlag">1 X复位；2 Y复位；3 TIP复位；4 Pump复位；5 PMT复位;6 磁套；7 磁棒</param>
        /// <param name="iOffset">复位偏移</param>
        /// <param name="iSpeed">速度</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Move_Reset_Order(Int32 iFlag, Int32 iOffset, Int32 iSpeed)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[3];
            Int32[] i_Data_Length = new Int32[3];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 3000;
            Int32 iid = 0x02;
            string strname = "";
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 2;
            i_Data_Length[2] = 1;

            switch (iFlag)
            {
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1473", "X复位");//"X复位";
                    i_Data[0] = 0x15;
                    i_Data_Length_Recive = new Int32[3];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 2;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1474", "Y复位");// "Y复位";
                    i_Data[0] = 0x25;
                    i_Data_Length_Recive = new Int32[3];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 2;
                    break;
                case 3:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1415", "TIP复位");//"TIP复位";
                    i_Data[0] = 0x35;
                    break;
                case 4:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1416", "Pump复位");//"Pump复位";
                    i_Data[0] = 0x55;
                    break;
                case 5:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1417", "PMT复位");// "PMT复位";
                    i_Data[0] = 0x65;
                    break;
                case 6:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1413", "磁套复位");//"磁套复位";
                    i_Data[0] = 0x45;
                    break;
                case 7:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1414", "磁棒复位");// "磁棒复位";
                    i_Data[0] = 0xC5;
                    break;
            }
            i_Data[1] = iOffset;
            i_Data[2] = iSpeed;
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 复位_Z轴
        /// </summary> 
        /// <param name="iOffset">磁棒偏移</param>
        /// <param name="iSpeed">速度</param>
        /// <param name="iOffset_01">护套偏移</param> 
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Move_Reset_Z_Order(Int32 iOffset, Int32 iSpeed, Int32 iOffset_01)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[4];
            Int32[] i_Data_Length = new Int32[4];
            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 3000;
            Int32 iid = 0x02;
            string strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1475", "Z复位");//"Z复位";
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 2;
            i_Data_Length[2] = 2;
            i_Data_Length[3] = 1;

            i_Data[0] = 0x45;
            i_Data[1] = iOffset;
            i_Data[2] = iOffset_01;
            i_Data[3] = iSpeed;
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="iFlag">1 X运行；2 Y运行；3 TIP运行；4 Pump运行;5 PMT运行；6 磁棒运行；7 护套运行</param>
        /// <param name="iPlace">位置</param>
        /// <param name="iSpeed">速度</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Move_Run_Order(Int32 iFlag, Int32 iPlace, Int32 iSpeed)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[3];
            Int32[] i_Data_Length = new Int32[3];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 3000;
            Int32 iid = 0x02;
            string strname = "";
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 2;
            i_Data_Length[2] = 1;

            switch (iFlag)
            {
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1476", "X运行");// "X运行";
                    i_Data[0] = 0x75;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1477", "Y运行");//"Y运行";
                    i_Data[0] = 0x85;
                    break;
                case 3:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1400", "TIP运行");//"TIP运行";
                    i_Data[0] = 0x95;
                    break;
                case 4:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1478", "Pump运行");//"Pump运行";
                    i_Data[0] = 0xD5;
                    break;
                case 5:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1401", "PMT运行");//"PMT运行";
                    i_Data[0] = 0xE5;
                    break;
                case 6:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1479", "护套运行");// "护套运行";
                    i_Data[0] = 0xB5;
                    break;
                case 7:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1480", "磁棒运行");//  "磁棒运行";
                    i_Data[0] = 0xA5;
                    break;
            }
            i_Data[1] = iPlace;
            i_Data[2] = iSpeed;
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        ///// <summary>
        ///// 运行
        ///// </summary>
        ///// <param name="iFlag">1 X运行；2 Y运行；3 TIP运行；4 磁棒运行；5 护套运行；6 Z运行；7 Pump运行;8 PMT运行</param>
        ///// <param name="iPlace">位置</param>
        ///// <param name="iSpeed">速度</param>
        ///// <returns></returns>
        //public PrjExecutiveOrder.OrderModelCls get_Move_Run_Order(Int32 iFlag, Int32 iPlace, Int32 iSpeed)
        //{
        //    PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
        //    Int32[] i_Data = new Int32[3];
        //    Int32[] i_Data_Length = new Int32[3];
        //    Int32 outime = 3000;
        //    Int32 iid = 0x02;
        //    string strname = "";
        //    i_Data_Length[0] = 1;
        //    i_Data_Length[1] = 2;
        //    i_Data_Length[2] = 1;

        //    switch (iFlag)
        //    {
        //        case 1:
        //            strname = "X运行";
        //            i_Data[0] = 0x75;
        //            break;
        //        case 2:
        //            strname = "Y运行";
        //            i_Data[0] = 0x85;
        //            break;
        //        case 3:
        //            strname = "TIP运行";
        //            i_Data[0] = 0x95;
        //            break;
        //        case 4:
        //            strname = "磁棒运行";
        //            i_Data[0] = 0xA5;
        //            break;
        //        case 5:
        //            strname = "护套运行";
        //            i_Data[0] = 0xB5;
        //            break;
        //        case 6:
        //            strname = "Z运行";
        //            i_Data[0] = 0xC5;
        //            break;
        //        case 7:
        //            strname = "Pump运行";
        //            i_Data[0] = 0xD5;
        //            break;
        //        case 8:
        //            strname = "PMT运行";
        //            i_Data[0] = 0xE5;
        //            break;
        //    }
        //    i_Data[1] = iPlace;
        //    i_Data[2] = iSpeed;
        //    rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, outime);
        //    return rtnvalue;
        //}

        /// <summary>
        /// 吸附
        /// </summary> 
        /// <param name="iHeigth">高度</param>
        /// <param name="iSpeed">速度</param>
        /// <param name="iOutTime">延时</param> 
        /// <param name="iSpeed1">上升速度</param> 
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Move_Adsorbent_Order(Int32 iHeigth, Int32 iSpeed, Int32 iOutTime, Int32 iSpeed1, int board_id)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();

            Int32[] i_Data = new Int32[8];
            Int32[] i_Data_Length = new Int32[6];

            Int32[] i_Data_Length_Recive = new Int32[5];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 3;
            i_Data_Length_Recive[3] = 1;
            i_Data_Length_Recive[4] = 1;

            Int32 outime = 3000;
            Int32 iid = board_id;
            string strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1104", "吸附");// "吸附";
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            i_Data_Length[4] = 1;
            i_Data_Length[5] = 1;


            i_Data[0] = 0x05;
            i_Data[1] = 0x01;
            i_Data[2] = iHeigth; //振荡幅度300步起（如果小于300 则振荡300）
            i_Data[3] = iSpeed;
            i_Data[4] = iSpeed1;
            i_Data[5] = iOutTime;



            //Int32[] i_Data = new Int32[5];
            //Int32[] i_Data_Length = new Int32[5];

            //Int32[] i_Data_Length_Recive = new Int32[2];
            //i_Data_Length_Recive[0] = 1;
            //i_Data_Length_Recive[1] = 1;

            //Int32 outime = 1000 * iOutTime + iHeigth * 20;
            //Int32 iid = board_id;
            //string strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1104", "吸附");// "吸附";
            //i_Data_Length[0] = 1;
            //i_Data_Length[1] = 2;
            //i_Data_Length[2] = 1;
            //i_Data_Length[3] = 1;
            //i_Data_Length[4] = 1;

            //i_Data[0] = 0x05;
            //i_Data[1] = iHeigth;
            //i_Data[2] = iSpeed;
            //i_Data[3] = iOutTime;
            //i_Data[4] = iSpeed1;

            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 振荡
        /// </summary> 
        /// <param name="iHeigth">幅度</param>
        /// <param name="iSpeed">速度</param>
        /// <param name="iOutTime">次数</param> 
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Move_Oscillation_Order(Int32 iHeigth, Int32 iSpeed, Int32 iOutTime, int board_id)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[8];
            Int32[] i_Data_Length = new Int32[6];

            Int32[] i_Data_Length_Recive = new Int32[5];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 3;
            i_Data_Length_Recive[3] = 1;
            i_Data_Length_Recive[4] = 1;

            Int32 outime = 3000;
            Int32 iid = board_id;
            string strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1103", "震荡");// "震荡";
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;
            i_Data_Length[2] = 3;
            i_Data_Length[3] = 1;
            i_Data_Length[4] = 1;
            i_Data_Length[5] = 1;


            i_Data[0] = 0xF5;
            i_Data[1] = 0x01;
            i_Data[2] = iHeigth; //振荡幅度300步起（如果小于300 则振荡300）
            i_Data[3] = iSpeed;
            i_Data[4] = iOutTime;
            i_Data[5] = iOutTime;
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        #endregion

        #region 测读

        /// <summary>
        /// 测读模块：设置时间、设置指令、设置分辨率.
        /// </summary>
        /// <param name="iFlag">标记：0 设置返回ID；1 设置脉冲分辨时间；2 读数；3 设置测读时间</param>
        /// <param name="iReadTime">测读时间(0.01 秒)</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Read_Set_Order(Int32 iFlag, Int32 iReadTime)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[8];
            Int32[] i_Data_Length = new Int32[8];

            Int32[] i_Data_Length_Recive = new Int32[8];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            i_Data_Length_Recive[2] = 1;
            i_Data_Length_Recive[3] = 1;
            i_Data_Length_Recive[4] = 1;
            i_Data_Length_Recive[5] = 1;
            i_Data_Length_Recive[6] = 1;
            i_Data_Length_Recive[7] = 1;

            Int32 outime = 1000;
            Int32 iid = 0x54;
            string strname = "";
            for (int i = 0; i < 8; i++)
            {
                i_Data[i] = 0x00;
                i_Data_Length[i] = 1;
            }
            switch (iFlag)
            {
                case 0:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1252", "设置返回ID");// "设置返回ID";
                    i_Data[1] = 0x49;
                    i_Data[3] = 0x54;
                    break;
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1253", "设置脉冲分辨时间");// "设置脉冲分辨时间";
                    i_Data[1] = 0x43;
                    i_Data[3] = 0x11;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1254", "读数");// "读数";
                    i_Data[1] = 0x4A;
                    i_Data_Length_Recive = new Int32[5];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 4;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    break;
                case 3:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1255", "设置测读时间");// "设置测读时间";
                    i_Data = new Int32[7];
                    i_Data_Length = new Int32[7];
                    for (int i = 0; i < 7; i++)
                    {
                        i_Data[i] = 0x00;
                        i_Data_Length[i] = 1;
                    }
                    i_Data[1] = 0x54;

                    i_Data[2] = iReadTime;
                    i_Data_Length[2] = 2;
                    i_Data_Length_Recive = new Int32[7];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 2;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    i_Data_Length_Recive[5] = 1;
                    i_Data_Length_Recive[6] = 1;
                    break;
            }
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        #endregion

        #region 温育 Temperature

        /// <summary>
        /// 返回运动中：快门开关、查询和查询版本.
        /// </summary>
        /// <param name="iFlag">标记：0 打开PWM1；1 关闭PWM1；2 打开PWM2；3 关闭PWM2；4 打开PWM3；5 关闭PWM3；
        /// 6 查询当前温度;7 查询目标温度;8 查询试剂条状态;9 查询版本</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Temperature_Select_Order(Int32 iFlag)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[1];
            Int32[] i_Data_Length = new Int32[1];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 1;
            Int32 iid = 0x06;
            string strname = "";
            i_Data_Length[0] = 1;

            switch (iFlag)
            {
                case 0:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1123", "打开") + "PWM1";// "打开PWM1";
                    i_Data[0] = 0x11;
                    break;
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1119", "关闭") + "PWM1";//"关闭PWM1";
                    i_Data[0] = 0x21;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1123", "打开") + "PWM2";// "打开PWM2";
                    i_Data[0] = 0x31;
                    break;
                case 3:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1119", "关闭") + "PWM2";//"关闭PWM2";
                    i_Data[0] = 0x41;
                    break;
                case 4:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1123", "打开") + "PWM3";// "打开PWM3";
                    i_Data[0] = 0x51;
                    break;
                case 5:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1119", "关闭") + "PWM3";//"关闭PWM3";
                    i_Data[0] = 0x61;
                    break;
                case 6:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1423", "查询当前温度");//  "查询当前温度";
                    i_Data[0] = 0x12;
                    i_Data_Length_Recive = new Int32[5];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 2;
                    i_Data_Length_Recive[3] = 2;
                    i_Data_Length_Recive[4] = 2;
                    break;
                case 7:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1422", "查询目标温度");//  "查询目标温度";
                    i_Data[0] = 0x22;
                    i_Data_Length_Recive = new Int32[5];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 2;
                    i_Data_Length_Recive[3] = 2;
                    i_Data_Length_Recive[4] = 2;
                    break;
                case 8:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1424", "查询试剂条状态");// "查询试剂条状态";
                    i_Data[0] = 0x32;
                    i_Data_Length_Recive = new Int32[6];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    i_Data_Length_Recive[5] = 1;
                    break;
                case 9:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1256", "查询版本");// "查询版本";
                    i_Data[0] = 0xF2;
                    i_Data_Length_Recive = new Int32[8];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    i_Data_Length_Recive[5] = 1;
                    i_Data_Length_Recive[6] = 1;
                    i_Data_Length_Recive[7] = 1;
                    break;
            }
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 返回运动中：快门开关、查询和查询版本.
        /// </summary>
        /// <param name="iFlag">标记：0 设置目标温度；1 设置PID1；2 设置PID2；3 设置PID3</param>
        /// <param name="ip1">设置目标温度1 P参数；目标温度2 I参数；目标温度3 D参数</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Temperature_Set_Order(Int32 iFlag, Int32 ip1, Int32 ip2, Int32 ip3, Int32 board_id)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[7];
            Int32[] i_Data_Length = new Int32[4];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;
            //i_Data_Length_Recive[2] = 1;
            //i_Data_Length_Recive[3] = 1;
            //i_Data_Length_Recive[4] = 1;
            //i_Data_Length_Recive[5] = 1;
            //i_Data_Length_Recive[6] = 1;
            //i_Data_Length_Recive[7] = 1;

            Int32 outime = 1000;
            Int32 iid = board_id;

            if (board_id == 2)
            {
                iid = 0x05;
            }
            if (board_id == 3)
            {
                iid = 0x06;
            }
            if (board_id == 4)
            {
                iid = 0x07;
            }

            string strname = "";
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 2;
            i_Data_Length[2] = 2;
            i_Data_Length[3] = 2;

            switch (iFlag)
            {
                case 0:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1048", "设置目标温度");// "设置目标温度";
                    i_Data[0] = 0x13;
                    break;
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1257", "设置") + "PID1";// "设置PID1";
                    i_Data[0] = 0x23;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1257", "设置") + "PID2";//"设置PID2";
                    i_Data[0] = 0x33;
                    break;
                case 3:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1257", "设置") + "PID3";//"设置PID3";
                    i_Data[0] = 0x43;
                    break;
            }
            i_Data[1] = ip1;
            i_Data[2] = ip2;
            i_Data[3] = ip3;
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }
        #endregion

        #region 开关检测

        /// <summary>
        /// 开关检测.
        /// </summary>
        /// <param name="iFlag">标记：0 查询状态；1 查询版本；2 蜂鸣器控制</param>
        /// <param name="iTimes">次数</param>
        /// <param name="b_time">开启</param>
        /// <param name="c_time">关闭</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Open_Close_Order(Int32 iFlag, Int32 iTimes, Int32 b_time, Int32 c_time, int board_id)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[1];
            Int32[] i_Data_Length = new Int32[1];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 1000;
            Int32 iid = board_id;
            string strname = "";
            i_Data_Length[0] = 1;

            switch (iFlag)
            {
                case 0:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1126", "查询状态");// "查询状态";
                    i_Data[0] = 0x12;
                    i_Data_Length_Recive = new Int32[5];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    break;
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1256", "查询版本");// "查询版本";
                    i_Data[0] = 0xF2;
                    i_Data_Length_Recive = new Int32[8];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    i_Data_Length_Recive[5] = 1;
                    i_Data_Length_Recive[6] = 1;
                    i_Data_Length_Recive[7] = 1;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1258", "蜂鸣器控制");// "蜂鸣器控制";
                    i_Data = new Int32[5];
                    i_Data_Length = new Int32[5];
                    i_Data_Length[0] = 1;
                    i_Data_Length[1] = 1;
                    i_Data_Length[2] = 1;
                    i_Data_Length[3] = 1;
                    i_Data_Length[4] = 1;

                    i_Data[0] = 0x2C;
                    i_Data[1] = 0x00;
                    i_Data[2] = iTimes;
                    i_Data[3] = b_time;
                    i_Data[4] = c_time;

                    i_Data[2] = c_time;
                    i_Data[3] = b_time;
                    i_Data[4] = iTimes;
                    iid = 0x01;
                    break;
                case 3:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1481", "同轴光源开启");// "同轴光源开启";
                    i_Data[0] = 0x21;
                    i_Data_Length_Recive = new Int32[2];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    break;
                case 4:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1482", "同轴光源关闭");//"同轴光源关闭";
                    i_Data[0] = 0x31;
                    i_Data_Length_Recive = new Int32[2];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    break;
            }
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        #endregion

        #region Scann 扫描
        /// <summary>
        /// 二维码扫描.
        /// </summary>
        /// <param name="iFlag">标记：0 查询版本；1 恢复默认设置</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Scann_Select_Vision_Order(Int32 iFlag)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[1];
            Int32[] i_Data_Length = new Int32[1];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;


            Int32 outime = 1000;
            Int32 iid = 0x07;
            string strname = "";
            i_Data_Length[0] = 1;

            switch (iFlag)
            {
                case 0:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1256", "查询版本");// "查询版本";
                    i_Data[0] = 0xF2;
                    i_Data_Length_Recive = new Int32[8];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    i_Data_Length_Recive[5] = 1;
                    i_Data_Length_Recive[6] = 1;
                    i_Data_Length_Recive[7] = 1;
                    break;
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1259", "恢复默认设置");//  "恢复默认设置";
                    i_Data[0] = 0x23;
                    break;
            }
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 二维码扫描 读码传输.
        /// </summary>
        /// <param name="iFlag">标记：0 查询；1 读码；2 数据传输</param>
        /// <param name="iP1">参数1:延时，起始地址</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Scann_Select_Order(Int32 iFlag, Int32 iP1)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[2];
            Int32[] i_Data_Length = new Int32[2];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 1000;
            Int32 iid = 0x07;
            string strname = "";
            i_Data_Length[0] = 1;
            i_Data_Length[1] = 1;

            switch (iFlag)
            {
                case 0:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1002", "查询");// 
                    i_Data[0] = 0x12;
                    break;
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1260", "读码");// 
                    i_Data[0] = 0x22;
                    i_Data_Length_Recive = new Int32[3];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1261", "数据传输");//  
                    i_Data[0] = 0x32;
                    i_Data_Length_Recive = new Int32[8];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    i_Data_Length_Recive[2] = 1;
                    i_Data_Length_Recive[3] = 1;
                    i_Data_Length_Recive[4] = 1;
                    i_Data_Length_Recive[5] = 1;
                    i_Data_Length_Recive[6] = 1;
                    i_Data_Length_Recive[7] = 1;
                    break;
            }
            i_Data[1] = iP1;
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        /// <summary>
        /// 二维码扫描 设置.
        /// </summary>
        /// <param name="iFlag">参数</param>
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_Scann_Set_Order(Int32[] iFlag)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[1 + iFlag.Length];
            Int32[] i_Data_Length = new Int32[1 + iFlag.Length];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;

            Int32 outime = 1000;
            Int32 iid = 0x07;
            string strname = "";
            i_Data_Length[0] = 1;
            strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1268", "设置");// 
            i_Data[0] = 0x13;
            for (int i = 0; i < iFlag.Length; i++)
            {
                i_Data_Length[i + 1] = 1;
                i_Data[i + 1] = iFlag[i];
            }
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        #endregion

        #region USB CAN 指令

        /// <summary>
        /// 二维码扫描 读码传输.
        /// </summary>
        /// <param name="iFlag">标记：0 重复发送上条指令（校验和出错）；1 当前指令执行状态（上位机超时）；2 重复发送当前指令（超时）</param> 
        /// <returns></returns>
        public PrjExecutiveOrder.OrderModelCls get_USB_Can_Order(Int32 iFlag)
        {
            PrjExecutiveOrder.OrderModelCls rtnvalue = new PrjExecutiveOrder.OrderModelCls();
            Int32[] i_Data = new Int32[1];
            Int32[] i_Data_Length = new Int32[1];

            Int32[] i_Data_Length_Recive = new Int32[2];
            i_Data_Length_Recive[0] = 1;
            i_Data_Length_Recive[1] = 1;


            Int32 outime = 1000;
            Int32 iid = 0x07;
            string strname = "";
            i_Data_Length[0] = 1;

            switch (iFlag)
            {
                case 0:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1483", "重复发送上条指令");// 
                    i_Data[0] = 0x11;
                    break;
                case 1:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1484", "当前指令执行状态");// 
                    i_Data[0] = 0x12;
                    i_Data_Length_Recive = new Int32[2];
                    i_Data_Length_Recive[0] = 1;
                    i_Data_Length_Recive[1] = 1;
                    break;
                case 2:
                    strname = ""; //A07PublicFunctionCls.GetLanguageCls.getLanguage("K1485", "重复发送当前指令");// 
                    i_Data[0] = 0x13;
                    break;
            }
            rtnvalue = getOrderModel(strname, iid, i_Data, i_Data_Length, i_Data_Length_Recive, outime);
            return rtnvalue;
        }

        #endregion


        #region 数据类型转换
        #region 有符号
        /// <summary>
        /// 将二个整型转换为两个字节
        /// </summary>
        /// <param name="isetvalue"></param>
        /// <returns></returns>
        public byte[] get2byteforint16(Int32 isetvalue)
        {
            byte[] rtnvalue = new byte[2];
            Int16 isetv = (Int16)isetvalue;
            byte[] rtnvalue1 = BitConverter.GetBytes(isetv);
            //rtnvalue = BitConverter.GetBytes(isetv);

            rtnvalue[0] = rtnvalue1[1];
            rtnvalue[1] = rtnvalue1[0];
            return rtnvalue;
        }
        /// <summary>
        /// 将二个整型转换为两个字节
        /// </summary>
        /// <param name="isetvalue"></param>
        /// <returns></returns>
        public byte[] get4byteforint32(Int32 isetvalue)
        {
            byte[] rtnvalue = new byte[4];
            rtnvalue = BitConverter.GetBytes(isetvalue);
            return rtnvalue;
        }
        #endregion

        #region 字节与整型间的转换
        /// <summary>
        /// 将一个整型转换为一个字节
        /// </summary>
        /// <param name="isetvalue"></param>
        /// <returns></returns>
        private byte get1byteforint(Int32 isetvalue)
        {
            byte rtnvalue = new byte();
            rtnvalue = Convert.ToByte(isetvalue % 256);
            return rtnvalue;
        }

        /// <summary>
        /// 将二个整型转换为两个字节
        /// </summary>
        /// <param name="isetvalue"></param>
        /// <returns></returns>
        private byte[] get2byteforint(Int32 isetvalue)
        {
            byte[] rtnvalue = new byte[2];
            Int32 iv1 = isetvalue / 256;
            Int32 iv2 = isetvalue % 256;
            rtnvalue[0] = Convert.ToByte(iv1 % 256);
            rtnvalue[1] = Convert.ToByte(iv2 % 256);
            return rtnvalue;
        }

        /// <summary>
        /// 将三个整型转换为两个字节
        /// </summary>
        /// <param name="isetvalue"></param>
        /// <returns></returns>
        private byte[] get3byteforint(Int32 isetvalue)
        {
            byte[] rtnvalue = new byte[3];
            if (isetvalue < 0)
            {
                isetvalue = Math.Abs(isetvalue);
                Int32 iv1 = (isetvalue / (256 * 256));
                iv1 = (isetvalue / (256 * 256)) | 128;
                Int32 iv2 = (isetvalue - iv1) / 256;
                Int32 iv3 = isetvalue % 256;

                rtnvalue[0] = Convert.ToByte(iv1 % 256);
                rtnvalue[1] = Convert.ToByte(iv2 % 256);
                rtnvalue[2] = Convert.ToByte(iv3 % 256);
            }
            else
            {
                Int32 iv1 = isetvalue / (256 * 256);
                Int32 iv2 = (isetvalue - iv1) / 256;
                Int32 iv3 = isetvalue % 256;

                rtnvalue[0] = Convert.ToByte(iv1 % 256);
                rtnvalue[1] = Convert.ToByte(iv2 % 256);
                rtnvalue[2] = Convert.ToByte(iv3 % 256);
            }





            return rtnvalue;
        }

        /// <summary>
        /// 将四个整型转换为两个字节
        /// </summary>
        /// <param name="isetvalue"></param>
        /// <returns></returns>
        private byte[] get4byteforint(Int32 isetvalue)
        {
            byte[] rtnvalue = new byte[4];
            Int32 iv1 = isetvalue / (256 * 256 * 256);
            Int32 iv2 = (isetvalue - iv1) / (256 * 256);
            Int32 iv3 = (isetvalue - iv1 - iv2) / (256);
            Int32 iv4 = isetvalue % 256;

            rtnvalue[0] = Convert.ToByte(iv1 % 256);
            rtnvalue[1] = Convert.ToByte(iv2 % 256);
            rtnvalue[2] = Convert.ToByte(iv3 % 256);
            rtnvalue[3] = Convert.ToByte(iv4 % 256);
            return rtnvalue;
        }
        #endregion

        #region  字节与汉字间的转换
        /// <summary>
        /// 将16进制保存的字节转换为汉字并返回
        /// </summary>
        /// <param name="strvalue"></param>
        /// <returns></returns>
        public string strChineseFor16String(string strvalue)
        {
            string rtnvalue = "";

            #region  将二维码字符串转换为行子
            byte[] getbytevalue = new byte[strvalue.Length / 2];
            Int32 imm = 0;
            for (int i = 0; i < strvalue.Length / 2; i++)
            {
                string strnext1 = strvalue.Substring(i * 2, 2);
                if (strnext1 == "MM")
                {
                    break;
                }
                getbytevalue[i] = getByteFor16String(strnext1);
                imm = imm + 1;
            }
            if (imm > 0)
            {
                byte[] setvyes = new byte[imm];
                for (int i = 0; i < imm; i++)
                {
                    setvyes[i] = getbytevalue[i];
                }
                rtnvalue = getString_Byte(setvyes);
            }
            #endregion

            return rtnvalue;
        }
        /// <summary>
        /// 读取对应的字节数据
        /// </summary>
        /// <param name="strvalue"></param>
        /// <returns></returns>
        public byte[] getByte_String(string strvalue)
        {
            byte[] rtnvalue = new byte[1];
            rtnvalue = Encoding.Unicode.GetBytes(strvalue);
            return rtnvalue;
        }

        /// <summary>
        /// 将字节数据转换为字符
        /// </summary>
        /// <param name="strvalue"></param>
        /// <returns></returns>
        public string getString_Byte(byte[] strvalue)
        {
            string rtnvalue = "";
            rtnvalue = Encoding.Unicode.GetString(strvalue);
            return rtnvalue;
        }

        /// <summary>
        /// 讲一个字符串按ASCII转换为字节
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public byte[] get_Byte_For_ASCII(string setvalue)
        {
            byte[] getvalue = Encoding.Default.GetBytes(setvalue);
            return getvalue;
        }
        /// <summary>
        /// 将一组字节数组转换成字ASCII字符串.
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public string get_ASCII_For_Byte(byte[] setvalue)
        {
            string getvalue = Encoding.Default.GetString(setvalue);
            return getvalue;
        }
        #endregion

        #region 字节与16进制字符串间的转换
        /// <summary>
        /// 将字节转换为双字节16进制
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public string get16StringForByte(byte setvalue)
        {
            string rtnvalue = "";
            string strv = Convert.ToString(setvalue, 16);
            rtnvalue = strv.PadLeft(2, '0');
            return rtnvalue;
        }

        /// <summary>
        /// 将16进制的字符串，转换为字节
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public byte getByteFor16String(string setvalue)
        {
            byte rtnvalue = 0;
            rtnvalue = Convert.ToByte(setvalue, 16);
            return rtnvalue;
        }
        #endregion

        #region 字节与Double型之间的转换

        /// <summary>
        /// 将16进制保存的字节转换为双精度并返回
        /// </summary>
        /// <param name="strvalue"></param>
        /// <returns></returns>
        public double getDoubleFor16String(string strvalue)
        {
            double rtnvalue = 0;
            byte[] getbytevalue = new byte[strvalue.Length / 2];
            Int32 imm = 0;
            for (int i = 0; i < strvalue.Length / 2; i++)
            {
                string strnext1 = strvalue.Substring(i * 2, 2);
                if (strnext1 == "MM")
                {
                    break;
                }
                getbytevalue[i] = getByteFor16String(strnext1);
                imm = imm + 1;
            }
            if (imm > 0)
            {
                byte[] setvyes = new byte[imm];
                for (int i = 0; i < imm; i++)
                {
                    setvyes[i] = getbytevalue[i];
                }
                rtnvalue = getDoubleForByte(setvyes);
            }
            return rtnvalue;
        }
        /// <summary>
        /// 将Double型转换为字节数组
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public byte[] getByteForDouble(double setvalue)
        {
            byte[] rtnvalue = new byte[4];
            rtnvalue = BitConverter.GetBytes(setvalue);

            return rtnvalue;
        }

        /// <summary>
        /// 将字节转换为Double
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public double getDoubleForByte(byte[] setvalue)
        {
            double rtnvalue = 0;
            rtnvalue = BitConverter.ToDouble(setvalue, 0);
            return rtnvalue;
        }
        #endregion

        #region 字节与float型之间的转换

        /// <summary>
        /// 返回浮点型对应的16进制字节字符串
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public string get16StringForFloat(float setvalue)
        {
            string rtnvalue = "";
            byte[] pbytevalue = getByteForFloat(setvalue);

            for (int i = 0; i < pbytevalue.Length; i++)
            {
                string strv1 = get16StringForByte(pbytevalue[i]);
                rtnvalue = rtnvalue + strv1;
            }
            if (rtnvalue.Length < 8)
            {
                rtnvalue = rtnvalue.PadRight(8, 'M');
            }
            return rtnvalue;
        }

        /// <summary>
        /// 将16进制保存的字节转换为双精度并返回
        /// </summary>
        /// <param name="strvalue"></param>
        /// <returns></returns>
        public float getFloatFor16String(string strvalue)
        {
            float rtnvalue = 0;
            byte[] getbytevalue = new byte[strvalue.Length / 2];
            Int32 imm = 0;
            for (int i = 0; i < strvalue.Length / 2; i++)
            {
                string strnext1 = strvalue.Substring(i * 2, 2);
                if (strnext1 == "MM")
                {
                    break;
                }
                getbytevalue[i] = getByteFor16String(strnext1);
                imm = imm + 1;
            }
            if (imm > 0)
            {
                byte[] setvyes = new byte[imm];
                for (int i = 0; i < imm; i++)
                {
                    setvyes[i] = getbytevalue[i];
                }
                rtnvalue = getFloatForByte(setvyes);
            }
            return rtnvalue;
        }

        /// <summary>
        /// 将Double型转换为字节数组
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public byte[] getByteForFloat(float setvalue)
        {
            byte[] rtnvalue = new byte[4];
            rtnvalue = BitConverter.GetBytes(setvalue);

            return rtnvalue;
        }

        /// <summary>
        /// 将字节转换为Double
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public float getFloatForByte(byte[] setvalue)
        {
            float rtnvalue = 0;
            rtnvalue = BitConverter.ToSingle(setvalue, 0);
            return rtnvalue;
        }
        #endregion

        #region 基础数据转换
        /// <summary>
        /// 将符合数据转换为十进制的整数
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public Int32 getInt32(object setvalue)
        {
            try
            {
                Int32 irtnvalue = Convert.ToInt32(setvalue);

                return irtnvalue;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 转换为浮点型
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public float getFloat(object setvalue)
        {
            try
            {
                return Convert.ToSingle(setvalue);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 转换为双精度型
        /// </summary>
        /// <param name="setvalue"></param>
        /// <returns></returns>
        public double getDouble(object setvalue)
        {
            try
            {
                return Convert.ToDouble(setvalue);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 将字符串转换日期
        /// </summary>
        /// <param name="str8string"></param>
        /// <returns></returns>
        public DateTime getDate(string str8string)
        {
            DateTime dtrtnvalue = DateTime.Now;
            try
            {
                string strvalue = str8string.Substring(0, 4) + "-" + str8string.Substring(4, 2) + "-" + str8string.Substring(6, 2);
                dtrtnvalue = Convert.ToDateTime(strvalue);
            }
            catch
            {
            }

            return dtrtnvalue;
        }

        ///// <summary>
        ///// 获取16进制字符串.
        ///// </summary>
        ///// <param name="setvalue">自己</param>
        ///// <returns></returns>
        //public string get16StringForByte(byte setvalue)
        //{
        //    string rtnvalue = "";
        //    rtnvalue = Convert.ToString(setvalue, 16).PadLeft(2, '0');
        //    return rtnvalue;
        //}
        #endregion

        #endregion
    }
}
