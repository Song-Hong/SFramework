using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SFramework.Core.Editor.Support
{
    /// <summary>
    /// SFramework 反射工具集
    /// </summary>
    public static class SfReflection
    {
        /// <summary>
        /// 使用纯反射获取所有继承自 T 的 Runtime 类型
        /// </summary>
        public static List<Type> GetRuntimeSubclasses<T>()
        {
            var baseType = typeof(T);
            var result = new List<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var runtimeAssemblies = allAssemblies.Where(asm =>
                !asm.IsDynamic &&
                !asm.FullName.Contains("Editor")
            );

            foreach (var asm in runtimeAssemblies)
            {
                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        if (type.IsClass &&
                            !type.IsAbstract &&
                            baseType.IsAssignableFrom(type) &&
                            type != baseType)
                        {
                            result.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogWarning($"[RuntimeTypeFinder] 无法完全加载程序集: {asm.FullName}. 错误: {ex.Message}");
                    foreach (var type in ex.Types.Where(t => t != null))
                    {
                        if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type) && type != baseType)
                        {
                            result.Add(type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[RuntimeTypeFinder] 遍历程序集时出错: {asm.FullName}. 错误: {ex.Message}");
                }
            }

            return result.Distinct().ToList(); // 确保唯一性
        }

        // ----------------------------------------------------
        // 2. 核心反射代码 (获取字段的方法)
        // ----------------------------------------------------
        /// <summary>
        /// 使用反射获取一个对象实例的所有公开字段及其值
        /// </summary>
        /// <param name="objInstance">要检查的对象实例（如果是静态字段，可传入 Type 对象）</param>
        /// <returns>包含字段名称、类型名称和值的 Tuple 列表</returns>
        public static List<Tuple<string, string, string>> GetPublicFields(object objInstance)
        {
            // 检查传入对象是否为空
            if (objInstance == null)
            {
                // 如果是 null，则无法获取实例字段，但仍然可以获取静态字段的元数据，但这里为了获取值，直接返回空列表更安全。
                return new List<Tuple<string, string, string>>();
            }

            // 确定目标类型
            Type targetType = objInstance.GetType();

            // 如果传入的是 Type 对象本身，则将 targetType 设为 Type，objInstance 设为 null (仅用于处理静态字段)
            if (objInstance is Type)
            {
                targetType = (Type)objInstance;
                objInstance = null; // 实例字段将无法获取值
            }

            // 全部公开字段
            var allFields = new List<Tuple<string, string, string>>();

            // 定义字段的类型：同时获取公共的实例字段和静态字段
            const BindingFlags flags = BindingFlags.Public |
                                       BindingFlags.Instance |
                                       BindingFlags.Static |
                                       BindingFlags.FlattenHierarchy;

            // 获取字段
            var publicFields = targetType.GetFields(flags);

            if (publicFields.Length == 0)
            {
                return allFields;
            }

            foreach (var field in publicFields)
            {
                // 1. 确定访问类型 (仅用于描述)
                var access = field.IsStatic ? "public static" : "public instance";

                // 2. 检查继承信息 (仅用于描述)
                if (field.DeclaringType != null)
                {
                    var inherited = field.DeclaringType != targetType ? " (继承自 " + field.DeclaringType.Name + ")" : "";
                }

                // 3. 核心：安全地获取字段值
                object fieldValue;

                if (field.IsStatic)
                {
                    // 静态字段：传入 null 获取值
                    fieldValue = field.GetValue(null);
                }
                else if (objInstance != null)
                {
                    // 实例字段：传入对象实例获取值
                    fieldValue = field.GetValue(objInstance);
                }
                else
                {
                    // 无法获取实例字段的值 (因为 objInstance 为 null)
                    fieldValue = "[无法获取值]";
                }

                // 4. 将值转换为字符串，然后添加到列表中
                allFields.Add(new Tuple<string, string, string>(
                    field.Name,
                    field.FieldType.Name,
                    fieldValue?.ToString() ?? "null" // 处理值为 null 的情况
                ));
            }

            return allFields;
        }
    }
}