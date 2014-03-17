using System;
using Microsoft.SPOT;

// ReSharper disable ReplaceWithStringIsNullOrEmpty
namespace Skewworks.NETMF
{
   /// <summary>
   /// Class for managing Fonts
   /// </summary>
   public static class FontManager
   {
      #region Public Methods

      /// <summary>
      /// Computes the total extent of a text for a specific font, similar to <see cref="Font.ComputeExtent(string, out int, out int)"/>
      /// </summary>
      /// <param name="font">Font to use</param>
      /// <param name="value">Value to compute extent for</param>
      /// <returns>Returns the total text extent in pixel of the given value</returns>
      /// <remarks>
      /// A string containing multiple lines separated by an \n will be spitted and the extents of every line are summed up.
      /// </remarks>
      public static size ComputeExtentEx(Font font, String value)
      {
         var sz = new size();

         if (value == null || value == string.Empty)
         {
            return new size(0, font.Height);
         }

         string[] lines = value.Split('\n');

         for (int i = 0; i < lines.Length; i++)
         {
            int w;
            int h;
            font.ComputeExtent(lines[i], out w, out h);
            if (w > sz.Width)
            {
               sz.Width = w;
            }
         }

         sz.Height = font.Height * lines.Length;

         return sz;
      }

      #endregion
   }
}
// ReSharper restore ReplaceWithStringIsNullOrEmpty
