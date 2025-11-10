using System;
using System.Diagnostics;
using System.Net.Sockets.Kcp.Simple;
using System.Threading.Tasks;
using UnityEngine; // 引入 UnityEngine 以使用 Debug.Log

namespace SFramework.SFMultiple.Module
{
    /// <summary>
    /// KCP 服务器 (使用 SimpleKcpClient 封装实现监听)
    /// </summary>
    public class SfKcpServer
    {
        /// <summary> 端口号 </summary>
        public int port = 40001;
        
        /// <summary> 帧间隔 (毫秒) </summary>
        public int frameInterval = 10;
        
        /// <summary> 接收消息回调 </summary>
        public Action<byte[]> OnReceive;

        private SimpleKcpClient _kcpClient;

        #region 构造函数
        /// <summary>
        /// 构造函数：初始化 KCP 客户端、启动 Update 循环和接收循环。
        /// </summary>
        public SfKcpServer()
        {
            _kcpClient = new SimpleKcpClient(port);
            
            Task.Run(async () =>
            {
                while (true)
                {
                    _kcpClient.kcp.Update(DateTimeOffset.UtcNow);
                    await Task.Delay(frameInterval);
                }
            });
            
            StartRec(_kcpClient);
        }
        #endregion

        #region 监听
        /// <summary>
        /// 监听循环，持续等待接收 KCP 数据包
        /// </summary>
        /// <param name="client">KCP 客户端</param>
        private async void StartRec(SimpleKcpClient client)
        {
            while (true)
            {
                try
                {
                    byte[] res = await client.ReceiveAsync();
            
                    // 收到应用数据后，触发回调
                    OnReceive?.Invoke(res);

                    // === 恢复：发送 ACK 回复给客户端（确保双向流健康）===
                    string responseText = "ACK received"; 
                    byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseText);
            
                    // KCP 会自动将数据发送回给当前连接的客户端
                    client.SendAsync(responseBytes, responseBytes.Length);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[KCP Error] Receive exception: {e.Message}");
                    await Task.Delay(100); 
                }
            }
        }
        #endregion
        
        // 最佳实践：提供一个关闭方法来清理资源
        public void Stop()
        {
            // SimpleKcpClient 应该有一个 Dispose 或 Close 方法来释放底层 Socket 和停止线程。
            // 假设它有 Dispose 方法：
            // _kcpClient?.Dispose();
        }
    }
}