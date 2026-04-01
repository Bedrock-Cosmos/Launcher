using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
    public static class LanguageHandler
    {
        // For Lanuage Lookup
        private static readonly Dictionary<string, string> LanguageDict = new Dictionary<string, string>
        {
            { "English", "en_US" },
            { "বাংলা", "bn_BD" },
            { "Español", "es_ES" },
            { "Indonesia", "id_ID" },
            { "日本語", "ja_JP" },
            { "Tiếng Việt", "vi_VN" }
        };

        // App
        public static string App_TopLabel_Name { get; set; }

        // Home - Launch Button
        public static string Home_LaunchButton_Launch { get; set; }
        public static string Home_LaunchButton_Updating { get; set; }
        public static string Home_LaunchButton_Entering { get; set; }
        public static string Home_LaunchButton_Running { get; set; }
        public static string Home_LaunchButton_Listening { get; set; }

        // Home - Status Label
        public static string Home_StatusLabel_Waiting { get; set; }
        public static string Home_StatusLabel_NoInternet { get; set; }
        public static string Home_StatusLabel_ProxyDisabled { get; set; }
        public static string Home_StatusLabel_ProxyEnabled { get; set; }

        // About
        public static string About_AboutLabel_Text { get; set; }
        public static string About_DiscordLabel_Text { get; set; }
        public static string About_GitHubLabel_Text { get; set; }
        public static string About_WebsiteLabel_Text { get; set; }

        // Settings
        public static string Settings_BackgroundMode_Title { get; set; }
        public static string Settings_BackgroundMode_Description { get; set; }
        public static string Settings_Language_Title { get; set; }
        public static string Settings_Language_Description { get; set; }

        // Update
        public static string Update_UpdateLabel_Text { get; set; }
        public static string Update_ChangelogLabel_Text { get; set; }
        public static string Update_UpdateButton_Text { get; set; }
        public static string Update_CancelUpdateButton_Text { get; set; }

        public static void Load(string path)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] split = line.Split(new[] { '=' }, 2);
                if (split.Length != 2)
                    continue;

                string key = split[0].Trim().Replace(".", "_");
                string value = split[1].Replace("\\n", "\n").Trim();

                PropertyInfo prop = typeof(LanguageHandler).GetProperty(
                    key,
                    BindingFlags.Static | BindingFlags.Public
                );

                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(null, value);
                }
            }
        }

        public static string GetLangFileName(string selectedLang)
        {
            return LanguageDict.ContainsKey(selectedLang) ? LanguageDict[selectedLang] : "en_US";
        }

        public static string GetLanguageName(string selectedLang)
        {
            var languageKey = LanguageDict.FirstOrDefault(x => x.Value == selectedLang).Key;
            return string.IsNullOrEmpty(languageKey) ? "English" : languageKey;
        }
    }
}