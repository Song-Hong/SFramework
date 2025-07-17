using UnityEngine;

namespace Song.Core.Extends.Unity
{
    public static class SongUnityVectorUtil
    {
        /// <summary>
        /// 给Vector3赋值字符串
        /// </summary>
        /// <param name="vector3"></param>
        public static Vector3 String2Vector3(string x,string y,string z)
        {
            var vector3 = new Vector3();
            float.TryParse(x, out var xValue);
            float.TryParse(y, out var yValue);
            float.TryParse(z, out var zValue);
            vector3.x = xValue;
            vector3.y = yValue;
            vector3.z = zValue;
            return vector3;
        }
    }
}