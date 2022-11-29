using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BaseCore.Models.Enums
{
    /// <summary>
    /// 串口类型
    /// </summary>
    public enum SerialTypeEnum : byte
    {
        [Description("注射泵")]
        SerialPump = 0,

        [Description("电磁阀")]
        SerialSwitch = 1
    }
}
