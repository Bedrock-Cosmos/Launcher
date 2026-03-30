using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BedrockCosmos.Proxy
{
    public sealed class TcpProxyAvailabilityProbe : IProxyAvailabilityProbe
    {
        private readonly TimeSpan timeout;

        public TcpProxyAvailabilityProbe(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        public async Task<bool> IsListeningAsync(string host, int port)
        {
            using (var client = new TcpClient())
            {
                Task connectTask = client.ConnectAsync(host, port);
                Task completedTask = await Task.WhenAny(connectTask, Task.Delay(timeout)).ConfigureAwait(false);

                if (completedTask != connectTask)
                    return false;

                try
                {
                    await connectTask.ConfigureAwait(false);
                    return client.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
