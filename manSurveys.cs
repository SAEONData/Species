using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class manSurveys
    {
        public int FindSurveyID(int surveyType, String name)
        {
            int surveyID = 0;
            String query = String.Format("SELECT fSurveyID FROM TblSurvey WHERE fSurveyTypeRef = {0} AND fSurveyLabel = '{1}'", surveyType, name.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            surveyID = (int)set[0];
                        }
                    }
                }
            }
            return surveyID;
        }

        public void AddSurvey(int surveyType, String name)
        {
            String sql = "INSERT INTO TblSurvey (fSurveyTypeRef, fSurveyLabel, fSurveyDesc, fSystem) VALUES (@fSurveyTypeRef, @fSurveyLabel, @fSurveyDesc, 0)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fSurveyTypeRef", surveyType);
                    cmd.Parameters.AddWithValue("@fSurveyLabel", name);
                    cmd.Parameters.AddWithValue("@fSurveyDesc", name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetSurveyID(int surveyType, String name)
        {
            int nSurveyID = FindSurveyID(surveyType, name);
            if (nSurveyID < 1)
            {
                AddSurvey(surveyType, name);
                nSurveyID = FindSurveyID(surveyType, name);
            }
            return nSurveyID;
        }
    }
}