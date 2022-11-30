using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustCollector.Model
{
    public class SendQueryXYR_Model
    {
        string xyr_type = "";
        int send_status = 1; //1未发送查询位置，2已经发送查询位置，3收到查询位置并且数值一致,已经查询队列中的查询条件;收到查询结果，但是不一致， 状态改为1;
        float offset_value = 0;
        int id = 0;

        public string Xyr_type { get => xyr_type; set => xyr_type = value; }
        public int Send_status { get => send_status; set => send_status = value; }
        public float offset { get => offset_value; set => offset_value = value; }
        public int Id { get => id; set => id = value; }
    }
}
