using MGNBIO.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Common
{
    /// <summary>
    /// 全局数据
    /// </summary>
    public static class CommonData
    {
        private static int data = 0;
        public static int Data
        {
            get { return data; }
            set { data = value; }
        }
        public static List<PrintContent> print_content_list_g = new List<PrintContent>();
        /// <summary>
        /// 打印序列
        /// </summary>
        public static List<PrintContent> PrintContentList_g
        {
            get { return print_content_list_g; }
            set { print_content_list_g = value; }
        }

        public static int Line_point_number { get => line_point_number; set => line_point_number = value; }
        public static int Lin_number { get => lin_number; set => lin_number = value; }

        /// <summary>
        /// 每次点数
        /// </summary>
        private static int line_point_number = 0;

        /// <summary>
        /// 一共多少行
        /// </summary>
        private static int lin_number = 0;
    }
}
