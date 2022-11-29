namespace ME.BaseCore.Models
{
    public class ExportExcelTableInfo
    {
        private int _sheet;//Sheet页
        private string _title;//第一行标题
        private string[] _header;//第二行表头

        /// <summary>
        /// Sheet页
        /// </summary>
        public int Sheet
        {
            get { return _sheet; }
            set { _sheet = value; }
        }

        /// <summary>
        /// 第一行标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        /// <summary>
        /// 第二行表头
        /// </summary>
        public string[] Header
        {
            get { return _header; }
            set { _header = value; }
        }
    }
    /// <summary>
    /// 表头
    /// </summary>
    public class Headers
    {
        private string _testPoint;//测试点
        private string _position;//通道
        private string _emptyCupQuality;//空杯质量
        private string _qualityAfterLiquidAddition;//加液后质量
        private string _liquidQuality;//液体质量

        /// <summary>
        /// 测试点
        /// </summary>
        public string TestPoint
        {
            get { return _testPoint; }
            set { _testPoint = value; }
        }

        /// <summary>
        /// 通道
        /// </summary>
        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// 空杯质量
        /// </summary>
        public string EmptyCupQuality
        {
            get { return _emptyCupQuality; }
            set { _emptyCupQuality = value; }
        }

        /// <summary>
        /// 加液后质量
        /// </summary>
        public string QualityAfterLiquidAddition
        {
            get { return _qualityAfterLiquidAddition; }
            set { _qualityAfterLiquidAddition = value; }
        }

        /// <summary>
        /// 液体质量
        /// </summary>
        public string LiquidQuality
        {
            get { return _liquidQuality; }
            set { _liquidQuality = value; }
        }
    }
}
