using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace SFramework.SFTask.Data
{
    /// <summary>
    /// 任务数据类
    /// </summary>
    [Serializable]
    public class SfTaskData
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string taskName;

        /// <summary>
        /// 任务类型
        /// </summary>
        public string taskType;

        /// <summary>
        /// 任务组件字段
        /// </summary>
        public List<SfTaskFieldData> fields = new List<SfTaskFieldData>();
    }
}