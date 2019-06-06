using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using species;
using System.Data.SqlClient;

namespace PLUploadTest
{
    /// <summary>
    /// Summary description for UploadHandler
    /// </summary>
    public class UploadHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            int chunk = context.Request["chunk"] != null ? int.Parse(context.Request["chunk"]) : 0;
            string rootPath = "temp";
            string fileName = context.Request["name"] != null ? context.Request["name"] : string.Empty;
            string folder = context.Request["folder"] != null ? context.Request["folder"] : string.Empty;
            if (folder != string.Empty)
            {
                rootPath += "/" + folder;
                string rootDir = context.Server.MapPath(rootPath);
                if (Directory.Exists(rootDir) == false)
                    Directory.CreateDirectory(rootDir);
            }
            HttpPostedFile fileUpload = context.Request.Files[0];
            var uploadPath = context.Server.MapPath(rootPath);
            using (var fs = new FileStream(Path.Combine(uploadPath, fileName), chunk == 0 ? FileMode.Create : FileMode.Append))
            {
                var buffer = new byte[fileUpload.InputStream.Length];
                fileUpload.InputStream.Read(buffer, 0, buffer.Length);

                fs.Write(buffer, 0, buffer.Length);
            }

            if (folder == "templates")
            {
                String path = context.Server.MapPath(rootPath + "/" + fileName);
                AddTemplate(path);
                context.Response.ContentType = "text/plain";
                context.Response.Write("Success: " + path);


            }
            else
            {
                String fileLocation = rootPath + "/" + fileName;
                String url = HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath + '/' + fileLocation;
                AddFileToDatabase(fileName, fileLocation);
                context.Response.ContentType = "text/plain";
                context.Response.Write("Success: " + url);

            }


        }

        public void AddTemplate(String fileName)
        {
            String name = Path.GetFileNameWithoutExtension(fileName);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                String sql = "INSERT INTO TblTemplate (fTemplateName,fTemplatePath,fPhotoCount) VALUES (@fTemplateName, @fTemplatePath, @fPhotoCount)";
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    command.Parameters.AddWithValue("@fTemplateName", name);
                    command.Parameters.AddWithValue("@fTemplatePath", fileName);
                    command.Parameters.AddWithValue("@fPhotoCount", 1);
                    command.ExecuteNonQuery();
                }
            }

        }

        public void AddFileToDatabase(String fileName, String url)
        {
            String name = Path.GetFileNameWithoutExtension(fileName);

            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                String sql = "INSERT INTO TblMedia (fMediaName,fMediaPath,fMediaType,fEventID) VALUES (@fMediaName, @fMediaPath, @fMediaType, @fEventID)";
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    command.Parameters.AddWithValue("@fMediaName", name);
                    command.Parameters.AddWithValue("@fMediaPath", url);
                    command.Parameters.AddWithValue("@fMediaType", 1);
                    command.Parameters.AddWithValue("@fEventID", 1);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}