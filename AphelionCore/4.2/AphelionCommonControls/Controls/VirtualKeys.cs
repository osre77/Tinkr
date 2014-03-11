using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class VirtualKeys : Control
    {

        #region Enumerations

        private enum BtnType
        {
            Character = 0,
            Left = 1,
            Right = 2,
            Clear = 3,
            Done = 4,
            Shift = 5,
            ShiftLock = 6,
            ShiftUnlock = 7,
            ABC = 8,
            Num = 9,
            Alt = 10,
            Back = 11,
            Return = 12,
        }

        #endregion

        #region Structures

        private struct Btn
        {
            public BtnType Type;
            public bool Pressed;
            public bool mDown;
            public Bitmap Image;
            public rect rect;
            public string Text;
            public bool Enabled;
            public Btn(BtnType Type, rect rect)
            {
                this.Type = Type;
                this.rect = rect;
                this.Image = null;
                this.Text = null;
                this.Pressed = false;
                this.mDown = false;
                this.Enabled = true;
            }
            public Btn(BtnType Type, rect rect, Bitmap Image)
            {
                this.Type = Type;
                this.rect = rect;
                this.Image = Image;
                this.Text = null;
                this.Pressed = false;
                this.mDown = false;
                this.Enabled = true;
            }
            public Btn(BtnType Type, rect rect, string Text)
            {
                this.Type = Type;
                this.rect = rect;
                this.Image = null;
                this.Text = Text;
                this.Pressed = false;
                this.mDown = false;
                this.Enabled = true;
            }
        }

        #endregion

        #region Variables

        private Font _font;
        private KeyboardLayout _layout;
        private Color _bkg = Colors.LightGray;

        private int btnH, btnW;
        private Btn[] _btns;

        private byte _shft;
        private byte _scrn;
        private char _lastc;

        private Color _nBtm = Colors.LightGray;
        private Color _nTop = Colors.White;
        private Color _pBtm = ColorUtility.ColorFromRGB(123, 131, 143);
        private Color _pTop = ColorUtility.ColorFromRGB(156, 162, 175);

        private double lStart, lEnd;
        private int _iSel = -1;
        private bool bQuick;

        private int _caret, _length;

        #endregion

        #region Constructor

        public VirtualKeys(string name, int x, int y, int width, int height, Font font, KeyboardLayout layout = KeyboardLayout.QWERTY)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;
            _layout = layout;

            CalculatePositions();
            SetLayout();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Sets background color
        /// </summary>
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

        protected internal int Caret
        {
            set
            {
                _caret = value;
                EnableButtons();
            }
        }

        protected internal int Length
        {
            set
            {
                _length = value;
                EnableButtons();
            }
        }

        #endregion

        #region Events

        public event OnVirtualKeyBackspace VirtualKeyBackspace;
        protected virtual void OnVirtualKeyBackspace(object sender)
        {
            if (VirtualKeyBackspace != null)
                VirtualKeyBackspace(sender);
        }

        public event OnVirtualKeyCaretMove VirtualKeyCaretMove;
        protected virtual void OnVirtualKeyCaretMove(object sender, int amount)
        {
            if (VirtualKeyCaretMove != null)
                VirtualKeyCaretMove(sender, amount);
        }

        public event OnVirtualKeyClear VirtualKeyClear;
        protected virtual void OnVirtualKeyClear(object sender)
        {
            if (VirtualKeyClear != null)
                VirtualKeyClear(sender);
        }

        public event OnVirtualKeyDelete VirtualKeyDelete;
        protected virtual void OnVirtualKeyDelete(object sender)
        {
            if (VirtualKeyDelete != null)
                VirtualKeyDelete(sender);
        }

        public event OnVirtualKeyReturn VirtualKeyReturn;
        protected virtual void OnVirtualKeyReturn(object sender)
        {
            if (VirtualKeyReturn != null)
                VirtualKeyReturn(sender);
        }

        public event OnVirtualKeyTap VirtualKeyTap;
        protected virtual void OnVirtualKeyTap(object sender, string value)
        {
            if (VirtualKeyTap != null)
                VirtualKeyTap(sender, value);
        }

        public event OnVirtualKeysDone VirtualKeysDone;
        protected virtual void OnVirtualKeysDone(object sender)
        {
            if (VirtualKeysDone != null)
                VirtualKeysDone(sender);
        }

        #endregion

        #region Touch Invokes

        protected override void TouchDownMessage(object sender, point e, ref bool handled)
        {
            int i;

            e.X += Left;
            e.Y += Top;

            // Check buttons
            for (i = 0; i < _btns.Length; i++)
            {
                if (_btns[i].rect.Contains(e))
                {
                    _iSel = i;
                    _btns[i].mDown = true;
                    _btns[i].Pressed = true;
                    if (Parent != null)
                    {
                        bQuick = true;
                        Parent.Render(_btns[i].rect, true);
                    }
                    return;
                }
            }

            _iSel = -1;
        }

        protected override void TouchUpMessage(object sender, point e, ref bool handled)
        {
            int j;

            e.X += Left;
            e.Y += Top;

            if (_iSel == -1)
                return;

            _btns[_iSel].mDown = false;
            _btns[_iSel].Pressed = false;
            if (Parent != null)
            {
                bQuick = true;
                Parent.Render(_btns[_iSel].rect, true);
            }

            // Check buttons
            if (_btns[_iSel].rect.Contains(e) && _btns[_iSel].Enabled)
            {
                switch (_btns[_iSel].Type)
                {
                    case BtnType.ABC:
                        SetLayout();
                        Invalidate();
                        return;
                    case BtnType.Alt:
                        SetLayoutAlt();
                        return;
                    case BtnType.Back:
                        _lastc = 'x';
                        OnVirtualKeyBackspace(this);
                        break;
                    case BtnType.Character:
                        OnVirtualKeyTap(this, _btns[_iSel].Text);
                        if (_shft == 1)
                            Unlock();
                        break;
                    case BtnType.Clear:
                        _lastc = 'x';
                        if (_shft == 0)
                            Shift();
                        OnVirtualKeyClear(this);
                        break;
                    case BtnType.Done:
                        OnVirtualKeysDone(this);
                        break;
                    case BtnType.Left:
                        OnVirtualKeyCaretMove(this, -1);
                        _lastc = 'x';
                        break;
                    case BtnType.Num:
                        SetLayoutNumeric();
                        return;
                    case BtnType.Return:
                        if (_shft == 0)
                        {
                            Shift();
                            return;
                        }
                        SensitiveShift(_btns[_iSel].Text);
                        OnVirtualKeyReturn(this);
                        break;
                    case BtnType.Right:
                        OnVirtualKeyCaretMove(this, 1);
                        _lastc = 'x';
                        break;
                    case BtnType.Shift:
                        _btns[_iSel].Image = Resources.GetBitmap(Resources.BitmapResources.shift2);
                        _btns[_iSel].Type = BtnType.ShiftLock;
                        //for (j = 4; j < _btns.Length; j++)
                        //{
                        //    if (_btns[j].Type == BtnType.Character)
                        //        _btns[j].Text = _btns[j].Text.ToUpper();
                        //}
                        _shft = 1;
                        bQuick = true;
                        Parent.Render(_btns[_iSel].rect, true);
                        return;
                    case BtnType.ShiftLock:
                        _btns[_iSel].Image = Resources.GetBitmap(Resources.BitmapResources.shift3);
                        _btns[_iSel].Type = BtnType.ShiftUnlock;
                        if (Parent != null)
                        {
                            bQuick = true;
                            Parent.Render(_btns[_iSel].rect, true);
                        }
                        _shft = 2;
                        break;
                    case BtnType.ShiftUnlock:
                        _btns[_iSel].Image = Resources.GetBitmap(Resources.BitmapResources.shift1);
                        _btns[_iSel].Type = BtnType.Shift;
                        //for (j = 4; j < _btns.Length; j++)
                        //{
                        //    if (_btns[j].Type == BtnType.Character)
                        //        _btns[j].Text = _btns[j].Text.ToLower();
                        //}
                        _shft = 0;
                        bQuick = true;
                        Parent.Render(_btns[_iSel].rect, true);
                        return;
                }
            }
        }

        #endregion

        #region Private Methods

        private void AddFirstBtns()
        {
            int x = Left;
            _btns[0] = new Btn(BtnType.Left, new rect(Left + x, Top, btnW, btnH), Resources.GetBitmap(Resources.BitmapResources.back));
            _btns[0].Enabled = (_caret > 0);
            x += btnW + 4;
            _btns[1] = new Btn(BtnType.Right, new rect(Left + x, Top, btnW, btnH), Resources.GetBitmap(Resources.BitmapResources.next));
            _btns[1].Enabled = (_caret < _length);
            x += btnW + 4;
            _btns[2] = new Btn(BtnType.Clear, new rect(Left + x, Top, btnW, btnH), Resources.GetBitmap(Resources.BitmapResources.clear));
            _btns[2].Enabled = (_length > 0);

            int w = FontManager.ComputeExtentEx(_font, "Done").Width + 17;
            _btns[3] = new Btn(BtnType.Done, new rect(Left + Width - w + 3, Top + 0, w, btnH), "Done");
        }

        private void CalculatePositions()
        {
            btnH = (Height - 28) / 5;
            btnW = (Width - 44) / 10;
        }

        private void CreateButtons(string[] Row1, string[] Row2, string[] Row3)
        {
            int y, x, w, i;
            int b = 4;

            // Row 1
            y = Top + btnH + 6;
            x = Left + (Width / 2 - (((Row1.Length * btnW) + (Row1.Length * 4)) / 2));
            for (i = 0; i < Row1.Length; i++)
            {
                _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row1[i]);
                x += btnW + 4;
            }

            // Row 2
            y += btnH + 6;
            x = Left + (Width / 2 - (((Row2.Length * btnW) + (Row2.Length * 4)) / 2));
            for (i = 0; i < Row2.Length; i++)
            {
                _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row2[i]);
                x += btnW + 4;
            }

            // Row 3 (with specials)
            y += btnH + 6;
            x = Left + (Width / 2 - (((Row3.Length * btnW) + (Row3.Length * 4)) / 2));
            w = x - Left - 4;

            switch (_scrn)
            {
                case 0:
                    _btns[b++] = new Btn(BtnType.Shift, new rect(Left, y, w, btnH), Resources.GetBitmap(Resources.BitmapResources.shift1));
                    break;
                case 1:
                    _btns[b++] = new Btn(BtnType.Alt, new rect(Left, y, w, btnH), "#+=");
                    break;
                case 2:
                    _btns[b++] = new Btn(BtnType.Num, new rect(Left, y, w, btnH), "?123");
                    break;
            }

            for (i = 0; i < Row3.Length; i++)
            {
                _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row3[i]);
                x += btnW + 4;
            }
            _btns[b++] = new Btn(BtnType.Back, new rect(x, y, w, btnH), Resources.GetBitmap(Resources.BitmapResources.del));

            // Row 4
            y += btnH + 6;
            i = Width - w - w - btnW - btnW - 20;
            x = Left;

            switch (_scrn)
            {
                case 0:
                    _btns[b++] = new Btn(BtnType.Num, new rect(x, y, w, btnH), "?123");
                    break;
                default:
                    _btns[b++] = new Btn(BtnType.ABC, new rect(x, y, w, btnH), "ABC");
                    break;
            }
            x += w + 4;
            _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), "@");
            x += btnW + 4;
            _btns[b++] = new Btn(BtnType.Character, new rect(x, y, i, btnH), " ");
            x += i + 4;
            _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), ".");
            x += btnW + 4;
            _btns[b] = new Btn(BtnType.Character, new rect(x, y, w, btnH), ".com");
        }

        private void EnableButtons()
        {
            bool b = _caret > 0;
            if (_btns[0].Enabled != b)
            {
                _btns[0].Enabled = b;
                if (Parent != null)
                {
                    _iSel = 0;
                    bQuick = true;
                    Parent.Render(_btns[0].rect, true);
                }
            }

            b = _caret < _length;
            if (_btns[1].Enabled != b)
            {
                _btns[1].Enabled = b;
                if (Parent != null)
                {
                    _iSel = 1;
                    bQuick = true;
                    Parent.Render(_btns[1].rect, true);
                }
            }

            b = _length != 0;
            if (_btns[2].Enabled != b)
            {
                _btns[2].Enabled = b;
                if (Parent != null)
                {
                    _iSel = 2;
                    bQuick = true;
                    Parent.Render(_btns[2].rect, true);
                }
            }
        }

        private void SensitiveShift(string value)
        {
            if ((value == "\n" || (value == " " && (_lastc == '.' || _lastc == '!' || _lastc == '?'))) && _shft == 0 && _scrn == 0)
                Shift();
            else if (_shft == 1 && value != " ")
            {
                _shft = 2;
                Shift();
            }

        }

        private void SetLayout()
        {
            _scrn = 0;
            switch (_layout)
            {
                case KeyboardLayout.AZERTY:
                    _btns = new Btn[37];
                    CreateButtons(new string[] { "A", "Z", "E", "R", "T", "Y", "U", "I", "O", "P" }, new string[] { "Q", "S", "D", "F", "G", "H", "J", "K", "L", "M" }, new string[] { "W", "X", "C", "V", "B", "N" });
                    break;
                case KeyboardLayout.Numeric:
                    _btns = new Btn[16];

                    int y, x, w, i;
                    int b = 4;
                    string[] Row1 = new string[] { "7", "8", "9" };
                    string[] Row2 = new string[] { "4", "5", "6" };
                    string[] Row3 = new string[] { "1", "2", "3" };
                    string[] Row4 = new string[] { ",", "0", "." };

                    // Row 1
                    y = btnH + 6;
                    x = Left + (Width / 2 - (((Row1.Length * btnW) + (Row1.Length * 4)) / 2));
                    for (i = 0; i < Row1.Length; i++)
                    {
                        _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row1[i]);
                        x += btnW + 4;
                    }

                    // Row 2
                    y += btnH + 6;
                    x = Left + (Width / 2 - (((Row2.Length * btnW) + (Row2.Length * 4)) / 2));
                    for (i = 0; i < Row2.Length; i++)
                    {
                        _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row2[i]);
                        x += btnW + 4;
                    }

                    // Row 3
                    y += btnH + 6;
                    x = Left + (Width / 2 - (((Row3.Length * btnW) + (Row3.Length * 4)) / 2));
                    w = x - Left - 4;
                    for (i = 0; i < Row3.Length; i++)
                    {
                        _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row3[i]);
                        x += btnW + 4;
                    }

                    // Row 3
                    y += btnH + 6;
                    x = Left + (Width / 2 - (((Row4.Length * btnW) + (Row4.Length * 4)) / 2));
                    w = x - Left - 4;
                    for (i = 0; i < Row4.Length; i++)
                    {
                        _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row4[i]);
                        x += btnW + 4;
                    }


                    break;
                case KeyboardLayout.QWERTY:
                    _btns = new Btn[37];
                    CreateButtons(new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" }, new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" }, new string[] { "Z", "X", "C", "V", "B", "N", "M" });
                    break;
            }
            AddFirstBtns();
        }

        private void SetLayoutAlt()
        {
            _shft = 0;
            _btns = new Btn[37];
            AddFirstBtns();
            _scrn = 2;
            CreateButtons(new string[] { "~", "`", "|", "·", "µ", "≠", "{", "}" }, new string[] { "√", "$", "Ω", "°", "^", "_", "=", "[", "]" }, new string[] { "™", "®", "©", "¶", "\\", "<", ">" });
            Invalidate();
        }

        private void SetLayoutNumeric()
        {
            _shft = 0;
            _btns = new Btn[37];
            AddFirstBtns();
            _scrn = 1;
            CreateButtons(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" }, new string[] { "-", "/", ":", ";", "(", ")", "$", "&", "@" }, new string[] { "\"", ",", "?", "!", "'", "#", "%" });
            Invalidate();
        }

        private void Shift()
        {
            _shft = 1;
            for (int i = 4; i < _btns.Length; i++)
            {
                switch (_btns[i].Type)
                {
                    case BtnType.Shift:
                    case BtnType.ShiftLock:
                    case BtnType.ShiftUnlock:
                        _btns[i].Image = Resources.GetBitmap(Resources.BitmapResources.shift2);
                        _btns[i].Type = BtnType.ShiftLock;
                        Invalidate();
                        return;
                }
            }
        }

        private void Unlock()
        {
            _shft = 0;
            for (int i = 4; i < _btns.Length; i++)
            {
                switch (_btns[i].Type)
                {
                    case BtnType.Shift:
                    case BtnType.ShiftLock:
                    case BtnType.ShiftUnlock:
                        _btns[i].Image = Resources.GetBitmap(Resources.BitmapResources.shift1);
                        _btns[i].Type = BtnType.ShiftLock;
                        Invalidate();
                        return;
                }
            }
        }

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int w, int h)
        {
            if (bQuick)
                RenderSingleButton();
            else
            {
                Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);
                RenderAllButtons();
            }
        }

        private void RenderAllButtons()
        {
            Color _btm, _top, _fore;

            lock (Core.Screen)
            {
                int i;
                int w, h;
                for (i = 0; i < _btns.Length; i++)
                {
                    if (_btns[i].Type != BtnType.Character)
                    {
                        _btm = _pBtm;
                        _top = _pTop;
                        _fore = Colors.White;
                    }
                    else
                    {
                        _btm = _nBtm;
                        _top = _nTop;
                        _fore = 0;
                    }

                    if (_btns[i].rect.Width != 0)
                    {
                        // Draw Background
                        if (_btns[i].Pressed)
                            Core.Screen.DrawRectangle(Colors.DarkGray, 1, _btns[i].rect.X, _btns[i].rect.Y, _btns[i].rect.Width - 1, _btns[i].rect.Height - 1, 0, 0, _btm, 0, 0, _btm, 0, 0, 256);
                        else
                            Core.Screen.DrawRectangle(Colors.DarkGray, 1, _btns[i].rect.X, _btns[i].rect.Y, _btns[i].rect.Width - 1, _btns[i].rect.Height - 1, 0, 0, _top, 0, 0, _top, 0, 0, 256);

                        // Draw Image
                        if (_btns[i].Image != null)
                            Core.Screen.DrawImage(_btns[i].rect.X + (_btns[i].rect.Width / 2 - _btns[i].Image.Width / 2), _btns[i].rect.Y + (_btns[i].rect.Height / 2 - _btns[i].Image.Height / 2), _btns[i].Image, 0, 0, _btns[i].Image.Width, _btns[i].Image.Height, (_btns[i].Enabled) ? (ushort)256 : (ushort)128);

                        // Vertically Center Text
                        if (_btns[i].Text != null)
                        {
                            _font.ComputeTextInRect(_btns[i].Text, out w, out h, _btns[i].rect.Width - 6);
                            Core.Screen.DrawTextInRect(_btns[i].Text, _btns[i].rect.X + 4, _btns[i].rect.Y + (_btns[i].rect.Height / 2 - h / 2) - 1, _btns[i].rect.Width - 8, _font.Height, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingNone, (_btns[i].Enabled) ? _fore : Colors.DarkGray, _font);
                        }
                    }
                }
            }
        }

        private void RenderSingleButton()
        {

            Color _btm, _top, _fore;

            int w, h;
            if (_btns[_iSel].Type != BtnType.Character)
            {
                _btm = _pBtm;
                _top = _pTop;
                _fore = Colors.White;
            }
            else
            {
                _btm = _nBtm;
                _top = _nTop;
                _fore = 0;
            }

            if (_btns[_iSel].Pressed)
                Core.Screen.DrawRectangle(Colors.DarkGray, 1, _btns[_iSel].rect.X, _btns[_iSel].rect.Y, _btns[_iSel].rect.Width - 1, _btns[_iSel].rect.Height - 1, 0, 0, _btm, 0, 0, _btm, 0, 0, 256);
            else
                Core.Screen.DrawRectangle(Colors.DarkGray, 1, _btns[_iSel].rect.X, _btns[_iSel].rect.Y, _btns[_iSel].rect.Width - 1, _btns[_iSel].rect.Height - 1, 0, 0, _top, 0, 0, _top, 0, 0, 256);

            // Draw Image
            if (_btns[_iSel].Image != null)
                Core.Screen.DrawImage(_btns[_iSel].rect.X + (_btns[_iSel].rect.Width / 2 - _btns[_iSel].Image.Width / 2), _btns[_iSel].rect.Y + (_btns[_iSel].rect.Height / 2 - _btns[_iSel].Image.Height / 2), _btns[_iSel].Image, 0, 0, _btns[_iSel].Image.Width, _btns[_iSel].Image.Height, (_btns[_iSel].Enabled) ? (ushort)256 : (ushort)128);

            // Vertically Center Text
            if (_btns[_iSel].Text != null)
            {
                _font.ComputeTextInRect(_btns[_iSel].Text, out w, out h, _btns[_iSel].rect.Width - 6);
                Core.Screen.DrawTextInRect(_btns[_iSel].Text, _btns[_iSel].rect.X + 4, _btns[_iSel].rect.Y + (_btns[_iSel].rect.Height / 2 - h / 2) - 1, _btns[_iSel].rect.Width - 8, _font.Height, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingNone, (_btns[_iSel].Enabled) ? _fore : Colors.DarkGray, _font);
            }

            bQuick = false;
        }

        #endregion

    }
}
