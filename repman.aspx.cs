using species;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BLA
{
    public partial class repman : System.Web.UI.Page
    {
        SortedDictionary<DateTime, SpeciesRecords> master = new SortedDictionary<DateTime, SpeciesRecords>();

        SortedDictionary<string, int> species = new SortedDictionary<string, int>();
        Dictionary<int, Transect> transects = new Dictionary<int, Transect>();


        protected void Page_Load(object sender, EventArgs e)
        {
            String transects = Request["items"];

            if (Request["trm"] != null && Request["trm"] != "" && Request["items"] != null && Request["items"] != "")
            {
                String trm = Request["trm"];

                String filter = "1 = 0";
                String[] items = Request["items"].Split(',');
                foreach (String t in items)
                {
                    if (t.Trim() != "")
                    {
                        switch (trm)
                        {
                            case "vessel":
                                filter += " OR VesselID = " + int.Parse(t);
                                break;
                            case "cruise":
                                filter += " OR fSurveyID = " + int.Parse(t);
                                break;
                            case "transect":
                                filter += " OR fStationID = " + int.Parse(t);
                                break;
                        }
                    }
                }

                SpeciesRecords recs = LoadTransects(filter);
                for (var i = 0; i < recs.records.Count; i++)
                {
                    SpeciesRepItem record = recs.records[i];
                    if (master.ContainsKey(record.date) == false)
                        master[record.date] = new SpeciesRecords();
                    master[record.date].records.Add(record);
                }
            }
            else
            {
                String species = Request["species"];
                if (species != null && species != "")
                {
                    String[] items = species.Split(',');
                    foreach (String item in items)
                    {
                        SpeciesRecords recs = LoadSpeciesSeries(int.Parse(item));
                        for (var i = 0; i < recs.records.Count; i++)
                        {
                            SpeciesRepItem record = recs.records[i];
                            if (master.ContainsKey(record.date) == false)
                                master[record.date] = new SpeciesRecords();
                            master[record.date].records.Add(record);
                        }
                    }
                }
            }
        }

        public void WriteReportData()
        {
            List<int> filteredSpecies = new List<int>();
            String filter = "";

            if (Request["trm"] != null && Request["trm"] != "" && Request["items"] != null && Request["items"] != "")
            {
                String trm = Request["trm"];

                filter = "1 = 0";
                String[] items = Request["items"].Split(',');
                foreach (String t in items)
                {
                    if (t.Trim() != "")
                    {
                        switch (trm)
                        {
                            case "vessel":
                                filter += " OR VesselID = " + int.Parse(t);
                                break;
                            case "cruise":
                                filter += " OR CruiseID = " + int.Parse(t);
                                break;
                            case "transect":
                                filter += " OR fStationID = " + int.Parse(t);
                                break;
                        }

                    }
                }

                String[] species = Request["species"].Split(',');
                foreach (String s in species)
                {
                    if (s.Trim() != "")
                    {
                        int speciesID = int.Parse(s);
                        if (filteredSpecies.Contains(speciesID) == false)
                            filteredSpecies.Add(speciesID);
                    }
                }
            }
            else
            {
                filter = "1 = 0";
                String[] species = Request["species"].Split(',');
                foreach (String s in species)
                {
                    if (s.Trim() != "")
                    {
                        filter += " OR SpeciesID = " + int.Parse(s);
                    }
                }
            }


            List<GrahpRecord> records = new List<GrahpRecord>();


            String query = "SELECT SUM(Observation.Flying) AS flying, SUM(Observation.Sitting) AS sitting, Transect.Date FROM Observation INNER JOIN Transect ON Observation.fStationID = Transect.fStationID WHERE (" + filter + ") GROUP BY dbo.Transect.Date";
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            DateTime date = (DateTime)set["fStartDate"];
                            GrahpRecord record = new GrahpRecord();
                            record.date = String.Format("{0}|{1}|{2})", date.Year, date.Month, date.Day);
                            record.sitting = (int)set["sitting"];
                            record.flying = (int)set["flying"];
                            records.Add(record);
                        }
                    }
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Response.Write(js.Serialize(records));

        }

        public void WriteData()
        {
            List<RecordDate> dates = new List<RecordDate>();

            foreach (DateTime date in master.Keys)
            {
                RecordDate rdate = new RecordDate();
                rdate.date = String.Format("new Date({0},{1},{2},{3},{4},{5})", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

                SpeciesRecords recs = master[date];
                foreach (SpeciesRepItem item in recs.records)
                {
                    rdate.items.Add(item);
                }


                dates.Add(rdate);
            }



            JavaScriptSerializer js = new JavaScriptSerializer();
            Response.Write(js.Serialize(dates));
        }


        public void WriteTable()
        {
            foreach (DateTime date in master.Keys)
            {
                SpeciesRecords recs = master[date];
                foreach (SpeciesRepItem item in recs.records)
                {
                    Response.Write("<tr>");

                    Response.Write("<td>" + date.ToString("yyyy-MM-dd") + "</td>");
                    Response.Write("<td>" + date.ToString("HH:mm:ss") + "</td>");

                    Response.Write("<td>" + Server.HtmlEncode(item.commonName) + "</td>");
                    Response.Write("<td>" + Server.HtmlEncode(item.scientName) + "</td>");
                    Response.Write("<td>" + Server.HtmlEncode(item.flying.ToString()) + "</td>");
                    Response.Write("<td>" + Server.HtmlEncode(item.sitting.ToString()) + "</td>");

                    Response.Write("</tr>");
                }





            }


        }

        SpeciesRecords LoadTransects(String filter)
        {
            List<int> filteredSpecies = new List<int>();
            String[] species = Request["species"].Split(',');
            foreach (String s in species)
            {
                if (s.Trim() != "")
                {
                    int speciesID = int.Parse(s);
                    if (filteredSpecies.Contains(speciesID) == false)
                        filteredSpecies.Add(speciesID);
                }
            }
            if (filteredSpecies.Count < 1)
                filteredSpecies = null;

            String query = "SELECT * FROM vwSpeciesEvent WHERE fStartLat IS NOT NULL AND fStartLng IS NOT NULL AND (" + filter + filtertools.BuildFilter(Context) + ") ORDER BY fStationID, fSpeciesID";
//            query += filtertools.BuildFilter(Context);
            // Response.Write(query);
            return LoadRecords(query, filteredSpecies);
        }

        SpeciesRecords LoadSpeciesSeries(int speciesID)
        {
            String query = "SELECT * FROM vwSpeciesEvent WHERE fStartLat IS NOT NULL AND fStartLng IS NOT NULL AND fSpeciesID = " + speciesID;
            query += filtertools.BuildFilter(Context);
            return LoadRecords(query, null);
        }

        SpeciesRecords LoadRecords(String query, List<int> filteredSpecies)
        {
            int prevTransect = -1;
            bool speciesInTransect = true;
            SpeciesRepItem prevItem = null;


            SpeciesRecords records = new SpeciesRecords();
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            int fStationID = (int)set["fStationID"];
                            DateTime date = (DateTime)set["fStartDate"];
                            DateTime time1 = new DateTime(2010, 1, 1, 12, 0, 0);
                            DateTime time2 = new DateTime(2010, 1, 1, 12, 0, 0);

                            if (transects.ContainsKey(fStationID) == false)
                            {
                                Transect tr = new Transect();
                                tr.Date = String.Format("{0}-{1}-{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2"));
                                tr.StartTime = String.Format("{0}:{1}:{2}", time1.Hour.ToString("D2"), time1.Minute.ToString("D2"), time1.Second.ToString("D2"));
                                tr.EndTime = String.Format("{0}:{1}:{2}", time2.Hour.ToString("D2"), time2.Minute.ToString("D2"), time2.Second.ToString("D2"));
                                tr.StartLat = set["fStartLat"].ToString();
                                tr.StartLong = set["fStartLng"].ToString();
                                tr.EndLat = set["fEndLat"].ToString();
                                tr.EndLong = set["fEndLng"].ToString();

                                tr.Cruise = set["fSurveyLabel"].ToString();
                                tr.transect = set["fLevel3Name"].ToString();


                                transects[fStationID] = tr;
                            }

                            Transect trans = transects[fStationID];

                            int speciesID = (int)set["fSpeciesID"];
                            if (filteredSpecies == null || filteredSpecies.Contains(speciesID))
                            {
                                String speciesName = set["fCommonName"].ToString();

                                species[speciesName] = speciesID;

                                if (trans.counts.ContainsKey(speciesID) == false)
                                    trans.counts[speciesID] = new TCount();
                                TCount tc = trans.counts[speciesID];

                                if (set["fOccurance"] != DBNull.Value)
                                {
                                    int flying = (int)set["fOccurance"];
                                    if (tc.flying == "")
                                        tc.flying = "0";
                                    tc.flying = (int.Parse(tc.flying) + flying).ToString();
                                }

                            }







                            if (prevTransect != fStationID)
                            {
                                

                                if (speciesInTransect == false)
                                {
                                    if (filteredSpecies != null && prevItem != null)
                                    {
                                        // add null species record
                                        prevItem.commonName = "None observed";
                                        prevItem.scientName = "None observed";
                                        prevItem.flying = 0;
                                        prevItem.sitting = 0;
                                        records.records.Add(prevItem);
                                    }

                                }

                                prevTransect = fStationID;
                                speciesInTransect = false;
                            }

                            



                            SpeciesRepItem item = new SpeciesRepItem();
                            item.species = (int)set["fSpeciesID"];
                            item.commonName = set["fCommonName"].ToString();
                            item.scientName = set["fSpeciesName"].ToString();

                            item.flying = (int)set["fOccurance"];

                            DateTime time = new DateTime(2010, 1, 1, 12, 0, 0);
                            item.date = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

                            item.startLat = (double)set["fStartLat"];
                            item.startLng = (double)set["fStartLng"];
                            item.endLat = set["fEndLat"].ToString();
                            item.endLng = set["fEndLng"].ToString();
                            item.LatitudeEnd = set["fEndLat"].ToString();
                            item.LongitudeEnd = set["fEndLng"].ToString();


                            if (filteredSpecies == null || filteredSpecies.Contains(item.species))
                            {
                                speciesInTransect = true;
                                records.records.Add(item);
                            }

                            prevItem = item;
                        }
                    }
                }
            }

            if (speciesInTransect == false)
            {
                if (filteredSpecies != null && prevItem != null)
                {
                    // add null species record
                    prevItem.commonName = "None observed";
                    prevItem.scientName = "None observed";
                    prevItem.flying = 0;
                    prevItem.sitting = 0;
                    records.records.Add(prevItem);
                }
            }


            return records;
        }

        public String CSVString(string intext)
        {
            if (intext == null)
                return "";
            if (intext.IndexOf(',') != -1 || intext.IndexOf('"') != -1)
            {
                intext = intext.Replace("\"", "\"\"");
                return "\"" + intext + "\"";
            }
            return intext;
        }

        public void WriteCSVFile() 
        {
            String seperator = Request["seperator"].Trim();
            String header = "Date,Survey,Trawl,Start Lat,Start Long, End Lat, End Long,";
            foreach (String s in species.Keys)
            {
                header += CSVString(s) + ",";
            }
            header = header.Replace(",", seperator);
            Response.Write(header + "\r\n");

            foreach (int key in transects.Keys)
            {
                Transect tr = transects[key];

                Response.Write(CSVString(tr.Date) + seperator);
                // Response.Write(CSVString(tr.StartTime) + seperator);
                // Response.Write(CSVString(tr.EndTime) + seperator);
                Response.Write(CSVString(tr.Cruise) + seperator);
                Response.Write(CSVString(tr.transect) + seperator);
                Response.Write(CSVString(tr.StartLat) + seperator);
                Response.Write(CSVString(tr.StartLong) + seperator);
                Response.Write(CSVString(tr.EndLat) + seperator);
                Response.Write(CSVString(tr.EndLong) + seperator);

                foreach (String sp in species.Keys)
                {
                    int speciesID = species[sp];
                    if (tr.counts.ContainsKey(speciesID))
                    {
                        TCount tc = tr.counts[speciesID];
                        Response.Write(tc.flying + seperator);
                    }
                    else
                    {
                        Response.Write(seperator);
                    }
                }


                Response.Write("\r\n");

            }

            

            /*
            foreach (DateTime date in master.Keys)
            {
                SpeciesRecords recs = master[date];
                foreach (SpeciesRepItem item in recs.records)
                {
                    Response.Write(date.ToString("yyyy-MM-dd") + seperator);
                    Response.Write(date.ToString("HH:mm:ss") + seperator);

                    Response.Write(CSVString(item.Taxonomic_Genus) + seperator);
                    Response.Write(CSVString(item.Taxonomic_Species) + seperator);

                    Response.Write(CSVString(item.English_Genus) + seperator);
                    Response.Write(CSVString(item.English_Species) + seperator);

                    Response.Write(CSVString(item.Taxon_order) + seperator);
                    Response.Write(CSVString(item.Taxon_family) + seperator);
                    Response.Write(CSVString(item.Taxon_group) + seperator);
                    Response.Write(CSVString(item.Continent) + seperator);
                    Response.Write(CSVString(item.Region) + seperator);

                    Response.Write(CSVString(item.flying.ToString()) + seperator);
                    Response.Write(CSVString(item.sitting.ToString()) + seperator);
                    Response.Write(CSVString(item.observer1) + seperator);
                    Response.Write(CSVString(item.observer2) + seperator);
                    Response.Write(CSVString(item.vessel) + seperator);
                    Response.Write(CSVString(item.cruise) + seperator);
                    Response.Write(CSVString(item.transect) + seperator);
                    Response.Write(item.startLat.ToString() + seperator);
                    Response.Write(item.startLng.ToString() + seperator);
                    Response.Write(CSVString(item.endLat) + seperator);
                    Response.Write(CSVString(item.endLng) + seperator);
                    Response.Write(CSVString(item.CountType) + seperator);
                    Response.Write(CSVString(item.WindDirection) + seperator);
                    Response.Write(CSVString(item.WindDirection_deg) + seperator);
                    Response.Write(CSVString(item.WindStrength_kmph) + seperator);
                    Response.Write(CSVString(item.CloudCover) + seperator);
                    Response.Write(CSVString(item.Visibility_m) + seperator);
                    Response.Write(CSVString(item.Precipitation) + seperator);
                    Response.Write(CSVString(item.CountingShipFollowers) + seperator);
                    Response.Write(CSVString(item.SwellHeight_m) + seperator);
                    Response.Write(CSVString(item.CoordinateSystem) + seperator);
                    Response.Write(CSVString(item.CountArea) + seperator);
                    Response.Write(CSVString(item.ArcFromBow) + seperator);
                    Response.Write(CSVString(item.PositionOnVessel) + seperator);
                    



                    Response.Write("\r\n");
             
                }
            }
             */
        }

        class SpeciesRecords
        {
            public List<SpeciesRepItem> records = new List<SpeciesRepItem>();
        }

        class SpeciesRepItem
        {
            public int species;
            public string commonName;
            public string scientName;

            public int flying;
            public int sitting;
            public DateTime date;
            public double startLat;
            public double startLng;
            public string endLat;
            public string endLng;
            public string LatitudeEnd;
            public string LongitudeEnd;
        }

        class RecordDate
        {
            public string date;
            public List<SpeciesRepItem> items = new List<SpeciesRepItem>();
        }

        class GrahpRecord
        {
            public string date;
            public int flying;
            public int sitting;
            
        };

        class TCount
        {
            public string flying = "";
            public string sitting = "";
        }

        class Transect
        {
            public String Date;
            public String StartTime;
            public String EndTime;
            public String Cruise;
            public String transect;
            public String StartLat;
            public String StartLong;
            public String EndLat;
            public String EndLong;
            public Dictionary<int, TCount> counts = new Dictionary<int,TCount>();
        }

            


        class RecordCount
        {
            public int sitting = 0;
            public int flying = 0;
        }

    }
}