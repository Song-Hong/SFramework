using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.SFTask.Data
{
    /// <summary>
    /// 任务列表数据类
    /// </summary>
    [Serializable]
    public class SfTaskListData
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        public List<SfTaskPointData> tasks = new List<SfTaskPointData>();
    }
}