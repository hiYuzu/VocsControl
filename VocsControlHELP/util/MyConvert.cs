using System;
using System.Data;
using System.Collections.Generic;

namespace VocsControlHELP.util
{
    public class MyConvert
    {
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("x2") + " ";
                }
            }
            return returnStr;
        }

        #region DataTable转化为List<T>
        /// <summary>
        /// DataTable转List<T>集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="dataTable">数据表</param>
        /// <returns>List<T></returns>
        public static List<T> DataTableToList<T>(DataTable dataTable)
        {
            var list = new List<T>();
            foreach (DataRow item in dataTable.Rows)
            {
                list.Add(DataRowToModel<T>(item));
            }
            return list;
        }

        /// <summary>
        /// 类型枚举
        /// </summary>
        private enum ModelType
        {
            Struct,
            Enum,
            String,
            Object,
            Else
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="modelType">typeof T</param>
        /// <returns>enum</returns>
        private static ModelType GetModelType(Type modelType)
        {
            if (modelType.IsEnum)
            {
                return ModelType.Enum;
            }
            if (modelType.IsValueType)
            {
                return ModelType.Struct;
            }
            if (modelType == typeof(string))
            {
                return ModelType.String;
            }
            return modelType == typeof(object) ? ModelType.Object : ModelType.Else;
        }

        /// <summary>
        /// 单行转换
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="row">DataTable的一行</param>
        /// <returns>单个泛型</returns>
        private static T DataRowToModel<T>(DataRow row)
        {
            T model;
            var type = typeof(T);
            var modelType = GetModelType(type);
            switch (modelType)
            {
                case ModelType.Struct:
                    {
                        model = default(T);
                        if (row[0] != null)
                        {
                            model = (T)row[0];
                        }
                    }
                    break;
                case ModelType.Enum:
                    {
                        model = default(T);
                        if (row[0] != null)
                        {
                            var fiType = row[0].GetType();
                            if (fiType == typeof(int))
                            {
                                model = (T)row[0];
                            }
                            else if (fiType == typeof(string))
                            {
                                model = (T)Enum.Parse(typeof(T), row[0].ToString());
                            }
                        }
                    }
                    break;
                case ModelType.String:
                    {
                        model = default(T);
                        if (row[0] != null)
                            model = (T)row[0];
                    }
                    break;
                case ModelType.Object:
                    {
                        model = default(T);
                        if (row[0] != null)
                        {
                            model = (T)row[0];
                        }
                    }
                    break;
                case ModelType.Else:
                    {
                        model = Activator.CreateInstance<T>();
                        var modelPropertyInfos = type.GetProperties();
                        foreach (var pi in modelPropertyInfos)
                        {
                            var name = pi.Name;
                            if (!row.Table.Columns.Contains(name) || row[name] == null)
                            {
                                continue;
                            }
                            var piType = GetModelType(pi.PropertyType);
                            switch (piType)
                            {
                                case ModelType.Struct:
                                    {
                                        var value = Convert.ChangeType(row[name], pi.PropertyType);
                                        pi.SetValue(model, value, null);
                                    }
                                    break;
                                case ModelType.Enum:
                                    {
                                        var fiType = row[0].GetType();
                                        if (fiType == typeof(int))
                                        {
                                            pi.SetValue(model, row[name], null);
                                        }
                                        else if (fiType == typeof(string))
                                        {
                                            var value = (T)Enum.Parse(typeof(T), row[name].ToString());
                                            if (value != null)
                                            {
                                                pi.SetValue(model, value, null);
                                            }
                                        }
                                    }
                                    break;
                                case ModelType.String:
                                    {
                                        var value = Convert.ChangeType(row[name], pi.PropertyType);
                                        pi.SetValue(model, value, null);
                                    }
                                    break;
                                case ModelType.Object:
                                    {
                                        pi.SetValue(model, row[name], null);
                                    }
                                    break;
                                case ModelType.Else:
                                    throw new Exception("不支持该类型转换");
                                default:
                                    throw new Exception("未知类型");
                            }
                        }
                        break;
                    }
                default:
                    model = default(T);
                    break;
            }
            return model;
        }
        #endregion
    }
}
