using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;

namespace BedrockCosmos
{
    public static class JsonData
    {
        public static List<string> AllowedUrls = 
            JsonConvert.DeserializeObject<List<string>>
            (File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Jsons\AllowedUrls.json"));

        public static List<Endpoint> MainPages =
            JsonConvert.DeserializeObject<List<Endpoint>>
            (File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Jsons\MainResponses.json"));

        public static List<MarketItem> MarketItems =
            JsonConvert.DeserializeObject<List<MarketItem>>
            (File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Jsons\PlayfabGetPublishItemResponses.json"));

        public static List<MarketItem> PackSearchIds =
            JsonConvert.DeserializeObject<List<MarketItem>>
            (File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Jsons\PlayfabSearchResponses.json"));
    }
}
