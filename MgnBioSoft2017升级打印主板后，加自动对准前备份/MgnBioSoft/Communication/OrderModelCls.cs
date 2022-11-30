using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Model
{
    /// <summary>
    /// .
    /// Description :   指令实体类.
    /// date      :   2021-07-26
    /// coding      :   linshi
    /// </summary>
    public class OrderModelClsLS
    {

        private Int32 pMarkID = 0;
        /// <summary>
        /// 标识id:指令标志，默认0
        /// </summary>
        public Int32 MarkID
        {
            get { return pMarkID; }
            set { pMarkID = value; }
        }

        //int motor_id, string step_name, int offset, int speed,string order_content

        private int motor_id;

        public int Motor_id
        {
            get { return motor_id; }
            set { motor_id = value; }
        }

        private string step_name;

        public string Step_name
        {
            get { return step_name; }
            set { step_name = value; }
        }

        private int offset;

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        private int speed;

        public int Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        private byte[] send_order;

        public byte[] Send_order_a
        {
            get { return send_order; }
            set { send_order = value; }
        }

        private string hole;

        public string Hole
        {
            get { return hole; }
            set { hole = value; }
        }

        /// <summary>
        /// 接收到响应数据
        /// </summary>
        private byte[] receive_data;

        public byte[] Receive_data
        {
            get { return receive_data; }
            set { receive_data = value; }
        }

        /// <summary>
        /// 重发次数
        /// </summary>
        private int re_send_number;

        public int Re_send_number
        {
            get { return re_send_number; }
            set { re_send_number = value; }
        }


        private Int32 pOrderID = 0;
        /// <summary>
        /// 指令ID：对应ID信息，默认0.
        /// </summary>
        public Int32 OrderID
        {
            get { return pOrderID; }
            set { pOrderID = value; }
        }

        private Int32 pOrderKZZFlg = 0;
        /// <summary>
        /// 扩展帧标志 1 扩展帧，0 非扩展帧.
        /// </summary>
        public Int32 OrderKZZFlg
        {
            get { return pOrderKZZFlg; }
            set { pOrderKZZFlg = value; }
        }

        private Int32 pOrderResiveID = 0;
        /// <summary>
        /// 返回指令ID：对应ID信息，默认0.
        /// </summary>
        public Int32 OrderResiveID
        {
            get { return pOrderResiveID; }
            set { pOrderResiveID = value; }
        }

        private string pstrOrderFlag = "";
        /// <summary>
        /// 指令分类：.
        /// S_T     样品液面探测 0xx2；.
        /// F_T     试剂液面探测 0xx3；.
        /// P_P     电源板             电源开 0x11 电源关 0x21；P_C 清洗泵开 0x31 清洗泵关 0x41；P_F 废液自排阀开 0x51 废液自排阀关 0x61；P_Q 气泵开 0x71 气泵关 0x81；P_S 电源查询 0x12；.
        /// R_S     测读模块           查询指令 0x12
        /// S_I     样品架进样         查询指令 0x12；复位		0x15；退样		0x25；进样		0x35
        /// S_O     样品架送出         查询指令		0x12；复位		0x15；送出		0x25
        /// S_S     样品条码扫描       查询指令		0x12；条码扫描		0x22；复位		0x15；X-		0x25；X+		0x35
        /// T_I     TIP盒进样YZ轴      查询指令		0x12；Z复位		0x15；Z-		0x25；Z+		0x35；Y复位		0x15；Y退样		0x25；Y进样		0x35
        /// T_A_X   TIP取放加样臂X轴    查询指令		0x12；复位		0x15；X-		0x25；X+		0x35
        /// T_A_Z   TIP取放加样臂Z轴    查询指令		0x12；复位		0x15；Z-		0x25；Z+		0x35；液面探测		0x85
        /// S_P     样品加样泵           查询指令		0x12；阀复位		0x15；阀运行至1		0x25；阀运行至2		0x35；阀运行至3		0x45；柱塞复位		0x55；柱塞推		0x65；柱塞拉		0x75；.
        /// R_I_YZ  反应杯进样YZ轴        查询指令		0x12；Z复位		0x15；Z-		0x25；Z+		0x35；Y复位		0x15；Y退样		0x25；Y进样		0x35
        /// A_A_X   反应杯取放加样臂1X轴     查询指令		0x12；复位		0x15；X-		0x25；X+		0x35；.
        /// A_A_Z   反应杯取放加样臂1Z轴     查询指令		0x12；电磁铁开		0x11；电磁铁关		0x21；Z复位		0x15；Z-		0x25；Z+		0x35；取放		0x45
        /// F_S     试剂条码扫描          查询指令		0x12；条码扫描		0x22；复位		0x15；X-		0x25；X+		0x35；.
        /// F_Z     试剂振荡            查询指令		0x12；复位		0x15；振荡		0x25；.
        /// F_A_X   试剂加样臂X轴     查询指令		0x12；复位		0x15；X-		0x25；X+		0x35；.
        /// F_A_YZ  试剂加样臂YZ轴    查询指令		0x12；Z复位		0x15；Z-		0x25；Z+		0x35；Y复位		0x45；Y-		0x55；Y+		0x65；Y复位偏移		0x75；液面探测		0x85；.
        /// F_A_T   试剂加样泵   查询指令		0x12；阀复位		0x15；阀运行至1		0x25；阀运行至2		0x35；阀运行至3		0x45；柱塞复位		0x55；柱塞推		0x65；柱塞拉		0x75；.
        /// T_R     温育盘运动Y      查询指令		0x12；复位		0x15；Y-		0x25；Y+		0x35；振荡		0x45；.
        /// Z_L     制冷      温育/制冷开		0x11；温育/制冷关		0x21；查询指令		0x12；设置目标温度		0x13；设置保护温度		0x23；.
        /// Z_T     温育        温育/制冷开		0x11；温育/制冷关		0x21；查询指令		0x12；设置目标温度		0x13；设置保护温度		0x23；.
        /// A_B_X   反应杯取放加样臂2X轴     查询指令		0x12；复位		0x15；X-		0x25；X+		0x35；.
        /// A_B_Z   反应杯取放加样臂2Z轴     查询指令		0x12；电磁铁开		0x11；电磁铁关		0x21；Z复位		0x15；Z-		0x25；Z+		0x35；取放		0x45
        /// W_R     洗板盘旋转       查询指令		0x12；复位		0x15；旋转		0x25
        /// W_R_Z   吸液针旋转/Z轴    查询指令		0x12；Z复位		0x15；Z-		0x25；Z+		0x35；旋转复位		0x45；-		0x55；+		0x65；吸液	.
        /// W_Z_P   洗板注液泵（蠕动泵） 查询指令		0x12；注液		0x15；.
        /// R_R     测读盘旋转       查询指令		0x12；复位		0x15；反转		0x25；正转		0x35；.
        /// D_A_P   底物加样泵       查询指令		0x12；复位		0x15；推		0x25；拉		0x35
        /// </summary>
        public string StrOrderFlag
        {
            get { return pstrOrderFlag; }
            set { pstrOrderFlag = value; }
        }

        private Int32 pintOrderFlag = 0;
        /// <summary>
        /// 指令分类：.
        /// 1     辅助部分回收的指令；
        /// 2     测读回收的指令；
        /// 3     样品架进入、送出、扫描.
        /// 4     TIP盒进样和送出（Y、Z）.
        /// 5     反应杯进样和送出（Y、Z）.
        /// 6     加样A部分回收的指令（TIP头加样）
        /// 7    机器手A部分回收的指令（运动（X、Z）、夹取（夹、取）、光电开关检测等）.
        /// 8    试剂条码扫描回收的指令 
        /// 9   试剂震荡回收的指令.
        /// 10   加样B部分回收的指令（加试剂）
        /// 11     温育盘运动回收的指令
        /// 12  温育制冷回收的指令.
        /// 13   机器手B部分回收的指令（运动（X、Z）、夹取（夹、取））
        /// 14   洗板盘旋转回收的指令 
        /// 15     洗板回收的指令 )	吸液针（旋转、Z轴、吸液、洗针）、注液针（注液）
        /// 16    测读旋转回收的指令.
        /// 17   底物加样泵A回收的指令.
        /// 18  底物加样泵B回收的指令.
        /// 0   如果不是需要的则为非正常回收指令.
        /// </summary>
        public Int32 IntOrderFlag
        {
            get { return pintOrderFlag; }
            set { pintOrderFlag = value; }
        }

        private string pOrderName = "";
        /// <summary>
        /// 指令名称：对应的名称，用于显示.
        /// </summary>
        public string OrderName
        {
            get { return pOrderName; }
            set { pOrderName = value; }
        }

        private string[] pOrderInfo;
        /// <summary>
        /// 指令内容：发送的指令信息，为16进制 ；最大长度为8
        /// </summary>
        public string[] OrderInfo
        {
            get { return pOrderInfo; }
            set { pOrderInfo = value; }
        }

        private string[] pOrderResiveInfo = { "", "", "", "", "", "", "", "" };
        /// <summary>
        /// 返回的指令内容：发送的指令信息，为16进制 ；最大长度为8
        /// </summary>
        public string[] OrderResiveInfo
        {
            get { return pOrderResiveInfo; }
            set { pOrderResiveInfo = value; }
        }

        /// <summary>
        /// 运行耗时的操作，0为不需要中断结果，1为需要中断结果查询
        /// </summary>
        private int RunLongTimeAct = 0;

        public int RunLongTimeAct1
        {
            get { return RunLongTimeAct; }
            set { RunLongTimeAct = value; }
        }

        private Int32 pOrderState = 1;
        /// <summary>
        /// 指令状态：默认为1 为等待发送状态；2 发送完成等待接收状态；9接收完成状态


        /// </summary>
        public Int32 OrderState
        {
            get { return pOrderState; }
            set { pOrderState = value; }
        }

        private Int32 pSpanTime = 20;
        /// <summary>
        /// 指令耗时(当前指令预计需要的时间）.
        /// </summary>
        public Int32 SpanTime
        {
            get { return pSpanTime; }
            set { pSpanTime = value; }
        }

        private Int32 pTimeLate = 0;
        /// <summary>
        /// 指令延时（当前指令完成接收后等待的时间然后在发送下一条）
        /// </summary>
        public Int32 TimeLate
        {
            get { return pTimeLate; }
            set { pTimeLate = value; }
        }

        private Int32 pSavetime = 0;
        /// <summary>
        /// 指令保护时间（如果有多条需要发送的数据，发送当前指令后等待时间后再发送下一条指令）
        /// </summary>
        public Int32 Savetime
        {
            get { return pSavetime; }
            set { pSavetime = value; }
        }

        private string pStrShowMessage = "";
        /// <summary>
        /// 返回信息：检测返回内容：最大为8位16进制数据
        /// </summary>
        public string StrShowMessage
        {
            get { return pStrShowMessage; }
            set { pStrShowMessage = value; }
        }

        private string pOrderSubordinate = "";
        /// <summary>
        /// 指令对应的项目名称.
        /// </summary>
        public string OrderSubordinate
        {
            get { return pOrderSubordinate; }
            set { pOrderSubordinate = value; }
        }

        private Int32 pLockFlg = 0;
        /// <summary>
        /// 锁定标识：1 锁定了；0 没有锁定
        /// </summary>
        public Int32 LockFlg
        {
            get { return pLockFlg; }
            set { pLockFlg = value; }
        }

        private double pMoveToPlace = 0;
        /// <summary>
        /// 本指令欲达到的位置：预计运行到某个位置.
        /// </summary>
        public double MoveToPlace
        {
            get { return pMoveToPlace; }
            set { pMoveToPlace = value; }
        }

        private string pOrderMemo = "";
        /// <summary>
        /// 指令备注：备注信息.
        /// </summary>
        public string OrderMemo
        {
            get { return pOrderMemo; }
            set { pOrderMemo = value; }
        }

        private string pChkResiveOrderMsg = "01";
        /// <summary>
        /// 检查返回信息：检测返回指令信息的正确性.
        /// </summary>
        public string ChkResiveOrderMsg
        {
            get { return pChkResiveOrderMsg; }
            set { pChkResiveOrderMsg = value; }
        }

        private string pRetMsg = "";
        /// <summary>
        /// 返回的信息：返回的异常信息.
        /// </summary>
        public string RetMsg
        {
            get { return pRetMsg; }
            set { pRetMsg = value; }
        }


        private Int32 iLoopCount = 0;
        /// <summary>
        /// 循环发送次数:2013 10 31 增加修改
        /// </summary>
        public Int32 ILoopCount
        {
            get { return iLoopCount; }
            set { iLoopCount = value; }
        }


        private Int32[] _Send_Data = new Int32[1];
        /// <summary>
        /// 发送数据内容：整型存在的内容（未转化前）.
        /// </summary>
        public Int32[] Send_Data
        {
            get { return _Send_Data; }
            set { _Send_Data = value; }
        }

        private Int32[] _Send_Data_Length = new Int32[1];
        /// <summary>
        /// 发送数据长度：数据长度（占用字节是）.
        /// </summary>
        public Int32[] Send_Data_Length
        {
            get { return _Send_Data_Length; }
            set { _Send_Data_Length = value; }
        }

        private Int32[] _Recive_Data = new Int32[1];
        /// <summary>
        /// 接收数据内容：整型存在的内容（转化后：去掉了帧头、长度、ID、和校验和）.
        /// </summary>
        public Int32[] Recive_Data
        {
            get { return _Recive_Data; }
            set { _Recive_Data = value; }
        }

        private byte[] _Recive_Data_Byte = new byte[8];
        /// <summary>
        /// 接收数据内容：整型存在的内容（转化后：去掉了帧头、长度、ID、和校验和）.
        /// </summary>
        public byte[] Recive_Data_Byte
        {
            get { return _Recive_Data_Byte; }
            set { _Recive_Data_Byte = value; }
        }


        private Int32[] _Recive_Data_Length = new Int32[1];
        /// <summary>
        /// 接收数据长度：数据长度（占用字节是）.
        /// </summary>
        public Int32[] Recive_Data_Length
        {
            get { return _Recive_Data_Length; }
            set { _Recive_Data_Length = value; }
        }

    }

}
