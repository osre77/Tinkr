using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
   public class TextArea : Control
   {

      #region Variables

      private string[] _lines;
      private Font _font;
      private Color _color;
      private int _curLine;
      private int _curColumn;
      private int _caretX;
      //private int _caretY;
#pragma warning disable 649
      private int _scrollX;
#pragma warning restore 649
      private int _scrollY;
      private bool _moved;
      private bool _bQuick;

      #endregion

      #region Constructors

      public TextArea(string name, string text, Font font, Color color, int x, int y, int width)
      {
         Name = name;
         _font = font;
         _color = color;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = font.Height + 9;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         if (text == null)
            text = string.Empty;
         text = Strings.Replace(text, "\r\n", "\n");
         text = Strings.Replace(text, "\r", "\n");
         _lines = text.Split('\n');
         UpdateCaret();
      }

      public TextArea(string name, string text, Font font, Color color, int x, int y, int width, int height)
      {
         Name = name;
         _font = font;
         _color = color;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         if (text == null)
            text = string.Empty;
         text = Strings.Replace(text, "\r\n", "\n");
         text = Strings.Replace(text, "\r", "\n");
         _lines = text.Split('\n');
         UpdateCaret();
      }

      #endregion

      #region Properties

      public int Column
      {
         get { return _curColumn; }
         set
         {
            if (_curColumn == value)
               return;

            _curColumn = value;
            bool lineMoved = false;

            // Going back line(s)
            while (_curColumn < 0)
            {
               lineMoved = true;
               if (_curLine > 0)
               {
                  _curLine -= 1;
                  _curColumn += _lines[_curLine].Length + 1;
               }
               else
                  _curLine = 0;
            }

            // Going forward line(s)
            while (_curColumn > _lines[_curLine].Length)
            {
               lineMoved = true;
               if (_curLine == _lines.Length - 1)
                  _curColumn = _lines[_curLine].Length - 1;
               else
               {
                  _curColumn -= _lines[_curLine].Length + 1;
                  _curLine += 1;
               }
            }

            if (_curColumn < 1)
            {
               _curColumn = 0;
               _caretX = 0;
            }
            else
               _caretX = FontManager.ComputeExtentEx(_font, _lines[_curLine].Substring(0, _curColumn)).Width;

            if (lineMoved)
               Invalidate();
            else
            {
               _bQuick = true;
               Invalidate(new rect(Left + 3, Top + 3 + (_curLine * _font.Height), Width - 6, _font.Height));
            }
         }
      }

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

      public Color ForeColor
      {
         get { return _color; }
         set { _color = value; Invalidate(); }
      }

      public string Text
      {
         get
         {
            if (_lines == null || _lines.Length == 0)
               return string.Empty;

            string sres = string.Empty;
            for (int i = 0; i < _lines.Length; i++)
            {
               sres += _lines[i];
               if (i < _lines.Length - 1)
                  sres += "\n";
            }
            return sres;
         }
         set
         {
            _lines = value.Split('\n');
            Invalidate();
         }
      }

      #endregion

      #region Keyboard

      protected override void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled)
      {
         if (pressed)
         {
            switch (key)
            {
               case 74: // Home
                  Column = 0;
                  break;
               case 76: // Delete
                  Delete();
                  if (Parent != null)
                  {
                     Parent.Render(new rect(Left, Top + (_curLine*_font.Height), Width, _font.Height));
                  }
                  break;
               case 77: // End
                  Column = _lines[_curLine].Length;
                  break;
               case 79: // Right
                  Column += 1;
                  break;
               case 80: // Left
                  Column -= 1;
                  break;
               case 81: // Down
                  if (_curLine != _lines.Length - 1)
                  {
                     _curLine += 1;
                     Invalidate();
                  }
                  break;
               case 82: // Up
                  if (_curLine != 0)
                  {
                     _curLine -= 1;
                     Invalidate();
                  }
                  break;
            }
         }
         base.KeyboardAltKeyMessage(key, pressed, ref handled);
      }

      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         if (pressed)
         {
            if (key >= 32 && key <= 190)
            {
               InsertStrChr(new string(new[] {key}));
            }
            else if (key == 8)
            {
               Backspace();
               Invalidate();
            }
            else if (key == 9)
            {
               InsertStrChr("\t");
               Invalidate();
            }
            else if (key == 10)
            {
               InsertStrChr("\n");
               Invalidate();
            }
         }
         base.KeyboardKeyMessage(key, pressed, ref handled);
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {

      }

      protected override void TouchMoveMessage(object sender, point point, ref bool handled)
      {
         _moved = true;
         base.TouchMoveMessage(sender, point, ref handled);
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (!_moved)
         {
            int x = point.X - Left - 3;
            int y = point.Y - Top - 3;
            int icl = _curLine;

            _curLine = y / _font.Height;
            if (_curLine >= _lines.Length - 1)
            {
               _curLine = _lines.Length - 1;
            }

            _curColumn = 0;
            while (x > 0 && _curColumn < _lines[_curLine].Length)
            {
               int v = _font.CharWidth(_lines[_curLine][_curColumn]);
               x -= v;
               _curColumn += 1;
            }
            UpdateCaret();

            if (_curLine != icl)
            {
               Invalidate();
            }
            else
            {
               _bQuick = true;
               Invalidate(new rect(Left + 3, Top + 3 + (_curLine * _font.Height), Width - 6, _font.Height));
            }
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region GUI

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int width, int height)
      // ReSharper restore RedundantAssignment
      {
         if (_bQuick)
         {
            RenderLine();
         }
         else
         {
            Core.Screen.DrawRectangle(Colors.White, 1, Left + 1, Top + 1, Width - 1, Height - 1, 1, 1, Colors.White, 0, 0, Colors.White, 0, 0, 256);

            // Draw Outline
            Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Colors.DarkGray, 1, Left, Top, Width - 1, Height - 1, 1, 1, 0, 0, 0, 0, 0, 0, 256);

            if (Enabled)
               Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, Width - 3, Height - 3, 0, 0, Colors.LightGray, Left, Top, Colors.White, Left, Top + 5, 256);
            else
               Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, Width - 3, Height - 3, 0, 0, Colors.LightGray, 0, 0, Colors.LightGray, 0, 0, 256);

            // Draw lines
            Core.Screen.SetClippingRectangle(Left + 3, Top + 3, Width - 6, Height - 6);
            int yy = Top + 3 - _scrollY;
            for (int i = 0; i < _lines.Length; i++)
            {
               Core.Screen.DrawTextInRect(_lines[i], Left + 3 - _scrollX, yy, Width - 6 + _scrollX, _font.Height, Bitmap.DT_AlignmentLeft, _color, _font);
               yy += _font.Height;
               if (yy >= Top + Height)
                  break;
            }

            // Draw Caret
            if (Focused)
            {
               y = Top + 3 + (_curLine * _font.Height) - _scrollY;
               Core.Screen.DrawLine(Core.SystemColors.SelectionColor, 1, Left + 3 + _caretX, y, Left + 3 + _caretX, y + _font.Height);
            }
         }
      }

      private void RenderLine()
      {
         int x = Left + 3;
         int w = Width - 6;
         int y = Top + 3 + (_curLine * _font.Height) - _scrollY;
         int h = _font.Height;

         Core.Screen.SetClippingRectangle(x, y, w, h);
         Core.Screen.DrawRectangle(0, 0, x, y, w, h, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 256);
         Core.Screen.DrawTextInRect(_lines[_curLine], Left + 3 - _scrollX, y, Width - 6 + _scrollX, _font.Height, Bitmap.DT_AlignmentLeft, 0, _font);

         // Draw Caret
         if (Focused)
            Core.Screen.DrawLine(Core.SystemColors.SelectionColor, 1, Left + 3 + _caretX, y, Left + 3 + _caretX, y + _font.Height);

         Core.Screen.Flush(x, y, w, h);
         _bQuick = false;
      }

      #endregion

      #region Private Methods

      private void Backspace()
      {
         if (_curColumn == 0)
         {
            if (_curLine == 0)
               return;
            _curLine -= 1;
            _curColumn = _lines[_curLine].Length;
         }
         else
         {
            string chr = _lines[_curLine].Substring(_curColumn - 1, 1);
            _lines[_curLine] = _lines[_curLine].Substring(0, _curColumn - 1) + _lines[_curLine].Substring(_curColumn--);
            _caretX -= FontManager.ComputeExtentEx(_font, chr).Width;
         }

         _bQuick = true;
         Render(true);
         //Invalidate(new rect(Left + 3, Top + 3 + (_curLine * _font.Height), Width - 6, _font.Height));
      }

      private void Delete()
      {
         //if (_caret == _text.Length)
         //    return;

         //int w, h;
         //string chr = pwdString().Substring(_caret, 1);
         //_font.ComputeExtent(chr, out w, out h);

         //_text = _text.Substring(0, _caret) + _text.Substring(_caret + 1);
      }

      private void InsertStrChr(string chr)
      {
         if (chr == "\r" || chr == "\n" || chr == "\r\n")
         {
            _curLine += 1;
            if (_curLine >= _lines.Length)
            {
               // Add a new line
               var tmp = new string[_lines.Length + 1];
               Array.Copy(_lines, tmp, _lines.Length);
               tmp[tmp.Length - 1] = string.Empty;
               _lines = tmp;
            }
            _curColumn = 0;
            _caretX = 0;
         }
         else
         {
            if (_curColumn == 0)
               _lines[_curLine] = chr + _lines[_curLine];
            else
               _lines[_curLine] = _lines[_curLine].Substring(0, _curColumn) + chr + _lines[_curLine].Substring(_curColumn);

            _curColumn += 1;
            _caretX += FontManager.ComputeExtentEx(_font, chr).Width;
         }

         _bQuick = true;
         Render(true);
         //Invalidate(new rect(Left + 3, Top + 3 + (_curLine * _font.Height), Width - 6, _font.Height));
      }

      private void UpdateCaret()
      {
         try
         {
            _caretX = FontManager.ComputeExtentEx(_font, _lines[_curLine].Substring(0, _curColumn)).Width;

            // Update scroll
            int h = _font.Height * _curLine;
            if (h - _scrollY > Height - _font.Height - 3)
               _scrollY = h - Height + _font.Height + 6;

         }
         catch (Exception) { _caretX = 0; }

      }

      #endregion

   }
}
