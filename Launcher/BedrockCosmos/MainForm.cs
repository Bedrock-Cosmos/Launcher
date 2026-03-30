using AutoUpdaterDotNET;
using BedrockCosmos.App;
using BedrockCosmos.App.UI;
using BedrockCosmos.Proxy;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

namespace BedrockCosmos
{
    public partial class MainForm : Form
    {
        private const int SettingsLeftMargin = 20;
        private const int SettingsRightMargin = 24;
        private const int SettingsTopMargin = 30;
        private const int SettingsControlGap = 34;
        private const int SettingsRowGap = 28;
        private const int SettingsSectionGap = 34;
        private const int SettingsTitleGap = 8;
        private const int SettingsButtonGap = 14;
        private const int SettingsBottomMargin = 24;

        private readonly LaunchManager launchManager;
        private readonly ProxyController controller;
        private readonly AsyncFileDownload asyncDownload;
        private readonly ProxyLifecycleManager proxyLifecycleManager;
        private readonly ProxyStartupRecoveryResult startupRecoveryResult;

        private readonly Label proxyManagementTitleLabel = new Label();
        private readonly Label proxyManagementDescriptionLabel = new Label();
        private readonly RoundButton repairInternetSettingsButton = new RoundButton();
        private readonly RoundButton removeProxySettingsButton = new RoundButton();
        private bool suppressLanguageSelectionChanged;

        // For window movement
        private bool drag = false;
        private Point startPoint = new Point(0, 0);

        public MainForm(ProxyStartupRecoveryResult startupRecoveryResult)
        {
            this.startupRecoveryResult = startupRecoveryResult ?? ProxyStartupRecoveryResult.NoRecovery();
            InitializeComponent();
            InitializeProxyManagementControls();
            ApplySettingsVisualStyle();

            CosmosConsole.Initialize(DevConsole);
            DiscordRichPresence.InitializeRpc();
            launchManager = new LaunchManager();
            controller = new ProxyController();
            asyncDownload = new AsyncFileDownload();
            proxyLifecycleManager = ProxyLifecycleFactory.CreateDefault();
            proxyLifecycleManager.ProxyHealthCheckFailed += ProxyLifecycleManager_ProxyHealthCheckFailed;

            // Will also log messages to the main console if uncommented.
            //CosmosConsole.LogToMainConsole = true;
            launchManager.InitializeMgrAsyncFileDownload(asyncDownload);
            launchManager.InitializeMgrLaunchButton(LaunchButton);
            launchManager.InitializeMgrVersionLabel(VersionLabel);
            launchManager.SetCurrentVersions();

            ApplySettings();
            UpdateLauncherLanguage();
            DiscordRichPresence.UpdatePresence();
            Shown += MainForm_Shown;
        }

        private void InitializeProxyManagementControls()
        {
            SettingsPage.AutoScroll = true;
            SettingsPage.Resize += SettingsPage_Resize;

            proxyManagementTitleLabel.ForeColor = Color.FromArgb(153, 153, 153);
            proxyManagementTitleLabel.TextAlign = ContentAlignment.TopLeft;

            proxyManagementDescriptionLabel.ForeColor = Color.FromArgb(153, 153, 153);
            proxyManagementDescriptionLabel.TextAlign = ContentAlignment.TopLeft;

            ConfigureSecondaryButton(repairInternetSettingsButton, RepairInternetSettingsButton_Click);
            ConfigureSecondaryButton(removeProxySettingsButton, RemoveProxySettingsButton_Click);

            SettingsPage.Controls.Add(proxyManagementTitleLabel);
            SettingsPage.Controls.Add(proxyManagementDescriptionLabel);
            SettingsPage.Controls.Add(repairInternetSettingsButton);
            SettingsPage.Controls.Add(removeProxySettingsButton);
        }

        private static void ConfigureSecondaryButton(RoundButton button, EventHandler onClick)
        {
            button.BackColor = Color.Transparent;
            button.Cursor = Cursors.Hand;
            button.DialogResult = DialogResult.None;
            button.FilledBackColor = Color.FromArgb(30, 30, 30);
            button.Font = new Font("Segoe UI", 10F);
            button.ForeColor = Color.FromArgb(153, 153, 153);
            button.HoverBackColor = Color.FromArgb(75, 75, 75);
            button.HoverFillColor = Color.FromArgb(75, 75, 75);
            button.HoverForeColor = Color.FromArgb(153, 153, 153);
            button.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            button.MinimumSize = new Size(190, 47);
            button.Name = Guid.NewGuid().ToString("N");
            button.NormalBackColor = Color.FromArgb(75, 75, 75);
            button.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            button.PressedBackColor = Color.FromArgb(75, 75, 75);
            button.PressedForeColor = Color.FromArgb(153, 153, 153);
            button.Radius = 5;
            button.Size = new Size(190, 47);
            button.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            button.Click += onClick;
        }

        private void ApplySettingsVisualStyle()
        {
            Font settingsTitleFont = new Font("Segoe UI Semibold", 14F, FontStyle.Regular);
            Font settingsDescriptionFont = new Font("Segoe UI", 10F, FontStyle.Regular);

            BackgroundModeTitleLabel.Font = settingsTitleFont;
            LanguageTitleLabel.Font = settingsTitleFont;
            proxyManagementTitleLabel.Font = settingsTitleFont;

            BackgroundModeDescriptionLabel.Font = settingsDescriptionFont;
            LanguageDescriptionLabel.Font = settingsDescriptionFont;
            proxyManagementDescriptionLabel.Font = settingsDescriptionFont;

            BackgroundModeTitleLabel.ForeColor = Color.FromArgb(182, 182, 182);
            LanguageTitleLabel.ForeColor = Color.FromArgb(182, 182, 182);
            proxyManagementTitleLabel.ForeColor = Color.FromArgb(182, 182, 182);

            BackgroundModeDescriptionLabel.ForeColor = Color.FromArgb(153, 153, 153);
            LanguageDescriptionLabel.ForeColor = Color.FromArgb(153, 153, 153);
            proxyManagementDescriptionLabel.ForeColor = Color.FromArgb(153, 153, 153);
        }

        private void SettingsPage_Resize(object sender, EventArgs e)
        {
            LayoutSettingsPage();
        }

        private void LayoutSettingsPage()
        {
            if (SettingsPage.ClientSize.Width <= 0)
                return;

            SettingsPage.SuspendLayout();

            try
            {
                BackgroundModeTitleLabel.TextAlign = ContentAlignment.TopLeft;
                BackgroundModeDescriptionLabel.TextAlign = ContentAlignment.TopLeft;
                LanguageTitleLabel.TextAlign = ContentAlignment.TopLeft;
                LanguageDescriptionLabel.TextAlign = ContentAlignment.TopLeft;

                LanguageComboBox.Width = CalculateLanguageComboBoxWidth();

                int controlColumnWidth = Math.Max(160, LanguageComboBox.Width);
                int textX = SettingsLeftMargin + controlColumnWidth + SettingsControlGap;
                int textWidth = Math.Max(280, SettingsPage.ClientSize.Width - textX - SettingsRightMargin);
                int y = SettingsTopMargin;

                y = LayoutSettingsRow(BackgroundModeSwitch, BackgroundModeTitleLabel, BackgroundModeDescriptionLabel, y, controlColumnWidth, textX, textWidth);
                y += SettingsRowGap;
                y = LayoutSettingsRow(LanguageComboBox, LanguageTitleLabel, LanguageDescriptionLabel, y, controlColumnWidth, textX, textWidth);
                y += SettingsSectionGap;
                y = LayoutProxyManagementSection(y, textX, textWidth);

                int backButtonY = Math.Max(y + 18, SettingsPage.ClientSize.Height - SettingsBackButton.Height - SettingsBottomMargin);
                SettingsBackButton.Location = new Point(15, backButtonY);
                SettingsPage.AutoScrollMinSize = new Size(0, backButtonY + SettingsBackButton.Height + SettingsBottomMargin);
            }
            finally
            {
                SettingsPage.ResumeLayout();
            }
        }

        private int LayoutSettingsRow(Control leadingControl, Label titleLabel, Label descriptionLabel, int top, int controlColumnWidth, int textX, int textWidth)
        {
            int titleHeight = MeasureTextHeight(titleLabel.Text, titleLabel.Font, textWidth);
            int descriptionY = top + titleHeight + SettingsTitleGap;
            int descriptionHeight = MeasureTextHeight(descriptionLabel.Text, descriptionLabel.Font, textWidth);
            int textBlockHeight = titleHeight + SettingsTitleGap + descriptionHeight;

            int controlX = SettingsLeftMargin + Math.Max(0, (controlColumnWidth - leadingControl.Width) / 2);
            int controlY = top + Math.Max(0, (textBlockHeight - leadingControl.Height) / 2);

            leadingControl.Location = new Point(controlX, controlY);
            titleLabel.SetBounds(textX, top, textWidth, titleHeight);
            descriptionLabel.SetBounds(textX, descriptionY, textWidth, descriptionHeight);

            return top + Math.Max(textBlockHeight, leadingControl.Height);
        }

        private int LayoutProxyManagementSection(int top, int contentX, int contentWidth)
        {
            int titleHeight = MeasureTextHeight(proxyManagementTitleLabel.Text, proxyManagementTitleLabel.Font, contentWidth);
            int descriptionY = top + titleHeight + SettingsTitleGap;
            int descriptionHeight = MeasureTextHeight(proxyManagementDescriptionLabel.Text, proxyManagementDescriptionLabel.Font, contentWidth);
            int buttonsY = descriptionY + descriptionHeight + 18;

            proxyManagementTitleLabel.SetBounds(contentX, top, contentWidth, titleHeight);
            proxyManagementDescriptionLabel.SetBounds(contentX, descriptionY, contentWidth, descriptionHeight);

            int preferredButtonWidth = Math.Max(
                MeasureButtonWidth(repairInternetSettingsButton),
                MeasureButtonWidth(removeProxySettingsButton));
            int maxRowButtonWidth = (contentWidth - SettingsButtonGap) / 2;
            bool stackButtons = preferredButtonWidth * 2 + SettingsButtonGap > contentWidth;

            if (stackButtons)
            {
                int buttonWidth = Math.Min(contentWidth, Math.Max(preferredButtonWidth, 260));
                int buttonX = contentX + Math.Max(0, (contentWidth - buttonWidth) / 2);

                int repairHeight = MeasureButtonHeight(repairInternetSettingsButton, buttonWidth);
                repairInternetSettingsButton.SetBounds(buttonX, buttonsY, buttonWidth, repairHeight);

                int removeHeight = MeasureButtonHeight(removeProxySettingsButton, buttonWidth);
                removeProxySettingsButton.SetBounds(buttonX, repairInternetSettingsButton.Bottom + SettingsButtonGap, buttonWidth, removeHeight);
            }
            else
            {
                int buttonWidth = Math.Min(maxRowButtonWidth, Math.Max(preferredButtonWidth, 220));
                int repairHeight = MeasureButtonHeight(repairInternetSettingsButton, buttonWidth);
                int removeHeight = MeasureButtonHeight(removeProxySettingsButton, buttonWidth);
                int buttonHeight = Math.Max(repairHeight, removeHeight);
                int totalButtonsWidth = (buttonWidth * 2) + SettingsButtonGap;
                int firstButtonX = contentX + Math.Max(0, (contentWidth - totalButtonsWidth) / 2);
                int secondButtonX = firstButtonX + buttonWidth + SettingsButtonGap;

                repairInternetSettingsButton.SetBounds(firstButtonX, buttonsY, buttonWidth, buttonHeight);
                removeProxySettingsButton.SetBounds(secondButtonX, buttonsY, buttonWidth, buttonHeight);
            }

            return Math.Max(repairInternetSettingsButton.Bottom, removeProxySettingsButton.Bottom);
        }

        private int CalculateLanguageComboBoxWidth()
        {
            int widestItem = 0;

            foreach (object item in LanguageComboBox.Items)
            {
                string itemText = item?.ToString() ?? string.Empty;
                widestItem = Math.Max(widestItem, TextRenderer.MeasureText(itemText, LanguageComboBox.Font).Width);
            }

            return Clamp(widestItem + 48, 160, 210);
        }

        private static int MeasureTextHeight(string text, Font font, int width)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Math.Max(font.Height + 4, 24);

            Size measured = TextRenderer.MeasureText(
                text,
                font,
                new Size(Math.Max(1, width), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            return Math.Max(font.Height + 4, measured.Height + 2);
        }

        private static int MeasureButtonHeight(RoundButton button, int width)
        {
            Size measured = TextRenderer.MeasureText(
                button.Text ?? string.Empty,
                button.Font,
                new Size(Math.Max(1, width - 28), int.MaxValue),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            return Math.Max(button.MinimumSize.Height, measured.Height + 26);
        }

        private static int MeasureButtonWidth(RoundButton button)
        {
            int measuredWidth = TextRenderer.MeasureText(button.Text ?? string.Empty, button.Font).Width;
            return Clamp(measuredWidth + 52, 220, 320);
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (startupRecoveryResult.RepairedSettings)
            {
                StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyFailedRestored");
                LocalizedMessageBox.ShowWarning(LanguageHandler.Get("Proxy.Recovery.StartupWarning"));
            }
        }

        private void ProxyLifecycleManager_ProxyHealthCheckFailed(object sender, ProxyHealthFailureEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, ProxyHealthFailureEventArgs>(ProxyLifecycleManager_ProxyHealthCheckFailed), sender, e);
                return;
            }

            try
            {
                if (controller.IsRunning)
                    controller.Stop();
            }
            catch
            {

            }

            SettingsManager.ProxyStarted = false;
            ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyFailedRestored"));
            CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.RuntimeFailureRestored"));

            if (!Visible)
                Show();

            WindowState = FormWindowState.Normal;
            TrayIcon.Visible = false;
            LocalizedMessageBox.ShowError(LanguageHandler.Get("Proxy.Errors.RuntimeStopped.Message"));
        }

        public void HandleIncomingArgs(string[] args)
        {
            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg)) continue;

                if (arg.StartsWith("bedrockcosmos://", StringComparison.OrdinalIgnoreCase))
                    HandleUri(arg);
                else if (arg.EndsWith(".bcpack", StringComparison.OrdinalIgnoreCase))
                    HandleBcPackFile(arg);
                else if (arg.EndsWith(".bcpersona", StringComparison.OrdinalIgnoreCase))
                    HandleBcPersonaFile(arg);
            }
        }

        private void HandleUri(string uri)
        {
            // For bedrockcosmos:// URIs
            string handledUri = UriHandler.Handle(uri);
            if (!SettingsManager.ProxyStarted && !SettingsManager.BackgroundMode)
                LaunchButton.PerformClick();

            Process.Start("minecraft://" + handledUri);
        }

        private void HandleBcPackFile(string filePath)
        {
            LocalizedMessageBox.ShowInfoWithTitle(
                LanguageHandler.Get("Files.BCPack.ComingSoon"),
                Path.GetFileName(filePath));
        }

        private void HandleBcPersonaFile(string filePath)
        {
            LocalizedMessageBox.ShowInfoWithTitle(
                LanguageHandler.Get("Files.BCPersona.ComingSoon"),
                Path.GetFileName(filePath));
        }

        private void ApplySettings()
        {
            suppressLanguageSelectionChanged = true;
            LanguageComboBox.Items.Clear();
            LanguageComboBox.Items.AddRange(LanguageHandler.GetAvailableLanguageNames().Cast<object>().ToArray());
            LanguageComboBox.SelectedItem = LanguageHandler.GetLanguageName(SettingsManager.Language);
            suppressLanguageSelectionChanged = false;

            BackgroundModeSwitch.Checked = SettingsManager.BackgroundMode;

            if (SettingsManager.DevMenuEnabled)
            {
                SettingsManager.DevMenuClicks = 7;
                AppIcon.Cursor = Cursors.Hand;
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DevModeEnabled"));
            }

            EnableLoggingSwitch.Checked = SettingsManager.EnableLogging;
            DetailedLoggingSwitch.Checked = SettingsManager.DetailedLogging;
            ApplyProxyStateUi(null);
        }

        private void ApplyProxyStateUi(string explicitStatus)
        {
            if (SettingsManager.ProxyStarted)
            {
                launchManager.UpdateLaunchButtonColor("purple");
                LaunchButton.Text = SettingsManager.BackgroundMode
                    ? LanguageHandler.Home_LaunchButton_Listening
                    : LanguageHandler.Home_LaunchButton_Running;
                LaunchButton.Enabled = !SettingsManager.BackgroundMode;
                StatusLabel.Text = explicitStatus ?? LanguageHandler.Get("Home.StatusLabel.ProxyActive");
                return;
            }

            launchManager.UpdateLaunchButtonColor("green");
            LaunchButton.Text = SettingsManager.BackgroundMode
                ? LanguageHandler.Home_LaunchButton_Listening
                : LanguageHandler.Home_LaunchButton_Launch;
            LaunchButton.Enabled = !SettingsManager.BackgroundMode;
            StatusLabel.Text = explicitStatus ?? LanguageHandler.Get("Home.StatusLabel.ProxyInactive");
        }

        private void ApplyProxyStartingUi()
        {
            launchManager.UpdateLaunchButtonColor("purple");
            LaunchButton.Enabled = false;
            LaunchButton.Text = LanguageHandler.Home_LaunchButton_Entering;
            StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyStarting");
        }

        private async Task<bool> StartProxyFlowAsync(bool openMinecraft)
        {
            LaunchButton.Enabled = false;
            ApplyProxyStartingUi();
            CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.Starting"));
            await launchManager.InternetCheck();

            if (launchManager.LatestLauncherVersion <= new Version("0.0.0.0"))
            {
                launchManager.ResetLaunchStatus();
                ApplyProxyStateUi(LanguageHandler.Home_StatusLabel_NoInternet);
                return false;
            }

            bool updateLauncher = launchManager.CheckLauncherUpdate();
            if (updateLauncher && !SettingsManager.LauncherUpdatePrompted)
            {
                SettingsManager.LauncherUpdatePrompted = true;
                TabControl.SelectedTab = UpdatePage;
                launchManager.ResetLaunchStatus();
                ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyInactive"));
                return false;
            }

            if ((launchManager.CheckResponsesUpdate() && !updateLauncher) ||
                !Directory.Exists(PathDefinitions.ResponsesDirectory))
            {
                await launchManager.UpdateResponses();
            }

            JsonData.InitializeJsons();

            try
            {
                await Task.Run(() => controller.StartProxy());
                ProxyApplyResult applyResult = await proxyLifecycleManager.ApplyProxyAsync(ProxyController.DefaultHost, ProxyController.DefaultPort);
                if (!applyResult.Applied)
                {
                    if (controller.IsRunning)
                        controller.Stop();

                    launchManager.ResetLaunchStatus();
                    ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyFailedRestored"));
                    CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.ReadinessFailed"));
                    LocalizedMessageBox.ShowError(LanguageHandler.Get("Proxy.Errors.NotReady.Message"));
                    return false;
                }

                SettingsManager.ProxyStarted = true;
                ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyActive"));
                CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.Started"));

                if (openMinecraft)
                    launchManager.OpenMinecraft();

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    proxyLifecycleManager.RestoreIfOwned("start_exception");
                }
                catch
                {

                }

                try
                {
                    if (controller.IsRunning)
                        controller.Stop();
                }
                catch
                {

                }

                SettingsManager.ProxyStarted = false;
                launchManager.ResetLaunchStatus();
                ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyFailedRestored"));
                CosmosConsole.WriteLine(LanguageHandler.Format("Proxy.Log.StartFailed", ex.Message));
                LocalizedMessageBox.ShowError(LanguageHandler.Format("Proxy.Errors.StartFailed.Message", ex.Message));
                return false;
            }
            finally
            {
                if (!SettingsManager.BackgroundMode)
                    LaunchButton.Enabled = true;
            }
        }

        private void StopProxyFlow(string reason, string statusKey = null)
        {
            CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.Stopping"));

            try
            {
                proxyLifecycleManager.RestoreIfOwned(reason);
            }
            catch (Exception ex)
            {
                CosmosConsole.WriteLine(LanguageHandler.Format("Proxy.Log.RestoreFailed", ex.Message));
            }

            try
            {
                if (controller.IsRunning)
                    controller.Stop();
            }
            catch (Exception ex)
            {
                CosmosConsole.WriteLine(LanguageHandler.Format("Proxy.Log.StopFailed", ex.Message));
            }

            SettingsManager.ProxyStarted = false;
            ApplyProxyStateUi(statusKey != null ? LanguageHandler.Get(statusKey) : LanguageHandler.Get("Home.StatusLabel.ProxyInactive"));
            CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.Stopped"));
        }

        private void RepairInternetSettingsButton_Click(object sender, EventArgs e)
        {
            if (SettingsManager.ProxyStarted)
            {
                StopProxyFlow("manual_repair_active", "Home.StatusLabel.ProxyInactive");
                LocalizedMessageBox.ShowInfo(LanguageHandler.Get("Proxy.Repair.Completed"));
                return;
            }

            ProxyRestoreResult restoreResult = proxyLifecycleManager.RestoreIfOwned("manual_repair");
            if (restoreResult.Restored)
            {
                ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyInactive"));
                LocalizedMessageBox.ShowInfo(LanguageHandler.Get("Proxy.Repair.Completed"));
            }
            else
            {
                LocalizedMessageBox.ShowInfo(LanguageHandler.Get("Proxy.Repair.NothingToRepair"));
            }
        }

        private void RemoveProxySettingsButton_Click(object sender, EventArgs e)
        {
            if (SettingsManager.ProxyStarted)
                StopProxyFlow("manual_remove");
            else
                proxyLifecycleManager.RestoreIfOwned("manual_remove");

            ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyInactive"));
            LocalizedMessageBox.ShowInfo(LanguageHandler.Get("Proxy.Remove.Completed"));
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            if (!SettingsManager.BackgroundMode)
            {
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                Hide();
                TrayIcon.Visible = true;
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SettingsManager.ProxyStarted)
                StopProxyFlow("clean_shutdown");

            try
            {
                asyncDownload.Dispose();
            }
            catch
            {

            }

            try
            {
                proxyLifecycleManager.Dispose();
            }
            catch
            {

            }

            try
            {
                controller.Dispose();
            }
            catch
            {

            }

            try
            {
                DiscordRichPresence.DisposeRpc();
            }
            catch
            {

            }
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            TrayIcon.Visible = false;
        }

        private void TopPanel_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void TopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }

        private void TopPanel_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }

        private async void LaunchButton_Click(object sender, EventArgs e)
        {
            if (!SettingsManager.ProxyStarted)
            {
                await StartProxyFlowAsync(true);
            }
            else
            {
                StopProxyFlow("manual_stop");
            }
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            TabControl.SelectedTab = AboutPage;
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            TabControl.SelectedTab = SettingsPage;
        }

        private void AppIcon_Click(object sender, EventArgs e)
        {
            bool isDevMenuEnabled = SettingsManager.DevMenuCheck();
            if (isDevMenuEnabled)
            {
                AppIcon.Cursor = Cursors.Default;
                TabControl.SelectedTab = DevPage;
            }
        }

        private void ReturnToHomeScreen(object sender, EventArgs e)
        {
            if (sender == DevBackButton)
                AppIcon.Cursor = Cursors.Hand;

            TabControl.SelectedTab = HomePage;
        }

        private void OpenDiscordLink(object sender, EventArgs e)
        {
            Process.Start("https://www.discord.com/invite/hBujWqu");
        }

        private void OpenGitHubLink(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Bedrock-Cosmos");
        }

        private void OpenWebsiteLink(object sender, EventArgs e)
        {
            Process.Start("https://bedrock-cosmos.app/");
        }

        private void BackgroundModeToggle_CheckedChanged(object sender, EventArgs e)
        {
            SettingsManager.BackgroundMode = BackgroundModeSwitch.Checked;

            if (SettingsManager.BackgroundMode)
            {
                BackgroundModeTimer.Start();
                ApplyProxyStateUi(SettingsManager.ProxyStarted
                    ? LanguageHandler.Get("Home.StatusLabel.ProxyActive")
                    : LanguageHandler.Get("Home.StatusLabel.ProxyInactive"));
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.BackgroundModeEnabled"));
            }
            else
            {
                BackgroundModeTimer.Stop();
                ApplyProxyStateUi(SettingsManager.ProxyStarted
                    ? LanguageHandler.Get("Home.StatusLabel.ProxyActive")
                    : LanguageHandler.Get("Home.StatusLabel.ProxyInactive"));
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.BackgroundModeDisabled"));
            }
        }

        private async void BackgroundModeTimer_Tick(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Minecraft.Windows");

            if (pname.Length != 0)
            {
                if (!SettingsManager.ProxyStarted)
                {
                    bool started = await StartProxyFlowAsync(false);
                    if (!started && TabControl.SelectedTab == UpdatePage)
                    {
                        Show();
                        WindowState = FormWindowState.Normal;
                        TrayIcon.Visible = false;
                    }
                }
            }
            else
            {
                if (SettingsManager.ProxyStarted)
                {
                    StopProxyFlow("background_minecraft_closed");
                }
            }
        }

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressLanguageSelectionChanged || LanguageComboBox.SelectedItem == null)
                return;

            string selectedLanguage = LanguageComboBox.SelectedItem.ToString();
            string langFile = LanguageHandler.GetLangFileName(selectedLanguage);
            SettingsManager.Language = langFile;

            LanguageHandler.Load(langFile);
            UpdateLauncherLanguage();
            DiscordRichPresence.UpdatePresence();
            CosmosConsole.WriteLine(LanguageHandler.Format("Localization.Log.LanguageSet", selectedLanguage));
        }

        private void UpdateLauncherLanguage()
        {
            TopLabel.Text = LanguageHandler.App_TopLabel_Name;
            Text = LanguageHandler.Get("App.WindowTitle");
            AccessibleName = LanguageHandler.Get("App.AccessibleName");
            TrayIcon.Text = LanguageHandler.Get("App.TrayIcon.Text");
            HomePage.Text = LanguageHandler.Get("Tabs.Home");
            AboutPage.Text = LanguageHandler.Get("Tabs.About");
            SettingsPage.Text = LanguageHandler.Get("Tabs.Settings");
            UpdatePage.Text = LanguageHandler.Get("Tabs.Update");
            DevPage.Text = LanguageHandler.Get("Tabs.Dev");
            AboutLabel.Text = LanguageHandler.About_AboutLabel_Text;
            DiscordLabel.Text = LanguageHandler.About_DiscordLabel_Text;
            GitHubLabel.Text = LanguageHandler.About_GitHubLabel_Text;
            WebsiteLabel.Text = LanguageHandler.About_WebsiteLabel_Text;
            BackgroundModeTitleLabel.Text = LanguageHandler.Settings_BackgroundMode_Title;
            BackgroundModeDescriptionLabel.Text = LanguageHandler.Settings_BackgroundMode_Description;
            LanguageTitleLabel.Text = LanguageHandler.Settings_Language_Title;
            LanguageDescriptionLabel.Text = LanguageHandler.Settings_Language_Description;
            proxyManagementTitleLabel.Text = LanguageHandler.Get("Settings.ProxyManagement.Title");
            proxyManagementDescriptionLabel.Text = LanguageHandler.Get("Settings.ProxyManagement.Description");
            repairInternetSettingsButton.Text = LanguageHandler.Get("Settings.ProxyManagement.RepairButton");
            removeProxySettingsButton.Text = LanguageHandler.Get("Settings.ProxyManagement.RemoveButton");
            UpdateLabel.Text = LanguageHandler.Update_UpdateLabel_Text;
            ChangelogLabel.Text = LanguageHandler.Update_ChangelogLabel_Text;
            UpdateButton.Text = LanguageHandler.Update_UpdateButton_Text;
            CancelUpdateButton.Text = LanguageHandler.Update_CancelUpdateButton_Text;
            DownloadZipButton.Text = LanguageHandler.Get("Dev.DownloadZipButton");
            ExportLogsButton.Text = LanguageHandler.Get("Dev.ExportLogsButton");
            ClearLogsButton.Text = LanguageHandler.Get("Dev.ClearLogsButton");
            FixProxyHangButton.Text = LanguageHandler.Get("Dev.FixProxyButton");
            ResetNewsButton.Text = LanguageHandler.Get("Dev.ResetNewsButton");
            DisableDevMenuButton.Text = LanguageHandler.Get("Dev.DisableDevMenuButton");
            EnableLoggingLabel.Text = LanguageHandler.Get("Dev.EnableLoggingLabel");
            DetailedLoggingLabel.Text = LanguageHandler.Get("Dev.DetailedLoggingLabel");
            DownloadZipProgressLabel.Text = LanguageHandler.Get("Dev.DownloadIdle");
            LayoutSettingsPage();
            ApplyProxyStateUi(null);
        }

        private async void DownloadZipButton_Click(object sender, EventArgs e)
        {
            string fileUrl = "https://github.com/Bedrock-Cosmos/Responses/archive/refs/heads/main.zip";
            string downloadPath = PathDefinitions.CosmosAppData + @"main.zip";
            string extractPath = PathDefinitions.CosmosAppData;

            DownloadZipButton.Enabled = false;
            DownloadZipProgressLabel.Visible = true;

            try
            {
                DownloadZipProgressLabel.Text = LanguageHandler.Get("Dev.DownloadInProgress");
                await asyncDownload.DownloadFileAsync(fileUrl, downloadPath);

                DownloadZipProgressLabel.Text = LanguageHandler.Get("Dev.ExtractInProgress");
                await asyncDownload.ExtractFileAsync(downloadPath, extractPath, true);

                if (Directory.Exists(PathDefinitions.ResponsesDirectory))
                {
                    await Task.Run(() =>
                    {
                        Directory.Delete(PathDefinitions.ResponsesDirectory, true);
                    });
                } 

                Directory.Move(PathDefinitions.CosmosAppData + "Responses-main", PathDefinitions.ResponsesDirectory);
                DownloadZipButton.Enabled = true;
                DownloadZipProgressLabel.Text = LanguageHandler.Get("Dev.DownloadCompleted");
            }
            catch (Exception)
            {
                DownloadZipButton.Enabled = true;
                DownloadZipProgressLabel.Text = LanguageHandler.Get("Dev.DownloadFailed");
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DownloadCanceled"));
            }
        }

        private void EnableLoggingSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableLoggingSwitch.Checked)
            {
                SettingsManager.EnableLogging = true;
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.LoggingEnabled"));
            }
            else
            {
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.LoggingDisabled"));
                SettingsManager.EnableLogging = false;
            }
        }

        private void DetailedLoggingSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (DetailedLoggingSwitch.Checked)
            {
                SettingsManager.DetailedLogging = true;
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DetailedLoggingEnabled"));
            }
            else
            {
                SettingsManager.DetailedLogging = false;
                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DetailedLoggingDisabled"));
            }
        }

        private void ExportLogsButton_Click(object sender, EventArgs e)
        {
            CosmosConsole.ExportLogs();
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            DevConsole.Text = "";
        }

        private void FixProxyHangButton_Click(object sender, EventArgs e)
        {
            if (!SettingsManager.ProxyStarted)
            {
                ProxyRestoreResult restoreResult = proxyLifecycleManager.RestoreIfOwned("dev_fix_proxy");
                CosmosConsole.WriteLine(restoreResult.Restored
                    ? LanguageHandler.Get("Proxy.Log.ManualRepairCompleted")
                    : LanguageHandler.Get("Proxy.Log.ManualRepairNotNeeded"));
            }
        }

        private void ResetNewsButton_Click(object sender, EventArgs e)
        {
            //NewsManager.RetrieveNewsHistory();
            //NewsManager.RetrieveCurrentNews();
            //NewsManager.CheckForNews();
        }

        private void DisableDevMenuButton_Click(object sender, EventArgs e)
        {
            SettingsManager.DisableDevMenu();
            TabControl.SelectedTab = HomePage;
        }

        private void ChangelogLabel_Click(object sender, EventArgs e)
        {
            Process.Start("https://bedrock-cosmos.app/changelogs/");
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            UpdateButton.Enabled = false;
            CancelUpdateButton.Enabled = false;
            if (SettingsManager.BackgroundMode)
                BackgroundModeTimer.Stop();

            try
            {
                //AutoUpdater.ReportErrors = true;
                AutoUpdater.Mandatory = true;
                AutoUpdater.UpdateMode = Mode.ForcedDownload;
                AutoUpdater.Start("https://raw.githubusercontent.com/Bedrock-Cosmos/Website/refs/heads/main/CurrentVersion.xml");
            }
            catch (Exception)
            {
                StatusLabel.Text = LanguageHandler.Home_StatusLabel_NoInternet;
                TabControl.SelectedTab = HomePage;

                SettingsManager.LauncherUpdatePrompted = false;
                UpdateButton.Enabled = true;
                CancelUpdateButton.Enabled = true;
                CloseButton.Enabled = true;
                if (SettingsManager.BackgroundMode)
                    BackgroundModeTimer.Start();
            }
        }

        private void CancelUpdateButton_Click(object sender, EventArgs e)
        {
            TabControl.SelectedTab = HomePage;
        }
    }
}
