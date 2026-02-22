using BedrockCosmos.App;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.StreamExtended.Network;
using Titanium.Web.Proxy;

namespace BedrockCosmos
{
    public class ProxyController : IDisposable
    {
        private readonly ProxyServer proxyServer;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentQueue<Tuple<string, string>> consoleMessageQueue
            = new ConcurrentQueue<Tuple<string, string>>();

        private ExplicitProxyEndPoint explicitEndPoint;

        private string currentPathForResponse = AppDomain.CurrentDomain.BaseDirectory + @"Responses-main\";
        private string consoleSender = "Proxy";

        public ProxyController()
        {
            Task.Run(() => ListenToConsole());

            proxyServer = new ProxyServer();

            proxyServer.ExceptionFunc = async exception =>
            {
                if (exception is ProxyHttpException phex)
                    ProxyConsoleWriteLine(consoleSender, exception.Message + ": " + phex.InnerException?.Message);
                else
                    ProxyConsoleWriteLine(consoleSender, exception.Message);
            };

            proxyServer.TcpTimeWaitSeconds = 10;
            proxyServer.ConnectionTimeOutSeconds = 15;
            proxyServer.ReuseSocket = false;
            proxyServer.EnableConnectionPool = false;
            proxyServer.ForwardToUpstreamGateway = true;
            proxyServer.CertificateManager.SaveFakeCertificates = true;
        }

        private CancellationToken CancellationToken => cancellationTokenSource.Token;

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
            proxyServer.Dispose();
        }

        public void StartProxy()
        {
            // Hooks for proxy events
            proxyServer.BeforeRequest += OnRequest;
            proxyServer.BeforeResponse += OnResponse;
            proxyServer.AfterResponse += OnAfterResponse;

            explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);

            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse += OnBeforeTunnelConnectResponse;

            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

            foreach (var endPoint in proxyServer.ProxyEndPoints)
                ProxyConsoleWriteLine(consoleSender, $"Listening on '{endPoint.GetType().Name}' endpoint at Ip {endPoint.IpAddress}" +
                    $" and port: {endPoint.Port}");

            if (RunTime.IsWindows) proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);
        }

        public void Stop()
        {
            explicitEndPoint.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse -= OnBeforeTunnelConnectResponse;

            proxyServer.BeforeRequest -= OnRequest;
            proxyServer.BeforeResponse -= OnResponse;

            proxyServer.Stop();

            // Remove generated certificates
            //proxyServer.CertificateManager.RemoveTrustedRootCertificates();
        }

        private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            var hostname = e.HttpClient.Request.RequestUri.Host;
            e.GetState().PipelineInfo.AppendLine(nameof(OnBeforeTunnelConnectRequest) + ":" + hostname);
            ProxyConsoleWriteLine(consoleSender, "Tunnel to: " + hostname);

            var clientLocalIp = e.ClientLocalEndPoint.Address;
            if (!clientLocalIp.Equals(IPAddress.Loopback) && !clientLocalIp.Equals(IPAddress.IPv6Loopback))
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(clientLocalIp, 0);

            bool existsInAllowedUrls = JsonData.AllowedUrls.Any(url => url == hostname);
            if (!existsInAllowedUrls)
                e.DecryptSsl = false; // Exempts Non-Allowed Urls from proxy
        }

        private void WebSocket_DataSent(object sender, DataEventArgs e)
        {
            var args = (SessionEventArgs)sender;
            WebSocketDataSentReceived(args, e, true);
        }

        private void WebSocket_DataReceived(object sender, DataEventArgs e)
        {
            var args = (SessionEventArgs)sender;
            WebSocketDataSentReceived(args, e, false);
        }

        private void WebSocketDataSentReceived(SessionEventArgs args, DataEventArgs e, bool sent)
        {
            var decoder = sent
                ? args.WebSocketDecoderSend
                : args.WebSocketDecoderReceive;

            foreach (var frame in decoder.Decode(e.Buffer, e.Offset, e.Count))
            {
                if (frame.OpCode == WebsocketOpCode.Binary)
                {
                    var data = frame.Data.ToArray();
                    var str = string.Join(",", data.Select(x => x.ToString("X2")));
                    ProxyConsoleWriteLine(consoleSender, str);
                }
                else if (frame.OpCode == WebsocketOpCode.Text)
                {
                    ProxyConsoleWriteLine(consoleSender, frame.GetText());
                }
            }
        }

        private Task OnBeforeTunnelConnectResponse(object sender, TunnelConnectSessionEventArgs e)
        {
            e.GetState().PipelineInfo
                .AppendLine(nameof(OnBeforeTunnelConnectResponse) + ":" + e.HttpClient.Request.RequestUri);

            return Task.CompletedTask;
        }

        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnRequest) + ":" + e.HttpClient.Request.RequestUri);

            string body = await e.GetRequestBodyAsString();
            e.UserData = body;

            ProxyConsoleWriteLine(consoleSender, "Active Client Connections:" + ((ProxyServer)sender).ClientConnectionCount);
        }

        private async Task MultipartRequestPartSent(object sender, MultipartRequestPartSentEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(MultipartRequestPartSent));

            var session = (SessionEventArgs)sender;
            ProxyConsoleWriteLine(consoleSender, "Multipart form data headers:");
            foreach (var header in e.Headers) ProxyConsoleWriteLine(consoleSender, header.ToString());
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnResponse));

            if (e.HttpClient.ConnectRequest?.TunnelType == TunnelType.Websocket)
            {
                e.DataSent += WebSocket_DataSent;
                e.DataReceived += WebSocket_DataReceived;
            }

            ProxyConsoleWriteLine(consoleSender, "Active Server Connections:" + ((ProxyServer)sender).ServerConnectionCount);

            Endpoint urlData = JsonData.MainPages.FirstOrDefault(o => o.url == e.HttpClient.Request.RequestUri.AbsoluteUri);
            string requestBody = e.UserData as string;
            string currentUri = e.HttpClient.Request.RequestUri.AbsoluteUri;

            if (e.HttpClient.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            urlData != null)
            {
                try
                {
                    e.HttpClient.Response.StatusCode = 200; // Set to OK, otherwise stays at a not found error
                    string localPath = currentPathForResponse + urlData.response;

                    if (currentUri == "https://20ca2.playfabapi.com/Catalog/GetPublishedItem")
                    {
                        // Search for UUID if a PlayFab Get Item endpoint and use as response
                        PlayfabGetPublishedItemBody getItemBody = JsonConvert.DeserializeObject<PlayfabGetPublishedItemBody>(requestBody);
                        MarketItem mItem = JsonData.MarketItems.FirstOrDefault(o => o.uuid == getItemBody.itemid);
                        if (mItem != null)
                        {
                            localPath = currentPathForResponse + mItem.response;
                            //ProxyConsoleWriteLine(consoleSender, "Local path is now " + localPath);

                            string jsonContent = JsonParser.ReadJsonFileContent(localPath);
                            e.SetResponseBodyString(jsonContent);
                            ProxyConsoleWriteLine("Parser", $"Replaced response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");
                        }
                    }
                    else if (currentUri == "https://20ca2.playfabapi.com/Catalog/Search")
                    {
                        // Search for UUID if a PlayFab Search Item endpoint and use as response
                        PlayfabGetSearchedItemBody getSearchBody = JsonConvert.DeserializeObject<PlayfabGetSearchedItemBody>(requestBody);
                        string searchUuid = JsonParser.ExtractPlayfabSearchId(getSearchBody.filter);
                        MarketItem mItem = JsonData.PackSearchIds.FirstOrDefault(o => o.uuid == searchUuid);
                        if (mItem != null)
                        {
                            localPath = currentPathForResponse + mItem.response;
                            //ProxyConsoleWriteLine(consoleSender, "Local path is now " + localPath);

                            string jsonContent = JsonParser.ReadJsonFileContent(localPath);
                            e.SetResponseBodyString(jsonContent);
                            ProxyConsoleWriteLine("Parser", $"Replaced response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");
                        }
                    }
                    else if (currentUri == "https://store.mktpl.minecraft-services.net/api/v1.0/layout/pages/MultiItemPage_StoreRoot" ||
                        currentUri == "https://store.mktpl.minecraft-services.net/api/v1.0/layout/pages/MultiItemPage_SkinsRoot")
                    {
                        // Append custom marketplace button to response if accessing the main page
                        string responseBody = await e.GetResponseBodyAsString();
                        string location = "result.rows"; // Works the same as ["result"]["rows"]
                        string appendedJson = JsonParser.AppendJsonToStart(responseBody, localPath, location); ;

                        e.SetResponseBodyString(appendedJson);
                        CosmosConsole.WriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");
                    }
                    else if (currentUri == "https://messaging.mktpl.minecraft-services.net/api/v1.0/session/start")
                    {
                        string responseBody = await e.GetResponseBodyAsString();
                        // string location = "result.messages";
                        // string announcementPath = currentPathForResponse + @"News\LoginAnnouncement_append.json";
                        // string newsPath = currentPathForResponse + @"News\CurrentNews_append.json";

                        NewsManager.RetrieveNewsHistory();
                        string newsTabDataPath = AppDomain.CurrentDomain.BaseDirectory + @"CustomJsons\news.json";

                        // Append front announcement
                        //string appendedJson = JsonParser.AppendJsonToEnd(responseBody, announcementPath, location);

                        // Append news
                        string location = "result.inboxSummary.categories";
                        string appendedJson = JsonParser.AppendJsonToStart(responseBody, newsTabDataPath, location);

                        e.SetResponseBodyString(appendedJson);
                        ProxyConsoleWriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");
                    }
                    /*else if (currentUri == "https://store.mktpl.minecraft-services.net/api/v1.0/layout/pages/DressingRoom_PersonaProfile")
                    {
                        // Append custom items to main dressing room page
                        string responseBody = await e.GetResponseBodyAsString();
                        string location = "result.rows[2].items[0]";
                        string firstAppendPath = currentPathForResponse + @"MainPages\DressingRoom_PersonaProfile_Persona_append.json";

                        string appendedJson = JsonParser.AppendJsonToStart(responseBody, firstAppendPath, location);

                        e.SetResponseBodyString(appendedJson);
                        ProxyConsoleWriteLine("Parser", appendedJson);
                        ProxyConsoleWriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");
                    }*/
                    else
                    {
                        string jsonContent = JsonParser.ReadJsonFileContent(localPath);
                        e.SetResponseBodyString(jsonContent);
                        ProxyConsoleWriteLine("Parser", $"Replaced response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");
                    }
                }
                catch (Exception ex)
                {
                    ProxyConsoleWriteLine("Parser", $"Error replacing response: {ex.Message}");
                }
            }
        }

        private async Task OnAfterResponse(object sender, SessionEventArgs e)
        {
            ProxyConsoleWriteLine(consoleSender, $"Pipelineinfo: {e.GetState().PipelineInfo}");
        }

        private void ProxyConsoleWriteLine(string sender, string message)
        {
            consoleMessageQueue.Enqueue(new Tuple<string, string>(sender, message));
        }

        private async Task ListenToConsole()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                while (consoleMessageQueue.TryDequeue(out var item))
                {
                    var sender = item.Item1;
                    var message = item.Item2;

                    CosmosConsole.WriteLine(sender, message);
                }

                // Reduce CPU usage
                await Task.Delay(50);
            }
        }
    }
}