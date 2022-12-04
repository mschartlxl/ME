using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ME.DB
{
    public class PlatformActionDAL : DB.DALBase<PlatformAction>
    {

        #region 静态对象
        private static PlatformActionDAL _instance;
        /// <summary>
        /// 实例对象
        /// </summary>
        public static PlatformActionDAL Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (lockObj)
                    {
                        if (null == _instance)
                        {
                            _instance = new PlatformActionDAL();
                        }
                    }
                }
                return _instance;
            }
        }

        private static object lockObj = new object();
        #endregion

        #region 构造函数
        private PlatformActionDAL()
        { }
        #endregion
        public override IEnumerable<PlatformAction> SearchMany(Func<PlatformAction, bool> search)
        {
            try
            {
                List<PlatformAction> lst = null;
                using (var db = new DB.MEDB())
                {
                    var data = from n in db.PlatformActions
                               select n;
                    lst = data == null ? new List<PlatformAction>() : data.ToList();
                }
                if (null != search)
                    lst = lst.Where(x => search.Invoke(x)).ToList();
              
                return lst;
            }
            catch (Exception ex)
            {
                //LogHelper.Error($"批量查询标定数据异常：{ex.Message}", ex);
                return default(IEnumerable<PlatformAction>);
            }
        }
    }
}
