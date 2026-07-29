using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    public enum ScrollBarOrientation
    {
        Vertical,
        Horizontal
    }

    public static class ScrollBarColors
    {
        public static Color Track = Color.FromArgb(35, 35, 35);
        public static Color Thumb = Color.FromArgb(90, 90, 90);
        public static Color ThumbHover = Color.FromArgb(125, 125, 125);
        public static Color ThumbPressed = Color.FromArgb(0, 122, 204);
    }

    public class CustomScrollBar : Control
    {
        public event EventHandler ValueChanged;

        private const int Thickness = 10;
        private const int ThumbMinLength = 24;

        private int _minimum;
        private int _maximum = 100;
        private int _value;
        private int _largeChange = 10;
        private int _smallChange = 1;

        private bool _thumbHot;
        private bool _thumbDragging;
        private int _dragStartOffset;

        public ScrollBarOrientation Orientation { get; private set; }

        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                ClampValue();
                Invalidate();
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                ClampValue();
                Invalidate();
            }
        }

        public int LargeChange
        {
            get { return _largeChange; }
            set
            {
                _largeChange = Math.Max(1, value);
                ClampValue();
                Invalidate();
            }
        }

        public int SmallChange
        {
            get { return _smallChange; }
            set { _smallChange = Math.Max(1, value); }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                int clamped = ClampToRange(value);

                if (_value == clamped)
                {
                    return;
                }

                _value = clamped;

                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public CustomScrollBar(ScrollBarOrientation orientation)
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            Orientation = orientation;

            if (orientation == ScrollBarOrientation.Vertical)
            {
                Width = Thickness;
            }
            else
            {
                Height = Thickness;
            }
        }

        private int MaxScrollValue
        {
            get { return Math.Max(_minimum, _maximum - _largeChange + 1); }
        }

        private void ClampValue()
        {
            int clamped = ClampToRange(_value);

            if (clamped == _value)
            {
                return;
            }

            _value = clamped;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private int ClampToRange(int value)
        {
            if (value < _minimum)
            {
                return _minimum;
            }

            if (value > MaxScrollValue)
            {
                return MaxScrollValue;
            }

            return value;
        }

        private int TrackLength
        {
            get { return Orientation == ScrollBarOrientation.Vertical ? Height : Width; }
        }

        private int ThumbLength
        {
            get
            {
                int effectiveRange = Math.Max(1, _maximum - _minimum + 1);
                int length = (int)((double)_largeChange / effectiveRange * TrackLength);

                return Math.Max(ThumbMinLength, Math.Min(TrackLength, length));
            }
        }

        private int ThumbTravel
        {
            get { return Math.Max(0, TrackLength - ThumbLength); }
        }

        private int ValueRange
        {
            get { return Math.Max(1, MaxScrollValue - _minimum); }
        }

        private int ThumbPosition
        {
            get
            {
                if (ThumbTravel == 0)
                {
                    return 0;
                }

                return (int)((double)(_value - _minimum) / ValueRange * ThumbTravel);
            }
        }

        private Rectangle ThumbBounds
        {
            get
            {
                int pos = ThumbPosition;
                int length = ThumbLength;

                if (Orientation == ScrollBarOrientation.Vertical)
                {
                    return new Rectangle(1, pos, Width - 2, length);
                }

                return new Rectangle(pos, 1, length, Height - 2);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            Rectangle thumb = ThumbBounds;

            if (thumb.Contains(e.Location))
            {
                _thumbDragging = true;
                _dragStartOffset = Orientation == ScrollBarOrientation.Vertical
                    ? e.Y - thumb.Top
                    : e.X - thumb.Left;

                Invalidate();
                return;
            }

            // Track click (above/below or left/right of the thumb): page toward the click.
            int clickPos = Orientation == ScrollBarOrientation.Vertical ? e.Y : e.X;
            int thumbStart = Orientation == ScrollBarOrientation.Vertical ? thumb.Top : thumb.Left;

            Value = clickPos < thumbStart ? Value - LargeChange : Value + LargeChange;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool wasHot = _thumbHot;
            _thumbHot = ThumbBounds.Contains(e.Location);

            if (_thumbDragging)
            {
                int pos = Orientation == ScrollBarOrientation.Vertical ? e.Y : e.X;
                int newThumbPos = pos - _dragStartOffset;

                newThumbPos = Math.Max(0, Math.Min(ThumbTravel, newThumbPos));

                Value = ThumbTravel == 0
                    ? Minimum
                    : Minimum + (int)Math.Round((double)newThumbPos / ThumbTravel * ValueRange);
            }
            else if (wasHot != _thumbHot)
            {
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_thumbDragging)
            {
                _thumbDragging = false;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_thumbHot)
            {
                _thumbHot = false;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush trackBrush = new SolidBrush(ScrollBarColors.Track))
            {
                g.FillRectangle(trackBrush, ClientRectangle);
            }

            Rectangle thumb = ThumbBounds;

            if (thumb.Width <= 0 || thumb.Height <= 0)
            {
                return;
            }

            Color thumbColor = ScrollBarColors.Thumb;

            if (_thumbDragging)
            {
                thumbColor = ScrollBarColors.ThumbPressed;
            }
            else if (_thumbHot)
            {
                thumbColor = ScrollBarColors.ThumbHover;
            }

            int radius = Math.Min(thumb.Width, thumb.Height) / 2;

            using (GraphicsPath path = RoundedRect(thumb, radius))
            using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
            {
                g.FillPath(thumbBrush, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int d = radius * 2;

            if (d <= 0 || d >= bounds.Width || d >= bounds.Height)
            {
                path.AddRectangle(bounds);
                return path;
            }

            Rectangle arc = new Rectangle(bounds.Location, new Size(d, d));

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - d;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - d;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();

            return path;
        }
    }
}