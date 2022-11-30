using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MGNBIO.Model;
using System.Collections;

namespace MGNBIO
{
    public class DeviceInfo
    {
        /// <summary>
        /// 多通道切换阀
        /// </summary>
        private List<Valve> valve_list = new List<Valve>();

        public List<Valve> Valve_list
        {
            get { return valve_list; }
            set { valve_list = value; }
        }
        /// <summary>
        /// 注射泵
        /// </summary>
        private List<ValvePump> valve_pump_list = new List<ValvePump>();

        public List<ValvePump> Valve_pump_list
        {
            get { return valve_pump_list; }
            set { valve_pump_list = value; }
        }

        public List<string> valve_pump1_send_cammand_list = new List<string>();
        public List<string> valve_pump2_send_cammand_list = new List<string>();
        public List<string> valve1_send_cammand_list = new List<string>();
        public List<string> valve2_send_cammand_list = new List<string>();

        /// <summary>
        ///  添加注射泵发送指令
        /// </summary>
        /// <param name="valve_pump_id"></param>
        /// <param name="cammand_sz"></param>
        public void add_valve_pump_send_cammand_list(string valve_pump_id, string cammand_sz)
        {
            if (valve_pump_id.Equals("01"))
            {
                valve_pump1_send_cammand_list.Add(cammand_sz);
            }
            if (valve_pump_id.Equals("02"))
            {
                valve_pump2_send_cammand_list.Add(cammand_sz);
            }
        }

        /// <summary>
        /// 添加阀发送指令
        /// </summary>
        /// <param name="valve_pump_id"></param>
        /// <param name="cammand_sz"></param>
        public void add_valve_send_cammand_list(string valve_pump_id, string cammand_sz)
        {
            if (valve_pump_id.Equals("03"))
            {
                valve1_send_cammand_list.Add(cammand_sz);
            }

            if (valve_pump_id.Equals("04"))
            {
                valve2_send_cammand_list.Add(cammand_sz);
            }
        }


        //private Hashtable ht_send_cammand_list = new Hashtable();

        //public Hashtable Ht_send_cammand_list
        //{
        //    get { return ht_send_cammand_list; }
        //    set { ht_send_cammand_list = value; }
        //}
 

    }
}
