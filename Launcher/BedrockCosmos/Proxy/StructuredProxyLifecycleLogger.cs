using BedrockCosmos.App;

namespace BedrockCosmos.Proxy
{
    public sealed class StructuredProxyLifecycleLogger : IProxyLifecycleLogger
    {
        public void Log(string eventName, object details)
        {
            StructuredLogger.Log("proxy", eventName, details);
        }
    }
}
