using System;
using System.Collections;

// ReSharper disable StringCompareToIsCultureSpecific
namespace Skewworks.NETMF
{
   public class StringSorter
   {

      #region Variables

      private readonly ArrayList _values;

      #endregion

      #region Constructor

      public StringSorter()
      {
         _values = new ArrayList();
      }

      public StringSorter(string[] values)
      {
         _values = new ArrayList();
         for (int i = 0; i < values.Length; i++)
            _values.Add(new SortableString(values[i]));
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds a string to the list to be sorted
      /// </summary>
      /// <param name="value"></param>
      public void AddValue(string value)
      {
         _values.Add(new SortableString(value));
      }

      /// <summary>
      /// Removes all values from list to be sorted
      /// </summary>
      public void ClearValues()
      {
         _values.Clear();
      }

      /// <summary>
      /// Sorts strings (case-insensitive, ascending)
      /// </summary>
      /// <returns></returns>
      public string[] InsensitiveSort()
      {
         var str = new string[_values.Count];
         int i;

         var strings = (SortableString[])_values.ToArray(typeof(SortableString));
         for (i = 0; i < strings.Length; i++)
            strings[i].Insensitive = true;

         strings = SortableString.Sort(strings);

         for (i = 0; i < str.Length; i++)
            str[i] = strings[i].Value;

         return str;
      }

      /// <summary>
      /// Sorts strings (case-insensitive, descending)
      /// </summary>
      /// <returns></returns>
      public string[] InsensitiveSortDescending()
      {
         var str = new string[_values.Count];
         int i;

         var strings = (SortableString[])_values.ToArray(typeof(SortableString));
         for (i = 0; i < strings.Length; i++)
            strings[i].Insensitive = true;

         strings = SortableString.Sort(strings);

         for (i = str.Length - 1; i > -1; i--)
            str[i] = strings[i].Value;

         return str;
      }

      /// <summary>
      /// Sorts strings (case-sensitive, ascending)
      /// </summary>
      /// <returns></returns>
      public string[] Sort()
      {
         var str = new string[_values.Count];

         SortableString[] strings = SortableString.Sort((SortableString[])_values.ToArray(typeof(SortableString)));

         for (int i = 0; i < str.Length; i++)
            str[i] = strings[i].Value;

         return str;
      }

      /// <summary>
      /// Sorts strings (case-sensitive, descending)
      /// </summary>
      /// <returns></returns>
      public string[] SortDescending()
      {
         var str = new string[_values.Count];

         SortableString[] strings = SortableString.Sort((SortableString[])_values.ToArray(typeof(SortableString)));

         for (int i = str.Length - 1; i > -1; i--)
            str[i] = strings[i].Value;

         return str;
      }

      #endregion

      #region Private Classes

      private class SortableString : IComparable
      {

         #region Variables

         #endregion

         #region Constructor

         public SortableString(string value)
         {
            Value = value;
         }

         #endregion

         #region Properties

         // ReSharper disable once MemberCanBePrivate.Local
         public bool Insensitive { get; set; }

         public string Value { get; private set; }

         #endregion

         #region Public Methods

         public int CompareTo(object x)
         {
            // We can’t check 0
            if (x == null) return 0;

            // Check if object under test is a SortableTimeZoneInfo
            var sortableString = x as SortableString;
            if (sortableString != null)
            {
               if (!Insensitive)
               {
                  return Value.CompareTo(sortableString.Value);
               }
               return Value.ToLower().CompareTo(sortableString.Value.ToLower());
            }
            throw new ArgumentException("Must be a SortableString");
         }

         public static SortableString[] Sort(SortableString[] list)
         {
            for (int pos = list.Length - 2; pos >= 0; pos--)
            {
               for (int scan = 0; scan <= pos; scan++)
               {
                  if (list[scan].CompareTo(list[scan + 1]) > 0)
                  {
                     SortableString temp = list[scan];
                     list[scan] = list[scan + 1];
                     list[scan + 1] = temp;
                  }
               }
            }

            return list;
         }

         #endregion

      }

      #endregion

   }
}
// ReSharper restore StringCompareToIsCultureSpecific
