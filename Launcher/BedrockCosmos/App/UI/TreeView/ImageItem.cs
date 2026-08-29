namespace BedrockCosmos.App.UI
{
    /// <summary>
    /// A single grid cell (child node) inside an <see cref="ImageCategory"/>.
    /// Works for capes, skins, or any other collection of square thumbnails -
    /// deliberately a plain POCO with no UI or JSON-library dependency so it can
    /// be reused anywhere in the app (serialization, view-models, etc.).
    /// </summary>
    public sealed class ImageItem
    {
        /// <summary>Stable identifier pulled from the source JSON ("id"). Also used as the on-disk thumbnail cache file name.</summary>
        public string Id { get; set; }

        /// <summary>Display name pulled from the source JSON ("title").</summary>
        public string Title { get; set; }

        /// <summary>Thumbnail image URL pulled from the source JSON ("thumbnail.url").</summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// True if this node should render with the green "enabled" highlight
        /// border (e.g. currently equipped / active). Ignored while the node is
        /// selected, since the selection background takes visual priority.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>Optional bag for whatever extra data the host app wants to keep alongside a node.</summary>
        public object Tag { get; set; }

        public override string ToString() => Title ?? Id ?? base.ToString();
    }
}