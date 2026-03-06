using BedrockCosmos.App;
using BedrockCosmos.Proxy;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockCosmos
{
    public partial class MainForm : Form
    {
        private LaunchManager launchManager;
        private static ProxyController controller;
        private readonly AsyncFileDownload asyncDownload;

        // For window movement
        bool drag = false;
        Point startPoint = new Point(0, 0);

        public MainForm()
        {
            InitializeComponent();
            CosmosConsole.Initialize(DevConsole);
            DiscordRichPresence.InitializeRpc();
            DiscordRichPresence.UpdatePresence();
            launchManager = new LaunchManager();
            controller = new ProxyController();
            asyncDownload = new AsyncFileDownload();
            StatusLabel.Text = "";

            // Will also log messages to the main console if uncommented.
            //CosmosConsole.LogToMainConsole = true;
            launchManager.InitializeMgrAsyncFileDownload(asyncDownload);
            launchManager.InitializeMgrLaunchButton(LaunchButton);
            launchManager.InitializeMgrVersionLabel(VersionLabel);
            launchManager.SetCurrentVersions();
            

            LanguageHandler.Load(AppDomain.CurrentDomain.BaseDirectory + @"Texts\" + SettingsManager.Language + ".lang");
            SettingsManager.LoadSettings();
            ApplySettings();
        }

        private void ApplySettings()
        {
            // Language
            LanguageComboBox.SelectedItem = LanguageHandler.GetLanguageName(SettingsManager.Language);

            // Background Mode
            BackgroundModeSwitch.Checked = SettingsManager.BackgroundMode;

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
                try
                {
                    controller.Stop();
                    controller.Dispose();
                    controller = null;
                }
                catch
                {

                }
            }

            asyncDownload.Dispose();
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
                LaunchButton.Text = LanguageHandler.Home_LaunchButton_Entering;
                launchManager.UpdateLaunchButtonColor("purple");
                await launchManager.InternetCheck();

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Misc\CurrentVersion.json"))
                {
                    launchManager.SetLatestVersions();
                    bool updateLauncher = launchManager.CheckLauncherUpdate();

                    if (updateLauncher && !SettingsManager.LauncherUpdatePrompted)
                    {
                        SettingsManager.LauncherUpdatePrompted = true;
                        TabControl.SelectedTab = UpdatePage;
                        launchManager.ResetLaunchStatus();
                    }
                    else
                    {
                        if (launchManager.CheckResponsesUpdate() && !updateLauncher)
                            await launchManager.UpdateResponses();

                        JsonData.InitializeJsons();

                        await Task.Run(() =>
                        {
                            controller.StartProxy();
                        });

                        LaunchButton.Text = LanguageHandler.Home_LaunchButton_Running;
                        CosmosConsole.WriteLine("Proxy started!");

                        SettingsManager.ProxyStarted = true;
                        launchManager.OpenMinecraft();
                    }
                }
                else
                {
                    launchManager.ResetLaunchStatus();
                    StatusLabel.Text = LanguageHandler.Home_StatusLabel_NoInternet;
                }

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
                LaunchButton.Enabled = false;
                LaunchButton.Text = LanguageHandler.Home_LaunchButton_Listening;

                if (SettingsManager.ProxyStarted)
                    StatusLabel.Text = LanguageHandler.Home_StatusLabel_ProxyEnabled;
                else
                    StatusLabel.Text = LanguageHandler.Home_StatusLabel_ProxyDisabled;

                BackgroundModeTimer.Start();
                CosmosConsole.WriteLine("Background Mode enabled.");
            }
            else
            {
                LaunchButton.Enabled = true;

                if (SettingsManager.ProxyStarted)
                    LaunchButton.Text = LanguageHandler.Home_LaunchButton_Running;
                else
                    LaunchButton.Text = LanguageHandler.Home_LaunchButton_Launch;

                StatusLabel.Text = "";
                BackgroundModeTimer.Stop();
                CosmosConsole.WriteLine("Background Mode disabled.");
            }
        }

        private async void BackgroundModeTimer_Tick(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Minecraft.Windows");

            if (pname.Length != 0)
            {
                if (!SettingsManager.ProxyStarted)
                {
                    await launchManager.InternetCheck();

                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Misc\CurrentVersion.json"))
                    {
                        launchManager.SetLatestVersions();
                        bool updateLauncher = launchManager.CheckLauncherUpdate();

                        if (updateLauncher && !SettingsManager.LauncherUpdatePrompted)
                        {
                            SettingsManager.LauncherUpdatePrompted = true;
                            TabControl.SelectedTab = UpdatePage;
                            Show();
                            WindowState = FormWindowState.Normal;
                            TrayIcon.Visible = false;
                        }
                        else
                        {
                            if (launchManager.CheckResponsesUpdate() && !updateLauncher)
                                await launchManager.UpdateResponses();

                            JsonData.InitializeJsons();

                            await Task.Run(() =>
                            {
                                controller.StartProxy();
                            });

                            LaunchButton.Text = LanguageHandler.Home_LaunchButton_Listening;
                            CosmosConsole.WriteLine("Proxy started!");

                            SettingsManager.ProxyStarted = true;
                            launchManager.UpdateLaunchButtonColor("purple");
                            StatusLabel.Text = LanguageHandler.Home_StatusLabel_ProxyEnabled;
                        }
                    }
                    else
                    {
                        StatusLabel.Text = LanguageHandler.Home_StatusLabel_NoInternet;
                    }
                }
                //CosmosConsole.WriteLine("Minecraft is open.");
            }
            else
            {
                if (SettingsManager.ProxyStarted)
                {
                    SettingsManager.ProxyStarted = false;
                    launchManager.UpdateLaunchButtonColor("green");
                    StatusLabel.Text = LanguageHandler.Home_StatusLabel_ProxyDisabled;
                    controller.Stop();
                }
                //CosmosConsole.WriteLine("Minecraft is closed.");
            }
        }

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLanguage = LanguageComboBox.SelectedItem.ToString();
            string langFile = LanguageHandler.GetLangFileName(selectedLanguage);
            SettingsManager.Language = langFile;

            LanguageHandler.Load(AppDomain.CurrentDomain.BaseDirectory + @"Texts\" + langFile + ".lang");
            UpdateLauncherLanguage();
            CosmosConsole.WriteLine($"Language set to {selectedLanguage}.");
        }

        private void UpdateLauncherLanguage()
        {
            TopLabel.Text = LanguageHandler.App_TopLabel_Name;
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

            if (!SettingsManager.ProxyStarted && !SettingsManager.BackgroundMode)
                LaunchButton.Text = LanguageHandler.Home_LaunchButton_Launch;
            else if (SettingsManager.ProxyStarted && !SettingsManager.BackgroundMode)
                LaunchButton.Text = LanguageHandler.Home_LaunchButton_Running;
            else
                LaunchButton.Text = LanguageHandler.Home_LaunchButton_Listening;

            if (!SettingsManager.ProxyStarted && SettingsManager.BackgroundMode)
                StatusLabel.Text = LanguageHandler.Home_StatusLabel_ProxyDisabled;
            else if (SettingsManager.ProxyStarted && SettingsManager.BackgroundMode)
                StatusLabel.Text = LanguageHandler.Home_StatusLabel_ProxyEnabled;
            else
                StatusLabel.Text = "";
        }

        private async void DownloadZipButton_Click(object sender, EventArgs e)
        {
            string fileUrl = "https://github.com/Bedrock-Cosmos/Responses/archive/refs/heads/main.zip";
            string downloadPath = AppDomain.CurrentDomain.BaseDirectory + @"main.zip";
            string extractPath = AppDomain.CurrentDomain.BaseDirectory;

            DownloadZipButton.Enabled = false;
            DownloadZipProgressLabel.Visible = true;

            try
            {
                DownloadZipProgressLabel.Text = "Downloading...";
                await asyncDownload.DownloadFileAsync(fileUrl, downloadPath);

                DownloadZipProgressLabel.Text = "Extracting...";
                await asyncDownload.ExtractFileAsync(downloadPath, extractPath, true);

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
            string fileUrl = "https://raw.githubusercontent.com/Bedrock-Cosmos/Launcher/main/LauncherFiles/Updater.zip";
            string downloadPath = AppDomain.CurrentDomain.BaseDirectory + @"Updater.zip";
            string extractPath = AppDomain.CurrentDomain.BaseDirectory;

            UpdateButton.Enabled = false;
            CancelUpdateButton.Enabled = false;
            CloseButton.Enabled = false;

            try
            {
                await asyncDownload.DownloadFileAsync(fileUrl, downloadPath);
                await asyncDownload.ExtractFileAsync(downloadPath, extractPath, true);

                if (File.Exists(extractPath + "Updater.exe"))
                    Process.Start(extractPath + "Updater.exe");

                Close();
            }
            catch (Exception)
            {
                StatusLabel.Text = LanguageHandler.Home_StatusLabel_NoInternet;
                TabControl.SelectedTab = HomePage;
                
                UpdateButton.Enabled = true;
                CancelUpdateButton.Enabled = true;
                CloseButton.Enabled = true;
                SettingsManager.LauncherUpdatePrompted = false;
            }
        }

        private void CancelUpdateButton_Click(object sender, EventArgs e)
        {
            TabControl.SelectedTab = HomePage;
        }
    }
}