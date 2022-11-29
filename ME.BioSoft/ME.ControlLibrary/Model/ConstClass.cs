using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ME.ControlLibrary.Model
{
    /// <summary>
    /// 常量类
    /// </summary>
    public sealed class ConstClass
    {
        #region 模块

        #region 颜色
        /// <summary>
        /// 执行失败后显示背景颜色
        /// </summary>
        public static readonly Brush ExcuteFailedBackgroundColor = Brushes.Red;

        /// <summary>
        /// 执行成功后显示背景颜色
        /// </summary>
        public static readonly Brush ExcuteSuccessBackgroundColor = Brushes.Yellow;

        /// <summary>
        /// 正在执行中显示背景颜色
        /// </summary>
        public static readonly Brush ExcutingBackgroundColor = Brushes.Green;

        /// <summary>
        /// 等待执行中的背景颜色 
        /// </summary>
        public static readonly Brush WaitForExcuteBackgroundColor = Brushes.Transparent;

        /// <summary>
        /// 选中背景颜色
        /// </summary>
        public static readonly Brush SelectedBackgroundColor = Brushes.Blue;
        #endregion



        #endregion

        #region 配置信息
        ///// <summary>
        ///// 全局配置文件信息
        ///// </summary>
        //public static readonly string GlobalConfigurationPath = AppDomain.CurrentDomain.BaseDirectory + "Config" + System.IO.Path.DirectorySeparatorChar + "GlobalConfiguration.xml";

        //public static GlobalConfiguration GetGlobalConfiguration()
        //{
        //    var res = XmlConvert.DeSerialize<GlobalConfiguration>(GlobalConfigurationPath);
        //    if (!res.Item1 || res.Item2 == null)
        //    {
        //        return new GlobalConfiguration();
        //    }
        //    return res.Item2;
        //}
        #endregion
    }
}
