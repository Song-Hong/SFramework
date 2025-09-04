using System;
using UnityEngine.Events;

namespace SFramework.Core.Support.HTTP
{
    [Serializable]
    public class SongHttpServerGetRequest
    {
        /// <summary>
        /// 请求类型
        /// </summary>
        public SongHttpServerRequestType requestType;
        /// <summary>
        /// 请求参数
        /// </summary>
        public string requestParams;
        /// <summary>
        /// 返回数据
        /// </summary>
        public string requestData;
        /// <summary>
        /// 响应处理
        /// </summary>
        public UnityEvent ResponseHandler;
    }
}