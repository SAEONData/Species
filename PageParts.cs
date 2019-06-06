using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace species
{
    public class PageParts
    {
        static void WriteHead(HttpResponse resp)
        {
            resp.Write("    <meta name='viewport' content='width=device-width, initial-scale=1.0' />\r\n");
            resp.Write("    <link href='Content/bootstrap.min.css' rel='stylesheet' />\r\n");
            resp.Write("    <link href='Content/bootstrap-theme.css' rel='stylesheet' />\r\n");
            resp.Write("    <script type='text/javascript' src='scripts/jquery-2.1.1.min.js'></script>\r\n");
            resp.Write("    <script type='text/javascript' src='scripts/bootstrap.min.js'></script>\r\n");
        }
    }
}