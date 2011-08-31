using System;
using System.Collections.Generic;
using System.Text;

using Aga.Controls.Tree;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Xml;

namespace SIWViewer
{
    public class DetailsTreeModel : ITreeModel
    {
        DetailTreeNode node_;

        public DetailsTreeModel(DetailTreeNode node)
        {
            node_ = node;
        }

        public IEnumerable GetChildren(TreePath treePath)
        {
            DetailTreeNode parent = treePath.LastNode as DetailTreeNode;
            if (parent == null)
            {
                return node_.ChildNodes();
            }
            return parent.ChildNodes();
        }

        public bool IsLeaf(TreePath treePath)
        {
            DetailTreeNode node = treePath.LastNode as DetailTreeNode;
            return node.isLeaf();
        }

        public event EventHandler<TreeModelEventArgs> NodesChanged;
        public event EventHandler<TreeModelEventArgs> NodesInserted;
        public event EventHandler<TreeModelEventArgs> NodesRemoved;
        public event EventHandler<TreePathEventArgs> StructureChanged;
    }
}