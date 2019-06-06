using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace species
{
    public class global
    {
        public static bool IsLocalHost()
        {
            return HttpContext.Current.Request.IsLocal; 
        }

        public static string getGoogleMapsAPISource()
        {
            String source = "https://maps.googleapis.com/maps/api/js";
            if (IsLocalHost() == true)
                source += "?key=AIzaSyBd07P0gb_cNMvMC74VMbNFy9ogx7iFkF4";
            return source;
        }
    }
}