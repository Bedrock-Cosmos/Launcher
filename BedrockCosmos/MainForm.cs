using BedrockCosmos.App;
using System;
using System.Drawing;
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
        private static readonly ProxyController controller = new ProxyController();
        SettingsManager sManager;
        string consoleSender = "App";

        // For window movement
        bool drag = false;
        Point start_point = new Point(0, 0);

        public MainForm()
        {
            InitializeComponent();
            CosmosConsole.Initialize(DevConsole);

            // Will also log messages to the main console if uncommented.
            //CosmosConsole.logToMainConsole = true;

            if (RunTime.IsWindows)
                // Fix console hang due to QuickEdit mode
                ConsoleHelper.DisableQuickEditMode();

            sManager = new SettingsManager();
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
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

        private void AppIcon_Click(object sender, EventArgs e)
        {
            bool isDevMenuEnabled = sManager.DevMenuCheck();
            if (isDevMenuEnabled)
            {
                AppIcon.Cursor = Cursors.Default;
                TabControl.SelectedTab = DevPage;
            }
        }

        private void DevBackButton_Click(object sender, EventArgs e)
        {
            AppIcon.Cursor = Cursors.Hand;
            TabControl.SelectedTab = HomePage;
        }

        public void UpdateCosmosConsole(string logs)
        {
            DevConsole.Text = logs;
        }

        private void DownloadZipButton_Click(object sender, EventArgs e)
        {

        }

        private void EnableLoggingSwitch_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            DevConsole.Text = "";
        }

        private void ExportLogsButton_Click(object sender, EventArgs e)
        {

        }
    }
}
