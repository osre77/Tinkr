using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    internal class VKB : MarshalByRefObject, IContainer
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

        // SPECIFIC

        private Font _font;
        private string _text;
        private KeyboardLayout _layout;
        private string _title;
        private char _pwd = ' ';                // Password character ' ' for none

        private rect _keyRR;
        private rect _lastKeyRR;
        private bool _fullRendered;

        // Rendering Queue
        private bool _renderRequested;
        private bool _canRender;
        internal bool _continue;
        private bool _rendering;

        // Textbox
        private rect rTxt;
        private int _caret;
        private int _caretX, _caretY;           // Caret's screen poistion
        private bool _txtDown;
        private int _iLine;                     // Caret's current line
        private int _col;                       // Caret's position on current line
        protected internal int _showChar = -1;  // 
        private int _scrollX;
        private Thread thShow;                  // Thread for hidding a typed character w/ a pwd

        // Virtual Keys
        private Bitmap _keyBuffer;
        private int _vkX, _vkY;
        private int btnH, btnW;
        private Btn[] _btns;
        private byte _shft;
        private byte _scrn;
        private char _lastc;
        private int _iSel;
        private int _iRender;

        // CONTROL COMMON

        // Name & Tag
        private string _name;
        private object _tag;

        // Size & Location
        private int _x, _y;
        private int _offsetX, _offsetY;
        private int _w, _h;

        // Owner
        private IContainer _parent;

        // States
        private bool _enabled = true;
        private bool _visible = true;
        private bool _suspended;

        // Touch
        private bool _mDown;            // Touching when true
        private point _ptTapHold;       // Location where touch down occurred
        private Thread _thHold;         // Tap & Hold thread
        private long _lStop;            // Stop waiting for hold after this tick
        private long _lastTap;          // Tick count of last time occurrance

        // Dispose
        private bool _disposing;

        // CONTAINER COMMON

        // Children
        private IControl[] _children;
        private IControl _active;


        #endregion

        #region Constructors

        public VKB(Font Font, string DefaultValue, char PasswordChar, KeyboardLayout Layout, string Title)
        {
            _font = Font;
            _text = DefaultValue;
            _layout = Layout;
            _title = Title;
            _pwd = PasswordChar;

            if (_title == null)
                _title = string.Empty;
            if (_text == null)
                _text = string.Empty;

            _w = Core.ScreenWidth;
            _h = Core.ScreenHeight;

            int y = 4;
            if (_title != string.Empty)
                y += _font.Height + 4;

            // Textbox Definitions
            rTxt = new rect(4, y, Width - 8, _font.Height + 7);
            y += rTxt.Height + 4;
            CurrentCharacter = _text.Length;

            // Virtual Keys
            _vkX = 4;
            _vkY = y;
            _keyBuffer = new Bitmap(Width - 8, Height - y - 4);
            btnH = (_keyBuffer.Height - 28) / 5;
            btnW = (_keyBuffer.Width - 44) / 10;
            _iSel = -1;
            SetLayout();
            _keyRR = new rect(0, 0, _keyBuffer.Width, _keyBuffer.Height);

            // Render Queue
            _continue = true;
            new Thread(RenderWatch).Start();
        }

        #endregion

        #region Events

        public event OnVirtualKeysDone VirtualKeysDone;
        protected virtual void OnVirtualKeysDone(object sender)
        {
            if (VirtualKeysDone != null)
                VirtualKeysDone(sender);
        }

        public event OnGotFocus GotFocus;
        protected virtual void OnGotFocus(object sender)
        {
            if (GotFocus != null)
                GotFocus(sender);
        }

        public event OnLostFocus LostFocus;
        protected virtual void OnLostFocus(object sender)
        {
            if (LostFocus != null)
                LostFocus(sender);
        }

        public event OnButtonPressed ButtonPressed;
        protected virtual void OnButtonPressed(object sender, int buttonID)
        {
            if (ButtonPressed != null)
                ButtonPressed(sender, buttonID);
        }

        public event OnButtonReleased ButtonReleased;
        protected virtual void OnButtonReleased(object sender, int buttonID)
        {
            if (ButtonReleased != null)
                ButtonReleased(sender, buttonID);
        }

        /// <summary>
        /// Raised on a double-tap
        /// </summary>
        public event OnDoubleTap DoubleTap;
        protected virtual void OnDoubleTap(object sender, point e)
        {
            if (DoubleTap != null)
                DoubleTap(sender, e);
        }

        public event OnKeyboardAltKey KeyboardAltKey;
        protected virtual void OnKeyboardAltKey(object sender, int key, bool pressed)
        {
            if (KeyboardAltKey != null)
                KeyboardAltKey(sender, key, pressed);
        }

        public event OnKeyboardKey KeyboardKey;
        protected virtual void OnKeyboardKey(object sender, char key, bool pressed)
        {
            if (KeyboardKey != null)
                KeyboardKey(sender, key, pressed);
        }

        /// <summary>
        /// Raised on tap
        /// </summary>
        public event OnTap Tap;
        public virtual void OnTap(object sender, point e)
        {
            if (Tap != null)
                Tap(sender, e);
        }

        /// <summary>
        /// Raised on tap and hold
        /// </summary>
        public event OnTapHold TapHold;
        public virtual void OnTapHold(object sender, point e)
        {
            if (TapHold != null)
                TapHold(sender, e);
        }

        /// <summary>
        /// Raised on touch down
        /// </summary>
        public event OnTouchDown TouchDown;
        public virtual void OnTouchDown(object sender, point e)
        {
            if (TouchDown != null)
                TouchDown(sender, e);
        }

        /// <summary>
        /// Raised on touch gesture
        /// </summary>
        public event OnTouchGesture TouchGesture;
        public virtual void OnTouchGesture(object sender, TouchType e, float force)
        {
            if (TouchGesture != null)
                TouchGesture(sender, e, force);
        }

        /// <summary>
        /// Raised on touch move
        /// </summary>
        public event OnTouchMove TouchMove;
        public virtual void OnTouchMove(object sender, point e)
        {
            if (TouchMove != null)
                TouchMove(sender, e);
        }

        /// <summary>
        /// Raised on touch up
        /// </summary>
        public event OnTouchUp TouchUp;
        public virtual void OnTouchUp(object sender, point e)
        {
            if (TouchUp != null)
                TouchUp(sender, e);
        }

        #endregion

        #region Properties

        public bool CanRender
        {
            get { return true; }
        }

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
                UpdateCaretPos();
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
            }
        }


        public virtual IControl ActiveChild
        {
            get { return _active; }
            set
            {
                if (_active == value)
                    return;

                IControl tmp = _active;
                _active = null;

                if (tmp != null)
                    tmp.Blur();

                _active = value;
                if (_active != null)
                    _active.Activate();
            }
        }

        public IControl[] Children
        {
            get { return _children; }
        }

        public virtual IContainer TopLevelContainer
        {
            get { return Parent.TopLevelContainer; }
        }



        public virtual bool CanFocus
        {
            get { return true; }
        }

        public bool Disposing
        {
            get { return _disposing; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                Render(true);
            }
        }

        public bool Focused
        {
            get
            {
                if (_parent == null)
                    return false;
                return _parent.ActiveChild == this;
            }
        }

        public virtual int Height
        {
            get { return _h; }
            set
            {
                if (_h == value)
                    return;

                // Create Update Region
                rect r = new rect(_x, _y, _w, System.Math.Max(_h, value));

                _h = value;
                if (_parent != null)
                    _parent.Render(r, true);
            }
        }

        public point LastTouch
        {
            get { return _ptTapHold; }
            protected set { _ptTapHold = value; }
        }

        public int Left
        {
            get { return _x + _offsetX; }
        }

        public string Name
        {
            get { return _name; }
            protected set { _name = value; }
        }

        public virtual IContainer Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value)
                    return;

                if (_parent != null)
                    _parent.RemoveChild(this);

                _parent = value;
                UpdateOffsets();
            }
        }

        public rect ScreenBounds
        {
            get { return new rect(Left, Top, Width, Height); }
        }

        public virtual bool Suspended
        {
            get { return _suspended; }
            set
            {
                if (_suspended == value)
                    return;
                _suspended = value;
                if (!_suspended)
                    Invalidate();
            }
        }

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public int Top
        {
            get { return _y + _offsetY; }
        }

        public virtual bool Touching
        {
            get { return _mDown; }
        }

        public virtual bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible == value)
                    return;
                _visible = value;
                if (_parent != null)
                    _parent.Render(new rect(Left, Top, Width, Height), true);
            }
        }

        public virtual int Width
        {
            get { return _w; }
            set
            {
                if (_w == value)
                    return;

                // Create Update Region
                rect r = new rect(_x, _y, System.Math.Max(_w, value), _h);

                _w = value;
                if (_parent != null)
                    _parent.Render(r, true);
            }
        }

        public virtual int X
        {
            get { return _x; }
            set
            {
                if (_x == value)
                    return;

                // Create update region
                rect r = new rect(System.Math.Min(_x, value), _y, System.Math.Abs(_x - value) + Width, Height);

                _x = value;
                if (_parent != null)
                    _parent.Render(r, true);
            }
        }

        public virtual int Y
        {
            get { return _y; }
            set
            {
                if (_y == value)
                    return;

                // Create update region
                rect r = new rect(_x, System.Math.Min(_y, value), Width, System.Math.Abs(_y - value) + Height);

                _y = value;
                if (_parent != null)
                    _parent.Render(r, true);
            }
        }

        #endregion

        #region Buttons

        protected virtual void ButtonPressedMessage(int buttonID, ref bool handled) { }

        protected virtual void ButtonReleasedMessage(int buttonID, ref bool handled) { }

        public void SendButtonEvent(int buttonID, bool pressed)
        {
            bool handled = false;
            if (pressed)
            {
                ButtonPressedMessage(buttonID, ref handled);
                if (handled)
                    return;

                OnButtonPressed(this, buttonID);
                if (buttonID == (int)ButtonIDs.Click || buttonID == (int)ButtonIDs.Select)
                    SendTouchDown(this, Core.MousePosition);
            }
            else
            {
                ButtonReleasedMessage(buttonID, ref handled);
                if (handled)
                    return;

                if (buttonID == (int)ButtonIDs.Click || buttonID == (int)ButtonIDs.Select)
                    SendTouchUp(this, Core.MousePosition);
                OnButtonReleased(this, buttonID);
            }
        }

        #endregion

        #region Keyboard

        public void SendKeyboardAltKeyEvent(int key, bool pressed)
        {
          
        }

        public void SendKeyboardKeyEvent(char key, bool pressed)
        {
          
        }

        #endregion

        #region Touch

        /// <summary>
        /// Called when a touch down occurs and the control is active
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Location of touch</param>
        public void SendTouchDown(object sender, point e)
        {
            // Exit if needed
            if (!_enabled || !_visible || _suspended)
                return;

            // Allow Override
            int i;

            if (e.Y >= _vkY)
            {
                e.X -= _vkX;
                e.Y -= _vkY;

                // Check buttons
                for (i = 0; i < _btns.Length; i++)
                {
                    if (_btns[i].rect.Contains(e))
                    {
                        _iSel = i;
                        _iRender = i;
                        _btns[i].mDown = true;
                        _btns[i].Pressed = true;
                        RenderButton(_iSel);
                        return;
                    }
                }
            }

            if (rTxt.Contains(e))
                _txtDown = true;

            _iSel = -1;


            // Set down state
            _mDown = true;

            // Raise Event
            OnTouchDown(sender, e);
        }

        /// <summary>
        /// Called when a touch up occurs and the control is active
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Location of touch</param>
        public void SendTouchUp(object sender, point e)
        {
            if (!_enabled || !_visible || _suspended)
            {
                _mDown = false;
                return;
            }

            // Allow Override
            if (_txtDown || rTxt.Contains(e))
                TextUp(e);

            if (_iSel != -1)
                ButtonsUp(e);


            // Perform normal tap
            if (_mDown)
            {
                if (new rect(Left, Top, Width, Height).Contains(e))
                {
                    if (DateTime.Now.Ticks - _lastTap < (TimeSpan.TicksPerMillisecond * 500))
                    {
                        OnDoubleTap(this, new point(e.X - Left, e.Y - Top));
                        _lastTap = 0;
                    }
                    else
                    {
                        OnTap(this, new point(e.X - Left, e.Y - Top));
                        _lastTap = DateTime.Now.Ticks;
                    }
                }
                _mDown = false;
                OnTouchUp(this, e);
            }
        }

        /// <summary>
        /// Called when a touch move occurs and the control is active
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Location of touch</param>
        public void SendTouchMove(object sender, point e)
        {
            if (!_enabled || !_visible || _suspended)
                return;

            // Allow Override


            OnTouchMove(this, e);
        }

        /// <summary>
        /// Called when a gesture occurs and the control is active
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Type of touch gesture</param>
        public void SendTouchGesture(object sender, TouchType e, float force)
        {
            if (!_enabled || !_visible || _suspended)
                return;

            // Allow Override


            OnTouchGesture(this, e, force);
        }

        private void ButtonsUp(point e)
        {
            _iSel = -1;
            _btns[_iRender].mDown = false;
            _btns[_iRender].Pressed = false;

            e.X -= _vkX;
            e.Y -= _vkY;

            // Check buttons
            if (_btns[_iRender].rect.Contains(e) && _btns[_iRender].Enabled)
            {
                switch (_btns[_iRender].Type)
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
                        Backspace();
                        break;
                    case BtnType.Character:
                        if (_shft == 0)
                            InsertStrChr(_btns[_iRender].Text.ToLower());
                        else
                            InsertStrChr(_btns[_iRender].Text);

                        if (_shft == 1)
                            Unlock();
                        break;
                    case BtnType.Clear:
                        _lastc = 'x';
                        if (_shft == 0)
                            Shift();
                        _text = string.Empty;
                        _caret = -1;
                        _caretX = -1;
                        _caretY = -1;
                        _scrollX = 0;
                        _renderRequested = true;
                        break;
                    case BtnType.Done:
                        OnVirtualKeysDone(this);
                        break;
                    case BtnType.Left:
                        CurrentCharacter -= 1;
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
                        SensitiveShift(_btns[_iRender].Text);
                        Return();
                        break;
                    case BtnType.Right:
                        CurrentCharacter += 1;
                        _lastc = 'x';
                        break;
                    case BtnType.Shift:
                        _btns[_iRender].Image = Resources.GetBitmap(Resources.BitmapResources.shift2);
                        _btns[_iRender].Type = BtnType.ShiftLock;
                        _shft = 1;
                        RenderButton(_iRender);
                        return;
                    case BtnType.ShiftLock:
                        _btns[_iRender].Image = Resources.GetBitmap(Resources.BitmapResources.shift3);
                        _btns[_iRender].Type = BtnType.ShiftUnlock;
                        _shft = 2;
                        RenderButton(_iRender);
                        break;
                    case BtnType.ShiftUnlock:
                        _btns[_iRender].Image = Resources.GetBitmap(Resources.BitmapResources.shift1);
                        _btns[_iRender].Type = BtnType.Shift;
                        _shft = 0;
                        RenderButton(_iRender);
                        return;
                }
            }

            RenderButton(_iRender);
        }

        private void TextUp(point e)
        {
            _txtDown = false;
        }

        #endregion

        #region Disposing

        public void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;

            // Remove Events
            DoubleTap = null;
            Tap = null;
            TapHold = null;
            TouchDown = null;
            TouchGesture = null;
            TouchMove = null;
            TouchUp = null;
        }

        #endregion

        #region Focus

        public virtual void Activate()
        {
            Invalidate();
        }

        public virtual void Blur()
        {
            Invalidate();
        }

        public virtual bool HitTest(point e)
        {
            return ScreenBounds.Contains(e);
        }

        #endregion

        #region GUI

        public void Invalidate()
        {
            if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible || _suspended)
                return;

            if (_parent == null)
                Render(true);
            else
                _parent.Render(this.ScreenBounds, true);
        }


        public void Invalidate(rect area)
        {
            if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible || _suspended)
                return;

            if (_parent == null)
                Render(true);
            else
                _parent.TopLevelContainer.Render(area, true);
        }

        public void QuiteUnsuspend()
        {
            _suspended = false;
        }

        public void Render(bool flush = false)
        {
            if (!_canRender)
            {
                _renderRequested = true;
                return;
            }

            while (_rendering)
                ;

            _rendering = true;
            Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, Colors.LightGray, 0, 0, Colors.LightGray, 0, 0, 256);

            // Title
            if (_title != string.Empty)
                Core.Screen.DrawText(_title, _font, 0, 4, 4);

            // Virtual Keys
            if (_lastKeyRR.Width != 0)
            {
                Core.Screen.DrawImage(_vkX + _lastKeyRR.X, _vkY + _lastKeyRR.Y, _keyBuffer, _lastKeyRR.X, _lastKeyRR.Y, _lastKeyRR.Width, _lastKeyRR.Height);
                if (_fullRendered)
                    Core.SafeFlush(_vkX + _lastKeyRR.X, _vkY + _lastKeyRR.Y, _lastKeyRR.Width, _lastKeyRR.Height);
            }
            _lastKeyRR = _keyRR;

            if (_keyRR.Width != 0)
            {
                Core.Screen.DrawImage(_vkX + _keyRR.X, _vkY + _keyRR.Y, _keyBuffer, _keyRR.X, _keyRR.Y, _keyRR.Width, _keyRR.Height);
                if (_fullRendered)
                    Core.SafeFlush(_vkX + _keyRR.X, _vkY + _keyRR.Y, _keyRR.Width, _keyRR.Height);
            }

            // Textbox
            Core.Screen.DrawRectangle(0, 1, rTxt.X, rTxt.Y, rTxt.Width, rTxt.Height, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 256);
            Core.Screen.SetClippingRectangle(rTxt.X + 1, rTxt.Y + 1, rTxt.Width - 2, rTxt.Height - 2);
            Core.Screen.DrawTextInRect(pwdString(), rTxt.X + 3 - _scrollX, rTxt.Y + 3, rTxt.Width - 6 + _scrollX, _font.Height, Bitmap.DT_AlignmentLeft, 0, _font);
            Core.Screen.DrawLine(Core.SystemColors.SelectionColor, 1, rTxt.X + _caretX - _scrollX + 3, rTxt.Y + 3, rTxt.X + _caretX - _scrollX + 3, rTxt.Y + _font.Height);
            if (_fullRendered)
                Core.SafeFlush(rTxt.X, rTxt.Y, rTxt.Width, rTxt.Height);

            _rendering = false;
            if (!_fullRendered)
            {
                Core.SafeFlush();
                _fullRendered = true;
            }
        }

        public void Render(rect region, bool flush = false)
        {
            Render(flush);
        }

        private void RenderWatch()
        {
            while (_continue)
            {
                if (_renderRequested)
                {
                    _renderRequested = false;
                    while (_rendering)
                        ;
                    _canRender = true;
                    Render(true);
                    _canRender = false;
                    while (_rendering)
                        ;
                }
                Thread.Sleep(100);
            }
        }

        public void UpdateOffsets()
        {
            if (_parent != null)
            {
                _offsetX = _parent.Left;
                _offsetY = _parent.Top;
            }
            else
            {
                _offsetX = 0;
                _offsetY = 0;
            }
        }

        public void UpdateOffsets(point pt)
        {
            _offsetX = pt.X;
            _offsetY = pt.Y;
        }

        #endregion

        #region Public Container Methods

        /// <summary>
        /// Adds a AddChild
        /// </summary>
        /// <param name="Control">Control to add</param>
        public virtual void AddChild(IControl child)
        {
           
        }

        public virtual void BringControlToFront(IControl child)
        {
            
        }

        /// <summary>
        /// Removes all controls
        /// </summary>
        public virtual void ClearChildren(bool DisposeChildren = true)
        {
            
        }

        /// <summary>
        /// Returns a control by index
        /// </summary>
        /// <param name="index">Index of control</param>
        /// <returns>Control at index</returns>
        public virtual IControl GetChildByIndex(int index)
        {
            return null;
        }

        /// <summary>
        /// Returns a control by name
        /// </summary>
        /// <param name="Name">Name of control</param>
        /// <returns>Retrieved control</returns>
        public virtual IControl GetChildByName(string name)
        {
            return null;
        }

        public virtual int GetChildIndex(IControl child)
        {
            return -1;
        }

        /// <summary>
        /// Removes a specific control
        /// </summary>
        /// <param name="Control">Control to remove</param>
        public virtual void RemoveChild(IControl child)
        {
           
        }

        /// <summary>
        /// Removes a control by index
        /// </summary>
        /// <param name="Index">Index of control</param>
        public virtual void RemoveChildAt(int index)
        {
           
        }

        public virtual void NextChild()
        {
        }

        public virtual void PreviousChild()
        {
        }

        #endregion

        #region VirtualKeys

        private void AddFirstBtns()
        {
            int x = 0;
            _btns[0] = new Btn(BtnType.Left, new rect(x, 0, btnW, btnH), Resources.GetBitmap(Resources.BitmapResources.back));
            _btns[0].Enabled = (_caret > 0);
            x += btnW + 4;
            _btns[1] = new Btn(BtnType.Right, new rect(x, 0, btnW, btnH), Resources.GetBitmap(Resources.BitmapResources.next));
            _btns[1].Enabled = (_caret < _text.Length);
            x += btnW + 4;
            _btns[2] = new Btn(BtnType.Clear, new rect(x, 0, btnW, btnH), Resources.GetBitmap(Resources.BitmapResources.clear));
            _btns[2].Enabled = (_text.Length > 0);

            int w = FontManager.ComputeExtentEx(_font, "Done").Width + 17;
            _btns[3] = new Btn(BtnType.Done, new rect(_keyBuffer.Width - w + 3, 0, w, btnH), "Done");
        }

        private void CreateButtons(string[] Row1, string[] Row2, string[] Row3)
        {
            int y, x, w, i;
            int b = 4;

            // Row 1
            y = btnH + 6;
            x = (_keyBuffer.Width / 2 - (((Row1.Length * btnW) + (Row1.Length * 4)) / 2));
            for (i = 0; i < Row1.Length; i++)
            {
                _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row1[i]);
                x += btnW + 4;
            }

            // Row 2
            y += btnH + 6;
            x = (_keyBuffer.Width / 2 - (((Row2.Length * btnW) + (Row2.Length * 4)) / 2));
            for (i = 0; i < Row2.Length; i++)
            {
                _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row2[i]);
                x += btnW + 4;
            }

            // Row 3 (with specials)
            y += btnH + 6;
            x = (_keyBuffer.Width / 2 - (((Row3.Length * btnW) + (Row3.Length * 4)) / 2));
            w = x - 4;

            switch (_scrn)
            {
                case 0:
                    _btns[b++] = new Btn(BtnType.Shift, new rect(0, y, w, btnH), Resources.GetBitmap(Resources.BitmapResources.shift1));
                    break;
                case 1:
                    _btns[b++] = new Btn(BtnType.Alt, new rect(0, y, w, btnH), "#+=");
                    break;
                case 2:
                    _btns[b++] = new Btn(BtnType.Num, new rect(0, y, w, btnH), "?123");
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
            i = _keyBuffer.Width - w - w - btnW - btnW - 20;
            x = 0;

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
                    _renderRequested = true;
                }
            }

            b = _caret < _text.Length;
            if (_btns[1].Enabled != b)
            {
                _btns[1].Enabled = b;
                if (Parent != null)
                {
                    _iSel = 1;
                    _renderRequested = true;
                }
            }

            b = _text.Length != 0;
            if (_btns[2].Enabled != b)
            {
                _btns[2].Enabled = b;
                if (Parent != null)
                {
                    _iSel = 2;
                    _renderRequested = true;
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
                    x = (_keyBuffer.Width / 2 - (((Row1.Length * btnW) + (Row1.Length * 4)) / 2));
                    for (i = 0; i < Row1.Length; i++)
                    {
                        _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row1[i]);
                        x += btnW + 4;
                    }

                    // Row 2
                    y += btnH + 6;
                    x = (_keyBuffer.Width / 2 - (((Row2.Length * btnW) + (Row2.Length * 4)) / 2));
                    for (i = 0; i < Row2.Length; i++)
                    {
                        _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row2[i]);
                        x += btnW + 4;
                    }

                    // Row 3
                    y += btnH + 6;
                    x = (_keyBuffer.Width / 2 - (((Row3.Length * btnW) + (Row3.Length * 4)) / 2));
                    w = x - 4;
                    for (i = 0; i < Row3.Length; i++)
                    {
                        _btns[b++] = new Btn(BtnType.Character, new rect(x, y, btnW, btnH), Row3[i]);
                        x += btnW + 4;
                    }

                    // Row 3
                    y += btnH + 6;
                    x = (_keyBuffer.Width / 2 - (((Row4.Length * btnW) + (Row4.Length * 4)) / 2));
                    w = x - 4;
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
            RenderButtons();
        }

        private void SetLayoutAlt()
        {
            _shft = 0;
            _btns = new Btn[37];
            AddFirstBtns();
            _scrn = 2;
            CreateButtons(new string[] { "~", "`", "|", "·", "µ", "≠", "{", "}" }, new string[] { "√", "$", "Ω", "°", "^", "_", "=", "[", "]" }, new string[] { "™", "®", "©", "¶", "\\", "<", ">" });
            RenderButtons();
        }

        private void SetLayoutNumeric()
        {
            _shft = 0;
            _btns = new Btn[37];
            AddFirstBtns();
            _scrn = 1;
            CreateButtons(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" }, new string[] { "-", "/", ":", ";", "(", ")", "$", "&", "@" }, new string[] { "\"", ",", "?", "!", "'", "#", "%" });
            RenderButtons();
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
                        _btns[i].Type = BtnType.Shift;
                        RenderButton(i, true);
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
                        _btns[i].Type = BtnType.Shift;
                        RenderButton(i, true);
                        return;
                }
            }
        }

        private void RenderButtons()
        {
            while (_rendering)
                ;
            lock (_keyBuffer)
            {
                Color _btm, _top, _fore;

                _keyBuffer.DrawRectangle(0, 0, 0, 0, _keyBuffer.Width, _keyBuffer.Height, 0, 0, Colors.LightGray, 0, 0, Colors.LightGray, 0, 0, 256);

                int i;
                int w, h;
                for (i = 0; i < _btns.Length; i++)
                {
                    if (_btns[i].Type != BtnType.Character)
                    {
                        _btm = ColorUtility.ColorFromRGB(123, 131, 143);
                        _top = ColorUtility.ColorFromRGB(156, 162, 175);
                        _fore = Colors.White;
                    }
                    else
                    {
                        _btm = Colors.LightGray;
                        _top = Colors.White;
                        _fore = 0;
                    }

                    if (_btns[i].rect.Width != 0)
                    {
                        // Draw Background
                        if (_btns[i].Pressed)
                            _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[i].rect.X, _btns[i].rect.Y, _btns[i].rect.Width - 1, _btns[i].rect.Height - 1, 0, 0, _btm, 0, 0, _btm, 0, 0, 256);
                        else
                            _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[i].rect.X, _btns[i].rect.Y, _btns[i].rect.Width - 1, _btns[i].rect.Height - 1, 0, 0, _top, 0, 0, _top, 0, 0, 256);

                        // Draw Image
                        if (_btns[i].Image != null)
                            _keyBuffer.DrawImage(_btns[i].rect.X + (_btns[i].rect.Width / 2 - _btns[i].Image.Width / 2), _btns[i].rect.Y + (_btns[i].rect.Height / 2 - _btns[i].Image.Height / 2), _btns[i].Image, 0, 0, _btns[i].Image.Width, _btns[i].Image.Height, (_btns[i].Enabled) ? (ushort)256 : (ushort)128);

                        // Vertically Center Text
                        if (_btns[i].Text != null)
                        {
                            _font.ComputeTextInRect(_btns[i].Text, out w, out h, _btns[i].rect.Width - 6);
                            _keyBuffer.DrawTextInRect(_btns[i].Text, _btns[i].rect.X + 4, _btns[i].rect.Y + (_btns[i].rect.Height / 2 - h / 2) - 1, _btns[i].rect.Width - 8, _font.Height, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingNone, (_btns[i].Enabled) ? _fore : Colors.DarkGray, _font);
                        }
                    }
                }
            }
            _keyRR = new rect(0, 0, _keyBuffer.Width, _keyBuffer.Height);
            _renderRequested = true;
        }

        private void RenderButton(int buttonIndex, bool force = false)
        {
            int w, h;
            Color _btm, _top, _fore;

            while (_rendering)
                ;

            _rendering = true;
            lock (_keyBuffer)
            {
                if (_btns[buttonIndex].Type != BtnType.Character)
                {
                    _btm = ColorUtility.ColorFromRGB(123, 131, 143);
                    _top = ColorUtility.ColorFromRGB(156, 162, 175);
                    _fore = Colors.White;
                }
                else
                {
                    _btm = Colors.LightGray;
                    _top = Colors.White;
                    _fore = 0;
                }

                if (_btns[buttonIndex].rect.Width != 0)
                {
                    // Draw Background
                    if (_btns[buttonIndex].Pressed)
                        _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[buttonIndex].rect.X, _btns[buttonIndex].rect.Y, _btns[buttonIndex].rect.Width - 1, _btns[buttonIndex].rect.Height - 1, 0, 0, _btm, 0, 0, _btm, 0, 0, 256);
                    else
                        _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[buttonIndex].rect.X, _btns[buttonIndex].rect.Y, _btns[buttonIndex].rect.Width - 1, _btns[buttonIndex].rect.Height - 1, 0, 0, _top, 0, 0, _top, 0, 0, 256);

                    // Draw Image
                    if (_btns[buttonIndex].Image != null)
                        _keyBuffer.DrawImage(_btns[buttonIndex].rect.X + (_btns[buttonIndex].rect.Width / 2 - _btns[buttonIndex].Image.Width / 2), _btns[buttonIndex].rect.Y + (_btns[buttonIndex].rect.Height / 2 - _btns[buttonIndex].Image.Height / 2), _btns[buttonIndex].Image, 0, 0, _btns[buttonIndex].Image.Width, _btns[buttonIndex].Image.Height, (_btns[buttonIndex].Enabled) ? (ushort)256 : (ushort)128);

                    // Vertically Center Text
                    if (_btns[buttonIndex].Text != null)
                    {
                        _font.ComputeTextInRect(_btns[buttonIndex].Text, out w, out h, _btns[buttonIndex].rect.Width - 6);
                        _keyBuffer.DrawTextInRect(_btns[buttonIndex].Text, _btns[buttonIndex].rect.X + 4, _btns[buttonIndex].rect.Y + (_btns[buttonIndex].rect.Height / 2 - h / 2) - 1, _btns[buttonIndex].rect.Width - 8, _font.Height, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingNone, (_btns[buttonIndex].Enabled) ? _fore : Colors.DarkGray, _font);
                    }
                }
            }

            _keyRR = _btns[buttonIndex].rect;
            _renderRequested = true;
            _rendering = false;

            if (force)
            {
                Core.Screen.DrawImage(_vkX + _keyRR.X, _vkY + _keyRR.Y, _keyBuffer, _keyRR.X, _keyRR.Y, _keyRR.Width, _keyRR.Height);
                Core.SafeFlush(_vkX + _keyRR.X, _vkY + _keyRR.Y, _keyRR.Width, _keyRR.Height);
            }
        }

        #endregion

        #region Textbox

        private void HideCharacters()
        {
            Thread.Sleep(750);
            _showChar = -1;

            int w = FontManager.ComputeExtentEx(_font, _text.Substring(_caret - 1, 1)).Width;
            int sw = FontManager.ComputeExtentEx(_font, new string(new char[] { _pwd })).Width;

            //_caretX -= w - sw;

            Invalidate();
        }

        public void InsertStrChr(string chr)
        {
            int w, h;

            _font.ComputeExtent(chr, out w, out h);

            if (_caret == -1)
                CurrentCharacter = 0;

            if (_pwd != ' ')
            {
                thShow = null;
                _showChar = _caret;
                thShow = new Thread(HideCharacters);
                thShow.Start();
            }

            if (_caret == 0)
                _text = chr + _text;
            else
                _text = _text.Substring(0, _caret) + chr + _text.Substring(_caret);

            _caret += 1;
            _caretX += w;

            if (_caretX > Width)
                _scrollX = _caretX - Width + 20;

            EnableButtons();
            _renderRequested = true;
        }

        private void UpdateCaretPos()
        {
            if (_caret == -1)
                _caretX = 0;
            else
                _caretX = FontManager.ComputeExtentEx(_font, _text.Substring(0, _caret)).Width;
            _renderRequested = true;
        }

        private string pwdString()
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

        protected internal void Return()
        {
            if (_caret == -1)
            {
                SendTouchDown(this, new point(5, 5));
                SendTouchUp(this, new point(5, 5));
            }

            if (_caret == 0)
                _text = "\n" + _text;
            else
                _text = _text.Substring(0, _caret) + "\n" + _text.Substring(_caret);
            _caret += 1;
            _iLine += 1;

            _caretX = Left + 5;
            _caretY += _font.Height;

            _renderRequested = true;
        }

        protected internal void Backspace()
        {
            if (_caret == 0)
                return;

            int w, h;
            string chr = pwdString().Substring(_caret - 1, 1);
            _font.ComputeExtent(chr, out w, out h);

            _text = _text.Substring(0, _caret - 1) + _text.Substring(_caret--);
            _caretX -= w;
            _renderRequested = true;
        }

        #endregion

    }
}
