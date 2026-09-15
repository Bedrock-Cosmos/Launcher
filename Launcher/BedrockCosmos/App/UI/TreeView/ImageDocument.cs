using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BedrockCosmos.App.UI
{
    // Loads category/item data out of marketplace JSON response.
    public sealed class ImageDocument
    {
        private readonly JsonNode _root;

        private readonly Dictionary<ImageCategory, JsonObject> _categoryNodes = new Dictionary<ImageCategory, JsonObject>();
        private readonly Dictionary<ImageCategory, JsonArray> _categoryOwningRowsArray = new Dictionary<ImageCategory, JsonArray>();
        private readonly Dictionary<ImageItem, JsonObject> _itemNodes = new Dictionary<ImageItem, JsonObject>();
        private JsonArray _primaryRowsArray;

        public List<ImageCategory> Categories { get; } = new List<ImageCategory>(); // Node operations (copy, move, etc.) are all stored here.

        public Func<string, JsonObject> NewRowFactory { get; set; } // Optional override for how a brand-new category's underlying JSON row is built - receives the category name.

        public Func<JsonObject> NewItemFactory { get; set; } // Optional override for how a brand-new item's underlying JSON object is built - used only when no sibling exists to clone.

        private ImageDocument(JsonNode root)
        {
            _root = root;
        }

        public static ImageDocument CreateEmpty()
        {
            return new ImageDocument(new JsonObject()); // Makes from no source JSON.
        }

        // Parses GridLists from marketplace JSON into ImageTreeView.
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
                        // Skips if error.
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

            // Skips any row with no ItemList in JSON.
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
                    // Skips if one item is malformed.
                }
            }

            Categories.Add(category);
            _categoryNodes[category] = rowNode;
            _categoryOwningRowsArray[category] = rowsArray;

            if (_primaryRowsArray == null)
                _primaryRowsArray = rowsArray;
        }

        public string ToJson(bool indented = true, bool includeItemCountInCategoryName = false)
        {
            SyncAllRowsArrays(includeItemCountInCategoryName);

            var options = new JsonSerializerOptions { WriteIndented = indented };
            return _root.ToJsonString(options);
        }

        private void SyncAllRowsArrays(bool includeItemCountInCategoryName = false)
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

            // Ensure every category has a backing row node, update its header text, and detach every item
            // array's existing children so item nodes can be reclaimed by whichever category they now belong to.
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
                        textObj["value"] = includeItemCountInCategoryName
                            ? $"{category.Name} ({category.Items.Count})"
                            : category.Name;
                    }
                    else if (type == "ItemListComponent")
                    {
                        DetachAllChildren(comp);
                        itemListComponentsByCategory.Add((comp, category));
                    }
                }
            }

            foreach (var pair in itemListComponentsByCategory)
                RebuildItemsArray(pair.component, pair.category); // Rebuild each item list in the category's current order.

            foreach (var pair in categoriesByArray)
                SyncRowSlots(pair.Key, pair.Value); // Reorder/add/remove the row slots within each rows array.
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
                    // Either its original slot or, if moved here from another category, its original node from that category.
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
            // Snapshots sequence and tags entries that are GridList rows (managed by us).
            // Other parts like dividers and text rows are kept in the same spot.
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
                    if (orderedRowNodes.Count > 0) // Category was removed if not true.
                        rowsArray.Add(orderedRowNodes.Dequeue());
                }
                else
                {
                    rowsArray.Add(originalSequence[i]);
                }
            }

            // Brand-new categories get appended at the end.
            while (orderedRowNodes.Count > 0)
                rowsArray.Add(orderedRowNodes.Dequeue());
        }

        private JsonArray EnsurePrimaryRowsArray()
        {
            if (_primaryRowsArray != null)
                return _primaryRowsArray;

            // Runs if no pre-existing "rows" array was found, e.g. started via CreateEmpty().
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

        // Reads a string value from a node that might be missing, null, etc.
        private static string GetString(JsonNode node)
        {
            return node is JsonValue value && value.TryGetValue<string>(out var s) ? s : null;
        }
    }
}