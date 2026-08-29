using System.Collections.Generic;

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

    public sealed class ImageCategory
    {
        public string Name { get; set; } // Display Name.
        public List<ImageItem> Items { get; set; } = new List<ImageItem>(); // All child grid nodes.
        public bool IsExpanded { get; set; } = true;
        public object Tag { get; set; }
        public override string ToString() => Name ?? base.ToString();
    }
}