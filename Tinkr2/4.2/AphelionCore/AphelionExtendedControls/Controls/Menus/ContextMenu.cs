using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class ContextMenu : Control
   {

      #region Variables

      private Font _font;
      private MenuItem[] _items;

      private readonly Bitmap _right = Resources.GetBitmap(Resources.BitmapResources.right);

      #endregion

      #region Constructors

      public ContextMenu(string name, Font font)
      {
         Name = name;
         _font = font;
      }

      public ContextMenu(string name, Font font, MenuItem[] menuItems)
      {
         Name = name;
         _font = font;
         _items = menuItems;
      }

      #endregion

      #region Properties

      public Font Font
      {
         get { return _font; }
         set
         {
            if (_font == value)
               return;
            _font = value;
            Core.ActiveContainer.Invalidate();
         }
      }

      public MenuItem[] MenuItems
      {
         get { return _items; }
      }

      #endregion

      #region Public Methods

      public void AddMenuItem(MenuItem value)
      {
         // Update Array Size
         if (_items == null)
         {
            _items = new[] { value };
         }
         else
         {
            var tmp = new MenuItem[_items.Length + 1];
            Array.Copy(_items, tmp, _items.Length);
            tmp[tmp.Length - 1] = value;
            _items = tmp;
         }

         if (Parent != null)
         {
            Parent.Render(ScreenBounds, true);
         }
      }

      public void AddMenuItems(MenuItem[] values)
      {
         if (_items == null)
         {
            _items = values;
         }
         else
         {
            var tmp = new MenuItem[_items.Length + values.Length];
            Array.Copy(_items, tmp, _items.Length);
            Array.Copy(values, 0, tmp, 0, values.Length);
         }

         if (Parent != null)
         {
            Parent.Render(ScreenBounds, true);
         }
      }

      public void ClearMenuItems()
      {
         if (_items == null)
         {
            return;
         }
         _items = null;
         if (Parent != null)
         {
            Parent.Render(ScreenBounds, true);
         }
      }

      public void InsertMenuItemAt(MenuItem value, int index)
      {
         if (_items == null && index == 0)
         {
            AddMenuItem(value);
            return;
         }

         if (_items == null || index < 0 || index > _items.Length)
            throw new ArgumentOutOfRangeException();

         var tmp = new MenuItem[_items.Length + 1];

         int i;

         // Copy upper
         for (i = 0; i < index; i++)
         {
            tmp[i] = _items[i];
         }

         // Insert
         tmp[index] = value;

         // Copy lower
         for (i = index; i < _items.Length; i++)
         {
            tmp[i + 1] = _items[i];
         }

         // Update
         _items = tmp;
         if (Parent != null)
         {
            Parent.Render(ScreenBounds, true);
         }
      }

      /// <summary>
      /// Removes item by value
      /// </summary>
      /// <param name="value"></param>
      public void RemoveMenuItem(MenuItem value)
      {
         if (_items == null)
            return;

         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i] == value)
            {
               RemoveMenuItemAt(i);
               return;
            }
         }
      }

      /// <summary>
      /// Removes item at specific point in array
      /// </summary>
      /// <param name="index"></param>
      public void RemoveMenuItemAt(int index)
      {
         if (_items.Length == 1)
         {
            ClearMenuItems();
            return;
         }

         var tmp = new MenuItem[_items.Length - 1];
         int c = 0;
         for (int i = 0; i < _items.Length; i++)
         {
            if (i != index)
               tmp[c++] = _items[i];
         }

         _items = tmp;
         if (Parent != null)
         {
            Parent.Render(ScreenBounds, true);
         }
      }

      public void Show(IContainer parent, int x, int y, bool bottomAlign = false)
      {
         int w = 0;
         int i;
         bool addSubs = false;

         // Check for empty or bad parent
         if (_items == null || parent.TopLevelContainer != Core.ActiveContainer)
            return;

         // Calculate Size
         int h = GetMenuItem(_items);
         for (i = 0; i < _items.Length; i++)
         {
            int j = FontManager.ComputeExtentEx(_font, _items[i].Text).Width;
            if (_items[i].Length > 0)
            {
               addSubs = true;
            }
            if (j > w)
            {
               w = j;
            }
         }
         w += (addSubs) ? 40 : 8;

         // Update Position
         if (bottomAlign)
         {
            y -= h;
         }

         if (y + h > Core.Screen.Height)
         {
            y = y - h;
         }
         if (y < 0)
         {
            y = 0;
         }
         if (x + w > Core.Screen.Width)
         {
            x = x - w;
         }
         if (x < 0)
         {
            x = 0;
         }

         X = x;
         Y = y;
         Width = w;
         Height = h;

         if (!Visible)
         {
            Visible = true;
         }

         parent.AddChild(this);
         parent.ActiveChild = this;
      }

      #endregion

      #region Focus

      public override void Blur()
      {
         if (Parent == null)
            return;

         IContainer ic = Parent;
         //ic.Suspended = true;
         ic.RemoveChild(this);

         // Collapse all menus
         if (_items != null)
         {
            for (int i = 0; i < _items.Length; i++)
            {
               if (_items[i].Expanded)
               {
                  _items[i].Expanded = false;
                  _items[i].SendTouchUp(null, new point(-1, -1));
                  BlurChildren(_items[i]);
               }
            }
         }

         OnLostFocus(this);
         //ic.QuiteUnsuspend();
         Core.ActiveContainer.Invalidate();
      }

      private void BlurChildren(MenuItem mnu)
      {
         for (int i = 0; i < mnu.Items.Length; i++)
         {
            if (mnu.Items[i].Expanded)
            {
               mnu.Items[i].Expanded = false;
               BlurChildren(mnu.Items[i]);
            }
         }
      }

      public override bool HitTest(point point)
      {
         // Check Main Bounds
         if (ScreenBounds.Contains(point))
            return true;

         // Check Expanded Menus
         if (_items != null)
         {
            for (int i = 0; i < _items.Length; i++)
            {
               if (_items[i].Expanded && _items[i].ExpandedBounds.Contains(point))
               {
                  return true;
               }
               if (_items[i].Expanded && _items.Length > 0 && HitTestSub(_items[i], point))
               {
                  return true;
               }
            }
         }

         return false;
      }

      private bool HitTestSub(MenuItem mnu, point e)
      {
         for (int i = 0; i < mnu.Items.Length; i++)
         {
            if (mnu.Items[i].Expanded && mnu.Items[i].ExpandedBounds.Contains(e))
            {
               return true;
            }
            if (mnu.Items[i].Expanded && mnu.Items.Length > 0 && HitTestSub(mnu.Items[i], e))
            {
               return true;
            }
         }

         return false;
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         if (_items == null)
            return;

         for (int i = 0; i < _items.Length; i++)
         {

            if (_items[i].Expanded && _items.Length > 0)
            {
               // Check subnodes first
               if (SendTouchDownSub(_items[i], point))
                  return;
            }
            else if (_items[i].ScreenBounds.Contains(point))
            {
               Parent.Suspended = true;
               _items[i].SendTouchDown(this, point);
               CollapseOnLne(_items, i);
               Parent.Suspended = false;
               return;
            }
         }

      }

      private bool SendTouchDownSub(MenuItem mnu, point e)
      {
         for (int i = 0; i < mnu.Items.Length; i++)
         {
            if (mnu.Items[i].Expanded && mnu.Items[i].Length > 0)
            {
               if (SendTouchDownSub(mnu.Items[i], e))
               {
                  return true;
               }
               mnu.Items[i].Expanded = false;
            }
            else if (mnu.Items[i].ScreenBounds.Contains(e))
            {
               mnu.Items[i].SendTouchDown(this, e);
               CollapseSubsFrom(mnu.Items, i + 1);
               Parent.Invalidate();
               return true;
            }
         }

         return false;
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (_items != null)
         {
            for (int i = 0; i < _items.Length; i++)
            {
               if (_items[i].Expanded && _items.Length > 0)
               {
                  if (SendTouchUpSub(_items[i], point))
                  {
                     Blur();
                     _items[i].Expanded = false;
                     break;
                  }

                  //_items[i].SetExpanded(false);
               }
               else if (_items[i].ScreenBounds.Contains(point))
               {
                  if (_items[i].Touching)
                  {
                     if (_items[i].Items != null)
                     {
                        _items[i].SetExpanded(!_items[i].Expanded);
                        Parent.Invalidate();
                     }
                     else
                     {
                        Blur();
                        _items[i].SendTouchUp(this, point);
                     }

                     break;
                  }
                  Invalidate(ScreenBounds);

                  _items[i].SendTouchUp(this, point);
                  break;
               }
               else if (_items[i].Touching)
               {
                  _items[i].SendTouchUp(this, point);
                  Invalidate();
               }
            }
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      private bool SendTouchUpSub(MenuItem mnu, point e)
      {
         for (int i = 0; i < mnu.Items.Length; i++)
         {
            if (mnu.Items[i].Expanded && mnu.Items.Length > 0)
            {
               if (SendTouchUpSub(mnu.Items[i], e))
                  return true;
            }
            else if (mnu.Items[i].ScreenBounds.Contains(e))
            {
               mnu.Items[i].SendTouchUp(this, e);
               if (mnu.Items[i].Touching)
               {
                  if (mnu.Items[i].Items != null)
                  {
                     mnu.Items[i].Expanded = !mnu.Items[i].Expanded;
                     Parent.Invalidate();
                  }
                  else
                     Blur();
               }
               else
                  Parent.Invalidate();
               return true;
            }
         }

         return false;
      }

      #endregion

      #region GUI

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int width, int height)
      // ReSharper restore RedundantAssignment
      {
         Core.Screen.SetClippingRectangle(0, 0, Core.ScreenWidth, Core.ScreenHeight);

         Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, Left, Top, Width, Height, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlBottom, Left + Width, Top + Height, 256);

         // Draw Items
         y = Top + 4;
         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i].Visible)
            {
               if (_items[i].Touching || _items[i].Expanded)
               {
                  Core.Screen.DrawRectangle(0, 0, Left + 1, y - 3, Width - 2, _font.Height + 8, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);
                  Core.Screen.DrawTextInRect(_items[i].Text, Left + 4, y, Width - 8, _font.Height, Bitmap.DT_AlignmentLeft, Core.SystemColors.SelectedFontColor, _font);
               }
               else
                  Core.Screen.DrawTextInRect(_items[i].Text, Left + 4, y, Width - 8, _font.Height, Bitmap.DT_AlignmentLeft, (_items[i].Enabled) ? Core.SystemColors.FontColor : Colors.DarkGray, _font);

               _items[i].X = Left;
               _items[i].Y = y;
               _items[i].Width = Width - 2;
               _items[i].Height = _font.Height + 8;
               _items[i].Parent = Parent;

               if (_items[i].Length > 0)
                  Core.Screen.DrawImage(Left + Width - 10, y + _font.Height / 2 - 2, _right, 0, 0, 4, 5);

               if (_items[i].Expanded)
                  DrawExpandedMenu(_items[i], Left + Width - 1, y - 4, Width);

               y += _font.Height + 5;
               if (i != _items.Length - 1)
                  Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, Left + 1, y, Left + Width - 2, y);
               y += 4;
            }
         }
      }

      private void DrawExpandedMenu(MenuItem mnu, int x, int y, int ownerWidth)
      {
         int w = 0;
         int i;
         bool addSubs = false;

         //_iSel = -1;

         // Calculate Size
         int h = GetMenuItem(mnu.Items);
         for (i = 0; i < mnu.Items.Length; i++)
         {
            int j = FontManager.ComputeExtentEx(_font, mnu.Items[i].Text).Width;
            if (mnu.Items[i].Length > 0)
            {
               addSubs = true;
            }
            if (j > w)
            {
               w = j;
            }
         }
         w += (addSubs) ? 40 : 8;

         // Update Position
         if (y + h > Core.Screen.Height)
            y = y - h;
         if (y < 0)
            y = 0;
         if (x + w > Core.Screen.Width)
            x = x - w - ownerWidth + 3;
         if (x < 0)
            x = 0;

         mnu.ExpandedBounds = new rect(x, y, w, h);
         Core.Screen.DrawRectangle(Colors.DarkGray, 1, x, y, w - 1, h, 0, 0, Core.SystemColors.ControlTop, x, y, Core.SystemColors.ControlBottom, x + w, y + h, 256);

         // Draw Items
         y += 4;
         for (i = 0; i < mnu.Items.Length; i++)
         {
            if (mnu.Items[i].Visible)
            {
               if (mnu.Items[i].Touching || mnu.Items[i].Expanded)
               {
                  Core.Screen.DrawRectangle(0, 0, x + 1, y - 3, w - 2, _font.Height + 8, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);
                  Core.Screen.DrawTextInRect(mnu.Items[i].Text, x + 4, y, w - 8, _font.Height, Bitmap.DT_AlignmentLeft, Core.SystemColors.SelectedFontColor, _font);
               }
               else
                  Core.Screen.DrawTextInRect(mnu.Items[i].Text, x + 4, y, w - 8, _font.Height, Bitmap.DT_AlignmentLeft, (mnu.Items[i].Enabled) ? Core.SystemColors.FontColor : Colors.DarkGray, _font);

               mnu.Items[i].X = x;// 1;
               mnu.Items[i].Y = y; // y; // y - y;
               mnu.Items[i].Width = w - 2;
               mnu.Items[i].Height = _font.Height + 8;
               mnu.Items[i].Parent = Parent;

               if (mnu.Items[i].Length > 0)
                  Core.Screen.DrawImage(x + w - 10, y + _font.Height / 2 - 2, _right, 0, 0, 4, 5);

               if (mnu.Items[i].Expanded)
               {
                  DrawExpandedMenu(mnu.Items[i], x + w - 1, y - 4, w);
               }

               y += _font.Height + 5;
               if (i != mnu.Items.Length - 1)
               {
                  Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, x + 1, y, x + w - 2, y);
               }
               y += 4;
            }
         }
      }

      #endregion

      #region Private Methods

      private void CollapseOnLne(MenuItem[] items, int exclude)
      {
         for (int i = 0; i < items.Length; i++)
         {
            if (items[i].Expanded && i != exclude)
            {
               items[i].Expanded = false;
               if (_items[i].Touching)
                  _items[i].SendTouchUp(null, new point(_items[i].Left - 1, 0));
            }
         }
      }

      private void CollapseSubsFrom(MenuItem[] items, int index)
      {
         for (int i = index; i < items.Length; i++)
            items[i].Expanded = false;
      }

      private int GetMenuItem(MenuItem[] items)
      {
         int c = 0;
         for (int i = 0; i < items.Length; i++)
         {
            if (items[i].Visible)
               c++;
         }
         return 1 + ((_font.Height + 9) * c);
      }

      #endregion

   }
}
