using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class events : System.Web.UI.Page
    {
        public String level2Name = "";
        public string level3Name = "";

        public List<ComboItem> level2Names = null;
        public List<ComboItem> stations = null;

        public void WriteComboItems(List<ComboItem> items, bool writaAll)
        {
            if (writaAll == true)
                Response.Write("<option>All</option>");
            foreach (ComboItem item in items)
                Response.Write(String.Format("<option value='{0}'>{1}</option>", Server.HtmlEncode(item.val), Server.HtmlEncode(item.name)));
        }

        protected List<ComboItem> GetStations()
        {
            List<ComboItem> list = new List<ComboItem>();
            String query = "SELECT fStationID, fStationName FROM dbo.TblStation WHERE fSystem = 0 ORDER BY fStationName";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            ComboItem item = new ComboItem();
                            item.val = set[0].ToString();
                            item.name = set[1].ToString();
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }


        protected List<ComboItem> GetLevel2Names(int surveyID)
        {
            List<ComboItem> list = new List<ComboItem>();
            String query = String.Format("SELECT fLevel2Name FROM TblEvent WHERE (fSurveyID = {0}) AND (fLevel2Name IS NOT NULL) GROUP BY fLevel2Name", surveyID);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            ComboItem item = new ComboItem();
                            item.val = set[0].ToString();
                            item.name = set[0].ToString();
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            string survey = Request["sv"];
            if (survey == null || survey == "")
            {
                if (Session["fSurveyID"] != null)
                    survey = Session["fSurveyID"].ToString();
                if (survey == null || survey == "")
                    survey = GetFirstSurveyID();
            }
            fSurveyID = int.Parse(survey);
            Session["fSurveyID"] = survey;

            level2Names = GetLevel2Names(fSurveyID);
            stations = GetStations();



            SurveyInfo info = GetSurveyInfo(fSurveyID);


            String query = "";
            String filter = Request["filter"];
            if (filter != null && filter != "")
            {
                String[] filters = filter.Split('|');
                foreach (String f in filters)
                {
                    String[] ftr = f.Split('=');
                    String name = ftr[0];
                    String val = ftr[1].Replace("'", "''");
                    if (val != "" && val != "All")
                    {
                        if (query != "")
                            query += " AND ";
                        switch (name)
                        {
                            case "level2":
                                query += "fLevel2Name = '" + val + "'";
                                break;
                            case "level3":
                                query += "fLevel3Name = '" + val + "'";
                                break;
                            case "station":
                                if (double.Parse(val) > 0)
                                {
                                    double max = double.Parse(val) * 100;
                                    double min = max - 100; 
                                    query += "fDepth  >= " + min + " AND fDepth <= " + max;
                                }
                                break;
                            case "date1":
                                query += "fStartDate >= '" + val + "'";
                                break;
                            case "date2":
                                query += "fStartDate <= '" + val + "'";
                                break;
                        }
                    }
                }
            }



            list = new GenericList(Context);
            list.type = "Event";
            list.table = "TblEvent";
            list.idField = "fEventID";

            list.fields.Add(new Field("fSurveyID", "SurveyID", FieldType.Constant, 0, 0, fSurveyID.ToString()));


            if (info.type.fSurveyLevel2Name.Length > 0)
            {
                list.fields.Add(new Field("fLevel2Name", info.type.fSurveyLevel2Name, FieldType.String, 80, 100));
                level2Name = info.type.fSurveyLevel2Name;
            }

            if (info.type.fSurveyLevel3Name.Length > 0)
            {
                list.fields.Add(new Field("fLevel3Name", info.type.fSurveyLevel3Name, FieldType.String, 80, 100));
                level3Name = info.type.fSurveyLevel3Name;
            }

            list.fields.Add(new Field("fEventName", "Event", FieldType.String, 80, 100));

            Field station = new Field("fStationID", "Station", FieldType.Combo, 50, 50);
            station.lookUpTable = "TblStation";
            station.lookUpFieldID = "fStationID";
            station.lookUpFieldName = "fStationName";
            station.lookUpFilter = "fSystem = 0";
            list.fields.Add(station);

            list.fields.Add(new Field("fStartDate", "Date", FieldType.Date, 80, 0));

            list.fields.Add(new Field("fDepth", "Depth", FieldType.String, 80, 0));
            list.fields.Add(new Field("fDuration", "Duration", FieldType.String, 80, 0));
            list.exportButton = true;
            


            list.dataSavedEvent = SaveIndicators;
            list.customEditDialog = true;
            list.listFilter = "fSystemEvent = 0 AND fSurveyID = " + fSurveyID;
            if (query != "")
                list.listFilter += " AND " + query;

            list.showAllButton = true;

            list.InitList(Context);

            

        }

        public void SaveIndicators(int id, HttpRequest req, HttpResponse res)
        {
            SurveyInfo info = GetSurveyInfo(fSurveyID);
            List<Indicator> indicators = LoadIndicators(info.fSurveyTypeRef, id, false);
            foreach (Indicator indicator in indicators)
            {
                list.ExecuteSQL(String.Format("DELETE FROM TblEventIndicator WHERE fEventID={0} AND fIndicatorID={1}", id, indicator.id));

                String sql = "INSERT INTO TblEventIndicator (fEventID, fIndicatorID, fValue) VALUES (@fEventID, @fIndicatorID, @fValue)";
                using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("@fEventID", id);
                        command.Parameters.AddWithValue("@fIndicatorID", indicator.id);
                        command.Parameters.AddWithValue("@fValue", Request["ff" + indicator.id]);
                        command.ExecuteNonQuery();
                    }
                }


            }



        }

        public string GetFirstSurveyID()
        {
            string fSurveyID = "";
            String sql = "SELECT * FROM dbo.TblSurvey WHERE fSystem = 0 ORDER BY fSurveyLabel";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("No surveys found");
                            Response.End();
                        }
                        fSurveyID = set["fSurveyID"].ToString();
                    }
                }
            }
            return fSurveyID;
        }


        public void WriteSurveys()
        {
            String sql = "SELECT * FROM dbo.TblSurvey WHERE fSystem = 0 ORDER BY fSurveyLabel";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            int id = int.Parse(set["fSurveyID"].ToString());
                            string selected = (id == fSurveyID) ? " selected " : "";
                            Response.Write("<option value='" + set["fSurveyID"].ToString() + "' " + selected + ">" + Server.HtmlEncode(set["fSurveyLabel"].ToString() + ": " + set["fSurveyDesc"].ToString()) + "</option>");
                        }
                    }
                }
            }
        }

        public SurveyInfo GetSurveyInfo(int nSurveyID)
        {
            SurveyInfo info = new SurveyInfo();
            String sql = "SELECT * FROM dbo.TblSurvey WHERE fSurveyID = " + nSurveyID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("No surveys found");
                            Response.End();
                        }
                        info.fSurveyLabel = (string)set["fSurveyLabel"];
                        info.fSurveyDesc = (string)set["fSurveyName"];
                        info.fSurveyTypeRef = (int)set["fSurveyTypeRef"];
                        info.type = GetSurveyTypeInfo(info.fSurveyTypeRef);
                    }
                }
            }
            return info;
        }

        public SurveyTypeInfo GetSurveyTypeInfo(int nSurveyTypeID)
        {
            SurveyTypeInfo info = new SurveyTypeInfo();
            String sql = "SELECT * FROM TblSurveyType WHERE fSurveyTypeID = " + nSurveyTypeID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("No surveys found");
                            Response.End();
                        }
                        info.fSurveyTypeName = (string)set["fSurveyTypeName"];
                        info.fSurveyLevel2Name = (string)set["fSurveyLevel2Name"];
                        info.fSurveyLevel3Name = (string)set["fSurveyLevel3Name"];
                    }
                }
            }
            return info;
        }

        public String GetIndicatorValue(int nIndicatorID, int nEventID)
        {
            String value = "";
            String sql = String.Format("SELECT fValue FROM dbo.TblEventIndicator WHERE fIndicatorID = {0} AND fEventID = {1}", nIndicatorID, nEventID);

            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                            value = set["fValue"].ToString();
                    }
                }
            }
            return value;
        }

        public List<Indicator> LoadIndicators(int nSurveyType, int nEventID, bool loadValues)
        {
            List<Indicator> list = new List<Indicator>();

            String sql = "SELECT * FROM dbo.vwIndicators WHERE fSurveyTypeID = " + nSurveyType + " ORDER BY fIndicatorID";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Indicator ind = new Indicator();
                            ind.id = (int)set["fIndicatorID"];
                            ind.name = (string)set["fIndicatorName"];
                            ind.unit = (string)set["fUnitName"];
                            ind.abbr = (string)set["fUnitABBR"];
                            ind.unitID = (int)set["fUnitID"];
                            ind.value = loadValues ? GetIndicatorValue(ind.id, nEventID) : "";
                            list.Add(ind);
                        }
                    }
                }
            }
            return list;
        }



        public int fSurveyID;
        public GenericList list;
    }

    public class SurveyInfo
    {
        public int fSurveyTypeRef;
        public String fSurveyLabel;
        public String fSurveyDesc;
        public SurveyTypeInfo type;
    }

    public class SurveyTypeInfo
    {
        public String fSurveyTypeName;
        public String fSurveyLevel2Name;
        public String fSurveyLevel3Name;
    }

    public class Indicator
    {
        public int id;
        public String name;
        public String value;
        public String unit;
        public String abbr;
        public int unitID;
    }

    public class ComboItem
    {
        public string val;
        public string name;
    }



}