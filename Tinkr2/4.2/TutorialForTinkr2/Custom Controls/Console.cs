using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.NETMF.Tinkr2.Tutorial.Custom_Controls
{
    /// <summary>
    /// Example custom control
    /// </summary>
    [Serializable]
    public class Console : Control
    {

        #region Variables

        private Font _font;
        private Color _bkg;
        private Color _fore;
        private int _x, _y, _h;
        private string _text;

        #endregion

        #region Constructors

        public Console(string name, Font font, int x, int y, int width, int height)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            _font = font;
            _bkg = Core.SystemColors.WindowColor;
            _fore = Core.SystemColors.FontColor;

            _x = 0;
            _y = 0;
            _h = 0;
            _text = string.Empty;
        }

        public Console(string name, Font font, Color backColor, Color foreColor, int x, int y, int width, int height)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            _font = font;
            _bkg = backColor;
            _fore = foreColor;

            _x = 0;
            _y = 0;
            _h = 0;
            _text = string.Empty;
        }

        #endregion

        #region Properties

        public Color BackColor
        {
            get { return _bkg; }
            set
            {
                if (_bkg == value)
                    return;
                _bkg = value;
                Invalidate();
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
            get { return _fore; }
            set
            {
                if (_fore == value)
                    return;
                _fore = value;
                Invalidate();
            }
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            _x = 0;
            _y = 0;
            _h = 0;
            _text = string.Empty;
            Invalidate();
        }

        public void Print(string text)
        {
            _text += text;
            Invalidate(new rect(Left + _x, Top + _y, Width - _x, Font.Height));
            _x += FontManager.ComputeExtentEx(_font, text).Width;
        }

        public void PrintLine(string text)
        {
            if (_y + _font.Height > Core.ScreenHeight)
                Clear();

            _text += text + "\n";
            _h += _font.Height;
            Invalidate(new rect(Left, Top + _y, Width, Font.Height));
            _y += _font.Height;
            _x = 0;
        }

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int w, int h)
        {
            Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);
            Core.Screen.DrawTextInRect(_text, Left, Top, Width, Height, Bitmap.DT_AlignmentLeft, _fore, _font);
        }

        #endregion

    }
}
