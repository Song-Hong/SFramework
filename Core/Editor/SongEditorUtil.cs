using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Song.Core.Editor
{
    public static class SongEditorUtil
    {
        /// <summary>
        /// 获取指定基类的所有非抽象子类。
        /// </summary>
        /// <typeparam name="TBaseType">基类类型。</typeparam>
        /// <returns>所有非抽象子类的 Type 列表。</returns>
        public static List<Type> FindAllSubclassesOf<TBaseType>()
        {
            var subclasses = new List<Type>();
            var baseType = typeof(TBaseType);

            // 获取当前应用程序域中所有已加载的程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    // 获取程序集中的所有类型
                    var types = assembly.GetTypes();

                    foreach (var type in types)
                    {
                        // 检查是否是基类的子类，且不是基类本身，也不是抽象类
                        if (baseType.IsAssignableFrom(type) && type != baseType && !type.IsAbstract)
                        {
                            subclasses.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // 在 Unity 中，有时会遇到加载类型失败的情况，特别是与编辑器相关的程序集。
                    // 捕获此异常可以让你检查 LoaderExceptions 来了解具体原因。
                    Debug.LogWarning($"Failed to load types from assembly: {assembly.FullName}. Some types might be skipped. Errors: ");
                    foreach (Exception loaderEx in ex.LoaderExceptions)
                    {
                        Debug.LogWarning($"- {loaderEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An unexpected error occurred while processing assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return subclasses;
        }
    }
}