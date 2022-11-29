using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ME.Language
{
    public class LangHelper
    {
        /// <summary>
        /// 语言标识
        /// </summary>
        public static LanguageType LangType { get; set; }
        /// <summary>
        /// 多语言资源获取
        /// </summary>
        /// <param name="res">资源名称</param>
        /// <returns></returns>
        public static string LangRes(string res)
        {
            try
            {
                return Application.Current.Resources[res].ToString();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Res Error：" + ex.Message);
                return "";
            }
        }
    }
}
