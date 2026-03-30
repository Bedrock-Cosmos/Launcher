namespace BedrockCosmos.Proxy
{
    public interface IProxyStateStore
    {
        ProxyOwnershipState Load();
        void Save(ProxyOwnershipState state);
        void Delete();
    }
}
