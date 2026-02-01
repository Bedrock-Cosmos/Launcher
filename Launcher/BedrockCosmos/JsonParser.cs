using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BedrockCosmos
{
    internal class JsonParser
    {
        internal static string ReadJsonFileContent(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                Console.WriteLine($"[!] File not found: {filePath}");
                return string.Empty;
            }
        }

        internal static string AppendJsonToStart(string originalJsonContent, string jsonToAppendPath, string appendLocation)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);
                JObject originalJson = JObject.Parse(originalJsonContent);
                JObject jsonToAppend = JObject.Parse(jsonToAppendContent);
                JArray targetArray = (JArray)originalJson.SelectToken(appendLocation);

                if (targetArray != null)
                {
                    targetArray.Insert(0, jsonToAppend);  // Insert new content at the beginning
                    string updatedJson = originalJson.ToString();
                    return updatedJson;
                }
                else
                {
                    Console.WriteLine($"[!] Could not find array at path: {appendLocation} in {originalJsonContent}");
                    return string.Empty;
                }
            }
            else
            {
                Console.WriteLine($"[!] File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }

        internal static string AppendJsonToEnd(string originalJsonContent, string jsonToAppendPath, string appendLocation)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);
                JObject originalJson = JObject.Parse(originalJsonContent);
                JObject jsonToAppend = JObject.Parse(jsonToAppendContent);
                JArray targetArray = (JArray)originalJson.SelectToken(appendLocation);

                if (targetArray != null)
                {
                    targetArray.Add(jsonToAppend); // Insert new content at the end
                    string updatedJson = originalJson.ToString();
                    return updatedJson;
                }
                else
                {
                    Console.WriteLine($"[!] Could not find array at path: {appendLocation} in {originalJsonContent}");
                    return string.Empty;
                }
            }
            else
            {
                Console.WriteLine($"[!] File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }

        internal static string AppendJsonToSpecificLocation(string originalJsonContent, string jsonToAppendPath, string appendLocation, int position)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);
                JObject originalJson = JObject.Parse(originalJsonContent);
                JObject jsonToAppend = JObject.Parse(jsonToAppendContent);
                JArray targetArray = (JArray)originalJson.SelectToken(appendLocation);

                if (targetArray != null)
                {
                    targetArray.Insert(position, jsonToAppend);  // Insert new content at position
                    string updatedJson = originalJson.ToString();
                    return updatedJson;
                }
                else
                {
                    Console.WriteLine($"[!] Could not find array at path: {appendLocation} in {originalJsonContent}");
                    return string.Empty;
                }
            }
            else
            {
                Console.WriteLine($"[!] File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }

        internal static string ExtractPlayfabSearchId(string originalPlayfabData)
        {
            // UUID Pattern
            string pattern = @"\b[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}\b";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(originalPlayfabData);

            if (match.Success)
                return match.Value;
            else
                return string.Empty;
        }
    }
}
