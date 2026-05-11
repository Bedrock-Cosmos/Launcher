using BedrockCosmos.App;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace BedrockCosmos
{
    internal class NewsManager
    {
        private static string _currentNewsUuid = "00000000-0000-4000-0000-000000000000";
        private static bool _sendNewsToInbox = true;
        private static bool _sendNewsToAnnouncement = true;

        private static JObject _newsHistoryObj = null;
        private static JObject _currentNewsObj = null;
        private static List<string> _seenUuids = null;

        private static readonly string _newsHistoryPath = Path.Combine(PathDefinitions.CustomJsonsDirectory, "News.json");
        private static readonly string _newsHistoryUuidsPath = Path.Combine(PathDefinitions.MiscDirectory, "NewsHistory.json");
        private static readonly string _currentNewsPath = Path.Combine(PathDefinitions.ResponsesDirectory, @"MainPages\CurrentNews_append.json");

        internal static bool SendToNewsInbox
        {
            get { return _sendNewsToInbox; }
        }

        internal static bool SendToNewsAnnouncement
        {
            get { return _sendNewsToAnnouncement; }
        }

        internal static string NewsHistoryPath
        {
            get { return _newsHistoryPath; }
        }

        // String properties to serialize from  cached JObjects on demand
        internal static string NewsHistory =>
            _newsHistoryObj?.ToString(Formatting.Indented) ?? string.Empty;

        internal static string CurrentNews =>
            _currentNewsObj?.ToString(Formatting.Indented) ?? string.Empty;

        internal static void CreateNewsHistoryFile()
        {
            _newsHistoryObj = new JObject
            {
                ["category"] = "BedrockCosmosNews",
                ["totalNumberOfUnreadMessages"] = 0,
                ["totalNumberOfMessages"] = 0,
                ["messages"] = new JArray(),
                ["categoryInfo"] = new JObject
                {
                    ["name"] = "Bedrock Cosmos News",
                    ["image"] = new JObject
                    {
                        ["id"] = "8642b05e-b0e1-4057-94d8-98552e53a23a",
                        ["url"] = "https://bedrock-cosmos.app/icons/NewsIcon.png"
                    },
                    ["type"] = "BedrockCosmosNews"
                }
            };

            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToString(Formatting.Indented));
        }

        internal static void RetrieveNewsHistory()
        {
            if (!Directory.Exists(PathDefinitions.CustomJsonsDirectory))
                Directory.CreateDirectory(PathDefinitions.CustomJsonsDirectory);

            if (!File.Exists(_newsHistoryPath))
                CreateNewsHistoryFile();

            _newsHistoryObj = JObject.Parse(File.ReadAllText(_newsHistoryPath));
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

            _currentNewsObj = JObject.Parse(File.ReadAllText(_currentNewsPath));

            // Optional booleans only used in Bedrock Cosmos news system
            // "cosmosSendToInbox": true/false
            JToken sendToInbox = _currentNewsObj["cosmosSendToInbox"];
            if (sendToInbox != null)
                _sendNewsToInbox = (bool)sendToInbox;

            // "cosmosSendToAnnouncement": true/false
            JToken sendToAnnouncement = _currentNewsObj["cosmosSendToAnnouncement"];
            if (sendToAnnouncement != null)
                _sendNewsToAnnouncement = (bool)sendToAnnouncement;

            // Extracts current news UUID for logging
            string uuid = (string)_currentNewsObj["id"];
            if (!string.IsNullOrWhiteSpace(uuid))
                _currentNewsUuid = uuid;

            // Update dateReceived in the first message to current UTC time
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

            JObject announcement = (JObject)_currentNewsObj.DeepClone();
            announcement["surface"] = "LoginAnnouncement";

            // Handles inboxed persona item lists
            if ((string)announcement["template"] == "ContentListNoCTA")
                announcement["template"] = "HeroImageCTA";

            File.WriteAllText(
                PathDefinitions.CustomJsonsDirectory + "CurrentLoginAnnouncement.json",
                announcement.ToString(Formatting.Indented)
            );

            CosmosConsole.WriteLine($"LoginAnnouncement queued for ID: {announcement["id"]}");
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

            JArray messages = (JArray)(_newsHistoryObj["messages"] ?? new JArray());

            foreach (JObject existing in messages)
            {
                if ((string)existing["id"] == _currentNewsUuid)
                {
                    CosmosConsole.WriteLine("News item already present in history. Skipping.");
                    return;
                }
            }

            messages.Insert(0, _currentNewsObj.DeepClone());
            _newsHistoryObj["messages"] = messages;
            _newsHistoryObj["totalNumberOfMessages"] = messages.Count;

            int unreadCount = 0;
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["status"], "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }
            _newsHistoryObj["totalNumberOfUnreadMessages"] = unreadCount;

            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToString(Formatting.Indented));

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

            JObject eventObj;
            try
            {
                eventObj = JObject.Parse(eventJson);
            }
            catch (JsonException ex)
            {
                CosmosConsole.WriteLine($"InterpretNewsEvent: Failed to parse JSON. {ex.Message}");
                return;
            }

            JArray events = (JArray)(eventObj["events"] ?? new JArray());

            if (events.Count == 0)
            {
                CosmosConsole.WriteLine("InterpretNewsEvent: No events found in payload.");
                return;
            }

            foreach (JObject ev in events)
            {
                string eventType = (string)ev["eventType"] ?? string.Empty;
                string instanceId = (string)ev["instanceId"] ?? string.Empty;

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
            JArray messages = GetMessagesOrWarn("OnNewsImpression");
            if (messages == null) return;

            JObject targetMessage = FindByInstanceId(messages, instanceId);
            if (targetMessage == null)
            {
                CosmosConsole.WriteLine($"OnNewsImpression: No message found with instanceId '{instanceId}'. Nothing to update.");
                return;
            }

            if (string.Equals((string)targetMessage["status"], "Read", StringComparison.OrdinalIgnoreCase))
            {
                CosmosConsole.WriteLine($"OnNewsImpression: Message '{instanceId}' is already marked as read. Skipping.");
                return;
            }

            targetMessage["status"] = "Read";
            UpdateAndSaveUnreadCount(messages);

            int unreadCount = (int)_newsHistoryObj["totalNumberOfUnreadMessages"];
            CosmosConsole.WriteLine($"OnNewsImpression: Message '{instanceId}' marked as read. Unread remaining: {unreadCount}.");
        }

        private static void OnNewsDelete(string instanceId)
        {
            JArray messages = GetMessagesOrWarn("OnNewsDelete");
            if (messages == null) return;

            JObject targetMessage = FindByInstanceId(messages, instanceId);
            if (targetMessage == null)
            {
                CosmosConsole.WriteLine($"OnNewsDelete: No message found with instanceId '{instanceId}'. Nothing to update.");
                return;
            }

            messages.Remove(targetMessage);
            _newsHistoryObj["totalNumberOfMessages"] = messages.Count;
            UpdateAndSaveUnreadCount(messages);

            int unreadCount = (int)_newsHistoryObj["totalNumberOfUnreadMessages"];
            CosmosConsole.WriteLine($"OnNewsDelete: Message '{instanceId}' removed. " +
                                    $"Remaining: {messages.Count}, Unread: {unreadCount}.");
        }

        private static void OnNewsReadAll()
        {
            JArray messages = GetMessagesOrWarn("OnNewsReadAll");
            if (messages == null) return;

            foreach (JObject msg in messages)
                msg["status"] = "Read";

            _newsHistoryObj["totalNumberOfUnreadMessages"] = 0;

            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToString(Formatting.Indented));

            CosmosConsole.WriteLine($"OnNewsReadAll: Marked {messages.Count} message(s) as read and saved to history.");
        }

        private static void OnNewsDeleteAllRead()
        {
            JArray messages = GetMessagesOrWarn("OnNewsDeleteAllRead");
            if (messages == null) return;

            JArray remaining = new JArray();
            int deletedCount = 0;

            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["status"], "Read", StringComparison.OrdinalIgnoreCase))
                    deletedCount++;
                else
                    remaining.Add(msg);
            }

            if (deletedCount == 0)
            {
                CosmosConsole.WriteLine("OnNewsDeleteAllRead: No read messages found. Nothing to delete.");
                return;
            }

            _newsHistoryObj["messages"] = remaining;
            _newsHistoryObj["totalNumberOfMessages"] = remaining.Count;
            UpdateAndSaveUnreadCount(remaining);

            int unreadCount = (int)_newsHistoryObj["totalNumberOfUnreadMessages"];
            CosmosConsole.WriteLine($"OnNewsDeleteAllRead: Deleted {deletedCount} read message(s). " +
                                    $"Remaining: {remaining.Count}, Unread: {unreadCount}.");
        }

        // Helpers
        private static JArray GetMessagesOrWarn(string callerName)
        {
            if (_newsHistoryObj == null)
            {
                CosmosConsole.WriteLine($"{callerName}: News history not loaded. Nothing to update.");
                return null;
            }

            JArray messages = (JArray)(_newsHistoryObj["messages"] ?? new JArray());

            if (messages.Count == 0)
            {
                CosmosConsole.WriteLine($"{callerName}: No messages found in history. Nothing to update.");
                return null;
            }

            return messages;
        }

        private static JObject FindByInstanceId(JArray messages, string instanceId)
        {
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["instanceId"], instanceId, StringComparison.OrdinalIgnoreCase))
                    return msg;
            }
            return null;
        }

        private static void UpdateAndSaveUnreadCount(JArray messages)
        {
            int unreadCount = 0;
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["status"], "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }

            _newsHistoryObj["totalNumberOfUnreadMessages"] = unreadCount;
            File.WriteAllText(_newsHistoryPath, _newsHistoryObj.ToString(Formatting.Indented));
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
                : JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        private static void SaveSeenUuids()
        {
            File.WriteAllText(
                _newsHistoryUuidsPath,
                JsonConvert.SerializeObject(_seenUuids, Formatting.Indented)
            );
        }
    }
}
