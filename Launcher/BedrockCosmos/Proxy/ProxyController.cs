using BedrockCosmos.App;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.StreamExtended.Network;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.Proxy
{
    public class ProxyController : IDisposable
    {
        private readonly ProxyServer proxyServer;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ConcurrentQueue<Tuple<string, string>> consoleMessageQueue
            = new ConcurrentQueue<Tuple<string, string>>();
        private ExplicitProxyEndPoint explicitEndPoint;
        private string consoleSender = "Proxy";

        public ProxyController()
        {
            Task.Run(() => ListenToConsole());

            proxyServer = new ProxyServer();
            proxyServer.CertificateManager.PfxFilePath = Path.Combine(PathDefinitions.CosmosAppData, @"CosmosRootCert.pfx");
            proxyServer.CertificateManager.CertificateStorage = new CertificateStorage(PathDefinitions.CosmosAppData);

            proxyServer.ExceptionFunc = async exception =>
            {
                if (exception is ProxyHttpException)
                    return; // Supresses "errors" for requests that would have originally 404'd before proxy

                if (exception is ProxyHttpException phex)
                    ProxyConsoleWriteLine(consoleSender, $"{exception.GetType().Name}: {exception.Message} - {phex.InnerException?.Message}");
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
            //e.GetState().PipelineInfo.AppendLine(nameof(OnBeforeTunnelConnectRequest) + ":" + hostname);

            var clientLocalIp = e.ClientLocalEndPoint.Address;
            if (!clientLocalIp.Equals(IPAddress.Loopback) && !clientLocalIp.Equals(IPAddress.IPv6Loopback))
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(clientLocalIp, 0);

            bool existsInAllowedUrls = JsonData.AllowedUrls.Any(url => url == hostname);
            if (!existsInAllowedUrls)
            {
                if (SettingsManager.DetailedLogging)
                    ProxyConsoleWriteLine(consoleSender, "Tunnel to: " + hostname);

                e.DecryptSsl = false; // Exempts Non-Allowed Urls from proxy
            }
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
            //e.GetState().PipelineInfo
            //    .AppendLine(nameof(OnBeforeTunnelConnectResponse) + ":" + e.HttpClient.Request.RequestUri);

            return Task.CompletedTask;
        }

        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            //e.GetState().PipelineInfo.AppendLine(nameof(OnRequest) + ":" + e.HttpClient.Request.RequestUri);

            try
            {
                e.UserData = new CustomUserData
                {
                    RequestBodyString = await e.GetRequestBodyAsString(),
                    RequestLogs = $"Processed Request: {e.HttpClient.Request.RequestUri.AbsoluteUri}\n" +
                        $"└── Active Client Connections: {((ProxyServer)sender).ClientConnectionCount}\n" +
                        $"└── On Request: Saved response body.\n"
                };
            }
            catch (Exception)
            {
                e.UserData = new CustomUserData
                {
                    RequestBodyString = string.Empty,
                    RequestLogs = $"Processed Request: {e.HttpClient.Request.RequestUri.AbsoluteUri}\n" +
                        $"└── Active Client Connections: {((ProxyServer)sender).ClientConnectionCount}\n" +
                        $"└── On Request: Response body was empty.\n"
                };
            }

            //ProxyConsoleWriteLine(consoleSender, "Active Client Connections:" + ((ProxyServer)sender).ClientConnectionCount);
        }

        private async Task MultipartRequestPartSent(object sender, MultipartRequestPartSentEventArgs e)
        {
            //e.GetState().PipelineInfo.AppendLine(nameof(MultipartRequestPartSent));

            var session = (SessionEventArgs)sender;
            ProxyConsoleWriteLine(consoleSender, "Multipart form data headers:");
            foreach (var header in e.Headers) ProxyConsoleWriteLine(consoleSender, header.ToString());
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            //e.GetState().PipelineInfo.AppendLine(nameof(OnResponse));

            if (e.HttpClient.ConnectRequest?.TunnelType == TunnelType.Websocket)
            {
                e.DataSent += WebSocket_DataSent;
                e.DataReceived += WebSocket_DataReceived;
            }

            //ProxyConsoleWriteLine(consoleSender, "Active Server Connections:" + ((ProxyServer)sender).ServerConnectionCount);

            Endpoint urlData = JsonData.MainPages.FirstOrDefault(o => o.url == e.HttpClient.Request.RequestUri.AbsoluteUri);
            var userData = e.UserData as CustomUserData;
            string requestBody = userData.RequestBodyString;
            string currentUri = e.HttpClient.Request.RequestUri.AbsoluteUri;

            if (urlData != null)
            {
                try
                {
                    e.HttpClient.Response.StatusCode = 200; // Set to OK, otherwise stays at a not found error
                    string localPath = Path.Combine(PathDefinitions.ResponsesDirectory + urlData.response);
                    localPath = Path.GetFullPath(localPath);

                    switch (currentUri)
                    {
                        case ProxyUrlDefinitions.PlayfabGetPublishedItemUrl:
                            await HandleGetPublishedItemRequest(requestBody, e);
                            break;

                        case ProxyUrlDefinitions.PlayfabSearchUrl:
                            await HandlePlayfabSearchRequest(requestBody, e);
                            break;

                        case ProxyUrlDefinitions.SkinsRootUrl:
                        case ProxyUrlDefinitions.StoreRootUrl:
                        //case ProxyUrlDefinitions.PopCultureUrl:
                        //case ProxyUrlDefinitions.TopSellersUrl:
                        //case ProxyUrlDefinitions.PopularUrl:
                        case ProxyUrlDefinitions.PauseMenuUrl:
                        case ProxyUrlDefinitions.CubeCraftUrl:
                        case ProxyUrlDefinitions.EnchantedUrl:
                        case ProxyUrlDefinitions.GalaxiteUrl:
                        case ProxyUrlDefinitions.LifeboatUrl:
                        case ProxyUrlDefinitions.MegaSmpUrl:
                        case ProxyUrlDefinitions.MinevilleUrl:
                        case ProxyUrlDefinitions.MobMazeUrl:
                        case ProxyUrlDefinitions.SoulSteelUrl:
                        case ProxyUrlDefinitions.TheHiveUrl:
                            await HandleAppendStoreButtonRequest(localPath, e);
                            break;

                        case ProxyUrlDefinitions.SessionStartUrl:
                            await HandleSessionStartRequest(localPath, e);
                            break;

                        case ProxyUrlDefinitions.MessagesEventUrl:
                            await HandleMessagesEventRequest(localPath, e);
                            break;

                        case ProxyUrlDefinitions.PersonaSkinSelectorUrl:
                            await HandlePersonaSkinSelectorRequest(localPath, e);
                            break;

                        default:
                            HandleDefaultRequest(localPath, e);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //ProxyConsoleWriteLine("Parser", $"Error replacing response: {ex.Message}");
                    userData.RequestLogs = userData.RequestLogs + $"└── On Response: Error replacing response - {ex.Message}\n";
                }
            }
            else
            {
                userData.RequestLogs = userData.RequestLogs + $"└── On Response: No corresponding Json file was found. Processed request normally.\n";
            }
        }

        private async Task OnAfterResponse(object sender, SessionEventArgs e)
        {
            //ProxyConsoleWriteLine(consoleSender, $"Pipelineinfo: {e.GetState().PipelineInfo}");
            var userData = e.UserData as CustomUserData;
            ProxyConsoleWriteLine(consoleSender, userData.RequestLogs);
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

        private async Task HandleGetPublishedItemRequest(string requestBody, SessionEventArgs e)
        {
            // Search for UUID if a PlayFab Get Item endpoint and use as response
            var userData = e.UserData as CustomUserData;

            PlayfabGetPublishedItemBody getItemBody = JsonConvert.DeserializeObject<PlayfabGetPublishedItemBody>(requestBody);
            MarketItem mItem = JsonData.MarketItems.FirstOrDefault(o => o.uuid == getItemBody.itemid);
            if (mItem != null)
            {
                userData.RequestLogs = userData.RequestLogs + $"└── Get Item: Found local Json for item {getItemBody.itemid}\n";
                string localPath = Path.Combine(PathDefinitions.ResponsesDirectory + mItem.response);
                localPath = Path.GetFullPath(localPath);
                SetResponseBodyFromFile(localPath, e);
            }
            else
            {
                if (getItemBody.itemid.Length < 1)
                    userData.RequestLogs = userData.RequestLogs + $"└── On Response: No local Json file was found from Playfab Get Item " +
                        $"search of item [NONE]. Processed request normally.\n";
                else
                    userData.RequestLogs = userData.RequestLogs + $"└── On Response: No local Json file was found from Playfab Get Item " +
                        $"search of item {getItemBody.itemid}. Processed request normally.\n";
            }
        }

        private async Task HandlePlayfabSearchRequest(string requestBody, SessionEventArgs e)
        {
            // Search for UUID if a PlayFab Search Item endpoint and use as response
            var userData = e.UserData as CustomUserData;

            PlayfabGetSearchedItemBody getSearchBody = JsonConvert.DeserializeObject<PlayfabGetSearchedItemBody>(requestBody);
            string searchUuid = JsonParser.ExtractPlayfabSearchId(getSearchBody.filter);
            MarketItem mItem = JsonData.PackSearchIds.FirstOrDefault(o => o.uuid == searchUuid);
            if (mItem != null)
            {
                userData.RequestLogs = userData.RequestLogs + $"└── Search: Found local Json for item {searchUuid}\n";
                string localPath = Path.Combine(PathDefinitions.ResponsesDirectory + mItem.response);
                localPath = Path.GetFullPath(localPath);
                SetResponseBodyFromFile(localPath, e);
            }
            else
            {
                if (searchUuid.Length < 1)
                    userData.RequestLogs = userData.RequestLogs + $"└── On Response: No local Json file was found from Playfab Search " +
                        $"of item [NONE]. Processed request normally.\n";
                else
                    userData.RequestLogs = userData.RequestLogs + $"└── On Response: No local Json file was found from Playfab Search " +
                        $"of item {searchUuid}. Processed request normally.\n";
            }
        }

        private async Task HandleAppendStoreButtonRequest(string localPath, SessionEventArgs e)
        {
            // Append custom marketplace button to response if accessing a specified marketplace page
            string responseBody = await e.GetResponseBodyAsString();
            string location = "result.layout"; // Works the same as ["result"]["layout"]
            string appendedJson = JsonParser.AppendJsonToSpecificRows(responseBody, localPath, location, 0);
            e.SetResponseBodyString(appendedJson);
            //CosmosConsole.WriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");

            var userData = e.UserData as CustomUserData;
            userData.RequestLogs = userData.RequestLogs + $"└── On Response: Appended original response using {Path.GetFileName(localPath)}\n";
        }

        private async Task HandleSessionStartRequest(string localPath, SessionEventArgs e)
        {
            string responseBody = await e.GetResponseBodyAsString();
            string announcementPath = PathDefinitions.ResponsesDirectory + @"News\LoginAnnouncement_append.json";
            string newsPath = PathDefinitions.ResponsesDirectory + @"News\CurrentNews_append.json";
            string bannerDataPath = PathDefinitions.CustomJsonsDirectory + @"CurrentLoginAnnouncement.json";
            string newsTabDataPath = PathDefinitions.CustomJsonsDirectory + @"News.json";
            string location = "result.messages";
            string appendedJson = "";

            NewsManager.RetrieveNewsHistory();
            NewsManager.RetrieveCurrentNews();
            NewsManager.QueueLoginAnnouncementIfNew();
            if (NewsManager.IsCurrentNewsNew())
            {
                // Append front announcement
                appendedJson = JsonParser.AppendJsonToEnd(responseBody, bannerDataPath, location);
                NewsManager.AddNewsToHistory();
                NewsManager.MarkCurrentNewsAsSeen();

                // Append news
                location = "result.inboxSummary.categories";
                appendedJson = JsonParser.AppendJsonToStart(appendedJson, newsTabDataPath, location);
            }
            else
            {
                // Only append news
                location = "result.inboxSummary.categories";
                appendedJson = JsonParser.AppendJsonToStart(responseBody, newsTabDataPath, location);
            }

            e.SetResponseBodyString(appendedJson);
            //CosmosConsole.WriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");

            var userData = e.UserData as CustomUserData;
            userData.RequestLogs = userData.RequestLogs + $"└── On Response: Appended original response using {Path.GetFileName(localPath)}\n";
        }

        private async Task HandleMessagesEventRequest(string localPath, SessionEventArgs e)
        {
            var userData = e.UserData as CustomUserData;
            NewsManager.InterpretNewsEvent(userData.RequestBodyString);
        }

        private async Task HandlePersonaSkinSelectorRequest(string localPath, SessionEventArgs e)
        {
            // Append custom pack to skin packs menu
            string responseBody = await e.GetResponseBodyAsString();
            string appendedJson = JsonParser.AppendJsonToSkinPackMenu(responseBody, localPath);
            e.SetResponseBodyString(appendedJson);
            //CosmosConsole.WriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");

            var userData = e.UserData as CustomUserData;
            userData.RequestLogs = userData.RequestLogs + $"└── On Response: Appended original response using {Path.GetFileName(localPath)}\n";
        }

        private void HandleDefaultRequest(string localPath, SessionEventArgs e)
        {
            SetResponseBodyFromFile(localPath, e);
        }

        private void SetResponseBodyFromFile(string localPath, SessionEventArgs e)
        {
            string jsonContent = JsonParser.ReadJsonFileContent(localPath);
            e.SetResponseBodyString(jsonContent);
            //CosmosConsole.WriteLine("Parser", $"Replaced response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");

            var userData = e.UserData as CustomUserData;
            userData.RequestLogs = userData.RequestLogs + $"└── On Response: Replaced original response using {Path.GetFileName(localPath)}\n";
        }

        public class CustomUserData
        {
            public string RequestBodyString { get; set; }
            public string RequestLogs { get; set; }
        }
    }
}