using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{

   [Serializable]
   public delegate void OnCombinationEntered(object sender, string combination);

   [Serializable]
   public class GraphicLock : Control
   {

      #region Structures

      private struct DirImg
      {
         public byte[] Pts;
         public int Used;
         public Bitmap Bmp;
      }

      #endregion

      #region Variables

      private readonly Bitmap[] _arrows;
      private readonly Bitmap[] _circle;
      private rect[] _rects;
      private byte[] _pips;
      private int _curPip;
      private DirImg[] _dirs;
      private string _combo;

      #endregion

      #region Constructor

      public GraphicLock(string name, int x, int y)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _arrows = new[] { Resources.GetBitmap(Resources.BitmapResources.down), Resources.GetBitmap(Resources.BitmapResources.downleft), Resources.GetBitmap(Resources.BitmapResources.downright),
                                     Resources.GetBitmap(Resources.BitmapResources.left), Resources.GetBitmap(Resources.BitmapResources.right),
                                     Resources.GetBitmap(Resources.BitmapResources.up), Resources.GetBitmap(Resources.BitmapResources.upleft), Resources.GetBitmap(Resources.BitmapResources.upright) };
         _circle = new[] { Resources.GetBitmap(Resources.BitmapResources.gray), Resources.GetBitmap(Resources.BitmapResources.green),
                                     Resources.GetBitmap(Resources.BitmapResources.blue), Resources.GetBitmap(Resources.BitmapResources.red) };
         Reset();
      }

      #endregion

      #region Events

      public event OnCombinationEntered CombinationEntered;
      protected virtual void OnCombinationEntered(object sender, string combination)
      {
         var tmp = CombinationEntered;
         if (tmp != null)
         {
            tmp(sender, combination);
         }
      }

      #endregion

      #region Properties

      public override int Height
      {
         get
         {
            return 216;
         }
         set
         {
            throw new Exception("GraphicLock must be 216x216");
         }
      }

      public override int Width
      {
         get
         {
            return 216;
         }
         set
         {
            throw new Exception("GraphicLock must be 216x216");
         }
      }

      #endregion

      #region Public Methods

      public void Explode(Bitmap image)
      {
         if (Parent == null || Parent.Suspended || Suspended || !Enabled)
            return;

         if (image.Width != image.Height || image.Width > 128)
            throw new Exception("Image must be square and no more than 128x128");

         int sz = (image.Width / 4);
         int hsz = sz / 2;
         int y = Top + (Height / 2 - image.Height / 2) + (sz / 2) - 1;
         int i;
         int ix = Core.ScreenWidth / 2 - image.Width / 2;
         int iy = Core.ScreenHeight / 2 - image.Height / 2;
         Color c = ColorUtility.ColorFromRGB(220, 223, 223);

         // Rects
         var r = new rect[16];
         point[] p =
         { 
            new point(Left+24-hsz,Top+24-hsz),new point(Left+80-hsz,Top+24-hsz),new point(Left+136-hsz,Top+24-hsz), new point(Left+192-hsz,Top+24-hsz),
            new point(Left+24-hsz,Top+80-hsz),new point(Left+80-hsz,Top+80-hsz),new point(Left+136-hsz,Top+80-hsz), new point(Left+192-hsz,Top+80-hsz),
            new point(Left+24-hsz,Top+136-hsz),new point(Left+80-hsz,Top+136-hsz),new point(Left+136-hsz,Top+136-hsz), new point(Left+192-hsz,Top+136-hsz),
            new point(Left+24-hsz,Top+192-hsz),new point(Left+80-hsz,Top+192-hsz),new point(Left+136-hsz,Top+192-hsz), new point(Left+192-hsz,Top+192-hsz)
         };
         for (i = 0; i < r.Length; i += 4)
         {
            int x = Left + (Width / 2 - image.Width / 2) + (sz / 2) - 1;
            r[i] = new rect(x, y, 1, 1);
            x += sz;
            r[i + 1] = new rect(x, y, 1, 1);
            x += sz;
            r[i + 2] = new rect(x, y, 1, 1);
            x += sz;
            r[i + 3] = new rect(x, y, 1, 1);
            y += sz;
         }

         // Initial
         Core.Screen.SetClippingRectangle(0, 0, Core.Screen.Width, Core.Screen.Height);
         Core.Screen.DrawImage(ix, iy, image, 0, 0, image.Width, image.Height);
         Core.Screen.Flush(ix, iy, image.Width, image.Height);

         // Cover
         while (r[0].Width <= sz + 1)
         {
            for (i = 0; i < r.Length; i++)
            {
               Core.Screen.DrawRectangle(0, 0, r[i].X, r[i].Y, r[i].Width + 1, r[i].Height + 1, 0, 0, c, 0, 0, c, 0, 0, 256);
               r[i].X -= 1;
               r[i].Y -= 1;
               r[i].Width += 2;
               r[i].Height += 2;
            }
            Core.Screen.Flush(ix, iy, image.Width + 1, image.Height + 1);
            Thread.Sleep(10);
         }

         // Split Cells
         while (true)
         {
            bool isDone = true;
            Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, Color.White, 0, 0, Color.White, 0, 0, 256);

            for (i = 0; i < r.Length; i++)
            {
               if (r[i].X > p[i].X)
               {
                  r[i].X -= 10;
                  if (r[i].X < p[i].X)
                     r[i].X = p[i].X;
                  isDone = false;
               }
               else if (r[i].X < p[i].X)
               {
                  r[i].X += 10;
                  if (r[i].X > p[i].X)
                     r[i].X = p[i].X;
                  isDone = false;
               }
               if (r[i].Y > p[i].Y)
               {
                  r[i].Y -= 10;
                  if (r[i].Y < p[i].Y)
                     r[i].Y = p[i].Y;
                  isDone = false;
               }
               else if (r[i].Y < p[i].Y)
               {
                  r[i].Y += 10;
                  if (r[i].Y > p[i].Y)
                     r[i].Y = p[i].Y;
                  isDone = false;
               }

               Core.Screen.DrawRectangle(0, 0, r[i].X, r[i].Y, r[i].Width, r[i].Height, 0, 0, c, 0, 0, c, 0, 0, 256);
            }


            Core.Screen.Flush(Left, Top, Width, Height);
            if (isDone)
               break;
            Thread.Sleep(10);
         }

         // Morph
         int img = 0;
         if (sz > 40)
            img = 4;
         else if (sz > 32)
            img = 3;
         else if (sz > 22)
            img = 2;
         else if (sz > 10)
            img = 1;

         Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, Color.White, 0, 0, Color.White, 0, 0, 256);
         while (img < 4)
         {
            Bitmap bmp;
            switch (img)
            {
               case 0:
                  bmp = Resources.GetBitmap(Resources.BitmapResources.g1);
                  break;
               case 1:
                  bmp = Resources.GetBitmap(Resources.BitmapResources.g2);
                  break;
               case 2:
                  bmp = Resources.GetBitmap(Resources.BitmapResources.g3);
                  break;
               case 3:
                  bmp = Resources.GetBitmap(Resources.BitmapResources.g4);
                  break;
               default:
                  bmp = Resources.GetBitmap(Resources.BitmapResources.gray);
                  break;
            }

            Core.Screen.DrawImage(Left, Top, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 56, Top, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 112, Top, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 168, Top, bmp, 0, 0, 48, 48);

            Core.Screen.DrawImage(Left, Top + 56, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 56, Top + 56, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 112, Top + 56, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 168, Top + 56, bmp, 0, 0, 48, 48);

            Core.Screen.DrawImage(Left, Top + 112, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 56, Top + 112, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 112, Top + 112, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 168, Top + 112, bmp, 0, 0, 48, 48);

            Core.Screen.DrawImage(Left, Top + 168, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 56, Top + 168, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 112, Top + 168, bmp, 0, 0, 48, 48);
            Core.Screen.DrawImage(Left + 168, Top + 168, bmp, 0, 0, 48, 48);

            Core.Screen.Flush(Left, Top, Width, Height);
            img += 1;
            Thread.Sleep(5);
         }
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         for (int i = 0; i < 16; i++)
         {
            if (_rects[i].Contains(e))
            {
               _pips[i] = 1;
               _curPip = i;

               if (_curPip < 10)
                  _combo += "0" + _curPip;
               else
                  _combo += _curPip;

               Invalidate();
               return;
            }
         }
      }

      protected override void TouchMoveMessage(object sender, point e, ref bool handled)
      {
         for (int i = 0; i < 16; i++)
         {
            if (_rects[i].Contains(e))
            {
               if (_curPip == i)
                  return;

               if (_curPip == -1)
                  _pips[i] = 1;
               else
               {
                  if (_pips[i] < 3)
                  {
                     _pips[i]++;
                     AddDirection(_curPip, i);
                     _curPip = i;
                  }
               }
               _curPip = i;
               Invalidate();
               return;
            }
         }
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         // save _combo because its cleared in Reset() (Thanks to han-swurst for finding and providing a solution for this)
         var combo = _combo;
         Reset();
         Invalidate();
         OnCombinationEntered(this, combo);
      }

      #endregion

      #region GUI

      protected override void OnRender(int x, int y, int w, int h)
      {
         int i;
         int xx;
         int yy = y + 9;

         Core.Screen.DrawRectangle(0, 0, x, y, w, h, 0, 0, Color.White, 0, 0, Color.White, 0, 0, 256);

         // Left/Right Up/Down
         for (i = 0; i < 32; i += 10)
         {
            xx = x + 48;
            if (_dirs[i].Used > 0)
               Core.Screen.DrawImage(xx, yy, _dirs[i].Bmp, 0, 0, _dirs[i].Bmp.Width, _dirs[i].Bmp.Height);
            xx += 56;
            if (_dirs[i + 1].Used > 0)
               Core.Screen.DrawImage(xx, yy, _dirs[i + 1].Bmp, 0, 0, _dirs[i + 1].Bmp.Width, _dirs[i + 1].Bmp.Height);
            xx += 56;
            if (_dirs[i + 2].Used > 0)
               Core.Screen.DrawImage(xx, yy, _dirs[i + 2].Bmp, 0, 0, _dirs[i + 2].Bmp.Width, _dirs[i + 2].Bmp.Height);
            yy += 39;

            if (i < 30)
            {
               xx = x + 9;
               if (_dirs[i + 3].Used > 0)
                  Core.Screen.DrawImage(xx, yy, _dirs[i + 3].Bmp, 0, 0, _dirs[i + 3].Bmp.Width, _dirs[i + 3].Bmp.Height);
               xx += 56;
               if (_dirs[i + 5].Used > 0)
                  Core.Screen.DrawImage(xx, yy, _dirs[i + 5].Bmp, 0, 0, _dirs[i + 5].Bmp.Width, _dirs[i + 5].Bmp.Height);
               xx += 56;
               if (_dirs[i + 7].Used > 0)
                  Core.Screen.DrawImage(xx, yy, _dirs[i + 7].Bmp, 0, 0, _dirs[i + 7].Bmp.Width, _dirs[i + 7].Bmp.Height);
               xx += 56;
               if (_dirs[i + 9].Used > 0)
                  Core.Screen.DrawImage(xx, yy, _dirs[i + 9].Bmp, 0, 0, _dirs[i + 9].Bmp.Width, _dirs[i + 9].Bmp.Height);
               //xx += 56;
            }
            yy += 17;
         }

         // Diags
         yy = y + 37;
         for (i = 4; i < 32; i += 10)
         {
            xx = x + 49;
            if (_dirs[i].Used > 0)
               Core.Screen.DrawImage(xx, yy, _dirs[i].Bmp, 0, 0, _dirs[i].Bmp.Width, _dirs[i].Bmp.Height);
            xx += 56;
            if (_dirs[i + 2].Used > 0)
               Core.Screen.DrawImage(xx, yy, _dirs[i + 2].Bmp, 0, 0, _dirs[i + 2].Bmp.Width, _dirs[i + 2].Bmp.Height);
            xx += 56;
            if (_dirs[i + 4].Used > 0)
               Core.Screen.DrawImage(xx, yy, _dirs[i + 4].Bmp, 0, 0, _dirs[i + 4].Bmp.Width, _dirs[i + 4].Bmp.Height);

            yy += 56;
         }

         yy = y;
         for (i = 0; i < 16; i += 4)
         {
            xx = x;
            Core.Screen.DrawImage(xx, yy, _circle[_pips[i]], 0, 0, 48, 48);
            xx += 56;
            Core.Screen.DrawImage(xx, yy, _circle[_pips[i + 1]], 0, 0, 48, 48);
            xx += 56;
            Core.Screen.DrawImage(xx, yy, _circle[_pips[i + 2]], 0, 0, 48, 48);
            xx += 56;
            Core.Screen.DrawImage(xx, yy, _circle[_pips[i + 3]], 0, 0, 48, 48);
            yy += 56;
         }
      }

      #endregion

      #region Private Methods

      private void AddDirection(int pip1, int pip2)
      {
         int idx;

         // integer devision is like floor by default
         if (pip2 == pip1 + 1)           // Right
         {
            //idx = (int)(pip1 + (System.Math.Floor(pip1 / 4) * 6));
            idx = pip1 + ((pip1 / 4) * 6);
            AddPicture((byte)idx, 4);
         }
         else if (pip2 == pip1 - 1)      // Left
         {
            //idx = (int)(pip2 + (System.Math.Floor(pip2 / 4) * 6));
            idx = pip2 + ((pip2 / 4) * 6);
            AddPicture((byte)idx, 3);
         }
         else if (pip2 == pip1 + 4)      // Down
         {
            idx = pip2 - 1 + ((pip1 / 4) * 6) + (pip1 % 4);
            AddPicture((byte)idx, 0);
         }
         else if (pip2 == pip1 - 4)      // Up
         {
            idx = pip1 - 1 + ((pip2 / 4) * 6) + (pip2 % 4);
            AddPicture((byte)idx, 5);
         }
         else if (pip2 == pip1 + 5)      // Down Right
         {
            idx = 4 + ((pip1 % 4) * 2) + ((pip1 / 4) * 10);
            AddPicture((byte)idx, 2);
         }
         else if (pip2 == pip1 - 5)      // Up Left
         {
            idx = 4 + ((pip2 % 4) * 2) + ((pip2 / 4) * 10);
            AddPicture((byte)idx, 6);
         }
         else if (pip2 == pip1 + 3)      // Down Left
         {
            idx = 4 + (((pip1 - 1) % 4) * 2) + ((pip1 / 4) * 10);
            AddPicture((byte)idx, 1);
         }
         else if (pip2 == pip1 - 3)      // Up Right
         {
            idx = 4 + (((pip2 - 1) % 4) * 2) + ((pip2 / 4) * 10);
            AddPicture((byte)idx, 7);
         }

         if (pip2 < 10)
            _combo += "0" + pip2;
         else
            _combo += pip2;
      }

      private void AddPicture(byte toArea, byte picType)
      {
         DirImg di = _dirs[toArea];

         if (di.Used == 0)
         {
            switch (picType)
            {
               case 3:     // Left & Right
               case 4:
                  di.Bmp = new Bitmap(8, 30);
                  break;
               case 0:     // Up & Down
               case 5:
                  di.Bmp = new Bitmap(30, 8);
                  break;
               default:    // Diags
                  di.Bmp = new Bitmap(7, 30);
                  break;
            }

            di.Used = 1;
            di.Pts = new byte[3];
            di.Pts[0] = picType;
         }
         else if (di.Used < 3)
         {
            di.Pts[di.Used] = picType;
            di.Used += 1;
         }

         di.Bmp.DrawRectangle(0, 0, 0, 0, di.Bmp.Width, di.Bmp.Height, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 256);

         int x, y, i;
         switch (picType)
         {
            case 3:     // Left & Right
            case 4:
               x = 1;
               y = 15 - ((8 * di.Used) + (3 * (di.Used - 1))) / 2;
               for (i = 0; i < di.Used; i++)
               {
                  di.Bmp.DrawImage(x, y, _arrows[di.Pts[i]], 0, 0, 5, 8);
                  y += 11;
               }
               break;
            case 0:     // Up & Down
            case 5:
               y = 1;
               x = 15 - ((8 * di.Used) + (3 * (di.Used - 1))) / 2;
               for (i = 0; i < di.Used; i++)
               {
                  di.Bmp.DrawImage(x, y, _arrows[di.Pts[i]], 0, 0, 8, 5);
                  x += 11;
               }
               break;
            default:    // Diags
               x = 1;
               y = 15 - ((8 * di.Used) + (3 * (di.Used - 1))) / 2;
               for (i = 0; i < di.Used; i++)
               {
                  di.Bmp.DrawImage(x, y, _arrows[di.Pts[i]], 0, 0, 17, 15);
                  y += 9;
               }
               break;
         }

         di.Bmp.MakeTransparent(Color.White);
         _dirs[toArea] = di;
      }

      private void Reset()
      {
         _pips = new byte[16];
         _dirs = new DirImg[33];
         _rects = new rect[16];
         int y = 5;
         for (int i = 0; i < 16; i += 4)
         {
            int x = 5;
            _rects[i] = new rect(Left + x, Top + y, 38, 38);
            x += 56;
            _rects[i + 1] = new rect(Left + x, Top + y, 38, 38);
            x += 56;
            _rects[i + 2] = new rect(Left + x, Top + y, 38, 38);
            x += 56;
            _rects[i + 3] = new rect(Left + x, Top + y, 38, 38);
            y += 56;
         }
         _curPip = -1;
         _combo = string.Empty;
      }

      #endregion

   }
}
