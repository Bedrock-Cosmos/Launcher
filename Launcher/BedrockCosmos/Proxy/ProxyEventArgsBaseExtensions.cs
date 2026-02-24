using Titanium.Web.Proxy.EventArguments;

namespace BedrockCosmos.Proxy
{
    public static class ProxyEventArgsBaseExtensions
    {
        public static ProxyClientState GetState(this ProxyEventArgsBase args)
        {
            if (args.ClientUserData == null) args.ClientUserData = new ProxyClientState();

            return (ProxyClientState)args.ClientUserData;
        }
    }
}