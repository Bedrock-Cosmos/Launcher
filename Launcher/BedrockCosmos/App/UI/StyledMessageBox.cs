using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BedrockCosmos.App.UI
{
    public sealed class StyledMessageBox : Form
    {
        private static readonly Color BackdropColor = Color.FromArgb(20, 20, 20);
        private static readonly Color BarColor      = Color.FromArgb(15, 15, 15);
        private static readonly Color BorderColor   = Color.FromArgb(60, 60, 60);
        private static readonly Color TextColor     = Color.FromArgb(153, 153, 153);
        private static readonly Color Accent        = Color.FromArgb(0, 188, 71);
        private static readonly Color AccentHover   = Color.FromArgb(0, 208, 80);
        public static string DefaultCaption { get; set; } = "Bedrock Cosmos";
        public static Func<DialogResult, string> ButtonTextProvider { get; set; } // For multi-language.

        public static DialogResult Show(
            string text,
            string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            return Show((IWin32Window)null, text, caption, buttons, icon, defaultButton);
        }

        public static DialogResult Show(
            IWin32Window owner,
            string text,
            string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            using (var dialog = new StyledMessageBox(owner, text, caption ?? DefaultCaption, buttons, icon, defaultButton))
            {
                return dialog.ShowDialog(owner);
            }
        }

        private readonly float _scale;
        private readonly string _text;
        private readonly string _caption;
        private readonly MessageBoxIcon _icon;
        private readonly DialogResult[] _results;
        private readonly DialogResult _escapeResult;
        private readonly Dictionary<Control, DialogResult> _buttonMap = new Dictionary<Control, DialogResult>();
        private readonly Control _defaultControl;
        private readonly Rectangle _ownerBounds;

        private int S(int value) => (int)Math.Round(value * _scale);

        private StyledMessageBox(IWin32Window owner, string text, string caption,
            MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            _scale = DeviceDpi / 96f;
            _text = text ?? string.Empty;
            _caption = caption ?? string.Empty;
            _icon = icon;
            _results = ResultsFor(buttons);

            if (Array.IndexOf(_results, DialogResult.Cancel) >= 0) _escapeResult = DialogResult.Cancel;
            else if (_results.Length == 1) _escapeResult = _results[0];
            else _escapeResult = DialogResult.None;

            var ownerForm = owner == null ? null : Control.FromHandle(owner.Handle) as Form;
            _ownerBounds = ownerForm != null && ownerForm.Visible && ownerForm.WindowState != FormWindowState.Minimized
                ? ownerForm.Bounds
                : Screen.FromPoint(Cursor.Position).WorkingArea;

            SuspendLayout();

            AutoScaleMode = AutoScaleMode.None; // Scaled manually via S()
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
                MinimumSize = new Size(S(360), 0),
                Padding = new Padding(1)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < 3; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            root.Controls.Add(BuildTitleBar(ownerForm), 0, 0);
            root.Controls.Add(BuildContent(), 0, 1);
            root.Controls.Add(BuildButtonBar(defaultButton, out _defaultControl), 0, 2);

            Controls.Add(root);
            ResumeLayout(true);

            if (_defaultControl != null) ActiveControl = _defaultControl;
        }

        private static DialogResult[] ResultsFor(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK:               return new[] { DialogResult.OK };
                case MessageBoxButtons.OKCancel:         return new[] { DialogResult.OK, DialogResult.Cancel };
                case MessageBoxButtons.AbortRetryIgnore: return new[] { DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore };
                case MessageBoxButtons.YesNoCancel:      return new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel };
                case MessageBoxButtons.YesNo:            return new[] { DialogResult.Yes, DialogResult.No };
                case MessageBoxButtons.RetryCancel:      return new[] { DialogResult.Retry, DialogResult.Cancel };
                default: throw new InvalidEnumArgumentException(nameof(buttons), (int)buttons, typeof(MessageBoxButtons));
            }
        }

        private Control BuildTitleBar(Form ownerForm)
        {
            var bar = new Panel
            {
                BackColor = BarColor,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Size = new Size(S(100), S(40))
            };

            var title = new Label
            {
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextColor,
                Text = _caption,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false
            };

            bar.Controls.Add(title);

            if (ownerForm != null && ownerForm.Icon != null)
            {
                var iconBox = new PictureBox
                {
                    Dock = DockStyle.Left,
                    Image = new Icon(ownerForm.Icon, new Size(S(20), S(20))).ToBitmap(),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Width = S(40)
                };
                iconBox.MouseDown += BeginDrag;
                bar.Controls.Add(iconBox);
            }
            else
            {
                title.Padding = new Padding(S(14), 0, 0, 0);
            }

            if (_escapeResult != DialogResult.None)
            {
                var close = new Label
                {
                    Cursor = Cursors.Hand,
                    Dock = DockStyle.Right,
                    Font = new Font("Segoe UI Symbol", 10F),
                    ForeColor = TextColor,
                    Text = "\u2715",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = S(46)
                };
                close.MouseEnter += (s, e) => { close.BackColor = Color.FromArgb(196, 43, 28); close.ForeColor = Color.White; };
                close.MouseLeave += (s, e) => { close.BackColor = Color.Transparent; close.ForeColor = TextColor; };
                close.Click += (s, e) => DialogResult = _escapeResult;
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
                Padding = new Padding(S(24)),
                WrapContents = false
            };

            bool hasGlyph = _icon != MessageBoxIcon.None;
            if (hasGlyph)
            {
                content.Controls.Add(new GlyphIcon(_icon, S(40)) { Margin = new Padding(0, 0, S(16), 0) });
            }

            content.Controls.Add(new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextColor,
                Margin = Padding.Empty,
                MaximumSize = new Size(S(440), 0),
                MinimumSize = new Size(0, hasGlyph ? S(40) : 0), // Stays centered on icon.
                Text = _text,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false
            });

            return content;
        }

        private Control BuildButtonBar(MessageBoxDefaultButton defaultButton, out Control defaultControl)
        {
            var bar = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BarColor,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = Padding.Empty,
                Padding = new Padding(S(12), S(12), S(6), S(12)),
                WrapContents = false
            };

            int defaultIndex = Math.Min((int)defaultButton / 256, _results.Length - 1);   // Button1/2/3 = 0/256/512
            defaultControl = null;

            // Right-To-Left flow.
            for (int i = _results.Length - 1; i >= 0; i--)
            {
                var result = _results[i];
                var button = CreateButton(result, primary: i == defaultIndex);
                button.TabIndex = i;
                button.Click += (s, e) => DialogResult = result;
                _buttonMap[button] = result;
                if (i == defaultIndex) defaultControl = button;
                bar.Controls.Add(button);
            }

            return bar;
        }

        private RoundButton CreateButton(DialogResult result, bool primary)
        {
            var secondaryBorder = Color.FromArgb(75, 75, 75);
            var secondaryFill = Color.FromArgb(30, 30, 30);

            var button = new RoundButton
            {
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12F),
                Margin = new Padding(S(6), 0, S(6), 0),
                Radius = 5,
                Size = new Size(S(112), S(40)),
                Text = ButtonTextProvider?.Invoke(result) ?? result.ToString()
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
                case MessageBoxIcon.Error:       SystemSounds.Hand.Play(); break;
                case MessageBoxIcon.Warning:     SystemSounds.Exclamation.Play(); break;
                case MessageBoxIcon.Information: SystemSounds.Asterisk.Play(); break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing
                && DialogResult == DialogResult.Cancel
                && _escapeResult != DialogResult.Cancel)
            {
                if (_escapeResult == DialogResult.None) e.Cancel = true;
                else DialogResult = _escapeResult;
            }
            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    if (_escapeResult != DialogResult.None) DialogResult = _escapeResult;
                    return true;

                case Keys.Enter:
                    for (Control c = ActiveControl; c != null; c = c.Parent)
                    {
                        if (_buttonMap.TryGetValue(c, out var focusedResult))
                        {
                            DialogResult = focusedResult;
                            return true;
                        }
                    }
                    if (_defaultControl != null) DialogResult = _buttonMap[_defaultControl];
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
            foreach (var r in _results)
                sb.Append(ButtonTextProvider?.Invoke(r) ?? r.ToString()).Append("   ");
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

            public GlyphIcon(MessageBoxIcon kind, int size)
            {
                _kind = kind;
                Size = new Size(size, size);
                TabStop = false;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                float s = Math.Min(Width, Height);
                var box = new RectangleF(1, 1, s - 2, s - 2);
                string glyph = null;
                Color fill, glyphColor = Color.White;

                switch (_kind)
                {
                    case MessageBoxIcon.Error:       fill = Color.FromArgb(214, 62, 62);  break;
                    case MessageBoxIcon.Warning:     fill = Color.FromArgb(255, 185, 0);  glyph = "!"; glyphColor = Color.FromArgb(20, 20, 20); break;
                    case MessageBoxIcon.Information: fill = Color.FromArgb(56, 152, 255); glyph = "i"; break;
                    default:                         fill = Color.FromArgb(0, 188, 71);   glyph = "?"; break;   // Question
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
                        box.Y += s * 0.10f;
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
                    using (var font = new Font("Segoe UI", s * 0.52f, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var brush = new SolidBrush(glyphColor))
                    using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(glyph, font, brush, box, format);
                }
            }
        }
    }
}