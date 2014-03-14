using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class Listview : Control
   {

      #region Variables

      private ListviewColumn[] _cols;
      private ListviewItem[] _items;
      private Font _header;
      private Font _item;
      private Color _headerC, _itemC, _selC, _selTextC;

      //TODO: check wat _colDown was intended to do
      //private int _colDown = -1;
      private int _rowDown = -1;
      private int _iSel = -1;
      private bool _bMoved;
      private int _scrollY;

      //private bool _continueScroll;
      //private int _asr;               // Animated Scroll Remaining
      //private int _ass;               // Animated Scroll Speed

      private int _autoW;             // Automatic Column Width

      #endregion

      #region Constructors

      public Listview(string name, Font headerFont, Font itemFont, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _header = headerFont;
         _item = itemFont;

         UpdateColumns();
         DefaultColors();
      }

      public Listview(string name, Font headerFont, Font itemFont, int x, int y, int width, int height, string[] columns)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _header = headerFont;
         _item = itemFont;

         if (columns != null)
         {
            _cols = new ListviewColumn[columns.Length];
            for (int i = 0; i < columns.Length; i++)
               _cols[i] = new ListviewColumn("column" + i, columns[i]);
         }

         UpdateColumns();
         DefaultColors();
      }

      public Listview(string name, Font headerFont, Font itemFont, int x, int y, int width, int height, ListviewColumn[] columns)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         _header = headerFont;
         _item = itemFont;
         _cols = columns;

         UpdateColumns();
         DefaultColors();
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

      public ListviewColumn[] Columns
      {
         get { return _cols; }
         set
         {
            if (_cols == value)
               return;
            _cols = value;
            Invalidate();
         }
      }

      public Color HeaderColor
      {
         get { return _headerC; }
         set
         {
            if (_headerC == value)
               return;
            _headerC = value;
            Invalidate();
         }
      }

      public Font HeaderFont
      {
         get { return _header; }
         set
         {
            if (_header == value)
               return;
            _header = value;
            Invalidate();
         }
      }

      public ListviewItem[] Items
      {
         get { return _items; }
         set
         {
            if (_items == value)
               return;
            _items = value;
            Invalidate();
         }
      }

      public Color ItemColor
      {
         get { return _itemC; }
         set
         {
            if (_itemC == value)
               return;
            _itemC = value;
            Invalidate();
         }
      }

      public Font ItemFont
      {
         get { return _item; }
         set
         {
            if (_item == value)
               return;
            _item = value;
            Invalidate();
         }
      }

      public int SelectedIndex
      {
         get { return _iSel; }
         set
         {
            if (_iSel == value)
               return;
            _iSel = value;
            Invalidate();
         }
      }

      public Color SelectionColor
      {
         get { return _selC; }
         set
         {
            if (_selC == value)
               return;
            _selC = value;
            Invalidate();
         }
      }

      public Color SelectedTextColor
      {
         get { return _selTextC; }
         set
         {
            if (_selTextC == value)
               return;
            _selTextC = value;
            Invalidate();
         }
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         e.X -= Left;
         e.Y -= Top;

         if (_cols != null && e.Y < _header.Height + 9)
         {
            e.X--;
            for (int i = 0; i < _cols.Length; i++)
            {
               var bounds = new rect(_cols[i].X, 1, _cols[i].W, _header.Height + 8);
               if (bounds.Contains(e))
               {
                  //_colDown = i;
                  return;
               }
            }

            //_colDown = -1;
            return;
         }

         _rowDown = (e.Y - 9 - _header.Height + _scrollY) / (_item.Height + 8);
      }

      protected override void TouchMoveMessage(object sender, point e, ref bool handled)
      {
         if (_items == null)
            return;

         int diff = e.Y - LastTouch.Y;
         if (diff == 0 || !Touching || (diff < 10 && diff > -10))
            return;
         int max = _items.Length * (_item.Height + 8) - (Height - 3 - (_header.Height + 9));
         if (max <= 0)
            return;
         int ns = _scrollY - diff;
         if (ns < 0)
            ns = 0;
         else if (ns > max)
            ns = max;
         if (_scrollY != ns)
         {
            _scrollY = ns;
            Invalidate();
         }
         LastTouch = e;
         _bMoved = true;
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         //_colDown = -1;

         e.X -= Left;
         e.Y -= Top;

         if (_items == null || _bMoved)
         {
            if (_bMoved)
            {
               _bMoved = false;
               Invalidate();
            }
            _rowDown = -1;
            return;
         }

         if (_rowDown != -1 && _iSel != _rowDown && (e.Y - 9 - _header.Height + _scrollY) / (_item.Height + 8) == _rowDown)
         {
            if (_rowDown < _items.Length)
               _iSel = _rowDown;
            else
               _iSel = -1;

            OnSelectedIndexChanged(this, _iSel);
            Invalidate();
         }
      }

      #endregion

      #region Public Methods

      public void AddColumn(ListviewColumn column)
      {
         if (_cols == null)
         {
            _cols = new[] { column };
         }
         else
         {
            var tmp = new ListviewColumn[_cols.Length + 1];
            Array.Copy(_cols, tmp, _cols.Length);
            tmp[tmp.Length - 1] = column;
            _cols = tmp;
         }

         UpdateColumns();
         Invalidate();
      }

      public void ClearColumns()
      {
         if (_cols != null)
         {
            _cols = null;
            UpdateColumns();
            Invalidate();
         }
      }

      public void RemoveColumn(ListviewColumn column)
      {
         if (_cols == null)
            return;

         for (int i = 0; i < _cols.Length; i++)
         {
            if (_cols[i] == column)
            {
               RemoveColumnAt(i);
               return;
            }
         }
      }

      public void RemoveColumnAt(int index)
      {
         if (_cols == null || index < 0 || index >= _cols.Length)
            return;

         if (_cols.Length == 1)
         {
            _cols = null;
         }
         else
         {
            var tmp = new ListviewColumn[_cols.Length - 1];
            int c = 0;
            for (int i = 0; i < _cols.Length; i++)
            {
               if (i != index)
                  tmp[c++] = _cols[i];
            }
            _cols = tmp;
         }

         UpdateColumns();
         Invalidate();
      }

      public void AddLine(ListviewItem line)
      {
         if (_items == null)
            _items = new[] { line };
         else
         {
            var tmp = new ListviewItem[_items.Length + 1];
            Array.Copy(_items, tmp, _items.Length);
            tmp[tmp.Length - 1] = line;
            _items = tmp;
         }

         Invalidate();
      }

      public void AddLines(ListviewItem[] lines)
      {
         if (lines == null)
            return;

         if (_items == null)
            _items = lines;
         else
         {
            var tmp = new ListviewItem[_items.Length + lines.Length];
            Array.Copy(_items, tmp, _items.Length);
            Array.Copy(lines, 0, tmp, _items.Length, lines.Length);
            _items = tmp;
         }

         Invalidate();
      }

      public void ClearLines()
      {
         _items = null;

         Invalidate();
      }

      public void RemoveLine(ListviewItem line)
      {
         if (_items == null)
            return;

         for (int i = 0; i < _items.Length; i++)
         {
            if (_items[i] == line)
            {
               RemoveLineAt(i);
               return;
            }
         }
      }

      public void RemoveLineAt(int index)
      {
         if (_items == null || index < 0 || index >= _items.Length)
            return;

         if (_items.Length == 1)
         {
            _items = null;
         }
         else
         {
            var tmp = new ListviewItem[_items.Length - 1];
            int c = 0;
            for (int i = 0; i < _items.Length; i++)
            {
               if (i != index)
                  tmp[c++] = _items[i];
            }
            _items = tmp;
         }

         Invalidate();
      }

      #endregion

      #region GUI

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int w, int h)
      // ReSharper restore RedundantAssignment
      {
         x = Left;
         y = Top;

         // Draw Background
         Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1, Left, Top, Width - 1, Height - 1, 0, 0,
             Core.SystemColors.WindowColor, 0, 0, Core.SystemColors.WindowColor, 0, 0, 256);

         // Draw Drop
         Core.Screen.DrawLine(Colors.White, 1, x + Width - 1, y, x + Width - 1, y + Height - 1);
         Core.Screen.DrawLine(Colors.White, 1, x, y + Height - 1, x + Width - 1, y + Height - 1);

         // Draw Data
         h = _header.Height + 9;
         bool bOn = true;
         x++;
         y++;
         Core.Screen.DrawRectangle(0, 0, x, y, Width - 3, h, 0, 0, Core.SystemColors.ControlTop, x, y, Core.SystemColors.ControlBottom, x, y + 4, 256);
         Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, x, y + h, x + Width - 3, y + h);
         if (_cols != null)
         {
            // Columns
            for (int i = 0; i < _cols.Length; i++)
            {
               w = _cols[i].W;
               if (w == -1)
                  w = _autoW;
               _cols[i].X = x;
               Core.Screen.DrawTextInRect(_cols[i].Text, x + 4, y + 4, w - 8, _header.Height, Bitmap.DT_TrimmingCharacterEllipsis, _headerC, _header);
               x += w;
               Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, x - 1, y + 1, x - 1, y + h - 2);
            }
            y += h + 1;

            // Items
            Core.ClipForControl(this, Left + 1, y, Width - 3, Height - h - 4);
            y -= _scrollY;
            h = _item.Height + 8;
            if (_items != null)
            {
               for (int i = 0; i < _items.Length; i++)
               {
                  bOn = !bOn;
                  x = Left + 1;

                  if (y + h > 0)
                  {

                     if (_iSel == i)
                        Core.Screen.DrawRectangle(0, 0, Left + 1, y, Width - 3, h, 0, 0, _selC, 0, 0, _selC, 0, 0, 256);
                     else if (bOn)
                        Core.Screen.DrawRectangle(0, 0, Left + 1, y, Width - 3, h, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);

                     if (_items[i].Values != null)
                     {
                        int c = (_items[i].Values.Length < _cols.Length) ? _items[i].Values.Length : _cols.Length;
                        for (int j = 0; j < c; j++)
                        {
                           w = _cols[j].Width;
                           if (w == -1)
                              w = _autoW;
                           Core.Screen.DrawTextInRect(_items[i].Values[j], x + 4, y + 4, w - 8, _item.Height, Bitmap.DT_TrimmingCharacterEllipsis, (i == _iSel) ? _selTextC : _itemC, _item);
                           x += w;
                        }
                     }
                  }
                  y += h;
                  if (y >= Top + Height - 2)
                     break;
               }
            }
         }

         // Scroll
         if (_bMoved && _items != null)
         {
            Core.ClipForControl(this, Left, Top, Width, Height);
            int lineHeight = _item.Height + 8;
            int msa = (_items.Length * lineHeight) - (Height - _header.Height - 16);
            int iGripSize = (Height - _header.Height - 16) - (msa / lineHeight);
            if (iGripSize < 8)
               iGripSize = 8;

            int iOffset = Top + _header.Height + 14 + (int)(((Height - _header.Height - 16) - iGripSize) * (_scrollY / (float)msa));

            Core.Screen.DrawRectangle(Colors.Black, 0, Left + Width - 7, iOffset, 4, iGripSize, 0, 0, 0, 0, 0, 0, 0, 0, 80);

         }
      }

      #endregion

      #region Private Methods

      //TODO: check what AnimateScroll was intended to do
      /*private void AnimateScroll()
      {
          while (_continueScroll && _scrollY != _asr)
          {
              if (!_continueScroll)
              {
                  Invalidate();
                  break;
              }
              _scrollY += _ass;
              if (_ass > 0 && _scrollY > _asr)
              {
                  _scrollY = _asr;
                  _continueScroll = false;
              }
              else if (_ass < 0 && _scrollY < _asr)
              {
                  _scrollY = _asr;
                  _continueScroll = false;
              }
              Invalidate();
          }
      }*/

      private void DefaultColors()
      {
         _headerC = Core.SystemColors.FontColor;
         _itemC = Core.SystemColors.FontColor;
         _selC = Core.SystemColors.SelectionColor;
         _selTextC = Core.SystemColors.SelectedFontColor;
      }

      private void UpdateColumns()
      {
         if (_cols == null)
            return;

         int availW = Width - 3;
         int unsizedCols = 0;
         int i;

         // Check unsized columns
         for (i = 0; i < _cols.Length; i++)
         {
            if (_cols[i].Width < 1)
               unsizedCols++;
            else
               availW -= _cols[i].Width;
         }

         if (unsizedCols == 0)
            return;

         _autoW = availW / unsizedCols;
         if (_autoW < 10)
            _autoW = 10;
      }

      #endregion

   }
}
