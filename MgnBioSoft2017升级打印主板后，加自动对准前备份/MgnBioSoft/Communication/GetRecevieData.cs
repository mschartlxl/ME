using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Communication
{
    public class GetRecevieData
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }


        private int status = 0;

        /// <summary>
        /// 指令状态　0为默认状态，1已经发送,2返回成功
        /// </summary>
        public int Status
        {
            get { return status; }
            set { status = value; }
        }


        private string thread_id = "0";
        /// <summary>
        /// 线程ID，默认都是0，1为1线程，2为2线程
        /// </summary>
        public string Thread_id
        {
            get { return thread_id; }
            set { thread_id = value; }
        }


        private string task_id = "0";
        /// <summary>
        /// 任务编号
        /// </summary>
        public string Task_id
        {
            get { return task_id; }
            set { task_id = value; }
        }

        private int need_return_value = -1;

        public int Need_return_value
        {
            get { return need_return_value; }
            set { need_return_value = value; }
        }





    }
}
