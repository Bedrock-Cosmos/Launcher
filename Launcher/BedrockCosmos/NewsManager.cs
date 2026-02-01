using BedrockCosmos.App;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace BedrockCosmos
{
    internal class NewsManager
    {
        private static string _newsHistory = "";
        private static string _currentNews = "";
        private static string _currentNewsUuid = "00000000-0000-4000-0000-000000000000";
        private static int _cosmosUnreadNewsCount = 0;
        private static int _cosmosNewsCount = 0;
        
        private static string newsHistoryPath = AppDomain.CurrentDomain.BaseDirectory + @"news.json";
        private static string receivedNewsPath = AppDomain.CurrentDomain.BaseDirectory + @"receivedNews.json";
        private static string currentResponsePath = AppDomain.CurrentDomain.BaseDirectory + @"Responses-main\";

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

        internal static int CosmosUnreadNewsCount
        {
            get { return _cosmosUnreadNewsCount; }
            set { _cosmosUnreadNewsCount = value; }
        }

        internal static int CosmosNewsCount
        {
            get { return _cosmosNewsCount; }
            set { _cosmosNewsCount = value; }
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
        ""url"": ""https://raw.githubusercontent.com/Bedrock-Cosmos/Backend/main/Main/Thumbnails/News/NewsIcon.png""
     },
     ""type"": ""BedrockCosmosNews""
  }
}";

            File.WriteAllText(newsHistoryPath, newHistoryFile);
        }

        internal static void RetrieveNewsHistory()
        {
            if (!File.Exists(newsHistoryPath))
                CreateNewsHistoryFile();

            /*_newsHistory = File.ReadAllText(newsHistoryPath);

            JObject newsHistoryObj = JObject.Parse(_newsHistory);
            _cosmosUnreadNewsCount = (int)newsHistoryObj["totalNumberOfUnreadMessages"];
            _cosmosNewsCount = (int)newsHistoryObj["totalNumberOfMessages"];*/
        }

        internal static void RetrieveCurrentNews()
        {
            string currentNewsPath = currentResponsePath + @"News\CurrentNews_append.json";

            if (File.Exists(currentNewsPath))
            {
                _currentNews = File.ReadAllText(currentNewsPath);
                JObject newsObj = JObject.Parse(_currentNews);
                string uuid = (string)newsObj["id"];
                _currentNewsUuid = uuid;
            }
        }

        internal static void CheckForNews()
        {
            if (!File.Exists(receivedNewsPath))
                File.WriteAllText(receivedNewsPath, "[]");

            List<string> previousNewsUuids;

            string json = File.ReadAllText(receivedNewsPath);

            previousNewsUuids = string.IsNullOrWhiteSpace(json)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

            if (!previousNewsUuids.Contains(_currentNewsUuid))
            {
                previousNewsUuids.Add(_currentNewsUuid);

                string updatedJson = JsonConvert.SerializeObject(
                    previousNewsUuids,
                    Formatting.Indented
                );

                // Move the line below to when request is ran
                File.WriteAllText(receivedNewsPath, updatedJson);

                CosmosConsole.WriteLine("App", "Found a news update. Queued for display.");
            }
            else
            {
                CosmosConsole.WriteLine("App", "No updated news found.");
            }
        }

        internal static void AddNewsToHistory()
        {

        }
    }
}