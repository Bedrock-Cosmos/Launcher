using System;
using System.Collections.Generic;
using System.IO;
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

namespace BedrockCosmos.App
{
    internal class NewsManager
    {
        private static string _currentNewsUuid = "00000000-0000-4000-0000-000000000000";
        private static bool _sendNewsToInbox = true;
        private static bool _sendNewsToAnnouncement = true;

        private static JsonObject _newsHistoryObj = null;
        private static JsonObject _currentNewsObj = null;
        private static List<string> _seenUuids = null;

        private static readonly string _newsIconUrl = "https://bedrock-cosmos.app/icons/NewsIcon.png";
        private static readonly string _newsHistoryPath = Path.Combine(PathDefinitions.CustomJsonsDirectory, "News.json");
        private static readonly string _newsHistoryUuidsPath = Path.Combine(PathDefinitions.MiscDirectory, "NewsHistory.json");
        private static readonly string _currentNewsPath = Path.Combine(PathDefinitions.ResponsesDirectory, @"MainPages\CurrentNews_append.json");

        private static readonly JsonSerializerOptions _jsonWriteIndented = new JsonSerializerOptions { WriteIndented = true };

        internal static bool SendToNewsInbox
        {
            get { return _sendNewsToInbox; }
        }

        internal static bool SendToNewsAnnouncement
        {
            get { return _sendNewsToAnnouncement; }
        }

        internal static string NewsIconUrl
        {
            get { return _newsIconUrl; }
        }

        internal static string NewsHistoryPath
        {
            get { return _newsHistoryPath; }
        }

        internal static string NewsUuidsPath
        {
            get { return _newsHistoryUuidsPath; }
        }

        internal static string CurrentNewsPath
        {
            get { return _currentNewsPath; }
        }

        // String properties to serialize from cached JsonObjects on demand
        internal static string NewsHistory =>
            _newsHistoryObj?.ToJsonString(_jsonWriteIndented) ?? string.Empty;

        internal static string CurrentNews =>
            _currentNewsObj?.ToJsonString(_jsonWriteIndented) ?? string.Empty;

        internal static void CreateNewsHistoryFile()
        {
            _newsHistoryObj = new JsonObject
            {
                ["category"] = "BedrockCosmosNews",
                ["totalNumberOfUnreadMessages"] = 0,
                ["totalNumberOfMessages"] = 0,
                ["messages"] = new JsonArray(),
                ["categoryInfo"] = new JsonObject
                {
                    ["name"] = "Bedrock Cosmos News",
                    ["image"] = new JsonObject
                    {
                        ["id"] = "8642b05e-b0e1-4057-94d8-98552e53a23a",
                        ["url"] = _newsIconUrl
                    },
                    ["type"] = "BedrockCosmosNews"
                }
            };

            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToJsonString(_jsonWriteIndented));
        }

        internal static void RetrieveNewsHistory()
        {
            if (!Directory.Exists(PathDefinitions.CustomJsonsDirectory))
                Directory.CreateDirectory(PathDefinitions.CustomJsonsDirectory);

            if (!File.Exists(_newsHistoryPath))
                CreateNewsHistoryFile();

            _newsHistoryObj = JsonNode.Parse(File.ReadAllText(_newsHistoryPath))?.AsObject();

            // Re-updates old image URL in case it needs to change
            if (_newsHistoryObj?["categoryInfo"]?["image"] is JsonObject imageObj)
            {
                var currentUrl = imageObj["url"]?.GetValue<string>();
                if (currentUrl != _newsIconUrl)
                {
                    imageObj["url"] = _newsIconUrl;
                    File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToJsonString(_jsonWriteIndented));
                }
;            }
        }

        internal static void RetrieveCurrentNews()
        {
            // Reset defaults
            _sendNewsToInbox = true;
            _sendNewsToAnnouncement = true;

            if (!File.Exists(_currentNewsPath))
            {
                CosmosConsole.WriteLine("No current news file found.");
                return;
            }

            _currentNewsObj = JsonNode.Parse(File.ReadAllText(_currentNewsPath))?.AsObject();

            // Optional booleans only used in Bedrock Cosmos news system
            // "cosmosSendToInbox": true/false
            JsonNode sendToInbox = _currentNewsObj?["cosmosSendToInbox"];
            if (sendToInbox != null)
                _sendNewsToInbox = sendToInbox.GetValue<bool>();

            // "cosmosSendToAnnouncement": true/false
            JsonNode sendToAnnouncement = _currentNewsObj?["cosmosSendToAnnouncement"];
            if (sendToAnnouncement != null)
                _sendNewsToAnnouncement = sendToAnnouncement.GetValue<bool>();

            // Extracts current news UUID for logging
            string uuid = _currentNewsObj?["id"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(uuid))
                _currentNewsUuid = uuid;

            // Sanitize surface/category and update dateReceived to current UTC time
            _currentNewsObj["surface"] = "InboxMessage";
            _currentNewsObj["inboxCategory"] = "BedrockCosmosNews";
            _currentNewsObj["dateReceived"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

            CosmosConsole.WriteLine($"Current news retrieved. ID: {_currentNewsUuid}");
        }

        internal static bool IsCurrentNewsNew()
        {
            EnsureSeenUuidsLoaded();
            return !_seenUuids.Contains(_currentNewsUuid);
        }

        internal static void MarkCurrentNewsAsSeen()
        {
            EnsureSeenUuidsLoaded();

            if (_seenUuids.Contains(_currentNewsUuid))
            {
                CosmosConsole.WriteLine("UUID already recorded in news history, skipping.");
                return;
            }

            _seenUuids.Add(_currentNewsUuid);
            SaveSeenUuids();

            CosmosConsole.WriteLine($"UUID {_currentNewsUuid} added to news history.");
        }

        internal static void QueueLoginAnnouncementIfNew()
        {
            if (!IsCurrentNewsNew())
            {
                CosmosConsole.WriteLine("No new news to queue.");
                return;
            }

            if (_currentNewsObj == null)
            {
                CosmosConsole.WriteLine("Current news data is empty; cannot queue announcement.");
                return;
            }

            JsonObject announcement = JsonNode.Parse(_currentNewsObj.ToJsonString())?.AsObject();
            announcement["surface"] = "LoginAnnouncement";

            // Generates new UUID for announcement to prevent not showing in inbox for versions >26.30
            string annoucementId = announcement["id"]?.GetValue<string>();
            string newId = Guid.NewGuid().ToString();
            announcement["id"] = newId;
            announcement["instanceId"] = newId;

            // Handles inboxed persona item lists
            if (announcement["template"]?.GetValue<string>() == "ContentListNoCTA")
                announcement["template"] = "HeroImageCTA";

            File.WriteAllText(
                PathDefinitions.CustomJsonsDirectory + "CurrentLoginAnnouncement.json",
                announcement.ToJsonString(_jsonWriteIndented)
            );

            CosmosConsole.WriteLine($"LoginAnnouncement queued for ID: {annoucementId}");
        }

        internal static void AddNewsToHistory()
        {
            if (_currentNewsObj == null)
            {
                CosmosConsole.WriteLine("No news data loaded. Call RetrieveCurrentNews() first.");
                return;
            }

            if (_newsHistoryObj == null)
                CreateNewsHistoryFile();

            JsonArray messages = _newsHistoryObj["messages"]?.AsArray() ?? new JsonArray();

            foreach (JsonNode node in messages)
            {
                JsonObject existing = node?.AsObject();
                if (existing?["id"]?.GetValue<string>() == _currentNewsUuid)
                {
                    CosmosConsole.WriteLine("News item already present in history. Skipping.");
                    return;
                }
            }

            JsonNode clonedNews = JsonNode.Parse(_currentNewsObj.ToJsonString());
            messages.Insert(0, clonedNews);
            _newsHistoryObj["messages"] = messages;
            _newsHistoryObj["totalNumberOfMessages"] = messages.Count;

            int unreadCount = 0;
            foreach (JsonNode node in messages)
            {
                JsonObject msg = node?.AsObject();
                if (string.Equals(msg?["status"]?.GetValue<string>(), "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }
            _newsHistoryObj["totalNumberOfUnreadMessages"] = unreadCount;

            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToJsonString(_jsonWriteIndented));

            CosmosConsole.WriteLine($"News item {_currentNewsUuid} added to history. " +
                                    $"Total: {messages.Count}, Unread: {unreadCount}");
        }

        internal static void InterpretNewsEvent(string eventJson)
        {
            if (string.IsNullOrWhiteSpace(eventJson))
            {
                CosmosConsole.WriteLine("InterpretNewsEvent: Empty or null JSON received.");
                return;
            }

            JsonObject eventObj;
            try
            {
                eventObj = JsonNode.Parse(eventJson)?.AsObject();
            }
            catch (JsonException ex)
            {
                CosmosConsole.WriteLine($"InterpretNewsEvent: Failed to parse JSON. {ex.Message}");
                return;
            }

            JsonArray events = eventObj?["events"]?.AsArray() ?? new JsonArray();

            if (events.Count == 0)
            {
                CosmosConsole.WriteLine("InterpretNewsEvent: No events found in payload.");
                return;
            }

            foreach (JsonNode node in events)
            {
                JsonObject ev = node?.AsObject();
                string eventType = ev?["eventType"]?.GetValue<string>() ?? string.Empty;
                string instanceId = ev?["instanceId"]?.GetValue<string>() ?? string.Empty;

                CosmosConsole.WriteLine($"InterpretNewsEvent: Processing event type '{eventType}'.");

                switch (eventType)
                {
                    case "Impression":
                        if (string.IsNullOrWhiteSpace(instanceId))
                        {
                            CosmosConsole.WriteLine("InterpretNewsEvent: Impression event missing instanceId. Skipping.");
                            break;
                        }
                        OnNewsImpression(instanceId);
                        break;

                    case "Delete":
                        if (string.IsNullOrWhiteSpace(instanceId))
                        {
                            CosmosConsole.WriteLine("InterpretNewsEvent: Delete event missing instanceId. Skipping.");
                            break;
                        }
                        OnNewsDelete(instanceId);
                        break;

                    case "ReadAll":
                        OnNewsReadAll();
                        break;

                    case "DeleteAllRead":
                        OnNewsDeleteAllRead();
                        break;

                    default:
                        CosmosConsole.WriteLine($"InterpretNewsEvent: Unknown event type '{eventType}'. Skipping.");
                        break;
                }
            }
        }

        internal static void ResetNewsVariables()
        {
            _currentNewsUuid = "00000000-0000-4000-0000-000000000000";
            _sendNewsToInbox = true;
            _sendNewsToAnnouncement = true;
            _newsHistoryObj = null;
            _currentNewsObj = null;
            _seenUuids = null;
        }

        private static void OnNewsImpression(string instanceId)
        {
            JsonArray messages = GetMessagesOrWarn("OnNewsImpression");
            if (messages == null) return;

            JsonObject targetMessage = FindByInstanceId(messages, instanceId);
            if (targetMessage == null)
            {
                CosmosConsole.WriteLine($"OnNewsImpression: No message found with instanceId '{instanceId}'. Nothing to update.");
                return;
            }

            if (string.Equals(targetMessage["status"]?.GetValue<string>(), "Read", StringComparison.OrdinalIgnoreCase))
            {
                CosmosConsole.WriteLine($"OnNewsImpression: Message '{instanceId}' is already marked as read. Skipping.");
                return;
            }

            targetMessage["status"] = "Read";
            UpdateAndSaveUnreadCount(messages);

            int unreadCount = _newsHistoryObj["totalNumberOfUnreadMessages"]?.GetValue<int>() ?? 0;
            CosmosConsole.WriteLine($"OnNewsImpression: Message '{instanceId}' marked as read. Unread remaining: {unreadCount}.");
        }

        private static void OnNewsDelete(string instanceId)
        {
            JsonArray messages = GetMessagesOrWarn("OnNewsDelete");
            if (messages == null) return;

            JsonObject targetMessage = FindByInstanceId(messages, instanceId);
            if (targetMessage == null)
            {
                CosmosConsole.WriteLine($"OnNewsDelete: No message found with instanceId '{instanceId}'. Nothing to update.");
                return;
            }

            int indexToRemove = -1;
            for (int i = 0; i < messages.Count; i++)
            {
                if (ReferenceEquals(messages[i]?.AsObject(), targetMessage))
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove >= 0)
                messages.RemoveAt(indexToRemove);

            _newsHistoryObj["totalNumberOfMessages"] = messages.Count;
            UpdateAndSaveUnreadCount(messages);

            int unreadCount = _newsHistoryObj["totalNumberOfUnreadMessages"]?.GetValue<int>() ?? 0;
            CosmosConsole.WriteLine($"OnNewsDelete: Message '{instanceId}' removed. " +
                                    $"Remaining: {messages.Count}, Unread: {unreadCount}.");
        }

        private static void OnNewsReadAll()
        {
            JsonArray messages = GetMessagesOrWarn("OnNewsReadAll");
            if (messages == null) return;

            foreach (JsonNode node in messages)
            {
                JsonObject msg = node?.AsObject();
                if (msg != null)
                    msg["status"] = "Read";
            }

            _newsHistoryObj["totalNumberOfUnreadMessages"] = 0;

            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToJsonString(_jsonWriteIndented));

            CosmosConsole.WriteLine($"OnNewsReadAll: Marked {messages.Count} message(s) as read and saved to history.");
        }

        private static void OnNewsDeleteAllRead()
        {
            JsonArray messages = GetMessagesOrWarn("OnNewsDeleteAllRead");
            if (messages == null) return;

            JsonArray remaining = new JsonArray();
            int deletedCount = 0;

            foreach (JsonNode node in messages)
            {
                JsonObject msg = node?.AsObject();
                if (string.Equals(msg?["status"]?.GetValue<string>(), "Read", StringComparison.OrdinalIgnoreCase))
                {
                    deletedCount++;
                }
                else
                {
                    remaining.Add(JsonNode.Parse(msg?.ToJsonString() ?? "{}"));
                }
            }

            if (deletedCount == 0)
            {
                CosmosConsole.WriteLine("OnNewsDeleteAllRead: No read messages found. Nothing to delete.");
                return;
            }

            _newsHistoryObj["messages"] = remaining;
            _newsHistoryObj["totalNumberOfMessages"] = remaining.Count;
            UpdateAndSaveUnreadCount(remaining);

            int unreadCount = _newsHistoryObj["totalNumberOfUnreadMessages"]?.GetValue<int>() ?? 0;
            CosmosConsole.WriteLine($"OnNewsDeleteAllRead: Deleted {deletedCount} read message(s). " +
                                    $"Remaining: {remaining.Count}, Unread: {unreadCount}.");
        }

        // Helpers
        private static JsonArray GetMessagesOrWarn(string callerName)
        {
            if (_newsHistoryObj == null)
            {
                CosmosConsole.WriteLine($"{callerName}: News history not loaded. Nothing to update.");
                return null;
            }

            JsonArray messages = _newsHistoryObj["messages"]?.AsArray() ?? new JsonArray();

            if (messages.Count == 0)
            {
                CosmosConsole.WriteLine($"{callerName}: No messages found in history. Nothing to update.");
                return null;
            }

            return messages;
        }

        private static JsonObject FindByInstanceId(JsonArray messages, string instanceId)
        {
            foreach (JsonNode node in messages)
            {
                JsonObject msg = node?.AsObject();
                if (string.Equals(msg?["instanceId"]?.GetValue<string>(), instanceId, StringComparison.OrdinalIgnoreCase))
                    return msg;
            }
            return null;
        }

        private static void UpdateAndSaveUnreadCount(JsonArray messages)
        {
            int unreadCount = 0;
            foreach (JsonNode node in messages)
            {
                JsonObject msg = node?.AsObject();
                if (string.Equals(msg?["status"]?.GetValue<string>(), "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }

            _newsHistoryObj["totalNumberOfUnreadMessages"] = unreadCount;
            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToJsonString(_jsonWriteIndented));
        }

        private static void EnsureSeenUuidsLoaded()
        {
            if (_seenUuids != null) return;

            string dir = Path.GetDirectoryName(_newsHistoryUuidsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_newsHistoryUuidsPath))
            {
                File.WriteAllText(_newsHistoryUuidsPath, "[]");
                _seenUuids = new List<string>();
                return;
            }

            string json = File.ReadAllText(_newsHistoryUuidsPath);
            _seenUuids = string.IsNullOrWhiteSpace(json)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        private static void SaveSeenUuids()
        {
            File.WriteAllText(
                _newsHistoryUuidsPath,
                JsonSerializer.Serialize(_seenUuids, _jsonWriteIndented)
            );
        }
    }
}