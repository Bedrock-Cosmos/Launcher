using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Built off of Dungeon Combo Box from ReaLTaiizor to work with .NET 4.7.2
// https://github.com/Taiizor/ReaLTaiizor

namespace BedrockCosmos.App.UI
{
    public class GradientComboBox : ComboBox
    {
        private int _StartIndex = 0;
        private Color _HoverSelectionColor;

        private Color _ColorA = Color.FromArgb(246, 132, 85);
        private Color _ColorB = Color.FromArgb(231, 108, 57);
        private Color _ColorC = Color.FromArgb(242, 241, 240);
        private Color _ColorD = Color.FromArgb(253, 252, 252);
        private Color _ColorE = Color.FromArgb(239, 237, 236);
        private Color _ColorF = Color.FromArgb(180, 180, 180);
        private Color _ColorG = Color.FromArgb(119, 119, 118);
        private Color _ColorH = Color.FromArgb(224, 222, 220);
        private Color _ColorI = Color.FromArgb(250, 249, 249);

        public int StartIndex
        {
            get { return _StartIndex; }
            set
            {
                _StartIndex = value;
                try
                {
                    base.SelectedIndex = value;
                }
                catch
                {
                }
                Invalidate();
            }
        }

        public Color HoverSelectionColor
        {
            get { return _HoverSelectionColor; }
            set
            {
                _HoverSelectionColor = value;
                Invalidate();
            }
        }

        public Color ColorA
        {
            get { return _ColorA; }
            set { _ColorA = value; }
        }

        public Color ColorB
        {
            get { return _ColorB; }
            set { _ColorB = value; }
        }

        public Color ColorC
        {
            get { return _ColorC; }
            set { _ColorC = value; }
        }

        public Color ColorD
        {
            get { return _ColorD; }
            set { _ColorD = value; }
        }

        public Color ColorE
        {
            get { return _ColorE; }
            set { _ColorE = value; }
        }

        public Color ColorF
        {
            get { return _ColorF; }
            set { _ColorF = value; }
        }

        public Color ColorG
        {
            get { return _ColorG; }
            set { _ColorG = value; }
        }

        public Color ColorH
        {
            get { return _ColorH; }
            set { _ColorH = value; }
        }

        public Color ColorI
        {
            get { return _ColorI; }
            set { _ColorI = value; }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (e.Index < 0)
                return;

            LinearGradientBrush lgb =
                new LinearGradientBrush(e.Bounds, _ColorA, _ColorB, 90f);

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(lgb, e.Bounds);
                e.Graphics.DrawString(
                    GetItemText(Items[e.Index]),
                    e.Font,
                    Brushes.WhiteSmoke,
                    e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(_ColorC),
                    e.Bounds);

                e.Graphics.DrawString(
                    GetItemText(Items[e.Index]),
                    e.Font,
                    Brushes.DimGray,
                    e.Bounds);
            }

            lgb.Dispose();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            SuspendLayout();
            Update();
            ResumeLayout();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Empty to prevent default background rectangle from being drawn over parent's background.
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!Focused)
            {
                SelectionLength = 0;
            }
        }

        public GradientComboBox()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);

            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;

            BackColor = Color.FromArgb(246, 246, 246);
            ForeColor = Color.FromArgb(76, 76, 97);
            Size = new Size(135, 26);
            ItemHeight = 20;
            DropDownHeight = 100;
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            LinearGradientBrush lgb = null;
            GraphicsPath gp = null;

            if (BackColor == Color.Transparent)
            {
                if (Parent != null)
                {
                    e.Graphics.TranslateTransform(-Left, -Top);
                    Rectangle parentClip = new Rectangle(Left, Top, Width, Height);
                    using (PaintEventArgs parentArgs = new PaintEventArgs(e.Graphics, parentClip))
                    {
                        InvokePaintBackground(Parent, parentArgs);
                        InvokePaint(Parent, parentArgs);
                    }
                    e.Graphics.TranslateTransform(Left, Top);
                }
            }
            else
            {
                e.Graphics.Clear(BackColor);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            gp = RoundRectangle.RoundRect(0, 0, Width - 1, Height - 1, 5);
            lgb = new LinearGradientBrush(ClientRectangle, _ColorD, _ColorE, 90f);

            e.Graphics.SetClip(gp);
            e.Graphics.FillRectangle(lgb, ClientRectangle);
            e.Graphics.ResetClip();

            e.Graphics.DrawPath(new Pen(_ColorF), gp);

            e.Graphics.DrawString(
                Text,
                Font,
                new SolidBrush(ForeColor),
                new Rectangle(3, 0, Width - 20, Height),
                new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                });

            e.Graphics.DrawString(
                "6",
                new Font("Marlett", 13f, FontStyle.Regular),
                new SolidBrush(_ColorG),
                new Rectangle(3, 0, Width - 4, Height),
                new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Far
                });

            e.Graphics.DrawLine(
                new Pen(_ColorH),
                Width - 24, 4,
                Width - 24, Height - 5);

            e.Graphics.DrawLine(
                new Pen(_ColorI),
                Width - 25, 4,
                Width - 25, Height - 5);

            if (gp != null)
                gp.Dispose();

            if (lgb != null)
                lgb.Dispose();
        }
    }

    public sealed class RoundRectangle
    {
        public static GraphicsPath RoundRect(Rectangle rectangle, int curve)
        {
            GraphicsPath gp = new GraphicsPath();

            int arcWidth = curve * 2;

            gp.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcWidth, arcWidth), -180, 90);
            gp.AddArc(new Rectangle(rectangle.Width - arcWidth + rectangle.X, rectangle.Y, arcWidth, arcWidth), -90, 90);
            gp.AddArc(new Rectangle(rectangle.Width - arcWidth + rectangle.X, rectangle.Height - arcWidth + rectangle.Y, arcWidth, arcWidth), 0, 90);
            gp.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcWidth + rectangle.Y, arcWidth, arcWidth), 90, 90);
            gp.AddLine(
                new Point(rectangle.X, rectangle.Height - arcWidth + rectangle.Y),
                new Point(rectangle.X, curve + rectangle.Y)
            );

            return gp;
        }

        public static GraphicsPath RoundRect(int x, int y, int width, int height, int curve)
        {
            Rectangle rectangle = new Rectangle(x, y, width, height);
            GraphicsPath gp = new GraphicsPath();

            int arcWidth = curve * 2;

            gp.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcWidth, arcWidth), -180, 90);
            gp.AddArc(new Rectangle(rectangle.Width - arcWidth + rectangle.X, rectangle.Y, arcWidth, arcWidth), -90, 90);
            gp.AddArc(new Rectangle(rectangle.Width - arcWidth + rectangle.X, rectangle.Height - arcWidth + rectangle.Y, arcWidth, arcWidth), 0, 90);
            gp.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcWidth + rectangle.Y, arcWidth, arcWidth), 90, 90);
            gp.AddLine(
                new Point(rectangle.X, rectangle.Height - arcWidth + rectangle.Y),
                new Point(rectangle.X, curve + rectangle.Y)
            );

            return gp;
        }

        public static GraphicsPath RoundedTopRect(Rectangle rectangle, int curve)
        {
            GraphicsPath gp = new GraphicsPath();

            int arcWidth = curve * 2;

            gp.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcWidth, arcWidth), -180, 90);
            gp.AddArc(new Rectangle(rectangle.Width - arcWidth + rectangle.X, rectangle.Y, arcWidth, arcWidth), -90, 90);
            gp.AddLine(
                new Point(rectangle.X + rectangle.Width, rectangle.Y + arcWidth),
                new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height - 1)
            );
            gp.AddLine(
                new Point(rectangle.X, rectangle.Height - 1 + rectangle.Y),
                new Point(rectangle.X, rectangle.Y + curve)
            );

            return gp;
        }

        public static GraphicsPath CreateRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            gp.AddLine(x + radius, y, x + width - (radius * 2), y);
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);

            gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);

            gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);

            gp.AddLine(x, y + height - (radius * 2), x, y + radius);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);

            gp.CloseFigure();
            return gp;
        }

        public static GraphicsPath CreateUpRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            gp.AddLine(x + radius, y, x + width - (radius * 2), y);
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);

            gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2) + 1);
            gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, 2, 0, 90);

            gp.AddLine(x + width, y + height, x + radius, y + height);
            gp.AddArc(x, y + height - (radius * 2) + 1, radius * 2, 1, 90, 90);

            gp.AddLine(x, y + height, x, y + radius);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);

            gp.CloseFigure();
            return gp;
        }

        public static GraphicsPath CreateLeftRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            gp.AddLine(x + radius, y, x + width - (radius * 2), y);
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);

            gp.AddLine(x + width, y, x + width, y + height);
            gp.AddArc(x + width - (radius * 2), y + height - 1, radius * 2, 1, 0, 90);

            gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);

            gp.AddLine(x, y + height - (radius * 2), x, y + radius);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);

            gp.CloseFigure();
            return gp;
        }

        public static Color BlendColor(Color backgroundColor, Color frontColor)
        {
            double ratio = 0 / 255d;
            double invRatio = 1d - ratio;

            int r = (int)((backgroundColor.R * invRatio) + (frontColor.R * ratio));
            int g = (int)((backgroundColor.G * invRatio) + (frontColor.G * ratio));
            int b = (int)((backgroundColor.B * invRatio) + (frontColor.B * ratio));

            return Color.FromArgb(r, g, b);
        }

        public static Color BackColor = ColorTranslator.FromHtml("#DADCDF");
        public static Color DarkBackColor = ColorTranslator.FromHtml("#90949A");
        public static Color LightBackColor = ColorTranslator.FromHtml("#F5F5F5");
    }
}
