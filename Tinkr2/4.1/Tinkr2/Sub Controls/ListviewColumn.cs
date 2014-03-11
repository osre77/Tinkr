using System;
using Microsoft.SPOT;

namespace Skewworks.Tinkr.Controls
{
    [Serializable]
    public class ListviewColumn
    {

        #region Variables

        protected internal int _x, _y, _w, _h;
        private string _text;
        private string _name;
        protected internal Listview _parent;

        #endregion

        #region Constructors

        public ListviewColumn(string name, string text)
        {
            _name = name;
            _text = text;
            _w = -1;
        }

        public ListviewColumn(string name, string text, int width)
        {
            _name = name;
            _text = text;
            _w = width;
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
                if (_parent != null)
                    _parent.Invalidate();
            }
        }

        public int Width
        {
            get { return _w; }
            set
            {
                if (_w == value || value < 1)
                    return;
                _w = value;
                if (_parent != null)
                    _parent.Invalidate();
            }
        }

        #endregion

    }
}
