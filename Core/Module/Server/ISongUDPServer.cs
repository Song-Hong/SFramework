using System.Collections.Generic;

namespace Song.Core.Module.Server
{
    public interface ISongUDPServer
    {
        #region 发送消息

        /// <summary>
        /// 向指定IP和端口发送消息
        /// </summary>
        public void Send(string ip, int port, string msg);

        /// <summary>
        /// 向当前网段中特定端口发送消息
        /// </summary>
        public void Send(int port, string msg);

        /// <summary>
        /// 向指定端口广播消息
        /// </summary>
        public void SendBroadcast(int port, string msg);

        /// <summary>
        /// 向绑定端口广播消息
        /// </summary>
        public void SendBroadcast(string msg);

        /// <summary>
        /// 通过遍历网段的方式广播消息 (主要用于Android平台)
        /// </summary>
        public void SendBroadcastWithForeach(int port, string msg);

        /// <summary>
        /// 通过遍历指定IP所在网段的方式广播消息
        /// </summary>
        public void SendBroadcastWithForeach(string baseIp, int port, string msg);

        #endregion

        #region 断开连接

        /// <summary>
        /// 断开连接并释放资源
        /// </summary>
        public void Disconnect();

        #endregion
    }
}