using System;
using Microsoft.SPOT;
using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Modal
{
   [Serializable]
   public class ProgressWaiter
   {

      #region Variables

      private readonly string _text;
      private readonly Font _font;
      private readonly int _x;
      private readonly int _y;
      private readonly int _w;
      private readonly int _h;
      //private int _ty;
      //private int _iy;
      //private Bitmap _img = Resources.GetBitmap(Resources.BitmapResources.wait);
      //private int _index = 0;
      private IContainer _prv;
      private Form _frmModal;
      private readonly long _min;
      private readonly long _max;
      private readonly long _value;
      private readonly bool _large;
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
         _w = sz.Width + 32;
         _h = sz.Height;
         _h += 64;

         // Add Progressbar Height
         _large = largeSize;
         _h += (largeSize) ? 22 : 15;

         // Now make sure we're within the screen width
         if (_w > Core.Screen.Width - 32)
            _w = Core.Screen.Width - 32;
         if (_h > Core.Screen.Height - 32)
            _h = Core.Screen.Height - 32;
         _x = Core.Screen.Width / 2 - _w / 2;
         _y = Core.Screen.Height / 2 - _h / 2;

         //_iy = y + (h / 2 - 16);
         //_ty = y + (h / 2 - sz.Height / 2);
      }


      #endregion

      #region Public Methods

      public void Start()
      {
         // Setup
         _prv = Core.ActiveContainer;
         _frmModal = new Form("modalWaitForm", Colors.Ghost);
         Core.SilentlyActivate(_frmModal);

         Core.Screen.SetClippingRectangle(0, 0, _frmModal.Width, _frmModal.Height);
         Core.ShadowRegion(_x, _y, _w, _h);
         Core.Screen.DrawRectangle(0, 0, _x, _y, _w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
         Core.Screen.DrawRectangle(0, 0, _x, _y + _h - 1, _w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
         Core.Screen.DrawRectangle(0, 0, _x, _y + 1, 1, _h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
         Core.Screen.DrawRectangle(0, 0, _x + _w - 1, _y + 1, 1, _h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
         Core.Screen.DrawRectangle(0, 0, _x + 1, _y + 1, _w - 2, _h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
         Core.Screen.DrawRectangle(0, 0, _x + 1, _y + 1, _w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 51);
         Core.Screen.DrawRectangle(0, 0, _x + 1, _y + 2, _w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 26);

         // Add Progressbar
         _prog = new Progressbar("prog1", _x + 16, _y + _h - ((_large) ? 38 : 31), _w - 32, ((_large) ? 22 : 15), _min, _max, _value);

         // Draw Text
         Core.Screen.DrawTextInRect(_text, _x + 16, _y + 16, _w - 32, _h - _prog.Height - 32, Bitmap.DT_WordWrap, Colors.CharcoalDust, _font);

         _frmModal.AddChild(_prog);
         Core.Screen.Flush(_x - 2, _y - 2, _w + 4, _h + 4);
      }

      public void Stop()
      {
         if (Core.ActiveContainer == _frmModal)
         {
            Core.ActiveContainer = _prv;
         }
      }

      public void SetValue(long value)
      {
         if (_prog != null)
         {
            _prog.Value = value;
         }
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
