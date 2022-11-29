using System;
using System.Collections.Generic;

namespace ME.BaseCore
{
    public static class StringToDigitUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strFloat"></param>
        /// <param name="floatValue"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool CanToFloat(this string strFloat, out float floatValue, out string errMsg)
        {
            floatValue = 0;
            errMsg = string.Empty;
            if (string.IsNullOrEmpty(strFloat))
            {
                errMsg = "不能为空！";
                return false;
            }
            else if (!float.TryParse(strFloat, out floatValue))
            {
                errMsg = "非法数字！";
                return false;
            }
            return true;
        }

        public static float ToFloat(this string strFloat)
        {
            return float.Parse(strFloat);
        }
        public static string ToSecond(this string str)
        {
            if (str.Contains(":")) 
            {
                var strs=str.Split(':');
                return strs[1];
            }
            else 
            {
                return str;
            }
        }
        public static byte[] ToByteArray(this string strValue, int fromBase)
        {
            int length;
            List<byte> result = new List<byte>();
            switch (fromBase)
            {
                case 16:
                    length = strValue.Replace(" ", "").Length / 2;
                    for (int i = 0; i < length; i++)
                    {
                        result.Add(Convert.ToByte(strValue.Substring(i * 2, 2), 16));
                    }
                    break;
            }
            return result.ToArray();
        }
    }
}
