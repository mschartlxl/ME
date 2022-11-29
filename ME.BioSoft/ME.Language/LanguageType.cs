using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.Language
{
    public enum LanguageType:int
    {
        /// <summary>
        /// 中文
        /// </summary>
        [Description("中文")]
        zh_CN = 0,
        /// <summary>
        /// 英文
        /// </summary>
        [Description("英文")]
        en = 1,
        /// <summary>
        /// 法文
        /// </summary>
        [Description("法文")]
        fr =2,
        /// <summary>
        /// 日文
        /// </summary>
        [Description("日文")]
        ja =3

    }
}
