using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class loadPortalFolder : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String url = Request["url"];
            int speciesID = int.Parse(Request["species"]);

            // create temp file name
            String file = "temp/" + Guid.NewGuid() + ".xml";
            String path = Server.MapPath(file);

            // read url to file
            try
            {
                System.Net.WebClient wc = new WebClient();
                wc.DownloadFile(url, path);
            }
            catch (Exception)
            {
                Response.Write("Failed to get xml<br>" + url);
                Response.End();
            }

            // read file to text
            String json = File.ReadAllText(path);
            File.Delete(path);
            
            // serialize text to object
            dynamic ret = null;
            try
            {
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                ret = json_serializer.DeserializeObject(json);
            }
            catch (Exception)
            {
                Response.Write("Failed to parse json<br>" + json);
                Response.End();
            }

            dynamic children = ret["children"];
            String size = "";

            foreach (dynamic child in children)
            {
                if (child["title"] != "")
                {
                    String name = child["title"].ToString();
                    String imgpath = child["context_path"].ToString();

                    manMedia mm = new manMedia();
                    int mediaID = mm.GetMediaID(name, imgpath);

                    if (speciesID > 0)
                    {
                        /****
                        // read image to file
                        try
                        {
                            System.Net.WebClient wc = new WebClient();
                            wc.DownloadFile(imgpath, path);
                        }
                        catch (Exception)
                        {
                            Response.Write("Failed to load image: " + imgpath);
                            Response.End();
                        }

                        // load bitmap
                        int cx = 0;
                        int cy = 0;
                        using (Bitmap bmp = new Bitmap(path))
                        {
                            cx = bmp.Width;
                            cy = bmp.Height;
                            size = bmp.Width + "," + bmp.Height;

                        }
                        File.Delete(path);
                        **/


                        manSpeciesMedia msm = new manSpeciesMedia();
                        int id = msm.GetSpeciesMediaID(speciesID, mediaID, 0, 0, 3000, 3000);

                    }


                    Response.Write(name + ": size=" + size + "<br />");
                    Response.Flush();
                }
            }
        }
    }
}