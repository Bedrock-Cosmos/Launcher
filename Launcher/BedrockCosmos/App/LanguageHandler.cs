using BedrockCosmos.App.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private static readonly KeyValuePair<string, string>[] Languages =
        {
            new KeyValuePair<string, string>("English", "en_US"),
            new KeyValuePair<string, string>("বাংলা", "bn_BD"),
            new KeyValuePair<string, string>("Deutsch", "de_DE"),
            new KeyValuePair<string, string>("Español", "es_ES"),
            new KeyValuePair<string, string>("Indonesia", "id_ID"),
            new KeyValuePair<string, string>("日本語", "ja_JP"),
            new KeyValuePair<string, string>("Tiếng Việt", "vi_VN")
        };

        private static readonly Dictionary<string, string> LanguageDict = Languages
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        private static readonly Dictionary<string, string> DefaultStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> ActiveStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string CurrentLanguage { get; private set; } = "en_US";

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

            //WriteMissingTranslationReport(languageCode);
        }

        // Retrieve string from language dictionary using code in lang file.
        // Example: Home.LaunchButton.Launch
        public static string Get(string key, bool debug)
        {
            string value;
            if (ActiveStrings.TryGetValue(key, out value))
                return value;

            if (debug)
                return "[[" + key + "]]";

            if (DefaultStrings.TryGetValue(key, out value))
                return value;

            return key;
        }

        public static string Get(string key)
        {
            return Get(key, false);
        }

        // Similar to Get(), but uses formatting like {0}.
        // Example: Localization.Log.LanguageSet, lang
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

        public static void AddLangsToComboBox(GradientComboBox comboBox)
        {
            comboBox.Items.Clear();
            foreach (var pair in Languages)
                comboBox.Items.Add(pair.Key);
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