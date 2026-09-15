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
        private const int BaseDropDownPadding = 10;
        private const int MaxVisibleDropDownItems = 8;
        private const int BaseBorderRadius = 5;
        private const int BaseTextLeftMargin = 12;
        private const int BaseTextRightReserve = 36;
        private const int BaseArrowLeftMargin = 3;
        private const int BaseArrowRightReserve = 4;
        private const int BaseDividerRightOffset = 24;
        private const int BaseDividerShadowRightOffset = 25;
        private const int BaseDividerTopMargin = 4;
        private const int BaseDividerBottomReserve = 9;
        private const int BaseItemHeightPadding = 8;
        private const int BaseMinItemHeight = 22;

        private int _DropDownPaddingScaled;
        private int _BorderRadiusScaled;
        private int _TextLeftMarginScaled;
        private int _TextRightReserveScaled;
        private int _ArrowLeftMarginScaled;
        private int _ArrowRightReserveScaled;
        private int _DividerRightOffsetScaled;
        private int _DividerShadowRightOffsetScaled;
        private int _DividerTopMarginScaled;
        private int _DividerBottomReserveScaled;

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

        // Replaces the native ComboBox listbox popup.
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private ToolStripDropDown _dropDownPopup;
        private GradientComboDropDownList _dropDownList;
        private bool _suppressNextOpen;

        private Color _ScrollTrackColor = Color.FromArgb(242, 241, 240);
        private Color _ScrollThumbColor = Color.FromArgb(180, 180, 180);
        private Color _ScrollThumbHoverColor = Color.FromArgb(119, 119, 118);
        private int _ScrollBarWidth = 12;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public Color ScrollTrackColor
        {
            get { return _ScrollTrackColor; }
            set { _ScrollTrackColor = value; _dropDownList?.Invalidate(); }
        }

        public Color ScrollThumbColor
        {
            get { return _ScrollThumbColor; }
            set { _ScrollThumbColor = value; _dropDownList?.Invalidate(); }
        }

        public Color ScrollThumbHoverColor
        {
            get { return _ScrollThumbHoverColor; }
            set { _ScrollThumbHoverColor = value; _dropDownList?.Invalidate(); }
        }

        public int ScrollBarWidth
        {
            get { return _ScrollBarWidth; }
            set { _ScrollBarWidth = Math.Max(4, value); _dropDownList?.Invalidate(); }
        }

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
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            Cursor = Cursors.Hand;

            RefreshScaledMetrics();
            Size = new Size(LogicalToDeviceUnits(135), LogicalToDeviceUnits(26));
            ItemHeight = Math.Max(_MinItemHeightScaled, Font.Height + _ItemHeightPaddingScaled);

            RefreshDropDownMetrics();
        }

        private int _ItemHeightPaddingScaled;
        private int _MinItemHeightScaled;

        private void RefreshScaledMetrics()
        {
            _DropDownPaddingScaled = LogicalToDeviceUnits(BaseDropDownPadding);
            _BorderRadiusScaled = LogicalToDeviceUnits(BaseBorderRadius);
            _TextLeftMarginScaled = LogicalToDeviceUnits(BaseTextLeftMargin);
            _TextRightReserveScaled = LogicalToDeviceUnits(BaseTextRightReserve);
            _ArrowLeftMarginScaled = LogicalToDeviceUnits(BaseArrowLeftMargin);
            _ArrowRightReserveScaled = LogicalToDeviceUnits(BaseArrowRightReserve);
            _DividerRightOffsetScaled = LogicalToDeviceUnits(BaseDividerRightOffset);
            _DividerShadowRightOffsetScaled = LogicalToDeviceUnits(BaseDividerShadowRightOffset);
            _DividerTopMarginScaled = LogicalToDeviceUnits(BaseDividerTopMargin);
            _DividerBottomReserveScaled = LogicalToDeviceUnits(BaseDividerBottomReserve);
            _ItemHeightPaddingScaled = LogicalToDeviceUnits(BaseItemHeightPadding);
            _MinItemHeightScaled = LogicalToDeviceUnits(BaseMinItemHeight);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RefreshDropDownMetrics();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            RefreshScaledMetrics();
            ItemHeight = Math.Max(_MinItemHeightScaled, Font.Height + _ItemHeightPaddingScaled);
            RefreshDropDownMetrics();
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ItemHeight = Math.Max(_MinItemHeightScaled, Font.Height + _ItemHeightPaddingScaled);
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
            // Kept for design-time / fallback compatibility.
            if (e.Index < 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            DrawDropDownItem(e.Graphics, e.Index, e.Bounds, isSelected);
        }

        internal string GetDropDownItemText(int index) => GetItemText(Items[index]);

        internal void DrawDropDownItem(Graphics graphics, int index, Rectangle itemBounds, bool isHighlighted)
        {
            Color selectedBackColor = _HoverSelectionColor.IsEmpty
                ? Color.FromArgb(70, 70, 70)
                : _HoverSelectionColor;
            Color itemBackColor = isHighlighted ? selectedBackColor : _ColorC;
            Color itemTextColor = isHighlighted ? Color.WhiteSmoke : ForeColor;

            using (SolidBrush backgroundBrush = new SolidBrush(itemBackColor))
            using (SolidBrush accentBrush = new SolidBrush(Color.FromArgb(153, 153, 153)))
            {
                graphics.FillRectangle(backgroundBrush, itemBounds);

                if (isHighlighted)
                    graphics.FillRectangle(accentBrush, new Rectangle(itemBounds.X, itemBounds.Y, 3, itemBounds.Height));
            }

            Rectangle textBounds = new Rectangle(
                itemBounds.X + _DropDownPaddingScaled,
                itemBounds.Y,
                Math.Max(0, itemBounds.Width - (_DropDownPaddingScaled * 2)),
                itemBounds.Height);

            TextRenderer.DrawText(
                graphics,
                GetDropDownItemText(index),
                Font,
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

        protected override bool IsInputKey(Keys keyData)
        {
            if (DropDownStyle == ComboBoxStyle.DropDownList)
            {
                Keys code = keyData & Keys.KeyCode;
                if (code == Keys.Enter || code == Keys.Space)
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        // Intercepts the clicks/keys that would open the native drop-down listbox.
        protected override void WndProc(ref Message m)
        {
            if (DropDownStyle == ComboBoxStyle.DropDownList && Enabled)
            {
                switch (m.Msg)
                {
                    case WM_LBUTTONDOWN:
                    case WM_LBUTTONDBLCLK:
                        Focus();
                        if (_suppressNextOpen)
                        {
                            _suppressNextOpen = false;
                            return;
                        }
                        ToggleCustomDropDown();
                        return;

                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        Keys key = (Keys)m.WParam.ToInt32();
                        bool altHeld = (ModifierKeys & Keys.Alt) == Keys.Alt;
                        if (key == Keys.F4 || key == Keys.Enter || key == Keys.Space
                            || ((key == Keys.Down || key == Keys.Up) && altHeld))
                        {
                            ToggleCustomDropDown();
                            return;
                        }
                        break;
                }
            }

            base.WndProc(ref m);
        }

        private bool CustomDropDownOpen => _dropDownPopup != null && _dropDownPopup.Visible;

        private void EnsureCustomDropDownCreated()
        {
            if (_dropDownList != null)
                return;

            _dropDownList = new GradientComboDropDownList(this);

            ToolStripControlHost host = new ToolStripControlHost(_dropDownList)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false
            };

            _dropDownPopup = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                AutoSize = false,
                DropShadowEnabled = true
            };
            _dropDownPopup.Items.Add(host);

            // Auto-closes when it detects a click outside its bounds
            _dropDownPopup.Closed += (s, e) =>
            {
                _suppressNextOpen = true;
                if (IsHandleCreated)
                    BeginInvoke(new Action(() => _suppressNextOpen = false));
                else
                    _suppressNextOpen = false;
            };
        }

        private void ToggleCustomDropDown()
        {
            if (CustomDropDownOpen)
                CloseCustomDropDown(commit: false);
            else
                OpenCustomDropDown();
        }

        private void OpenCustomDropDown()
        {
            RefreshDropDownMetrics();
            EnsureCustomDropDownCreated();

            int width = Math.Max(DropDownWidth, Width);
            int height = DropDownHeight;

            _dropDownList.Size = new Size(width, height);
            _dropDownList.PrepareForShow(SelectedIndex);

            _dropDownPopup.Size = new Size(width, height);
            _dropDownPopup.Show(this, new Point(0, Height));
            _dropDownList.Focus();
        }

        internal void CloseCustomDropDown(bool commit, int selectedIndex = -1)
        {
            if (commit && selectedIndex >= 0 && selectedIndex < Items.Count)
                SelectedIndex = selectedIndex;

            _dropDownPopup?.Close();
            if (IsHandleCreated)
                Focus();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _dropDownPopup?.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;
            graphics.Clear(Parent != null ? Parent.BackColor : BackColor);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath borderPath = RoundRectangle.RoundRect(0, 0, Width - 1, Height - 1, _BorderRadiusScaled))
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
                    new Rectangle(_TextLeftMarginScaled, 0, Width - _TextRightReserveScaled, Height),
                    ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                graphics.DrawString(
                    "6",
                    arrowFont,
                    arrowBrush,
                    new Rectangle(_ArrowLeftMarginScaled, 0, Width - _ArrowRightReserveScaled, Height),
                    new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Far
                    });

                int dividerWidth = Math.Max(1, LogicalToDeviceUnits(1));
                graphics.FillRectangle(dividerBrush, new Rectangle(Width - _DividerRightOffsetScaled, _DividerTopMarginScaled, dividerWidth, Height - _DividerBottomReserveScaled));
                graphics.FillRectangle(dividerShadowBrush, new Rectangle(Width - _DividerShadowRightOffsetScaled, _DividerTopMarginScaled, dividerWidth, Height - _DividerBottomReserveScaled));
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