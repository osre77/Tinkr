using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class SlidePanelDialog : Container
   {

      #region Variables

      private int _selIndex;
      private int _selDown;
      private Font _font;
      private Color _fore;
      private bool _shadow;

      private Bitmap _active;
      private Bitmap _inactive;
      private bool _ani;

      #endregion

      #region Constructors

      public SlidePanelDialog(string name, Font font, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;

         DefaultColors();
      }

      public SlidePanelDialog(string name, Font font, int x, int y, int width, int height, SlidePanel[] panels)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         _font = font;

         for (int i = 0; i < panels.Length; i++)
         {
            AddChild(panels[i]);
         }
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         DefaultColors();
      }

      public SlidePanelDialog(string name, Font font, int x, int y, int width, int height, bool shadow)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;
         _shadow = shadow;

         DefaultColors();
      }

      public SlidePanelDialog(string name, Font font, int x, int y, int width, int height, SlidePanel[] panels, bool shadow)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         _font = font;
         _shadow = shadow;

         for (int i = 0; i < panels.Length; i++)
         {
            AddChild(panels[i]);
         }
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         DefaultColors();
      }

      private void DefaultColors()
      {
         _fore = Core.SystemColors.FontColor;
         _active = Resources.GetBitmap(Resources.BitmapResources.active_cir);
         _inactive = Resources.GetBitmap(Resources.BitmapResources.inactive_cir);
         _ani = true;
      }

      #endregion

      #region Properties

      public bool AnimateSlide
      {
         get { return _ani; }
         set { _ani = value; }
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

      public Color ForeColor
      {
         get { return _fore; }
         set
         {
            if (_fore == value)
               return;
            _fore = value;
            Invalidate();
         }
      }

      public int SelectedIndex
      {
         get { return _selIndex; }
         set
         {
            if (Children == null || value < 0 || value > Children.Length || _selIndex == value)
               return;

            if (_ani)
               AniSwitch(_selIndex, value);

            _selIndex = value;
            Invalidate();
         }
      }

      public bool ShadowDialog
      {
         get { return _shadow; }
         set
         {
            if (_shadow == value)
               return;
            _shadow = value;
            Invalidate();
         }
      }

      #endregion

      #region Button Methods

      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int)ButtonIDs.Left)
         {
            int newSel = _selIndex - 1;
            if (newSel < 0)
               newSel = Children.Length - 1;
            SelectedIndex = newSel;

            handled = true;
            return;
         }
         if (buttonId == (int)ButtonIDs.Right)
         {
            int newSel = _selIndex + 1;
            if (newSel > Children.Length - 1)
               newSel = 0;
            SelectedIndex = newSel;

            handled = true;
            return;
         }

         if (Children != null)
         {
            if (ActiveChild != null)
               ActiveChild.SendButtonEvent(buttonId, true);
            else
            {
               for (int i = 0; i < Children.Length; i++)
               {
                  if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                  {
                     handled = true;
                     Children[i].SendButtonEvent(buttonId, true);
                     break;
                  }
               }
            }
         }
      }

      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         if (Children != null)
         {
            if (ActiveChild != null)
               ActiveChild.SendButtonEvent(buttonId, false);
            else
            {
               for (int i = 0; i < Children.Length; i++)
               {
                  if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                  {
                     handled = true;
                     Children[i].SendButtonEvent(buttonId, false);
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
         {
            handled = true;
            ActiveChild.SendKeyboardAltKeyEvent(key, pressed);
         }
         else if (pressed)
         {
            if (key == 80)
            {
               // Left
               int newSel = _selIndex - 1;
               if (newSel < 0)
                  newSel = Children.Length - 1;
               SelectedIndex = newSel;

               handled = true;
            }
            else if (key == 79)
            {
               // Right
               int newSel = _selIndex + 1;
               if (newSel > Children.Length - 1)
                  newSel = 0;
               SelectedIndex = newSel;

               handled = true;
            }
         }
         base.KeyboardAltKeyMessage(key, pressed, ref handled);
      }

      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         if (ActiveChild != null)
         {
            handled = true;
            ActiveChild.SendKeyboardKeyEvent(key, pressed);
         }
         base.KeyboardKeyMessage(key, pressed, ref handled);
      }

      #endregion

      #region Touch Methods

      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         // Check Controls
         if (Children != null)
         {

            _selDown = -1;

            // Check Tabs
            for (int i = 0; i < Children.Length; i++)
            {
               var tab = (SlidePanel)Children[i];
               if (tab.DispRect.Contains(point))
               {
                  _selDown = i;
                  break;
               }
            }

            if (Children[_selIndex].Visible && Children[_selIndex].HitTest(point))
            {
               ActiveChild = Children[_selIndex];

               Children[_selIndex].SendTouchDown(this, point);
               return;
            }
         }

         ActiveChild = null;
      }

      protected override void TouchGestureMessage(object sender, TouchType type, float force, ref bool handled)
      {
         if (Children == null)
            return;

         if (type == TouchType.GestureRight)
         {
            int newSel = _selIndex - 1;
            if (newSel < 0)
               newSel = Children.Length - 1;
            SelectedIndex = newSel;
         }
         else if (type == TouchType.GestureLeft)
         {
            int newSel = _selIndex + 1;
            if (newSel > Children.Length - 1)
               newSel = 0;
            SelectedIndex = newSel;
         }

         if (ActiveChild != null)
         {
            handled = true;
            ActiveChild.SendTouchGesture(sender, type, force);
         }
      }

      protected override void TouchMoveMessage(object sender, point point, ref bool handled)
      {
         // Check Controls
         if (ActiveChild != null && ActiveChild.Touching)
         {
            ActiveChild.SendTouchMove(this, new point(point.X - ActiveChild.Left, point.Y - ActiveChild.Top));
         }
         else if (Children != null)
         {
            if (Children[_selIndex].HitTest(point))
            {
               Children[_selIndex].SendTouchMove(this, point);
            }
         }
         base.TouchMoveMessage(sender, point, ref handled);
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         //bool ret = false;
         //bool ignoreUp = false;

         // Check Controls
         if (Children != null)
         {
            if (_selDown != -1)
            {
               var tab = (SlidePanel)Children[_selDown];
               if (tab.DispRect.Contains(point))
               {
                  SelectedIndex = _selDown;
               }
            }
            else
            {
               try
               {
                  if (Children[_selIndex].HitTest(point))// && !ignoreUp && !ret)
                  {
                     //ret = true;
                     Children[_selIndex].SendTouchUp(this, point);
                  }
                  else if (Children[_selIndex].Touching)
                  {
                     Children[_selIndex].SendTouchUp(this, point);
                  }
               }
               // ReSharper disable once EmptyGeneralCatchClause
               catch
               {
                  // This can happen if the user clears the Form during a tap
               }
            }
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region GUI

      private void AniSwitch(int startingIndex, int endingIndex)
      {
         int x = Left;
         int y = Top;
         int w = Width;
         int h = Height;
         int v = 0;
         int xy = 0;

         if (_shadow)
         {
            x += 4;
            y += 4;
            w -= 8;
            h -= 8;
            xy = 4;
         }

         h -= _inactive.Height + _font.Height + 38;

         lock (Core.Screen)
         {
            Core.Screen.SetClippingRectangle(x, y, w, h);
            var bmp2 = new Bitmap(w, h);
            var sp = (SlidePanel)Children[endingIndex];
            sp.X = xy;
            sp.Y = xy;
            sp.Width = w;
            sp.Height = h;
            Children[endingIndex].Render();
            bmp2.DrawImage(0, 0, Core.Screen, x, y, w, h);
            Children[startingIndex].Render();

            if (startingIndex < endingIndex)
            {
               while (v < w)
               {
                  Core.Screen.SetClippingRectangle(x, y, w, h);
                  Core.Screen.DrawImage(x + w - v, y, bmp2, 0, 0, v, h);
                  Core.Screen.Flush(x, y, w, h);
                  v += 10;
                  if (v > w)
                     v = w;
               }
            }
            else
            {
               while (v < w)
               {
                  Core.Screen.SetClippingRectangle(x, y, w, h);
                  Core.Screen.DrawImage(x, y, bmp2, w - v, 0, v, h);
                  Core.Screen.Flush(x, y, w, h);
                  v += 10;
                  if (v > w)
                     v = w;
               }
            }

            bmp2.Dispose();
         }
         Debug.GC(true);
      }

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int width, int height)
      // ReSharper restore RedundantAssignment
      {
         if (_shadow)
         {
            x = Left + 4;
            y = Top + 4;
            width = Width - 8;
            height = Height - 8;
            Core.ShadowRegion(x, y, width, height);
         }
         else
         {
            x = Left;
            y = Top;
            width = Width;
            height = Height;
         }

         Core.Screen.DrawRectangle(0, 0, x, y, width, height, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 256);

         if (Children != null)
         {
            var sp = (SlidePanel)Children[_selIndex];
            int i;
            int btmHeight = _inactive.Height + _font.Height + 38;

            // Draw text
            int tY = y + height - _inactive.Height - 28 - _font.Height;
            Core.Screen.DrawTextInRect(sp.Title, x + 4, tY, width - 8, _font.Height, Bitmap.DT_AlignmentCenter, _fore, _font);
            tY += _font.Height + 20;

            // Draw pips
            int tX = x + (width / 2 - (Children.Length * (_active.Width + 5)) / 2);
            for (i = 0; i < Children.Length; i++)
            {
               if (i == _selIndex)
                  Core.Screen.DrawImage(tX, tY, _active, 0, 0, _active.Width, _active.Height);
               else
                  Core.Screen.DrawImage(tX, tY, _inactive, 0, 0, _inactive.Width, _inactive.Height);

               ((SlidePanel)Children[i]).DispRect = new rect(tX - 5, tY - 5, _active.Width + 10, _active.Height + 10);
               tX += _inactive.Width + 10;

            }

            if (!_shadow)
            {
               sp.X = 0;
               sp.Y = 0;
            }
            else
            {
               sp.X = 4;
               sp.Y = 4;
            }
            sp.Width = width;
            sp.Height = height - btmHeight;
            sp.BeingDisplayed = true;
            sp.Render();
         }
      }

      #endregion

   }
}
