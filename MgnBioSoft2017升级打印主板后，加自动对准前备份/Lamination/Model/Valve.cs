using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Model
{
    /// <summary>
    /// 阀
    /// </summary>
    public class Valve
    {
        public string id;
        /// <summary>
        /// 通道状态　0为未打开，1为打开，2异常
        /// </summary>
        Dictionary<int, string> tunnel_status_dic_list = new Dictionary<int, string>();
        /// <summary>
        /// 发送指令列表
        /// </summary>
        List<string> valve_pump_send_cammand_list = new List<string>();



    }
}
