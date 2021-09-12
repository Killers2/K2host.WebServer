/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Net;

using Newtonsoft.Json.Linq;

using K2host.WebServer.Delegates;
using K2host.WebServer.Enums;
using K2host.Sockets.Tcp;
using K2host.Core.Delegates;
using K2host.Core;
using K2host.IO.Classes;
using K2host.IO.Delegates;
using K2host.Certificates.Extentions;

namespace K2host.WebServer.Classes
{

    /// <summary>
    /// This class hold the all the information used to run and create a web applciation on the <see cref="OWebServer"/>
    /// </summary>
    public class OWebApplication : IDisposable
    {

        #region Events

        /// <summary>
        /// The event used to trigger the page load on web pages help  in this instance.
        /// </summary>
        public OnORequestEvent PageLoad;

        /// <summary>
        /// This call back triggers before the request is processed by an application hosted.
        /// passes an OTCPWebRequest object
        /// </summary>
        public OServiceMethod BeforeHttpRequestProcessed;

        /// <summary>
        /// This call back triggers after the request is processed by an application hosted.
        /// passes an OTCPWebRequest object
        /// </summary>
        public OServiceMethod AfterHttpRequestProcessed;

        #endregion

        #region Properties

        /// <summary>
        /// This holds any web pages with code files which are used to execute on page load
        /// </summary>
        protected Dictionary<string, OWebPage> PageScripts { get; set; }

        /// <summary>
        /// This holds a stamp and is checked by the server to unload of ttl is hit.
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// This holds an ip and port that enabled binding if you want to seperate the apps from the server listeners.
        /// </summary>
        public List<IPEndPoint> Bound { get; }

        /// <summary>
        /// The webserver instance attached.
        /// </summary>
        public OWebServer Parent { get; set; }

        /// <summary>
        /// The ApplicationId of this instance.
        /// </summary>
        public Guid Ident { get; private set; }

        /// <summary>
        /// The Application root path set by adding the application from the server.
        /// </summary>
        public DirectoryInfo ApplicationRootPath { get; set; }

        /// <summary>
        /// The certificate mapping by key when we load any certificates for this instance.
        /// </summary>
        public Dictionary<string, int> CertificateMapping { get; set; }

        /// <summary>
        /// The webconfig file loaded as a json object.
        /// </summary>
        public JObject ApplicationConfig { get; set; }

        /// <summary>
        /// The clients session items saved in memory while the application ttl is alive.
        /// </summary>
        public Dictionary<string, object> Session { get; set; }

        /// <summary>
        /// The list of hostnames this web application can load from.
        /// </summary>
        public List<string> HostNames { get; set; }

        /// <summary>
        /// This helps the server create new instances cloned from the based one on the server.
        /// </summary>
        public bool AllowSessions { get; set; }

        /// <summary>
        /// This is the default home page file in a directory if none is added in the request path.
        /// </summary>
        public string DefaultWebPage { get; set; }

        /// <summary>
        /// This tells the server to reject requests on https if this is set.
        /// </summary>
        public bool AllowHttps { get; set; }

        /// <summary>
        /// The list of certificates this application can use based on the host names and https request.
        /// </summary>
        public List<X509Certificate2> AssignedCertificates { get; set; }

        /// <summary>
        /// This helps you define the CORS security on the application level which the server will render in requests.
        /// </summary>
        public bool AllowCors { get; set; }

        /// <summary>
        /// This CORS Allow-Origin request header.
        /// </summary>
        public List<string> CorsAllowOrigin { get; set; }

        /// <summary>
        /// This CORS Allow-Methods request header.
        /// </summary>
        public List<string> CorsAllowMethods { get; set; }

        /// <summary>
        /// This CORS Allow-Headers request header.
        /// </summary>
        public List<string> CorsAllowHeaders { get; set; }

        /// <summary>
        /// This CORS Max-Age in seconds request header.
        /// </summary>
        public int CorsMaxAgeSeconds { get; set; }

        /// <summary>
        /// This helps you define the HSTS (HTTP Strict Transport Security) on the application level which the server will render in requests.
        /// HSTS only works on the default ports (80 / 443) and preload only works if the max-age is about a year and it includes sub domains.
        /// Refer to https://www.twilio.com/blog/a-http-headers-for-the-responsible-developer
        /// </summary>
        public bool AllowHSTS { get; set; }

        /// <summary>
        /// This HSTS Max-Age in seconds request header.
        /// </summary>
        public int HSTSMaxAge { get; set; }

        /// <summary>
        /// This HSTS IncludeSubDomain (if you want to include all sub domains).
        /// </summary>
        public bool HSTSIncludeSubDomains { get; set; }

        /// <summary>
        /// This HSTS Preload.
        /// </summary>
        public bool HSTSPreLoad { get; set; }

        /// <summary>
        /// CSP Headers as KeyValuePairs
        /// Content-Security-Policy: upgrade-insecure-requests (Directive tells the browser to upgrade all HTTP requests to HTTPS magically).
        /// Content-Security-Policy: default-src 'self'; script-src 'self' www.google-analytics.com storage.googleapis.com; style-src 'self' 'unsafe-inline'; img-src 'self' data: www.google-analytics.com; font-src 'self' data:; connect-src 'self' www.google-analytics.com; media-src 'self' videos.contentful.com videos.ctfassets.net; object-src 'self'; frame-src codepen.io; frame-ancestors 'self'; worker-src 'self'; block-all-mixed-content; manifest-src 'self' 'self'; disown-opener; prefetch-src 'self'
        /// Content-Security-Policy-Report-Only: default-src 'self'; ... report-uri https://stefanjudis.report-uri.com/r/d/csp/reportOnly
        /// </summary>
        public Dictionary<string, string> HSTSCPSHeaders { get; set; }

        /// <summary>
        /// This tells the application to pre compile the script / code pages which increases the performace of you page loads.
        /// The idea here is to allow the editing of code while the server is running, almost like a debug mode :)
        /// </summary>
        public bool RunTimeCompile { get; set; }

        /// <summary>
        /// The list of resouces that will be reconised as html resouces with the code file counter parts.
        /// Example: .aspx .ascx
        /// </summary>
        public List<string> DotNetResoucesExtentions { get; set; }

        /// <summary>
        /// The list of code file type extentions. 
        /// Example: .vb .cs
        /// </summary>
        public List<string> DotNetCodeExtentions { get; set; }

        /// <summary>
        /// This holds the list of virtual paths given to load a page and pass parms values in the request.
        /// </summary>
        public Dictionary<string, OWebRoute> RouteTable { get; set; }

        /// <summary>
        /// This is where the application settings are listed from the loaded web config file.
        /// </summary>
        public Dictionary<string, string> ApplicationSettings { get; set; }

        /// <summary>
        /// This helps the server ignore file resources and run directy to code processing.
        /// </summary>
        public bool VirturalService { get; private set; }

        #endregion

        #region Constuctor

        /// <summary>
        /// Creates the instance of this class.
        /// </summary>
        public OWebApplication()
        {
            Ident                       = Guid.NewGuid();
            AllowCors                   = false;
            CorsAllowOrigin             = new List<string>() { "*" };
            CorsAllowMethods            = new List<string>() { "POST", "GET", "DELETE", "OPTIONS", "PUT", "PATCH" };
            CorsAllowHeaders            = new List<string>() { "*" };
            CorsMaxAgeSeconds           = 300;
            ApplicationRootPath         = null;
            DefaultWebPage              = "index.html";
            ApplicationConfig           = null;
            ApplicationSettings         = new Dictionary<string, string>();
            Session                     = new Dictionary<string, object>();
            HostNames                   = new List<string>();
            AssignedCertificates        = new List<X509Certificate2>();
            CertificateMapping          = new Dictionary<string, int>();
            PageScripts                 = new Dictionary<string, OWebPage>();
            DotNetResoucesExtentions    = new List<string>();
            DotNetCodeExtentions        = new List<string>();
            RouteTable                  = new Dictionary<string, OWebRoute>();
            HSTSCPSHeaders              = new Dictionary<string, string>();
            Bound                       = new List<IPEndPoint>();
        }

        /// <summary>
        /// Creates the instance of this class.
        /// </summary>
        /// <param name="applicationFullPath"></param>
        public OWebApplication(string applicationFullPath, IPEndPoint[] iPEndPoint, bool isVirturalService = false)
           : this()
        {
            ApplicationRootPath = new DirectoryInfo(applicationFullPath);
            Bound.AddRange(iPEndPoint);
            VirturalService = isVirturalService;
            LoadWebConfiguration();
        }

        /// <summary>
        /// Creates the instance of this class.
        /// </summary>
        /// <param name="applicationFullPath"></param>
        public OWebApplication(DirectoryInfo applicationFullPath, IPEndPoint[] iPEndPoint, bool isVirturalService = false)
            : this()
        {
            ApplicationRootPath = applicationFullPath;
            Bound.AddRange(iPEndPoint);
            VirturalService = isVirturalService;
            LoadWebConfiguration();
        }

        #endregion

        #region Methods

        /// <summary>
        /// This is used when the instance is created to create and list all configuration items to the application instance.
        /// </summary>
        private void LoadWebConfiguration()
        {

            try
            {
                if (File.Exists(ApplicationRootPath.FullName + "\\WebConfig.json"))
                    ApplicationConfig = JObject.Parse(File.ReadAllText(ApplicationRootPath.FullName + "\\WebConfig.json"));
            }
            catch { }

            //Load Config data
            if (ApplicationConfig != null)
            {

                // ====================================================================================================================================

                //HTTP Defaults

                JArray HostNameList = null;

                try
                {
                    HostNameList = (JArray)ApplicationConfig.Properties().Where(x => x.Name == "HostNames").First().Value;
                }
                catch { throw new Exception("Please make sure there is at least a host name / domain name for this web application."); }

                if (HostNameList != null)
                {
                    HostNames.Clear();
                    foreach (string h in HostNameList)
                        HostNames.Add(h);
                }

                // ====================================================================================================================================

                try
                {
                    AllowSessions = Convert.ToBoolean(ApplicationConfig.Properties().Where(x => x.Name == "AllowSessions").First().Value.ToString());
                }
                catch { }

                // ====================================================================================================================================

                try
                {
                    DefaultWebPage = ApplicationConfig.Properties().Where(x => x.Name == "DefaultWebPage").First().Value.ToString();
                }
                catch { }

                // ====================================================================================================================================

                //SSL Stuff

                JObject SSLSection = (JObject)ApplicationConfig.Properties().Where(x => x.Name == "SSL").First().Value;

                try
                {
                    AllowHttps = Convert.ToBoolean(SSLSection.Properties().Where(x => x.Name == "AllowHttps").First().Value.ToString());
                }
                catch { }

                // ====================================================================================================================================

                JArray CertificateNames = null;

                try
                {
                    CertificateNames = (JArray)SSLSection.Properties().Where(x => x.Name == "CertificateNames").First().Value;
                }
                catch { }

                if (CertificateNames != null)
                {
                    AssignedCertificates.Clear();
                    foreach (string m in CertificateNames)
                    {
                        X509Certificate2 c = OWebServer.GetCertificate(m, new TimeSpan(0, 0, 30, 0));
                        if (!AssignedCertificates.Contains(c))
                        {
                            AssignedCertificates.Add(c);
                            CertificateMapping.Add(c.Subject.ToLower().Replace("cn=", string.Empty), (AssignedCertificates.Count - 1));
                            c.GetAlternativeSubjectNames()
                                .ForEach(sname =>
                                {
                                    if (!CertificateMapping.ContainsKey(sname))
                                        CertificateMapping.Add(sname, (AssignedCertificates.Count - 1));
                                });
                        }
                    }
                }

                // ====================================================================================================================================

                //CORS Stuff

                JObject CORSSection = (JObject)ApplicationConfig.Properties().Where(x => x.Name == "CORS").First().Value;

                try
                {
                    AllowCors = Convert.ToBoolean(CORSSection.Properties().Where(x => x.Name == "AllowCors").First().Value.ToString());
                }
                catch { }

                // ====================================================================================================================================

                JArray CorsAllowOriginList = null;

                try
                {
                    CorsAllowOriginList = (JArray)CORSSection.Properties().Where(x => x.Name == "CorsAllowOrigin").First().Value;
                }
                catch { throw new Exception("Please make sure there is at least a host name / domain name for this configuration."); }

                if (CorsAllowOriginList != null)
                {
                    CorsAllowOrigin.Clear();
                    foreach (string h in CorsAllowOriginList)
                        CorsAllowOrigin.Add(h);
                }

                // ====================================================================================================================================

                JArray CorsAllowMethodsList = null;

                try
                {
                    CorsAllowMethodsList = (JArray)CORSSection.Properties().Where(x => x.Name == "CorsAllowMethods").First().Value;
                }
                catch { }

                if (CorsAllowMethodsList != null)
                {
                    CorsAllowMethods.Clear();
                    foreach (string m in CorsAllowMethodsList)
                        CorsAllowMethods.Add(m);
                }

                // ====================================================================================================================================

                JArray CorsAllowHeadersList = null;

                try
                {
                    CorsAllowHeadersList = (JArray)CORSSection.Properties().Where(x => x.Name == "CorsAllowHeaders").First().Value;
                }
                catch { }

                if (CorsAllowHeadersList != null)
                {
                    CorsAllowHeaders.Clear();
                    foreach (string h in CorsAllowHeadersList)
                        CorsAllowHeaders.Add(h);
                }

                // ====================================================================================================================================

                try
                {
                    CorsMaxAgeSeconds = Convert.ToInt32(ApplicationConfig.Properties().Where(x => x.Name == "CorsMaxAgeSeconds").First().Value.ToString());
                }
                catch { }

                // ====================================================================================================================================

                //HSTS Stuff

                JObject HSTSSection = (JObject)ApplicationConfig.Properties().Where(x => x.Name == "HSTS").First().Value;

                try
                {
                    AllowHSTS = Convert.ToBoolean(HSTSSection.Properties().Where(x => x.Name == "Enabled").First().Value.ToString());
                }
                catch { }

                try
                {
                    HSTSMaxAge = Convert.ToInt32(HSTSSection.Properties().Where(x => x.Name == "MaxAge").First().Value.ToString());
                }
                catch { }

                try
                {
                    HSTSIncludeSubDomains = Convert.ToBoolean(HSTSSection.Properties().Where(x => x.Name == "IncludeSubDomains").First().Value.ToString());
                }
                catch { }

                try
                {
                    HSTSPreLoad = Convert.ToBoolean(HSTSSection.Properties().Where(x => x.Name == "PreLoad").First().Value.ToString());
                }
                catch { }

                JObject CPSSection = (JObject)HSTSSection.Properties().Where(x => x.Name == "CPS").First().Value;

                foreach (KeyValuePair<string, JToken> o in CPSSection)
                    try { HSTSCPSHeaders.Add(o.Key, o.Value.ToString()); } catch { }

                // ====================================================================================================================================

                //CODE Stuff

                JObject CODESSection = (JObject)ApplicationConfig.Properties().Where(x => x.Name == "CODE").First().Value;

                try
                {
                    RunTimeCompile = Convert.ToBoolean(CODESSection.Properties().Where(x => x.Name == "RunTimeCompile").First().Value.ToString());
                }
                catch { }

                // ====================================================================================================================================

                JArray DotNetResoucesExtentionList = null;

                try
                {
                    DotNetResoucesExtentionList = (JArray)CODESSection.Properties().Where(x => x.Name == "DotNetResoucesExtention").First().Value;
                }
                catch { }

                if (DotNetResoucesExtentionList != null)
                {
                    DotNetResoucesExtentions.Clear();
                    foreach (string h in DotNetResoucesExtentionList)
                        DotNetResoucesExtentions.Add(h);

                }

                // ====================================================================================================================================

                JArray DotNetCodeExtentionList = null;

                try
                {
                    DotNetCodeExtentionList = (JArray)CODESSection.Properties().Where(x => x.Name == "DotNetCodeExtention").First().Value;
                }
                catch { }

                if (DotNetCodeExtentionList != null)
                {
                    DotNetCodeExtentions.Clear();
                    foreach (string h in DotNetCodeExtentionList)
                        DotNetCodeExtentions.Add(h);

                }

                // ====================================================================================================================================

                JArray RouteTableList = null;

                try
                {
                    RouteTableList = (JArray)CODESSection.Properties().Where(x => x.Name == "RouteTable").First().Value;
                }
                catch { }

                if (RouteTableList != null)
                {
                    RouteTable.Clear();
                    foreach (JObject o in RouteTableList)
                        RouteTable.Add(o.Properties().First().Name.ToString(), new OWebRoute(o.Properties().First().Name.ToString(), o.Properties().First().Value.ToString()));

                }

                // ====================================================================================================================================

                OFolderFileLooper ffl = new()
                {
                    OnFileFound = new OnFileFoundEvent(w => {
                        if (DotNetCodeExtentions.Contains(w.Extension))
                        {
                            bool found = false;
                            string pagename = w.Name.Substring(0, w.Name.IndexOf("."));
                            string webpage = w.Directory.FullName + "\\" + pagename;

                            foreach (string ext in DotNetResoucesExtentions)
                                if (File.Exists(webpage + ext))
                                {
                                    webpage += ext;
                                    pagename += ext;
                                    found = true;
                                    break;
                                }

                            if (found)
                                PageScripts.Add(pagename, new OWebPage(w, new FileInfo(webpage), this));

                        }
                    })
                };

                ffl.IterateDirectories(
                    new DirectoryInfo(
                        ApplicationRootPath.FullName
                    )
                );

                ffl.Dispose();
                ffl = null;

                // ====================================================================================================================================

                // Application Settings Stored at runtime and saved.

                JArray ApplicationSettingsList = null;

                try
                {
                    ApplicationSettingsList = (JArray)ApplicationConfig.Properties().Where(x => x.Name == "ApplicationSettings").First().Value;
                }
                catch { }

                if (ApplicationSettingsList != null)
                {
                    ApplicationSettings.Clear();
                    foreach (JObject o in ApplicationSettingsList)
                        ApplicationSettings.Add(o.Properties().First().Name.ToString(), o.Properties().First().Value.ToString());

                }

                // ====================================================================================================================================


            }

        }

        /// <summary>
        /// This is used by the server to run the application session / instance when a client request comes in over the internet.
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public OPageType Process(OWebRequest Request)
        {

            LastActivity            = DateTime.Now;
            Request.Application     = this;
            Request.ApplicationId   = Request.Application.Ident;
            bool AllowProcess       = true;
            OPageType output        = OPageType.None;

            if (Request.Paths.Count > 0)
                output = PageScripts.ContainsKey(Request.Paths.Last()) ? OPageType.Script : OPageType.None;

            //Check to see if this is a service path, at this point a route only has a path part as a webhook.
            if (RouteTable.ContainsKey(Request.Paths[0])) // check for the first part of the path in the rout table.
                if (RouteTable[Request.Paths[0]].MergePath() == "/" + Request.Paths[0] + "/" + Request.Paths[1]) // if they both match when merged .
                {
                    AllowProcess = false;// stop other processing and try and invoke the on relay request method in the route.
                    RouteTable[Request.Paths[0]].OnRelay?.Invoke(Request);
                }

            if (AllowProcess && output == OPageType.Script)
                PageScripts[Request.Paths.Last()].PageLoad?.Invoke(Request, RunTimeCompile);

            if (AllowProcess && output == OPageType.None)
                PageLoad?.Invoke(Request);

            return output;

        }

        /// <summary>
        /// This reloads the applciation and configuration settings.
        /// </summary>
        public void Reload()
        {
            HostNames.Clear();
            CorsAllowOrigin.Clear();
            CorsAllowMethods.Clear();
            CorsAllowHeaders.Clear();
            AssignedCertificates.Clear();
            CertificateMapping.Clear();
            ApplicationSettings.Clear();
            Session.Clear();
            PageScripts.Clear();
            DotNetResoucesExtentions.Clear();
            DotNetCodeExtentions.Clear();
            RouteTable.Clear();

            LoadWebConfiguration();
        }

        /// <summary>
        /// This helps clone and instance for sessions based applications using the config from the original one.
        /// </summary>
        /// <returns></returns>
        public OWebApplication Clone()
        {

            OWebApplication o = new()
            {
                Parent              = Parent,
                AllowHttps          = AllowHttps,
                AllowCors           = AllowCors,
                CorsMaxAgeSeconds   = CorsMaxAgeSeconds,
                DefaultWebPage      = DefaultWebPage,
                RunTimeCompile      = RunTimeCompile,
                ApplicationRootPath = ApplicationRootPath,
                ApplicationConfig   = ApplicationConfig,
                VirturalService     = VirturalService,
                Ident               = Ident
            };

            o.HostNames.Clear();
            o.CorsAllowOrigin.Clear();
            o.CorsAllowMethods.Clear();
            o.CorsAllowHeaders.Clear();
            o.AssignedCertificates.Clear();
            o.CertificateMapping.Clear();
            o.ApplicationSettings.Clear();
            o.PageScripts.Clear();
            o.DotNetResoucesExtentions.Clear();
            o.DotNetCodeExtentions.Clear();
            o.RouteTable.Clear();

            o.HostNames.AddRange(HostNames);
            o.CorsAllowOrigin.AddRange(CorsAllowOrigin);
            o.CorsAllowMethods.AddRange(CorsAllowMethods);
            o.CorsAllowHeaders.AddRange(CorsAllowHeaders);
            o.AssignedCertificates.AddRange(AssignedCertificates);
            o.DotNetResoucesExtentions.AddRange(DotNetResoucesExtentions);
            o.DotNetCodeExtentions.AddRange(DotNetCodeExtentions);

            foreach (string key in CertificateMapping.Keys)
                o.CertificateMapping.Add(key, CertificateMapping[key]);

            foreach (string key in ApplicationSettings.Keys)
                o.ApplicationSettings.Add(key, ApplicationSettings[key]);

            foreach (string key in PageScripts.Keys)
                o.PageScripts.Add(key, PageScripts[key]);

            foreach (string key in RouteTable.Keys)
                o.RouteTable.Add(key, RouteTable[key]);

            return o;
        }

        /// <summary>
        /// This is used by the server to get the certificate relevent in an ssl / tls connection via the SNI information.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public X509Certificate2 GetAssignedCertificate(OServerSNI e)
        {
            X509Certificate2 output;

            if (CertificateMapping.ContainsKey(e.ServerNameIndication))
                output = AssignedCertificates[CertificateMapping[e.ServerNameIndication]];
            else
            {

                // SNI always comes in as full host name.. 
                // Lets remove the subdomain to get the main domain name.
                // Loading a cert from the main domain and not the full can cause problems if the cert is just a single domain one.
                // and not a multi domain cert / multi sub domain cert.

                string hostname = e.ServerNameIndication.Remove(0, e.ServerNameIndication.IndexOf(".") + 1);

                output = AssignedCertificates[CertificateMapping[hostname]];

            }

            return output;
        }

        /// <summary>
        /// Used by the server to match a url path to a virtual path on this instance.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>The real path to the server</returns>
        public string MatchRouteUrl(OWebRequest e)
        {

            string AccualPath = string.Empty;

            foreach (OWebRoute r in RouteTable.Values)
                if (r.MatchRouteUrl(e))
                    return r.RealPath;

            return AccualPath;
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
                    PageLoad = null;
                    HostNames.Clear();
                    CorsAllowOrigin.Clear();
                    CorsAllowMethods.Clear();
                    CorsAllowHeaders.Clear();
                    AssignedCertificates.Clear();
                    ApplicationSettings.Clear();
                    Session.Clear();
                }

            IsDisposed = true;
        }

        #endregion

    }


}
