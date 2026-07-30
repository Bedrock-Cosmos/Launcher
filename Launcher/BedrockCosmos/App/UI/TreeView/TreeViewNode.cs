using System;
using System.Drawing;

// Built off of Crown Tree Node from ReaLTaiizor to work with .NET 4.7.2
// https://github.com/Taiizor/ReaLTaiizor

namespace BedrockCosmos.App.UI
{
    public class TreeViewNode
    {
        public event EventHandler<ObservableListModified<TreeViewNode>> ItemsAdded;
        public event EventHandler<ObservableListModified<TreeViewNode>> ItemsRemoved;

        public event EventHandler TextChanged;
        public event EventHandler NodeExpanded;
        public event EventHandler NodeCollapsed;

        private string _text;
        private bool _expanded;
        private ObservableList<TreeViewNode> _nodes;
        private TreeViewControl _parentTree;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                {
                    return;
                }

                _text = value;

                OnTextChanged();
            }
        }

        public Rectangle ExpandArea { get; set; }

        public Rectangle IconArea { get; set; }

        public Rectangle TextArea { get; set; }

        public Rectangle FullArea { get; set; }

        public bool ExpandAreaHot { get; set; }

        // Optional per-node text color. Leave null to use the tree's default
        // (TreeViewColors.LightText). Set back to null to clear the override.
        public Color? ForeColor { get; set; }

        public Bitmap Icon { get; set; }

        public Bitmap ExpandedIcon { get; set; }

        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded == value)
                {
                    return;
                }

                if (value == true && Nodes.Count == 0)
                {
                    return;
                }

                _expanded = value;

                if (_expanded)
                {
                    NodeExpanded?.Invoke(this, null);
                }
                else
                {
                    NodeCollapsed?.Invoke(this, null);
                }
            }
        }

        public ObservableList<TreeViewNode> Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes != null)
                {
                    _nodes.ItemsAdded -= Nodes_ItemsAdded;
                    _nodes.ItemsRemoved -= Nodes_ItemsRemoved;
                }

                _nodes = value;

                _nodes.ItemsAdded += Nodes_ItemsAdded;
                _nodes.ItemsRemoved += Nodes_ItemsRemoved;
            }
        }

        public bool IsRoot { get; set; }

        public TreeViewControl ParentTree
        {
            get { return _parentTree; }
            set
            {
                if (_parentTree == value)
                {
                    return;
                }

                _parentTree = value;

                foreach (TreeViewNode node in Nodes)
                {
                    node.ParentTree = _parentTree;
                }
            }
        }

        public TreeViewNode ParentNode { get; set; }

        public bool Odd { get; set; }

        public object NodeType { get; set; }

        public object Tag { get; set; }

        public string FullPath
        {
            get
            {
                TreeViewNode parent = ParentNode;
                string path = Text;

                while (parent != null)
                {
                    path = string.Format("{0}{1}{2}", parent.Text, "\\", path);
                    parent = parent.ParentNode;
                }

                return path;
            }
        }

        public TreeViewNode PrevVisibleNode { get; set; }

        public TreeViewNode NextVisibleNode { get; set; }

        public int VisibleIndex { get; set; }

        public bool IsNodeAncestor(TreeViewNode node)
        {
            TreeViewNode parent = ParentNode;
            while (parent != null)
            {
                if (parent == node)
                {
                    return true;
                }

                parent = parent.ParentNode;
            }

            return false;
        }

        public TreeViewNode()
        {
            Nodes = new ObservableList<TreeViewNode>();
        }

        public TreeViewNode(string text)
            : this()
        {
            Text = text;
        }

        public void Remove()
        {
            if (ParentNode != null)
            {
                ParentNode.Nodes.Remove(this);
            }
            else
            {
                ParentTree.Nodes.Remove(this);
            }
        }

        public void EnsureVisible()
        {
            TreeViewNode parent = ParentNode;

            while (parent != null)
            {
                parent.Expanded = true;
                parent = parent.ParentNode;
            }
        }

        // Sets ForeColor on this node and every node beneath it (pass null to
        // clear the override back to the tree's default color). Repaints the
        // tree once afterward rather than per-node.
        public void SetForeColorRecursive(Color? color)
        {
            ApplyForeColorRecursive(color);

            if (ParentTree != null)
            {
                ParentTree.Invalidate();
            }
        }

        private void ApplyForeColorRecursive(Color? color)
        {
            ForeColor = color;

            foreach (TreeViewNode child in Nodes)
            {
                child.ApplyForeColorRecursive(color);
            }
        }

        // Call this after changing THIS node's ForeColor (setting it,
        // clearing it back to null, or changing it to a different color).
        public void UpdateAncestorForeColors()
        {
            TreeViewNode current = ParentNode;

            while (current != null)
            {
                Color? uniform = GetUniformChildForeColor(current);

                if (current.ForeColor == uniform)
                {
                    break;
                }

                current.ForeColor = uniform;
                current = current.ParentNode;
            }

            if (ParentTree != null)
            {
                ParentTree.Invalidate();
            }
        }

        // Returns the single ForeColor shared by every child of parentNode,
        // or null if parentNode has no children, or its children don't all
        // share the same non-null color.
        private static Color? GetUniformChildForeColor(TreeViewNode parentNode)
        {
            if (parentNode.Nodes.Count == 0)
            {
                return null;
            }

            Color? candidate = parentNode.Nodes[0].ForeColor;

            if (candidate == null)
            {
                return null;
            }

            for (int i = 1; i < parentNode.Nodes.Count; i++)
            {
                if (parentNode.Nodes[i].ForeColor != candidate)
                {
                    return null;
                }
            }

            return candidate;
        }

        private void OnTextChanged()
        {
            if (ParentTree != null && ParentTree.TreeViewNodeSorter != null)
            {
                if (ParentNode != null)
                {
                    ParentNode.Nodes.Sort(ParentTree.TreeViewNodeSorter);
                }
                else
                {
                    ParentTree.Nodes.Sort(ParentTree.TreeViewNodeSorter);
                }
            }

            TextChanged?.Invoke(this, null);
        }

        private void Nodes_ItemsAdded(object sender, ObservableListModified<TreeViewNode> e)
        {
            foreach (TreeViewNode node in e.Items)
            {
                node.ParentNode = this;
                node.ParentTree = ParentTree;
            }

            if (ParentTree != null && ParentTree.TreeViewNodeSorter != null)
            {
                Nodes.Sort(ParentTree.TreeViewNodeSorter);
            }

            ItemsAdded?.Invoke(this, e);
        }

        private void Nodes_ItemsRemoved(object sender, ObservableListModified<TreeViewNode> e)
        {
            if (Nodes.Count == 0)
            {
                Expanded = false;
            }

            ItemsRemoved?.Invoke(this, e);
        }
    }
}