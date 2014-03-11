using System;
using Microsoft.SPOT;

namespace Skewworks.Tinkr.Controls
{
    [Serializable]
    public class ListviewItem
    {

        #region Variables

        protected internal string[] _values;
        private object _tag;

        #endregion

        #region Constructors

        public ListviewItem(string value)
        {
            _values = new string[] { value };
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
                if (_values == value)
                    return;
                _values = value;
            }
        }

        #endregion

    }
}
