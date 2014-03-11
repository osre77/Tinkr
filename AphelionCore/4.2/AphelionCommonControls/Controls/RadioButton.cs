using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class RadioButton : Control
    {

        #region Variables

        private bool _checked;
        private string _text;
        private Font _font;
        private int _chkH;
        private bool _autoSize;

        private Color _fore;
        private Color _brdr;
        private Color _bkgTop;
        private Color _bkgBtm;

        private string _group;
        private bool _sDown;

        #endregion

        #region Constructors

        public RadioButton(string name, string text, Font font, int x, int y, string groupName = "default", bool checkValue = false)
        {
            Name = name;
            _text = text;
            _font = font;
            X = x;
            Y = y;
            Height = font.Height + 13;
            _checked = checkValue;
            _chkH = Height - 19;
            Width = _chkH + 12 + FontManager.ComputeExtentEx(font, text).Width;
            _group = groupName;

            DefaultColors();
        }

        public RadioButton(string name, string text, Font font, int x, int y, int height, string groupName = "default", bool checkValue = false)
        {
            Name = name;
            _text = text;
            _font = font;
            X = x;
            Y = y;
            Height = height;
            _checked = checkValue;
            _chkH = Height - 19;
            Width = _chkH + 12 + FontManager.ComputeExtentEx(font, text).Width;
            _group = groupName;

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

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value)
                    return;
                _checked = value;
                if (_checked)
                    SetThisChecked();
                else
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
                Width = _chkH + 12 + FontManager.ComputeExtentEx(_font, _text).Width;
            }
        }

        #endregion

        #region Button

        protected override void ButtonPressedMessage(int buttonID, ref bool handled)
        {
            if (buttonID == (int)ButtonIDs.Click)
                _sDown = true;
        }

        protected override void ButtonReleasedMessage(int buttonID, ref bool handled)
        {
            if (_sDown && buttonID == (int)ButtonIDs.Click)
            {
                SetThisChecked();
                _sDown = false;
            }
        }

        #endregion

        #region Touch

        protected override void TouchUpMessage(object sender, point e, ref bool handled)
        {
            if (Touching && ScreenBounds.Contains(e))
                SetThisChecked();
        }

        #endregion

        #region GUI

        private void DefaultColors()
        {
            _fore = Core.SystemColors.FontColor;
            _brdr = Core.SystemColors.BorderColor;
            _bkgTop = Core.SystemColors.ControlTop;
            _bkgBtm = Core.SystemColors.ControlBottom;
        }

        protected override void OnRender(int x, int y, int w, int h)
        {
            x = Left;
            y = Top;

            int cSz = _chkH + 6;
            int cY = y + (Height / 2 - cSz / 2);
            int iSz = cSz;
            int i14 = (int)(cSz * 0.6);
            while (iSz > 4 && iSz > i14)
                iSz -= 2;
            int iDf = (cSz - iSz) / 2;

            // Draw Outline
            Core.Screen.DrawRectangle((Parent.ActiveChild == this) ? Core.SystemColors.SelectionColor : _brdr, 1, x, cY, cSz, cSz, 1, 1, 0, 0, 0, 0, 0, 0, 256);

            if (Core.FlatRadios)
            {
                // Draw Fill
                if (Enabled)
                    Core.Screen.DrawRectangle(0, 0, x, cY, cSz, cSz, 0, 0, _bkgBtm, x, cY, _bkgBtm, x, cY + cSz, 256);
                else
                    Core.Screen.DrawRectangle(0, 0, x, cY, cSz, cSz, 0, 0, _bkgTop, x, cY, _bkgTop, x, cY + cSz, 256);

                // Draw Fill
                if (_checked)
                    Core.Screen.DrawRectangle(0, 0, x + iDf + 1, cY + iDf + 1, iSz - 2, iSz - 2, 0, 0, Core.SystemColors.SelectionColor, x, cY + iDf, Core.SystemColors.SelectionColor, x, cY + iDf + (iSz / 2), 256);
                else
                    Core.Screen.DrawRectangle(0, 0, x + iDf + 1, cY + iDf + 1, iSz - 2, iSz - 2, 0, 0, _bkgTop, x, cY + iDf, _bkgTop, x, cY + iDf + iSz, 256);

                if (Focused)
                    Core.ShadowRegionInset(x, cY, cSz, cSz, Core.SystemColors.SelectionColor);
            }
            else
            {
                // Draw Fill
                if (Enabled)
                    Core.Screen.DrawRectangle(0, 0, x + 1, cY + 1, cSz - 2, cSz - 2, 0, 0, _bkgTop, x, cY, _bkgBtm, x, cY + cSz, 256);
                else
                    Core.Screen.DrawRectangle(0, 0, x + 1, cY + 1, cSz - 2, cSz - 2, 0, 0, _bkgTop, x, cY, _bkgTop, x, cY + cSz, 256);

                // Draw Inner Outline
                Core.Screen.DrawRectangle((_checked) ? Core.SystemColors.SelectionColor : _brdr, 1, x + iDf, cY + iDf, iSz, iSz, 1, 1, 0, 0, 0, 0, 0, 0, 256);

                // Draw Fill
                if (_checked)
                    Core.Screen.DrawRectangle(0, 0, x + iDf + 1, cY + iDf + 1, iSz - 2, iSz - 2, 0, 0, Core.SystemColors.AltSelectionColor, x, cY + iDf, Core.SystemColors.SelectionColor, x, cY + iDf + (iSz / 2), 256);
                else
                    Core.Screen.DrawRectangle(0, 0, x + iDf + 1, cY + iDf + 1, iSz - 2, iSz - 2, 0, 0, _bkgBtm, x, cY + iDf, _bkgTop, x, cY + iDf + iSz, 256);

                if (Focused)
                    Core.ShadowRegionInset(x + 1, cY + 1, cSz - 2, cSz - 2, Core.SystemColors.SelectionColor);
            }

            // Draw Text
            Core.Screen.DrawText(_text, _font, _fore, x + cSz + 6, y + (h / 2 - _font.Height / 2));

        }

        #endregion

        #region Private Methods

        private void SetThisChecked()
        {
            if (Parent == null)
                return;

            RadioButton rdo;

            Parent.Suspended = true;
            for (int i = 0; i < Parent.Children.Length; i++)
            {
                if (Parent.Children[i] is RadioButton && Parent.Children[i] != this)
                {
                    rdo = (RadioButton)Parent.Children[i];
                    if (rdo._group == _group)
                    {
                        rdo._checked = false;
                        OnCheckChanged(rdo, false);
                    }
                }
            }
            _checked = true;
            OnCheckChanged(this, true);
            Parent.Suspended = false;
        }

        #endregion

    }
}
