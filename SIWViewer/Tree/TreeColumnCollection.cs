using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Aga.Controls.Tree
{
	internal class TreeColumnCollection : Collection<TreeColumn>
	{
		private TreeViewAdv _treeView;

		public TreeColumnCollection(TreeViewAdv treeView)
		{
			_treeView = treeView;
		}

		protected override void InsertItem(int index, TreeColumn item)
		{
			base.InsertItem(index, item);
			item.Index = index;
			item.TreeView = _treeView;
			_treeView.UpdateColumns();
		}

		protected override void RemoveItem(int index)
		{
			this[index].TreeView = null;
			base.RemoveItem(index);
			_treeView.UpdateColumns();
		}

		protected override void SetItem(int index, TreeColumn item)
		{
			this[index].TreeView = null;
			base.SetItem(index, item);
			item.Index = index;
			this[index].TreeView = _treeView;
			_treeView.UpdateColumns();
		}

		protected override void ClearItems()
		{
			foreach (TreeColumn c in Items)
				c.TreeView = null;
			Items.Clear();
			_treeView.UpdateColumns();
		}
	}
}
