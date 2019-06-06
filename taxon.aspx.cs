using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class taxon : System.Web.UI.Page
    {
        public string find = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            find = Request["find"];



        }
    }


}