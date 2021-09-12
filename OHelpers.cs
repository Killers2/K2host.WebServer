/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Linq;

using Newtonsoft.Json.Linq;

using K2host.WebServer.Enums;

namespace K2host.WebServer
{

    public static class OHelpers
    {

        public static OHttpMethodType GetMethodType(string e)
        {

            var result = (e.ToLower()) switch
            {
                "get" => OHttpMethodType.GET,
                "head" => OHttpMethodType.HEAD,
                "post" => OHttpMethodType.POST,
                "put" => OHttpMethodType.PUT,
                "delete" => OHttpMethodType.DELETE,
                "connect" => OHttpMethodType.CONNECT,
                "options" => OHttpMethodType.OPTIONS,
                "trace" => OHttpMethodType.TRACE,
                "patch" => OHttpMethodType.PATCH,
                _ => OHttpMethodType.NONE,
            };
            return result;

        }

        public static OMimeType GetResourceMime(FileInfo request)
        {

            if (request == null)
                return OMimeType.none;

            return GetContentTypeByExtension(request.Extension.Remove(0, 1));

        }

        public static OMimeType GetResourceMime(string request)
        {

            if (string.IsNullOrEmpty(request))
                return OMimeType.none;

            string[] filepats = request.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            if (filepats.Length > 0)
            {
                string extension = filepats[^1];
                return GetContentTypeByExtension(extension);
            }
            else
                return OMimeType.none;

        }

        public static OMimeType GetContentTypeByExtension(string extension)
        {
            return extension switch
            {
                "json"                                  => OMimeType.application_json,
                "css"                                   => OMimeType.text_css,
                "gif"                                   => OMimeType.image_gif,
                "jpg" or "jpeg"                         => OMimeType.image_jpeg,
                "ico" or "png"                          => OMimeType.image_png,
                "htm" or "html" or "xhtml" or "dhtml"   => OMimeType.text_html,
                "js"                                    => OMimeType.text_javascript,
                "xml"                                   => OMimeType.multipart_xmixedreplace,
                "zip" or "rar"                          => OMimeType.application_x_zip_compressed,
                _                                       => OMimeType.none,
            };
        }

        public static string GetStringMimeType(OMimeType mime)
        {
            string strMime = string.Empty;

            switch (mime)
            {
                case OMimeType.text_html:
                    strMime = "text/html";
                    break;
                case OMimeType.text_html_charsetutf8:
                    strMime = "text/html; charset=utf-8";
                    break;
                case OMimeType.text_xml:
                    strMime = "text/xml";
                    break;
                case OMimeType.text_javascript:
                    strMime = "text/javascript";
                    break;
                case OMimeType.multipart_xmixedreplace:
                    strMime = "multipart/x-mixed-replace; boundary=rnA00A";
                    break;
                case OMimeType.application_xml_charsetutf8:
                    strMime = "application/xml; charset=utf-8";
                    break;
                case OMimeType.application_xml:
                    strMime = "application/xml";
                    break;
                case OMimeType.application_json:
                    strMime = "application/json";
                    break;
                case OMimeType.application_x_zip_compressed:
                    strMime = "application/x-zip-compressed";
                    break;
                case OMimeType.text_css:
                    break;
                case OMimeType.image_gif:
                    break;
                case OMimeType.image_jpeg:
                    break;
                case OMimeType.image_png:
                    break;
                case OMimeType.none:
                    break;
                default:
                    strMime = mime.ToString().Replace("_", "/");
                    break;
            }

            return strMime;

        }

        public static string GetMimeType(string extension)
        {
            if (extension == null)
                throw new Exception("invalid extension");

            if (extension.StartsWith("."))
                extension = extension[1..];

            return extension.ToLower() switch 
            {
                #region Big freaking list of mime types
                "323" => "text/h323",
                "3g2" =>  "video/3gpp2",
                "3gp" =>  "video/3gpp",
                "3gp2" =>  "video/3gpp2",
                "3gpp" =>  "video/3gpp",
                "7z" =>  "application/x-7z-compressed",
                "aa" =>  "audio/audible",
                "aac" =>  "audio/aac",
                "aaf" =>  "application/octet-stream",
                "aax" =>  "audio/vnd.audible.aax",
                "ac3" =>  "audio/ac3",
                "aca" =>  "application/octet-stream",
                "accda" =>  "application/msaccess.addin",
                "accdb" =>  "application/msaccess",
                "accdc" =>  "application/msaccess.cab",
                "accde" =>  "application/msaccess",
                "accdr" =>  "application/msaccess.runtime",
                "accdt" =>  "application/msaccess",
                "accdw" =>  "application/msaccess.webapplication",
                "accft" =>  "application/msaccess.ftemplate",
                "acx" =>  "application/internet-property-stream",
                "addin" =>  "text/xml",
                "ade" =>  "application/msaccess",
                "adobebridge" =>  "application/x-bridge-url",
                "adp" =>  "application/msaccess",
                "adt" =>  "audio/vnd.dlna.adts",
                "adts" =>  "audio/aac",
                "afm" =>  "application/octet-stream",
                "ai" =>  "application/postscript",
                "aif" =>  "audio/x-aiff",
                "aifc" =>  "audio/aiff",
                "aiff" =>  "audio/aiff",
                "air" =>  "application/vnd.adobe.air-application-installer-package+zip",
                "amc" =>  "application/x-mpeg",
                "application" =>  "application/x-ms-application",
                "art" =>  "image/x-jg",
                "asa" =>  "application/xml",
                "asax" =>  "application/xml",
                "ascx" =>  "application/xml",
                "asd" =>  "application/octet-stream",
                "asf" =>  "video/x-ms-asf",
                "ashx" =>  "application/xml",
                "asi" =>  "application/octet-stream",
                "asm" =>  "text/plain",
                "asmx" =>  "application/xml",
                "aspx" =>  "application/xml",
                "asr" =>  "video/x-ms-asf",
                "asx" =>  "video/x-ms-asf",
                "atom" =>  "application/atom+xml",
                "au" =>  "audio/basic",
                "avi" =>  "video/x-msvideo",
                "axs" =>  "application/olescript",
                "bas" =>  "text/plain",
                "bcpio" =>  "application/x-bcpio",
                "bin" =>  "application/octet-stream",
                "bmp" =>  "image/bmp",
                "c" =>  "text/plain",
                "cab" =>  "application/octet-stream",
                "caf" =>  "audio/x-caf",
                "calx" =>  "application/vnd.ms-office.calx",
                "cat" =>  "application/vnd.ms-pki.seccat",
                "cc" =>  "text/plain",
                "cd" =>  "text/plain",
                "cdda" =>  "audio/aiff",
                "cdf" =>  "application/x-cdf",
                "cer" =>  "application/x-x509-ca-cert",
                "chm" =>  "application/octet-stream",
                "class" =>  "application/x-java-applet",
                "clp" =>  "application/x-msclip",
                "cmx" =>  "image/x-cmx",
                "cnf" =>  "text/plain",
                "cod" =>  "image/cis-cod",
                "config" =>  "application/xml",
                "contact" =>  "text/x-ms-contact",
                "coverage" =>  "application/xml",
                "cpio" =>  "application/x-cpio",
                "cpp" =>  "text/plain",
                "crd" =>  "application/x-mscardfile",
                "crl" =>  "application/pkix-crl",
                "crt" =>  "application/x-x509-ca-cert",
                "cs" =>  "text/plain",
                "csdproj" =>  "text/plain",
                "csh" =>  "application/x-csh",
                "csproj" =>  "text/plain",
                "css" =>  "text/css",
                "csv" =>  "text/csv",
                "cur" =>  "application/octet-stream",
                "cxx" =>  "text/plain",
                "dat" =>  "application/octet-stream",
                "datasource" =>  "application/xml",
                "dbproj" =>  "text/plain",
                "dcr" =>  "application/x-director",
                "def" =>  "text/plain",
                "deploy" =>  "application/octet-stream",
                "der" =>  "application/x-x509-ca-cert",
                "dgml" =>  "application/xml",
                "dib" =>  "image/bmp",
                "dif" =>  "video/x-dv",
                "dir" =>  "application/x-director",
                "disco" =>  "text/xml",
                "dll" =>  "application/x-msdownload",
                "dll.config" =>  "text/xml",
                "dlm" =>  "text/dlm",
                "doc" =>  "application/msword",
                "docm" =>  "application/vnd.ms-word.document.macroenabled.12",
                "docx" =>  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "dot" =>  "application/msword",
                "dotm" =>  "application/vnd.ms-word.template.macroenabled.12",
                "dotx" =>  "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
                "dsp" =>  "application/octet-stream",
                "dsw" =>  "text/plain",
                "dtd" =>  "text/xml",
                "dtsconfig" =>  "text/xml",
                "dv" =>  "video/x-dv",
                "dvi" =>  "application/x-dvi",
                "dwf" =>  "drawing/x-dwf",
                "dwp" =>  "application/octet-stream",
                "dxr" =>  "application/x-director",
                "eml" =>  "message/rfc822",
                "emz" =>  "application/octet-stream",
                "eot" =>  "application/octet-stream",
                "eps" =>  "application/postscript",
                "etl" =>  "application/etl",
                "etx" =>  "text/x-setext",
                "evy" =>  "application/envoy",
                "exe" =>  "application/octet-stream",
                "exe.config" =>  "text/xml",
                "fdf" =>  "application/vnd.fdf",
                "fif" =>  "application/fractals",
                "filters" =>  "application/xml",
                "fla" =>  "application/octet-stream",
                "flr" =>  "x-world/x-vrml",
                "flv" =>  "video/x-flv",
                "fsscript" =>  "application/fsharp-script",
                "fsx" =>  "application/fsharp-script",
                "generictest" =>  "application/xml",
                "gif" =>  "image/gif",
                "group" =>  "text/x-ms-group",
                "gsm" =>  "audio/x-gsm",
                "gtar" =>  "application/x-gtar",
                "gz" =>  "application/x-gzip",
                "h" =>  "text/plain",
                "hdf" =>  "application/x-hdf",
                "hdml" =>  "text/x-hdml",
                "hhc" =>  "application/x-oleobject",
                "hhk" =>  "application/octet-stream",
                "hhp" =>  "application/octet-stream",
                "hlp" =>  "application/winhlp",
                "hpp" =>  "text/plain",
                "hqx" =>  "application/mac-binhex40",
                "hta" =>  "application/hta",
                "htc" =>  "text/x-component",
                "htm" =>  "text/html",
                "html" =>  "text/html",
                "htt" =>  "text/webviewhtml",
                "hxa" =>  "application/xml",
                "hxc" =>  "application/xml",
                "hxd" =>  "application/octet-stream",
                "hxe" =>  "application/xml",
                "hxf" =>  "application/xml",
                "hxh" =>  "application/octet-stream",
                "hxi" =>  "application/octet-stream",
                "hxk" =>  "application/xml",
                "hxq" =>  "application/octet-stream",
                "hxr" =>  "application/octet-stream",
                "hxs" =>  "application/octet-stream",
                "hxt" =>  "text/html",
                "hxv" =>  "application/xml",
                "hxw" =>  "application/octet-stream",
                "hxx" =>  "text/plain",
                "i" =>  "text/plain",
                "ico" =>  "image/x-icon",
                "ics" =>  "application/octet-stream",
                "idl" =>  "text/plain",
                "ief" =>  "image/ief",
                "iii" =>  "application/x-iphone",
                "inc" =>  "text/plain",
                "inf" =>  "application/octet-stream",
                "inl" =>  "text/plain",
                "ins" =>  "application/x-internet-signup",
                "ipa" =>  "application/x-itunes-ipa",
                "ipg" =>  "application/x-itunes-ipg",
                "ipproj" =>  "text/plain",
                "ipsw" =>  "application/x-itunes-ipsw",
                "iqy" =>  "text/x-ms-iqy",
                "isp" =>  "application/x-internet-signup",
                "ite" =>  "application/x-itunes-ite",
                "itlp" =>  "application/x-itunes-itlp",
                "itms" =>  "application/x-itunes-itms",
                "itpc" =>  "application/x-itunes-itpc",
                "ivf" =>  "video/x-ivf",
                "jar" =>  "application/java-archive",
                "java" =>  "application/octet-stream",
                "jck" =>  "application/liquidmotion",
                "jcz" =>  "application/liquidmotion",
                "jfif" =>  "image/pjpeg",
                "jnlp" =>  "application/x-java-jnlp-file",
                "jpb" =>  "application/octet-stream",
                "jpe" =>  "image/jpeg",
                "jpeg" =>  "image/jpeg",
                "jpg" =>  "image/jpeg",
                "js" =>  "application/x-javascript",
                "jsx" =>  "text/jscript",
                "jsxbin" =>  "text/plain",
                "latex" =>  "application/x-latex",
                "library-ms" =>  "application/windows-library+xml",
                "lit" =>  "application/x-ms-reader",
                "loadtest" =>  "application/xml",
                "lpk" =>  "application/octet-stream",
                "lsf" =>  "video/x-la-asf",
                "lst" =>  "text/plain",
                "lsx" =>  "video/x-la-asf",
                "lzh" =>  "application/octet-stream",
                "m13" =>  "application/x-msmediaview",
                "m14" =>  "application/x-msmediaview",
                "m1v" =>  "video/mpeg",
                "m2t" =>  "video/vnd.dlna.mpeg-tts",
                "m2ts" =>  "video/vnd.dlna.mpeg-tts",
                "m2v" =>  "video/mpeg",
                "m3u" =>  "audio/x-mpegurl",
                "m3u8" =>  "audio/x-mpegurl",
                "m4a" =>  "audio/m4a",
                "m4b" =>  "audio/m4b",
                "m4p" =>  "audio/m4p",
                "m4r" =>  "audio/x-m4r",
                "m4v" =>  "video/x-m4v",
                "mac" =>  "image/x-macpaint",
                "mak" =>  "text/plain",
                "man" =>  "application/x-troff-man",
                "manifest" =>  "application/x-ms-manifest",
                "map" =>  "text/plain",
                "master" =>  "application/xml",
                "mda" =>  "application/msaccess",
                "mdb" =>  "application/x-msaccess",
                "mde" =>  "application/msaccess",
                "mdp" =>  "application/octet-stream",
                "me" =>  "application/x-troff-me",
                "mfp" =>  "application/x-shockwave-flash",
                "mht" =>  "message/rfc822",
                "mhtml" =>  "message/rfc822",
                "mid" =>  "audio/mid",
                "midi" =>  "audio/mid",
                "mix" =>  "application/octet-stream",
                "mk" =>  "text/plain",
                "mmf" =>  "application/x-smaf",
                "mno" =>  "text/xml",
                "mny" =>  "application/x-msmoney",
                "mod" =>  "video/mpeg",
                "mov" =>  "video/quicktime",
                "movie" =>  "video/x-sgi-movie",
                "mp2" =>  "video/mpeg",
                "mp2v" =>  "video/mpeg",
                "mp3" =>  "audio/mpeg",
                "mp4" =>  "video/mp4",
                "mp4v" =>  "video/mp4",
                "mpa" =>  "video/mpeg",
                "mpe" =>  "video/mpeg",
                "mpeg" =>  "video/mpeg",
                "mpf" =>  "application/vnd.ms-mediapackage",
                "mpg" =>  "video/mpeg",
                "mpp" =>  "application/vnd.ms-project",
                "mpv2" =>  "video/mpeg",
                "mqv" =>  "video/quicktime",
                "ms" =>  "application/x-troff-ms",
                "msi" =>  "application/octet-stream",
                "mso" =>  "application/octet-stream",
                "mts" =>  "video/vnd.dlna.mpeg-tts",
                "mtx" =>  "application/xml",
                "mvb" =>  "application/x-msmediaview",
                "mvc" =>  "application/x-miva-compiled",
                "mxp" =>  "application/x-mmxp",
                "nc" =>  "application/x-netcdf",
                "nsc" =>  "video/x-ms-asf",
                "nws" =>  "message/rfc822",
                "ocx" =>  "application/octet-stream",
                "oda" =>  "application/oda",
                "odc" =>  "text/x-ms-odc",
                "odh" =>  "text/plain",
                "odl" =>  "text/plain",
                "odp" =>  "application/vnd.oasis.opendocument.presentation",
                "ods" =>  "application/oleobject",
                "odt" =>  "application/vnd.oasis.opendocument.text",
                "one" =>  "application/onenote",
                "onea" =>  "application/onenote",
                "onepkg" =>  "application/onenote",
                "onetmp" =>  "application/onenote",
                "onetoc" =>  "application/onenote",
                "onetoc2" =>  "application/onenote",
                "orderedtest" =>  "application/xml",
                "osdx" =>  "application/opensearchdescription+xml",
                "p10" =>  "application/pkcs10",
                "p12" =>  "application/x-pkcs12",
                "p7b" =>  "application/x-pkcs7-certificates",
                "p7c" =>  "application/pkcs7-mime",
                "p7m" =>  "application/pkcs7-mime",
                "p7r" =>  "application/x-pkcs7-certreqresp",
                "p7s" =>  "application/pkcs7-signature",
                "pbm" =>  "image/x-portable-bitmap",
                "pcast" =>  "application/x-podcast",
                "pct" =>  "image/pict",
                "pcx" =>  "application/octet-stream",
                "pcz" =>  "application/octet-stream",
                "pdf" =>  "application/pdf",
                "pfb" =>  "application/octet-stream",
                "pfm" =>  "application/octet-stream",
                "pfx" =>  "application/x-pkcs12",
                "pgm" =>  "image/x-portable-graymap",
                "pic" =>  "image/pict",
                "pict" =>  "image/pict",
                "pkgdef" =>  "text/plain",
                "pkgundef" =>  "text/plain",
                "pko" =>  "application/vnd.ms-pki.pko",
                "pls" =>  "audio/scpls",
                "pma" =>  "application/x-perfmon",
                "pmc" =>  "application/x-perfmon",
                "pml" =>  "application/x-perfmon",
                "pmr" =>  "application/x-perfmon",
                "pmw" =>  "application/x-perfmon",
                "png" =>  "image/png",
                "pnm" =>  "image/x-portable-anymap",
                "pnt" =>  "image/x-macpaint",
                "pntg" =>  "image/x-macpaint",
                "pnz" =>  "image/png",
                "pot" =>  "application/vnd.ms-powerpoint",
                "potm" =>  "application/vnd.ms-powerpoint.template.macroenabled.12",
                "potx" =>  "application/vnd.openxmlformats-officedocument.presentationml.template",
                "ppa" =>  "application/vnd.ms-powerpoint",
                "ppam" =>  "application/vnd.ms-powerpoint.addin.macroenabled.12",
                "ppm" =>  "image/x-portable-pixmap",
                "pps" =>  "application/vnd.ms-powerpoint",
                "ppsm" =>  "application/vnd.ms-powerpoint.slideshow.macroenabled.12",
                "ppsx" =>  "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
                "ppt" =>  "application/vnd.ms-powerpoint",
                "pptm" =>  "application/vnd.ms-powerpoint.presentation.macroenabled.12",
                "pptx" =>  "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "prf" =>  "application/pics-rules",
                "prm" =>  "application/octet-stream",
                "prx" =>  "application/octet-stream",
                "ps" =>  "application/postscript",
                "psc1" =>  "application/powershell",
                "psd" =>  "application/octet-stream",
                "psess" =>  "application/xml",
                "psm" =>  "application/octet-stream",
                "psp" =>  "application/octet-stream",
                "pub" =>  "application/x-mspublisher",
                "pwz" =>  "application/vnd.ms-powerpoint",
                "qht" =>  "text/x-html-insertion",
                "qhtm" =>  "text/x-html-insertion",
                "qt" =>  "video/quicktime",
                "qti" =>  "image/x-quicktime",
                "qtif" =>  "image/x-quicktime",
                "qtl" =>  "application/x-quicktimeplayer",
                "qxd" =>  "application/octet-stream",
                "ra" =>  "audio/x-pn-realaudio",
                "ram" =>  "audio/x-pn-realaudio",
                "rar" =>  "application/octet-stream",
                "ras" =>  "image/x-cmu-raster",
                "rat" =>  "application/rat-file",
                "rc" =>  "text/plain",
                "rc2" =>  "text/plain",
                "rct" =>  "text/plain",
                "rdlc" =>  "application/xml",
                "resx" =>  "application/xml",
                "rf" =>  "image/vnd.rn-realflash",
                "rgb" =>  "image/x-rgb",
                "rgs" =>  "text/plain",
                "rm" =>  "application/vnd.rn-realmedia",
                "rmi" =>  "audio/mid",
                "rmp" =>  "application/vnd.rn-rn_music_package",
                "roff" =>  "application/x-troff",
                "rpm" =>  "audio/x-pn-realaudio-plugin",
                "rqy" =>  "text/x-ms-rqy",
                "rtf" =>  "application/rtf",
                "rtx" =>  "text/richtext",
                "ruleset" =>  "application/xml",
                "s" =>  "text/plain",
                "safariextz" =>  "application/x-safari-safariextz",
                "scd" =>  "application/x-msschedule",
                "sct" =>  "text/scriptlet",
                "sd2" =>  "audio/x-sd2",
                "sdp" =>  "application/sdp",
                "sea" =>  "application/octet-stream",
                "searchconnector-ms" =>  "application/windows-search-connector+xml",
                "setpay" =>  "application/set-payment-initiation",
                "setreg" =>  "application/set-registration-initiation",
                "settings" =>  "application/xml",
                "sgimb" =>  "application/x-sgimb",
                "sgml" =>  "text/sgml",
                "sh" =>  "application/x-sh",
                "shar" =>  "application/x-shar",
                "shtml" =>  "text/html",
                "sit" =>  "application/x-stuffit",
                "sitemap" =>  "application/xml",
                "skin" =>  "application/xml",
                "sldm" =>  "application/vnd.ms-powerpoint.slide.macroenabled.12",
                "sldx" =>  "application/vnd.openxmlformats-officedocument.presentationml.slide",
                "slk" =>  "application/vnd.ms-excel",
                "sln" =>  "text/plain",
                "slupkg-ms" =>  "application/x-ms-license",
                "smd" =>  "audio/x-smd",
                "smi" =>  "application/octet-stream",
                "smx" =>  "audio/x-smd",
                "smz" =>  "audio/x-smd",
                "snd" =>  "audio/basic",
                "snippet" =>  "application/xml",
                "snp" =>  "application/octet-stream",
                "sol" =>  "text/plain",
                "sor" =>  "text/plain",
                "spc" =>  "application/x-pkcs7-certificates",
                "spl" =>  "application/futuresplash",
                "src" =>  "application/x-wais-source",
                "srf" =>  "text/plain",
                "ssisdeploymentmanifest" =>  "text/xml",
                "ssm" =>  "application/streamingmedia",
                "sst" =>  "application/vnd.ms-pki.certstore",
                "stl" =>  "application/vnd.ms-pki.stl",
                "sv4cpio" =>  "application/x-sv4cpio",
                "sv4crc" =>  "application/x-sv4crc",
                "svc" =>  "application/xml",
                "swf" =>  "application/x-shockwave-flash",
                "t" =>  "application/x-troff",
                "tar" =>  "application/x-tar",
                "tcl" =>  "application/x-tcl",
                "testrunconfig" =>  "application/xml",
                "testsettings" =>  "application/xml",
                "tex" =>  "application/x-tex",
                "texi" =>  "application/x-texinfo",
                "texinfo" =>  "application/x-texinfo",
                "tgz" =>  "application/x-compressed",
                "thmx" =>  "application/vnd.ms-officetheme",
                "thn" =>  "application/octet-stream",
                "tif" =>  "image/tiff",
                "tiff" =>  "image/tiff",
                "tlh" =>  "text/plain",
                "tli" =>  "text/plain",
                "toc" =>  "application/octet-stream",
                "tr" =>  "application/x-troff",
                "trm" =>  "application/x-msterminal",
                "trx" =>  "application/xml",
                "ts" =>  "video/vnd.dlna.mpeg-tts",
                "tsv" =>  "text/tab-separated-values",
                "ttf" =>  "application/octet-stream",
                "tts" =>  "video/vnd.dlna.mpeg-tts",
                "txt" =>  "text/plain",
                "u32" =>  "application/octet-stream",
                "uls" =>  "text/iuls",
                "user" =>  "text/plain",
                "ustar" =>  "application/x-ustar",
                "vb" =>  "text/plain",
                "vbdproj" =>  "text/plain",
                "vbk" =>  "video/mpeg",
                "vbproj" =>  "text/plain",
                "vbs" =>  "text/vbscript",
                "vcf" =>  "text/x-vcard",
                "vcproj" =>  "application/xml",
                "vcs" =>  "text/plain",
                "vcxproj" =>  "application/xml",
                "vddproj" =>  "text/plain",
                "vdp" =>  "text/plain",
                "vdproj" =>  "text/plain",
                "vdx" =>  "application/vnd.ms-visio.viewer",
                "vml" =>  "text/xml",
                "vscontent" =>  "application/xml",
                "vsct" =>  "text/xml",
                "vsd" =>  "application/vnd.visio",
                "vsi" =>  "application/ms-vsi",
                "vsix" =>  "application/vsix",
                "vsixlangpack" =>  "text/xml",
                "vsixmanifest" =>  "text/xml",
                "vsmdi" =>  "application/xml",
                "vspscc" =>  "text/plain",
                "vss" =>  "application/vnd.visio",
                "vsscc" =>  "text/plain",
                "vssettings" =>  "text/xml",
                "vssscc" =>  "text/plain",
                "vst" =>  "application/vnd.visio",
                "vstemplate" =>  "text/xml",
                "vsto" =>  "application/x-ms-vsto",
                "vsw" =>  "application/vnd.visio",
                "vsx" =>  "application/vnd.visio",
                "vtx" =>  "application/vnd.visio",
                "wav" =>  "audio/wav",
                "wave" =>  "audio/wav",
                "wax" =>  "audio/x-ms-wax",
                "wbk" =>  "application/msword",
                "wbmp" =>  "image/vnd.wap.wbmp",
                "wcm" =>  "application/vnd.ms-works",
                "wdb" =>  "application/vnd.ms-works",
                "wdp" =>  "image/vnd.ms-photo",
                "webarchive" =>  "application/x-safari-webarchive",
                "webtest" =>  "application/xml",
                "wiq" =>  "application/xml",
                "wiz" =>  "application/msword",
                "wks" =>  "application/vnd.ms-works",
                "wlmp" =>  "application/wlmoviemaker",
                "wlpginstall" =>  "application/x-wlpg-detect",
                "wlpginstall3" =>  "application/x-wlpg3-detect",
                "wm" =>  "video/x-ms-wm",
                "wma" =>  "audio/x-ms-wma",
                "wmd" =>  "application/x-ms-wmd",
                "wmf" =>  "application/x-msmetafile",
                "wml" =>  "text/vnd.wap.wml",
                "wmlc" =>  "application/vnd.wap.wmlc",
                "wmls" =>  "text/vnd.wap.wmlscript",
                "wmlsc" =>  "application/vnd.wap.wmlscriptc",
                "wmp" =>  "video/x-ms-wmp",
                "wmv" =>  "video/x-ms-wmv",
                "wmx" =>  "video/x-ms-wmx",
                "wmz" =>  "application/x-ms-wmz",
                "wpl" =>  "application/vnd.ms-wpl",
                "wps" =>  "application/vnd.ms-works",
                "wri" =>  "application/x-mswrite",
                "wrl" =>  "x-world/x-vrml",
                "wrz" =>  "x-world/x-vrml",
                "wsc" =>  "text/scriptlet",
                "wsdl" =>  "text/xml",
                "wvx" =>  "video/x-ms-wvx",
                "x" =>  "application/directx",
                "xaf" =>  "x-world/x-vrml",
                "xaml" =>  "application/xaml+xml",
                "xap" =>  "application/x-silverlight-app",
                "xbap" =>  "application/x-ms-xbap",
                "xbm" =>  "image/x-xbitmap",
                "xdr" =>  "text/plain",
                "xht" =>  "application/xhtml+xml",
                "xhtml" =>  "application/xhtml+xml",
                "xla" =>  "application/vnd.ms-excel",
                "xlam" =>  "application/vnd.ms-excel.addin.macroenabled.12",
                "xlc" =>  "application/vnd.ms-excel",
                "xld" =>  "application/vnd.ms-excel",
                "xlk" =>  "application/vnd.ms-excel",
                "xll" =>  "application/vnd.ms-excel",
                "xlm" =>  "application/vnd.ms-excel",
                "xls" =>  "application/vnd.ms-excel",
                "xlsb" =>  "application/vnd.ms-excel.sheet.binary.macroenabled.12",
                "xlsm" =>  "application/vnd.ms-excel.sheet.macroenabled.12",
                "xlsx" =>  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xlt" =>  "application/vnd.ms-excel",
                "xltm" =>  "application/vnd.ms-excel.template.macroenabled.12",
                "xltx" =>  "application/vnd.openxmlformats-officedocument.spreadsheetml.template",
                "xlw" =>  "application/vnd.ms-excel",
                "xml" =>  "text/xml",
                "xmta" =>  "application/xml",
                "xof" =>  "x-world/x-vrml",
                "xoml" =>  "text/plain",
                "xpm" =>  "image/x-xpixmap",
                "xps" =>  "application/vnd.ms-xpsdocument",
                "xrm-ms" =>  "text/xml",
                "xsc" =>  "application/xml",
                "xsd" =>  "text/xml",
                "xsf" =>  "text/xml",
                "xsl" =>  "text/xml",
                "xslt" =>  "text/xml",
                "xsn" =>  "application/octet-stream",
                "xss" =>  "application/xml",
                "xtp" =>  "application/octet-stream",
                "xwd" =>  "image/x-xwindowdump",
                "z" =>  "application/x-compress",
                "zip" =>  "application/x-zip-compressed",
                #endregion
                _ => "application/octet-stream"
            };

        }

        public static string TryExtentionFromMimeType(string mime)
        {
           
            return mime.ToLower() switch
            {
                #region Big freaking list of mime types
                "audio/audible" => "aa",
                "audio/aac" => "aac",
                "audio/vnd.audible.aax" => "aax",
                "audio/ac3" => "ac3",
                "application/msaccess" => "accdb",
                "application/msaccess.addin" => "accda",
                "application/msaccess.cab" => "accdc",
                "application/msaccess.runtime" => "accdr",
                "application/msaccess.webapplication" => "accdw",
                "application/msaccess.ftemplate" => "accft",
                "application/internet-property-stream" =>  "acx",
                "text/xml" =>  "xml",
                "application/x-bridge-url" =>  "adobebridge",
                "audio/vnd.dlna.adts" =>  "adt",
                "audio/x-aiff" =>  "aif",
                "audio/aiff" =>  "aiff",
                "application/vnd.adobe.air-application-installer-package+zip" =>  "air",
                "application/x-mpeg" =>  "amc",
                "application/x-ms-application" =>  "application",
                "image/x-jg" =>  "art",
                "application/xml" =>  "xml",
                "video/x-ms-asf" =>  "asf",
                "application/octet-stream" =>  "asi",
                "text/plain" =>  "txt",
                "application/atom+xml" =>  "atom",
                "audio/basic" =>  "au",
                "video/x-msvideo" =>  "avi",
                "application/olescript" =>  "axs",
                "application/x-bcpio" =>  "bcpio",
                "image/bmp" =>  "bmp",
                "audio/x-caf" =>  "caf",
                "application/vnd.ms-office.calx" =>  "calx",
                "application/vnd.ms-pki.seccat" =>  "cat",
                "application/x-cdf" =>  "cdf",
                "application/x-x509-ca-cert" =>  "cer",
                "application/x-java-applet" =>  "class",
                "application/x-msclip" =>  "clp",
                "image/x-cmx" =>  "cmx",
                "image/cis-cod" =>  "cod",
                "text/x-ms-contact" =>  "contact",
                "application/x-cpio" =>  "cpio",
                "application/x-mscardfile" =>  "crd",
                "application/pkix-crl" =>  "crl",
                "application/x-csh" =>  "csh",
                "text/css" =>  "css",
                "text/csv" =>  "csv",
                "application/x-director" =>  "dcr",
                "video/x-dv" =>  "dif",
                "application/x-msdownload" =>  "dll",
                "text/dlm" =>  "dlm",
                "application/msword" =>  "doc",
                "application/vnd.ms-word.document.macroenabled.12" =>  "docm",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" =>  "docx",
                "application/vnd.ms-word.template.macroenabled.12" =>  "dotm",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.template" =>  "dotx",
                "application/x-dvi" =>  "dvi",
                "drawing/x-dwf" =>  "dwf",
                "message/rfc822" =>  "eml",
                "application/etl" =>  "etl",
                "text/x-setext" =>  "etx",
                "application/envoy" =>  "evy",
                "application/vnd.fdf" =>  "fdf",
                "application/fractals" =>  "fif",
                "x-world/x-vrml" =>  "flr",
                "video/x-flv" =>  "flv",
                "application/fsharp-script" =>  "fsscript",
                "image/gif" =>  "gif",
                "text/x-ms-group" =>  "group",
                "audio/x-gsm" =>  "gsm",
                "application/x-gtar" =>  "gtar",
                "application/x-gzip" =>  "gz",
                "application/x-hdf" =>  "hdf",
                "text/x-hdml" =>  "hdml",
                "application/x-oleobject" =>  "hhc",
                "application/winhlp" =>  "hlp",
                "application/mac-binhex40" =>  "hqx",
                "application/hta" =>  "hta",
                "text/x-component" =>  "htc",
                "text/html" =>  "html",
                "text/webviewhtml" =>  "htt",
                "image/x-icon" =>  "ico",
                "image/ief" =>  "ief",
                "application/x-iphone" =>  "iii",
                "application/x-internet-signup" =>  "ins",
                "application/x-itunes-ipa" =>  "ipa",
                "application/x-itunes-ipg" =>  "ipg",
                "application/x-itunes-ipsw" =>  "ipsw",
                "text/x-ms-iqy" =>  "iqy",
                "application/x-itunes-ite" =>  "ite",
                "application/x-itunes-itlp" =>  "itlp",
                "application/x-itunes-itms" =>  "itms",
                "application/x-itunes-itpc" =>  "itpc",
                "video/x-ivf" =>  "ivf",
                "application/java-archive" =>  "jar",
                "application/liquidmotion" =>  "jck",
                "image/pjpeg" =>  "jfif",
                "application/x-java-jnlp-file" =>  "jnlp",
                "image/jpeg" =>  "jpeg",
                "image/jpg" =>  "jpg",
                "application/x-javascript" =>  "js",
                "text/jscript" =>  "jsx",
                "application/x-latex" =>  "latex",
                "application/windows-library+xml" =>  "library-ms",
                "application/x-ms-reader" =>  "lit",
                "video/x-la-asf" =>  "lsf",
                "application/x-msmediaview" =>  "m14",
                "video/vnd.dlna.mpeg-tts" =>  "m2t",
                "audio/x-mpegurl" =>  "m3u",
                "audio/m4a" =>  "m4a",
                "audio/m4b" =>  "m4b",
                "audio/m4p" =>  "m4p",
                "audio/x-m4r" =>  "m4r",
                "video/x-m4v" =>  "m4v",
                "image/x-macpaint" =>  "mac",
                "application/x-troff-man" =>  "man",
                "application/x-ms-manifest" =>  "manifest",
                "application/x-msaccess" =>  "mdb",
                "application/x-troff-me" =>  "me",
                "text/mhtml" =>  "mhtml",
                "audio/mid" =>  "mid",
                "application/x-smaf" =>  "mmf",
                "application/x-msmoney" =>  "mny",
                "video/quicktime" =>  "mov",
                "video/x-sgi-movie" =>  "movie",
                "audio/mpeg" =>  "mp3",
                "video/mp4" =>  "mp4",
                "application/vnd.ms-mediapackage" =>  "mpf",
                "video/mpeg" =>  "mpg",
                "application/vnd.ms-project" =>  "mpp",
                "application/x-troff-ms" =>  "ms",
                "application/x-miva-compiled" =>  "mvc",
                "application/x-mmxp" =>  "mxp",
                "application/x-netcdf" =>  "nc",
                "application/oda" =>  "oda",
                "text/x-ms-odc" =>  "odc",
                "application/vnd.oasis.opendocument.presentation" =>  "odp",
                "application/oleobject" =>  "ods",
                "application/vnd.oasis.opendocument.text" =>  "odt",
                "application/onenote" =>  "one",
                "application/opensearchdescription+xml" =>  "osdx",
                "application/pkcs10" =>  "p10",
                "application/x-pkcs7-certificates" =>  "p7b",
                "application/pkcs7-mime" =>  "p7c",
                "application/x-pkcs7-certreqresp" =>  "p7r",
                "application/pkcs7-signature" =>  "p7s",
                "image/x-portable-bitmap" =>  "pbm",
                "application/x-podcast" =>  "pcast",
                "image/pict" =>  "pct",
                "application/pdf" =>  "pdf",
                "application/x-pkcs12" =>  "pfx",
                "image/x-portable-graymap" =>  "pgm",
                "application/vnd.ms-pki.pko" =>  "pko",
                "audio/scpls" =>  "pls",
                "application/x-perfmon" =>  "pma",
                "image/png" =>  "png",
                "image/x-portable-anymap" =>  "pnm",
                "application/vnd.ms-powerpoint.template.macroenabled.12" =>  "potm",
                "application/vnd.openxmlformats-officedocument.presentationml.template" =>  "potx",
                "application/vnd.ms-powerpoint.addin.macroenabled.12" =>  "ppam",
                "image/x-portable-pixmap" =>  "ppm",
                "application/vnd.ms-powerpoint.slideshow.macroenabled.12" =>  "ppsm",
                "application/vnd.openxmlformats-officedocument.presentationml.slideshow" =>  "ppsx",
                "application/vnd.ms-powerpoint" =>  "ppt",
                "application/vnd.ms-powerpoint.presentation.macroenabled.12" =>  "pptm",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" =>  "pptx",
                "application/pics-rules" =>  "prf",
                "application/postscript" =>  "ps",
                "application/powershell" =>  "psc1",
                "application/x-mspublisher" =>  "pub",
                "text/x-html-insertion" =>  "qhtm",
                "application/x-quicktimeplayer" =>  "qtl",
                "audio/x-pn-realaudio" =>  "ram",
                "image/x-cmu-raster" =>  "ras",
                "application/rat-file" =>  "rat",
                "image/vnd.rn-realflash" =>  "rf",
                "image/x-rgb" =>  "rgb",
                "application/vnd.rn-realmedia" =>  "rm",
                "application/vnd.rn-rn_music_package" =>  "rmp",
                "application/x-troff" =>  "roff",
                "audio/x-pn-realaudio-plugin" =>  "rpm",
                "text/x-ms-rqy" =>  "rqy",
                "application/rtf" =>  "rtf",
                "text/richtext" =>  "rtx",
                "application/x-safari-safariextz" =>  "safariextz",
                "application/x-msschedule" =>  "scd",
                "text/scriptlet" =>  "sct",
                "audio/x-sd2" =>  "sd2",
                "application/sdp" =>  "sdp",
                "application/windows-search-connector+xml" =>  "searchconnector-ms",
                "application/set-payment-initiation" =>  "setpay",
                "application/set-registration-initiation" =>  "setreg",
                "application/x-sgimb" =>  "sgimb",
                "text/sgml" =>  "sgml",
                "application/x-sh" =>  "sh",
                "application/x-shar" =>  "shar",
                "application/x-stuffit" =>  "sit",
                "application/vnd.ms-powerpoint.slide.macroenabled.12" =>  "sldm",
                "application/vnd.openxmlformats-officedocument.presentationml.slide" =>  "sldx",
                "application/x-ms-license" =>  "slupkg-ms",
                "audio/x-smd" =>  "smd",
                "application/futuresplash" =>  "spl",
                "application/x-wais-source" =>  "src",
                "application/streamingmedia" =>  "ssm",
                "application/vnd.ms-pki.certstore" =>  "sst",
                "application/vnd.ms-pki.stl" =>  "stl",
                "application/x-sv4cpio" =>  "sv4cpio",
                "application/x-sv4crc" =>  "sv4crc",
                "application/x-shockwave-flash" =>  "swf",
                "application/x-tar" =>  "tar",
                "application/x-tcl" =>  "tcl",
                "application/x-tex" =>  "tex",
                "application/x-texinfo" =>  "texi",
                "application/x-compressed" =>  "tgz",
                "application/vnd.ms-officetheme" =>  "thmx",
                "image/tiff" =>  "tiff",
                "application/x-msterminal" =>  "trm",
                "text/tab-separated-values" =>  "tsv",
                "text/iuls" =>  "uls",
                "application/x-ustar" =>  "ustar",
                "text/vbscript" =>  "vbs",
                "text/x-vcard" =>  "vcf",
                "application/vnd.ms-visio.viewer" =>  "vdx",
                "application/vnd.visio" =>  "vsd",
                "application/ms-vsi" =>  "vsi",
                "application/vsix" =>  "vsix",
                "application/x-ms-vsto" =>  "vsto",
                "audio/wav" =>  "wav",
                "audio/x-ms-wax" =>  "wax",
                "image/vnd.wap.wbmp" =>  "wbmp",
                "image/vnd.ms-photo" =>  "wdp",
                "application/x-safari-webarchive" =>  "webarchive",
                "application/vnd.ms-works" =>  "wks",
                "application/wlmoviemaker" =>  "wlmp",
                "application/x-wlpg-detect" =>  "wlpginstall",
                "application/x-wlpg3-detect" =>  "wlpginstall3",
                "video/x-ms-wm" =>  "wm",
                "audio/x-ms-wma" =>  "wma",
                "application/x-ms-wmd" =>  "wmd",
                "application/x-msmetafile" =>  "wmf",
                "text/vnd.wap.wml" =>  "wml",
                "application/vnd.wap.wmlc" =>  "wmlc",
                "text/vnd.wap.wmlscript" =>  "wmls",
                "application/vnd.wap.wmlscriptc" =>  "wmlsc",
                "video/x-ms-wmp" =>  "wmp",
                "video/x-ms-wmv" =>  "wmv",
                "video/x-ms-wmx" =>  "wmx",
                "application/x-ms-wmz" =>  "wmz",
                "application/vnd.ms-wpl" =>  "wpl",
                "application/x-mswrite" =>  "wri",
                "video/x-ms-wvx" =>  "wvx",
                "application/directx" =>  "x",
                "application/xaml+xml" =>  "xaml",
                "application/x-silverlight-app" =>  "xap",
                "application/x-ms-xbap" =>  "xbap",
                "image/x-xbitmap" =>  "xbm",
                "application/xhtml+xml" =>  "xhtml",
                "application/vnd.ms-excel.addin.macroenabled.12" =>  "xlam",
                "application/vnd.ms-excel" =>  "xls",
                "application/vnd.ms-excel.sheet.binary.macroenabled.12" =>  "xlsb",
                "application/vnd.ms-excel.sheet.macroenabled.12" =>  "xlsm",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" =>  "xlsx",
                "application/vnd.ms-excel.template.macroenabled.12" =>  "xltm",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.template" =>  "xltx",
                "image/x-xpixmap" =>  "xpm",
                "application/vnd.ms-xpsdocument" =>  "xps",
                "image/x-xwindowdump" =>  "xwd",
                "application/x-compress" =>  "z",
                "application/x-zip-compressed" =>  "zip",
                #endregion
                _ => "application/octet-stream"
            };


        }

        public static bool IsStaticResource(string resource)
        {
            return Regex.IsMatch(resource, @"(.*?)\.(ico|css|gif|jpg|jpeg|png|js|xml|ttf)$");
        }

        public static bool IsDynamicResource(string resource)
        {
            return Regex.IsMatch(resource, @"(.*?)\.(htm|html|xhtml|mhtml|dhtml)$");
        }

        public static bool IsDotNetResource(string resource)
        {
            return Regex.IsMatch(resource, @"(.*?)\.(aspx|ascx)$");
        }

        public static byte[] CompressGZIP(byte[] data)
        {

            MemoryStream streamoutput = new();
            GZipStream gzip = new(streamoutput, CompressionMode.Compress, false);

            gzip.Write(data, 0, data.Length);
            gzip.Close();

            return streamoutput.ToArray();

        }

        public static string CleanJsonString(string url)
        {
            if (url == null)
                return null;

            return url.Replace("%27", "\"").Replace("%35", "#").Replace("%20", " ").Replace("%61", "=").Replace("%63", "?");

        }

        public static string RemoveToken(string completeReq)
        {
            if (completeReq.Contains("__"))
                completeReq = completeReq.Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries)[0];

            string[] _pReq = completeReq.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);

            if (_pReq.Length < 2)
                return string.Empty;
            else
                return _pReq[1];

        }

        public static bool SendAsyncResponce(string message, string returnUrl, OMimeType contentType, out string response)
        {

            response = string.Empty;

            StreamWriter sw;
            WebRequest wr = WebRequest.Create(returnUrl);
            wr.ContentType = GetStringMimeType(contentType);
            wr.Method = "POST";

            try
            {
                sw = new StreamWriter(wr.GetRequestStream());
                sw.Write(message);
            }
            catch
            {
                return false;
            }

            sw?.Close();

            try
            {
                WebResponse wp = wr?.GetResponse();
                StreamReader rr = new(wp?.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                response = rr?.ReadToEnd();
            }
            catch
            {
                return false;
            }

            return true;

        }

        public static WebResponse SendAsyncRequest(string message, string returnUrl, OMimeType contentType, string method, out string response)
        {

            StreamWriter sw;
            WebRequest wr = WebRequest.Create(returnUrl);
            wr.ContentType = GetStringMimeType(contentType);
            wr.Method = method;

            try
            {
                sw = new StreamWriter(wr.GetRequestStream());
                sw.Write(message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return null;
            }

            sw?.Close();

            WebResponse wp;

            try
            {
                wp = wr?.GetResponse();
                StreamReader rr = new(wp?.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                response = rr?.ReadToEnd();
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return null;
            }

            return wp;

        }

        public static string GetXmlBuilderTemplate(string message)
        {

            StringBuilder result = new();

            result.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            result.Append("<builder version=\"2.3\">");
            result.Append("<message>");
            result.Append(message);
            result.Append("</message>");
            result.Append("{0}");
            result.Append("</builder>");

            return result.ToString();

        }
       
        public static string GetErrorXmlResponse(string messsage)
        {

            string result = GetXmlBuilderTemplate(messsage);

            result = result.Replace("{0}", string.Empty);

            return result;

        }

        public static string GetOKXmlResponse(string messsage, string instanceId, string serviceId)
        {

            string result = GetXmlBuilderTemplate(messsage);

            StringBuilder innerXml = new();

            innerXml.Append("<instance>");
            innerXml.Append(instanceId);
            innerXml.Append("</instance>");
            innerXml.Append("<service>");
            innerXml.Append(serviceId);
            innerXml.Append("</service>");

            result = string.Format(result, innerXml.ToString());

            return result;

        }

        public static JObject GetJsonBuilderTemplate(string message)
        {

            JObject j = new(
                new JProperty("builder", 
                    new JObject(
                        new JProperty("version", "2.3"),
                        new JProperty("message", message)
                    )
                )
            );

            return j;

        }

        public static JObject GetOKJsonResponse(string messsage, string instanceId, string serviceId)
        {

            JObject result  = GetJsonBuilderTemplate(messsage);
          
            JObject builder = (JObject)result.Properties().Where(p => p.Name == "builder").FirstOrDefault().Value;

            _ = builder.Properties().Append(new JProperty("instance", instanceId));
            _ = builder.Properties().Append(new JProperty("service", serviceId));

            return result;

        }


    }
}
