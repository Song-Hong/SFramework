using System.IO;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace Song.Core.Editor
{
    /// <summary>
    /// 同步Song工具包代码
    /// </summary>
    public class SyncSongCore:UnityEditor.Editor
    {
        [MenuItem("Song/推送至Local")]
        public static void Pull()
        {
            var dataPath = "F:/SongUnityProject/song-tools/Song/Core/";
            var exists = Directory.Exists(Application.dataPath + "/Scripts/Core/");
            var path = exists ? Application.dataPath + "/Scripts/Core/": Application.dataPath + "/Song/Core/";
            Debug.Log(path);

            if (!Directory.Exists(dataPath))
            {
                Debug.LogError("你无法进行此操作");
                return;
            }

            if (Directory.Exists(dataPath))
            {
                Directory.Delete(dataPath, true);
            }
            if (Directory.Exists(path))
            {
                FileUtil.CopyFileOrDirectory(path, dataPath);
            }
            Debug.Log("同步完成");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Song/拉取自Local")]
        public static void Push()
        {
            var path = "F:/SongUnityProject/song-tools/Song/Core";
            var exists = Directory.Exists(Application.dataPath + "/Scripts/Core/");
            var dataPath = exists ? Application.dataPath + "/Scripts/Core/": Application.dataPath + "/Song/Core/";

            if (!Directory.Exists(path))
            {
                Debug.LogError("你无法进行此操作");
                return;
            }
            
            if (Directory.Exists(dataPath))
            {
                Directory.Delete(dataPath, true);
            }
            if (Directory.Exists(path))
            {
                FileUtil.CopyFileOrDirectory(path, dataPath);
            }
            Debug.Log("同步完成");
            AssetDatabase.Refresh();
        }
    }
}