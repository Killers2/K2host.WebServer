/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using K2host.WebServer.Enums;
using K2host.Core;

using gl = K2host.Core.OHelpers;

namespace K2host.WebServer.Classes
{

    /// <summary>
    /// This class is used to collect and set data for web http comunication between a client and server web pages.
    /// </summary>
    public class OWebRequest : IDisposable
    {

        #region Embedded

        /// <summary>
        /// This class is used to build the boundy objects that come back in http requests body data.
        /// Unsupported At the moment
        /// </summary>
        public class Boundray : IDisposable
        {

            #region Properties

            /// <summary>
            /// Holds the Boundray Id from the web request
            /// </summary>
            public string BoundrayId
            {
                get;
                set;
            }

            /// <summary>
            /// This the content disposition which is in each boundary part.
            /// </summary>
            public string ContentDisposition
            {
                get;
                set;
            }

            /// <summary>
            /// This the content type which is in each boundary part.
            /// </summary>      
            public string ContentType
            {
                get;
                set;
            }

            /// <summary>
            /// This the content data length from the value part of the bounary.
            /// </summary>  
            public long ContentLength64
            {
                get;
                set;
            }

            /// <summary>
            /// This the field name of the bounary which is in each boundary part.
            /// </summary>
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// This the file name if the boundary containes a file upload.
            /// </summary>   
            public string Filename
            {
                get;
                set;
            }

            /// <summary>
            /// The vaalue data as a string if it is not a file upload.
            /// </summary>    
            public string Data
            {
                get;
                set;
            }

            /// <summary>
            /// The value content in a stream to read for later.
            /// </summary>
            public MemoryStream Stream
            {
                get;
                set;
            }

            #endregion

            #region Constuctor

            /// <summary>
            /// Creates a new instance of the boundary class
            /// </summary>
            /// <param name="name">Key name of the field</param>
            /// <param name="boundrayId"></param>
            public Boundray(string name, string boundrayId)
            {
                Name = name;
                BoundrayId = boundrayId;
            }

            /// <summary>
            /// Creates a new instance of the boundary class
            /// </summary>
            /// <param name="name">Key name of the field</param>
            /// <param name="boundrayId"></param>
            /// <param name="segment"></param>
            public Boundray(string name, string boundrayId, byte[] segment)
               : this(name, boundrayId)
            {
                Build(segment);
            }

            #endregion

            #region Methods

            /// <summary>
            /// This parses the data in to the objects property values.
            /// Perforance: parsing the data. needs work.
            /// </summary>
            /// <param name="segment"></param>
            public void Build(byte[] segment)
            {

                byte[] boundaryBytesMarker  = Encoding.UTF8.GetBytes("\r\n" + "--" + BoundrayId + "\r\n");
                byte[] markerBreak          = Encoding.UTF8.GetBytes("\r\n");
                byte[] markerDoubleBreak    = Encoding.UTF8.GetBytes("\r\n\r\n");
                
                segment                     = segment.Skip(boundaryBytesMarker.Length - 1).ToArray();
                byte[] lineOne              = segment.Take(segment.WhereIsFirstIndex(markerBreak)).ToArray();
                
                segment                     = segment.Skip(lineOne.Length).ToArray();
                byte[] lineTwo              = segment.Take(segment.WhereIsFirstIndex(markerDoubleBreak)).ToArray();
                
                segment                     = segment.Skip(lineTwo.Length).ToArray();
                byte[] lineTree             = segment.Skip(markerDoubleBreak.Length).ToArray();

                //Lets Parse the data.
                string      line    = Encoding.UTF8.GetString(lineOne);
                string[]    parts   = line.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                string      key     = parts[1].Remove(0, parts[1].IndexOf("\"") + "\"".Length);
                
                ContentDisposition  = parts[0].Remove(0, parts[0].IndexOf(": ") + 2);
                Name                = key.Substring(0, key.IndexOf("\""));

                if (line.ToLower().Contains("filename"))
                {
                    key         = parts[2].Remove(0, parts[2].IndexOf("\"") + "\"".Length);
                    Filename    = key.Substring(0, key.IndexOf("\""));
                }

                line        = Encoding.UTF8.GetString(lineTwo);
                ContentType = line.Remove(0, line.IndexOf(": ") + 2);

                Stream = new MemoryStream(lineTree);
                ContentLength64 = Stream.Length;
                
                if (string.IsNullOrEmpty(Filename))
                    Data = line;

            }

            /// <summary>
            /// Builds a new boundary item from just the id and byte data segment.
            /// </summary>
            /// <param name="boundrayId"></param>
            /// <param name="segment"></param>
            /// <returns></returns>
            public static Boundray Parse(string boundrayId, byte[] segment)
            {
                return new Boundray(string.Empty, boundrayId, segment);
            }

            #endregion

            #region Destuctor

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

                        if (Stream != null)
                            Stream.Dispose();

                    }

                IsDisposed = true;
            }

            #endregion

        }

        #endregion

        #region Properties

        /// <summary>
        /// This holds the request headers in a default keyValuePair
        /// </summary>
        readonly Dictionary<string, string> _requests;

        /// <summary>
        /// This helps get the values by default as in the class.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string this[string item]
        {
            get
            {
                try
                {
                    return _requests[item];
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    _requests[item] = value;
                }
                catch { }
            }
        }

        /// <summary>
        /// This holds any query string or path routing values
        /// </summary>
        public Dictionary<string, string> Parms
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the boundary elements send over as multipart/form-data
        /// </summary>
        public Dictionary<string, Boundray> Boundries
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the Url information.
        /// </summary>
        public Uri Url
        {
            get;
            set;
        }

        /// <summary>
        /// The http protocol / schema
        /// </summary>
        public string Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// The http content type
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// The http header 'via' normally filled from a proxy server
        /// </summary>
        public string Via
        {
            get;
            set;
        }

        /// <summary>
        /// The http accept header
        /// </summary>
        public string Accept
        {
            get;
            set;
        }

        /// <summary>
        /// The http connection header for keep alive etc.
        /// </summary>
        public OResponseConnection Connection
        {
            get;
            set;
        }

        /// <summary>
        /// The http header for user-agent
        /// </summary>
        public string UserAgent
        {
            get;
            set;
        }

        /// <summary>
        /// The http header content-length
        /// </summary>
        public int ContentLength
        {
            get;
            set;
        }

        /// <summary>
        /// The http header host
        /// </summary>
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// The original http header in one string.
        /// </summary>
        public string RequestData
        {
            get;
            set;
        }

        /// <summary>
        /// The complete path as a string including the querystring.
        /// </summary>
        public string CompletePath
        {
            get;
            set;
        }

        /// <summary>
        /// The complete path as a string excluding the querystring.
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// The http header referer
        /// </summary>
        public string Referer
        {
            get;
            set;
        }

        /// <summary>
        /// The http header Access-Control-Request-Method for CORS
        /// </summary>
        public string AccessControlRequestMethod
        {
            get;
            set;
        }

        /// <summary>
        /// The http header Access-Control-Request-Headers for CORS
        /// </summary>
        public string AccessControlRequestHeaders
        {
            get;
            set;
        }

        /// <summary>
        /// The http header Origin for CORS
        /// </summary>
        public string Origin
        {
            get;
            set;
        }

        /// <summary>
        /// The http header Sec-Fetch-Mode for CORS
        /// </summary
        public string SecFetchMode
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the path as a list of path segments.
        /// </summary
        public IList<string> Paths
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the method type of <see cref="OHttpMethodType"/>
        /// </summary>
        public OHttpMethodType Method
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the request type of <see cref="OHttpRequestType"/>
        /// </summary>
        public OHttpRequestType Type
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the header data from the stream.
        /// </summary>
        public byte[] Header
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the body data from the stream.
        /// </summary>
        public byte[] Body
        {
            get;
            set;
        }

        /// <summary>
        /// This is the ident of the <see cref="OWebApplication"/>
        /// </summary>
        public Guid ApplicationId
        {
            get;
            set;
        }

        /// <summary>
        /// This is the the <see cref="OWebApplication"/> attached.
        /// </summary>
        [JsonIgnore]
        public OWebApplication Application
        {
            get;
            set;
        }

        /// <summary>
        /// This is the <see cref="OWebResponse"/> object attached.
        /// </summary>
        [JsonIgnore]
        public OWebResponse Response
        {
            get;
            set;
        }

        #endregion

        #region Constuctor

        /// <summary>
        /// Creates the instance.
        /// </summary>
        public OWebRequest()
        {

            _requests                   = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Type                        = OHttpRequestType.HttpPage;
            Parms                       = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Boundries                   = new Dictionary<string, Boundray>();
            Url                         = null;
            ContentLength               = 0;
            Protocol                    = string.Empty;
            ContentType                 = string.Empty;
            Via                         = string.Empty;
            Accept                      = string.Empty;
            Connection                  = OResponseConnection.Close;
            UserAgent                   = string.Empty;
            Host                        = string.Empty;
            CompletePath                = string.Empty;
            Referer                     = string.Empty;
            AccessControlRequestMethod  = string.Empty;
            AccessControlRequestHeaders = string.Empty;
            Origin                      = string.Empty;
            SecFetchMode                = string.Empty;
            Response                    = new OWebResponse();
            ApplicationId               = Guid.Empty;

        }

        /// <summary>
        /// Creates the instance from the data held in the stream.
        /// </summary>
        /// <param name="e"></param>
        public OWebRequest(byte[] e)
            : this()
        {
            InitiateData(e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// This allows you to add headers to headers to the request.
        /// </summary>
        /// <param name="key">key as string</param>
        /// <param name="value">value as string</param>
        /// <returns></returns>
        public bool AddHeader(string key, string value)
        {
            _requests.Add(key, value);
            return true;
        }

        /// <summary>
        /// This helps to find / check to see if there is header in the request.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsRequest(string key)
        {

            if (_requests.ContainsKey(key))
                return true;
            else
                return false;

        }
        
        /// <summary>
        /// Creates a list of value key pairs based on the headers in this request.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, string>[] CompileHeaders()
        {
            return _requests.ToArray();
        }

        /// <summary>
        /// This checks the headers for the Content-Encoding and gzip values
        /// </summary>
        /// <returns></returns>
        public bool IsGZIPCommpressed()
        {
            return (_requests != null && _requests.ContainsKey("Content-Encoding") && _requests["Content-Encoding"].Contains("gzip"));
        }

        /// <summary>
        /// This checks the headers for the Accept-Encoding and gzip values
        /// </summary>
        /// <returns></returns>
        public bool IsGZIPSupported()
        {
            return (_requests != null && _requests.ContainsKey("Accept-Encoding") && _requests["Accept-Encoding"].Contains("gzip"));
        }

        /// <summary>
        /// This helps to find / check and returns a value in the params in the request.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetQueryStringValue(string key)
        {

            if (Parms == null || Parms.Count == 0 || !Parms.ContainsKey(key))
                return string.Empty;

            return Parms[key];

        }

        /// <summary>
        /// This help render the request object from the data stream of a connection.
        /// </summary>
        /// <param name="e"></param>
        public void InitiateData(byte[] e)
        {

            byte[] marker = gl.StrToByteArray("\r\n\r\n");
            int found = e.WhereIsFirstIndex(marker);

            if (found > 0)
            {
                Header = new byte[found];
                Array.Copy(e, 0, Header, 0, Header.Length);
                Body = new byte[e.Length - (found + marker.Length)];
                Array.Copy(e, (found + marker.Length), Body, 0, Body.Length);
            }
            else
            {
                Body = new byte[e.Length];
                Array.Copy(e, Body, Body.Length);
            }

            if (Header != null)
                RequestData = new string(Encoding.UTF8.GetChars(Header));

        }

        #endregion

        #region Destuctor

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
