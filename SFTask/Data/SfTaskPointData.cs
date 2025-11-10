using System;
using System.Collections.Generic;
using SFramework.SFTask.Module;

namespace SFramework.SFTask.Data
{
    /// <summary>
    /// 任务数据类
    /// </summary>
    [Serializable]
    public class SfTaskPointData
    {
        /// <summary>
        /// 任务点X坐标
        /// </summary>
        public float x;
        /// <summary>
        /// 任务点Y坐标
        /// </summary>
        public float y;
        /// <summary>
        /// 任务点标题
        /// </summary>
        public string title;
        /// <summary>
        /// 任务列表
        /// </summary>
        public List<SfTaskData> tasks = new List<SfTaskData>();
        /// <summary>
        /// 任务点任务执行类型
        /// </summary>
        public string type;
    }
}