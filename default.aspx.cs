using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Species";
            list.table = "TblSpecies";
            list.idField = "fSpeciesID";
            list.staticList = true;

            list.customViewDialog = true;
            list.customEditDialog = true;
            list.filterDelegate = TaxonFilter;

            list.filterFields.Add("fCommonName");
            list.filterFields.Add("fSpeciesName");
            list.filterFields.Add("fTaxonomy"); 
            list.showFilterFields = false;

            list.filterDelegate = SpeciesFilter;
            list.exportButton = false;

            list.speciesQuery = true;


            // list.rowDescriptorDelegate = GetSpeciesDescription;

            list.fields.Add(new Field("", "Image", FieldType.Image, 80, 100, "", GetSpeciesImage));
            list.fields.Add(new Field("fCommonName", "Common Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fSpeciesName", "Species Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fFeatures", "Features", FieldType.String, 80, 0));
            list.fields.Add(new Field("fColour", "Colour", FieldType.String, 80, 0));
            list.fields.Add(new Field("fSize", "Size", FieldType.String, 80, 0));
            list.fields.Add(new Field("fDistribution", "Distribution", FieldType.Paragraph, 80, 0));
            list.fields.Add(new Field("fReferences", "References", FieldType.Paragraph, 80, 0));
            list.fields.Add(new Field("fNotes", "Notes", FieldType.String, 80, 0));
            list.fields.Add(new Field("fSimilar", "Similar", FieldType.String, 80, 0));
            list.fields.Add(new Field("fLSID", "fLSID", FieldType.String, 80, 0));
            list.fields.Add(new Field("fWoRMID", "fWoRMID", FieldType.String, 80, 0));


            list.orderField = "fCommonName";

            list.InitList(Context);
        }

       


        public String GetSpeciesDescription(SqlDataReader set)
        {
            int species = int.Parse(set["fSpeciesID"].ToString());
            int worms = int.Parse(set["fWoRMID"].ToString());
            String code = "<table>";
            code += "<tr>";
            code += "<td>WORMS:&nbsp;</td><td><a target='_blank' href='http://www.marinespecies.org/aphia.php?p=taxdetails&id=" + worms + "'>http://www.marinespecies.org/aphia.php?p=taxdetails&id=" + worms + "</a></td>";
            code += "</tr>";
            code += "<tr>";
            code += "<td>LSID</td><td>urn:lsid:marinespecies.org:taxname:" + worms + "</td>";
            code += "</tr>";
            code += "</table>";
            return code;
        }

        bool SpeciesFilter(int id, string filter)
        {
            if (filter != "")
            {
                species sp = new species();
                return sp.TaxonFilter(id, filter);
            }
            
            bool image = false;
            String query = String.Format("SELECT TOP (1) dbo.TblMedia.fMediaName, dbo.TblMedia.fMediaPath FROM dbo.TblSpeciesMedia INNER JOIN dbo.TblMedia ON dbo.TblSpeciesMedia.fMediaID = dbo.TblMedia.fMediaID INNER JOIN dbo.TblSpecies ON dbo.TblSpeciesMedia.fSpeciesID = dbo.TblSpecies.fSpeciesID WHERE (dbo.TblSpeciesMedia.fSpeciesID = {0}) AND (dbo.TblSpecies.fWoRMID <> 0 AND TblSpecies.fCommonName != '' AND TblSpecies.fCommonName != TblSpecies.fSpeciesName)", id);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                            image = true;
                    }
                }
            }
            return image;
        }



        public String GetSpeciesImage(int id)
        {
            String path = "";
            String query = "SELECT TOP 1 TblMedia.fMediaName, TblMedia.fMediaPath FROM TblSpeciesMedia INNER JOIN TblMedia ON TblSpeciesMedia.fMediaID = TblMedia.fMediaID WHERE TblSpeciesMedia.fSpeciesID = " + id;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                            path = set["fMediaPath"].ToString();
                    }
                }
            }
            return path;
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

        int getSpeciesTaxonID(int id)
        {
            int nTaxonID = 0;
            String sql = "SELECT fTaxonomyID FROM TblSpecies WHERE fSpeciesID = " + id;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("Invalid species id");
                            Response.End();
                        }
                        if (set["fTaxonomyID"] != DBNull.Value)
                            nTaxonID = (int)set["fTaxonomyID"];
                    }
                }
            }
            return nTaxonID;
        }

        bool FindTaxon(int nTaxonID, int nSelectedTaxon)
        {
            String sql = "SELECT fTaxonomyParentID FROM TblTaxonomy WHERE fTaxonomyID = " + nTaxonID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("Invalid taxon id");
                            Response.End();
                        }

                        nTaxonID = int.Parse(set["fTaxonomyParentID"].ToString());
                        if (nTaxonID == nSelectedTaxon)
                            return true;
                        if (nTaxonID == 0)
                            return false;
                        if (FindTaxon(nTaxonID, nSelectedTaxon) == true)
                            return true;
                    }
                }
            }
            return false;
        }

        bool TaxonFilter(int id, string filter)
        {
            if (filter == null || filter == "")
                filter = "0";
            int taxon = int.Parse(filter);
            if (taxon < 1)
                return true;

            int nParentTaxon = getSpeciesTaxonID(id);
            return FindTaxon(nParentTaxon, taxon);
        }




        public GenericList list;
    }
}

/**

Hatch shell whelk-yellow foot, Athleta abyssicola, Athleta, Athletinae, Volutidae, Muricoidea, Neogastropoda, Caenogastropoda, Gastropoda, Mollusca, Animalia, Biota

**/