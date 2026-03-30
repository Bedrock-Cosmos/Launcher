using BedrockCosmos.App;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

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
    internal static string Handle(string uri)
    {
        try
        {
            // Expected formats:
            // bedrockcosmos://openStore?showStoreOffer=UUID
            // bedrockcosmos://showDressingRoomOffer?offerID=UUID
            // bedrockcosmos://showDressingRoomOffer?offerID=UUID/?creator=CreatorName

            if (uri.EndsWith("/"))
                uri = uri.Remove(uri.Length - 1);

            if (!uri.StartsWith("bedrockcosmos://", StringComparison.OrdinalIgnoreCase))
                return "";

            string path = uri.Substring("bedrockcosmos://".Length);

            if (path.StartsWith("/"))
                path = path.Substring("/".Length);

            CosmosConsole.WriteLine(LanguageHandler.Format("Logs.OpenedUriPath", path));

            // openStore?showStoreOffer=UUID
            if (path.StartsWith("openStore", StringComparison.OrdinalIgnoreCase))
            {
                string uuid = "";

                if (path.StartsWith("openStore?showStoreOffer=", StringComparison.OrdinalIgnoreCase))
                    uuid = path.Substring("openStore?showStoreOffer=".Length).Trim();
                else if (path.StartsWith("openStore/?showStoreOffer=", StringComparison.OrdinalIgnoreCase))
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
                if (query.StartsWith("?"))
                    query = query.Substring(1);

                // Split on '/?' to separate offerID segment from optional creator segment
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
            CosmosConsole.WriteLine(LanguageHandler.Format("Logs.UriHandlerError", ex.Message));
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

        JObject foundItem = FindItemInJson(jsonPath, offerID);

        if (foundItem != null)
        {
            WritePersonaItemPreviewJson(foundItem, offerID);
            return $"showDressingRoomOffer?offerID=fa359c7a-889b-4ce1-9d68-08691ca7303c";
        }
        else
        {
            return "";
        }
    }

    // Searches in capes file for the ID that matches the URI's UUID.
    private static JObject FindItemInJson(string jsonPath, string offerID)
    {
        if (!File.Exists(jsonPath))
            return null;

        JObject root;
        try
        {
            string raw = File.ReadAllText(jsonPath);
            root = JObject.Parse(raw);
        }
        catch
        {
            return null;
        }

        // Check all GridLists in file
        JArray rows = root["result"]?["rows"] as JArray;
        if (rows == null)
            return null;

        foreach (JToken row in rows)
        {
            if (!string.Equals(row["controlId"]?.ToString(), "GridList", StringComparison.OrdinalIgnoreCase))
                continue;

            JArray components = row["components"] as JArray;
            if (components == null)
                continue;

            foreach (JToken component in components)
            {
                JArray items = component["items"] as JArray;
                if (items == null)
                    continue;

                foreach (JToken item in items)
                {
                    string id = item["id"]?.ToString();
                    if (string.Equals(id, offerID, StringComparison.OrdinalIgnoreCase))
                        return (JObject)item;
                }
            }
        }

        return null;
    }

    // Creates PersonaItemPreview.json to use when loading the persona item.
    private static void WritePersonaItemPreviewJson(JObject foundItem, string offerID)
    {
        // Build the preview JSON structure
        JObject preview = JObject.Parse(@"
        {
          ""result"": {
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
            ],
            ""sidebarLayoutType"": ""Persona""
          }
        }");

        // Insert the found item into the GridList's items array
        JArray gridListItems = preview["result"]["rows"][1]["components"][0]["items"] as JArray;
        gridListItems.Add(foundItem);

        // Set the previewedId
        preview["result"]["rows"][1]["components"][1]["previewedId"] = offerID;

        // Write to disk
        string outputPath = Path.Combine(PathDefinitions.CustomJsonsDirectory, "PersonaItemPreview.json");
        string outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        File.WriteAllText(outputPath, preview.ToString(Formatting.Indented));
    }
}
