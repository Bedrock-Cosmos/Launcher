using BedrockCosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Examples.Basic;
using Titanium.Web.Proxy.Examples.Basic.Helpers;
using Titanium.Web.Proxy.Helpers;

namespace BedrockCosmos
{
    public partial class BedrockCosmos : Form
    {
        private static readonly ProxyController controller = new ProxyController();

        // For window movement
        bool drag = false;
        Point start_point = new Point(0, 0);

        public BedrockCosmos()
        {
            InitializeComponent();

            if (RunTime.IsWindows)
                // Fix console hang due to QuickEdit mode
                ConsoleHelper.DisableQuickEditMode();
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

        private void StartButton_Click(object sender, EventArgs e)
        {
            controller.StartProxy();
            StatusLabel.Text = "Program started!";
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            controller.Stop();
            StatusLabel.Text = "Program stopped!";
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            /*WebClient DownloadZip = new WebClient();
            DownloadZip.DownloadFileCompleted += new AsyncCompletedEventHandler(ZipCompleted);
            DownloadZip.DownloadFileAsync(new Uri("https://github.com/Bedrock-Cosmos/Responses/archive/refs/heads/main.zip"), @"AppDirectory + Responses.zip");
            DownloadZip.Dispose();*/
        }

        private void ZipCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
        }
    }
}
