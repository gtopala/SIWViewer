using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Aga.Controls.Tree
{
	internal class NormalInputState : InputState
	{
		private bool _mouseDownFlag = false;

		public NormalInputState(TreeViewAdv tree) : base(tree)
		{
		}

		public override void KeyDown(KeyEventArgs args)
		{
			if (Tree.CurrentNode == null && Tree.Root.Nodes.Count > 0)
				Tree.CurrentNode = Tree.Root.Nodes[0];

			if (Tree.CurrentNode != null)
			{
				switch (args.KeyCode)
				{
					case Keys.Right:
						if (!Tree.CurrentNode.IsExpanded)
							Tree.CurrentNode.IsExpanded = true;
						else if (Tree.CurrentNode.Nodes.Count > 0)
							Tree.SelectedNode = Tree.CurrentNode.Nodes[0];
						args.Handled = true;
						break;
					case Keys.Left:
						if (Tree.CurrentNode.IsExpanded)
							Tree.CurrentNode.IsExpanded = false;
						else if (Tree.CurrentNode.Parent != Tree.Root)
							Tree.SelectedNode = Tree.CurrentNode.Parent;
						args.Handled = true;
						break;
					
					case Keys.Down:
						NavigateForward(1);
						args.Handled = true;
						break;
					case Keys.Up:
						NavigateBackward(1);
						args.Handled = true;
						break;
					case Keys.PageDown:
						NavigateForward(Tree.PageRowCount);
						args.Handled = true;
						break;
					case Keys.PageUp:
						NavigateBackward(Tree.PageRowCount);
						args.Handled = true;
						break;

					case Keys.Home:
						if (Tree.RowMap.Count > 0)
							FocusRow(Tree.RowMap[0]);
						args.Handled = true;
						break;
					case Keys.End:
						if (Tree.RowMap.Count > 0)
							FocusRow(Tree.RowMap[Tree.RowMap.Count-1]);
						args.Handled = true;
						break;
				}
			}
		}

		public override void MouseDown(TreeNodeAdvMouseEventArgs args)
		{
			if (args.Node != null)
			{
				Tree.ItemDragMode = true;
				Tree.ItemDragStart = args.ViewLocation;

				if (args.Button == MouseButtons.Left || args.Button == MouseButtons.Right)
				{
					Tree.BeginUpdate();
					try
					{
						Tree.CurrentNode = args.Node;
						if (args.Node.IsSelected)
							_mouseDownFlag = true;
						else
						{
							_mouseDownFlag = false;
							DoMouseOperation(args);
						}
					}
					finally
					{
						Tree.EndUpdate();
					}
				}

			}
			else
			{
				Tree.ItemDragMode = false;
				MouseDownAtEmptySpace(args);
			}
		}

		public override void MouseUp(TreeNodeAdvMouseEventArgs args)
		{
			Tree.ItemDragMode = false;
			if (_mouseDownFlag)
			{
				if (args.Button == MouseButtons.Left)
					DoMouseOperation(args);
				else if (args.Button == MouseButtons.Right)
					Tree.CurrentNode = args.Node;
			}
			_mouseDownFlag = false;
		}


		private void NavigateBackward(int n)
		{
			int row = Math.Max(Tree.CurrentNode.Row - n, 0);
			if (row != Tree.CurrentNode.Row)
				FocusRow(Tree.RowMap[row]);
		}

		private void NavigateForward(int n)
		{
			int row = Math.Min(Tree.CurrentNode.Row + n, Tree.RowCount - 1);
			if (row != Tree.CurrentNode.Row)
				FocusRow(Tree.RowMap[row]);
		}

		protected virtual void MouseDownAtEmptySpace(TreeNodeAdvMouseEventArgs args)
		{
			Tree.ClearSelection();
		}

		protected virtual void FocusRow(TreeNodeAdv node)
		{
			Tree.SuspendSelectionEvent = true;
			try
			{
				Tree.ClearSelection();
				Tree.CurrentNode = node;
				Tree.SelectionStart = node;
				node.IsSelected = true;
				Tree.ScrollTo(node);
			}
			finally
			{
				Tree.SuspendSelectionEvent = false;
			}
		}

		protected bool CanSelect(TreeNodeAdv node)
		{
			if (Tree.SelectionMode == TreeSelectionMode.MultiSameParent)
			{
				return (Tree.SelectionStart == null || node.Parent == Tree.SelectionStart.Parent);
			}
			else
				return true;
		}

		protected virtual void DoMouseOperation(TreeNodeAdvMouseEventArgs args)
		{
			Tree.SuspendSelectionEvent = true;
			try
			{
				Tree.ClearSelection();
				if (args.Node != null)
					args.Node.IsSelected = true;
				Tree.SelectionStart = args.Node;
			}
			finally
			{
				Tree.SuspendSelectionEvent = false;
			}
		}
	}
}
