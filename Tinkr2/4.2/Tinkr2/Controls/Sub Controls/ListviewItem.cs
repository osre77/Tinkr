using System;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class ListviewItem
   {

      #region Variables

      private string[] _values;
      private object _tag;

      #endregion

      #region Constructors

      public ListviewItem(string value)
      {
         _values = new[] { value };
      }

      public ListviewItem(string[] values)
      {
         _values = values;
      }

      #endregion

      #region Properties

      public object Tag
      {
         get { return _tag; }
         set { _tag = value; }
      }

      public string[] Values
      {
         get { return _values; }
         set
         {
            _values = value;
         }
      }

      #endregion

   }
}
