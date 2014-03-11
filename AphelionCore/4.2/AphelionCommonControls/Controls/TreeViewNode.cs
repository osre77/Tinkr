using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class TreeviewNode : MarshalByRefObject
    {

        #region Variables

        private rect _Bounds;
        private bool _sel = false;
        private object _tag;
        private string _text;
        private TreeviewNode _parent;
        private Treeview _container;
        private TreeviewNode[] _nodes;
        protected internal bool _expanded = false;
        private bool _mDown = false;

        #endregion

        #region Constructors

        public TreeviewNode(string text)
        {
            _text = text;
        }

        public TreeviewNode(string text, object tag)
        {
            _text = text;
            _tag = tag;
        }

        #endregion

        #region Properties

        internal rect Bounds
        {
            get { return _Bounds; }
            set { _Bounds = value; }
        }

        /// <summary>
        /// Gets containing treeview
        /// </summary>
        public Treeview Container
        {
            get { return _container; }
            internal set 
            {
                _container = value;

                if (_nodes != null)
                {
                    for (int i = 0; i < _nodes.Length; i++)
                        _nodes[i].Container = _container;
                }

            }
        }

        /// <summary>
        /// Gets/Sets expanded state
        /// </summary>
        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_nodes == null || _nodes.Length == 0)
                    value = false;
                if (_expanded == value)
                    return;
                _expanded = value;
                if (_container != null)
                {
                    _container.Invalidate();


                    if (_expanded)
                        _container.ChildExpanded(this);
                    else
                        _container.ChildCollapsed(this);
                }
            }
        }

        /// <summary>
        /// Gets number of subnodes
        /// </summary>
        public int Length
        {
            get 
            {
                if (_nodes == null)
                    return 0;
                return _nodes.Length;
            }
        }

        public TreeviewNode[] Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes == value)
                    return;
                _nodes = value;

                if (_nodes != null)
                    for (int i = 0; i < _nodes.Length; i++)
                        _nodes[i].Container = _container;

                if (_container != null)
                    _container.Invalidate();
            }
        }

        /// <summary>
        /// Gets containing treeviewnode
        /// </summary>
        public TreeviewNode Parent
        {
            get { return _parent; }
            internal set { _parent = value; }
        }

        internal bool PenDown
        {
            get { return _mDown; }
            set { _mDown = value; }
        }

        /// <summary>
        /// Gets selected state
        /// </summary>
        public bool Selected
        {
            get { return _sel; }
            internal set { _sel = value; }
        }

        /// <summary>
        /// Gets/Sets tag
        /// </summary>
        public object Tag
        {
            get { return _tag; }
            set
            {
                if (_tag == value)
                    return;
                _tag = value;
            }
        }

        /// <summary>
        /// Gets/Sets text
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                if (_container != null)
                    _container.Render(true);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a subnode
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(TreeviewNode node)
        {
            node.Parent = this;
            node.Container = _container;

            if (_nodes == null)
                _nodes = new TreeviewNode[] { node };
            else
            {
                TreeviewNode[] tmp = new TreeviewNode[_nodes.Length + 1];
                Array.Copy(_nodes, tmp, _nodes.Length);
                tmp[tmp.Length - 1] = node;
                _nodes = tmp;
                tmp = null;
            }

            if (_expanded || _nodes.Length == 1 && _container != null)
                _container.Invalidate();
        }

        /// <summary>
        /// Removes all subnodes
        /// </summary>
        public void ClearNodes()
        {
            _nodes = null;
            Expanded = false;
        }

        /// <summary>
        /// Removes a specific subnode
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(TreeviewNode node)
        {
            if (_nodes == null)
                return;

            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i] == node)
                {
                    RemoveNodeAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Removes a subnode from a specific point in the array
        /// </summary>
        /// <param name="index"></param>
        public void RemoveNodeAt(int index)
        {
            if (_nodes == null)
                return;
            if (_nodes.Length == 1)
                _nodes = null;
            else
            {
                TreeviewNode[] tmp = new TreeviewNode[_nodes.Length];
                int c = 0;
                for (int i = 0; i < _nodes.Length; i++)
                {
                    if (i != index)
                        tmp[c++] = _nodes[i];
                }
                _nodes = tmp;
            }

            Expanded = _expanded;   // Automatically updates for 0 nodes
        }

        #endregion

    }
}
