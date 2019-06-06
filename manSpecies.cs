using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class manSpecies
    {
        public Worms.AphiaRecord FindSpeciesByScientificName(String search)
        {
            Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();
            Worms.AphiaRecord[] records = client.getAphiaRecords(search, true, true, true, 0);
            if (records == null || records.Length == 0)
                return null;
            return records[0];
        }

        public Worms.AphiaRecord FindSpeciesByCommonName(String name)
        {
            Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();
            Worms.AphiaRecord[] records = client.getAphiaRecordsByVernacular(name, true, 0);
            if (records == null || records.Length == 0)
                return null;
            return records[0];
        }

        public Worms.AphiaRecord FindSpecies(String Genus, String Species, String Common)
        {
            Worms.AphiaRecord record = null;
            if (Genus != "" && Species != "")
            {
                String search = Genus + " " + Species;
                record = FindSpeciesByScientificName(search);
            }
            if (record == null && Genus != "")
                record = FindSpeciesByScientificName(Genus);
            if (record == null && Species != "")
                record = FindSpeciesByScientificName(Species);
            if (record == null && Common != "")
                record = FindSpeciesByCommonName(Common);
            return record;
        }

        int GetSpeciesID(String name)
        {
            int speciesID = 0;
            String query = String.Format("SELECT fSpeciesID FROM TblSpecies WHERE fSpeciesName = '{0}'", name.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            speciesID = (int)set[0];
                        }
                    }
                }
            }
            return speciesID;
        }

        void AddSpecies(String genus, String species, String common, int taxonID, int wormnID)
        {
            String speciesName = genus + " " + species;
            String sql = "INSERT INTO TblSpecies (fTaxonomyID, fLSID, fWoRMID, fSpeciesName, fCommonName) VALUES (@fTaxonomyID, @fLSID, @fWoRMID, @fSpeciesName, @fCommonName)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fTaxonomyID", taxonID);
                    cmd.Parameters.AddWithValue("@fLSID", wormnID);
                    cmd.Parameters.AddWithValue("@fWoRMID", wormnID);
                    cmd.Parameters.AddWithValue("@fSpeciesName", speciesName);
                    cmd.Parameters.AddWithValue("@fCommonName", common);
                    cmd.ExecuteNonQuery();
                }
            }

        }


        public int GetSpeciesID(String genus, String species, String common)
        {
            String speciesName = genus + " " + species;
            int speciesID = GetSpeciesID(speciesName);
            if (speciesID != 0)
                return speciesID;
            int taxonID = 0;
            int wormsID = 0;


            Worms.AphiaRecord record = FindSpecies(genus, species, common);
            if (record != null)
            {
                wormsID = record.AphiaID;
                GenericList gl = new GenericList(null);
                if (record.scientificname == null || record.scientificname.Trim() == "")
                    record.scientificname = "Unknown";
                taxonID = int.Parse(gl.getTaxonID(int.Parse(record.AphiaID.ToString()), record.scientificname));
                AddSpecies(genus, species, common, taxonID, wormsID);
            }

            return GetSpeciesID(speciesName);

        }

    }
}