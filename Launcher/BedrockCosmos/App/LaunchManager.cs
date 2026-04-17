using BedrockCosmos.App.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.App
{
    internal class LaunchManager
    {
        private Version _currentLauncherVersion = new Version("0.0.0.0");
        private Version _latestLauncherVersion = new Version("0.0.0.0");
        private int _currentResponsesVersion = 0;
        private int _latestResponsesVersion = 0;
        private AsyncFileOperations _asyncFileOps = null;
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

        internal void InitializeMgrAsyncFileOps(AsyncFileOperations async)
        {
            _asyncFileOps = async;
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
            _currentLauncherVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (_versionLabel != null)
                _versionLabel.Text = $"v{_currentLauncherVersion.Major + "." + _currentLauncherVersion.Minor + "." + _currentLauncherVersion.Build} - Minecraft v26.10+";

            // Responses
            string savedResponseVersionPath = PathDefinitions.MiscDirectory + @"ResponsesVersion.txt";
            string fileContent = "";

            if (File.Exists(savedResponseVersionPath))
                fileContent = File.ReadAllText(savedResponseVersionPath);

            if (int.TryParse(fileContent, out int resultInt))
                _currentResponsesVersion = resultInt;
        }

        internal async Task InternetCheck()
        {
            string version = "";
            string responsesVersion = "";

            if (!Directory.Exists(PathDefinitions.MiscDirectory))
                Directory.CreateDirectory(PathDefinitions.MiscDirectory);

            try
            {
                (version, responsesVersion) = await _asyncFileOps.ReadVersionFileAsync();
                _latestLauncherVersion = new Version(version);
                _latestResponsesVersion = int.Parse(responsesVersion);
            }
            catch (Exception)
            {
                CosmosConsole.WriteLine("Unable to download file. Download canceled.");
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
                CosmosConsole.WriteLine("Launcher is up to date.");
                return false;
            } 
        }

        internal bool CheckResponsesUpdate()
        {
            if (_latestResponsesVersion > _currentResponsesVersion)
            {
                CosmosConsole.WriteLine($"Launcher update found (v{_latestResponsesVersion}).");
                return true;
            }
            else
            {
                CosmosConsole.WriteLine("Responses are up to date.");
                return false;
            }
        }

        internal async Task UpdateResponses()
        {
            string latestResponsesVersionStr = _latestResponsesVersion.ToString();
            string fileUrl = "https://github.com/Bedrock-Cosmos/Responses/archive/refs/tags/" + latestResponsesVersionStr + ".zip";
            string downloadPath = PathDefinitions.CosmosAppData + @"main.zip";
            string extractPath = PathDefinitions.CosmosAppData;

            if (_launchButton != null)
                UpdateLaunchButtonText(LanguageHandler.Get("Home.LaunchButton.Updating"));

            try
            {
                await _asyncFileOps.DownloadFileAsync(fileUrl, downloadPath);
                await _asyncFileOps.ExtractFileAsync(downloadPath, extractPath, true);
                if (Directory.Exists(PathDefinitions.ResponsesDirectory))
                {
                    await Task.Run(() =>
                    {
                        Directory.Delete(PathDefinitions.ResponsesDirectory, true);
                    });
                }

                if (!Directory.Exists(PathDefinitions.ResponsesDirectory))
                    Directory.Move(PathDefinitions.CosmosAppData + "Responses-" + latestResponsesVersionStr, PathDefinitions.ResponsesDirectory);
                else // Workaround if old directory was not deleted due to accessing elsewhere
                    await _asyncFileOps.MoveFolderContentsAsync(PathDefinitions.CosmosAppData + "Responses-" + latestResponsesVersionStr, PathDefinitions.ResponsesDirectory, true);

                File.WriteAllText(PathDefinitions.MiscDirectory + @"ResponsesVersion.txt", latestResponsesVersionStr);
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
                CosmosConsole.WriteLine("Unable to launch Minecraft (ensure that the game is installed).");
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
                UpdateLaunchButtonText(LanguageHandler.Get("Home.LaunchButton.Launch"));
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
