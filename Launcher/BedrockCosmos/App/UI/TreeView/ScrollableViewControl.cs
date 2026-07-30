using System;
using System.Drawing;
using System.Windows.Forms;

// Built off of Crown Scroll View from ReaLTaiizor to work with .NET 4.7.2
// https://github.com/Taiizor/ReaLTaiizor

namespace BedrockCosmos.App.UI
{
    public class ScrollableViewControl : Control
    {
        protected readonly CustomScrollBar _vScrollBar;
        protected readonly CustomScrollBar _hScrollBar;
        protected readonly Timer _dragTimer;

        private Size _contentSize;
        private bool _isDragging;
        private bool _disposed;

        // The size of the "virtual" content being drawn, in content coordinates.
        protected Size ContentSize
        {
            get { return _contentSize; }
            set
            {
                if (_contentSize == value)
                {
                    return;
                }

                _contentSize = value;

                UpdateScrollBars();
                Invalidate();
            }
        }

        // The currently visible region, expressed in content coordinates
        // (Top/Left are the current scroll offsets).
        protected Rectangle Viewport
        {
            get
            {
                int width = ClientRectangle.Width - (_vScrollBar.Visible ? _vScrollBar.Width : 0);
                int height = ClientRectangle.Height - (_hScrollBar.Visible ? _hScrollBar.Height : 0);

                int x = _hScrollBar.Visible ? _hScrollBar.Value : 0;
                int y = _vScrollBar.Visible ? _vScrollBar.Value : 0;

                return new Rectangle(x, y, Math.Max(0, width), Math.Max(0, height));
            }
        }

        // The current mouse position, translated into content coordinates
        protected Point OffsetMousePosition
        {
            get
            {
                Point p = PointToClient(MousePosition);

                p.X += _hScrollBar.Visible ? _hScrollBar.Value : 0;
                p.Y += _vScrollBar.Visible ? _vScrollBar.Value : 0;

                return p;
            }
        }

        // Maximum number of content pixels the view will auto-scroll per
        // drag-timer tick.
        protected int MaxDragChange { get; set; }

        protected bool IsDragging
        {
            get { return _isDragging; }
        }

        protected ScrollableViewControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            TabStop = true;

            _vScrollBar = new CustomScrollBar(ScrollBarOrientation.Vertical)
            {
                Dock = DockStyle.Right,
                Visible = false
            };
            _vScrollBar.ValueChanged += ScrollBar_ValueChanged;
            Controls.Add(_vScrollBar);

            _hScrollBar = new CustomScrollBar(ScrollBarOrientation.Horizontal)
            {
                Dock = DockStyle.Bottom,
                Visible = false
            };
            _hScrollBar.ValueChanged += ScrollBar_ValueChanged;
            Controls.Add(_hScrollBar);

            _dragTimer = new Timer
            {
                Interval = 20
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_dragTimer != null)
                {
                    _dragTimer.Stop();
                    _dragTimer.Dispose();
                }

                if (_vScrollBar != null)
                {
                    _vScrollBar.ValueChanged -= ScrollBar_ValueChanged;
                }

                if (_hScrollBar != null)
                {
                    _hScrollBar.ValueChanged -= ScrollBar_ValueChanged;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private void ScrollBar_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateScrollBars();
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            HandleMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.TranslateTransform(-Viewport.X, -Viewport.Y);
            PaintContent(g);
            g.TranslateTransform(Viewport.X, Viewport.Y);
        }

        // Recomputes scrollbar visibility and ranges based on ContentSize
        // versus the current client area.
        private void UpdateScrollBars()
        {
            bool needV = _contentSize.Height > ClientRectangle.Height;
            bool needH = _contentSize.Width > (ClientRectangle.Width - (needV ? _vScrollBar.Width : 0));

            // Re-check V now that H may consume some vertical space.
            needV = _contentSize.Height > (ClientRectangle.Height - (needH ? _hScrollBar.Height : 0));

            _vScrollBar.Visible = needV;
            _hScrollBar.Visible = needH;

            if (needV)
            {
                int viewHeight = Math.Max(1, ClientRectangle.Height - (needH ? _hScrollBar.Height : 0));

                _vScrollBar.Maximum = Math.Max(0, _contentSize.Height - 1);
                _vScrollBar.LargeChange = Math.Max(1, viewHeight);
                _vScrollBar.SmallChange = MaxDragChange > 0 ? MaxDragChange : 1;

                if (_vScrollBar.Value > _vScrollBar.Maximum)
                {
                    _vScrollBar.Value = _vScrollBar.Maximum;
                }
            }
            else
            {
                _vScrollBar.Value = 0;
            }

            if (needH)
            {
                int viewWidth = Math.Max(1, ClientRectangle.Width - (needV ? _vScrollBar.Width : 0));

                _hScrollBar.Maximum = Math.Max(0, _contentSize.Width - 1);
                _hScrollBar.LargeChange = Math.Max(1, viewWidth);
                _hScrollBar.SmallChange = MaxDragChange > 0 ? MaxDragChange : 1;

                if (_hScrollBar.Value > _hScrollBar.Maximum)
                {
                    _hScrollBar.Value = _hScrollBar.Maximum;
                }
            }
            else
            {
                _hScrollBar.Value = 0;
            }
        }

        // Scrolls in response to the mouse wheel. Vertical scrolling is used
        // by default; hold Shift to scroll horizontally instead.
        protected virtual void HandleMouseWheel(MouseEventArgs e)
        {
            bool wantHorizontal = ModifierKeys == Keys.Shift;

            CustomScrollBar bar = null;

            if (wantHorizontal && _hScrollBar.Visible)
            {
                bar = _hScrollBar;
            }
            else if (_vScrollBar.Visible)
            {
                bar = _vScrollBar;
            }
            else if (_hScrollBar.Visible)
            {
                bar = _hScrollBar;
            }

            if (bar == null)
            {
                return;
            }

            int wheelLines = SystemInformation.MouseWheelScrollLines;
            if (wheelLines < 1)
            {
                wheelLines = 3;
            }

            int step = bar.SmallChange > 0 ? bar.SmallChange : 1;
            int delta = -(e.Delta / 120) * wheelLines * step;

            bar.Value = bar.Value + delta;
        }

        protected void VScrollTo(int position)
        {
            if (!_vScrollBar.Visible)
            {
                return;
            }

            if (position < _vScrollBar.Minimum)
            {
                position = _vScrollBar.Minimum;
            }

            int maxValue = _vScrollBar.Maximum - _vScrollBar.LargeChange + 1;
            if (maxValue < _vScrollBar.Minimum)
            {
                maxValue = _vScrollBar.Minimum;
            }

            if (position > maxValue)
            {
                position = maxValue;
            }

            _vScrollBar.Value = position;
        }

        protected virtual void StartDrag()
        {
            _isDragging = true;
            _dragTimer.Start();
        }

        protected virtual void StopDrag()
        {
            _isDragging = false;
            _dragTimer.Stop();
        }

        // Override in derived classes to draw content in content-space coords.
        protected virtual void PaintContent(Graphics g)
        {
        }
    }
}