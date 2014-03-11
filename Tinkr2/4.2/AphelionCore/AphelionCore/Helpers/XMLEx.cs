using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Skewworks.NETMF
{

   [Serializable]
   // ReSharper disable once InconsistentNaming
   public class XMLReaderEX
   {

      #region Variables

      private XMLNodeEX[] _nodes;

      #endregion

      #region Constructor

      public XMLReaderEX(string filename)
      {
         // Read the file
         var iFile = new FileStream(filename, FileMode.Open);
         var data = new byte[(int)iFile.Length];
         iFile.Read(data, 0, data.Length);
         iFile.Close();

         // Create the Memory Stream
         var ms = new MemoryStream(data);

         // Read XML Structure
         ReadStructure(ms);
      }

      public XMLReaderEX(MemoryStream ms)
      {
         // Read XML Structure
         ReadStructure(ms);
      }

      public XMLReaderEX(FileStream fs)
      {
         var data = new byte[(int)fs.Length];
         fs.Read(data, 0, data.Length);
         fs.Close();

         // Create the Memory Stream
         var ms = new MemoryStream(data);

         // Read XML Structure
         ReadStructure(ms);
      }

      public XMLReaderEX(byte[] data)
      {
         // Create the Memory Stream
         var ms = new MemoryStream(data);

         // Read XML Structure
         ReadStructure(ms);
      }

      #endregion

      #region Properties

      public XMLNodeEX[] Nodes
      {
         get { return _nodes; }
      }

      #endregion

      #region Public Methods

      public XMLNodeEX NodeByName(string name)
      {
         name = name.ToLower();
         for (int i = 0; i < _nodes.Length; i++)
         {
            if (_nodes[i].Name.ToLower() == name)
            {
               return _nodes[i];
            }
            if (_nodes[i].Nodes.Length > 0)
            {
               for (int j = 0; j < _nodes[i].Nodes.Length; j++)
               {
                  XMLNodeEX subnode = CheckSubNode(_nodes[i].Nodes[j], name);
                  if (subnode != null)
                     return subnode;
               }
            }
         }
         return null;
      }

      public void Save(string filename)
      {
         string xml = string.Empty;

         for (int i = 0; i < _nodes.Length; i++)
         {
            if (_nodes[i].Nodes.Length == 0)
            {
               xml = string.Concat(xml,
                  "<" + _nodes[i].ShortName + ">" + _nodes[i].Value + "</" + _nodes[i].ShortName + ">");
            }
            else
            {
               xml = string.Concat(xml, "<" + _nodes[i].ShortName + ">" + SubNodeData(_nodes[i]) + "</" + _nodes[i].ShortName + ">");
            }
         }

         byte[] b = Encoding.UTF8.GetBytes(xml);
         if (File.Exists(filename))
            File.Delete(filename);
         var iFile = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);
         iFile.Write(b, 0, b.Length);
         iFile.Close();
      }

      #endregion

      #region Private Methods

      private void AddNode(XMLNodeEX node)
      {
         if (_nodes == null)
            _nodes = new[] { node };
         else
         {
            var tmp = new XMLNodeEX[_nodes.Length + 1];
            Array.Copy(_nodes, tmp, _nodes.Length);
            tmp[tmp.Length - 1] = node;
            _nodes = tmp;
         }
      }

      private XMLNodeEX CheckSubNode(XMLNodeEX node, string name)
      {
         name = name.ToLower();

         if (node.Name.ToLower() == name)
            return node;

         if (node.Nodes != null)
         {
            for (int i = 0; i < node.Nodes.Length; i++)
            {
               XMLNodeEX subnode = node.Nodes[i];
               if (subnode.Name.ToLower() == name)
               {
                  return subnode;
               }
               if (node.Nodes.Length > 0)
               {
                  for (int j = 0; j < node.Nodes.Length; j++)
                  {
                     subnode = CheckSubNode(node.Nodes[i], name);
                     if (subnode != null)
                        return subnode;
                  }
               }
            }
         }

         return null;
      }

      private void ReadStructure(MemoryStream ms)
      {
         // Create Base Node
         XMLNodeEX node = null;
         XMLNodeEX curn = null;

         // Create Reader Settings
         var ss = new XmlReaderSettings
         {
            IgnoreWhitespace = true, 
            IgnoreComments = false
         };

         // Create Reader
         XmlReader xr = XmlReader.Create(ms, ss);

         // Loop through nodes to create structure
         while (!xr.EOF)
         {
            xr.Read();
            switch (xr.NodeType)
            {
               case XmlNodeType.Element:
                  if (node == null)
                  {
                     node = new XMLNodeEX(xr.Name);
                     curn = node;
                     AddNode(node);
                  }
                  else
                  {
                     var subn = new XMLNodeEX(xr.Name, curn);
                     curn.AddNode(subn);
                     curn = subn;
                  }
                  break;
               case XmlNodeType.CDATA:
               case XmlNodeType.Text:
                  if (curn != null)
                  {
                     curn.Value = xr.Value;
                  }
                  break;
               case XmlNodeType.XmlDeclaration:
               case XmlNodeType.Comment:
               case XmlNodeType.Whitespace:
               case XmlNodeType.None:
                  // We do not care
                  break;
               case XmlNodeType.EndElement:
                  if (curn != null)
                  {
                     curn = curn.Parent;
                     if (curn == null)
                     {
                        node = null;
                     }
                  }
                  break;
            }
         }

      }

      private string SubNodeData(XMLNodeEX node)
      {
         string xml = string.Empty;

         for (int i = 0; i < node.Nodes.Length; i++)
         {
            if (node.Nodes[i].Nodes.Length == 0)
            {
               xml = string.Concat(xml,
                  "<" + node.Nodes[i].ShortName + ">" + node.Nodes[i].Value + "</" + node.Nodes[i].ShortName + ">");
            }
            else
            {
               xml = string.Concat(xml, "<" + node.Nodes[i].ShortName + ">" + SubNodeData(node.Nodes[i]) + "</" + node.Nodes[i].ShortName + ">");
            }
         }

         return xml;
      }

      #endregion

   }

   // ReSharper disable once InconsistentNaming
   public class XMLNodeEX
   {

      #region Variables

      private readonly string _shortname;
      private XMLNodeEX[] _nodes;
      private readonly XMLNodeEX _parent;

      #endregion

      #region Constructor

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

      public string Name { get; internal set; }

      public XMLNodeEX[] Nodes
      {
         get { return _nodes; }
      }

      public XMLNodeEX Parent
      {
         get { return _parent; }
      }

      public string ShortName
      {
         get { return _shortname; }
      }

      public string Value { get; internal set; }

      public bool ValueToBool
      {
         get
         {
            if (Value.ToLower() == "false" || Value == "0") return false;
            return true;
         }
      }

      public int ValueToInt
      {
         get { return int.Parse(Value); }
      }

      #endregion

      #region Public Methods

      public void AddNode(XMLNodeEX node)
      {
         if (_nodes == null)
            _nodes = new[] { node };
         else
         {
            var tmp = new XMLNodeEX[_nodes.Length + 1];
            Array.Copy(_nodes, tmp, _nodes.Length);
            tmp[tmp.Length - 1] = node;
            _nodes = tmp;
         }
      }

      public XMLNodeEX NodeByName(string name)
      {
         name = name.ToLower();
         for (int i = 0; i < _nodes.Length; i++)
            if (_nodes[i].Name.ToLower() == name || _nodes[i].ShortName.ToLower() == name)
               return _nodes[i];
         return null;
      }

      #endregion

   }

}
