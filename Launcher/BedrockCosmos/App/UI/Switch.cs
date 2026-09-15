using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

// Built off of Hope Switch from ReaLTaiizor to work with .NET 4.7.2
// https://github.com/Taiizor/ReaLTaiizor

namespace BedrockCosmos.App.UI
{
    public class Switch : System.Windows.Forms.CheckBox
    {
        private const int BaseWidth = 40;
        private const int BaseHeight = 20;
        private const int BaseKnobSize = 16;
        private const int BaseKnobMarginY = 2;
        private const int BaseKnobOffX = 3;
        private const int BaseAnimationStep = 2;

        private readonly Timer AnimationTimer;
        private int PointAnimationNum;
        private int _ScaledWidth;
        private int _ScaledHeight;
        private int _ScaledKnobSize;
        private int _ScaledKnobMarginY;
        private int _ScaledKnobOffX;
        private int _ScaledKnobOnX;
        private int _ScaledAnimationStep;
        private Color _BaseColor = Color.White;
        private Color _BaseOnColor = Color.Cyan;
        private Color _BaseOffColor = Color.Gray;

        public Color BaseColor
        {
            get { return _BaseColor; }
            set { _BaseColor = value; Invalidate(); }
        }

        public Color BaseOnColor
        {
            get { return _BaseOnColor; }
            set { _BaseOnColor = value; Invalidate(); }
        }

        public Color BaseOffColor
        {
            get { return _BaseOffColor; }
            set { _BaseOffColor = value; Invalidate(); }
        }

        public Switch()
        {
            AnimationTimer = new Timer();
            AnimationTimer.Interval = 1;
            AnimationTimer.Tick += new EventHandler(AnimationTick);

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;

            RefreshScaledMetrics();
            Height = _ScaledHeight;
            Width = _ScaledWidth;
            PointAnimationNum = Checked ? _ScaledKnobOnX : _ScaledKnobOffX;
        }

        private void RefreshScaledMetrics()
        {
            _ScaledWidth = LogicalToDeviceUnits(BaseWidth);
            _ScaledHeight = LogicalToDeviceUnits(BaseHeight);
            _ScaledKnobSize = LogicalToDeviceUnits(BaseKnobSize);
            _ScaledKnobMarginY = LogicalToDeviceUnits(BaseKnobMarginY);
            _ScaledKnobOffX = LogicalToDeviceUnits(BaseKnobOffX);
            _ScaledAnimationStep = Math.Max(1, LogicalToDeviceUnits(BaseAnimationStep));
            _ScaledKnobOnX = _ScaledWidth - _ScaledKnobSize - _ScaledKnobOffX;
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);

            bool wasOn = PointAnimationNum > (_ScaledKnobOffX + _ScaledKnobOnX) / 2;
            RefreshScaledMetrics();

            AnimationTimer.Stop();
            PointAnimationNum = wasOn ? _ScaledKnobOnX : _ScaledKnobOffX;

            Height = _ScaledHeight;
            Width = _ScaledWidth;
            Invalidate();
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);

            if (Checked)
            {
                if (PointAnimationNum < _ScaledKnobOnX)
                    AnimationTimer.Start();
            }
            else
            {
                if (PointAnimationNum > _ScaledKnobOffX)
                    AnimationTimer.Start();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = _ScaledHeight;
            Width = _ScaledWidth;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.InterpolationMode = InterpolationMode.High;

            if (BackColor == Color.Transparent || BackColor.A == 0)
            {
                if (Parent != null)
                {
                    Rectangle rect = new Rectangle(Left, Top, Width, Height);
                    graphics.TranslateTransform(-Left, -Top);
                    InvokePaintBackground(Parent, new PaintEventArgs(graphics, rect));
                    InvokePaint(Parent, new PaintEventArgs(graphics, rect));
                    graphics.TranslateTransform(Left, Top);
                }
            }
            else
            {
                graphics.Clear(BackColor);
            }

            using (GraphicsPath backRect = new GraphicsPath())
            {
                backRect.AddArc(new RectangleF(0.5f, 0.5f, Height - 1, Height - 1), 90, 180);
                backRect.AddArc(new RectangleF(Width - Height + 0.5f, 0.5f, Height - 1, Height - 1), 270, 180);
                backRect.CloseAllFigures();

                using (SolidBrush trackBrush = new SolidBrush(Checked ? _BaseOnColor : _BaseOffColor))
                    graphics.FillPath(trackBrush, backRect);
            }

            using (SolidBrush knobBrush = new SolidBrush(_BaseColor))
                graphics.FillEllipse(knobBrush, new RectangleF(PointAnimationNum, _ScaledKnobMarginY, _ScaledKnobSize, _ScaledKnobSize));
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (Checked)
            {
                if (PointAnimationNum < _ScaledKnobOnX)
                {
                    PointAnimationNum = Math.Min(_ScaledKnobOnX, PointAnimationNum + _ScaledAnimationStep);
                    Invalidate();
                }
                else
                {
                    AnimationTimer.Stop();
                }
            }
            else
            {
                if (PointAnimationNum > _ScaledKnobOffX)
                {
                    PointAnimationNum = Math.Max(_ScaledKnobOffX, PointAnimationNum - _ScaledAnimationStep);
                    Invalidate();
                }
                else
                {
                    AnimationTimer.Stop();
                }
            }
        }
    }
}