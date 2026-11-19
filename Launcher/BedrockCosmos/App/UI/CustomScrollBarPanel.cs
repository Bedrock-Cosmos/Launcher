using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BedrockCosmos.App.UI
{
    public class CustomScrollBarPanel : Panel
    {
        private const int ScrollBarWidth = 12;
        private const int MinThumbHeight = 24;
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private Color _trackColor = Color.FromArgb(90, 30, 30, 30); // Alpha < 255 lets background image show through.
        private Color _thumbColor = Color.FromArgb(75, 75, 75);
        private Color _thumbHoverColor = Color.FromArgb(110, 110, 110);

        private int _scrollOffset;
        private int _contentHeight; // 0 = auto-detect from children.
        private bool _isDraggingThumb;
        private bool _thumbHot;
        private int _dragStartMouseY;
        private int _dragStartScrollOffset;

        public CustomScrollBarPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();
        }

        #region Properties

        [Category("Appearance")]
        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color ThumbHoverColor
        {
            get => _thumbHoverColor;
            set { _thumbHoverColor = value; Invalidate(); }
        }

        [Category("Behavior"), DefaultValue(48)]
        public int WheelStep { get; set; } = 48; // Pixels per wheel notch.

        // Total scrollable height. 0 = computed from children's bottom edges.
        [Category("Layout"), DefaultValue(0)]
        public int ScrollContentHeight
        {
            get => _contentHeight;
            set { _contentHeight = Math.Max(0, value); ClampAndApply(); Invalidate(); }
        }

        private bool IsDesignTime =>
            DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private bool _runtimeModeApplied;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (IsDesignTime || _runtimeModeApplied)
                return;

            _runtimeModeApplied = true;

            // Reuse AutoScrollMinSize set in designer as the content height.
            if (_contentHeight == 0 && AutoScrollMinSize.Height > 0)
                _contentHeight = AutoScrollMinSize.Height;

            AutoScrollMinSize = Size.Empty;
            AutoScroll = false;
            Invalidate(true);
        }

        [Browsable(false)]
        public int ScrollOffset => _scrollOffset;

        #endregion

        #region Scroll math

        private int GetContentHeight()
        {
            if (_contentHeight > 0)
                return _contentHeight;

            int bottom = 0;
            foreach (Control c in Controls)
            {
                if (!c.Visible) continue;
                bottom = Math.Max(bottom, c.Bottom + _scrollOffset); // Undo current shift to get unscrolled bottom.
            }
            return bottom + Padding.Bottom;
        }

        private int MaxOffset => Math.Max(0, GetContentHeight() - ClientSize.Height);

        private Rectangle GetTrackRect() =>
            new Rectangle(ClientSize.Width - ScrollBarWidth, 0, ScrollBarWidth, ClientSize.Height);

        private Rectangle GetThumbRect()
        {
            if (IsDesignTime)
                return Rectangle.Empty; // Native scrollbars are shown in the designer instead.

            int content = GetContentHeight();
            if (content <= ClientSize.Height || ClientSize.Height <= 0)
                return Rectangle.Empty;

            var track = GetTrackRect();
            double visibleRatio = (double)ClientSize.Height / content;
            int thumbHeight = Math.Max(MinThumbHeight, (int)(track.Height * visibleRatio));

            int scrollRange = Math.Max(1, content - ClientSize.Height);
            double scrollRatio = (double)_scrollOffset / scrollRange;
            int thumbTop = (int)(scrollRatio * (track.Height - thumbHeight));

            return new Rectangle(track.X + 2, thumbTop, track.Width - 4, thumbHeight);
        }

        public void ScrollToTop() => SetScrollOffset(0);

        public void ScrollToBottom() => SetScrollOffset(MaxOffset);

        public void SetScrollOffset(int value)
        {
            if (IsDesignTime)
                return;

            value = Math.Max(0, Math.Min(value, MaxOffset));
            int delta = value - _scrollOffset;
            if (delta == 0)
                return;

            bool freeze = IsHandleCreated;
            if (freeze)
                SendMessage(Handle, WM_SETREDRAW, 0, 0); // One repaint at the end instead of one per child.

            SuspendLayout();
            foreach (Control c in Controls)
                c.Top -= delta;
            _scrollOffset = value;
            ResumeLayout(false);

            if (freeze)
                SendMessage(Handle, WM_SETREDRAW, 1, 0);

            Invalidate(true); // Full repaint so background can't smear.
            Update();
        }

        private void ClampAndApply()
        {
            if (_scrollOffset > MaxOffset)
                SetScrollOffset(MaxOffset);
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var thumb = GetThumbRect();
            if (thumb.IsEmpty)
                return;

            var g = e.Graphics;
            using (var trackBrush = new SolidBrush(_trackColor))
                g.FillRectangle(trackBrush, GetTrackRect());

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var thumbBrush = new SolidBrush(_thumbHot || _isDraggingThumb ? _thumbHoverColor : _thumbColor))
            using (var path = RoundedRect(thumb, 4))
                g.FillPath(thumbBrush, path);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region Input

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
                return;

            var thumb = GetThumbRect();
            if (thumb.IsEmpty)
                return;

            if (thumb.Contains(e.Location))
            {
                _isDraggingThumb = true;
                _dragStartMouseY = e.Y;
                _dragStartScrollOffset = _scrollOffset;
                Invalidate(GetTrackRect());
            }
            else if (GetTrackRect().Contains(e.Location))
            {
                SetScrollOffset(_scrollOffset + (e.Y < thumb.Top ? -ClientSize.Height : ClientSize.Height));
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
                int scrollRange = Math.Max(1, GetContentHeight() - ClientSize.Height);

                int deltaPixels = e.Y - _dragStartMouseY;
                SetScrollOffset(_dragStartScrollOffset + (int)(deltaPixels * ((double)scrollRange / trackRange)));
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
            if (_isDraggingThumb)
            {
                _isDraggingThumb = false;
                Invalidate(GetTrackRect());
            }
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

            if (e is HandledMouseEventArgs handled)
                handled.Handled = true;

            SetScrollOffset(_scrollOffset - (e.Delta / 120) * WheelStep);
        }

        #endregion

        #region Children / resize

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (_scrollOffset != 0)
                e.Control.Top -= _scrollOffset; // Keep new children consistent with the current scroll position.

            // Controls with focus receive scroll wheel (forwarded).
            e.Control.MouseWheel += Child_MouseWheel;
            Invalidate(GetTrackRect());
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            e.Control.MouseWheel -= Child_MouseWheel;
            ClampAndApply();
            Invalidate(GetTrackRect());
        }

        private void Child_MouseWheel(object sender, MouseEventArgs e)
        {
            SetScrollOffset(_scrollOffset - (e.Delta / 120) * WheelStep);
            if (e is HandledMouseEventArgs handled)
                handled.Handled = true;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            ClampAndApply();
        }

        #endregion
    }
}