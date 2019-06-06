using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class species : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Species";
            list.table = "TblSpecies";
            list.idField = "fSpeciesID";
            list.customViewDialog = true;
            list.customEditDialog = true;
            list.filterDelegate = TaxonFilter;
            list.orderField = "fCommonName";
            list.exportButton = true;


            list.fields.Add(new Field("fSpeciesID", "Species ID", FieldType.Hidden, 0, 80));
//             list.fields.Add(new Field("fTaxonomyID", "Taxonomy ID", FieldType.Taxon, 80, 0));
            list.fields.Add(new Field("fCommonName", "Common Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fSpeciesName", "Species Name", FieldType.String, 80, 100));

            list.fields.Add(new Field("fFishBoard", "Fish Board Code", FieldType.String, 80, 0));
            
            list.fields.Add(new Field("fWoRMID", "WoRMS ID", FieldType.ReadOnly, 80, 0));
            list.fields.Add(new Field("fGenus", "Genus", FieldType.ReadOnly, 80, 0));
            list.fields.Add(new Field("fPhylum", "Phylum", FieldType.ReadOnly, 80, 0));
            list.fields.Add(new Field("fClass", "Class", FieldType.ReadOnly, 80, 0));
            list.fields.Add(new Field("fOrder", "Order", FieldType.ReadOnly, 80, 0));
            list.fields.Add(new Field("fFamily", "Family", FieldType.ReadOnly, 80, 0));
            list.fields.Add(new Field("fSpecies", "Species", FieldType.ReadOnly, 80, 0));

            
            
            
            list.fields.Add(new Field("fFeatures", "Features", FieldType.String, 80, 0));
            list.fields.Add(new Field("fColour", "Colour", FieldType.String, 80, 0));
            list.fields.Add(new Field("fSize", "Size", FieldType.String, 80, 0));
            list.fields.Add(new Field("fDistribution", "Distribution", FieldType.Paragraph, 80, 0));
            list.fields.Add(new Field("fReferences", "References", FieldType.Paragraph, 80, 0));
            list.fields.Add(new Field("fNotes", "Notes", FieldType.String, 80, 0));  
            list.fields.Add(new Field("fSimilar", "Similar", FieldType.String, 80, 0));

            list.filterFields.Add("fCommonName");
            list.filterFields.Add("fSpeciesName");
            list.filterFields.Add("fFeatures");
            list.filterFields.Add("fWoRMID");
            list.filterFields.Add("fFeatures");
            list.filterFields.Add("fTaxonomy"); 

            list.filterFields.Add("fColour");
            list.filterFields.Add("fSize");
            list.filterFields.Add("fDistribution");
            list.filterFields.Add("fSimilar");
            list.filterFields.Add("fReferences");
            list.filterFields.Add("fNotes");

            list.showFilterFields = false;
            list.showAllButton = true;
            list.speciesQuery = true;


            // tools.FixWormID();


            list.InitList(Context);
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

        public bool TaxonFilter(int id, string filter)
        {
            if (filter == null || filter == "")
                filter = "0";
            int taxon = int.Parse(filter);
            if (taxon < 1)
                return true;

            int nParentTaxon = getSpeciesTaxonID(id);
            if (nParentTaxon == 0)
                return false;

            return FindTaxon(nParentTaxon, taxon);
        }



        public GenericList list;
    }
}