using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class media : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Image";
            list.table = "TblMedia";
            list.idField = "fMediaID";
            list.webFolderButton = false;
            list.fields.Add(new Field("fMediaName", "Image Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fMediaPath", "Image", FieldType.Photo, 50, 50));
            list.fields.Add(new Field("fMediaTitle", "File Name", FieldType.ReadOnly, 50, 50));
            list.filterDelegate = filterMedia;
            list.exportButton = false;
            list.showAllButton = true;

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

        bool IsMediaTagged(int id)
        {
            bool IsTagged = false;
            String sql = "SELECT fSpeciesID FROM TblSpeciesMedia WHERE fMediaID = " + id;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        IsTagged = set.Read();
                    }
                }
            }
            return IsTagged;
        }

        bool IsMediaSpecies(int id, int species)
        {
            bool recFound = false;
            String sql = "SELECT fSpeciesID FROM TblSpeciesMedia WHERE fMediaID = " + id + " AND fSpeciesID = " + species;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        recFound = set.Read();
                    }
                }
            }
            return recFound;
        }

        public bool IsTaxonInTree(int nTaxonID, int nRankID, int nValueID)
        {
            if (nTaxonID == nValueID)
                return true;

            String sql = "SELECT fTaxonomyParentID FROM dbo.TblTaxonomy WHERE fTaxonomyID = " + nTaxonID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            int nParentTaxon = int.Parse(set[0].ToString());
                            if (nParentTaxon != 0)
                            {
                                if (IsTaxonInTree(nParentTaxon, nRankID, nValueID) == true)
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;


        }

        public int GetSpeciesTaxon(int id)
        {
            int taxonID = 0;
            String sql = "SELECT fTaxonomyID FROM TblSpecies WHERE fSpeciesID = " + id;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            taxonID = (int)set[0];
                        }
                    }
                }
            }
            return taxonID;
        }

        public bool IsMediaTaxonomy(int mediaID, int nRankID, int nValueID)
        {
            String sql = "SELECT fSpeciesID FROM TblSpeciesMedia WHERE fMediaID = " + mediaID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            int speciesID = int.Parse(set[0].ToString());
                            int taxonID = GetSpeciesTaxon(speciesID);
                            if (IsTaxonInTree(taxonID, nRankID, nValueID))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool FilterSpeciesName(int media, string name)
        {
            bool found = false;

            String sql = String.Format("SELECT fSpeciesID FROM vwSpeciesMedia WHERE fMediaID = {0} AND (fSpeciesName LIKE '%{1}%' OR fCommonName LIKE '%{1}%')", media, name.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                            found = true;
                    }
                }
            }
            return found;
        }


        public bool filterMedia(int id, string filter)
        {
            if (filter == "")
                return true;

            Dictionary<String, String> filters = tools.parseFilters(filter);
            foreach (String key in filters.Keys)
            {
                String val = filters[key];
                if (val != "0" && val != "" && val != "false")
                {
                    switch (key)
                    {
                        case "taxon":
                            if (!IsMediaTaxonomy(id, int.Parse(filters["rank"]), int.Parse(filters["taxon"])))
                                return false;
                            break;

                        case "find":
                            if (!FilterSpeciesName(id, val))
                                return false;
                            break;

                        case "untagged":
                            if (IsMediaTagged(id))
                                return false;
                            break;

                    }
                }


            }


            return true;
        }

        public void WriteSpecies()
        {
            String sql = "SELECT * FROM TblSpecies ORDER BY fCommonName";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Response.Write("<option value='" + set["fSpeciesID"].ToString() + "'>" + Server.HtmlEncode(set["fCommonName"].ToString() + ": " + set["fSpeciesName"].ToString()) + "</option>");

                        }
                    }
                }
            }

        }

        public GenericList list;
    }
}