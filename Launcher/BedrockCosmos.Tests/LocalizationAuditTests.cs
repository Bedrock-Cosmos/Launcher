using Xunit;
using System.Text.RegularExpressions;

namespace BedrockCosmos.Tests
{
    public class LocalizationAuditTests
    {
        [Fact]
        public void GermanTranslation_ContainsAllEnglishKeys()
        {
            string launcherRoot = GetLauncherRoot();
            var english = ParseLangFile(Path.Combine(launcherRoot, "Texts", "en_US.lang"));
            var german = ParseLangFile(Path.Combine(launcherRoot, "Texts", "de_DE.lang"));

            var missingInGerman = english.Keys.Except(german.Keys).OrderBy(key => key).ToArray();
            var extraInGerman = german.Keys.Except(english.Keys).OrderBy(key => key).ToArray();

            Assert.True(missingInGerman.Length == 0, "Missing German keys: " + string.Join(", ", missingInGerman));
            Assert.True(extraInGerman.Length == 0, "Unexpected German-only keys: " + string.Join(", ", extraInGerman));
        }

        [Fact]
        public void RuntimeSource_DoesNotContainHardcodedVisibleStrings()
        {
            string launcherRoot = GetLauncherRoot();
            string[] sourceFiles = Directory.GetFiles(launcherRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains(Path.Combine("Properties", "Resources.Designer.cs")))
                .ToArray();

            var violations = new List<string>();
            var patterns = new Dictionary<string, Regex>
            {
                { "MessageBox.Show", new Regex(@"MessageBox\.Show\(\s*(\$)?""[^""]*[A-Za-z][^""]*""", RegexOptions.Compiled) },
                { "CosmosConsole.WriteLine", new Regex(@"CosmosConsole\.WriteLine\(\s*(\$)?""[^""]*[A-Za-z][^""]*""\s*\)", RegexOptions.Compiled) },
                { ".Text =", new Regex(@"\.Text\s*=\s*(\$)?""[^""]*[A-Za-z][^""]*""", RegexOptions.Compiled) },
                { "Label =", new Regex(@"Label\s*=\s*(\$)?""[^""]*[A-Za-z][^""]*""", RegexOptions.Compiled) },
                { "Details =", new Regex(@"Details\s*=\s*""[^""]*[A-Za-z][^""]*""", RegexOptions.Compiled) },
                { "State =", new Regex(@"State\s*=\s*""[^""]*[A-Za-z][^""]*""", RegexOptions.Compiled) }
            };

            foreach (string file in sourceFiles)
            {
                string content = File.ReadAllText(file);
                foreach (var pattern in patterns)
                {
                    if (pattern.Value.IsMatch(content) && !IsAllowedTechnicalLiteral(file, pattern.Key, content))
                        violations.Add(Path.GetFileName(file) + " => " + pattern.Key);
                }
            }

            Assert.True(violations.Count == 0, "Potential hardcoded UI strings found: " + string.Join(" | ", violations));
        }

        private static string GetLauncherRoot()
        {
            string current = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(current))
            {
                string candidate = Path.Combine(current, "BedrockCosmos");
                if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "BedrockCosmos.csproj")))
                    return candidate;

                current = Path.GetDirectoryName(current);
            }

            throw new DirectoryNotFoundException("Unable to locate the launcher source root.");
        }

        private static Dictionary<string, string> ParseLangFile(string path)
        {
            return File.ReadAllLines(path)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Where(line => !line.StartsWith("#", StringComparison.Ordinal))
                .Select(line => line.Split(new[] { '=' }, 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsAllowedTechnicalLiteral(string file, string patternKey, string content)
        {
            if (patternKey == ".Text ="
                && Path.GetFileName(file).Equals("LaunchManager.cs", StringComparison.OrdinalIgnoreCase)
                && content.Contains("_versionLabel.Text = $\"v{", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
    }
}
