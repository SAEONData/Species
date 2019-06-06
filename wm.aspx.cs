using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class wm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String name = "wm.xlsx"; 
            String path = Server.MapPath(name);

            string con = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path  + "; Extended Properties='Excel 8.0;HDR=Yes;'";
             // con = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0;";
             using (OleDbConnection connection = new OleDbConnection(con))
             {
                 connection.Open();
                 OleDbCommand command = new OleDbCommand("select * from [Sheet1]", connection); 
                 using (OleDbDataReader dr = command.ExecuteReader())
                 {
                     while (dr.Read())
                     {
                         string species = dr[0].ToString();
                         String aphiaID = dr[1].ToString();
                         if (species.Trim() == "")
                             return;

                         Response.Write(species + "=" + aphiaID + "<br>");
                         Response.Flush();
                     }
                 }
             }

        }



    }
}