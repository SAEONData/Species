using System;

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Net.SourceForge.Koogra;
using System.Web.Script.Serialization;
using System.IO;

namespace species
{
    public partial class import : System.Web.UI.Page
    {
        public string formID = "";

        Dictionary<string, uint> fileColums = null;
        Dictionary<string, uint> mappedColumns = null;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["mode"] == "getfields")
            {
                GetFileFields(Request["file"]);
                Response.End();
            }

            if (Request["mode"] == "importExcel")
            {


                if (Request["importType"] == "catch")
                {
                    ImportExcelCatch();
                }
                else
                {
                    ImportExcelTrawl();
                }
                Response.End();
            }

        }

        public void WriteSurveyTypes()
        {
            String query = "SELECT * FROM dbo.TblSurveyType ORDER BY fSurveyTypeName";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Response.Write(String.Format("<option value='{0}'>{1}</option>", set["fSurveyTypeID"].ToString(), Server.HtmlEncode(set["fSurveyTypeName"].ToString())));
                        }
                    }
                }
            }

        }

        public void WriteIndicators()
        {
            String query = "SELECT * FROM TblIndicator ORDER BY fIndicatorName";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            Response.Write(String.Format("<option value='{0}'>{1}</option>", set["fIndicatorID"].ToString(), Server.HtmlEncode(set["fIndicatorName"].ToString())));
                        }
                    }
                }
            }

        }


        public void GetFileFields(String file)
        {
            String path = Server.MapPath(file);
            List<String> list = new List<String>();
            IWorkbook genericWB = WorkbookFactory.GetExcel2007Reader(path);
            IWorksheet genericWS = genericWB.Worksheets.GetWorksheetByIndex(0);
            uint r = genericWS.FirstRow;
            IRow row = genericWS.Rows.GetRow(r);
            for (uint c = genericWS.FirstCol; c <= genericWS.LastCol; ++c)
            {
                Object cell = row.GetCell(c).Value;
                if (cell != null)
                    list.Add(cell.ToString());
            }

            String[] items = list.ToArray();
            var json = new JavaScriptSerializer().Serialize(items);
            Response.Write(json);
        }

        Dictionary<string, uint> GetFileColumns(String path)
        {
            Dictionary<string, uint> cols = new Dictionary<string, uint>();
            IWorkbook genericWB = WorkbookFactory.GetExcel2007Reader(path);
            IWorksheet genericWS = genericWB.Worksheets.GetWorksheetByIndex(0);
            uint r = genericWS.FirstRow;
            IRow row = genericWS.Rows.GetRow(r);
            for (uint c = genericWS.FirstCol; c <= genericWS.LastCol; ++c)
            {
                String cell = row.GetCell(c).Value.ToString();
                if (cell != null)
                    cols[cell] = c;
            }
            return cols;
        }

        public double ParseCoord(String hour, String minute)
        {
            double h = double.Parse(hour);
            double m = double.Parse(minute);
            return h + m / 60.0;
        }

        public void ImportExcelCatch()
        {
            String path = Server.MapPath(Request["file"]);
            if (File.Exists(path) == false)
                die("File not found: " + path);
            fileColums = GetFileColumns(path);
            mappedColumns = new Dictionary<string, uint>();

            IWorkbook genericWB = WorkbookFactory.GetExcel2007Reader(path);
            IWorksheet genericWS = genericWB.Worksheets.GetWorksheetByIndex(0);

            for (uint r = genericWS.FirstRow + 1; r <= genericWS.LastRow; r++)
            {
                IRow row = genericWS.Rows.GetRow(r);
                String Cruise = GetCellValue(row, "catchCruise");
                String Trawl = GetCellValue(row, "catchTrawl");
                String Station = GetCellValue(row, "catchStation");
                String Common = GetCellValue(row, "catchCommon");
                String Genus = GetCellValue(row, "catchGenus");
                String Species = GetCellValue(row, "catchSpecies");
                String Abundance = GetCellValue(row, "catchAbundance");
                String Biomass = GetCellValue(row, "catchBiomass");
                String Description = GetCellValue(row, "catchDescription");

                if (Abundance.Trim() == "")
                    Abundance = "0";
                if (Biomass.Trim() == "")
                    Biomass = "0";

                Response.Write(Common + " " + Genus + " " + Species + ": ");
                manSpecies ms = new manSpecies();
                int species = ms.GetSpeciesID(Genus, Species, Common);
                if (species == 0)
                {
                    Response.Write("Failed to add species");
                }
                else
                {
                    manEvents me = new manEvents();
                    int eventID = me.FindEventID(Cruise, Trawl, Station);
                    if (eventID == 0)
                        die(String.Format("Event not found: {0}, {1}, {2}", Cruise, Trawl, Station));

                    manEventSpecies mes = new manEventSpecies();
                    mes.addEventSpecies(eventID, species, double.Parse(Abundance), double.Parse(Biomass), Description);


                }

                Response.Write("<br>");


                Response.Flush();



            }


        }


        public void ImportExcelTrawl()
        {
            String path = Server.MapPath(Request["file"]);
            if (File.Exists(path) == false)
                die("File not found: " + path);
            fileColums = GetFileColumns(path);
            mappedColumns = new Dictionary<string, uint>();


            // resolve indicators
            Dictionary<uint, int> findicators = new Dictionary<uint, int>();
            if (Request["indicators"] != null && Request["indicators"] != "")
            {
                String[] sindicators = Request["indicators"].Split('~');
                foreach (String indicator in sindicators)
                {
                    String[] indic = indicator.Split('|');
                    int indicatorID = int.Parse(indic[0]);
                    uint column = fileColums[indic[1]];
                    findicators[column] = indicatorID;
                }
            }

            

            IWorkbook genericWB = WorkbookFactory.GetExcel2007Reader(path);
            IWorksheet genericWS = genericWB.Worksheets.GetWorksheetByIndex(0);

            for (uint r = genericWS.FirstRow + 1; r <= genericWS.LastRow; r++)
            {
                IRow row = genericWS.Rows.GetRow(r);

                String survey = GetCellValue(row, "fieldSurvey");
                String cruise = GetCellValue(row, "fieldCruise");
                String trawl = GetCellValue(row, "fieldTrawl");
                String Station = GetCellValue(row, "fieldStation");
                String sLatHour = GetCellValue(row, "sLatHour");
                String sLatMin = GetCellValue(row, "sLatMin");
                String sLonHour = GetCellValue(row, "sLonHour");
                String sLonMin = GetCellValue(row, "sLonMin");
                String eLatHour = GetCellValue(row, "eLatHour");
                String eLatMin = GetCellValue(row, "eLatMin");
                String eLonHour = GetCellValue(row, "eLonHour");
                String eLonMin = GetCellValue(row, "eLonMin");
                String comments = GetCellValue(row, "Comments");
                int dateYear = int.Parse(GetCellValue(row, "dateYear"));
                int dateMonth = int.Parse(GetCellValue(row, "dateMonth"));
                int dateDay = int.Parse(GetCellValue(row, "dateDay"));
                double duration = double.Parse(GetCellValue(row, "Duration"));

                Dictionary<int, string> indicators = new Dictionary<int, string>();
                foreach (uint col in findicators.Keys)
                {
                    int indicator = findicators[col];
                    indicators[indicator] = GetCellValue(row, col);
                    Response.Write("Row = " + r + ", Indicator: " + indicator + " = " + indicators[indicator] + "<br>");
                }

                double lat1 = -ParseCoord(sLatHour, sLatMin);
                double lon1 =  ParseCoord(sLonHour, sLonMin);
                double lat2 = -ParseCoord(eLatHour, eLatMin);
                double lon2 =  ParseCoord(eLonHour, eLonMin);

                manSurveys sv = new manSurveys();
                int nSurveyID = sv.GetSurveyID(2, survey);
                if (nSurveyID < 1)
                    die("Failed to create survey");

                manStations st = new manStations();
                int nStationID = st.GetStationID(Station, lat1, lon1);
                if (nStationID < 1)
                    die("Failed to create station");

                manEvents ev = new manEvents();
                ev.res = Response;
                int nEventID = ev.GetEventID(nSurveyID, nStationID, dateYear, dateMonth, dateDay, cruise, trawl, lat1, lon1, lat2, lon2, duration, comments, indicators);



            }


            Response.Write("cools!");
        }

        public String GetCellValue(IRow row, uint col)
        {
            object cell = row.GetCell(col).Value;
            return cell != null ? cell.ToString() : "";
        }

        public String GetCellValue(IRow row, String name)
        {
            uint index = GetColumnIndex(name);
            return GetCellValue(row, index);
        }


        public uint GetColumnIndex(String name)
        {
            if (mappedColumns.Keys.Contains(name))
                return mappedColumns[name];
            String field = Request[name];
            if (field == null || field == "")
                die("Invalid column selected for field: " + name);
            if (fileColums.ContainsKey(field) == false)
                die("Invalid column selected for field: " + name + ", column index: " + field);
            uint index = fileColums[field];
            mappedColumns[name] = index;
            return index;
        }

        public void die(String msg)
        {
            Response.Write(msg);
            Response.End();
        }

    }
}