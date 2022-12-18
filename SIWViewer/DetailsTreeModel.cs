using Aga.Controls.Tree;
using System;
using System.Collections;

namespace SIWViewer
{
    public class DetailsTreeModel : ITreeModel
    {
        private readonly DetailTreeNode node_;

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
#pragma warning disable CS0067

        public event EventHandler<TreeModelEventArgs> NodesChanged;
        public event EventHandler<TreeModelEventArgs> NodesInserted;
        public event EventHandler<TreeModelEventArgs> NodesRemoved;
        public event EventHandler<TreePathEventArgs> StructureChanged;
    }
}