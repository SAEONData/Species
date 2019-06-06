using System;
using System.Collections.Generic;
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
    public partial class parsemedia : System.Web.UI.Page
    {
        public SortedDictionary<String, int> media = new SortedDictionary<string, int>();
        public String survey = "";
        public String trawlLog = "";
        public String catchLog = "";
        

        public String url;
        public List<String> fileParts = new List<String>();
        public String sampleFileName;


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

        public String GetSurveyName(String src)
        {
            String url = src + "?__ac_name=SpeciesDB&__ac_password=SpeciesDB";

            String name = "temp/sheet.xlsx"; 
            String path = Server.MapPath(name);
            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }

            string con = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path  + "; Extended Properties='Excel 8.0;HDR=Yes;'";
            con = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + "; Extended Properties=Excel 12.0;";
            String survey = "";
            using (OleDbConnection connection = new OleDbConnection(con))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);
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
                    Response.Write("found excel: " + title + "<br>");

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
                        sampleFileName = name;
                        String[] parts = name.Split('_');
                        foreach (String part in parts)
                            fileParts.Add(part);
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