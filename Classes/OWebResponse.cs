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
using System.Text;

using Microsoft.AspNetCore.Http;

using K2host.Core;
using K2host.WebServer.Enums;

using gw = K2host.WebServer.OHelpers;
using gl = K2host.Core.OHelpers;

namespace K2host.WebServer.Classes
{

    /// <summary>
    /// This class will be embedded within the <see cref="OWebRequest"/>
    /// This contains the resources to relay messages back to the client connection.
    /// </summary>
    public class OWebResponse : IDisposable
    {

        #region Properties

        /// <summary>
        /// This is the http status code that always needs setting.
        /// This is normally set via the <see cref="OTCPWebServer"/>
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// This is the http status description that always needs setting.
        /// This is normally set via the <see cref="OTCPWebServer"/>
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// This is where you will add the http response heads for the reply message.
        /// </summary>
        public Dictionary<string, string> Headers { get; }

        /// <summary>
        /// This the length in bytes of the content body and not the header, This is normally set automatically.
        /// </summary>
        public long ContentLength64 { get; set; }

        /// <summary>
        /// This is where data is stored prior the information being formatted for sending back to the client connection.
        /// </summary>
        public MemoryStream OutputStream { get; }

        /// <summary>
        /// This holds the type of data being sent and sets Content-Type.
        /// </summary>
        public OMimeType ContentType { get; set; }

        /// <summary>
        /// This adds the connection header Content-Encoding: gzip.
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// This adds the connection header Keep-Alive.
        /// </summary>
        public OResponseConnection Connection { get; set; }

        /// <summary>
        /// If the Connection Keep-Alive is set we can define the timeout here in seconds.
        /// </summary>
        public int KeepAliveTimeout { get; set; }

        /// <summary>
        /// If the Connection Keep-Alive is set we can define the max timeout here in seconds.
        /// </summary>
        public int KeepAliveTimeoutMax { get; set; }

        /// <summary>
        /// If the request was converted from an mvc app then the responce will be here.
        /// </summary>
        public HttpResponse MvcResponse { get; set; }

        #endregion

        #region Constuctor

        /// <summary>
        /// Creates the instance of the responce object for the <see cref="OWebRequest"/>
        /// </summary>
        public OWebResponse()
        {
            Headers = new Dictionary<string, string>();
            OutputStream = new MemoryStream();
            KeepAliveTimeout = 30;
            KeepAliveTimeoutMax = 60;
            MvcResponse = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Used to ad text / string data to the stream ready for sending.
        /// </summary>
        /// <param name="data"></param>
        public void Write(string data)
        {
            if (MvcResponse != null)
            {
                MvcResponse.StatusCode      = StatusCodes.Status200OK;
                MvcResponse.ContentType     = gw.GetStringMimeType(ContentType);
                MvcResponse.WriteAsync(data);
            }
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// This joins and compiles the data in to a byte array for the server to send back to the client.
        /// Will be used by the server to send to the client connection.
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {

            byte[] body = OutputStream.ToArray();

            if (IsCompressed)
                body = gw.CompressGZIP(body);

            ContentLength64 = body.Length;

            byte[] header = GetHeader();

            return gl.CombineByteArrays(
                header,
                body
            );

        }

        /// <summary>
        /// Used by <see cref="GetData"/> to build and comile the header first to preset the data for sending back.
        /// </summary>
        /// <returns></returns>
        private byte[] GetHeader()
        {
            StringBuilder output = new();

            output.Append("HTTP/1.1 " + StatusCode.ToString() + " " + StatusDescription);

            List<string> deny = new() { { "content-encoding" }, { "content-type" }, { "content-length" }, { "connection" } };

            Headers.Each((kvp) => {
                if (!deny.Contains(kvp.Key.ToLower()))
                    output.Append("\r\n" + kvp.Key + ": " + kvp.Value);
                return true;
            });

            if (IsCompressed)
                output.Append("\r\n" + "Content-Encoding: gzip");

            output.Append("\r\n" + "Content-Type: " + gw.GetStringMimeType(ContentType));
            output.Append("\r\n" + "Content-Length: " + ContentLength64.ToString());
            output.Append("\r\n" + "Connection: " + (Connection == OResponseConnection.KeepAlive ? "Keep-Alive" : "Close"));

            if (Connection == OResponseConnection.KeepAlive)
                output.Append("\r\n" + "Keep-Alive: timeout=" + KeepAliveTimeout.ToString() + ", max=" + KeepAliveTimeoutMax.ToString() + "");

            output.Append("\r\n\r\n");

            return Encoding.UTF8.GetBytes(output.ToString());
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

                }

            IsDisposed = true;
        }

        #endregion

    }


}
