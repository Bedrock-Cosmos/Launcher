namespace BedrockCosmos.Proxy
{
    public sealed class ProxySettingsSnapshot
    {
        public const int ProxyTypeDirect = 0x00000001;
        public const int ProxyTypeProxy = 0x00000002;
        public const int ProxyTypeAutoProxyUrl = 0x00000004;
        public const int ProxyTypeAutoDetect = 0x00000008;

        public int Flags { get; set; }
        public string ProxyServer { get; set; }
        public string ProxyBypass { get; set; }
        public string AutoConfigUrl { get; set; }
        public string Classification { get; set; }

        public static ProxySettingsSnapshot CreateBedrockCosmosProxy(string host, int port, string previousBypass)
        {
            return new ProxySettingsSnapshot
            {
                Flags = ProxyTypeDirect | ProxyTypeProxy,
                ProxyServer = string.Format("http={0}:{1};https={0}:{1}", host, port),
                ProxyBypass = previousBypass,
                AutoConfigUrl = null,
                Classification = "bedrock-cosmos-manual-proxy"
            };
        }

        public ProxySettingsSnapshot Clone()
        {
            return new ProxySettingsSnapshot
            {
                Flags = Flags,
                ProxyServer = ProxyServer,
                ProxyBypass = ProxyBypass,
                AutoConfigUrl = AutoConfigUrl,
                Classification = Classification
            };
        }

        public void RefreshClassification()
        {
            Classification = DetermineClassification(Flags);
        }

        public bool SemanticallyEquals(ProxySettingsSnapshot other)
        {
            if (other == null)
                return false;

            return Flags == other.Flags
                && Normalize(ProxyServer) == Normalize(other.ProxyServer)
                && Normalize(ProxyBypass) == Normalize(other.ProxyBypass)
                && Normalize(AutoConfigUrl) == Normalize(other.AutoConfigUrl);
        }

        public static string DetermineClassification(int flags)
        {
            bool usesManual = (flags & ProxyTypeProxy) != 0;
            bool usesAutoConfig = (flags & ProxyTypeAutoProxyUrl) != 0;
            bool usesAutoDetect = (flags & ProxyTypeAutoDetect) != 0;

            if (!usesManual && !usesAutoConfig && !usesAutoDetect)
                return "no-proxy";

            if (usesManual && !usesAutoConfig && !usesAutoDetect)
                return "manual-proxy";

            if (!usesManual && usesAutoConfig && !usesAutoDetect)
                return "pac-script";

            if (!usesManual && !usesAutoConfig && usesAutoDetect)
                return "auto-detect";

            return "mixed";
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }
    }
}
