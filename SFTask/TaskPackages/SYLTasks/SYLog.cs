using System.Threading.Tasks;
using SFramework.SFTask.Module;
using UnityEngine;

namespace SFramework.SFTask.TaskPackages.SYLTasks
{
    /// <summary>
    /// 日志任务
    /// </summary>
    public class SyLog:SfTaskNode
    {
        public string content;

        public override async Task<int> Start()
        {
            await Task.Delay(1000);
            Debug.Log(content);
            return await base.Start();
        }
    }
}