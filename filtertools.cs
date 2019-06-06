using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace species
{
    public class filtertools
    {
        public static String BuildFilter(HttpContext cnt)
        {
            String filter = "";

            if (cnt.Request["date1"] != null && cnt.Request["date1"] != "")
                filter += " AND fStartDate >= '" + cnt.Request["date1"] + "'";

            if (cnt.Request["date2"] != null && cnt.Request["date2"] != "")
                filter += " AND fStartDate <= '" + cnt.Request["date2"] + "'";

            if (cnt.Request["dep1"] != null && cnt.Request["dep1"] != "")
                filter += " AND fDepth >= " + cnt.Request["dep1"] + "";

            if (cnt.Request["dep2"] != null && cnt.Request["dep2"] != "")
                filter += " AND fDepth <= " + cnt.Request["dep2"] + "";
            


            if (cnt.Request["spatfilter"] == "within")
            {
                filter += " AND fStartLng >= '" + cnt.Request["x1"] + "'";
                filter += " AND fStartLat >= '" + cnt.Request["y1"] + "'";
                filter += " AND fStartLng <= '" + cnt.Request["x2"] + "'";
                filter += " AND fStartLat <= '" + cnt.Request["y2"] + "'";
            }

            if (cnt.Request["months"] != null)
            {
                String[] months = cnt.Request["months"].Split(',');
                String qmonth = "";
                foreach (String month in months)
                {
                    if (month.Trim() != "")
                    {
                        if (qmonth != "")
                            qmonth += " OR ";
                        qmonth += "DATEPART(MONTH, Date) = " + int.Parse(month);
                    }
                }
                if (qmonth != "")
                    filter += " AND (" + qmonth + ")";
                else
                    filter += " AND (1 = 0)";
            }

            if (cnt.Request["hours"] != null )
            {
                String[] hours = cnt.Request["hours"].Split(',');
                String qhour = "";
                foreach (String hour in hours)
                {
                    if (hour.Trim() != "")
                    {
                        if (qhour != "")
                            qhour += " OR ";
                        qhour += "DATEPART(HOUR, TimeStart) = " + int.Parse(hour);
                    }
                }
                if (qhour != "")
                    filter += " AND (" + qhour + ")";
                else
                    filter += " AND (1 = 0)";
            }



            return filter;
        }
    }
}