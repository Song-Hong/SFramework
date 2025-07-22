using Song.Core.Module; // 假设你的 SongConfig 特性和相关类型在这里
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using LitJson;
using SFramework.Core.Module.Enum;
using UnityEngine.Serialization;

namespace SFramework.Core.Mono
{
    /// <summary>
    /// Config文件配置
    /// </summary>
    public class SongConfigMono : MonoSingleton<SongConfigMono>
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        [Header("配置文件路径")] public DirPath configDirPath = DirPath.StreamingAssets;

        /// <summary>
        /// 配置文件数据
        /// </summary>
        private Dictionary<string, Dictionary<string, object>> _loadedConfig;

        protected override void Awake()
        {
            base.Awake();
            LoadConfig();
            ApplyConfigToComponents();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                var jsonContent = configDirPath switch
                {
                    DirPath.StreamingAssets => File.ReadAllText(Application.streamingAssetsPath + "/SFConfig/SongConfig.json"),
                    DirPath.Resources => Resources.Load<TextAsset>("/SFConfig/SongConfig").text,
                    DirPath.PersistentDataPath => File.ReadAllText(Application.persistentDataPath + "/SFConfig/SongConfig.json"),
                    _ => throw new ArgumentOutOfRangeException()
                };

                _loadedConfig = JsonMapper.ToObject<Dictionary<string, Dictionary<string, object>>>(jsonContent);

                if (_loadedConfig == null)
                {
                    Debug.LogError("[SongConfigMono] 反序列化配置文件 JSON 失败。请检查 JSON 格式。");
                }
                else
                {
                    Debug.Log("[SongConfigMono] 配置加载成功。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SongConfigMono] 加载配置文件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 使用反射将配置动态赋值给组件或相关类的字段
        /// </summary>
        private void ApplyConfigToComponents()
        {
            if (_loadedConfig == null || _loadedConfig.Count == 0)
            {
                Debug.LogWarning("[SongConfigMono] 没有加载配置或配置为空。跳过应用。");
                return;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (_loadedConfig.TryGetValue(type.FullName, out Dictionary<string, object> typeConfig))
                        {
                            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                            var targetMono = FindObjectOfType(type) as MonoBehaviour;
                            object targetInstance = targetMono;

                            if (targetInstance == null)
                            {
                                Debug.LogWarning($"[SongConfigMono] 在场景中没有找到类型 '{type.FullName}' 的活跃实例来应用配置。跳过。");
                                continue;
                            }

                            foreach (var field in fields)
                            {
                                if (Attribute.IsDefined(field, typeof(Module.Config.SongConfig)))
                                {
                                    if (typeConfig.TryGetValue(field.Name, out var configValue))
                                    {
                                        try
                                        {
                                            var convertedValue = Convert.ChangeType(configValue, field.FieldType);
                                            field.SetValue(targetInstance, convertedValue);
                                            Debug.Log(
                                                $"[SongConfigMono] 已将 {field.Name} = {convertedValue} 应用到 {type.Name}。");
                                        }
                                        catch (InvalidCastException)
                                        {
                                            Debug.LogError(
                                                $"[SongConfigMono] 类型 '{type.Name}' 中字段 '{field.Name}' 的类型不匹配。预期类型为 {field.FieldType}，实际得到 {configValue.GetType()}。");
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.LogError(
                                                $"[SongConfigMono] 设置 '{type.Name}' 上字段 '{field.Name}' 时出错: {ex.Message}");
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning(
                                            $"[SongConfigMono] 类型 '{type.Name}' 的配置中缺少字段 '{field.Name}' 的值。");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogError($"[SongConfigMono] 从程序集 {assembly.FullName} 加载类型失败: {ex.Message}");
                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        Debug.LogError($"  加载器异常: {loaderEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SongConfigMono] 处理程序集 {assembly.FullName} 时发生意外错误: {ex.Message}");
                }
            }
        }
    }
}