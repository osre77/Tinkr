using System;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;

namespace Skewworks.Tinkr.Controls
{
    [Serializable]
    public class LineGraphItem
    {

        #region Variables

        private Color _color;
        private precisionpoint[] _pts;
        private LineGraph _parent;
        private bool _visible;

        #endregion

        #region Constructors

        public LineGraphItem(Color color)
        {
            _color = color;
            _visible = true;
        }

        public LineGraphItem(Color color, precisionpoint point)
        {
            _color = color;
            _pts = new[] { point };
            _visible = true;
        }

        public LineGraphItem(Color color, precisionpoint[] points)
        {
            _color = color;
            _pts = points;
            _visible = true;
        }

        #endregion

        #region Properties

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                if (_parent != null)
                    _parent.Invalidate();
            }
        }

        internal LineGraph Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value)
                    return;
                _parent = value;
                if (_parent != null)
                    _parent.Invalidate();
            }
        }

        public precisionpoint[] Points
        {
            get { return _pts; }
            set
            {
                _pts = value;
                if (_parent != null)
                    _parent.Invalidate();
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible == value)
                    return;
                _visible = value;
                if (_parent != null)
                    _parent.Invalidate();
            }
        }

        #endregion

        #region Public Methods

        public void AddPoint(precisionpoint point)
        {
           if (_pts == null)
           {
              _pts = new[] { point };
           }
            else
            {
                var tmp = new precisionpoint[_pts.Length + 1];
                Array.Copy(_pts, tmp, _pts.Length);
                tmp[tmp.Length - 1] = point;
                _pts = tmp;
            }

            if (_parent != null)
                _parent.Invalidate();
        }

        public void AddPoints(precisionpoint[] points)
        {
            if (_pts == null)
                _pts = points;
            else
            {
                var tmp = new precisionpoint[_pts.Length + points.Length];
                Array.Copy(_pts, tmp, _pts.Length);
                Array.Copy(points, 0, tmp, _pts.Length, points.Length);
                _pts = tmp;
            }

            if (_parent != null)
                _parent.Invalidate();
        }

        public void ClearPoints()
        {
            _pts = null;
            if (_parent != null)
                _parent.Invalidate();
        }

        public void RemovePoint(precisionpoint point)
        {
            if (_pts == null)
                return;

            for (int i = 0; i < _pts.Length; i++)
            {
                if (Math.Abs(_pts[i].X - point.X) <= Single.Epsilon && Math.Abs(_pts[i].Y - point.Y) <= Single.Epsilon)
                {
                    RemovePointAt(i);
                    return;
                }
            }
        }

        public void RemovePointAt(int index)
        {
            if (_pts == null || index < 0 || index >= _pts.Length)
                return;

            if (_pts.Length == 1)
                _pts = null;
            else
            {
                var tmp = new precisionpoint[_pts.Length - 1];
                int c = 0;
                for (int i = 0; i < _pts.Length; i++)
                {
                    if (i != index)
                        tmp[c++] = _pts[i];
                }
                _pts = tmp;
            }

            if (_parent != null)
                _parent.Invalidate();
        }

        #endregion

    }
}
