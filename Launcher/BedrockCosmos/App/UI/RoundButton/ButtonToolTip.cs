using System;
using System.Drawing;
using System.Windows.Forms;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.App.UI
{
    internal sealed class ButtonToolTip : ToolTip
    {
        private static readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private static readonly Color BorderColor = Color.FromArgb(60, 60, 60);
        private static readonly Color TextColor = Color.FromArgb(153, 153, 153);

        private const int PaddingX = 8;
        private const int PaddingY = 5;
        private const int MaxTextWidth = 320;
        private const double MaxDelaySeconds = 32.0;

        private const TextFormatFlags TextFlags =
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.WordBreak |
            TextFormatFlags.NoPrefix;

        private const int ShowDurationMs = 15000;
        private const int CursorOffsetY = 22;

        private readonly Font _font = new Font("Segoe UI", 9F);
        private readonly Timer _showTimer = new Timer();
        private Control _hoverOwner;
        private string _text = string.Empty;
        private bool _enabled;
        private bool _shown;

        public ButtonToolTip()
        {
            OwnerDraw = true;
            ShowAlways = true;
            UseFading = false;
            UseAnimation = false;
            AutoPopDelay = 15000;
            Popup += OnPopup;
            Draw += OnDraw;
            _showTimer.Tick += OnShowTimerTick;
        }

        public void Apply(Control owner, bool show, string text, double delaySeconds)
        {
            if (double.IsNaN(delaySeconds) || delaySeconds < 0)
                delaySeconds = 0;
            if (delaySeconds > MaxDelaySeconds)
                delaySeconds = MaxDelaySeconds;

            // 0 ms is treated as "use the system default" by some versions of native control, so use 1 ms is effectively instant tooltip.
            InitialDelay = Math.Max(1, (int)Math.Round(delaySeconds * 1000.0));

            _text = text ?? string.Empty;
            _enabled = show && !string.IsNullOrWhiteSpace(_text);

            EndHover();
        }

        public void BeginHover(Control owner)
        {
            if (!_enabled || owner == null)
                return;

            EndHover();
            _hoverOwner = owner;
            _showTimer.Interval = InitialDelay;
            _showTimer.Start();
        }

        public void EndHover()
        {
            _showTimer.Stop();

            Control owner = _hoverOwner;
            _hoverOwner = null;

            if (_shown && owner != null && !owner.IsDisposed)
                Hide(owner);

            _shown = false;
        }

        private void OnShowTimerTick(object sender, EventArgs e)
        {
            _showTimer.Stop();

            Control owner = _hoverOwner;
            if (!_enabled || owner == null || owner.IsDisposed || !owner.IsHandleCreated ||
                !owner.Enabled || !owner.Visible)
                return;

            // Only show if the mouse is still over the button.
            Point mouse = owner.PointToClient(Cursor.Position);
            if (!owner.ClientRectangle.Contains(mouse))
                return;

            int x = mouse.X;
            int y = mouse.Y + (int)(CursorOffsetY * owner.DeviceDpi / 96f);

            // Keep the tooltip on screen: flip above mouse / shift left when it would overflow.
            Size size = MeasureTip();
            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Point screenPos = owner.PointToScreen(new Point(x, y));

            if (screenPos.Y + size.Height > area.Bottom)
                y = mouse.Y - size.Height - 4;
            if (screenPos.X + size.Width > area.Right)
                x -= (screenPos.X + size.Width) - area.Right;

            Show(_text, owner, x, y, ShowDurationMs);
            _shown = true;
        }

        private Size MeasureTip()
        {
            Size measured = TextRenderer.MeasureText(_text, _font, new Size(MaxTextWidth, 0), TextFlags);
            return new Size(measured.Width + PaddingX * 2, measured.Height + PaddingY * 2);
        }

        private void OnPopup(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = MeasureTip();
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            Rectangle bounds = e.Bounds;

            using (var back = new SolidBrush(BackgroundColor))
                e.Graphics.FillRectangle(back, bounds);

            using (var pen = new Pen(BorderColor))
                e.Graphics.DrawRectangle(pen, 0, 0, bounds.Width - 1, bounds.Height - 1);

            var textBounds = new Rectangle(
                PaddingX, PaddingY,
                bounds.Width - PaddingX * 2,
                bounds.Height - PaddingY * 2);

            TextRenderer.DrawText(e.Graphics, e.ToolTipText, _font, textBounds, TextColor, TextFlags);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _showTimer.Stop();
                _showTimer.Dispose();
                _font.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}