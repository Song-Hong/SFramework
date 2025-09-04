using System.IO;
using System.Net;
using SFramework.Core.Mono;
using UnityEngine;

namespace SFramework.Core.Support.HTTP
{
    /// <summary>
    /// HTTP 接口文件管理器
    /// </summary>
    public class SongHttpServerFileSupport : MonoBehaviour, ISongHttpServerSupport
    {
        /// <summary>
        /// 文件类型
        /// </summary>
        public enum FileType
        {
            /// <summary>
            /// 项目文件
            /// </summary>
            Assets,

            /// <summary>
            /// 流文件
            /// </summary>
            StreamingAssets,

            /// <summary>
            /// 持久化文件
            /// </summary>
            PersistentDataPath,

            /// <summary>
            /// 自定义文件
            /// </summary>
            CustomDataPath,
        }

        /// <summary>
        /// 文件类型
        /// </summary>
        public FileType fileType = FileType.StreamingAssets;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string filePath = "";

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            // 设置文件路径
            filePath = fileType switch
            {
                FileType.Assets => Application.dataPath,
                FileType.StreamingAssets => Application.streamingAssetsPath,
                FileType.PersistentDataPath => Application.persistentDataPath,
                _ => filePath,
            };
            // 设置监听事件
            SongHttpServerMono.Instance.OnRequest += (request, response) =>
            {
                // 处理请求
                var file = filePath + request.Url.AbsolutePath;
                // 返回文件
                if (request.Url.AbsolutePath.Contains("songFileManager"))
                {
                    var replace = request.Url.AbsolutePath.Replace("songFileManager/", "");
                    file = filePath + replace;
                    SongHttpServerMono.Instance.SendFileResponse(response, file);
                    return;
                }

                if (string.CompareOrdinal(request.Url.AbsolutePath, "/") == 0)
                    return;
                
                // 读取文件
                if (File.Exists(file))
                {
                    DownloadFileHtml(response, request.Url.AbsolutePath);
                }
                else
                {
                    SongHttpServerMono.Instance.SendTextResponse(response, "File not found", HttpStatusCode.NotFound);
                }
            };
        }

        /// <summary>
        /// 下载文件界面
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void DownloadFileHtml(HttpListenerResponse response, string filePath)
        {
            var html =
                "<!doctype html>\n"+
                "<html lang=\"zh-Hans\">\n"+
                "<head>\n"+
                "<meta charset=\"UTF-8\">\n"+
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n"+
                "<title>文件下载</title>\n"+
                "</head>\n"+
                "<body>\n"+
                "\t<p>文件即将开始下载，如果没有开始点击下方按钮开始下载文件。</p>\n"+
                "\t<button id=\"downloadBtn\">下载文件</button>\n"+
                "\t<script>\n"+
                "\tdocument.addEventListener('DOMContentLoaded', function() {\n"+
                "\tconst downloadBtn = document.getElementById('downloadBtn');\n"+
                "\tdownloadBtn.addEventListener('click', function() {\n"+
                "\tdownloadFile();\n"+
                "\t});\n"+
                "\tfunction downloadFile() {\n"+
                "\tconst fileUrl = 'SERVERIP';\n"+
                "\twindow.open(fileUrl);\n"+
                "\t}\n"+
                "\tdownloadFile();\n"+
                "</script>\n"+
                "</body>\n"+
                "</html>";
            html = html.Replace("SERVERIP","http://"+SongHttpServerMono.Instance.ip+":"+SongHttpServerMono.Instance.port+"/songFileManager"+filePath);
            html = html.Replace("FILENAME",filePath);
            // 发送文件响应
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/html";
            var buffer = System.Text.Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}