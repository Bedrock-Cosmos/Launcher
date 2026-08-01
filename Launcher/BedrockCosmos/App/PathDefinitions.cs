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

namespace BedrockCosmos.App
{
    internal static class PathDefinitions
    {
        internal static string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        internal static string AppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\";
        internal static string CosmosAppData = Path.Combine(AppDataDirectory, @"Bedrock Cosmos\");
        internal static string CacheDirectory = Path.Combine(CosmosAppData, @"Cache\");
        internal static string CustomJsonsDirectory = Path.Combine(CosmosAppData, @"CustomJsons\");
        internal static string MiscDirectory = Path.Combine(CosmosAppData, @"Misc\");
        internal static string ResponsesDirectory = Path.Combine(CosmosAppData, @"Responses\");
    }
}