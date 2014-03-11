using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Ext.Xml;

namespace Skewworks.NETMF
{

    [Serializable]
    public class XMLReaderEX
    {

        #region Variables

        private XMLNodeEX[] _nodes;

        #endregion

        #region Constructor

        public XMLReaderEX(string Filename)
        {
            // Read the file
            FileStream iFile = new FileStream(Filename, FileMode.Open);
            byte[] data = new byte[iFile.Length];
            iFile.Read(data, 0, data.Length);
            iFile.Close();

            // Create the Memory Stream
            MemoryStream ms = new MemoryStream(data);

            // Read XML Structure
            ReadStructure(ms);
        }

        public XMLReaderEX(MemoryStream MS)
        {
            // Read XML Structure
            ReadStructure(MS);
        }

        public XMLReaderEX(FileStream FS)
        {
            byte[] data = new byte[FS.Length];
            FS.Read(data, 0, data.Length);
            FS.Close();

            // Create the Memory Stream
            MemoryStream ms = new MemoryStream(data);

            // Read XML Structure
            ReadStructure(ms);
        }

        public XMLReaderEX(byte[] Data)
        {
            // Create the Memory Stream
            MemoryStream ms = new MemoryStream(Data);

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

        public XMLNodeEX NodeByName(string Name)
        {
            XMLNodeEX subnode;
            Name = Name.ToLower();
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i].Name.ToLower() == Name)
                    return _nodes[i];
                else if (_nodes[i].Nodes.Length > 0)
                {
                    for (int j = 0; j < _nodes[i].Nodes.Length; j++)
                    {
                        subnode = CheckSubNode(_nodes[i].Nodes[j], Name);
                        if (subnode != null)
                            return subnode;
                    }
                }
            }
            return null;
        }

        public void Save(string Filename)
        {
            string XML = string.Empty;

            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i].Nodes.Length == 0)
                    XML = string.Concat(XML, "<" + _nodes[i].ShortName + ">" + _nodes[i].Value + "</" + _nodes[i].ShortName + ">");
                else
                    XML = string.Concat(XML, "<" + _nodes[i].ShortName + ">" + SubNodeData(_nodes[i]) + "</" + _nodes[i].ShortName + ">");
            }

            byte[] b = UTF8Encoding.UTF8.GetBytes(XML);
            if (File.Exists(Filename))
                File.Delete(Filename);
            FileStream iFile = new FileStream(Filename, FileMode.CreateNew, FileAccess.Write);
            iFile.Write(b, 0, b.Length);
            iFile.Close();
        }

        #endregion

        #region Private Methods

        private void AddNode(XMLNodeEX node)
        {
            if (_nodes == null)
                _nodes = new XMLNodeEX[] { node };
            else
            {
                XMLNodeEX[] tmp = new XMLNodeEX[_nodes.Length + 1];
                Array.Copy(_nodes, tmp, _nodes.Length);
                tmp[tmp.Length - 1] = node;
                _nodes = tmp;
                tmp = null;
            }
        }

        private XMLNodeEX CheckSubNode(XMLNodeEX node, string Name)
        {
            XMLNodeEX subnode;

            Name = Name.ToLower();

            if (node.Name.ToLower() == Name)
                return node;

            if (node.Nodes != null)
            {
                for (int i = 0; i < node.Nodes.Length; i++)
                {
                    subnode = node.Nodes[i];
                    if (subnode.Name.ToLower() == Name)
                        return subnode;
                    else if (node.Nodes.Length > 0)
                    {
                        for (int j = 0; j < node.Nodes.Length; j++)
                        {
                            subnode = CheckSubNode(node.Nodes[i], Name);
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
            XMLNodeEX subn = null;
            XMLNodeEX curn = null;

            // Create Reader Settings
            XmlReaderSettings ss = new XmlReaderSettings();
            ss.IgnoreWhitespace = true;
            ss.IgnoreComments = false;

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
                            subn = new XMLNodeEX(xr.Name, curn);
                            curn.AddNode(subn);
                            curn = subn;
                        }
                        break;
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        curn.Value = xr.Value;
                        break;
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.None:
                        // We do not care
                        break;
                    case XmlNodeType.EndElement:
                        curn = curn.Parent;
                        if (curn == null) node = null;
                        break;
                }
            }

        }

        private string SubNodeData(XMLNodeEX node)
        {
            string XML = string.Empty;

            for (int i = 0; i < node.Nodes.Length; i++)
            {
                if (node.Nodes[i].Nodes.Length == 0)
                    XML = string.Concat(XML, "<" + node.Nodes[i].ShortName + ">" + node.Nodes[i].Value + "</" + node.Nodes[i].ShortName + ">");
                else
                    XML = string.Concat(XML, "<" + node.Nodes[i].ShortName + ">" + SubNodeData(node.Nodes[i]) + "</" + node.Nodes[i].ShortName + ">");
            }

            return XML;
        }

        #endregion

    }

    public class XMLNodeEX
    {

        #region Variables

        private string _name;
        private string _shortname;
        private string _value;
        private XMLNodeEX[] _nodes;
        private XMLNodeEX _parent = null;

        #endregion

        #region Constructor

        public XMLNodeEX(XMLNodeEX Parent, string Name, string Value)
        {
            _name = Parent.Name + "\\" + Name;
            _shortname = Name;
            _parent = Parent;
            _value = Value;
        }

        internal XMLNodeEX(string Name)
        {
            _name = Name;
            _shortname = Name;
        }

        internal XMLNodeEX(string Name, XMLNodeEX Parent)
        {
            _name = Parent.Name + "\\" + Name;
            _shortname = Name;
            _parent = Parent;
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            internal set { _name = Value; }
        }

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

        public string Value
        {
            get { return _value; }
            internal set { _value = Value; }
        }

        public bool ValueToBool
        {
            get
            {
                if (_value.ToLower() == "false" || _value == "0") return false;
                return true;
            }
        }

        public int ValueToInt
        {
            get { return int.Parse(_value); }
        }

        #endregion

        #region Public Methods

        public void AddNode(XMLNodeEX node)
        {
            if (_nodes == null)
                _nodes = new XMLNodeEX[] { node };
            else
            {
                XMLNodeEX[] tmp = new XMLNodeEX[_nodes.Length + 1];
                Array.Copy(_nodes, tmp, _nodes.Length);
                tmp[tmp.Length - 1] = node;
                _nodes = tmp;
                tmp = null;
            }
        }

        public XMLNodeEX NodeByName(string Name)
        {
            Name = Name.ToLower();
            for (int i = 0; i < _nodes.Length; i++)
                if (_nodes[i].Name.ToLower() == Name || _nodes[i].ShortName.ToLower() == Name)
                    return _nodes[i];
            return null;
        }

        #endregion

    }

}
