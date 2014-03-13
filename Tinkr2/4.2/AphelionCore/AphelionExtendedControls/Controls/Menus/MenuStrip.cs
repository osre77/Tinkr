using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class MenuStrip : Control
   {

      #region Variables

      private MenuItem[] _items;
      private Font _font;

      #endregion

      #region Constructors

      public MenuStrip(string name, Font font, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;
      }

      public MenuStrip(string name, Font font, MenuItem[] items, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _items = items;
         _font = font;
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
            Invalidate();
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
            _items = new[] { value };
         else
         {
            var tmp = new MenuItem[_items.Length + 1];
            Array.Copy(_items, tmp, _items.Length);
            tmp[tmp.Length - 1] = value;
            _items = tmp;
         }

         if (Parent != null)
            Parent.Render(ScreenBounds, true);
      }

      public void AddMenuItems(MenuItem[] values)
      {
         if (_items == null)
            _items = values;
         else
         {
            var tmp = new MenuItem[_items.Length + values.Length];
            Array.Copy(_items, tmp, _items.Length);
            Array.Copy(values, 0, tmp, 0, values.Length);
         }

         if (Parent != null)
            Parent.Render(ScreenBounds, true);
      }

      public void ClearMenuItems()
      {
         if (_items == null)
            return;
         _items = null;
         if (Parent != null)
            Parent.Render(ScreenBounds, true);
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
            tmp[i] = _items[i];

         // Insert
         tmp[index] = value;

         // Copy lower
         for (i = index; i < _items.Length; i++)
            tmp[i + 1] = _items[i];

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
      /// <param name="item"></param>
      public void RemoveMenuItem(MenuItem item)
      {
         if (_items == null)
            return;

         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i] == item)
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

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         if (_items == null)
            return;

         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i].ScreenBounds.Contains(e))
            {
               Parent.Suspended = true;
               _items[i].SendTouchDown(this, e);
               Parent.Suspended = false;
               return;
            }
         }
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         if (_items == null)
            return;

         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i].ScreenBounds.Contains(e) && _items[i].Touching)
            {
               if (_items[i].Items != null)
               {
                  var cm = new ContextMenu(Name + "_cm", _font, _items[i].Items);
                  cm.LostFocus += s => Collapse();
                  _items[i].Expanded = true;
                  cm.Show(Parent, _items[i].ScreenBounds.X, Top + Height);
               }
               else
               {
                  _items[i].SendTouchUp(this, e);
                  Invalidate();
               }
            }
            else if (_items[i].Touching)
            {
               _items[i].SendTouchUp(this, e);
               Invalidate();
            }
         }
      }

      #endregion

      #region GUI

      private void Collapse()
      {
         Parent.TopLevelContainer.Suspended = true;
         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i].Expanded)
            {
               _items[i].SendTouchUp(this, new point(_items[i].X - 1, 0));
               _items[i].Expanded = false;
            }
         }
         Parent.TopLevelContainer.Suspended = false;
      }

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int w, int h)
      // ReSharper restore RedundantAssignment
      {
         Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, Left, Top, Width, Height, 0, 0, Core.SystemColors.ControlTop, Left, Top,
             Core.SystemColors.ControlBottom, Left + Width, Top + Height, 256);

         if (_items == null || _items.Length == 0)
            return;

         x = Left + 4;
         y = Top + (Height / 2 - _font.Height / 2);

         for (int i = 0; i < _items.Length; i++)
         {
            w = FontManager.ComputeExtentEx(_font, _items[i].Text).Width;
            _items[i].X = x - 4;
            _items[i].Y = Top;
            _items[i].Height = Height;
            _items[i].Width = w + 8;
            _items[i].Parent = Parent;
            if (_items[i].Touching || _items[i].Expanded)
            {
               Core.Screen.DrawRectangle(0, 0, x - 4, Top + 1, w + 8, Height - 2, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);
               Core.Screen.DrawText(_items[i].Text, _font, Core.SystemColors.SelectedFontColor, x, y);
            }
            else
               Core.Screen.DrawText(_items[i].Text, _font, Core.SystemColors.FontColor, x, y);
            x += w + 9;
         }

      }

      #endregion

   }
}
