using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class ic : System.Web.UI.Page
    {
        public Bitmap ScaleImage(Bitmap image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            String url = Request["url"];

            String guid = getFileGuidDB(url);
            if (guid == "")
            {
                guid = Guid.NewGuid().ToString();
                String name = "cache/" + guid + ".bin";
                String path = Server.MapPath(name);
                String fullurl = url + "/download?__ac_name=SpeciesDB&__ac_password=SpeciesDB";

                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(fullurl, path);
                    }
                }
                catch (Exception err)
                {
                    Response.Write(err.Message + "<br>");
                    Response.Write(fullurl);
                    Response.End();
                }
                

                addFileToDB(url, guid);
            }

            String fileName = "cache/" + guid + ".bin";
            String filePath = Server.MapPath(fileName);

            int maxWidth = 800;
            int maxHeight = 600;

            if (Request["maxHeight"] != null && Request["maxHeight"] != "")
                maxHeight = int.Parse(Request["maxHeight"]);

            if (Request["maxWidth"] != null && Request["maxWidth"] != "")
                maxWidth = int.Parse(Request["maxWidth"]);

            

            Response.ContentType = "image/jpeg";
            using (Bitmap bmp = new Bitmap(filePath))
            {
                using (Bitmap img = ScaleImage(bmp, maxWidth, maxHeight))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ms.WriteTo(Response.OutputStream);
                    }
                }
            }

        }

        String getFileGuidDB(String url)
        {
            String guid = "";
            String sql = "SELECT fChacheFileGUID FROM TblChacheFile WHERE fChacheFileURL = '@fChacheFileURL'";
            sql = sql.Replace("@fChacheFileURL", url);

            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = cmd.ExecuteReader())
                    {
                        if (set.Read())
                            guid = set[0].ToString();
                    }
                }
            }
            return guid;
        }

        void addFileToDB(String url, String guid)
        {
            String sql = "INSERT INTO TblChacheFile (fChacheFileURL, fChacheFileGUID) VALUES (@fChacheFileURL, @fChacheFileGUID)";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fChacheFileURL", url);
                    cmd.Parameters.AddWithValue("@fChacheFileGUID", guid);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}