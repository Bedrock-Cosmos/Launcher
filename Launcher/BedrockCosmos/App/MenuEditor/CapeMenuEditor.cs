using BedrockCosmos.App.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BedrockCosmos.App.MenuEditor
{
    // Stored in TreeViewNode.Tag for every cape.
    public class CapeItemData
    {
        public string Id { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public static class CapeMenuEditor
    {
        // Dictionary for updating category titles.
        private static readonly Dictionary<string, string> HeaderTitleOverrides =
            new Dictionary<string, string>
            {
                { "dr.collector_title.owned", "Default Capes" },
                { "Vanilla", "Vanilla Capes" },
                { "Spin-Off", "Spin-Off Capes" },
                { "Custom", "Custom Capes" },
                { "Click on \'\'By Creator\'\'", "Creator Capes" },
                { "Skin Pack", "Skin Pack Capes" }

            };

        // Parses the JSON at jsonFilePath and rebuilds treeView.Nodes from it.
        // GridLists are top-level, Items are within them.
        public static void PopulateTree(TreeViewControl treeView, string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                PopulateTree(treeView, doc.RootElement);
            }
        }

        // Overload for passing JSON elsewhere.
        public static void PopulateTree(TreeViewControl treeView, JsonElement root)
        {
            List<TreeViewNode> topNodes = new List<TreeViewNode>();

            if (root.TryGetProperty("result", out JsonElement result) &&
                result.TryGetProperty("layout", out JsonElement layout) &&
                layout.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement section in layout.EnumerateArray())
                {
                    if (!section.TryGetProperty("rows", out JsonElement rows) ||
                        rows.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (JsonElement row in rows.EnumerateArray())
                    {
                        TreeViewNode gridNode = TryBuildGridListNode(row);

                        if (gridNode != null)
                        {
                            topNodes.Add(gridNode);
                        }
                    }
                }
            }

            treeView.Nodes.Clear();
            treeView.Nodes.AddRange(topNodes);
        }

        // Creates rows from GridList if it has a headerComp and an itemListComp.
        private static TreeViewNode TryBuildGridListNode(JsonElement row)
        {
            if (!row.TryGetProperty("components", out JsonElement components) ||
                components.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            string headerTitle = null;
            JsonElement itemsElement = default;
            bool hasItems = false;

            foreach (JsonElement component in components.EnumerateArray())
            {
                if (!component.TryGetProperty("type", out JsonElement typeProp))
                {
                    continue;
                }

                string componentType = typeProp.GetString();

                if (componentType == "headerComp")
                {
                    if (component.TryGetProperty("text", out JsonElement textEl) &&
                        textEl.TryGetProperty("value", out JsonElement valueEl))
                    {
                        headerTitle = valueEl.GetString();
                    }
                }
                else if (componentType == "itemListComp")
                {
                    if (component.TryGetProperty("items", out JsonElement itemsEl) &&
                        itemsEl.ValueKind == JsonValueKind.Array)
                    {
                        itemsElement = itemsEl;
                        hasItems = true;
                    }
                }
            }

            if (headerTitle == null || !hasItems)
            {
                return null;
            }

            foreach (KeyValuePair<string, string> entry in HeaderTitleOverrides)
            {
                if (headerTitle.StartsWith(entry.Key, StringComparison.Ordinal))
                {
                    headerTitle = entry.Value;
                }
            }

            TreeViewNode gridNode = new TreeViewNode(headerTitle);

            foreach (JsonElement item in itemsElement.EnumerateArray())
            {
                TreeViewNode itemNode = BuildItemNode(item);

                if (itemNode != null)
                {
                    gridNode.Nodes.Add(itemNode);
                }
            }

            return gridNode;
        }

        // Builds a single leaf node for one entry in "items" array.
        private static TreeViewNode BuildItemNode(JsonElement item)
        {
            string title = item.TryGetProperty("title", out JsonElement titleEl)
                ? titleEl.GetString()
                : "(untitled)";

            string id = item.TryGetProperty("id", out JsonElement idEl)
                ? idEl.GetString()
                : null;

            string thumbnailUrl = null;

            if (item.TryGetProperty("thumbnail", out JsonElement thumbEl))
            {
                if (thumbEl.TryGetProperty("url", out JsonElement urlEl))
                {
                    thumbnailUrl = urlEl.GetString();
                }
                else if (thumbEl.TryGetProperty("urlWithResolution", out JsonElement urlResEl))
                {
                    thumbnailUrl = urlResEl.GetString();
                }
            }

            return new TreeViewNode(title)
            {
                Tag = new CapeItemData
                {
                    Id = id,
                    ThumbnailUrl = thumbnailUrl
                }
            };
        }
    }
}