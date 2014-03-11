using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
    [Serializable]
    public class Window : Container
    {

        #region Variables

        private string _title;
        private WindowBorderStyle _border;
        private Font _font;
        private rect _titlebar;
        private bool _drag;
        private point _ptDown;
        private int _x, _y;
        private Panel pnl;

        // Sizing
        private int _dW, _dH;
        private int _dX, _dY;
        private bool _canMax, _canMin, _canClose;
        private rect _rMax, _rMin, _rClose;
        private bool _bMax, _bMin, _bClose;
        private bool _maxed;

        // Colors
        private Color cRed = ColorUtility.ColorFromRGB(246, 83, 20);
        private Color cGreen = ColorUtility.ColorFromRGB(124, 187, 0);
        private Color cBlue = ColorUtility.ColorFromRGB(211, 155, 0);

        #endregion

        #region Constructors

        public Window(string name, string title, Font font, int x, int y, int width, int height)
        {
            Name = name;
            _title = title;
            _font = font;
            _border = WindowBorderStyle.Sizeable;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            pnl = new Panel("pnl", x, y, width, height);
            pnl.BackColor = Colors.Wheat;
            ActiveChild = pnl;

            _canClose = true;
            _canMax = true;
            _canMin = true;
        }

        #endregion

        #region Events

        public event OnResized Resized;
        protected virtual void OnResized(object sender, rect bounds)
        {
            if (Resized != null)
                Resized(sender, bounds);
        }

        public event OnWindowClosed WindowClosed;
        protected virtual void OnWindowClosed(object sender)
        {
            if (WindowClosed != null)
                WindowClosed(sender);
        }

        public event OnWindowClosed WindowMinimized;
        protected virtual void OnWindowMinimized(object sender)
        {
            if (WindowMinimized != null)
                WindowMinimized(sender);
        }

        #endregion

        #region Properties

        public bool AllowClose
        {
            get { return _canClose; }
            set
            {
                if (_canClose == value)
                    return;
                _canClose = value;
                Invalidate();
            }
        }

        public bool AllowMaximize
        {
            get { return _canMax; }
            set
            {
                if (_canMax == value)
                    return;
                _canMax = value;
                Invalidate();
            }
        }

        public bool AllowMinimize
        {
            get { return _canMin; }
            set
            {
                if (_canMin == value)
                    return;
                _canMin = value;
                Invalidate();
            }
        }

        public int AvailableHeight
        {
            get { return Height - 17 - _font.Height; }
        }

        public int AvailableWidth
        {
            get
            {
                return Width - 10;
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

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                    return;
                _title = value;
                Invalidate();
            }
        }

        public override int X
        {
            get { return _x; }
            set
            {
                if (_x == value)
                    return;

                // Create update region
                rect r = new rect(System.Math.Min(_x, value), _y, System.Math.Abs(_x - value) + Width, Height);

                _x = value;
                if (Parent != null)
                    Parent.Render(r, true);
            }
        }

        public override int Y
        {
            get { return _y; }
            set
            {
                if (_y == value)
                    return;

                // Create update region
                rect r = new rect(_x, System.Math.Min(_y, value), Width, System.Math.Abs(_y - value) + Height);

                _y = value;
                if (Parent != null)
                    Parent.Render(r, true);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a AddChild
        /// </summary>
        /// <param name="Control">Control to add</param>
        public override void AddChild(IControl child)
        {
            pnl.AddChild(child);
        }

        public override void BringControlToFront(IControl child)
        {
            pnl.BringControlToFront(child);
        }

        /// <summary>
        /// Removes all controls
        /// </summary>
        public override void ClearChildren(bool DisposeChildren = true)
        {
            pnl.ClearChildren();
        }

        /// <summary>
        /// Returns a control by index
        /// </summary>
        /// <param name="index">Index of control</param>
        /// <returns>Control at index</returns>
        public override IControl GetChildByIndex(int index)
        {
            return pnl.GetChildByIndex(index);
        }

        /// <summary>
        /// Returns a control by name
        /// </summary>
        /// <param name="Name">Name of control</param>
        /// <returns>Retrieved control</returns>
        public override IControl GetChildByName(string name)
        {
            return pnl.GetChildByName(name);
        }

        public override int GetChildIndex(IControl child)
        {
            return GetChildIndex(child);
        }

        /// <summary>
        /// Removes a specific control
        /// </summary>
        /// <param name="Control">Control to remove</param>
        public override void RemoveChild(IControl child)
        {
            pnl.RemoveChild(child);
        }

        /// <summary>
        /// Removes a control by index
        /// </summary>
        /// <param name="Index">Index of control</param>
        public override void RemoveChildAt(int index)
        {
            pnl.RemoveChildAt(index);
        }

        #endregion

        #region Keyboard Methods

        protected override void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled)
        {
            if (ActiveChild != null)
            {
                handled = true;
                ActiveChild.SendKeyboardAltKeyEvent(key, pressed);
            }
        }

        protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
        {
            if (ActiveChild != null)
            {
                handled = true;
                ActiveChild.SendKeyboardKeyEvent(key, pressed);
            }
        }

        #endregion

        #region Touch

        protected override void TouchDownMessage(object sender, point e, ref bool handled)
        {
            if (_titlebar.Contains(e))
            {
                if (_rClose.Contains(e))
                {
                    _bClose = true;
                    return;
                }

                if (_rMin.Contains(e))
                {
                    _bMin = true;
                    return;
                }

                if (_rMax.Contains(e))
                {
                    _bMax = true;
                    return;
                }

                _ptDown = e;
                _drag = true;
                return;
            }

            pnl.SendTouchDown(this, e);
        }

        protected override void TouchMoveMessage(object sender, point e, ref bool handled)
        {
            if (_drag)
            {
                int cX = _x + (e.X - _ptDown.X);
                int cY = _y + (e.Y - _ptDown.Y);

                // Create update region
                rect r = new rect(System.Math.Min(_x, cX), System.Math.Min(_y, cY), System.Math.Abs(_x + cX) + Width, System.Math.Abs(_y + cY) + Height);

                _x = cX;
                _y = cY;
                //_titlebar = new rect(cX + 4, cY + 4, Width - 8, _font.Height + 8);
                _ptDown = e;

                Parent.Render(r, true);
                return;
            }
            else
                pnl.SendTouchMove(sender, e);
        }

        protected override void TouchUpMessage(object sender, point e, ref bool handled)
        {
            if (_drag)
            {
                _drag = false;
                Invalidate();
                return;
            }

            if (_bClose)
            {
                OnWindowClosed(this);
                Parent.RemoveChild(this);
                this.Dispose();
                IContainer _parent = this.Parent;
                _bClose = false;
                return;
            }

            if (_bMax)
            {
                if (_rMax.Contains(e))
                {
                    if (_maxed)
                    {
                        Parent.Suspended = true;

                        X = _dX;
                        Y = _dY;
                        Width = _dW;
                        Height = _dH;
                        _maxed = false;

                        OnResized(this, new rect(X, Y, Width, Height));
                        Parent.Suspended = false;
                    }
                    else
                    {
                        _dW = Width;
                        _dH = Height;
                        _dX = X;
                        _dY = Y;

                        Parent.Suspended = true;

                        X = 0;
                        Y = 0;
                        Width = Core.ScreenWidth;
                        Height = Core.ScreenHeight;
                        _maxed = true;

                        OnResized(this, new rect(X, Y, Width, Height));
                        Parent.Suspended = false;
                    }

                }

                _bMax = false;
                return;
            }

            if (_bMin)
            {
                this.Visible = false;
                OnWindowMinimized(this);
                _bMin = false;
                return;
            }
            
            pnl.SendTouchUp(sender, e);
        }

        #endregion

        #region Focus

        public override void Activate()
        {
            Parent.BringControlToFront(this);
            base.Activate();
        }

        public override void Blur()
        {
            Invalidate();
            base.Blur();
        }

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int width, int height)
        {
            switch (_border)
            {
                case WindowBorderStyle.Fixed:
                    RenderFixed(x, y, width, height);
                    break;
                case WindowBorderStyle.FixedSingle:
                    RenderFixedSingle(x, y, width, height);
                    break;
                default:
                    RenderSizeable(x, y, width, height);
                    break;
            }

            pnl.UpdateOffsets();
            pnl.Render();
        }

        private void RenderFixed(int x, int y, int width, int height)
        {
        }

        private void RenderFixedSingle(int x, int y, int width, int height)
        {
        }

        private void RenderSizeable(int x, int y, int width, int height)
        {
            Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, x, y, width, height, 0, 0, 
                Core.SystemColors.ControlTop, x, y, Core.SystemColors.ControlBottom, x, y + height, 256);
            Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, x + 4, y + 4, width - 8, height - 8, 0, 0,
                Core.SystemColors.WindowColor, 0, 0, Core.SystemColors.WindowColor, 0, 0, 256);


            if (Focused)
            {
                Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, x + 4, y + 4, width - 8, _font.Height + 8, 0, 0,
                    Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);
                Core.Screen.DrawTextInRect(_title, x + 8, y + 8, width - 16, _font.Height, Bitmap.DT_TrimmingWordEllipsis, Core.SystemColors.SelectedFontColor, _font);
            }
            else
            {
                Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, x + 4, y + 4, width - 8, _font.Height + 8, 0, 0,
                    Core.SystemColors.ControlTop, 0, 0, Core.SystemColors.ControlTop, 0, 0, 256);
                Core.Screen.DrawTextInRect(_title, x + 8, y + 8, width - 16, _font.Height, Bitmap.DT_TrimmingWordEllipsis, Core.SystemColors.FontColor, _font);
            }

            _titlebar = new rect(Left + 4, Top + 4, Width - 8, _font.Height + 8);
            pnl.Parent = null;
            pnl.X = 5;
            pnl.Y = 12 + _font.Height;
            pnl.Width = width - 10;
            pnl.Height = height - 17 - _font.Height;
            pnl.Parent = this;

            if (!_canClose && !_canMax && !_canMin)
                return;

            x = Left + Width - 20;
            y = Top + 8;

            if (_canClose)
            {
                Core.Screen.DrawRectangle(cRed, 1, x, y, 12, 12, 0, 0, Colors.White, x, y, cRed, x, y + 4, 256);
                _rClose = new rect(x, y, 12, 12);
                x -= 16;
            }

            if (_canMax)
            {
                Core.Screen.DrawRectangle(cGreen, 1, x, y, 12, 12, 0, 0, Colors.White, x, y, cGreen, x, y + 4, 256);
                _rMax = new rect(x, y, 12, 12);
                x -= 16;
            }

            if (_canMin)
            {
                Core.Screen.DrawRectangle(cBlue, 1, x, y, 12, 12, 0, 0, Colors.White, x, y, cBlue, x, y + 4, 256);
                _rMin = new rect(x, y, 12, 12);
                x -= 16;
            }
        }

        #endregion

    }
}
