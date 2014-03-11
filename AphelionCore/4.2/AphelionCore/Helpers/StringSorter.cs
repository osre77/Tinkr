using System;
using System.Collections;

using Microsoft.SPOT;

namespace Skewworks.NETMF
{
    public class StringSorter
    {

        #region Variables

        private ArrayList _values;

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
            string[] str = new string[_values.Count];
            SortableString[] Strings;
            int i;

            Strings = (SortableString[])_values.ToArray(typeof(SortableString));
            for (i = 0; i < Strings.Length; i++)
                Strings[i].Insensitive = true;

            Strings = SortableString.Sort(Strings);

            for (i = 0; i < str.Length; i++)
                str[i] = Strings[i].Value;

            return str;
        }

        /// <summary>
        /// Sorts strings (case-insensitive, descending)
        /// </summary>
        /// <returns></returns>
        public string[] InsensitiveSortDescending()
        {
            string[] str = new string[_values.Count];
            SortableString[] Strings;
            int i;

            Strings = (SortableString[])_values.ToArray(typeof(SortableString));
            for (i = 0; i < Strings.Length; i++)
                Strings[i].Insensitive = true;

            Strings = SortableString.Sort(Strings);

            for (i = str.Length - 1; i > -1; i--)
                str[i] = Strings[i].Value;

            return str;
        }

        /// <summary>
        /// Sorts strings (case-sensitive, ascending)
        /// </summary>
        /// <returns></returns>
        public string[] Sort()
        {
            string[] str = new string[_values.Count];
            SortableString[] Strings;

            Strings = SortableString.Sort((SortableString[])_values.ToArray(typeof(SortableString)));

            for (int i = 0; i < str.Length; i++)
                str[i] = Strings[i].Value;

            return str;
        }

        /// <summary>
        /// Sorts strings (case-sensitive, descending)
        /// </summary>
        /// <returns></returns>
        public string[] SortDescending()
        {
            string[] str = new string[_values.Count];
            SortableString[] Strings;

            Strings = SortableString.Sort((SortableString[])_values.ToArray(typeof(SortableString)));

            for (int i = str.Length - 1; i > -1; i--)
                str[i] = Strings[i].Value;

            return str;
        }

        #endregion

        #region Private Classes

        private class SortableString : IComparable
        {

            #region Variables

            private string _value;
            private bool _insensitive;

            #endregion

            #region Constructor

            public SortableString(string value)
            {
                _value = value;
            }

            #endregion

            #region Properties

            public bool Insensitive
            {
                get { return _insensitive; }
                set { _insensitive = value; }
            }

            public string Value
            {
                get { return _value; }
                set { _value = value; }
            }

            #endregion

            #region Public Methods

            public int CompareTo(object x)
            {
                // We can’t check 0
                if (x == null) return 0;

                // Check if object under test is a SortableTimeZoneInfo
                if (x is SortableString)
                {
                    if (!Insensitive)
                        return Value.CompareTo(((SortableString)x).Value);
                    else
                        return Value.ToLower().CompareTo(((SortableString)x).Value.ToLower());
                }
                else
                    throw new ArgumentException("Must be a SortableString");
            }

            public static SortableString[] Sort(SortableString[] List)
            {
                SortableString temp;

                for (int pos = List.Length - 2; pos >= 0; pos--)
                {
                    for (int scan = 0; scan <= pos; scan++)
                    {
                        if (List[scan].CompareTo(List[scan + 1]) > 0)
                        {
                            temp = List[scan];
                            List[scan] = List[scan + 1];
                            List[scan + 1] = temp;
                        }
                    }
                }

                return List;
            }

            #endregion

        }

        #endregion

    }
}
