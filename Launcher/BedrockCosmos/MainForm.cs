using BedrockCosmos.App;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Examples.Basic;
using Titanium.Web.Proxy.Examples.Basic.Helpers;
using Titanium.Web.Proxy.Helpers;

namespace BedrockCosmos
{
    public partial class MainForm : Form
    {
        private static ProxyController controller = new ProxyController();
        private readonly AsyncFileDownload asyncDownload;
        string consoleSender = "App";

        // For window movement
        bool drag = false;
        Point start_point = new Point(0, 0);

        public MainForm()
        {
            InitializeComponent();
            CosmosConsole.Initialize(DevConsole);
            asyncDownload = new AsyncFileDownload();

            // Will also log messages to the main console if uncommented.
            //CosmosConsole.LogToMainConsole = true;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Text = "v" + version.Major + "." + version.Minor + "." + version.Build;

            SettingsManager.LoadSettings();
            ApplySettings();

            if (RunTime.IsWindows)
                // Fix console hang due to QuickEdit mode
                ConsoleHelper.DisableQuickEditMode();
        }

        private void ApplySettings()
        {
            EnableLoggingSwitch.Checked = SettingsManager.EnableLogging;
            BackgroundModeSwitch.Checked = SettingsManager.BackgroundMode;

            if (SettingsManager.DevMenuEnabled)
            {
                SettingsManager.DevMenuClicks = 7;
                AppIcon.Cursor = Cursors.Hand;
                CosmosConsole.WriteLine(consoleSender, "Developer mode enabled.");
            }
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            if (!SettingsManager.BackgroundMode)
            {
                this.WindowState = FormWindowState.Minimized;
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
            /*if (controller != null)
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
            }*/

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
            start_point = new Point(e.X, e.Y);
        }

        private void TopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - start_point.X, p.Y - start_point.Y);
            }
        }

        private void TopPanel_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }

        private async void LaunchButton_Click(object sender, EventArgs e)
        {
            if (LaunchButton.Text != "RUNNING")
            {
                CosmosConsole.WriteLine(consoleSender, "Starting proxy...");
                SettingsManager.ProxyStarted = true;

                LaunchButton.Text = "Entering The Cosmos...";
                LaunchButton.FilledBackColorBottom = Color.FromArgb(66, 0, 113);
                LaunchButton.FilledBackColorTop = Color.FromArgb(138, 0, 234);
                LaunchButton.HoverBackColor = Color.FromArgb(138, 0, 234);
                LaunchButton.HoverFillColor = Color.FromArgb(138, 0, 234);
                LaunchButton.NormalBackColor = Color.FromArgb(138, 0, 234);
                LaunchButton.PressedBackColor = Color.FromArgb(138, 0, 234);

                await Task.Run(() =>
                {
                    controller.StartProxy();
                });

                LaunchButton.Text = "RUNNING";
                CosmosConsole.WriteLine(consoleSender, "Proxy started!");
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, "Stopping proxy...");
                SettingsManager.ProxyStarted = false;

                LaunchButton.Text = "LAUNCH";
                LaunchButton.FilledBackColorBottom = Color.FromArgb(0, 114, 47);
                LaunchButton.FilledBackColorTop = Color.FromArgb(0, 188, 71);
                LaunchButton.HoverBackColor = Color.FromArgb(0, 188, 71);
                LaunchButton.HoverFillColor = Color.FromArgb(0, 188, 71);
                LaunchButton.NormalBackColor = Color.FromArgb(0, 188, 71);
                LaunchButton.PressedBackColor = Color.FromArgb(0, 188, 71);
                controller.Stop();

                CosmosConsole.WriteLine(consoleSender, "Proxy stopped!");
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

        private void BackgroundModeToggle_CheckedChanged(object sender, EventArgs e)
        {
            SettingsManager.BackgroundMode = BackgroundModeSwitch.Checked;

            if (SettingsManager.BackgroundMode)
            {
                LaunchButton.Enabled = false;
                LaunchButton.Text = "LISTENING";
                BackgroundModeTimer.Start();
                CosmosConsole.WriteLine(consoleSender, "Background Mode enabled.");
            }
            else
            {
                LaunchButton.Enabled = true;
                LaunchButton.Text = "LAUNCH";
                BackgroundModeTimer.Stop();
                CosmosConsole.WriteLine(consoleSender, "Background Mode disabled.");
            } 
        }

        private async void BackgroundModeTimer_Tick(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Minecraft.Windows");

            if (pname.Length != 0)
            {
                if (!SettingsManager.ProxyStarted)
                {
                    SettingsManager.ProxyStarted = true;

                    LaunchButton.FilledBackColorBottom = Color.FromArgb(66, 0, 113);
                    LaunchButton.FilledBackColorTop = Color.FromArgb(138, 0, 234);
                    LaunchButton.HoverBackColor = Color.FromArgb(138, 0, 234);
                    LaunchButton.HoverFillColor = Color.FromArgb(138, 0, 234);
                    LaunchButton.NormalBackColor = Color.FromArgb(138, 0, 234);
                    LaunchButton.PressedBackColor = Color.FromArgb(138, 0, 234);

                    await Task.Run(() =>
                    {
                        controller.StartProxy();
                    });
                }
                //CosmosConsole.WriteLine(consoleSender, "Minecraft is open.");
            }
            else
            {
                if (SettingsManager.ProxyStarted)
                {
                    SettingsManager.ProxyStarted = false;

                    LaunchButton.FilledBackColorBottom = Color.FromArgb(0, 114, 47);
                    LaunchButton.FilledBackColorTop = Color.FromArgb(0, 188, 71);
                    LaunchButton.HoverBackColor = Color.FromArgb(0, 188, 71);
                    LaunchButton.HoverFillColor = Color.FromArgb(0, 188, 71);
                    LaunchButton.NormalBackColor = Color.FromArgb(0, 188, 71);
                    LaunchButton.PressedBackColor = Color.FromArgb(0, 188, 71);

                    controller.Stop();
                }
                //CosmosConsole.WriteLine(consoleSender, "Minecraft is closed.");
            }
        }

        private async void DownloadZipButton_Click(object sender, EventArgs e)
        {
            string fileUrl = "https://github.com/Bedrock-Cosmos/Responses/archive/refs/heads/main.zip";
            string downloadPath = AppDomain.CurrentDomain.BaseDirectory + @"main.zip";
            string extractPath = AppDomain.CurrentDomain.BaseDirectory;

            DownloadZipButton.Enabled = false;
            DownloadZipProgressLabel.Visible = true;

            DownloadZipProgressLabel.Text = "Downloading...";
            await asyncDownload.DownloadFileAsync(fileUrl, downloadPath);

            DownloadZipProgressLabel.Text = "Extracting...";
            await asyncDownload.ExtractFileAsync(downloadPath, extractPath, true);

            DownloadZipButton.Enabled = true;
            DownloadZipProgressLabel.Text = "Done!";
        }

        private void EnableLoggingSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableLoggingSwitch.Checked)
            {
                SettingsManager.EnableLogging = true;
                CosmosConsole.WriteLine(consoleSender, "Logging enabled.");
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, "Logging disabled.");
                SettingsManager.EnableLogging = false;
            }
        }

        private void ResetNewsButton_Click(object sender, EventArgs e)
        {
            //NewsManager.RetrieveNewsHistory();
            //NewsManager.RetrieveCurrentNews();
            //NewsManager.CheckForNews();
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            DevConsole.Text = "";
        }

        private void ExportLogsButton_Click(object sender, EventArgs e)
        {
            CosmosConsole.ExportLogs();
        }

        private void DisableDevMenuButton_Click(object sender, EventArgs e)
        {
            SettingsManager.DisableDevMenu();
            TabControl.SelectedTab = HomePage;
        }
    }
}
