using System;

namespace SFramework.SFNet.Data
{
    /// <summary>
    /// 服务器数据
    /// </summary>
    [System.Serializable]
    public class SfServerData
    {
        /// <summary>
        /// IP
        /// </summary>
        public string ip;
        /// <summary>
        /// 端口号
        /// </summary>
        public int port = 8787;
        /// <summary>
        /// 开启成功
        /// </summary>
        public bool autoStart = true;
        /// <summary>
        /// 打印消息日志
        /// </summary>
        public bool printLog = true;
        /// <summary>
        /// 接收到消息
        /// </summary>
        public  Action<string> Received;
        /// <summary>
        /// 接收到消息 IP Port 消息
        /// </summary>
        public  Action<string,int,string> ReceivedIPPort;
    }
}