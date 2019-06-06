using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class eventIndicators : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        

        public void WriteForm()
        {
            int nSurveyID = int.Parse(Request["survey"]);
            int nEventID = int.Parse(Request["id"]);

            events e = new events();
            SurveyInfo info = e.GetSurveyInfo(nSurveyID);
            int nSurveyTypeID = info.fSurveyTypeRef;

            List<Indicator> list = e.LoadIndicators(nSurveyTypeID, nEventID, true);
            for (int i = 0; i < list.Count; i++)
            {
                Indicator indicator = list[i];

                Response.Write("<table style='width: 100%'>");
                Response.Write("<tr>");
                Response.Write("<td colspan=2>");
                Response.Write("<strong>" + indicator.name + "</strong>");
                Response.Write("</td>");
                Response.Write("</tr>");

                Response.Write("<tr>");
                Response.Write("<td>");
                Response.Write("<input id='ff" + indicator.id + "' class='form-control' type='text' value='" + Server.HtmlEncode(indicator.value) + "' />");
                Response.Write("</td>");

                Response.Write("<td style='width: 60px;'>");
                Response.Write("&nbsp;" + indicator.abbr);
                Response.Write("</td>");

                Response.Write("</tr>");

                Response.Write("<tr>");
                Response.Write("<td colspan=2>");
                Response.Write("&nbsp;");
                Response.Write("</td>");
                Response.Write("</tr>");


                Response.Write("</table>");
            }

            Response.Write("\n\n<script>\n");
            Response.Write("var indicators = [\n");
            for (int i = 0; i < list.Count; i++)
            {
                Indicator indicator = list[i];
                if (i > 0)
                    Response.Write(", ");
                Response.Write("{");
                Response.Write("'id': " + indicator.id);
                Response.Write("}\n");

            }

            Response.Write("];\n\n");


            Response.Write("</script>\n\n");





            


        }




 

    }



}