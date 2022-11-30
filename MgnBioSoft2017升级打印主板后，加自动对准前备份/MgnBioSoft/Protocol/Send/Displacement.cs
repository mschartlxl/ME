using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustCollector.protocol
{
    static class Displacement
    {
        /// <summary>
        /// 1.归零
        /// </summary>
        /// <returns></returns>
        //HOMECMD<FD> 归零
        public static byte[] HomeCmd()
        {
            string s = "HOMECMD";//"DATA";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }
        /// <summary>
        /// 2.增量位置
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        //MOVEINC 30 50<19>   增量位置
        public static byte[] MoveINC(int offset,int speed)
        {
            string s = "MOVEINC " + offset + " " + speed;//"DATA";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }
        /// <summary>
        /// 3.绝对位置
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        //MOVEABS 10 50<13>   绝对位置
        public static byte[] MoveABS(int offset, int speed)
        {
            string s = "MOVEABS " + offset + " " + speed;
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<"+ sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }

        public static byte[] MoveABSFloat(float offset, int speed)
        {
            string s = "MOVEABS " + offset + " " + speed;
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }
        /// <summary>
        /// 校验总和
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetCheckSum(Byte[] bytes)
        {
            int cks = 0;
            foreach (byte item in bytes)
            {
                cks = (cks + item) % 0xffff;
            }
            byte b1 = (byte)((cks & 0xff00) >> 8);//取校验和高8位
            byte b2 = (byte)(cks & 0xff);//低8位
            string sum_sz = b2.ToString("X2");
            return sum_sz;
        }
        /// <summary>
        /// 4.清除故障
        /// </summary>
        /// <returns></returns>
        //clearfaults<96> 清除故障
        public static byte[] Clearfaults()
        {
            string s = "clearfaults";//"DATA";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }
        /// <summary>
        /// 5.停止找0的过程
        /// </summary>
        /// <returns></returns>
        //HOMECMD 0<4D>  停止找0的过程
        public static byte[] ShopHomeCmdFindZero()
        {
            string s = "HOMECMD 0";//"DATA";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }
        /// <summary>
        /// 6.查询位置
        /// </summary>
        /// <returns></returns>
        //PFB<D8>
        //以用户定义的单位获取主（电机）反馈的值，包括已添加的任何偏移量。
        //PFB是根据电机反馈确定的实际位置。
        public static byte[] PFB()
        {
            string s = "PFB";//"DATA";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }
        /// <summary>
        /// 7.使能
        /// </summary>
        /// <returns></returns>
        //en<D3> 使能
        public static byte[] EnableNeed()
        {
            string s = "en";//"DATA";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }
        /// <summary>
        /// 8.去使能
        /// </summary>
        /// <returns></returns>
        //k<6B> 去使能
        public static byte[] DisEnableNeed()
        {
            string s = "k";//"DATA";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string sum_str = GetCheckSum(buf);
            string sumstr = s + "<" + sum_str + ">\r";
            byte[] buf1 = Encoding.ASCII.GetBytes(sumstr);
            return buf1;
        }

    }
}
