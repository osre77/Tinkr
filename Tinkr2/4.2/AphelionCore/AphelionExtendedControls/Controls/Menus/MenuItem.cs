using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class MenuItem : Control
    {

        #region Variables

        protected internal bool _expanded;
        private string _text;

        protected internal rect ExpandedBounds;
        protected internal MenuItem[] _items;

        #endregion

        #region Constructor

        public MenuItem(string name, string text)
        {
            Name = name;
            _text = text;
        }

        #endregion

        #region Properties

        public bool Expanded
        {
            get { return _expanded; }
            set 
            {
                if (_items == null || _items.Length == 0)
                    return;

                _expanded = value;

                if (Parent != null)
                    Parent.TopLevelContainer.Invalidate();
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

        public void AddMenuItem(MenuItem Value)
        {
            // Update Array Size
            if (_items == null)
                _items = new MenuItem[] { Value };
            else
            {
                MenuItem[] tmp = new MenuItem[_items.Length + 1];
                Array.Copy(_items, tmp, _items.Length);
                tmp[tmp.Length - 1] = Value;
                _items = tmp;
            }

            Invalidate();
        }

        public void AddMenuItems(MenuItem[] Values)
        {
            if (_items == null)
                _items = Values;
            else
            {
                MenuItem[] tmp = new MenuItem[_items.Length + Values.Length];
                Array.Copy(_items, tmp, _items.Length);
                Array.Copy(Values, 0, tmp, 0, Values.Length);
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

        public void InsertMenuItemAt(MenuItem Value, int Index)
        {
            if (_items == null && Index == 0)
            {
                AddMenuItem(Value);
                return;
            }

            if (_items == null || Index < 0 || Index > _items.Length)
                throw new ArgumentOutOfRangeException();

            MenuItem[] tmp = new MenuItem[_items.Length + 1];

            int i;

            // Copy upper
            for (i = 0; i < Index; i++)
                tmp[i] = _items[i];

            // Insert
            tmp[Index] = Value;

            // Copy lower
            for (i = Index; i < _items.Length; i++)
                tmp[i - 1] = _items[i];

            // Update
            _items = tmp;
            Invalidate();
        }

        /// <summary>
        /// Removes item by value
        /// </summary>
        /// <param name="Text"></param>
        public void RemoveMenuItem(MenuItem Value)
        {
            if (_items == null)
                return;

            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] == Value)
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
        public void RemoveMenuItemAt(int Index)
        {
            if (_items.Length == 1)
            {
                ClearMenuItems();
                return;
            }

            MenuItem[] tmp = new MenuItem[_items.Length - 1];
            int c = 0;
            for (int i = 0; i < _items.Length; i++)
            {
                if (i != Index)
                    tmp[c++] = _items[i];
            }

            _items = tmp;
            Invalidate();
        }

        #endregion

    }
}
