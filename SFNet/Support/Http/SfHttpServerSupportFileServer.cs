using System;
using System.IO;
using System.Net;
using System.Text;
using SFramework.SFNet.Mono;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SFramework.SFNet.Support.Http
{
    /// <summary>
    /// HTTP服务器支持文件服务器
    /// </summary>
    public class SfHttpServerSupportFileServer : SfHttpServerSupportBase
    {
        /// <summary>
        /// 文件服务器根目录路径
        /// </summary>
        // 确保使用 StreamingAssetsPath 作为默认路径
        public string path = Application.streamingAssetsPath;

        /// <summary>
        /// 支持开始
        /// </summary>
        public override void SupportStart()
        {
            // 当路径为空时 自动使用 StreamingAssets 目录
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Application.streamingAssetsPath;
            }
            // 确保文件根目录存在并规范化路径
            path = Path.GetFullPath(path);
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // 注册文件服务器路由
            Debug.Log($"File Server Root: {path}");
            // 提示客户端访问 URL
            Debug.Log($"HTTP Client URL: http://{SfHttpServerMono.Instance.ip}:{SfHttpServerMono.Instance.port}/file-server");
            SfHttpServerMono.Instance.OnRequest += HandleRequest;
        }

        private void Start()
        {
            SupportStart();
        }

        /// <summary>
        /// 处理文件请求 (重构: 明确区分路由)
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <param name="response">HTTP响应</param>
        public void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            // 移除路径开头和结尾的斜杠，保持一致性，例如 "file-server/list"
            var absPath = request.Url.AbsolutePath.Trim('/');
            
            Debug.Log($"[HTTP] Path: {absPath}, Method: {request.HttpMethod}, Query: {request.Url.Query}");

            try
            {
                if (request.HttpMethod == "GET")
                {
                    // 1. 客户端 HTML 页面请求
                    if (absPath == "file-server")
                    {
                        ServeHtmlClient(response);
                        return;
                    }
                    // 2. 文件列表请求: /file-server/list?dir=...
                    else if (absPath == "file-server/list")
                    {
                        // dir 参数用于子目录导航
                        var subPath = request.QueryString["dir"] ?? string.Empty;
                        GetFileList(response, subPath);
                        return;
                    }
                    // 3. 文件详情请求: /file-server/get?file=...
                    else if (absPath == "file-server/get")
                    {
                        var fileName = request.QueryString["file"];
                        GetFileDetails(response, fileName);
                        return;
                    }
                    // 4. 文件下载请求: /file-server/download?file=...
                    else if (absPath == "file-server/download")
                    {
                        var fileName = request.QueryString["file"];
                        DownloadFile(request, response, fileName);
                        return;
                    }

                    // 其他 GET 请求返回 404
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not Found: Invalid GET endpoint.";

                }
                else if (request.HttpMethod == "POST")
                {
                    string fileName = null;
                    
                    // 1. 检查路径变量格式 (兼容旧客户端，解决 404 问题)
                    if (absPath.StartsWith("file-server/upload_file/"))
                    {
                        fileName = absPath.Substring("file-server/upload_file/".Length); // 从路径中获取文件名
                    }
                    
                    // 2. 如果路径精确匹配 /file-server/upload_file (新的规范)
                    else if (absPath == "file-server/upload_file")
                    {
                        // 尝试从查询参数中获取文件名
                        fileName = request.QueryString["file"]; 
                    }

                    // 如果文件名已成功获取到 (来自路径变量或查询参数)，则处理上传
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        UploadFile(request, response, fileName);
                        return;
                    }
                    
                    // 如果路径不匹配任何上传格式或文件名缺失，则返回 404
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not Found: Invalid POST endpoint.";
                }
                else
                {
                    // 不支持的方法
                    response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    response.StatusDescription = "Method Not Allowed";
                }
            }
            catch (Exception ex)
            {
                // 统一的异常处理
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var errorMessage = $"Server Error: {ex.Message}";
                var errorBytes = Encoding.UTF8.GetBytes(errorMessage);
                response.ContentLength64 = errorBytes.Length;
                response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                Debug.LogError($"[HTTP Server Error] {request.Url.AbsolutePath}: {ex}");
            }
            finally
            {
                // 确保关闭输出流
                if (response.OutputStream.CanWrite)
                {
                    response.OutputStream.Close();
                }
            }
        }
        
        /// <summary>
        /// 提供 HTML 客户端文件
        /// </summary>
        private void ServeHtmlClient(HttpListenerResponse response)
        {
             // 假设 file_server.html 放在 Assets/SFramework/SFNet/Support/Http/ 路径下
            var filePath = Path.Combine(Application.dataPath,
                "SFramework/SFNet/Support/Http/file_server.html");

            if (File.Exists(filePath))
            {
                try
                {
                    var bytes = File.ReadAllBytes(filePath);
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = "text/html";
                    response.ContentLength64 = bytes.Length;
                    response.OutputStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    Debug.LogError($"Error reading HTML file: {ex.Message}");
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.StatusDescription = "File not found: file_server.html";
            }
        }

        /// <summary>
        /// 获取文件列表 (实现)
        /// </summary>
        /// <param name="response">HTTP响应</param>
        /// <param name="subPath">请求的子目录相对路径</param>
        public void GetFileList(HttpListenerResponse response, string subPath)
        {
            // 规范化子路径，防止路径遍历攻击 (e.g., ../../)
            var cleanSubPath = subPath.Replace("..", "").Trim('/', '\\');
            var targetPath = Path.Combine(path, cleanSubPath);
            targetPath = Path.GetFullPath(targetPath);

            // 安全检查：确保请求的路径仍在根目录内
            if (!targetPath.StartsWith(path) || targetPath.Length > path.Length + cleanSubPath.Length + 2)
            {
                SendErrorResponse(response, HttpStatusCode.Forbidden, "Access Denied: Path outside root directory.");
                return;
            }
            
            if (!Directory.Exists(targetPath))
            {
                SendErrorResponse(response, HttpStatusCode.NotFound, $"Directory not found: {cleanSubPath}");
                return;
            }

            var entries = new List<object>();

            try
            {
                // 1. 获取所有子目录
                var directories = Directory.GetDirectories(targetPath);
                foreach (var dir in directories)
                {
                    // 排除隐藏目录（可选）
                    if ((File.GetAttributes(dir) & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        
                        entries.Add(new { 
                            name = Path.GetFileName(dir), 
                            dir = true, 
                            size = 0 
                        });
                    }
                }

                // 2. 获取所有文件
                var files = Directory.GetFiles(targetPath);
                foreach (var file in files)
                {
                    if(file.EndsWith(".meta"))continue;
                    var fileInfo = new FileInfo(file);
                    entries.Add(new { 
                        name = Path.GetFileName(file), 
                        dir = false, 
                        size = fileInfo.Length 
                    });
                }

                // 3. 构建成功响应的 JSON 对象 (目录在前，文件在后，按名称排序)
                var responseData = new {
                    ok = true,
                    entries = entries.OrderByDescending(e => (bool)e.GetType().GetProperty("dir").GetValue(e)).ThenBy(e => (string)e.GetType().GetProperty("name").GetValue(e)).ToList(),
                    currentDir = cleanSubPath 
                };

                // 使用简化的 JsonUtility 序列化
                var json = JsonUtility.ToJson(responseData); 
                SendJsonResponse(response, HttpStatusCode.OK, json);

            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting file list: {ex.Message}");
                SendErrorResponse(response, HttpStatusCode.InternalServerError, "Failed to retrieve file list.");
            }
        }
        
        /// <summary>
        /// 获取文件详情 (实现)
        /// </summary>
        public void GetFileDetails(HttpListenerResponse response, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest, "File name is missing.");
                return;
            }

            var cleanFileName = fileName.Replace("..", "").Trim('/', '\\');
            var filePath = Path.Combine(path, cleanFileName);
            filePath = Path.GetFullPath(filePath);

            // 安全检查
            if (!filePath.StartsWith(path))
            {
                SendErrorResponse(response, HttpStatusCode.Forbidden, "Access Denied: File outside root directory.");
                return;
            }

            if (!File.Exists(filePath))
            {
                SendErrorResponse(response, HttpStatusCode.NotFound, $"File not found: {cleanFileName}");
                return;
            }

            try
            {
                var fileInfo = new FileInfo(filePath);

                var responseData = new
                {
                    ok = true,
                    name = fileInfo.Name,
                    path = cleanFileName,
                    size = fileInfo.Length,
                    ext = Path.GetExtension(fileName),
                    lastModified = fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                var json = JsonUtility.ToJson(responseData);
                SendJsonResponse(response, HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting file details: {ex.Message}");
                SendErrorResponse(response, HttpStatusCode.InternalServerError, "Failed to retrieve file details.");
            }
        }

        /// <summary>
        /// 上传文件：从请求体中读取数据并保存到磁盘
        /// </summary>
        public void UploadFile(HttpListenerRequest request, HttpListenerResponse response, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest, "Error: File name is required.");
                return;
            }

            // 显式解码文件名，解决中文文件名在URL编码后显示不正确的问题
            // 确保无论是来自路径变量还是查询参数，文件名的中文都能被正确识别
            string decodedFileName = Uri.UnescapeDataString(fileName);

            // 组合目标路径, 确保使用 Path.GetFileName 避免路径遍历攻击
            var safeFileName = Path.GetFileName(decodedFileName); 
            var targetPath = Path.Combine(path, safeFileName);
            targetPath = Path.GetFullPath(targetPath); // 再次规范化

            // 最终安全检查
            if (!targetPath.StartsWith(path))
            {
                SendErrorResponse(response, HttpStatusCode.Forbidden, "Upload Denied: Invalid target path.");
                return;
            }

            Debug.Log($"Attempting to upload file to: {targetPath}");

            try
            {
                // 使用 using 确保流被正确关闭
                using (var inputStream = request.InputStream)
                using (var outputStream = File.Create(targetPath))
                {
                    inputStream.CopyTo(outputStream);
                }

                // 发送成功响应 (JSON 格式，便于客户端处理)
                // 注意：这里返回的是已经解码的 safeFileName，客户端应该能正确显示
                var successMessage = $"File '{safeFileName}' successfully uploaded.";
                var responseData = new { ok = true, file = safeFileName, message = successMessage };
                var json = JsonUtility.ToJson(responseData);
                SendJsonResponse(response, HttpStatusCode.OK, json);

                Debug.Log(successMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"File upload failed: {ex.Message}";
                Debug.LogError(errorMessage);
                SendErrorResponse(response, HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public void DownloadFile(HttpListenerRequest request, HttpListenerResponse response, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest, "File name is missing.");
                return;
            }

            var cleanFileName = fileName.Replace("..", "").Trim('/', '\\');
            var filePath = Path.Combine(path, cleanFileName);
            filePath = Path.GetFullPath(filePath);

            // 安全检查
            if (!filePath.StartsWith(path))
            {
                SendErrorResponse(response, HttpStatusCode.Forbidden, "Access Denied: File outside root directory.");
                return;
            }
            
            if (File.Exists(filePath))
            {
                try
                {
                    var fileBytes = File.ReadAllBytes(filePath);

                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = GetMimeType(fileName); 
                    response.ContentLength64 = fileBytes.Length;
                    // 设置 Content-Disposition 使得浏览器提示下载
                    response.AddHeader("Content-Disposition", $"attachment; filename=\"{Path.GetFileName(filePath)}\"");

                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during file download: {ex.Message}");
                    SendErrorResponse(response, HttpStatusCode.InternalServerError, "Error during file download.");
                }
            }
            else
            {
                SendErrorResponse(response, HttpStatusCode.NotFound, $"File not found: {cleanFileName}");
            }
        }

        // --- 辅助方法 ---

        /// <summary>
        /// 发送 JSON 格式的响应
        /// </summary>
        private void SendJsonResponse(HttpListenerResponse response, HttpStatusCode statusCode, string jsonContent)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(jsonContent);
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 发送错误响应
        /// </summary>
        private void SendErrorResponse(HttpListenerResponse response, HttpStatusCode statusCode, string message)
        {
            response.StatusCode = (int)statusCode;
            // 尝试返回 JSON 格式的错误，便于客户端解析
            var errorData = new { ok = false, message = message, status = (int)statusCode };
            var json = JsonUtility.ToJson(errorData);
            
            response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }
        
        /// <summary>
        /// 根据文件名获取MIME类型
        /// </summary>
        private static string GetMimeType(string fileName)
        {
            // 获取小写的扩展名
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".zip" => "application/zip",
                ".bin" => "application/octet-stream",
                // 默认类型：二进制流
                _ => "application/octet-stream", 
            };
        }
    }

    // 注意：如果您的项目没有引用 System.Text.Json 或 Newtonsoft.Json，
    // 并且不能使用 UnityEngine.JsonUtility，您需要使用以下简化的存根。
    // 在生产环境中，建议使用成熟的 JSON 库。
    public static class JsonUtility
    {
        public static string ToJson(object obj)
        {
            // 确保只处理简单类型和 List<object>，更复杂的对象需要完整的序列化器
            var type = obj.GetType();
            var props = type.GetProperties();
            var sb = new StringBuilder("{");
            
            foreach(var prop in props)
            {
                var value = prop.GetValue(obj, null);
                sb.Append($"\"{prop.Name}\":");
                
                if (value == null)
                {
                    sb.Append("null,");
                }
                else if (value is string s)
                {
                    sb.Append($"\"{s.Replace("\"", "\\\"")}\",");
                }
                else if (value is bool b)
                {
                    sb.Append(b ? "true," : "false,");
                }
                else if (value is int i)
                {
                    sb.Append($"{i},");
                }
                else if (value is long l)
                {
                    sb.Append($"{l},");
                }
                else if (value is IEnumerable<object> list)
                {
                    sb.Append("[");
                    bool firstItem = true;
                    foreach (var item in list)
                    {
                        if (!firstItem) sb.Append(",");
                        sb.Append(ToJson(item));
                        firstItem = false;
                    }
                    sb.Append("],");
                }
                else // 默认作为字符串处理，或自行扩展
                {
                    sb.Append($"\"{value.ToString().Replace("\"", "\\\"")}\",");
                }
            }
            if (sb.Length > 1 && sb[sb.Length - 1] == ',') sb.Length--; // 移除末尾逗号
            sb.Append("}");
            return sb.ToString();
        }
    }
}