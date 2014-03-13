using System;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class MenuItem : Control
   {
      #region Variables

      private bool _expanded;
      private string _text;

      private rect _expandedBounds;
      private MenuItem[] _items;

      #endregion

      #region Constructor

      public MenuItem(string name, string text)
      {
         Name = name;
         _text = text;
      }

      #endregion

      #region Properties

      internal rect ExpandedBounds
      {
         get { return _expandedBounds; }
         set { _expandedBounds = value; }
      }

      internal void SetExpanded(bool value)
      {
         _expanded = value;
      }

      public bool Expanded
      {
         get { return _expanded; }
         set
         {
            if (_items == null || _items.Length == 0)
               return;

            _expanded = value;

            if (Parent != null)
            {
               Parent.TopLevelContainer.Invalidate();
            }
         }
      }

      public int Length
      {
         get
         {
            if (_items == null)
               return 0;
            return _items.Length;
         }
      }

      public MenuItem[] Items
      {
         get { return _items; }
      }

      public string Text
      {
         get { return _text; }
         set
         {
            _text = value;
         }
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

         Invalidate();
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

         Invalidate();
      }

      public void ClearMenuItems()
      {
         if (_items == null)
            return;
         _items = null;
         Invalidate();
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
            tmp[i - 1] = _items[i];

         // Update
         _items = tmp;
         Invalidate();
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
            if (ReferenceEquals(_items[i], item))
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
         Invalidate();
      }

      #endregion

   }
}
