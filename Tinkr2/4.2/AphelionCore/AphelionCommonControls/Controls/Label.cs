using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Displays a static block of text
   /// </summary>
   [Serializable]
   public class Label : AutoSizeControl
   {
      #region Variables

      private string _text = "";
      private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
      private VerticalAlignment _verticalAlignment = VerticalAlignment.Top;
      private Font _font;
      private Color _color;
      private Color _backColor;
      private bool _isTransparent;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new label
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="text">Text to display</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="isTransparentBackground">true if the background should be rendered transparent</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> and <see cref="Control.Height"/> is calculated automatically to fit the font.
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.WidthAndHeight"/>
      /// </remarks>
      public Label(string name, string text, Font font, int x, int y, bool isTransparentBackground = true) :
         base(name, x, y)
      {
         _text = text;
         _font = font;
         _isTransparent = isTransparentBackground;
         DefaultColors();
         if (!MeasureControl())
         {
            Invalidate();
         }
      }

      /// <summary>
      /// Creates a new label
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="text">Text to display</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="foreColor">Color to use for text rendering</param>
      /// <param name="isTransparentBackground">true if the background should be rendered transparent</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> and <see cref="Control.Height"/> is calculated automatically to fit the font.
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.WidthAndHeight"/>
      /// </remarks>
      public Label(string name, string text, Font font, int x, int y, Color foreColor, bool isTransparentBackground = true) :
         base(name, x, y)
      {
         _text = text;
         _font = font;
         _isTransparent = isTransparentBackground;
         DefaultColors();
         _color = foreColor;
         if (!MeasureControl())
         {
            Invalidate();
         }
      }

      /// <summary>
      /// Creates a new label
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="text">Text to display</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <param name="isTransparentBackground">true if the background should be rendered transparent</param>
      /// <remarks>
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.None"/>
      /// </remarks>
      public Label(string name, string text, Font font, int x, int y, int width, int height, bool isTransparentBackground = true) :
         base(name, x, y, width, height)
      {
         _text = text;
         _font = font;
         _isTransparent = isTransparentBackground;
         DefaultColors();
         Invalidate();
      }

      /// <summary>
      /// Creates a new label
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="text">Text to display</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="foreColor">Color to use for text rendering</param>
      /// <param name="isTransparentBackground">true if the background should be rendered transparent</param>
      /// <remarks>
      /// The <see cref="Control.Height"/> is calculated automatically to fit the font.
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.Height"/>
      /// </remarks>
      public Label(string name, string text, Font font, int x, int y, int width, Color foreColor, bool isTransparentBackground = true) :
         base(name, x, y, width, 0)
      {
         _text = text;
         _font = font;
         DefaultColors();
         _color = foreColor;
         _isTransparent = isTransparentBackground;
         AutoSizeMode = AutoSizeModes.Height;
      }

      /// <summary>
      /// Creates a new label
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="text">Text to display</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <param name="foreColor">Color to use for text rendering</param>
      /// <param name="isTransparentBackground">true if the background should be rendered transparent</param>
      /// <remarks>
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.None"/>
      /// </remarks>
      public Label(string name, string text, Font font, int x, int y, int width, int height, Color foreColor, bool isTransparentBackground = true) :
         base(name, x, y, width, height)
      {
         _text = text;
         _font = font;
         DefaultColors();
         _color = foreColor;
         _isTransparent = isTransparentBackground;
         Invalidate();
      }

      #endregion

      #region  Properties

      /// <summary>
      /// Gets/Sets auto size state
      /// </summary>
      [Obsolete("Use AutoSizeControl.AutoSizeMode")]
      public bool AutoSize
      {
         get { return AutoSizeMode == AutoSizeModes.WidthAndHeight; }
         set 
         {
            AutoSizeMode = value ? AutoSizeModes.WidthAndHeight : AutoSizeModes.None;
         }
      }

      /// <summary>
      /// Gets/Sets label back color (sets transparent false)
      /// </summary>
      public Color BackColor
      {
         get { return _backColor; }
         set
         {
            if (_backColor == value)
            {
               return;
            }
            _backColor = value;
            _isTransparent = false;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets the controls ability to receive focus
      /// </summary>
      /// <remarks>
      /// Labels can not be focused. Always returns false
      /// </remarks>
      public override bool CanFocus
      {
         get { return false; }
      }

      /// <summary>
      /// Gets/Sets font color
      /// </summary>
      public Color Color
      {
         get { return _color; }
         set
         {
            if (_color == value)
            {
               return;
            }
            _color = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets font
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
      /// Gets/Sets the text
      /// </summary>
      public string Text
      {
         get { return _text; }
         set
         {
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
      public HorizontalAlignment TextAlignment
      {
         get { return _horizontalAlignment; }
         set
         {
            if (_horizontalAlignment == value)
            {
               return;
            }
            _horizontalAlignment = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the vertical text alignment
      /// </summary>
      public VerticalAlignment VerticalAlignment
      {
         get { return _verticalAlignment; }
         set
         {
            if (_verticalAlignment == value)
            {
               return;
            }
            _verticalAlignment = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Get/Sets transparent background state
      /// </summary>
      public bool IsTransparentBackground
      {
         get { return _isTransparent; }
         set
         {
            if (_isTransparent == value)
            {
               return;
            }
            _isTransparent = value;
            Invalidate();
         }
      }

      #endregion

      #region GUI

      private void DefaultColors()
      {
         _backColor = Core.SystemColors.ContainerBackground;
         _color = Core.SystemColors.FontColor;
      }

      /// <summary>
      /// Is called when ever the size of the control needs to be calculated.
      /// </summary>
      /// <param name="autoSizeMode">Current <see cref="AutoSizeControl.AutoSizeMode"/></param>
      /// <returns>Returns the needed size for the control</returns>
      protected override size OnMeasureControl(AutoSizeModes autoSizeMode)
      {
         var size = FontManager.ComputeExtentEx(_font, _text);
         size.Grow(Padding.Horizontal, Padding.Vertical);
         return size;
      }

      /// <summary>
      /// Renders the control contents
      /// </summary>
      /// <param name="x">X position in screen coordinates</param>
      /// <param name="y">Y position in screen coordinates</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <remarks>
      /// Renders the label
      /// </remarks>
      protected override void OnRender(int x, int y, int width, int height)
      {
         uint align;

         if (!_isTransparent)
         {
            Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, _backColor, 0, 0, _backColor, 0, 0, 256);
         }

         switch (_horizontalAlignment)
         {
            case HorizontalAlignment.Center:
               align = Bitmap.DT_AlignmentCenter;
               break;

            case HorizontalAlignment.Right:
               align = Bitmap.DT_AlignmentRight;
               break;

            default:
               align = Bitmap.DT_AlignmentLeft;
               break;
         }

         int paddedHeight = Height - Padding.Vertical;
         int textY;
         int textHeight;

         switch (VerticalAlignment)
         {
            case VerticalAlignment.Center:
               textHeight = OnMeasureControl(AutoSizeModes.Height).Height - Padding.Horizontal;
               textY = Top + (paddedHeight - textHeight) / 2;
               break;

           case VerticalAlignment.Bottom:
               textHeight = OnMeasureControl(AutoSizeModes.Height).Height - Padding.Vertical;
               textY = (Top + Height - Padding.Bottom) - textHeight;
               break;

            //case VerticalAlignment.Top:
            default:
               textY = Top + Padding.Top;
               textHeight = paddedHeight;
               break;
         }
         if (textHeight > paddedHeight)
         {
            textHeight = paddedHeight;
            textY = Top + Padding.Top;
         }

         // Draw String
         Core.Screen.DrawTextInRect(_text, Left + Padding.Left, textY, Width - Padding.Horizontal, textHeight, align + Bitmap.DT_WordWrap, _color, _font);

         // no need to call base.OnRender
      }
      #endregion
   }
}
