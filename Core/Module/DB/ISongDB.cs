namespace SFramework.Core.Extends.DB
{
    /// <summary>
    /// 数据库接口
    /// </summary>
    public interface ISongDB
    {
        /// <summary>
        /// 连接数据库
        /// </summary>
        public ISongDB Connect();
        
        /// <summary>
        /// 断开数据库连接
        /// </summary>
        public ISongDB Disconnect();
    }
}