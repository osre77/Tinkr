using System;
using System.Threading;

using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class ScrollableControl : MarshalByRefObject, IControl
    {

        #region Variables

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
        private TapState _eTapHold;     // Current tap state
        private long _lastTap;          // Tick count of last time occurrance

        // Dispose
        private bool _disposing;

        // Scrolling
        private int _rqWidth;
        private int _rqHeight;
        private int _scrX, _scrY;
        private bool _needsX, _needsY;
        private int _sqW, _sqH;
        private bool _showScroll;
        private bool _cancelHide;
        private bool _bMoving;
        private int _horzW, _horzSize, _horzX;
        private int _vertH, _vertSize, _vertY;

        #endregion

        #region Events

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

        public event OnGotFocus GotFocus;
        protected virtual void OnGotFocus(object sender)
        {
            if (GotFocus != null)
                GotFocus(sender);
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

        public event OnLostFocus LostFocus;
        protected virtual void OnLostFocus(object sender)
        {
            if (LostFocus != null)
                LostFocus(sender);
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
                rect r = new rect(Left, Top, Width, System.Math.Max(_h, value));

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
            get { return X + _offsetX; }
        }

        public int MaxScrollX
        {
            get { return _rqWidth; }
        }

        public int MaxScrollY
        {
            get { return _rqHeight; }
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

        protected int RequiredHeight
        {
            get { return _rqHeight; }
            set
            {
                if (_rqHeight == value)
                    return;
                _rqHeight = value;
                UpdateNeeds();
            }
        }

        protected int RequiredWidth
        {
            get { return _rqWidth; }
            set
            {
                if (_rqWidth == value)
                    return;
                _rqWidth = value;
                UpdateNeeds();
            }
        }

        public rect ScreenBounds
        {
            get { return new rect(Left, Top, Width, Height); }
        }

        public bool Scrolling
        {
            get { return _bMoving; }
        }

        public virtual int ScrollX
        {
            get { return _scrX; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > _sqW)
                    value = _sqW;

                if (!_needsX || _scrX == value)
                    return;

                _scrX = value;
                Invalidate();
            }
        }

        public virtual int ScrollY
        {
            get { return _scrY; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > _sqH)
                    value = _sqH;

                if (!_needsY || _scrY == value)
                    return;

                _scrY = value;
                Invalidate();
            }
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
            get { return Y + _offsetY; }
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
                rect r = new rect(Left, Top, System.Math.Max(_w, value), Height);

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
                rect r = new rect(System.Math.Min(_x, value), Top, System.Math.Abs(_x - value) + Width, Height);

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
                rect r = new rect(Left, System.Math.Min(_y, value), Width, System.Math.Abs(_y - value) + Height);

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

        protected virtual void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled) { }

        protected virtual void KeyboardKeyMessage(char key, bool pressed, ref bool handled) { }

        public void SendKeyboardAltKeyEvent(int key, bool pressed)
        {
            bool handled = false;
            KeyboardAltKeyMessage(key, pressed, ref handled);
            if (!handled)
                OnKeyboardAltKey(this, key, pressed);
        }

        public void SendKeyboardKeyEvent(char key, bool pressed)
        {
            bool handled = false;
            KeyboardKeyMessage(key, pressed, ref handled);
            if (!handled)
                OnKeyboardKey(this, key, pressed);
        }

        #endregion

        #region Touch

        protected virtual void TouchDownMessage(object sender, point e, ref bool handled)
        {
        }

        protected virtual void TouchGestureMessage(object sender, TouchType e, float force, ref bool handled)
        {
        }

        protected virtual void TouchMoveMessage(object sender, point e, ref bool handled)
        {
        }

        protected virtual void TouchUpMessage(object sender, point e, ref bool handled)
        {
        }

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

            bool bHandled = false;

            // Allow Override
            TouchDownMessage(sender, e, ref bHandled);

            // Exit if handled
            if (bHandled)
                return;

            // Set down state
            _mDown = true;

            // Begin Tap/Hold
            _ptTapHold = e;
            _eTapHold = TapState.TapHoldWaiting;
            _lStop = DateTime.Now.Ticks + (500 * TimeSpan.TicksPerMillisecond);
            if (_thHold == null || !_thHold.IsAlive)
            {
                _thHold = new Thread(TapHoldWaiter);
                _thHold.Start();
            }

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

            bool bHandled = false;

            // Check Tap Hold
            _eTapHold = TapState.Normal;

            // Handle Moving
            if (_bMoving)
            {
                _bMoving = false;
                _showScroll = false;
                _cancelHide = true;
                Invalidate();
            }
            else
                TouchUpMessage(sender, e, ref bHandled);    // Allow Override

            // Exit if handled
            if (bHandled)
                return;

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

            bool bHandled = false;

            // Handle Scrolling
            if (_needsX || _needsY)
            {
                bool doInvalidate = false; 
                _bMoving = true;
                _cancelHide = true;
                _showScroll = true;

                int dest = _scrY - (e.Y - _ptTapHold.Y);
                if (dest < 0)
                    dest = 0;
                else if (dest > _rqHeight - Height)
                    dest = _rqHeight - Height;
                if (_scrY != dest && dest > 0)
                {
                    _scrY = dest;
                    doInvalidate = true;
                }

                dest = _scrX - (e.X - LastTouch.X);
                if (dest < 0)
                    dest = 0;
                else if (dest > _rqWidth - Width)
                    dest = _rqWidth - Width;
                if (_scrX != dest && dest > 0)
                {
                    _scrX = dest;
                    doInvalidate = true;
                }

                _ptTapHold = e;
                if (doInvalidate)
                    UpdateValues();
            }

            // Allow Override
            TouchMoveMessage(sender, e, ref bHandled);

            // Exit if handled
            if (bHandled)
                return;

            _eTapHold = TapState.Normal;
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

            bool bHandled = false;

            // Allow Override
            TouchGestureMessage(sender, e, force, ref bHandled);

            // Exit if handled
            if (bHandled)
                return;

            OnTouchGesture(this, e, force);
        }

        /// <summary>
        /// Method for detecting Tap & Hold
        /// </summary>
        private void TapHoldWaiter()
        {
            while (DateTime.Now.Ticks < _lStop)
            {
                Thread.Sleep(10);
                if (_eTapHold == TapState.Normal)
                    return;
            }
            if (_eTapHold == TapState.Normal || !_enabled || !_visible || _suspended)
                return;

            //_mDown = false;
            _eTapHold = TapState.Normal;

            OnTapHold(this, _ptTapHold);
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
                _parent.TopLevelContainer.Render(this.ScreenBounds, true);
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

        public void Render(bool flush = false)
        {
            // Check if we actually need to render
            if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible || _suspended)
                return;

            int cw, ch;

            if (_parent == null)
            {
                cw = Width;
                ch = Height;
            }
            else
            {
                cw = System.Math.Min(_parent.Width - X, Width);
                ch = System.Math.Min(_parent.Height - Y, Height);
            }

            if (cw < 1 || ch < 1)
                return;

            // Update clip
            Core.Screen.SetClippingRectangle(Left, Top, cw, ch);

            // Render control
            lock (Core.Screen)
                OnRender(Left, Top, cw, ch);

            // Render scrollbar(s)
            if (_showScroll)
                RenderScrollbar();

            // Flush as needed
            if (flush)
                Core.SafeFlush(Left, Top, cw, ch);
        }

        protected virtual void OnRender(int x, int y, int w, int h)
        {
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

        #region Scrollbar Methods

        private void AutoHideScrollbar()
        {
            Thread.Sleep(1000);
            if (_cancelHide || !_showScroll)
                return;
            _showScroll = false;
            Invalidate();
        }

        private void RenderScrollbar()
        {

            if (_needsX)
            {
                int y = Top + Height - 4;
                Core.Screen.DrawRectangle(0, 0, Left, y, _horzW, 4, 0, 0, Core.SystemColors.ScrollbarBackground, 0, 0, Core.SystemColors.ScrollbarBackground, 0, 0, 128);
                Core.Screen.DrawRectangle(0, 0, Left + _horzX, y, _horzSize, 4, 0, 0, Core.SystemColors.ScrollbarGrip, 0, 0, Core.SystemColors.ScrollbarGrip, 0, 0, 128);
            }

            if (_needsY)
            {
                int x = Left + Width - 4;
                Core.Screen.DrawRectangle(0, 0, x, Top, 4, _vertH, 0, 0, Core.SystemColors.ScrollbarBackground, 0, 0, Core.SystemColors.ScrollbarBackground, 0, 0, 128);
                Core.Screen.DrawRectangle(0, 0, x, Top + _vertY, 4, _vertSize, 0, 0, Core.SystemColors.ScrollbarGrip, 0, 0, Core.SystemColors.ScrollbarGrip, 0, 0, 128);
            }
        }

        protected void UpdateScrollbar(bool invalidate = true)
        {
            UpdateNeeds(invalidate);
        }

        private void UpdateNeeds(bool invalidate = true)
        {
            bool nX = false;
            bool nY = false;

            if (_rqWidth > Width)
                nX = true;
            if (_rqHeight > Height)
                nY = true;

            if (nX || nY)
            {
                //_scrX = 0;
                //_scrY = 0;
                _needsX = nX;
                _needsY = nY;
                _sqW = _rqWidth - Width;
                _sqH = _rqHeight - Height;
                _showScroll = true;
                UpdateValues(invalidate);

                _cancelHide = false;
                new Thread(AutoHideScrollbar).Start();
            }

        }

        private void UpdateValues(bool invalidate = true)
        {
            int w = Width;
            int h = Height;

            if (_needsX && _needsY)
            {
                w -= 4;
                h -= 4;
            }

            if (_needsX)
            {
                _horzW = w;
                _horzSize = _horzW - (_sqW / 4);
                if (_horzSize < 8)
                    _horzSize = 8;
                _horzX = (int)((_horzW - _horzSize) * ((float)_scrX / (float)_sqW));
            }

            if (_needsY)
            {
                _vertH = h;
                _vertSize = _vertH - (_sqH / 4);
                if (_vertSize < 8)
                    _vertSize = 8;
                _vertY = (int)((_vertH - _vertSize) * ((float)_scrY / (float)_sqH));
            }

            if (invalidate)
                Invalidate();
        }

        #endregion

    }
}
