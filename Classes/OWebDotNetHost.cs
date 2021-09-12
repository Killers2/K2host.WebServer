/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
//using System.Web;
//using System.Web.Hosting;

namespace K2host.WebServer.Classes
{

    // Problems, its slow processing..
    // can not send the http request to the page
    // 

    // //OTCPPageType.DotNet https://symsign.liquid-streams.co.uk:446/Processor.aspx
    // // https://flylib.com/books/en/2.371.1/hosting_aspnet_outside_iis.html
    // // http://radio-weblogs.com/0105476/stories/2002/07/12/executingAspxPagesWithoutAWebServer.html
    // // https://codingvision.net/c-http-server-with-aspnet
    //try
    //{

    //    //MemoryStream htmlOutPut = new MemoryStream();

    //    OTCPWebDotNetHost h = (OTCPWebDotNetHost)ApplicationHost.CreateApplicationHost(
    //         typeof(OTCPWebDotNetHost),
    //         "/",
    //         @"D:\Development\WorkFlow Systems\LiquidStream\LiquidObjects\bin\Debug\WebService\root\SymSign"
    //     );

    //    string x = @"D:\Development\WorkFlow Systems\LiquidStream\LiquidObjects\bin\Debug\WebService\root\SymSign\Processor.aspx";
    //    h.CreateHtmlPage(x, null, Request.Response.OutputStream);
    //    //htmlOutPut.CopyTo(Request.Response.OutputStream);
    //}
    //catch (Exception ex)
    //{


    //}

    public class OWebDotNetHost : MarshalByRefObject
    {


        // Processes the ASPX file and creates the corresponding HTML file
        //public void CreateHtmlPage(string aspxFile, string queryString, Stream htmlStream)
        //{

        //    StreamWriter writer = new StreamWriter(htmlStream);

        //    SimpleWorkerRequest req = new SimpleWorkerRequest(
        //        aspxFile,       // ASPX file name
        //        queryString,    // Query string 
        //        writer          // Output stream (i.e., Console.Out)
        //    );

        //    HttpRuntime.ProcessRequest(req);

        //    writer.Close();

        //}

    }



}
