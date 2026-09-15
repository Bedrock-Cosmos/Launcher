using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.App.UI
{
    public class ImageTreeView : Control
    {
        #region Events

        public event EventHandler SelectionChanged; // Ran when selection changes.

        public event EventHandler DataChanged; // Ran if an operation takes place (add, remove, move, copy, expand/collapse).

        #endregion

        #region Appearance

        public Color IdleBackColor { get; set; } = Color.FromArgb(20, 20, 20);
        public Color SelectedBackColor { get; set; } = Color.FromArgb(153, 153, 153);
        public Color EnabledHighlightColor { get; set; } = Color.FromArgb(0, 188, 71);
        public Color HeaderTextColor { get; set; } = Color.FromArgb(153, 153, 153);
        public Color SelectedHeaderTextColor { get; set; } = Color.FromArgb(20, 20, 20);
        public Color ItemTitleColor { get; set; } = Color.FromArgb(153, 153, 153);
        public Color IdleBorderColor { get; set; } = Color.FromArgb(60, 60, 60);
        public Color TrackColor { get; set; } = Color.FromArgb(30, 30, 30);
        public Color ThumbColor { get; set; } = Color.FromArgb(75, 75, 75);
        public Color ThumbHoverColor { get; set; } = Color.FromArgb(110, 110, 110);
        public Color DropIndicatorColor { get; set; } = Color.FromArgb(0, 188, 71);

        public int ItemSize { get; set; } = 64;
        public int ItemPadding { get; set; } = 10;
        public int HeaderHeight { get; set; } = 30;
        public int ItemLabelHeight { get; set; } = 16;
        public int CornerRadius { get; set; } = 8;
        public int DragThreshold { get; set; } = 4;

        private const int ScrollBarWidth = 12;
        private const int MinThumbHeight = 24;

        #endregion

        #region Data

        private List<ImageCategory> _categories = new List<ImageCategory>();
        private readonly Dictionary<ImageItem, ImageCategory> _parentMap = new Dictionary<ImageItem, ImageCategory>();

        public IReadOnlyList<ImageCategory> Categories => _categories;

        #endregion

        #region Layout (virtualized row list)

        private abstract class VisualRow
        {
            public int Top;
            public int Height;
        }

        private sealed class HeaderVisualRow : VisualRow
        {
            public ImageCategory Category;
        }

        private sealed class ItemsVisualRow : VisualRow
        {
            public ImageCategory Category;
            public List<ImageItem> Items;
            public int StartIndex; // Index in Category.Items of the row's first item.
        }

        private readonly List<VisualRow> _rows = new List<VisualRow>();
        private int _contentHeight;
        private int _itemsPerRow = 6;

        #endregion

        #region Selection

        private readonly List<object> _selectionOrder = new List<object>();
        private readonly HashSet<object> _selectionSet = new HashSet<object>();
        private object _shiftAnchor;

        public IReadOnlyList<object> SelectedNodes => _selectionOrder; // Selected items in the order they were selected.

        public IEnumerable<ImageItem> SelectedItems => _selectionOrder.OfType<ImageItem>();

        public IEnumerable<ImageCategory> SelectedCategories => _selectionOrder.OfType<ImageCategory>();

        public int GetSelectedNodeCount()
        {
            var covered = new HashSet<ImageItem>();

            foreach (var category in SelectedCategories)
                foreach (var item in category.Items)
                    covered.Add(item);

            foreach (var item in SelectedItems)
                covered.Add(item);

            return covered.Count;
        }

        public bool GenerateNewIdOnCopy { get; set; } = true;

        // When true, adding, pasting, or dropping a node into a collapsed category automatically expands it.
        public bool AutoExpandCategoryOnInsert { get; set; } = false;

        private bool _showItemCountInHeader = true;

        public bool ShowItemCountInHeader
        {
            get => _showItemCountInHeader;
            set
            {
                if (_showItemCountInHeader == value)
                    return;

                _showItemCountInHeader = value;
                Invalidate();
            }
        }

        #endregion

        #region Clipboard

        // References to the actual nodes (never copies) - resolved into new
        // instances (Copy) or moved in place (Cut) only once Paste runs.
        private readonly List<object> _clipboard = new List<object>();
        private bool _clipboardIsCut;

        public bool CanCopy => _selectionOrder.Count > 0;
        public bool CanCut => _selectionOrder.Count > 0;
        public bool CanPaste => _clipboard.Count > 0;

        // Where pasted nodes land relative to the currently selected node(s).
        public enum PastePosition
        {
            Right, // Insert pasted nodes after the current selection (default, existing behavior).
            Left   // Insert pasted nodes before the current selection.
        }

        public PastePosition PastePlacement { get; set; } = PastePosition.Right;

        #endregion

        #region Scrolling

        private int _scrollOffset;
        private bool _isDraggingThumb;
        private int _dragStartMouseY;
        private int _dragStartScrollOffset;
        private bool _thumbHot;

        #endregion

        #region Click vs. drag resolution

        private bool _pendingSelectClick;
        private object _dragCandidateNode;
        private Point _dragCandidateLocation;

        private bool _isDragging;
        private List<ImageItem> _draggedItems;
        private ImageCategory _dropTargetCategory;
        private ImageItem _dropTargetBeforeItem;

        private int _dropTargetGlobalIndex = -1; // Insertion index used by moving/copying. -1 = no target.

        private ItemsVisualRow _dropTargetVisualRow; // Caret rendered with this + dropTargetLocalSlot.

        private int _dropTargetLocalSlot; // Slot where caret renders.

        private List<ImageCategory> _draggedCategories; // Non-null while dragging one or more categories.

        private bool _categoryDropHasTarget; // True once a valid category reorder target has been resolved.

        private ImageCategory _categoryDropBeforeCategory; // The category the dragged group should land before. null appends at the end of top-level list.

        #endregion

        #region Auto-scroll while dragging

        private const int AutoScrollEdgeSize = 28;
        private const int AutoScrollMinSpeed = 4;
        private const int AutoScrollMaxSpeed = 24;

        private readonly Timer _autoScrollTimer;
        private int _autoScrollDirection;
        private Point _lastDragMouseLocation;

        #endregion

        #region Construction

        private readonly bool _isDesignMode; // Used to skip network/background-thread activity in VS designer.

        public ImageTreeView()
        {
            _isDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);

            BackColor = IdleBackColor;
            TabStop = true;

            if (!_isDesignMode)
                ImageCache.ImageLoaded += OnImageLoaded;

            _autoScrollTimer = new Timer { Interval = 40 };
            _autoScrollTimer.Tick += OnAutoScrollTick;

            BuildContextMenu();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ImageCache.ImageLoaded -= OnImageLoaded; // Unsub from the static/shared cache event.

                _autoScrollTimer.Stop();
                _autoScrollTimer.Dispose();

                _contextMenu?.Dispose();
                _renameTextBox?.Dispose();
                _renameFont?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnImageLoaded(string id)
        {
            // Can run on a background thread at any time (after control has been disposed, form closing, etc.).
            try
            {
                if (IsDisposed || Disposing)
                    return;

                if (InvokeRequired)
                {
                    if (!IsHandleCreated)
                        return;

                    BeginInvoke((Action)(() =>
                    {
                        if (!IsDisposed)
                            Invalidate();
                    }));
                }
                else
                {
                    Invalidate();
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        #endregion

        #region Loading data

        public void LoadData(IEnumerable<ImageCategory> categories)
        {
            // Replaces all data, clears selection, and rebuilds layout.
            _categories = categories as List<ImageCategory> ?? categories?.ToList() ?? new List<ImageCategory>();
            RebuildParentMap();
            ClearSelectionInternal();
            _scrollOffset = 0;
            RebuildLayout();
            Invalidate();
        }

        private void RebuildParentMap()
        {
            _parentMap.Clear();
            foreach (var category in _categories)
                foreach (var item in category.Items)
                    _parentMap[item] = category;
        }

        #endregion

        #region Layout building

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RebuildLayout();
            RepositionRenameBoxIfActive();
            Invalidate();
        }

        private void RebuildLayout()
        {
            _rows.Clear();

            int usableWidth = Math.Max(ClientSize.Width - ScrollBarWidth, ItemSize + ItemPadding * 2);
            int cellStride = ItemSize + ItemPadding;
            _itemsPerRow = Math.Max(1, usableWidth / cellStride);

            int y = 0;

            foreach (var category in _categories)
            {
                _rows.Add(new HeaderVisualRow { Category = category, Top = y, Height = HeaderHeight });
                y += HeaderHeight;

                if (category.IsExpanded)
                {
                    var items = category.Items;
                    for (int i = 0; i < items.Count; i += _itemsPerRow)
                    {
                        int count = Math.Min(_itemsPerRow, items.Count - i);
                        int rowHeight = ItemSize + ItemLabelHeight + ItemPadding;

                        _rows.Add(new ItemsVisualRow
                        {
                            Category = category,
                            Items = items.GetRange(i, count),
                            StartIndex = i,
                            Top = y,
                            Height = rowHeight
                        });

                        y += rowHeight;
                    }
                }
            }

            _contentHeight = y;
            ClampScrollOffset();
        }

        private void ClampScrollOffset()
        {
            int maxScroll = Math.Max(0, _contentHeight - ClientSize.Height);
            if (_scrollOffset > maxScroll) _scrollOffset = maxScroll;
            if (_scrollOffset < 0) _scrollOffset = 0;
        }

        // Scroll into view when a node operation takes place.
        private void ScrollNodeIntoView(object node)
        {
            if (TryGetNodeVerticalBounds(node, out int top, out int bottom))
                ScrollRangeIntoView(top, bottom);
        }

        // Same as above but for multiple nodes.
        private void ScrollNodesIntoView(IEnumerable<object> nodes)
        {
            int? minTop = null;
            int? maxBottom = null;

            foreach (var node in nodes)
            {
                if (!TryGetNodeVerticalBounds(node, out int top, out int bottom))
                    continue;

                minTop = minTop.HasValue ? Math.Min(minTop.Value, top) : top;
                maxBottom = maxBottom.HasValue ? Math.Max(maxBottom.Value, bottom) : bottom;
            }

            if (minTop.HasValue)
                ScrollRangeIntoView(minTop.Value, maxBottom.Value);
        }

        private void ScrollRangeIntoView(int top, int bottom)
        {
            int viewportHeight = ClientSize.Height;

            if (bottom - top > viewportHeight)
                _scrollOffset = top; // Anchors to top if range is taller than viewport.
            else if (bottom > _scrollOffset + viewportHeight)
                _scrollOffset = bottom - viewportHeight; // Off the bottom - scroll down.
            else if (top < _scrollOffset)
                _scrollOffset = top; // Off the top - scroll up.
            else
                return; // Already fully visible.

            ClampScrollOffset();
        }

        private bool TryGetNodeVerticalBounds(object node, out int top, out int bottom)
        {
            foreach (var row in _rows)
            {
                if (row is HeaderVisualRow headerRow && headerRow.Category == node)
                {
                    top = row.Top;
                    bottom = row.Top + row.Height;
                    return true;
                }

                if (row is ItemsVisualRow itemsRow && node is ImageItem item && itemsRow.Items.Contains(item))
                {
                    top = row.Top;
                    bottom = row.Top + row.Height;
                    return true;
                }
            }

            top = 0;
            bottom = 0;
            return false;
        }

        // Binary search for the row (if any) whose vertical span contains content-space y.
        private int FindRowIndexAtY(int y)
        {
            int lo = 0, hi = _rows.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                var row = _rows[mid];
                if (y < row.Top) hi = mid - 1;
                else if (y >= row.Top + row.Height) lo = mid + 1;
                else return mid;
            }
            return -1;
        }

        // Binary search for the first row that is partially at current scroll offset.
        private int FindFirstVisibleRowIndex()
        {
            int lo = 0, hi = _rows.Count - 1, answer = _rows.Count;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (_rows[mid].Top + _rows[mid].Height > _scrollOffset) { answer = mid; hi = mid - 1; }
                else lo = mid + 1;
            }
            return answer;
        }

        #endregion

        #region Hit testing

        private struct HitResult
        {
            public object Node;
            public bool OnHeaderArrow;
            public bool OnHeaderIndicator;
        }

        private const int HeaderIndicatorRightMargin = 10; // Right margin (pixels) between the enabled/disabled indicator dot and scrollbar.

        private const int HeaderIndicatorSize = 10; // Diameter (pixels) of enabled/disabled indicator dot.

        private const int HeaderTextStartX = 28; // Left edge (pixels) where header's text label starts (just after the arrow).

        private int GetHeaderIndicatorX() // Left edge, in pixels, of the header's enabled/disabled indicator dot.
        {
            int rowWidth = ClientSize.Width - ScrollBarWidth;
            return rowWidth - HeaderIndicatorSize - HeaderIndicatorRightMargin;
        }

        private HitResult HitTest(Point location)
        {
            var result = new HitResult();
            if (location.X >= ClientSize.Width - ScrollBarWidth)
                return result;

            int rowIndex = FindRowIndexAtY(location.Y + _scrollOffset);
            if (rowIndex < 0)
                return result;

            var row = _rows[rowIndex];

            if (row is HeaderVisualRow header)
            {
                int indicatorX = GetHeaderIndicatorX();
                result.Node = header.Category;
                result.OnHeaderArrow = location.X >= 4 && location.X <= 24;
                result.OnHeaderIndicator = location.X >= indicatorX - 4 && location.X <= indicatorX + HeaderIndicatorSize + 4;
                return result;
            }

            if (row is ItemsVisualRow itemsRow)
            {
                int cellStride = ItemSize + ItemPadding;
                int localX = location.X - ItemPadding;
                if (localX < 0) return result;

                int column = localX / cellStride;
                if (column < 0 || column >= itemsRow.Items.Count) return result;

                int cellLeft = ItemPadding + column * cellStride;
                if (location.X < cellLeft || location.X > cellLeft + ItemSize) return result; // Gap between cells.

                result.Node = itemsRow.Items[column];
            }

            return result;
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(IdleBackColor);

            if (_rows.Count > 0)
            {
                int firstIndex = FindFirstVisibleRowIndex();
                int viewBottom = _scrollOffset + ClientSize.Height;

                for (int i = firstIndex; i < _rows.Count; i++)
                {
                    var row = _rows[i];
                    if (row.Top >= viewBottom)
                        break;

                    int drawTop = row.Top - _scrollOffset;

                    if (row is HeaderVisualRow header)
                        DrawHeaderRow(g, header, drawTop);
                    else if (row is ItemsVisualRow itemsRow)
                        DrawItemsRow(g, itemsRow, drawTop);

                    bool isLastRow = i == _rows.Count - 1;
                    if (isLastRow && _isDragging && _draggedCategories != null && _categoryDropHasTarget && _categoryDropBeforeCategory == null)
                        DrawCategoryDropLine(g, drawTop + row.Height);
                }
            }

            DrawScrollBar(g);
        }

        private void DrawHeaderRow(Graphics g, HeaderVisualRow row, int drawTop)
        {
            var category = row.Category;
            bool selected = _selectionSet.Contains(category);
            bool beingDragged = _isDragging && _draggedCategories != null && _draggedCategories.Contains(category);
            int rowWidth = ClientSize.Width - ScrollBarWidth;

            using (var bgBrush = new SolidBrush(selected ? SelectedBackColor : IdleBackColor))
                g.FillRectangle(bgBrush, 0, drawTop, rowWidth, row.Height);

            if (beingDragged || (_clipboardIsCut && _clipboard.Contains(category)))
            {
                using (var fadeBrush = new SolidBrush(Color.FromArgb(140, IdleBackColor)))
                    g.FillRectangle(fadeBrush, 0, drawTop, rowWidth, row.Height);
            }

            const int arrowSize = 8;
            const int arrowX = 10;
            int arrowY = drawTop + row.Height / 2;

            using (var arrowBrush = new SolidBrush(selected ? SelectedHeaderTextColor : HeaderTextColor))
            {
                Point[] points = category.IsExpanded
                    ? new[]
                    {
                        new Point(arrowX, arrowY - arrowSize / 2),
                        new Point(arrowX + arrowSize, arrowY - arrowSize / 2),
                        new Point(arrowX + arrowSize / 2, arrowY + arrowSize / 2)
                    }
                    : new[]
                    {
                        new Point(arrowX, arrowY - arrowSize / 2),
                        new Point(arrowX, arrowY + arrowSize / 2),
                        new Point(arrowX + arrowSize, arrowY)
                    };

                g.FillPolygon(arrowBrush, points);
            }

            int enabledCount = category.Items.Count(i => i.IsEnabled);
            int totalCount = category.Items.Count;
            int indicatorX = GetHeaderIndicatorX();
            int indicatorY = arrowY - HeaderIndicatorSize / 2;
            var indicatorRect = new Rectangle(indicatorX, indicatorY, HeaderIndicatorSize, HeaderIndicatorSize);

            if (totalCount > 0 && enabledCount == totalCount)
            {
                // All children enabled = solid dot.
                using (var brush = new SolidBrush(EnabledHighlightColor))
                    g.FillEllipse(brush, indicatorRect);
            }
            else if (enabledCount > 0)
            {
                // Some children enabled = hollow ring.
                using (var pen = new Pen(EnabledHighlightColor, 2f))
                    g.DrawEllipse(pen, indicatorRect);
            }
            // No children enabled = no indicator drawn.

            string label = _showItemCountInHeader
                ? $"{category.Name} ({category.Items.Count})"
                : category.Name;
            using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(selected ? SelectedHeaderTextColor : HeaderTextColor))
            using (var format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                int textWidth = Math.Max(0, indicatorX - HeaderTextStartX - 8);
                var textRect = new RectangleF(HeaderTextStartX, drawTop, textWidth, row.Height);
                g.DrawString(label, font, textBrush, textRect, format);
            }

            if (_isDragging && _draggedItems != null && _dropTargetCategory == category && _dropTargetBeforeItem == null)
            {
                using (var pen = new Pen(DropIndicatorColor, 2f))
                    g.DrawRectangle(pen, 1, drawTop + 1, rowWidth - 2, row.Height - 2);
            }

            if (_isDragging && _draggedCategories != null && _categoryDropHasTarget && _categoryDropBeforeCategory == category)
                DrawCategoryDropLine(g, drawTop);
        }

        private void DrawItemsRow(Graphics g, ItemsVisualRow row, int drawTop)
        {
            int x = ItemPadding;
            int cellStride = ItemSize + ItemPadding;

            // Reference equality (visually) on the exact row that was hovered
            // Used mainly for end/start of rows.
            bool showCaretInThisRow = _isDragging && _dropTargetVisualRow == row;

            for (int i = 0; i < row.Items.Count; i++)
            {
                if (showCaretInThisRow && _dropTargetLocalSlot == i)
                    DrawInsertionCaret(g, x, drawTop);

                DrawItemCell(g, row.Items[i], x, drawTop);
                x += cellStride;
            }

            if (showCaretInThisRow && _dropTargetLocalSlot == row.Items.Count)
                DrawInsertionCaret(g, x, drawTop);

            if (_isDragging && _draggedCategories != null && _draggedCategories.Contains(row.Category))
            {
                int rowWidth = ClientSize.Width - ScrollBarWidth;
                using (var fadeBrush = new SolidBrush(Color.FromArgb(140, IdleBackColor)))
                    g.FillRectangle(fadeBrush, 0, drawTop, rowWidth, row.Height);
            }
        }

        private void DrawInsertionCaret(Graphics g, int x, int y)
        {
            using (var pen = new Pen(DropIndicatorColor, 3f))
                g.DrawLine(pen, x - ItemPadding / 2, y, x - ItemPadding / 2, y + ItemSize);
        }

        // Horizontal line marking where a dragged category will land.
        private void DrawCategoryDropLine(Graphics g, int y)
        {
            int width = ClientSize.Width - ScrollBarWidth;
            using (var pen = new Pen(DropIndicatorColor, 3f))
                g.DrawLine(pen, 0, y, width, y);
        }

        private void DrawItemCell(Graphics g, ImageItem item, int x, int y)
        {
            bool selected = _selectionSet.Contains(item);
            bool beingDragged = _isDragging && _draggedItems != null && _draggedItems.Contains(item);
            var cellRect = new Rectangle(x, y, ItemSize, ItemSize);

            using (var path = RoundedRect(cellRect, CornerRadius))
            {
                // Background of node color, determined based on enabled, selected, or idle.
                Color backgroundColor;
                if (selected)
                    backgroundColor = item.IsEnabled ? EnabledHighlightColor : SelectedBackColor;
                else
                    backgroundColor = IdleBackColor;

                using (var bgBrush = new SolidBrush(backgroundColor))
                    g.FillPath(bgBrush, path);

                if (!selected)
                {
                    var borderColor = item.IsEnabled ? EnabledHighlightColor : IdleBorderColor;
                    float borderWidth = item.IsEnabled ? 2f : 1f;
                    using (var pen = new Pen(borderColor, borderWidth))
                        g.DrawPath(pen, path);
                }
            }

            if (beingDragged || (_clipboardIsCut && _clipboard.Contains(item)))
            {
                // "Ghost" look for items currently being dragged or Cut.
                using (var fadeBrush = new SolidBrush(Color.FromArgb(140, IdleBackColor)))
                    g.FillPath(fadeBrush, RoundedRect(cellRect, CornerRadius));
            }

            const int inset = 6;
            var imageRect = new Rectangle(x + inset, y + inset, ItemSize - inset * 2, ItemSize - inset * 2);

            if (_isDesignMode)
            {
                // Ignores thumbnail cache in VS design mode using a placeholder.
                DrawPlaceholderThumbnail(g, imageRect);
            }
            else
            {
                Image thumbnail = string.IsNullOrEmpty(item.Id) ? null : ImageCache.TryGet(item.Id);
                if (thumbnail == null)
                    ImageCache.RequestLoad(item.Id, item.ThumbnailUrl);
                else
                    g.DrawImage(thumbnail, imageRect);
            }

            using (var font = new Font("Segoe UI", 7.5f))
            using (var brush = new SolidBrush(ItemTitleColor))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                var labelRect = new RectangleF(x - 4, y + ItemSize + 2, ItemSize + 8, ItemLabelHeight);
                g.DrawString(item.Title, font, brush, labelRect, format);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Placeholder thumbnails for VS designer.
        private static void DrawPlaceholderThumbnail(Graphics g, Rectangle rect)
        {
            var glyphColor = Color.FromArgb(90, 90, 90);

            using (var pen = new Pen(glyphColor, 1.5f))
            {
                g.DrawRectangle(pen, rect);

                var points = new[]
                {
                    new Point(rect.Left + 3, rect.Bottom - 3),
                    new Point(rect.Left + rect.Width / 3, rect.Top + rect.Height / 3),
                    new Point(rect.Left + rect.Width * 2 / 3, rect.Bottom - rect.Height / 3),
                    new Point(rect.Right - 3, rect.Bottom - 3)
                };
                g.DrawLines(pen, points);
            }

            using (var brush = new SolidBrush(glyphColor))
            {
                int sunSize = Math.Max(4, rect.Width / 6);
                g.FillEllipse(brush, rect.Right - sunSize - 4, rect.Top + 4, sunSize, sunSize);
            }
        }

        #endregion

        #region Custom scrollbar

        private Rectangle GetTrackRect() => new Rectangle(ClientSize.Width - ScrollBarWidth, 0, ScrollBarWidth, ClientSize.Height);

        private Rectangle GetThumbRect()
        {
            var track = GetTrackRect();
            if (_contentHeight <= ClientSize.Height)
                return Rectangle.Empty;

            double visibleRatio = (double)ClientSize.Height / _contentHeight;
            int thumbHeight = Math.Max(MinThumbHeight, (int)(track.Height * visibleRatio));

            int scrollRange = Math.Max(1, _contentHeight - ClientSize.Height);
            double scrollRatio = (double)_scrollOffset / scrollRange;
            int thumbTop = (int)(scrollRatio * (track.Height - thumbHeight));

            return new Rectangle(track.X + 2, thumbTop, track.Width - 4, thumbHeight);
        }

        private void DrawScrollBar(Graphics g)
        {
            var track = GetTrackRect();
            using (var trackBrush = new SolidBrush(TrackColor))
                g.FillRectangle(trackBrush, track);

            var thumb = GetThumbRect();
            if (thumb.IsEmpty)
                return;

            using (var thumbBrush = new SolidBrush(_thumbHot || _isDraggingThumb ? ThumbHoverColor : ThumbColor))
            using (var path = RoundedRect(thumb, 4))
                g.FillPath(thumbBrush, path);
        }

        #endregion

        #region Mouse / keyboard input

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Finishes rename if user clicked off the control.
            if (_renameTextBox != null)
                CommitRename();

            Focus();

            if (e.Button == MouseButtons.Right)
            {
                HandleRightClick(e.Location);
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            var thumb = GetThumbRect();
            if (!thumb.IsEmpty && thumb.Contains(e.Location))
            {
                _isDraggingThumb = true;
                _dragStartMouseY = e.Y;
                _dragStartScrollOffset = _scrollOffset;
                return;
            }

            if (e.X >= ClientSize.Width - ScrollBarWidth)
            {
                if (e.Y < GetThumbRect().Top) ScrollByPixels(-ClientSize.Height);
                else ScrollByPixels(ClientSize.Height);
                return;
            }

            var hit = HitTest(e.Location);

            if (hit.Node is ImageCategory hitCategory)
            {

                if (hit.OnHeaderArrow)
                {
                    ToggleCategoryExpansion(hitCategory);
                    return;
                }

                // Can uncomment to allow for the enabled indicator to also be used as a means of enabling all nodes in a category.
                /*if (hit.OnHeaderIndicator)
                {
                    ToggleCategoryEnabledState(hitCategory);
                    return;
                }*/
            }

            if (hit.Node == null)
            {
                ClearSelection();
                return;
            }

            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

            // Both items and category headers can be dragged.
            bool isDraggableCandidate = hit.Node is ImageItem || hit.Node is ImageCategory;

            if (isDraggableCandidate && _selectionSet.Contains(hit.Node) && !shift)
            {
                // Defers click's effect on selection until MouseUp rather than applying immediately.
                _pendingSelectClick = true;
            }
            else
            {
                SelectNode(hit.Node);
            }

            if (isDraggableCandidate)
            {
                _dragCandidateNode = hit.Node;
                _dragCandidateLocation = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDraggingThumb)
            {
                var track = GetTrackRect();
                var thumb = GetThumbRect();
                int trackRange = Math.Max(1, track.Height - thumb.Height);
                int scrollRange = Math.Max(1, _contentHeight - ClientSize.Height);

                int deltaPixels = e.Y - _dragStartMouseY;
                _scrollOffset = _dragStartScrollOffset + (int)(deltaPixels * ((double)scrollRange / trackRange));
                ClampScrollOffset();
                RepositionRenameBoxIfActive();
                Invalidate();
                return;
            }

            if (_isDragging)
            {
                _lastDragMouseLocation = e.Location;
                UpdateActiveDropTarget(e.Location);
                UpdateAutoScroll(e.Location);
                Invalidate();
                return;
            }

            if (_dragCandidateNode != null)
            {
                int dx = e.X - _dragCandidateLocation.X;
                int dy = e.Y - _dragCandidateLocation.Y;
                if (Math.Abs(dx) > DragThreshold || Math.Abs(dy) > DragThreshold)
                {
                    BeginDrag();
                    _lastDragMouseLocation = e.Location;
                    UpdateActiveDropTarget(e.Location);
                    Invalidate();
                }
                return;
            }

            bool wasHot = _thumbHot;
            var thumbRect = GetThumbRect();
            _thumbHot = !thumbRect.IsEmpty && thumbRect.Contains(e.Location);
            if (_thumbHot != wasHot)
                Invalidate(GetTrackRect());
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isDraggingThumb = false;

            if (_isDragging)
            {
                bool copy = (ModifierKeys & Keys.Control) == Keys.Control;
                CompleteDrag(copy);
            }
            else if (_pendingSelectClick && _dragCandidateNode != null)
            {
                SelectNode(_dragCandidateNode);
            }

            EndDragState();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_thumbHot)
            {
                _thumbHot = false;
                Invalidate(GetTrackRect());
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollByPixels(-e.Delta);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.X >= ClientSize.Width - ScrollBarWidth)
                return;

            var hit = HitTest(e.Location);

            // Prevents caret from being affected by double-clicking to expand a tree.
            if (hit.Node is ImageCategory category && !hit.OnHeaderArrow /*&& !hit.OnHeaderIndicator*/) // Can uncomment if OnHeaderIndicator functionality is re-enabled.
                ToggleCategoryExpansion(category);
        }

        private void ScrollByPixels(int delta)
        {
            _scrollOffset += delta;
            ClampScrollOffset();
            RepositionRenameBoxIfActive();
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData) => true;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedNodes();
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelection();
            }
            else if (e.Control && e.KeyCode == Keys.X)
            {
                CutSelection();
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                Paste();
            }
            else if (e.KeyCode == Keys.F2 && _selectionOrder.Count == 1)
            {
                BeginRename();
            }
            else if (e.KeyCode == Keys.Escape && _isDragging)
            {
                EndDragState();
                Invalidate();
            }
        }

        #endregion

        #region Right-click context menu

        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _copyMenuItem;
        private ToolStripMenuItem _cutMenuItem;
        private ToolStripMenuItem _pasteMenuItem;
        private ToolStripMenuItem _renameMenuItem;
        private ToolStripMenuItem _deleteMenuItem;

        private void BuildContextMenu()
        {
            _contextMenu = new ContextMenuStrip
            {
                Renderer = new DarkContextMenuRenderer(),
                ShowImageMargin = false,
                BackColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 9F)
            };

            _copyMenuItem = new ToolStripMenuItem("Copy", null, (s, e) => CopySelection());
            _cutMenuItem = new ToolStripMenuItem("Cut", null, (s, e) => CutSelection());
            _pasteMenuItem = new ToolStripMenuItem("Paste", null, (s, e) => Paste());
            _renameMenuItem = new ToolStripMenuItem("Rename", null, (s, e) => BeginRename());
            _deleteMenuItem = new ToolStripMenuItem("Delete", null, (s, e) => RemoveSelectedNodes());

            _contextMenu.Items.Add(_copyMenuItem);
            _contextMenu.Items.Add(_cutMenuItem);
            _contextMenu.Items.Add(_pasteMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(_renameMenuItem);
            _contextMenu.Items.Add(_deleteMenuItem);

            // Enable/disable state is recomputed every time the menu opens rather than after every selection change (cheaper).
            _contextMenu.Opening += (s, e) =>
            {
                _copyMenuItem.Enabled = CanCopy;
                _cutMenuItem.Enabled = CanCut;
                _pasteMenuItem.Enabled = CanPaste;
                _renameMenuItem.Enabled = _selectionOrder.Count == 1; // Renaming more than one node at once doesn't make sense.
                _deleteMenuItem.Enabled = _selectionOrder.Count > 0;
            };
        }

        // Right-clicking a node that's part of the current multi-selection leaves the selection intact.
        // Right-clicking an unselected node empty space replaces the selection first like a plain left click.
        private void HandleRightClick(Point location)
        {
            if (location.X >= ClientSize.Width - ScrollBarWidth)
                return;

            var hit = HitTest(location);

            if (hit.Node == null)
                ClearSelection();
            else if (!_selectionSet.Contains(hit.Node))
                SelectNode(hit.Node);

            _contextMenu.Show(this, location);
        }

        private sealed class DarkContextMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkContextMenuRenderer() : base(new DarkContextMenuColorTable()) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? Color.FromArgb(153, 153, 153) : Color.FromArgb(90, 90, 90);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (var pen = new Pen(Color.FromArgb(60, 60, 60)))
                    e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                var bounds = new Rectangle(Point.Empty, e.Item.Size);
                using (var pen = new Pen(Color.FromArgb(60, 60, 60)))
                    e.Graphics.DrawLine(pen, 4, bounds.Height / 2, bounds.Width - 4, bounds.Height / 2);
            }
        }

        private sealed class DarkContextMenuColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(75, 75, 75);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(75, 75, 75);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(75, 75, 75);
            public override Color MenuItemBorder => Color.FromArgb(0, 188, 71);
            public override Color MenuBorder => Color.FromArgb(60, 60, 60);
            public override Color ToolStripDropDownBackground => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientEnd => Color.FromArgb(30, 30, 30);
        }

        #endregion

        #region Inline rename

        private TextBox _renameTextBox;
        private Font _renameFont;
        private object _renameTarget;

        // Starts inline-editing the display name of a node, Esc discards the rename.
        public void BeginRename(object node = null)
        {
            node = node ?? _selectionOrder.FirstOrDefault();
            if (node == null)
                return;

            CommitRename(); // Finish any rename already in progress first.

            string currentText;
            bool centered;

            if (node is ImageCategory category)
            {
                currentText = category.Name;
                centered = false;
            }
            else if (node is ImageItem item)
            {
                currentText = item.Title;
                centered = true;
            }
            else
            {
                return;
            }

            if (!TryGetNodeLabelBounds(node, out var bounds))
                return;

            _renameTarget = node;
            _renameFont = node is ImageCategory
                ? new Font("Segoe UI", 10f, FontStyle.Bold)
                : new Font("Segoe UI", 7.5f);

            _renameTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = _renameFont,
                Text = currentText,
                TextAlign = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left
            };

            _renameTextBox.KeyDown += RenameTextBox_KeyDown;
            _renameTextBox.LostFocus += RenameTextBox_LostFocus;

            Controls.Add(_renameTextBox);
            PositionRenameBox(bounds);
            _renameTextBox.BringToFront();
            _renameTextBox.Focus();
            _renameTextBox.SelectAll();
        }

        private void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Otherwise it also dings/adds a newline.
                CommitRename();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                CancelRename();
            }
        }

        private void RenameTextBox_LostFocus(object sender, EventArgs e) => CommitRename();

        private void CommitRename()
        {
            var box = _renameTextBox;
            var target = _renameTarget;
            if (box == null || target == null)
                return;

            string newText = box.Text.Trim();
            RemoveRenameBox();

            // Discard an empty name.
            if (string.IsNullOrEmpty(newText))
                return;

            if (target is ImageCategory category)
            {
                category.Name = newText;
            }
            else if (target is ImageItem item)
            {
                item.Title = newText;

                // When copies intentionally share an Id (GenerateNewIdOnCopy == false), keeps display names in sync.
                if (!GenerateNewIdOnCopy && !string.IsNullOrEmpty(item.Id))
                {
                    foreach (var other in _parentMap.Keys)
                    {
                        if (other != item && other.Id == item.Id)
                            other.Title = newText;
                    }
                }
            }
            else
            {
                return;
            }

            RebuildLayout();
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CancelRename() => RemoveRenameBox();

        private void RemoveRenameBox()
        {
            var box = _renameTextBox;
            if (box == null)
                return;

            // Null fields before touching Controls collectionto prevent re-firing LostFocus.
            _renameTextBox = null;
            _renameTarget = null;

            box.KeyDown -= RenameTextBox_KeyDown;
            box.LostFocus -= RenameTextBox_LostFocus;
            Controls.Remove(box);
            box.Dispose();

            _renameFont?.Dispose();
            _renameFont = null;
        }

        private void PositionRenameBox(RectangleF bounds)
        {
            var box = _renameTextBox;
            if (box == null)
                return;

            var expanded = ComputeExpandedRenameBounds(_renameTarget, bounds, box.Text);

            box.Multiline = expanded.NeedsWrap;
            box.WordWrap = expanded.NeedsWrap;
            box.SetBounds((int)expanded.Bounds.X, (int)expanded.Bounds.Y, (int)expanded.Bounds.Width, (int)expanded.Bounds.Height);
        }

        private struct ExpandedRenameBounds
        {
            public RectangleF Bounds;
            public bool NeedsWrap;
        }

        // Grows rename box so the full name is visible while editing.
        private ExpandedRenameBounds ComputeExpandedRenameBounds(object node, RectangleF baseBounds, string text)
        {
            if (string.IsNullOrEmpty(text) || _renameFont == null)
                return new ExpandedRenameBounds { Bounds = baseBounds, NeedsWrap = false };

            const float horizontalPadding = 12f; // Room for the textbox's border/internal margin.

            float neededWidth;
            using (var g = CreateGraphics())
                neededWidth = g.MeasureString(text, _renameFont, int.MaxValue, StringFormat.GenericTypographic).Width + horizontalPadding;

            if (neededWidth <= baseBounds.Width)
                return new ExpandedRenameBounds { Bounds = baseBounds, NeedsWrap = false };

            float availableLeft = 0f;
            float availableRight = ClientSize.Width - ScrollBarWidth;
            float availableWidth = Math.Max(baseBounds.Width, availableRight - availableLeft);

            bool centered = node is ImageItem; // Item labels are centered under their cell; headers are left-aligned.
            float width = Math.Min(neededWidth, availableWidth);
            float x;

            if (centered)
            {
                float centerX = baseBounds.X + baseBounds.Width / 2f;
                x = centerX - width / 2f;
            }
            else
            {
                x = baseBounds.X;
            }

            if (x < availableLeft) x = availableLeft;
            if (x + width > availableRight) x = Math.Max(availableLeft, availableRight - width);

            bool needsWrap = neededWidth > width; // Even at full available width, the text still doesn't fit on one line.
            float height = baseBounds.Height;

            if (needsWrap)
            {
                int lineCount = Math.Max(1, (int)Math.Ceiling(neededWidth / width));
                height = baseBounds.Height * lineCount;
            }

            return new ExpandedRenameBounds
            {
                Bounds = new RectangleF(x, baseBounds.Y, width, height),
                NeedsWrap = needsWrap
            };
        }

        // Keeps the rename textbox glued to its node's label whenever the scroll position or the control's size changes underneath it.
        private void RepositionRenameBoxIfActive()
        {
            if (_renameTarget == null)
                return;

            if (TryGetNodeLabelBounds(_renameTarget, out var bounds))
                PositionRenameBox(bounds);
            else
                CommitRename();
        }

        // Finds on-screen rect of a category header's name label or an item's title label.
        private bool TryGetNodeLabelBounds(object node, out RectangleF bounds)
        {
            foreach (var row in _rows)
            {
                if (row is HeaderVisualRow headerRow && headerRow.Category == node)
                {
                    int indicatorX = GetHeaderIndicatorX();
                    int textWidth = Math.Max(0, indicatorX - HeaderTextStartX - 8);
                    int drawTop = row.Top - _scrollOffset;

                    bounds = new RectangleF(HeaderTextStartX, drawTop, textWidth, row.Height);
                    return true;
                }

                if (row is ItemsVisualRow itemsRow && node is ImageItem item)
                {
                    int index = itemsRow.Items.IndexOf(item);
                    if (index < 0)
                        continue;

                    int cellStride = ItemSize + ItemPadding;
                    int x = ItemPadding + index * cellStride;
                    int drawTop = row.Top - _scrollOffset;

                    bounds = new RectangleF(x - 4, drawTop + ItemSize + 2, ItemSize + 8, ItemLabelHeight);
                    return true;
                }
            }

            bounds = RectangleF.Empty;
            return false;
        }

        #endregion

        #region Drag and drop between categories

        // Called on every mouse-move while dragging items to update auto-scroll timer based on movement.
        private void UpdateAutoScroll(Point location)
        {
            int direction = 0;

            if (_contentHeight > ClientSize.Height)
            {
                if (location.Y < AutoScrollEdgeSize && _scrollOffset > 0)
                    direction = -1;
                else if (location.Y > ClientSize.Height - AutoScrollEdgeSize && _scrollOffset < _contentHeight - ClientSize.Height)
                    direction = 1;
            }

            _autoScrollDirection = direction;

            if (direction != 0)
            {
                if (!_autoScrollTimer.Enabled)
                    _autoScrollTimer.Start();
            }
            else
            {
                _autoScrollTimer.Stop();
            }
        }

        private void OnAutoScrollTick(object sender, EventArgs e)
        {
            if (!_isDragging || _autoScrollDirection == 0)
            {
                _autoScrollTimer.Stop();
                return;
            }

            ScrollByPixels(_autoScrollDirection * ComputeAutoScrollSpeed(_lastDragMouseLocation.Y));

            // Content just moved under a possibly stationary cursor, recomputes what is under it and redraws caret there to not be behind.
            UpdateDropTarget(_lastDragMouseLocation);
            Invalidate();
        }

        // Faster the closer the cursor is to the exact top/bottom edge, slower near the inner boundary of the trigger zone.
        private int ComputeAutoScrollSpeed(int y)
        {
            int distanceFromEdge = y < AutoScrollEdgeSize ? y : ClientSize.Height - y;
            distanceFromEdge = Math.Max(0, Math.Min(AutoScrollEdgeSize, distanceFromEdge));

            double t = 1.0 - (double)distanceFromEdge / AutoScrollEdgeSize;
            return AutoScrollMinSpeed + (int)((AutoScrollMaxSpeed - AutoScrollMinSpeed) * t);
        }

        private void BeginDrag()
        {
            _isDragging = true;
            _pendingSelectClick = false;

            if (_dragCandidateNode is ImageCategory candidateCategory)
            {
                _draggedCategories = _selectionSet.Contains(candidateCategory)
                    ? SelectedCategories.ToList()
                    : new List<ImageCategory> { candidateCategory };
                _draggedItems = null;
            }
            else
            {
                _draggedItems = _selectionSet.Contains(_dragCandidateNode)
                    ? SelectedItems.ToList()
                    : new List<ImageItem> { (ImageItem)_dragCandidateNode };
                _draggedCategories = null;
            }

            Cursor = Cursors.Hand;

            // Without capture, mouse events stop arriving the instant the cursor leaves the control's bounds.
            Capture = true;
        }

        private void UpdateActiveDropTarget(Point location)
        {
            if (_draggedCategories != null)
                UpdateCategoryDropTarget(location);
            else
                UpdateDropTarget(location);
        }

        // Resolves where a dragged group of categories would land.
        private void UpdateCategoryDropTarget(Point location)
        {
            if (location.X >= ClientSize.Width - ScrollBarWidth)
            {
                _categoryDropHasTarget = false;
                _categoryDropBeforeCategory = null;
                return;
            }

            int contentY = location.Y + _scrollOffset;
            int rowIndex = FindRowIndexAtY(contentY);

            if (rowIndex < 0)
            {
                // Below everything (or above it, if the content doesn't fill the control).
                bool pastEnd = _rows.Count > 0 && contentY >= _rows[_rows.Count - 1].Top + _rows[_rows.Count - 1].Height;
                _categoryDropHasTarget = pastEnd;
                _categoryDropBeforeCategory = null;
                return;
            }

            var row = _rows[rowIndex];
            ImageCategory hoveredCategory = (row as HeaderVisualRow)?.Category ?? (row as ItemsVisualRow)?.Category;

            if (hoveredCategory == null || !TryGetCategoryBlockBounds(hoveredCategory, out int blockTop, out int blockBottom))
            {
                _categoryDropHasTarget = false;
                _categoryDropBeforeCategory = null;
                return;
            }

            _categoryDropHasTarget = true;
            bool topHalf = contentY < (blockTop + blockBottom) / 2;

            if (topHalf)
            {
                _categoryDropBeforeCategory = hoveredCategory;
            }
            else
            {
                int index = _categories.IndexOf(hoveredCategory);
                _categoryDropBeforeCategory = (index >= 0 && index + 1 < _categories.Count) ? _categories[index + 1] : null;
            }
        }

        // Finds the top/bottom content-space bounds spanning every visual row (header and item rows alike) that belongs to the given category.
        private bool TryGetCategoryBlockBounds(ImageCategory category, out int top, out int bottom)
        {
            top = 0;
            bottom = 0;
            bool found = false;

            foreach (var row in _rows)
            {
                bool belongsToCategory = (row as HeaderVisualRow)?.Category == category || (row as ItemsVisualRow)?.Category == category;
                if (!belongsToCategory)
                    continue;

                if (!found)
                {
                    top = row.Top;
                    found = true;
                }

                bottom = row.Top + row.Height;
            }

            return found;
        }

        private void UpdateDropTarget(Point location)
        {
            if (location.X >= ClientSize.Width - ScrollBarWidth)
            {
                ClearDropTarget();
                return;
            }

            int rowIndex = FindRowIndexAtY(location.Y + _scrollOffset);
            if (rowIndex < 0)
            {
                ClearDropTarget();
                return;
            }

            var row = _rows[rowIndex];

            if (row is HeaderVisualRow header)
            {
                _dropTargetCategory = header.Category;
                _dropTargetBeforeItem = null; // Drop = append to this category.
                _dropTargetGlobalIndex = header.Category.Items.Count;
                _dropTargetVisualRow = null; // No items row involved, the header's own highlight is the visual cue.
                _dropTargetLocalSlot = 0;
                return;
            }

            if (row is ItemsVisualRow itemsRow)
            {
                _dropTargetCategory = itemsRow.Category;
                _dropTargetBeforeItem = ResolveDropAnchor(itemsRow, location.X, out _dropTargetGlobalIndex, out _dropTargetLocalSlot);
                _dropTargetVisualRow = itemsRow;
                return;
            }

            ClearDropTarget();
        }

        private void ClearDropTarget()
        {
            _dropTargetCategory = null;
            _dropTargetBeforeItem = null;
            _dropTargetGlobalIndex = -1;
            _dropTargetVisualRow = null;
            _dropTargetLocalSlot = 0;
        }

        // Resolves an x coordinate within an items row to an insertion anchor:
        // Hovering the left half of a cell (& gap before it) targets "insert before the item"
        // Hovering the right half of a cell (& gap after it) targets "insert after that item".
        private ImageItem ResolveDropAnchor(ItemsVisualRow row, int x, out int globalIndex, out int localSlot)
        {
            int cellStride = ItemSize + ItemPadding;
            int localX = x - ItemPadding;

            int rawSlot;
            if (localX < 0)
            {
                rawSlot = 0;
            }
            else
            {
                int column = localX / cellStride;
                int withinCell = localX - column * cellStride;
                bool rightHalf = withinCell >= ItemSize / 2; // Trailing padding gap counts as the right half of its column.
                rawSlot = column + (rightHalf ? 1 : 0);
            }

            localSlot = Math.Min(rawSlot, row.Items.Count);

            var category = row.Category;
            globalIndex = row.StartIndex + localSlot;

            // Returns the raw item at this position to render the caret in the right spot.
            return globalIndex < category.Items.Count ? category.Items[globalIndex] : null;
        }

        private void CompleteDrag(bool copy)
        {
            if (_draggedCategories != null)
            {
                CompleteCategoryDrag(copy);
            }
            else if (_dropTargetCategory != null && _draggedItems != null && _draggedItems.Count > 0)
            {
                List<ImageItem> affectedItems;

                if (copy)
                {
                    // Copies are brand-new ImageItem instances.
                    var copies = _draggedItems
                        .Select(item => CopyItemToCategory(item, _dropTargetCategory, _dropTargetBeforeItem))
                        .ToList();

                    ClearSelectionInternal();
                    foreach (var newItem in copies)
                        AddToSelection(newItem);

                    affectedItems = copies;
                }
                else
                {
                    // For an actual move, the hover anchor might itself be one of the items being moved.
                    // Resolves to the nearest not-moved item up front.
                    var stableAnchor = ResolveStableAnchor(_dropTargetCategory, _dropTargetBeforeItem, _draggedItems);

                    foreach (var item in _draggedItems)
                        MoveItemToCategory(item, _dropTargetCategory, stableAnchor);

                    affectedItems = _draggedItems;
                }

                RebuildLayout();
                ScrollNodesIntoView(affectedItems);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                DataChanged?.Invoke(this, EventArgs.Empty);
            }

            EndDragState();
        }

        // Reorders (or, with Ctrl held, duplicates) the dragged categories to just before the resolved drop target.
        private void CompleteCategoryDrag(bool copy)
        {
            if (!_categoryDropHasTarget || _draggedCategories == null || _draggedCategories.Count == 0)
                return;

            List<ImageCategory> affectedCategories;

            if (copy)
            {
                // Copies are brand-new ImageCategory instances.
                var copies = _draggedCategories
                    .Select(category => CopyCategory(category, _categoryDropBeforeCategory))
                    .ToList();

                ClearSelectionInternal();
                foreach (var newCategory in copies)
                    AddToSelection(newCategory);

                affectedCategories = copies;
            }
            else
            {
                var stableAnchor = ResolveStableCategoryAnchor(_categoryDropBeforeCategory, _draggedCategories);

                foreach (var category in _draggedCategories)
                    _categories.Remove(category);

                if (stableAnchor == null)
                {
                    _categories.AddRange(_draggedCategories);
                }
                else
                {
                    int insertIndex = _categories.IndexOf(stableAnchor);
                    _categories.InsertRange(insertIndex, _draggedCategories);
                }

                affectedCategories = _draggedCategories;
            }

            RebuildLayout();
            ScrollNodesIntoView(affectedCategories);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        // Walks forward from rawAnchor to the nearest item that isn't itself one of the items being
        // moved, so a move can use a fixed reference point that won't be relocated.
        private static ImageItem ResolveStableAnchor(ImageCategory category, ImageItem rawAnchor, ICollection<ImageItem> movingItems)
        {
            if (rawAnchor == null || !movingItems.Contains(rawAnchor))
                return rawAnchor;

            int index = category.Items.IndexOf(rawAnchor);
            if (index < 0)
                return null;

            index++;
            while (index < category.Items.Count && movingItems.Contains(category.Items[index]))
                index++;

            return index < category.Items.Count ? category.Items[index] : null;
        }

        // Same idea as ResolveStableAnchor but for top-level categories.
        private ImageCategory ResolveStableCategoryAnchor(ImageCategory rawAnchor, ICollection<ImageCategory> movingCategories)
        {
            if (rawAnchor == null || !movingCategories.Contains(rawAnchor))
                return rawAnchor;

            int index = _categories.IndexOf(rawAnchor);
            if (index < 0)
                return null;

            index++;
            while (index < _categories.Count && movingCategories.Contains(_categories[index]))
                index++;

            return index < _categories.Count ? _categories[index] : null;
        }

        private void EndDragState()
        {
            _pendingSelectClick = false;
            _dragCandidateNode = null;
            _isDragging = false;
            _draggedItems = null;
            _draggedCategories = null;
            _categoryDropHasTarget = false;
            _categoryDropBeforeCategory = null;
            ClearDropTarget();

            _autoScrollTimer.Stop();
            _autoScrollDirection = 0;

            Cursor = Cursors.Default;
            Capture = false;
            Invalidate();
        }

        // Expands a category automatically if AutoExpandCategoryOnInsert is turned on and it's currently collapsed.
        private void MaybeAutoExpand(ImageCategory category)
        {
            if (AutoExpandCategoryOnInsert && category != null && !category.IsExpanded)
                category.IsExpanded = true;
        }

        public void MoveItemToCategory(ImageItem item, ImageCategory targetCategory, ImageItem insertBefore = null)
        {
            if (item == null || targetCategory == null)
                return;

            if (_parentMap.TryGetValue(item, out var currentOwner))
                currentOwner.Items.Remove(item);

            InsertItem(targetCategory, item, insertBefore);
            _parentMap[item] = targetCategory;
            MaybeAutoExpand(targetCategory);
        }

        public ImageItem CopyItemToCategory(ImageItem item, ImageCategory targetCategory, ImageItem insertBefore = null, string newId = null)
        {
            if (item == null || targetCategory == null)
                return null;

            var copy = new ImageItem
            {
                Id = newId ?? (GenerateNewIdOnCopy ? Guid.NewGuid().ToString() : item.Id),
                Title = item.Title,
                ThumbnailUrl = item.ThumbnailUrl,
                IsEnabled = item.IsEnabled,
                Tag = item.Tag
            };

            InsertItem(targetCategory, copy, insertBefore);
            _parentMap[copy] = targetCategory;
            MaybeAutoExpand(targetCategory);
            return copy;
        }

        private static void InsertItem(ImageCategory category, ImageItem item, ImageItem insertBefore)
        {
            int index = insertBefore != null ? category.Items.IndexOf(insertBefore) : -1;
            if (index < 0) category.Items.Add(item);
            else category.Items.Insert(index, item);
        }

        public ImageCategory CopyCategory(ImageCategory category, ImageCategory insertBefore = null)
        {
            if (category == null)
                return null;

            var copy = new ImageCategory
            {
                Name = category.Name,
                IsExpanded = category.IsExpanded,
                Tag = category.Tag,
                Items = new List<ImageItem>()
            };

            foreach (var item in category.Items)
            {
                var itemCopy = new ImageItem
                {
                    Id = GenerateNewIdOnCopy ? Guid.NewGuid().ToString() : item.Id,
                    Title = item.Title,
                    ThumbnailUrl = item.ThumbnailUrl,
                    IsEnabled = item.IsEnabled,
                    Tag = item.Tag
                };

                copy.Items.Add(itemCopy);
                _parentMap[itemCopy] = copy;
            }

            int index = insertBefore != null ? _categories.IndexOf(insertBefore) : -1;
            if (index < 0) _categories.Add(copy);
            else _categories.Insert(index, copy);

            return copy;
        }

        // Copy, cut, and paste use a clipboard in the app for their operations.
        public void CopySelection()
        {
            if (_selectionOrder.Count == 0)
                return;

            _clipboard.Clear();
            _clipboard.AddRange(_selectionOrder);
            _clipboardIsCut = false;
            Invalidate();
        }

        public void CutSelection()
        {
            if (_selectionOrder.Count == 0)
                return;

            _clipboard.Clear();
            _clipboard.AddRange(_selectionOrder);
            _clipboardIsCut = true;
            Invalidate();
        }

        public void Paste()
        {
            if (_clipboard.Count == 0)
                return;

            ImageCategory targetCategory = null;
            ImageItem insertBeforeItem = null;
            ImageCategory insertBeforeCategory = null;

            object firstSelected = _selectionOrder.FirstOrDefault();
            if (firstSelected is ImageCategory selectedCategory)
            {
                targetCategory = selectedCategory;

                if (PastePlacement == PastePosition.Left)
                {
                    insertBeforeCategory = selectedCategory;
                }
                else
                {
                    int categoryIndex = _categories.IndexOf(selectedCategory) + 1;
                    insertBeforeCategory = categoryIndex < _categories.Count ? _categories[categoryIndex] : null;
                }
            }
            else if (firstSelected is ImageItem selectedItem && _parentMap.TryGetValue(selectedItem, out var owner))
            {
                targetCategory = owner;

                if (PastePlacement == PastePosition.Left)
                {
                    insertBeforeItem = selectedItem;
                }
                else
                {
                    int itemIndex = owner.Items.IndexOf(selectedItem) + 1;
                    insertBeforeItem = itemIndex < owner.Items.Count ? owner.Items[itemIndex] : null;
                }
            }
            else if (_categories.Count > 0)
            {
                targetCategory = _categories[0];
            }

            var clipboardCategories = _clipboard.OfType<ImageCategory>().ToList();
            var clipboardItems = _clipboard.OfType<ImageItem>().ToList();
            var pastedNodes = new List<object>();

            if (_clipboardIsCut)
            {
                var stableAnchorCategory = ResolveStableCategoryAnchor(insertBeforeCategory, clipboardCategories);

                foreach (var category in clipboardCategories)
                {
                    _categories.Remove(category);
                    int index = stableAnchorCategory != null ? _categories.IndexOf(stableAnchorCategory) : -1;
                    if (index < 0) _categories.Add(category);
                    else _categories.Insert(index, category);
                    pastedNodes.Add(category);
                }

                if (targetCategory != null)
                {
                    var stableAnchorItem = ResolveStableAnchor(targetCategory, insertBeforeItem, clipboardItems);

                    foreach (var item in clipboardItems)
                    {
                        MoveItemToCategory(item, targetCategory, stableAnchorItem);
                        pastedNodes.Add(item);
                    }
                }

                // Cut is single-use.
                _clipboard.Clear();
                _clipboardIsCut = false;
            }
            else
            {
                foreach (var category in clipboardCategories)
                    pastedNodes.Add(CopyCategory(category, insertBeforeCategory));

                if (targetCategory != null)
                {
                    foreach (var item in clipboardItems)
                        pastedNodes.Add(CopyItemToCategory(item, targetCategory, insertBeforeItem));
                }
            }

            if (pastedNodes.Count == 0)
                return;

            ClearSelectionInternal();
            foreach (var node in pastedNodes)
                AddToSelection(node);

            RebuildLayout();
            ScrollNodesIntoView(pastedNodes);
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Selection

        private void SelectNode(object node)
        {
            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (shift && _shiftAnchor != null)
            {
                SelectRange(_shiftAnchor, node, additive: ctrl);
            }
            else if (ctrl)
            {
                ToggleSelection(node);
                _shiftAnchor = node;
            }
            else
            {
                ClearSelectionInternal();
                AddToSelection(node);
                _shiftAnchor = node;
            }

            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private IEnumerable<object> FlattenSelectableNodes()
        {
            foreach (var category in _categories)
            {
                yield return category;
                if (category.IsExpanded)
                    foreach (var item in category.Items)
                        yield return item;
            }
        }

        private void SelectRange(object from, object to, bool additive)
        {
            var flat = FlattenSelectableNodes().ToList();
            int i1 = flat.IndexOf(from);
            int i2 = flat.IndexOf(to);

            if (i1 < 0 || i2 < 0)
            {
                AddToSelection(to);
                return;
            }

            if (i1 > i2) { var tmp = i1; i1 = i2; i2 = tmp; }

            if (!additive)
                ClearSelectionInternal();

            for (int i = i1; i <= i2; i++)
                AddToSelection(flat[i]);
        }

        private void ToggleSelection(object node)
        {
            if (_selectionSet.Contains(node))
            {
                _selectionSet.Remove(node);
                _selectionOrder.Remove(node);
            }
            else
            {
                AddToSelection(node);
            }
        }

        private void AddToSelection(object node)
        {
            if (_selectionSet.Add(node))
                _selectionOrder.Add(node);
        }

        private void ClearSelectionInternal()
        {
            _selectionSet.Clear();
            _selectionOrder.Clear();
        }

        public void ClearSelection()
        {
            ClearSelectionInternal();
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        // Selects every category and (if expanded) every item.
        public void SelectAll()
        {
            ClearSelectionInternal();
            foreach (var node in FlattenSelectableNodes())
                AddToSelection(node);

            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Expand / collapse

        public void ToggleCategoryExpansion(ImageCategory category)
        {
            category.IsExpanded = !category.IsExpanded;
            RebuildLayout();
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ExpandAll()
        {
            foreach (var category in _categories) category.IsExpanded = true;
            RebuildLayout();
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CollapseAll()
        {
            foreach (var category in _categories) category.IsExpanded = false;
            RebuildLayout();
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Enabled state

        // Sets selected nodes to be "enabled" (highlighted a different color).
        public void SetSelectedEnabled(bool enabled)
        {
            bool changed = false;

            foreach (var node in _selectionOrder)
            {
                if (node is ImageItem item)
                {
                    changed |= SetItemEnabledCore(item, enabled);
                }
                else if (node is ImageCategory category)
                {
                    foreach (var childItem in category.Items)
                        changed |= SetItemEnabledCore(childItem, enabled);
                }
            }

            if (!changed)
                return;

            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        // Flips enabled state on current selection.
        public void ToggleSelectedEnabled()
        {
            bool changed = false;

            foreach (var node in _selectionOrder)
            {
                if (node is ImageItem item)
                {
                    item.IsEnabled = !item.IsEnabled;
                    changed = true;
                }
                else if (node is ImageCategory category && category.Items.Count > 0)
                {
                    changed |= ToggleCategoryEnabledCore(category);
                }
            }

            if (!changed)
                return;

            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetCategoryEnabled(ImageCategory category, bool enabled)
        {
            if (category == null)
                return;

            bool changed = false;
            foreach (var item in category.Items)
                changed |= SetItemEnabledCore(item, enabled);

            if (!changed)
                return;

            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ToggleCategoryEnabledState(ImageCategory category)
        {
            if (category == null || category.Items.Count == 0)
                return;

            if (!ToggleCategoryEnabledCore(category))
                return;

            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private static bool SetItemEnabledCore(ImageItem item, bool enabled)
        {
            if (item.IsEnabled == enabled)
                return false;

            item.IsEnabled = enabled;
            return true;
        }

        // Returns true if anything actually changed.
        private static bool ToggleCategoryEnabledCore(ImageCategory category)
        {
            bool allEnabled = category.Items.All(i => i.IsEnabled);
            bool newValue = !allEnabled;

            bool changed = false;
            foreach (var item in category.Items)
                changed |= SetItemEnabledCore(item, newValue);

            return changed;
        }

        #endregion

        #region Reordering (within the same parent)

        public void MoveSelectedUp() => MoveSelected(-1);

        public void MoveSelectedDown() => MoveSelected(1);

        private void MoveSelected(int direction)
        {
            var selectedCategories = new HashSet<ImageCategory>(SelectedCategories);
            if (selectedCategories.Count > 0)
                MoveWithinList(_categories, selectedCategories, direction);

            var itemGroups = SelectedItems
                .GroupBy(item => _parentMap.TryGetValue(item, out var owner) ? owner : null)
                .Where(group => group.Key != null);

            foreach (var group in itemGroups)
                MoveWithinList(group.Key.Items, new HashSet<ImageItem>(group), direction);

            RebuildLayout();
            ScrollNodesIntoView(_selectionOrder);
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void MoveWithinList<T>(List<T> list, HashSet<T> selected, int direction)
        {
            if (direction < 0)
            {
                for (int i = 1; i < list.Count; i++)
                {
                    if (selected.Contains(list[i]) && !selected.Contains(list[i - 1]))
                    {
                        var tmp = list[i - 1];
                        list[i - 1] = list[i];
                        list[i] = tmp;
                    }
                }
            }
            else
            {
                for (int i = list.Count - 2; i >= 0; i--)
                {
                    if (selected.Contains(list[i]) && !selected.Contains(list[i + 1]))
                    {
                        var tmp = list[i + 1];
                        list[i + 1] = list[i];
                        list[i] = tmp;
                    }
                }
            }
        }

        public void MoveSelectedToTop() => MoveSelectedToEdge(toTop: true); // Moves to top of parent list.

        public void MoveSelectedToBottom() => MoveSelectedToEdge(toTop: false); // Moves to bottom of parent list.

        private void MoveSelectedToEdge(bool toTop)
        {
            var selectedCategoriesInOrder = _selectionOrder.OfType<ImageCategory>().ToList();
            if (selectedCategoriesInOrder.Count > 0)
                MoveToEdge(_categories, selectedCategoriesInOrder, toTop);

            var itemGroups = _selectionOrder.OfType<ImageItem>()
                .GroupBy(item => _parentMap.TryGetValue(item, out var owner) ? owner : null)
                .Where(group => group.Key != null);

            foreach (var group in itemGroups)
                MoveToEdge(group.Key.Items, group.ToList(), toTop);

            RebuildLayout();
            ScrollNodesIntoView(_selectionOrder);
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void MoveToEdge<T>(List<T> list, List<T> selectedInOrder, bool toTop)
        {
            foreach (var node in selectedInOrder)
                list.Remove(node);

            if (toTop) list.InsertRange(0, selectedInOrder);
            else list.AddRange(selectedInOrder);
        }

        #endregion

        #region Add / remove

        public ImageCategory AddCategory(string name)
        {
            var category = new ImageCategory { Name = name, Items = new List<ImageItem>() };
            _categories.Add(category);
            RebuildLayout();
            ScrollNodeIntoView(category);
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
            return category;
        }

        public ImageItem AddItem(string title, string thumbnailUrl, string id = null) // Adds child node.
        {
            ImageCategory targetCategory = null;
            int insertIndex = -1;

            object firstSelected = _selectionOrder.FirstOrDefault();
            if (firstSelected is ImageCategory category)
            {
                targetCategory = category;
            }
            else if (firstSelected is ImageItem existingItem && _parentMap.TryGetValue(existingItem, out var owner))
            {
                targetCategory = owner;
                insertIndex = owner.Items.IndexOf(existingItem) + 1;
            }
            else if (_categories.Count > 0)
            {
                targetCategory = _categories[0];
            }

            if (targetCategory == null)
                return null;

            var newItem = new ImageItem
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Title = string.IsNullOrEmpty(title) ? "New Item" : title,
                ThumbnailUrl = thumbnailUrl
            };

            if (insertIndex < 0 || insertIndex > targetCategory.Items.Count)
                targetCategory.Items.Add(newItem);
            else
                targetCategory.Items.Insert(insertIndex, newItem);

            _parentMap[newItem] = targetCategory;
            MaybeAutoExpand(targetCategory);

            RebuildLayout();
            ScrollNodeIntoView(newItem);
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
            return newItem;
        }

        // Removes every currently selected category (and its children) and every currently selected item.
        public void RemoveSelectedNodes()
        {
            if (_selectionOrder.Count == 0)
                return;

            foreach (var category in SelectedCategories.ToList())
            {
                foreach (var item in category.Items)
                    _parentMap.Remove(item);

                _categories.Remove(category);
            }

            foreach (var item in SelectedItems.ToList())
            {
                if (_parentMap.TryGetValue(item, out var owner))
                    owner.Items.Remove(item);

                _parentMap.Remove(item);
            }

            ClearSelectionInternal();
            RebuildLayout();
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}