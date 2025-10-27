using System;
using UnityEngine;

namespace SFramework.Core.Support
{
    /// <summary>
    /// more colors 更多颜色
    /// </summary>
    public static class SfColor
    {
        //Tools 工具
        /// <summary>
        /// hex value change to unity color
        /// hex值转为unity颜色
        /// </summary>
        /// <param name="hex">hex</param>
        /// <returns>Unity Color  unity颜色</returns>
        public static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');

            if (hex.Length == 3) hex += hex;
            if(hex.Length<6) return Color.clear;
            
            // Extract red, green and blue components
            var r = hex.Substring(0, 2);
            var g = hex.Substring(2, 2);
            var b = hex.Substring(4, 2);
  
            // Convert to decimal
            var ri = Convert.ToInt32(r, 16);
            var gi = Convert.ToInt32(g, 16);  
            var bi = Convert.ToInt32(b, 16);
            
            // Convert to Color
            var color = new Color(ri/255f, gi/255f, bi/255f);

            return color;
        }

        /// <summary>
        /// hex value converted to unity color added Alpha channel
        /// hex值转为unity颜色添加透明度
        /// </summary>
        /// <param name="hex">hex</param>
        /// <param name="alpha">Alpha value 透明度值</param>
        /// <returns>Unity Color  unity Alpha Alpha channel 颜色添加透明度</returns>
        public static Color HexToColorAlpha(string hex,float alpha)
        {
            var color = HexToColor(hex);
            return new Color(color.r,color.g,color.b,alpha);
        }
        //preColor 预制色
        
        /// <summary>
        /// #0085FF
        /// </summary>
        /// <returns></returns>
        public static string KleinBlueHex() => "#0085FF";
        
        /// <summary>
        /// #0085FF Klein Blue 克莱因蓝 
        /// </summary>
        /// <returns></returns>
        public static Color KleinBlue() => new Color(0,0.521569f,1);
        
        /// <summary>
        /// #FFB6C1 LightPink 浅粉红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightPinkHex() => "#FFB6C1";

        /// <summary>
        /// #FFB6C1 LightPink 浅粉红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightPink() => new Color(1,0.713725f,0.756863f);

        /// <summary>
        /// #FFC0CB Pink 粉红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PinkHex() => "#FFC0CB";

        /// <summary>
        /// #FFC0CB Pink 粉红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Pink() => new Color(1,0.752941f,0.796078f);

        /// <summary>
        /// #DC143C Crimson 深红(猩红)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string CrimsonHex() => "#DC143C";

        /// <summary>
        /// #DC143C Crimson 深红(猩红)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Crimson() => new Color(0.862745f,0.078431f,0.235294f);

        /// <summary>
        /// #FFF0F5 LavenderBlush 淡紫红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LavenderBlushHex() => "#FFF0F5";

        /// <summary>
        /// #FFF0F5 LavenderBlush 淡紫红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LavenderBlush() => new Color(1,0.941176f,0.960784f);

        /// <summary>
        /// #DB7093 PaleVioletRed 弱紫罗兰红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PaleVioletRedHex() => "#DB7093";

        /// <summary>
        /// #DB7093 PaleVioletRed 弱紫罗兰红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color PaleVioletRed() => new Color(0.858824f,0.439216f,0.576471f);

        /// <summary>
        /// #FF69B4 HotPink 热情的粉红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string HotPinkHex() => "#FF69B4";

        /// <summary>
        /// #FF69B4 HotPink 热情的粉红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color HotPink() => new Color(1,0.411765f,0.705882f);

        /// <summary>
        /// #FF1493 DeepPink 深粉红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DeepPinkHex() => "#FF1493";

        /// <summary>
        /// #FF1493 DeepPink 深粉红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DeepPink() => new Color(1,0.078431f,0.576471f);

        /// <summary>
        /// #C71585 MediumVioletRed 中紫罗兰红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumVioletRedHex() => "#C71585";

        /// <summary>
        /// #C71585 MediumVioletRed 中紫罗兰红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumVioletRed() => new Color(0.780392f,0.082353f,0.521569f);

        /// <summary>
        /// #DA70D6 Orchid 暗紫色(兰花紫)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string OrchidHex() => "#DA70D6";

        /// <summary>
        /// #DA70D6 Orchid 暗紫色(兰花紫)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Orchid() => new Color(0.854902f,0.439216f,0.839216f);

        /// <summary>
        /// #D8BFD8 Thistle 蓟色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string ThistleHex() => "#D8BFD8";

        /// <summary>
        /// #D8BFD8 Thistle 蓟色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Thistle() => new Color(0.847059f,0.74902f,0.847059f);

        /// <summary>
        /// #DDA0DD Plum 洋李色(李子紫)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PlumHex() => "#DDA0DD";

        /// <summary>
        /// #DDA0DD Plum 洋李色(李子紫)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Plum() => new Color(0.866667f,0.627451f,0.866667f);

        /// <summary>
        /// #EE82EE Violet 紫罗兰
        /// </summary>
        /// <returns>HEX值</returns>
        public static string VioletHex() => "#EE82EE";

        /// <summary>
        /// #EE82EE Violet 紫罗兰
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Violet() => new Color(0.933333f,0.509804f,0.933333f);

        /// <summary>
        /// #FF00FF Magenta 洋红(玫瑰红)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MagentaHex() => "#FF00FF";

        /// <summary>
        /// #FF00FF Magenta 洋红(玫瑰红)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Magenta() => new Color(1,0,1);

        /// <summary>
        /// #FF00FF Fuchsia 紫红(灯笼海棠)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string FuchsiaHex() => "#FF00FF";

        /// <summary>
        /// #FF00FF Fuchsia 紫红(灯笼海棠)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Fuchsia() => new Color(1,0,1);

        /// <summary>
        /// #8B008B DarkMagenta 深洋红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkMagentaHex() => "#8B008B";

        /// <summary>
        /// #8B008B DarkMagenta 深洋红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkMagenta() => new Color(0.545098f,0,0.545098f);

        /// <summary>
        /// #800080 Purple 紫色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PurpleHex() => "#800080";

        /// <summary>
        /// #800080 Purple 紫色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Purple() => new Color(0.501961f,0,0.501961f);

        /// <summary>
        /// #BA55D3 MediumOrchid 中兰花紫
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumOrchidHex() => "#BA55D3";

        /// <summary>
        /// #BA55D3 MediumOrchid 中兰花紫
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumOrchid() => new Color(0.729412f,0.333333f,0.827451f);

        /// <summary>
        /// #9400D3 DarkViolet 暗紫罗兰
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkVioletHex() => "#9400D3";

        /// <summary>
        /// #9400D3 DarkViolet 暗紫罗兰
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkViolet() => new Color(0.580392f,0,0.827451f);

        /// <summary>
        /// #9932CC DarkOrchid 暗兰花紫
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkOrchidHex() => "#9932CC";

        /// <summary>
        /// #9932CC DarkOrchid 暗兰花紫
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkOrchid() => new Color(0.6f,0.196078f,0.8f);

        /// <summary>
        /// #4B0082 Indigo 靛青/紫兰色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string IndigoHex() => "#4B0082";

        /// <summary>
        /// #4B0082 Indigo 靛青/紫兰色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Indigo() => new Color(0.294118f,0,0.509804f);

        /// <summary>
        /// #8A2BE2 BlueViolet 蓝紫罗兰
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BlueVioletHex() => "#8A2BE2";

        /// <summary>
        /// #8A2BE2 BlueViolet 蓝紫罗兰
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color BlueViolet() => new Color(0.541176f,0.168627f,0.886275f);

        /// <summary>
        /// #9370DB MediumPurple 中紫色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumPurpleHex() => "#9370DB";

        /// <summary>
        /// #9370DB MediumPurple 中紫色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumPurple() => new Color(0.576471f,0.439216f,0.858824f);

        /// <summary>
        /// #7B68EE MediumSlateBlue 中暗蓝色(中板岩蓝)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumSlateBlueHex() => "#7B68EE";

        /// <summary>
        /// #7B68EE MediumSlateBlue 中暗蓝色(中板岩蓝)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumSlateBlue() => new Color(0.482353f,0.407843f,0.933333f);

        /// <summary>
        /// #6A5ACD SlateBlue 石蓝色(板岩蓝)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SlateBlueHex() => "#6A5ACD";

        /// <summary>
        /// #6A5ACD SlateBlue 石蓝色(板岩蓝)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SlateBlue() => new Color(0.415686f,0.352941f,0.803922f);

        /// <summary>
        /// #483D8B DarkSlateBlue 暗灰蓝色(暗板岩蓝)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkSlateBlueHex() => "#483D8B";

        /// <summary>
        /// #483D8B DarkSlateBlue 暗灰蓝色(暗板岩蓝)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkSlateBlue() => new Color(0.282353f,0.239216f,0.545098f);

        /// <summary>
        /// #E6E6FA Lavender 淡紫色(熏衣草淡紫)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LavenderHex() => "#E6E6FA";

        /// <summary>
        /// #E6E6FA Lavender 淡紫色(熏衣草淡紫)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Lavender() => new Color(0.901961f,0.901961f,0.980392f);

        /// <summary>
        /// #F8F8FF GhostWhite 幽灵白
        /// </summary>
        /// <returns>HEX值</returns>
        public static string GhostWhiteHex() => "#F8F8FF";

        /// <summary>
        /// #F8F8FF GhostWhite 幽灵白
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color GhostWhite() => new Color(0.972549f,0.972549f,1);

        /// <summary>
        /// #0000FF Blue 纯蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BlueHex() => "#0000FF";

        /// <summary>
        /// #0000FF Blue 纯蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Blue() => new Color(0,0,1);

        /// <summary>
        /// #0000CD MediumBlue 中蓝色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumBlueHex() => "#0000CD";

        /// <summary>
        /// #0000CD MediumBlue 中蓝色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumBlue() => new Color(0,0,0.803922f);

        /// <summary>
        /// #191970 MidnightBlue 午夜蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MidnightBlueHex() => "#191970";

        /// <summary>
        /// #191970 MidnightBlue 午夜蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MidnightBlue() => new Color(0.098039f,0.098039f,0.439216f);

        /// <summary>
        /// #00008B DarkBlue 暗蓝色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkBlueHex() => "#00008B";

        /// <summary>
        /// #00008B DarkBlue 暗蓝色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkBlue() => new Color(0,0,0.545098f);

        /// <summary>
        /// #000080 Navy 海军蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string NavyHex() => "#000080";

        /// <summary>
        /// #000080 Navy 海军蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Navy() => new Color(0,0,0.501961f);

        /// <summary>
        /// #4169E1 RoyalBlue 皇家蓝/宝蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string RoyalBlueHex() => "#4169E1";

        /// <summary>
        /// #4169E1 RoyalBlue 皇家蓝/宝蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color RoyalBlue() => new Color(0.254902f,0.411765f,0.882353f);

        /// <summary>
        /// #6495ED CornflowerBlue 矢车菊蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string CornflowerBlueHex() => "#6495ED";

        /// <summary>
        /// #6495ED CornflowerBlue 矢车菊蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color CornflowerBlue() => new Color(0.392157f,0.584314f,0.929412f);

        /// <summary>
        /// #B0C4DE LightSteelBlue 亮钢蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightSteelBlueHex() => "#B0C4DE";

        /// <summary>
        /// #B0C4DE LightSteelBlue 亮钢蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightSteelBlue() => new Color(0.690196f,0.768627f,0.870588f);

        /// <summary>
        /// #778899 LightSlateGray 亮蓝灰(亮石板灰)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightSlateGrayHex() => "#778899";

        /// <summary>
        /// #778899 LightSlateGray 亮蓝灰(亮石板灰)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightSlateGray() => new Color(0.466667f,0.533333f,0.6f);

        /// <summary>
        /// #708090 SlateGray 灰石色(石板灰)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SlateGrayHex() => "#708090";

        /// <summary>
        /// #708090 SlateGray 灰石色(石板灰)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SlateGray() => new Color(0.439216f,0.501961f,0.564706f);

        /// <summary>
        /// #1E90FF DodgerBlue 闪兰色(道奇蓝)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DodgerBlueHex() => "#1E90FF";

        /// <summary>
        /// #1E90FF DodgerBlue 闪兰色(道奇蓝)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DodgerBlue() => new Color(0.117647f,0.564706f,1);

        /// <summary>
        /// #F0F8FF AliceBlue 爱丽丝蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string AliceBlueHex() => "#F0F8FF";

        /// <summary>
        /// #F0F8FF AliceBlue 爱丽丝蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color AliceBlue() => new Color(0.941176f,0.972549f,1);

        /// <summary>
        /// #4682B4 SteelBlue 钢蓝/铁青
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SteelBlueHex() => "#4682B4";

        /// <summary>
        /// #4682B4 SteelBlue 钢蓝/铁青
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SteelBlue() => new Color(0.27451f,0.509804f,0.705882f);

        /// <summary>
        /// #87CEFA LightSkyBlue 亮天蓝色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightSkyBlueHex() => "#87CEFA";

        /// <summary>
        /// #87CEFA LightSkyBlue 亮天蓝色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightSkyBlue() => new Color(0.529412f,0.807843f,0.980392f);

        /// <summary>
        /// #87CEEB SkyBlue 天蓝色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SkyBlueHex() => "#87CEEB";

        /// <summary>
        /// #87CEEB SkyBlue 天蓝色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SkyBlue() => new Color(0.529412f,0.807843f,0.921569f);

        /// <summary>
        /// #00BFFF DeepSkyBlue 深天蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DeepSkyBlueHex() => "#00BFFF";

        /// <summary>
        /// #00BFFF DeepSkyBlue 深天蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DeepSkyBlue() => new Color(0,0.74902f,1);

        /// <summary>
        /// #ADD8E6 LightBlue 亮蓝
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightBlueHex() => "#ADD8E6";

        /// <summary>
        /// #ADD8E6 LightBlue 亮蓝
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightBlue() => new Color(0.678431f,0.847059f,0.901961f);

        /// <summary>
        /// #B0E0E6 PowderBlue 粉蓝色(火药青)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PowderBlueHex() => "#B0E0E6";

        /// <summary>
        /// #B0E0E6 PowderBlue 粉蓝色(火药青)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color PowderBlue() => new Color(0.690196f,0.878431f,0.901961f);

        /// <summary>
        /// #5F9EA0 CadetBlue 军兰色(军服蓝)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string CadetBlueHex() => "#5F9EA0";

        /// <summary>
        /// #5F9EA0 CadetBlue 军兰色(军服蓝)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color CadetBlue() => new Color(0.372549f,0.619608f,0.627451f);

        /// <summary>
        /// #F0FFFF Azure 蔚蓝色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string AzureHex() => "#F0FFFF";

        /// <summary>
        /// #F0FFFF Azure 蔚蓝色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Azure() => new Color(0.941176f,1,1);

        /// <summary>
        /// #E0FFFF LightCyan 淡青色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightCyanHex() => "#E0FFFF";

        /// <summary>
        /// #E0FFFF LightCyan 淡青色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightCyan() => new Color(0.878431f,1,1);

        /// <summary>
        /// #AFEEEE PaleTurquoise 弱绿宝石
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PaleTurquoiseHex() => "#AFEEEE";

        /// <summary>
        /// #AFEEEE PaleTurquoise 弱绿宝石
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color PaleTurquoise() => new Color(0.686275f,0.933333f,0.933333f);

        /// <summary>
        /// #00FFFF Cyan 青色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string CyanHex() => "#00FFFF";

        /// <summary>
        /// #00FFFF Cyan 青色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Cyan() => new Color(0,1,1);

        /// <summary>
        /// #00FFFF Aqua 浅绿色(水色)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string AquaHex() => "#00FFFF";

        /// <summary>
        /// #00FFFF Aqua 浅绿色(水色)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Aqua() => new Color(0,1,1);

        /// <summary>
        /// #00CED1 DarkTurquoise 暗绿宝石
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkTurquoiseHex() => "#00CED1";

        /// <summary>
        /// #00CED1 DarkTurquoise 暗绿宝石
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkTurquoise() => new Color(0,0.807843f,0.819608f);

        /// <summary>
        /// #2F4F4F DarkSlateGray 暗瓦灰色(暗石板灰)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkSlateGrayHex() => "#2F4F4F";

        /// <summary>
        /// #2F4F4F DarkSlateGray 暗瓦灰色(暗石板灰)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkSlateGray() => new Color(0.184314f,0.309804f,0.309804f);

        /// <summary>
        /// #008B8B DarkCyan 暗青色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkCyanHex() => "#008B8B";

        /// <summary>
        /// #008B8B DarkCyan 暗青色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkCyan() => new Color(0,0.545098f,0.545098f);

        /// <summary>
        /// #008080 Teal 水鸭色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string TealHex() => "#008080";

        /// <summary>
        /// #008080 Teal 水鸭色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Teal() => new Color(0,0.501961f,0.501961f);

        /// <summary>
        /// #48D1CC MediumTurquoise 中绿宝石
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumTurquoiseHex() => "#48D1CC";

        /// <summary>
        /// #48D1CC MediumTurquoise 中绿宝石
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumTurquoise() => new Color(0.282353f,0.819608f,0.8f);

        /// <summary>
        /// #20B2AA LightSeaGreen 浅海洋绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightSeaGreenHex() => "#20B2AA";

        /// <summary>
        /// #20B2AA LightSeaGreen 浅海洋绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightSeaGreen() => new Color(0.12549f,0.698039f,0.666667f);

        /// <summary>
        /// #40E0D0 Turquoise 绿宝石
        /// </summary>
        /// <returns>HEX值</returns>
        public static string TurquoiseHex() => "#40E0D0";

        /// <summary>
        /// #40E0D0 Turquoise 绿宝石
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Turquoise() => new Color(0.25098f,0.878431f,0.815686f);

        /// <summary>
        /// #7FFFD4 Aquamarine 宝石碧绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string AquamarineHex() => "#7FFFD4";

        /// <summary>
        /// #7FFFD4 Aquamarine 宝石碧绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Aquamarine() => new Color(0.498039f,1,0.831373f);

        /// <summary>
        /// #66CDAA MediumAquamarine 中宝石碧绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumAquamarineHex() => "#66CDAA";

        /// <summary>
        /// #66CDAA MediumAquamarine 中宝石碧绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumAquamarine() => new Color(0.4f,0.803922f,0.666667f);

        /// <summary>
        /// #00FA9A MediumSpringGreen 中春绿色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumSpringGreenHex() => "#00FA9A";

        /// <summary>
        /// #00FA9A MediumSpringGreen 中春绿色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumSpringGreen() => new Color(0,0.980392f,0.603922f);

        /// <summary>
        /// #F5FFFA MintCream 薄荷奶油
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MintCreamHex() => "#F5FFFA";

        /// <summary>
        /// #F5FFFA MintCream 薄荷奶油
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MintCream() => new Color(0.960784f,1,0.980392f);

        /// <summary>
        /// #00FF7F SpringGreen 春绿色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SpringGreenHex() => "#00FF7F";

        /// <summary>
        /// #00FF7F SpringGreen 春绿色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SpringGreen() => new Color(0,1,0.498039f);

        /// <summary>
        /// #3CB371 MediumSeaGreen 中海洋绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MediumSeaGreenHex() => "#3CB371";

        /// <summary>
        /// #3CB371 MediumSeaGreen 中海洋绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MediumSeaGreen() => new Color(0.235294f,0.701961f,0.443137f);

        /// <summary>
        /// #2E8B57 SeaGreen 海洋绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SeaGreenHex() => "#2E8B57";

        /// <summary>
        /// #2E8B57 SeaGreen 海洋绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SeaGreen() => new Color(0.180392f,0.545098f,0.341176f);

        /// <summary>
        /// #F0FFF0 Honeydew 蜜色(蜜瓜色)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string HoneydewHex() => "#F0FFF0";

        /// <summary>
        /// #F0FFF0 Honeydew 蜜色(蜜瓜色)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Honeydew() => new Color(0.941176f,1,0.941176f);

        /// <summary>
        /// #90EE90 LightGreen 淡绿色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightGreenHex() => "#90EE90";

        /// <summary>
        /// #90EE90 LightGreen 淡绿色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightGreen() => new Color(0.564706f,0.933333f,0.564706f);

        /// <summary>
        /// #98FB98 PaleGreen 弱绿色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PaleGreenHex() => "#98FB98";

        /// <summary>
        /// #98FB98 PaleGreen 弱绿色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color PaleGreen() => new Color(0.596078f,0.984314f,0.596078f);

        /// <summary>
        /// #8FBC8F DarkSeaGreen 暗海洋绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkSeaGreenHex() => "#8FBC8F";

        /// <summary>
        /// #8FBC8F DarkSeaGreen 暗海洋绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkSeaGreen() => new Color(0.560784f,0.737255f,0.560784f);

        /// <summary>
        /// #32CD32 LimeGreen 闪光深绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LimeGreenHex() => "#32CD32";

        /// <summary>
        /// #32CD32 LimeGreen 闪光深绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LimeGreen() => new Color(0.196078f,0.803922f,0.196078f);

        /// <summary>
        /// #00FF00 Lime 闪光绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LimeHex() => "#00FF00";

        /// <summary>
        /// #00FF00 Lime 闪光绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Lime() => new Color(0,1,0);

        /// <summary>
        /// #228B22 ForestGreen 森林绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string ForestGreenHex() => "#228B22";

        /// <summary>
        /// #228B22 ForestGreen 森林绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color ForestGreen() => new Color(0.133333f,0.545098f,0.133333f);

        /// <summary>
        /// #008000 Green 纯绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string GreenHex() => "#008000";

        /// <summary>
        /// #008000 Green 纯绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Green() => new Color(0,0.501961f,0);

        /// <summary>
        /// #006400 DarkGreen 暗绿色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkGreenHex() => "#006400";

        /// <summary>
        /// #006400 DarkGreen 暗绿色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkGreen() => new Color(0,0.392157f,0);

        /// <summary>
        /// #7FFF00 Chartreuse 黄绿色(查特酒绿)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string ChartreuseHex() => "#7FFF00";

        /// <summary>
        /// #7FFF00 Chartreuse 黄绿色(查特酒绿)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Chartreuse() => new Color(0.498039f,1,0);

        /// <summary>
        /// #7CFC00 LawnGreen 草绿色(草坪绿_
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LawnGreenHex() => "#7CFC00";

        /// <summary>
        /// #7CFC00 LawnGreen 草绿色(草坪绿_
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LawnGreen() => new Color(0.486275f,0.988235f,0);

        /// <summary>
        /// #ADFF2F GreenYellow 绿黄色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string GreenYellowHex() => "#ADFF2F";

        /// <summary>
        /// #ADFF2F GreenYellow 绿黄色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color GreenYellow() => new Color(0.678431f,1,0.184314f);

        /// <summary>
        /// #556B2F DarkOliveGreen 暗橄榄绿
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkOliveGreenHex() => "#556B2F";

        /// <summary>
        /// #556B2F DarkOliveGreen 暗橄榄绿
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkOliveGreen() => new Color(0.333333f,0.419608f,0.184314f);

        /// <summary>
        /// #9ACD32 YellowGreen 黄绿色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string YellowGreenHex() => "#9ACD32";

        /// <summary>
        /// #9ACD32 YellowGreen 黄绿色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color YellowGreen() => new Color(0.603922f,0.803922f,0.196078f);

        /// <summary>
        /// #6B8E23 OliveDrab 橄榄褐色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string OliveDrabHex() => "#6B8E23";

        /// <summary>
        /// #6B8E23 OliveDrab 橄榄褐色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color OliveDrab() => new Color(0.419608f,0.556863f,0.137255f);

        /// <summary>
        /// #F5F5DC Beige 米色/灰棕色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BeigeHex() => "#F5F5DC";

        /// <summary>
        /// #F5F5DC Beige 米色/灰棕色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Beige() => new Color(0.960784f,0.960784f,0.862745f);

        /// <summary>
        /// #FAFAD2 LightGoldenrodYellow 亮菊黄
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightGoldenrodYellowHex() => "#FAFAD2";

        /// <summary>
        /// #FAFAD2 LightGoldenrodYellow 亮菊黄
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightGoldenrodYellow() => new Color(0.980392f,0.980392f,0.823529f);

        /// <summary>
        /// #FFFFF0 Ivory 象牙色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string IvoryHex() => "#FFFFF0";

        /// <summary>
        /// #FFFFF0 Ivory 象牙色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Ivory() => new Color(1,1,0.941176f);

        /// <summary>
        /// #FFFFE0 LightYellow 浅黄色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightYellowHex() => "#FFFFE0";

        /// <summary>
        /// #FFFFE0 LightYellow 浅黄色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightYellow() => new Color(1,1,0.878431f);

        /// <summary>
        /// #FFFF00 Yellow 纯黄
        /// </summary>
        /// <returns>HEX值</returns>
        public static string YellowHex() => "#FFFF00";

        /// <summary>
        /// #FFFF00 Yellow 纯黄
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Yellow() => new Color(1,1,0);

        /// <summary>
        /// #808000 Olive 橄榄
        /// </summary>
        /// <returns>HEX值</returns>
        public static string OliveHex() => "#808000";

        /// <summary>
        /// #808000 Olive 橄榄
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Olive() => new Color(0.501961f,0.501961f,0);

        /// <summary>
        /// #BDB76B DarkKhaki 暗黄褐色(深卡叽布)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkKhakiHex() => "#BDB76B";

        /// <summary>
        /// #BDB76B DarkKhaki 暗黄褐色(深卡叽布)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkKhaki() => new Color(0.741176f,0.717647f,0.419608f);

        /// <summary>
        /// #FFFACD LemonChiffon 柠檬绸
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LemonChiffonHex() => "#FFFACD";

        /// <summary>
        /// #FFFACD LemonChiffon 柠檬绸
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LemonChiffon() => new Color(1,0.980392f,0.803922f);

        /// <summary>
        /// #EEE8AA PaleGoldenrod 灰菊黄(苍麒麟色)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PaleGoldenrodHex() => "#EEE8AA";

        /// <summary>
        /// #EEE8AA PaleGoldenrod 灰菊黄(苍麒麟色)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color PaleGoldenrod() => new Color(0.933333f,0.909804f,0.666667f);

        /// <summary>
        /// #F0E68C Khaki 黄褐色(卡叽布)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string KhakiHex() => "#F0E68C";

        /// <summary>
        /// #F0E68C Khaki 黄褐色(卡叽布)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Khaki() => new Color(0.941176f,0.901961f,0.54902f);

        /// <summary>
        /// #FFD700 Gold 金色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string GoldHex() => "#FFD700";

        /// <summary>
        /// #FFD700 Gold 金色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Gold() => new Color(1,0.843137f,0);

        /// <summary>
        /// #FFF8DC Cornsilk 玉米丝色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string CornsilkHex() => "#FFF8DC";

        /// <summary>
        /// #FFF8DC Cornsilk 玉米丝色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Cornsilk() => new Color(1,0.972549f,0.862745f);

        /// <summary>
        /// #DAA520 Goldenrod 金菊黄
        /// </summary>
        /// <returns>HEX值</returns>
        public static string GoldenrodHex() => "#DAA520";

        /// <summary>
        /// #DAA520 Goldenrod 金菊黄
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Goldenrod() => new Color(0.854902f,0.647059f,0.12549f);

        /// <summary>
        /// #B8860B DarkGoldenrod 暗金菊黄
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkGoldenrodHex() => "#B8860B";

        /// <summary>
        /// #B8860B DarkGoldenrod 暗金菊黄
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkGoldenrod() => new Color(0.721569f,0.52549f,0.043137f);

        /// <summary>
        /// #FFFAF0 FloralWhite 花的白色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string FloralWhiteHex() => "#FFFAF0";

        /// <summary>
        /// #FFFAF0 FloralWhite 花的白色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color FloralWhite() => new Color(1,0.980392f,0.941176f);

        /// <summary>
        /// #FDF5E6 OldLace 老花色(旧蕾丝)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string OldLaceHex() => "#FDF5E6";

        /// <summary>
        /// #FDF5E6 OldLace 老花色(旧蕾丝)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color OldLace() => new Color(0.992157f,0.960784f,0.901961f);

        /// <summary>
        /// #F5DEB3 Wheat 浅黄色(小麦色)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string WheatHex() => "#F5DEB3";

        /// <summary>
        /// #F5DEB3 Wheat 浅黄色(小麦色)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Wheat() => new Color(0.960784f,0.870588f,0.701961f);

        /// <summary>
        /// #FFE4B5 Moccasin 鹿皮色(鹿皮靴)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MoccasinHex() => "#FFE4B5";

        /// <summary>
        /// #FFE4B5 Moccasin 鹿皮色(鹿皮靴)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Moccasin() => new Color(1,0.894118f,0.709804f);

        /// <summary>
        /// #FFA500 Orange 橙色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string OrangeHex() => "#FFA500";

        /// <summary>
        /// #FFA500 Orange 橙色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Orange() => new Color(1,0.647059f,0);

        /// <summary>
        /// #FFEFD5 PapayaWhip 番木色(番木瓜)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PapayaWhipHex() => "#FFEFD5";

        /// <summary>
        /// #FFEFD5 PapayaWhip 番木色(番木瓜)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color PapayaWhip() => new Color(1,0.937255f,0.835294f);

        /// <summary>
        /// #FFEBCD BlanchedAlmond 白杏色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BlanchedAlmondHex() => "#FFEBCD";

        /// <summary>
        /// #FFEBCD BlanchedAlmond 白杏色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color BlanchedAlmond() => new Color(1,0.921569f,0.803922f);

        /// <summary>
        /// #FFDEAD NavajoWhite 纳瓦白(土著白)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string NavajoWhiteHex() => "#FFDEAD";

        /// <summary>
        /// #FFDEAD NavajoWhite 纳瓦白(土著白)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color NavajoWhite() => new Color(1,0.870588f,0.678431f);

        /// <summary>
        /// #FAEBD7 AntiqueWhite 古董白
        /// </summary>
        /// <returns>HEX值</returns>
        public static string AntiqueWhiteHex() => "#FAEBD7";

        /// <summary>
        /// #FAEBD7 AntiqueWhite 古董白
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color AntiqueWhite() => new Color(0.980392f,0.921569f,0.843137f);

        /// <summary>
        /// #D2B48C Tan 茶色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string TanHex() => "#D2B48C";

        /// <summary>
        /// #D2B48C Tan 茶色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Tan() => new Color(0.823529f,0.705882f,0.54902f);

        /// <summary>
        /// #DEB887 BurlyWood 硬木色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BurlyWoodHex() => "#DEB887";

        /// <summary>
        /// #DEB887 BurlyWood 硬木色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color BurlyWood() => new Color(0.870588f,0.721569f,0.529412f);

        /// <summary>
        /// #FFE4C4 Bisque 陶坯黄
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BisqueHex() => "#FFE4C4";

        /// <summary>
        /// #FFE4C4 Bisque 陶坯黄
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Bisque() => new Color(1,0.894118f,0.768627f);

        /// <summary>
        /// #FF8C00 DarkOrange 深橙色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkOrangeHex() => "#FF8C00";

        /// <summary>
        /// #FF8C00 DarkOrange 深橙色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkOrange() => new Color(1,0.54902f,0);

        /// <summary>
        /// #FAF0E6 Linen 亚麻布
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LinenHex() => "#FAF0E6";

        /// <summary>
        /// #FAF0E6 Linen 亚麻布
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Linen() => new Color(0.980392f,0.941176f,0.901961f);

        /// <summary>
        /// #CD853F Peru 秘鲁色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PeruHex() => "#CD853F";

        /// <summary>
        /// #CD853F Peru 秘鲁色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Peru() => new Color(0.803922f,0.521569f,0.247059f);

        /// <summary>
        /// #FFDAB9 PeachPuff 桃肉色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string PeachPuffHex() => "#FFDAB9";

        /// <summary>
        /// #FFDAB9 PeachPuff 桃肉色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color PeachPuff() => new Color(1,0.854902f,0.72549f);

        /// <summary>
        /// #F4A460 SandyBrown 沙棕色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SandyBrownHex() => "#F4A460";

        /// <summary>
        /// #F4A460 SandyBrown 沙棕色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SandyBrown() => new Color(0.956863f,0.643137f,0.376471f);

        /// <summary>
        /// #D2691E Chocolate 巧克力色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string ChocolateHex() => "#D2691E";

        /// <summary>
        /// #D2691E Chocolate 巧克力色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Chocolate() => new Color(0.823529f,0.411765f,0.117647f);

        /// <summary>
        /// #8B4513 SaddleBrown 重褐色(马鞍棕色)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SaddleBrownHex() => "#8B4513";

        /// <summary>
        /// #8B4513 SaddleBrown 重褐色(马鞍棕色)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color SaddleBrown() => new Color(0.545098f,0.270588f,0.07451f);

        /// <summary>
        /// #FFF5EE Seashell 海贝壳
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SeashellHex() => "#FFF5EE";

        /// <summary>
        /// #FFF5EE Seashell 海贝壳
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Seashell() => new Color(1,0.960784f,0.933333f);

        /// <summary>
        /// #A0522D Sienna 黄土赭色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SiennaHex() => "#A0522D";

        /// <summary>
        /// #A0522D Sienna 黄土赭色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Sienna() => new Color(0.627451f,0.321569f,0.176471f);

        /// <summary>
        /// #FFA07A LightSalmon 浅鲑鱼肉色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightSalmonHex() => "#FFA07A";

        /// <summary>
        /// #FFA07A LightSalmon 浅鲑鱼肉色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightSalmon() => new Color(1,0.627451f,0.478431f);

        /// <summary>
        /// #FF7F50 Coral 珊瑚
        /// </summary>
        /// <returns>HEX值</returns>
        public static string CoralHex() => "#FF7F50";

        /// <summary>
        /// #FF7F50 Coral 珊瑚
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Coral() => new Color(1,0.498039f,0.313725f);

        /// <summary>
        /// #FF4500 OrangeRed 橙红色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string OrangeRedHex() => "#FF4500";

        /// <summary>
        /// #FF4500 OrangeRed 橙红色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color OrangeRed() => new Color(1,0.270588f,0);

        /// <summary>
        /// #E9967A DarkSalmon 深鲜肉/鲑鱼色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkSalmonHex() => "#E9967A";

        /// <summary>
        /// #E9967A DarkSalmon 深鲜肉/鲑鱼色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkSalmon() => new Color(0.913725f,0.588235f,0.478431f);

        /// <summary>
        /// #FF6347 Tomato 番茄红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string TomatoHex() => "#FF6347";

        /// <summary>
        /// #FF6347 Tomato 番茄红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Tomato() => new Color(1,0.388235f,0.278431f);

        /// <summary>
        /// #FFE4E1 MistyRose 浅玫瑰色(薄雾玫瑰)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MistyRoseHex() => "#FFE4E1";

        /// <summary>
        /// #FFE4E1 MistyRose 浅玫瑰色(薄雾玫瑰)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color MistyRose() => new Color(1,0.894118f,0.882353f);

        /// <summary>
        /// #FA8072 Salmon 鲜肉/鲑鱼色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SalmonHex() => "#FA8072";

        /// <summary>
        /// #FA8072 Salmon 鲜肉/鲑鱼色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Salmon() => new Color(0.980392f,0.501961f,0.447059f);

        /// <summary>
        /// #FFFAFA Snow 雪白色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SnowHex() => "#FFFAFA";

        /// <summary>
        /// #FFFAFA Snow 雪白色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Snow() => new Color(1,0.980392f,0.980392f);

        /// <summary>
        /// #F08080 LightCoral 淡珊瑚色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightCoralHex() => "#F08080";

        /// <summary>
        /// #F08080 LightCoral 淡珊瑚色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightCoral() => new Color(0.941176f,0.501961f,0.501961f);

        /// <summary>
        /// #BC8F8F RosyBrown 玫瑰棕色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string RosyBrownHex() => "#BC8F8F";

        /// <summary>
        /// #BC8F8F RosyBrown 玫瑰棕色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color RosyBrown() => new Color(0.737255f,0.560784f,0.560784f);

        /// <summary>
        /// #CD5C5C IndianRed 印度红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string IndianRedHex() => "#CD5C5C";

        /// <summary>
        /// #CD5C5C IndianRed 印度红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color IndianRed() => new Color(0.803922f,0.360784f,0.360784f);

        /// <summary>
        /// #FF0000 Red 纯红
        /// </summary>
        /// <returns>HEX值</returns>
        public static string RedHex() => "#FF0000";

        /// <summary>
        /// #FF0000 Red 纯红
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Red() => new Color(1,0,0);

        /// <summary>
        /// #A52A2A Brown 棕色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BrownHex() => "#A52A2A";

        /// <summary>
        /// #A52A2A Brown 棕色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Brown() => new Color(0.647059f,0.164706f,0.164706f);

        /// <summary>
        /// #B22222 FireBrick 火砖色(耐火砖)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string FireBrickHex() => "#B22222";

        /// <summary>
        /// #B22222 FireBrick 火砖色(耐火砖)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color FireBrick() => new Color(0.698039f,0.133333f,0.133333f);

        /// <summary>
        /// #8B0000 DarkRed 深红色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkRedHex() => "#8B0000";

        /// <summary>
        /// #8B0000 DarkRed 深红色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkRed() => new Color(0.545098f,0,0);

        /// <summary>
        /// #800000 Maroon 栗色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string MaroonHex() => "#800000";

        /// <summary>
        /// #800000 Maroon 栗色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Maroon() => new Color(0.501961f,0,0);

        /// <summary>
        /// #FFFFFF White 纯白
        /// </summary>
        /// <returns>HEX值</returns>
        public static string WhiteHex() => "#FFFFFF";

        /// <summary>
        /// #FFFFFF White 纯白
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color White() => new Color(1,1,1);

        /// <summary>
        /// #F5F5F5 WhiteSmoke 白烟
        /// </summary>
        /// <returns>HEX值</returns>
        public static string WhiteSmokeHex() => "#F5F5F5";

        /// <summary>
        /// #F5F5F5 WhiteSmoke 白烟
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color WhiteSmoke() => new Color(0.960784f,0.960784f,0.960784f);

        /// <summary>
        /// #DCDCDC Gainsboro 淡灰色(庚斯博罗灰)
        /// </summary>
        /// <returns>HEX值</returns>
        public static string GainsboroHex() => "#DCDCDC";

        /// <summary>
        /// #DCDCDC Gainsboro 淡灰色(庚斯博罗灰)
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Gainsboro() => new Color(0.862745f,0.862745f,0.862745f);

        /// <summary>
        /// #D3D3D3 LightGrey 浅灰色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string LightGreyHex() => "#D3D3D3";

        /// <summary>
        /// #D3D3D3 LightGrey 浅灰色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color LightGrey() => new Color(0.827451f,0.827451f,0.827451f);

        /// <summary>
        /// #C0C0C0 Silver 银灰色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string SilverHex() => "#C0C0C0";

        /// <summary>
        /// #C0C0C0 Silver 银灰色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Silver() => new Color(0.752941f,0.752941f,0.752941f);

        /// <summary>
        /// #A9A9A9 DarkGray 深灰色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DarkGrayHex() => "#A9A9A9";

        /// <summary>
        /// #A9A9A9 DarkGray 深灰色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DarkGray() => new Color(0.662745f,0.662745f,0.662745f);

        /// <summary>
        /// #808080 Gray 灰色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string GrayHex() => "#808080";

        /// <summary>
        /// #808080 Gray 灰色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Gray() => new Color(0.501961f,0.501961f,0.501961f);

        /// <summary>
        /// #696969 DimGray 暗淡的灰色
        /// </summary>
        /// <returns>HEX值</returns>
        public static string DimGrayHex() => "#696969";

        /// <summary>
        /// #696969 DimGray 暗淡的灰色
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color DimGray() => new Color(0.411765f,0.411765f,0.411765f);

        /// <summary>
        /// #000000 Black 纯黑
        /// </summary>
        /// <returns>HEX值</returns>
        public static string BlackHex() => "#000000";

        /// <summary>
        /// #000000 Black 纯黑
        /// </summary>
        /// <returns>RGB值</returns>
        public static Color Black() => new Color(0,0,0);
    }
}