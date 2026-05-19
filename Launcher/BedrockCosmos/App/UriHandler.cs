using BedrockCosmos.App;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

internal static class UriHandler
{
    private static readonly JsonSerializerOptions _jsonWriteIndented = new JsonSerializerOptions { WriteIndented = true };

    internal static string Handle(string uri)
    {
        try
        {
            // Expected formats:
            // bedrockcosmos://openStore/?showStoreOffer=UUID
            // bedrockcosmos://showDressingRoomOffer/?offerID=UUID
            // bedrockcosmos://showDressingRoomOffer/?offerID=UUID/?creator=CreatorName

            if (uri.EndsWith("/"))
                uri = uri.Remove(uri.Length - 1);

            if (!uri.StartsWith("bedrockcosmos://", StringComparison.OrdinalIgnoreCase))
                return "";

            string path = uri.Substring("bedrockcosmos://".Length);

            if (path.StartsWith("/"))
                path = path.Substring("/".Length);

            CosmosConsole.WriteLine($"Opened URI path: {path}");

            // openStore?showStoreOffer=UUID
            if (path.StartsWith("openStore", StringComparison.OrdinalIgnoreCase))
            {
                string uuid = "";

                if (path.StartsWith("openStore/?showStoreOffer=", StringComparison.OrdinalIgnoreCase))
                    uuid = path.Substring("openStore/?showStoreOffer=".Length).Trim();

                if (!string.IsNullOrEmpty(uuid))
                    return $"?showStoreOffer={uuid}"; // Removed openStore for now since it was causing some issues
                else
                    return "";
            }

            // showDressingRoomOffer?offerID=UUID[?creator=CreatorName]
            if (path.StartsWith("showDressingRoomOffer", StringComparison.OrdinalIgnoreCase))
            {
                string query = path.Substring("showDressingRoomOffer".Length);
                query = query.TrimStart('/', '?');

                string offerID = null;
                string creatorName = null;

                string[] parts = query.Split(new string[] { "/?" }, StringSplitOptions.None);
                foreach (string part in parts)
                {
                    if (part.StartsWith("offerID=", StringComparison.OrdinalIgnoreCase))
                        offerID = part.Substring("offerID=".Length).Trim();
                    else if (part.StartsWith("creator=", StringComparison.OrdinalIgnoreCase))
                        creatorName = part.Substring("creator=".Length).Trim();
                }

                if (string.IsNullOrEmpty(offerID))
                    return "";

                string dro = HandleDressingRoomOffer(offerID, creatorName);
                if (!string.IsNullOrEmpty(dro))
                    return dro;
                else
                    return "";
            }

            return "";
        }
        catch (Exception ex)
        {
            CosmosConsole.WriteLine($"URI handler error: {ex.Message}");
            return "";
        }
    }

    private static string HandleDressingRoomOffer(string offerID, string creatorName)
    {
        // Determine which JSON file to search
        string jsonPath;
        if (!string.IsNullOrEmpty(creatorName))
        {
            jsonPath = Path.Combine(
                PathDefinitions.ResponsesDirectory,
                "MainPages", "Creators", "Persona",
                $"{creatorName}_Persona.json"
            );
        }
        else
        {
            jsonPath = Path.Combine(
                PathDefinitions.ResponsesDirectory,
                "MainPages", "Capes.json"
            );
        }

        JsonObject foundItem = FindItemInJson(jsonPath, offerID);

        if (foundItem != null)
        {
            WritePersonaItemPreviewJson(foundItem, offerID);
            return $"showDressingRoomOffer/?offerID=fa359c7a-889b-4ce1-9d68-08691ca7303c";
        }
        else
        {
            return "";
        }
    }

    // Searches in capes file for the ID that matches the URI's UUID.
    private static JsonObject FindItemInJson(string jsonPath, string offerID)
    {
        if (!File.Exists(jsonPath))
            return null;

        JsonObject root;
        try
        {
            string raw = File.ReadAllText(jsonPath);
            root = JsonNode.Parse(raw)?.AsObject();
        }
        catch
        {
            return null;
        }

        if (root == null)
            return null;

        // Check all GridLists in file
        JsonArray rows = root["result"]?["layout"]?[0]?["rows"]?.AsArray();
        if (rows == null)
            return null;

        foreach (JsonNode row in rows)
        {
            if (row == null)
                continue;

            string controlId = row["controlId"]?.GetValue<string>();
            if (!string.Equals(controlId, "GridList", StringComparison.OrdinalIgnoreCase))
                continue;

            JsonArray components = row["components"]?.AsArray();
            if (components == null)
                continue;

            foreach (JsonNode component in components)
            {
                if (component == null)
                    continue;

                JsonArray items = component["items"]?.AsArray();
                if (items == null)
                    continue;

                foreach (JsonNode item in items)
                {
                    if (item == null)
                        continue;

                    string id = item["id"]?.GetValue<string>();
                    if (string.Equals(id, offerID, StringComparison.OrdinalIgnoreCase))
                        return item.AsObject();
                }
            }
        }

        return null;
    }

    // Creates PersonaItemPreview.json to use when loading the persona item.
    private static void WritePersonaItemPreviewJson(JsonObject foundItem, string offerID)
    {
        // Build the preview JSON structure
        JsonNode preview = JsonNode.Parse(@"
        {
          ""result"": {
            ""layout"": [
              {
                ""sectionName"": ""rows"",
                ""rows"": [
                  {
                    ""controlId"": ""Layout"",
                    ""components"": [
                      { ""type"": ""personaOfferInteractionComp"", ""$type"": ""PersonaOfferInteractionComponent"" },
                      { ""type"": ""appearanceInteractionComp"", ""$type"": ""AppearanceInteractionComponent"" },
                      { ""type"": ""openColorPickerComp"", ""$type"": ""OpenColorPickerComponent"" },
                      { ""type"": ""openExpandedAppearanceViewComp"", ""$type"": ""OpenExpandedAppearanceViewComponent"" },
                      { ""type"": ""dispPreviewPieceComp"", ""$type"": ""DisplayPreviewedPieceOfferComponent"" },
                      { ""type"": ""sideSelectionComp"", ""$type"": ""PersonaSideSelection"" },
                      {
                        ""linksToInfo"": {
                          ""linksTo"": ""MultiItemPage_DressingRoomCoinScreen"",
                          ""linkType"": ""pageId"",
                          ""displayType"": ""store_layout.character_creator_screen"",
                          ""screenTitle"": {
                            ""value"": ""dr.header.minecoin.screen"",
                            ""style"": {
                              ""highlightColor"": [],
                              ""alignment"": ""Left"",
                              ""textColor"": [],
                              ""font"": ""MinecraftTen"",
                              ""showBackground"": false,
                              ""showOutline"": false,
                              ""indent"": 0.0,
                              ""buttonWidth"": 0.0,
                              ""color"": [],
                              ""offerControlIdType"": ""None"",
                              ""outlineColor"": []
                            },
                            ""replacements"": []
                          },
                          ""navigateInPlace"": false
                        },
                        ""isVisible"": true,
                        ""type"": ""topBarMinecoinComp"",
                        ""$type"": ""TopBarMinecoinComponent""
                      }
                    ]
                  },
                  {
                    ""controlId"": ""GridList"",
                    ""components"": [
                      {
                        ""items"": [],
                        ""totalItems"": 1,
                        ""type"": ""pagedItemListComp"",
                        ""$type"": ""PagedItemListComponent""
                      },
                      {
                        ""previewedId"": """",
                        ""type"": ""dispPreviewPieceComp"",
                        ""$type"": ""DisplayPreviewedPieceOfferComponent""
                      }
                    ]
                  }
                ]
              },
              { ""sectionName"": ""navigation"", ""rows"": [] }
            ]
          }
        }");

        // Navigate to layout[0].rows (sectionName: "rows") -> GridList row
        JsonArray rows = preview["result"]?["layout"]?[0]?["rows"]?.AsArray();
        JsonObject gridListRow = rows?[1]?.AsObject(); // controlId: "GridList"

        // Insert the found item into the GridList's items array
        // Deep clone required — a JsonNode can only belong to one parent at a time
        JsonArray gridListItems = gridListRow?["components"]?[0]?["items"]?.AsArray();
        if (gridListItems != null)
            gridListItems.Add(JsonNode.Parse(foundItem.ToJsonString()));

        // Set the previewedId (C# 7.0: no null-conditional on left side of assignment)
        JsonObject previewPieceComp = gridListRow?["components"]?[1]?.AsObject();
        if (previewPieceComp != null)
            previewPieceComp["previewedId"] = offerID;

        // Write to disk
        string outputPath = Path.Combine(PathDefinitions.CustomJsonsDirectory, "PersonaItemPreview.json");
        string outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        File.WriteAllText(outputPath, preview.ToJsonString(_jsonWriteIndented));
    }
}