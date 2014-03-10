using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Modal
{
    [Serializable]
    public class Waiter
    {

        #region Variables

        private string _text;
        private Font _font;
        private int x, y, w, h, ty, iy;
        private Bitmap _img = Resources.GetBitmap(Resources.BitmapResources.wait);
        private int _index = 0;
        private IContainer prv;
        private Form frmModal;
        private bool _continue;
        private bool _done;

        #endregion

        #region Constructor

        public Waiter(string text, Font font)
        {
            _text = text;
            _font = font;

            // Determine required size
            size sz = FontManager.ComputeExtentEx(font, text);
            w = sz.Width + 80;
            h = sz.Height;
            if (h < 32)
                h = 32;
            h += 32;

            // Now make sure we're within the screen width
            if (w > Core.ScreenWidth - 32)
                w = Core.ScreenWidth - 32;
            if (h > Core.ScreenHeight - 32)
                h = Core.ScreenHeight - 32;
            x = Core.ScreenWidth / 2 - w / 2;
            y = Core.ScreenHeight / 2 - h / 2;

            iy = y + (h / 2 - 16);
            ty = y + (h / 2 - sz.Height / 2);

        }


        #endregion

        #region Public Methods

        public void Start()
        {
            // Setup
            prv = Core.ActiveContainer;
            frmModal = new Form("modalWaitForm");
            Core.SilentlyActivate(frmModal);

            Core.Screen.SetClippingRectangle(0, 0, frmModal.Width, frmModal.Height);
            Core.ShadowRegion(x, y, w, h);
            Core.Screen.DrawRectangle(0, 0, x, y, w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x, y + h - 1, w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x, y + 1, 1, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x + w - 1, y + 1, 1, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, w - 2, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 51);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 2, w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 26);

            // Draw Text
            Core.Screen.DrawTextInRect(_text, x + 64, ty, w - 80, h + y - ty, Bitmap.DT_WordWrap, Colors.CharcoalDust, _font);

            // Draw Image
            DrawImage();

            Core.Screen.Flush(x - 2, y - 2, w + 4, h + 4);

            _continue = true;
            new Thread(UpdateImage).Start();

        }

        public void Stop()
        {
            _continue = false;
            while (!_done)
                Thread.Sleep(10);

            if (Core.ActiveContainer == frmModal)
                Core.ActiveContainer = prv;
        }

        private void UpdateImage()
        {
            _done = false;
            while (_continue)
            {
                Thread.Sleep(100);
                if (!_continue)
                    break;
                DrawImage();
                Core.Screen.Flush(x + 16, iy, 32, 32);
            }
            _done = true;
        }

        private void DrawImage()
        {
            if (Core.ActiveContainer != frmModal)
            {
                _continue = false;
                return;
            }

            Core.Screen.SetClippingRectangle(x + 16, y + 16, 32, 32);
            Core.Screen.DrawRectangle(0, 0, x + 16, y + 16, 16, 16, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
            int lx, ly;
            switch (_index)
            {
                case 0:
                    lx = 0;
                    ly = 0;
                    _index = 1;
                    break;
                case 1:
                    lx = 32;
                    ly = 0;
                    _index = 2;
                    break;
                case 2:
                    lx = 64;
                    ly = 0;
                    _index = 3;
                    break;
                case 3:
                    lx = 96;
                    ly = 0;
                    _index = 4;
                    break;
                case 4:
                    lx = 0;
                    ly = 32;
                    _index = 5;
                    break;
                case 5:
                    lx = 32;
                    ly = 32;
                    _index = 6;
                    break;
                case 6:
                    lx = 64;
                    ly = 32;
                    _index = 7;
                    break;
                default:
                    lx = 96;
                    ly = 32;
                    _index = 0;
                    break;
            }
            Core.Screen.DrawImage(x + 16, iy, _img, lx, ly, 32, 32);
        }

        #endregion

    }
}
