using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace species
{
    public class manEvents
    {
        public HttpResponse res = null;

        int FindEventID(int surveyID, int stationID, int year, int month, int day, string level2, string level3)
        {
            int EventID = 0;
            String query = String.Format("SELECT fEventID FROM TblEvent WHERE fSurveyID = {0} AND fStationID = {1} AND fLevel2Name = '{2}' AND fLevel3Name = '{3}' AND fEventName = '{3}'", surveyID, stationID, level2.Replace("'", "''"), level3.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            EventID = int.Parse(set[0].ToString());
                        }
                    }
                }
            }
            return EventID;
        }

        void AddEvent(int surveyID, int stationID, int year, int month, int day, string level2, string level3, double lat1, double lon1, double lat2, double lon2, double duration, String comments, Dictionary<int, string> indicators)
        {
            DateTime dt = new DateTime(year, month, day);
            String date = dt.ToString("yyyy-MM-dd HH:mm:ss");


            String sql = "INSERT INTO TblEvent (fSurveyID, fStationID, fLevel2Name, fLevel3Name, fEventName, fStartDate,  fDuration, fSystemEvent, fStartLat, fStartLng, fEndLat, fEndLng, fComment) VALUES (@fSurveyID, @fStationID, @fLevel2Name, @fLevel3Name, @fEventName, @fStartDate,  @fDuration, @fSystemEvent, @fStartLat, @fStartLng, @fEndLat, @fEndLng, @fComment)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fSurveyID", surveyID);
                    cmd.Parameters.AddWithValue("@fStationID", stationID); 
                    cmd.Parameters.AddWithValue("@fLevel2Name", level2);
                    cmd.Parameters.AddWithValue("@fLevel3Name", level3);
                    cmd.Parameters.AddWithValue("@fEventName", level3);
                    cmd.Parameters.AddWithValue("@fStartDate", date);
                    cmd.Parameters.AddWithValue("@fDuration", duration);
                    cmd.Parameters.AddWithValue("@fSystemEvent", 0);
                    cmd.Parameters.AddWithValue("@fStartLat", lat1);
                    cmd.Parameters.AddWithValue("@fStartLng", lon1);
                    cmd.Parameters.AddWithValue("@fEndLat", lat2);
                    cmd.Parameters.AddWithValue("@fEndLng", lon2);
                    cmd.Parameters.AddWithValue("@fComment", comments);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddIndicators(int nEventID, Dictionary<int, string> indicators)
        {
            tools.ExecuteSQL(String.Format("DELETE FROM TblEventIndicator WHERE fEventID = {0}", nEventID));

            foreach (int indicator in indicators.Keys)
            {
                double val = 0;
                String value = indicators[indicator];
                if (value != "")
                    val = double.Parse(value);

                String sql = "INSERT INTO TblEventIndicator (fEventID, fIndicatorID, fValue) VALUES (@fEventID, @fIndicatorID, @fValue)";
                using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@fEventID", nEventID);
                        cmd.Parameters.AddWithValue("@fIndicatorID", indicator);
                        cmd.Parameters.AddWithValue("@fValue", val);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

        }


        public int GetEventID(int surveyID, int stationID, int year, int month, int day, string level2, string level3, double lat1, double lon1, double lat2, double lon2, double duration, String comments, Dictionary<int, string> indicators)
        {
            int eventID = FindEventID(surveyID, stationID, year, month, day, level2, level3);
            if (eventID < 1)
            {
                AddEvent(surveyID, stationID, year, month, day, level2, level3, lat1, lon1, lat2, lon2, duration, comments, indicators);
                eventID = FindEventID(surveyID, stationID, year, month, day, level2, level3);
            }
            if (eventID < 1)
                return eventID;
            AddIndicators(eventID, indicators);
            return eventID;
        }

        public int FindEventID(String level2, String level3, String stationName)
        {
            int EventID = 0;
            String query = String.Format("SELECT TblEvent.fEventID FROM TblEvent INNER JOIN TblStation ON TblEvent.fStationID = TblStation.fStationID WHERE (TblEvent.fLevel2Name = '{0}') AND (TblEvent.fLevel3Name = '{1}') AND (TblStation.fStationName = '{2}')", level2.Replace("'", "''"), level3.Replace("'", "''"), stationName.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            EventID = int.Parse(set[0].ToString());
                        }
                    }
                }
            }
            return EventID;
        }
    }
}