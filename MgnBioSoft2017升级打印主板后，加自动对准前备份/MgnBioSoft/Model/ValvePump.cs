using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Model
{
    /// <summary>
    /// 阀和泵
    /// </summary>
    public class ValvePump
    {
        public string id;
        /// <summary>
        /// 通道状态　0为未打开，1为打开
        /// </summary>
        Dictionary<int, string> tunnel_status_dic_list = new Dictionary<int, string>();
        /// <summary>
        /// 发送指令列表
        /// </summary>
        List<string> valve_pump_send_cammand_list = new List<string>();
        /// <summary>
        /// 速度
        /// </summary>
        public int speed;
        /// <summary>
        /// 偏移量
        /// </summary>
        public int offset;
        /// <summary>
        /// 运行状态　0为无异常，1为异常
        /// </summary>
        public int run_status;


    }
}
