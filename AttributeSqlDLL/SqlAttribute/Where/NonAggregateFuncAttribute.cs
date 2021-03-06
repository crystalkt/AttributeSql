﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AttributeSqlDLL.SqlAttribute.Where
{
    /// <summary>
    /// 非聚合函数
    /// </summary>
    public class NonAggregateFuncAttribute : Attribute
    {
        private StringBuilder funcoperate;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="FuncName">非聚合函数名称</param>
        /// <param name="fields">字段集合</param>
        /// <param name="values">字段值</param>
        /// <param name="value"></param>
        /// <param name="optioncode">操作符</param>
        public NonAggregateFuncAttribute(string FuncName, string[] fields = null, string[] values = null, string value = null, string optioncode = "=")
        {
            try
            {
                switch (FuncName.ToUpper())
                {
                    case "CONCAT"://字符串连接函数
                        funcoperate.Append($" {FuncName}");
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (i < fields.Length - 1)
                                funcoperate.Append($" {fields[i]}, ");
                            else
                                funcoperate.Append($" {fields[i]} ");
                        }
                        funcoperate.Append(optioncode);
                        if (optioncode.ToUpper() == "IN" || optioncode.ToUpper() == "NOT IN")
                        {
                            funcoperate.Append(" (");
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (i < values.Length - 1)
                                    funcoperate.Append($" {values[i]}, ");
                                else
                                    funcoperate.Append($" {values[i]} ");
                            }
                            funcoperate.Append(" )");
                        }
                        else if (optioncode.ToUpper() == "=")
                        {
                            funcoperate.Append($" '{value}'");
                        }
                        else //if (optioncode.ToUpper() == "LIKE")
                        {
                            funcoperate.Append($" '%{value}%'");
                        }
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            catch (NullReferenceException ex)
            {
                throw new Exception("需要的条件值为空，请检查模型端特性[NonAggregateFuncAttribute]的参数配置！");
            }
            catch (ArgumentException ex)
            {
                throw new Exception("未定义该函数的操作！");
            }

        }
        public string GetFuncoperate()
        {
            return funcoperate.ToString();
        }
    }
}
