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
        private LaunchManager launchManager;
        private static ProxyController controller;
        private readonly AsyncFileOperations asyncFileOps;

        // For window movement
        bool drag = false;
        Point startPoint = new Point(0, 0);

        public MainForm()
        {
            InitializeComponent();

            if (!Directory.Exists(PathDefinitions.CosmosAppData))
                Directory.CreateDirectory(PathDefinitions.CosmosAppData);

            CosmosConsole.Initialize(DevConsole);
            DiscordRichPresence.InitializeRpc();
            DiscordRichPresence.UpdatePresence();
            launchManager = new LaunchManager();
            controller = new ProxyController();
            asyncFileOps = new AsyncFileOperations();
            StatusLabel.Text = "";

            // Will also log messages to the main console if uncommented.
            //CosmosConsole.LogToMainConsole = true;
            launchManager.InitializeMgrAsyncFileOps(asyncFileOps);
            launchManager.InitializeMgrLaunchButton(LaunchButton);
            launchManager.InitializeMgrVersionLabel(VersionLabel);
            launchManager.SetCurrentVersions();

            LanguageHandler.Load(PathDefinitions.AppDirectory + @"Texts\" + SettingsManager.Language + ".lang");
            SettingsManager.LoadSettings();
            ApplySettings();

            if (File.Exists(PathDefinitions.AppDirectory + @"Background.png"))
                ApplyLauncherBackground(PathDefinitions.AppDirectory + @"Background.png");
        }

        private void ApplyLauncherBackground(string imagePath)
        {
            Image background = Image.FromFile(imagePath);
            HomePage.BackgroundImage = background;
            AboutPage.BackgroundImage = background;
            SettingsPage.BackgroundImage = background;
            UpdatePage.BackgroundImage = background;
            DevPage.BackgroundImage = background;
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
                    controller.Stop();
                }
                catch
                {

                }

                SettingsManager.ProxyStarted = false;
                launchManager.ResetLaunchStatus();
                ApplyProxyStateUi(LanguageHandler.Get("Home.StatusLabel.ProxyInactive"));
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

        private void StopProxyFlow(string statusKey = null)
        {
            CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.Stopping"));

            try
            {
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
                StopProxyFlow();

            try
            {
                asyncFileOps.Dispose();
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
                StopProxyFlow();
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
            Process.Start("https://discord.gg/HRG2NapegP");
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
                    StopProxyFlow();
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
                await asyncFileOps.DownloadFileAsync(fileUrl, downloadPath);

                DownloadZipProgressLabel.Text = LanguageHandler.Get("Dev.ExtractInProgress");
                await asyncFileOps.ExtractFileAsync(downloadPath, extractPath, true);

                if (Directory.Exists(PathDefinitions.ResponsesDirectory))
                {
                    await Task.Run(() =>
                    {
                        Directory.Delete(PathDefinitions.ResponsesDirectory, true);
                    });
                }

                if (!Directory.Exists(PathDefinitions.ResponsesDirectory))
                    Directory.Move(PathDefinitions.CosmosAppData + "Responses-main", PathDefinitions.ResponsesDirectory);
                else // Workaround if old directory was not deleted due to accessing elsewhere
                    await asyncFileOps.MoveFolderContentsAsync(PathDefinitions.CosmosAppData + "Responses-main", PathDefinitions.ResponsesDirectory, true);

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
                try
                {
                    controller.StartProxy();
                    controller.Stop();
                    CosmosConsole.WriteLine(LanguageHandler.Get("Proxy.Log.Reset"));
                }
                catch (Exception ex)
                {
                    CosmosConsole.WriteLine(LanguageHandler.Format("Proxy.Log.StopFailed", ex.Message));
                }
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
