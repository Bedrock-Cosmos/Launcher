using BedrockCosmos.Proxy;
using System;

namespace BedrockCosmos.App
{
    internal static class ProxyLifecycleFactory
    {
        internal static ProxyLifecycleManager CreateDefault()
        {
            return new ProxyLifecycleManager(
                new WinInetProxySettingsAccessor(),
                new ProxyStateStore(PathDefinitions.ProxyStateFile),
                new TcpProxyAvailabilityProbe(TimeSpan.FromMilliseconds(750)),
                new StructuredProxyLifecycleLogger(),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromMilliseconds(150));
        }
    }
}
