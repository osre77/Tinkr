using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Graphics
{
   /// <summary>
   /// Alpha-blended Image
   /// </summary>
   [Serializable]
   public class Image32 : MarshalByRefObject
   {

      #region Variables

      private readonly Bitmap _bmp;
      private readonly ColorEX[] _pixels;

      #endregion

      #region Constructor

      /// <summary>
      /// Alpha-blended Image
      /// </summary>
      /// <param name="bytes">Image32 Data</param>
      public Image32(byte[] bytes)
      {
         // Check Magic Key
         if (bytes[0] != 80 || bytes[1] != 50 || bytes[2] != 73)
            throw new Exception("Invalid image format");

         // Get Number of Pixels
         int lPixels = BytesToInt(bytes[3], bytes[4], bytes[5], bytes[6]);

         // Confim Number
         // In future version -2 will denote version 2
         if (lPixels < 0)
            throw new Exception("Invalid Color Length");

         // Create Array
         _pixels = new ColorEX[lPixels];

         // Copy out colors
         int lIndex = 7;
         for (int l = 0; l < _pixels.Length; l++)
         {
            var c = new ColorEX
            {
               X = BytesToInt(bytes[lIndex], bytes[lIndex + 1])
            };
            lIndex += 2;
            c.Y = BytesToInt(bytes[lIndex], bytes[lIndex + 1]);
            lIndex += 2;
            c.Alpha = (ushort)(bytes[lIndex++] + 1);
            c.Color = ColorUtility.ColorFromRGB(bytes[lIndex], bytes[lIndex + 1], bytes[lIndex + 2]);
            lIndex += 3;
            _pixels[l] = c;
         }

         // Remainder of file is flat Bitmap with RGB(255, 0, 255) transparency color
         try
         {
            var bBmp = new byte[bytes.Length - lIndex];
            Array.Copy(bytes, (int)lIndex, bBmp, 0, bBmp.Length);
            _bmp = new Bitmap(bBmp, Bitmap.BitmapImageType.Bmp);
            _bmp.MakeTransparent(ColorUtility.ColorFromRGB(255, 0, 255));
         }
         // ReSharper disable once EmptyGeneralCatchClause
         catch
         { }
      }

      #endregion

      #region Private Methods

      private int BytesToInt(byte b1, byte b2, byte b3, byte b4)
      {
         return ((b4 & 0xFF) << 24) + ((b3 & 0xFF) << 16) + ((b2 & 0xFF) << 8) + (b1 & 0xFF);
      }

      private int BytesToInt(byte b1, byte b2)
      {
         return ((b2 & 0xFF) << 8) + (b1 & 0xFF);
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Draws image onto a supplied bitmap
      /// </summary>
      /// <param name="dest">Destination bitmap</param>
      /// <param name="x">Destination X</param>
      /// <param name="y">Destination Y</param>
      public void Draw(Bitmap dest, int x, int y)
      {
         int i = 0;

         lock (dest)
         {
            dest.DrawImage(x, y, _bmp, 0, 0, _bmp.Width, _bmp.Height);

            while (i < _pixels.Length)
            {
               var c = _pixels[i++];
               dest.DrawRectangle(Color.Black, 0, c.X + x, c.Y + y, 1, 1, 0, 0, c.Color, 0, 0, c.Color, 0, 0, c.Alpha);
            }
         }

      }

      /// <summary>
      /// Draws image onto a supplied bitmap
      /// </summary>
      /// <param name="x">Destination X</param>
      /// <param name="y">Destination Y</param>
      /// <param name="dest">Destination bitmap</param>
      /// <param name="srcX">Source X</param>
      /// <param name="srcY">Source Y</param>
      /// <param name="width">Source width</param>
      /// <param name="height">Source height</param>
      public void Draw(int x, int y, Bitmap dest, int srcX, int srcY, int width, int height)
      {
         int i = 0;

         lock (dest)
         {
            dest.DrawRectangle(Color.White, 0, x, y, width, height, 0, 0, Color.White, 0, 0, Color.White, 0, 0, 256);
            dest.DrawImage(x, y, _bmp, srcX, srcY, width, height);

            while (i < _pixels.Length)
            {
               var c = _pixels[i++];
               if (c.X >= srcX && c.X < srcX + width && c.Y >= srcY && c.Y < srcY + height)
                  dest.DrawRectangle(Color.Black, 0, c.X + x - srcX, c.Y + y - srcY, 1, 1, 0, 0, c.Color, 0, 0, c.Color, 0, 0, c.Alpha);
            }
         }
      }

      #endregion

      #region Properties

      /// <summary>
      /// Returns image width
      /// </summary>
      public int Width
      {
         get { return _bmp.Width; }
      }

      /// <summary>
      /// Retuns image height
      /// </summary>
      public int Height
      {
         get { return _bmp.Height; }
      }

      #endregion

   }
}
