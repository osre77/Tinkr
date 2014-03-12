using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class ListboxItem : MarshalByRefObject
   {

      #region Variables

      private string _text;
      private Bitmap _image;
      private bool _checked;
      private Listbox _parent;
      private bool _allowCheck;
      private object _tag;
      private Font _fnt;
      private Color _bkg;

      #endregion

      #region Constructors

      public ListboxItem(string text, bool allowCheckbox = true)
      {
         _text = text;
         _image = null;
         _checked = false;
         _allowCheck = allowCheckbox;
      }

      public ListboxItem(string text, Bitmap image, bool allowCheckbox = true)
      {
         _text = text;
         _image = image;
         _checked = false;
         _allowCheck = allowCheckbox;
      }

      public ListboxItem(string text, bool checkValue, bool allowCheckbox = true)
      {
         _text = text;
         _image = null;
         _checked = checkValue;
         _allowCheck = allowCheckbox;
      }

      public ListboxItem(string text, Bitmap image, bool checkValue, bool allowCheckbox = true)
      {
         _text = text;
         _image = image;
         _checked = checkValue;
         _allowCheck = allowCheckbox;
      }

      public ListboxItem(string text, Font font, Color backColor, bool allowCheckbox = true)
      {
         _text = text;
         _image = null;
         _checked = false;
         _allowCheck = allowCheckbox;
         _fnt = font;
         _bkg = backColor;
      }

      public ListboxItem(string text, Font font, Color backColor, Bitmap image, bool allowCheckbox = true)
      {
         _text = text;
         _image = image;
         _checked = false;
         _allowCheck = allowCheckbox;
         _fnt = font;
         _bkg = backColor;
      }

      public ListboxItem(string text, Font font, Color backColor, bool checkValue, bool allowCheckbox = true)
      {
         _text = text;
         _image = null;
         _checked = checkValue;
         _allowCheck = allowCheckbox;
         _fnt = font;
         _bkg = backColor;
      }

      public ListboxItem(string text, Font font, Color backColor, Bitmap image, bool checkValue, bool allowCheckbox = true)
      {
         _text = text;
         _image = image;
         _checked = checkValue;
         _allowCheck = allowCheckbox;
         _fnt = font;
         _bkg = backColor;
      }


      #endregion

      #region Properties

      public bool AllowCheckbox
      {
         get { return _allowCheck; }
         set
         {
            if (_allowCheck == value)
               return;
            _allowCheck = value;
            if (_parent != null)
               _parent.Invalidate();
         }
      }

      public Color AlternateBackColor
      {
         get { return _bkg; }
         set
         {
            if (_bkg == value)
               return;
            _bkg = value;
            if (_parent != null)
               _parent.Invalidate();
         }
      }

      public Font AlternateFont
      {
         get { return _fnt; }
         set
         {
            if (_fnt == value)
               return;
            _fnt = value;
            if (_parent != null)
               _parent.Invalidate();
         }
      }

      public bool Checked
      {
         get { return _checked; }
         set
         {
            if (_checked == value)
               return;
            _checked = value;
            if (_parent != null)
               _parent.Invalidate();
         }
      }

      public Bitmap Image
      {
         get { return _image; }
         set
         {
            if (_image == value)
               return;
            _image = value;
            if (_parent != null)
               _parent.Invalidate();
         }
      }

      internal Listbox Parent
      {
         get { return _parent; }
         set { _parent = value; }
      }

      public object Tag
      {
         get { return _tag; }
         set { _tag = value; }
      }

      public string Text
      {
         get { return _text; }
         set
         {
            if (_text == value)
               return;
            _text = value;
            if (_parent != null)
               _parent.Invalidate();
         }
      }

      internal rect Bounds
      {
         get;
         set;
      }

      internal rect CheckboxBounds
      {
         get;
         set;
      }

      #endregion

   }
}
