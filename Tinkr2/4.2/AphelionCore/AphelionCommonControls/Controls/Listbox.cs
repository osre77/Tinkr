using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Displays a list of options
   /// </summary>
   [Serializable]
   public class Listbox : ScrollableControl
   {
      #region Variables

      private ListboxItem[] _items;
      private bool _showCheckboxes;
      private int _selIndex;
      private bool _showImages;
      private Font _font;
      private size _imgSize;

      private Color _bkg;
      private Color _fore;
      private Color _sel;
      private Color _selFore;
      private Color _zebra;

      private bool _bZebra;

      private int _minH;

      private int _lineHeight;        // Pixels needed for each line
      private bool _continueScroll;
      private bool _btnDown;
      private int _incAmount;
      private int _newSel;

      private int _ass, _asr;

      //private IControl _removing;

      private int _txtX, _txtW;

      private Font _chkFnt;
      private int _chkH, _chkX, _chkY;
      private bool _recalc;

      #endregion

      #region Constructors

      public Listbox(string name, Font font, int x, int y, int width, int height) :
         base(name, x, y, width, height)
      {
         _font = font;
         DefaultColors();
         _recalc = true;
      }

      public Listbox(string name, Font font, int x, int y, int width, int height, ListboxItem[] items) :
         base(name, x, y, width, height)
      {
         _font = font;
         _items = items;
         if (_items != null)
         {
            for (int i = 0; i < _items.Length; i++)
            {
               _items[i].Parent = this;
            }
         }

         DefaultColors();
         _recalc = true;
      }

      public Listbox(string name, Font font, int x, int y, int width, int height, bool displayCheckboxes, bool displayImages, size imageSize) :
         base(name, x, y, width, height)
      {
         _font = font;
         _showCheckboxes = displayCheckboxes;
         _showImages = displayImages;
         _imgSize = imageSize;
         DefaultColors();
         _recalc = true;
      }

      public Listbox(string name, Font font, int x, int y, int width, int height, ListboxItem[] items, bool displayCheckboxes, bool displayImages, size imageSize) :
         base(name, x, y, width, height)
      {
         _font = font;
         _showCheckboxes = displayCheckboxes;
         _showImages = displayImages;
         _imgSize = imageSize;

         _items = items;
         if (_items != null)
         {
            for (int i = 0; i < _items.Length; i++)
            {
               _items[i].Parent = this;
            }
         }
         DefaultColors();
         _recalc = true;
      }

      #endregion

      #region Events

      public event OnSelectedIndexChanged SelectedIndexChanged;

      protected virtual void OnSelectedIndexChanged(object sender, int index)
      {
         if (SelectedIndexChanged != null)
            SelectedIndexChanged(sender, index);
      }

      public event OnItemSelected ItemSelected;

      protected virtual void OnItemSelected(object sender, int index)
      {
         if (ItemSelected != null)
            ItemSelected(sender, index);
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
            Invalidate();
         }
      }

      public bool DisplayCheckboxes
      {
         get { return _showCheckboxes; }
         set
         {
            if (_showCheckboxes == value)
               return;
            _showCheckboxes = value;
            GetLineHeight();
            Invalidate();
         }
      }

      public bool DisplayImages
      {
         get { return _showImages; }
         set
         {
            if (_showImages == value)
               return;
            _showImages = value;
            GetLineHeight();
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
            GetLineHeight();
            Invalidate();
         }
      }

      public size ImageSize
      {
         get { return _imgSize; }
         set
         {
            if (_imgSize.Width == value.Width && _imgSize.Height == value.Height)
               return;
            _imgSize = value;
            GetLineHeight();
            Invalidate();
         }
      }

      public ListboxItem[] Items
      {
         get { return _items; }
      }

      public int MinimumLineHeight
      {
         get { return _minH; }
         set
         {
            if (_minH == value)
               return;
            _minH = value;
            GetLineHeight();
            Invalidate();
         }
      }

      public Color SelectedBackColor
      {
         get { return _sel; }
         set
         {
            if (_sel == value)
               return;
            _sel = value;
            Invalidate();
         }
      }

      public int SelectedIndex
      {
         get { return _selIndex; }
         set
         {
            if (_selIndex == value)
               return;
            if (value < 0 || _items == null)
               value = 0;
            else if (value > _items.Length - 1)
               value = _items.Length - 1;
            _selIndex = value;
            ShowSelected();
            Invalidate();
            OnSelectedIndexChanged(this, _selIndex);
         }
      }

      public ListboxItem SelectedItem
      {
         get
         {
            if (_selIndex < 0 || _selIndex >= _items.Length)
               return null;
            return _items[_selIndex];
         }
      }

      public Color SelectedTextColor
      {
         get { return _selFore; }
         set
         {
            if (_selFore == value)
               return;
            _selFore = value;
            Invalidate();
         }
      }

      public Color TextColor
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

      public bool ZebraStripe
      {
         get { return _bZebra; }
         set
         {
            if (_bZebra == value)
               return;
            _bZebra = value;
            Invalidate();
         }
      }

      public Color ZebraStripeColor
      {
         get { return _zebra; }
         set
         {
            if (_zebra == value)
               return;
            _zebra = value;
            if (_bZebra)
               Invalidate();
         }
      }

      #endregion

      #region Buttons

      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (_items == null || _items.Length == 0)
            return;

         _btnDown = true;

         if (buttonId == (int)ButtonIDs.Up)
         {
            _btnDown = true;
            _incAmount = -1;

            SelectedIndex -= 1;
            if (SelectedIndex > 0)
               new Thread(IncDec).Start();
         }
         else if (buttonId == (int)ButtonIDs.Down)
         {
            _btnDown = true;
            _incAmount = 1;

            SelectedIndex += 1;
            if (SelectedIndex < _items.Length - 1)
               new Thread(IncDec).Start();
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
               OnItemSelected(this, _selIndex);
         }
         handled = true;
      }

      #endregion

      #region Keyboard

      protected override void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled)
      {
         if (!pressed || _items == null || _items.Length == 0)
            return;

         switch (key)
         {
            case 82:        // Up
               SelectedIndex -= 1;
               handled = true;
               break;
            case 81:        // Down
               SelectedIndex += 1;
               handled = true;
               break;
         }
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         _continueScroll = false;
         LastTouch = point;

         point.Y -= Top;

         if (_items != null)
         {
            // Find Selected Index
            int y = point.Y + ScrollY;
            int idx = y / _lineHeight;

            if (idx > _items.Length - 1)
               idx = -1;

            _newSel = idx;
         }
      }

      protected override void TouchGestureMessage(object sender, TouchType e, float force, ref bool handled)
      {
         // Don't bother if we don't need scroll
         if (_items == null || MaxScrollY == 0)
            return;

         // Ignore MS Gestures & small forces
         // ReSharper disable once CompareOfFloatsByEqualityOperator
         if (force == 1.0f || force < .25)
            return;

         // Calculate Change
         // ReSharper disable PossibleLossOfFraction
         switch (e)
         {
            case TouchType.GestureUp:
               // Scroll Down (inverted)
               _asr = ScrollY + (int)((MaxScrollY - ScrollY) * (force / 10));
               _ass = (int)((Height / 4) * force);
               if (_ass < 1)
                  _ass = 1;
               break;

            case TouchType.GestureDown:
               _asr = ScrollY - (int)(MaxScrollY * (force));
               _ass = -(int)((Height / 4) * force);
               if (_ass > -1)
                  _ass = -1;
               break;
         }
         // ReSharper restore PossibleLossOfFraction

         _continueScroll = true;
         new Thread(AnimateScroll).Start();
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (_items != null)
         {
            point.Y -= Top;

            // Find Selected Index
            int y = point.Y + ScrollY;
            int idx = y / _lineHeight;

            if (idx > _items.Length - 1)
            {
               idx = -1;
            }

            if (_newSel == idx && _selIndex != idx)
            {
               _selIndex = idx;
               OnSelectedIndexChanged(this, _selIndex);
            }

            if (_showCheckboxes && _items[_selIndex].AllowCheckbox &&
                _items[_selIndex].CheckboxBounds.Contains(new point(point.X, point.Y)))
            {
               _items[_selIndex].Checked = !_items[_selIndex].Checked;
            }

            Invalidate();
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region Public Methods

      public void AddItem(ListboxItem item)
      {
         // Update Array Size
         if (_items == null)
         {
            _items = new[] { item };
         }
         else
         {
            var tmp = new ListboxItem[_items.Length + 1];
            Array.Copy(_items, tmp, _items.Length);
            tmp[tmp.Length - 1] = item;
            _items = tmp;
         }
         item.Parent = this;
         _recalc = true;
         Invalidate();
      }

      public void AddItems(ListboxItem[] items)
      {
         if (items == null)
         {
            return;
         }

         Suspended = true;

         for (int i = 0; i < items.Length; i++)
         {
            items[i].Parent = this;
         }

         if (_items == null)
         {
            _items = items;
         }
         else
         {
            var tmp = new ListboxItem[_items.Length + items.Length];
            Array.Copy(_items, tmp, _items.Length);
            Array.Copy(items, 0, tmp, _items.Length, items.Length);
            _items = tmp;
         }

         _recalc = true;

         Suspended = false;
      }

      public void ClearItems()
      {
         if (_items == null)
            return;
         _items = null;
         _recalc = true;
         Invalidate();
      }

      public void RemoveItem(ListboxItem item)
      {
         if (_items == null) // || item == _removing)
         {
            return;
         }

         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i] == item)
            {
               RemoveItemAt(i);
               return;
            }
         }
      }

      public void RemoveItemAt(int index)
      {
         if (_items == null || index < 0 || index >= _items.Length)
            return;

         if (_items.Length == 1)
         {
            ClearItems();
            return;
         }

         //if (_items[index] == _removing)
         //    return;

         Suspended = true;
         _items[index].Parent = null;

         var tmp = new ListboxItem[_items.Length - 1];
         int c = 0;
         for (int i = 0; i < _items.Length; i++)
         {
            if (i != index)
               tmp[c++] = _items[i];
         }
         _items = tmp;

         _recalc = true;

         Suspended = false;
      }

      #endregion

      #region GUI

      private void AnimateScroll()
      {
         while (_continueScroll && ScrollY != _asr)
         {
            if (!_continueScroll)
            {
               Invalidate();
               break;
            }
            int newScrollY = ScrollY + _ass;
            ScrollY += _ass;
            if (_ass > 0 && newScrollY > MaxScrollY)
            {
               ScrollY = MaxScrollY;
               _continueScroll = false;
            }
            else if (_ass < 0 && newScrollY < _asr)
            {
               ScrollY = _asr;
               _continueScroll = false;
            }
            else
               ScrollY = newScrollY;
         }
      }

      private void IncDec()
      {
         int iWait = 1200;
         while (_btnDown)
         {
            Thread.Sleep(iWait);
            if (!_btnDown)
               return;
            switch (iWait)
            {
               case 1200:
                  iWait = 750;
                  SelectedIndex += _incAmount;
                  break;
               case 750:
                  iWait = 500;
                  SelectedIndex += (_incAmount * 2);
                  break;
               case 500:
                  iWait = 250;
                  SelectedIndex += (_incAmount * 3);
                  break;
               default:
                  SelectedIndex += (_incAmount * 4);
                  break;
            }
         }
      }

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int width, int height)
      // ReSharper restore RedundantAssignment
      {
         //x is Left anyway
         //x = Left;

         //int rndY = ScrollY;
         int chkX = x + width;

         // Draw Border & Background
         Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, x, y, width, height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);

         // Check for null items
         if (_items == null || _items.Length == 0)
         {
            return;
         }

         if (_recalc)
         {
            GetLineHeight();
         }

         chkX -= _chkH + 4;

         Color c = _bkg;

         int yy = Top - ScrollY;

         // Render
         Core.Screen.SetClippingRectangle(Left + 1, Top + 1, Width - 2, Height - 2);
         for (int i = 0; i < _items.Length; i++)
         {
            if (yy + _lineHeight > Top)
            {
               // Item background
               if (_items[i].AlternateFont != null)
               {
                  Core.Screen.DrawRectangle(0, 0, x + 1, yy, Width - 2, _lineHeight, 0, 0, _items[i].AlternateBackColor, 0, 0, _items[i].AlternateBackColor, 0, 0, 256);
               }
               else
               {
                  if (i == _selIndex)
                  {
                     Core.Screen.DrawRectangle(0, 0, x + 1, yy, Width - 2, _lineHeight, 0, 0, _sel, 0, 0, _sel, 0, 0, 256);
                  }
                  else
                  {
                     Core.Screen.DrawRectangle(0, 0, x + 1, yy, Width - 2, _lineHeight, 0, 0, c, 0, 0, c, 0, 0, 256);
                  }
               }

               _items[i].Bounds = new rect(x + 1, yy, Width - 2, _lineHeight);

               // Item Image
               if (_showImages && _items[i].Image != null)
               {
                  Core.Screen.DrawImage(x + 4, yy + (_lineHeight / 2 - _imgSize.Height / 2), _items[i].Image, 0, 0, _imgSize.Width, _imgSize.Height);
               }

               // Draw Text
               if (_items[i].AlternateFont != null)
               {
                  Core.Screen.DrawTextInRect(_items[i].Text, _txtX, yy + (_lineHeight / 2 - _items[i].AlternateFont.Height / 2), _txtW, _items[i].AlternateFont.Height,
                      Bitmap.DT_None, _fore, _items[i].AlternateFont);
               }
               else
               {
                  if (i == _selIndex)
                  {
                     Core.Screen.DrawTextInRect(_items[i].Text, _txtX, yy + (_lineHeight / 2 - _font.Height / 2), _txtW,
                        _font.Height, Bitmap.DT_None, _selFore, _font);
                  }
                  else
                  {
                     Core.Screen.DrawTextInRect(_items[i].Text, _txtX, yy + (_lineHeight / 2 - _font.Height / 2), _txtW, _font.Height, Bitmap.DT_None, _fore, _font);
                  }
               }

               // Draw Check box
               if (_showCheckboxes && _items[i].AllowCheckbox)
               {
                  if (Core.FlatCheckboxes)
                  {
                     if (Enabled)
                     {
                        Core.Screen.DrawRectangle(0, 0, chkX, yy + 4, _chkH, _chkH, 0, 0, Core.SystemColors.ControlBottom,
                           Left, yy + 4, Core.SystemColors.ControlBottom, Left, yy + 4 + (_chkH / 2), 256);
                     }
                     else
                     {
                        Core.Screen.DrawRectangle(0, 0, chkX, yy + 4, _chkH, _chkH, 0, 0, Core.SystemColors.ControlTop, Left, yy + 4, Core.SystemColors.ControlTop, Left, yy + 4 + (_chkH / 2), 256);
                     }
                  }
                  else
                  {
                     Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, chkX, yy + 4, _chkH, _chkH, 1, 1, 0, 0, 0, 0, 0, 0, 256);

                     if (Enabled)
                     {
                        Core.Screen.DrawRectangle(0, 0, chkX + 1, yy + 5, _chkH - 2, _chkH - 2, 0, 0,
                           Core.SystemColors.ControlTop, Left, yy + 4, Core.SystemColors.ControlBottom, Left,
                           yy + 4 + (_chkH / 2), 256);
                     }
                     else
                     {
                        Core.Screen.DrawRectangle(0, 0, chkX + 1, yy + 5, _chkH - 2, _chkH - 2, 0, 0, Core.SystemColors.ControlTop, Left, yy + 4, Core.SystemColors.ControlTop, Left, yy + 4 + (_chkH / 2), 256);
                     }
                  }

                  _items[i].CheckboxBounds = new rect(chkX, yy + 4, _chkH, _chkH);
                  Core.Screen.DrawTextInRect("a", Left + chkX - _chkX, yy + 4 - _chkY, _chkFnt.CharWidth('a'), _chkFnt.Height, Bitmap.DT_AlignmentCenter, (_items[i].Checked) ? Core.SystemColors.SelectionColor : Colors.DarkGray, _chkFnt);
               }
            }

            yy += _lineHeight;
            if (yy >= Top + Height)
            {
               break;
            }

            // Zebra stripe flipping
            if (_bZebra)
            {
               if (c == _bkg)
               {
                  c = _zebra;
               }
               else
               {
                  c = _bkg;
               }
            }
         }

         if (Focused)
         {
            Core.ShadowRegionInset(Left, Top, Width, Height, Core.SystemColors.SelectionColor);
         }
         base.OnRender(x, y, width, height);
      }

      #endregion

      #region Private Methods

      private void DefaultColors()
      {
         _bkg = Core.SystemColors.WindowColor;
         _fore = Core.SystemColors.FontColor;
         _sel = Core.SystemColors.SelectionColor;
         _selFore = Core.SystemColors.SelectedFontColor;
         _zebra = Core.SystemColors.AltSelectionColor;
      }

      private void GetChkFont(int baseH)
      {
         _chkH = baseH;
         _chkY = 0;

         if (_chkH < 6)
            _chkH = 6;

         if (_chkH < 14)
         {
            _chkH = 12;
            _chkX = 4;
            _chkY += 3;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk8);
         }
         else if (_chkH < 16)
         {
            _chkH = 14;
            _chkX = 5;
            _chkY += 3;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk12);
         }
         else if (_chkH < 21)
         {
            _chkH = 16;
            _chkX = 5;
            _chkY += 5;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk16);
         }
         else if (_chkH < 26)
         {
            _chkH = 21;
            _chkX = 9;
            _chkY += 7;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk24);
         }
         else if (_chkH < 36)
         {
            _chkH = 26;
            _chkX = 11;
            _chkY += 11;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk32);
         }
         else if (_chkH < 51)
         {
            _chkH = 36;
            _chkX = 16;
            _chkY += 17;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk48);
         }
         else
         {
            _chkH = 51;
            _chkX = 24;
            _chkY += 26;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk72);
         }
      }

      private void GetLineHeight()
      {
         _txtX = 4;
         _txtW = Width - 8;
         _lineHeight = _font.Height + 8;

         // Adjust for images
         if (_showImages)
         {
            if (_imgSize.Width != 0 && _imgSize.Height != 0)
            {
               // Text X
               _txtX = _imgSize.Width + 8;

               // Text Width
               _txtW = Width - _imgSize.Width - 12;

               // Item height
               if (_imgSize.Height > _font.Height)
                  _lineHeight = _imgSize.Height + 8;
            }
         }

         // Adjust for checkboxes
         if (_showCheckboxes)
            _txtW -= (_lineHeight - 4);

         // Adjust for Minimum Height
         _lineHeight = System.Math.Max(_lineHeight, _minH);

         // Checkbox adjustments
         GetChkFont(_lineHeight - 8);

         _lineHeight = System.Math.Max(_lineHeight, _chkH);

         _txtX += Left;

         if (_items != null)
            RequiredHeight = _lineHeight * _items.Length;
         else
            RequiredHeight = 0;

         _recalc = false;
      }

      private void ShowSelected()
      {
         int minY = (_selIndex + 1) * _lineHeight;

         RequiredHeight = _items.Length * _lineHeight;

         if (ScrollY > minY - _lineHeight)
         {
            ScrollY = minY - _lineHeight;
            return;
         }

         if (minY > Height - ScrollY)
            ScrollY = minY - Height;

      }

      #endregion

   }
}
