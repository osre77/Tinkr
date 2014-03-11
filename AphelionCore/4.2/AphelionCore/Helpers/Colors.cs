using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF
{
    [Serializable]
    public static class Colors
    {

        #region Constants

        /// <summary>
        /// Black RGB(0, 0, 0)
        /// </summary>
        public const Color Black = 0;

        /// <summary>
        /// Blue RGB(0, 0, 255)
        /// </summary>
        public const Color Blue = (Color)16711680;

        /// <summary>
        /// Brown RGB(165, 42, 42)
        /// </summary>
        public const Color Brown = (Color)2763429;

        /// <summary>
        /// Charcoal RGB(51, 51, 51)
        /// </summary>
        public const Color Charcoal = (Color)3355443;

        /// <summary>
        /// Charcoal Dust RGB(82, 82, 82)
        /// </summary>
        public const Color CharcoalDust = (Color)5395026;

        /// <summary>
        /// Cyan RGB(0, 255, 255)
        /// </summary>
        public const Color Cyan = (Color)16776960;

        /// <summary>
        /// Dark Gray RGB(169, 169, 169)
        /// </summary>
        public const Color DarkGray = (Color)11119017;

        /// <summary>
        /// Dark Red RGB(183, 17, 17)
        /// </summary>
        public const Color DarkRed = (Color)1118647;

        /// <summary>
        /// Ghost RGB(242, 241, 240);
        /// </summary>
        public const Color Ghost = (Color)15790578;

        /// <summary>
        /// Gray RGB(128, 128, 128)
        /// </summary>
        public const Color Gray = (Color)8421504;

        /// <summary>
        /// Green RGB(0, 128, 0)
        /// </summary>
        public const Color Green = (Color)32768;

        /// <summary>
        /// Light Blue RGB(27, 161, 226)
        /// </summary>
        public const Color LightBlue = (Color)14852379;

        /// <summary>
        /// LightGray RGB(211, 211, 211)
        /// </summary>
        public const Color LightGray = (Color)13882323;

        /// <summary>
        /// Magenta RGB(255, 0, 255)
        /// </summary>
        public const Color Magenta = (Color)16711935;

        /// <summary>
        /// Orange RGB(255, 165, 0)
        /// </summary>
        public const Color Orange = (Color)42495;

        /// <summary>
        /// Purple RGB(128, 0, 128)
        /// </summary>
        public const Color Purple = (Color)8388736;

        /// <summary>
        /// Red RGB(255, 0, 0)
        /// </summary>
        public const Color Red = (Color)255;

        /// <summary>
        /// Wheat RGB(237, 237, 237)
        /// </summary>
        public const Color Wheat = (Color)15592941;

        /// <summary>
        /// White RGB(255, 255, 255)
        /// </summary>
        public const Color White = (Color)16777215;

        /// <summary>
        /// Yellow RGB(255, 255, 0)
        /// </summary>
        public const Color Yellow = (Color)65535;

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a color from an RGB string
        /// </summary>
        /// <param name="ColVal"></param>
        /// <returns></returns>
        public static Color FromString(string ColVal)
        {
            string[] vals = ColVal.Split(',');
            return ColorUtility.ColorFromRGB(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));
        }

        /// <summary>
        /// Creates a color from a long value
        /// </summary>
        /// <param name="ColVal"></param>
        /// <returns></returns>
        public static Color FromValue(string ColVal)
        {
            return FromValue(long.Parse(ColVal));
        }

        /// <summary>
        /// Creates a color from a long value
        /// </summary>
        /// <param name="ColVal"></param>
        /// <returns></returns>
        public static Color FromValue(long ColVal)
        {
            return (Color)ColVal;
        }

        #endregion

    }
}
