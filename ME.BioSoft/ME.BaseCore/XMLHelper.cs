using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ME.BaseCore
{
    
    public class XMLHelper
    {
        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="xml">XML字符串</param>
        /// <returns></returns>
        public static object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            try
            {
                XmlSerializer xmldes = new XmlSerializer(type);
                return xmldes.Deserialize(stream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            try
            {
                //序列化对象
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        public static void Serializer(Type type, object obj, FileStream fs)
        {
            XmlSerializer formatter = new XmlSerializer(type);
            formatter.Serialize(fs, obj);
        }
        #endregion

        /// <summary>
        /// 读xml文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ReadXMLFile<T>(ref T t, string fileName)
        {
           
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        t = (T)XMLHelper.Deserialize(typeof(T), fs);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 保持xml文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool SaveXMLFile<T>(T t, string fileName)
        {
           
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);

                    //开始写入
                    sw.Write(XMLHelper.Serializer(typeof(T), t), true);
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static T XmlToModel<T>(string fileName)
        {
            if(File.Exists(fileName))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(fileName);
                var xml = doc.InnerXml;
                xml = Regex.Replace(xml, @"<\?xml*.*?>", "", RegexOptions.IgnoreCase);
                XmlSerializer xmlSer = new XmlSerializer(typeof(T));
                using (StringReader xmlReader = new StringReader(xml))
                {
                    return (T)xmlSer.Deserialize(xmlReader);
                }
            }
            return default(T);
        }



        public static string GetAttributeStringValue(XElement element, string attributeName)
        {
            if (element != null)
            {
                XAttribute attr = element.Attributes().FirstOrDefault(p => p.Name.LocalName.ToLower() == attributeName.ToLower());
                if (attr != null)
                {
                    return attr.Value;
                }
            }
            return string.Empty;
        }

        public static int GetAttributeIntValue(XElement element, string attributeName, int defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            int result = defaultValue;
            int.TryParse(value, out result);
            return result;
        }
        public static int? GetAttributeIntValue(XElement element, string attributeName, int? defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            int result = 0;
            if (defaultValue != null)
            {
                result = defaultValue.Value;
            }
            return result;
        }
        public static double GetAttributeDoubleValue(XElement element, string attributeName, double defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            double result = defaultValue;
            double.TryParse(value, out result);
            return result;
        }
        public static long GetAttributeLongValue(XElement element, string attributeName, long defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            long result = defaultValue;
            long.TryParse(value, out result);
            return result;
        }

        public static long? GetAttributeLongValue(XElement element, string attributeName, long? defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            long result = 0;
            if (defaultValue != null)
            {
                result = defaultValue.Value;
            }
            long.TryParse(value, out result);
            return result;
        }
        public static DateTime GetAttributeDatetimeValue(XElement element, string attributeName, DateTime defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            DateTime result = defaultValue;
            DateTime.TryParse(value, out result);
            return result;
        }
        public static DateTime? GetAttributeDatetimeValue(XElement element, string attributeName, DateTime? defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            DateTime result = DateTime.Now;
            if (defaultValue != null)
            {
                result = defaultValue.Value;
            }
            DateTime.TryParse(value, out result);
            return result;
        }

        public static bool GetAttributeBoolValue(XElement element, string attributeName)
        {
            string value = "false";
            if (element != null)
            {
                value = GetAttributeStringValue(element, attributeName);
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
            }
            return value.ToLower() == "true";
        }
        public static decimal GetAttributeDecimalValue(XElement element, string attributeName, decimal defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }
            string value = GetAttributeStringValue(element, attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            decimal result = defaultValue;
            decimal.TryParse(value, out result);
            return result;
        }
    }


    /// <summary>
    /// 对加密解密的处理类
    /// </summary>
    public partial class ABTEncrypt
    {
        //默认密钥向量
        private static byte[] Keys = { 0x35, 0x36, 0x39, 0x68, 0x97, 0xBE, 0xCE, 0xED };

        static string keyValue = "ZCHS_2021!@#$%^";

        /// <summary>
        /// DES加密字符串（可逆加密）
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <returns></returns>
        public static string Encode(string encryptString)
        {
            return Encode(encryptString, keyValue);
        }
        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <returns></returns>
        public static string Decode(string decryptString)
        {
            return Decode(decryptString, keyValue);
        }

        /// <summary>
        /// DES加密字符串（可逆加密）
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="encryptKey">加密密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串,失败返回源串</returns>
        public static string Encode(string encryptString, string encryptKey)
        {
            encryptKey = keyValue;

            if (encryptKey.Length > 8)
                encryptKey = encryptKey.Substring(0, 8);

            encryptKey = encryptKey.PadRight(8, ' ');
            byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
            byte[] rgbIV = Keys;
            byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
            DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());

        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
        public static string Decode(string decryptString, string decryptKey)
        {
            try
            {
                decryptKey = keyValue;

                if (decryptKey.Length > 8)
                    decryptKey = decryptKey.Substring(0, 8);

                decryptKey = decryptKey.PadRight(8, ' ');
                byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey);
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();

                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return "";
            }

        }


        /// <summary>
        /// DES加密字符串（可逆加密）
        /// </summary>
        /// <param name="source"></param>
        public static void Encode_Xml(string source)
        {
            string text = System.IO.File.ReadAllText(source);
            string encode = ABTEncrypt.Encode(text);
            using (FileStream fsWrtie = new FileStream(source, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fsWrtie);
                try
                {
                    sw.WriteLine(encode);
                }
                finally
                {
                    if (sw != null) { sw.Close(); }
                }
            }
        }
        public static string temp1 = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "temp1";
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="source"></param>
        public static void Decode_Xml(string source)
        {
            string text = System.IO.File.ReadAllText(source);
            string encode = ABTEncrypt.Decode(text);
            using (FileStream fsWrtie = new FileStream(temp1, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fsWrtie);
                try
                {
                    sw.WriteLine(encode);
                }
                finally
                {
                    if (sw != null) { sw.Close(); }
                }
            }
        }
    }
}
