using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class Checkbox : Control
    {

        #region Variables

        private bool _checked;
        private string _text;
        private Font _font;
        private Font _chkFnt;
        private int _chkH, _chkX, _chkY;

        private Color _fore;
        private Color _brdr;
        private Color _bkgTop;
        private Color _bkgBtm;
        private Color _markUn;
        private Color _mark;

        #endregion

        #region Constructors

        public Checkbox(string name, string text, Font font, int x, int y, bool checkValue = false)
        {
            Name = name;
            _text = text;
            _font = font;
            X = x;
            Y = y;
            _checked = checkValue;
            GetChkFont(font.Height);
            Width = _chkH + 12 + FontManager.ComputeExtentEx(font, text).Width;
            Height = System.Math.Max(_chkH, _font.Height);
            DefaultColors();
        }

        public Checkbox(string name, string text, Font font, int x, int y, int height, bool checkValue = false)
        {
            Name = name;
            _text = text;
            _font = font;
            X = x;
            Y = y;
            Height = height;
            _checked = checkValue;
            GetChkFont(height);
            Width = _chkH + 12 + FontManager.ComputeExtentEx(font, text).Width;

            DefaultColors();
        }

        #endregion

        #region Events

        public event OnCheckChanged CheckChanged;
        protected virtual void OnCheckChanged(object sender, bool checkValue)
        {
            if (CheckChanged != null)
                CheckChanged(sender, checkValue);
        }

        #endregion

        #region Properties

        public Color BackgroundBottom
        {
            get { return _bkgBtm; }
            set
            {
                if (_bkgBtm == value)
                    return;
                _bkgBtm = value;
                Invalidate();
            }
        }

        public Color BackgroundTop
        {
            get { return _bkgTop; }
            set
            {
                if (_bkgTop == value)
                    return;
                _bkgTop = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get { return _brdr; }
            set
            {
                if (_brdr == value)
                    return;
                _brdr = value;
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

        public Font Font
        {
            get { return _font; }
            set
            {
                if (_font == value)
                    return;
                _font = value;
                GetChkFont(_font.Height);
                Width = _chkH + 12 + FontManager.ComputeExtentEx(_font, _text).Width;
                Invalidate();
            }
        }

        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                GetChkFont(Height);
                Invalidate();
            }
        }

        public Color MarkColor
        {
            get { return _markUn; }
            set
            {
                if (_markUn == value)
                    return;
                _markUn = value;
                Invalidate();
            }
        }

        public Color MarkSelectedColor
        {
            get { return _mark; }
            set
            {
                if (_mark == value)
                    return;
                _mark = value;
                Invalidate();
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                Width = _chkH + 12 + FontManager.ComputeExtentEx(_font, _text).Width;
            }
        }

        /// <summary>
        /// Gets/Sets value
        /// </summary>
        public bool Value
        {
            get { return _checked; }
            set
            {
                if (_checked == value)
                    return;
                _checked = value;
                Invalidate();
                OnCheckChanged(this, _checked);
            }
        }

        #endregion

        #region Buttons

        protected override void ButtonPressedMessage(int buttonID, ref bool handled)
        {
            if (buttonID == (int)ButtonIDs.Click || buttonID == (int)ButtonIDs.Select)
                Value = !_checked;
        }

        #endregion

        #region Keyboard

        protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
        {
            if (key == 10)
                Value = !_checked;
        }

        #endregion

        #region Touch

        protected override void TouchUpMessage(object sender, point e, ref bool handled)
        {
            if (Touching)
                Value = !_checked;
        }

        #endregion

        #region GUI

        private void DefaultColors()
        {
            _fore = Core.SystemColors.FontColor;
            _brdr = Core.SystemColors.BorderColor;
            _bkgBtm = Core.SystemColors.ControlBottom;
            _bkgTop = Core.SystemColors.ControlTop;
            _mark = Core.SystemColors.SelectionColor;
            _markUn = Colors.DarkGray;
        }

        protected override void OnRender(int x, int y, int w, int h)
        {
            int cY = Top + (Height / 2 - _chkH / 2);


            // Draw Fill
            int ho = (Height - 3) / 2;

            if (Core.FlatCheckboxes)
            {
                if (Enabled)
                    Core.Screen.DrawRectangle(0, 0, Left, cY, _chkH, _chkH, 0, 0, _bkgBtm, Left, cY, _bkgBtm, Left, cY + (_chkH / 2), 256);
                else
                    Core.Screen.DrawRectangle(0, 0, Left, cY, _chkH, _chkH, 0, 0, _bkgTop, Left, cY, _bkgTop, Left, cY + (_chkH / 2), 256);

                if (Focused)
                    Core.ShadowRegionInset(Left, cY, _chkH, _chkH, Core.SystemColors.SelectionColor);
            }
            else
            {
                // Draw Outline
                Core.Screen.DrawRectangle((Parent.ActiveChild == this) ? Core.SystemColors.SelectionColor : _brdr, 1, Left, cY, _chkH, _chkH, 1, 1, 0, 0, 0, 0, 0, 0, 256);

                if (Enabled)
                    Core.Screen.DrawRectangle(0, 0, Left + 1, cY + 1, _chkH - 2, _chkH - 2, 0, 0, _bkgTop, Left, cY, _bkgBtm, Left, cY + (_chkH / 2), 256);
                else
                    Core.Screen.DrawRectangle(0, 0, Left + 1, cY + 1, _chkH - 2, _chkH - 2, 0, 0, _bkgTop, Left, cY, _bkgTop, Left, cY + (_chkH / 2), 256);

                if (Focused)
                    Core.ShadowRegionInset(Left + 1, cY + 1, _chkH - 2, _chkH - 2, Core.SystemColors.SelectionColor);
            }

            // Draw Checkmark
            cY -= _chkY;
            int cX = _chkH / 2 - _chkX;
            Core.Screen.DrawTextInRect("a", Left + cX,  cY, _chkFnt.CharWidth('a'), _chkFnt.Height, Bitmap.DT_AlignmentCenter, (_checked) ? _mark : _markUn, _chkFnt);

            // Draw Text
            Core.Screen.DrawText(_text, _font, _fore, Left + _chkH + 6, Top + (h / 2 - _font.Height / 2));
        }

        private void GetChkFont(int baseH)
        {
            _chkH = baseH;
            _chkY = 0;
            
            if (_chkH < 6)
                _chkH = 6;

            if (_chkH < 14)
            {
                _chkH = 12;
                _chkX = 6;
                _chkY += 3;
                _chkFnt = Resources.GetFont(Resources.FontResources.chk8);
            }
            else if (_chkH < 16)
            {
                _chkH = 14;
                _chkX = 7;
                _chkY += 3;
                _chkFnt = Resources.GetFont(Resources.FontResources.chk12);
            }
            else if (_chkH < 21)
            {
                _chkH = 16;
                _chkX = 9;
                _chkY += 5;
                _chkFnt = Resources.GetFont(Resources.FontResources.chk16);
            }
            else if (_chkH < 26)
            {
                _chkH = 21;
                _chkX = 14;
                _chkY += 7;
                _chkFnt = Resources.GetFont(Resources.FontResources.chk24);
            }
            else if (_chkH < 36)
            {
                _chkH = 26;
                _chkX = 20;
                _chkY += 11;
                _chkFnt = Resources.GetFont(Resources.FontResources.chk32);
            }
            else if (_chkH < 51)
            {
                _chkH = 36;
                _chkX = 30;
                _chkY += 17;
                _chkFnt = Resources.GetFont(Resources.FontResources.chk48);
            }
            else
            {
                _chkH = 51;
                _chkX = 45;
                _chkY += 26;
                _chkFnt = Resources.GetFont(Resources.FontResources.chk72);
            }
        }

        #endregion

    }
}
