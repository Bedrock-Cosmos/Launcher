using BedrockCosmos.App.UI;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BedrockCosmos.App
{
    internal class LaunchManager
    {
        private Version _currentLauncherVersion = new Version("0.0.1");
        private Version _latestLauncherVersion = new Version("0.0.1");
        private int _currentResponsesVersion = 0;
        private int _latestResponsesVersion = 0;
        private string _miscDirectory = AppDomain.CurrentDomain.BaseDirectory + @"Misc";
        private AsyncFileDownload _asyncDownload = null;
        private RoundGradientButton _launchButton = null;
        private System.Windows.Forms.Label _versionLabel = null;

        internal Version CurrentLauncherVersion
        {
            get { return _currentLauncherVersion; }
            set { _currentLauncherVersion = value; }
        }

        internal Version LatestLauncherVersion
        {
            get { return _latestLauncherVersion; }
            set { _latestLauncherVersion = value; }
        }

        internal int CurrentResponsesVersion
        {
            get { return _currentResponsesVersion; }
            set { _currentResponsesVersion = value; }
        }

        internal int LatestResponsesVersion
        {
            get { return _latestResponsesVersion; }
            set { _latestResponsesVersion = value; }
        }

        internal void InitializeMgrAsyncFileDownload(AsyncFileDownload async)
        {
            _asyncDownload = async;
        }

        internal void InitializeMgrLaunchButton(RoundGradientButton RGButton)
        {
            _launchButton = RGButton;
        }

        internal void InitializeMgrVersionLabel(System.Windows.Forms.Label label)
        {
            _versionLabel = label;
        }

        internal void SetCurrentVersions()
        {
            // Launcher
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            _currentLauncherVersion = new Version(version.Major + "." + version.Minor + "." + version.Build);

            if (_versionLabel != null)
                _versionLabel.Text = $"v{_currentLauncherVersion}";

            // Responses
            string savedResponseVersionPath = _miscDirectory + @"\ResponsesVersion.txt";
            string fileContent = "";

            if (File.Exists(savedResponseVersionPath))
                fileContent = File.ReadAllText(savedResponseVersionPath);

            if (int.TryParse(fileContent, out int resultInt))
                _currentResponsesVersion = resultInt;
        }

        internal async Task InternetCheck()
        {
            string fileUrl = "https://raw.githubusercontent.com/Bedrock-Cosmos/Backend/main/CurrentVersion.json";
            string downloadPath = _miscDirectory + @"\CurrentVersion.json";

            if (!Directory.Exists(_miscDirectory))
                Directory.CreateDirectory(_miscDirectory);

            if (File.Exists(downloadPath))
                File.Delete(downloadPath);

            try
            {
                await _asyncDownload.DownloadFileAsync(fileUrl, downloadPath);
            }
            catch (Exception)
            {
                CosmosConsole.WriteLine("Unable to download file. Download canceled.");
            }
        }

        internal void SetLatestVersions()
        {
            string versionJsonPath = _miscDirectory + @"\CurrentVersion.json";
            string savedResponseVersionPath = _miscDirectory + @"\ResponsesVersion.txt";
            AppVersions ver = null;

            if (File.Exists(versionJsonPath))
            {
                string json = File.ReadAllText(versionJsonPath);
                ver = JsonConvert.DeserializeObject<AppVersions>(json);
                _latestLauncherVersion = new Version(ver.launcherVersion);
                _latestResponsesVersion = ver.responsesVersion;
            }
        }

        internal bool CheckLauncherUpdate()
        {
            if (_latestLauncherVersion > _currentLauncherVersion)
            {
                CosmosConsole.WriteLine($"Launcher update found (v{_latestLauncherVersion}).");
                return true;
            } 
            else
            {
                CosmosConsole.WriteLine($"Launcher is up to date.");
                return false;
            } 
        }

        internal bool CheckResponsesUpdate()
        {
            if (_latestResponsesVersion > _currentResponsesVersion || 
                !Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Responses-main"))
            {
                CosmosConsole.WriteLine($"Responses update found (v{_latestResponsesVersion}).");
                return true;
            }
            else
            {
                CosmosConsole.WriteLine($"Responses are up to date.");
                return false;
            }
        }

        internal async Task UpdateResponses()
        {
            string fileUrl = "https://github.com/Bedrock-Cosmos/Responses/archive/refs/heads/main.zip";
            string downloadPath = AppDomain.CurrentDomain.BaseDirectory + @"main.zip";
            string extractPath = AppDomain.CurrentDomain.BaseDirectory;

            if (_launchButton != null)
                UpdateLaunchButtonText(LanguageHandler.Home_LaunchButton_Updating);

            try
            {
                await _asyncDownload.DownloadFileAsync(fileUrl, downloadPath);
                await _asyncDownload.ExtractFileAsync(downloadPath, extractPath, true);
                File.WriteAllText(_miscDirectory + @"\ResponsesVersion.txt", _latestResponsesVersion.ToString());
                _currentResponsesVersion = _latestResponsesVersion;
            }
            catch (Exception)
            {
                CosmosConsole.WriteLine("Unable to download file. Download canceled.");
            }
        }

        internal void OpenMinecraft()
        {
            try
            {
                Process.Start("minecraft://");
                CosmosConsole.WriteLine("Opened Minecraft.");
            }
            catch (Exception)
            {
                CosmosConsole.WriteLine("Unable to launch Minecraft (ensure that the game in installed).");
            }
        }

        internal void UpdateLaunchButtonText(string text)
        {
            if (_launchButton != null)
                _launchButton.Text = text;
        }

        internal void UpdateLaunchButtonColor(string color)
        {
            if (_launchButton != null)
            {
                if (color.ToLower() == "purple")
                {
                    _launchButton.FilledBackColorBottom = Color.FromArgb(66, 0, 113);
                    _launchButton.FilledBackColorTop = Color.FromArgb(138, 0, 234);
                    _launchButton.HoverBackColor = Color.FromArgb(138, 0, 234);
                    _launchButton.HoverFillColor = Color.FromArgb(138, 0, 234);
                    _launchButton.NormalBackColor = Color.FromArgb(138, 0, 234);
                    _launchButton.PressedBackColor = Color.FromArgb(138, 0, 234);
                }
                else
                {
                    _launchButton.FilledBackColorBottom = Color.FromArgb(0, 114, 47);
                    _launchButton.FilledBackColorTop = Color.FromArgb(0, 188, 71);
                    _launchButton.HoverBackColor = Color.FromArgb(0, 188, 71);
                    _launchButton.HoverFillColor = Color.FromArgb(0, 188, 71);
                    _launchButton.NormalBackColor = Color.FromArgb(0, 188, 71);
                    _launchButton.PressedBackColor = Color.FromArgb(0, 188, 71);
                }
            } 
        }

        internal void ResetLaunchStatus()
        {
            SettingsManager.ProxyStarted = false;

            if (_launchButton != null)
            {
                UpdateLaunchButtonText(LanguageHandler.Home_LaunchButton_Launch);
                UpdateLaunchButtonColor("green");
                _launchButton.Enabled = true;
            }
        }
    }

    public class AppVersions
    {
        public string launcherVersion { get; set; }
        public int responsesVersion { get; set; }
    }
}
