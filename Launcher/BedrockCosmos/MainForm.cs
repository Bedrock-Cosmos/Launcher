using AutoUpdaterDotNET;
using BedrockCosmos.App;
using BedrockCosmos.Proxy;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

            LanguageHandler.AddLangsToComboBox(LanguageComboBox);
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
            // For .bcpack files
            MessageBox.Show(
                "Support for BCPack files is coming soon!",
                Path.GetFileName(filePath),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void HandleBcPersonaFile(string filePath)
        {
            // For .bcpersona files
            MessageBox.Show(
                "Support for BCPersona files is coming soon!",
                Path.GetFileName(filePath),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ApplySettings()
        {
            // Language
            LanguageComboBox.SelectedItem = LanguageHandler.GetLanguageName(SettingsManager.Language);

            // Background Mode
            BackgroundModeSwitch.Checked = SettingsManager.BackgroundMode;

            // Discord RPC
            DiscordRichPresenceSwitch.Checked = SettingsManager.DiscordRpc;

            // Dev Menu
            if (SettingsManager.DevMenuEnabled)
            {
                SettingsManager.DevMenuClicks = 7;
                AppIcon.Cursor = Cursors.Hand;
                CosmosConsole.WriteLine("Developer mode enabled.");
            }

            // Logging
            EnableLoggingSwitch.Checked = SettingsManager.EnableLogging;

            // Detailed Logs
            DetailedLoggingSwitch.Checked = SettingsManager.DetailedLogging;
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
            {
                try { controller.Stop(); } catch { }
                try { controller.Dispose(); } catch { }
                try { controller = null; } catch { }
            }

            try { DiscordRichPresence.DisposeRpc(); } catch { }
            try { asyncFileOps.Dispose(); } catch { }
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
                LaunchButton.Enabled = false;
                StatusLabel.Text = "";

                CosmosConsole.WriteLine("Starting proxy...");
                LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Entering");
                launchManager.UpdateLaunchButtonColor("purple");
                StartLaunch();

                LaunchButton.Enabled = true;
            }
            else
            {
                CosmosConsole.WriteLine("Stopping proxy...");
                SettingsManager.ProxyStarted = false;

                launchManager.ResetLaunchStatus();
                controller.Stop();

                CosmosConsole.WriteLine("Proxy stopped!");
            }
        }

        private async void StartLaunch(bool backgroundMode = false)
        {
            await launchManager.InternetCheck();

            if (launchManager.LatestLauncherVersion > new Version("0.0.0.0"))
            {
                bool updateLauncher = launchManager.CheckLauncherUpdate();

                if (updateLauncher && !SettingsManager.LauncherUpdatePrompted)
                {
                    SettingsManager.LauncherUpdatePrompted = true;
                    TabControl.SelectedTab = UpdatePage;
                    if (!backgroundMode)
                    {
                        launchManager.ResetLaunchStatus();
                    }
                    else
                    {
                        Show(); // Launcher pops back up if there is an update while in background mode.
                        WindowState = FormWindowState.Normal;
                        TrayIcon.Visible = false;
                    }
                }
                else if (!backgroundMode || (backgroundMode && TabControl.SelectedTab != UpdatePage))
                {
                    if (launchManager.CheckResponsesUpdate() && !updateLauncher ||
                        !Directory.Exists(PathDefinitions.ResponsesDirectory))
                        await launchManager.UpdateResponses();

                    JsonData.InitializeJsons();
                    SettingsManager.ProxyStarted = true;

                    await Task.Run(() =>
                    {
                        controller.StartProxy();
                    });

                    if (!backgroundMode) // Different button/label texts depending on current mode.
                    {
                        LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Running");
                        launchManager.OpenMinecraft();
                    }
                    else
                    {
                        launchManager.UpdateLaunchButtonColor("purple");
                        LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Listening");
                        StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyEnabled");
                    }
                    CosmosConsole.WriteLine("Proxy started!");
                }
            }
            else
            {
                if (!backgroundMode)
                    launchManager.ResetLaunchStatus();

                StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.NoInternet");
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
                LaunchButton.Enabled = false;
                LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Listening");

                if (SettingsManager.ProxyStarted)
                    StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyEnabled");
                else
                    StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyDisabled");

                BackgroundModeTimer.Start();
                CosmosConsole.WriteLine("Background Mode enabled.");
            }
            else
            {
                LaunchButton.Enabled = true;

                if (SettingsManager.ProxyStarted)
                    LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Running");
                else
                    LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Launch");

                StatusLabel.Text = "";
                BackgroundModeTimer.Stop();
                CosmosConsole.WriteLine("Background Mode disabled.");
            }
        }

        private void DiscordRichPresenceSwitch_CheckedChanged(object sender, EventArgs e)
        {
            SettingsManager.DiscordRpc = DiscordRichPresenceSwitch.Checked;

            if (SettingsManager.DiscordRpc)
            {
                try
                {
                    DiscordRichPresence.InitializeRpc();
                    DiscordRichPresence.UpdatePresence();
                }
                catch
                {

                }
            }
            else
            {
                try { DiscordRichPresence.DisposeRpc(); } catch { }
            }
        }

        private async void BackgroundModeTimer_Tick(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Minecraft.Windows");

            if (pname.Length != 0)
            {
                if (!SettingsManager.ProxyStarted)
                {
                    StartLaunch(true);
                }
                //CosmosConsole.WriteLine("Minecraft is open.");
            }
            else
            {
                if (SettingsManager.ProxyStarted)
                {
                    SettingsManager.ProxyStarted = false;
                    launchManager.UpdateLaunchButtonColor("green");
                    StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyDisabled");
                    controller.Stop();
                    CosmosConsole.WriteLine("Proxy stopped!");
                }
                //CosmosConsole.WriteLine("Minecraft is closed.");
            }
        }

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLanguage = LanguageComboBox.SelectedItem.ToString();
            string langFile = LanguageHandler.GetLangFileName(selectedLanguage);
            SettingsManager.Language = langFile;

            LanguageHandler.Load(PathDefinitions.AppDirectory + @"Texts\" + langFile + ".lang");
            UpdateLauncherLanguage();
            CosmosConsole.WriteLine($"Language set to {selectedLanguage}.");
        }

        private void UpdateLauncherLanguage()
        {
            TopLabel.Text = LanguageHandler.Get("App.TopLabel.Name");
            AboutLabel.Text = LanguageHandler.Get("About.AboutLabel.Text");
            DiscordLabel.Text = LanguageHandler.Get("About.DiscordLabel.Text");
            GitHubLabel.Text = LanguageHandler.Get("About.GitHubLabel.Text");
            WebsiteLabel.Text = LanguageHandler.Get("About.WebsiteLabel.Text");
            BackgroundModeTitleLabel.Text = LanguageHandler.Get("Settings.BackgroundMode.Title");
            BackgroundModeDescriptionLabel.Text = LanguageHandler.Get("Settings.BackgroundMode.Description");
            DiscordRichPresenceLabel.Text = LanguageHandler.Get("Settings.DiscordRichPresence.Title");
            DiscordRichPresenceDescription.Text = LanguageHandler.Get("Settings.DiscordRichPresence.Description");
            LanguageTitleLabel.Text = LanguageHandler.Get("Settings.Language.Title");
            LanguageDescriptionLabel.Text = LanguageHandler.Get("Settings.Language.Description");
            UpdateLabel.Text = LanguageHandler.Get("Update.UpdateLabel.Text");
            ChangelogLabel.Text = LanguageHandler.Get("Update.ChangelogLabel.Text");
            UpdateButton.Text = LanguageHandler.Get("Update.UpdateButton.Text");
            CancelUpdateButton.Text = LanguageHandler.Get("Update.CancelUpdateButton.Text");

            if (!SettingsManager.ProxyStarted && !SettingsManager.BackgroundMode)
                LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Launch");
            else if (SettingsManager.ProxyStarted && !SettingsManager.BackgroundMode)
                LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Running");
            else
                LaunchButton.Text = LanguageHandler.Get("Home.LaunchButton.Listening");

            if (!SettingsManager.ProxyStarted && SettingsManager.BackgroundMode)
                StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyDisabled");
            else if (SettingsManager.ProxyStarted && SettingsManager.BackgroundMode)
                StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.ProxyEnabled");
            else
                StatusLabel.Text = "";

            if (DiscordRichPresence.IsInitialized())
                DiscordRichPresence.UpdatePresence();
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
                DownloadZipProgressLabel.Text = "Downloading...";
                await asyncFileOps.DownloadFileAsync(fileUrl, downloadPath);

                DownloadZipProgressLabel.Text = "Extracting...";
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
                DownloadZipProgressLabel.Text = "Done!";
            }
            catch (Exception)
            {
                DownloadZipButton.Enabled = true;
                DownloadZipProgressLabel.Text = "Unable to download zip file.";
                CosmosConsole.WriteLine("Unable to download file. Download canceled.");
            }
        }

        private void EnableLoggingSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableLoggingSwitch.Checked)
            {
                SettingsManager.EnableLogging = true;
                CosmosConsole.WriteLine("Logging enabled.");
            }
            else
            {
                CosmosConsole.WriteLine("Logging disabled.");
                SettingsManager.EnableLogging = false;
            }
        }

        private void DetailedLoggingSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (DetailedLoggingSwitch.Checked)
            {
                SettingsManager.DetailedLogging = true;
                CosmosConsole.WriteLine("Detailed logs enabled.");
            }
            else
            {
                SettingsManager.DetailedLogging = false;
                CosmosConsole.WriteLine("Detailed logs disabled.");
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

        private async void FixProxyHangButton_Click(object sender, EventArgs e)
        {
            if (!SettingsManager.ProxyStarted)
            {
                await Task.Run(() =>
                {
                    controller.StartProxy();
                });

                controller.Stop();
                CosmosConsole.WriteLine("Reset proxy.");
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
                StatusLabel.Text = LanguageHandler.Get("Home.StatusLabel.NoInternet");
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