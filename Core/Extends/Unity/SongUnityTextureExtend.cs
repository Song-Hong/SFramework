using UnityEngine;

namespace Song.Core.Extends.Unity
{
    public static class SongUnityTextureExtend
    {
        /// <summary>
        /// 将Texture2D转换为Sprite
        /// </summary>
        /// <param name="texture">Texture2D</param>
        /// <returns>Sprite</returns>
        public static Sprite ToSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1f);
        }
    }
}