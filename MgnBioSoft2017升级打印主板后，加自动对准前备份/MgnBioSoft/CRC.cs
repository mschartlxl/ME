using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO
{
    public class CRC
    {
        #region CRC16
        public static byte[] CRC16(byte[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;

                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8); //高位置
                byte lo = (byte)(crc & 0x00FF); //低位置

                return new byte[] { hi, lo };
            }
            return new byte[] { 0, 0 };
        }
        #endregion



        #region ToCRC16
        public static string ToCRC16(string content)
        {
            return ToCRC16(content, Encoding.UTF8);
        }

        public static string ToCRC16(string content, bool isReverse)
        {
            return ToCRC16(content, Encoding.UTF8, isReverse);
        }

        public static string ToCRC16(string content, Encoding encoding)
        {
            return ByteToString(CRC16(encoding.GetBytes(content)), true);
        }

        public static string ToCRC16(string content, Encoding encoding, bool isReverse)
        {
            return ByteToString(CRC16(encoding.GetBytes(content)), isReverse);
        }

        public static string ToCRC16(byte[] data)
        {
            return ByteToString(CRC16(data), true);
        }

        public static string ToCRC16(byte[] data, bool isReverse)
        {
            return ByteToString(CRC16(data), isReverse);
        }
        #endregion

        #region ToModbusCRC16
        public static string ToModbusCRC16(string s)
        {
            return ToModbusCRC16(s, true);
        }

        public static string ToModbusCRC16(string s, bool isReverse)
        {
            return ByteToString(CRC16(StringToHexByte(s)), isReverse);
        }

        public static string ToModbusCRC16(byte[] data)
        {
            return ToModbusCRC16(data, true);
        }

        public static string ToModbusCRC16(byte[] data, bool isReverse)
        {
            return ByteToString(CRC16(data), isReverse);
        }
        #endregion

        #region ByteToString
        public static string ByteToString(byte[] arr, bool isReverse)
        {
            try
            {
                byte hi = arr[0], lo = arr[1];
                return Convert.ToString(isReverse ? hi + lo * 0x100 : hi * 0x100 + lo, 16).ToUpper().PadLeft(4, '0');
            }
            catch (Exception ex) { throw (ex); }
        }

        public static string ByteToString(byte[] arr)
        {
            try
            {
                return ByteToString(arr, true);
            }
            catch (Exception ex) { throw (ex); }
        }
        #endregion

        #region StringToHexString
        public static string StringToHexString(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                s.Append(c.ToString("X4"));
            }
            return s.ToString();
        }
        #endregion

        #region StringToHexByte
        private static string ConvertChinese(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                if (c <= 0 || c >= 127)
                {
                    s.Append(c.ToString("X4"));
                }
                else
                {
                    s.Append((char)c);
                }
            }
            return s.ToString();
        }

        private static string FilterChinese(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                if (c > 0 && c < 127)
                {
                    s.Append((char)c);
                }
            }
            return s.ToString();
        }

        /// <summary>
        /// 字符串转16进制字符数组
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToHexByte(string str)
        {
            return StringToHexByte(str, false);
        }

        /// <summary>
        /// 字符串转16进制字符数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isFilterChinese">是否过滤掉中文字符</param>
        /// <returns></returns>
        public static byte[] StringToHexByte(string str, bool isFilterChinese)
        {
            string hex = isFilterChinese ? FilterChinese(str) : ConvertChinese(str);

            //清除所有空格
            hex = hex.Replace(" ", "");
            //若字符个数为奇数，补一个0
            hex += hex.Length % 2 != 0 ? "0" : "";

            byte[] result = new byte[hex.Length / 2];
            for (int i = 0, c = result.Length; i < c; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }
        /// <summary>
        /// 添加CRC校验
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GetNewCrcArray(byte[] data)
        {
            int count = data.Length;
            byte[] buf = new byte[data.Length + 2];
            data.CopyTo(buf, 0);

            int cks = 0;
            foreach (byte item in data)
            {
                cks = (cks + item) % 0xffff ;
            }
            //data[data.Length - 2] = (byte)((cks & 0xff00) >> 8);//取校验和高8位
            //data[data.Length - 1] = (byte)(cks & 0xff);//低8位
            buf[buf.Length - 1] = (byte)((cks & 0xff00) >> 8);//取校验和高8位
            buf[buf.Length - 2] = (byte)(cks & 0xff);//低8位
            return buf;
        }

        public static int GetKey(byte[] data)
        {
            int count = data.Length;
            byte[] buf = new byte[data.Length + 2];
            data.CopyTo(buf, 0);
            int ptr = 0;
            int i = 0;
            int crc = 0;
            byte crc1, crc2, crc3;
            crc1 = buf[ptr++];
            crc2 = buf[ptr++];
            buf[count] = 0;
            buf[count + 1] = 0;
            while (--count >= 0)
            {
                crc3 = buf[ptr++];
                for (i = 0; i < 8; i++)
                {
                    if (((crc1 & 0x80) >> 7) == 1)//判断crc1高位是否为1
                    {
                        crc1 = (byte)(crc1 << 1); //移出高位
                        if (((crc2 & 0x80) >> 7) == 1)//判断crc2高位是否为1
                        {
                            crc1 = (byte)(crc1 | 0x01);//crc1低位由0变1
                        }
                        crc2 = (byte)(crc2 << 1);//crc2移出高位
                        if (((crc3 & 0x80) >> 7) == 1) //判断crc3高位是否为1
                        {
                            crc2 = (byte)(crc2 | 0x01); //crc2低位由0变1
                        }
                        crc3 = (byte)(crc3 << 1);//crc3移出高位
                        crc1 = (byte)(crc1 ^ 0x10);
                        crc2 = (byte)(crc2 ^ 0x21);
                    }
                    else
                    {
                        crc1 = (byte)(crc1 << 1); //移出高位
                        if (((crc2 & 0x80) >> 7) == 1)//判断crc2高位是否为1
                        {
                            crc1 = (byte)(crc1 | 0x01);//crc1低位由0变1
                        }
                        crc2 = (byte)(crc2 << 1);//crc2移出高位
                        if (((crc3 & 0x80) >> 7) == 1) //判断crc3高位是否为1
                        {
                            crc2 = (byte)(crc2 | 0x01); //crc2低位由0变1
                        }
                        crc3 = (byte)(crc3 << 1);//crc3移出高位
                    }
                }
            }
            crc = (int)((crc1 << 8) + crc2);


            int cks = 0;
            foreach (byte item in data)
            {
                cks = (cks + item) % 0xffff;
            }

            buf[buf.Length - 1] = (byte)((cks & 0xff00) >> 8);//取校验和高8位
            buf[buf.Length - 2] = (byte)(cks & 0xff);//低8位




            return crc;
        }




        #endregion
    }
}
