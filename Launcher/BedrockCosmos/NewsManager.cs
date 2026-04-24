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
        private static string _newsHistory = "";
        private static string _currentNews = "";
        private static string _currentNewsUuid = "00000000-0000-4000-0000-000000000000";

        private static readonly string _newsHistoryPath = PathDefinitions.CustomJsonsDirectory + @"News.json";
        private static readonly string _newsHistoryUuidsPath = PathDefinitions.MiscDirectory + @"NewsHistory.json";
        private static readonly string _currentNewsPath = PathDefinitions.ResponsesDirectory + @"News\CurrentNews_append.json";
        private static readonly string _loginAnnouncementPath = PathDefinitions.ResponsesDirectory + @"News\LoginAnnouncement_append.json";

        internal static string NewsHistory
        {
            get { return _newsHistory; }
            set { _newsHistory = value; }
        }

        internal static string CurrentNews
        {
            get { return _currentNews; }
            set { _currentNews = value; }
        }

        internal static void CreateNewsHistoryFile()
        {
            string newHistoryFile = @"{
  ""category"": ""BedrockCosmosNews"",
  ""totalNumberOfUnreadMessages"": 0,
  ""totalNumberOfMessages"": 0,
  ""messages"": [],
  ""categoryInfo"": {
     ""name"": ""Bedrock Cosmos News"",
     ""image"": {
        ""id"": ""8642b05e-b0e1-4057-94d8-98552e53a23a"",
        ""url"": ""https://bedrock-cosmos.app/icons/NewsIcon.png""
     },
     ""type"": ""BedrockCosmosNews""
  }
}";

            File.WriteAllText(_newsHistoryPath, newHistoryFile);
        }

        internal static void RetrieveNewsHistory()
        {
            if (!Directory.Exists(PathDefinitions.CustomJsonsDirectory))
                Directory.CreateDirectory(PathDefinitions.CustomJsonsDirectory);

            if (!File.Exists(_newsHistoryPath))
                CreateNewsHistoryFile();

            _newsHistory = File.ReadAllText(_newsHistoryPath);

            JObject newsHistoryObj = JObject.Parse(_newsHistory);
        }

        internal static void RetrieveCurrentNews()
        {
            if (!File.Exists(_currentNewsPath))
            {
                CosmosConsole.WriteLine("No current news file found.");
                return;
            }

            _currentNews = File.ReadAllText(_currentNewsPath);

            JObject newsObj = JObject.Parse(_currentNews);
            string uuid = (string)newsObj["id"];

            if (!string.IsNullOrWhiteSpace(uuid))
                _currentNewsUuid = uuid;

            CosmosConsole.WriteLine($"Current news retrieved. ID: {_currentNewsUuid}");
        }

        internal static bool IsCurrentNewsNew()
        {
            EnsureNewsHistoryFileExists();

            string json = File.ReadAllText(_newsHistoryUuidsPath);

            List<string> seenUuids = string.IsNullOrWhiteSpace(json)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

            return !seenUuids.Contains(_currentNewsUuid);
        }

        internal static void MarkCurrentNewsAsSeen()
        {
            EnsureNewsHistoryFileExists();

            string json = File.ReadAllText(_newsHistoryUuidsPath);

            List<string> seenUuids = string.IsNullOrWhiteSpace(json)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

            if (seenUuids.Contains(_currentNewsUuid))
            {
                CosmosConsole.WriteLine("UUID already recorded in news history, skipping.");
                return;
            }

            seenUuids.Add(_currentNewsUuid);

            File.WriteAllText(
                _newsHistoryUuidsPath,
                JsonConvert.SerializeObject(seenUuids, Formatting.Indented)
            );

            CosmosConsole.WriteLine($"UUID {_currentNewsUuid} added to news history.");
        }

        internal static void QueueLoginAnnouncementIfNew()
        {
            if (!IsCurrentNewsNew())
            {
                CosmosConsole.WriteLine("No new news to queue.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentNews))
            {
                CosmosConsole.WriteLine("Current news data is empty; cannot queue announcement.");
                return;
            }

            JObject source = JObject.Parse(_currentNews);

            // Pull the fields needed from CurrentNews
            string id = (string)source["id"] ?? Guid.NewGuid().ToString();
            string header = (string)source["header"] ?? "";
            string body = (string)source["body"] ?? "";
            string imageId = (string)source["images"]?["Primary"]?["id"] ?? Guid.NewGuid().ToString();
            string imageUrl = (string)source["images"]?["Primary"]?["url"] ?? "";
            string dateReceived = (string)source["dateReceived"] ?? DateTime.UtcNow.ToString("YYYY-MM-DDT12:00:00.0000000Z");
            string msgHeader = (string)source["messageText"]?["header"] ?? header;
            string msgBody = (string)source["messageText"]?["body"] ?? body;

            // Build the LoginAnnouncement object
            JObject announcement = new JObject
            {
                ["id"] = id,
                ["instanceId"] = id,
                ["surface"] = "LoginAnnouncement",
                ["template"] = "HeroImageCTA",
                ["header"] = header,
                ["body"] = body,
                ["images"] = new JObject
                {
                    ["Primary"] = new JObject
                    {
                        ["id"] = imageId,
                        ["url"] = imageUrl
                    }
                },
                ["buttons"] = new JObject(),
                ["reportId"] = "Njk0NWIwMGM1Y2MzOTUwMDY1NTViNTRjXyRfY2M9NzJhMTJlYzgtMzU5Mi00ZmU0LTRhOWItMDNiYThiNjk2ZWNiJm12PTY5NDViMDBjNWNjMzk1MDA2NTU1YjU0NiZwaT1jbXA=",
                ["isControl"] = false,
                ["sender"] = "Bedrock Cosmos",
                ["status"] = "Unread",
                ["persistToInboxOnImpression"] = true,
                ["inboxCategory"] = "BedrockCosmosNews",
                ["dateReceived"] = dateReceived,
                ["itemList"] = new JArray(),
                ["style"] = "Default",
                ["associatedProducts"] = new JArray(),
                ["messageItemList"] = new JArray(),
                ["messageText"] = new JObject
                {
                    ["header"] = msgHeader,
                    ["body"] = msgBody
                },
                ["versionConstraint"] = "≥ 1.21.100",
                ["colors"] = new JObject(),
                ["isSentimentSubmitButtonVisible"] = false
            };

            File.WriteAllText(
                PathDefinitions.CustomJsonsDirectory + "CurrentLoginAnnouncement.json",
                announcement.ToString(Formatting.Indented)
            );

            CosmosConsole.WriteLine($"LoginAnnouncement queued for ID: {id}");
        }

        private static void EnsureNewsHistoryFileExists()
        {
            string dir = Path.GetDirectoryName(_newsHistoryUuidsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_newsHistoryUuidsPath))
                File.WriteAllText(_newsHistoryUuidsPath, "[]");
        }

        internal static void AddNewsToHistory()
        {
            if (string.IsNullOrWhiteSpace(_currentNews))
            {
                CosmosConsole.WriteLine("No news data loaded. Call RetrieveCurrentNews() first.");
                return;
            }

            if (!File.Exists(_newsHistoryPath))
                CreateNewsHistoryFile();

            JObject history = JObject.Parse(File.ReadAllText(_newsHistoryPath));
            JArray messages = (JArray)(history["messages"] ?? new JArray());
            JObject incomingNews = JObject.Parse(_currentNews);

            // Prevent duplicate entries if the UUID is already in the message list.
            foreach (JObject existing in messages)
            {
                if ((string)existing["id"] == _currentNewsUuid)
                {
                    CosmosConsole.WriteLine("News item already present in history. Skipping.");
                    return;
                }
            }

            messages.Insert(0, incomingNews);
            history["messages"] = messages;
            history["totalNumberOfMessages"] = messages.Count;

            // Count unread by checking the status field on each message.
            int unreadCount = 0;
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["status"], "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }
            history["totalNumberOfUnreadMessages"] = unreadCount;

            _newsHistory = history.ToString(Formatting.Indented);
            File.WriteAllText(_newsHistoryPath, _newsHistory);

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

            foreach (JObject ev in events) // Can process multiple events at once, though this shouldn't really happen naturally.
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

        private static void OnNewsImpression(string instanceId)
        {
            if (!File.Exists(_newsHistoryPath))
            {
                CosmosConsole.WriteLine("OnNewsImpression: No news history file found. Nothing to update.");
                return;
            }

            JObject history;
            try
            {
                history = JObject.Parse(File.ReadAllText(_newsHistoryPath));
            }
            catch (JsonException ex)
            {
                CosmosConsole.WriteLine($"OnNewsImpression: Failed to parse news history. {ex.Message}");
                return;
            }

            JArray messages = (JArray)(history["messages"] ?? new JArray());

            if (messages.Count == 0)
            {
                CosmosConsole.WriteLine("OnNewsImpression: No messages found in history. Nothing to update.");
                return;
            }

            JObject targetMessage = null;
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["instanceId"], instanceId, StringComparison.OrdinalIgnoreCase))
                {
                    targetMessage = msg;
                    break;
                }
            }

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

            int unreadCount = 0;
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["status"], "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }

            history["messages"] = messages;
            history["totalNumberOfUnreadMessages"] = unreadCount;

            _newsHistory = history.ToString(Formatting.Indented);
            File.WriteAllText(_newsHistoryPath, _newsHistory);

            CosmosConsole.WriteLine($"OnNewsImpression: Message '{instanceId}' marked as read. Unread remaining: {unreadCount}.");
        }

        private static void OnNewsDelete(string instanceId)
        {
            if (!File.Exists(_newsHistoryPath))
            {
                CosmosConsole.WriteLine("OnNewsDelete: No news history file found. Nothing to update.");
                return;
            }

            JObject history;
            try
            {
                history = JObject.Parse(File.ReadAllText(_newsHistoryPath));
            }
            catch (JsonException ex)
            {
                CosmosConsole.WriteLine($"OnNewsDelete: Failed to parse news history. {ex.Message}");
                return;
            }

            JArray messages = (JArray)(history["messages"] ?? new JArray());

            if (messages.Count == 0)
            {
                CosmosConsole.WriteLine("OnNewsDelete: No messages found in history. Nothing to update.");
                return;
            }

            JObject targetMessage = null;
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["instanceId"], instanceId, StringComparison.OrdinalIgnoreCase))
                {
                    targetMessage = msg;
                    break;
                }
            }

            if (targetMessage == null)
            {
                CosmosConsole.WriteLine($"OnNewsDelete: No message found with instanceId '{instanceId}'. Nothing to update.");
                return;
            }

            messages.Remove(targetMessage);

            int unreadCount = 0;
            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["status"], "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }

            history["messages"] = messages;
            history["totalNumberOfMessages"] = messages.Count;
            history["totalNumberOfUnreadMessages"] = unreadCount;

            _newsHistory = history.ToString(Formatting.Indented);
            File.WriteAllText(_newsHistoryPath, _newsHistory);

            CosmosConsole.WriteLine($"OnNewsDelete: Message '{instanceId}' removed. " +
                                    $"Remaining: {messages.Count}, Unread: {unreadCount}.");
        }

        private static void OnNewsReadAll()
        {
            if (!File.Exists(_newsHistoryPath))
            {
                CosmosConsole.WriteLine("OnNewsReadAll: No news history file found. Nothing to update.");
                return;
            }

            JObject history;
            try
            {
                history = JObject.Parse(File.ReadAllText(_newsHistoryPath));
            }
            catch (JsonException ex)
            {
                CosmosConsole.WriteLine($"OnNewsReadAll: Failed to parse news history. {ex.Message}");
                return;
            }

            JArray messages = (JArray)(history["messages"] ?? new JArray());

            if (messages.Count == 0)
            {
                CosmosConsole.WriteLine("OnNewsReadAll: No messages found in history. Nothing to update.");
                return;
            }

            foreach (JObject msg in messages)
            {
                msg["status"] = "Read";
            }

            history["messages"] = messages;
            history["totalNumberOfUnreadMessages"] = 0;

            _newsHistory = history.ToString(Formatting.Indented);
            File.WriteAllText(_newsHistoryPath, _newsHistory);

            CosmosConsole.WriteLine($"OnNewsReadAll: Marked {messages.Count} message(s) as read and saved to history.");
        }

        private static void OnNewsDeleteAllRead()
        {
            if (!File.Exists(_newsHistoryPath))
            {
                CosmosConsole.WriteLine("OnNewsDeleteAllRead: No news history file found. Nothing to update.");
                return;
            }

            JObject history;
            try
            {
                history = JObject.Parse(File.ReadAllText(_newsHistoryPath));
            }
            catch (JsonException ex)
            {
                CosmosConsole.WriteLine($"OnNewsDeleteAllRead: Failed to parse news history. {ex.Message}");
                return;
            }

            JArray messages = (JArray)(history["messages"] ?? new JArray());

            if (messages.Count == 0)
            {
                CosmosConsole.WriteLine("OnNewsDeleteAllRead: No messages found in history. Nothing to update.");
                return;
            }

            JArray remainingMessages = new JArray();
            int deletedCount = 0;

            foreach (JObject msg in messages)
            {
                if (string.Equals((string)msg["status"], "Read", StringComparison.OrdinalIgnoreCase))
                    deletedCount++;
                else
                    remainingMessages.Add(msg);
            }

            if (deletedCount == 0)
            {
                CosmosConsole.WriteLine("OnNewsDeleteAllRead: No read messages found. Nothing to delete.");
                return;
            }

            int unreadCount = 0;
            foreach (JObject msg in remainingMessages)
            {
                if (string.Equals((string)msg["status"], "Unread", StringComparison.OrdinalIgnoreCase))
                    unreadCount++;
            }

            history["messages"] = remainingMessages;
            history["totalNumberOfMessages"] = remainingMessages.Count;
            history["totalNumberOfUnreadMessages"] = unreadCount;

            _newsHistory = history.ToString(Formatting.Indented);
            File.WriteAllText(_newsHistoryPath, _newsHistory);

            CosmosConsole.WriteLine($"OnNewsDeleteAllRead: Deleted {deletedCount} read message(s). " +
                                    $"Remaining: {remainingMessages.Count}, Unread: {unreadCount}.");
        }
    }
}