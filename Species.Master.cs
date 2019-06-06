using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class Species : System.Web.UI.MasterPage
    {
        public String loadString = "";
        public bool loggedIn = false;


        protected void Page_Load(object sender, EventArgs e)
        {
            String usr = Request["usr"];
            String psw = Request["psw"];
            if (usr != null && usr != "")
            {
                

                if (usr.IndexOf("-logout-") != -1)
                {
                    Session["loggedin"] = null;
                    Response.Redirect(".");
                }
                else
                {
                    if (usr != "admin" || psw != "e010ea1272e498197646d6585e41200f")
                    {
//                        loadString = "Invalid username or password";
                    }
                    else
                    {
                        Session["loggedin"] = true;
                    }
                }
            }

            if (Session["loggedin"] != null)
                loggedIn = true;

            if (loggedIn == false)
            {
                String page = this.Page.ToString();
                if (page != "ASP.default_aspx" && page != "ASP.map_aspx")
                {
//                    Response.Write(page);
//                    Response.Redirect(".");
                }
            }
                    


        }

        public void writeVesselList()
        {
            Response.Write("<option value='0'>All</option>");
            /*
            String query = String.Format("SELECT * FROM Vessel ORDER BY Name");
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Response.Write("<option value='" + set["VesselID"].ToString() + "'>" + Server.HtmlEncode(set["Name"].ToString()) + "</option>");
                        }
                    }
                }
            }
             */
        }

        public void writeStationList()
        {
            String query = "SELECT * FROM TblStation ORDER BY fStationName";
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            String commonName = set["fStationName"].ToString();
                            Response.Write("<option value='" + set["fStationID"].ToString() + "'>" + Server.HtmlEncode(commonName) + "</option>");
                        }
                    }
                }
            }
        }

        public void writeSpeciesList()
        {
            String query = "SELECT * FROM TblSpecies ORDER BY fCommonName";
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            String commonName = set["fCommonName"].ToString();
                            Response.Write("<option value='" + set["fSpeciesID"].ToString() + "'>" + Server.HtmlEncode(commonName) + "</option>");
                        }
                    }
                }
            }
        }


        public void WriteRanks()
        {
            String sql = "SELECT * FROM TblTaxonRank WHERE fTaxonRankID <> 1 AND fTaxonRankID <> 11 ORDER BY fTaxonRankID";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Response.Write("<option value='" + set["fTaxonRankID"].ToString() + "'>" + Server.HtmlEncode(set["fTaxonRankName"].ToString()) + "</option>");

                        }
                    }
                } 
            }
        }



    }
}