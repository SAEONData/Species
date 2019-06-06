using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class map : System.Web.UI.Page
    {
        public int vesselID = -1;
        public int cruiseID = -1;
        public bool loggedIn = false;


        public String pageTitle = "";
        public double maxTransectLength = 2;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public String getDataPoints()
        {
            return "[]";
        }

        public bool isAdminLoggedIn()
        {
            return false;
        }



    }

    public class DataObservation
    {
        public int id;
        public int species;
        public string sname;

        public int sitting;
        public int flying;
        public string color;

    }



    public class DataPoint : IComparable
    {
        public string name;
        public string shape;
        public double x;
        public double y;
        public int transect;
        public List<DataObservation> obs = new List<DataObservation>();
        public double x1;
        public double y1;
        public double x2;
        public double y2;
        public double len;
        public int id;

        public int CompareTo(object obj)
        {
            DataPoint pnt = (DataPoint)obj;
            if (pnt == null || pnt.name == null || name == null)
                return 0;
            return name.CompareTo(pnt.name);
        }
    };

}