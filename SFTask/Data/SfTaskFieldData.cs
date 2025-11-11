using System;

namespace SFramework.SFTask.Data
{
    /// <summary>
    /// 任务组件数据类
    /// </summary>
    [Serializable]
    public class SfTaskFieldData
    {
        /// <summary>
        /// 任务组件字段名称
        /// </summary>
        public string fieldName;
        /// <summary>
        /// 任务组件字段类型
        /// </summary>
        public string fieldType;
        /// <summary>
        /// 任务组件字段值
        /// </summary>
        public string fieldValue;
    }
}