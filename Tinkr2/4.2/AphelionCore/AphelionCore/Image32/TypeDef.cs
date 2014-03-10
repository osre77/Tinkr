using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Graphics
{
    /// <summary>
    /// Alpha-blended Pixel
    /// (Skewworks NETMF Graphics Standard v1.0)
    /// </summary>
    [Serializable]
    public struct ColorEX
    {

        #region Variables

        /// <summary>
        /// X location
        /// </summary>
        public int X;

        /// <summary>
        /// Y location
        /// </summary>
        public int Y;

        /// <summary>
        /// Alpha value (0 - 256)
        /// </summary>
        public ushort Alpha;

        /// <summary>
        /// Color
        /// </summary>
        public Color Color;

        #endregion

        #region Constructor

        /// <summary>
        /// Alpha-blended Pixel
        /// (Skewworks NETMF Graphics Standard v1.0)
        /// </summary>
        /// <param name="X">X location</param>
        /// <param name="Y">Y location</param>
        /// <param name="Alpha">Alpha value (0 - 256)</param>
        /// <param name="Color">Color</param>
        public ColorEX(int X, int Y, ushort Alpha, Color Color)
        {
            this.X = X;
            this.Y = Y;
            this.Alpha = (ushort)((Alpha > 0) ? Alpha + 1 : 0);
            this.Color = Color;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// String representation of object
        /// </summary>
        /// <returns>ColorEX: X, Y; RGB(R, G, B) @ Alpha</returns>
        public override string ToString()
        {
            return "ColorEX: " + this.X + ", " + this.Y + "; RGB(" + ColorUtility.GetRValue(this.Color) + ", " + ColorUtility.GetGValue(this.Color) + ", " + ColorUtility.GetBValue(this.Color) + ") @ " + this.Alpha;
        }

        #endregion

    }
}
