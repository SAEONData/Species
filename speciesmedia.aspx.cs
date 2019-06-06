using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class speciesmedia : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void WriteMedia()
        {
            int speciesID = int.Parse(Request["id"]);
            bool defview = false;
            if (Request["df"] == "1")
                defview = true;


            String sql = "SELECT * FROM vwSpeciesMedia WHERE fSpeciesID = " + speciesID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                { 
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            int fMediaID = (int)set["fMediaID"];
                            int fSpeciesMediaID = (int)set["fSpeciesMediaID"];
                            int fCatalogueOrder = (int)set["fCatalogueOrder"];
                            string itemSelected = fCatalogueOrder > 0 ? " checked " : "";


                            String url = set["fMediaPath"].ToString();
                            if (url.IndexOf("http") != 0)
                            {
                                url = HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath + '/' + set["fMediaPath"].ToString();
                                url = url.Replace("//", "/");
                                if (url.IndexOf("http://") == -1)
                                    url = "http://" + url;
                            }

                            if (defview == false)
                            {
                                Response.Write("<label><input " + itemSelected + " id='sm_" + fSpeciesMediaID + "' type='checkbox' onclick='updateCat(this.id, this.checked);'>Include in catalog</label>");
                                Response.Write("<br />");
                                Response.Write("<iframe src='photobox.aspx?id=" + fMediaID + "&photo=" + Server.UrlEncode(url) + "&species=" + speciesID + "' style='width: 100%; height: 460px; border: 0px solid #f0f0f0'></iframe>");
                            }
                            else
                            {
                                Response.Write("<img src='ic.aspx?url=" + Server.UrlEncode(url) + "' style='width: 540px' />");
                            }


                            
                            // photobox.aspx?id=8864&photo=http://media.dirisa.org/inventory/upload/egagasini/species/Invertebrate%20Trawl%20Species%20Database/trawl-surveys-final/africana-v270-jan-2011/africana-v270-trawl-photos/invert-photos-from-rob/2015-12-08-10-44-40-274270-gmt-2
                            

                            Response.Write("<br />");
                            Response.Write("<br />");

                        }
                    }
                }
            }



        }
    }
}