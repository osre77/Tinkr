using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF.Modal;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class Combobox : Control
   {

      #region Variables

      private ListboxItem[] _items;
      private int _selIndex = -1;
      private Font _font;

      private int _lineHeight;        // Pixels needed for each line
      private readonly Bitmap _down;
      private bool _expand;
      private bool _forcePopup;

      private bool _bZebra;
      //private Color _fore;
      private Color _zebra;
      //private Color _sel;
      //private Color _selText;
      //private IControl _removing;

      #endregion

      #region Constructors

      public Combobox(string name, Font font, int x, int y, int width, bool forcePopup = false)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = font.Height + 7;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;
         _forcePopup = forcePopup;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         UpdateValues();
      }

      public Combobox(string name, Font font, int x, int y, int width, int height, bool forcePopup = false)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;
         _forcePopup = forcePopup;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         UpdateValues();
      }

      public Combobox(string name, Font font, int x, int y, int width, ListboxItem[] items, bool forcePopup = false)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = font.Height + 7;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;
         _forcePopup = forcePopup;
         _items = items;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         if (_items != null && _items.Length != 0)
            _selIndex = 0;
         UpdateValues();
      }

      public Combobox(string name, Font font, int x, int y, int width, int height, ListboxItem[] items, bool forcePopup = false)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;
         _forcePopup = forcePopup;
         _items = items;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         if (_items != null && _items.Length != 0)
            _selIndex = 0;
         UpdateValues();
      }

      #endregion

      #region Events

      public event OnSelectedIndexChanged SelectedIndexChanged;

      protected virtual void OnSelectedIndexChanged(object sender, int index)
      {
         if (SelectedIndexChanged != null)
            SelectedIndexChanged(sender, index);
      }

      #endregion

      #region Properties

      public bool AlwaysPopup
      {
         get { return _forcePopup; }
         set { _forcePopup = value; }
      }

      public Font Font
      {
         get { return _font; }
         set
         {
            if (value == null || _font == value)
               return;
            _font = value;
            UpdateValues();
            Invalidate();
         }
      }

      public ListboxItem[] Items
      {
         get { return _items; }
         set
         {
            _items = value;
            UpdateValues();
            Invalidate();
         }
      }

      public int SelectedIndex
      {
         get { return _selIndex; }
         set
         {
            if (_items == null)
               return;
            if (_selIndex == value)
               return;
            if (value < -1)
               value = -1;
            else if (value > _items.Length - 1)
               value = _items.Length - 1;

            _selIndex = value;

            Invalidate();

            OnSelectedIndexChanged(this, _selIndex);
         }
      }

      public string SelectedValue
      {
         get
         {
            if (_items == null || _selIndex < 0 || _selIndex > _items.Length - 1)
               return string.Empty;
            return _items[_selIndex].Text;
         }
         set
         {
            if (_items == null || _selIndex < 0 || _selIndex > _items.Length - 1 || _items[_selIndex].Text == value)
               return;
            _items[_selIndex].Text = value;
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
         if (buttonId == (int)ButtonIDs.Select)
            _expand = true;
      }

      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int)ButtonIDs.Select && _expand)
         {
            int iMin = (_items.Length > 5) ? _lineHeight * 5 : _lineHeight * _items.Length;
            int h = Parent.Height - Y - Height;

            if (_forcePopup || h < iMin)
               new Thread(Modal).Start();
            else
            {
               if (h > Core.ScreenHeight / 2)
                  h = Core.ScreenHeight / 2;
               if (h > (_items.Length * _lineHeight) + 3)
                  h = (_items.Length * _lineHeight) + 3;

               var lstDynamic = new Listbox(Name + "_lst", _font, X, Y + Height - 1, Width - 31, h, _items)
               {
                  ZebraStripe = _bZebra,
                  ZebraStripeColor = _zebra,
                  SelectedIndex = _selIndex
               };
               lstDynamic.DoubleTap += (sender2, e2) => CleanUp(lstDynamic);
               lstDynamic.LostFocus += sender3 => CleanUp(lstDynamic, false);
               Parent.AddChild(lstDynamic);
               Parent.ActiveChild = lstDynamic;
            }
         }

         _expand = false;
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         if (e.X > Width - 31)
            _expand = true;
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         if (_items != null)
         {
            if (e.X > Width - 31 && _expand)
            {
               int iMin = (_items.Length > 5) ? _lineHeight * 5 : _lineHeight * _items.Length;
               int h = Parent.Height - Y - Height;

               if (_forcePopup || h < iMin)
                  new Thread(Modal).Start();
               else
               {
                  if (h > Core.ScreenHeight / 2)
                     h = Core.ScreenHeight / 2;
                  if (h > (_items.Length * _lineHeight) + 3)
                     h = (_items.Length * _lineHeight) + 3;
                  
                  var lstDynamic = new Listbox(Name + "_lst", _font, X, Y + Height - 1, Width - 31, h, _items)
                  {
                     ZebraStripe = _bZebra,
                     ZebraStripeColor = _zebra,
                     SelectedIndex = _selIndex
                  };
                  lstDynamic.Tap += (sender2, e2) => CleanUp(lstDynamic);
                  lstDynamic.LostFocus += sender3 => CleanUp(lstDynamic, false);
                  Parent.AddChild(lstDynamic);
                  Parent.ActiveChild = lstDynamic;
               }
            }
         }

         _expand = false;
      }

      #endregion

      #region Public Methods

      public void AddItem(ListboxItem item)
      {
         // Update Array Size
         if (_items == null)
            _items = new[] { item };
         else
         {
            var tmp = new ListboxItem[_items.Length + 1];
            Array.Copy(_items, tmp, _items.Length);
            tmp[tmp.Length - 1] = item;
            _items = tmp;
         }

         if (_selIndex == -1)
         {
            _selIndex = 0;
            Invalidate();
         }
         //item.Parent = this;
      }

      public void AddItems(ListboxItem[] items)
      {
         if (items == null)
            return;

         Suspended = true;

         //for (int i = 0; i < items.Length; i++)
         //    items[i].Parent = this;

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

         if (_selIndex == -1)
            _selIndex = 0;

         Suspended = false;
      }

      public void ClearItems()
      {
         if (_items == null)
            return;
         _items = null;
         Invalidate();
      }

      public void RemoveItem(ListboxItem item)
      {
         if (_items == null)// || item == _removing)
            return;

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
            {
               tmp[c++] = _items[i];
            }
         }

         Suspended = false;
      }

      #endregion

      #region GUI

// ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int w, int h)
// ReSharper restore RedundantAssignment
      {
         //x = Left;
         y = Top;

         // Draw Shadow
         Core.Screen.DrawRectangle(Colors.White, 1, Left + 1, Top + 1, Width - 1, Height - 1, 1, 1, Colors.White, 0, 0, Colors.White, 0, 0, 256);

         // Draw Outline
         Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1, Left, Top,
             Width - 1, Height - 1, 1, 1, 0, 0, 0, 0, 0, 0, 256);

         // Draw Fill
         if (Enabled)
         {
            Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, Width - 3, Height - 3, 0, 0, Core.SystemColors.ControlTop,
               Left, Top, Core.SystemColors.ControlBottom, Left, Top + (Height/2), 256);
         }
         else
         {
            Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, Width - 3, Height - 3, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlTop, Left, Top + (Height / 2), 256);
         }

         // Draw Button
         Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, Left + Width - 31, Top + 1, Left + Width - 31, Top + Height - 3);
         Core.Screen.DrawImage(Left + Width - 16 - (_down.Width / 2), y + (Height / 2) - (_down.Height / 2), _down, 0, 0, _down.Width, _down.Height);

         // Draw Text
         y = y + (Height / 2 - _font.Height / 2);
         if (_items != null && _selIndex != -1)
         {
            Core.Screen.DrawTextInRect(_items[_selIndex].Text, Left + 6, y, Width - 39, _font.Height, Bitmap.DT_TrimmingCharacterEllipsis, (Enabled) ? Core.SystemColors.FontColor : Colors.DarkGray, _font);
         }
      }

      #endregion

      #region Private Methods

      private void CleanUp(Listbox lst, bool updateSelection = true)
      {
         if (updateSelection || Focused)
         {
            SelectedIndex = lst.SelectedIndex;
         }
         Parent.RemoveChild(lst);
      }

      private void DefaultColors()
      {
         //_fore = Core.SystemColors.FontColor;
         //_sel = Core.SystemColors.SelectionColor;
         //_selText = Core.SystemColors.SelectedFontColor;
         _zebra = Colors.Ghost;
      }

      private void Modal()
      {
         SelectedIndex = SelectionDialog.Show(_items, _font, _selIndex, _bZebra);
      }

      private void UpdateValues()
      {
         _lineHeight = _font.Height + 4;
      }

      #endregion

   }
}
