using BedrockCosmos;
using BedrockCosmos.App;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Http.Responses;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.Examples.Basic
{
    public class ProxyController : IDisposable
    {
        private readonly ProxyServer proxyServer;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentQueue<Tuple<ConsoleColor?, string>> consoleMessageQueue
            = new ConcurrentQueue<Tuple<ConsoleColor?, string>>();

        private ExplicitProxyEndPoint explicitEndPoint;

        string currentPathForResponse = AppDomain.CurrentDomain.BaseDirectory + @"Responses-main\";
        string consoleSender = "Proxy";

        public ProxyController()
        {
            Task.Run(() => ListenToConsole());

            proxyServer = new ProxyServer();

            //proxyServer.EnableHttp2 = true;

            // generate root certificate without storing it in file system
            //proxyServer.CertificateManager.CreateRootCertificate(false);

            //proxyServer.CertificateManager.TrustRootCertificate();
            //proxyServer.CertificateManager.TrustRootCertificateAsAdmin();

            proxyServer.ExceptionFunc = async exception =>
            {
                if (exception is ProxyHttpException phex)
                    CosmosConsole.WriteLine(consoleSender, exception.Message + ": " + phex.InnerException?.Message);
                else
                    CosmosConsole.WriteLine(consoleSender, exception.Message);
            };

            proxyServer.TcpTimeWaitSeconds = 10;
            proxyServer.ConnectionTimeOutSeconds = 15;
            proxyServer.ReuseSocket = false;
            proxyServer.EnableConnectionPool = false;
            proxyServer.ForwardToUpstreamGateway = true;
            proxyServer.CertificateManager.SaveFakeCertificates = true;
            //proxyServer.ProxyBasicAuthenticateFunc = async (args, userName, password) =>
            //{
            //    return true;
            //};

            // this is just to show the functionality, provided implementations use junk value
            //proxyServer.GetCustomUpStreamProxyFunc = onGetCustomUpStreamProxyFunc;
            //proxyServer.CustomUpStreamProxyFailureFunc = onCustomUpStreamProxyFailureFunc;

            // optionally set the Certificate Engine
            // Under Mono or Non-Windows runtimes only BouncyCastle will be supported
            //proxyServer.CertificateManager.CertificateEngine = Network.CertificateEngine.BouncyCastle;

            // optionally set the Root Certificate
            //proxyServer.CertificateManager.RootCertificate = new X509Certificate2("myCert.pfx", string.Empty, X509KeyStorageFlags.Exportable);
        }

        private CancellationToken CancellationToken => cancellationTokenSource.Token;

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
            proxyServer.Dispose();
        }

        public void StartProxy()
        {
            proxyServer.BeforeRequest += OnRequest;
            proxyServer.BeforeResponse += OnResponse;
            proxyServer.AfterResponse += OnAfterResponse;

            proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            //proxyServer.EnableWinAuth = true;

            explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);

            // Fired when a CONNECT request is received
            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse += OnBeforeTunnelConnectResponse;

            // An explicit endpoint is where the client knows about the existence of a proxy
            // So client sends request in a proxy friendly manner
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

            // Transparent endpoint is useful for reverse proxy (client is not aware of the existence of proxy)
            // A transparent endpoint usually requires a network router port forwarding HTTP(S) packets
            // or by DNS to send data to this endPoint.
            //var transparentEndPoint = new TransparentProxyEndPoint(IPAddress.Any, 443, true)
            //{
            //    // Generic Certificate hostname to use
            //    // When SNI is disabled by client
            //    GenericCertificateName = "localhost"
            //};

            //proxyServer.AddEndPoint(transparentEndPoint);
            //proxyServer.UpStreamHttpProxy = new ExternalProxy("localhost", 8888);
            //proxyServer.UpStreamHttpsProxy = new ExternalProxy("localhost", 8888);

            // SOCKS proxy
            //proxyServer.UpStreamHttpProxy = new ExternalProxy("127.0.0.1", 1080)
            //    { ProxyType = ExternalProxyType.Socks5, UserName = "User1", Password = "Pass" };
            //proxyServer.UpStreamHttpsProxy = new ExternalProxy("127.0.0.1", 1080)
            //    { ProxyType = ExternalProxyType.Socks5, UserName = "User1", Password = "Pass" };


            //var socksEndPoint = new SocksProxyEndPoint(IPAddress.Any, 1080, true)
            //{
            //    // Generic Certificate hostname to use
            //    // When SNI is disabled by client
            //    GenericCertificateName = "google.com"
            //};

            //proxyServer.AddEndPoint(socksEndPoint);

            foreach (var endPoint in proxyServer.ProxyEndPoints)
                CosmosConsole.WriteLine(consoleSender, $"Listening on '{endPoint.GetType().Name}' endpoint at Ip {endPoint.IpAddress}" +
                    $" and port: {endPoint.Port}");

            // Only explicit proxies can be set as system proxy!
            //proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            //proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);
            if (RunTime.IsWindows) proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);
        }

        public void Stop()
        {
            explicitEndPoint.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse -= OnBeforeTunnelConnectResponse;

            proxyServer.BeforeRequest -= OnRequest;
            proxyServer.BeforeResponse -= OnResponse;
            proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;

            proxyServer.Stop();

            // remove the generated certificates
            //proxyServer.CertificateManager.RemoveTrustedRootCertificates();
        }

        private async Task<IExternalProxy> OnGetCustomUpStreamProxyFunc(SessionEventArgsBase arg)
        {
            arg.GetState().PipelineInfo.AppendLine(nameof(OnGetCustomUpStreamProxyFunc));

            // this is just to show the functionality, provided values are junk
            return new ExternalProxy
            {
                BypassLocalhost = false,
                HostName = "127.0.0.9",
                Port = 9090,
                Password = "fake",
                UserName = "fake",
                UseDefaultCredentials = false
            };
        }

        private async Task<IExternalProxy> OnCustomUpStreamProxyFailureFunc(SessionEventArgsBase arg)
        {
            arg.GetState().PipelineInfo.AppendLine(nameof(OnCustomUpStreamProxyFailureFunc));

            // this is just to show the functionality, provided values are junk
            return new ExternalProxy
            {
                BypassLocalhost = false,
                HostName = "127.0.0.10",
                Port = 9191,
                Password = "fake2",
                UserName = "fake2",
                UseDefaultCredentials = false
            };
        }

        private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            var hostname = e.HttpClient.Request.RequestUri.Host;
            e.GetState().PipelineInfo.AppendLine(nameof(OnBeforeTunnelConnectRequest) + ":" + hostname);
            CosmosConsole.WriteLine(consoleSender, "Tunnel to: " + hostname);

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
            var color = sent ? ConsoleColor.Green : ConsoleColor.Blue;

            var decoder = sent
                ? args.WebSocketDecoderSend
                : args.WebSocketDecoderReceive;

            foreach (var frame in decoder.Decode(e.Buffer, e.Offset, e.Count))
            {
                if (frame.OpCode == WebsocketOpCode.Binary)
                {
                    var data = frame.Data.ToArray();
                    var str = string.Join(",", data.Select(x => x.ToString("X2")));
                    CosmosConsole.WriteLine(consoleSender, str);
                }
                else if (frame.OpCode == WebsocketOpCode.Text)
                {
                    CosmosConsole.WriteLine(consoleSender, frame.GetText());
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

            var clientLocalIp = e.ClientLocalEndPoint.Address;
            if (!clientLocalIp.Equals(IPAddress.Loopback) && !clientLocalIp.Equals(IPAddress.IPv6Loopback))
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(clientLocalIp, 0);

            string body = await e.GetRequestBodyAsString();
            e.UserData = body;

            CosmosConsole.WriteLine(consoleSender, "Active Client Connections:" + ((ProxyServer)sender).ClientConnectionCount);
            CosmosConsole.WriteLine(consoleSender, e.HttpClient.Request.Url);
        }

        private async Task MultipartRequestPartSent(object sender, MultipartRequestPartSentEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(MultipartRequestPartSent));

            var session = (SessionEventArgs)sender;
            CosmosConsole.WriteLine(consoleSender, "Multipart form data headers:");
            foreach (var header in e.Headers) CosmosConsole.WriteLine(consoleSender, header.ToString());
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnResponse));

            if (e.HttpClient.ConnectRequest?.TunnelType == TunnelType.Websocket)
            {
                e.DataSent += WebSocket_DataSent;
                e.DataReceived += WebSocket_DataReceived;
            }

            CosmosConsole.WriteLine(consoleSender, "Active Server Connections:" + ((ProxyServer)sender).ServerConnectionCount);

            var ext = Path.GetExtension(e.HttpClient.Request.RequestUri.AbsolutePath);

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
                            //CosmosConsole.WriteLine(consoleSender, "Local path is now " + localPath);

                            string jsonContent = JsonParser.ReadJsonFileContent(localPath);
                            e.SetResponseBodyString(jsonContent);
                            CosmosConsole.WriteLine(consoleSender, $"Replaced response for {e.HttpClient.Request.Url}");
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
                            //CosmosConsole.WriteLine(consoleSender, "Local path is now " + localPath);

                            string jsonContent = JsonParser.ReadJsonFileContent(localPath);
                            e.SetResponseBodyString(jsonContent);
                            CosmosConsole.WriteLine(consoleSender, $"Replaced response for {e.HttpClient.Request.Url}");
                        }
                    }
                    else if (currentUri == "https://store.mktpl.minecraft-services.net/api/v1.0/layout/pages/MultiItemPage_StoreRoot")
                    {
                        // Append custom marketplace button to response if accessing the main page
                        string responseBody = await e.GetResponseBodyAsString();
                        string location = "result.rows"; // Works the same as ["result"]["rows"]
                        string appendedJson = JsonParser.AppendJsonToStart(responseBody, localPath, location);
                        e.SetResponseBodyString(appendedJson);
                        CosmosConsole.WriteLine(consoleSender, $"Appended response for {e.HttpClient.Request.Url}");
                    }
                    else if (currentUri == "https://messaging.mktpl.minecraft-services.net/api/v1.0/session/start")
                    {
                        string responseBody = await e.GetResponseBodyAsString();
                        // string location = "result.messages";
                        // string announcementPath = currentPathForResponse + @"News\LoginAnnouncement_append.json";
                        // string newsPath = currentPathForResponse + @"News\CurrentNews_append.json";

                        NewsManager.RetrieveNewsHistory();
                        string newsTabDataPath = AppDomain.CurrentDomain.BaseDirectory + @"news.json";

                        // Append front announcement
                        //string appendedJson = JsonParser.AppendJsonToEnd(responseBody, announcementPath, location);

                        // Append news
                        string location = "result.inboxSummary.categories";
                        string appendedJson = JsonParser.AppendJsonToStart(responseBody, newsTabDataPath, location);

                        e.SetResponseBodyString(appendedJson);
                        CosmosConsole.WriteLine(consoleSender, $"Appended response for {e.HttpClient.Request.Url}");
                    }
                    else
                    {
                        string jsonContent = JsonParser.ReadJsonFileContent(localPath);
                        e.SetResponseBodyString(jsonContent);
                        CosmosConsole.WriteLine(consoleSender, $"Replaced response for {e.HttpClient.Request.Url}");
                    }
                }
                catch (Exception ex)
                {
                    CosmosConsole.WriteLine(consoleSender, $"Error replacing response: {ex.Message}");
                }
            }
        }

        private async Task OnAfterResponse(object sender, SessionEventArgs e)
        {
            CosmosConsole.WriteLine(consoleSender, $"Pipelineinfo: {e.GetState().PipelineInfo}");
        }

        /// <summary>
        ///     Allows overriding default certificate validation logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnCertificateValidation));

            // set IsValid to true/false based on Certificate Errors
            if (e.SslPolicyErrors == SslPolicyErrors.None) e.IsValid = true;

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Allows overriding default client certificate selection logic during mutual authentication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnCertificateSelection));

            // set e.clientCertificate to override

            return Task.CompletedTask;
        }

        // Original Proxy Console
        /*private void WriteToConsole(string message, ConsoleColor? consoleColor = null)
        {
            consoleMessageQueue.Enqueue(new Tuple<ConsoleColor?, string>(consoleColor, message));
        }*/

        private async Task ListenToConsole()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                while (consoleMessageQueue.TryDequeue(out var item))
                {
                    var consoleColor = item.Item1;
                    var message = item.Item2;

                    if (consoleColor.HasValue)
                    {
                        var existing = Console.ForegroundColor;
                        Console.ForegroundColor = consoleColor.Value;
                        CosmosConsole.WriteLine(consoleSender, message);
                        Console.ForegroundColor = existing;
                    }
                    else
                    {
                        CosmosConsole.WriteLine(consoleSender, message);
                    }
                }

                //reduce CPU usage
                await Task.Delay(50);
            }
        }

        ///// <summary>
        ///// User data object as defined by user.
        ///// User data can be set to each SessionEventArgs.HttpClient.UserData property
        ///// </summary>
        //public class CustomUserData
        //{
        //    public HeaderCollection RequestHeaders { get; set; }
        //    public byte[] RequestBody { get; set; }
        //    public string RequestBodyString { get; set; }
        //}
    }
}