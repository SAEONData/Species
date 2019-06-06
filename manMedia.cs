using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class manMedia
    {
        public int FindMediaID(String name, String url)
        {
            int MediaID = 0;
            String query = String.Format("SELECT fMediaID FROM TblMedia WHERE fMediaName = '{0}' AND fMediaPath = '{1}'", name.Replace("'", "''"), url.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            MediaID = (int)set[0];
                        }
                    }
                }
            }
            return MediaID;
        }

        public void AddMedia(String name, String url)
        {
            String sql = "INSERT INTO TblMedia (fMediaName, fMediaPath, fEventID, fMediaType) VALUES (@fMediaName, @fMediaPath, 1, 1)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fMediaName", name);
                    cmd.Parameters.AddWithValue("@fMediaPath", url);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetMediaID(String name, String url)
        {
            int nMediaID = FindMediaID(name, url);
            if (nMediaID < 1)
            {
                AddMedia(name, url);
                nMediaID = FindMediaID(name, url);
            }
            return nMediaID;
        }
    }
}