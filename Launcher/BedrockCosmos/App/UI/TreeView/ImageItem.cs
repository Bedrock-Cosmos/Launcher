// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.App.UI
{
    // Child node under the expandable parent, like a specific cape or skin pack.
    public sealed class ImageItem
    {
        public string Id { get; set; } // Marketplace UUID.
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsEnabled { get; set; }
        public object Tag { get; set; }
        public override string ToString() => Title ?? Id ?? base.ToString();
    }
}