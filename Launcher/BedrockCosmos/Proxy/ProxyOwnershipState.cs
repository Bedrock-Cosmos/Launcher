using System;

namespace BedrockCosmos.Proxy
{
    public sealed class ProxyOwnershipState
    {
        public string OwnershipMarker { get; set; }
        public int OwnerProcessId { get; set; }
        public string LauncherVersion { get; set; }
        public DateTime AppliedAtUtc { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public ProxySettingsSnapshot PreviousSettings { get; set; }
        public ProxySettingsSnapshot AppliedSettings { get; set; }
    }
}
