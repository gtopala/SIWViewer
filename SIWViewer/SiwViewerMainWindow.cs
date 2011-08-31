using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Aga.Controls.Tree.NodeControls;

namespace SIWViewer
{
    public partial class SiwViewerMainWindow : Form
    {
        private XmlDocument dom;

        public SiwViewerMainWindow()
        {
            InitializeComponent();
            // add the listeners to the tree...
            treeView.BeforeExpand += new TreeViewCancelEventHandler(treeView1_BeforeExpand);
            treeView.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            // some initializations...
            initControls();
//            System.Environment.GetCommandLineArgs();
            String[] arguments = Environment.GetCommandLineArgs();
            if (arguments.GetLength(0) == 2)
            {
                processFile(arguments[1]);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog dlg = new AboutDialog();
            dlg.ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /**
         * Loading from the XML file ... which has the following structure
         * <report [attrs]>
         *    <xml_version>
         *    <page>
         *      <item [attrs] >
         *          <details title="aaa">
         *      ...or...
         *      <details title="aaa">
         *          <item [attrs]>
         * 
         */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("OPEN what ...");
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FilterIndex = 1;
            // dlg.Filter = "SIW Files (*.xml) | *.xml|SIW Files (*.csv) | *.csv";
            dlg.Filter = "SIW Files (*.xml) | *.xml";
            if (DialogResult.OK == dlg.ShowDialog(this))
            {
                String fileName = dlg.FileName;
                processFile(fileName);
            }
        }

        private void processFile(String fileName)
        {
            // implement the open...
            try
            {
                // SECTION 1. Create a DOM Document and load the XML data into it.
                dom = new XmlDocument();
                dom.Load(fileName);
                Console.WriteLine("Loaded DOM from: " + fileName);

                //SECTION 1.5. Verify the version and set the title
                if (!"report".Equals(dom.DocumentElement.LocalName))
                {
                    MessageBox.Show(this, "The file '" + fileName + "' is not a SIW 'report' file !", "SIW Viewer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                System.Globalization.NumberFormatInfo provider = new System.Globalization.NumberFormatInfo();

                provider.NumberDecimalSeparator = ".";
                provider.NumberGroupSeparator = ",";
                provider.NumberGroupSizes = new int[] { 3 };

                float ver = System.Convert.ToSingle(getAttribute(dom.DocumentElement, "xml_version"), provider);
                if (ver < 1.2f)
                {
                    MessageBox.Show(this, "Only the versions >= 1.2 are supported, not this one: " + ver + " !", "SIW Viewer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                this.Text = "SIW Viewer [ " + fileName + "] - \\\\" + getAttribute(dom.DocumentElement, "computer_name");
                // SECTION 2. Initialize the TreeView control.
                treeView.Nodes.Clear();
                // add the root...
                String name = getAttribute(dom.DocumentElement, "Title");
                int type = getNodeType(dom.DocumentElement);
                treeView.Nodes.Add(new MyTreeNode(name, type));
                MyTreeNode tNode = (MyTreeNode)treeView.Nodes[0];
                tNode.Tag = getXPath(dom.DocumentElement);

                // SECTION 3. Populate the TreeView with the DOM nodes.
                addNextLevelNodes(tNode);
                try
                {
                    tNode.Nodes[2].ExpandAll();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                try
                {
                    tNode.Nodes[1].ExpandAll();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                try
                {
                    tNode.Nodes[0].ExpandAll();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                tNode.Expand();

                // SECTION 4: some initializations...
                initControls();
            }
            catch (XmlException xmlEx)
            {
                MessageBox.Show(xmlEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int getNodeType(XmlNode node)
        {
            String sType = getAttribute(node, "__type__");
            int type = sType == null ? 1 : System.Convert.ToInt32(sType);
            return type;
        }

        private void initControls()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView2.Columns.Clear();
            dataGridView2.Rows.Clear();
            splitContainer2.Panel2.Hide();
            splitContainer2.Panel2Collapsed = true;
            splitContainer2.Panel2.Enabled = false;
            splitContainer2.SplitterDistance = splitContainer2.Size.Height;
            treeViewDetails.Hide();
        }

        private XmlNode evalXPath(XmlNode parent, String xp)
        {
            XmlNode result = parent.SelectSingleNode(xp);
            Console.Out.WriteLine("Found node: " + result + ", xp: " + xp);
            return result;
        }

        private String getXPath(XmlNode xnode)
        {
            if (xnode == null)
            {
                return null;
            }
            if (xnode.NodeType == XmlNodeType.Document)
            {
                return "/";
            }
            String xp = xnode.LocalName;
            Boolean first = true;
            foreach (XmlAttribute a in xnode.Attributes)
            {
                if (first)
                {
                    xp += "[";
                    first = false;
                }
                else
                {
                    xp += " and ";
                }
                xp += "@" + a.Name + "=\"" + encodeXml(a.Value) + "\"";
            }
            if (!first)
            {
                xp += "]";
            }
            String x = getXPath(xnode.ParentNode);
            if (x != null)
            {
                xp = x + "/" + xp;
            }
            return xp;
        }

        private static String encodeXml(String str)
        {
            String encStr = "";
            char[] array = str.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                switch (array[i])
                {
                    case '\"': encStr += "&quot;"; break;
                    case '<': encStr += "&lt;"; break;
                    case '>': encStr += "&gt;"; break;
                    case '&': encStr += "&amp;"; break;
                    //case '\'': encStr += "&apos;"; break;
                    default: encStr += array[i]; break;
                }
            }
            return encStr;
        }

        private void addNextLevelNodes(MyTreeNode tParentNode)
        {
            String xp = tParentNode.Tag.ToString();
            if (xp == null)
            {
                return;
            }
            XmlNode xParentNode = evalXPath(dom, xp);
            // Add the nodes to the TreeView during the looping process.
            if (xParentNode != null && xParentNode.HasChildNodes)
            {
                if ("page".Equals(xParentNode.Name))
                {
                    return; // don't go deeper that the "page" element...
                }
                XmlNodeList nodeList = xParentNode.ChildNodes;
                int size = nodeList.Count - 1;
                for (int i = 0; i <= size; i++)
                {
                    XmlNode xNode = xParentNode.ChildNodes[i];
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        MyTreeNode tnode = new MyTreeNode(getAttribute(xNode, "Title"), getNodeType(xNode));
                        tnode.Tag = getXPath(xNode);
                        tParentNode.Nodes.Add(tnode);

                        MyTreeNode fakeNode = new MyTreeNode("loading...", 1);
                        tnode.Nodes.Add(fakeNode);
                    }
                }
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            MyTreeNode tNode = (MyTreeNode)e.Node;
            if (tNode.Nodes.Count == 1 && "loading...".Equals(tNode.Nodes[0].Text))
            {
                tNode.Nodes.Clear();
                addNextLevelNodes(tNode);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MyTreeNode tNode = (MyTreeNode)e.Node;
            String xp = tNode.Tag.ToString();
            XmlNode xmlNode = evalXPath(dom, xp);

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            if (xmlNode != null && "page".Equals(xmlNode.LocalName))
            {
                // hide the second grid, end expand the container
                if (splitContainer2.Panel2.Enabled)
                {
                    splitContainer2.Panel2Collapsed = true;
                    splitContainer2.Panel2.Hide();
                    splitContainer2.SplitterDistance = splitContainer2.Size.Height;
                    splitContainer2.Panel2.Enabled = false;
                }

                if (tNode.TypeNode == 2)
                {
                    dataGridView1.Hide();
                    treeViewDetails.Show();

                    populateDetailTree(xmlNode);
                }
                else
                {
                    treeViewDetails.Hide();
                    dataGridView1.Show();

                    populateDetailTables(xmlNode);
                }
            }
            else
            {
                initControls();
            }
            label1.Text = tNode.Text; // +" (type:" + tNode.TypeNode + ")";
            //Console.Out.WriteLine("selected xmlNode = " + (xmlNode != null ? xmlNode.Name : "null") + ", xp = " + xp);
        }

        private void populateDetailTree(XmlNode xmlNode)
        {
            treeViewDetails.Columns.Clear();
            treeViewDetails.NodeControls.Clear();

            treeViewDetails.Model = createTreeModel(xmlNode);
            treeViewDetails.ExpandAll();
        }

        private DetailsTreeModel createTreeModel(XmlNode pageElem)
        {
            // create the columns first
            int cols = 0;
            for (int i = 0; i < pageElem.Attributes.Count; i++)
            {
                XmlNode a = pageElem.Attributes.Item(i);
                if (a.Name.StartsWith("H"))
                {
                    Aga.Controls.Tree.TreeColumn tc = new Aga.Controls.Tree.TreeColumn();
                    tc.Header = a.Value;

                    // compute the width of the max value.
                    String maxValue = getMaxValue(pageElem, a.Value, "");
                    SizeF width = treeViewDetails.CreateGraphics().MeasureString(maxValue, treeViewDetails.Font);
                    tc.Width = 20 + width.ToSize().Width + (cols == 0 ? 50 : 0);

                    treeViewDetails.Columns.Add(tc);

                    NodeTextBox columnControl = new NodeTextBox();
                    if (cols > 0)
                    {
                        columnControl.Column = cols;
                    }
                    columnControl.DataPropertyName = a.Name;

                    treeViewDetails.NodeControls.Add(columnControl);

                    cols++;
                }
            }

            // return the model
            return new DetailsTreeModel(new DetailTreeNode(pageElem));
        }

        private String getMaxValue(XmlNode xNode, String attrNm, String maxValue)
        {
            if (xNode is XmlElement)
            {
                attrNm = attrNm.Replace(' ', '_');

                XmlNode attr = xNode.Attributes.GetNamedItem(attrNm);
                if (attr != null)
                {
                    String value = attr.Value.Replace('\n', ' ');
                    if (value.Length > maxValue.Length)
                    {
                        maxValue = value;
                    }
                }
                foreach (XmlNode kid in xNode.ChildNodes)
                {
                    String max = getMaxValue(kid, attrNm, maxValue);
                    if (max.Length > maxValue.Length)
                    {
                        maxValue = max;
                    }
                }
            }
            return maxValue;
        }

        private void populateDetailTables(XmlNode xmlNode)
        {
            List<XmlNode> kids = getChildElements(xmlNode);
            XmlNode itemEl = null;
            for (int i = kids.Count; --i >= 0; )
            {
                if (itemEl == null)
                {
                    itemEl = kids[i];
                }
                else if (itemEl.Attributes.Count < kids[i].Attributes.Count)
                {
                    itemEl = kids[i];
                }
            }

            // creates the columns
            if (itemEl != null)
            {
                for (int i = 0; i < itemEl.Attributes.Count; i++)
                {
                    String attr_nm = itemEl.Attributes.Item(i).Name;
                    dataGridView1.Columns.Add(attr_nm, getAttribute(xmlNode, "H" + (i + 1)));
                }
            }
            else
            {
                for (int i = 1; i < 50; i++)
                {
                    String colNm = getAttribute(xmlNode, "H" + i);
                    if (colNm == null)
                    {
                        break;
                    }
                    else
                    {
                        dataGridView1.Columns.Add("H" + i, colNm);
                    }
                }
            }
            // add the rows...
            if (xmlNode.HasChildNodes)
            {
                XmlNodeList nodeList = xmlNode.ChildNodes;
                int size = nodeList.Count - 1;
                for (int i = 0; i <= size; i++)
                {
                    XmlNode xNode = nodeList.Item(i);
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        object[] rowValues = new object[dataGridView1.Columns.Count];
                        for (int j = 0; j < xNode.Attributes.Count; j++)
                        {
                            rowValues[j] = xNode.Attributes.Item(j).Value;
                        }
                        dataGridView1.Rows.Add(rowValues);
                    }
                }
            }
        }

        private List<XmlNode> getChildElements(XmlNode xParentNode)
        {
            List<XmlNode> kids = new List<XmlNode>();
            if (xParentNode != null && xParentNode.HasChildNodes)
            {
                XmlNodeList nodeList = xParentNode.ChildNodes;
                for (int i = nodeList.Count; --i >= 0; )
                {
                    XmlNode xNode = nodeList.Item(i);
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        kids.Insert(0, xNode);
                    }
                }
            }
            return kids;
        }

        public static String getAttribute(XmlNode xmlNode, String attrName)
        {
            for (int i = xmlNode.Attributes.Count; --i >= 0; )
            {
                if (xmlNode.Attributes.Item(i).Name.Equals(attrName))
                {
                    return xmlNode.Attributes.Item(i).Value;
                }
            }
            return null;
        }

        public static String getFirstAttribute(XmlNode xmlNode)
        {
            if (xmlNode.Attributes.Count > 0)
            {
                return xmlNode.Attributes.Item(0).Value;
            }
            return null;
        }

        private void AddNode(XmlNode inXmlNode, MyTreeNode inTreeNode)
        {
            XmlNode xNode;
            MyTreeNode tNode;
            XmlNodeList nodeList;
            int i;
            Console.Out.WriteLine("Date " + System.DateTime.Now + " tree.node = " + inTreeNode);

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                int size = nodeList.Count - 1;
                for (i = 0; i <= size; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        String name = getAttribute(xNode, "Title");
                        inTreeNode.Nodes.Add(new MyTreeNode(name, getNodeType(xNode)));
                        tNode = (MyTreeNode)inTreeNode.Nodes[i];
                        AddNode(xNode, tNode);
                    }
                }
            }
            else
            {
                inTreeNode.Text = getAttribute(inXmlNode, "Title");
            }
        }

        private void SiwViewerMainWindow_MouseEnter(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = sender.ToString();
        }

        private void SiwViewerMainWindow_MouseLeave(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "";
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int selRow = -1;
            if (dataGridView1.SelectedRows.Count == 1)
            {
                selRow = dataGridView1.SelectedRows[0].Index;
            }
            if (selRow == -1)
            {
                return;
            }
            String attrs = "";
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (dataGridView1.Rows[selRow].Cells[i].Value != null)
                {
                    String val = dataGridView1.Rows[selRow].Cells[i].Value.ToString();
                    if (attrs.Length > 0)
                    {
                        attrs += " and ";
                    }
                    attrs += "@" + dataGridView1.Columns[i].Name + "=\"" + encodeXml(val) + "\"";
                }
            }

            String xp = "//item[" + attrs + "]";
            Console.Out.WriteLine("table selection row = " + selRow + " xp = " + xp);

            XmlNode itemXml = evalXPath(dom, xp);
            List<XmlNode> kids = getChildElements(itemXml);
            dataGridView2.Columns.Clear();
            dataGridView2.Rows.Clear();
            if (kids != null && kids.Count == 1 && "detail".Equals(kids[0].LocalName))
            {
                if (!splitContainer2.Panel2.Enabled)
                {
                    splitContainer2.Panel2Collapsed = false;
                    splitContainer2.Panel2.Enabled = true;
                    splitContainer2.SplitterDistance = 2 * splitContainer2.Size.Height / 3;
                    splitContainer2.Panel2.Show();
                }
                // here we have to create the second table ...
                XmlNode xmlNode = kids[0];
                kids = getChildElements(xmlNode);
                XmlNode itemEl = null;
                for (int i = kids.Count; --i >= 0; )
                {
                    if (itemEl == null)
                    {
                        itemEl = kids[i];
                    }
                    else if (itemEl.Attributes.Count < kids[i].Attributes.Count)
                    {
                        itemEl = kids[i];
                    }
                }

                // creates the columns
                if (itemEl != null)
                {
                    for (int i = 0; i < itemEl.Attributes.Count; i++)
                    {
                        String attr_nm = itemEl.Attributes.Item(i).Name;
                        dataGridView2.Columns.Add(attr_nm, getAttribute(xmlNode, "H" + (i + 1)));
                    }
                }
                else
                {
                    for (int i = 1; i < 50; i++)
                    {
                        String colNm = getAttribute(xmlNode, "H" + i);
                        if (colNm == null)
                        {
                            break;
                        }
                        else
                        {
                            dataGridView2.Columns.Add("H" + i, colNm);
                        }
                    }
                }
                // add the rows...
                if (xmlNode.HasChildNodes)
                {
                    XmlNodeList nodeList = xmlNode.ChildNodes;
                    int size = nodeList.Count - 1;
                    for (int i = 0; i <= size; i++)
                    {
                        XmlNode xNode = nodeList.Item(i);
                        if (xNode.NodeType == XmlNodeType.Element)
                        {
                            object[] rowValues = new object[dataGridView2.Columns.Count];
                            for (int j = 0; j < xNode.Attributes.Count; j++)
                            {
                                rowValues[j] = xNode.Attributes.Item(j).Value;
                            }
                            dataGridView2.Rows.Add(rowValues);
                        }
                    }
                }
            }
            else
            {
                splitContainer2.Panel2Collapsed = true;
                splitContainer2.Panel2.Hide();
                splitContainer2.SplitterDistance = splitContainer2.Size.Height;
                splitContainer2.Panel2.Enabled = false;
            }
        }

        private void onlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com");
        }

        private void buyNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com/siw_buy.html");
        }

        private void registerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com");
        }

        private void visitMyHomepageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com");
        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com/siw-download.html");
        }

    }

    class MyTreeNode : TreeNode
    {
        //
        // Summary:
        //     Gets or sets the type of the tree node.
        //
        // Returns:
        //     An integer that contains type of the tree node. The default is 1. 
        //     Another known value is 2.
        [DefaultValue(1)]
        public int TypeNode { get; set; }

        public MyTreeNode(String name, int type)
            : base(name)
        {
            this.TypeNode = type;
        }
    }

}