using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Skewworks.NETMF
{
   /// <summary>
   /// Class for reading XML into node arrays
   /// </summary>
   [Serializable]
   // ReSharper disable once InconsistentNaming
   public class XMLReaderEX
   {
      #region Variables

      private XMLNodeEX[] _nodes;

      #endregion

      #region Constructor

      //TODO: check why everything is copied to a memory stream, even if the data is already in a stream!?

      /// <summary>
      /// Load a XML file
      /// </summary>
      /// <param name="filename">Name of the XML file</param>
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

      /// <summary>
      /// Load XML file from <see cref="MemoryStream"/>
      /// </summary>
      /// <param name="ms"><see cref="MemoryStream"/> to read from</param>
      public XMLReaderEX(MemoryStream ms)
      {
         // Read XML Structure
         ReadStructure(ms);
      }

      /// <summary>
      /// Load XML file from <see cref="FileStream"/>
      /// </summary>
      /// <param name="fs"><see cref="FileStream"/> to read from</param>
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

      /// <summary>
      /// Load XML file from byte array
      /// </summary>
      /// <param name="data">Byte array to read from</param>
      public XMLReaderEX(byte[] data)
      {
         // Create the Memory Stream
         var ms = new MemoryStream(data);

         // Read XML Structure
         ReadStructure(ms);
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets the associated nodes
      /// </summary>
      public XMLNodeEX[] Nodes
      {
         get { return _nodes; }
      }

      #endregion

      #region Public Methods

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
                  {
                     return subnode;
                  }
               }
            }
         }
         return null;
      }

      /// <summary>
      /// Saves structure to an XML file
      /// </summary>
      /// <param name="filename">Full path of the file to save to</param>
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
         {
            File.Delete(filename);
         }
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
}