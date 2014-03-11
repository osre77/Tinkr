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

        private Bitmap _bmp;
        private ColorEX[] pixels;

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
                new Exception("Invalid image format");

            // Get Number of Pixels
            long lPixels = bytesToLong(bytes[3], bytes[4], bytes[5], bytes[6]);

            // Confim Number
            // In future version -2 will denote version 2
            if (lPixels < 0)
                throw new Exception("Invalid Color Length");

            // Create Array
            pixels = new ColorEX[lPixels];

            // Copy out colors
            long lIndex = 7;
            ColorEX c;
            for (long l = 0; l < pixels.Length; l++)
            {
                c = new ColorEX();
                c.X = bytesToInt(bytes[lIndex], bytes[lIndex + 1]);
                lIndex += 2;
                c.Y = bytesToInt(bytes[lIndex], bytes[lIndex + 1]);
                lIndex += 2;
                c.Alpha = (ushort)(bytes[lIndex++] + 1);
                c.Color = ColorUtility.ColorFromRGB(bytes[lIndex], bytes[lIndex + 1], bytes[lIndex + 2]);
                lIndex += 3;
                pixels[l] = c;
            }

            // Remainder of file is flat Bitmap with RGB(255, 0, 255) transparency color
            try
            {
                byte[] bBMP = new byte[bytes.Length - lIndex];
                Array.Copy(bytes, (int)lIndex, bBMP, 0, bBMP.Length);
                _bmp = new Bitmap(bBMP, Bitmap.BitmapImageType.Bmp);
                _bmp.MakeTransparent(ColorUtility.ColorFromRGB(255, 0, 255));
            }
            catch (Exception) { }
        }

        #endregion

        #region Private Methods

        private int bytesToLong(byte b1, byte b2, byte b3, byte b4)
        {
            return ((b4 & 0xFF) << 24) + ((b3 & 0xFF) << 16) + ((b2 & 0xFF) << 8) + (b1 & 0xFF);
        }

        private int bytesToInt(byte b1, byte b2)
        {
            return ((b2 & 0xFF) << 8) + (b1 & 0xFF);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Draws image onto a supplied bitmap
        /// </summary>
        /// <param name="Dest">Destination bitmap</param>
        /// <param name="X">Destination X</param>
        /// <param name="Y">Destination Y</param>
        public void Draw(Bitmap Dest, int X, int Y)
        {
            ColorEX c;
            int i = 0;

            lock (Dest)
            {
                Dest.DrawImage(X, Y, _bmp, 0, 0, _bmp.Width, _bmp.Height);

                while (i < pixels.Length)
                {
                    c = pixels[i++];
                    Dest.DrawRectangle(Color.Black, 0, c.X + X, c.Y + Y, 1, 1, 0, 0, c.Color, 0, 0, c.Color, 0, 0, c.Alpha);
                }
            }

        }

        /// <summary>
        /// Draws image onto a supplied bitmap
        /// </summary>
        /// <param name="X">Destination X</param>
        /// <param name="Y">Destination Y</param>
        /// <param name="Dest">Destination bitmap</param>
        /// <param name="SrcX">Source X</param>
        /// <param name="SrcY">Source Y</param>
        /// <param name="Width">Source Width</param>
        /// <param name="Height">Source Height</param>
        public void Draw(int X, int Y, Bitmap Dest, int SrcX, int SrcY, int Width, int Height)
        {
            ColorEX c;
            int i = 0;

            lock (Dest)
            {
                Dest.DrawRectangle(Color.White, 0, X, Y, Width, Height, 0, 0, Color.White, 0, 0, Color.White, 0, 0, 256);
                Dest.DrawImage(X, Y, _bmp, SrcX, SrcY, Width, Height);

                while (i < pixels.Length)
                {
                    c = pixels[i++];
                    if (c.X >= SrcX && c.X < SrcX + Width && c.Y >= SrcY && c.Y < SrcY + Height)
                        Dest.DrawRectangle(Color.Black, 0, c.X + X - SrcX, c.Y + Y - SrcY, 1, 1, 0, 0, c.Color, 0, 0, c.Color, 0, 0, c.Alpha);
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
