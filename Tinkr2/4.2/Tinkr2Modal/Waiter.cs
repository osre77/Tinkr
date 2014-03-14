using System;
using System.Threading;
using Microsoft.SPOT;
using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Modal
{
   [Serializable]
   public class Waiter
   {

      #region Variables

      private readonly string _text;
      private readonly Font _font;
      private readonly int _x;
      private readonly int _y;
      private readonly int _w;
      private readonly int _h;
      private readonly int _ty;
      private readonly int _iy;
      private readonly Bitmap _img = Resources.GetBitmap(Resources.BitmapResources.wait);
      private int _index;
      private IContainer _prv;
      private Form _frmModal;
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
         _w = sz.Width + 80;
         _h = sz.Height;
         if (_h < 32)
            _h = 32;
         _h += 32;

         // Now make sure we're within the screen width
         if (_w > Core.ScreenWidth - 32)
            _w = Core.ScreenWidth - 32;
         if (_h > Core.ScreenHeight - 32)
            _h = Core.ScreenHeight - 32;
         _x = Core.ScreenWidth / 2 - _w / 2;
         _y = Core.ScreenHeight / 2 - _h / 2;

         _iy = _y + (_h / 2 - 16);
         _ty = _y + (_h / 2 - sz.Height / 2);

      }


      #endregion

      #region Public Methods

      public void Start()
      {
         // Setup
         _prv = Core.ActiveContainer;
         _frmModal = new Form("modalWaitForm");
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

         // Draw Text
         Core.Screen.DrawTextInRect(_text, _x + 64, _ty, _w - 80, _h + _y - _ty, Bitmap.DT_WordWrap, Colors.CharcoalDust, _font);

         // Draw Image
         DrawImage();

         Core.Screen.Flush(_x - 2, _y - 2, _w + 4, _h + 4);

         _continue = true;
         new Thread(UpdateImage).Start();

      }

      public void Stop()
      {
         _continue = false;
         while (!_done)
            Thread.Sleep(10);

         if (Core.ActiveContainer == _frmModal)
            Core.ActiveContainer = _prv;
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
            Core.Screen.Flush(_x + 16, _iy, 32, 32);
         }
         _done = true;
      }

      private void DrawImage()
      {
         if (Core.ActiveContainer != _frmModal)
         {
            _continue = false;
            return;
         }

         Core.Screen.SetClippingRectangle(_x + 16, _y + 16, 32, 32);
         Core.Screen.DrawRectangle(0, 0, _x + 16, _y + 16, 16, 16, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
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
         Core.Screen.DrawImage(_x + 16, _iy, _img, lx, ly, 32, 32);
      }

      #endregion

   }
}
