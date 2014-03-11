using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class TabDialog : Container
    {

        #region Variables

        private int _selIndex;
        private int _selDown;
        private Font _font;
        private Color _fore;

        #endregion

        #region Constructors

        public TabDialog(string name, Font font, int x, int y, int width, int height)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;

            DefaultColors();
        }

        public TabDialog(string name, Font font, int x, int y, int width, int height, Tab[] panels)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;

            for (int i = 0; i < panels.Length; i++)
                AddChild(panels[i]);

            DefaultColors();
        }

        public TabDialog(string name, Font font, int x, int y, int width, int height, bool shadow)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;

            DefaultColors();
        }

        public TabDialog(string name, Font font, int x, int y, int width, int height, Tab[] panels, bool shadow)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;

            for (int i = 0; i < panels.Length; i++)
                AddChild(panels[i]);

            DefaultColors();
        }

        private void DefaultColors()
        {
            _fore = Core.SystemColors.FontColor;
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

                _selIndex = value;
                Invalidate();
            }
        }

        #endregion

        #region Button Methods

        protected override void ButtonPressedMessage(int buttonID, ref bool handled)
        {
            if (buttonID == (int)ButtonIDs.Left)
            {
                int newSel = _selIndex - 1;
                if (newSel < 0)
                    newSel = Children.Length - 1;
                SelectedIndex = newSel;

                handled = true;
                return;
            }
            else if (buttonID == (int)ButtonIDs.Right)
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
                    ActiveChild.SendButtonEvent(buttonID, true);
                else
                {
                    for (int i = 0; i < Children.Length; i++)
                    {
                        if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                        {
                            handled = true;
                            Children[i].SendButtonEvent(buttonID, true);
                            break;
                        }
                    }
                }
            }
        }

        protected override void ButtonReleasedMessage(int buttonID, ref bool handled)
        {
            if (Children != null)
            {
                if (ActiveChild != null)
                    ActiveChild.SendButtonEvent(buttonID, false);
                else
                {
                    for (int i = 0; i < Children.Length; i++)
                    {
                        if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                        {
                            handled = true;
                            Children[i].SendButtonEvent(buttonID, false);
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
                return;
            }

            if (pressed)
            {
                if (key == 80)
                {
                    // Left
                    int newSel = _selIndex - 1;
                    if (newSel < 0)
                        newSel = Children.Length - 1;
                    SelectedIndex = newSel;

                    handled = true;
                    return;
                }
                else if (key == 79)
                {
                    // Right
                    int newSel = _selIndex + 1;
                    if (newSel > Children.Length - 1)
                        newSel = 0;
                    SelectedIndex = newSel;

                    handled = true;
                    return;
                }
            }


        }

        protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
        {
            if (ActiveChild != null)
            {
                handled = true;
                ActiveChild.SendKeyboardKeyEvent(key, pressed);
            }
        }

        #endregion

        #region Touch Methods

        protected override void TouchDownMessage(object sender, point e, ref bool handled)
        {
            // Check Controls
            if (Children != null)
            {

                _selDown = -1;

                // Check Tabs
                Tab tab;
                for (int i = 0; i < Children.Length; i++)
                {
                    tab = (Tab)Children[i];
                    if (tab._dispRect.Contains(e))
                    {
                        _selDown = i;
                        break;
                    }
                }

                if (Children[_selIndex].Visible && Children[_selIndex].HitTest(e))
                {
                    if (ActiveChild != Children[_selIndex])
                        ActiveChild = (Control)Children[_selIndex];
                    Children[_selIndex].SendTouchDown(this, e);
                    return;
                }
            }

            ActiveChild = null;
        }

        protected override void TouchGestureMessage(object sender, TouchType e, float force, ref bool handled)
        {
            if (Children == null)
                return;

            if (e == TouchType.GestureRight)
            {
                int newSel = _selIndex - 1;
                if (newSel < 0)
                    newSel = Children.Length - 1;
                SelectedIndex = newSel;
            }
            else if (e == TouchType.GestureLeft)
            {
                int newSel = _selIndex + 1;
                if (newSel > Children.Length - 1)
                    newSel = 0;
                SelectedIndex = newSel;
            }

            if (ActiveChild != null)
            {
                handled = true;
                ActiveChild.SendTouchGesture(sender, e, force);
            }
        }

        protected override void TouchMoveMessage(object sender, point e, ref bool handled)
        {
            // Check Controls
            if (ActiveChild != null && ActiveChild.Touching)
            {
                ActiveChild.SendTouchMove(this, new point(e.X - ActiveChild.Left, e.Y - ActiveChild.Top));
                return;
            }
            else if (Children != null)
            {
                if (Children[_selIndex].HitTest(e))
                    Children[_selIndex].SendTouchMove(this, e);
            }
        }

        protected override void TouchUpMessage(object sender, point e, ref bool handled)
        {
            bool ret = false;
            bool ignoreUp = false;

            // Check Controls
            if (Children != null)
            {
                if (_selDown != -1)
                {
                    Tab tab = (Tab)Children[_selDown];
                    if (tab._dispRect.Contains(e))
                        SelectedIndex = _selDown;
                }
                else
                {
                    try
                    {
                        if (Children[_selIndex].HitTest(e) && !ignoreUp && !ret)
                        {
                            ret = true;
                            Children[_selIndex].SendTouchUp(this, e);
                        }
                        else if (Children[_selIndex].Touching)
                            Children[_selIndex].SendTouchUp(this, e);
                    }
                    catch (Exception)
                    {
                        // This can happen if the user clears the Form during a tap
                    }
                }
            }
        }

        #endregion

        #region Children

        public override void AddChild(IControl child)
        {
            if (child is Tab)
            {
                child.Width = Width;
                child.Height = Height - (_font.Height + 12);
                base.AddChild(child);
            }
            else
                throw new Exception("Only Tabs can be added to TabDialogs");
        }

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int w, int h)
        {
            int i;
            y = Top;
            x = Left;
            int barHeight = _font.Height + 12;
            int tx = x + 8;
            int tw;
            Tab tb;

            // Draw Top Bar
            Core.Screen.DrawRectangle(0, 0, x, y, Width, barHeight, 0, 0, Colors.Gray, 0, 0, Colors.Gray, 0, 0, 256);
            Core.Screen.DrawLine(Colors.LightGray, 1, x, y + barHeight - 1, x + Width, y + barHeight - 1);

            // Draw Tabs
            if (Children != null)
            {
                for (i = 0; i < Children.Length; i++)
                {
                    tb = (Tab)Children[i];
                    tw = FontManager.ComputeExtentEx(_font, tb.Title).Width + 16;

                    tb.X = 0;
                    tb.Y = barHeight;
                    tb.Width = Width;
                    tb.Height = Height - barHeight;
                    tb._dispRect = new rect(tx, y + 4, tw, barHeight - 4);

                    if (i == _selIndex)
                        Core.Screen.DrawRectangle(0, 0, tx, y + 4, tw, barHeight - 4, 0, 0, Colors.Wheat, 0, 0, Colors.LightGray, 0, 0, 256);
                    else
                        tb._beingDisplayed = false;

                    Core.Screen.DrawTextInRect(tb.Title, tx + 8, y + 8, tw - 16, _font.Height, Bitmap.DT_AlignmentCenter, (i == _selIndex) ? Colors.Charcoal : Colors.White, _font);
                    tx += tw;
                }

                tb = (Tab)Children[_selIndex];
                tb._beingDisplayed = true;
                tb.Render();
            }

        }

        #endregion

    }
}
