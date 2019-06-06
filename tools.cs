using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class tools
    {
        public static Dictionary<String, String> parseFilters(String filter)
        {
            Dictionary<String, String> dict = new Dictionary<String, String>();
            String[] filters = filter.Split('|');
            foreach (String f in filters)
            {
                String[] args = f.Split('=');
                dict[args[0]] = args[1];
            }
            return dict;
        }

        public static int ExecuteSQL(String sql)
        {
            int result = 0;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }

        public static String GetWormsID(int speciesID)
        {
            String worms = "";
            String query = "SELECT TblTaxonomy.fTaxonomyName FROM TblSpecies INNER JOIN TblTaxonomy ON TblSpecies.fTaxonomyID = TblTaxonomy.fTaxonomyID WHERE TblSpecies.fSpeciesID = " + speciesID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            String worm = set[0].ToString();
                            int index = worm.IndexOf('(');
                            String sub = worm.Substring(index + 1);
                            worms = sub.Replace(")", "");
                        }
                    }
                }
            }
            return worms;
        }

        public static void FixWormID()
        {
            Dictionary<int, int> map = new Dictionary<int, int>();
            String query = "SELECT fSpeciesID FROM TblSpecies WHERE fWoRMID IS NULL";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            int speciesID = (int)set[0];
                            int wormsID = int.Parse(tools.GetWormsID(speciesID));
                            map[speciesID] = wormsID;
                        }
                    }
                }
            }

            foreach (int key in map.Keys)
            {
                int speciesID = key;
                int wormsID = map[key];
                String sql = String.Format("UPDATE TblSpecies SET fWoRMID = {0} WHERE fSpeciesID = {1}", wormsID, speciesID);
                ExecuteSQL(sql);
            }
        }


    }
}