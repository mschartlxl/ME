using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.DB
{
    /// <summary>
    /// 数据表扩展类
    /// </summary>
    public static class TableClassExtension
    {
        //public static string ToStr(this UserOperation userOperation)
        //{
        //    if (userOperation == null)
        //        return string.Empty;
        //    return $"Id = {userOperation.Id},UserId = {userOperation.UserId},UserName = {userOperation.UserName},Description = {userOperation.Description},OperationTime = {userOperation.OperationTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff")}";
        //}


        //public static string ToStr(this Result result)
        //{
        //    if (result == null)
        //        return string.Empty;
        //    return $"Id = {result.Id},CheckingBatchId = {result.CheckingBatchId},Concentration = {result.Concentration},CT = {result.CT},Department = {result.Department},Num={result.Num},PatientId={result.PatientId},PatientName={result.PatientName},Position={result.Position},Sex={result.Sex},ResultValueCode={result.ResultValueCode},={result.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss.fffffff")}";
        //}
        
        /// <summary>
        /// 该函数只反射了直接属性，没有处理嵌套的属性，该函数仅供数据表对象使用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToStr(this object obj)
        {
            try
            {
                if (null == obj)
                    return string.Empty;

                StringBuilder strResult = new StringBuilder();
                var objType = obj.GetType();
                foreach (var property in objType.GetProperties())
                {
                    strResult.Append($"{property.Name}={property.GetValue(obj)?.ToString()},");
                }

                return strResult.ToString().TrimEnd(",");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
