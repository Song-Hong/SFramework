using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;

namespace SFramework.Core.Editor.SongConfig
{
    /// <summary>
    /// SongConfig 一键生成配置文件
    /// </summary>
    public class SongConfigEditor : EditorWindow
    {
        public static string outputPath = Application.streamingAssetsPath;
        
        [MenuItem("Song/生成配置文件编辑器")]
        public static void ShowWindow()
        {
            if (!outputPath.EndsWith("/"))
                outputPath += "/";
            
            // 确保输出目录存在
            var directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                if (directory != null) Directory.CreateDirectory(directory);
            }
            outputPath+="/SongConfig.json";

            // 用于存储所有配置的字典
            var allConfigs = new Dictionary<string, object>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();

                    foreach (var type in types)
                    {
                        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance); 
                        
                        bool hasSongConfigField = false;
                        foreach (var field in fields)
                        {
                            if (Attribute.IsDefined(field, typeof(Module.Config.SongConfig)))
                            {
                                hasSongConfigField = true;
                                break;
                            }
                        }

                        if (!hasSongConfigField) continue; 
                        
                        object instance = null;
                        try
                        {
                            instance = Activator.CreateInstance(type);
                        }
                        catch (MissingMethodException)
                        {
                            Debug.LogWarning($"类型 {type.Name} 没有公共无参构造函数，无法创建实例获取字段值。跳过此类型。");
                            continue; 
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"创建 {type.Name} 实例时发生错误: {ex.Message}。跳过此类型。");
                            continue;
                        }
                        
                        var typeConfig = new Dictionary<string, object>();

                        foreach (var field in fields)
                        {
                            if (!Attribute.IsDefined(field, typeof(Module.Config.SongConfig))) continue;
                            var value = field.GetValue(instance) ?? "";
                            typeConfig[field.Name] = value;
                        }
                        
                        allConfigs[type.FullName] = typeConfig; 
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogError($"加载程序集失败 {assembly.FullName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理程序集时发生意外错误 {assembly.FullName}: {ex.Message}");
                }
            }

            try
            {
                var json = JsonConvert.SerializeObject(allConfigs, Formatting.Indented);
                File.WriteAllText(outputPath, json);
                Debug.Log($"配置文件生成成功: {outputPath}");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成 JSON 文件失败: {ex.Message}");
            }
        }
    }
}