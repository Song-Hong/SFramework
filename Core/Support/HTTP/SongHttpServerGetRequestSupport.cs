using System.Collections.Generic;
using SFramework.Core.Mono;
using UnityEngine;

namespace SFramework.Core.Support.HTTP
{
    /// <summary>
    /// HTTP 接口文件管理器
    /// </summary>
    public class SongHttpServerGetRequestSupport:MonoBehaviour,ISongHttpServerSupport
    {
        public List<SongHttpServerGetRequest> requests = new List<SongHttpServerGetRequest>();
        
        public void Init()
        {
            // 设置监听事件
            SongHttpServerMono.Instance.OnRequest += (request, response) =>
            {
                var url = request.Url.AbsolutePath.Substring(1, request.Url.AbsolutePath.Length - 1);
                foreach (var serverGetRequest in requests)
                {
                    if (serverGetRequest.requestParams == url)
                    {
                        serverGetRequest.ResponseHandler?.Invoke();
                        switch (serverGetRequest.requestType)
                        {
                            case SongHttpServerRequestType.Text:
                                SongHttpServerMono.Instance.SendTextResponse(response, serverGetRequest.requestData);
                                break;
                            // case SongHttpServerRequestType.Json:
                            //     break;
                            // case SongHttpServerRequestType.File:
                            //     break;
                            // case SongHttpServerRequestType.Html:
                            //     break;
                            default:
                                break;
                        }
                    }
                }
            };
        }
    }
}