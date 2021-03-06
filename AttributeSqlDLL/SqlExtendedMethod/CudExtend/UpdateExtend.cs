﻿using System;
using System.Collections.Generic;
using System.Text;
using AttributeSqlDLL.ExceptionExtension;
using AttributeSqlDLL.Model;
using AttributeSqlDLL.SqlAttribute.CudAttr;

namespace AttrSqlDbLite.SqlExtendedMethod
{
    public static class UpdateExtend
    {
        /// <summary>
        /// 根据主键更新
        /// 若不指定主键，则第一个包含ID的字段为主键
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string UpdateField(this AttrEntityBase entity, string PrimaryKey = "")
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"Update {entity.GetType().Name} SET ");
            string Primary = PrimaryKey;
            foreach (var prop in entity.GetType().GetProperties())
            {
                //存一下主键字段(默认名字为ID的或者以ID结尾的)
                if (string.IsNullOrEmpty(Primary))
                {
                    if (prop.Name.ToUpper() == "ID" || prop.Name.ToUpper().EndsWith("ID"))
                    {
                        Primary = prop.Name;
                        //主键字段不做更新，直接跳出
                        continue;
                    }
                }
                if (prop.Name.ToUpper() == PrimaryKey.ToUpper())
                    continue;
                sql.Append($"{prop.Name} = @{prop.Name},");
            }
            if (sql.ToString() == $"Update {entity.GetType().Name} SET ")
            {
                return string.Empty;
            }
            sql = sql.Remove(sql.Length - 1, 1);
            if (string.IsNullOrEmpty(Primary))
            {
                throw new AttrSqlException($"未找到{entity.GetType().Name}表包含ID的主键字段,更新失败！");
            }
            sql.Append($" Where {Primary} = @{Primary}");
            return sql.ToString();
        }
        /// <summary>
        /// 根据实体的字段是否有值来更新有值的字段
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="IngnorIntDefault">int类型的默认值是否忽略,默认忽略</param>
        /// <returns></returns>
        public static string GetUpdateField(this AttrEntityBase entity, string PrimaryKey = "", bool IngnorIntDefault = true)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"Update {entity.GetType().Name} SET ");
            string Primary = PrimaryKey;
            foreach (var prop in entity.GetType().GetProperties())
            {
                //存一下主键字段(默认名字为ID的或者以ID结尾的)
                if (string.IsNullOrEmpty(Primary))
                {
                    if (prop.Name.ToUpper() == "ID" || prop.Name.ToUpper().EndsWith("ID"))
                    {
                        Primary = prop.Name;
                        //主键字段不做更新，直接跳出
                        continue;
                    }
                }
                if (prop.Name.ToUpper() == PrimaryKey.ToUpper())
                    continue;
                //判断实体是否标记了AllowUpdateEmpty属性
                //if (prop.IsDefined(typeof(AllowUpdateEmptyAttribute), true))
                //{
                //    sql.Append($"{prop.Name} = @{prop.Name},");
                //    continue;
                //}
                bool isnull = false;
                //判断当前属性是否有值(主要针对string、int和数组类型)
                object objvalue = prop.GetValue(entity, null);
                if (objvalue == null)
                    continue;
                if (objvalue is string && string.IsNullOrEmpty((string)objvalue))
                    isnull = true;
                else if ((objvalue is int && (int)objvalue == 0) || objvalue is byte && (byte)objvalue == 0 || objvalue is long && (long)objvalue == 0 || objvalue is double && (double)objvalue == 0)
                {
                    if (IngnorIntDefault)
                        isnull = true;
                }
                else if (objvalue is DateTime && (DateTime)objvalue == default(DateTime))
                {
                    isnull = true;
                }
                else if (objvalue is Array)
                {
                    Array array = (Array)objvalue;
                    if (array == null || array.Length == 0)
                        isnull = true;
                }
                //对有值的进行操作
                if (!isnull)
                {
                    sql.Append($"{prop.Name} = @{prop.Name},");
                }
            }
            if (sql.ToString() == $"Update {entity.GetType().Name} SET ")
            {
                return string.Empty;
            }
            sql = sql.Remove(sql.Length - 1, 1);
            if (string.IsNullOrEmpty(Primary))
            {
                throw new AttrSqlException($"未找到{entity.GetType().Name}表包含ID的主键字段,更新失败！");
            }
            sql.Append($" Where {Primary} = @{Primary}");
            return sql.ToString();
        }

        /// <summary>
        /// 根据实体配置为更新条件获取需要更新的字段
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="IngnorIntDefault"></param>
        /// <returns></returns>
        public static string UpdateFieldByEntityCondition(this AttrBaseModel dto, AttrEntityBase entity, bool IngnorIntDefault = true)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"Update {entity.GetType().Name} SET ");
            List<string> condition = new List<string>();
            foreach (var prop in dto.GetType().GetProperties())
            {
                if (prop.IsDefined(typeof(UpdateConditionFieldAttribute), true))
                {
                    condition.Add(prop.Name);
                }
                else
                {
                    //判断实体是否标记了AllowUpdateEmpty属性
                    //if (prop.IsDefined(typeof(AllowUpdateEmptyAttribute), true))
                    //{
                    //    sql.Append($"{prop.Name} = @{prop.Name},");
                    //    continue;
                    //}
                    bool isnull = false;
                    //判断当前属性是否有值(主要针对string、int和数组类型)
                    object objvalue = prop.GetValue(dto, null);
                    if (objvalue == null)
                        continue;
                    if (objvalue is string && string.IsNullOrEmpty((string)objvalue))
                        isnull = true;
                    else if (objvalue is int && (int)objvalue == 0)
                    {
                        if (IngnorIntDefault)
                            isnull = true;
                    }
                    else if (objvalue is DateTime && (DateTime)objvalue == default(DateTime))
                    {
                        isnull = true;
                    }
                    else if (objvalue is Array)
                    {
                        Array array = (Array)objvalue;
                        if (array == null || array.Length == 0)
                            isnull = true;
                    }
                    //对有值的进行操作
                    if (!isnull)
                    {
                        sql.Append($"{prop.Name} = @{prop.Name},");
                    }
                }
            }
            if (sql.ToString() == $"Update {entity.GetType().Name} SET ")
            {
                return string.Empty;
            }
            sql = sql.Remove(sql.Length - 1, 1);
            if (condition.Count == 0)
            {
                throw new AttrSqlException($"未找到{entity.GetType().Name}表的更新条件,更新失败！");
            }
            sql.Append(" WHERE ");
            for (var i = 0; i < condition.Count; i++)
            {
                sql.Append($"{condition[i]} = @{condition[i]}");
                if (i + 1 < condition.Count)
                    sql.Append(" AND ");
            }
            return sql.ToString();
        }       
    }
}
