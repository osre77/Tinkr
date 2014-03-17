using System;

namespace Skewworks.NETMF
{
   // ReSharper disable once InconsistentNaming
   /// <summary>
   /// Class for working with XML nodes
   /// </summary>
   public class XMLNodeEX
   {
      #region Variables

      private readonly string _shortname;
      private XMLNodeEX[] _nodes;
      private readonly XMLNodeEX _parent;

      #endregion

      #region Constructor

      /// <summary>
      /// Creates a new node
      /// </summary>
      /// <param name="parent">Parent node</param>
      /// <param name="name">Name of the node</param>
      /// <param name="value">Value of the node</param>
      public XMLNodeEX(XMLNodeEX parent, string name, string value)
      {
         Name = parent.Name + "\\" + name;
         _shortname = name;
         _parent = parent;
         Value = value;
      }

      internal XMLNodeEX(string name)
      {
         Name = name;
         _shortname = name;
      }

      internal XMLNodeEX(string name, XMLNodeEX parent)
      {
         Name = parent.Name + "\\" + name;
         _shortname = name;
         _parent = parent;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets the node's name
      /// </summary>
      public string Name { get; internal set; }

      /// <summary>
      /// Gets the associated nodes
      /// </summary>
      public XMLNodeEX[] Nodes
      {
         get { return _nodes; }
      }

      /// <summary>
      /// Gets the object's parent
      /// </summary>
      public XMLNodeEX Parent
      {
         get { return _parent; }
      }

      /// <summary>
      /// Get's the short name for the object
      /// </summary>
      public string ShortName
      {
         get { return _shortname; }
      }

      /// <summary>
      /// Gets the value as a string
      /// </summary>
      public string Value { get; internal set; }

      /// <summary>
      /// Gets the value as a boolean
      /// </summary>
      public bool ValueToBool
      {
         get
         {
            if (Value.ToLower() == "false" || Value == "0") return false;
            return true;
         }
      }

      /// <summary>
      /// Gets the value as an integer
      /// </summary>
      public int ValueToInt
      {
         get { return int.Parse(Value); }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds a node
      /// </summary>
      /// <param name="node">Node to be added</param>
      public void AddNode(XMLNodeEX node)
      {
         if (_nodes == null)
         {
            _nodes = new[] { node };
         }
         else
         {
            var tmp = new XMLNodeEX[_nodes.Length + 1];
            Array.Copy(_nodes, tmp, _nodes.Length);
            tmp[tmp.Length - 1] = node;
            _nodes = tmp;
         }
      }

      /// <summary>
      /// Returns an XMLNodeEX by it's name
      /// </summary>
      /// <param name="name">Name of the node to retrieve</param>
      /// <returns>Returns the node or null if not found</returns>
      public XMLNodeEX NodeByName(string name)
      {
         name = name.ToLower();
         for (int i = 0; i < _nodes.Length; i++)
         {
            if (_nodes[i].Name.ToLower() == name || _nodes[i].ShortName.ToLower() == name)
            {
               return _nodes[i];
            }
         }
         return null;
      }
      #endregion
   }
}
