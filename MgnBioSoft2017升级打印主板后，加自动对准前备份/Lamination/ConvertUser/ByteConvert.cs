using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.ConvertUser
{
    /// <summary>
    /// 字节转换
    /// </summary>
    public static class ByteConvert
    {
        /// <summary>
        /// 将字节转换为16进制字符串
        /// </summary>
        /// <param name="source">待转换的字节</param>
        /// <returns>16进制字符串</returns>
        public static string ToHexString(byte source)
        {
            return string.Format("{0:X2}", source);
        }

        /// <summary>
        /// 将字节集合转换为16进制字符串
        /// </summary>
        /// <param name="source">待转换的字节集合</param>
        /// <returns>16进制字符串</returns>
        public static string ToHexString(IEnumerable<byte> source)
        {
            StringBuilder sb = new StringBuilder(source.Count() * 3);
            foreach (byte b in source)
            {
                sb.AppendFormat("{0:X2} ", b);
            }
            return sb.ToString().TrimEnd(' ');
        }

        /// <summary>
        /// 将字节集合转换为字符串
        /// </summary>
        /// <param name="source">待转换的字节集合</param>
        /// <returns>转换后的字符</returns>
        public static string ToString(IEnumerable<byte> source)
        {
            StringBuilder sb = new StringBuilder(source.Count());
            foreach (byte b in source)
            {
                sb.Append((char)b);
            }
            return sb.ToString();
        }
    }
}
