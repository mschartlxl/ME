using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BaseCore
{
    public enum NodeModel
    {
        /// <summary>
        /// 节点模型
        /// </summary>
        LoopMode = 0,
        /// <summary>
        /// 命令模型
        /// </summary>
        CommonMode = 1,
        /// <summary>
        /// 等待
        /// </summary>
        WaitMode = 2,
        /// <summary>
        /// 温度模型
        /// </summary>
        TemperatureMode = 3,
        /// <summary>
        /// 荧光数据
        /// </summary>
        FluorescenceMode = 4,
        /// <summary>
        /// 显示
        /// </summary>
        ShowMode = 5

    }
}
