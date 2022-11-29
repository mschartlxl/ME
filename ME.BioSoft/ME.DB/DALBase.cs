using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Utility.Logger;

namespace ME.DB
{
    public class DALBase<T> where T:class
    {
        public virtual bool Add(T data)
        {
            using (var db = new MEDBDB())
            {
                try
                {
                    db.Insert(data);
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"插入{data.GetType()}记录异常,操作信息：{data.ToStr()}", ex);
                    return false;
                }
            }
        }

        public virtual bool Delete(T data)
        {
            using (var db = new MEDBDB())
            {
                try
                {
                    db.Delete(data);
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"删除{data.GetType()}记录异常,操作信息：{data.ToStr()}", ex);
                    return false;
                }
            }
        }

        public virtual bool Update(T data)
        {
            using (var db = new MEDBDB())
            {
                try
                {
                    db.Update(data);
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"更新{data.GetType()}记录异常,操作信息：{data.ToStr()}", ex);
                    return false;
                }
            }
        }

        public virtual T Search(Func<T> search)
        {
            T wf = null;
            using (var db = new DB.MEDBDB())
            {
                try
                {
                    wf = db.Select(() => search());
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"查询{typeof(T)}记录异常:{ex.Message}", ex);
                }
            }
            return wf;
        }

        public virtual IEnumerable<T> SearchMany(Func<T,bool> search)
        {
            return default(IEnumerable<T>);
        }

        public virtual bool BulkCopy(IEnumerable<T> dataArray)
        {
            using (var db = new MEDBDB())
            {
                try
                {
                    db.BulkCopy(dataArray);
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"批量插入{dataArray?.GetType()}记录异常,记录数据总数：{dataArray?.Count()},明细：{string.Join("\r\n", dataArray?.Select(x => x.ToStr()))}", ex);
                    return false;
                }
            }
        }

        public virtual IEnumerable<M> Query<M>(string sqlCommand)
        {
            using (var db = new MEDBDB())
            {
                if (string.IsNullOrEmpty(sqlCommand))
                    return null;
                return db.SetCommand(sqlCommand).Query<M>()?.ToList();
            }
        }
    }
}
