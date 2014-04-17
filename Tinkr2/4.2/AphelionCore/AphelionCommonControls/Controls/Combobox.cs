using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF.Modal;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Provides a compact list of items which can be navigated via drop-down or pop-out
   /// </summary>
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

      /// <summary>
      /// Creates a new combo box
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="forcePopup">true if the combo box should always display a pop-up even if it has room for a drop-down</param>
      /// <remarks>
      /// The <see cref="Control.Height"/> is calculated automatically to fit the font.
      /// </remarks>
      public Combobox(string name, Font font, int x, int y, int width, bool forcePopup = false) :
         base(name, x, y, width, font.Height + 7)
      {
         _font = font;
         _forcePopup = forcePopup;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         UpdateValues();
      }

      /// <summary>
      /// Creates a new combo box
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <param name="forcePopup">true if the combo box should always display a pop-up even if it has room for a drop-down</param>
      public Combobox(string name, Font font, int x, int y, int width, int height, bool forcePopup = false) :
         base(name, x, y, width, height)
      {
         _font = font;
         _forcePopup = forcePopup;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         UpdateValues();
      }

      /// <summary>
      /// Creates a new combo box
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="items">Items to initially add to the combo box</param>
      /// <param name="forcePopup">true if the combo box should always display a pop-up even if it has room for a drop-down</param>
      /// <remarks>
      /// The <see cref="Control.Height"/> is calculated automatically to fit the font.
      /// </remarks>
      public Combobox(string name, Font font, int x, int y, int width, ListboxItem[] items, bool forcePopup = false) :
         base(name, x, y, width, font.Height + 7)
      {
         _font = font;
         _forcePopup = forcePopup;
         _items = items;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         if (_items != null && _items.Length != 0)
         {
            _selIndex = 0;
         }
         UpdateValues();
      }

      /// <summary>
      /// Creates a new combo box
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <param name="items">Items to initially add to the combo box</param>
      /// <param name="forcePopup">true if the combo box should always display a pop-up even if it has room for a drop-down</param>
      public Combobox(string name, Font font, int x, int y, int width, int height, ListboxItem[] items, bool forcePopup = false) :
         base(name, x, y, width, height)
      {
         _font = font;
         _forcePopup = forcePopup;
         _items = items;
         DefaultColors();
         _down = Resources.GetBitmap(Resources.BitmapResources.down);
         if (_items != null && _items.Length != 0)
         {
            _selIndex = 0;
         }
         UpdateValues();
      }

      #endregion

      #region Events

      /// <summary>
      /// Adds or removes callback methods for SelectedIndexChanged events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a button release occurs
      /// </remarks>
      public event OnSelectedIndexChanged SelectedIndexChanged;

      /// <summary>
      /// Fires the <see cref="SelectedIndexChanged"/> event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="index">New item index</param>
      protected virtual void OnSelectedIndexChanged(object sender, int index)
      {
         if (SelectedIndexChanged != null)
         {
            SelectedIndexChanged(sender, index);
         }
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets if the combo box should always display a pop-up even if it has room for a drop-down
      /// </summary>
      public bool AlwaysPopup
      {
         get { return _forcePopup; }
         set { _forcePopup = value; }
      }

      /// <summary>
      /// Gets/Sets the font to use for the text and item
      /// </summary>
      public Font Font
      {
         get { return _font; }
         set
         {
            if (value == null || _font == value)
            {
               return;
            }
            _font = value;
            UpdateValues();
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the combo box items
      /// </summary>
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

      /// <summary>
      /// Gets/Sets the index of the selected item.
      /// </summary>
      public int SelectedIndex
      {
         get { return _selIndex; }
         set
         {
            if (_items == null)
            {
               return;
            }
            if (_selIndex == value)
            {
               return;
            }
            if (value < -1)
            {
               value = -1;
            }
            else if (value > _items.Length - 1)
            {
               value = _items.Length - 1;
            }

            _selIndex = value;

            Invalidate();

            OnSelectedIndexChanged(this, _selIndex);
         }
      }

      /// <summary>
      /// Gets/Sets the string value of the selected item
      /// </summary>
      public string SelectedValue
      {
         get
         {
            if (_items == null || _selIndex < 0 || _selIndex > _items.Length - 1)
            {
               return string.Empty;
            }
            return _items[_selIndex].Text;
         }
         set
         {
            if (_items == null || _selIndex < 0 || _selIndex > _items.Length - 1 || _items[_selIndex].Text == value)
            {
               return;
            }
            _items[_selIndex].Text = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets whether or not to enable Zebra Striping
      /// </summary>
      /// <see cref="ZebraStripeColor"/>
      public bool ZebraStripe
      {
         get { return _bZebra; }
         set
         {
            if (_bZebra == value)
            {
               return;
            }
            _bZebra = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets color to use with Zebra Stripes
      /// </summary>
      /// <see cref="ZebraStripe"/>
      public Color ZebraStripeColor
      {
         get { return _zebra; }
         set
         {
            if (_zebra == value)
            {
               return;
            }
            _zebra = value;
            if (_bZebra)
            {
               Invalidate();
            }
         }
      }

      #endregion

      #region Buttons

      /// <summary>
      /// Override this message to handle button pressed events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Expands the combo box on <see cref="ButtonIDs.Select"/>
      /// </remarks>
      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int) ButtonIDs.Select)
         {
            _expand = true;
         }
         base.ButtonPressedMessage(buttonId, ref handled);
      }

      /// <summary>
      /// Override this message to handle button released events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Shows the pop up or item list on <see cref="ButtonIDs.Select"/> if _expand is true
      /// </remarks>
      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         if (_items != null && buttonId == (int)ButtonIDs.Select && _expand)
         {
            ShowPopupOrList();
         }
         _expand = false;

         base.ButtonReleasedMessage(buttonId, ref handled);
      }

      #endregion

      #region Touch

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Expands the combo box on <see cref="ButtonIDs.Select"/>
      /// </remarks>
      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         if (point.X > Width - 31)
         {
            _expand = true;
         }
         base.TouchDownMessage(sender, point,ref handled);
      }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Shows the pop up or item list on <see cref="ButtonIDs.Select"/> if _expand is true
      /// </remarks>
      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (_items != null && point.X > Width - 31 && _expand)
         {
            ShowPopupOrList();
         }
         _expand = false;

         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds a item to the combo box
      /// </summary>
      /// <param name="item">Item to add</param>
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

         if (_selIndex == -1)
         {
            _selIndex = 0;
            Invalidate();
         }
         //item.Parent = this;
      }

      /// <summary>
      /// Adds a number of items to the combo box
      /// </summary>
      /// <param name="items">Items to add</param>
      public void AddItems(ListboxItem[] items)
      {
         if (items == null)
         {
            return;
         }

         Suspended = true;
         try
         {
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
            {
               _selIndex = 0;
            }
         }
         finally
         {
            Suspended = false;
         }
      }

      /// <summary>
      /// Removes all items from the combo box
      /// </summary>
      public void ClearItems()
      {
         if (_items == null)
         {
            return;
         }
         _items = null;
         Invalidate();
      }

      /// <summary>
      /// Removes a single item from the combo box
      /// </summary>
      /// <param name="item">Item to remove</param>
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

      /// <summary>
      /// Removes a item by its index
      /// </summary>
      /// <param name="index">Index of the item to remove</param>
      public void RemoveItemAt(int index)
      {
         if (_items == null || index < 0 || index >= _items.Length)
         {
            return;
         }

         if (_items.Length == 1)
         {
            ClearItems();
            return;
         }

         //if (_items[index] == _removing)
         //    return;

         Suspended = true;
         try
         {
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
         }
         finally
         {
            Suspended = false;
         }
      }

      #endregion

      #region GUI

      protected override void OnRender(int x, int y, int width, int height)
      {
         //x = Left;
         //y = Top;

         // Draw Shadow
         Core.Screen.DrawRectangle(Colors.White, 1, Left + 1, Top + 1, Width - 1, Height - 1, 1, 1, Colors.White, 0, 0, Colors.White, 0, 0, 256);

         // Draw Outline
         Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1, Left, Top,
             Width - 1, Height - 1, 1, 1, 0, 0, 0, 0, 0, 0, 256);

         // Draw Fill
         Core.Screen.DrawRectangle(
            0, 0, Left + 1, Top + 1, Width - 3, Height - 3, 0, 0, 
            Core.SystemColors.ControlTop, Left, Top, 
            Enabled ? Core.SystemColors.ControlBottom : Core.SystemColors.ControlTop, Left, Top + (Height/2), 
            256);

         // Draw Button
         Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, Left + Width - 31, Top + 1, Left + Width - 31, Top + Height - 3);
         Core.Screen.DrawImage(Left + Width - 16 - (_down.Width / 2), y + (Height / 2) - (_down.Height / 2), _down, 0, 0, _down.Width, _down.Height);

         // Draw Text
         y = y + (Height / 2 - _font.Height / 2);
         if (_items != null && _selIndex != -1)
         {
            Core.Screen.DrawTextInRect(_items[_selIndex].Text, Left + 6, y, Width - 39, _font.Height, Bitmap.DT_TrimmingCharacterEllipsis, (Enabled) ? Core.SystemColors.FontColor : Colors.DarkGray, _font);
         }

         // no need to cal base.OnRender
      }

      #endregion

      #region Private Methods

      private void ShowPopupOrList()
      {
         int iMin = (_items.Length > 5) ? _lineHeight * 5 : _lineHeight * _items.Length;
         int h = Parent.Height - Y - Height;

         if (_forcePopup || h < iMin)
         {
            new Thread(Modal).Start();
         }
         else
         {
            if (h > Core.ScreenHeight / 2)
            {
               h = Core.ScreenHeight / 2;
            }
            if (h > (_items.Length * _lineHeight) + 3)
            {
               h = (_items.Length * _lineHeight) + 3;
            }

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
