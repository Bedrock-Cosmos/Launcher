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
        private readonly Timer AnimationTimer;
        private int PointAnimationNum = 3;
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
            BackColor = Color.Transparent; // Default to transparent
            Height = 20;
            Width = 42;
            Cursor = Cursors.Hand;
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);

            if (Checked)
            {
                if (PointAnimationNum < 21)
                    AnimationTimer.Start();
            }
            else
            {
                if (PointAnimationNum > 3)
                    AnimationTimer.Start();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = 20;
            Width = 40;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.InterpolationMode = InterpolationMode.High;

            // Branch on whether the background is transparent or a solid color.
            // Color.Transparent has an Alpha of 0; we treat any fully-transparent
            // color the same way so callers can also pass Color.FromArgb(0, ...).
            if (BackColor == Color.Transparent || BackColor.A == 0)
            {
                // Ask the parent to paint itself into our graphics context at the
                // correct offset so gradients, images, and themed panels show through.
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
                // A real BackColor was set — just flood-fill with it.
                graphics.Clear(BackColor);
            }

            // Draw the pill-shaped track.
            using (GraphicsPath backRect = new GraphicsPath())
            {
                backRect.AddArc(new RectangleF(0.5f, 0.5f, Height - 1, Height - 1), 90, 180);
                backRect.AddArc(new RectangleF(Width - Height + 0.5f, 0.5f, Height - 1, Height - 1), 270, 180);
                backRect.CloseAllFigures();

                using (SolidBrush trackBrush = new SolidBrush(Checked ? _BaseOnColor : _BaseOffColor))
                    graphics.FillPath(trackBrush, backRect);
            }

            // Draw the knob.
            using (SolidBrush knobBrush = new SolidBrush(_BaseColor))
                graphics.FillEllipse(knobBrush, new RectangleF(PointAnimationNum, 2, 16, 16));
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (Checked)
            {
                if (PointAnimationNum < 21)
                {
                    PointAnimationNum += 2;
                    Invalidate();
                }
                else
                {
                    AnimationTimer.Stop();
                }
            }
            else
            {
                if (PointAnimationNum > 3)
                {
                    PointAnimationNum -= 2;
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