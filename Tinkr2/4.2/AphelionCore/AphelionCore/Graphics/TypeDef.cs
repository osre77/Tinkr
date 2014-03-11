using System;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Graphics
{
   /// <summary>
   /// Alpha-blended Pixel
   /// (Skewworks NETMF Graphics Standard v1.0)
   /// </summary>
   [Serializable]
   // ReSharper disable once InconsistentNaming
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
      /// <param name="x">X location</param>
      /// <param name="y">Y location</param>
      /// <param name="alpha">Alpha value (0 - 256)</param>
      /// <param name="color">Color</param>
      public ColorEX(int x, int y, ushort alpha, Color color)
      {
         X = x;
         Y = y;
         Alpha = (ushort)((alpha > 0) ? alpha + 1 : 0);
         Color = color;
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// String representation of object
      /// </summary>
      /// <returns>ColorEX: X, Y; RGB(R, G, B) @ Alpha</returns>
      public override string ToString()
      {
         return "ColorEX: " + X + ", " + Y + "; RGB(" + ColorUtility.GetRValue(Color) + ", " + ColorUtility.GetGValue(Color) + ", " + ColorUtility.GetBValue(Color) + ") @ " + Alpha;
      }

      #endregion

   }
}
