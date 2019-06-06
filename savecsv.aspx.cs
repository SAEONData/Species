using species;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BLA
{
    public partial class savecsv : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String fileName = Request["name"];
            String url = Request["url"];
            // SaveDownloadLog(url);

            // Response.Write(url);
            // Response.End();




            if (url.IndexOf("http://") == -1 && url.IndexOf("https://") == -1)
            {
                String page = "http://" + Request.ServerVariables["HTTP_HOST"] + Request.ServerVariables["URL"];
                url = page.Replace("savecsv.aspx", url);
            }

            byte[] data = null;
            using (WebClient client = new WebClient())
            {
                data = client.DownloadData(url);
            }

            Response.Clear();
            Response.AppendHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/octet-stream";
            Response.BinaryWrite(data);
            Response.Flush();
            Response.End();
        }

        protected void SaveDownloadLog(String url)
        {
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    SuperSQL sql = new SuperSQL(con, trn, Response);
                    sql.add("UserID", int.Parse(Session["userID"].ToString()));
                    sql.add("DownloadURL", url);
                    sql.insert("DataDownload");
                    trn.Commit();
                }
            }
        }

    }

}