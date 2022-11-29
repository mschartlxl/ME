
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace ME.BaseCore
{
    /// <summary>
    /// 框架全局变量类型
    /// </summary>
    public class ABTGlobal
    {
        private static ABTGlobal _instance;
        public static ABTGlobal Instance
        {
            get
            {
                if(null== _instance)
                {
                    _instance = new ABTGlobal();
                }
                return _instance;
            }
        }

        private ABTGlobal()
        { }

        #region   实例属性

        

   
        private bool _IsRun = false;
        /// <summary>
        /// 程序开始执行
        /// </summary>
        public bool IsRun
        {
            get=> _IsRun;
            set
            {
                _IsRun = value;
               // RaisePropertyChanged(nameof(IsRun));
                //if (!IsClient)
                //{
                //    ServerDataManageCenterV2.Instance.NotifyMessageFromServerToClient(new Channel.Models.ABTBaseMessage() { MessageType = MessageTypeEnum.ABTGlobal_IsRunStatus, Data = SerializeHelper.Serialize(IsRun) });
                //}
            }
        }

       
        #endregion

        #region 串口状态
        /// <summary>
        /// 串口通知状态
        /// </summary>
        public static event EventHandler<DriverStatusEnum> NotifySerialPortStatus;

        private static DriverStatusEnum _SerialPortStatus = DriverStatusEnum.FailedToConnecte;

        /// <summary>
        /// 串口状态
        /// </summary>
        public static DriverStatusEnum SerialPortStatus
        {
            get => _SerialPortStatus;
            set
            {
                _SerialPortStatus = value;
                NotifySerialPortStatus?.Invoke(null, SerialPortStatus);
            }
        }
        #endregion

        #region 全局设置信息

        /// <summary>
        /// 温度超限制
        /// </summary>
        public static decimal MaxTempLimit = 60;

        /// <summary>
        /// 监视水箱的温度
        /// </summary>
        public static bool IsMonitor_WaterTemp = true;
        /// <summary>
        /// 水箱的温度 告警
        /// </summary>
        public static bool IsWarning_WaterTemp = false;

        #endregion
        /// <summary>
        /// 控制停止键
        /// </summary>
        public static bool IsStop { get; set; } = false;

        /// <summary>
        /// 设备 电源   0  1串口打开  2有电   3断电
        /// </summary>
        public static int IsDevicePower { get; set; } = 0;

    }
    /// <summary>
    /// 驱动状态
    /// </summary>
    public enum DriverStatusEnum
    {
        [Description("连接失败")]
        FailedToConnecte = 1,

        [Description("正常工作")]
        WorkingFine = 2,

        [Description("超时")]
        TimeOut = 3,
    }
}
