using UnityEngine;
using System.Net; // 引入 WebClient
using System.IO;
// 如果在编辑器模式下使用 AssetDatabase，仍需引入
#if UNITY_EDITOR
using UnityEditor; 
#endif

public class ContentDownloader
{
    public bool Download(string url, string savePath)
    {
        Debug.Log($"开始使用 WebClient 同步下载: {url} 到 {savePath}");

        try
        {
            // 确保目标文件夹存在
            string directoryPath = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (WebClient client = new WebClient())
            {
                // 使用 WebClient 的同步方法直接下载并保存文件
                client.DownloadFile(url, savePath);
            }

            Debug.Log($"文件保存成功到: {savePath}");

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            
            return true;
        }
        catch (WebException e)
        {
            Debug.LogError($"WebClient 同步下载失败: {url}, 错误: {e.Message}");
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存文件或未知错误: {e.Message}");
            return false;
        }
    }
}