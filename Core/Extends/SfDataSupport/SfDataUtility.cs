using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SFramework.Core.Extends.SfDataSupport
{
    /// <summary>
    /// 提供 SfFormat 字符串到 C# 对象的反序列化功能。
    /// </summary>
    public static class SfDataUtility
    {
        /// <summary>
        /// 将 SfFormat 字符串转换为指定的 C# 对象 T。
        /// </summary>
        /// <param name="sfFormatString">输入的 SfFormat 文本。</param>
        /// <typeparam name="T">目标对象类型。</typeparam>
        public static T FromSfFormat<T>(string sfFormatString) where T : new()
        {
            var rootNode = SfData.Load(sfFormatString);
            return (T)SfToObject(rootNode, typeof(T));
        }

        /// <summary>
        /// 将 <see cref="SfData"/> 节点递归转换为指定类型对象。
        /// </summary>
        /// <param name="node">输入节点。</param>
        /// <param name="targetType">目标类型。</param>
        private static object SfToObject(SfData node, Type targetType)
        {
            if (node.Type == SfDataType.None)
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
            
            string rawValue = node.ToStringValue();
            if (rawValue == null) rawValue = node.Value?.ToString() ?? ""; // 兜底获取值

            // --- 1. 处理简单值类型 ---
            if (targetType.IsPrimitive || targetType == typeof(string) || targetType.IsEnum || targetType == typeof(float) || targetType == typeof(decimal))
            {
                try
                {
                    if (targetType.IsEnum)
                    {
                        // 尝试通过字符串名称解析枚举
                        return Enum.Parse(targetType, rawValue, true);
                    }
                    if (targetType == typeof(float))
                    {
                         // 专门处理float，使用 InvariantCulture
                         return float.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (targetType == typeof(double))
                    {
                         // 专门处理double，使用 InvariantCulture
                         return double.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    
                    // 使用Convert.ChangeType进行类型转换
                    return Convert.ChangeType(rawValue, targetType);
                }
                catch 
                {
                    // 转换失败，返回默认值
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                }
            }

            // --- 2. 处理数组/List 类型 ---
            if (node.IsArray && (targetType.IsArray || (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))))
            {
                var elementType = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];
                var listInstance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

                foreach (SfData elementNode in node.ArrayList)
                {
                    object element = SfToObject(elementNode, elementType);
                    listInstance.Add(element);
                }

                if (targetType.IsArray)
                {
                    if (elementType == null) return listInstance;
                    var arrayInstance = Array.CreateInstance(elementType, listInstance.Count);
                    listInstance.CopyTo(arrayInstance, 0);
                    return arrayInstance;
                }
                
                return listInstance; 
            }

            // --- 3. 处理对象类型 (POCO) ---
            if (node.IsObject || (node.Type == SfDataType.None && (targetType.IsClass || targetType.IsValueType)))
            {
                object instance = Activator.CreateInstance(targetType);
                // 仅获取 public 实例字段
                FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    string key = field.Name;
                    if (node.Keys.Contains(key))
                    {
                        object fieldValue = SfToObject(node[key], field.FieldType);
                        field.SetValue(instance, fieldValue);
                    }
                }
                return instance;
            }

            return null;
        }
        
        /// <summary>
        /// 将 C# 对象转换为 SfFormat 格式的字符串。
        /// </summary>
        /// <param name="obj">输入对象。</param>
        /// <param name="indent">是否缩进排版。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        public static string ToSfFormat<T>(T obj, bool indent = true)
        {
            SfData rootNode = ObjectToNode(obj);
            return rootNode.Dump(indent);
        }

        /// <summary>
        /// 将对象递归转换为 <see cref="SfData"/> 树。
        /// </summary>
        /// <param name="obj">输入对象。</param>
        private static SfData ObjectToNode(object obj)
        {
            if (obj == null) return new SfData();

            Type type = obj.GetType();

            // --- 1. 处理简单值类型 ---
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum || type == typeof(float) || type == typeof(decimal))
            {
                if (obj is int i) return new SfData(i);
                if (obj is double d) return new SfData(d);
                if (obj is float f) return new SfData(f); // 自动转换为 double
                if (obj is bool b) return new SfData(b);
                // 默认返回字符串，适用于 enum, decimal 等
                return new SfData(obj.ToString());
            }

            // --- 2. 处理数组/List 类型 (IList 接口) ---
            if (obj is IList list)
            {
                SfData arrayNode = new SfData(); 
                foreach (var element in list)
                {
                    arrayNode.Add(ObjectToNode(element));
                }
                return arrayNode;
            }

            // --- 3. 处理对象类型 (POCO) ---
            if (type.IsClass || type.IsValueType)
            {
                SfData objectNode = new SfData(); 
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    objectNode[field.Name] = ObjectToNode(fieldValue); 
                }
                return objectNode;
            }

            return new SfData();
        }
    }
}
