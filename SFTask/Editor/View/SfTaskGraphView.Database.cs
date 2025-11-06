using System;
using System.Collections.Generic;
using SFramework.Core.Editor.Support;
using SFramework.SFTask.Module;
using UnityEditor.Experimental.GraphView;

namespace SFramework.SFTask.Editor.View
{
    /// <summary>
    /// 任务图视图数据库
    /// </summary>
    public partial class SfTaskGraphView:GraphView
    {
        /// <summary>
        /// 节点数据库
        /// </summary>
        private List<Tuple<string,List<Tuple<string,string>>>> _nodes = new List<Tuple<string,List<Tuple<string,string>>>> ();

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void InitNodes()
        {
            // 初始化节点数据库
            foreach (var runtimeSubclass in SfReflection.GetRuntimeSubclasses<SfTaskNode>())
            {
                //获取所有公开的字段
                var publicFields = SfReflection.GetPublicFields(runtimeSubclass);
                publicFields.RemoveAll(x => x.Item1 == "isComplete");
                
                //获取节点的名称
                var createdInstance = System.Activator.CreateInstance(runtimeSubclass) as SfTaskNode;
                if (createdInstance == null) continue;
                _nodes.Add(new Tuple<string,List<Tuple<string,string>>>(createdInstance.GetTaskNodeName(),publicFields));
            }
        }
    }
}