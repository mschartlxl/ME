using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Communication
{
    public class GetOrderClsSimple
    {
        private int id = 0;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private string order_content;

        private int data_transf_type = 0;
        /// <summary>
        /// 0为串，1为并行
        /// </summary>
        public int Data_transf_type
        {
            get { return data_transf_type; }
            set { data_transf_type = value; }
        }

        private int status = 0;

        /// <summary>
        /// 指令状态　0为默认状态，1等待发送,2已经发送,3发送执行完成,4
        /// </summary>
        public int Status
        {
            get { return status; }
            set { status = value; }
        }

        private int iLoop = 1;
        /// <summary>
        /// 第一层循环
        /// </summary>
        public int ILoop
        {
            get { return iLoop; }
            set { iLoop = value; }
        }

        private int iLoopNext = 1;
        /// <summary>
        /// 第二层循环
        /// </summary>
        public int ILoopNext
        {
            get { return iLoopNext; }
            set { iLoopNext = value; }
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

        private string tag;

        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        //private int loop_number = 1;
        //public int Loop_number
        //{
        //    get { return loop_number; }
        //    set { loop_number = value; }
        //}

        public string Order_content
        {
            get { return order_content; }
            set { order_content = value; }
        }


        //public byte[] Order_content_byte_array
        //{
        //    get { return order_content_byte_array; }
        //    set { order_content_byte_array = value; }
        //}

    }
}
