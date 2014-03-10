using System;
using System.IO;
using System.Text;

using Microsoft.SPOT;

using Skewworks.NETMF.Graphics;
using Skewworks.NETMF.Resources;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class Shortcut : Control
    {

        #region Enumerations

        public enum ImageTypes
        {
            Bitmap = 0,
            Image32 = 1,
        }

        #endregion

        #region Variables

        private Image32 _img;
        private Bitmap _bmp;
        private string _target;
        private string _title;
        
        private Font _font;
        private ImageTypes _imgType;

        private int _tT;

        #endregion

        #region Constructor

        public Shortcut(string name, Bitmap image, string title, string target, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;

            if (image.Width >= 48)
            {
                Width = 100;
                Height = 75;
                _tT = 52;
            }
            else
            {
                Width = 66;
                Height = 50;
                _tT = 36;
            }

            _imgType = ImageTypes.Bitmap;
            _bmp = image;

            _title = title;
            _target = target;

            UpdateFont();
        }

        public Shortcut(string name, Image32 image, string title, string target, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;

            if (image.Width >= 48)
            {
                Width = 100;
                Height = 75;
                _tT = 52;
            }
            else
            {
                Width = 66;
                Height = 50;
                _tT = 36;
            }

            _imgType = ImageTypes.Image32;
            _img = image;

            _title = title;
            _target = target;
            
            UpdateFont();
        }

        #endregion

        #region Properties

        public object Image
        {
            get 
            {
                if (_imgType == ImageTypes.Bitmap)
                    return _bmp;
                return _img;
            }
            set
            {
                if (_img == value)
                    return;

                if (value is Image32)
                {
                    _img = (Image32)value;
                    _bmp = null;
                    _imgType = ImageTypes.Image32;
                }
                else
                {
                    _bmp = (Bitmap)value;
                    _img = null;
                    _imgType = ImageTypes.Bitmap;
                }

                Invalidate();
            }
        }

        public ImageTypes ImageType
        {
            get { return _imgType; }
        }

        public string Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                    return;
                _title = value;
                UpdateFont();
                Invalidate();
            }
        }

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int w, int h)
        {
            int tH = FontManager.ComputeExtentEx(_font, _title).Height;
            int aH = Height - _tT;

            x = Left;
            y = Top;

            if (_imgType == ImageTypes.Bitmap)
                Core.Screen.DrawImage(x + (Width / 2 - _bmp.Width / 2), y, _bmp, 0, 0, _bmp.Width, _bmp.Height);
            else
                _img.Draw(Core.Screen, x + (Width / 2 - _img.Width / 2), y);

            if (Parent.ActiveChild == this)
            {
                Core.Screen.DrawRectangle(0, 0, x, y + _tT, Width, aH, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 160);
                Core.Screen.DrawTextInRect(_title, x + 2, y + _tT + (aH / 2 - tH / 2), Width - 4, tH, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingCharacterEllipsis,
                    Core.SystemColors.SelectedFontColor, _font);
            }
            else
            {
                Core.Screen.DrawRectangle(0, 0, x, y + _tT, Width, aH, 0, 0, Core.SystemColors.AltSelectionColor, 0, 0, Core.SystemColors.AltSelectionColor, 0, 0, 160);
                Core.Screen.DrawTextInRect(_title, x + 2, y + _tT + (aH / 2 - tH / 2), Width - 4, tH, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingCharacterEllipsis,
                    Core.SystemColors.AltSelectedFontColor, _font);
            }

        }

        public void RenderToBuffer(Bitmap Buffer)
        {
            if (_imgType == ImageTypes.Image32)
                _img.Draw(Buffer, Buffer.Width / 2 - _img.Width / 2, 0);
            else
                Buffer.DrawImage(Buffer.Width / 2 - _bmp.Width / 2, 0, _bmp, 0, 0, _bmp.Width, _bmp.Height);

            Buffer.DrawRectangle(0, 0, 0, _tT, Buffer.Width, Buffer.Height - _tT, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 160);

            int tH = FontManager.ComputeExtentEx(_font, _title).Height;
            int aH = Height - _tT;

            Buffer.DrawTextInRect(_title, 2, _tT + (aH / 2 - tH / 2), Width - 4, tH, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingCharacterEllipsis,
                Core.SystemColors.SelectedFontColor, _font);
        }

        #endregion

        #region Private Methods

        private void UpdateFont()
        {
            if (Fonts.Droid16.Height <= Height - _tT &&  FontManager.ComputeExtentEx(Fonts.Droid16, _title).Width < Width - 4)
                _font = Fonts.Droid16;
            else if (Fonts.Droid12.Height <= Height - _tT && FontManager.ComputeExtentEx(Fonts.Droid12, _title).Width < Width - 4)
                _font = Fonts.Droid12;
            else if (Fonts.Droid11.Height <= Height - _tT && FontManager.ComputeExtentEx(Fonts.Droid11, _title).Width < Width - 4)
                _font = Fonts.Droid11;
            else if (Fonts.Droid9.Height <= Height - _tT && FontManager.ComputeExtentEx(Fonts.Droid9, _title).Width < Width - 4)
                _font = Fonts.Droid9;
            else
                _font = Fonts.Droid8;
        }

        #endregion

    }
}
