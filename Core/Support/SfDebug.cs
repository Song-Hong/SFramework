using UnityEngine;

namespace SFramework.Core.Support
{
    /// <summary>
    /// 控制台打印
    /// </summary>
    public static class SfDebug
    {
        /// <summary>
        /// 彩色输出
        /// </summary>
        /// <param name="color">色彩值</param>
        /// <param name="info">信息</param>
        public static void ColorLog(Color color,params object[]info)
            => ColorLog(ColorUtility.ToHtmlStringRGB(color),info);
        
        /// <summary>
        /// 彩色输出
        /// </summary>
        /// <param name="color">色彩值</param>
        /// <param name="info">信息</param>
        public static void ColorLog(string color,params object[]info)
            => Log(string.Format("<color={0}>{1}</color>",color,Join(info)));

        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="info">信息</param>
        public static void Log(params object[] info)
            =>Log(Join(info));

        /// <summary>
        /// 拼接字符串
        /// </summary>
        /// <param name="info">信息</param>
        /// <returns>拼接后的字符串</returns>
        private static string Join(params object[] info)
        {
            if(info==null) Log("Null");
            var content = "";
            foreach (var s in info)//拼接
            {
                content += $"{s.ToString()},";
            }
            if (content.EndsWith(","))//消除最后一行的,
            {
                content = content.Remove(content.Length - 1);
            }

            return content;
        }
        
        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="info">输出信息</param>
        private static void Log(string info)
            =>Debug.Log(info);
    }
}