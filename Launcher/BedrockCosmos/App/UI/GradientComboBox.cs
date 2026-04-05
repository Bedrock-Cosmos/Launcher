using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Built off of Dungeon Combo Box from ReaLTaiizor to work with .NET 4.7.2
// https://github.com/Taiizor/ReaLTaiizor

namespace BedrockCosmos.App.UI
{
    public class GradientComboBox : ComboBox
    {
        private const int ComboBoxSetTopIndexMessage = 0x015C;
        private const int DropDownPadding = 10;
        private const int MaxVisibleDropDownItems = 8;

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

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

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

        public GradientComboBox()
        {
            SetStyle((ControlStyles)139286, true);
            SetStyle(ControlStyles.Selectable, true);

            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            IntegralHeight = false;

            BackColor = Color.FromArgb(246, 246, 246);
            ForeColor = Color.FromArgb(76, 76, 97);
            Size = new Size(135, 26);
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            ItemHeight = Math.Max(22, Font.Height + 8);
            Cursor = Cursors.Hand;

            RefreshDropDownMetrics();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RefreshDropDownMetrics();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ItemHeight = Math.Max(22, Font.Height + 8);
            RefreshDropDownMetrics();
            Invalidate();
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);
            e.ItemHeight = ItemHeight;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle itemBounds = e.Bounds;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color selectedBackColor = _HoverSelectionColor.IsEmpty
                ? Color.FromArgb(70, 70, 70)
                : _HoverSelectionColor;
            Color itemBackColor = isSelected ? selectedBackColor : _ColorC;
            Color itemTextColor = isSelected ? Color.WhiteSmoke : ForeColor;

            using (SolidBrush backgroundBrush = new SolidBrush(itemBackColor))
            using (SolidBrush accentBrush = new SolidBrush(Color.FromArgb(153, 153, 153)))
            {
                e.Graphics.FillRectangle(backgroundBrush, itemBounds);

                if (isSelected)
                    e.Graphics.FillRectangle(accentBrush, new Rectangle(itemBounds.X, itemBounds.Y, 3, itemBounds.Height));
            }

            Rectangle textBounds = new Rectangle(
                itemBounds.X + DropDownPadding,
                itemBounds.Y,
                Math.Max(0, itemBounds.Width - (DropDownPadding * 2)),
                itemBounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                GetItemText(Items[e.Index]),
                e.Font,
                textBounds,
                itemTextColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
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
            base.OnPaintBackground(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RefreshDropDownMetrics();

            if (!Focused)
                SelectionLength = 0;
        }

        protected override void OnDropDown(EventArgs e)
        {
            RefreshDropDownMetrics();
            base.OnDropDown(e);

            if (IsHandleCreated)
                SendMessage(Handle, ComboBoxSetTopIndexMessage, IntPtr.Zero, IntPtr.Zero);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;
            graphics.Clear(Parent != null ? Parent.BackColor : BackColor);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath borderPath = RoundRectangle.RoundRect(0, 0, Width - 1, Height - 1, 5))
            using (LinearGradientBrush backgroundBrush = new LinearGradientBrush(ClientRectangle, _ColorD, _ColorE, 90f))
            using (Pen borderPen = new Pen(_ColorF))
            using (Brush dividerBrush = new SolidBrush(_ColorH))
            using (Brush dividerShadowBrush = new SolidBrush(_ColorI))
            using (Brush arrowBrush = new SolidBrush(_ColorG))
            using (Font arrowFont = new Font("Marlett", 13f, FontStyle.Regular))
            {
                graphics.SetClip(borderPath);
                graphics.FillRectangle(backgroundBrush, ClientRectangle);
                graphics.ResetClip();
                graphics.DrawPath(borderPen, borderPath);

                TextRenderer.DrawText(
                    graphics,
                    Text,
                    Font,
                    new Rectangle(12, 0, Width - 36, Height),
                    ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                graphics.DrawString(
                    "6",
                    arrowFont,
                    arrowBrush,
                    new Rectangle(3, 0, Width - 4, Height),
                    new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Far
                    });

                graphics.FillRectangle(dividerBrush, new Rectangle(Width - 24, 4, 1, Height - 9));
                graphics.FillRectangle(dividerShadowBrush, new Rectangle(Width - 25, 4, 1, Height - 9));
            }
        }

        private void RefreshDropDownMetrics()
        {
            int visibleItems = Math.Max(1, Math.Min(Items.Count, MaxVisibleDropDownItems));
            DropDownWidth = Math.Max(Width, DropDownWidth);
            DropDownHeight = (visibleItems * ItemHeight) + 2;
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

            gp.AddLine(x + radius, y, x + width, y);
            gp.AddLine(x + width, y, x + width, y + height);
            gp.AddLine(x + width, y + height, x + radius, y + height);
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            gp.AddLine(x, y + height - (radius * 2), x, y + radius);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);

            gp.CloseFigure();
            return gp;
        }
    }
}
