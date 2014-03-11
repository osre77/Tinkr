using System;
using Microsoft.SPOT;

// ReSharper disable ReplaceWithStringIsNullOrEmpty
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
         var sz = new size();

         if (value == null || value == string.Empty)
            return new size(0, font.Height);

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
