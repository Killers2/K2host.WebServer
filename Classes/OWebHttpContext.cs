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
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using K2host.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;

using gl = K2host.Core.OHelpers;
using gw = K2host.WebServer.OHelpers;

namespace K2host.WebServer.Classes
{

    public class OWebHttpContext : HttpContext
    {

        public override IFeatureCollection Features { get; }
        public override HttpRequest Request { get; }
        public override HttpResponse Response { get; }
        public override ConnectionInfo Connection { get; }
        public override WebSocketManager WebSockets { get; }

        [Obsolete("This property is obsolete and will be removed in a future version.", false)]
        public override AuthenticationManager Authentication { get; }

        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; }
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; }
        public override ISession Session { get; set; }

        public OWebHttpContext(byte[] httprequest, object e) : base()
        {
            Request         = new OWebHttpContextRequest(this, httprequest);
            Response        = new OWebHttpContextResponse(this, e);
        }

        public OWebHttpContext(HttpRequest request, HttpResponse response) : base()
        {
            Request     = request;
            Response    = response;
        }
       
        public OWebHttpContext(IFeatureCollection features, HttpRequest request, HttpResponse response, ConnectionInfo connection, WebSocketManager webSockets) : base()
        {
            Features    = features;
            Request     = request;
            Response    = response;
            Connection  = connection;
            WebSockets  = webSockets;
        }

        [Obsolete("This property is obsolete and will be removed in a future version.", false)]
        public OWebHttpContext(IFeatureCollection features, HttpRequest request, HttpResponse response, ConnectionInfo connection, WebSocketManager webSockets, AuthenticationManager authentication) : base()
        {
            Features        = features;
            Request         = request;
            Response        = response;
            Connection      = connection;
            WebSockets      = webSockets;
            Authentication  = authentication;
        }

        public override void Abort()
        {
            
        }
    }

    public class OWebHttpContextRequest : HttpRequest
    {

        public override HttpContext HttpContext { get; }
        public override bool HasFormContentType { get; }
        public override IHeaderDictionary Headers { get; }

        public override string Method { get; set; }
        public override string Scheme { get; set; }
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; }
        public override string Protocol { get; set; }
        public override IRequestCookieCollection Cookies { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override Stream Body { get; set; }

        public override IFormCollection Form { get; set; }

        public OWebHttpContextRequest(HttpContext context, byte[] e) : base()
        {
            HttpContext = context;
            Headers     = new HeaderDictionary();

            byte[]  Marker      = gl.StrToByteArray("\r\n\r\n");
            int     Found       = e.WhereIsFirstIndex(Marker);
            byte[]  Header      = Array.Empty<byte>();
            string  Request     = string.Empty;

            if (Found > 0)
            {
                Header = new byte[Found];
                Array.Copy(e, 0, Header, 0, Header.Length);
                var b = new byte[e.Length - (Found + Marker.Length)];
                Array.Copy(e, (Found + Marker.Length), b, 0, b.Length);
                Body = new MemoryStream(b);
            }
            else
            {
                var b = new byte[e.Length];
                Array.Copy(e, b, b.Length);
                Body = new MemoryStream(b);
            }

            if (Header != null)
                Request = new string(Encoding.UTF8.GetChars(Header));

            //Build Header Values
            string[] lines = Request.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int j = 0; j < lines.Length; j++)
            {

                if (j == 0) // First line is always type and path.
                {
                    string[] parts      = lines[j].Split(new string[] { " " }, StringSplitOptions.None);
                    Method              = gw.GetMethodType(parts[0]).ToString();
                    Path                = parts[1];
                    Protocol            = parts[2];
                    IsHttps             = Protocol.ToLower().Contains("https");

                    string[] subPaths = parts[1].Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                    PathBase = subPaths[0];

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
                            Headers.Add(pairs[0], pairs[1]);
                        }
                        catch
                        {
                            Headers.Add(pairs[0], string.Empty);
                        }

                        switch (pairs[0].ToLower())
                        {
                            case "content-type":    ContentType     = pairs[1];     break;
                            case "content-length":  ContentLength   = Body.Length;  break;
                            default: break;
                        }

                    }

                }
            }


        }

        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }

    }

    public class OWebHttpContextResponse : HttpResponse
    {
        public override HttpContext HttpContext { get; }
        public override IResponseCookies Cookies { get; }
        public override bool HasStarted { get; }

        public override int StatusCode { get; set; }
        public override IHeaderDictionary Headers { get; }
        public override Stream Body { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }

        public object ResponseType { get; set; }

        public OWebHttpContextResponse(HttpContext context, object e) : base()
        {
            HttpContext = context;
            ResponseType = e;
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {

        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {

        }

        public override void Redirect(string location, bool permanent)
        {

        }

    }

    public static class OWebHttpContextExtentions
    {

        public static void TryWrite(this HttpResponse e, string data)
        {

            if (e.GetType() == typeof(OWebHttpContextResponse))
            {
                
                OWebHttpContextResponse res = (OWebHttpContextResponse)e;

                if (res.ResponseType.GetType().BaseType == typeof(HttpContext)) 
                {
                    ((HttpContext)res.ResponseType).Response.WriteAsync(data);
                }
                else if (res.ResponseType.GetType().BaseType == typeof(WebSocket))
                {
                    var socket = (WebSocket)res.ResponseType;

                    if (socket.State != WebSocketState.Open)
                        return;

                    byte[] buffer = Encoding.ASCII.GetBytes(data);

                    socket.SendAsync(
                        new ArraySegment<byte>(buffer, 0, buffer.Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );

                }

            }
            else 
                e.WriteAsync(data);

        }

    }

}
