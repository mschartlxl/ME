using MGNBIO.ConvertUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Check
{
    /// <summary>
    /// 循环冗余奇偶校验
    /// </summary>
    public static class CRC
    {
        /// <summary>
        /// 获取校验码
        /// </summary>
        /// <param name="source">待校验字符串</param>
        /// <returns>校验码</returns>
        public static string Check(string source)
        {
            var crc = Check(StringConvert.ToByteArray(source).ToArray());
            return ByteConvert.ToString(crc);
        }

        /// <summary>
        /// 获取校验码
        /// </summary>
        /// <param name="source">待校验数据</param>
        /// <returns>校验码</returns>
        public static byte[] Check(byte[] source)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in source)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if (0 != (crc & 0x1))
                    {
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return new byte[] { (byte)crc, (byte)(crc >> 8) };
        }
    }
}
