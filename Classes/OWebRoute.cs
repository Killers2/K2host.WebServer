/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using K2host.WebServer.Delegates;

namespace K2host.WebServer.Classes
{

    /// <summary>
    /// This class hold the route information for the <see cref="OWebApplication"/> instance.
    /// </summary>
    public class OWebRoute : IDisposable
    {

        #region Properties

        /// <summary>
        /// This is an overide PageLoad event that can held at the route instead of the default server processor.
        /// </summary>
        public OnORequestEvent OnRelay { get; set; }

        /// <summary>
        /// The path used in the request 
        /// </summary>
        public string RequestedPath { get; set; }

        /// <summary>
        /// The path to the resouce in the application. 
        /// </summary>
        public string RealPath { get; set; }

        /// <summary>
        /// The list of parms in the virtual folder path. 
        /// </summary>
        public List<string> Parameters { get; set; }

        /// <summary>
        /// This holds the regular expression to check the path against.
        /// </summary>
        private string RegexString { get; set; }

        /// <summary>
        /// This holds the regular expression and object to use when checking the path.
        /// </summary>
        private Regex RegexPattern { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the instance.
        /// </summary>
        public OWebRoute()
        {

            RequestedPath = string.Empty;
            RealPath = string.Empty;
            Parameters = new List<string>();
        }

        /// <summary>
        /// Creates the instance with the virtual path and the real path
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="realPath"></param>
        public OWebRoute(string virtualPath, string realPath)
            : this()
        {

            RequestedPath = virtualPath;
            RealPath = realPath;
            RegexString = virtualPath;

            if (virtualPath.Contains("{"))
            {
                string[] parts = virtualPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (part.StartsWith("{") && part.EndsWith("}"))
                        RegexString = RegexString.Replace(part, @"(\w+)");
                    else
                        RegexString = RegexString.Replace(part, @"(" + part + ")");
                    Parameters.Add(part);
                }
            }

            RegexPattern = new Regex(RegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        }

        #endregion

        #region Method

        /// <summary>
        /// Used to check the requested path againts the regex and returns a boolean.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool MatchRouteUrl(OWebRequest e)
        {

            MatchCollection Routes = RegexPattern.Matches(e.Path);

            if (Routes.Count != 1)
                return false;

            for (int index = 0; index <= Routes[0].Groups.Count - 1; index++)
                if (index > 0)
                    if (Routes[0].Groups[index].Value != Parameters[index - 1])
                        e.Parms.Add(Parameters[index - 1].Replace("{", string.Empty).Replace("}", string.Empty), Routes[0].Groups[index].Value);

            return true;

        }

        /// <summary>
        /// Used to merge both parts and build a path relivent for routing.
        /// </summary>
        /// <returns></returns>
        public string MergePath()
        {

            return RequestedPath + RealPath;

        }

        #endregion

        #region Destructor

        bool IsDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                Parameters.Clear();
                OnRelay = null;
            }

            IsDisposed = true;
        }

        #endregion

    }


}
