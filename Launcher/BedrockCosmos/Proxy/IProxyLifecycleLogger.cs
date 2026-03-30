namespace BedrockCosmos.Proxy
{
    public interface IProxyLifecycleLogger
    {
        void Log(string eventName, object details);
    }
}
