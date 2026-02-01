using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Based off of Night Button from ReaLTaiizor
namespace BedrockCosmos.App.UI
{
    public class RoundButton : Control, IButtonControl
    {
        private int _Radius = 20;

        private Timer animationTimer;
        private int buttonGlow;
        private int stringGlow;
        private bool hoverButton;

        private int mouseState;

        private Color _FilledBackColor = Color.Transparent;
        private Color _NormalBackColor = ColorTranslator.FromHtml("#F25D59");
        private Color _PressedBackColor = Color.FromArgb(100, ColorTranslator.FromHtml("#F25D59"));
        private Color _HoverBackColor = Color.FromArgb(50, ColorTranslator.FromHtml("#F25D59"));
        private Color _PressedForeColor = Color.White;
        private Color _HoverForeColor = Color.White;
        private Color _HoverFillColor = Color.White;

        private float margin, width, height;
        private Rectangle stringRect;
        private RectangleF buttonRect;
        private GraphicsPath roundRectPath;

        private InterpolationMode _InterpolationType = InterpolationMode.HighQualityBicubic;
        private PixelOffsetMode _PixelOffsetType = PixelOffsetMode.HighQuality;
        private SmoothingMode _SmoothingType = SmoothingMode.AntiAlias;

        [Browsable(true)]
        [Description("Sets the radius of curvature for the control.")]
        public int Radius
        {
            get { return _Radius; }
            set
            {
                if (value < 1 || value > 20)
                    throw new Exception("The entered value cannot be less than 1 or greater than 20.");

                _Radius = value;
                Invalidate();
            }
        }

        public SmoothingMode SmoothingType
        {
            get { return _SmoothingType; }
            set { _SmoothingType = value; Invalidate(); }
        }

        public InterpolationMode InterpolationType
        {
            get { return _InterpolationType; }
            set { _InterpolationType = value; Invalidate(); }
        }

        public PixelOffsetMode PixelOffsetType
        {
            get { return _PixelOffsetType; }
            set { _PixelOffsetType = value; Invalidate(); }
        }

        public Color PressedForeColor
        {
            get { return _PressedForeColor; }
            set { _PressedForeColor = value; Invalidate(); }
        }

        public Color HoverForeColor
        {
            get { return _HoverForeColor; }
            set { _HoverForeColor = value; Invalidate(); }
        }

        public Color HoverFillColor
        {
            get { return _HoverFillColor; }
            set { _HoverFillColor = value; Invalidate(); }
        }

        public Color NormalBackColor
        {
            get { return _NormalBackColor; }
            set { _NormalBackColor = value; Invalidate(); }
        }

        public Color FilledBackColor
        {
            get { return _FilledBackColor; }
            set { _FilledBackColor = value; Invalidate(); }
        }

        public Color PressedBackColor
        {
            get { return _PressedBackColor; }
            set { _PressedBackColor = value; Invalidate(); }
        }

        public Color HoverBackColor
        {
            get { return _HoverBackColor; }
            set { _HoverBackColor = value; Invalidate(); }
        }

        private bool _IsDefault;
        private DialogResult dlgResult;

        public DialogResult DialogResult
        {
            get { return dlgResult; }
            set
            {
                if (Enum.IsDefined(typeof(DialogResult), value))
                    dlgResult = value;
            }
        }

        public void NotifyDefault(bool value)
        {
            _IsDefault = value;
        }

        public void PerformClick()
        {
            if (CanSelect)
                OnClick(EventArgs.Empty);
        }

        private GraphicsPath RoundedRect(
            RectangleF rect,
            float x_radius,
            float y_radius,
            bool round_upperLeft,
            bool round_upperRight,
            bool round_lowerRight,
            bool round_lowerLeft)
        {
            GraphicsPath path = new GraphicsPath();
            PointF point1, point2;

            if (round_upperLeft)
            {
                RectangleF corner = new RectangleF(rect.X, rect.Y, 2 * x_radius, 2 * y_radius);
                path.AddArc(corner, 180, 90);
                point1 = new PointF(rect.X + x_radius, rect.Y);
            }
            else
            {
                point1 = new PointF(rect.X, rect.Y);
            }

            if (round_upperRight)
                point2 = new PointF(rect.Right - x_radius, rect.Y);
            else
            {
                point2 = new PointF(rect.Right, rect.Y);
                path.AddLine(point1, point2);
            }

            if (round_upperRight)
            {
                RectangleF corner = new RectangleF(rect.Right - 2 * x_radius, rect.Y, 2 * x_radius, 2 * y_radius);
                path.AddArc(corner, 270, 90);
                point1 = new PointF(rect.Right, rect.Y + y_radius);
            }
            else
            {
                point1 = new PointF(rect.Right, rect.Y);
            }

            if (round_lowerRight)
                point2 = new PointF(rect.Right, rect.Bottom - y_radius);
            else
            {
                point2 = new PointF(rect.Right, rect.Bottom);
                path.AddLine(point1, point2);
            }

            if (round_lowerRight)
            {
                RectangleF corner = new RectangleF(rect.Right - 2 * x_radius, rect.Bottom - 2 * y_radius, 2 * x_radius, 2 * y_radius);
                path.AddArc(corner, 0, 90);
                point1 = new PointF(rect.Right - x_radius, rect.Bottom);
            }
            else
            {
                point1 = new PointF(rect.Right, rect.Bottom);
            }

            if (round_lowerLeft)
                point2 = new PointF(rect.X + x_radius, rect.Bottom);
            else
            {
                point2 = new PointF(rect.X, rect.Bottom);
                path.AddLine(point1, point2);
            }

            if (round_lowerLeft)
            {
                RectangleF corner = new RectangleF(rect.X, rect.Bottom - 2 * y_radius, 2 * x_radius, 2 * y_radius);
                path.AddArc(corner, 90, 90);
                point1 = new PointF(rect.X, rect.Bottom - y_radius);
            }
            else
            {
                point1 = new PointF(rect.X, rect.Bottom);
            }

            if (round_upperLeft)
                point2 = new PointF(rect.X, rect.Y + y_radius);
            else
            {
                point2 = new PointF(rect.X, rect.Y);
                path.AddLine(point1, point2);
            }

            path.CloseFigure();
            return path;
        }

        public RoundButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint, true);

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10f);
            ForeColor = ColorTranslator.FromHtml("#F25D59");
            Size = new Size(144, 47);
            MinimumSize = new Size(144, 47);
            Cursor = Cursors.Hand;

            animationTimer = new Timer();
            animationTimer.Interval = 15;
            animationTimer.Tick += OnAnimation;
        }

        private void FillButton(Graphics g)
        {
            Color fillColor;

            // Choose fill color based on mouse state
            switch (mouseState)
            {
                case 1: // Pressed
                    fillColor = PressedBackColor;
                    break;
                case 3: // Hover
                    fillColor = HoverBackColor;
                    break;
                default: // Normal
                    fillColor = FilledBackColor;
                    break;
            }

            // Fill the rounded rectangle with the chosen background color
            using (SolidBrush backBrush = new SolidBrush(fillColor))
            {
                g.FillPath(backBrush, roundRectPath);
            }

            using (SolidBrush animBrush = new SolidBrush(Color.FromArgb(buttonGlow, HoverFillColor)))
            {
                g.FillPath(animBrush, roundRectPath);
            }
        }

        private void DrawButton(Graphics g)
        {
            Color penColor;
            Color brushColor;

            switch (mouseState)
            {
                default:
                case 0:
                    penColor = NormalBackColor;
                    brushColor = ForeColor;
                    break;
                case 1:
                    penColor = PressedBackColor;
                    brushColor = PressedForeColor;
                    break;
                case 3:
                    penColor = HoverBackColor;
                    brushColor = Color.FromArgb(80 + stringGlow, HoverForeColor);
                    break;
            }

            using (Pen pathPen = new Pen(penColor, 2f))
            using (SolidBrush stringBrush = new SolidBrush(brushColor))
            using (StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawPath(pathPen, roundRectPath);
                g.DrawString(Text, Font, stringBrush, stringRect, sf);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = _SmoothingType;
            g.InterpolationMode = _InterpolationType;
            g.PixelOffsetMode = _PixelOffsetType;

            stringRect = new Rectangle(0, 0, Width, Height);

            margin = 3;
            width = ClientSize.Width - 2 * margin;
            height = ClientSize.Height - 6;

            buttonRect = new RectangleF(margin, margin, width, height);
            roundRectPath = RoundedRect(buttonRect, _Radius, _Radius, true, true, true, true);

            FillButton(g);
            DrawButton(g);
        }

        private void OnAnimation(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            if (hoverButton)
            {
                if (buttonGlow < 242)
                    buttonGlow += 15;

                if (stringGlow < 160)
                    stringGlow += 15;

                if (buttonGlow >= 242)
                    animationTimer.Stop();
            }
            else
            {
                if (buttonGlow >= 15)
                    buttonGlow -= 15;

                if (stringGlow >= 15)
                    stringGlow -= 15;

                if (buttonGlow <= 0)
                    animationTimer.Stop();
            }

            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hoverButton = true;
            mouseState = 3;
            Invalidate();
            base.OnMouseEnter(e);

            if (!DesignMode)
                animationTimer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hoverButton = false;
            mouseState = 0;
            Invalidate();
            base.OnMouseLeave(e);

            if (!DesignMode)
                animationTimer.Start();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseState = 1;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseState = hoverButton ? 3 : 0;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }
    }
}