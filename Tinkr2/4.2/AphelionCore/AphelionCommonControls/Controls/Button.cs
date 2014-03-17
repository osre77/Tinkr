using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{

   [Serializable]
   public class Button : Control
   {
      #region Variables

      private string _text;
      private Font _font;
      private Color _nfore;
      private Color _pfore;
      private PressState _state;
      private Color _normC1;
      private Color _normC2;
      private Color _pressC1;
      private Color _pressC2;
      private Color _border;
      private Color _pressedBorder;
      private Bitmap _img;
      private ScaleMode _scale;
      private bool _showFocus;
      //private Color _shd;
      private Color _pressedShd;

      #endregion

      #region Constructors

      public Button(string name, string text, Font font, int x, int y, bool showFocusRect = true)
      {
         DefaultColors();

         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         _showFocus = showFocusRect;
         _state = PressState.Normal;

         // Size button
         size sz = FontManager.ComputeExtentEx(_font, _text);
         Width = sz.Width + 22;
         Height = sz.Height + 13;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
      }

      public Button(string name, string text, Font font, int x, int y, int width, int height, bool showFocusRect = true)
      {
         DefaultColors();

         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _showFocus = showFocusRect;
         _state = PressState.Normal;
      }

      public Button(string name, string text, Font font, int x, int y, Color normalTextColor, Color pressedTextColor, Color borderColor, Color pressedBorderColor, bool showFocusRect = true)
      {
         DefaultColors();

         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         _nfore = normalTextColor;
         _pfore = pressedTextColor;
         _showFocus = showFocusRect;
         _state = PressState.Normal;
         _border = borderColor;
         _pressedBorder = pressedBorderColor;

         // Size button
         size sz = FontManager.ComputeExtentEx(_font, _text);
         Width = sz.Width + 22;
         Height = sz.Height + 13;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
      }

      public Button(string name, string text, Font font, int x, int y, int width, int height, Color normalTextColor, Color pressedTextColor, Color borderColor, Color pressedBorderColor, bool showFocusRect = true)
      {
         DefaultColors();

         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _nfore = normalTextColor;
         _pfore = pressedTextColor;
         _border = borderColor;
         _pressedBorder = pressedBorderColor;

         _showFocus = showFocusRect;
         _state = PressState.Normal;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Image to display behind text
      /// </summary>
      public Bitmap BackgroundImage
      {
         get { return _img; }
         set
         {
            if (_img == value)
               return;
            _img = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Scalemode to use when displaying background image
      /// </summary>
      public ScaleMode BackgroundImageScaleMode
      {
         get { return _scale; }
         set
         {
            if (_scale == value)
               return;
            _scale = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Color of border to draw around control
      /// </summary>
      public Color BorderColor
      {
         get { return _border; }
         set
         {
            if (_border == value)
               return;
            _border = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Color of border to use when pressed
      /// </summary>
      public Color BorderColorPressed
      {
         get { return _pressedBorder; }
         set
         {
            if (_pressedBorder == value)
               return;
            _pressedBorder = value;
            if (_state == PressState.Pressed)
               Invalidate();
         }
      }

      /// <summary>
      /// The font used when rendering text
      /// </summary>
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

      /// <summary>
      /// Bottom gradient color to use when button is not pressed
      /// </summary>
      public Color NormalColorBottom
      {
         get { return _normC2; }
         set
         {
            if (_normC2 == value)
               return;
            _normC2 = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Top gradient color to use when button is not pressed
      /// </summary>
      public Color NormalColorTop
      {
         get { return _normC1; }
         set
         {
            if (_normC1 == value)
               return;
            _normC1 = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Color to draw text with when button is not pressed
      /// </summary>
      public Color NormalTextColor
      {
         get { return _nfore; }
         set
         {
            if (_nfore == value)
               return;
            _nfore = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Bottom gradient color to use when button is pressed
      /// </summary>
      public Color PressedColorBottom
      {
         get { return _pressC2; }
         set
         {
            if (_pressC2 == value)
               return;
            _pressC2 = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Top gradient color to use when button is pressed
      /// </summary>
      public Color PressedColorTop
      {
         get { return _pressC1; }
         set
         {
            if (_pressC1 == value)
               return;
            _pressC1 = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Color to draw text with when button is pressed
      /// </summary>
      public Color PressedTextColor
      {
         get { return _pfore; }
         set
         {
            if (_pfore == value)
               return;
            _pfore = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Displays a rect around the control when it has focus if true
      /// </summary>
      public bool ShowFocusRectangle
      {
         get { return _showFocus; }
         set
         {
            if (_showFocus == value)
               return;
            _showFocus = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Text to display on button
      /// </summary>
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
            Invalidate();
         }
      }

      #endregion

      #region Button Invokes

      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int)ButtonIDs.Select || buttonId == (int)ButtonIDs.Click)
         {
            _state = PressState.Pressed;
            Invalidate();
         }
      }

      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         if ((buttonId == (int)ButtonIDs.Select && _state == PressState.Pressed) || (buttonId == (int)ButtonIDs.Click && _state == PressState.Pressed))
         {
            _state = PressState.Normal;
            Invalidate();
            OnTap(this, new point(X, Y));
         }
      }

      #endregion

      #region Touch Invokes

      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         _state = PressState.Pressed;
         Invalidate();
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (Touching)
         {
            _state = PressState.Normal;
            Invalidate();
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region Keyboard Methods

      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         if (key == 10)
         {
            handled = true;
            if (pressed)
            {
               if (_state != PressState.Pressed)
               {
                  _state = PressState.Pressed;
                  Invalidate();
               }
            }
            else
            {
               if (_state == PressState.Pressed)
               {
                  _state = PressState.Normal;
                  Invalidate();
                  OnTap(this, new point(Left, Top));
               }
            }
         }
         base.KeyboardKeyMessage(key, pressed, ref handled);
      }

      #endregion

      #region GUI

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int width, int height)
      // ReSharper restore RedundantAssignment
      {
         _font.ComputeTextInRect(_text, out width, out height, Width - 6);

         // Draw Background
         if (Enabled)
         {
            if (_state == PressState.Pressed)
            {
               Core.Screen.DrawRectangle(_pressedBorder, 1, Left, Top, Width, Height, 0, 0, _pressC1, Left, Top, _pressC2, Left, Top + Height, 256);

               if (_img != null)
                  DrawBackground();

               Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - height / 2) - 1, Width - 9, height, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _pressedShd, _font);
               Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - height / 2), Width - 8, height, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _pfore, _font);
               Core.ShadowRegionInset(Left, Top, Width, Height);
            }
            else
            {
               Core.Screen.DrawRectangle(_border, 1, Left, Top, Width, Height, 0, 0, _normC1, Left, Top, _normC2, Left, Top + Height, 256);

               if (_img != null)
                  DrawBackground();

               Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - height / 2) + 1, Width - 7, height, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, Colors.White, _font);
               Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - height / 2), Width - 8, height, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _nfore, _font);
            }
         }
         else
         {
            Core.Screen.DrawRectangle(_border, 1, Left, Top, Width, Height, 0, 0, _normC2, Left, Top, _normC2, Left, Top + Height, 256);

            if (_img != null)
               DrawBackground();

            Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - height / 2), Width - 8, height, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _nfore, _font);
         }


         // Focus rect
         if (_state != PressState.Pressed && _showFocus && Focused)
         {
            Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, Width - 2, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, Left + 1, Top + Height - 2, Width - 2, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 2, 1, Height - 4, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, Left + Width - 2, Top + 2, 1, Height - 4, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, Left + 2, Top + 2, Width - 4, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
            Core.Screen.DrawRectangle(0, 0, Left + 2, Top + Height - 3, Width - 4, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
            Core.Screen.DrawRectangle(0, 0, Left + 2, Top + 3, 1, Height - 6, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
            Core.Screen.DrawRectangle(0, 0, Left + Width - 3, Top + 3, 1, Height - 6, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
         }
      }

      #endregion

      #region Private Methods

      private void DefaultColors()
      {
         _nfore = Core.SystemColors.FontColor;
         _pfore = Core.SystemColors.SelectedFontColor;
         _border = Core.SystemColors.BorderColor;
         _pressedBorder = Core.SystemColors.PressedControlTop;

         _normC1 = Core.SystemColors.ControlTop;
         _normC2 = Core.SystemColors.ControlBottom;

         _pressC1 = Core.SystemColors.PressedControlTop;
         _pressC2 = Core.SystemColors.PressedControlBottom;

         //_shd = Core.SystemColors.TextShadow;
         _pressedShd = Core.SystemColors.PressedTextShadow;
      }

      private void DrawBackground()
      {
         int w = Width;
         int h = Height;

         switch (_scale)
         {
            case ScaleMode.Center:
               Core.Screen.DrawImage(Left + (w / 2 - _img.Width / 2), Top + (h / 2 - _img.Height / 2), _img, 0, 0, _img.Width, _img.Height);
               break;
            case ScaleMode.Normal:
               Core.Screen.DrawImage(Left, Top, _img, 0, 0, _img.Width, _img.Height);
               break;
            case ScaleMode.Scale:
               float multiplier;

               if (_img.Height > _img.Width)
               {
                  // Portrait
                  if (h > w)
                     multiplier = w / (float)_img.Width;
                  else
                     multiplier = h / (float)_img.Height;
               }
               else
               {
                  // Landscape
                  if (h > w)
                  {
                     multiplier = w / (float)_img.Width;
                  }
                  else
                  {
                     multiplier = h / (float)_img.Height;
                  }
               }

               var dsW = (int)(_img.Width * multiplier);
               var dsH = (int)(_img.Height * multiplier);
               var dX = (int)((float)w / 2 - (float)dsW / 2);
               var dY = (int)((float)h / 2 - (float)dsH / 2);

               Core.Screen.StretchImage(Left + dX, Top + dY, _img, dsW, dsH, 256);
               break;

            case ScaleMode.Stretch:
               Core.Screen.StretchImage(Left, Top, _img, w, h, 256);
               break;

            case ScaleMode.Tile:
               Core.Screen.TileImage(Left, Top, _img, w, h, 256);
               break;
         }
      }

      #endregion
   }
}
