using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class manSpeciesMedia
    {
        int FindSpeciesMediaID(int SpeciesID, int MediaID)
        {
            int SpeciesMediaID = 0;
            String query = String.Format("SELECT fSpeciesMediaID FROM TblSpeciesMedia WHERE fSpeciesID = {0} AND fMediaID = {1}", MediaID, SpeciesID);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            SpeciesMediaID = int.Parse(set[0].ToString());
                        }
                    }
                }
            }
            return SpeciesMediaID;
        }

        void AddSpeciesMedia(int SpeciesID, int MediaID, int x1, int y1, int x2, int y2)
        {
            String sql = "INSERT INTO TblSpeciesMedia (fSpeciesID, fMediaID, x1, y1, x2, y2) VALUES (@fSpeciesID, @fMediaID, @x1, @y1, @x2, @y2)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fSpeciesID", SpeciesID);
                    cmd.Parameters.AddWithValue("@fMediaID", MediaID);
                    cmd.Parameters.AddWithValue("@x1", x1);
                    cmd.Parameters.AddWithValue("@x2", x2);
                    cmd.Parameters.AddWithValue("@y1", y1);
                    cmd.Parameters.AddWithValue("@y2", y2);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetSpeciesMediaID(int SpeciesID, int MediaID, int x1, int y1, int x2, int y2)
        {
            int SpeciesMediaID = FindSpeciesMediaID(SpeciesID, MediaID);
            if (SpeciesMediaID < 1)
            {
                AddSpeciesMedia(SpeciesID, MediaID, x1, y1, x2, y2);
                SpeciesMediaID = FindSpeciesMediaID(SpeciesID, MediaID);
            }
            return SpeciesMediaID;
        }

    }
}