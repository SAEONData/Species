using BLA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class isheet : System.Web.UI.Page
    {

        private String[] GetExcelSheetNames(string excelFile)
        {
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;

            try
            {
                // Connection String. Change the excel file to the file you
                // will search.
                String connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excelFile + "; Extended Properties=Excel 12.0;";
                // Create connection object by using the preceding connection string.
                objConn = new OleDbConnection(connString);
                // Open connection with the database.
                objConn.Open();
                // Get the data table containg the schema guid.
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }


                List<string> excelSheets = new List<string>();

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets.Add(row["TABLE_NAME"].ToString());
                }

                return excelSheets.ToArray();
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                return null;
            }
            finally
            {
                // Clean up.
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            String url = Request["url"] + "?__ac_name=SpeciesDB&__ac_password=SpeciesDB";

            String name = "temp/sheet.xlsx"; 
            String path = Server.MapPath(name);
            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }

            String sheetType = Request["type"];

            String surveyName = "";
            String fileName = Path.GetFileNameWithoutExtension(url);
            String[] fparts = fileName.Split('_');
            if (fparts.Length > 0)
                surveyName = fparts[0].ToUpper();

            Dictionary<String, int> stations = new Dictionary<string, int>();

            int surveyID = -1;
            int stationID = -1;


            Dictionary<string, int> fieldMap = new Dictionary<string, int>();



            string con = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path  + "; Extended Properties='Excel 8.0;HDR=Yes;'";
            con = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + "; Extended Properties=Excel 12.0;";

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");

            trawl tr = new trawl();

            String[] sheets = GetExcelSheetNames(path);
            if (sheets.Length == 0)
            {
                Response.Write("No sheets in workbook: " + path + "<br>");
                Response.End();
            }
            String sheet = sheets[0];



            using (OleDbConnection connection = new OleDbConnection(con))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("select * from [" + sheet + "]", connection);
                using (OleDbDataReader dr = command.ExecuteReader())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        String field = rgx.Replace(dr.GetName(i), "").Replace(" ", "").ToLower();
                        // Response.Write(field + "<br>");

                        fieldMap[field] = i;
                    }

                    // Response.End();


                    String prevStation = "";


                    while (dr.Read())
                    {
                        if (sheetType == "catch")
                        {
                            String cruise = dr[fieldMap["cruise"]].ToString();
                            String trawl = dr[fieldMap["trawl"]].ToString();
                            String station = dr[fieldMap["station"]].ToString();
                            if (station.Trim() == "")
                                break;

                            String commonname = dr[fieldMap["commonname"]].ToString();
                            String genus = dr[fieldMap["genus"]].ToString();
                            String species = dr[fieldMap["species"]].ToString();
                            String fbcode = dr[fieldMap["fbcode"]].ToString();
                            String abundanceindividuals = dr[fieldMap["abundanceindividuals"]].ToString();
                            String biomasskg = dr[fieldMap["biomasskg"]].ToString();
                            String subsampleweightkg = dr[fieldMap["subsampleweightkg"]].ToString();
                            String totalinvertcatchweightkg = dr[fieldMap["totalinvertcatchweightkg"]].ToString();

                            String phylum = fieldMap.ContainsKey("phylum") ? dr[fieldMap["phylum"]].ToString() : "";
                            String sclass = fieldMap.ContainsKey("class") ? dr[fieldMap["class"]].ToString() : "";
                            String order = fieldMap.ContainsKey("order") ? dr[fieldMap["order"]].ToString() : "";
                            String family = fieldMap.ContainsKey("family") ? dr[fieldMap["family"]].ToString() : "";
                            String resonforchange = fieldMap.ContainsKey("resonforchange") ? dr[fieldMap["resonforchange"]].ToString() : "";
                            String description = dr[fieldMap["description"]].ToString();
                            if (surveyID == -1)
                                surveyID = GetSurveyID(cruise, cruise);

                            if (prevStation != station)
                            {
                                prevStation = station;
                                stationID = GetStationID(surveyID, station, -100, -100);
                            }
                            int eventID = GetEventID(surveyID, stationID, cruise, trawl, trawl, "", -100, -100, -100, -100, -100, -100);

                            Response.Write("common name: " + commonname + "<br>");

                            int speciesID = GetSpeciesID(commonname, genus, species, phylum, sclass, order, family, fbcode);

                            Response.Write("speciesid:" + speciesID + "<br>");

                            if (abundanceindividuals == "")
                                abundanceindividuals = "1";

                            if (eventID != 0)
                            
                            tr.GetSpeciesEventID(speciesID, eventID, double.Parse(abundanceindividuals));

                            Response.Write("event = " + eventID + ", species = " + speciesID + "<br>");
                            Response.Flush();
                        }
                        else
                        {
                            String survey = dr[fieldMap["survey"]].ToString();
                            String cruise = dr[fieldMap["cruise"]].ToString();
                            String trawl = dr[fieldMap["trawlno"]].ToString();
                            String station = dr[fieldMap["station"]].ToString();
                            String duration = dr[fieldMap["durationmin"]].ToString();
                            String yy = dr[fieldMap["yy"]].ToString();
                            String mm = dr[fieldMap["mm"]].ToString();
                            String dd = dr[fieldMap["dd"]].ToString();
                            if (yy == "")
                                break;

                            String lat1 = "0";
                            String lng1 = "0";
                            String lat2 = "0";
                            String lng2 = "0";

                            if (fieldMap.ContainsKey("startlathour"))
                            {
                                lat1 = ((double.Parse(dr[fieldMap["startlathour"]].ToString()) + double.Parse(dr[fieldMap["startlatmin"]].ToString()) / 60.0) * -1).ToString();
                                lng1 = ((double.Parse(dr[fieldMap["startlonghour"]].ToString()) + double.Parse(dr[fieldMap["startlongmin"]].ToString()) / 60.0) * 1).ToString();
                                lat2 = ((double.Parse(dr[fieldMap["endlathour"]].ToString()) + double.Parse(dr[fieldMap["endlatmin"]].ToString()) / 60.0) * -1).ToString();
                                lng2 = ((double.Parse(dr[fieldMap["endlonghour"]].ToString()) + double.Parse(dr[fieldMap["endlongmin"]].ToString()) / 60.0) * 1).ToString();
                            }
                            else
                            {
                                lat1 = dr[fieldMap["startlat"]].ToString();
                                lng1 = dr[fieldMap["startlong"]].ToString();
                                lat2 = dr[fieldMap["endlat"]].ToString();
                                lng2 = dr[fieldMap["endlong"]].ToString();
                            }

                            String depth = dr[fieldMap["depthm"]].ToString();

                            DateTime dt = new DateTime(int.Parse(yy), int.Parse(mm), int.Parse(dd));
                            String date = dt.ToString();


                            if (surveyID == -1)
                                surveyID = GetSurveyID(survey, surveyName);

                            // get station
                            if (stations.ContainsKey(station) == false)
                            {
                                int id = GetStationID(surveyID, station, double.Parse(lat1), double.Parse(lng1));
                                stations[station] = id;
                            }
                            stationID = stations[station];

                            // get event
                            int eventID = GetEventID(surveyID, stationID, cruise, trawl, trawl, date, double.Parse(duration), double.Parse(lat1), double.Parse(lng1), double.Parse(lat2), double.Parse(lng2), double.Parse(depth));

                            Response.Write(station + "<br>");
                        }
                    }
                }
            }

            Response.Write("done");
       
        }

        public int GetSpeciesID(int wormsID)
        {
            int speciesID = 0;
            String query = String.Format("SELECT fSpeciesID FROM dbo.TblSpecies WHERE fWoRMID = {0}", wormsID);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            speciesID = (int)set["fSpeciesID"];
                        }
                    }
                }
            }
            return speciesID;
        }

        public int GetExistingSpeciesID(String CommonName, String Genus, String Species, String Phylum, String Class, String Order, String Family, String FBcode)
        {
            String speciesName = (Genus.Trim() + " " + Species.Trim()).Trim();
            String query = String.Format("SELECT fSpeciesID FROM TblSpecies WHERE fGenus LIKE '{0}' AND fPhylum LIKE '{1}' AND fClass LIKE '{2}' AND fOrder LIKE '{3}' AND fFamily LIKE '{4}' AND fSpecies LIKE '{5}' AND fFishBoard LIKE '{6}'", Genus.Replace("'", "''"), Phylum.Replace("'", "''"), Class.Replace("'", "''"), Order.Replace("'", "''"), Family.Replace("'", "''"), Species.Replace("'", "''"), FBcode.Replace("'", "''"));
            Response.Write(query + "<br>");

            int speciesID = 0;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            speciesID = (int)set["fSpeciesID"];
                        }
                    }
                }
            }
            return speciesID;
        }


        public int GetSpeciesID(String CommonName, String Genus, String Species, String Phylum, String Class, String Order, String Family, String FBcode)
        {
            String speciesName = (Genus.Trim() + " " + Species.Trim()).Trim();
            int wormsid = 0;
            int speciesid = GetExistingSpeciesID(CommonName, Genus, Species, Phylum, Class, Order, Family, FBcode);
            if (speciesid > 0)
                return speciesid;

            GenericList gl = new GenericList(Context);


            String query = String.Format("SELECT * FROM TblSpeciesLookup WHERE fScienceNameAlts LIKE '%{0}%' OR fCommonNameAlts LIKE '%{1}%'", speciesName.Replace("'", "''"), CommonName.Replace("'", "''"));
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            wormsid = (int)set["fWoRMID"];
                            speciesid = GetSpeciesID(wormsid);
                            if (speciesid > 0)
                                return speciesid;

                            // add new species
                            int nTaxonID = int.Parse(gl.getTaxonID(wormsid, CommonName));
                            SuperSQL sql = new SuperSQL(con, null, Response);
                            sql.add("fTaxonomyID", nTaxonID);
                            sql.add("fLSID", wormsid);
                            sql.add("fWoRMID", wormsid);
                            sql.add("fSpeciesName", set["fSpeciesLookupName"].ToString());
                            sql.add("fCommonName", set["fCommonName"].ToString());
                            sql.add("fFeatures", set["fFeatures"].ToString());
                            sql.add("fColour", set["fColour"].ToString());
                            sql.add("fSize", set["fSize"].ToString());
                            sql.add("fDistribution", set["fDistribution"].ToString());
                            sql.add("fSimilar", set["fSimilar"].ToString());
                            sql.add("fReferences", set["fReferences"].ToString());
                            sql.add("fNotes", set["fNotes"].ToString());
                            sql.add("fFAFFCode", set["fFAFFCode"].ToString());
                            sql.add("fHabitat", set["fHabitat"].ToString());
                            sql.add("fFishBoard", FBcode);
                            sql.add("fGenus", Genus);
                            sql.add("fPhylum", Phylum);
                            sql.add("fClass", Class);
                            sql.add("fOrder", Order);
                            sql.add("fFamily", Family);
                            sql.add("fSpecies", Species);

                            set.Close();
                            sql.insert("TblSpecies");
                            return GetSpeciesID(wormsid);
                        }
                    }
                }
                
                // check if we can resolve against worms
                wormsid = GetWormsID(speciesName, CommonName);
                Response.Write("wormslookup: " + wormsid + "<br>");
                if (wormsid > 0)
                {
                    // add worm to species lookup
                    SuperSQL sql = new SuperSQL(con, null, Response);
                    sql.add("fWoRMID", wormsid);
                    sql.add("fSpeciesLookupName", speciesName);
                    sql.add("fCommonName", CommonName);
                    sql.add("fScienceNameAlts", speciesName);
                    sql.add("fCommonNameAlts", CommonName);
                    sql.insert("TblSpeciesLookup");
                    return GetSpeciesID(CommonName, Genus, Species, Phylum, Class, Order, Family, FBcode);
                }

                // add the species without worms id
                SuperSQL sq2 = new SuperSQL(con, null, Response);
                sq2.add("fLSID", 0);
                sq2.add("fWoRMID", 0);
                sq2.add("fSpeciesName", speciesName);
                sq2.add("fCommonName", CommonName);
                sq2.add("fFishBoard", FBcode);
                sq2.add("fGenus", Genus);
                sq2.add("fPhylum", Phylum);
                sq2.add("fClass", Class);
                sq2.add("fOrder", Order);
                sq2.add("fFamily", Family);
                sq2.add("fSpecies", Species);
                return sq2.insert("TblSpecies", "fSpeciesID");
            }

        }

        int GetWormsID(String scientificName, String commonName)
        {
            try
            {
                Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();
                Worms.AphiaRecord[] records = null;

                // search by scientfic name
                Response.Write("search_sc: " + scientificName + "<br>");
                records = client.getAphiaRecords(scientificName, true, true, true, 0);
                if (records != null && records.Length != 0)
                {
                    return records[0].AphiaID;
                }


                // search by common name
                Response.Write("search_cm: " + commonName + "<br>");
                records = client.getAphiaRecordsByVernacular(commonName, true, 0);
                if (records != null && records.Length != 0)
                {
                    return records[0].AphiaID;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception msg)
            {
                return 0;
            }
        }



        int GetEventID(int surveyID, int stationID, String cruise, String trawl, String name, String date, double duration, double lat1, double lng1, double lat2, double lng2, double depth)
        {
            String query = String.Format("SELECT fEventID FROM TblEvent WHERE fSurveyID = {0} AND fEventName = '{1}'", surveyID, name);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fEventID"];
                        }
                        else
                        {
                            if (duration == -100)
                            {
                                Response.Write("Invalid Event: " + name + "<br>");
                                return 0;
                            }

                            set.Close();
                            String sql = "INSERT INTO TblEvent (fSurveyID, fStationID, fLevel2Name, fLevel3Name, fEventName, fStartDate, fDuration, fStartLat, fStartLng, fEndLat, fEndLng, fDepth) VALUES (@fSurveyID, @fStationID, @fLevel2Name, @fLevel3Name, @fEventName, @fStartDate, @fDuration, @fStartLat, @fStartLng, @fEndLat, @fEndLng, @fDepth)";
                            
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fSurveyID", surveyID);
                                cmd.Parameters.AddWithValue("@fStationID", stationID);
                                cmd.Parameters.AddWithValue("@fLevel2Name", cruise);
                                cmd.Parameters.AddWithValue("@fLevel3Name", trawl);
                                cmd.Parameters.AddWithValue("@fEventName", name);
                                cmd.Parameters.AddWithValue("@fStartDate", date);
                                cmd.Parameters.AddWithValue("@fDuration", duration);
                                cmd.Parameters.AddWithValue("@fStartLat", lat1);
                                cmd.Parameters.AddWithValue("@fStartLng", lng1);
                                cmd.Parameters.AddWithValue("@fEndLat", lat2);
                                cmd.Parameters.AddWithValue("@fEndLng", lng2);
                                cmd.Parameters.AddWithValue("@fDepth", depth);
                                
                                cmd.ExecuteNonQuery();
                                return GetEventID(surveyID, stationID, cruise, trawl, name, date, duration, lat1, lng1, lat2, lng2, depth);
                            }
                        }
                    }
                }
            }
        }

        int GetStationID(int surveyID, String name, double lat, double lng)
        {
            String query = "SELECT * FROM TblStation WHERE fStationName = '" + name + "'";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fStationID"];
                        }
                        else
                        {
                            if (lat == -100 || lng == -100)
                            {
                                Response.Write("Invalid station: " + name + "<br>");
                                return 1;
                            }
                            set.Close();
                            String sql = "INSERT INTO TblStation (fStationName, fStartLat, fStartLng) VALUES (@fStationName, @fStartLat, @fStartLng)";
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fStationName", name);
                                cmd.Parameters.AddWithValue("@fStartLat", lat);
                                cmd.Parameters.AddWithValue("@fStartLng", lng);
                                cmd.ExecuteNonQuery();
                                return GetStationID(surveyID, name, lat, lng);
                            }
                        }
                    }
                }
            }
        }

        int GetSurveyID(String name, String key)
        {
            String query = "SELECT * FROM dbo.TblSurvey WHERE fSurveyLabel = '" + name + "' OR fSurveyName LIKE '%" + key + "%'";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fSurveyID"];
                        }
                        else
                        {
                            set.Close();
                            String sql = "INSERT INTO TblSurvey (fSurveyLabel, fSurveyName, fSurveyTypeRef) VALUES (@fSurveyLabel, @fSurveyName, 2)";
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fSurveyLabel", name);
                                cmd.Parameters.AddWithValue("@fSurveyName", key);
                                cmd.ExecuteNonQuery();
                                return GetSurveyID(name, key);
                            }
                        }
                    }
                }
            }
        }
    }
}