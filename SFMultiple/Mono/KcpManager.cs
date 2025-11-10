using UnityEngine;
using System.Text;
using SFramework.SFMultiple.Module; // 引入 SfKcpServer 所在的命名空间
using System.Threading; // 用于在日志中显示线程ID

public class KcpManager : MonoBehaviour
{
    [Tooltip("KCP 服务器监听的端口")]
    public int Port = 40001;
    [Tooltip("KCP Update 帧间隔 (毫秒)")]
    public int FrameInterval = 10;

    private SfKcpServer _kcpServer;

    void Awake()
    {
        // 1. 实例化 KCP 服务器
        _kcpServer = new SfKcpServer();

        // 2. 核心步骤：设置接收消息的回调函数
        _kcpServer.OnReceive = HandleReceivedMessage;
        
        // 打印初始化信息，确认 KcpManager 启动在主线程
        Debug.Log($"KCP Server Manager Initialized (Main Thread: {Thread.CurrentThread.ManagedThreadId}). Listening on port {_kcpServer.port}.");
    }

    /// <summary>
    /// 处理从 KCP 接收到的字节数组。
    /// 这个方法会被 SfKcpServer 在异步线程中调用。
    /// </summary>
    /// <param name="data">接收到的字节数组</param>
    private void HandleReceivedMessage(byte[] data)
    {
        // 线程信息：显示当前执行回调的线程
        string threadInfo = $"({Thread.CurrentThread.ManagedThreadId})";

        if (data == null || data.Length == 0)
        {
            // 收到空数据包，可能是在特定条件下发生的 KCP 内部事件
            Debug.LogWarning($"[KCP 消息回调 {threadInfo}] 收到空数据包，可能为 KCP 内部信号。");
            return;
        }
        
        // 将接收到的 UTF8 字节数组解码成字符串
        string message = Encoding.UTF8.GetString(data);

        // 3. 在 Unity Console 打印消息 (包含内容)
        // 这一行负责打印消息内容！
        Debug.Log($"[KCP 消息成功接收 {threadInfo}] 长度: {data.Length} bytes. 消息内容: {message}");
    }

    /// <summary>
    /// 在应用退出时打印服务器关闭信息
    /// </summary>
    void OnApplicationQuit()
    {
        Debug.Log("KCP Server Manager 正在关闭...");
    }
}