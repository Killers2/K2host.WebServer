/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using K2host.WebServer.Classes;

namespace K2host.WebServer.Delegates
{

    public delegate void OnORequestEvent(OWebRequest request);

    public delegate void OnORequestCompileEvent(OWebRequest request, bool IsRunTimeCompile);

    public delegate void OnBeforeHttpResponceSentEvent(byte[] data);

    public delegate void OnAfterHttpResponceSentEvent(OWebRequest request);
    
    public delegate void OnBeforeHttpRequestProcessedEvent(OWebRequest request);

    public delegate void OnAfterHttpRequestProcessedEvent(OWebRequest request);

    public delegate void OnSessionCreatedEvent(OWebApplication application);

    public delegate void OnSessionUnloadedEvent(OWebApplication application);

    public delegate void OnWebServerErrorEvent(Exception ex);

}
