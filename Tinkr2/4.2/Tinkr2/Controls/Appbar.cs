using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class Appbar : Control
   {

      #region Variables

      // Appearance
      private Font _font;
      private Font _timeFont;
      private Color _bkg;
      private Color _fore;
      private DockLocation _dock;
      private bool _displayTime;
      private TimeFormat _timeFormat;
      private bool _expanded;
      private bool _farAlign;
      private int _selIndex;

      // Metrics
      private int _x, _y, _w, _h;
      private int _dW, _dH;

      // Time Keeping
      private Thread _mon;
      private int _min;

      // Children
      private AppbarIcon[] _icons;
      private AppbarMenuItem[] _menus;

      // Ellipsis
      private readonly Bitmap _elli;
      private rect _elliRect;

      // Expand/Collapse
      //private bool _eDown;

      // Scrolling
      private int _scrollValue;
      private rect _scrollArea;
      private bool _sDown;
      private bool _moved;
      private int _requiredDisplaySize;
      private int _scrollValueRange;
      private int _gripSize;

      #endregion

      #region Constructor

      public Appbar(string name, int size, Font timeFont, Font menuFont, DockLocation dock)
      {
         Name = name;
         _timeFont = timeFont;
         _font = menuFont;
         _dock = dock;
         _timeFormat = TimeFormat.Hour12;
         _displayTime = true;
         Metrics(size);
         _bkg = ColorUtility.ColorFromRGB(30, 30, 30);
         _fore = Colors.White;
         _elli = new Bitmap(4, 4);
         _elliRect = new rect(Left, Top, 64, 64);
         UpdateEllipsis();
      }

      public Appbar(string name, int size, Font timeFont, Font menuFont, DockLocation dock, bool displayTime)
      {
         Name = name;
         _timeFont = timeFont;
         _font = menuFont;
         _dock = dock;
         _timeFormat = TimeFormat.Hour12;
         _displayTime = displayTime;
         Metrics(size);
         _bkg = ColorUtility.ColorFromRGB(30, 30, 30);
         _fore = Colors.White;
         _elli = new Bitmap(4, 4);
         _elliRect = new rect(Left, Top, 64, 64);
         UpdateEllipsis();
      }

      #endregion

      #region Properties

      public Color BackColor
      {
         get { return _bkg; }
         set
         {
            if (_bkg == value)
               return;
            _bkg = value;
            UpdateEllipsis();
            Invalidate();
         }
      }

      public int BaseHeight
      {
         get { return _dH; }
      }

      public int BaseWidth
      {
         get { return _dW; }
      }

      /// <summary>
      /// Location to dock AppBar
      /// </summary>
      public DockLocation Dock
      {
         get { return _dock; }
         set
         {
            if (_dock == value)
               return;
            int size;
            if (value == DockLocation.Bottom || value == DockLocation.Top)
            {
               size = _dH;
            }
            else
            {
               size = _dW;
            }
            _dock = value;
            _expanded = false;
            Metrics(size);
            Invalidate();
         }
      }

      /// <summary>
      /// Displays time when true
      /// </summary>
      public bool DisplayTime
      {
         get { return _displayTime; }
         set
         {
            if (_displayTime == value)
               return;
            _displayTime = value;
            Invalidate();
         }
      }

      public bool Expanded
      {
         get { return _expanded; }
         set
         {
            if (_expanded == value)
               return;

            _expanded = !value;
            ShowHideDock();
         }
      }

      public bool FarAlignment
      {
         get { return _farAlign; }
         set
         {
            if (_farAlign == value)
               return;
            _farAlign = value;
            Invalidate();
         }
      }

      public Color ForeColor
      {
         get { return _fore; }
         set
         {
            if (_fore == value)
               return;
            _fore = value;
            UpdateEllipsis();
            Invalidate();
         }
      }

      public Font Font
      {
         get { return _font; }
         set
         {
            if (_font == value)
               return;
            _font = value;
            Invalidate();
         }
      }

      public override int Height
      {
         get
         {
            if (_expanded)
               return _h;
            return _dH;
         }
         set
         {
            if (_dH == value)
               return;
            _dH = value;
            if (!_expanded)
               Invalidate();
         }
      }

      public AppbarIcon[] Icons
      {
         get { return _icons; }
      }

      public AppbarMenuItem[] Menus
      {
         get { return _menus; }
      }

      /// <summary>
      /// Gets/Sets containing parent
      /// </summary>
      public override IContainer Parent
      {
         get { return base.Parent; }
         set
         {
            base.Parent = value;

            if (value != null)
            {
               if (_mon == null || !_mon.IsAlive)
               {
                  _mon = new Thread(MonitorTime);
                  _mon.Start();
               }
            }
            else if (_mon != null)
               _mon.Abort();

         }
      }

      public Font TimeFont
      {
         get { return _timeFont; }
         set
         {
            if (_timeFont == value)
               return;
            _timeFont = value;
            Invalidate();
         }
      }

      public TimeFormat TimeFormat
      {
         get { return _timeFormat; }
         set
         {
            if (_timeFormat == value)
               return;
            _timeFormat = value;
            Invalidate();
         }
      }

      public override int Width
      {
         get
         {
            if (_expanded)
               return _w;
            return _dW;
         }
         set
         {
            if (_dW == value)
               return;
            _dW = value;
            if (!_expanded)
               Invalidate();
         }
      }

      public override int X
      {
         get { return _x; }
         set { } // throw new Exception("Cannot modify Appbar X"); }
      }

      public override int Y
      {
         get { return _y; }
         set { } // throw new Exception("Cannot modify Appbar Y"); }
      }

      #endregion

      #region Buttons

      bool _btnDown;
      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (!_expanded || _menus == null || _menus.Length == 0)
            return;

         if (buttonId == (int)ButtonIDs.Up)
         {
            _btnDown = true;
            _selIndex -= 1;
            if (_selIndex < 0)
               _selIndex = 0;
            Invalidate();
         }
         else if (buttonId == (int)ButtonIDs.Down)
         {
            _btnDown = true;
            _selIndex += 1;
            if (_selIndex > _menus.Length - 1)
               _selIndex = _menus.Length - 1;
            Invalidate();
         }

         handled = true;
      }

      // ReSharper disable once RedundantAssignment
      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         if (_btnDown)
         {
            _btnDown = false;
            if (buttonId == (int)ButtonIDs.Select)
            {
               ShowHideDock();
               _menus[_selIndex].SendTouchDown(this, new point(_menus[_selIndex].X, _menus[_selIndex].Y));
               _menus[_selIndex].SendTouchUp(this, new point(_menus[_selIndex].X, _menus[_selIndex].Y));
            }
         }
         handled = true;
      }

      #endregion

      #region Keyboard

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         if (_elliRect.Contains(e))
         {
            //_eDown = true;
            //handled = true;
            return;
         }

         int i;

         if (_icons != null)
         {
            for (i = 0; i < _icons.Length; i++)
            {
               if (_icons[i].HitTest(e))
               {
                  _icons[i].SendTouchDown(this, e);
                  //handled = true;
                  return;
               }
            }
         }

         if (_expanded)
         {
            if (!_scrollArea.Contains(e))
               return;

            _sDown = true;

            for (i = 0; i < _menus.Length; i++)
            {
               if (_menus[i].HitTest(e))
               {
                  _menus[i].SendTouchDown(this, e);
                  //handled = true;
                  Invalidate();
                  return;
               }
            }
         }
      }

      protected override void TouchMoveMessage(object sender, point e, ref bool handled)
      {
         if (!_sDown)
            return;

         if (!_moved && (e.Y - LastTouch.Y) < 12)
            return;

         _moved = true;

         int dest = _scrollValue - (e.Y - LastTouch.Y);
         if (dest < 0)
            dest = 0;
         else if (dest > _requiredDisplaySize - _scrollArea.Height)
            dest = _requiredDisplaySize - _scrollArea.Height;
         if (_scrollValue != dest && dest > 0)
         {
            _scrollValue = dest;
            Invalidate();
         }


         LastTouch = e;
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         _sDown = false;

         //if (_moved)
         //    return;

         if (_elliRect.Contains(e))
         {
            ShowHideDock();
            handled = true;
            return;
         }

         int i;

         if (_icons != null)
         {
            for (i = 0; i < _icons.Length; i++)
            {
               if (_icons[i].Touching || _icons[i].HitTest(e))
               {
                  handled = true;
                  _icons[i].SendTouchUp(this, e);
               }
            }

            if (handled)
               return;
         }

         if (_expanded)
         {
            for (i = 0; i < _menus.Length; i++)
            {
               if (_menus[i].Touching)
               {
                  handled = true;
                  if (!_moved)
                  {
                     if (_menus[i].HitTest(e))
                        ShowHideDock();
                     _menus[i].SendTouchUp(this, e);
                  }
                  else
                  {
                     _menus[i].SendTouchUp(this, new point(_menus[i].ScreenBounds.X - 10, 0));
                     Invalidate();
                  }
                  return;
               }
               if (_menus[i].HitTest(e))
               {
                  // ReSharper disable once RedundantAssignment ??
                  handled = true;
                  if (_moved)
                  {
                     _menus[i].SendTouchUp(this, new point(_menus[i].ScreenBounds.X - 10, 0));
                  }
                  else
                  {
                     _menus[i].SendTouchUp(this, e);
                  }
               }
            }
         }
      }

      #endregion

      #region Public Methods

      public void AddIcon(AppbarIcon icon)
      {
         // Update Array Size
         if (_icons == null)
            _icons = new AppbarIcon[1];
         else
         {
            var tmp = new AppbarIcon[_icons.Length + 1];
            Array.Copy(_icons, tmp, _icons.Length);
            _icons = tmp;
         }

         // Update Owner
         if (icon.Owner != null)
            icon.Owner.RemoveIcon(icon);
         icon.Owner = this;

         // Assign Value
         _icons[_icons.Length - 1] = icon;

         Invalidate();
      }

      public void AddMenuItem(AppbarMenuItem menu)
      {
         if (_menus == null)
         {
            _menus = new[] { menu };
         }
         else
         {
            var tmp = new AppbarMenuItem[_menus.Length + 1];
            Array.Copy(_menus, tmp, _menus.Length);
            tmp[tmp.Length - 1] = menu;
            _menus = tmp;
         }

         if (_expanded)
         {
            Invalidate();
         }
      }

      public void ClearIcons()
      {
         if (_icons == null)
            return;

         _icons = null;
         Invalidate();
      }

      public void ClearMenuItems()
      {
         if (_menus == null)
            return;

         _menus = null;
         if (_expanded)
            Invalidate();
      }

      public AppbarIcon GetIconByName(string name)
      {
         if (_icons == null)
            return null;
         for (int i = 0; i < _icons.Length; i++)
         {
            if (_icons[i].Name == name)
               return _icons[i];
         }
         return null;
      }

      public AppbarMenuItem GetMenuByName(string name)
      {
         if (_menus == null)
            return null;
         for (int i = 0; i < _menus.Length; i++)
         {
            if (_menus[i].Name == name)
               return _menus[i];
         }
         return null;
      }

      public void RemoveIcon(AppbarIcon icon)
      {
         if (_icons == null)
            return;

         for (int i = 0; i < _icons.Length; i++)
         {
            if (_icons[i] == icon)
            {
               RemoveIconAt(i);
               return;
            }
         }
      }

      public void RemoveIconAt(int index)
      {
         if (_icons == null || index < 0 || index >= _icons.Length)
            return;

         if (_icons.Length == 1)
         {
            ClearIcons();
            return;
         }

         Suspended = true;
         _icons[index].Owner = null;

         var tmp = new AppbarIcon[_icons.Length - 1];
         int c = 0;
         for (int i = 0; i < _icons.Length; i++)
         {
            if (i != index)
               tmp[c++] = _icons[i];
         }

         _icons = tmp;
         Suspended = false;
      }

      public void RemoveMenuItem(AppbarMenuItem menu)
      {
         if (_menus == null)
            return;

         for (int i = 0; i < _menus.Length; i++)
         {
            if (_menus[i] == menu)
            {
               RemoveMenuItemAt(i);
               return;
            }
         }
      }

      public void RemoveMenuItemAt(int index)
      {
         if (_menus == null || index < 0 || index >= _menus.Length)
            return;

         if (_menus.Length == 1)
         {
            ClearMenuItems();
            return;
         }

         if (_expanded)
            Suspended = true;

         var tmp = new AppbarMenuItem[_menus.Length - 1];
         int c = 0;
         for (int i = 0; i < _menus.Length; i++)
         {
            if (i != index)
               tmp[c++] = _menus[i];
         }

         _menus = tmp;

         if (_expanded)
            Suspended = false;
      }

      #endregion

      #region GUI

      protected override void OnRender(int x, int y, int w, int h)
      {
         Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);

         switch (_dock)
         {
            case DockLocation.Bottom:
               RenderBottom(FontManager.ComputeExtentEx(_timeFont, "XX:XX PM").Width);
               break;
            case DockLocation.Left:
               RenderLeft(_timeFont.Height * 2);
               break;
            case DockLocation.Right:
               RenderRight(_timeFont.Height * 2);
               break;
            case DockLocation.Top:
               RenderTop(FontManager.ComputeExtentEx(_timeFont, "XX:XX PM").Width);
               break;
         }

      }

      private void RenderBottom(int tw)
      {
         int x = Left + 64;
         int y = Top;

         // Ellipsis
         if (_menus != null)
         {
            Core.Screen.DrawImage(Left + 7, Top + 7, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + 17, Top + 7, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + 27, Top + 7, _elli, 0, 0, 4, 4);
         }

         // Icons
         if (_icons != null)
         {
            if (_displayTime)
            {
               Core.Screen.SetClippingRectangle(Left + 64, Top, Width - 70 - tw, _dH);
               if (_farAlign)
                  x = Width - 8 - tw;
            }
            else
            {
               Core.Screen.SetClippingRectangle(Left + 64, Top, Width - 70, _dH);
               if (_farAlign)
                  x = Width - 8;
            }

            int i;
            for (i = 0; i < _icons.Length; i++)
            {
               if (_icons[i].Image != null)
               {
                  if (_farAlign)
                     x -= _icons[i].Width;

                  _icons[i].X = x;
                  _icons[i].Y = y + (_dH / 2 - _icons[i].Height / 2);

                  Core.Screen.DrawImage(x, _icons[i].Y, _icons[i].Image, 0, 0, _icons[i].Width, _icons[i].Height);

                  if (_farAlign)
                     x -= 8;
                  else
                     x += _icons[i].Width + 8;
               }
            }
         }

         // Time
         if (_displayTime)
         {
            Core.Screen.SetClippingRectangle(Left + Width - tw - 8, Top, tw, _dH);
            Core.Screen.DrawText(FormatTime(), _timeFont, _fore, Left + Width - tw - 8, Top + (_dH / 2 - _timeFont.Height / 2));
         }

         // Menu Items
         if (_expanded)
            DrawMenuItems();
      }

      private void RenderLeft(int tw)
      {
         int x = Left + _w - _dW;
         int y = Top + 64;


         // Ellipsis
         if (_menus != null)
         {
            Core.Screen.DrawImage(Left + _w - 11, Top + 7, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + _w - 11, Top + 17, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + _w - 11, Top + 27, _elli, 0, 0, 4, 4);
         }

         // Icons
         if (_icons != null)
         {
            if (_displayTime)
            {
               Core.Screen.SetClippingRectangle(x, Top + 64, _dW, Height - 70 - tw);
               if (_farAlign)
                  y = Height - 8 - tw;
            }
            else
            {
               Core.Screen.SetClippingRectangle(x, Top + 64, _dW, Height - 70);
               if (_farAlign)
                  y = Height - 8 - tw;
            }

            for (int i = 0; i < _icons.Length; i++)
            {
               if (_icons[i].Image != null)
               {
                  if (_farAlign)
                     y -= _icons[i].Height;

                  _icons[i].X = x + (_dW / 2 - _icons[i].Width / 2);
                  _icons[i].Y = y;

                  Core.Screen.DrawImage(_icons[i].X, _icons[i].Y, _icons[i].Image, 0, 0, _icons[i].Width, _icons[i].Height);
                  if (_farAlign)
                     y -= 8;
                  else
                     y += _icons[i].Height + 8;
               }
            }
         }

         // Time
         if (_displayTime)
         {
            Core.Screen.SetClippingRectangle(Left + 4 + _w - _dW, Top + _h - tw - 8, _dW - 8, tw);
            Core.Screen.DrawTextInRect(FormatTime(), Left + 4 + _w - _dW, Top + _h - tw - 8, _dW - 8, tw, Bitmap.DT_AlignmentCenter, _fore, _timeFont);
         }

         if (_expanded)
            DrawMenuItems();
      }

      private void RenderRight(int tw)
      {
         int x = Left;
         int y = Top + 64;


         // Ellipsis
         if (_menus != null)
         {
            Core.Screen.DrawImage(Left + 7, Top + 7, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + 7, Top + 17, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + 7, Top + 27, _elli, 0, 0, 4, 4);
         }

         // Icons
         if (_icons != null)
         {
            if (_displayTime)
            {
               Core.Screen.SetClippingRectangle(x, Top + 64, _dW, Height - 70 - tw);
               if (_farAlign)
                  y = Height - 8 - tw;
            }
            else
            {
               Core.Screen.SetClippingRectangle(x, Top + 64, _dW, Height - 70);
               if (_farAlign)
                  y = Height - 8 - tw;
            }

            for (int i = 0; i < _icons.Length; i++)
            {
               if (_icons[i].Image != null)
               {
                  if (_farAlign)
                     y -= _icons[i].Height;

                  _icons[i].X = x + (_dW / 2 - _icons[i].Width / 2);
                  _icons[i].Y = y;

                  Core.Screen.DrawImage(_icons[i].X, _icons[i].Y, _icons[i].Image, 0, 0, _icons[i].Width, _icons[i].Height);
                  if (_farAlign)
                     y -= 8;
                  else
                     y += _icons[i].Height + 8;
               }
            }
         }

         // Time
         if (_displayTime)
         {
            Core.Screen.SetClippingRectangle(Left + 4, Top + _h - tw - 8, _dW - 8, tw);
            Core.Screen.DrawTextInRect(FormatTime(), Left + 4, Top + _h - tw - 8, _dW - 8, tw, Bitmap.DT_AlignmentCenter, _fore, _timeFont);
         }

         if (_expanded)
            DrawMenuItems();
      }

      private void RenderTop(int tw)
      {
         int x = Left + 64;
         int y = Top;

         // Ellipsis
         if (_menus != null)
         {
            Core.Screen.DrawImage(Left + 7, Top + _h - 11, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + 17, Top + _h - 11, _elli, 0, 0, 4, 4);
            Core.Screen.DrawImage(Left + 27, Top + _h - 11, _elli, 0, 0, 4, 4);
         }

         // Icons
         if (_icons != null)
         {
            if (_displayTime)
            {
               Core.Screen.SetClippingRectangle(Left + 64, Top + _h - _dH, Width - 70 - tw, _dH);
               if (_farAlign)
                  x = Width - 8 - tw;
            }
            else
            {
               Core.Screen.SetClippingRectangle(Left + 64, Top + _h - _dH, Width - 70, _dH);
               if (_farAlign)
                  x = Width - 8;
            }

            for (int i = 0; i < _icons.Length; i++)
            {
               if (_icons[i].Image != null)
               {
                  if (_farAlign)
                     x -= _icons[i].Width;

                  _icons[i].X = x;
                  _icons[i].Y = y + _h - _dH + (_dH / 2 - _icons[i].Height / 2);

                  Core.Screen.DrawImage(x, _icons[i].Y, _icons[i].Image, 0, 0, _icons[i].Width, _icons[i].Height);

                  if (_farAlign)
                     x -= 8;
                  else
                     x += _icons[i].Width + 8;
               }
            }

         }

         // Time
         if (DisplayTime)
         {
            Core.Screen.SetClippingRectangle(Left + Width - tw - 8, Top + _h - _dH, tw, _dH);
            Core.Screen.DrawText(FormatTime(), _timeFont, _fore, Left + Width - tw - 8, Top + _h - _dH + (_dH / 2 - _timeFont.Height / 2));
         }

         // Menu Items
         if (_expanded)
            DrawMenuItems();
      }

      private void DrawMenuItems()
      {
         if (_menus == null)
            return;

         int y = _scrollArea.Y - _scrollValue;
         int x = _scrollArea.X;
         int i;

         Core.Screen.SetClippingRectangle(_scrollArea.X, _scrollArea.Y, _scrollArea.Width, _scrollArea.Height);

         for (i = 0; i < _menus.Length; i++)
         {
            if (_menus[i].Touching || _selIndex == i)
            {
               Core.Screen.DrawRectangle(0, 0, x, y - 2, _scrollArea.Width, _font.Height + 4, 0, 0,
                   Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);
               Core.Screen.DrawText(_menus[i].Text, _font, Core.SystemColors.SelectedFontColor, x, y + 2);
            }
            else
               Core.Screen.DrawText(_menus[i].Text, _font, _fore, x, y + 2);

            _menus[i].X = x;
            _menus[i].Y = y;
            if (_menus[i].Width == 0)
            {
               _menus[i].Width = _scrollArea.Width;
               _menus[i].Height = _font.Height;
            }
            y += _font.Height + 4;
         }

         // Scrolling
         if (_requiredDisplaySize > _scrollArea.Height)
         {
            Core.Screen.DrawRectangle(0, 0, _scrollArea.X + _scrollArea.Width - 4, _scrollArea.Y, 4, _scrollArea.Height, 0, 0,
                Core.SystemColors.ScrollbarBackground, 0, 0, Core.SystemColors.ScrollbarBackground, 0, 0, 128);

            i = (int)((_scrollArea.Height - _gripSize) * (_scrollValue / (float)_scrollValueRange));
            Core.Screen.DrawRectangle(0, 0, _scrollArea.X + _scrollArea.Width - 4, _scrollArea.Y + i, 4, _gripSize, 0, 0,
                Core.SystemColors.ScrollbarGrip, 0, 0, Core.SystemColors.ScrollbarGrip, 0, 0, 128);
         }
      }

      #endregion

      #region Private Methods

      private string FormatTime()
      {
         int h = DateTime.Now.Hour;
         string m = DateTime.Now.Minute.ToString();
         string ap = " AM";

         if (m.Length == 1)
            m = "0" + m;

         if (_timeFormat == TimeFormat.Hour12)
         {
            if (h > 12)
            {
               h -= 12;
               ap = " PM";
            }
            else if (h == 12)
               ap = " PM";
            else if (h == 0)
               h = 12;



            _min = DateTime.Now.Minute;
            return h + ":" + m + ap;
         }
         if (h < 10)
         {
            return "0" + h + ":" + m;
         }
         return h + ":" + m;
      }

      private void Metrics(int size)
      {
         switch (_dock)
         {
            case DockLocation.Bottom:
               _x = 0;
               _w = Core.ScreenWidth;
               _h = size;
               _y = Core.ScreenHeight - _h;
               break;
            case DockLocation.Left:
               _x = 0;
               _y = 0;
               _w = size;
               _h = Core.ScreenHeight;
               break;
            case DockLocation.Right:
               _x = Core.ScreenWidth - size;
               _y = 0;
               _w = size;
               _h = Core.ScreenHeight;
               break;
            default:
               _x = 0;
               _y = 0;
               _h = size;
               _w = Core.ScreenWidth;
               break;
         }

         _dH = _h;
         _dW = _w;
      }

      private void MonitorTime()
      {
         while (Parent != null)
         {
            if (_displayTime && DateTime.Now.Minute != _min)
            {
               _min = DateTime.Now.Minute;
               Invalidate();
            }

            Thread.Sleep(1000);
         }
      }

      private void ShowHideDock()
      {
         _selIndex = -1;
         _expanded = !_expanded;

         if (!_expanded)
         {
            UpdateDock();
            Parent.Invalidate();
            return;
         }

         int i, d;
         Parent.BringControlToFront(this);

         // Calculate size required to display all menus
         _requiredDisplaySize = (_font.Height + 4) * _menus.Length;

         switch (_dock)
         {
            case DockLocation.Bottom:

               // Calculate new height
               i = _dH + _requiredDisplaySize;

               // Restrict height to 1/2 parent height
               if (_dH + i > Parent.Height / 2)
                  i = Parent.Height / 2 - _dH;

               // Update Y & Height
               _h += i;
               _y -= i;

               // Define Scrollable Area
               _scrollArea = new rect(Left + 8, Top + _dH + 8, _w - 8, _h - _dH - 8);

               // Calculate Scroll Value Range
               _scrollValueRange = _requiredDisplaySize - _scrollArea.Height;

               // Calculate Grip Size
               if (_scrollValueRange > 0)
                  _gripSize = _scrollArea.Height / (_requiredDisplaySize / _scrollArea.Height);
               else
                  _gripSize = 8;

               // Enforce minimum grip size
               if (_gripSize < 8)
                  _gripSize = 8;

               // Update Ellipsis Rect
               _elliRect = new rect(Left, Top, 64, 64);
               break;
            case DockLocation.Left:

               // Calculate new width
               d = 0;
               for (i = 0; i < _menus.Length; i++)
               {
                  _gripSize = FontManager.ComputeExtentEx(_font, _menus[i].Text).Width;
                  if (_gripSize > d)
                     d = _gripSize;
               }
               i = _dW + d;

               // Restrict width to 1/2 parent width
               if (_dW + i > Parent.Width / 2)
                  i = Parent.Width / 2 - _dW;

               // Update Width
               _w += i;

               // Define Scrollable Area
               _scrollArea = new rect(Left + 8, Top + 8, _w - _dW - 8, _h - 16);

               // Calculate Scroll Value Range
               if (_scrollValueRange > 0)
                  _scrollValueRange = _requiredDisplaySize - _scrollArea.Height;
               else
                  _gripSize = 8;

               // Calculate Grip Size
               _gripSize = _scrollArea.Height / (_requiredDisplaySize / _scrollArea.Height);

               // Enforce minimum grip size
               if (_gripSize < 8)
                  _gripSize = 8;

               // Update Ellipsis Rect
               _elliRect = new rect(Left + _w - _dW, Top, 64, 64);

               break;
            case DockLocation.Right:
               // Calculate new width
               d = 0;
               for (i = 0; i < _menus.Length; i++)
               {
                  _gripSize = FontManager.ComputeExtentEx(_font, _menus[i].Text).Width;
                  if (_gripSize > d)
                     d = _gripSize;
               }
               i = _dW + d;

               // Restrict width to 1/2 parent width
               if (_dW + i > Parent.Width / 2)
                  i = Parent.Width / 2 - _dW;

               // Update X & Width
               _x -= i;
               _w += i;

               // Define Scrollable Area
               _scrollArea = new rect(Left + 8 + _dW, Top + 8, _w - _dW - 8, _h - 16);

               // Calculate Scroll Value Range
               _scrollValueRange = _requiredDisplaySize - _scrollArea.Height;

               // Calculate Grip Size
               if (_scrollValueRange > 0)
                  _gripSize = _scrollArea.Height / (_requiredDisplaySize / _scrollArea.Height);
               else
                  _gripSize = 8;

               // Enforce minimum grip size
               if (_gripSize < 8)
                  _gripSize = 8;

               // Update Ellipsis Rect
               _elliRect = new rect(Left, Top, 64, 64);
               break;
            default:

               // Calculate new height
               i = _dH + _requiredDisplaySize;

               // Restrict height to 1/2 parent height
               if (_dH + i > Parent.Height / 2)
                  i = Parent.Height / 2 - _dH;

               // Update Height
               _h += i;

               // Define Scrollable Area
               _scrollArea = new rect(Left + 8, Top + 8, _w - 8, _h - _dH - 8);

               // Calculate Scroll Value Range
               _scrollValueRange = _requiredDisplaySize - _scrollArea.Height;

               // Calculate Grip Size
               if (_scrollValueRange > 0)
                  _gripSize = _scrollArea.Height / (_requiredDisplaySize / _scrollArea.Height);
               else
                  _gripSize = 8;

               // Enforce minimum grip size
               if (_gripSize < 8)
                  _gripSize = 8;

               // Update Ellipsis Rect
               _elliRect = new rect(Left, Top + _h - 64, 64, 64);
               break;
         }

         _scrollValue = 0;
         Invalidate();
      }

      private void UpdateDock()
      {
         switch (_dock)
         {
            case DockLocation.Bottom:
               _x = 0;
               _w = Core.ScreenWidth;
               _h = _dH;
               _y = Core.ScreenHeight - _dH;
               break;
            case DockLocation.Left:
               _x = 0;
               _y = 0;
               _w = _dW;
               _h = Core.ScreenHeight;
               break;
            case DockLocation.Right:
               _x = Core.ScreenWidth - _dW;
               _y = 0;
               _w = _dW;
               _h = Core.ScreenHeight;
               break;
            default:
               _x = 0;
               _y = 0;
               _h = _dH;
               _w = Core.ScreenWidth;
               break;
         }
         _elliRect = new rect(Left, Top, 64, 64);
      }

      private void UpdateEllipsis()
      {
         _elli.DrawRectangle(0, 0, 0, 0, 4, 4, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);
         _elli.DrawRectangle(0, 0, 1, 0, 1, 4, 0, 0, _fore, 0, 0, _fore, 0, 0, 256);
         _elli.DrawRectangle(0, 0, 2, 1, 1, 2, 0, 0, _fore, 0, 0, _fore, 0, 0, 256);
         _elli.DrawRectangle(0, 0, 0, 0, 1, 1, 0, 0, _fore, 0, 0, _fore, 0, 0, 35);
         _elli.DrawRectangle(0, 0, 0, 3, 1, 1, 0, 0, _fore, 0, 0, _fore, 0, 0, 35);
         _elli.DrawRectangle(0, 0, 0, 1, 1, 2, 0, 0, _fore, 0, 0, _fore, 0, 0, 199);
         _elli.DrawRectangle(0, 0, 3, 0, 1, 1, 0, 0, _fore, 0, 0, _fore, 0, 0, 35);
         _elli.DrawRectangle(0, 0, 3, 3, 1, 1, 0, 0, _fore, 0, 0, _fore, 0, 0, 35);
         _elli.DrawRectangle(0, 0, 2, 0, 1, 1, 0, 0, _fore, 0, 0, _fore, 0, 0, 199);
         _elli.DrawRectangle(0, 0, 2, 3, 1, 1, 0, 0, _fore, 0, 0, _fore, 0, 0, 199);
         _elli.DrawRectangle(0, 0, 2, 3, 1, 1, 0, 0, _fore, 0, 0, _fore, 0, 0, 199);
         _elli.DrawRectangle(0, 0, 3, 1, 1, 2, 0, 0, _fore, 0, 0, _fore, 0, 0, 199);
      }

      #endregion

   }
}
