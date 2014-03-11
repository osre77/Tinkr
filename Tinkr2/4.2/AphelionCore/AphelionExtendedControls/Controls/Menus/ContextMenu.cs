using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class ContextMenu : Control
    {

        #region Variables

        private Font _font;
        private MenuItem[] _items;

        private Bitmap right = Resources.GetBitmap(Resources.BitmapResources.right);

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

        public void Show(IContainer parent, int x, int y, bool bottomAlign = false)
        {
            int w = 0;
            int h = 0;
            int i, j;
            bool addSubs = false;

            // Check for empty or bad parent
            if (_items == null || parent.TopLevelContainer != Core.ActiveContainer)
                return;

            // Calculate Size
            h = GetMenuItem(_items);
            for (i = 0; i < _items.Length; i++)
            {
                j = FontManager.ComputeExtentEx(_font, _items[i].Text).Width;
                if (_items[i].Length > 0)
                    addSubs = true;
                if (j > w)
                    w = j;
            }
            w += (addSubs) ? 40 : 8;

            // Update Position
            if (bottomAlign)
                y -= h;

            if (y + h > Core.Screen.Height)
                y = y - h;
            if (y < 0)
                y = 0;
            if (x + w > Core.Screen.Width)
                x = x - w;
            if (x < 0)
                x = 0;

            X = x;
            Y = y;
            Width = w;
            Height = h;

            if (!Visible)
                Visible = true;

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
            for (int i = 0; i < mnu._items.Length; i++)
            {
                if (mnu._items[i].Expanded)
                {
                    mnu._items[i].Expanded = false;
                    BlurChildren(mnu._items[i]);
                }
            }
        }

        public override bool HitTest(point e)
        {
            // Check Main Bounds
            if (ScreenBounds.Contains(e))
                return true;

            // Check Expanded Menus
            if (_items != null)
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    if (_items[i].Expanded && _items[i].ExpandedBounds.Contains(e))
                        return true;
                    else if (_items[i].Expanded && _items.Length > 0 && HitTestSub(_items[i], e))
                        return true;
                }
            }

            return false;
        }

        private bool HitTestSub(MenuItem mnu, point e)
        {
            for (int i = 0; i < mnu._items.Length; i++)
            {
                if (mnu._items[i].Expanded && mnu._items[i].ExpandedBounds.Contains(e))
                    return true;
                else if (mnu._items[i].Expanded && mnu._items.Length > 0 && HitTestSub(mnu._items[i], e))
                    return true;
            }

            return false;
        }

        #endregion

        #region Touch

        protected override void TouchDownMessage(object sender, point e, ref bool handled)
        {
            if (_items == null)
                return;

            for (int i = 0; i < _items.Length; i++)
            {

                if (_items[i].Expanded && _items.Length > 0)
                {
                    // Check subnodes first
                    if (SendTouchDownSub(_items[i], e))
                        return;
                }
                else if (_items[i].ScreenBounds.Contains(e))
                {
                    Parent.Suspended = true;
                    _items[i].SendTouchDown(this, e);
                    CollapseOnLne(_items, i);
                    Parent.Suspended = false;
                    return;
                }
            }

        }

        private bool SendTouchDownSub(MenuItem mnu, point e)
        {
            for (int i = 0; i < mnu._items.Length; i++)
            {
                if (mnu._items[i].Expanded && mnu._items[i].Length > 0)
                {
                    if (SendTouchDownSub(mnu._items[i], e))
                        return true;
                    mnu._items[i].Expanded = false;
                }
                else if (mnu._items[i].ScreenBounds.Contains(e))
                {
                    mnu._items[i].SendTouchDown(this, e);
                    CollapseSubsFrom(mnu._items, i + 1);
                    Parent.Invalidate();
                    return true;
                }
            }

            return false;
        }

        protected override void TouchUpMessage(object sender, point e, ref bool handled)
        {
            if (_items == null)
                return;

            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].Expanded && _items.Length > 0)
                {
                    if (SendTouchUpSub(_items[i], e))
                    {
                        Blur();
                        _items[i].Expanded = false;
                        return;
                    }

                    //_items[i].Expanded = false;
                }
                else if (_items[i].ScreenBounds.Contains(e))
                {
                    if (_items[i].Touching)
                    {
                        if (_items[i]._items != null)
                        {
                            _items[i]._expanded = !_items[i]._expanded;
                            Parent.Invalidate();
                        }
                        else
                        {
                            Blur();
                            _items[i].SendTouchUp(this, e);
                        }

                        return;
                    }
                    else
                        Invalidate(this.ScreenBounds);

                    _items[i].SendTouchUp(this, e);
                    return;
                }
                else if (_items[i].Touching)
                {
                    _items[i].SendTouchUp(this, e);
                    Invalidate();
                }
            }
        }

        private bool SendTouchUpSub(MenuItem mnu, point e)
        {
            for (int i = 0; i < mnu._items.Length; i++)
            {
                if (mnu._items[i].Expanded && mnu._items.Length > 0)
                {
                    if (SendTouchUpSub(mnu._items[i], e))
                        return true;
                }
                else if (mnu._items[i].ScreenBounds.Contains(e))
                {
                    mnu._items[i].SendTouchUp(this, e);
                    if (mnu._items[i].Touching)
                    {
                        if (mnu._items[i]._items != null)
                        {
                            mnu._items[i]._expanded = !mnu._items[i]._expanded;
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

        protected override void OnRender(int x, int y, int w, int h)
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
                        Core.Screen.DrawImage(Left + Width - 10, y + _font.Height / 2 - 2, right, 0, 0, 4, 5);

                    if (_items[i].Expanded)
                        DrawExpandedMenu(_items[i], Left + Width - 1, y - 4, Width);

                    y += _font.Height + 5;
                    if (i != _items.Length - 1)
                        Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, Left + 1, y, Left + Width - 2, y);
                    y += 4;
                }
            }
        }

        private void DrawExpandedMenu(MenuItem mnu, int X, int Y, int OwnerWidth)
        {
            int w = 0;
            int h = 0;
            int i, j;
            bool addSubs = false;

            //_iSel = -1;

            // Calculate Size
            h = GetMenuItem(mnu._items);
            for (i = 0; i < mnu._items.Length; i++)
            {
                j = FontManager.ComputeExtentEx(_font, mnu._items[i].Text).Width;
                if (mnu._items[i].Length > 0)
                    addSubs = true;
                if (j > w)
                    w = j;
            }
            w += (addSubs) ? 40 : 8;

            // Update Position
            if (Y + h > Core.Screen.Height)
                Y = Y - h;
            if (Y < 0)
                Y = 0;
            if (X + w > Core.Screen.Width)
                X = X - w - OwnerWidth + 3;
            if (X < 0)
                X = 0;

            mnu.ExpandedBounds = new rect(X, Y, w, h);
            Core.Screen.DrawRectangle(Colors.DarkGray, 1, X, Y, w - 1, h, 0, 0, Core.SystemColors.ControlTop, X, Y, Core.SystemColors.ControlBottom, X + w, Y + h, 256);

            // Draw Items
            int y = Y + 4;
            for (i = 0; i < mnu._items.Length; i++)
            {
                if (mnu._items[i].Visible)
                {
                    if (mnu._items[i].Touching || mnu._items[i].Expanded)
                    {
                        Core.Screen.DrawRectangle(0, 0, X + 1, y - 3, w - 2, _font.Height + 8, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);
                        Core.Screen.DrawTextInRect(mnu._items[i].Text, X + 4, y, w - 8, _font.Height, Bitmap.DT_AlignmentLeft, Core.SystemColors.SelectedFontColor, _font);
                    }
                    else
                        Core.Screen.DrawTextInRect(mnu._items[i].Text, X + 4, y, w - 8, _font.Height, Bitmap.DT_AlignmentLeft, (mnu._items[i].Enabled) ? Core.SystemColors.FontColor : Colors.DarkGray, _font);

                    mnu._items[i].X = X;// 1;
                    mnu._items[i].Y = y; // Y; // y - Y;
                    mnu._items[i].Width = w - 2;
                    mnu._items[i].Height = _font.Height + 8;
                    mnu._items[i].Parent = Parent;

                    if (mnu._items[i].Length > 0)
                        Core.Screen.DrawImage(X + w - 10, y + _font.Height / 2 - 2, right, 0, 0, 4, 5);

                    if (mnu._items[i].Expanded)
                        DrawExpandedMenu(mnu._items[i], X + w - 1, y - 4, w);

                    y += _font.Height + 5;
                    if (i != mnu._items.Length - 1)
                        Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, X + 1, y, X + w - 2, y);
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

        private void CollapseSubsFrom(MenuItem[] items, int Index)
        {
            for (int i = Index; i < items.Length; i++)
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
