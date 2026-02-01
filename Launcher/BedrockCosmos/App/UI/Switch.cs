using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

// Based off of Hope Switch from ReaLTaiizor
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
            set { _BaseColor = value; }
        }

        public Color BaseOnColor
        {
            get { return _BaseOnColor; }
            set { _BaseOnColor = value; }
        }

        public Color BaseOffColor
        {
            get { return _BaseOffColor; }
            set { _BaseOffColor = value; }
        }

        public Switch()
        {
            AnimationTimer = new Timer();
            AnimationTimer.Interval = 1; // Set to a small interval for smooth animation
            AnimationTimer.Tick += new EventHandler(AnimationTick);

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
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
            graphics.Clear(Parent.BackColor);

            // Create the graphics path manually
            GraphicsPath backRect = new GraphicsPath();
            backRect.AddArc(new RectangleF(0.5f, 0.5f, Height - 1, Height - 1), 90, 180);
            backRect.AddArc(new RectangleF(Width - Height + 0.5f, 0.5f, Height - 1, Height - 1), 270, 180);
            backRect.CloseAllFigures();

            graphics.FillPath(new SolidBrush(Checked ? _BaseOnColor : _BaseOffColor), backRect);
            graphics.FillEllipse(new SolidBrush(_BaseColor), new RectangleF(PointAnimationNum, 2, 16, 16));
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
