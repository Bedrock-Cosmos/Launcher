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

        /// <summary>
        /// The specific visual row the cursor was hovering when the drop
        /// target was last resolved (null if the target came from a header
        /// hover, or there's no target). A boundary insertion point - e.g.
        /// "before the first item of row 2" and "after the last item of row
        /// 1" are the same list position - is genuinely ambiguous in terms of
        /// a global index alone, so the caret is rendered using this plus
        /// <see cref="_dropTargetLocalSlot"/> instead: always in whichever row
        /// the cursor is actually over, never a neighboring one.
        /// </summary>
        private ItemsVisualRow _dropTargetVisualRow;

        /// <summary>Slot within <see cref="_dropTargetVisualRow"/> (0..row.Items.Count) where the caret should render.</summary>
        private int _dropTargetLocalSlot;

        /// <summary>
        /// Non-null while dragging one or more category headers to reorder
        /// them - mutually exclusive with <see cref="_draggedItems"/> (a given
        /// drag is always either items or categories, never both).
        /// </summary>
        private List<ImageCategory> _draggedCategories;

        /// <summary>True once a valid category reorder target has been resolved (distinct from "append at the end", which is also represented by a null <see cref="_categoryDropBeforeCategory"/>).</summary>
        private bool _categoryDropHasTarget;

        /// <summary>The category the dragged group should land before, or null to append at the very end of the top-level list.</summary>
        private ImageCategory _categoryDropBeforeCategory;

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

        /// <summary>
        /// True while running inside a WinForms designer host. Deliberately
        /// computed via <see cref="LicenseManager.UsageMode"/> rather than the
        /// inherited <see cref="Control.DesignMode"/> property - DesignMode is
        /// unreliable (always false) when read from within a constructor,
        /// since the control's Site isn't assigned until construction
        /// finishes. Used to skip network/background-thread activity while
        /// designing: the designer repeatedly creates and tears down control
        /// instances (every rebuild, undo/redo, property change, etc.), and a
        /// thumbnail load completing against an already-torn-down instance is
        /// exactly the kind of race that used to be able to crash the design
        /// surface - see the fix in ImageCache.RaiseImageLoaded and
        /// OnImageLoaded below for the general-purpose (not just design-time)
        /// version of that fix.
        /// </summary>
        private readonly bool _isDesignMode;

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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from the static/shared cache event - forgetting this
                // would keep every control instance alive for the life of the process.
                // Safe to call even if we never subscribed (design-time instances).
                ImageCache.ImageLoaded -= OnImageLoaded;

                _autoScrollTimer.Stop();
                _autoScrollTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnImageLoaded(string id)
        {
            // Belt-and-suspenders alongside the fix in ImageCache.RaiseImageLoaded:
            // this can run on a background thread at any time, including after
            // the control has been disposed or its handle destroyed (e.g. the
            // form closing while a thumbnail is still loading, or a WinForms
            // designer reload). Every exit here is deliberately silent rather
            // than throwing - a missed repaint is harmless, an unhandled
            // exception on a ThreadPool thread is not.
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

        /// <summary>
        /// Replaces all data, clears selection, and rebuilds the layout. If
        /// <paramref name="categories"/> is already a <see cref="List{T}"/> (for
        /// example <c>ImageDocument.Categories</c>), that exact list is used as
        /// the control's backing store rather than copied, so edits made here
        /// stay live-visible to whoever handed it to you.
        /// </summary>
        public void LoadData(IEnumerable<ImageCategory> categories)
        {
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

        /// <summary>Binary search for the row (if any) whose vertical span contains content-space y.</summary>
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

        /// <summary>Binary search for the first row that is even partially visible given the current scroll offset.</summary>
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

        /// <summary>Right margin, in pixels, between the enabled/disabled indicator dot and the scrollbar track.</summary>
        private const int HeaderIndicatorRightMargin = 10;

        /// <summary>Diameter, in pixels, of the enabled/disabled indicator dot.</summary>
        private const int HeaderIndicatorSize = 10;

        /// <summary>Left edge, in pixels, where the header's text label starts (just after the arrow).</summary>
        private const int HeaderTextStartX = 28;

        /// <summary>Left edge, in pixels, of the header's enabled/disabled indicator dot - right-aligned against the scrollbar rather than tied to the arrow/text, so there's no dead space between them.</summary>
        private int GetHeaderIndicatorX()
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
                if (location.X < cellLeft || location.X > cellLeft + ItemSize) return result; // gap between cells

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

            if (beingDragged)
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
                // All children enabled - solid dot.
                using (var brush = new SolidBrush(EnabledHighlightColor))
                    g.FillEllipse(brush, indicatorRect);
            }
            else if (enabledCount > 0)
            {
                // Some, but not all, children enabled - hollow ring.
                using (var pen = new Pen(EnabledHighlightColor, 2f))
                    g.DrawEllipse(pen, indicatorRect);
            }
            // No children enabled: no indicator drawn, but the hit-zone at
            // indicatorX still works so the user can click there to enable
            // everything from a fully-off state.

            string label = $"{category.Name} ({category.Items.Count})";
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

            // Reference equality on the exact row that was hovered - not just
            // a matching list index - so a boundary position (which is
            // ambiguous in terms of a plain index: "end of this row" and
            // "start of the next" are the same list position) always renders
            // in whichever row the cursor is actually over, never a neighbor.
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

        /// <summary>Full-width horizontal line marking where a dragged category will land.</summary>
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
                // Selection always wins over the plain "enabled" border, but if
                // the selected item is also enabled the selection background
                // itself becomes the enabled color instead of plain gray, so
                // you can still tell at a glance which selected items are enabled.
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

            float previousAlpha = 1f;
            if (beingDragged)
            {
                // Faint "ghost" look for items currently being dragged elsewhere.
                using (var fadeBrush = new SolidBrush(Color.FromArgb(140, IdleBackColor)))
                    g.FillPath(fadeBrush, RoundedRect(cellRect, CornerRadius));
            }

            const int inset = 6;
            var imageRect = new Rectangle(x + inset, y + inset, ItemSize - inset * 2, ItemSize - inset * 2);

            if (_isDesignMode)
            {
                // Never touch the network or the disk cache from inside a
                // WinForms designer host - a plain placeholder is enough to
                // preview the layout.
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

        /// <summary>A generic "picture" glyph shown in place of a real thumbnail while running inside the designer.</summary>
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
            Focus();

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

                if (hit.OnHeaderIndicator)
                {
                    ToggleCategoryEnabledState(hitCategory);
                    return;
                }
            }

            if (hit.Node == null)
            {
                ClearSelection();
                return;
            }

            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

            // Both items and category headers (aside from their arrow/indicator
            // hot-zones, already handled above) can be dragged.
            bool isDraggableCandidate = hit.Node is ImageItem || hit.Node is ImageCategory;

            if (isDraggableCandidate && _selectionSet.Contains(hit.Node) && !shift)
            {
                // Might be the start of a drag of the whole current selection -
                // defer this click's effect on selection until MouseUp rather
                // than applying it immediately. This matters for Ctrl too, not
                // just a plain click: Ctrl+clicking an already-selected node
                // is normally a "toggle it off" gesture, but if the user is
                // about to Ctrl-drag the whole selection to copy it, applying
                // that toggle immediately would rip the node right back out of
                // the selection before the drag even starts. If this turns out
                // to be a plain click with no drag, SelectNode (called from
                // MouseUp) re-reads the modifier keys at that point and does
                // the right thing either way - toggles off if Ctrl is still
                // held, or collapses the selection down to just this node if not.
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

            // The arrow and indicator each already toggle on every single
            // click (both clicks of a double-click fire their own MouseDown
            // normally). If this handler also toggled expansion there, a
            // quick double-click on the arrow ended up performing three
            // toggles instead of two - two from the clicks themselves plus
            // one extra from here - leaving it in the wrong state. Only a
            // double-click on the "plain" part of the header (not the arrow
            // or indicator) should expand/collapse.
            if (hit.Node is ImageCategory category && !hit.OnHeaderArrow && !hit.OnHeaderIndicator)
                ToggleCategoryExpansion(category);
        }

        private void ScrollByPixels(int delta)
        {
            _scrollOffset += delta;
            ClampScrollOffset();
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
            else if (e.KeyCode == Keys.Escape && _isDragging)
            {
                EndDragState();
                Invalidate();
            }
        }

        #endregion

        #region Drag and drop between categories

        /// <summary>
        /// Called on every mouse-move while dragging items. Starts, adjusts,
        /// or stops the auto-scroll timer depending on how close the cursor
        /// is to the top/bottom edge of the control.
        /// </summary>
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

            // The content just moved under a (possibly) stationary cursor -
            // recompute what's actually under it now and redraw the caret
            // there, otherwise it would lag a whole mouse-move behind.
            UpdateDropTarget(_lastDragMouseLocation);
            Invalidate();
        }

        /// <summary>Faster the closer the cursor is to the exact top/bottom edge, slower near the inner boundary of the trigger zone.</summary>
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

            // Without capture, mouse events stop arriving the instant the
            // cursor leaves the control's bounds - which is exactly what
            // happens when dragging toward the top/bottom edge to trigger
            // auto-scroll, or just dragging slightly too far in general.
            Capture = true;
        }

        private void UpdateActiveDropTarget(Point location)
        {
            if (_draggedCategories != null)
                UpdateCategoryDropTarget(location);
            else
                UpdateDropTarget(location);
        }

        /// <summary>
        /// Resolves where a dragged group of categories would land. Hovering
        /// anywhere within a category's whole visual block - its header plus
        /// every one of its currently-visible item rows, not just the thin
        /// header strip - counts: the top half of that whole block targets
        /// "insert before this category", the bottom half targets "insert
        /// after" (i.e. before whichever category currently follows it, or
        /// append if it's the last one).
        /// </summary>
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
                // Below everything (or above it, if the content doesn't even
                // fill the control) - only "past the very end" is a
                // meaningful target here, meaning append.
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

        /// <summary>Finds the top/bottom content-space bounds spanning every visual row (header and item rows alike) that belongs to the given category.</summary>
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
                _dropTargetBeforeItem = null; // drop = append to this category
                _dropTargetGlobalIndex = header.Category.Items.Count;
                _dropTargetVisualRow = null; // no items row involved - the header's own highlight is the visual cue
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

        /// <summary>
        /// Resolves an x coordinate within an items row to an insertion anchor:
        /// hovering the left half of a cell (or the gap before it) targets
        /// "insert before that item"; hovering the right half of a cell (or the
        /// gap after it, including empty trailing space in a short last row)
        /// targets "insert after that item" - which becomes "insert before the
        /// next item", or null (append) past the category's last item.
        /// </summary>
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
                bool rightHalf = withinCell >= ItemSize / 2; // the trailing padding gap counts as the right half of its column
                rawSlot = column + (rightHalf ? 1 : 0);
            }

            localSlot = Math.Min(rawSlot, row.Items.Count);

            var category = row.Category;
            globalIndex = row.StartIndex + localSlot;

            // Deliberately returns the raw item at this position even if it's
            // one of the items currently being dragged, so the caret can
            // render exactly where the cursor is - including right next to,
            // or between, items in the drag itself. See ResolveStableAnchor
            // for how this gets reconciled into a fixed reference point once
            // the drop actually happens.
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
                if (copy)
                {
                    // Copies are brand-new ImageItem instances, never part of
                    // _draggedItems, so the raw hover anchor is always safe to
                    // use directly here even if it happens to be one of the
                    // originals - nothing about the originals is being removed.
                    var copies = _draggedItems
                        .Select(item => CopyItemToCategory(item, _dropTargetCategory, _dropTargetBeforeItem))
                        .ToList();

                    ClearSelectionInternal();
                    foreach (var newItem in copies)
                        AddToSelection(newItem);
                }
                else
                {
                    // For an actual move, the hover anchor might itself be one
                    // of the items being moved (that's exactly what lets the
                    // caret track the cursor between/next to dragged items -
                    // see ResolveDropAnchor). Resolve it to the nearest stable
                    // (not-being-moved) item once, up front, so every dragged
                    // item lands relative to the same fixed reference point
                    // regardless of processing order.
                    var stableAnchor = ResolveStableAnchor(_dropTargetCategory, _dropTargetBeforeItem);

                    foreach (var item in _draggedItems)
                        MoveItemToCategory(item, _dropTargetCategory, stableAnchor);
                }

                RebuildLayout();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                DataChanged?.Invoke(this, EventArgs.Empty);
            }

            EndDragState();
        }

        /// <summary>Reorders (or, with Ctrl held, duplicates) the dragged categories to just before the resolved drop target.</summary>
        private void CompleteCategoryDrag(bool copy)
        {
            if (!_categoryDropHasTarget || _draggedCategories == null || _draggedCategories.Count == 0)
                return;

            if (copy)
            {
                // Copies are brand-new ImageCategory instances, never part of
                // _draggedCategories, so the raw hover anchor is always safe
                // to use directly - nothing about the originals is being
                // removed or reordered.
                var copies = _draggedCategories
                    .Select(category => CopyCategory(category, _categoryDropBeforeCategory))
                    .ToList();

                ClearSelectionInternal();
                foreach (var newCategory in copies)
                    AddToSelection(newCategory);
            }
            else
            {
                // Same idea as ResolveStableAnchor for items: the raw hover
                // target might itself be one of the categories being moved
                // (that's what lets the drop line track the cursor
                // between/next to dragged categories), so resolve it to a
                // fixed, non-moving reference point once before actually
                // reordering the list.
                var stableAnchor = _categoryDropBeforeCategory;
                if (stableAnchor != null && _draggedCategories.Contains(stableAnchor))
                {
                    int index = _categories.IndexOf(stableAnchor) + 1;
                    while (index < _categories.Count && _draggedCategories.Contains(_categories[index]))
                        index++;

                    stableAnchor = index < _categories.Count ? _categories[index] : null;
                }

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
            }

            RebuildLayout();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Walks forward from <paramref name="rawAnchor"/> to the nearest item
        /// that isn't part of the current drag (or off the end, meaning
        /// append), so a move can use a fixed reference point that won't
        /// itself be relocated partway through the operation.
        /// </summary>
        private ImageItem ResolveStableAnchor(ImageCategory category, ImageItem rawAnchor)
        {
            if (rawAnchor == null || !_draggedItems.Contains(rawAnchor))
                return rawAnchor;

            int index = category.Items.IndexOf(rawAnchor);
            if (index < 0)
                return null;

            index++;
            while (index < category.Items.Count && _draggedItems.Contains(category.Items[index]))
                index++;

            return index < category.Items.Count ? category.Items[index] : null;
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

        /// <summary>Moves an item into a (possibly different) category, optionally inserting it before an existing sibling instead of at the end.</summary>
        public void MoveItemToCategory(ImageItem item, ImageCategory targetCategory, ImageItem insertBefore = null)
        {
            if (item == null || targetCategory == null)
                return;

            if (_parentMap.TryGetValue(item, out var currentOwner))
                currentOwner.Items.Remove(item);

            InsertItem(targetCategory, item, insertBefore);
            _parentMap[item] = targetCategory;
        }

        /// <summary>Duplicates an item into a (possibly different) category, leaving the original in place. Returns the new copy.</summary>
        public ImageItem CopyItemToCategory(ImageItem item, ImageCategory targetCategory, ImageItem insertBefore = null, string newId = null)
        {
            if (item == null || targetCategory == null)
                return null;

            var copy = new ImageItem
            {
                Id = newId ?? Guid.NewGuid().ToString(),
                Title = item.Title,
                ThumbnailUrl = item.ThumbnailUrl,
                IsEnabled = item.IsEnabled,
                Tag = item.Tag
            };

            InsertItem(targetCategory, copy, insertBefore);
            _parentMap[copy] = targetCategory;
            return copy;
        }

        private static void InsertItem(ImageCategory category, ImageItem item, ImageItem insertBefore)
        {
            int index = insertBefore != null ? category.Items.IndexOf(insertBefore) : -1;
            if (index < 0) category.Items.Add(item);
            else category.Items.Insert(index, item);
        }

        /// <summary>
        /// Duplicates an entire category - including a fresh copy of every
        /// child item, each with a newly generated id so it never collides
        /// with the original - and inserts it among the top-level categories,
        /// optionally right before an existing one instead of at the end.
        /// </summary>
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
                    Id = Guid.NewGuid().ToString(),
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

        /// <summary>Clears the current selection.</summary>
        public void ClearSelection()
        {
            ClearSelectionInternal();
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Selects every category and (if expanded) every item.</summary>
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

        /// <summary>
        /// Sets the "enabled" highlight (see <see cref="ImageItem.IsEnabled"/>)
        /// on every currently selected item. If a category is selected, every
        /// item under it is set as well (whether or not those items are
        /// individually selected too). Does nothing (and doesn't raise
        /// <see cref="DataChanged"/>) if nothing is selected or the value
        /// wouldn't actually change anything.
        /// </summary>
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

        /// <summary>
        /// Flips the "enabled" highlight on the current selection. Individual
        /// items toggle their own current state. A selected category toggles
        /// as a whole - the way a tri-state checkbox usually does - going to
        /// "all enabled" unless every item under it is already enabled, in
        /// which case it goes to "all disabled".
        /// </summary>
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

        /// <summary>Sets every item under <paramref name="category"/> to the given enabled state, independent of the current selection.</summary>
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

        /// <summary>
        /// Toggles a category as a whole (see <see cref="ToggleSelectedEnabled"/>
        /// for the exact rule), independent of the current selection. This is
        /// what clicking a header's enabled-indicator dot calls.
        /// </summary>
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

        /// <summary>Returns true if anything actually changed.</summary>
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

        /// <summary>Moves every selected node up one position within its own parent list.</summary>
        public void MoveSelectedUp() => MoveSelected(-1);

        /// <summary>Moves every selected node down one position within its own parent list.</summary>
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

        /// <summary>Moves every selected node to the very top of its own parent list.</summary>
        public void MoveSelectedToTop() => MoveSelectedToEdge(toTop: true);

        /// <summary>Moves every selected node to the very bottom of its own parent list.</summary>
        public void MoveSelectedToBottom() => MoveSelectedToEdge(toTop: false);

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

        /// <summary>Adds a brand-new, empty category (e.g. for a new skin/cape grouping) and returns it.</summary>
        public ImageCategory AddCategory(string name)
        {
            var category = new ImageCategory { Name = name, Items = new List<ImageItem>() };
            _categories.Add(category);
            RebuildLayout();
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
            return category;
        }

        /// <summary>
        /// Adds a new child node. It is placed directly under whichever node was
        /// selected first: if that's a category, the new node is appended to it;
        /// if it's an existing item, the new node is inserted right after it in
        /// the same category. Falls back to the first category if nothing is
        /// selected, and returns null if there are no categories at all.
        /// </summary>
        public ImageItem AddItem(string title, string thumbnailUrl, string id = null)
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
            targetCategory.IsExpanded = true;

            RebuildLayout();
            Invalidate();
            DataChanged?.Invoke(this, EventArgs.Empty);
            return newItem;
        }

        /// <summary>Removes every currently selected category (and its children) and every currently selected item.</summary>
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