using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MGNBIO.Common
{
    public class LSXmlSerializer
    {
        public static void SaveToXml(string filePath, object sourceObj, Type type, string xmlRootName)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && sourceObj != null)
            {
                type = type != null ? type : sourceObj.GetType();

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = string.IsNullOrWhiteSpace(xmlRootName) ?
                        new System.Xml.Serialization.XmlSerializer(type) :
                        new System.Xml.Serialization.XmlSerializer(type, new XmlRootAttribute(xmlRootName));
                    xmlSerializer.Serialize(writer, sourceObj);
                }
            }
        }

        public static object LoadFromXml(string filePath, Type type)
        {
            object result = null;

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(type);
                    result = xmlSerializer.Deserialize(reader);
                }
            }

            return result;
        }

        public static List<T> XmlToList<T>(string xml, string rootName) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute(rootName));
            using (StringReader sr = new StringReader(xml))
            {
                List<T> list = serializer.Deserialize(sr) as List<T>;
                return list;
            }
        }

        //实体类转换XML，xml序列化
        public static string XmlSerialize<T>(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                Type t = obj.GetType();
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(sw, obj);
                sw.Close();
                return sw.ToString();
            }
        }
        public static bool XmlSerializeSaveFile<T>(T obj,string file_name)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                try
                {
                    Type t = obj.GetType();
                    XmlSerializer serializer = new XmlSerializer(obj.GetType());
                    serializer.Serialize(stringWriter, obj);
                    FileStream fs = new FileStream(file_name, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(stringWriter.ToString());
                    sw.Close();
                    fs.Close();
                    stringWriter.Close();
                    return true;//stringWriter.ToString();
                }
                catch(Exception e){
                    return false;
                }
            }
        }
        public static string ReadXmlFromFile(string file_name)
        {
            //实例化一个xml操作对象
            //XmlDocument xDoc = new XmlDocument();
            //xDoc.Load(file_name);
            //string xml = xDoc.ToString();
            //return xml;
            using (FileStream fsRead = new FileStream(file_name, FileMode.Open))
            {
                int fsLen = (int)fsRead.Length;
                byte[] heByte = new byte[fsLen];
                int r = fsRead.Read(heByte, 0, heByte.Length);
                string myStr = System.Text.Encoding.UTF8.GetString(heByte);
                return myStr;
                Console.WriteLine(myStr);
                Console.ReadKey();
            }
        }
        //xml反序列化
        public static T DESerializer<T>(string strXML) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(strXML))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
