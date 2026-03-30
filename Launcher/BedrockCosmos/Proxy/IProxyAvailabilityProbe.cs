using System.Threading.Tasks;

namespace BedrockCosmos.Proxy
{
    public interface IProxyAvailabilityProbe
    {
        Task<bool> IsListeningAsync(string host, int port);
    }
}
