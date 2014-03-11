using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class Panel : Container
    {

        #region Variables

        // Background
        private Color _bkg;
        private Bitmap _img;
        private ScaleMode _scale;
        private bool _trans;
        private bool _border;

        // Auto Scroll
        private bool _autoScroll;
        private int _maxX, _maxY;
        private int _minX, _minY;
        private bool _moving;
        private bool _touch;

        #endregion

        #region Constructors

        public Panel(string name, int x, int y, int width, int height)
        {
            this.Name = name;
            _bkg = Core.SystemColors.ContainerBackground;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Panel(string name, int x, int y, int width, int height, Color backColor)
        {
            this.Name = name;
            _bkg = backColor;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Form will automatically display scrollbars as needed when true
        /// </summary>
        public bool AutoScroll
        {
            get { return _autoScroll; }
            set
            {
                if (_autoScroll == value)
                    return;
                _autoScroll = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets/Sets background color
        /// </summary>
        public Color BackColor
        {
            get { return _bkg; }
            set
            {
                if (value == _bkg)
                    return;

                _bkg = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets/Sets background image
        /// </summary>
        public Bitmap BackgroundImage
        {
            get { return _img; }
            set
            {
                if (_img == value)
                    return;

                _img = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets/Sets scale mode for background image
        /// </summary>
        public ScaleMode BackgroundImageScaleMode
        {
            get { return _scale; }
            set
            {
                if (_scale == value)
                    return;

                _scale = value;

                if (_img != null)
                    Invalidate();
            }
        }

        public bool DrawBorder
        {
            get { return _border; }
            set
            {
                if (_border == value)
                    return;
                _border = value;
                Invalidate();
            }
        }

        public bool TransparentBackground
        {
            get { return _trans; }
            set
            {
                if (_trans == value)
                    return;
                _trans = value;
                Invalidate();
            }
        }

        public override bool Touching
        {
            get
            {
                if (base.Touching)
                    return true;

                if (Children != null)
                {
                    for (int i = 0; i < Children.Length; i++)
                    {
                        if (Children[i].Touching)
                            return true;
                    }
                }

                return false;
            }
        }

        public override int X
        {
            get { return base.X; }
            set
            {
                if (base.X == value)
                    return;
                base.X = value;
                if (Children != null)
                {
                    for (int i = 0; i < Children.Length; i++)
                        Children[i].UpdateOffsets();
                }
            }
        }

        public override int Y
        {
            get { return base.Y; }
            set
            {
                if (base.Y == value)
                    return;
                base.Y = value;
                if (Children != null)
                {
                    for (int i = 0; i < Children.Length; i++)
                        Children[i].UpdateOffsets();
                }
            }
        }

        #endregion

        #region Button Methods

        protected override void ButtonPressedMessage(int buttonID, ref bool handled)
        {
            if (Children != null)
            {
                if (ActiveChild != null)
                    ActiveChild.SendButtonEvent(buttonID, true);
                else
                {
                    for (int i = 0; i < Children.Length; i++)
                    {
                        if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                        {
                            handled = true;
                            Children[i].SendButtonEvent(buttonID, true);
                            break;
                        }
                    }
                }
            }
        }

        protected override void ButtonReleasedMessage(int buttonID, ref bool handled)
        {
            if (Children != null)
            {
                if (ActiveChild != null)
                    ActiveChild.SendButtonEvent(buttonID, false);
                else
                {
                    for (int i = 0; i < Children.Length; i++)
                    {
                        if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                        {
                            handled = true;
                            Children[i].SendButtonEvent(buttonID, false);
                            break;
                        }
                    }
                }
            }
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

        #region Touch Methods

        protected override void TouchDownMessage(object sender, point e, ref bool handled)
        {
            int i;

            // Check Controls
            if (Children != null)
            {
                lock (Children)
                {
                    for (i = Children.Length - 1; i >= 0; i--)
                    {
                        if (Children[i].Visible && Children[i].HitTest(e))
                        {
                            if (ActiveChild != Children[i])
                                ActiveChild = Children[i];
                            Children[i].SendTouchDown(this, e);
                            handled = true;
                            return;
                        }
                    }
                }
            }

            if (ActiveChild != null)
            {
                ActiveChild.Blur();
                Render(ActiveChild.ScreenBounds, true);
                ActiveChild = null;
            }

            _touch = true;
        }

        protected override void TouchGestureMessage(object sender, TouchType e, float force, ref bool handled)
        {
            if (ActiveChild != null)
            {
                handled = true;
                ActiveChild.SendTouchGesture(sender, e, force);
            }
        }

        protected override void TouchMoveMessage(object sender, point e, ref bool handled)
        {
            if (_touch)
            {
                bool bUpdated = false;

                int diffY = 0;
                int diffX = 0;

                // Scroll Y
                if (_maxY > Height)
                {
                    diffY = e.Y - LastTouch.Y;

                    if (diffY > 0 && _minY < Top)
                    {
                        diffY = System.Math.Min(diffY, _minY + Top);
                        bUpdated = true;
                    }
                    else if (diffY < 0 && _maxY > Height)
                    {
                        diffY = System.Math.Min(-diffY, _maxY - _minY - Height);
                        bUpdated = true;
                    }
                    else
                        diffY = 0;

                    _moving = true;
                }

                // Scroll X
                if (_maxX > Width)
                {
                    diffX = e.X - LastTouch.X;

                    if (diffX > 0 && _minX < Left)
                    {
                        diffX = System.Math.Min(diffX, _minX + Left);
                        bUpdated = true;
                    }
                    else if (diffX < 0 && _maxX > Width)
                    {
                        diffX = System.Math.Min(-diffX, _maxX - _minX - Width);
                        bUpdated = true;
                    }
                    else
                        diffX = 0;

                    _moving = true;
                    handled = true;
                }

                LastTouch = e;
                if (bUpdated)
                {
                    point ptOff = new point(diffX, diffY);
                    for (int i = 0; i < Children.Length; i++)
                        Children[i].UpdateOffsets(ptOff);
                    Render(true);
                    handled = true;
                }
                else if (_moving)
                    Render(true);
            }
            else
            {
                // Check Controls
                if (ActiveChild != null && ActiveChild.Touching)
                {
                    ActiveChild.SendTouchMove(this, e);
                    handled = true;
                    return;
                }
                else if (Children != null)
                {
                    for (int i = Children.Length - 1; i >= 0; i--)
                    {
                        if (Children[i].Touching || Children[i].HitTest(e))
                            Children[i].SendTouchMove(this, e);
                    }
                }
            }
        }

        protected override void TouchUpMessage(object sender, point e, ref bool handled)
        {
            _touch = false;

            if (_moving)
            {
                _moving = false;
                Render(true);
                return;
            }
            else
            {
                // Check Controls
                if (Children != null)
                {
                    for (int i = Children.Length - 1; i >= 0; i--)
                    {
                        try
                        {
                            if (Children[i].HitTest(e) && !handled)
                            {
                                handled = true;
                                Children[i].SendTouchUp(this, e);
                            }
                            else if (Children[i].Touching)
                                Children[i].SendTouchUp(this, e);
                        }
                        catch (Exception)
                        {
                            // This can happen if the user clears the Form during a tap
                        }
                    }
                }
            }
        }

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int width, int height)
        {
            rect area = new rect(x, y, width, height);

            if (!_trans)
                DrawBackground();

            _maxX = Width;
            _maxY = Height;

            // Render controls
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    if (Children[i] != null)
                    {
                        if (Children[i].ScreenBounds.Intersects(area))
                            Children[i].Render();

                        if (Children[i].Y < _minY)
                            _minY = Children[i].Y;
                        else if (Children[i].Y + Children[i].Height > _maxY)
                            _maxY = Children[i].Y + Children[i].Height;

                        if (Children[i].X < _minX)
                            _minX = Children[i].X;
                        else if (Children[i].X + Children[i].Width > _maxX)
                            _maxX = Children[i].X + Children[i].Width;

                    }
                }
            }

            if (_border)
                Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, Left, Top, Width, Height, 0, 0, 0, 0, 0, 0, 0, 0, 256);
        }

        #endregion

        #region Private Methods

        private void DrawBackground()
        {
            Core.Screen.DrawRectangle(_bkg, 0, Left, Top, Width, Height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);

            if (_img != null)
            {
                switch (_scale)
                {
                    case ScaleMode.Center:
                        Core.Screen.DrawImage(Left + (Core.ScreenWidth / 2 - _img.Width / 2), Top + (Core.ScreenHeight / 2 - _img.Height / 2), _img, 0, 0, _img.Width, _img.Height);
                        break;
                    case ScaleMode.Normal:
                        Core.Screen.DrawImage(Left, Top, _img, 0, 0, _img.Width, _img.Height);
                        break;
                    case ScaleMode.Scale:
                        float multiplier;

                        if (_img.Height > _img.Width)
                        {
                            // Portrait
                            if (Height > Width)
                                multiplier = (float)Width / (float)_img.Width;
                            else
                                multiplier = (float)Height / (float)_img.Height;
                        }
                        else
                        {
                            // Landscape
                            if (Height > Width)
                                multiplier = (float)Width / (float)_img.Width;
                            else
                                multiplier = (float)Height / (float)_img.Height;
                        }

                        int dsW = (int)((float)_img.Width * multiplier);
                        int dsH = (int)((float)_img.Height * multiplier);
                        int dX = (int)((float)Width / 2 - (float)dsW / 2);
                        int dY = (int)((float)Height / 2 - (float)dsH / 2);

                        Core.Screen.StretchImage(Left + dX, Top + dY, _img, dsW, dsH, 256);
                        break;
                    case ScaleMode.Stretch:
                        Core.Screen.StretchImage(Left, Top, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                        break;
                    case ScaleMode.Tile:
                        Core.Screen.TileImage(Left, Top, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                        break;
                }

            }
        }

        #endregion

    }
}
