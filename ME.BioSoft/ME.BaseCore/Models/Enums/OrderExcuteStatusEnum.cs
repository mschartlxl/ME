using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BaseCore.Models.Enums
{
    /// <summary>
    /// 指令执行状态，描述单条指令的执行状态
    /// </summary>
    public enum OrderExcuteStatusEnum : byte
    {
        [Description("等待执行")]
        WaitForExcute = 0,

        [Description("执行中")]
        Excuting = 1,

        [Description("执行成功")]
        ExcuteSuccess = 2,

        [Description("执行失败")]
        ExcuteFailed = 3
    }
}
