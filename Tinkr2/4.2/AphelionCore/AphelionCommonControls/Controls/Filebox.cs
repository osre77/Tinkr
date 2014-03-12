using System;
using System.IO;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
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

      public Filebox(string name, string path, Font font, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _font = font;
         _path = path;
         _mode = FileListMode.NameSizeModified;
         _file = Resources.GetBitmap(Resources.BitmapResources.file);
         _folder = Resources.GetBitmap(Resources.BitmapResources.folder);
         UpdateCols();
         UpdateItems();
         _autoNav = true;
      }

      public Filebox(string name, string path, Font font, FileListMode listMode, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
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

      public event OnSelectedIndexChanged SelectedIndexChanged;

      protected virtual void OnSelectedIndexChanged(object sender, int index)
      {
         if (SelectedIndexChanged != null)
            SelectedIndexChanged(sender, index);
      }

      public event OnPathChanged PathChanged;

      protected virtual void OnPathChanged(object sender, string value)
      {
         if (PathChanged != null)
            PathChanged(sender, value);
      }

      #endregion

      #region Properties

      public bool AutoNavigate
      {
         get { return _autoNav; }
         set { _autoNav = value; }
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

      public string FilePath
      {
         get { return _path; }
         set
         {
            if (_path == value)
               return;
            _path = value;
            _iSel = -1;
            UpdateItems();
            Invalidate();
            OnPathChanged(this, _path);
         }
      }

      public int SelectedIndex
      {
         get { return _iSel; }
      }

      public bool SelectedIsFile
      {
         get
         {
            if (_items == null || _iSel == -1)
               return false;
            return _items[_iSel].Type == FileType.File;
         }
      }

      public string SelectedValue
      {
         get
         {
            if (_items == null || _iSel == -1)
               return string.Empty;
            return _items[_iSel].FullPath;
         }
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         e.X -= Left - ScrollX;
         e.Y -= Top - ScrollY;

         if (e.Y < _font.Height + 9)
         {
            e.X--;
            for (int i = 0; i < _cols.Length; i++)
            {
               if (_cols[i].Contains(e))
               {
                  _colDown = i;
                  return;
               }
            }

            _colDown = -1;
            return;
         }

         _rowDown = (e.Y - 9 - _font.Height + _scrollY) / (_font.Height + 8);
      }

      protected override void TouchMoveMessage(object sender, point e, ref bool handled)
      {
         if (_colDown != -1 && _colDown < _cols.Length)
         {
            _cols[_colDown].Width += e.X - LastTouch.X;
            LastTouch = e;
            Invalidate();
         }
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         if (!Touching || _items == null)
            return;

         e.X -= Left - ScrollX;
         e.Y -= Top - ScrollY;

         if (_rowDown != -1 && _iSel != _rowDown && (e.Y - 9 - _font.Height + _scrollY) / (_font.Height + 8) == _rowDown)
         {
            if (_rowDown < _items.Length)
               _iSel = _rowDown;
            else
               _iSel = -1;

            if (_autoNav && _iSel > -1 && _items[_iSel].Type == FileType.Folder)
               FilePath = _items[_iSel].FullPath;
            else
            {
               OnSelectedIndexChanged(this, _iSel);
               Invalidate();
            }
         }
      }

      #endregion

      #region GUI

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int w, int h)
      // ReSharper restore RedundantAssignment
      {
         int i;

         x = Left;
         y = Top;

         if (Focused)
         {
            Core.Screen.DrawRectangle(Core.SystemColors.SelectionColor, 1, x, y, Width, Height, 0, 0,
               Core.SystemColors.WindowColor, 0, 0, Core.SystemColors.WindowColor, 0, 0, 256);
         }
         else
         {
            Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, x, y, Width, Height, 0, 0, Core.SystemColors.WindowColor, 0, 0, Core.SystemColors.WindowColor, 0, 0, 256);
         }

         // Draw Columns
         x += 1;
         y += 1;
         h = _font.Height + 9;
         Core.Screen.DrawRectangle(0, 0, x, y, Width - 2, h, 0, 0, Core.SystemColors.ControlTop, x, y, Core.SystemColors.ControlBottom, x, y + h - 1, 256);
         Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, x, y + h, x + Width - 2, y + h);
         for (i = 0; i < _cols.Length; i++)
         {
            _cols[i].X = x;
            Core.Screen.DrawTextInRect(_titles[i], x + 4, y + 4, _cols[i].Width - 8, _font.Height, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
            x += _cols[i].Width;
            Core.Screen.DrawLine(Core.SystemColors.BorderColor, 1, x - 1, y + 1, x - 1, y + h - 2);
         }
         y += h - ScrollY;

         // Draw Items
         if (_items != null && _items.Length > 0)
         {
            Core.Screen.SetClippingRectangle(Left + 1, Top + _font.Height + 10, Width - 2, Height - _font.Height - 12);
            for (i = 0; i < _items.Length; i++)
            {
               if (y + h > 0)
               {
                  if (_iSel == i)
                     Core.Screen.DrawRectangle(0, 0, Left + 1, y, Width - 3, h, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 256);

                  if (_items[i].Type == FileType.File)
                  {
                     w = _file.Width + 8;
                     Core.Screen.DrawImage(_cols[0].X + 4, y + (h / 2 - _file.Height / 2), _file, 0, 0, w, _file.Height);
                  }
                  else
                  {
                     w = _folder.Width + 8;
                     Core.Screen.DrawImage(_cols[0].X + 4, y + (h / 2 - _folder.Height / 2), _folder, 0, 0, w, _folder.Height);
                  }

                  if (_iSel == i)
                  {
                     Core.Screen.DrawTextInRect(_items[i].Name, _cols[0].X + w, y + 4, _cols[0].Width - w - 4, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.SelectedFontColor, _font);
                     if (_cols.Length > 1)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Size, _cols[1].X + 4, y + 4, _cols[1].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.SelectedFontColor, _font);
                     }
                     if (_cols.Length > 2)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Modified, _cols[2].X + 4, y + 4, _cols[2].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.SelectedFontColor, _font);
                     }
                  }
                  else
                  {
                     Core.Screen.DrawTextInRect(_items[i].Name, _cols[0].X + w, y + 4, _cols[0].Width - w - 4, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
                     if (_cols.Length > 1)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Size, _cols[1].X + 4, y + 4, _cols[1].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
                     }
                     if (_cols.Length > 2)
                     {
                        Core.Screen.DrawTextInRect(_items[i].Modified, _cols[2].X + 4, y + 4, _cols[2].Width - 8, h, Bitmap.DT_TrimmingCharacterEllipsis, Core.SystemColors.FontColor, _font);
                     }
                  }

               }

               y += h;
               if (y >= Top + Height)
               {
                  break;
               }
            }
         }
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

      public void UpdateItems()
      {
         if (_path == null || !Directory.Exists(_path))
         {
            RequiredHeight = 0;
            _items = null;
            return;
         }

         int i;
         int c = 0;
         string[] s = new StringSorter(Directory.GetDirectories(_path)).InsensitiveSort();

         _items = new ItemData[s.Length + Directory.GetFiles(_path).Length];

         for (i = 0; i < s.Length; i++)
            _items[c++] = new ItemData(s[i].Substring(s[i].LastIndexOf('\\') + 1), "", new DirectoryInfo(s[i]).LastWriteTime.ToString(), s[i], FileType.Folder);

         s = new StringSorter(Directory.GetFiles(_path)).InsensitiveSort();
         for (i = 0; i < s.Length; i++)
            _items[c++] = new ItemData(Path.GetFileName(s[i]), Filelen(new FileInfo(s[i]).Length), new FileInfo(s[i]).LastWriteTime.ToString(), s[i], FileType.File);

         RequiredHeight = ((_items.Length + 1) * (_font.Height + 9)) + 2;
      }

      #endregion

   }
}