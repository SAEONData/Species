using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class manEventSpecies
    {
        public void addEventSpecies(int eventID, int speciesID, double abundance, double biomass, string description)
        {
            tools.ExecuteSQL(String.Format("DELETE FROM TblSpeciesEvent WHERE fEventID = {0} AND fSpeciesID = {1}", eventID, speciesID));

            String sql = "INSERT INTO TblSpeciesEvent (fSpeciesID, fEventID, fAbundance, fBioMass, fDescription) VALUES (@fSpeciesID, @fEventID, @fAbundance, @fBioMass, @fDescription)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fSpeciesID", speciesID);
                    cmd.Parameters.AddWithValue("@fEventID", eventID);
                    cmd.Parameters.AddWithValue("@fAbundance", abundance);
                    cmd.Parameters.AddWithValue("@fBioMass", biomass);
                    cmd.Parameters.AddWithValue("@fDescription", description);
                    cmd.ExecuteNonQuery();
                }
            }


        }
    }
}