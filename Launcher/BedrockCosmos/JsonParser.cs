using BedrockCosmos.App;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos
{
    internal class JsonParser
    {
        private static string consoleSender = "Parser";

        internal static string ReadJsonFileContent(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserFileNotFound", filePath));
                return string.Empty;
            }
        }

        internal static string AppendJsonToStart(string originalJsonContent, string jsonToAppendPath, string appendLocation)
        {
            // Appends to position 0, the start of the json
            string appendedJson = AppendJsonToSpecificLocation(originalJsonContent, jsonToAppendPath, appendLocation, 0);
            return appendedJson;
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
                    CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserArrayNotFound", appendLocation));
                    return string.Empty;
                }
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserFileNotFound", jsonToAppendPath));
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
                    CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserArrayNotFound", appendLocation));
                    return string.Empty;
                }
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserFileNotFound", jsonToAppendPath));
                return string.Empty;
            }
        }

        internal static string AppendJsonToSpecificRows(string originalJsonContent, string jsonToAppendPath, string appendLocation, int position)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);

                JObject originalJson = JObject.Parse(originalJsonContent);
                JObject jsonToAppend = JObject.Parse(jsonToAppendContent);

                // Navigate to ["result"]["layout"][position]["rows"]
                JArray rowsArray = originalJson["result"]?["layout"]?[position]?["rows"] as JArray;

                if (rowsArray == null)
                {
                    CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserArrayNotFound", string.Format("result.layout[{0}].rows", position)));
                    return string.Empty;
                }

                rowsArray.Insert(position, jsonToAppend);
                return originalJson.ToString();
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserFileNotFound", jsonToAppendPath));
                return string.Empty;
            }
        }

        internal static string AppendJsonToSkinPackMenu(string originalJsonContent, string jsonToAppendPath)
        {
            string dividerPath = PathDefinitions.ResponsesDirectory + @"MainPages\VerticalLineDivider_append.json";

            if (File.Exists(jsonToAppendPath))
            {
                string dividerToAppendContent = File.ReadAllText(dividerPath); // Method includes divider for skins menu
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);
                JObject originalJson = JObject.Parse(originalJsonContent);
                JObject dividerToAppend = JObject.Parse(dividerToAppendContent);
                JObject jsonToAppend = JObject.Parse(jsonToAppendContent);
                JArray targetArray = originalJson["result"]?["layout"]?[0]?["rows"] as JArray;

                if (targetArray != null)
                {
                    targetArray.Insert(3, dividerToAppend);  // Insert divider at position
                    targetArray.Insert(4, jsonToAppend);  // Insert new content at position
                    string updatedJson = originalJson.ToString();
                    return updatedJson;
                }
                else
                {
                    CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserArrayNotFound", "result.layout[0].rows"));
                    return string.Empty;
                }
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, LanguageHandler.Format("Logs.ParserFileNotFound", jsonToAppendPath));
                return string.Empty;
            }
        }

        // Not used right now, may be in the future
        /*internal static string AppendJsonToPersonaProfile(string originalJsonContent, string jsonToAppendPath)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);

                JObject originalJson = JObject.Parse(originalJsonContent);
                JObject jsonToAppend = JObject.Parse(jsonToAppendContent);
                JArray rowsArray = (JArray)originalJson.SelectToken("result.rows");

                if (rowsArray != null)
                {
                    // Find the SkinPackList inside the rows array (search by controlId)
                    JObject storeRow = rowsArray
                        .FirstOrDefault(row => row["controlId"]?.ToString() == "StoreRow") as JObject;

                    if (storeRow != null)
                    {
                        // Find the 'items' array inside the SkinPackList
                        JArray itemsArray = (JArray)storeRow.SelectToken("components[?(@.type == 'itemListComp')].items");

                        if (itemsArray != null)
                        {
                            // Append the new content to the 'items' array
                            itemsArray.Insert(0, jsonToAppend); // Append content

                            // Convert the modified JSON back to string
                            string updatedJson = originalJson.ToString();
                            return updatedJson;
                        }
                        else
                        {
                            Console.WriteLine("Could not find 'items' array in 'StoreRow'.");
                            return string.Empty;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find 'StoreRow' in 'result.rows'.");
                        return string.Empty;
                    }
                }
                else
                {
                    Console.WriteLine("Could not find 'result.rows' in the JSON.");
                    return string.Empty;
                }
            }
            else
            {
                Console.WriteLine($"File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }*/

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
