using System;

using Microsoft.SPOT;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class LineGraph : Control
   {

      #region Variables

      private Font _font;
      private LineGraphItem[] _lines;
      private float _minX, _maxX;
      private float _minY, _maxY;
      private int _thickness;

      #endregion

      #region Constructor

      public LineGraph(string name, Font font, int maximumX, int maximumY, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _maxX = maximumX;
         _maxY = maximumY;
         _font = font;
         _thickness = 1;
      }

      public LineGraph(string name, Font font, int minimumX, int maximumX, int minimumY, int maximumY, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _maxX = maximumX;
         _maxY = maximumY;
         _minX = minimumX;
         _minY = minimumY;
         _font = font;
         _thickness = 1;
      }

      #endregion

      #region Properties

      public Font Font
      {
         get { return _font; }
         set
         {
            if (_font == value)
               return;
            _font = value;
            Invalidate();
         }
      }

      public LineGraphItem[] Lines
      {
         get { return _lines; }
         set
         {
            if (_lines == value)
               return;
            _lines = value;
            Invalidate();
         }
      }

      public int LineThichkness
      {
         get { return _thickness; }
         set
         {
            if (_thickness == value)
               return;
            _thickness = value;
            Invalidate();
         }
      }

      public float XMaximum
      {
         get { return _maxX; }
         set
         {
            _maxX = value;
            Invalidate();
         }
      }

      public float XMinimum
      {
         get { return _minX; }
         set
         {
            _minX = value;
            Invalidate();
         }
      }

      public float YMaximum
      {
         get { return _maxY; }
         set
         {
            _maxY = value;
            Invalidate();
         }
      }

      public float YMinimum
      {
         get { return _minY; }
         set
         {
            _minY = value;
            Invalidate();
         }
      }

      #endregion

      #region Public Methods

      public void AddLine(LineGraphItem line)
      {
         line.Parent = this;

         if (_lines == null)
         {
            _lines = new[] { line };
         }
         else
         {
            var tmp = new LineGraphItem[_lines.Length + 1];
            Array.Copy(_lines, tmp, _lines.Length);
            tmp[tmp.Length - 1] = line;
            _lines = tmp;
         }

         Invalidate();
      }

      public void AddLines(LineGraphItem[] lines)
      {
         if (lines == null)
            return;

         for (int i = 0; i < lines.Length; i++)
            lines[i].Parent = this;

         if (_lines == null)
            _lines = lines;
         else
         {
            var tmp = new LineGraphItem[_lines.Length + lines.Length];
            Array.Copy(_lines, tmp, _lines.Length);
            Array.Copy(lines, 0, tmp, _lines.Length, lines.Length);
            _lines = tmp;
         }

         Invalidate();
      }

      public void ClearLines()
      {
         _lines = null;

         Invalidate();
      }

      public void RemoveLine(LineGraphItem line)
      {
         if (_lines == null)
            return;

         for (int i = 0; i < _lines.Length; i++)
         {
            if (_lines[i] == line)
            {
               RemoveLineAt(i);
               return;
            }
         }
      }

      public void RemoveLineAt(int index)
      {
         if (_lines == null || index < 0 || index >= _lines.Length)
            return;

         _lines[index].Parent = null;

         if (_lines.Length == 1)
            _lines = null;
         else
         {
            var tmp = new LineGraphItem[_lines.Length - 1];
            int c = 0;
            for (int i = 0; i < _lines.Length; i++)
            {
               if (i != index)
                  tmp[c++] = _lines[i];
            }
            _lines = tmp;
         }

         Invalidate();
      }

      #endregion

      #region GUI

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int w, int h)
      // ReSharper restore RedundantAssignment
      {
         // Calculate Value Width
         //int vWx = System.Math.Max(FontManager.ComputeExtentEx(_font, _minX.ToString()).Width, FontManager.ComputeExtentEx(_font, _maxX.ToString()).Width);
         int vWy = System.Math.Max(FontManager.ComputeExtentEx(_font, _minY.ToString()).Width, FontManager.ComputeExtentEx(_font, _maxY.ToString()).Width);

         // Calculat Space
         w = Width - vWy - 10;
         h = Height - _font.Height - 10;
         x = Left + vWy + 11;
         y = Top;

         // Draw Base
         Core.Screen.DrawLine(0, 1, Left + vWy + 10, Top, Left + vWy + 10, Top + h);
         Core.Screen.DrawLine(0, 1, Left + vWy + 10, Top + h, Left + w + vWy + 10, Top + h);

         // Draw Y Range
         Core.Screen.DrawText(_maxY.ToString(), _font, 0, Left, Top);
         Core.Screen.DrawText(_minY.ToString(), _font, 0, Left, Top + h - _font.Height);

         // Draw X Range
         Core.Screen.DrawText(_minX.ToString(), _font, 0, Left + vWy + 10, Top + h + 10);
         Core.Screen.DrawText(_maxX.ToString(), _font, 0, Left + Width - FontManager.ComputeExtentEx(_font, _maxX.ToString()).Width, Top + h + 10);

         // Draw Graph
         Core.Screen.SetClippingRectangle(x, y, w, h);
         Core.Screen.DrawRectangle(0, 0, x, y, w, h, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 256);

         float xR = _maxX - _minX;
         float yR = _maxY - _minY;
         var lastP = new point();

         if (_lines != null)
         {
            for (int i = 0; i < _lines.Length; i++)
            {
               if (_lines[i].Visible && _lines[i].Points != null)
               {
                  for (int j = 0; j < _lines[i].Points.Length; j++)
                  {
                     int xx = (int)(x + ((_lines[i].Points[j].X - _minX) / xR) * w);
                     int yy = (int)((y + h) - (((_lines[i].Points[j].Y - _minY) / yR) * h));
                     if (j > 0)
                        Core.Screen.DrawLine(_lines[i].Color, _thickness, lastP.X, lastP.Y, xx, yy);
                     lastP = new point(xx, yy);
                  }
               }
            }
         }
      }

      #endregion

   }
}
