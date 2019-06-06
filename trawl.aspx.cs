using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Worms;

namespace species
{
    public partial class trawl : System.Web.UI.Page
    {
        String url;
        String src;
        SortedDictionary<String, String> media = new SortedDictionary<string, String>();
        SortedDictionary<String, int> species = new SortedDictionary<String, int>();

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void WriteSpeciesText()
        {
            foreach (String key in species.Keys)
            {
                Response.Write(key + "<br>");
            }

        }

        public void WriteSpecies()
        {

            foreach (String key in species.Keys)
            {
                Response.Write("<tr>");
                Response.Write("<td>");
                Response.Write(key);
                Response.Write("</td>");

                Response.Write("<td>");
                Response.Write("<a target='_blank' href='https://www.google.co.za/search?q=" + Server.UrlEncode(key + " site:marinespecies.org") + "'>Find</a>");
                Response.Write("</td>");

                Response.Write("<td>");
                Response.Write("<input type='text' />");
                Response.Write("</td>");


                Response.Write("</tr>");


            }

        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if ((c < '0' || c > '9') && c != '(' && c != ')' && c != ' ')
                    return false;
            }

            return true;
        }

        public void Trawl()
        {
            src = Request["url"];
            url = Request["url"];
            String srv = url;
            if (srv.IndexOf("jsonContent") == -1)
                srv += "/jsonContent?depth=-1&__ac_name=SpeciesDB&__ac_password=SpeciesDB";

            




            String filename = "temp/trawl.txt";
            String filepath = Server.MapPath(filename);
            using (var client = new WebClient())
            {
                client.DownloadFile(srv, filepath);
            }



            String code = File.ReadAllText(filepath);
//            Response.Write(code.Length);
  //          Response.End();




            var json_serializer = new JavaScriptSerializer();
            json_serializer.MaxJsonLength = 50000000;


            dynamic json = json_serializer.DeserializeObject(code);

            dynamic children = json["children"];

            for (int i = 0; i < children.Length; i++)
            {
                dynamic child = children[i];
                ParseChild(child);


            }


            foreach (String key in media.Keys)
            {
                String path = key;
                String title = media[key];

                String name = "" + title + "";
                String[] args = name.Split('_');


                int fsurvey = int.Parse(Request["fsurvey"]);
                int ftrawl = int.Parse(Request["ftrawl"]);
                int fdepth = int.Parse(Request["fdepth"]);
                int fspecies = int.Parse(Request["fspecies"]);


                if (args.Length >= 4)
                {
                    String survey = Request["survey"];
                    String trawl = ftrawl != -1 ? args[ftrawl] : "";
                    String depth = fdepth != -1 ? args[fdepth] : "";
                    String venacular = fspecies != -1 ? args[fspecies].Trim() : "";

                    String[] parts = venacular.Split('(');
                    if (parts.Length > 1)
                        venacular = parts[0];


                    if (venacular == "" || IsDigitsOnly(venacular) || venacular.IndexOf(".JP") != -1)
                    {
                        venacular = trawl;
                        trawl = "";
                        depth = "";
                    }

                    Response.Write("PATH: " + path + "<br>");
                    Response.Write("TITLE: " + title + "<br>");
                    Response.Write("SURVEY: " + survey + "<br>");
                    Response.Write("TRAWL: " + trawl + "<br>");
                    Response.Write("DEPTH: " + depth + "<br>");
                    Response.Write("NAME:" + venacular + "<br>");

                    int surveyID = GetSurveyID(survey);
                    Response.Write("SURVEYID: " + surveyID + "<br>");

                    if (surveyID > 0)
                    {
                        
                        int eventID = GetEventID(surveyID, trawl);
                        if (eventID > 0)
                        {
                            int mediaID = GetMediaID(eventID, venacular, path, title);
                            int speciesID = 0;

                            if (species.ContainsKey(venacular) == false)
                            {
                                int id = GetSpeciesID(venacular, false);
                                species[venacular] = id;
                                speciesID = id;
                            }
                            else
                            {
                                speciesID = species[venacular];
                            }

                            int speciesMedia = 0;
                            int speciesEvent = 0;

                            if (speciesID > 0)
                            {
                                speciesMedia = GetSpeciesMediaID(speciesID, mediaID);
                                // speciesEvent = GetSpeciesEventID(speciesID, eventID);
                            }

                            Response.Write("MEDIA: [" + mediaID + "]<br>");
                            Response.Write("EVENT: [" + eventID + "]<br>");
                            Response.Write("SPECIES: [" + speciesID + "]<br>");


                            Response.Write("<br>");
                            Response.Flush();
                        }
                    }
                }
            }
        }

        void ParseChild(dynamic parent)
        {
            // use title
            String path = Server.UrlDecode(parent["context_path"]).Replace(url, "");
            String title = Server.UrlDecode(parent["title"]).Replace(url, "");
            if (media.ContainsKey(path) == false)
            {
                String ext = Path.GetExtension(title).ToLower().Trim();

                if (ext == ".xlsx")
                    Response.Write("EXCEL FILE: " + path + "<br><br>");

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    media[path] = title;
            }

            dynamic children = parent["children"];
            for (int i = 0; i < children.Length; i++)
            {
                dynamic child = children[i];
                ParseChild(child);
            }
        }


        int GetSurveyID(String name)
        {
            String query = "SELECT * FROM dbo.TblSurvey WHERE fSurveyLabel = '" + name + "' OR fSurveyName LIKE '%" + name + "%'";
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
                            return 0;
                            /*
                            set.Close();
                            String sql = "INSERT INTO TblSurvey (fSurveyLabel, fSurveyName, fSurveyTypeRef) VALUES (@fSurveyLabel, @fSurveyName, 2)";
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fSurveyLabel", name);
                                cmd.Parameters.AddWithValue("@fSurveyName", name);

                                cmd.ExecuteNonQuery();
                                return GetSurveyID(name);
                            }
                             */
                        }
                    }
                }
            }
        }

        int GetSpecieLookupID(String commonName, String scientName)
        {
            String query = "SELECT fWormsID FROM TblWormsLookup WHERE fCommonName LIKE '" + commonName.Replace("'", "''") + "'";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                { 
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fWormsID"];
                        }
                        else
                        {
                            int wormID = GetWormsID(commonName, scientName);
                            set.Close();
                            String sql = "INSERT INTO TblWormsLookup (fCommonName, fWormsID) VALUES (@fCommonName, @fWormsID)";
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fCommonName", commonName);
                                cmd.Parameters.AddWithValue("@fWormsID", wormID);
                                cmd.ExecuteNonQuery();
                                return wormID;
                            }
                        }
                    }
                }
            }

        }

        int GetWormsID(String commonName, String scientificName)
        {
            try
            {
                Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();
                Worms.AphiaRecord[] records = null;

                // search by scientfic name
                List<string> scientificNames = new List<string>();
                scientificNames.Add(scientificName);
                string[] names = scientificNames.ToArray();
                records = client.getAphiaRecords(scientificName, true, true, true, 0);
                if (records != null && records.Length != 0)
                {
                    return records[0].AphiaID;
                }


                // search by common name
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

        public int GetSpeciesID(String name, bool force)
        {
            String query = "SELECT fSpeciesID FROM TblSpecies WHERE fSpeciesName LIKE '" + name.Replace("'", "''") + "' OR fCommonName LIKE '" + name.Replace("'", "''") + "'";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fSpeciesID"];
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }


        public int GetSpeciesIDAdv(String CommonName, String Genus, String Species, String Phylum, String Class, String Order, String Family, String FBcode)
        {
            String query = "SELECT fSpeciesID FROM TblSpecies WHERE fCommonName LIKE '" + CommonName.Replace("'", "''") + "'";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fSpeciesID"];
                        }
                        else
                        {
                            set.Close();

                            String speciesName = Genus.Trim() + " " + Species.Trim();
                            int aphiaID = GetSpecieLookupID(CommonName, speciesName);
                            if (aphiaID > 0)
                            {
                                GenericList gl = new GenericList(Context);
                                int nTaxon = int.Parse(gl.getTaxonID(aphiaID, CommonName));
                                String sql = "INSERT INTO TblSpecies (fTaxonomyID, fLSID, fWoRMID, fSpeciesName, fCommonName, fSpecies, fGenus, fPhylum, fClass, fOrder, fFamily, fFishBoard) VALUES (@fTaxonomyID, @fLSID, @fWoRMID, @fSpeciesName, @fCommonName, @fSpecies, @fGenus, @fPhylum, @fClass, @fOrder, @fFamily, @fFishBoard)";
                                using (SqlCommand cmd = new SqlCommand(sql, con))
                                {
                                    cmd.Parameters.AddWithValue("@fTaxonomyID", nTaxon);
                                    cmd.Parameters.AddWithValue("@fLSID", aphiaID);
                                    cmd.Parameters.AddWithValue("@fWoRMID", aphiaID);
                                    cmd.Parameters.AddWithValue("@fSpeciesName", speciesName);
                                    cmd.Parameters.AddWithValue("@fCommonName", CommonName);
                                    cmd.Parameters.AddWithValue("@fSpecies", Species);
                                    cmd.Parameters.AddWithValue("@fGenus", Genus);
                                    cmd.Parameters.AddWithValue("@fPhylum", Phylum);
                                    cmd.Parameters.AddWithValue("@fClass", Class);
                                    cmd.Parameters.AddWithValue("@fOrder", Order);
                                    cmd.Parameters.AddWithValue("@fFamily", Family);
                                    cmd.Parameters.AddWithValue("@fFishBoard", FBcode);
                                    cmd.ExecuteNonQuery();
                                    return GetSpeciesIDAdv(CommonName, Genus, Species, Phylum, Class, Order, Family, FBcode);
                                }
                            }
                            else
                            {
                                String sql = "INSERT INTO TblSpecies (fSpeciesName, fCommonName, fSpecies, fGenus, fPhylum, fClass, fOrder, fFamily, fFishBoard) VALUES (@fSpeciesName, @fCommonName, @fSpecies, @fGenus, @fPhylum, @fClass, @fOrder, @fFamily, @fFishBoard)";
                                using (SqlCommand cmd = new SqlCommand(sql, con))
                                {
                                    cmd.Parameters.AddWithValue("@fSpeciesName", speciesName);
                                    cmd.Parameters.AddWithValue("@fCommonName", CommonName);
                                    cmd.Parameters.AddWithValue("@fSpecies", Species);
                                    cmd.Parameters.AddWithValue("@fGenus", Genus);
                                    cmd.Parameters.AddWithValue("@fPhylum", Phylum);
                                    cmd.Parameters.AddWithValue("@fClass", Class);
                                    cmd.Parameters.AddWithValue("@fOrder", Order);
                                    cmd.Parameters.AddWithValue("@fFamily", Family);
                                    cmd.Parameters.AddWithValue("@fFishBoard", FBcode);
                                    cmd.ExecuteNonQuery();
                                    return GetSpeciesIDAdv(CommonName, Genus, Species, Phylum, Class, Order, Family, FBcode);
                                }
                            }
                        }
                    }
                }
            }




        }




        int GetMediaID(int eventID, String name, String path, String title)
        {
            String query = "SELECT fMediaID FROM TblMedia WHERE fEventID = @fEventID AND fMediaName = @fMediaName AND fMediaPath = @fMediaPath";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@fEventID", eventID);
                    command.Parameters.AddWithValue("@fMediaName", name);
                    command.Parameters.AddWithValue("@fMediaPath", path);
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fMediaID"];
                        }
                        else
                        {
                            set.Close();
                            String sql = "INSERT INTO TblMedia (fEventID, fMediaName, fMediaPath, fMediaType, fMediaTitle) VALUES (@fEventID, @fMediaName, @fMediaPath, @fMediaType, @fMediaTitle)";
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fEventID", eventID);
                                cmd.Parameters.AddWithValue("@fMediaName", name);
                                cmd.Parameters.AddWithValue("@fMediaPath", path);
                                cmd.Parameters.AddWithValue("@fMediaType", 1);
                                cmd.Parameters.AddWithValue("@fMediaTitle", title);

                                cmd.ExecuteNonQuery();
                                return GetMediaID(eventID, name, path, title);
                            }
                        }
                    }
                }
            }
        }

        void AddGenericStation()
        {
            String sql = "INSERT INTO TblStation (fStationName, fStartLat, fStartLng) VALUES (@fStationName, @fStartLat, @fStartLng)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    command.Parameters.AddWithValue("@fStationName", "General");
                    command.Parameters.AddWithValue("@fStartLat", 0);
                    command.Parameters.AddWithValue("@fStartLng", 0);
                    command.ExecuteNonQuery();
                }
            }
        }

        int GetGenericStation()
        {
            String sql = "SELECT fStationID FROM TblStation WHERE fStationName = 'General'";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fStationID"];
                        }
                        else
                        {
                            set.Close();
                            AddGenericStation();
                            return GetGenericStation();
                        }
                    }
                }
            }
        }



        void AddGenericEvent(int surveyID)
        {
            int stationID = GetGenericStation();
            String sql = "INSERT INTO TblEvent (fSurveyID, fStationID, fLevel2Name, fLevel3Name, fEventName, fStartLat, fStartLng) VALUES (@fSurveyID, @fStationID, @fLevel2Name, @fLevel3Name, @fEventName, @fStartLat, @fStartLng)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    command.Parameters.AddWithValue("@fSurveyID", surveyID);
                    command.Parameters.AddWithValue("@fStationID", stationID);
                    command.Parameters.AddWithValue("@fLevel2Name", "");
                    command.Parameters.AddWithValue("@fLevel3Name", "");
                    command.Parameters.AddWithValue("@fEventName", "General");
                    command.Parameters.AddWithValue("@fStartLat", 0);
                    command.Parameters.AddWithValue("@fStartLng", 0);
                    command.ExecuteNonQuery();
                }
            }
        }

        int GetGenericEvent(int surveyID)
        {
            String sql = "SELECT fEventID FROM TblEvent WHERE fEventName = 'General' AND fSurveyID = " + surveyID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fEventID"];
                        }
                        else
                        {
                            set.Close();
                            AddGenericEvent(surveyID);
                            return GetGenericEvent(surveyID);
                        }
                    }
                }
            }
        }

        int GetEventID(int surveyID, String name)
        {
            if (name.Trim() == "")
                return GetGenericEvent(surveyID);

            String query = String.Format("SELECT fEventID FROM TblEvent WHERE fSurveyID = {0} AND (fEventName = '{1}' OR fEventName LIKE '{2}-%')", surveyID, name, name.Substring(1));

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
                            Response.Write("** INVALID TRAWL: " + name + " **<br>");
                            Response.Write(query + "<br>");
                            return 0;
                        }
                    }
                }
            }
        }

        int GetSpeciesMediaID(int speciesID, int mediaID)
        {
            String query = String.Format("SELECT fSpeciesMediaID FROM TblSpeciesMedia WHERE fSpeciesID = {0} AND fMediaID = {1}", speciesID, mediaID);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            return (int)set["fSpeciesMediaID"];
                        }
                        else
                        {
                            set.Close();
                            String sql = "INSERT INTO TblSpeciesMedia (fSpeciesID, fMediaID, fCatalogueOrder) VALUES (@fSpeciesID, @fMediaID, @fCatalogueOrder)";
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fSpeciesID", speciesID);
                                cmd.Parameters.AddWithValue("@fMediaID", mediaID);
                                cmd.Parameters.AddWithValue("@fCatalogueOrder", 1000);
                                cmd.ExecuteNonQuery();
                                return GetSpeciesMediaID(speciesID, mediaID);
                            }
                        }
                    }
                }
            }
        }


        public int GetSpeciesEventID(int speciesID, int eventID, double abundance = 1)
        {
            String query = String.Format("SELECT fSpeciesEventID, fOccurance FROM TblSpeciesEvent WHERE fSpeciesID = {0} AND fEventID = {1}", speciesID, eventID);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            int speciesEventID = (int)set["fSpeciesEventID"];
                            int occurance = (int)set["fOccurance"];
                            set.Close();
                            return speciesEventID;
                        }
                        else
                        {
                            set.Close();
                            String sql = "INSERT INTO TblSpeciesEvent (fSpeciesID, fEventID, fOccurance) VALUES (@fSpeciesID, @fEventID, @fOccurance)";
                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@fSpeciesID", speciesID);
                                cmd.Parameters.AddWithValue("@fEventID", eventID);
                                cmd.Parameters.AddWithValue("@fOccurance", abundance);
                                
                                cmd.ExecuteNonQuery();
                                return GetSpeciesEventID(speciesID, eventID);
                            }
                        }
                    }
                }
            }
        }


    }
}