using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BedrockCosmos.App.UI
{
    internal sealed class GradientComboDropDownList : Control
    {
        private const int MinThumbHeight = 24;

        private readonly GradientComboBox _owner;

        private int _scrollOffset;
        private bool _isDraggingThumb;
        private int _dragStartMouseY;
        private int _dragStartScrollOffset;
        private bool _thumbHot;
        private int _highlightedIndex = -1;

        public GradientComboDropDownList(GradientComboBox owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            TabStop = true;
            Cursor = Cursors.Hand;
        }

        private int ItemCount => _owner.Items.Count;

        private int ItemHeight => Math.Max(1, _owner.ItemHeight);

        private int ScrollBarWidth => _owner.ScrollBarWidth;

        private int ContentHeight => ItemCount * ItemHeight;

        // Resets the list for display, highlights current selection and scrolls into view.
        public void PrepareForShow(int selectedIndex)
        {
            _highlightedIndex = selectedIndex;
            _scrollOffset = 0;
            ClampScrollOffset();
            if (selectedIndex > 0)
                EnsureVisible(selectedIndex);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(_owner.ColorC);

            int rowWidth = ClientSize.Width - ScrollBarWidth;
            int itemHeight = ItemHeight;
            int firstIndex = _scrollOffset / itemHeight;
            int y = (firstIndex * itemHeight) - _scrollOffset;

            for (int i = firstIndex; i < ItemCount && y < ClientSize.Height; i++, y += itemHeight)
            {
                Rectangle rowBounds = new Rectangle(0, y, rowWidth, itemHeight);
                _owner.DrawDropDownItem(g, i, rowBounds, i == _highlightedIndex);
            }

            DrawScrollBar(g);
        }

        #region Custom scrollbar (same approach as ImageTreeView)

        private Rectangle GetTrackRect() => new Rectangle(ClientSize.Width - ScrollBarWidth, 0, ScrollBarWidth, ClientSize.Height);

        private Rectangle GetThumbRect()
        {
            Rectangle track = GetTrackRect();
            int contentHeight = ContentHeight;
            if (contentHeight <= ClientSize.Height)
                return Rectangle.Empty;

            double visibleRatio = (double)ClientSize.Height / contentHeight;
            int thumbHeight = Math.Max(MinThumbHeight, (int)(track.Height * visibleRatio));

            int scrollRange = Math.Max(1, contentHeight - ClientSize.Height);
            double scrollRatio = (double)_scrollOffset / scrollRange;
            int thumbTop = (int)(scrollRatio * (track.Height - thumbHeight));

            return new Rectangle(track.X + 2, thumbTop, track.Width - 4, thumbHeight);
        }

        private void DrawScrollBar(Graphics g)
        {
            Rectangle track = GetTrackRect();
            using (var trackBrush = new SolidBrush(_owner.ScrollTrackColor))
                g.FillRectangle(trackBrush, track);

            Rectangle thumb = GetThumbRect();
            if (thumb.IsEmpty)
                return;

            using (var thumbBrush = new SolidBrush(_thumbHot || _isDraggingThumb ? _owner.ScrollThumbHoverColor : _owner.ScrollThumbColor))
            using (var path = RoundedRect(thumb, 4))
                g.FillPath(thumbBrush, path);
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

        #endregion

        #region Mouse input

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
                return;

            Rectangle thumb = GetThumbRect();
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

            int index = IndexFromY(e.Y);
            if (index >= 0 && index < ItemCount)
            {
                _highlightedIndex = index;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDraggingThumb)
            {
                Rectangle track = GetTrackRect();
                Rectangle thumb = GetThumbRect();
                int trackRange = Math.Max(1, track.Height - thumb.Height);
                int scrollRange = Math.Max(1, ContentHeight - ClientSize.Height);

                int deltaPixels = e.Y - _dragStartMouseY;
                _scrollOffset = _dragStartScrollOffset + (int)(deltaPixels * ((double)scrollRange / trackRange));
                ClampScrollOffset();
                Invalidate();
                return;
            }

            bool wasHot = _thumbHot;
            Rectangle thumbRect = GetThumbRect();
            _thumbHot = !thumbRect.IsEmpty && thumbRect.Contains(e.Location);

            bool changed = _thumbHot != wasHot;

            if (e.X < ClientSize.Width - ScrollBarWidth)
            {
                int hoverIndex = IndexFromY(e.Y);
                if (hoverIndex >= 0 && hoverIndex < ItemCount && hoverIndex != _highlightedIndex)
                {
                    _highlightedIndex = hoverIndex;
                    changed = true;
                }
            }

            if (changed)
                Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDraggingThumb = false;

            if (e.Button != MouseButtons.Left)
                return;

            if (e.X >= ClientSize.Width - ScrollBarWidth)
                return;

            int index = IndexFromY(e.Y);
            if (index >= 0 && index < ItemCount)
                CommitSelection(index);
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

        private void ScrollByPixels(int delta)
        {
            _scrollOffset += delta;
            ClampScrollOffset();
            Invalidate();
        }

        private void ClampScrollOffset()
        {
            int maxScroll = Math.Max(0, ContentHeight - ClientSize.Height);
            if (_scrollOffset > maxScroll) _scrollOffset = maxScroll;
            if (_scrollOffset < 0) _scrollOffset = 0;
        }

        private int IndexFromY(int y) => (y + _scrollOffset) / ItemHeight;

        #endregion

        #region Keyboard input

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Home:
                case Keys.End:
                case Keys.Enter:
                case Keys.Escape:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (ItemCount == 0)
                return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    SetHighlighted(Math.Max(0, (_highlightedIndex < 0 ? 0 : _highlightedIndex) - 1));
                    break;
                case Keys.Down:
                    SetHighlighted(Math.Min(ItemCount - 1, _highlightedIndex + 1));
                    break;
                case Keys.Home:
                    SetHighlighted(0);
                    break;
                case Keys.End:
                    SetHighlighted(ItemCount - 1);
                    break;
                case Keys.PageUp:
                    SetHighlighted(Math.Max(0, _highlightedIndex - VisibleItemCount()));
                    break;
                case Keys.PageDown:
                    SetHighlighted(Math.Min(ItemCount - 1, _highlightedIndex + VisibleItemCount()));
                    break;
                case Keys.Enter:
                    CommitSelection(_highlightedIndex);
                    break;
                case Keys.Escape:
                    _owner.CloseCustomDropDown(commit: false);
                    break;
            }
        }

        private int VisibleItemCount() => Math.Max(1, ClientSize.Height / ItemHeight);

        private void SetHighlighted(int index)
        {
            _highlightedIndex = index;
            EnsureVisible(index);
            Invalidate();
        }

        private void EnsureVisible(int index)
        {
            int itemHeight = ItemHeight;
            int top = index * itemHeight;
            int bottom = top + itemHeight;

            if (top < _scrollOffset)
                _scrollOffset = top;
            else if (bottom > _scrollOffset + ClientSize.Height)
                _scrollOffset = bottom - ClientSize.Height;

            ClampScrollOffset();
        }

        #endregion

        private void CommitSelection(int index)
        {
            _owner.CloseCustomDropDown(commit: true, selectedIndex: index);
        }
    }
}