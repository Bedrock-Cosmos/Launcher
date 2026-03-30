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
        public const string DefaultHost = "127.0.0.1";
        public const int DefaultPort = 8000;

        private ProxyServer proxyServer;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ConcurrentQueue<Tuple<string, string>> consoleMessageQueue
            = new ConcurrentQueue<Tuple<string, string>>();
        private ExplicitProxyEndPoint explicitEndPoint;
        private readonly string consoleSender = "Proxy";
        private bool eventsAttached;

        public bool IsRunning { get; private set; }

        public ProxyController()
        {
            Task.Run(() => ListenToConsole());
        }

        private CancellationToken CancellationToken => cancellationTokenSource.Token;

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch
            {

            }

            if (proxyServer != null)
            {
                proxyServer.Dispose();
                proxyServer = null;
            }

            cancellationTokenSource.Dispose();
        }

        public void StartProxy()
        {
            if (IsRunning)
                return;

            proxyServer = CreateProxyServer();
            AttachEvents(proxyServer);

            explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, DefaultPort);

            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse += OnBeforeTunnelConnectResponse;

            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start(false);
            IsRunning = true;

            foreach (var endPoint in proxyServer.ProxyEndPoints)
                ProxyConsoleWriteLine(consoleSender, LanguageHandler.Format("Logs.Proxy.ListeningEndpoint", endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port));
        }

        public void Stop()
        {
            if (!IsRunning && proxyServer == null)
                return;

            if (explicitEndPoint != null)
            {
                explicitEndPoint.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;
                explicitEndPoint.BeforeTunnelConnectResponse -= OnBeforeTunnelConnectResponse;
            }

            if (proxyServer != null)
                DetachEvents(proxyServer);

            if (proxyServer != null)
            {
                if (IsRunning)
                    proxyServer.Stop();

                proxyServer.Dispose();
                proxyServer = null;
            }

            explicitEndPoint = null;
            IsRunning = false;

            // Remove generated certificates
            //proxyServer.CertificateManager.RemoveTrustedRootCertificates();
        }

        private ProxyServer CreateProxyServer()
        {
            var server = new ProxyServer();
            server.CertificateManager.PfxFilePath = Path.Combine(PathDefinitions.CosmosAppData, @"CosmosRootCert.pfx");
            server.CertificateManager.CertificateStorage = new CertificateStorage(PathDefinitions.CosmosAppData);

            server.ExceptionFunc = async exception =>
            {
                if (exception is ProxyHttpException)
                    return; // Supresses "errors" for requests that would have originally 404'd before proxy

                if (exception is ProxyHttpException phex)
                    ProxyConsoleWriteLine(consoleSender, LanguageHandler.Format("Logs.Proxy.ExceptionWithInner", exception.GetType().Name, exception.Message, phex.InnerException?.Message));
                else
                    ProxyConsoleWriteLine(consoleSender, LanguageHandler.Format("Logs.Proxy.Exception", exception.Message));
            };

            server.TcpTimeWaitSeconds = 10;
            server.ConnectionTimeOutSeconds = 15;
            server.ReuseSocket = false;
            server.EnableConnectionPool = false;
            server.ForwardToUpstreamGateway = true;
            server.CertificateManager.SaveFakeCertificates = true;
            return server;
        }

        private void AttachEvents(ProxyServer server)
        {
            if (eventsAttached || server == null)
                return;

            server.BeforeRequest += OnRequest;
            server.BeforeResponse += OnResponse;
            server.AfterResponse += OnAfterResponse;
            eventsAttached = true;
        }

        private void DetachEvents(ProxyServer server)
        {
            if (!eventsAttached || server == null)
                return;

            server.BeforeRequest -= OnRequest;
            server.BeforeResponse -= OnResponse;
            server.AfterResponse -= OnAfterResponse;
            eventsAttached = false;
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
                    ProxyConsoleWriteLine(consoleSender, LanguageHandler.Format("Logs.Proxy.TunnelTo", hostname));

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
                    RequestLogs = LanguageHandler.Format("Logs.Proxy.ProcessedRequest", e.HttpClient.Request.RequestUri.AbsoluteUri) + "\n" +
                        LanguageHandler.Format("Logs.Proxy.ActiveClientConnections", ((ProxyServer)sender).ClientConnectionCount) + "\n" +
                        LanguageHandler.Get("Logs.Proxy.RequestBodySaved") + "\n"
                };
            }
            catch (Exception)
            {
                e.UserData = new CustomUserData
                {
                    RequestBodyString = string.Empty,
                    RequestLogs = LanguageHandler.Format("Logs.Proxy.ProcessedRequest", e.HttpClient.Request.RequestUri.AbsoluteUri) + "\n" +
                        LanguageHandler.Format("Logs.Proxy.ActiveClientConnections", ((ProxyServer)sender).ClientConnectionCount) + "\n" +
                        LanguageHandler.Get("Logs.Proxy.RequestBodyEmpty") + "\n"
                };
            }

            //ProxyConsoleWriteLine(consoleSender, "Active Client Connections:" + ((ProxyServer)sender).ClientConnectionCount);
        }

        private async Task MultipartRequestPartSent(object sender, MultipartRequestPartSentEventArgs e)
        {
            //e.GetState().PipelineInfo.AppendLine(nameof(MultipartRequestPartSent));

            var session = (SessionEventArgs)sender;
            ProxyConsoleWriteLine(consoleSender, LanguageHandler.Get("Logs.Proxy.MultipartHeaders"));
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
                        case ProxyUrlDefinitions.MinevilleUrl:
                        case ProxyUrlDefinitions.MobMazeUrl:
                        case ProxyUrlDefinitions.SoulSteelUrl:
                        case ProxyUrlDefinitions.TheHiveUrl:
                            await HandleAppendStoreButtonRequest(localPath, e);
                            break;

                        case ProxyUrlDefinitions.SessionStartUrl:
                            await HandleSessionStartRequest(localPath, e);
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
                    userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.ResponseReplaceFailed", ex.Message) + "\n";
                }
            }
            else
            {
                userData.RequestLogs = userData.RequestLogs + LanguageHandler.Get("Logs.Proxy.NoMatchingJson") + "\n";
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
                userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.PlayfabGetFound", getItemBody.itemid) + "\n";
                string localPath = Path.Combine(PathDefinitions.ResponsesDirectory + mItem.response);
                localPath = Path.GetFullPath(localPath);
                SetResponseBodyFromFile(localPath, e);
            }
            else
            {
                if (getItemBody.itemid.Length < 1)
                    userData.RequestLogs = userData.RequestLogs + LanguageHandler.Get("Logs.Proxy.PlayfabGetNotFoundEmpty") + "\n";
                else
                    userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.PlayfabGetNotFound", getItemBody.itemid) + "\n";
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
                userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.PlayfabSearchFound", searchUuid) + "\n";
                string localPath = Path.Combine(PathDefinitions.ResponsesDirectory + mItem.response);
                localPath = Path.GetFullPath(localPath);
                SetResponseBodyFromFile(localPath, e);
            }
            else
            {
                if (searchUuid.Length < 1)
                    userData.RequestLogs = userData.RequestLogs + LanguageHandler.Get("Logs.Proxy.PlayfabSearchNotFoundEmpty") + "\n";
                else
                    userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.PlayfabSearchNotFound", searchUuid) + "\n";
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
            userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.AppendedResponse", Path.GetFileName(localPath)) + "\n";
        }

        private async Task HandleSessionStartRequest(string localPath, SessionEventArgs e)
        {
            string responseBody = await e.GetResponseBodyAsString();
            // string location = "result.messages";
            // string announcementPath = PathDefinitions.ResponsesDirectory + @"News\LoginAnnouncement_append.json";
            // string newsPath = PathDefinitions.ResponsesDirectory + @"News\CurrentNews_append.json";

            NewsManager.RetrieveNewsHistory();
            string newsTabDataPath = PathDefinitions.CustomJsonsDirectory + @"news.json";

            // Append front announcement
            //string appendedJson = JsonParser.AppendJsonToEnd(responseBody, announcementPath, location);

            // Append news
            string location = "result.inboxSummary.categories";
            string appendedJson = JsonParser.AppendJsonToStart(responseBody, newsTabDataPath, location);

            e.SetResponseBodyString(appendedJson);
            //CosmosConsole.WriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");

            var userData = e.UserData as CustomUserData;
            userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.AppendedResponse", Path.GetFileName(localPath)) + "\n";
        }

        private async Task HandlePersonaSkinSelectorRequest(string localPath, SessionEventArgs e)
        {
            // Append custom pack to skin packs menu
            string responseBody = await e.GetResponseBodyAsString();
            string appendedJson = JsonParser.AppendJsonToSkinPackMenu(responseBody, localPath);
            e.SetResponseBodyString(appendedJson);
            //CosmosConsole.WriteLine("Parser", $"Appended response for {e.HttpClient.Request.Url} using {Path.GetFileName(localPath)}");

            var userData = e.UserData as CustomUserData;
            userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.AppendedResponse", Path.GetFileName(localPath)) + "\n";
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
            userData.RequestLogs = userData.RequestLogs + LanguageHandler.Format("Logs.Proxy.ReplacedResponse", Path.GetFileName(localPath)) + "\n";
        }

        public class CustomUserData
        {
            public string RequestBodyString { get; set; }
            public string RequestLogs { get; set; }
        }
    }
}
