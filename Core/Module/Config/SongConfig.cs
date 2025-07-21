using System;

namespace SFramework.Core.Module.Config
{
    /// <summary>
    /// SF 配置文件特性
    /// 1. 标记在字段或属性上可以直接设置为配置文件，自动读取初始化
    /// </summary>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public class SongConfig:Attribute
    {
        
    }
}