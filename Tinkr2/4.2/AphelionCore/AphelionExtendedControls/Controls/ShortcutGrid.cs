using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class ShortcutGrid : Container
   {

      #region Variables

      private readonly int _rows;
      private readonly int _cols;
      private readonly int _ySpace;
      private readonly int _xSpace;

      private int _mY;
      private int _mX;
      private bool _bMove;

      //private Shortcut _sel;
      private Bitmap _buffer;
      private Bitmap _bkg;
      private Color _bkgColor;

      private bool _autoSnap;
      private point _ptTapHold;

      readonly int _iW;
      readonly int _iH;

      // Scroll
      //private int _maxX, _maxY;
      //private int _minX, _minY;
      //private bool _moving;

      #endregion

      #region Constructor

      public ShortcutGrid(string name, int x, int y, int width, int height, bool largeIcons = false)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _autoSnap = true;

         if (largeIcons)
         {
            _iW = 108;
            _iH = 83;
         }
         else
         {
            _iW = 74;
            _iH = 58;
         }

         // Determine grid layout
         _rows = height / _iH;
         _cols = width / _iW;

         _xSpace = (width - (_cols * _iW)) / (_cols + 1);
         _ySpace = (height - (_rows * _iH)) / (_rows + 1);

         _bkg = null;
         _bkgColor = Colors.Black;
      }

      #endregion

      #region Properties

      public bool AutoSnap
      {
         get { return _autoSnap; }
         set { _autoSnap = value; }
      }

      public Color BackgroundColor
      {
         get { return _bkgColor; }
         set
         {
            if (_bkgColor == value)
               return;
            _bkgColor = value;
            Invalidate();
         }
      }

      public Bitmap BackgroundImage
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

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         if (Children == null)
            return;

         // Search for child
         for (int i = Children.Length - 1; i >= 0; i--)
         {
            if (Children[i].Visible && Children[i].HitTest(e))
            {
               Children[i].SendTouchDown(this, new point(e.X - Children[i].Left, e.Y - Children[i].Top));
               _ptTapHold = e;
               ActiveChild = Children[i];
               handled = true;
               return;
            }
         }

         // Deactivate Current Active
         ActiveChild = null;

         base.TouchDownMessage(sender, e, ref handled);
      }

      protected override void TouchMoveMessage(object sender, point e, ref bool handled)
      {
         if (ActiveChild == null)
         {
            base.TouchUpMessage(sender, e, ref handled);
            return;
         }

         if (!_bMove)
         {
            int diff = System.Math.Abs(e.X - _ptTapHold.X) + System.Math.Abs(e.Y - _ptTapHold.Y);

            // Let's not just go moving if the users finger slides a bit
            if (diff < 32)
               return;
         }

         if (_buffer == null)
         {
            _bMove = true;
            _buffer = new Bitmap(_iW - 8, _iH - 8);
            _buffer.DrawRectangle(0, 0, 0, 0, _iW - 8, _iH - 8, 0, 0, Colors.LightBlue, 0, 0, Colors.LightBlue, 0, 0, 256);
            ((Shortcut)ActiveChild).RenderToBuffer(_buffer);
            _mY = ActiveChild.Y;
            _mX = ActiveChild.X;
         }

         // Invalidate last location, silently
         lock (Core.Screen)
         {
            Core.Screen.SetClippingRectangle(_mX, _mY, _iW - 8, _iH - 8);
            Core.Screen.DrawRectangle(0, 0, _mX, _mY, _iW - 8, _iH - 8, 0, 0, _bkgColor, 0, 0, _bkgColor, 0, 0, 256);
            if (_bkg != null)
            {
               Core.Screen.DrawImage(0, 0, _bkg, 0, 0, _bkg.Width, _bkg.Height);
            }
            var r = new rect(_mX, _mY, _iW - 8, _iH - 8);
            for (int i = 0; i < Children.Length; i++)
            {
               if (Children[i] != ActiveChild && Children[i].ScreenBounds.Intersects(r))
                  Children[i].Render();
            }

            // Update Location
            int tmpX = _mX;
            int tmpY = _mY;

            _mY += e.Y - _ptTapHold.Y;
            _mX += e.X - _ptTapHold.X;
            if (_mX < 0)
               _mX = 0;
            else if (_mX + _iW - 8 > Width)
               _mX = Width - _iW - 8;
            if (_mY < 0)
               _mY = 0;
            else if (_mY + _iH - 8 > Height)
               _mY = Height - _iH - 8;
            _ptTapHold = new point(_mX, _mY);

            // Draw NEW location
            Core.Screen.SetClippingRectangle(Top, Left, Width, Height);
            Core.Screen.DrawImage(_mX, _mY, _buffer, 0, 0, 100, 750);

            // Flush
            int x = (tmpX < _mX) ? tmpX : _mX;
            int y = (tmpY < _mY) ? tmpY : _mY;

            int w = System.Math.Abs(_mX - tmpX) + _iW - 8;
            int h = System.Math.Abs(_mY - tmpY) + _iH - 8;
            Core.Screen.Flush(x, y, w, h);
         }
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         if (Children == null || ActiveChild == null)
         {
            base.TouchUpMessage(sender, e, ref handled);
            return;
         }

         ActiveChild.SendTouchUp(this, e);
         handled = true;

         if (_bMove)
         {
            _bMove = false;
            _buffer = null;
            ((Shortcut)ActiveChild).Location = new point(_mX, _mY);
            ActiveChild = null;
         }
      }

      #endregion

      #region Public Methods

      public override void AddChild(IControl child)
      {
         if (child is Shortcut)
         {
            if (_autoSnap)
            {
               int iscc = 0;
               int iRow = 0;

               // Find first available spot
               int x = _xSpace;
               int fX = x;
               int mX = 0;
               int y = _ySpace;
               if (Children != null)
               {
                  while (true)
                  {
                     bool bBlocked = false;
                     int i;
                     for (i = 0; i < Children.Length; i++)
                     {
                        if (Children[i].X == x && Children[i].Y == y)
                        {
                           iscc += 1;
                           if (x > mX)
                              mX = x;

                           x += _iW + _xSpace;

                           if (iscc == _cols)
                           {
                              int nX = x;
                              y += _iH + _ySpace;
                              iRow += 1;
                              iscc = 0;

                              if (iRow == _rows)
                              {
                                 iRow = 0;
                                 y = _ySpace;
                                 x = nX;
                                 fX = x;
                              }
                              else
                                 x = fX;
                           }

                           bBlocked = true;
                           break;
                        }
                     }

                     if (!bBlocked)
                        break;
                  }
               }

               // Update Control
               child.X = x;
               child.Y = y;
            }

            base.AddChild(child);
         }
         else
            throw new Exception("Only Shortcuts can be added to ShortcutGrids");
      }

      #endregion

      #region GUI

      protected override void OnRender(int x, int y, int w, int h)
      {
         Core.Screen.DrawRectangle(_bkgColor, 0, Left, Top, Width, Height, 0, 0, _bkgColor, 0, 0, _bkgColor, 0, 0, 256);
         if (_bkg != null)
            Core.Screen.DrawImage(Left, Top, _bkg, 0, 0, _bkg.Width, _bkg.Height);

         if (Children != null)
         {
            for (int i = 0; i < Children.Length; i++)
            {
               Children[i].Render();
            }
         }
      }

      #endregion

   }
}
