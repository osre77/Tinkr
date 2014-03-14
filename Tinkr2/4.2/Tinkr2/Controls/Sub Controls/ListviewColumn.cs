using System;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class ListviewColumn
   {
      #region Variables

      private string _text;
      private readonly string _name;

      protected internal int X { get; set; }
      protected internal int Y { get; set; }
      protected internal int W { get; set; }
      protected internal int H { get; set; }
      protected internal Listview Parent { get; set; }

      #endregion

      #region Constructors

      public ListviewColumn(string name, string text)
      {
         _name = name;
         _text = text;
         W = -1;
      }

      public ListviewColumn(string name, string text, int width)
      {
         _name = name;
         _text = text;
         W = width;
      }

      #endregion

      #region Properties

      public string Name
      {
         get { return _name; }
      }

      public string Text
      {
         get { return _text; }
         set
         {
            if (_text == value)
               return;
            _text = value;
            if (Parent != null)
               Parent.Invalidate();
         }
      }

      public int Width
      {
         get { return W; }
         set
         {
            if (W == value || value < 1)
               return;
            W = value;
            if (Parent != null)
               Parent.Invalidate();
         }
      }

      #endregion

   }
}
