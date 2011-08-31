using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SIWViewer
{
    public class DetailTreeNode
    {
        XmlNode xmlNode_;
        XmlNode pageNode_;
        List<DetailTreeNode> children_;

        public DetailTreeNode(XmlNode xNode)
        {
            xmlNode_ = xNode;
            pageNode_ = getParentPageElem(xmlNode_);
        }

        public IEnumerable<DetailTreeNode> ChildNodes()
        {
            if (children_ == null)
            {
                children_ = new List<DetailTreeNode>();
                foreach (XmlNode kid in xmlNode_.ChildNodes)
                {
                    if (kid is XmlElement)
                    {
                        children_.Add(new DetailTreeNode(kid));
                    }
                }
            }
            return children_;
        }

        public bool isLeaf()
        {
            return xmlNode_.ChildNodes.Count == 0;
        }

        // The Name is used by the tree to display the first tree column.
        // This is always H1.
        public String Name
        {
            get
            {
                return H1;
            }
        }

        private String H(int i)
        {
            XmlNode attrH = pageNode_.Attributes.GetNamedItem("H" + i);
            if (attrH != null)
            {
                String attrNm = attrH.Value.Replace(' ', '_');
                XmlNode attr = xmlNode_.Attributes.GetNamedItem(attrNm);
                if (attr != null)
                {
                    return attr.Value.Replace('\n', ' ');
                }
            }
            return null;
        }

        private XmlNode getParentPageElem(XmlNode xmlNode)
        {
            while (xmlNode != null)
            {
                if (xmlNode.Name == "page")
                {
                    return xmlNode;
                }
                xmlNode = xmlNode.ParentNode;
            }
            return null;
        }

        public String H1
        {
            get
            {
                return H(1);
            }
        }
        public String H2
        {
            get
            {
                return H(2);
            }
        }
        public String H3
        {
            get
            {
                return H(3);
            }
        }
        public String H4
        {
            get
            {
                return H(4);
            }
        }
        public String H5
        {
            get
            {
                return H(5);
            }
        }
        public String H6
        {
            get
            {
                return H(6);
            }
        }
        public String H7
        {
            get
            {
                return H(7);
            }
        }
        public String H9
        {
            get
            {
                return H(9);
            }
        }
        public String H10
        {
            get
            {
                return H(10);
            }
        }
        public String H11
        {
            get
            {
                return H(11);
            }
        }
        public String H12
        {
            get
            {
                return H(12);
            }
        }
        public String H13
        {
            get
            {
                return H(13);
            }
        }
        public String H14
        {
            get
            {
                return H(14);
            }
        }
        public String H15
        {
            get
            {
                return H(15);
            }
        }
        public String H16
        {
            get
            {
                return H(16);
            }
        }
        public String H17
        {
            get
            {
                return H(17);
            }
        }
        public String H19
        {
            get
            {
                return H(19);
            }
        }
        public String H20
        {
            get
            {
                return H(20);
            }
        }
    }
}
