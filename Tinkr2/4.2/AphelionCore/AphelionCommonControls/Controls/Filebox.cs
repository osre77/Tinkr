using System;
using System.IO;
using Microsoft.SPOT;
using Math = System.Math;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Navigation enabled list of folders and files for a path
   /// </summary>
   [Serializable]
   public class Filebox : ScrollableControl
   {
      #region Structures

      private struct ItemData
      {
         public readonly FileType Type;
         public readonly string Name;
         public readonly string Size;
         public readonly string Modified;
         public readonly string FullPath;

         public ItemData(string name, string size, string modified, string fullPath, FileType type)
         {
            Name = name;
            Size = size;
            Modified = modified;
            FullPath = fullPath;
            Type = type;
         }
      }

      #endregion

      #region Variables

      private FileListMode _mode;
      private Font _font;
      private string _path;
      private rect[] _cols;
      private readonly string[] _titles = { "Name", "Size", "Modified" };
      private readonly Bitmap _file;
      private readonly Bitmap _folder;
      private ItemData[] _items;
      private bool _autoNav;

      private int _colDown = -1;
      private int _rowDown = -1;
#pragma warning disable 649
      private int _scrollY;
      //private bool _bMoved;
      private int _iSel = -1;
      //private bool _continueScroll;
      //private int _asr;               // Animated Scroll Remaining
      //private int _ass;               // Animated Scroll Speed
#pragma warning restore 649

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new file box
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="path">Path of the initial directory</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      public Filebox(string name, string path, Font font, int x, int y, int width, int height) :
         base(name, x, y, width, height)
      {
         _font = font;
         _path = path;
         _mode = FileListMode.NameSizeModified;
         _file = Resources.GetBitmap(Resources.BitmapResources.file);
         _folder = Resources.GetBitmap(Resources.BitmapResources.folder);
         UpdateCols();
         UpdateItems();
         _autoNav = true;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="path">Path of the initial directory</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="listMode">Mode of the file box. Controls which columns are displayed</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      public Filebox(string name, string path, Font font, FileListMode listMode, int x, int y, int width, int height) :
         base(name, x, y, width, height)
      {
         _font = font;
         _path = path;
         _mode = listMode;
         _file = Resources.GetBitmap(Resources.BitmapResources.file);
         _folder = Resources.GetBitmap(Resources.BitmapResources.folder);
         UpdateCols();
         UpdateItems();
         _autoNav = true;
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

      /// <summary>
      /// Adds or removes callback methods for PathChanged events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a button release occurs
      /// </remarks>
      public event OnPathChanged PathChanged;

      /// <summary>
      /// Fires the <see cref="PathChanged"/> event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="value">New value of path</param>
      protected virtual void OnPathChanged(object sender, string value)
      {
         if (PathChanged != null)
         {
            PathChanged(sender, value);
         }
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets whether to automatically navigate
      /// </summary>
      /// <remarks>
      /// When true tapping folders will automatically navigate to that folder
      /// </remarks>
      public bool AutoNavigate
      {
         get { return _autoNav; }
         set { _autoNav = value; }
      }

      /// <summary>
      /// Gets/Sets the font to use for items
      /// </summary>
      public Font Font
      {
         get { return _font; }
         set
         {
            if (_font == value)
            {
               return;
            }
            _font = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the ListMode
      /// </summary>
      /// <remarks>
      /// ListMode control which columns are displayed. 
      /// </remarks>
      public FileListMode ListMode
      {
         get { return _mode; }
         set
         {
            if (_mode == value)
               return;
            _mode = value;
            UpdateCols();
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the current file path
      /// </summary>
      public string FilePath
      {
         get { return _path; }
         set
         {
            if (_path == value)
            {
               return;
            }
            _path = value;
            _iSel = -1;
            UpdateItems();
            Invalidate();
            OnPathChanged(this, _path);
         }
      }

      /// <summary>
      /// Gets the index of the selected item.
      /// </summary>
      public int SelectedIndex
      {
         get { return _iSel; }
      }

      /// <summary>
      /// Gets if the selected item is a file.
      /// </summary>
      /// <remarks>
      /// If the selected item is a directory or if no item is selected, SelectedIsFile is false
      /// </remarks>
      public bool SelectedIsFile
      {
         get
         {
            if (_items == null || _iSel == -1)
            {
               return false;
            }
            return _items[_iSel].Type == FileType.File;
         }
      }

      /// <summary>
      /// Gets the full path of the selected item
      /// </summary>
      /// <remarks>
      /// If no item is selected, SelectedValue is an empty string 
      /// </remarks>
      public string SelectedValue
      {
         get
         {
            if (_items == null || _iSel == -1)
            {
               return string.Empty;
            }
            return _items[_iSel].FullPath;
         }
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
      /// Stores which column or item row was taped.
      /// </remarks>
      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         try
         {
            point.X -= Left - ScrollX;
            point.Y -= Top - ScrollY;

            if (point.Y < _font.Height + 9)
            {
               point.X--;
               for (int i = 0; i < _cols.Length; i++)
               {
                  if (_cols[i].Contains(point))
                  {
                     _colDown = i;
                     return;
                  }
               }

               _colDown = -1;
               return;
            }

            _rowDown = (point.Y - 9 - _font.Height + _scrollY) / (_font.Height + 8);
         }
         finally
         {
            base.TouchDownMessage(sender, point, ref handled);
         }
      }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// If a column was hit on touch down the width of the column is changed
      /// </remarks>
      protected override void TouchMoveMessage(object sender, point point, ref bool handled)
      {
         if (_colDown != -1 && _colDown < _cols.Length)
         {
            _cols[_colDown].Width = Math.Max(_cols[_colDown].Width + point.X - LastTouch.X, 10);
            LastTouch = point;
            Invalidate();
         }
         base.TouchMoveMessage(sender, point, ref handled);
      }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Selects the taped item or navigates to it if <see cref="AutoNavigate"/> is true.
      /// </remarks>
      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (Touching && _items != null)
         {
            point.X -= Left - ScrollX;
            point.Y -= Top - ScrollY;

            if (_rowDown != -1 && _iSel != _rowDown &&
                (point.Y - 9 - _font.Height + _scrollY) / (_font.Height + 8) == _rowDown)
            {
               if (_rowDown < _items.Length)
               {
                  _iSel = _rowDown;
               }
               else
               {
                  _iSel = -1;
               }

               if (_autoNav && _iSel > -1 && _items[_iSel].Type == FileType.Folder)
               {
                  FilePath = _items[_iSel].FullPath;
               }
               else
               {
                  OnSelectedIndexChanged(this, _iSel);
                  Invalidate();
               }
            }
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region GUI

      /// <summary>
      /// Renders the control contents
      /// </summary>
      /// <param name="x">X position in screen coordinates</param>
      /// <param name="y">Y position in screen coordinates</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <remarks>
      /// Renders the column header and the items
      /// </remarks>
      protected override void OnRender(int x, int y, int width, int height)
      {
         int i;

         //x and y are Left and Top anyway
         //x = Left;
         //y = Top;

         Core.Screen.DrawRectangle(
            Focused ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1,
            x, y, Width, Height, 0, 0,
            Core.SystemColors.WindowColor, 0, 0,
            Core.SystemColors.WindowColor, 0, 0,
            256);

         // Draw Columns
         int xx = x + 1;
         int yy = y + 1;
         int h = _font.Height + 9;
         Core.Screen.DrawRectangle(0, 0, xx, yy, Width - 2, h, 0, 0, Core.SystemColors.ControlTop, xx, yy, Core.SystemColors.ControlBottom, xx, yy + h - 1, 256);
         Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, xx, yy + h, xx + Width - 2, yy + h);
         for (i = 0; i < _cols.Length; i++)
         {
            _cols[i].X = xx;
            Core.Screen.DrawTextInRect(_titles[i], x + 4, yy + 4, _cols[i].Width - 8, _font.Height, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
            xx += _cols[i].Width;
            Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, xx - 1, yy + 1, xx - 1, yy + h - 2);
         }
         yy += h - ScrollY;

         // Draw Items
         if (_items != null && _items.Length > 0)
         {
            Core.Screen.SetClippingRectangle(Left + 1, Top + _font.Height + 10, Width - 2, Height - _font.Height - 12);
            for (i = 0; i < _items.Length; i++)
            {
               if (yy + h > 0)
               {
                  if (_iSel == i)
                  {
                     Core.Screen.DrawRectangle(0, 0, Left + 1, yy, Width - 3, h, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);
                  }

                  if (_items[i].Type == FileType.File)
                  {
                     width = _file.Width + 8;
                     Core.Screen.DrawImage(_cols[0].X + 4, yy + (h / 2 - _file.Height / 2), _file, 0, 0, width, _file.Height);
                  }
                  else
                  {
                     width = _folder.Width + 8;
                     Core.Screen.DrawImage(_cols[0].X + 4, yy + (h / 2 - _folder.Height / 2), _folder, 0, 0, width, _folder.Height);
                  }

                  if (_iSel == i)
                  {
                     Core.Screen.DrawTextInRect(_items[i].Name, _cols[0].X + width, yy + 4, _cols[0].Width - width - 4, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.SelectedFontColor, _font);
                     if (_cols.Length > 1)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Size, _cols[1].X + 4, yy + 4, _cols[1].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.SelectedFontColor, _font);
                     }
                     if (_cols.Length > 2)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Modified, _cols[2].X + 4, yy + 4, _cols[2].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.SelectedFontColor, _font);
                     }
                  }
                  else
                  {
                     Core.Screen.DrawTextInRect(_items[i].Name, _cols[0].X + width, yy + 4, _cols[0].Width - width - 4, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
                     if (_cols.Length > 1)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Size, _cols[1].X + 4, yy + 4, _cols[1].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
                     }
                     if (_cols.Length > 2)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Modified, _cols[2].X + 4, yy + 4, _cols[2].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
                     }
                  }
               }

               yy += h;
               if (yy >= Top + Height)
               {
                  break;
               }
            }
         }
         base.OnRender(x, y, width, height);
      }

      #endregion

      #region Private Methods

      //TODO: check what AnimatedScroll should have been used for
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

      private string Filelen(long length)
      {
         try
         {
            if (length < 1024) return length + " bytes";
            length = length / 1024;

            if (length < 1024) return length + " KB";
            length = length / 1024;

            if (length < 1024) return length + " MB";
            length = length / 1024;

            return length + " GB";
         }
         catch (Exception)
         {
            return "";
         }
      }

      private void UpdateCols()
      {
         int w;
         int h = _font.Height + 9;

         switch (_mode)
         {
            case FileListMode.NameOnly:
               _cols = new[] { new rect(Left + 1, Top + 1, Width - 5, h) };
               return;

            case FileListMode.NameSize:
               w = (Width - 5) / 2;
               _cols = new[] { new rect(Left + 1, Top + 1, w, h), new rect(Left + w + 1, Top + 1, w, h) };
               return;

            case FileListMode.NameSizeModified:
               w = (Width - 5) / 3;
               _cols = new[] { new rect(Left + 1, Top + 1, w, h), new rect(Left + w + 1, Top + 1, w, h), new rect(Left + w + w + 1, Top + 1, w, h) };
               return;
         }
      }

      /// <summary>
      /// Gets all directories and files from <see cref="FilePath"/> and sets them as items in a sorted order
      /// </summary>
      public void UpdateItems()
      {
         if (_path == null || !Directory.Exists(_path))
         {
            RequiredHeight = 0;
            _items = null;
            return;
         }

         try
         {
            int i;
            int c = 0;
            string[] s = new StringSorter(Directory.GetDirectories(_path)).InsensitiveSort();

            _items = new ItemData[s.Length + Directory.GetFiles(_path).Length];

            for (i = 0; i < s.Length; i++)
            {
               _items[c++] = new ItemData(s[i].Substring(s[i].LastIndexOf('\\') + 1), "",
                  new DirectoryInfo(s[i]).LastWriteTime.ToString(), s[i], FileType.Folder);
            }

            s = new StringSorter(Directory.GetFiles(_path)).InsensitiveSort();
            for (i = 0; i < s.Length; i++)
            {
               _items[c++] = new ItemData(Path.GetFileName(s[i]), Filelen(new FileInfo(s[i]).Length),
                  new FileInfo(s[i]).LastWriteTime.ToString(), s[i], FileType.File);
            }
         }
         // ReSharper disable once EmptyGeneralCatchClause
         catch { }
         if (_items != null)
         {
            RequiredHeight = ((_items.Length + 1) * (_font.Height + 9)) + 2;
         }
      }

      #endregion
   }
}