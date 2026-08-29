using System.Collections.Generic;

namespace BedrockCosmos.App.UI
{
    /// <summary>
    /// A collapsible top-level ("parent") node. Corresponds to one "GridList" row
    /// in the Marketplace-style source JSON. Its <see cref="Items"/> are rendered
    /// by <see cref="ImageTreeView"/> as a wrapping grid of square thumbnails.
    /// Used for cape categories, skin categories, or any similar grouping.
    /// </summary>
    public sealed class ImageCategory
    {
        /// <summary>Display name pulled from the source JSON header ("text.value").</summary>
        public string Name { get; set; }

        /// <summary>Child grid nodes belonging to this category, in display order.</summary>
        public List<ImageItem> Items { get; set; } = new List<ImageItem>();

        /// <summary>Whether the category is currently expanded in the UI. Defaults to expanded.</summary>
        public bool IsExpanded { get; set; } = true;

        /// <summary>Optional bag for whatever extra data the host app wants to keep alongside a category.</summary>
        public object Tag { get; set; }

        public override string ToString() => Name ?? base.ToString();
    }
}