using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.ConvertUser
{
    /// <summary>
    /// 字符串转换
    /// </summary>
    public static class StringConvert
    {
        /// <summary>
        /// 将字符串转换为字节集合（只取每个char的低字节）
        /// </summary>
        /// <param name="source">待转换的字符串</param>
        /// <returns>字节集合</returns>
        public static IEnumerable<byte> ToByteArray(string source)
        {
            return source.Select(i => (byte)i);
        }

        /// <summary>
        /// 将16进制字符串转换为字节集合
        /// </summary>
        /// <param name="hexString">参数格式："10 1A 20 44 FF AE 80"</param>
        /// <returns></returns>
        public static IEnumerable<byte> FromHexString(string hexString)
        {
            List<byte> result = new List<byte>();
            string[] array = hexString.Split(' ');
            foreach (string i in array)
            {
                string item = i.Trim();
                if (!string.IsNullOrEmpty(item))
                {
                    int offset = 0;
                    do
                    {
                        var subLen = i.Length >= 2 ? 2 : 1;
                        result.Add(System.Convert.ToByte(i.Substring(offset, subLen), 16));
                        offset += 2;
                    } while (offset < i.Length);
                }
            }
            return result;
        }
    }
}
