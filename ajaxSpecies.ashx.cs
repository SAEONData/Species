using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace species
{
    /// <summary>
    /// Summary description for ajaxSpecies
    /// </summary>
    public class ajaxSpecies : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            String mode = context.Request["mode"];

            switch (mode)
            {
                case "save":
                    SaveTags(context);
                    break;

                case "slist":
                    ListSpecies(context);
                    break;

                case "loadTags":
                    LoadTags(context);
                    break;

                case "loadTaxon":
                    LoadTaxon(context);
                    break;

                case "loadstations":
                    LoadStations(context);
                    break;

                case "movestation":
                    MoveStation(context);
                    break;


                case "loadCatTAxon":
                    loadCatTAxon(context);
                    break;

                case "loadLevel3":
                    loadLevel3(context);
                    break;

                case "loadSpeciesstations":
                    loadSpeciesStations(context);
                    break;

                case "updateCatFlag":
                    updateCatFlag(context);
                    break;


                default:
                    context.Response.Write("{ 'error': 'Invalid mode: " + mode + "'}" );
                    break;
            }
        }

        private void ListSpecies(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            String name = context.Request["name"];
            String query = String.Format("SELECT fSpeciesID, fCommonName, fSpeciesName FROM TblSpecies WHERE fCommonName LIKE '%{0}%' OR fSpeciesName LIKE '%{0}%' ORDER BY fCommonName", name);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            String commonName = context.Server.HtmlEncode(set["fCommonName"].ToString());
                            String scientName = context.Server.HtmlEncode(set["fSpeciesName"].ToString());
                            String fullName = commonName + ": " + scientName;

                            context.Response.Write("<tr>");
                            context.Response.Write("<td>");
                            context.Response.Write("<a href='" + fullName + "' class='spesiesRes' id='sp" + set["fSpeciesID"].ToString() + "' >" + commonName + "</a>");
                            context.Response.Write("</td>");
                            context.Response.Write("<td>");
                            context.Response.Write(scientName);
                            context.Response.Write("</td>");
                            context.Response.Write("</tr>");
                        }
                    }
                }
            }
        }

        

        private void SaveTags(HttpContext context)
        {
            int mediaID = int.Parse(context.Request["id"].ToString());
            String[] parms = context.Request["params"].Split('|');

            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();

                // delete old tags
                String cmd = "DELETE FROM TblSpeciesMedia WHERE fMediaID = " + mediaID;
                using (SqlCommand command = new SqlCommand(cmd, con))
                    command.ExecuteNonQuery();

                

                // add new tags
                foreach (String parm in parms)
                {
                    if (parm.Trim() == "")
                        return;

                    String[] args = parm.Split(',');
                    int speciedID = int.Parse(args[0]);
                    int x1 = int.Parse(args[1]);
                    int y1 = int.Parse(args[2]);
                    int x2 = int.Parse(args[3]);
                    int y2 = int.Parse(args[4]);

                    String sql = "INSERT INTO TblSpeciesMedia (fSpeciesID, fMediaID, fCatalogueOrder, x1, y1, x2, y2) VALUES (@fSpeciesID, @fMediaID, @fCatalogueOrder, @x1, @y1, @x2, @y2)";
                    using (SqlCommand command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("@fSpeciesID", speciedID);
                        command.Parameters.AddWithValue("@fMediaID", mediaID);
                        command.Parameters.AddWithValue("@fCatalogueOrder", 100);
                        command.Parameters.AddWithValue("@x1", x1);
                        command.Parameters.AddWithValue("@y1", y1);
                        command.Parameters.AddWithValue("@x2", x2);
                        command.Parameters.AddWithValue("@y2", y2);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }


        private void loadLevel3(HttpContext context)
        {
            int surveyID = int.Parse(context.Request["survey"]);
            String level2 = context.Request["level2"];
            String query = String.Format("SELECT fLevel3Name FROM TblEvent WHERE (fSurveyID = {0}) AND fLevel2Name = '{1}' AND fLevel3Name IS NOT NULL GROUP BY fLevel3Name", surveyID, level2);
            if (level2 == "All" || level2 == "")
                query = String.Format("SELECT fLevel3Name FROM TblEvent WHERE (fSurveyID = {0}) AND fLevel3Name IS NOT NULL GROUP BY fLevel3Name", surveyID);

            List<String> list = new List<string>();
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            list.Add(set[0].ToString());
                        }
                    }
                }
            }

            String[] items = list.ToArray();
            var json = new JavaScriptSerializer().Serialize(items);
            context.Response.Write(json);
        }


        private void loadCatTAxon(HttpContext context)
        {
            int catalog = int.Parse(context.Request["id"].ToString());
            List<taxonOption> tags = new List<taxonOption>();
            String query = "SELECT * FROM vwCatTaxonRank WHERE fCatalogueID = " + catalog;

            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read()) 
                        {
                            taxonOption tag = new taxonOption();
                            tag.id = (int)set["fTaxonomyID"];
                            tag.name = (string)set["fTaxonRankName"] + " -> " + (string)set["fTaxonomyName"];
                            tags.Add(tag);
                        }
                    }
                }
            }

            taxonOption[] tagArray = tags.ToArray();
            var json = new JavaScriptSerializer().Serialize(tagArray);
            context.Response.Write(json);

        }

        private void MoveStation(HttpContext context)
        {
            int id = int.Parse(context.Request["id"]);
            double lat = double.Parse(context.Request["lat"]);
            double lon = double.Parse(context.Request["lon"]);
            String sql = String.Format("UPDATE TblStation SET fStartLat = {0}, fStartLng = {1} WHERE fStationID = {2}", lat, lon, id);
            ExecuteSQL(sql);
            context.Response.Write("[]");
        }

        private void updateCatFlag(HttpContext context)
        {
            int id = int.Parse(context.Request["species"].Substring(3));
            String selected = context.Request["selected"];
            int order = selected == "true" ? 1000 : -1;
            String sql = String.Format("UPDATE TblSpeciesMedia SET fCatalogueOrder = {0} WHERE fSpeciesMediaID = {1}", order, id);
            ExecuteSQL(sql);
        }

        private void loadSpeciesStations(HttpContext context)
        {
            int speciesID = int.Parse(context.Request["id"]);

            List<Station> stations = new List<Station>();
            String query = "SELECT TblStation.* FROM TblSpeciesEvent INNER JOIN TblEvent ON TblSpeciesEvent.fEventID = TblEvent.fEventID INNER JOIN TblStation ON TblEvent.fStationID = TblStation.fStationID WHERE (TblSpeciesEvent.fSpeciesID = " + speciesID + ")";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Station station = new Station();
                            station.id = (int)set["fStationID"];
                            station.name = (string)set["fStationName"];
                            station.lat = (double)set["fStartLat"];
                            station.lon = (double)set["fStartLng"];
                            stations.Add(station);
                        }
                    }
                }
            }

            Station[] stationArray = stations.ToArray();
            var json = new JavaScriptSerializer().Serialize(stationArray);
            context.Response.Write(json);
        }


        private void LoadStations(HttpContext context)
        {
            List<Station> stations = new List<Station>();
            String query = "SELECT * FROM TblStation ORDER BY fStationName";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Station station = new Station();
                            station.id = (int)set["fStationID"];
                            station.name = (string)set["fStationName"];
                            station.lat = (double)set["fStartLat"];
                            station.lon = (double)set["fStartLng"];
                            stations.Add(station);
                        }
                    }
                }
            }

            Station[] stationArray = stations.ToArray();
            var json = new JavaScriptSerializer().Serialize(stationArray);
            context.Response.Write(json);
        }


        private void LoadTaxon(HttpContext context)
        {
            int rank = int.Parse(context.Request["rank"].ToString());
            List<taxonOption> tags = new List<taxonOption>();
            String query = "SELECT * FROM TblTaxonomy WHERE fTaxonRankID = " + rank + " ORDER BY fTaxonomyName";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            taxonOption tag = new taxonOption();
                            tag.id = (int)set["fTaxonomyID"];
                            tag.name = (string)set["fTaxonomyName"];
                            tags.Add(tag);
                        }
                    }
                }
            }

            taxonOption[] tagArray = tags.ToArray();
            var json = new JavaScriptSerializer().Serialize(tagArray);
            context.Response.Write(json);
        }

        private void LoadTags(HttpContext context)
        {
            int mediaID = int.Parse(context.Request["id"].ToString());
            List<MediaTag> tags = new List<MediaTag>();
            String query = "SELECT * FROM vwSpeciesMedia WHERE x1 IS NOT NULL AND fMediaID = " + mediaID;

            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            MediaTag tag = new MediaTag();
                            tag.id = (int)set["fSpeciesID"];
                            tag.name = (string)set["fCommonName"] + ": " + (string)set["fSpeciesName"];

                            tag.rect.x = int.Parse(set["x1"].ToString());
                            tag.rect.y = int.Parse(set["y1"].ToString());
                            tag.rect.x2 = int.Parse(set["x2"].ToString());
                            tag.rect.y2 = int.Parse(set["y2"].ToString());
                            tag.deleted = false;
                            tags.Add(tag);
                        }
                    }
                }
            }

            MediaTag[] tagArray = tags.ToArray();
            var json = new JavaScriptSerializer().Serialize(tagArray);
            context.Response.Write(json);
        }

        void ExecuteSQL(String sql)
        {
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class MediaTag
    {
        public struct rectangle
        {
            public int x;
            public int y;
            public int x2;
            public int y2;
        }

        public int id;
        public string name;
        public rectangle rect;
        public bool deleted;
    }

    public class taxonOption
    {
        public int id;
        public string name;
    };

    public class Station
    {
        public int id;
        public string name;
        public double lat;
        public double lon;
    };



}