using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class dbtools
    {
        public static object SimpleQuery(String query)
        {
            object res = null;
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                            res = set[0];
                    }
                }
            }
            return res;
        }
    }
}