using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BedrockCosmos.App
{
    public static class LanguageHandler
    {
        private static readonly KeyValuePair<string, string>[] Languages =
        {
            new KeyValuePair<string, string>("English", "en_US"),
            new KeyValuePair<string, string>("Deutsch", "de_DE"),
            new KeyValuePair<string, string>("Espa\u00f1ol", "es_ES"),
            new KeyValuePair<string, string>("Indonesia", "id_ID"),
            new KeyValuePair<string, string>("\u65e5\u672c\u8a9e", "ja_JP")
        };

        private static readonly Dictionary<string, string> LanguageDict = Languages
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        private static readonly Dictionary<string, string> DefaultStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> ActiveStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string CurrentLanguage { get; private set; } = "en_US";

        public static string App_TopLabel_Name { get { return Get("App.TopLabel.Name"); } }

        public static string Home_LaunchButton_Launch { get { return Get("Home.LaunchButton.Launch"); } }
        public static string Home_LaunchButton_Updating { get { return Get("Home.LaunchButton.Updating"); } }
        public static string Home_LaunchButton_Entering { get { return Get("Home.LaunchButton.Entering"); } }
        public static string Home_LaunchButton_Running { get { return Get("Home.LaunchButton.Running"); } }
        public static string Home_LaunchButton_Listening { get { return Get("Home.LaunchButton.Listening"); } }

        public static string Home_StatusLabel_Waiting { get { return Get("Home.StatusLabel.Waiting"); } }
        public static string Home_StatusLabel_NoInternet { get { return Get("Home.StatusLabel.NoInternet"); } }
        public static string Home_StatusLabel_ProxyDisabled { get { return Get("Home.StatusLabel.ProxyDisabled"); } }
        public static string Home_StatusLabel_ProxyEnabled { get { return Get("Home.StatusLabel.ProxyEnabled"); } }

        public static string About_AboutLabel_Text { get { return Get("About.AboutLabel.Text"); } }
        public static string About_DiscordLabel_Text { get { return Get("About.DiscordLabel.Text"); } }
        public static string About_GitHubLabel_Text { get { return Get("About.GitHubLabel.Text"); } }
        public static string About_WebsiteLabel_Text { get { return Get("About.WebsiteLabel.Text"); } }

        public static string Settings_BackgroundMode_Title { get { return Get("Settings.BackgroundMode.Title"); } }
        public static string Settings_BackgroundMode_Description { get { return Get("Settings.BackgroundMode.Description"); } }
        public static string Settings_Language_Title { get { return Get("Settings.Language.Title"); } }
        public static string Settings_Language_Description { get { return Get("Settings.Language.Description"); } }

        public static string Update_UpdateLabel_Text { get { return Get("Update.UpdateLabel.Text"); } }
        public static string Update_ChangelogLabel_Text { get { return Get("Update.ChangelogLabel.Text"); } }
        public static string Update_UpdateButton_Text { get { return Get("Update.UpdateButton.Text"); } }
        public static string Update_CancelUpdateButton_Text { get { return Get("Update.CancelUpdateButton.Text"); } }

        public static void Load(string languageOrPath)
        {
            string languageCode = NormalizeLanguageCode(languageOrPath);
            CurrentLanguage = languageCode;

            DefaultStrings.Clear();
            ActiveStrings.Clear();

            LoadFileIntoDictionary(GetLanguageFilePath("en_US"), DefaultStrings);
            CopyStrings(DefaultStrings, ActiveStrings);

            if (!string.Equals(languageCode, "en_US", StringComparison.OrdinalIgnoreCase))
                LoadFileIntoDictionary(GetLanguageFilePath(languageCode), ActiveStrings);

            WriteMissingTranslationReport(languageCode);
        }

        public static string Get(string key)
        {
            string value;
            if (ActiveStrings.TryGetValue(key, out value))
                return value;

#if DEBUG
            return "[[" + key + "]]";
#else
            if (DefaultStrings.TryGetValue(key, out value))
                return value;

            return key;
#endif
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        public static IReadOnlyCollection<string> GetAvailableLanguageNames()
        {
            return Languages.Select(pair => pair.Key).ToList().AsReadOnly();
        }

        public static IReadOnlyCollection<string> GetMissingKeys(string languageCode)
        {
            string normalizedLanguage = NormalizeLanguageCode(languageCode);
            var languageStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            LoadFileIntoDictionary(GetLanguageFilePath(normalizedLanguage), languageStrings);

            return DefaultStrings.Keys
                .Where(key => !languageStrings.ContainsKey(key))
                .OrderBy(key => key)
                .ToList()
                .AsReadOnly();
        }

        public static string GetLangFileName(string selectedLang)
        {
            return LanguageDict.ContainsKey(selectedLang) ? LanguageDict[selectedLang] : "en_US";
        }

        public static string GetLanguageName(string selectedLang)
        {
            string languageKey = Languages.FirstOrDefault(x => string.Equals(x.Value, selectedLang, StringComparison.OrdinalIgnoreCase)).Key;
            return string.IsNullOrEmpty(languageKey) ? "English" : languageKey;
        }

        private static void CopyStrings(Dictionary<string, string> source, Dictionary<string, string> destination)
        {
            foreach (var pair in source)
                destination[pair.Key] = pair.Value;
        }

        private static string NormalizeLanguageCode(string languageOrPath)
        {
            if (string.IsNullOrWhiteSpace(languageOrPath))
                return "en_US";

            if (languageOrPath.EndsWith(".lang", StringComparison.OrdinalIgnoreCase))
                return Path.GetFileNameWithoutExtension(languageOrPath);

            return languageOrPath;
        }

        private static string GetLanguageFilePath(string languageCode)
        {
            return Path.Combine(PathDefinitions.AppDirectory, "Texts", languageCode + ".lang");
        }

        private static void LoadFileIntoDictionary(string path, IDictionary<string, string> destination)
        {
            if (!File.Exists(path))
                return;

            foreach (string rawLine in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith("#"))
                    continue;

                string[] split = rawLine.Split(new[] { '=' }, 2);
                if (split.Length != 2)
                    continue;

                destination[split[0].Trim()] = split[1].Replace("\\n", "\n").Trim();
            }
        }

        private static void WriteMissingTranslationReport(string languageCode)
        {
            try
            {
                if (!Directory.Exists(PathDefinitions.MiscDirectory))
                    Directory.CreateDirectory(PathDefinitions.MiscDirectory);

                string path = Path.Combine(PathDefinitions.MiscDirectory, "MissingTranslations." + languageCode + ".txt");
                var missingKeys = GetMissingKeys(languageCode);
                File.WriteAllLines(path, missingKeys);
            }
            catch
            {
                // Missing-key reporting must never block the launcher.
            }
        }
    }
}
