using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BedrockCosmos.App.UI;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.App
{
    public class Theme
    {
        public Color Background { get; set; } = Color.FromArgb(20, 20, 20); // Form, tab pages, scrollbar tracks, tree background.
        public Color BackgroundDeep { get; set; } = Color.FromArgb(15, 15, 15); // Title bar panel.
        public Color Surface { get; set; } = Color.FromArgb(30, 30, 30); // Button and combo box fill, tree scroll track.
        public Color Border { get; set; } = Color.FromArgb(60, 60, 60); // Tree item idle outline.
        public Color ButtonNormal { get; set; } = Color.FromArgb(75, 75, 75);
        public Color ButtonHover { get; set; } = Color.FromArgb(75, 75, 75);
        public Color ButtonPressed { get; set; } = Color.FromArgb(75, 75, 75);
        public Color SwitchOff { get; set; } = Color.FromArgb(75, 75, 75);
        public Color SwitchOn { get; set; } = Color.FromArgb(153, 153, 153);
        public Color SwitchKnob { get; set; } = Color.White;
        public Color ComboBorder { get; set; } = Color.FromArgb(153, 153, 153);
        public Color ComboArrow { get; set; } = Color.FromArgb(153, 153, 153);
        public Color ComboDivider { get; set; } = Color.FromArgb(30, 30, 30);
        public Color ComboDividerShadow { get; set; } = Color.FromArgb(30, 30, 30);
        public Color DropdownHover { get; set; } = Color.FromArgb(70, 70, 70);
        public Color DropdownHoverText { get; set; } = Color.WhiteSmoke;
        public Color DropdownHoverAccent { get; set; } = Color.FromArgb(153, 153, 153);
        public Color ScrollThumb { get; set; } = Color.FromArgb(75, 75, 75);
        public Color ScrollThumbHover { get; set; } = Color.FromArgb(110, 110, 110);
        public Color Text { get; set; } = Color.FromArgb(153, 153, 153); // Default app text.
        public Color TextOnAccent { get; set; } = Color.White; // Launch button text.
        public Color TextOnSelection { get; set; } = Color.FromArgb(20, 20, 20); // Selected tree header item texts.
        public Color Accent { get; set; } = Color.FromArgb(0, 188, 71); // Launch button, tree drop marker and enabled color.
        public Color AccentDark { get; set; } = Color.FromArgb(0, 114, 47);
        public Color Link { get; set; } = Color.LightSkyBlue;
        public Color LinkActive { get; set; } = Color.Cyan;
        public Color LinkVisited { get; set; } = Color.FromArgb(128, 128, 255);
        public Color ConsoleBackground { get; set; } = Color.Black;
        public Color ConsoleText { get; set; } = Color.White;
        public Color RarityCommon { get; set; } = Color.FromArgb(153, 153, 153);
        public Color RarityUncommon { get; set; } = Color.FromArgb(76, 175, 80);
        public Color RarityRare { get; set; } = Color.FromArgb(52, 120, 246);
        public Color RarityEpic { get; set; } = Color.FromArgb(163, 73, 214);
        public Color RarityLegendary { get; set; } = Color.FromArgb(240, 180, 30);
        public Color? IconColor { get; set; } = null;
        public static Theme Dark => new Theme();

        public Theme Clone() => (Theme)MemberwiseClone();
    }

    public class ThemeManager
    {
        private readonly Dictionary<Control, Image> _originals = new Dictionary<Control, Image>();
        private readonly Dictionary<Control, Image> _tinted = new Dictionary<Control, Image>();
        private readonly HashSet<Control> _noTint = new HashSet<Control>(); // Whitelisted from icon tinting.

        public Theme Current { get; private set; }
        public event EventHandler ThemeChanged;

        public void ExcludeFromIconTint(params Control[] controls)
        {
            foreach (Control c in controls)
                if (c != null) _noTint.Add(c);
        }

        public void Apply(Control root, Theme theme)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (theme == null) throw new ArgumentNullException(nameof(theme));

            Current = theme;

            root.SuspendLayout();
            try { ApplyRecursive(root, theme); }
            finally { root.ResumeLayout(true); }

            root.Invalidate(true); // Custom-drawn controls repaint with new colors.

            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyRecursive(Control c, Theme t)
        {
            ApplyToControl(c, t);
            foreach (Control child in c.Controls)
                ApplyRecursive(child, t);
        }

        private void ApplyToControl(Control c, Theme t)
        {
            // Order matters, a derived type must be tested before its base type.
            // Ex. LinkLabel : Label, TabPage : Panel
            switch (c)
            {
                case Form f:
                    f.BackColor = t.Background;
                    break;

                case RoundGradientButton gb:
                    gb.FilledBackColorTop = t.Accent;
                    gb.FilledBackColorBottom = t.AccentDark;
                    gb.NormalBackColor = t.Accent;
                    gb.HoverBackColor = t.Accent;
                    gb.HoverFillColor = t.Accent;
                    gb.PressedBackColor = t.Accent;
                    gb.ForeColor = t.TextOnAccent;
                    gb.HoverForeColor = t.TextOnAccent;
                    gb.PressedForeColor = t.TextOnAccent;
                    TintImage(gb, () => gb.ButtonImage, i => gb.ButtonImage = i, t.IconColor);
                    break;

                case RoundButton rb:
                    rb.FilledBackColor = t.Surface;
                    rb.NormalBackColor = t.ButtonNormal;
                    rb.HoverBackColor = t.ButtonHover;
                    rb.HoverFillColor = t.ButtonHover;
                    rb.PressedBackColor = t.ButtonPressed;
                    rb.ForeColor = t.Text;
                    rb.HoverForeColor = t.Text;
                    rb.PressedForeColor = t.Text;
                    TintImage(rb, () => rb.ButtonImage, i => rb.ButtonImage = i, t.IconColor);
                    break;

                case Switch sw:
                    sw.BaseColor = t.SwitchKnob;
                    sw.BaseOffColor = t.SwitchOff;
                    sw.BaseOnColor = t.SwitchOn;
                    break;

                case GradientComboBox cb:
                    cb.ForeColor = t.Text;
                    cb.ColorC = t.Surface;
                    cb.ColorD = t.Surface;
                    cb.ColorE = t.Surface;
                    cb.ColorF = t.ComboBorder;
                    cb.ColorG = t.ComboArrow;
                    cb.ColorH = t.ComboDivider;
                    cb.ColorI = t.ComboDividerShadow;
                    cb.HoverSelectionColor = t.DropdownHover;
                    cb.HoverTextColor = t.DropdownHoverText;
                    cb.HoverAccentColor = t.DropdownHoverAccent;
                    cb.ScrollThumbColor = t.ScrollThumb;
                    cb.ScrollThumbHoverColor = t.ScrollThumbHover;
                    cb.ScrollTrackColor = t.Background;
                    break;

                case ImageTreeView tv:
                    tv.BackColor = t.Background;
                    tv.IdleBackColor = t.Background;
                    tv.IdleBorderColor = t.Border;
                    tv.TrackColor = t.Surface;
                    tv.ThumbColor = t.ScrollThumb;
                    tv.ThumbHoverColor = t.ScrollThumbHover;
                    tv.HeaderTextColor = t.Text;
                    tv.ItemTitleColor = t.Text;
                    tv.SelectedBackColor = t.Text;
                    tv.SelectedHeaderTextColor = t.TextOnSelection;
                    tv.DropIndicatorColor = t.Accent;
                    tv.EnabledHighlightColor = t.Accent;
                    tv.CommonRarityColor = t.RarityCommon;
                    tv.UncommonRarityColor = t.RarityUncommon;
                    tv.RareRarityColor = t.RarityRare;
                    tv.EpicRarityColor = t.RarityEpic;
                    tv.LegendaryRarityColor = t.RarityLegendary;
                    break;

                case LinkLabel ll:
                    ll.LinkColor = t.Link;
                    ll.ActiveLinkColor = t.LinkActive;
                    ll.VisitedLinkColor = t.LinkVisited;
                    ll.ForeColor = t.Text;
                    break;

                case Label l:
                    l.ForeColor = t.Text;
                    break;

                case Button b:
                    // Handling for transparent vs colored button backgrounds.
                    bool inTitleBar = b.Parent != null && b.Parent.GetType() == typeof(Panel);
                    Color edge = inTitleBar ? t.BackgroundDeep : t.Background;
                    b.FlatAppearance.BorderColor = edge;
                    if (inTitleBar)
                    {
                        b.FlatAppearance.MouseOverBackColor = t.BackgroundDeep;
                        b.FlatAppearance.MouseDownBackColor = t.BackgroundDeep;
                    }
                    TintImage(b, () => b.BackgroundImage, i => b.BackgroundImage = i, t.IconColor);
                    break;

                case PictureBox pb:
                    TintImage(pb, () => pb.BackgroundImage, i => pb.BackgroundImage = i, t.IconColor);
                    break;

                case TabPage tp: // Must come before panel, ignores transparent tab control.
                    if (tp.BackColor.A != 0)
                        tp.BackColor = t.Background;
                    break;

                case RichTextBox rtb:
                    rtb.BackColor = t.ConsoleBackground;
                    rtb.ForeColor = t.ConsoleText;
                    break;

                case Panel p: // Title bar.
                    p.BackColor = t.BackgroundDeep;
                    break;
            }
        }

        private void TintImage(Control c, Func<Image> get, Action<Image> set, Color? color)
        {
            if (_noTint.Contains(c)) return;

            Image current = get();
            Image original;
            Image mine;

            if (_originals.TryGetValue(c, out original))
            {
                _tinted.TryGetValue(c, out mine);
                if (!ReferenceEquals(current, original) && !ReferenceEquals(current, mine))
                {
                    // Stops running if image was swapped at runtime (a preview).
                    Forget(c);
                    _noTint.Add(c);
                    return;
                }
            }
            else
            {
                if (current == null)
                {
                    _noTint.Add(c);
                    return;
                }
                original = current;
                _originals[c] = original;
                c.Disposed += (s, e) => Forget(c);
            }

            Image previousTint;
            _tinted.TryGetValue(c, out previousTint);

            Image next = null;
            if (color.HasValue)
            {
                next = IconTinter.Tint(original, color.Value);
                _tinted[c] = next;
            }
            else
            {
                _tinted.Remove(c);
            }

            // Swap then free old copy.
            set(next ?? original);
            previousTint?.Dispose();
        }

        private void Forget(Control c)
        {
            Image tint;
            if (_tinted.TryGetValue(c, out tint))
            {
                _tinted.Remove(c);
                tint.Dispose();
            }
            _originals.Remove(c);
        }
    }

    public static class IconTinter
    {
        public static Bitmap Tint(Image source, Color color)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            int w = source.Width, h = source.Height;
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            result.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            float r = color.R / 255f, g = color.G / 255f, b = color.B / 255f, a = color.A / 255f;

            var matrix = new ColorMatrix(new[]
            {
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, a, 0 },
                new float[] { r, g, b, 0, 1 }
            });

            using (var attrs = new ImageAttributes())
            using (Graphics gfx = Graphics.FromImage(result))
            {
                attrs.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfx.CompositingMode = CompositingMode.SourceCopy;
                gfx.DrawImage(source, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel, attrs);
            }
            return result;
        }
    }
}