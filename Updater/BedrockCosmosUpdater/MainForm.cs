using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace BedrockCosmosUpdater
{
    public partial class MainForm : Form
    {
        int success = 0;
        string programPath = "";
        int process1 = 0;
        string internetCheckStartup;
        string appTest;

        public MainForm()
        {
            InitializeComponent();

            if (!Directory.Exists(@"C:\temp"))
            {
                Directory.CreateDirectory(@"C:\temp");
            }
            if (!Directory.Exists(@"C:\temp\ProjectStar\Miscellaneous"))
            {
                Directory.CreateDirectory(@"C:\temp\ProjectStar\Miscellaneous");
            }
            if (!Directory.Exists(@"C:\temp\ProjectStar\Updates"))
            {
                Directory.CreateDirectory(@"C:\temp\ProjectStar\Updates");
            }

            if (!File.Exists(@"C:\temp\ProjectStar\Updates\DelayTest.txt"))
            {
                using (FileStream fs = File.Create(@"C:\temp\ProjectStar\Updates\DelayTest.txt"))
                {

                }

                StreamWriter sw = new StreamWriter(@"C:\temp\ProjectStar\Updates\DelayTest.txt");
                sw.WriteLine("0");
                sw.Close();
            }

            StreamReader sr = new StreamReader(@"C:\temp\ProjectStar\Updates\DelayTest.txt");
            appTest = sr.ReadLine();
            sr.Close();

            if(appTest == "0")
            {
                using (FileStream fs = File.Create(@"C:\temp\ProjectStar\Updates\DelayTest.txt"))
                {

                }

                StreamWriter sw = new StreamWriter(@"C:\temp\ProjectStar\Updates\DelayTest.txt");
                sw.WriteLine("1");
                sw.Close();

                openDelay.Start();
            } 
            else if (appTest == "1")
            {
                using (FileStream fs = File.Create(@"C:\temp\ProjectStar\Updates\DelayTest.txt"))
                {

                }

                StreamWriter sw = new StreamWriter(@"C:\temp\ProjectStar\Updates\DelayTest.txt");
                sw.WriteLine("0");
                sw.Close();

                delay.Start();
            }
        }

        // Start of window movement
        bool drag = false;
        Point start_point = new Point(0, 0);

        private void dragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;
            start_point = new Point(e.X, e.Y);
        }

        private void dragPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - start_point.X, p.Y - start_point.Y);
            }
        }

        private void dragPanel_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }
        // End of window movement

        private void statusLabel_Click(object sender, EventArgs e)
        {

        }

        private void delay_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = "Checking internet connection...";
            WebClient internetCheckDownload = new WebClient();
            internetCheckDownload.DownloadFileAsync(new Uri("https://github.com/BionicBen/ProjectStarFiles/blob/main/InternetCheck.txt?raw=true"), @"C:\temp\ProjectStar\Miscellaneous\InternetCheck.txt");
            internetCheckDownload.DownloadFileCompleted += new AsyncCompletedEventHandler(internetCheckCompleted);
            delay.Stop();
        }

        private void internetCheckCompleted(object sender, AsyncCompletedEventArgs e)
        {
            StreamReader sr = new StreamReader(@"C:\temp\ProjectStar\Miscellaneous\InternetCheck.txt");
            internetCheckStartup = sr.ReadLine();
            sr.Close();

            if (internetCheckStartup != "1")
            {
                statusLabel.Text = "Error! Please connect to the internet and retry.";
                closeButton.Visible = true;
            }
            else if (internetCheckStartup == "1")
            {
                startUpdater();
            }
        }

        private void startUpdater()
        {
            statusLabel.Text = "Locating original files...";
            try
            {
                StreamReader sr = new StreamReader(@"C:\temp\ProjectStar\Updates\InstallationPath.txt");
                programPath = sr.ReadLine();
                sr.Close();

                process1 = 1;
            }
            catch (Exception)
            {
                statusLabel.Text = "Error! Try opening Project Star and retrying installation.";
                closeButton.Visible = true;
            }

            if (process1 == 1)
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileAsync(new Uri("https://github.com/BionicBen/ProjectStarFiles/blob/main/CurrentVersion.txt?raw=true"), @"C:\temp\ProjectStar\Updates\LatestVersion.txt");
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CurrentVersionCompleted);
            }
        }

        private void CurrentVersionCompleted(object sender, AsyncCompletedEventArgs e)
        {
            shortWait.Start();
        }

        private void DownloadNewVersion()
        {
            statusLabel.Text = "Downloading new launcher version...";

            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(FinishInstallation);
            webClient.DownloadFileAsync(new Uri("https://github.com/BionicBen/ProjectStarFiles/blob/main/ProjectStar/ProjectStar.exe?raw=true"), programPath + @"\ProjectStar.exe");
        }

        private void FinishInstallation(object sender, AsyncCompletedEventArgs e)
        {
            success = 1;
            statusLabel.Text = "Successfully updated Project Star.";
            closeButton.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (success == 1 && File.Exists(programPath + @"\ProjectStar.exe"))
            {
                Process.Start(programPath + @"\ProjectStar.exe");
            }
            this.Close();
        }

        private void shortWait_Tick(object sender, EventArgs e)
        {
            DownloadNewVersion();
            shortWait.Stop();
        }

        private void openDelay_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = "Opening...";
            statusLabel.Text = "Locating original files...";
            try
            {
                StreamReader sr = new StreamReader(@"C:\temp\ProjectStar\Updates\InstallationPath.txt");
                programPath = sr.ReadLine();
                sr.Close();
            }
            catch (Exception)
            {
                statusLabel.Text = "Error! Try opening Project Star and retrying installation.";
                closeButton.Visible = true;
            }

            if (File.Exists(Application.StartupPath + @"\ProjectStarUpdater.exe"))
            {
                if (File.Exists(programPath + @"\DiscordRPC.dll"))
                {
                    File.Delete(programPath + @"\DiscordRPC.dll");
                }
                if (File.Exists(programPath + @"\Guna.UI2.dll"))
                {
                    File.Delete(programPath + @"\Guna.UI2.dll");
                }
                if (File.Exists(programPath + @"\Newtonsoft.Json.dll"))
                {
                    File.Delete(programPath + @"\Newtonsoft.Json.dll");
                }
                if (File.Exists(programPath + @"\DotNetZip.dll"))
                {
                    File.Delete(programPath + @"\DotNetZip.dll");
                }
                if (File.Exists(programPath + @"\FilePermHelper.dll"))
                {
                    File.Delete(programPath + @"\FilePermHelper.dll");
                }
                if (File.Exists(programPath + @"\IObitUnlocker.sys"))
                {
                    try
                    {
                        File.Delete(programPath + @"\IObitUnlocker.sys");
                    }
                    catch (Exception)
                    {

                    }
                }
                if (File.Exists(programPath + @"\ProjectStar.exe"))
                {
                    File.Delete(programPath + @"\ProjectStar.exe");
                }
                if (Directory.Exists(programPath + @"\Confused"))
                {
                    Directory.Delete(programPath + @"\Confused");
                }

                Process.Start(Application.StartupPath + @"\ProjectStarUpdater.exe");
                this.Close();
            }
            else
            {
                statusLabel.Text = "Error! Try opening Project Star and retrying installation.";
                closeButton.Visible = true;
            }
        }
    }
}
