

using System.ComponentModel;

namespace ME.BaseCore.Models.Enums
{
    /// <summary>
    /// 公共枚举
    /// </summary>
    public static class PublicEnums
    {
        /// <summary>
        /// 设备类型
        /// </summary>
        public enum NodeMoveType
        {
            /// <summary>
            /// 向上移动
            /// </summary>
            [Description("向上移动")]
            UP,
            /// <summary>
            /// 向下移动
            /// </summary>
            [Description("向下移动")]
            Down
        }
    }
}
