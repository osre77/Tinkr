using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class Picturebox : ScrollableControl
    {

        #region Variables

        private ScaleMode _scale;
        private bool _autosize = true;
        private BorderStyle _border;
        private Color _brdr;
        private Color _bkg;
        private Bitmap _bmp = null;
        private bool _transBkg;

        private int _h, _w;

        #endregion

        #region Constructors

        public Picturebox(string name, Bitmap image, int x, int y, BorderStyle border = BorderStyle.Border3D)
        {
            this.Name = name;
            _bmp = image;
            X = x;
            Y = y;
            _border = border;
            _scale = NETMF.ScaleMode.Normal;
            if (image == null)
            {
                Width = 64;
                Height = 64;
            }
            else
            {
                switch (border)
                {
                    case NETMF.BorderStyle.Border3D:
                        Width = image.Width + 2;
                        Height = image.Height + 2;
                        break;
                    case NETMF.BorderStyle.BorderFlat:
                        Width = image.Width + 1;
                        Height = image.Height + 1;
                        break;
                    default:
                        Width = image.Width;
                        Height = image.Height;
                        break;
                }
            }
            DefaultColors();

            if (_bmp != null)
            {
                RequiredHeight = _bmp.Height;
                RequiredWidth = _bmp.Width;
            }
        }

        public Picturebox(string name, Bitmap image, int x, int y, int width, int height, BorderStyle border = BorderStyle.Border3D, ScaleMode scale = ScaleMode.Normal)
        {
            this.Name = name;
            _bmp = image;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _autosize = false;
            _border = border;
            _scale = scale;
            DefaultColors();
            if (_bmp != null)
            {
                RequiredHeight = _bmp.Height;
                RequiredWidth = _bmp.Width;
            }
        }

        public Picturebox(string name, Bitmap image, int x, int y, Color backgroundColor = Colors.DarkGray, bool transparentBackground = false, BorderStyle border = BorderStyle.Border3D, ScaleMode scale = ScaleMode.Normal)
        {
            this.Name = name;
            _bmp = image;
            X = x;
            Y = y;
            _border = border;
            _scale = scale;
            if (image != null)
            {
                switch (border)
                {
                    case NETMF.BorderStyle.Border3D:
                        Width = image.Width + 2;
                        Height = image.Height + 2;
                        break;
                    case NETMF.BorderStyle.BorderFlat:
                        Width = image.Width + 1;
                        Height = image.Height + 1;
                        break;
                    default:
                        Width = image.Width;
                        Height = image.Height;
                        break;
                }
            }
            DefaultColors();

            _bkg = backgroundColor;
            _transBkg = transparentBackground;
            if (_bmp != null)
            {
                RequiredHeight = _bmp.Height;
                RequiredWidth = _bmp.Width;
            }
        }

        public Picturebox(string name, Bitmap image, int x, int y, int width, int height, Color backgroundColor, bool transparentBackground = false, BorderStyle border = BorderStyle.Border3D, ScaleMode scale = ScaleMode.Normal)
        {
            this.Name = name;
            _bmp = image;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _autosize = false;
            _border = border;
            _scale = scale;
            DefaultColors();

            _bkg = backgroundColor;
            _transBkg = transparentBackground;
            if (_bmp != null)
            {
                RequiredHeight = _bmp.Height;
                RequiredWidth = _bmp.Width;
            }
        }

        #endregion

        #region  Properties

        /// <summary>
        /// Gets/Sets auto size state
        /// </summary>
        public bool AutoSize
        {
            get { return _autosize; }
            set
            {
                if (_autosize == value)
                    return;
                _autosize = value;
                if (_autosize && Parent != null)
                    Parent.Render(true);
                else
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets/Sets background color
        /// </summary>
        public Color Background
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

        public override bool CanFocus
        {
            get { return false; }
        }

        /// <summary>
        /// Gets/Sets image scale mode
        /// </summary>
        public ScaleMode ScaleMode
        {
            get { return _scale; }
            set
            {
                if (_scale == value)
                    return;
                _scale = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets/Sets border style
        /// </summary>
        public BorderStyle BorderStyle
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
                UpdateScrollbar(false);
                if (Parent != null)
                    Parent.Render(true);
            }
        }

        /// <summary>
        /// Gets/Sets image
        /// </summary>
        public Bitmap Image
        {
            get { return _bmp; }
            set
            {
                if (_bmp == value)
                    return;
                _bmp = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets/Sets width (sets autosize false)
        /// </summary>
        public override int Width
        {
            get { return _w; }
            set
            {
                if (_w == value)
                    return;
                _w = value;
                _autosize = false;
                UpdateScrollbar(false);
                if (Parent != null)
                    Parent.Render(true);
            }
        }

        #endregion

        #region Touch Invokes

        //protected override void TouchMoveMessage(object sender, point e, ref bool handled)
        //{
        //    if (!_canScroll)
        //        return;

        //    bool doInvalidate = false;

        //    _bMoving = true;

        //    int dest = _scrollY - (e.Y - LastTouch.Y);
        //    if (dest < 0)
        //        dest = 0;
        //    else if (dest > _bmp.Height - _h)
        //        dest = _bmp.Height - _h;
        //    if (_scrollY != dest)
        //    {
        //        _scrollY = dest;
        //        doInvalidate = true;
        //    }

        //    dest = _scrollX - (e.X - LastTouch.X);
        //    if (dest < 0)
        //        dest = 0;
        //    else if (dest > _bmp.Width - _w)
        //        dest = _bmp.Width - _w;
        //    if (_scrollX != dest)
        //    {
        //        _scrollX = dest;
        //        doInvalidate = true;
        //    }


        //    LastTouch = e;
        //    if (doInvalidate)
        //        Invalidate();
        //}

        //protected override void TouchUpMessage(object sender, point e, ref bool handled)
        //{
        //    if (_canScroll)
        //    {
        //        _bMoving = false;
        //        Invalidate();
        //    }
        //}

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int w, int h)
        {
            int dsW, dsH, dX, dY;

            // Get Border offset
            int bOffset = 0;
            switch (_border)
            {
                case BorderStyle.Border3D:
                    bOffset = 2;
                    break;
                case BorderStyle.BorderFlat:
                    bOffset = 1;
                    break;
            }

            // Check auto-size first
            if (_autosize && _bmp != null)
            {
                _w = _bmp.Width + bOffset + bOffset;
                _h = _bmp.Height + bOffset + bOffset;
            }

            // Render border & background
            ushort fillVal = 256;
            if (_transBkg)
                fillVal = 0;

            switch (_border)
            {
                case BorderStyle.Border3D:
                    Core.Screen.DrawRectangle(Colors.White, 1, Left, Top, _w, _h, 0, 0, _bkg, 0, 0, _bkg, 0, 0, fillVal);
                    Core.Screen.DrawRectangle(Colors.Gray, 0, Left + 1, Top + 1, _w - 2, _h - 2, 0, 0, _bkg, 0, 0, _bkg, 0, 0, fillVal);
                    break;
                case BorderStyle.BorderFlat:
                    Core.Screen.DrawRectangle(Colors.Black, 1, Left, Top, _w, _h, 0, 0, _bkg, 0, 0, _bkg, 0, 0, fillVal);
                    break;
                case BorderStyle.BorderNone:
                    Core.Screen.DrawRectangle(_bkg, 0, Left, Top, _w, _h, 0, 0, _bkg, 0, 0, _bkg, 0, 0, fillVal);
                    break;
            }

            // Render Image
            if (_bmp != null)
            {
                rect Region = rect.Intersect(new rect(Parent.Left, Parent.Top, Parent.Width, Parent.Height), new rect(Left + bOffset, Top + bOffset, _w - bOffset - bOffset, _h - bOffset - bOffset));
                Core.ClipForControl(this, Region.X, Region.Y, Region.Width, Region.Height);
                switch (_scale)
                {
                    case ScaleMode.Normal:
                        int bx2 = bOffset * 2;
                        Core.Screen.DrawImage(Left + bOffset - ScrollX, Top + bOffset - ScrollY, _bmp, 0, 0, _bmp.Width, _bmp.Height);
                        break;
                    case ScaleMode.Stretch:
                        Core.Screen.StretchImage(Left + bOffset, Top + bOffset, _bmp, _w - bOffset - bOffset, _h - bOffset - bOffset, 256);
                        break;
                    case ScaleMode.Scale:
                        float multiplier;
                        int dH = _h - bOffset - bOffset;
                        int dW = _w - bOffset - bOffset;


                        if (_bmp.Height > _bmp.Width)
                        {
                            // Portrait
                            if (dH > dW)
                                multiplier = (float)dW / (float)_bmp.Width;
                            else
                                multiplier = (float)dH / (float)_bmp.Height;
                        }
                        else
                        {
                            // Landscape
                            if (dH > dW)
                                multiplier = (float)dW / (float)_bmp.Width;
                            else
                                multiplier = (float)dH / (float)_bmp.Height;
                        }

                        dsW = (int)((float)_bmp.Width * multiplier);
                        dsH = (int)((float)_bmp.Height * multiplier);
                        dX = Left + bOffset + (int)((float)dW / 2 - (float)dsW / 2);
                        dY = Top + bOffset + (int)((float)dH / 2 - (float)dsH / 2);

                        Core.Screen.StretchImage(dX, dY, _bmp, dsW, dsH, 256);
                        break;
                    case ScaleMode.Center:
                        dX = Left + (_w / 2 - _bmp.Width / 2);
                        dY = Top + (_h / 2 - _bmp.Height / 2);
                        Core.Screen.DrawImage(dX, dY, _bmp, 0, 0, _bmp.Width, _bmp.Height);
                        break;
                    case ScaleMode.Tile:
                        for (dX = 0; dX < _w; dX += _bmp.Width)
                            for (dY = 0; dY < _h; dY += _bmp.Height)
                                Core.Screen.DrawImage(Left + dX, Top + dY, _bmp, 0, 0, _bmp.Width, _bmp.Height);
                        break;
                }
            }

        }

        private void DefaultColors()
        {
            _brdr = Core.SystemColors.BorderColor;
            _bkg = Core.SystemColors.ContainerBackground;
        }

        #endregion

    }
}
