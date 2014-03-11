using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

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
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;
        }

        public MenuStrip(string name, Font font, MenuItem[] items, int x, int y, int width, int height)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
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

            if (Parent != null)
                Parent.Render(this.ScreenBounds, true);
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

            if (Parent != null)
                Parent.Render(this.ScreenBounds, true);
        }

        public void ClearMenuItems()
        {
            if (_items == null)
                return;
            _items = null;
            if (Parent != null)
                Parent.Render(this.ScreenBounds, true);
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
                tmp[i + 1] = _items[i];

            // Update
            _items = tmp;
            if (Parent != null)
                Parent.Render(this.ScreenBounds, true);
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
            if (Parent != null)
                Parent.Render(this.ScreenBounds, true);
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
                    if (_items[i]._items != null)
                    {
                        ContextMenu cm = new ContextMenu(Name + "_cm", _font, _items[i]._items);
                        cm.LostFocus += (object s) => Collapse();
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
            this.Parent.TopLevelContainer.Suspended = true;
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].Expanded)
                {
                    _items[i].SendTouchUp(this, new point(_items[i].X - 1, 0));
                    _items[i].Expanded = false;
                }
            }
            this.Parent.TopLevelContainer.Suspended = false;
        }

        protected override void OnRender(int x, int y, int w, int h)
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
