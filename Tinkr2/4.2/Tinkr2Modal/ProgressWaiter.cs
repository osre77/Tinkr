using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Modal
{
    [Serializable]
    public class ProgressWaiter
    {

        #region Variables

        private string _text;
        private Font _font;
        private int x, y, w, h, ty, iy;
        private Bitmap _img = Resources.GetBitmap(Resources.BitmapResources.wait);
        private int _index = 0;
        private IContainer prv;
        private Form frmModal;
        private long _min, _max, _value;
        private bool _large;
        private Progressbar _prog;

        #endregion

        #region Constructor

        public ProgressWaiter(string text, Font font, long minimum, long maxmimum, long value, bool largeSize = false)
        {
            _text = text;
            _font = font;

            _min = minimum;
            _max = maxmimum;
            _value = value;

            // Determine required size
            size sz = FontManager.ComputeExtentEx(font, text);
            w = sz.Width + 32;
            h = sz.Height;
            h += 64;

            // Add Progressbar Height
            _large = largeSize;
            h += (largeSize) ? 22 : 15;

            // Now make sure we're within the screen width
            if (w > Core.Screen.Width - 32)
                w = Core.Screen.Width - 32;
            if (h > Core.Screen.Height - 32)
                h = Core.Screen.Height - 32;
            x = Core.Screen.Width / 2 - w / 2;
            y = Core.Screen.Height / 2 - h / 2;

            iy = y + (h / 2 - 16);
            ty = y + (h / 2 - sz.Height / 2);
        }


        #endregion

        #region Public Methods

        public void Start()
        {
            // Setup
            prv = Core.ActiveContainer;
            frmModal = new Form("modalWaitForm", Colors.Ghost);
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

            // Add Progressbar
            _prog = new Progressbar("prog1", x + 16, y + h - ((_large) ? 38 : 31), w - 32, ((_large) ? 22 : 15), _min, _max, _value);

            // Draw Text
            Core.Screen.DrawTextInRect(_text, x + 16, y + 16, w - 32, h - _prog.Height - 32, Bitmap.DT_WordWrap, Colors.CharcoalDust, _font);

            frmModal.AddChild(_prog);
            Core.Screen.Flush(x - 2, y - 2, w + 4, h + 4);
        }

        public void Stop()
        {
            if (Core.ActiveContainer == frmModal)
                Core.ActiveContainer = prv;
        }

        public void SetValue(long Value)
        {
            if (_prog != null)
                _prog.Value = Value;
        }

        #endregion

        #region Properties

        public long Value
        {
            get
            {
                if (_prog != null)
                    return _prog.Value;
                return _value;
            }
            set
            {
                if (_prog != null)
                    _prog.Value = value;
            }
        }

        #endregion


    }
}
