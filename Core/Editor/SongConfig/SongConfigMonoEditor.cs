using System.IO;
using SFramework.Core.Editor.Server;
using SFramework.Core.Module.Enum;
using SFramework.Core.Mono;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.Editor.SongConfig
{
    /// <summary>
    /// SongConfigMono 编辑器
    /// </summary>
    [CustomEditor(typeof(SongConfigMono))]
    public class SongConfigMonoEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("生成配置文件"))
            {
                var songConfig = (SongConfigMono)target;
                var path = songConfig.configDirPath switch
                {
                    DirPath.StreamingAssets => Application.streamingAssetsPath,
                    DirPath.PersistentDataPath => Application.persistentDataPath,
                    DirPath.Resources => Application.dataPath,
                };
                SongConfigEditor.outputPath = path;
                SongConfigEditor.ShowWindow();
            }
        }
    }
}