/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/

using K2host.WebServer.Classes;

namespace K2host.WebServer.Delegates
{

    public delegate void OnORequestEvent(OWebRequest request);

    public delegate void OnORequestCompileEvent(OWebRequest request, bool IsRunTimeCompile);

}
