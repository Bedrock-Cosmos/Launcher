using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

// Built off of Crown Tree View from ReaLTaiizor to work with .NET 4.7.2
// https://github.com/Taiizor/ReaLTaiizor

namespace BedrockCosmos.App.UI
{
    public class TreeViewControl : ScrollableViewControl
    {
        public event EventHandler SelectedNodesChanged;
        public event EventHandler AfterNodeExpand;
        public event EventHandler AfterNodeCollapse;

        private bool _disposed;

        private readonly int _expandAreaSize = 16;
        private readonly int _iconSize = 16;
        private TreeViewNode _anchoredNodeStart;
        private TreeViewNode _anchoredNodeEnd;

        private TreeViewNode _provisionalNode;
        private TreeViewNode _dropNode;
        private bool _provisionalDragging;
        private List<TreeViewNode> _dragNodes;
        private Point _dragPos;

        private ObservableList<TreeViewNode> _nodes;
        private int _itemHeight = 20;
        private int _indent = 20;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableList<TreeViewNode> Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes != null)
                {
                    _nodes.ItemsAdded -= Nodes_ItemsAdded;
                    _nodes.ItemsRemoved -= Nodes_ItemsRemoved;

                    foreach (TreeViewNode node in _nodes)
                    {
                        UnhookNodeEvents(node);
                    }
                }

                _nodes = value;

                _nodes.ItemsAdded += Nodes_ItemsAdded;
                _nodes.ItemsRemoved += Nodes_ItemsRemoved;

                foreach (TreeViewNode node in _nodes)
                {
                    HookNodeEvents(node);
                }

                UpdateNodes();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<TreeViewNode> SelectedNodes { get; private set; }

        [Category("Appearance")]
        [Description("Determines the height of tree nodes.")]
        [DefaultValue(20)]
        public int ItemHeight
        {
            get { return _itemHeight; }
            set
            {
                _itemHeight = value;
                MaxDragChange = _itemHeight;
                UpdateNodes();
            }
        }

        [Category("Appearance")]
        [Description("Determines the amount of horizontal space given by parent node.")]
        [DefaultValue(20)]
        public int Indent
        {
            get { return _indent; }
            set
            {
                _indent = value;
                UpdateNodes();
            }
        }

        [Category("Behavior")]
        [Description("Determines whether multiple tree nodes can be selected at once.")]
        [DefaultValue(false)]
        public bool MultiSelect { get; set; }

        [Category("Behavior")]
        [Description("Determines whether nodes can be moved within this tree view.")]
        [DefaultValue(false)]
        public bool AllowMoveNodes { get; set; }

        [Category("Appearance")]
        [Description("Determines whether icons are rendered with the tree nodes.")]
        [DefaultValue(false)]
        public bool ShowIcons { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int VisibleNodeCount { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IComparer<TreeViewNode> TreeViewNodeSorter { get; set; }

        public TreeViewControl()
        {
            Nodes = new ObservableList<TreeViewNode>();

            SelectedNodes = new ObservableCollection<TreeViewNode>();
            SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

            MaxDragChange = ItemHeight;

            _dragTimer.Tick += DragTimer_Tick;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _dragTimer.Tick -= DragTimer_Tick;

                SelectedNodesChanged = null;
                AfterNodeExpand = null;
                AfterNodeCollapse = null;

                if (Nodes != null)
                {
                    Nodes.Dispose();
                }

                if (SelectedNodes != null)
                {
                    SelectedNodes.CollectionChanged -= SelectedNodes_CollectionChanged;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private void Nodes_ItemsAdded(object sender, ObservableListModified<TreeViewNode> e)
        {
            foreach (TreeViewNode node in e.Items)
            {
                node.ParentTree = this;
                node.IsRoot = true;

                HookNodeEvents(node);
            }

            if (TreeViewNodeSorter != null)
            {
                Nodes.Sort(TreeViewNodeSorter);
            }

            UpdateNodes();
        }

        private void Nodes_ItemsRemoved(object sender, ObservableListModified<TreeViewNode> e)
        {
            foreach (TreeViewNode node in e.Items)
            {
                node.ParentTree = this;
                node.IsRoot = true;

                HookNodeEvents(node);
            }

            UpdateNodes();
        }

        private void ChildNodes_ItemsAdded(object sender, ObservableListModified<TreeViewNode> e)
        {
            foreach (TreeViewNode node in e.Items)
            {
                HookNodeEvents(node);
            }

            UpdateNodes();
        }

        private void ChildNodes_ItemsRemoved(object sender, ObservableListModified<TreeViewNode> e)
        {
            foreach (TreeViewNode node in e.Items)
            {
                if (SelectedNodes.Contains(node))
                {
                    SelectedNodes.Remove(node);
                }

                UnhookNodeEvents(node);
            }

            UpdateNodes();
        }

        private void SelectedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedNodesChanged?.Invoke(this, null);
        }

        private void Nodes_TextChanged(object sender, EventArgs e)
        {
            UpdateNodes();
        }

        private void Nodes_NodeExpanded(object sender, EventArgs e)
        {
            UpdateNodes();

            AfterNodeExpand?.Invoke(this, null);
        }

        private void Nodes_NodeCollapsed(object sender, EventArgs e)
        {
            UpdateNodes();

            AfterNodeCollapse?.Invoke(this, null);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_provisionalDragging)
            {
                if (OffsetMousePosition != _dragPos)
                {
                    StartDrag();
                    HandleDrag();
                    return;
                }
            }

            if (IsDragging)
            {
                if (_dropNode != null)
                {
                    Rectangle rect = GetNodeFullRowArea(_dropNode);
                    if (!rect.Contains(OffsetMousePosition))
                    {
                        _dropNode = null;
                        Invalidate();
                    }
                }
            }

            CheckHover();

            if (IsDragging)
            {
                HandleDrag();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // base.OnMouseWheel actually scrolls (see ScrollableViewControl.HandleMouseWheel);
            // re-check hover afterward so it reflects the new scroll offset.
            base.OnMouseWheel(e);

            CheckHover();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                foreach (TreeViewNode node in Nodes)
                {
                    CheckNodeClick(node, OffsetMousePosition, e.Button);
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IsDragging)
            {
                HandleDrop();
            }

            if (_provisionalDragging)
            {
                if (_provisionalNode != null)
                {
                    Point pos = _dragPos;
                    if (OffsetMousePosition == pos)
                    {
                        SelectNode(_provisionalNode);
                    }
                }

                _provisionalDragging = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                foreach (TreeViewNode node in Nodes)
                {
                    CheckNodeDoubleClick(node, OffsetMousePosition);
                }
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            foreach (TreeViewNode node in Nodes)
            {
                NodeMouseLeave(node);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsDragging)
            {
                return;
            }

            if (Nodes.Count == 0)
            {
                return;
            }

            if (e.KeyCode != Keys.Down && e.KeyCode != Keys.Up && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
            {
                return;
            }

            if (_anchoredNodeEnd == null)
            {
                if (Nodes.Count > 0)
                {
                    SelectNode(Nodes[0]);
                }

                return;
            }

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                if (MultiSelect && ModifierKeys == Keys.Shift)
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        if (_anchoredNodeEnd.PrevVisibleNode != null)
                        {
                            SelectAnchoredRange(_anchoredNodeEnd.PrevVisibleNode);
                            EnsureVisible();
                        }
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        if (_anchoredNodeEnd.NextVisibleNode != null)
                        {
                            SelectAnchoredRange(_anchoredNodeEnd.NextVisibleNode);
                            EnsureVisible();
                        }
                    }
                }
                else
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        if (_anchoredNodeEnd.PrevVisibleNode != null)
                        {
                            SelectNode(_anchoredNodeEnd.PrevVisibleNode);
                            EnsureVisible();
                        }
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        if (_anchoredNodeEnd.NextVisibleNode != null)
                        {
                            SelectNode(_anchoredNodeEnd.NextVisibleNode);
                            EnsureVisible();
                        }
                    }
                }
            }

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                if (e.KeyCode == Keys.Left)
                {
                    if (_anchoredNodeEnd.Expanded && _anchoredNodeEnd.Nodes.Count > 0)
                    {
                        _anchoredNodeEnd.Expanded = false;
                    }
                    else
                    {
                        if (_anchoredNodeEnd.ParentNode != null)
                        {
                            SelectNode(_anchoredNodeEnd.ParentNode);
                            EnsureVisible();
                        }
                    }
                }
                else if (e.KeyCode == Keys.Right)
                {
                    if (!_anchoredNodeEnd.Expanded)
                    {
                        _anchoredNodeEnd.Expanded = true;
                    }
                    else
                    {
                        if (_anchoredNodeEnd.Nodes.Count > 0)
                        {
                            SelectNode(_anchoredNodeEnd.Nodes[0]);
                            EnsureVisible();
                        }
                    }
                }
            }
        }

        private void DragTimer_Tick(object sender, EventArgs e)
        {
            if (!IsDragging)
            {
                StopDrag();
                return;
            }

            if (MouseButtons != MouseButtons.Left)
            {
                StopDrag();
                return;
            }

            Point pos = PointToClient(MousePosition);

            if (_vScrollBar.Visible)
            {
                // Scroll up
                if (pos.Y < ClientRectangle.Top)
                {
                    int difference = (pos.Y - ClientRectangle.Top) * -1;

                    if (difference > ItemHeight)
                    {
                        difference = ItemHeight;
                    }

                    _vScrollBar.Value = Math.Max(_vScrollBar.Minimum, _vScrollBar.Value - difference);
                }

                // Scroll down
                if (pos.Y > ClientRectangle.Bottom)
                {
                    int difference = pos.Y - ClientRectangle.Bottom;

                    if (difference > ItemHeight)
                    {
                        difference = ItemHeight;
                    }

                    int maxValue = Math.Max(_vScrollBar.Minimum, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1);
                    _vScrollBar.Value = Math.Min(maxValue, _vScrollBar.Value + difference);
                }
            }

            if (_hScrollBar.Visible)
            {
                // Scroll left
                if (pos.X < ClientRectangle.Left)
                {
                    int difference = (pos.X - ClientRectangle.Left) * -1;

                    if (difference > ItemHeight)
                    {
                        difference = ItemHeight;
                    }

                    _hScrollBar.Value = Math.Max(_hScrollBar.Minimum, _hScrollBar.Value - difference);
                }

                // Scroll right
                if (pos.X > ClientRectangle.Right)
                {
                    int difference = pos.X - ClientRectangle.Right;

                    if (difference > ItemHeight)
                    {
                        difference = ItemHeight;
                    }

                    int maxValue = Math.Max(_hScrollBar.Minimum, _hScrollBar.Maximum - _hScrollBar.LargeChange + 1);
                    _hScrollBar.Value = Math.Min(maxValue, _hScrollBar.Value + difference);
                }
            }
        }

        private void HookNodeEvents(TreeViewNode node)
        {
            node.Nodes.ItemsAdded += ChildNodes_ItemsAdded;
            node.Nodes.ItemsRemoved += ChildNodes_ItemsRemoved;

            node.TextChanged += Nodes_TextChanged;
            node.NodeExpanded += Nodes_NodeExpanded;
            node.NodeCollapsed += Nodes_NodeCollapsed;

            foreach (TreeViewNode childNode in node.Nodes)
            {
                HookNodeEvents(childNode);
            }
        }

        private void UnhookNodeEvents(TreeViewNode node)
        {
            node.Nodes.ItemsAdded -= ChildNodes_ItemsAdded;
            node.Nodes.ItemsRemoved -= ChildNodes_ItemsRemoved;

            node.TextChanged -= Nodes_TextChanged;
            node.NodeExpanded -= Nodes_NodeExpanded;
            node.NodeCollapsed -= Nodes_NodeCollapsed;

            foreach (TreeViewNode childNode in node.Nodes)
            {
                UnhookNodeEvents(childNode);
            }
        }

        private void UpdateNodes()
        {
            if (IsDragging)
            {
                return;
            }

            ContentSize = new Size(0, 0);

            if (Nodes.Count == 0)
            {
                return;
            }

            int yOffset = 0;
            bool isOdd = false;
            int index = 0;
            TreeViewNode prevNode = null;

            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                TreeViewNode node = Nodes[i];
                UpdateNode(node, ref prevNode, 0, ref yOffset, ref isOdd, ref index);
            }

            ContentSize = new Size(ContentSize.Width, yOffset);

            VisibleNodeCount = index;

            Invalidate();
        }

        private void UpdateNode(TreeViewNode node, ref TreeViewNode prevNode, int indent, ref int yOffset,
                                ref bool isOdd, ref int index)
        {
            UpdateNodeBounds(node, yOffset, indent);

            yOffset += ItemHeight;

            node.Odd = isOdd;
            isOdd = !isOdd;

            node.VisibleIndex = index;
            index++;

            node.PrevVisibleNode = prevNode;

            if (prevNode != null)
            {
                prevNode.NextVisibleNode = node;
            }

            prevNode = node;

            if (node.Expanded)
            {
                foreach (TreeViewNode childNode in node.Nodes)
                {
                    UpdateNode(childNode, ref prevNode, indent + Indent, ref yOffset, ref isOdd, ref index);
                }
            }
        }

        private void UpdateNodeBounds(TreeViewNode node, int yOffset, int indent)
        {
            int expandTop = yOffset + (ItemHeight / 2) - (_expandAreaSize / 2);
            node.ExpandArea = new Rectangle(indent + 3, expandTop, _expandAreaSize, _expandAreaSize);

            int iconTop = yOffset + (ItemHeight / 2) - (_iconSize / 2);

            if (ShowIcons)
            {
                node.IconArea = new Rectangle(node.ExpandArea.Right + 2, iconTop, _iconSize, _iconSize);
            }
            else
            {
                node.IconArea = new Rectangle(node.ExpandArea.Right, iconTop, 0, 0);
            }

            using (Graphics g = CreateGraphics())
            {
                int textSize = (int)g.MeasureString(node.Text, Font).Width;
                node.TextArea = new Rectangle(node.IconArea.Right + 2, yOffset, textSize + 1, ItemHeight);
            }

            node.FullArea = new Rectangle(indent, yOffset, node.TextArea.Right - indent, ItemHeight);

            if (ContentSize.Width < node.TextArea.Right + 2)
            {
                ContentSize = new Size(node.TextArea.Right + 2, ContentSize.Height);
            }
        }

        private void CheckHover()
        {
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                if (IsDragging)
                {
                    if (_dropNode != null)
                    {
                        _dropNode = null;
                        Invalidate();
                    }
                }

                return;
            }

            foreach (TreeViewNode node in Nodes)
            {
                CheckNodeHover(node, OffsetMousePosition);
            }
        }

        private void NodeMouseLeave(TreeViewNode node)
        {
            node.ExpandAreaHot = false;

            foreach (TreeViewNode childNode in node.Nodes)
            {
                NodeMouseLeave(childNode);
            }

            Invalidate();
        }

        private void CheckNodeHover(TreeViewNode node, Point location)
        {
            if (IsDragging)
            {
                Rectangle rect = GetNodeFullRowArea(node);
                if (rect.Contains(OffsetMousePosition))
                {
                    TreeViewNode newDropNode = _dragNodes.Contains(node) ? null : node;

                    if (_dropNode != newDropNode)
                    {
                        _dropNode = newDropNode;
                        Invalidate();
                    }
                }
            }
            else
            {
                bool hot = node.ExpandArea.Contains(location);
                if (node.ExpandAreaHot != hot)
                {
                    node.ExpandAreaHot = hot;
                    Invalidate();
                }
            }

            foreach (TreeViewNode childNode in node.Nodes)
            {
                CheckNodeHover(childNode, location);
            }
        }

        private void CheckNodeClick(TreeViewNode node, Point location, MouseButtons button)
        {
            Rectangle rect = GetNodeFullRowArea(node);
            if (rect.Contains(location))
            {
                if (node.ExpandArea.Contains(location))
                {
                    if (button == MouseButtons.Left)
                    {
                        node.Expanded = !node.Expanded;
                    }
                }
                else
                {
                    if (button == MouseButtons.Left)
                    {
                        if (MultiSelect && ModifierKeys == Keys.Shift)
                        {
                            SelectAnchoredRange(node);
                        }
                        else if (MultiSelect && ModifierKeys == Keys.Control)
                        {
                            ToggleNode(node);
                        }
                        else
                        {
                            if (!SelectedNodes.Contains(node))
                            {
                                SelectNode(node);
                            }

                            _dragPos = OffsetMousePosition;
                            _provisionalDragging = true;
                            _provisionalNode = node;
                        }

                        return;
                    }
                    else if (button == MouseButtons.Right)
                    {
                        if (MultiSelect && ModifierKeys == Keys.Shift)
                        {
                            return;
                        }

                        if (MultiSelect && ModifierKeys == Keys.Control)
                        {
                            return;
                        }

                        if (!SelectedNodes.Contains(node))
                        {
                            SelectNode(node);
                        }

                        return;
                    }
                }
            }

            if (node.Expanded)
            {
                foreach (TreeViewNode childNode in node.Nodes)
                {
                    CheckNodeClick(childNode, location, button);
                }
            }
        }

        private void CheckNodeDoubleClick(TreeViewNode node, Point location)
        {
            Rectangle rect = GetNodeFullRowArea(node);
            if (rect.Contains(location))
            {
                if (!node.ExpandArea.Contains(location))
                {
                    node.Expanded = !node.Expanded;
                }

                return;
            }

            if (node.Expanded)
            {
                foreach (TreeViewNode childNode in node.Nodes)
                {
                    CheckNodeDoubleClick(childNode, location);
                }
            }
        }

        public void SelectNode(TreeViewNode node)
        {
            SelectedNodes.Clear();
            SelectedNodes.Add(node);

            _anchoredNodeStart = node;
            _anchoredNodeEnd = node;

            Invalidate();
        }

        public void SelectNodes(TreeViewNode startNode, TreeViewNode endNode)
        {
            List<TreeViewNode> nodes = new List<TreeViewNode>();

            if (startNode == endNode)
            {
                nodes.Add(startNode);
            }

            if (startNode.VisibleIndex < endNode.VisibleIndex)
            {
                TreeViewNode node = startNode;
                nodes.Add(node);
                while (node != endNode && node != null)
                {
                    node = node.NextVisibleNode;
                    nodes.Add(node);
                }
            }
            else if (startNode.VisibleIndex > endNode.VisibleIndex)
            {
                TreeViewNode node = startNode;
                nodes.Add(node);
                while (node != endNode && node != null)
                {
                    node = node.PrevVisibleNode;
                    nodes.Add(node);
                }
            }

            SelectNodes(nodes, false);
        }

        public void SelectNodes(List<TreeViewNode> nodes, bool updateAnchors = true)
        {
            SelectedNodes.Clear();

            foreach (TreeViewNode node in nodes)
            {
                SelectedNodes.Add(node);
            }

            if (updateAnchors && SelectedNodes.Count > 0)
            {
                _anchoredNodeStart = SelectedNodes[SelectedNodes.Count - 1];
                _anchoredNodeEnd = SelectedNodes[SelectedNodes.Count - 1];
            }

            Invalidate();
        }

        private void SelectAnchoredRange(TreeViewNode node)
        {
            _anchoredNodeEnd = node;
            SelectNodes(_anchoredNodeStart, _anchoredNodeEnd);
        }

        public void ToggleNode(TreeViewNode node)
        {
            if (SelectedNodes.Contains(node))
            {
                SelectedNodes.Remove(node);

                // If removing both the anchor start AND end then reset them
                if (_anchoredNodeStart == node && _anchoredNodeEnd == node)
                {
                    if (SelectedNodes.Count > 0)
                    {
                        _anchoredNodeStart = SelectedNodes[0];
                        _anchoredNodeEnd = SelectedNodes[0];
                    }
                    else
                    {
                        _anchoredNodeStart = null;
                        _anchoredNodeEnd = null;
                    }
                }

                // If removing the anchor start then update it accordingly
                if (_anchoredNodeStart == node)
                {
                    if (_anchoredNodeEnd.VisibleIndex < node.VisibleIndex)
                    {
                        _anchoredNodeStart = node.PrevVisibleNode;
                    }
                    else if (_anchoredNodeEnd.VisibleIndex > node.VisibleIndex)
                    {
                        _anchoredNodeStart = node.NextVisibleNode;
                    }
                    else
                    {
                        _anchoredNodeStart = _anchoredNodeEnd;
                    }
                }

                // If removing the anchor end then update it accordingly
                if (_anchoredNodeEnd == node)
                {
                    if (_anchoredNodeStart.VisibleIndex < node.VisibleIndex)
                    {
                        _anchoredNodeEnd = node.PrevVisibleNode;
                    }
                    else if (_anchoredNodeStart.VisibleIndex > node.VisibleIndex)
                    {
                        _anchoredNodeEnd = node.NextVisibleNode;
                    }
                    else
                    {
                        _anchoredNodeEnd = _anchoredNodeStart;
                    }
                }
            }
            else
            {
                SelectedNodes.Add(node);

                _anchoredNodeStart = node;
                _anchoredNodeEnd = node;
            }

            Invalidate();
        }

        // Moves a single node one position earlier among its siblings
        // (siblings = ParentNode.Nodes, or the tree's root Nodes if it's a
        // root node). Returns true if the node actually moved. Does nothing
        // if the node is already first among its siblings.
        public bool MoveNodeUp(TreeViewNode node)
        {
            return MoveNode(node, -1);
        }

        // Moves a single node one position later among its siblings. Returns
        // true if the node actually moved. Does nothing if the node is
        // already last among its siblings.
        public bool MoveNodeDown(TreeViewNode node)
        {
            return MoveNode(node, 1);
        }

        private bool MoveNode(TreeViewNode node, int direction)
        {
            if (node == null)
            {
                return false;
            }

            ObservableList<TreeViewNode> siblings = node.ParentNode != null ? node.ParentNode.Nodes : Nodes;

            int index = siblings.IndexOf(node);
            int newIndex = index + direction;

            if (index < 0 || newIndex < 0 || newIndex >= siblings.Count)
            {
                return false;
            }

            siblings.Move(index, newIndex);

            UpdateNodes();
            EnsureVisible();

            return true;
        }

        // Moves every currently selected node up by one position among its
        // siblings. Selected nodes are processed top-to-bottom so a
        // contiguous run of selected siblings shifts up together correctly.
        // Note: if TreeViewNodeSorter is set, a later add/rename that
        // triggers a re-sort will undo manual reordering like this.
        public void MoveSelectedNodesUp()
        {
            List<TreeViewNode> ordered = SelectedNodes.OrderBy(n => n.VisibleIndex).ToList();

            foreach (TreeViewNode node in ordered)
            {
                MoveNodeUp(node);
            }
        }

        // Moves every currently selected node down by one position among its
        // siblings. Selected nodes are processed bottom-to-top so a
        // contiguous run of selected siblings shifts down together correctly.
        public void MoveSelectedNodesDown()
        {
            List<TreeViewNode> ordered = SelectedNodes.OrderByDescending(n => n.VisibleIndex).ToList();

            foreach (TreeViewNode node in ordered)
            {
                MoveNodeDown(node);
            }
        }

        public Rectangle GetNodeFullRowArea(TreeViewNode node)
        {
            if (node.ParentNode != null && !node.ParentNode.Expanded)
            {
                return new Rectangle(-1, -1, -1, -1);
            }

            int width = Math.Max(ContentSize.Width, Viewport.Width);
            Rectangle rect = new Rectangle(0, node.FullArea.Top, width, ItemHeight);
            return rect;
        }

        public void EnsureVisible()
        {
            if (SelectedNodes.Count == 0)
            {
                return;
            }

            foreach (TreeViewNode node in SelectedNodes)
            {
                node.EnsureVisible();
            }

            int itemTop = -1;

            if (!MultiSelect)
            {
                itemTop = SelectedNodes[0].FullArea.Top;
            }
            else
            {
                itemTop = _anchoredNodeEnd.FullArea.Top;
            }

            int itemBottom = itemTop + ItemHeight;

            if (itemTop < Viewport.Top)
            {
                VScrollTo(itemTop);
            }

            if (itemBottom > Viewport.Bottom)
            {
                VScrollTo(itemBottom - Viewport.Height);
            }
        }

        public void Sort()
        {
            if (TreeViewNodeSorter == null)
            {
                return;
            }

            Nodes.Sort(TreeViewNodeSorter);

            foreach (TreeViewNode node in Nodes)
            {
                SortChildNodes(node);
            }
        }

        private void SortChildNodes(TreeViewNode node)
        {
            node.Nodes.Sort(TreeViewNodeSorter);

            foreach (TreeViewNode childNode in node.Nodes)
            {
                SortChildNodes(childNode);
            }
        }

        public TreeViewNode FindNode(string path)
        {
            foreach (TreeViewNode node in Nodes)
            {
                TreeViewNode compNode = FindNode(node, path);
                if (compNode != null)
                {
                    return compNode;
                }
            }

            return null;
        }

        private TreeViewNode FindNode(TreeViewNode parentNode, string path, bool recursive = true)
        {
            if (parentNode.FullPath == path)
            {
                return parentNode;
            }

            foreach (TreeViewNode node in parentNode.Nodes)
            {
                if (node.FullPath == path)
                {
                    return node;
                }

                if (recursive)
                {
                    TreeViewNode compNode = FindNode(node, path);
                    if (compNode != null)
                    {
                        return compNode;
                    }
                }
            }

            return null;
        }

        protected override void StartDrag()
        {
            if (!AllowMoveNodes)
            {
                _provisionalDragging = false;
                return;
            }

            // Create initial list of nodes to drag
            _dragNodes = new List<TreeViewNode>(SelectedNodes);

            // Clear out any nodes with a parent that is being dragged
            foreach (TreeViewNode node in _dragNodes.ToList())
            {
                if (node.ParentNode == null)
                {
                    continue;
                }

                if (_dragNodes.Contains(node.ParentNode))
                {
                    _dragNodes.Remove(node);
                }
            }

            _provisionalDragging = false;

            Cursor = Cursors.SizeAll;

            base.StartDrag();
        }

        private void HandleDrag()
        {
            if (!AllowMoveNodes)
            {
                return;
            }

            TreeViewNode dropNode = _dropNode;

            if (dropNode == null)
            {
                if (Cursor != Cursors.No)
                {
                    Cursor = Cursors.No;
                }

                return;
            }

            if (ForceDropToParent(dropNode))
            {
                dropNode = dropNode.ParentNode;
            }

            if (!CanMoveNodes(_dragNodes, dropNode))
            {
                if (Cursor != Cursors.No)
                {
                    Cursor = Cursors.No;
                }

                return;
            }

            if (Cursor != Cursors.SizeAll)
            {
                Cursor = Cursors.SizeAll;
            }
        }

        private void HandleDrop()
        {
            if (!AllowMoveNodes)
            {
                return;
            }

            TreeViewNode dropNode = _dropNode;

            if (dropNode == null)
            {
                StopDrag();
                return;
            }

            if (ForceDropToParent(dropNode))
            {
                dropNode = dropNode.ParentNode;
            }

            if (CanMoveNodes(_dragNodes, dropNode, true))
            {
                List<TreeViewNode> cachedSelectedNodes = SelectedNodes.ToList();

                MoveNodes(_dragNodes, dropNode);

                foreach (TreeViewNode node in _dragNodes)
                {
                    if (node.ParentNode == null)
                    {
                        Nodes.Remove(node);
                    }
                    else
                    {
                        node.ParentNode.Nodes.Remove(node);
                    }

                    dropNode.Nodes.Add(node);
                }

                if (TreeViewNodeSorter != null)
                {
                    dropNode.Nodes.Sort(TreeViewNodeSorter);
                }

                dropNode.Expanded = true;

                NodesMoved(_dragNodes);

                foreach (TreeViewNode node in cachedSelectedNodes)
                {
                    SelectedNodes.Add(node);
                }
            }

            StopDrag();
            UpdateNodes();
        }

        protected override void StopDrag()
        {
            _dragNodes = null;
            _dropNode = null;

            Cursor = Cursors.Default;

            Invalidate();

            base.StopDrag();
        }

        protected virtual bool ForceDropToParent(TreeViewNode node)
        {
            return false;
        }

        protected virtual bool CanMoveNodes(List<TreeViewNode> dragNodes, TreeViewNode dropNode, bool isMoving = false)
        {
            if (dropNode == null)
            {
                return false;
            }

            foreach (TreeViewNode node in dragNodes)
            {
                if (node == dropNode)
                {
                    if (isMoving)
                    {
                        MessageBox.Show(
                            string.Format("Cannot move {0}. The destination folder is the same as the source folder.", node.Text),
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }

                    return false;
                }

                if (node.ParentNode != null && node.ParentNode == dropNode)
                {
                    if (isMoving)
                    {
                        MessageBox.Show(
                            string.Format("Cannot move {0}. The destination folder is the same as the source folder.", node.Text),
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }

                    return false;
                }

                TreeViewNode parentNode = dropNode.ParentNode;
                while (parentNode != null)
                {
                    if (node == parentNode)
                    {
                        if (isMoving)
                        {
                            MessageBox.Show(
                                string.Format("Cannot move {0}. The destination folder is a subfolder of the source folder.", node.Text),
                                Application.ProductName,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }

                        return false;
                    }

                    parentNode = parentNode.ParentNode;
                }
            }

            return true;
        }

        protected virtual void MoveNodes(List<TreeViewNode> dragNodes, TreeViewNode dropNode)
        {
        }

        protected virtual void NodesMoved(List<TreeViewNode> nodesMoved)
        {
        }

        protected override void PaintContent(Graphics g)
        {
            // Fill body (fill the currently visible viewport, in content coordinates)
            using (SolidBrush b = new SolidBrush(TreeViewColors.GreyBackground))
            {
                g.FillRectangle(b, Viewport);
            }

            foreach (TreeViewNode node in Nodes)
            {
                DrawNode(node, g);
            }
        }

        private void DrawNode(TreeViewNode node, Graphics g)
        {
            Rectangle rect = GetNodeFullRowArea(node);

            // 1. Draw background
            Color bgColor = node.Odd ? TreeViewColors.HeaderBackground : TreeViewColors.GreyBackground;

            if (SelectedNodes.Count > 0 && SelectedNodes.Contains(node))
            {
                bgColor = Focused ? TreeViewColors.BlueSelection : TreeViewColors.GreySelection;
            }

            if (IsDragging && _dropNode == node)
            {
                bgColor = Focused ? TreeViewColors.BlueSelection : TreeViewColors.GreySelection;
            }

            using (SolidBrush b = new SolidBrush(bgColor))
            {
                g.FillRectangle(b, rect);
            }

            // 2. Draw expand/collapse glyph (vector-drawn instead of an embedded bitmap)
            if (node.Nodes.Count > 0)
            {
                Color glyphColor = TreeViewColors.LightText;

                if (node.ExpandAreaHot)
                {
                    glyphColor = TreeViewColors.BlueHighlight;
                }

                DrawExpandGlyph(g, node.ExpandArea, node.Expanded, glyphColor);
            }

            // 3. Draw icon
            if (ShowIcons && node.Icon != null)
            {
                if (node.Expanded && node.ExpandedIcon != null)
                {
                    g.DrawImageUnscaled(node.ExpandedIcon, node.IconArea.Location);
                }
                else
                {
                    g.DrawImageUnscaled(node.Icon, node.IconArea.Location);
                }
            }

            // 4. Draw text
            Color textColor = node.ForeColor ?? TreeViewColors.LightText;

            using (SolidBrush b = new SolidBrush(textColor))
            {
                StringFormat stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(node.Text, Font, b, node.TextArea, stringFormat);
            }

            // 5. Draw child nodes
            if (node.Expanded)
            {
                foreach (TreeViewNode childNode in node.Nodes)
                {
                    DrawNode(childNode, g);
                }
            }
        }

        // Draws triangle icon
        private static void DrawExpandGlyph(Graphics g, Rectangle area, bool expanded, Color color)
        {
            int cx = area.Left + (area.Width / 2);
            int cy = area.Top + (area.Height / 2);
            int size = Math.Max(2, Math.Min(area.Width, area.Height) / 3);

            Point[] triangle;

            if (expanded)
            {
                triangle = new Point[]
                {
                    new Point(cx - size, cy - (size / 2)),
                    new Point(cx + size, cy - (size / 2)),
                    new Point(cx, cy + size)
                };
            }
            else
            {
                triangle = new Point[]
                {
                    new Point(cx - (size / 2), cy - size),
                    new Point(cx - (size / 2), cy + size),
                    new Point(cx + size, cy)
                };
            }

            SmoothingMode oldMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, triangle);
            }

            g.SmoothingMode = oldMode;
        }
    }

    public static class TreeViewColors
    {
        public static Color LightText = Color.FromArgb(220, 220, 220);
        public static Color BlueHighlight = Color.FromArgb(0, 122, 204);
        public static Color GreyBackground = Color.FromArgb(35, 35, 35);
        public static Color HeaderBackground = Color.FromArgb(42, 42, 42);
        public static Color BlueSelection = Color.FromArgb(0, 96, 160);
        public static Color GreySelection = Color.FromArgb(70, 70, 70);
    }
}