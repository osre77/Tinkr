using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF
{
    public static class FontManager
    {

        #region Public Methods

        /// <summary>
        /// Overload for Font.ComputeExtent
        /// </summary>
        /// <param name="font">Font to use</param>
        /// <param name="value">Value to compute extent for</param>
        /// <returns></returns>
        public static size ComputeExtentEx(Font font, String value)
        {
            int w = 0;
            int h = 0;
            size sz = new size();

            if (value == null || value == string.Empty)
                return new size(0, font.Height);

            string[] lines = value.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                font.ComputeExtent(lines[i], out w, out h);
                if (w > sz.Width)
                    sz.Width = w;
            }

            sz.Height = font.Height * lines.Length;

            return sz;
        }

        #endregion

    }
}
