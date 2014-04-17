using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Displays a simple button
   /// </summary>
   [Serializable]
   public class Button : AutoSizeControl
   {
      #region Variables

      private string _text;
      private Font _font;
      private PressState _state;
      private Color _textColor;
      private Color _pressedTextColor;
      private Color _backgroundGradientTopColor;
      private Color _backgroundGradientBottomColor;
      private Color _pressdBackgroundGradientTopColor;
      private Color _pressdBackgroundGradientBottomColor;
      private Color _borderColor;
      private Color _pressedBorderColor;
      private Bitmap _image;
      private ScaleMode _imageScaleMode;
      private Color _textShadowColor;
      private Color _pressedTextShadowColor;
      private ButtonRenderStyles _renderStyle = ButtonRenderStyles.Default;
      private HorizontalAlignment _horizontalTextAlignment = HorizontalAlignment.Center;
      private VerticalAlignment _verticalTextAlignment = VerticalAlignment.Center;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new button
      /// </summary>
      /// <param name="name">Name of the button</param>
      /// <param name="text">Content text of the button</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="showFocusRect">true if the focus rectangle should be visible; false if not</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> and <see cref="Control.Height"/> are calculated automatically to fit the content text.
      /// </remarks>
      public Button(string name, string text, Font font, int x, int y, bool showFocusRect = true) :
         base(name, x, y)
      {
         DefaultColors();

         _text = text;
         _font = font;
         if (!showFocusRect)
         {
            _renderStyle &= ~ButtonRenderStyles.ShowFocusRectangle;
         }
         _state = PressState.Normal;
         SetPadding(new Thickness(11, 6, 11, 7));

         if (!MeasureControl())
         {
            Invalidate();
         }
         /*// Size button
         size sz = FontManager.ComputeExtentEx(_font, _text);
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         Width = sz.Width + 22;
         Height = sz.Height + 13;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor*/
      }

      /// <summary>
      /// Creates a new button
      /// </summary>
      /// <param name="name">Name of the button</param>
      /// <param name="text">Content text of the button</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width of the button in pixel</param>
      /// <param name="height">Height of the button in pixel</param>
      /// <param name="showFocusRect">true if the focus rectangle should be visible; false if not</param>
      public Button(string name, string text, Font font, int x, int y, int width, int height, bool showFocusRect = true) :
         base(name, x, y, width, height)
      {
         DefaultColors();

         _text = text;
         _font = font;

         if (!showFocusRect)
         {
            _renderStyle &= ~ButtonRenderStyles.ShowFocusRectangle;
         }
         _state = PressState.Normal;

         SetPadding(new Thickness(11, 6, 11, 7));
      }

      /// <summary>
      /// Creates a new button
      /// </summary>
      /// <param name="name">Name of the button</param>
      /// <param name="text">Content text of the button</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="normalTextColor">Normal text color (unpressed)</param>
      /// <param name="pressedTextColor">Text color for pressed state</param>
      /// <param name="borderColorColor">Normal border color (unpressed)</param>
      /// <param name="pressedBorderColorColor">Border color for pressed state</param>
      /// <param name="showFocusRect">true if the focus rectangle should be visible; false if not</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> and <see cref="Control.Height"/> are calculated automatically to fit the content text.
      /// </remarks>
      public Button(string name, string text, Font font, int x, int y, Color normalTextColor, Color pressedTextColor, Color borderColorColor, Color pressedBorderColorColor, bool showFocusRect = true) :
         base(name, x, y)
      {
         DefaultColors();

         _text = text;
         _font = font;
         _textColor = normalTextColor;
         _pressedTextColor = pressedTextColor;
         if (!showFocusRect)
         {
            _renderStyle &= ~ButtonRenderStyles.ShowFocusRectangle;
         }
         _state = PressState.Normal;
         _borderColor = borderColorColor;
         _pressedBorderColor = pressedBorderColorColor;

         SetPadding(new Thickness(11, 6, 11, 7));

         /*// Size button
         size sz = FontManager.ComputeExtentEx(_font, _text);
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         Width = sz.Width + 22;
         Height = sz.Height + 13;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor*/
         if (!MeasureControl())
         {
            Invalidate();
         }
      }

      /// <summary>
      /// Creates a new button
      /// </summary>
      /// <param name="name">Name of the button</param>
      /// <param name="text">Content text of the button</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width of the button in pixel</param>
      /// <param name="height">Height of the button in pixel</param>
      /// <param name="normalTextColor">Normal text color (unpressed)</param>
      /// <param name="pressedTextColor">Text color for pressed state</param>
      /// <param name="borderColorColor">Normal border color (unpressed)</param>
      /// <param name="pressedBorderColorColor">Border color for pressed state</param>
      /// <param name="showFocusRect">true if the focus rectangle should be visible; false if not</param>
      public Button(string name, string text, Font font, int x, int y, int width, int height, Color normalTextColor, Color pressedTextColor, Color borderColorColor, Color pressedBorderColorColor, bool showFocusRect = true) :
         base(name, x, y, width, height)
      {
         DefaultColors();

         _text = text;
         _font = font;
         _textColor = normalTextColor;
         _pressedTextColor = pressedTextColor;
         _borderColor = borderColorColor;
         _pressedBorderColor = pressedBorderColorColor;

         if (!showFocusRect)
         {
            _renderStyle &= ~ButtonRenderStyles.ShowFocusRectangle;
         }
         _state = PressState.Normal;

         SetPadding(new Thickness(11, 6, 11, 7));
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets the background image to display
      /// </summary>
      /// <remarks>
      /// The <see cref="Text"/> is rendered above the image
      /// </remarks>
      public Bitmap BackgroundImage
      {
         get { return _image; }
         set
         {
            if (_image == value)
            {
               return;
            }
            _image = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the Scale Mode to use when rendering background image
      /// </summary>
      public ScaleMode BackgroundImageImageScaleModeMode
      {
         get { return _imageScaleMode; }
         set
         {
            if (_imageScaleMode == value)
            {
               return;
            }
            _imageScaleMode = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets color to use for border
      /// </summary>
      public Color BorderColor
      {
         get { return _borderColor; }
         set
         {
            if (_borderColor == value)
            {
               return;
            }
            _borderColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets color to use for border when pressed
      /// </summary>
      public Color BorderColorPressed
      {
         get { return _pressedBorderColor; }
         set
         {
            if (_pressedBorderColor == value)
            {
               return;
            }
            _pressedBorderColor = value;
            if (_state == PressState.Pressed)
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets the Font for rendering text
      /// </summary>
      public Font Font
      {
         get { return _font; }
         set
         {
            if (_font == value)
            {
               return;
            }
            _font = value;
            if (!MeasureControl())
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets bottom color to use in background gradient when button is not pressed
      /// </summary>
      public Color NormalColorBottom
      {
         get { return _backgroundGradientBottomColor; }
         set
         {
            if (_backgroundGradientBottomColor == value)
            {
               return;
            }
            _backgroundGradientBottomColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets top color to use in background gradient when button is not pressed
      /// </summary>
      public Color NormalColorTop
      {
         get { return _backgroundGradientTopColor; }
         set
         {
            if (_backgroundGradientTopColor == value)
            {
               return;
            }
            _backgroundGradientTopColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the color to use when rendering text when button is not pressed
      /// </summary>
      public Color NormalTextColor
      {
         get { return _textColor; }
         set
         {
            if (_textColor == value)
            {
               return;
            }
            _textColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the color to use when rendering text shadow when button is not pressed
      /// </summary>
      /// <remarks>
      /// Remove the <see cref="ButtonRenderStyles.TextShadow"/> flag from <see cref="RenderStyle"/> to turn off text shadows.
      /// </remarks>
      public Color NormalTextShadowColor
      {
         get { return _textShadowColor; }
         set
         {
            if (_textShadowColor == value)
            {
               return;
            }
            _textShadowColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets pressed state bottom color to use in background gradient when button is pressed
      /// </summary>
      public Color PressedColorBottom
      {
         get { return _pressdBackgroundGradientBottomColor; }
         set
         {
            if (_pressdBackgroundGradientBottomColor == value)
            {
               return;
            }
            _pressdBackgroundGradientBottomColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets pressed state top color to use in background gradient when button is pressed
      /// </summary>
      public Color PressedColorTop
      {
         get { return _pressdBackgroundGradientTopColor; }
         set
         {
            if (_pressdBackgroundGradientTopColor == value)
            {
               return;
            }
            _pressdBackgroundGradientTopColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets pressed state color to use when rendering text when button is pressed
      /// </summary>
      public Color PressedTextColor
      {
         get { return _pressedTextColor; }
         set
         {
            if (_pressedTextColor == value)
            {
               return;
            }
            _pressedTextColor = value;
            if (_state == PressState.Pressed)
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets the color to use when rendering text shadow when button is pressed
      /// </summary>
      /// <remarks>
      /// Remove the <see cref="ButtonRenderStyles.TextShadow"/> flag from <see cref="RenderStyle"/> to turn off text shadows.
      /// </remarks>
      public Color PressedTextShadowColor
      {
         get { return _pressedTextShadowColor; }
         set
         {
            if (_pressedTextShadowColor == value)
            {
               return;
            }
            _pressedTextShadowColor = value;
            if (_state == PressState.Pressed)
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets the render style of the button
      /// </summary>
      public ButtonRenderStyles RenderStyle
      {
         get { return _renderStyle; }
         set
         {
            if (value == _renderStyle)
            {
               return;
            }
            _renderStyle = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets whether or not to draw focus rectangle
      /// </summary>
      /// <remarks>
      /// If false a focus rectangle will not be drawn even if control has focus
      /// </remarks>
      [Obsolete("Use RenderStlye, RenderStyles.ShowFocusRectangle")]
      public bool ShowFocusRectangle
      {
         get { return (RenderStyle & ButtonRenderStyles.ShowFocusRectangle) != 0; }
         set
         {
            if (ShowFocusRectangle == value)
            {
               return;
            }
            if (value)
            {
               RenderStyle |= ButtonRenderStyles.ShowFocusRectangle;
            }
            else
            {
               RenderStyle &= ~ButtonRenderStyles.ShowFocusRectangle;
            }
         }
      }

      /// <summary>
      /// Gets/Sets the content text
      /// </summary>
      public string Text
      {
         get { return _text; }
         set
         {
            if (value == null)
            {
               value = string.Empty;
            }
            if (_text == value)
            {
               return;
            }
            _text = value;
            if (!MeasureControl())
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets horizontal text alignment
      /// </summary>
      public HorizontalAlignment HorizontalTextAlignment
      {
         get { return _horizontalTextAlignment; }
         set
         {
            if (_horizontalTextAlignment == value)
            {
               return;
            }
            _horizontalTextAlignment = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the vertical text alignment
      /// </summary>
      public VerticalAlignment VerticalTextAlignment
      {
         get { return _verticalTextAlignment; }
         set
         {
            if (_verticalTextAlignment == value)
            {
               return;
            }
            _verticalTextAlignment = value;
            Invalidate();
         }
      }

      #endregion

      #region Button Invokes

      /// <summary>
      /// Override this message to handle button pressed events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// On button id <see cref="ButtonIDs.Select"/> or <see cref="ButtonIDs.Click"/> the button is switched to <see cref="PressState.Pressed"/> state
      /// </remarks>
      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int)ButtonIDs.Select || buttonId == (int)ButtonIDs.Click)
         {
            _state = PressState.Pressed;
            Invalidate();
         }
         base.ButtonPressedMessage(buttonId, ref handled);
      }

      /// <summary>
      /// Override this message to handle button released events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// On button id <see cref="ButtonIDs.Select"/> or <see cref="ButtonIDs.Click"/> the button is switched back to <see cref="PressState.Normal"/> state
      /// and the <see cref="Control.Tap"/> event is fired.
      /// </remarks>
      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         if (_state == PressState.Pressed && (buttonId == (int)ButtonIDs.Select || buttonId == (int)ButtonIDs.Click))
         {
            _state = PressState.Normal;
            Invalidate();
            OnTap(this, new point(X, Y));
         }
         base.ButtonReleasedMessage(buttonId, ref handled);
      }

      #endregion

      #region Touch Invokes

      // ReSharper disable once RedundantAssignment
      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Switches the button into <see cref="PressState.Pressed"/> state
      /// </remarks>
      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         _state = PressState.Pressed;
         Invalidate();
         // do NOT set handled to true !
         base.TouchDownMessage(sender, point, ref handled);
      }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Switches the button to <see cref="PressState.Normal"/> state if currently pressed
      /// </remarks>
      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (Touching)
         {
            _state = PressState.Normal;
            Invalidate();
            // do NOT set handled to true !
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region Keyboard Methods

      /// <summary>
      /// Override this message to handle key events internally.
      /// </summary>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// If the return Key (key == 10) is pressed or released the button is switched to
      /// <see cref="PressState.Pressed"/> and <see cref="PressState.Normal"/> state.
      /// On release the <see cref="Control.Tap"/> event is fired.
      /// </remarks>
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

      private void CalcTextMetrics(AutoSizeModes autoSizeMode, out int textWidth, out int texhtHeight, out uint textFlags)
      {
         int availableWidth;
         int availHeight;
         textFlags = Bitmap.DT_TrimmingNone;

         if ((autoSizeMode & AutoSizeModes.Width) != 0)
         {
            availableWidth = Int32.MaxValue;
         }
         else
         {
            availableWidth = Width - Padding.Horizontal;
            textFlags |= Bitmap.DT_WordWrap;
         }
         if ((autoSizeMode & AutoSizeModes.Height) != 0)
         {
            availHeight = Int32.MaxValue;
         }
         else
         {
            availHeight = Height - Padding.Vertical;
         }
         switch (HorizontalTextAlignment)
         {
            case HorizontalAlignment.Center:
               textFlags |= Bitmap.DT_AlignmentCenter;
               break;

            case HorizontalAlignment.Right:
               textFlags |= Bitmap.DT_AlignmentRight;
               break;
         }
         _font.ComputeTextInRect(_text, out textWidth, out texhtHeight, 0, 0, availableWidth, availHeight, textFlags);
      }

      /// <summary>
      /// Is called when ever the size of the control needs to be calculated.
      /// </summary>
      /// <param name="autoSizeMode">Current <see cref="AutoSizeControl.AutoSizeMode"/></param>
      /// <returns>Returns the needed size for the control</returns>
      protected override size OnMeasureControl(AutoSizeModes autoSizeMode)
      {
         int textWidth;
         int textHeight;
         uint textFlags;

         CalcTextMetrics(autoSizeMode, out textWidth, out textHeight, out textFlags);

         return new size(textWidth + Padding.Horizontal, textHeight + Padding.Vertical);

         //var size = FontManager.ComputeExtentEx(_font, _text);
         //size.Grow(Padding.Horizontal, Padding.Vertical);
         //return size;
      }

      /// <summary>
      /// Renders the control contents
      /// </summary>
      /// <param name="x">X position in screen coordinates</param>
      /// <param name="y">Y position in screen coordinates</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <remarks>
      /// Renders the button in disabled, normal and pressed state and the focus rectangle if needed.
      /// </remarks>
      protected override void OnRender(int x, int y, int width, int height)
      {
         /*int w;
         int h;
         //_font.ComputeTextInRect(_text, out w, out h, Width - 6);
         _font.ComputeTextInRect(_text, out w, out h, Width - Padding.Horizontal);*/

         int textWidth;
         int textHeight;
         uint textFlags;

         CalcTextMetrics(AutoSizeMode, out textWidth, out textHeight, out textFlags);

         int textY;
         switch (VerticalTextAlignment)
         {
            case VerticalAlignment.Center:
               textY = Top + Padding.Top + ((Height - Padding.Vertical) - textHeight)/2;
               break;

            case VerticalAlignment.Bottom:
               textY = Top + Height - Padding.Bottom - textHeight;
               break;

            default:
               textY = Top + Padding.Top;
               break;
         }

         // Draw Background
         if (Enabled)
         {
            if (_state == PressState.Pressed)
            {
               Core.Screen.DrawRectangle(_pressedBorderColor, 1, Left, Top, Width, Height, 0, 0, _pressdBackgroundGradientTopColor, Left, Top, _pressdBackgroundGradientBottomColor, Left, Top + Height, 256);

               if (_image != null)
               {
                  DrawBackground();
               }

               if ((RenderStyle & ButtonRenderStyles.TextShadow) != 0)
               {
                  //Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - h / 2) - 1, Width - 9, h, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _pressedShd, _font);
                  Core.Screen.DrawTextInRect(_text, Left + Padding.Left, textY - 1, Width - Padding.Horizontal - 1, textHeight, textFlags, _pressedTextShadowColor, _font);
               }
               //Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - h / 2), Width - 8, h, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _pfore, _font);
               Core.Screen.DrawTextInRect(_text, Left + Padding.Left, textY, Width - Padding.Horizontal, textHeight, textFlags, _pressedTextColor, _font);

               if ((RenderStyle & ButtonRenderStyles.PressedShadowBorder) != 0)
               {
                  Core.ShadowRegionInset(Left, Top, Width, Height);
               }
            }
            else
            {
               Core.Screen.DrawRectangle(_borderColor, 1, Left, Top, Width, Height, 0, 0, _backgroundGradientTopColor, Left, Top, _backgroundGradientBottomColor, Left, Top + Height, 256);

               if (_image != null)
               {
                  DrawBackground();
               }

               if ((RenderStyle & ButtonRenderStyles.TextShadow) != 0)
               {
                  //Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - h / 2) + 1, Width - 7, h, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, Colors.White, _font);
                  Core.Screen.DrawTextInRect(_text, Left + Padding.Left, textY + 1, Width - Padding.Horizontal + 1, textHeight, textFlags, _textShadowColor, _font);
               }
               //Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - h / 2), Width - 8, h, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _nfore, _font);
               Core.Screen.DrawTextInRect(_text, Left + Padding.Left, textY, Width - Padding.Horizontal, textHeight, textFlags, _textColor, _font);
            }
         }
         else
         {
            Core.Screen.DrawRectangle(_borderColor, 1, Left, Top, Width, Height, 0, 0, _backgroundGradientBottomColor, Left, Top, _backgroundGradientBottomColor, Left, Top + Height, 256);

            if (_image != null)
            {
               DrawBackground();
            }

            //Core.Screen.DrawTextInRect(_text, Left + 4, Top + (Height / 2 - h / 2), Width - 8, h, Bitmap.DT_AlignmentCenter + Bitmap.DT_WordWrap + Bitmap.DT_TrimmingNone, _nfore, _font);
            Core.Screen.DrawTextInRect(_text, Left + Padding.Left, textY, Width - Padding.Horizontal, textHeight, textFlags, _textColor, _font);
         }

         // Focus rectangle
         if (_state != PressState.Pressed && Focused && (RenderStyle & ButtonRenderStyles.ShowFocusRectangle) != 0 && (Enabled || (RenderStyle & ButtonRenderStyles.ShowDisabledFocusRectangle) != 0))
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

         // no need to call base.OnRender
      }

      #endregion

      #region Private Methods

      private void DefaultColors()
      {
         _textColor = Core.SystemColors.FontColor;
         _pressedTextColor = Core.SystemColors.SelectedFontColor;
         _borderColor = Core.SystemColors.BorderColor;
         _pressedBorderColor = Core.SystemColors.PressedControlTop;

         _backgroundGradientTopColor = Core.SystemColors.ControlTop;
         _backgroundGradientBottomColor = Core.SystemColors.ControlBottom;

         _pressdBackgroundGradientTopColor = Core.SystemColors.PressedControlTop;
         _pressdBackgroundGradientBottomColor = Core.SystemColors.PressedControlBottom;

         _textShadowColor = Core.SystemColors.TextShadow;
         _pressedTextShadowColor = Core.SystemColors.PressedTextShadow;
      }

      private void DrawBackground()
      {
         int w = Width;
         int h = Height;

         switch (_imageScaleMode)
         {
            case ScaleMode.Center:
               Core.Screen.DrawImage(Left + (w / 2 - _image.Width / 2), Top + (h / 2 - _image.Height / 2), _image, 0, 0, _image.Width, _image.Height);
               break;

            case ScaleMode.Normal:
               Core.Screen.DrawImage(Left, Top, _image, 0, 0, _image.Width, _image.Height);
               break;

            case ScaleMode.Scale:
               float multiplier;

               if (_image.Height > _image.Width)
               {
                  // Portrait
                  if (h > w)
                  {
                     multiplier = w/(float) _image.Width;
                  }
                  else
                  {
                     multiplier = h / (float)_image.Height;
                  }
               }
               else
               {
                  // Landscape
                  if (h > w)
                  {
                     multiplier = w / (float)_image.Width;
                  }
                  else
                  {
                     multiplier = h / (float)_image.Height;
                  }
               }

               var dsW = (int)(_image.Width * multiplier);
               var dsH = (int)(_image.Height * multiplier);
               var dX = (int)((float)w / 2 - (float)dsW / 2);
               var dY = (int)((float)h / 2 - (float)dsH / 2);

               Core.Screen.StretchImage(Left + dX, Top + dY, _image, dsW, dsH, 256);
               break;

            case ScaleMode.Stretch:
               Core.Screen.StretchImage(Left, Top, _image, w, h, 256);
               break;

            case ScaleMode.Tile:
               Core.Screen.TileImage(Left, Top, _image, w, h, 256);
               break;
         }
      }

      #endregion
   }

   /// <summary>
   /// Render style flags for <see cref="Button"/>
   /// </summary>
   [Flags]
   public enum ButtonRenderStyles
   {
      /// <summary>
      /// Disables all render styles
      /// </summary>
      None = 0,

      /// <summary>
      /// Render text shadow
      /// </summary>
      TextShadow = 0x01,

      /// <summary>
      /// Draw inner shadow on <see cref="Button"/> when pressed
      /// </summary>
      PressedShadowBorder = 0x02,

      /// <summary>
      /// Draw rectangle around <see cref="Button"/> when focused
      /// </summary>
      ShowFocusRectangle = 0x04,

      /// <summary>
      /// Draw focus rectangle around <see cref="Button"/> even when disabled
      /// </summary>
      ShowDisabledFocusRectangle = 0x08,

      /// <summary>
      /// Default <see cref="Button"/> render style
      /// </summary>
      /// <remarks>
      /// The default style includes <see cref="TextShadow"/>, <see cref="PressedShadowBorder"/>, <see cref="ShowFocusRectangle"/> and <see cref="ShowDisabledFocusRectangle"/>.
      /// </remarks>
      Default = TextShadow | PressedShadowBorder | ShowFocusRectangle | ShowDisabledFocusRectangle
   }
}
