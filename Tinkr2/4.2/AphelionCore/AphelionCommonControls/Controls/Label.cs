using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class Label : Control
   {

      #region Variables

      private string _text = "";
      private HorizontalAlignment _align = HorizontalAlignment.Left;
      private bool _autosize = true;
      private Font _font;
      private Color _color = Colors.Charcoal;
      private Color _bkg = Colors.LightGray;
      private bool _transparent;
      private int _w, _h;

      #endregion

      #region Constructors

      public Label(string name, string text, Font font, int x, int y, bool transparentBackground = true)
      {
         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         _transparent = transparentBackground;

         int w, h;
         _font.ComputeExtent(_text, out w, out h);
         Width = w;
         Height = h;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         DefaultColors();
      }

      public Label(string name, string text, Font font, int x, int y, Color foreColor, bool transparentBackground = true)
      {
         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         _transparent = transparentBackground;
         DefaultColors();
         _color = foreColor;

         int w, h;
         _font.ComputeExtent(_text, out w, out h);
         Width = w;
         Height = h;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
      }

      public Label(string name, string text, Font font, int x, int y, int width, int height, bool transparentBackground = true)
      {
         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _autosize = false;
         _transparent = transparentBackground;
         DefaultColors();
      }

      public Label(string name, string text, Font font, int x, int y, int width, Color foreColor, bool transparentBackground = true)
      {
         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         DefaultColors();
         _color = foreColor;

         Height = FontManager.ComputeExtentEx(Font, Text).Height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _autosize = false;
         _transparent = transparentBackground;
      }

      public Label(string name, string text, Font font, int x, int y, int width, int height, Color foreColor, bool transparentBackground = true)
      {
         Name = name;
         _text = text;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         DefaultColors();
         _color = foreColor;
         _autosize = false;
         _transparent = transparentBackground;
      }

      #endregion

      #region  Properties

      /// <summary>
      /// Gets/Sets autosize state
      /// </summary>
      public bool AutoSize
      {
         get { return _autosize; }
         set
         {
            if (_autosize == value)
               return;
            _autosize = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets label backcolor (sets transparent false)
      /// </summary>
      public Color BackColor
      {
         get { return _bkg; }
         set
         {
            if (_bkg == value)
               return;
            _bkg = value;
            _transparent = false;
            if (_autosize)
               Invalidate();
         }
      }

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
               return;
            _color = value;
            if (_autosize)
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
               return;
            _font = value;
            if (_autosize)
               Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets height (sets autosize false)
      /// </summary>
      public override int Height
      {
         get { return _h; }
         set
         {
            if (_h == value)
               return;
            _h = value;
            _autosize = false;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets text
      /// </summary>
      public string Text
      {
         get { return _text; }
         set
         {
            if (_text == value)
               return;
            _text = value;

            // Auto-Size first
            if (_autosize)
            {
               int w, h;
               _font.ComputeExtent(_text, out w, out h);
               Width = w;
               Height = h;
            }

            Invalidate();

         }
      }

      /// <summary>
      /// Gets/Sets text alignment
      /// </summary>
      public HorizontalAlignment TextAlignment
      {
         get { return _align; }
         set
         {
            if (_align == value)
               return;
            _align = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Get/Sets transparent background state
      /// </summary>
      public bool TransparentBackground
      {
         get { return _transparent; }
         set
         {
            if (_transparent == value)
               return;
            _transparent = value;
            Invalidate();

         }
      }

      /// <summary>
      /// Gets/Sets width (sets autosize false)
      /// </summary>
      public override int Width
      {
         get
         {
            if (_autosize && _w == 0)
            {
               int w, h;
               _font.ComputeExtent(_text, out w, out h);
               _w = w;
            }

            return _w;
         }
         set
         {
            if (_w == value && _autosize == false)
               return;
            _w = value;
            _autosize = false;
            Invalidate();

         }
      }

      #endregion

      #region GUI

      private void DefaultColors()
      {
         _bkg = Core.SystemColors.ContainerBackground;
         _color = Core.SystemColors.FontColor;
      }

      protected override void OnRender(int x, int y, int width, int height)
      {
         uint align;

         if (!_transparent)
         {
            Core.Screen.DrawRectangle(0, 0, Left, Top, _w, _h, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);
         }

         switch (_align)
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

         // Draw String
         Core.Screen.DrawTextInRect(_text, Left, Top, _w, _h, align + Bitmap.DT_WordWrap, _color, _font);
      }

      #endregion

   }
}
