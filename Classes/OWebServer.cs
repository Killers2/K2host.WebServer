/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Concurrent;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Security.Cryptography;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;

using K2host.Core;
using K2host.Core.Classes;
using K2host.Core.Delegates;
using K2host.Sockets.Tcp;
using K2host.Sockets.Delegates;
using K2host.Threading.Classes;
using K2host.WebServer.Enums;
using K2host.Certificates.Classes;
using K2host.Threading.Extentions;
using K2host.Threading.Interface;
using K2host.Certificates.Interfaces;
using K2host.Certificates.Extentions;

using gw = K2host.WebServer.OHelpers;
using gl = K2host.Core.OHelpers;
using K2host.WebServer.Delegates;

namespace K2host.WebServer.Classes
{


    /// <summary>
    /// This WebServer help host and serve applications running .net or / and html
    /// Supports C# and VB.net code files.
    /// Routing virtual paths
    /// CORS Security
    /// HSTS Security
    /// Multi SSL/TLS certs on one port (SNI)
    /// Multi cert sites
    /// Session based or Singleton based application instances
    /// The code base in relies on the HtmlAgilityPack (https://html-agility-pack.net/)
    /// </summary>
    public class OWebServer : IDisposable
    {

        #region Vars

        /// <summary>
        /// Used interally to help with running a thread.
        /// </summary>
        bool ServerRunning = false;

        #endregion

        #region Delegates

        /// <summary>
        /// This call back triggers before the response is sent back down the client connection / stream
        /// passes a byte[] array of the request data
        /// </summary>
        public OnBeforeHttpResponceSentEvent OnBeforeHttpResponceSent;

        /// <summary>
        /// This call back triggers after the response is sent back down the client connection / stream
        /// passes a OTCPWebResponse object
        /// </summary>
        public OnAfterHttpResponceSentEvent OnAfterHttpResponceSent;

        /// <summary>
        /// This call back triggers as a new session is created on client connections.
        /// passes an OTCPWebApplication instance
        /// </summary>
        public OnSessionCreatedEvent OnSessionCreated;

        /// <summary>
        /// This call back triggers as a session has timed out via the TTL (SessionsTimeToLive).
        /// passes an OTCPWebApplication instance before its disposed.
        /// </summary>
        public OnSessionUnloadedEvent OnSessionUnloaded;

        /// <summary>
        /// This call back triggers before the request is processed by an application hosted.
        /// passes an OTCPWebRequest object
        /// </summary>
        public OnBeforeHttpRequestProcessedEvent OnBeforeHttpRequestProcessed;

        /// <summary>
        /// This call back triggers after the request is processed by an application hosted.
        /// passes an OTCPWebRequest object
        /// </summary>
        public OnAfterHttpRequestProcessedEvent OnAfterHttpRequestProcessed;

        /// <summary>
        /// Used to pass back an exception that was sent via callbacks interanally.
        /// </summary>
        public OnWebServerErrorEvent OnWebServerError;

        #endregion

        #region Properties

        /// <summary>
        /// This is the name of the web server service instance which will also be placed in all responce headers.
        /// </summary>
        public string ServerApplicationName { get; }

        /// <summary>
        /// This is the timeout to which a session will expire when there is no activity.
        /// </summary>
        public long SessionsTimeToLive { get; set; } // Seconds

        /// <summary>
        /// The thread manager instance added when this application is created.
        /// </summary>
        public IThreadManager ThreadManager { get; }

        /// <summary>
        /// This list hold the list of EndPoints for listeners
        /// </summary>
        public Dictionary<IPEndPoint, OHostType> EndPoints { get; set; }

        /// <summary>
        /// The list of tcp server base that will be doing the listening to incomming requests.
        /// </summary>
        public List<OServer> Listeners { get; set; }

        /// <summary>
        /// The list of loaded applications on this server.
        /// </summary>
        public ODictionary<Guid, List<string>, OWebApplication> Applications { get; }

        /// <summary>
        /// This allows the applications running on this server to allow all and any cors requests.
        /// Will allow any cross domain access.
        /// </summary>
        public bool ServerCorsSecurityOverride { get; set; }

        /// <summary>
        /// This holds the list of html content as pages along with the status number.
        /// </summary>
        public Dictionary<int, string> StatusHTMLPages { get; }

        /// <summary>
        /// Place holders content for the html listed in the <see cref="StatusHTMLPages"/>
        /// </summary>
        public Dictionary<int, Dictionary<string, string>> StatusHTMLPagePlaceHolders { get; }

        /// <summary>
        /// The list of application instances that are loaded.
        /// </summary>
        public ConcurrentDictionary<string, OWebApplication> Sessions { get; set; }

        /// <summary>
        /// The server root directory of where the applications are held.
        /// </summary>
        public string RootDirectory { get; set; }

        #endregion

        #region Constuctor

        /// <summary>
        /// Creates the instance of the server.
        /// </summary>
        /// <param name="e"></param>
        public OWebServer(IThreadManager e)
        {
            ThreadManager               = e;
            Listeners                   = new List<OServer>();
            EndPoints                   = new Dictionary<IPEndPoint, OHostType>();
            Applications                = new ODictionary<Guid, List<string>, OWebApplication>();
            StatusHTMLPages             = new Dictionary<int, string>();
            StatusHTMLPagePlaceHolders  = new Dictionary<int, Dictionary<string, string>>();
            Sessions                    = new ConcurrentDictionary<string, OWebApplication>();
            SessionsTimeToLive          = 1200;
        }

        /// <summary>
        /// Creates the instance of the server with the server service name.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="serverApplicationName"></param>
        public OWebServer(IThreadManager e, string serverApplicationName)
            : this(e)
        {
            ServerApplicationName = serverApplicationName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new Listener and Endpoint to this instance.
        /// </summary>
        /// <param name="e"></param>
        public void AddListener(KeyValuePair<IPEndPoint, OHostType> e)
        {

            if (EndPoints.Keys.Where(w => w.ToString() == e.ToString()).ToArray().Length > 0)
                throw new Exception("There is already a Listener of: " + e.ToString());

            Listeners.Add(new OServer(ThreadManager, e.Key.Address.ToString(), e.Key.Port)
            {
                StatusUpdate            = new OStatusUpdateEventHandler(ListenerStatusUpdate),
                DataReceived            = new ODataReceivedEventHandler(ListenerDataReceived),
                ClientConnected         = new OClientConnectedEventHandler(ListenerClientConnected),
                ClientDisConnected      = new OClientDisConnectedEventHandler(ListenerClientDisConnected),
                OnGetCertificateFromSNI = new OnGetCertificateFromSNIEvent(ListenerGetCertificate),
                DataSent                = new ODataSentEventHandler(ListenerDataSent),
                OnError                 = new OnErrorEventHandler(ListenerServerError),
                IsSecure                = e.Value == OHostType.HTTPS
            });

            EndPoints.Add(e.Key, e.Value);

        }

        /// <summary>
        /// Adds a list of new Listeners and Endpoints to this instance.
        /// </summary>
        /// <param name="e"></param>
        public void AddListeners(KeyValuePair<IPEndPoint, OHostType>[] e)
        {

            foreach (KeyValuePair<IPEndPoint, OHostType> p in e)
                try { AddListener(p); } catch { }

        }

        /// <summary>
        /// Adds a loaded instance of an <see cref="OWebApplication"/> to the server.
        /// </summary>
        /// <param name="e"></param>
        public void AddApplication(OWebApplication e)
        {

            foreach (List<string> hostNames in Applications.Keys)
                foreach (string HostName in e.HostNames)
                    if (hostNames.Contains(HostName))
                        throw new Exception("This host name is already in use on another application.");

            //If non specified then add all from the main server.
            if (e.Bound.Count <= 0)
                e.Bound.AddRange(EndPoints.Keys.ToArray());

            e.Parent = this;
            Applications.Add(e.Ident, e.HostNames, e);
        }

        /// <summary>
        /// Returns a <see cref="OWebApplication"/> based on the hostname given amd / or the bound endpoint.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public OWebApplication GetApplication(string hostName, IPEndPoint bound = null)
        {
            OWebApplication app = null;

            IEnumerable<List<string>> hostNames = Applications.Keys.Where(w => w.Contains(hostName));

            if (hostNames.Any())
                if (Applications.Keys.Contains(hostNames.First()))
                    app = Applications[hostNames.First()];

            if (app!= null)
                if (bound != null && !app.Bound.Where(w => w.ToString() == bound.ToString()).Any())
                    return null;

            return app;
        }

        /// <summary>
        /// Returns a <see cref="OWebApplication"/> based on the applicationId <see cref="Guid"/>.
        /// </summary>
        /// <param name="Ident"></param>
        /// <returns></returns>
        public OWebApplication GetApplication(Guid Ident, IPEndPoint bound = null)
        {
            OWebApplication app = null;

            if (Applications.Indexes.Contains(Ident))
                app = Applications[Ident];

            if (bound != null && !app.Bound.Where(w => w.ToString() == bound.ToString()).Any())
                return null;

            return app;
        }

        /// <summary>
        /// Removes and disposes a <see cref="OWebApplication"/>.
        /// </summary>
        /// <param name="e"></param>
        public void RemoveApplication(OWebApplication e)
        {

            if (Applications.Values.Contains(e))
                Applications.Remove(e);

            if (e != null)
                e.PageLoad = null;

            e?.Dispose();

        }

        /// <summary>
        /// Returns a <see cref="OWebApplication"/> based on the session key.
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public OWebApplication GetApplicationSession(string sessionKey)
        {
            OWebApplication ret = null;

            if (Sessions.Keys.Contains(sessionKey))
                ret = Sessions[sessionKey];

            return ret;
        }

        /// <summary>
        /// Removes and disposes a <see cref="OTCPWebApplication"/> based on the session key.
        /// </summary>
        /// <param name="sessionKey"></param>
        public void RemoveApplicationSession(string sessionKey)
        {

            if (!Sessions.Keys.Contains(sessionKey))
                return;

            Sessions.TryRemove(sessionKey, out OWebApplication e);

            e.PageLoad = null;
            e.Dispose();

        }

        /// <summary>
        /// Start all the listeners added to this server service instance.
        /// </summary>
        public void Start()
        {

            foreach (OServer s in Listeners)
                s.StartServer();

            ServerRunning = true;

            ThreadManager.Add(
                new OThread(
                    new ThreadStart(
                        ApplicationSessionTimer
                    )
                )
            ).Start();

        }

        /// <summary>
        /// Stops all the listeners added to this server service instance.
        /// Removes all <see cref="OWebApplication"/> sesssions loaded.
        /// </summary>
        public void Stop()
        {

            foreach (OServer s in Listeners)
                try { s.StopServer(); } catch { }

            ServerRunning = false;

            IList<OWebApplication> a = Sessions.Values.ToList();

            foreach (OWebApplication b in a)
            {
                string s = Sessions.FindKeyByValue(b);
                if (Sessions.TryRemove(s, out OWebApplication c))
                {
                    OnSessionUnloaded?.Invoke(c);
                    c.Dispose();
                }
            }

        }

        #endregion

        #region Event Callbacks

        /// <summary>
        /// Event call back for the listeners status update.
        /// </summary>
        /// <param name="status"></param>
        public static void ListenerStatusUpdate(string status)
        {

        }

        /// <summary>
        /// Event call back for the listeners after data sent back to the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">the stream attached to the clients connection</param>
        public static void ListenerDataSent(OConnection sender, Stream e)
        {
            e.Flush();
        }

        /// <summary>
        /// Event call back for the listeners after data is received by the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public void ListenerDataReceived(OConnection sender, byte[] data)
        {

            OnBeforeHttpResponceSent?.Invoke(data);

            OWebRequest r = Process(sender, data);

            OnAfterHttpResponceSent?.Invoke(r);

            sender.SendData(r.Response.GetData(), false);

            // Close the Stream if its a resouse file
            if (r.Type == OHttpRequestType.HttpStaticRequest) { }

            //Force close the client if they are supposed to close it remotly
            if (r.Response.Connection == OResponseConnection.Close)
                sender.CloseClientConnection();

            sender.ClientStream.Close();

        }

        /// <summary>
        /// Event call back for the listeners detects a client connected.
        /// </summary>
        /// <param name="sender"></param>
        public static void ListenerClientConnected(OConnection sender)
        {

        }

        /// <summary>
        /// Event call back for the listeners detects a client disconnected.
        /// </summary>
        /// <param name="sender"></param>
        public static void ListenerClientDisConnected(OConnection sender)
        {

        }

        /// <summary>
        /// Event call back for the listeners TLS/SSL handshake.
        /// With server name indication we can get the <see cref="X509Certificate2"/> and passbask to the connection.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public X509Certificate2 ListenerGetCertificate(OServerSNI e)
        {
            X509Certificate2 Certificate = null;
            OWebApplication Application = GetApplication(e.ServerNameIndication);

            if (Application != null)
                Certificate = Application.GetAssignedCertificate(e);

            return Certificate;
        }
       
        /// <summary>
        /// Used to log errors from a callback rather than a thrown one.
        /// </summary>
        /// <param name="e"></param>
        public void ListenerServerError(Exception e)
        {
            OnWebServerError?.Invoke(e);
        }

        /// <summary>
        /// This is run under a thread in this instance, to check and unload TTL applcations.
        /// </summary>
        private void ApplicationSessionTimer()
        {

            while (ServerRunning)
            {

                try
                {
                    UnloadUnsedSessions();
                }
                catch { }

                Thread.Sleep(((int)SessionsTimeToLive * 1000)); // TTL Timout
            }

        }

        #endregion

        #region Processing

        /// <summary>
        /// Processes the data sent by the client connection and returns a <see cref="OWebRequest"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private OWebRequest Process(OConnection sender, byte[] data)
        {

            bool validated = GetRequest(sender, data, out OWebRequest Request);

            //Start the responseBuilder headers
            Request.Response.Headers.Add("Date", string.Format("{0:r}", DateTime.Now));
            Request.Response.Headers.Add("Server", ServerApplicationName + " Application Server");
            Request.Response.Connection = OResponseConnection.Close;

            // Set the socket on the client connection to KeepAlive if requested.
            if (Request.Connection == OResponseConnection.KeepAlive)
            {
                Request.Response.Connection = OResponseConnection.KeepAlive;
                sender.Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }

            if (!validated)
                return GetDefaultHtmlContent(500, "Server Error", Request, "The HTTP Request was invalid.");

            // check to see if the app exsits on the end point the user is comming in on
            OWebApplication WebApplication = GetApplication(Request.Host, (IPEndPoint)sender.Client.Client.LocalEndPoint);

            //make sure the Session is avaliable
            if (WebApplication == null)
                return GetDefaultHtmlContent(500, "Server Error", Request, "There is no web application running with this host name.");

            //Process the response and request in the application
            if (WebApplication.AllowSessions)
            {
                string SessionKey = ((IPEndPoint)sender.Client.Client.RemoteEndPoint).Address.ToString() + "@" + Request.UserAgent;
                if (Sessions.ContainsKey(SessionKey))
                {
                    Request.Application = Sessions[SessionKey];
                    Request.ApplicationId = Request.Application.Ident;
                }
                else
                {
                    Request.Application = WebApplication.Clone();
                    Request.ApplicationId = Request.Application.Ident;
                    Sessions.TryAdd(SessionKey, Request.Application);
                    OnSessionCreated?.Invoke(Request.Application);
                }
            }
            else
            {
                Request.Application = WebApplication;
                Request.ApplicationId = WebApplication.Ident;
            }

            // if the Cors has validated already the make sure these are in the header of each response
            if (Request.Type != OHttpRequestType.PreFlight && !string.IsNullOrEmpty(Request.Origin))
            {
                Request.Response.Headers.Add("Access-Control-Allow-Origin", Request.Origin);
                Request.Response.Headers.Add("Vary: Origin", "*");
            }

            //If the application will not allow https
            if (Request.Url.Scheme == OHostType.HTTPS.ToString().ToLower() && !Request.Application.AllowHttps)
                return GetDefaultHtmlContent(500, "Server Error", Request, "This web application does not allow HTTPS connections.");

            //We make this request a redirect if hsts is enabled and the request http
            if (Request.Url.Scheme == OHostType.HTTP.ToString().ToLower() && Request.Application.AllowHSTS)
            {
                Request.Response.StatusCode = 301;
                Request.Response.StatusDescription = "Moved Permanently";
                Request.Response.Headers.Add("Location", "https://" + Request.Url.Host);
                Request.Response.ContentLength64 = 0;
                return Request;
            }

            //if CORS PreFlight request, this will auth the requests to the borwser, we can control this.
            if (Request.Type == OHttpRequestType.PreFlight)
            {
                Request.Response.StatusCode = 204;
                Request.Response.StatusDescription = "No Content";
                if (ServerCorsSecurityOverride)
                {
                    Request.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                    Request.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, DELETE, OPTIONS, PUT, PATCH");
                    Request.Response.Headers.Add("Access-Control-Allow-Origin", Request.Origin);
                    Request.Response.Headers.Add("Access-Control-Max-Age", "300");
                }
                else if (Request.Application.AllowCors)
                {
                    Request.Response.Headers.Add("Access-Control-Allow-Headers", Request.Application.CorsAllowHeaders.JoinWith(","));
                    Request.Response.Headers.Add("Access-Control-Allow-Methods", Request.Application.CorsAllowMethods.JoinWith(","));

                    if (Request.Application.CorsAllowOrigin.Contains("*"))
                        Request.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    else
                    {
                        if (Request.Application.CorsAllowOrigin.Where(w => w.Contains(Request.Url.Host)).Any())
                            Request.Response.Headers.Add("Access-Control-Allow-Origin", Request.Url.Host);
                    }

                    Request.Response.Headers.Add("Access-Control-Max-Age", Request.Application.CorsMaxAgeSeconds.ToString());
                }
                Request.Response.Headers.Add("Vary", "Origin");
                Request.Response.ContentLength64 = 0;
                return Request;
            }

            //https will need the hsts headers if allow hsts has tobe after the cors security
            if (Request.Url.Scheme == OHostType.HTTPS.ToString().ToLower() && Request.Application.AllowHSTS)
            {

                string STSString = string.Empty;

                if (Request.Application.HSTSIncludeSubDomains)
                    STSString += "; includeSubDomains";

                if (Request.Application.HSTSPreLoad && Request.Application.HSTSIncludeSubDomains && Request.Application.HSTSMaxAge >= 31536000)
                    STSString += "; preload";

                Request.Response.Headers.Add("Strict-Transport-Security", "max-age=" + Request.Application.HSTSMaxAge + STSString);

                foreach (string key in Request.Application.HSTSCPSHeaders.Keys)
                    if (!string.IsNullOrEmpty(Request.Application.HSTSCPSHeaders[key]))
                        Request.Response.Headers.Add(key, Request.Application.HSTSCPSHeaders[key]);

            }

            //if content compressed gzip, decompress body
            if (Request.IsGZIPCommpressed())
                using (var a = new MemoryStream(Request.Body))
                using (var b = new GZipStream(a, CompressionMode.Decompress, false))
                {
                    Array.Clear(Request.Body, 0, Request.Body.Length);
                    Request.Body = new byte[b.Length];
                    b.Read(Request.Body, 0, Request.Body.Length);
                }

            //At this point the default will be a 200 OK response unless the application changes it.
            Request.Response.StatusCode = 200;
            Request.Response.StatusDescription = "OK";

            Request.Application.BeforeHttpRequestProcessed?.Invoke(Request);

            OnBeforeHttpRequestProcessed?.Invoke(Request);

            if (Request.Type == OHttpRequestType.HttpStaticRequest || Request.Type == OHttpRequestType.HttpPage)
            {

                string resourceFilePath = GetResoucePathFile(Request);

                //If the file does not exist then throw a 404 error
                if (!File.Exists(resourceFilePath) && !Request.Application.VirturalService)
                    GetDefaultHtmlContent(404, "Page Not Found", Request, "The file was not found " + resourceFilePath + ".");
                else
                {
                    // Now lets get the file / script etc..
                    byte[] outputData = Array.Empty<byte>();

                    Request.Response.IsCompressed = Request.IsGZIPSupported();
                    Request.Response.ContentType = gw.GetResourceMime(new FileInfo(resourceFilePath));

                    if (Request.Type == OHttpRequestType.HttpStaticRequest)
                    {
                        Request.Response.Headers.Add("Cache-Control", "max-age=300, public");
                        outputData = gl.GetFile(resourceFilePath);
                    }

                    if (Request.Type == OHttpRequestType.HttpPage)
                        outputData = ResourceBuilder(Request, resourceFilePath);

                    Request.Response.OutputStream.Write(outputData, 0, outputData.Length);

                }
            }

            Request.Application.AfterHttpRequestProcessed?.Invoke(Request);

            OnAfterHttpRequestProcessed?.Invoke(Request);

            return Request;

        }

        /// <summary>
        /// Gets the file path of the resource requested.
        /// </summary>
        /// <param name="e">The instance of <see cref="OWebRequest>"/></param>
        /// <returns></returns>
        private static string GetResoucePathFile(OWebRequest e)
        {

            string resource = e.Application.MatchRouteUrl(e);

            if (string.IsNullOrEmpty(resource))
                resource = e.Path;

            //This is indecates the root domain folder (/) lets add the physical path.
            if (resource == "/")
            {
                resource += e.Application.DefaultWebPage;
                e.Path = resource;
            }

            //This is for a path that does not have the slash (/somthing)
            if (resource[0] != '/')
                resource = "/" + resource;

            //This grabs the path / file name location
            string resourceFilePath = e.Application.ApplicationRootPath.FullName + resource;

            //Does this path have the . for a file ext? if not add the default folder file.
            if (!resourceFilePath.Contains("."))
                resourceFilePath += "/" + e.Application.DefaultWebPage;

            //Lets add the accuall page file at the end of the path to the end of the path list.
            e.Paths.Add(resourceFilePath.Remove(0, resourceFilePath.LastIndexOf("/") + 1));

            //Return the full file path.
            return resourceFilePath;

        }

        /// <summary>
        /// Build the data up read to send back using the <see cref="OWebRequest"/> and the file resource requested.
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="ResourceFilePath"></param>
        /// <returns></returns>
        private static byte[] ResourceBuilder(OWebRequest Request, string ResourceFilePath)
        {

            byte[] output = Array.Empty<byte>();

            if (Request.Application.Process(Request) == OPageType.None && !Request.Application.VirturalService)
                output = gl.GetFile(ResourceFilePath);

            return output;

        }

        /// <summary>
        /// The parser will generate a <see cref="OWebRequest"/> and pass a boolean to validate the request.
        /// Perforance: building the request from the client data needs speeding up ?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The data received by the server.</param>
        /// <param name="req">A new instance of <see cref="OWebRequest"/></param>
        /// <returns></returns>
        private static bool GetRequest(OConnection sender, byte[] e, out OWebRequest req)
        {

            OHttpRequestType request = OHttpRequestType.HttpPage;

            req = new OWebRequest(e)
            {
                Method = OHttpMethodType.NONE
            };

            try
            {

                //Build Header Values
                string[] lines = req.RequestData.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                for (int j = 0; j < lines.Length; j++)
                {
                    if (j == 0) // First line is always type and path.
                    {
                        string[] parts      = lines[j].Split(new string[] { " " }, StringSplitOptions.None);
                        req.Method          = gw.GetMethodType(parts[0]);
                        req.CompletePath    = parts[1];
                        req.Protocol        = parts[2];

                        if (!string.IsNullOrEmpty(parts[1]))
                        {
                            string[] resourcePaths = parts[1].Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                            if (resourcePaths.Length > 0)
                                request = gw.IsStaticResource(resourcePaths[^1]) ? OHttpRequestType.HttpStaticRequest : OHttpRequestType.HttpPage;
                        }
                        else
                            throw new InvalidOperationException("Invalid Request : " + req.RequestData);

                        string[] subPaths   = parts[1].Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                        req.Path            = subPaths[0];
                        req.Paths           = subPaths[0].Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    else
                    {
                        if (lines[j] == string.Empty)
                            break;

                        if (lines[j].Contains(": "))
                        {
                            string[] pairs = lines[j].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                req.AddHeader(pairs[0], pairs[1]);
                            }
                            catch
                            {
                                req.AddHeader(pairs[0], string.Empty);
                            }

                            switch (pairs[0].ToLower())
                            {
                                case "referer":                         req.Referer                     = pairs[1]; break;
                                case "access-control-request-method":   req.AccessControlRequestMethod  = pairs[1]; break;
                                case "access-control-request-headers":  req.AccessControlRequestHeaders = pairs[1]; break;
                                case "origin":                          req.Origin                      = pairs[1]; break;
                                case "sec-fetch-mode":                  req.SecFetchMode                = pairs[1]; break;
                                case "via":                             req.Via                         = pairs[1]; break;
                                case "host":                            req.Host                        = pairs[1]; break;
                                case "content-type":                    req.ContentType                 = pairs[1]; break;
                                case "accept":                          req.Accept                      = pairs[1]; break;
                                case "connection":                      req.Connection                  = pairs[1].ToLower() == "close" ? OResponseConnection.Close : OResponseConnection.KeepAlive; break;
                                case "user-agent":                      req.UserAgent                   = pairs[1]; break;
                                case "content-length":                  req.ContentLength               = Convert.ToInt32(pairs[1]); break;
                                default: break;
                            }

                        }

                    }
                }

                //At this point if there is no method on the request assum this is a join from a prev request
                if (req.Method == OHttpMethodType.NONE)
                    throw new InvalidOperationException("Invalid Request");

                //Build QueryString Values                
                foreach (string p in gw.RemoveToken(req.CompletePath).Fracture("&"))
                {
                    string[] query = p.Fracture("=");
                    if (query.Length == 2)
                        try { req.Parms.Add(query[0], query[1]); } catch { }
                }

                //Check for Pre Flight Request for Cors Security from source
                if (!string.IsNullOrEmpty(req.SecFetchMode) && req.Method == OHttpMethodType.OPTIONS)
                    request = OHttpRequestType.PreFlight;

                //Create the url requested
                string prot = "http" + (sender.IsSecureSocket ? "s" : string.Empty);
                req.Url = new Uri(prot + "://" + req.Host);

                //Remove port from the hostname www.tonyspc.com:446
                if (req.Host.Contains(":"))
                    req.Host = req.Host.Substring(0, req.Host.LastIndexOf(":"));

                //Lets create a loop for X amount of time to try and garentee the data in the stream.
                int contentcurtrys = 0;
                int contentmaxtrys = 6;
                bool keeptrying = true;

                while (keeptrying)
                {

                    //Get the rest of data from the stream if required
                    if (req.ContentLength > req.Body.Length)
                    {

                        // Set the body data to the content length
                        int a = 0; byte[] b = new byte[sender.Client.ReceiveBufferSize]; MemoryStream c = new();

                        if (sender.Client.GetStream().DataAvailable)
                            do
                            {
                                // download the data from the stream as the data length.
                                a = sender.ClientStream.Read(b, 0, b.Length);
                                c.Write(b, 0, a);
                                // this can be increased to slow it down to catch the segments of reads.
                                if (!sender.Client.GetStream().DataAvailable)
                                    Thread.Sleep(5);
                            } while (sender.Client.GetStream().DataAvailable);

                        req.Body = gl.CombineByteArrays(req.Body, c.ToArray());
                        c.SetLength(0);
                        c.Dispose();
                        c = null;

                    }

                    //If we hit the max trys and we still don't have it. then finish.
                    if (req.ContentLength != req.Body.Length && contentcurtrys >= contentmaxtrys)
                        keeptrying = false;

                    //If we have the data then continue
                    if (req.ContentLength == req.Body.Length)
                        keeptrying = false;

                    contentcurtrys++;

                    if (keeptrying)
                        Thread.Sleep(sender.SleepTimeCheck);

                }

                //This means the data did not come properly
                if (req.ContentLength != req.Body.Length)
                    throw new InvalidOperationException("Invalid Request");

                //If the body is of content type x-www-form-urlencoded
                if (req.ContentType.Contains("application/x-www-form-urlencoded"))
                    foreach (string p in Encoding.UTF8.GetString(req.Body).Fracture("&"))
                    {
                        string[] query = p.Fracture("=");
                        if (query.Length == 2)
                            try { req.Parms.Add(query[0], query[1]); } catch { }
                    }

                //Support multipart/form-data parsing
                if (req.ContentType.Contains("multipart/form-data"))
                {

                    //The boundary setup
                    string  boundaryString  = req.ContentType.Fracture(";")[1].Fracture("=")[1];

                    //The boundary setup
                    byte[]  boundaryMarker  = Encoding.UTF8.GetBytes("\r\n" + "--" + boundaryString + "\r\n");
                    byte[]  content         = gl.CombineByteArrays(Encoding.UTF8.GetBytes("\r\n"), req.Body);

                    //Split the bounds by the bound marker.
                    byte[][] boundariesFound = content.Split(boundaryMarker);

                    //Create the boundary segments to the request list.
                    foreach (byte[] found in boundariesFound)
                        try
                        {
                            //Create the item and parse in each item.
                            var boundary = OWebRequest.Boundray.Parse(boundaryString, found);
                            //Add the boundary to the request.
                            req.Boundries.Add(boundary.Name, boundary);
                        }
                        catch (Exception) { }

                }



            }
            catch (Exception)
            {
                return false;
            }

            req.Type = request;

            return true;

        }

        /// <summary>
        /// This is used to get the default html content based on status codes, like a 404 or 500 error etc..
        /// </summary>
        /// <param name="StatusCode"></param>
        /// <param name="StatusDescription"></param>
        /// <param name="e"></param>
        /// <param name="Message"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public OWebRequest GetDefaultHtmlContent(int StatusCode, string StatusDescription, OWebRequest e, string Message, Dictionary<string, string> content = null)
        {

            e.Response.StatusCode = StatusCode;
            e.Response.StatusDescription = StatusDescription;

            if (StatusHTMLPages.ContainsKey(StatusCode))
            {

                string templateHtml = StatusHTMLPages[StatusCode];

                if (content != null)
                    foreach (string key in content.Keys)
                        templateHtml = templateHtml.Replace(key, content[key]);

                if (content == null && StatusHTMLPagePlaceHolders.ContainsKey(StatusCode))
                {
                    content = StatusHTMLPagePlaceHolders[StatusCode];
                    foreach (string key in content.Keys)
                        templateHtml = templateHtml.Replace(key, content[key]);
                }

                if (templateHtml.Contains("<%Exception%>"))
                    templateHtml = templateHtml.Replace("<%Exception%>", Message);

                byte[] b = Encoding.UTF8.GetBytes(templateHtml);

                e.Response.ContentLength64 = b.Length;

                e.Response.OutputStream.Write(b, 0, b.Length);

            }

            return e;

        }

        /// <summary>
        /// This is the session unloaded based on the TTL and activity.
        /// </summary>
        private void UnloadUnsedSessions()
        {

            IList<OWebApplication> a = Sessions.Values.ToList();

            foreach (OWebApplication b in a)
                if ((DateTime.Now - b.LastActivity).TotalSeconds > SessionsTimeToLive)
                {
                    string s = Sessions.FindKeyByValue(b);
                    if (Sessions.TryRemove(s, out OWebApplication c))
                    {
                        OnSessionUnloaded?.Invoke(c);
                        c.Dispose();
                    }
                }

        }

        /// <summary>
        /// Gets a <see cref="X509Certificate2"/> from the computers installed certificates.
        /// If not found it will create a tempory one using the <see cref="TimeSpan"/> var.
        /// </summary>
        /// <param name="certName"></param>
        /// <param name="expiresIn"></param>
        /// <returns></returns>
        public static X509Certificate2 GetCertificate(string certName, TimeSpan expiresIn)
        {

            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadWrite);

            var existingCert = store.Certificates
                .OfType<X509Certificate2>()
                .Where(cert => {
                    return cert.Subject
                     .ToLower()
                     .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0]
                     .ToLower()
                     .Contains("cn=" + certName);
                }
            );

            if (existingCert.Any())
            {
                store.Close();

                return existingCert.First();
            }
            else
            {

                OCertification cs = new(string.Empty)
                {
                    IsCertificationAuthority    = false,
                    SubjectName                 = "CN=" + certName,
                    FriendlyName                = certName,
                    Algorithm                   = "SHA256WithRSA",
                    ExpiresIn                   = DateTime.Now.Add(expiresIn),
                    KeyUsage                    = new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment),
                    KeySize                     = 2048,
                    AlternateDomainNames = new List<string>() {
                        certName
                    },
                    Oids = new List<Oid>()
                    {
                        new Oid("1.3.6.1.5.5.7.3.1"),   //Server Authentication
                        new Oid("1.3.6.1.5.5.7.3.2")    //Client Authentication
                    }
                };

                X509Certificate2 cert = cs.GenerateCertificate(out AsymmetricKeyParameter _, out AsymmetricKeyParameter _);

                if (cert != null)
                    store.Add(cert);

                store.Close();

                return cert;
            }
        }

        #endregion

        #region Deconstuctor

        bool IsDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (!IsDisposed)
                if (disposing)
                {

                    EndPoints.Clear();

                    foreach (OWebApplication app in Applications.Values)
                        try { app.Dispose(); } catch { }

                    Applications.Clear();
                    StatusHTMLPages.Clear();
                    StatusHTMLPagePlaceHolders.Clear();

                    foreach (OWebApplication app in Sessions.Values)
                        try { app.Dispose(); } catch { }

                    try{ Stop(); } catch { }

                }

            IsDisposed = true;
        }

        #endregion

    }

}
