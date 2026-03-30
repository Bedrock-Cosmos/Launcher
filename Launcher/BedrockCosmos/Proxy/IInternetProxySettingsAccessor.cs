namespace BedrockCosmos.Proxy
{
    public interface IInternetProxySettingsAccessor
    {
        ProxySettingsSnapshot ReadCurrentSettings();
        void ApplySettings(ProxySettingsSnapshot settings);
    }
}
