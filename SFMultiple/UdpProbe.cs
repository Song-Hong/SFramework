using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UdpProbe : MonoBehaviour
{
    UdpClient udp;
    IPEndPoint ep;

    void Start()
    {
        udp = new UdpClient(new IPEndPoint(IPAddress.Any, 40001));
        ep = new IPEndPoint(IPAddress.Any, 0);
        Thread t = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    var data = udp.Receive(ref ep);
                    var msg = Encoding.UTF8.GetString(data);
                    Debug.Log($"[UDP 收到] {ep.Address}:{ep.Port} len={data.Length} msg={msg}");
                }
                catch { break; }
            }
        });
        t.IsBackground = true;
        t.Start();
        Debug.Log("UDPProbe 监听 0.0.0.0:40001");
    }

    void OnApplicationQuit() { udp?.Close(); }
}