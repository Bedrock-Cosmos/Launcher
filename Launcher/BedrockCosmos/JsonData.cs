using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BedrockCosmos.App;

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
    public static class JsonData
    {
        public static List<string> AllowedUrls = null;
        public static List<Endpoint> MainPages = null;
        public static List<MarketItem> MarketItems = null;
        public static List<MarketItem> PackSearchIds = null;
        private static string jsonPath = PathDefinitions.ResponsesDirectory + @"LauncherJsons\";
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };

        public static void InitializeJsons()
        {
            AllowedUrls =
                JsonSerializer.Deserialize<List<string>>
                (File.ReadAllText(jsonPath + @"AllowedUrls.json"), _jsonOptions);

            MainPages =
                JsonSerializer.Deserialize<List<Endpoint>>
                (File.ReadAllText(jsonPath + @"MainResponses.json"), _jsonOptions);

            MarketItems =
                JsonSerializer.Deserialize<List<MarketItem>>
                (File.ReadAllText(jsonPath + @"PlayfabGetPublishItemResponses.json"), _jsonOptions);

            PackSearchIds =
                JsonSerializer.Deserialize<List<MarketItem>>
                (File.ReadAllText(jsonPath + @"PlayfabSearchResponses.json"), _jsonOptions);
        }
    }
}