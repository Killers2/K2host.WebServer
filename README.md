
# K2host.WebServer

A C# Http Web Server library for creating a custom internet information service using sockets.

The implementation supports :<br />
This WebServer will help host and serve applications running .net or / and html<br />
Supports C# and VB.net code files.<br />
Routing virtual paths<br />
CORS Security<br />
HSTS Security<br />
Multi SSL/TLS certs on one port (SNI)<br />
Multi cert sites<br />
Session based or Singleton based application instances<br />
The code base in relies on the HtmlAgilityPack (https://html-agility-pack.net/)<br /><br />

Nuget Package: https://www.nuget.org/packages/K2host.WebServer/

# Designed for Api driven services but can do a lot more.<br /><br />

---------------------------------------------------------------------------------------------------------------------------------------

First you need to create the web server object passing in a new thread manager and the default name for your server.

```c#

var InternetInformationService = new OWebServer(new OThreadManager(), "<DEFAULT SERVER NAME>")
{
    ServerCorsSecurityOverride      = false,
    SessionsTimeToLive              = 1200, // seconds (20 mins)
    RootDirectory                   = "<THE ROOT DIRECTORY OF THE SERVER>",
    OnBeforeHttpResponceSent        = data => { },
    OnAfterHttpResponceSent         = request => { },
    OnBeforeHttpRequestProcessed    = request => { },
    OnAfterHttpRequestProcessed     = request => { },
    OnSessionUnloaded               = session => { },
    OnSessionCreated                = session => { },
    OnWebServerError                = exception => { }
};
```
The properties are listed as : 
```c#

    //This is the name of the web server service instance which will also be placed in all responce headers.
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
        
```
At this point you will also need to setup the directory structure for you applications.<br />
Something like:

![RootDir](https://user-images.githubusercontent.com/5430175/133041391-d5aa0871-6ba6-43f0-b620-d4450e4b37a8.PNG)

In the application folder you will need:

![Dir](https://user-images.githubusercontent.com/5430175/133042901-bbc4cb74-efc1-4c5f-8a57-a26ee298bc1d.PNG)

The refs.cfg file will have a list of libraries your application needs when compiling c# or vb, if your application is script based.<br />
This is an exmple:

![refcfg](https://user-images.githubusercontent.com/5430175/133042921-b7234374-3171-4b9c-9345-f7b3b2619e5f.PNG)

In the webconfig file you will see or will create one based on this layout.
Adjust as you see fit.

```json
{
	"HostNames": [
		"<THE.HOSTNAME.FORTHIS.APPLICATION.COM>"
	],
	"AllowSessions": false,
	"DefaultWebPage": "index.html",
	"SSL": {
		"AllowHttps": true,
		"CertificateNames": [
			"<THE CERT NAME AS IN THE CERT STORE / FRIENDLY NAME>"
		]
	},
	"CORS": {
		"AllowCors": true,
		"CorsAllowOrigin": [
			"<THE.HOSTNAME.FORTHIS.APPLICATION.COM>",
			"*"
		],
		"CorsAllowMethods": [
			"POST",
			"GET",
			"DELETE",
			"OPTIONS",
			"PUT",
			"PATCH"
		],
		"CorsAllowHeaders": [
			"*"
		],
		"CorsMaxAgeSeconds": "300"
	},
	"HSTS": {
		"Enabled": false,
		"MaxAge": "1000",
		"IncludeSubDomains": false,
		"PreLoad": false,
		"CPS": {
			"Content-Security-Policy": "",
			"Content-Security-Policy-Report-Only": ""
		}
	},
	"CODE": {
		"RunTimeCompile": false,
		"DotNetResoucesExtention": [
			".aspx",
			".ascx"
		],
		"DotNetCodeExtention": [
			".cs",
			".vb"
		],
		"RouteTable": [
			{
				"/updates": "/update/service.aspx"
			},
			{
				"/testlong/{id}/another/{uuid}": "/testfolder/tester.aspx"
			},
			{
				"/testtwoid/{id}/{uuid}": "/testfolder/tester.aspx"
			},
			{
				"/testid/{id}/{uuid}/something/{uid}": "/testfolder/tester.aspx"
			},
			{
				"/something/{id}": "/testfolder/tester.aspx"
			}
    ]
	},
	"ApplicationSettings": [
		{
			"databaseConnectionString": "<THE CONNECTION STRING>"
		},
		{
			"SOMEKEY": "SOMESETTING"
		},
		{
			"SOMEKEY": "SOME SETTING"
		}
  ]
}
```
The ApplicationSettings can be an empty array as well as the RouteTable option.
For CORS the "*" means any

---------------------------------------------------------------------------------------------------------------------------------------

Now lets set up the server, In the root directory there is an error page with some placeholders which we can reuse for different errors.<br />

# Lets add some error pages to the server based on the staus codes.

```c#
InternetInformationService.StatusHTMLPages.Add(500, File.ReadAllText(InternetInformationService.RootDirectory + "\\error.html"));
InternetInformationService.StatusHTMLPages.Add(404, File.ReadAllText(InternetInformationService.RootDirectory + "\\error.html"));
InternetInformationService.StatusHTMLPages.Add(403, File.ReadAllText(InternetInformationService.RootDirectory + "\\error.html"));

InternetInformationService.StatusHTMLPagePlaceHolders.Add(500, new Dictionary<string, string>() {
	{ "<%placeholder_title%>",      "500 Server Error" },
	{ "<%placeholder_header%>",     "500 Server Error" },
	{ "<%placeholder_message%>",    "<%Exception%>" }
});

InternetInformationService.StatusHTMLPagePlaceHolders.Add(404, new Dictionary<string, string>() {
	{ "<%placeholder_title%>",      "404 Not Found" },
	{ "<%placeholder_header%>",     "404 Not Found" },
	{ "<%placeholder_message%>",    "<%Exception%>" }
});

InternetInformationService.StatusHTMLPagePlaceHolders.Add(403, new Dictionary<string, string>() {
	{ "<%placeholder_title%>",      "403 Forbidden" },
	{ "<%placeholder_header%>",     "403 Forbidden" },
	{ "<%placeholder_message%>",    "<%Exception%>" }
});
```
---------------------------------------------------------------------------------------------------------------------------------------

# Now lets add some end points for the listener.

```c#
InternetInformationService.AddListeners(new Dictionary<IPEndPoint, OHostType>() {
	{ new IPEndPoint(IPAddress.Parse("<YOUR IP ADDRESS>"), 80), OHostType.HTTP },
	{ new IPEndPoint(IPAddress.Parse("<YOUR IP ADDRESS>"), 443), OHostType.HTTPS }
}.ToArray());
```

---------------------------------------------------------------------------------------------------------------------------------------

# Now lets add an application to the service. The first added application is the primary app for the server.

```c#
InternetInformationService.AddApplication(
	new OWebApplication(
		InternetInformationService.RootDirectory + @"\<YOUR APPLICATION PATH>",
		InternetInformationService.EndPoints.Keys.ToArray()
	)
);
```
---------------------------------------------------------------------------------------------------------------------------------------

At this point we need to call back any requests sent to the application and intercept so we can do what we want to the request and response.<br />
There are 2 ways we can do this, if there is no route or file path for the given request we can intercept and responed here, otherwise we will get a page not found error from the server.<br />
We can ofcourse have both.

```c#
InternetInformationService.GetApplication("<YOUR.DOMAINNAME.COM>")
	.PageLoad += new OnORequestEvent(Request => {

		//Do something with the request.
		//Request

	});
```

After which we can also add routes which can be virtual paths or paths to a file passing variables based on the route.
As shown in the webconfig above.

```c#

    var app = InternetInformationService.GetApplication("<YOUR.DOMAINNAME.COM>");
    if (!app.RouteTable.ContainsKey("<YOUR VIRTUAL PATH>"))
	app.RouteTable.Add("<YOUR VIRTUAL PATH>", new OWebRoute("/<YOUR VIRTUAL PATH>", "/index.html") {
	    OnRelay = new OnORequestEvent(request => {

		//Do something with the request.
		//Request


	    })
	});

```
---------------------------------------------------------------------------------------------------------------------------------------

# At this point we can start the server using :

```c#
InternetInformationService.Start();
```
---------------------------------------------------------------------------------------------------------------------------------------

For destruction or disposing of the server we need to stop and dispose in this manor.
```c#
	InternetInformationService?.Stop();
	InternetInformationService?.Applications.Values.ForEach(webApplication => { webApplication.Dispose(); });
	InternetInformationService?.Dispose();
	InternetInformationService = null;
```
