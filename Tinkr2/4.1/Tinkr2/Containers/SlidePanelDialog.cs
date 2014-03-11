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
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;

            DefaultColors();
        }

        public SlidePanelDialog(string name, Font font, int x, int y, int width, int height, SlidePanel[] panels)
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

        public SlidePanelDialog(string name, Font font, int x, int y, int width, int height, bool shadow)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;
            _shadow = shadow;

            DefaultColors();
        }

        public SlidePanelDialog(string name, Font font, int x, int y, int width, int height, SlidePanel[] panels, bool shadow)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;
            _shadow = shadow;

            for (int i = 0; i < panels.Length; i++)
                AddChild(panels[i]);

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
                SlidePanel tab;
                for (int i = 0; i < Children.Length; i++)
                {
                    tab = (SlidePanel)Children[i];
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
                    SlidePanel tab = (SlidePanel)Children[_selDown];
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

        #region GUI

        private void AniSwitch(int startingIndex, int endingIndex)
        {
            int x = Left;
            int y = Top;
            int w = Width;
            int h = Height;
            int v = 0;
            int xy = 0;
            SlidePanel sp;

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
                Bitmap bmp2 = new Bitmap(w, h);
                sp = (SlidePanel)Children[endingIndex];
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

        protected override void OnRender(int x, int y, int w, int h)
        {
            if (_shadow)
            {
                x = Left + 4;
                y = Top + 4;
                w = Width - 8;
                h = Height - 8;
                Core.ShadowRegion(x, y, w, h);
            }
            else
            {
                x = Left;
                y = Top;
                w = Width;
                h = Height;
            }

            Core.Screen.DrawRectangle(0, 0, x, y, w, h, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 256);

            if (Children != null)
            {
                SlidePanel sp = (SlidePanel)Children[_selIndex];
                int i;
                int btmHeight = _inactive.Height + _font.Height + 38;

                // Draw text
                int tY = y + h - _inactive.Height - 28 - _font.Height;
                Core.Screen.DrawTextInRect(sp.Title, x + 4, tY, w - 8, _font.Height, Bitmap.DT_AlignmentCenter, _fore, _font);
                tY += _font.Height + 20;

                // Draw pips
                int tX = x + (w / 2 - (Children.Length * (_active.Width + 5)) / 2);
                for (i = 0; i < Children.Length; i++)
                {
                    if (i == _selIndex)
                        Core.Screen.DrawImage(tX, tY, _active, 0, 0, _active.Width, _active.Height);
                    else
                        Core.Screen.DrawImage(tX, tY, _inactive, 0, 0, _inactive.Width, _inactive.Height);

                    ((SlidePanel)Children[i])._dispRect = new rect(tX - 5, tY - 5, _active.Width + 10, _active.Height + 10);
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
                sp.Width = w;
                sp.Height = h - btmHeight;
                sp._beingDisplayed = true;
                sp.Render();
            }
        }

        #endregion

    }
}
