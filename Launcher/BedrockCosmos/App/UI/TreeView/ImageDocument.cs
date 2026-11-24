using BedrockCosmos.App.MenuEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

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

        // ---- Export layout constants (capes menu) ----
        private const int MaxItemsPerList = 400; // Ceiling for the game, not real item count.
        private const string OwnedTitleKey = "dr.collector_title.owned";
        private const string CreatorTitle = "Creator Capes";
        private const string CreatorDescription = "Click on ''By Creator'' to see all capes by them!";
        private const string GrayDividerColor = "[0.70196,0.8392,0.8588,0.2]";
        private const string BlueDividerColor = "[0.243,0.776,0.839,1]";

        private static readonly Regex TrailingCountRegex = new Regex(@"\s*\(\d+\)\s*$", RegexOptions.Compiled);

        private enum CategoryRole
        {
            Standard,
            Owned,
            Creator
        }

        private const string StandardHeaderTextJson = @"{
  ""value"": """",
  ""style"": {
    ""highlightColor"": [],
    ""alignment"": ""Left"",
    ""textColor"": [ 1.0, 1.0, 1.0 ],
    ""font"": ""MinecraftTen"",
    ""showBackground"": false,
    ""showOutline"": false,
    ""indent"": 0.0,
    ""buttonWidth"": 0.0,
    ""color"": [],
    ""offerControlIdType"": ""None""
  },
  ""replacements"": [ ""dr.categories.capes"" ]
}";

        // Header used by the Creator Capes grid itself (the ''By Creator'' hint line). Has no replacements.
        private const string CreatorDescriptionTextJson = @"{
  ""value"": """",
  ""style"": {
    ""highlightColor"": [],
    ""alignment"": ""Left"",
    ""textColor"": [ 0.243, 0.776, 0.839, 1.0 ],
    ""font"": ""SmoothWithEmoticons"",
    ""showBackground"": false,
    ""showOutline"": false,
    ""indent"": 0.0,
    ""buttonWidth"": 0.0,
    ""color"": [],
    ""offerControlIdType"": ""None""
  }
}";

        // Text row placed above the Creator Capes grid, holding the real category title.
        private const string CreatorTitleRowJson = @"{
  ""controlId"": ""TextRow"",
  ""components"": [
    {
      ""text"": {
        ""value"": """",
        ""style"": {
          ""highlightColor"": [],
          ""alignment"": ""Left"",
          ""textColor"": [ 0.098, 0.247, 0.608, 1.0 ],
          ""font"": ""MinecraftTen"",
          ""showBackground"": false,
          ""showOutline"": false,
          ""indent"": 0.0,
          ""buttonWidth"": 0.0,
          ""color"": [],
          ""offerControlIdType"": ""None""
        },
        ""replacements"": []
      },
      ""type"": ""headerComp"",
      ""$type"": ""HeaderComponent""
    }
  ]
}";

        private const string DividerJson = @"{
  ""controlId"": ""VerticalLineDivider"",
  ""components"": [
    {
      ""textureImage"": {
        ""type"": ""Unknown"",
        ""style"": {
          ""highlightColor"": [],
          ""alignment"": ""Left"",
          ""textColor"": [],
          ""font"": ""default"",
          ""showBackground"": false,
          ""showOutline"": false,
          ""indent"": 0.0,
          ""buttonWidth"": 0.0,
          ""color"": __COLOR__,
          ""offerControlIdType"": ""None""
        }
      },
      ""type"": ""textureComp"",
      ""$type"": ""TextureComponent""
    }
  ]
}";

        private const string PersonaDefaultButtonJson = @"{
  ""type"": ""personaDefaultButtonComp"",
  ""$type"": ""PersonaDefaultButton""
}";

        private const string DefaultRowJson = @"{
  ""controlId"": ""GridList"",
  ""components"": [
    {
      ""text"": {},
      ""type"": ""headerComp"",
      ""$type"": ""HeaderComponent""
    },
    {
      ""items"": [],
      ""totalItems"": 400,
      ""customStoreRowConfiguration"": {
        ""seeAllVisible"": false,
        ""maxOffers"": 400,
        ""arrowsVisible"": false,
        ""visibleOffersPerRow"": 6
      },
      ""type"": ""itemListComp"",
      ""$type"": ""ItemListComponent""
    },
    {
      ""type"": ""dispPreviewPieceComp"",
      ""$type"": ""DisplayPreviewedPieceOfferComponent""
    }
  ]
}";

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
                        ThumbnailUrl = GetString(itemNode["thumbnail"]?["url"]),
                        Rarity = ParseRarity(GetString(itemNode["rarity"]))
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

        public sealed class MergeResult
        {
            public int ItemsAdded { get; internal set; }
            public int CategoriesCreated { get; internal set; }

            // Incoming items that were NOT copied because their Id already existed
            // (or appeared earlier in the same file).
            public List<ImageItem> SkippedDuplicates { get; } = new List<ImageItem>();

            // Existing items that were moved to the position given by the new JSON.
            public List<ImageItem> RepositionedItems { get; } = new List<ImageItem>();

            public int DuplicatesSkipped => SkippedDuplicates.Count;
            public int ItemsRepositioned => RepositionedItems.Count;
            public bool HadDuplicates => SkippedDuplicates.Count > 0;
        }

        public MergeResult MergeFromMarketplaceJson(string json, bool allowDuplicateIds = false)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON is empty.", nameof(json));

            // Parsed first, so malformed JSON throws before anything is modified.
            ImageDocument incoming = LoadFromMarketplaceJson(json);
            var result = new MergeResult();

            // Existing items by Id, and Ids already handled from the incoming file.
            var existingById = new Dictionary<string, ImageItem>(StringComparer.OrdinalIgnoreCase);
            var seenIncomingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!allowDuplicateIds)
            {
                foreach (var category in Categories)
                    foreach (var item in category.Items)
                        if (!string.IsNullOrEmpty(item.Id) && !existingById.ContainsKey(item.Id))
                            existingById[item.Id] = item;
            }

            foreach (var incomingCategory in incoming.Categories)
            {
                var placements = new List<(ImageItem item, bool isNew, ImageItem incomingItem)>();

                foreach (var incomingItem in incomingCategory.Items)
                {
                    if (!allowDuplicateIds && !string.IsNullOrEmpty(incomingItem.Id))
                    {
                        // Ignores repeats of nodes if already handled.
                        if (!seenIncomingIds.Add(incomingItem.Id))
                        {
                            result.SkippedDuplicates.Add(incomingItem);
                            continue;
                        }

                        if (existingById.TryGetValue(incomingItem.Id, out var existingItem))
                        {
                            result.SkippedDuplicates.Add(incomingItem);
                            placements.Add((existingItem, false, incomingItem));
                            continue;
                        }
                    }

                    placements.Add((incomingItem, true, incomingItem));
                }

                if (placements.Count == 0 && incomingCategory.Items.Count > 0) // Ignores empty categories.
                    continue;

                string name = CapeMenuEditor.NormalizeCategoryName(incomingCategory.Name);
                ImageCategory target = FindCategoryForMerge(name);

                if (target == null)
                {
                    target = new ImageCategory { Name = name };
                    Categories.Add(target); // End of the list, JSON row is built on export.
                    result.CategoriesCreated++;
                }

                foreach (var placement in placements)
                {
                    if (placement.isNew)
                    {
                        target.Items.Add(placement.item);

                        // Keep item's original JSON fields by cloning its node.
                        if (incoming._itemNodes.TryGetValue(placement.item, out var node))
                            _itemNodes[placement.item] = (JsonObject)node.DeepClone();

                        result.ItemsAdded++;
                    }
                    else
                    {
                        // Find the category that currently holds this item.
                        ImageCategory currentOwner = null;
                        foreach (var category in Categories)
                        {
                            if (category.Items.Contains(placement.item))
                            {
                                currentOwner = category;
                                break;
                            }
                        }

                        // Leaves as is if already in the right category.
                        if (ReferenceEquals(currentOwner, target))
                            continue;

                        currentOwner?.Items.Remove(placement.item);
                        target.Items.Add(placement.item);
                        result.RepositionedItems.Add(placement.item);
                    }
                }
            }

            return result;
        }

        private ImageCategory FindCategoryForMerge(string normalizedName)
        {
            CategoryRole role = GetRole(new ImageCategory { Name = normalizedName });

            foreach (var category in Categories)
            {
                if (role != CategoryRole.Standard && GetRole(category) == role)
                    return category;

                if (string.Equals(StripTrailingCount(category.Name)?.Trim(), normalizedName?.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    return category;
            }

            return null;
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

            // Clears out old category/divider rows if every category is lost.
            foreach (var owningArray in _categoryOwningRowsArray.Values)
            {
                if (!categoriesByArray.ContainsKey(owningArray))
                    categoriesByArray[owningArray] = new List<ImageCategory>();
            }

            // Ensure every category has a backing row node, rewrite its header, and detach every item
            // array's existing children so item nodes can be reclaimed by whichever category they now belong to.
            var itemListComponentsByCategory = new List<(JsonObject component, ImageCategory category)>();

            foreach (var category in Categories)
            {
                var rowNode = GetOrCreateRowNode(category);
                if (!(rowNode["components"] is JsonArray components))
                    continue;

                // Snapshot, components get removed while looping.
                foreach (var comp in components.OfType<JsonObject>().ToList())
                {
                    string type = GetString(comp["$type"]);
                    if (type == "HeaderComponent")
                    {
                        // Rebuilt in full (style, replacements, etc.) based on category's name/role.
                        comp["text"] = BuildHeaderText(category, includeItemCountInCategoryName);
                    }
                    else if (type == "ItemListComponent")
                    {
                        DetachAllChildren(comp);
                        itemListComponentsByCategory.Add((comp, category));
                    }
                    else if (type == "PersonaDefaultButton")
                    {
                        components.Remove(comp); // Re-added on topmost category only.
                    }
                }
            }

            if (Categories.Count > 0 && GetOrCreateRowNode(Categories[0])["components"] is JsonArray topComponents)
                topComponents.Add(ParseObject(PersonaDefaultButtonJson));

            foreach (var pair in itemListComponentsByCategory)
                RebuildItemsArray(pair.component, pair.category); // Rebuild each item list in the category's current order.

            foreach (var pair in categoriesByArray)
                SyncRowSlots(pair.Key, pair.Value, includeItemCountInCategoryName); // Rebuild the row layout within each rows array.
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

                if (item.Rarity != ItemRarity.None)
                {
                    node["rarity"] = item.Rarity.ToString().ToLowerInvariant();
                }
                else if (ParseRarity(GetString(node["rarity"])) != ItemRarity.None)
                {
                    node["rarity"] = "common"; // Defaults to common if rarity value is missing.
                }

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

            if (itemListComponent["totalItems"] == null)
                itemListComponent["totalItems"] = MaxItemsPerList;
        }

        private void SyncRowSlots(JsonArray rowsArray, List<ImageCategory> categoriesForThisArray, bool includeItemCountInCategoryName)
        {
            // Empties the array, keeping only rows we don't manage.
            var leadingRows = new List<JsonNode>();
            var trailingRows = new List<JsonNode>();
            bool passedCategoryRows = false;

            while (rowsArray.Count > 0)
            {
                JsonNode node = rowsArray[0];
                rowsArray.RemoveAt(0);

                var obj = node as JsonObject;
                string controlId = obj != null ? GetString(obj["controlId"]) : null;

                if (controlId == "GridList" || (controlId == "TextRow" && IsCreatorTitleRow(obj)))
                {
                    passedCategoryRows = true;
                    continue;
                }

                if (controlId == "VerticalLineDivider")
                    continue;

                if (passedCategoryRows)
                    trailingRows.Add(node);
                else
                    leadingRows.Add(node);
            }

            foreach (var row in leadingRows)
                rowsArray.Add(row);

            for (int i = 0; i < categoriesForThisArray.Count; i++)
            {
                var category = categoriesForThisArray[i];
                bool isCreator = GetRole(category) == CategoryRole.Creator;
                bool previousIsCreator = i > 0 && GetRole(categoriesForThisArray[i - 1]) == CategoryRole.Creator;

                if (i > 0)
                    rowsArray.Add(BuildDivider(isCreator || previousIsCreator));

                if (isCreator)
                    rowsArray.Add(BuildCreatorTitleRow(category, includeItemCountInCategoryName));

                rowsArray.Add(_categoryNodes[category]);
            }

            if (categoriesForThisArray.Count > 0)
            {
                bool lastIsCreator = GetRole(categoriesForThisArray[categoriesForThisArray.Count - 1]) == CategoryRole.Creator;
                rowsArray.Add(BuildDivider(lastIsCreator));
            }

            foreach (var row in trailingRows)
                rowsArray.Add(row);
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
            var row = ParseObject(DefaultRowJson);
            var components = (JsonArray)row["components"];
            components[0]["text"] = BuildStandardHeaderText(categoryName ?? "Unnamed");
            return row;
        }

        private static JsonObject ParseObject(string json)
        {
            return (JsonObject)JsonNode.Parse(json); // Always a fresh node, node can only have one parent.
        }

        private static string StripTrailingCount(string name)
        {
            return string.IsNullOrEmpty(name) ? name : TrailingCountRegex.Replace(name, "");
        }

        private static CategoryRole GetRole(ImageCategory category)
        {
            string name = StripTrailingCount(category.Name)?.Trim();

            if (name == null)
                return CategoryRole.Standard;

            // Also accepts the raw JSON header values.
            if (string.Equals(name, "Default Capes", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "Owned Capes", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith(OwnedTitleKey, StringComparison.OrdinalIgnoreCase))
                return CategoryRole.Owned;

            if (string.Equals(name, CreatorTitle, StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith(CreatorDescription.Substring(0, 24), StringComparison.OrdinalIgnoreCase))
                return CategoryRole.Creator;

            return CategoryRole.Standard;
        }

        private static JsonObject BuildStandardHeaderText(string value)
        {
            var text = ParseObject(StandardHeaderTextJson);
            text["value"] = value;
            return text;
        }

        private static JsonObject BuildHeaderText(ImageCategory category, bool includeItemCount)
        {
            switch (GetRole(category))
            {
                case CategoryRole.Owned:
                    return BuildStandardHeaderText(OwnedTitleKey);

                case CategoryRole.Creator:
                    var description = ParseObject(CreatorDescriptionTextJson);
                    description["value"] = CreatorDescription;
                    return description;

                default:
                    string name = includeItemCount
                        ? $"{StripTrailingCount(category.Name)} ({category.Items.Count})"
                        : category.Name;
                    return BuildStandardHeaderText(name ?? "Unnamed");
            }
        }

        private static JsonObject BuildCreatorTitleRow(ImageCategory category, bool includeItemCount)
        {
            var row = ParseObject(CreatorTitleRowJson);
            var header = (JsonObject)((JsonArray)row["components"])[0];
            header["text"]["value"] = includeItemCount
                ? $"{CreatorTitle} ({category.Items.Count})"
                : CreatorTitle;
            return row;
        }

        private static JsonObject BuildDivider(bool blue)
        {
            return ParseObject(DividerJson.Replace("__COLOR__", blue ? BlueDividerColor : GrayDividerColor));
        }

        private static bool IsCreatorTitleRow(JsonObject row)
        {
            if (row == null || !(row["components"] is JsonArray components))
                return false;

            foreach (var comp in components.OfType<JsonObject>())
            {
                string value = GetString(comp["text"]?["value"]);
                if (string.Equals(StripTrailingCount(value)?.Trim(), CreatorTitle, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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

        // Converts JSON "rarity" string (common, uncommon, rare, epic, legendary) - anything else is None.
        private static ItemRarity ParseRarity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ItemRarity.None;

            return Enum.TryParse(value.Trim(), true, out ItemRarity rarity) && Enum.IsDefined(typeof(ItemRarity), rarity)
                ? rarity
                : ItemRarity.None;
        }

        // Reads a string value from a node that might be missing, null, etc.
        private static string GetString(JsonNode node)
        {
            return node is JsonValue value && value.TryGetValue<string>(out var s) ? s : null;
        }
    }
}