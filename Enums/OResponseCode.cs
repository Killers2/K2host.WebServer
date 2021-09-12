/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/
namespace K2host.WebServer.Enums
{

    public enum OResponseCode
    {
        OK = 200,
        Forbidden = 403,
        NotAcceptable = 406,
        InternalServerError = 500,
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        MethodNotAllowed = 405
    }

}
