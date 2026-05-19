using BedrockCosmos.App;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
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
                CosmosConsole.WriteLine(consoleSender, $"File not found: {filePath}");
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
                JsonObject originalJson = JsonNode.Parse(originalJsonContent)?.AsObject();
                JsonObject jsonToAppend = JsonNode.Parse(jsonToAppendContent)?.AsObject();
                JsonArray targetArray = SelectTokenAsArray(originalJson, appendLocation);

                if (targetArray != null)
                {
                    targetArray.Add(JsonNode.Parse(jsonToAppend.ToJsonString())); // Deep clone before inserting
                    return originalJson.ToJsonString();
                }
                else
                {
                    CosmosConsole.WriteLine(consoleSender, $"Could not find array at path: {appendLocation} in {originalJsonContent}");
                    return string.Empty;
                }
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, $"File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }

        internal static string AppendJsonToSpecificLocation(string originalJsonContent, string jsonToAppendPath, string appendLocation, int position)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);
                JsonObject originalJson = JsonNode.Parse(originalJsonContent)?.AsObject();
                JsonObject jsonToAppend = JsonNode.Parse(jsonToAppendContent)?.AsObject();
                JsonArray targetArray = SelectTokenAsArray(originalJson, appendLocation);

                if (targetArray != null)
                {
                    targetArray.Insert(position, JsonNode.Parse(jsonToAppend.ToJsonString())); // Deep clone before inserting
                    return originalJson.ToJsonString();
                }
                else
                {
                    CosmosConsole.WriteLine(consoleSender, $"Could not find array at path: {appendLocation} in {originalJsonContent}");
                    return string.Empty;
                }
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, $"File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }

        internal static string AppendJsonToSpecificRows(string originalJsonContent, string jsonToAppendPath, int position)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);

                JsonObject originalJson = JsonNode.Parse(originalJsonContent)?.AsObject();
                JsonObject jsonToAppend = JsonNode.Parse(jsonToAppendContent)?.AsObject();

                // Navigate to ["result"]["layout"][position]["rows"]
                JsonArray rowsArray = originalJson?["result"]?["layout"]?[position]?["rows"]?.AsArray();

                if (rowsArray == null)
                {
                    CosmosConsole.WriteLine(consoleSender, $"Could not find array at path: 'result.layout[{position}].rows' in {originalJsonContent}");
                    return string.Empty;
                }

                rowsArray.Insert(position, JsonNode.Parse(jsonToAppend.ToJsonString())); // Deep clone before inserting
                return originalJson.ToJsonString();
            }
            else
            {
                Console.WriteLine($"File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }

        internal static string AppendNews(string originalJsonContent)
        {
            string bannerDataPath = PathDefinitions.CustomJsonsDirectory + @"CurrentLoginAnnouncement.json";

            // Extra handling to ensure default news still displays
            if (string.IsNullOrWhiteSpace(originalJsonContent))
            {
                CosmosConsole.WriteLine(consoleSender, "Skipping session-start news append because the upstream response body was empty.");
                return originalJsonContent;
            }

            if (!LooksLikeJson(originalJsonContent))
            {
                CosmosConsole.WriteLine(consoleSender, $"Skipping session-start news append because the upstream response did not look like JSON. Response starts with: {GetContentSnippet(originalJsonContent)}");
                return originalJsonContent;
            }

            JsonObject originalJson = JsonNode.Parse(originalJsonContent)?.AsObject();
            JsonArray announcementTargetArray = originalJson?["result"]?["messages"]?.AsArray();
            JsonArray inboxTargetArray = originalJson?["result"]?["inboxSummary"]?["categories"]?.AsArray();

            try
            {
                // Should ignore saving news if an official Minecraft announcement is present
                // May need more testing
                bool loginAnnouncementExists = false;
                if (announcementTargetArray != null)
                {
                    foreach (JsonNode node in announcementTargetArray)
                    {
                        JsonObject msg = node?.AsObject();
                        if (msg != null && msg["surface"]?.GetValue<string>() == "LoginAnnouncement")
                        {
                            loginAnnouncementExists = true;
                            break;
                        }
                    }
                }

                if (!loginAnnouncementExists && NewsManager.IsCurrentNewsNew())
                {
                    // Append front announcement
                    if (NewsManager.SendToNewsAnnouncement)
                    {
                        JsonObject bannerJson = JsonNode.Parse(File.ReadAllText(bannerDataPath))?.AsObject();
                        announcementTargetArray?.Add(JsonNode.Parse(bannerJson.ToJsonString())); // Deep clone before inserting
                    }

                    if (NewsManager.SendToNewsInbox)
                        NewsManager.AddNewsToHistory();

                    NewsManager.MarkCurrentNewsAsSeen();
                }
                else if (loginAnnouncementExists)
                {
                    CosmosConsole.WriteLine(consoleSender, "Skipped appending and saving news data since an official Minecraft announcement is present.");
                }

                // Append Cosmos inbox
                JsonObject inboxJson = JsonNode.Parse(File.ReadAllText(NewsManager.NewsHistoryPath))?.AsObject();
                inboxTargetArray?.Insert(0, JsonNode.Parse(inboxJson.ToJsonString())); // Deep clone before inserting
                string updatedJson = originalJson?.ToJsonString();
                return string.IsNullOrWhiteSpace(updatedJson) ? originalJsonContent : updatedJson;
            }
            catch (JsonException ex)
            {
                CosmosConsole.WriteLine(consoleSender, $"Skipping session-start news append because the upstream response could not be parsed as JSON. Response starts with: {GetContentSnippet(originalJsonContent)} Error: {ex.Message}");
                return originalJsonContent;
            }
        }

        internal static string AppendJsonToPersonaMenu(string originalJsonContent, string jsonToAppendPath)
        {
            string featuredItemsPath = PathDefinitions.ResponsesDirectory + @"MainPages\PersonaCategories\FeaturedPersonaItems_append.json";

            if (File.Exists(jsonToAppendPath))
            {
                string featuredItemsToAppendContent = File.ReadAllText(featuredItemsPath);
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);
                JsonObject originalJson = JsonNode.Parse(originalJsonContent)?.AsObject();
                JsonObject featuredItemsToAppend = JsonNode.Parse(featuredItemsToAppendContent)?.AsObject();
                JsonObject jsonToAppend = JsonNode.Parse(jsonToAppendContent)?.AsObject();
                JsonArray targetArray = originalJson?["result"]?["layout"]?[0]?["rows"]?.AsArray();
                JsonArray featuredItemsArray = featuredItemsToAppend?["rows"]?.AsArray();
                JsonArray appendArray = jsonToAppend?["rows"]?.AsArray();

                if (targetArray != null)
                {
                    foreach (JsonNode row in featuredItemsArray)
                    {
                        // Needs to be placed after dropdownId -1 for some reason or else skins break
                        // Deep clone required — a JsonNode can only belong to one parent at a time
                        targetArray.Insert(1, JsonNode.Parse(row.ToJsonString()));
                    }

                    int insertIndex = -1;
                    for (int i = 0; i < targetArray.Count; i++)
                    {
                        JsonObject rowObj = targetArray[i]?.AsObject();
                        if (rowObj != null && rowObj["controlId"]?.GetValue<string>() == "LightDropdown")
                        {
                            insertIndex = i;
                            break;
                        }
                    }

                    if (insertIndex != -1)
                    {
                        foreach (JsonNode row in appendArray)
                        {
                            targetArray.Insert(insertIndex, JsonNode.Parse(row.ToJsonString())); // Deep clone before inserting
                            insertIndex++;
                        }
                    }
                    else
                    {
                        foreach (JsonNode row in appendArray)
                        {
                            targetArray.Add(JsonNode.Parse(row.ToJsonString())); // Deep clone before inserting
                        }
                    }

                    return originalJson.ToJsonString();
                }
                else
                {
                    CosmosConsole.WriteLine(consoleSender, $"Could not find array at path: 'result.layout[0].rows' in {originalJsonContent}");
                    return string.Empty;
                }
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, $"File not found: {jsonToAppendPath}");
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
                JsonObject originalJson = JsonNode.Parse(originalJsonContent)?.AsObject();
                JsonObject dividerToAppend = JsonNode.Parse(dividerToAppendContent)?.AsObject();
                JsonObject jsonToAppend = JsonNode.Parse(jsonToAppendContent)?.AsObject();
                JsonArray targetArray = originalJson?["result"]?["layout"]?[0]?["rows"]?.AsArray();

                if (targetArray != null)
                {
                    targetArray.Insert(3, JsonNode.Parse(dividerToAppend.ToJsonString())); // Deep clone before inserting
                    targetArray.Insert(4, JsonNode.Parse(jsonToAppend.ToJsonString()));    // Deep clone before inserting
                    return originalJson.ToJsonString();
                }
                else
                {
                    CosmosConsole.WriteLine(consoleSender, $"Could not find array at path: 'result.layout[0].rows' in {originalJsonContent}");
                    return string.Empty;
                }
            }
            else
            {
                CosmosConsole.WriteLine(consoleSender, $"File not found: {jsonToAppendPath}");
                return string.Empty;
            }
        }

        // Not used right now, may be in the future
        /*internal static string AppendJsonToPersonaProfile(string originalJsonContent, string jsonToAppendPath)
        {
            if (File.Exists(jsonToAppendPath))
            {
                string jsonToAppendContent = File.ReadAllText(jsonToAppendPath);

                JsonObject originalJson = JsonNode.Parse(originalJsonContent)?.AsObject();
                JsonObject jsonToAppend = JsonNode.Parse(jsonToAppendContent)?.AsObject();
                JsonArray rowsArray = originalJson?["result"]?["rows"]?.AsArray();

                if (rowsArray != null)
                {
                    // Find the StoreRow inside the rows array (search by controlId)
                    JsonObject storeRow = null;
                    foreach (JsonNode node in rowsArray)
                    {
                        JsonObject row = node?.AsObject();
                        if (row != null && row["controlId"]?.GetValue<string>() == "StoreRow")
                        {
                            storeRow = row;
                            break;
                        }
                    }

                    if (storeRow != null)
                    {
                        // Find the 'items' array inside the component with type == 'itemListComp'
                        JsonArray components = storeRow["components"]?.AsArray();
                        JsonArray itemsArray = null;
                        if (components != null)
                        {
                            foreach (JsonNode node in components)
                            {
                                JsonObject comp = node?.AsObject();
                                if (comp != null && comp["type"]?.GetValue<string>() == "itemListComp")
                                {
                                    itemsArray = comp["items"]?.AsArray();
                                    break;
                                }
                            }
                        }

                        if (itemsArray != null)
                        {
                            itemsArray.Insert(0, JsonNode.Parse(jsonToAppend.ToJsonString()));
                            return originalJson.ToJsonString();
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

        // Replaces Newtonsoft's SelectToken() for simple dot-notation paths on a JsonObject.
        // Supports dot-separated keys and integer array indices, e.g. "result.layout.0.rows".
        // Does not support JSONPath wildcards or filter expressions.
        private static JsonArray SelectTokenAsArray(JsonObject root, string path)
        {
            if (root == null || string.IsNullOrWhiteSpace(path))
                return null;

            // Normalise: "result.layout[0].rows" -> "result.layout.0.rows"
            path = Regex.Replace(path, @"\[(\d+)\]", ".$1").TrimStart('.');

            string[] segments = path.Split('.');
            JsonNode current = root;

            foreach (string segment in segments)
            {
                if (current == null)
                    return null;

                int index;
                if (int.TryParse(segment, out index))
                    current = current.AsArray()?[index];
                else
                    current = current.AsObject()?[segment];
            }

            return current?.AsArray();
        }

        private static bool LooksLikeJson(string content)
        {
            string trimmedContent = content.TrimStart();
            return trimmedContent.StartsWith("{") || trimmedContent.StartsWith("[");
        }

        private static string GetContentSnippet(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "[EMPTY]";

            string singleLine = content.TrimStart().Replace("\r", " ").Replace("\n", " ");
            return singleLine.Length <= 80 ? singleLine : singleLine.Substring(0, 80) + "...";
        }
    }
}