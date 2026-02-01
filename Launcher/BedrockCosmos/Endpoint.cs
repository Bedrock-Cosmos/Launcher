using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockCosmos
{
    public class Endpoint
    {
        public string url { get; set; }
        public string response { get; set; }
    }

    public class MarketItem
    {
        public string uuid { get; set; }
        public string response { get; set; }
    }

    public class PlayfabGetPublishedItemBody
    {
        public string etag { get; set; }
        public string itemid { get; set; }
    }

    public class PlayfabGetSearchedItemBody
    {
        public bool count { get; set; }
        public string filter { get; set; }
        public string orderBy { get; set; }
        public string scid { get; set; }
        public int top { get; set; }
    }
}
