using SFramework.SFNet.Module.Udp;
using SFramework.SFNet.Mono;

namespace SFramework.SFNet.Extends
{
    /// <summary>
    /// UDP 支持接口
    /// </summary>
    public interface ISfUDPSupport
    {
        /// <summary>
        /// 初始化UDP服务器
        /// </summary>
        /// <param name="udpServerSfMono">UDP服务器</param>
        public void Init(SfUDPServer udpServerSfMono);
    }
}