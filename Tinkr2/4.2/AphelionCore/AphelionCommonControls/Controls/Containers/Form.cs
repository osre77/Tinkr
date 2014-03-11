using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class Form : Container
    {

        #region Variables

        // Background
        private Color _bkg;
        private Bitmap _img;
        private ScaleMode _scale;

        // Auto Scroll
        private bool _autoScroll;
        private int _maxX, _maxY;
        private int _minX, _minY;
        private bool _moving;

        // Size
        private int _w, _h;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new Form
        /// </summary>
        /// <param name="name">Name of the Form</param>
        public Form(string name)
        {
            this.Name = name;
            _bkg = Core.SystemColors.ContainerBackground;
            _w = Core.ScreenWidth;
            _h = Core.ScreenHeight;
        }

        /// <summary>
        /// Creates a new Form
        /// </summary>
        /// <param name="name">Name of the Form</param>
        /// <param name="backColor">Background color</param>
        public Form(string name, Color backColor)
        {
            this.Name = name;
            _bkg = backColor;
            _w = Core.ScreenWidth;
            _h = Core.ScreenHeight;
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
                //if (value)
                //    CheckAutoScroll();
                Render(true);
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
                Render(true);
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
                Render(true);
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
                    Render(true);
            }
        }

        /// <summary>
        /// Forms do not have parents, attempting to set one will throw an exception
        /// </summary>
        public override IContainer Parent
        {
            get { return null; }
            set { throw new InvalidOperationException("Forms cannot have parents"); }
        }

        /// <summary>
        /// Height cannot be set on a Form; attempting to do so will throw an exception
        /// </summary>
        public override int Height
        {
            get { return _h; }
            set { throw new InvalidOperationException("Forms must always be the size of the screen"); }
        }

        public override IContainer TopLevelContainer
        {
            get { return this; }
        }

        /// <summary>
        /// Forms are not allowed to be invisible; attempting to change setting will throw exception
        /// </summary>
        public override bool Visible
        {
            get { return true; }
            set { throw new InvalidOperationException("Forms must always be visible"); }
        }

        /// <summary>
        /// Width cannot be set on a Form; attempting to do so will throw an exception
        /// </summary>
        public override int Width
        {
            get { return _w; }
            set { throw new InvalidOperationException("Forms must always be the size of the screen"); }
        }

        /// <summary>
        /// X must always be 0; attempting to change setting will throw exception
        /// </summary>
        public override int X
        {
            get { return 0; }
            set { throw new InvalidOperationException("Forms must always have X of 0"); }
        }

        /// <summary>
        /// Y must always be 0; attempting to change setting will throw exception
        /// </summary>
        public override int Y
        {
            get { return 0; }
            set { throw new InvalidOperationException("Forms must always have Y of 0"); }
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
                ActiveChild.SendKeyboardAltKeyEvent(key, pressed);
        }

        protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
        {
            if (ActiveChild != null)
                ActiveChild.SendKeyboardKeyEvent(key, pressed);
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
            if (Touching)
            {
                bool bUpdated = false;

                int diffY = 0;
                int diffX = 0;

                // Scroll Y
                if (_maxY > _h)
                {
                    diffY = e.Y - LastTouch.Y;

                    if (diffY > 0 && _minY < Top)
                    {
                        diffY = System.Math.Min(diffY, _minY + Top);
                        bUpdated = true;
                    }
                    else if (diffY < 0 && _maxY > _h)
                    {
                        diffY = System.Math.Min(-diffY, _maxY - _minY - _h);
                        bUpdated = true;
                    }
                    else
                        diffY = 0;

                    _moving = true;
                }

                // Scroll X
                if (_maxX > _w)
                {
                    diffX = e.X - LastTouch.X;

                    if (diffX > 0 && _minX < Left)
                    {
                        diffX = System.Math.Min(diffX, _minX + Left);
                        bUpdated = true;
                    }
                    else if (diffX < 0 && _maxX > _w)
                    {
                        diffX = System.Math.Min(-diffX, _maxX - _minX - _w);
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

            DrawBackground();

            _maxX = _w;
            _maxY = _h;

            // Render controls
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    if (Children[i] != null)
                    {
                        if (Children[i].ScreenBounds.Intersects(area))
                            Children[i].Render();

                        //if (Children[i].Y < _minY)
                        //    _minY = Children[i].Y;
                        //else if (Children[i].Y + Children[i].Height > _maxY)
                        //    _maxY = Children[i].Y + Children[i].Height;

                        //if (Children[i].X < _minX)
                        //    _minX = Children[i].X;
                        //else if (Children[i].X + Children[i].Width > _maxX)
                        //    _maxX = Children[i].X + Children[i].Width;

                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private void DrawBackground()
        {
            Core.Screen.DrawRectangle(_bkg, 0, 0, 0, _w, _h, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);

            if (_img != null)
            {
                switch (_scale)
                {
                    case ScaleMode.Center:
                        Core.Screen.DrawImage(Core.ScreenWidth / 2 - _img.Width / 2, Core.ScreenHeight / 2 - _img.Height / 2, _img, 0, 0, _img.Width, _img.Height);
                        break;
                    case ScaleMode.Normal:
                        Core.Screen.DrawImage(0, 0, _img, 0, 0, _img.Width, _img.Height);
                        break;
                    case ScaleMode.Scale:
                        float multiplier;

                        if (_img.Height > _img.Width)
                        {
                            // Portrait
                            if (_h > _w)
                                multiplier = (float)_w / (float)_img.Width;
                            else
                                multiplier = (float)_h / (float)_img.Height;
                        }
                        else
                        {
                            // Landscape
                            if (_h > _w)
                                multiplier = (float)_w / (float)_img.Width;
                            else
                                multiplier = (float)_h / (float)_img.Height;
                        }

                        int dsW = (int)((float)_img.Width * multiplier);
                        int dsH = (int)((float)_img.Height * multiplier);
                        int dX = (int)((float)_w / 2 - (float)dsW / 2);
                        int dY = (int)((float)_h / 2 - (float)dsH / 2);

                        Core.Screen.StretchImage(dX, dY, _img, dsW, dsH, 256);
                        break;
                    case ScaleMode.Stretch:
                        Core.Screen.StretchImage(0, 0, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                        break;
                    case ScaleMode.Tile:
                        Core.Screen.TileImage(0, 0, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                        break;
                }

            }
        }

        #endregion

    }
}
