using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class manStations
    {
        public int FindStationID(String name)
        {
            int StationID = 0;
            String query = String.Format("SELECT fStationID FROM TblStation WHERE fStationName = '{0}'", name.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            StationID = (int)set[0];
                        }
                    }
                }
            }
            return StationID;
        }

        public void AddStation(String name, double lat, double lng)
        {
            String sql = "INSERT INTO TblStation (fStationName, fStartLat, fStartLng, fSystem) VALUES (@fStationName, @fStartLat, @fStartLng, 0)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fStationName", name);
                    cmd.Parameters.AddWithValue("@fStartLat", lat);
                    cmd.Parameters.AddWithValue("@fStartLng", lng);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetStationID(String name, double lat, double lng)
        {
            int nStationID = FindStationID(name);
            if (nStationID < 1)
            {
                AddStation(name, lat, lng);
                nStationID = FindStationID(name);
            }
            return nStationID;
        }
    }
}