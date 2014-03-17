using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF.Modal;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class Textbox : Control
   {

      #region Variables

      private string _editorTitle;
      private Font _editorFont;
      private KeyboardLayout _layout;

      private string _text;
      private Font _font;
      private Color _color;
      private readonly bool _bReadonly;
      private char _pwd;
      private int _caret;
      private int _caretX;
      private bool _showCaret;
      private int _scrollX;

      private int _showChar;
      private Thread _thShow;                  // Thread for hidding a typed character w/ a pwd
      private bool _moved;
      private bool _readOnly;

      #endregion

      #region Constructors

      public Textbox(string name, string text, Font font, Color color, int x, int y, char pwdChar = ' ')
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = FontManager.ComputeExtentEx(font, text).Width + 9;
         Height = font.Height + 9;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _text = text ?? string.Empty;
         _font = font;
         _color = color;
         _bReadonly = false;
         _pwd = pwdChar;
         CurrentCharacter = _text.Length;
         _editorFont = font;
         _showChar = -1;
      }

      public Textbox(string name, string text, Font font, Color color, int x, int y, int width, char pwdChar = ' ')
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = font.Height + 9;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _text = text ?? string.Empty;
         _font = font;
         _color = color;
         _bReadonly = false;
         _pwd = pwdChar;
         CurrentCharacter = _text.Length;
         _editorFont = font;
         _showChar = -1;
      }

      public Textbox(string name, string text, Font font, Color color, int x, int y, int width, int height, char pwdChar = ' ', bool readOnly = false)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _text = text ?? string.Empty;
         _font = font;
         _color = color;
         _bReadonly = false;
         _pwd = pwdChar;
         CurrentCharacter = _text.Length;
         _editorFont = font;
         _showChar = -1;
         _readOnly = readOnly;
      }

      #endregion

      #region Events

      public event OnCaretChanged CaretChanged;
      protected virtual void OnCaretChanged(object sender, int value)
      {
         if (CaretChanged != null)
            CaretChanged(sender, value);
      }

      public event OnTextChanged TextChanged;
      protected virtual void OnTextChanged(object sender, string value)
      {
         if (TextChanged != null)
            TextChanged(sender, value);
      }

      public event OnTextEditorStart TextEditorStart;
      protected virtual void OnTextEditorStart(object sender, ref TextEditorArgs args)
      {
         if (TextEditorStart != null)
            TextEditorStart(sender, ref args);
      }

      public event OnTextEditorClosing TextEditorClosing;
      protected virtual void OnTextEditorClosing(object sender, ref string text, ref bool cancel)
      {
         if (TextEditorClosing != null)
            TextEditorClosing(sender, ref text, ref cancel);
      }

      #endregion

      #region Properties

      public int CurrentCharacter
      {
         get { return _caret; }
         set
         {
            if (_caret == value)
               return;
            if (value < 0)
               value = 0;
            if (value > _text.Length)
               value = _text.Length;

            _caret = value;
            _caretX = FontManager.ComputeExtentEx(_font, _text.Substring(0, _caret)).Width;
            Invalidate();
         }
      }

      public Font EditorFont
      {
         get { return _editorFont; }
         set { _editorFont = value; }
      }

      public KeyboardLayout EditorLayout
      {
         get { return _layout; }
         set { _layout = value; }
      }

      public string EditorTitle
      {
         get { return _editorTitle; }
         set { _editorTitle = value; }
      }

      public Color Forecolor
      {
         get { return _color; }
         set
         {
            if (value == _color)
               return;
            _color = value;
            Invalidate();
         }
      }

      public bool ReadOnly
      {
         get { return _readOnly; }
         set
         {
            if (_readOnly == value)
               return;
            _readOnly = value;
            Invalidate();
         }
      }

      public bool ShowCaret
      {
         get { return _showCaret; }
         set
         {
            if (_showCaret != value)
            {
               _showCaret = value;
               Invalidate();
            }
         }
      }

      public string Text
      {
         get { return _text; }
         set
         {
            if (value == null)
               value = string.Empty;
            if (_text == value)
               return;
            _text = value;
            CurrentCharacter = _text.Length;
            Invalidate();
            OnTextChanged(this, _text);
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

      public char PasswordCharacter
      {
         get { return _pwd; }
         set
         {
            if (_pwd == value)
               return;
            _pwd = value;
            Invalidate();
         }
      }

      #endregion

      #region GUI

      protected override void OnRender(int x, int y, int width, int height)
      {
         if (Core.FlatTextboxes)
         {
            if (Enabled && !_bReadonly)
            {
               Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, Core.SystemColors.WindowColor, 0, 0,
                  Core.SystemColors.WindowColor, 0, 0, 256);
            }
            else
            {
               Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, Core.SystemColors.ControlTop, 0, 0, Core.SystemColors.ControlTop, 0, 0, 256);
            }

            // Draw text & Caret
            Core.Screen.SetClippingRectangle(Left + 2, Top + 3, Width - 4, Height - 6);
            if (Enabled)
            {
               Core.Screen.DrawTextInRect(PwdString(), Left + 3 - _scrollX, Top + 3, Width - 6 + _scrollX,
                  _font.Height, Bitmap.DT_AlignmentLeft, Core.SystemColors.FontColor, _font);
            }
            else
            {
               Core.Screen.DrawTextInRect(PwdString(), Left + 3 - _scrollX, Top + 3, Width - 6 + _scrollX, _font.Height, Bitmap.DT_AlignmentLeft, Core.SystemColors.ControlBottom, _font);
            }

            if (_showCaret || (!Core.UseVirtualKeyboard && Parent.ActiveChild == this))
            {
               Core.Screen.DrawLine(Core.SystemColors.SelectionColor, 1, Left + _caretX - _scrollX + 2, Top, Left + _caretX - _scrollX + 2, Top + _font.Height);
            }

         }
         else
         {
            // Draw Shadow
            Core.Screen.DrawRectangle(Core.SystemColors.WindowColor, 1, Left + 1, Top + 1, Width - 1, Height - 1, 1, 1, Core.SystemColors.WindowColor, 0, 0, Core.SystemColors.WindowColor, 0, 0, 256);

            // Draw Outline
            Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1, Left, Top, Width - 1, Height - 1, 1, 1, 0, 0, 0, 0, 0, 0, 256);

            if (Enabled && !_bReadonly)
            {
               Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, Width - 3, Height - 3, 0, 0, Colors.LightGray, Left,
                  Top, Colors.White, Left, Top + 5, 256);
            }
            else
            {
               Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, Width - 3, Height - 3, 0, 0, Colors.LightGray, 0, 0, Colors.LightGray, 0, 0, 256);
            }

            // Draw text & Caret
            Core.Screen.SetClippingRectangle(Left + 2, Top + 3, Width - 4, Height - 6);
            Core.Screen.DrawTextInRect(PwdString(), Left + 3 - _scrollX, Top + 3, Width - 6 + _scrollX, _font.Height, Bitmap.DT_AlignmentLeft, Core.SystemColors.FontColor, _font);
            if (_showCaret || (!Core.UseVirtualKeyboard && Parent.ActiveChild == this))
            {
               Core.Screen.DrawLine(Core.SystemColors.SelectionColor, 1, Left + _caretX - _scrollX + 2, Top, Left + _caretX - _scrollX + 2, Top + _font.Height);
            }
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
               case 80:        // Left
                  CurrentCharacter -= 1;
                  break;
               case 79:        // Right
                  CurrentCharacter += 1;
                  break;
               case 76:
                  if (_readOnly)
                     return;
                  Delete();
                  Invalidate();
                  break;
            }
         }
         base.KeyboardAltKeyMessage(key, pressed, ref handled);
      }

      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         if (pressed && !_readOnly)
         {
            if (key >= 32 && key <= 190)
            {
               handled = true;
               InsertStrChr(new string(new[] { key }));
               Invalidate();
            }
            else if (key == 8)
            {
               handled = true;
               Backspace();
               Invalidate();
            }
         }
         base.KeyboardKeyMessage(key, pressed, ref handled);
      }

      #endregion

      #region Touch

      protected override void TouchMoveMessage(object sender, point point, ref bool handled)
      {
         _moved = true;
         base.TouchMoveMessage(sender, point, ref handled);
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (Core.UseVirtualKeyboard && Touching && !_moved && !_readOnly)
         {
            new Thread(ShowEditor).Start();
         }

         _moved = false;
         base.TouchUpMessage(sender, point, ref handled);
      }

      private void ShowEditor()
      {
         var args = new TextEditorArgs(_editorFont, _editorTitle, _text);
         OnTextEditorStart(this, ref args);
         var vk = new VirtualKeyboard();
         vk.TextEditorClosing += VK_TextEditorClosing;
         Text = vk.Show(args.EditorFont, args.DefaultValue, _pwd, _layout, args.EditorTitle);
      }

      private void VK_TextEditorClosing(object sender, ref string text, ref bool cancel)
      {
         OnTextEditorClosing(this, ref text, ref cancel);
      }

      #endregion

      #region Private Methods

      private void Backspace()
      {
         if (_caret == 0)
            return;

         int w, h;
         string chr = PwdString().Substring(_caret - 1, 1);
         _font.ComputeExtent(chr, out w, out h);

         _text = _text.Substring(0, _caret - 1) + _text.Substring(_caret--);
         _caretX -= w;
      }

      private void Delete()
      {
         if (_caret == _text.Length)
            return;

         int w, h;
         string chr = PwdString().Substring(_caret, 1);
         _font.ComputeExtent(chr, out w, out h);

         _text = _text.Substring(0, _caret) + _text.Substring(_caret + 1);
      }

      private void HideCharacters()
      {
         Thread.Sleep(750);
         _showChar = -1;

         int w = FontManager.ComputeExtentEx(_font, _text.Substring(_caret - 1, 1)).Width;
         int sw = FontManager.ComputeExtentEx(_font, new string(new[] { _pwd })).Width;

         _caretX -= w - sw;

         Invalidate();
      }

      private void InsertStrChr(string chr)
      {
         int w, h;

         _font.ComputeExtent(chr, out w, out h);

         if (_caret == -1)
            CurrentCharacter = 0;

         if (_pwd != ' ')
         {
            if (Core.UseVirtualKeyboard)
            {
               _thShow = null;
               _showChar = _caret;
               _thShow = new Thread(HideCharacters);
               _thShow.Start();
            }
            else
               w = _font.CharWidth(_pwd);
         }

         if (_caret == 0)
            _text = chr + _text;
         else
            _text = _text.Substring(0, _caret) + chr + _text.Substring(_caret);

         _caret += 1;
         _caretX += w;

         if (_caretX > Width)
            _scrollX = _caretX - Width + 20;
      }

      private string PwdString()
      {
         if (_pwd == ' ')
            return _text;

         string strOut = string.Empty;

         if (_showChar == -1 || _showChar >= _text.Length)
         {
            for (int i = 0; i < _text.Length; i++)
               strOut += _pwd;
         }
         else
         {
            for (int i = 0; i < _showChar; i++)
               strOut += _pwd;

            strOut += _text.Substring(_showChar, 1);

            for (int i = _showChar + 1; i < _text.Length; i++)
               strOut += _pwd;

         }
         return strOut;
      }

      /*private void UpdateCaretPos()
      {
          if (_caret == -1)
              _caretX = 0;
          else
              _caretX = FontManager.ComputeExtentEx(_font, _text.Substring(0, _caret + 1)).Width;
      }*/

      #endregion

   }
}
