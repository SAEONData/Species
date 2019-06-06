using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class parsemedia2 : System.Web.UI.Page
    {
        public SortedDictionary<String, int> media = new SortedDictionary<string, int>();
        public String survey = "";
        public String trawlLog = "";
        public String catchLog = "";
        

        public String url;
        public List<String> fileParts = new List<String>();
        public String sampleFileName = "";


        protected void Page_Load(object sender, EventArgs e)
        {
            url = Request["url"];


            String srv = url;
            if (srv.IndexOf("jsonContent") == -1)
                srv += "/jsonContent?depth=-1&__ac_name=SpeciesDB&__ac_password=SpeciesDB";

            


            String name = "temp/mf.txt";
            String path = Server.MapPath(name);
            using (var client = new WebClient())
            {
                client.DownloadFile(srv, path);
            }

//            path = Server.MapPath("trawl.txt");
            String code = File.ReadAllText(path);
 //           File.Delete(path);

            // Response.Write(url + "<br><br>");
            // Response.Write(srv + "<br><br>");
           // Response.Write(code + "<br><br>");

     //       Response.End();


            try
            {
                var json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = 50000000;
                dynamic json = json_serializer.DeserializeObject(code);

                dynamic children = json["children"];

                for (int i = 0; i < children.Length; i++)
                {
                    dynamic child = children[i];
                    ParseChild(child);
                }

                if (catchLog != "")
                    survey = GetSurveyName(catchLog);
            }
            catch (Exception err)
            {
                Response.Write(err.Message + "<hr>");
                Response.Write(srv + "<hr>");
                Response.Write(code + "<hr>");
            }
        }

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

                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }

                // Loop through all of the sheets if you want too...
                for (int j = 0; j < excelSheets.Length; j++)
                {
                    // Query each excel sheet.
                }

                return excelSheets;
            }
            catch (Exception ex)
            {
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

        public String GetSurveyName(String src)
        {
            String url = src + "?__ac_name=SpeciesDB&__ac_password=SpeciesDB";

            String name = "temp/sheet.xlsx"; 
            String path = Server.MapPath(name);
            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }

            String[] sheets = GetExcelSheetNames(path);
            if (sheets.Length == 0)
            {
                Response.Write("No sheets in workbook: " + path + "<br>");
                Response.End();
            }

            // for (int i = 0; i < sheets.Length; i++)
            //    Response.Write(sheets[i] + "<br>");

            String sheet = sheets[0];

            
            string con = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path  + "; Extended Properties='Excel 8.0;HDR=Yes;'";
            con = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + "; Extended Properties=Excel 12.0;";
            String survey = "";
            using (OleDbConnection connection = new OleDbConnection(con))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("select * from [" + sheet + "]", connection);
                using (OleDbDataReader dr = command.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        survey = dr[0].ToString();
                    }
                }
            }

            return survey;
        }


        String GetFileName(String href)
        {
            String filename = "";
            // Uri uri = new Uri(href);
            filename = System.IO.Path.GetFileNameWithoutExtension(href);
            return filename;
        }

        public void WritePartOptions()
        {
            for (int i = 0; i < fileParts.Count; i++)
            {
                Response.Write("<option value='" + i + "'>" + Server.HtmlEncode(fileParts[i]) + "</option>");
            }
            Response.Write("<option value='-1'>Not Available</option>");
        }



        void ParseChild(dynamic parent)
        {
            String path = Server.UrlDecode(parent["context_path"]);
            String title = Server.UrlDecode(parent["title"]);
            if (media.ContainsKey(path) == false)
            {
                String hach = path.Replace("http://", "c:\\");
                hach = hach.Replace("/", "\\");
                String ext = Path.GetExtension(title).ToLower().Trim();

                if (ext == ".xlsx" || ext == ".xls")
                {
                    // Response.Write("found excel: " + title + "<br>");

                    if (title.ToLower().IndexOf("trawl log") != -1)
                        trawlLog = path;
                    else
                        catchLog = path;
                }

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    media[path] = 1;
                    if (fileParts.Count == 0)
                    {
                        String name = title;
                        
                        String[] parts = name.Split('_');

//                        Response.Write(name + "<br>");


                        if (sampleFileName == "")
                        {
                            if (parts.Length >= 4)
                            {
                                bool allparts = true;
                                for (int p = 0; p < parts.Length; p++)
                                {
                                    String part = parts[p];
                                    if (part.Trim().Length == 0)
                                        allparts = false;

                                    if (p == 1)
                                    {
                                        if (part[0] != 'T' || part[1] < '0' || part[1] > '9')
                                            allparts = false;

                                    }

                                    if (p == 2)
                                    {
//                                        if (part[part.Length - 1] != 'm')
//                                            allparts = false;
                                    }
                                }

                                if (allparts == true)
                                {
                                    sampleFileName = name;

                                    foreach (String part in parts)
                                        fileParts.Add(part);
                                }
                            }

                        }
                    }
                }
            }

            dynamic children = parent["children"];
            for (int i = 0; i < children.Length; i++)
            {
                dynamic child = children[i];
                ParseChild(child);
            }
        }
    }
}