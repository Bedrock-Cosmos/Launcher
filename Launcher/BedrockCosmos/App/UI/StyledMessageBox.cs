using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BedrockCosmos.App.UI
{
    public sealed class StyledMessageBoxButton
    {
        public StyledMessageBoxButton(string text, Action onClick = null)
        {
            Text = text ?? string.Empty;
            OnClick = onClick;
        }

        // Used for the standard MessageBoxButtons sets.
        internal StyledMessageBoxButton(DialogResult result, string text)
        {
            Text = text ?? string.Empty;
            Result = result;
        }

        public string Text { get; }
        public Action OnClick { get; }
        public object Tag { get; set; }
        public bool IsCancel { get; set; }
        public bool IsPrimary { get; set; }
        public bool CloseOnClick { get; set; } = true;

        internal DialogResult Result { get; }
    }

    public sealed class StyledMessageBox : Form
    {
        // Palette
        private static readonly Color BackdropColor = Color.FromArgb(20, 20, 20);
        private static readonly Color BarColor = Color.FromArgb(15, 15, 15);
        private static readonly Color BorderColor = Color.FromArgb(60, 60, 60);
        private static readonly Color TextColor = Color.FromArgb(153, 153, 153);
        private static readonly Color Accent = Color.FromArgb(0, 188, 71);
        private static readonly Color AccentHover = Color.FromArgb(0, 208, 80);

        // Layout (pixels at 100% DPI, scaled automatically). Tweak these to resize every dialog.
        private const int MinDialogWidth = 280;   // Narrowest the dialog can get.
        private const int MaxTextWidth = 340;     // Text wraps beyond this; main driver of dialog width.
        private const int TitleBarHeight = 30;
        private const int TitleIconSize = 25;
        private const int ContentPadding = 16;
        private const int GlyphSize = 32;         // Warning / error / custom image size.
        private const int GlyphGap = 12;          // Space between the glyph and the text.
        private const int ButtonWidth = 84;
        private const int ButtonHeight = 28;
        private const int ButtonTextPadding = 12; // Per side; buttons grow past ButtonWidth for long text.
        private const int ButtonSpacing = 4;      // Applied on each side of every button.
        private const int ButtonBarPadding = 10;
        private const float TitleFontSize = 10F;
        private const float BodyFontSize = 10F;
        private const float ButtonFontSize = 10F;

        public static string DefaultCaption { get; set; } = "Bedrock Cosmos";
        public static Func<DialogResult, string> ButtonTextProvider { get; set; } // For multi-language.
        public static Image TitleBarImage { get; set; }
        public static bool HighlightDefaultButton { get; set; } = false;

        public static DialogResult Show(
            string text,
            string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
            Image customIcon = null,
            bool? showCloseButton = null)
        {
            return Show((IWin32Window)null, text, caption, buttons, icon, defaultButton, customIcon, showCloseButton);
        }
        public static DialogResult Show(
            IWin32Window owner,
            string text,
            string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
            Image customIcon = null,
            bool? showCloseButton = null)
        {
            using (var dialog = new StyledMessageBox(owner, text, caption ?? DefaultCaption,
                ItemsFor(buttons), icon, (int)defaultButton / 256, customIcon, showCloseButton))
            {
                return dialog.ShowDialog(owner);
            }
        }

        public static StyledMessageBoxButton Show(
            string text,
            string caption,
            IList<StyledMessageBoxButton> buttons,
            MessageBoxIcon icon = MessageBoxIcon.None,
            int defaultButtonIndex = 0,
            Image customIcon = null,
            bool? showCloseButton = null)
        {
            return Show((IWin32Window)null, text, caption, buttons, icon, defaultButtonIndex, customIcon, showCloseButton);
        }
        public static StyledMessageBoxButton Show(
            IWin32Window owner,
            string text,
            string caption,
            IList<StyledMessageBoxButton> buttons,
            MessageBoxIcon icon = MessageBoxIcon.None,
            int defaultButtonIndex = 0,
            Image customIcon = null,
            bool? showCloseButton = null)
        {
            if (buttons == null || buttons.Count == 0)
                throw new ArgumentException("At least one button is required.", nameof(buttons));

            using (var dialog = new StyledMessageBox(owner, text, caption ?? DefaultCaption,
                buttons, icon, defaultButtonIndex, customIcon, showCloseButton))
            {
                dialog.ShowDialog(owner);
                return dialog._clicked;
            }
        }

        private readonly float _scale;
        private readonly string _text;
        private readonly string _caption;
        private readonly MessageBoxIcon _icon;
        private readonly Image _customIcon;
        private readonly IList<StyledMessageBoxButton> _items;
        private readonly StyledMessageBoxButton _escapeItem;
        private readonly Dictionary<Control, StyledMessageBoxButton> _buttonMap = new Dictionary<Control, StyledMessageBoxButton>();
        private readonly bool _showClose;
        private StyledMessageBoxButton _clicked;
        private bool _dismissed; // Closed via X / Escape with no button to click.
        private readonly Control _defaultControl;
        private readonly Rectangle _ownerBounds;
        private Bitmap _titleBitmap;

        private int S(int value) => (int)Math.Round(value * _scale);

        private StyledMessageBox(IWin32Window owner, string text, string caption,
            IList<StyledMessageBoxButton> items, MessageBoxIcon icon, int defaultIndex, Image customIcon, bool? showCloseButton)
        {
            _scale = DeviceDpi / 96f;
            _text = text ?? string.Empty;
            _caption = caption ?? string.Empty;
            _icon = icon;
            _customIcon = customIcon;
            _items = items;
            _escapeItem = FindEscapeItem(items);
            _showClose = showCloseButton ?? (_escapeItem != null); // null = automatic.

            var ownerForm = owner == null ? null : Control.FromHandle(owner.Handle) as Form;
            _ownerBounds = ownerForm != null && ownerForm.Visible && ownerForm.WindowState != FormWindowState.Minimized
                ? ownerForm.Bounds
                : Screen.FromPoint(Cursor.Position).WorkingArea;

            SuspendLayout();

            AutoScaleMode = AutoScaleMode.None; // Scaled manually via S().
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = BackdropColor;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = _caption;
            TopMost = ownerForm == null || !ownerForm.Visible; // Findable if hidden.
            if (ownerForm != null) Icon = ownerForm.Icon;

            var root = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BorderColor,
                ColumnCount = 1,
                RowCount = 3,
                Location = Point.Empty,
                Margin = Padding.Empty,
                MinimumSize = new Size(S(MinDialogWidth), 0),
                Padding = new Padding(1)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < 3; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            root.Controls.Add(BuildTitleBar(ownerForm), 0, 0);
            root.Controls.Add(BuildContent(), 0, 1);
            root.Controls.Add(BuildButtonBar(defaultIndex, out _defaultControl), 0, 2);

            Controls.Add(root);
            ResumeLayout(true);

            if (_defaultControl != null) ActiveControl = _defaultControl;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _titleBitmap?.Dispose();
            base.Dispose(disposing);
        }

        private static DialogResult[] ResultsFor(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK: return new[] { DialogResult.OK };
                case MessageBoxButtons.OKCancel: return new[] { DialogResult.OK, DialogResult.Cancel };
                case MessageBoxButtons.AbortRetryIgnore: return new[] { DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore };
                case MessageBoxButtons.YesNoCancel: return new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel };
                case MessageBoxButtons.YesNo: return new[] { DialogResult.Yes, DialogResult.No };
                case MessageBoxButtons.RetryCancel: return new[] { DialogResult.Retry, DialogResult.Cancel };
                default: throw new InvalidEnumArgumentException(nameof(buttons), (int)buttons, typeof(MessageBoxButtons));
            }
        }

        private static IList<StyledMessageBoxButton> ItemsFor(MessageBoxButtons buttons)
        {
            var results = ResultsFor(buttons);
            var items = new List<StyledMessageBoxButton>(results.Length);
            foreach (var r in results)
                items.Add(new StyledMessageBoxButton(r, ButtonTextProvider?.Invoke(r) ?? r.ToString()));
            return items;
        }

        // What Escape / X button clicks (button flagged IsCancel, Cancel button, lone button).
        private static StyledMessageBoxButton FindEscapeItem(IList<StyledMessageBoxButton> items)
        {
            foreach (var item in items) if (item.IsCancel) return item;
            foreach (var item in items) if (item.Result == DialogResult.Cancel) return item;
            return items.Count == 1 ? items[0] : null;
        }

        private static DialogResult ResultOf(StyledMessageBoxButton item)
        {
            if (item.Result != DialogResult.None) return item.Result;
            return item.IsCancel ? DialogResult.Cancel : DialogResult.OK;
        }

        // Runs the button's code, then closes dialog unless CloseOnClick = false.
        private void ActivateButton(StyledMessageBoxButton item)
        {
            item.OnClick?.Invoke();
            if (!item.CloseOnClick) return;
            _clicked = item;
            DialogResult = ResultOf(item);
        }

        // Escape / title bar X. Clicks escape button if there is one, otherwise just dismisses the dialog.
        private void Dismiss()
        {
            if (_escapeItem != null) ActivateButton(_escapeItem);
            else if (_showClose)
            {
                _dismissed = true;
                DialogResult = DialogResult.Cancel;
            }
        }

        // Scales top left icon.
        private static Bitmap ScaleToFit(Image source, int size)
        {
            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                float ratio = Math.Min((float)size / source.Width, (float)size / source.Height);
                float w = source.Width * ratio;
                float h = source.Height * ratio;
                g.DrawImage(source, (size - w) / 2f, (size - h) / 2f, w, h);
            }
            return bitmap;
        }

        private Bitmap CreateTitleBitmap(Form ownerForm)
        {
            int size = S(TitleIconSize);
            try
            {
                if (TitleBarImage != null) return ScaleToFit(TitleBarImage, size);

                if (ownerForm != null && ownerForm.Icon != null)
                {
                    using (var icon = new Icon(ownerForm.Icon, new Size(256, 256)))
                    using (var source = icon.ToBitmap())
                        return ScaleToFit(source, size);
                }
            }
            catch (Exception) { }
            return null;
        }

        private Control BuildTitleBar(Form ownerForm)
        {
            var bar = new Panel
            {
                BackColor = BarColor,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Size = new Size(S(100), S(TitleBarHeight))
            };

            var title = new Label
            {
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", TitleFontSize),
                ForeColor = TextColor,
                Text = _caption,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false
            };

            bar.Controls.Add(title); // Fill control goes first.

            _titleBitmap = CreateTitleBitmap(ownerForm);
            if (_titleBitmap != null)
            {
                var iconBox = new PictureBox
                {
                    Dock = DockStyle.Left,
                    Image = _titleBitmap,
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Width = S(TitleBarHeight)
                };
                iconBox.MouseDown += BeginDrag;
                bar.Controls.Add(iconBox);
            }
            else
            {
                title.Padding = new Padding(S(10), 0, 0, 0);
            }

            if (_showClose)
            {
                var close = new Label
                {
                    Cursor = Cursors.Hand,
                    Dock = DockStyle.Right,
                    Font = new Font("Segoe UI Symbol", 9F),
                    ForeColor = TextColor,
                    Text = "\u2715",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = S(38)
                };
                close.MouseEnter += (s, e) => { close.BackColor = Color.FromArgb(196, 43, 28); close.ForeColor = Color.White; };
                close.MouseLeave += (s, e) => { close.BackColor = Color.Transparent; close.ForeColor = TextColor; };
                close.Click += (s, e) => Dismiss();
                bar.Controls.Add(close);
            }

            bar.MouseDown += BeginDrag;
            title.MouseDown += BeginDrag;
            return bar;
        }

        private Control BuildContent()
        {
            var content = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BackdropColor,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = new Padding(S(ContentPadding)),
                WrapContents = false
            };

            bool hasGlyph = _icon != MessageBoxIcon.None || _customIcon != null;
            if (hasGlyph)
            {
                content.Controls.Add(new GlyphIcon(_icon, _customIcon, S(GlyphSize)) { Margin = new Padding(0, 0, S(GlyphGap), 0) });
            }

            content.Controls.Add(new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", BodyFontSize),
                ForeColor = TextColor,
                Margin = Padding.Empty,
                MaximumSize = new Size(S(MaxTextWidth), 0),
                MinimumSize = new Size(0, hasGlyph ? S(GlyphSize) : 0), // Stays centered on icon.
                Text = _text,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false
            });

            return content;
        }

        private Control BuildButtonBar(int defaultIndex, out Control defaultControl)
        {
            var bar = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BarColor,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = Padding.Empty,
                Padding = new Padding(S(ButtonBarPadding), S(ButtonBarPadding), S(ButtonBarPadding - ButtonSpacing), S(ButtonBarPadding)),
                WrapContents = false
            };

            defaultIndex = Math.Max(0, Math.Min(defaultIndex, _items.Count - 1));
            defaultControl = null;

            // Right-To-Left flow.
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                var button = CreateButton(item, primary: item.IsPrimary || (HighlightDefaultButton && i == defaultIndex));
                button.TabIndex = i;
                button.Click += (s, e) => ActivateButton(item);
                _buttonMap[button] = item;
                if (i == defaultIndex) defaultControl = button;
                bar.Controls.Add(button);
            }

            return bar;
        }

        private RoundButton CreateButton(StyledMessageBoxButton item, bool primary)
        {
            var secondaryBorder = Color.FromArgb(75, 75, 75);
            var secondaryFill = Color.FromArgb(30, 30, 30);

            var font = new Font("Segoe UI", ButtonFontSize);
            int textWidth = TextRenderer.MeasureText(item.Text, font).Width;
            int width = Math.Max(S(ButtonWidth), textWidth + S(ButtonTextPadding) * 2); // Fit long custom text.

            var button = new RoundButton
            {
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Font = font,
                Margin = new Padding(S(ButtonSpacing), 0, S(ButtonSpacing), 0),
                Radius = 4,
                Size = new Size(width, S(ButtonHeight)),
                Text = item.Text
            };

            if (primary)
            {
                button.FilledBackColor = Accent;
                button.NormalBackColor = Accent;
                button.HoverBackColor = AccentHover;
                button.HoverFillColor = AccentHover;
                button.PressedBackColor = Accent;
                button.ForeColor = Color.White;
                button.HoverForeColor = Color.White;
                button.PressedForeColor = Color.White;
            }
            else
            {
                button.FilledBackColor = secondaryFill;
                button.NormalBackColor = secondaryBorder;
                button.HoverBackColor = secondaryBorder;
                button.HoverFillColor = secondaryBorder;
                button.PressedBackColor = secondaryBorder;
                button.ForeColor = TextColor;
                button.HoverForeColor = TextColor;
                button.PressedForeColor = TextColor;
            }

            return button;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Location = new Point(
                _ownerBounds.Left + (_ownerBounds.Width - Width) / 2,
                _ownerBounds.Top + (_ownerBounds.Height - Height) / 2);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            switch (_icon)
            {
                case MessageBoxIcon.Error: SystemSounds.Hand.Play(); break;
                case MessageBoxIcon.Warning: SystemSounds.Exclamation.Play(); break;
                case MessageBoxIcon.Information: SystemSounds.Asterisk.Play(); break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Closed without a button (Alt+F4, etc.): behave like the escape button, or refuse if none.
            if (e.CloseReason == CloseReason.UserClosing && _clicked == null && !_dismissed)
            {
                if (_escapeItem != null)
                {
                    _clicked = _escapeItem;
                    DialogResult = ResultOf(_escapeItem);
                    _escapeItem.OnClick?.Invoke();
                }
                else if (_showClose)
                {
                    _dismissed = true;
                    DialogResult = DialogResult.Cancel;
                }
                else e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    Dismiss();
                    return true;

                case Keys.Enter:
                    for (Control c = ActiveControl; c != null; c = c.Parent)
                    {
                        if (_buttonMap.TryGetValue(c, out var focusedItem))
                        {
                            ActivateButton(focusedItem);
                            return true;
                        }
                    }
                    if (_defaultControl != null) ActivateButton(_buttonMap[_defaultControl]);
                    return true;

                case Keys.Control | Keys.C:
                    CopyToClipboard();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void CopyToClipboard()
        {
            const string line = "---------------------------";
            var sb = new StringBuilder();
            sb.AppendLine(line).AppendLine(_caption)
              .AppendLine(line).AppendLine(_text)
              .AppendLine(line);
            foreach (var item in _items)
                sb.Append(item.Text).Append("   ");
            sb.AppendLine().AppendLine(line);

            try { Clipboard.SetText(sb.ToString()); }
            catch (ExternalException) { } // Clipboard busy.
        }

        // Dragging.
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private void BeginDrag(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
        }

        private sealed class GlyphIcon : Control
        {
            private readonly MessageBoxIcon _kind;
            private readonly Image _custom;

            public GlyphIcon(MessageBoxIcon kind, Image custom, int size)
            {
                _kind = kind;
                _custom = custom;
                Size = new Size(size, size);
                TabStop = false;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;

                // Custom image replaces drawn glyph.
                if (_custom != null)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    float ratio = Math.Min((float)Width / _custom.Width, (float)Height / _custom.Height);
                    float w = _custom.Width * ratio;
                    float h = _custom.Height * ratio;
                    g.DrawImage(_custom, (Width - w) / 2f, (Height - h) / 2f, w, h);
                    return;
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                float s = Math.Min(Width, Height);
                var box = new RectangleF(1, 1, s - 2, s - 2);
                float glyphCenterX = s / 2f;
                float glyphCenterY = s / 2f;
                string glyph = null;
                Color fill, glyphColor = Color.White;

                switch (_kind)
                {
                    case MessageBoxIcon.Error: fill = Color.FromArgb(214, 62, 62); break;
                    case MessageBoxIcon.Warning: fill = Color.FromArgb(255, 185, 0); glyph = "!"; glyphColor = Color.FromArgb(20, 20, 20); break;
                    case MessageBoxIcon.Information: fill = Color.FromArgb(56, 152, 255); glyph = "i"; break;
                    default: fill = Color.FromArgb(0, 188, 71); glyph = "?"; break;   // Question.
                }

                using (var brush = new SolidBrush(fill))
                {
                    if (_kind == MessageBoxIcon.Warning)
                    {
                        // Rounded triangle icon.
                        var tri = new[]
                        {
                            new PointF(s / 2f, s * 0.10f),
                            new PointF(s * 0.94f, s * 0.88f),
                            new PointF(s * 0.06f, s * 0.88f)
                        };
                        g.FillPolygon(brush, tri);
                        using (var pen = new Pen(fill, s * 0.10f) { LineJoin = LineJoin.Round })
                            g.DrawPolygon(pen, tri);
                        glyphCenterY = s * 0.62f; // Triangle's centroid: (0.10 + 0.88 + 0.88) / 3.
                    }
                    else
                    {
                        g.FillEllipse(brush, box);
                    }
                }

                if (_kind == MessageBoxIcon.Error)
                {
                    float m = s * 0.32f;
                    using (var pen = new Pen(Color.White, s * 0.09f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLine(pen, m, m, s - m, s - m);
                        g.DrawLine(pen, s - m, m, m, s - m);
                    }
                }
                else if (glyph != null)
                {
                    using (var family = new FontFamily("Segoe UI"))
                    using (var path = new GraphicsPath())
                    using (var brush = new SolidBrush(glyphColor))
                    {
                        path.AddString(glyph, family, (int)FontStyle.Bold, s * 0.52f, PointF.Empty, StringFormat.GenericTypographic);
                        RectangleF ink = path.GetBounds();
                        using (var move = new Matrix())
                        {
                            move.Translate(glyphCenterX - (ink.X + ink.Width / 2f), glyphCenterY - (ink.Y + ink.Height / 2f));
                            path.Transform(move);
                        }
                        g.FillPath(brush, path);
                    }
                }
            }
        }
    }
}