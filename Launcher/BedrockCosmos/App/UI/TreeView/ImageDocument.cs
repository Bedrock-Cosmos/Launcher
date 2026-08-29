using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BedrockCosmos.App.UI
{
    /// <summary>
    /// Loads category/item data out of a Marketplace-style JSON payload (the
    /// "GridList" rows format) into <see cref="ImageCategory"/>/<see cref="ImageItem"/>,
    /// and can write the *whole original document* back out to JSON after the
    /// user has added, removed, reordered, or moved nodes between categories via
    /// <see cref="ImageTreeView"/>.
    ///
    /// This keeps a live mutable copy of the source document
    /// (System.Text.Json.Nodes.JsonNode) and edits only the parts that changed,
    /// so unrelated fields on existing rows/items (rarity, ownership, pack
    /// identity, creator page links, etc.) are preserved untouched. Brand new
    /// rows/items (added at runtime rather than loaded from JSON) are
    /// synthesized from a clone of a sibling, or a minimal fallback if there is
    /// no sibling to copy from - see <see cref="NewRowFactory"/> and
    /// <see cref="NewItemFactory"/> if you want to control that shape yourself
    /// (useful when reusing this for a schema that differs slightly, e.g. skins
    /// vs. capes).
    ///
    /// Requires the System.Text.Json NuGet package, version 6.0 or newer (the
    /// System.Text.Json.Nodes mutable-DOM API used here isn't present in older
    /// 4.x builds of the package).
    /// </summary>
    public sealed class ImageDocument
    {
        private readonly JsonNode _root;

        private readonly Dictionary<ImageCategory, JsonObject> _categoryNodes = new Dictionary<ImageCategory, JsonObject>();
        private readonly Dictionary<ImageCategory, JsonArray> _categoryOwningRowsArray = new Dictionary<ImageCategory, JsonArray>();
        private readonly Dictionary<ImageItem, JsonObject> _itemNodes = new Dictionary<ImageItem, JsonObject>();
        private JsonArray _primaryRowsArray;

        /// <summary>
        /// The live category list. Pass this directly to <see cref="ImageTreeView.LoadData"/>
        /// (it will be used as the control's backing list rather than copied) so
        /// that every add/remove/move the user makes in the UI is automatically
        /// reflected here with no extra synchronization step - just call
        /// <see cref="ToJson"/> whenever you want to persist the current state.
        /// </summary>
        public List<ImageCategory> Categories { get; } = new List<ImageCategory>();

        /// <summary>Optional override for how a brand-new category's underlying JSON row is built. Receives the category name.</summary>
        public Func<string, JsonObject> NewRowFactory { get; set; }

        /// <summary>Optional override for how a brand-new item's underlying JSON object is built (used only when no sibling exists to clone).</summary>
        public Func<JsonObject> NewItemFactory { get; set; }

        private ImageDocument(JsonNode root)
        {
            _root = root;
        }

        /// <summary>Starts a brand-new, empty document (no source JSON) - useful when you want to build a category list from scratch and still export valid JSON later.</summary>
        public static ImageDocument CreateEmpty()
        {
            return new ImageDocument(new JsonObject());
        }

        /// <summary>
        /// Parses every "GridList" row found anywhere in the given JSON
        /// (searched recursively under any property named "rows", so it does
        /// not matter how deeply nested the layout is) into a flat category list.
        /// </summary>
        public static ImageDocument LoadFromMarketplaceJson(string json)
        {
            JsonNode root = JsonNode.Parse(json);
            var document = new ImageDocument(root);

            var rowsArrays = new List<JsonArray>();
            FindRowsArrays(root, rowsArrays);

            foreach (var rowsArray in rowsArrays)
            {
                foreach (var rowNode in rowsArray.OfType<JsonObject>())
                {
                    try
                    {
                        document.TryLoadRow(rowNode, rowsArray);
                    }
                    catch
                    {
                        // One malformed/unexpected row shouldn't take down the
                        // whole load - just skip it.
                    }
                }
            }

            return document;
        }

        private void TryLoadRow(JsonObject rowNode, JsonArray rowsArray)
        {
            if (GetString(rowNode["controlId"]) != "GridList")
                return;

            if (!(rowNode["components"] is JsonArray components))
                return;

            string categoryName = null;
            JsonArray itemsArray = null;

            foreach (var compNode in components.OfType<JsonObject>())
            {
                string type = GetString(compNode["$type"]);
                if (type == "HeaderComponent")
                    categoryName = GetString(compNode["text"]?["value"]);
                else if (type == "ItemListComponent")
                    itemsArray = compNode["items"] as JsonArray;
            }

            // Rows with no item list are decorative (dividers, preview pieces, etc.) - skip.
            if (itemsArray == null)
                return;

            var category = new ImageCategory
            {
                Name = categoryName ?? "Unnamed",
                Items = new List<ImageItem>()
            };

            foreach (var itemNode in itemsArray.OfType<JsonObject>())
            {
                try
                {
                    var item = new ImageItem
                    {
                        Id = GetString(itemNode["id"]),
                        Title = GetString(itemNode["title"]),
                        ThumbnailUrl = GetString(itemNode["thumbnail"]?["url"])
                    };

                    category.Items.Add(item);
                    _itemNodes[item] = itemNode;
                }
                catch
                {
                    // Skip malformed individual items rather than the whole category.
                }
            }

            Categories.Add(category);
            _categoryNodes[category] = rowNode;
            _categoryOwningRowsArray[category] = rowsArray;

            if (_primaryRowsArray == null)
                _primaryRowsArray = rowsArray;
        }

        /// <summary>
        /// Serializes the full original document back to JSON, with every
        /// add/remove/reorder/move currently reflected in <see cref="Categories"/>
        /// applied to the underlying tree first.
        /// </summary>
        public string ToJson(bool indented = true)
        {
            SyncAllRowsArrays();

            var options = new JsonSerializerOptions { WriteIndented = indented };
            return _root.ToJsonString(options);
        }

        #region Syncing the mutable JSON tree to match Categories

        private void SyncAllRowsArrays()
        {
            var categoriesByArray = new Dictionary<JsonArray, List<ImageCategory>>();

            foreach (var category in Categories)
            {
                JsonArray targetArray = _categoryOwningRowsArray.TryGetValue(category, out var array)
                    ? array
                    : EnsurePrimaryRowsArray();

                if (!categoriesByArray.TryGetValue(targetArray, out var list))
                {
                    list = new List<ImageCategory>();
                    categoriesByArray[targetArray] = list;
                }

                list.Add(category);
            }

            // Pass 1: ensure every category has a backing row node, update its
            // header text, and detach every item-array's existing children so
            // item nodes are free to be reclaimed by whichever category they
            // now belong to (including a different one than they started in),
            // regardless of which category happens to be processed first.
            var itemListComponentsByCategory = new List<(JsonObject component, ImageCategory category)>();

            foreach (var category in Categories)
            {
                var rowNode = GetOrCreateRowNode(category);
                if (!(rowNode["components"] is JsonArray components))
                    continue;

                foreach (var comp in components.OfType<JsonObject>())
                {
                    string type = GetString(comp["$type"]);
                    if (type == "HeaderComponent" && comp["text"] is JsonObject textObj)
                    {
                        textObj["value"] = category.Name;
                    }
                    else if (type == "ItemListComponent")
                    {
                        DetachAllChildren(comp);
                        itemListComponentsByCategory.Add((comp, category));
                    }
                }
            }

            // Pass 2: rebuild each item list in the category's current order.
            foreach (var pair in itemListComponentsByCategory)
                RebuildItemsArray(pair.component, pair.category);

            // Pass 3: reorder/add/remove the row slots within each rows array.
            foreach (var pair in categoriesByArray)
                SyncRowSlots(pair.Key, pair.Value);
        }

        private JsonObject GetOrCreateRowNode(ImageCategory category)
        {
            if (_categoryNodes.TryGetValue(category, out var existing))
                return existing;

            var newRow = (NewRowFactory ?? BuildDefaultRow).Invoke(category.Name);
            _categoryNodes[category] = newRow;
            return newRow;
        }

        private void DetachAllChildren(JsonObject itemListComponent)
        {
            if (!(itemListComponent["items"] is JsonArray itemsArray))
            {
                itemListComponent["items"] = new JsonArray();
                return;
            }

            while (itemsArray.Count > 0)
                itemsArray.RemoveAt(0);
        }

        private void RebuildItemsArray(JsonObject itemListComponent, ImageCategory category)
        {
            var itemsArray = (JsonArray)itemListComponent["items"];

            foreach (var item in category.Items)
            {
                JsonObject node;

                if (_itemNodes.TryGetValue(item, out var existingNode) && existingNode.Parent == null)
                {
                    // Either its original slot (now free after the detach pass
                    // above) or - if it was moved here from another category -
                    // its original node from that category, freed the same way.
                    node = existingNode;
                }
                else
                {
                    var template = itemsArray.OfType<JsonObject>().FirstOrDefault();
                    node = template != null
                        ? (JsonObject)template.DeepClone()
                        : (NewItemFactory ?? BuildDefaultItem).Invoke();
                }

                node["id"] = item.Id;
                node["title"] = item.Title;

                if (!string.IsNullOrEmpty(item.ThumbnailUrl))
                {
                    var thumbnail = node["thumbnail"] as JsonObject ?? new JsonObject();
                    thumbnail["tag"] = "Thumbnail";
                    thumbnail["type"] = "Thumbnail";
                    thumbnail["url"] = item.ThumbnailUrl;
                    thumbnail["urlWithResolution"] = item.ThumbnailUrl;
                    node["thumbnail"] = thumbnail;
                }

                _itemNodes[item] = node;
                itemsArray.Add(node);
            }

            if (itemListComponent["totalItems"] != null)
                itemListComponent["totalItems"] = category.Items.Count;
        }

        private void SyncRowSlots(JsonArray rowsArray, List<ImageCategory> categoriesForThisArray)
        {
            // Snapshot the original sequence, tagging which entries are
            // GridList rows (managed by us) versus anything else (dividers,
            // text rows, etc. - left alone and kept in their original relative
            // position).
            var originalSequence = new List<JsonNode>();
            var isGridListSlot = new List<bool>();

            for (int i = 0; i < rowsArray.Count; i++)
            {
                var node = rowsArray[i];
                originalSequence.Add(node);
                isGridListSlot.Add(node is JsonObject obj && GetString(obj["controlId"]) == "GridList");
            }

            while (rowsArray.Count > 0)
                rowsArray.RemoveAt(0);

            var orderedRowNodes = new Queue<JsonObject>(categoriesForThisArray.Select(c => _categoryNodes[c]));

            for (int i = 0; i < originalSequence.Count; i++)
            {
                if (isGridListSlot[i])
                {
                    if (orderedRowNodes.Count > 0)
                        rowsArray.Add(orderedRowNodes.Dequeue());
                    // else: this slot's category was removed - drop the slot.
                }
                else
                {
                    rowsArray.Add(originalSequence[i]);
                }
            }

            // Brand-new categories with no original slot get appended at the end.
            while (orderedRowNodes.Count > 0)
                rowsArray.Add(orderedRowNodes.Dequeue());
        }

        private JsonArray EnsurePrimaryRowsArray()
        {
            if (_primaryRowsArray != null)
                return _primaryRowsArray;

            // No pre-existing "rows" array was found (e.g. this document was
            // started via CreateEmpty()) - synthesize a minimal container so
            // new categories still have somewhere to live.
            var rowsArray = new JsonArray();

            if (_root is JsonObject rootObj)
            {
                if (!(rootObj["result"] is JsonObject resultObj))
                {
                    resultObj = new JsonObject();
                    rootObj["result"] = resultObj;
                }

                resultObj["layout"] = new JsonArray(new JsonObject
                {
                    ["sectionName"] = "rows",
                    ["rows"] = rowsArray
                });
            }

            _primaryRowsArray = rowsArray;
            return rowsArray;
        }

        #endregion

        #region Default row/item shapes for brand-new nodes

        private static JsonObject BuildDefaultRow(string categoryName)
        {
            var header = new JsonObject
            {
                ["type"] = "headerComp",
                ["$type"] = "HeaderComponent",
                ["text"] = new JsonObject
                {
                    ["value"] = categoryName,
                    ["replacements"] = new JsonArray()
                }
            };

            var itemList = new JsonObject
            {
                ["type"] = "itemListComp",
                ["$type"] = "ItemListComponent",
                ["items"] = new JsonArray()
            };

            return new JsonObject
            {
                ["controlId"] = "GridList",
                ["components"] = new JsonArray(header, itemList)
            };
        }

        private static JsonObject BuildDefaultItem()
        {
            return new JsonObject
            {
                ["contentType"] = "PersonaDurable"
            };
        }

        #endregion

        #region JSON helpers

        private static void FindRowsArrays(JsonNode node, List<JsonArray> result)
        {
            if (node is JsonObject obj)
            {
                foreach (var property in obj)
                {
                    if (property.Value == null)
                        continue;

                    if (property.Key == "rows" && property.Value is JsonArray rowsArray)
                        result.Add(rowsArray);

                    FindRowsArrays(property.Value, result);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var child in arr)
                {
                    if (child != null)
                        FindRowsArrays(child, result);
                }
            }
        }

        /// <summary>Safely reads a string value from a node that might be missing, null, or a different JSON kind.</summary>
        private static string GetString(JsonNode node)
        {
            return node is JsonValue value && value.TryGetValue<string>(out var s) ? s : null;
        }

        #endregion
    }
}