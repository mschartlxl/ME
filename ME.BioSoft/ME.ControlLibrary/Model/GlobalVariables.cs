﻿using ME.BaseCore.Instrument;
using ME.BaseCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
namespace ME.ControlLibrary.Model
{
    /// <summary>
    /// 全局变量
    /// </summary>
    public class GlobalVariables
    {
        #region 静态对象
        /// <summary>
        /// 单例对象
        /// </summary>
        private static GlobalVariables _instance;
        /// <summary>
        /// 实例
        /// </summary>
        public static GlobalVariables Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (lockObj)
                    {
                        if (null == _instance)
                        {
                            _instance = new GlobalVariables();
                        }
                    }
                }
                return _instance;
            }
        }

        private static object lockObj = new object();
        #endregion

        /// <summary>
        /// 是否是客户端
        /// </summary>
        public bool IsClient { get; private set; }

        #region 构造函数
        private GlobalVariables()
        {

        }
        #endregion

        private object _TreeItemCopy;
        /// <summary>
        /// 复制指令集
        /// </summary>
        public object TreeItemCopy
        {
            get => _TreeItemCopy;
            set { _TreeItemCopy = value; }
        }
        public void InitReset(bool flag)
        {
            System.Func<bool> cancelFun = () =>
            {
                return false;
            };
            AllPumpReset(cancelFun);
            AllReCircleReset(cancelFun);
            AllSwitchReset(cancelFun);
            if (flag)
            {
                AllZAxisReset(cancelFun);
            }

        }
        private void AllZAxisReset(Func<bool> cancelFun)
        {
            List<Task> tasklist = new List<Task>();
            var zlist = ListConfig.GetInstance().ListZAxisNumber;
            foreach (var z in zlist)
            {
                byte[] senddata = ZAxisResetConfig.cmdZAxisReset; //InstructionConfig.cmdZAxisReset;
                senddata[0] = Convert.ToByte(z.Number.ToString("X2"), 16);
                var crcdata = CRC.CRC16(senddata);
                var senddatanew = senddata.ToList();
                senddatanew.AddRange(crcdata.Reverse());
                var temps = senddatanew.ToArray().Clone() as byte[];
                tasklist.Add(Task.Run(() =>
                {
                    UtilsFun._AbtInstrument.Send_16(cancelFun, temps, true, UtilsFun._AbtInstrument.SerialSwitch, 5, ZAxisResetConfig.type,z.Number);
                }));
            }
            Task.WaitAll(tasklist.ToArray());
        }
        /// <summary>
        /// 所有电磁阀复位
        /// </summary>
        /// <param name="cancelFun"></param>
        private void AllSwitchReset(Func<bool> cancelFun)
        {
            List<Task> tasklist = new List<Task>();
            var list = ListConfig.GetInstance().ListSwitchNumber;
            var senddata = SwitchResetConfig.cmdSwitchReset.ToList(); //InstructionConfig.cmdSwitchReset.ToList();
            var crcarry = CRC.CRC16(senddata.ToArray());
            senddata.Add(crcarry[1]);
            senddata.Add(crcarry[0]);
            tasklist.Add(Task.Run(() =>
            {
               UtilsFun._AbtInstrument.Send_16(cancelFun, senddata.ToArray(), true, UtilsFun._AbtInstrument.SerialSwitch, 5, SwitchResetConfig.type,1);
            }));

            Task.WaitAll(tasklist.ToArray());
        }
        /// <summary>
        /// 所有旋切阀复位
        /// </summary>
        /// <param name="cancelFun"></param>
        private void AllReCircleReset(Func<bool> cancelFun)
        {
            List<Task> tasklist = new List<Task>();
            var list = ListConfig.GetInstance().ListReCircleNumber;
            byte[] senddata = ReCircleResetConfig.cmdReCircleReset;// InstructionConfig.cmdReCircleReset;
            foreach (var t in list)
            {
                Thread.Sleep(50);
                senddata[1] = Convert.ToByte(t.Number.ToString("X2"), 16); //X4;
                var senddatanew = CRC.GetNewCrcArray(senddata);
                tasklist.Add(Task.Run(() =>
                {
                    UtilsFun._AbtInstrument.Send_16(cancelFun, senddatanew, true, UtilsFun._AbtInstrument.SerialPump, 5, ReCircleResetConfig.type,t.Number);
                }));
            }
            Task.WaitAll(tasklist.ToArray());
        }
        /// <summary>
        /// 所有泵复位
        /// </summary>
        /// <param name="cancelFun"></param>
        private void AllPumpReset(Func<bool> cancelFun)
        {
            List<Task> tasklist = new List<Task>();
            var list = ListConfig.GetInstance().ListPumpNumber;
            byte[] senddata = PumpResetConfig.cmdPumpReset;// InstructionConfig.cmdPumpReset;
            foreach (var t in list)
            {
                Thread.Sleep(50);
                senddata[1] = Convert.ToByte(t.Number.ToString("X2"), 16); //X4;
                var senddatanew = CRC.GetNewCrcArray(senddata);
                tasklist.Add(Task.Run(() =>
                {
                    UtilsFun._AbtInstrument.Send_16(cancelFun, senddatanew, true, UtilsFun._AbtInstrument.SerialPump, 5, PumpResetConfig.type, t.Number);
                }));
            }
            Task.WaitAll(tasklist.ToArray());
        }
       

    }
}
