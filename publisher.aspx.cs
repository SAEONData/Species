using Novacode;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class publisher : System.Web.UI.Page
    {
        public string docName = "";
        public string docURL = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            int id = int.Parse(Request["id"]);

            // get list of species for this catalog
            List<int> list = GetSpeciesList(id);


            // get catalogue info
            CatalogInfo info = GetCatlogInfo(id);

            String rootDir = Server.MapPath("temp/catalog/" + id);
            if (Directory.Exists(rootDir) == false)
                Directory.CreateDirectory(rootDir);

            String ext = Path.GetExtension(info.fTemplatePath);

            List<CatalogPage> pages = new List<CatalogPage>();




            for (int i = 0; i < list.Count; i++)
            {
                int species = list[i];
                int page = i + 1;

                String outPath = rootDir + "\\Page" + page + ext;
                if (File.Exists(outPath))
                    File.Delete(outPath);
                CatalogPage cpage = new CatalogPage();
                cpage.index = i;
                cpage.path = outPath;
                pages.Add(cpage);

                File.Copy(info.fTemplatePath, outPath);
                using (DocX doc = DocX.Load(outPath))
                {
                    String query = "SELECT * FROM TblSpecies WHERE fSpeciesID = " + species;
                    using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            using (SqlDataReader set = command.ExecuteReader())
                            {
                                if (!set.Read())
                                {
                                    Response.Write("Invalid species id: " + species);
                                    Response.End();
                                }

                                Response.Write("Add: " + set["fSpeciesName"].ToString() + "<br>");
                                Response.Flush();

                                doc.ReplaceText("~SCIENT~", set["fSpeciesName"].ToString());
                                doc.ReplaceText("~COMMON~", set["fCommonName"].ToString());
                                doc.ReplaceText("~FISHBOARD~", set["fFishBoard"].ToString());
                                doc.ReplaceText("~FEATURES~", set["fFeatures"].ToString()); 
                                doc.ReplaceText("~COLOUR~", set["fColour"].ToString());
                                doc.ReplaceText("~SIZE~", set["fSize"].ToString());
                                doc.ReplaceText("~HABITAT~", set["fHabitat"].ToString());
                                doc.ReplaceText("~DISTRIBUTION~", set["fDistribution"].ToString());
                                doc.ReplaceText("~SIMILAR~", set["fSimilar"].ToString());
                                doc.ReplaceText("~REFERENCES~", set["fReferences"].ToString());
                                doc.ReplaceText("~NOTES~", set["fNotes"].ToString());
                                
//                                doc.r

                                ResolveTaxon(doc, (int)set["fTaxonomyID"]);
                                doc.ReplaceText("~CLASS~", "");
                                doc.ReplaceText("~ORDER~", "");
                                doc.ReplaceText("~FAMILY~", "");
                                doc.ReplaceText("~GENUS~", "");
                                doc.ReplaceText("~SPECIES~", "");
                                doc.ReplaceText("~COMMON~", "");
                            }
                        }
                    }


                    // add media
                    int photoIndex = 1;

                    String sql = "SELECT TOP 4 * FROM vwSpeciesMedia WHERE fCatalogueOrder > 0 AND fSpeciesID = " + species;
//                    Response.Write(sql);


                    using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand(sql, con))
                        {
                            using (SqlDataReader set = command.ExecuteReader())
                            {
                                while (set.Read())
                                {
                                    // String url = HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath + '/' + set["fMediaPath"].ToString();
                                    // url = url.Replace("//", "/");
                                    // if (url.IndexOf("http://") == -1)
                                    //    url = "http://" + url;

                                    String url = set["fMediaPath"].ToString();


                                    String srv = Request.Url.AbsoluteUri;
                                    int index = srv.IndexOf("publisher.aspx");
                                    srv = srv.Substring(0, index);

                                    url = srv + "ic.aspx?url=" + Server.UrlEncode(url);

                                    Response.Write("Add photo: " + url + "<br>");
                                    Response.Flush();

                                    String tag_old = "~PHOTO" + photoIndex + "~";
                                    String tag_new = "~PHOTO" + "_" + i + "_" + photoIndex + "~";

                                    doc.ReplaceText(tag_old, tag_new);

                                    CatalogPhoto photo = new CatalogPhoto();
                                    photo.url = url;
                                    photo.tag = tag_new;
                                    cpage.photos.Add(photo);

                                    photoIndex++;


                                    /*
                                    Bitmap bmp = LoadBitmap(url);
                                    Bitmap outBmp = ScaleImage(bmp, 400, 400);

                                    ReplaceTextWithImage(doc, tag, outBmp, 100);

                                    bmp.Dispose();
                                    outBmp.Dispose();
                                     */

                                    

                                }
                            }
                        }
                    }



                    doc.Save();
                }
            }

            String docPath = rootDir + "\\" + info.fCatalogueName + ext;

            if (pages.Count == 0)
            {
                Response.Write("No items found");
                Response.End();
                return;
            }

            CatalogPage catpage = pages[0];
            using (DocX master = DocX.Load(catpage.path))
            {
                for (int i = 1; i < pages.Count; i++)
                {
                    catpage = pages[i];
                    DocX d = DocX.Load(catpage.path);

                    master.InsertSectionPageBreak();
                    master.InsertDocument(d);
                }

                docName = info.fCatalogueName + ext;
                docURL = "temp/catalog/" + id + "/" + docName;

                
                master.SaveAs(docPath);
            }

            using (DocX master = DocX.Load(docPath))
            {
                // insert images into master
                for (int i = 0; i < pages.Count; i++)
                {
                    catpage = pages[i];
                    foreach (CatalogPhoto photo in catpage.photos)
                    {
                        Response.Write("Load: " + photo.url + "<br>");
                        Response.Flush();

                        Bitmap bmp = LoadBitmap(photo.url);
                        Bitmap outBmp = ScaleImage(bmp, 400, 400);
                        outBmp.SetResolution(300, 300);

                        ReplaceTextWithImage(master, photo.tag, outBmp, 100);
                        bmp.Dispose();
                        outBmp.Dispose();
                    }
                }

                master.ReplaceText("~PHOTO1~", "");
                master.ReplaceText("~PHOTO2~", "");
                master.ReplaceText("~PHOTO3~", "");
                master.ReplaceText("~PHOTO4~", "");

                String final = rootDir + "\\" + info.fCatalogueName + "" +  ext;
                master.SaveAs(final);


            }

        }


        public static Bitmap ScaleImage(Bitmap image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }        

        protected void ResolveTaxon(DocX doc, int taxonID)
        {
            String query = "SELECT * FROM vwTaxon WHERE fTaxonomyID = " + taxonID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("Invalid taxon id");
                            Response.End();
                        }

                        String rank = "~" + set["fTaxonRankName"].ToString().ToUpper() + "~";
                        // Response.Write(rank + " = " + set["fTaxonomyName"].ToString() + "<br>");
                        doc.ReplaceText(rank, set["fTaxonomyName"].ToString());


                        int parentID = (int)set["fTaxonomyParentID"];
                        if (parentID != 0)
                            ResolveTaxon(doc, parentID);
                    }
                }
            }

        }

        public Bitmap LoadBitmap(String url)
        {
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                return new Bitmap(responseStream);
            }
            catch (Exception e)
            {
                
                die(e.Message.ToString());
                return null;
            }
        }

        public void ReplaceWithImage(DocX document, string tagBody, byte[] replacement)
        {
            using (var image = new MemoryStream(replacement))
            {
                var img = document.AddImage(image);
                var picture = img.CreatePicture();

                foreach (var paragraph in document.Paragraphs)
                {
                    paragraph.FindAll(tagBody).ForEach(index =>
                    {
                        paragraph.RemoveText(index, tagBody.Length - 1);
                        paragraph.InsertPicture(picture, index + 1);
                        paragraph.RemoveText(index, 1);
                    });
                }
            }
        }

        public void ReplaceTextWithImage(DocX document, string value, Bitmap image, int count)
        {
            if (image == null)
                throw new ArgumentNullException("Bitmap expected");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                byte[] data = memoryStream.GetBuffer();

                ReplaceWithImage(document, value, data);
                /*


                Novacode.Image docImage = document.AddImage(memoryStream);
                Novacode.Picture docPicture = docImage.CreatePicture();

                int countReplace = 0;

                foreach (Paragraph paragraph in document.Paragraphs)
                {
                    List<int> valuesIndex = paragraph.FindAll(value);

                    if (valuesIndex.Count > 0)
                    {
                        if (count > 0)
                        {
                            if (valuesIndex.Count > count)
                                valuesIndex.RemoveRange(count, valuesIndex.Count - count);
                        }
                        valuesIndex.Reverse();

                        foreach (int valueIndex in valuesIndex)
                        {
                            countReplace += 1;
                            paragraph.InsertPicture(docPicture, valueIndex + value.Length);
                            paragraph.RemoveText(valueIndex, value.Length);
                            if (countReplace == count)
                                return;
                        }
                    }
                }
                 */
            }
        }

        public void die(object o)
        {
            Response.Write(o.ToString());
            Response.End();
        }

        protected CatalogInfo GetCatlogInfo(int id)
        {
            CatalogInfo info = new CatalogInfo();
            String query = "SELECT * FROM vwCatalogInfo WHERE fCatalogueID = " + id;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("Invalid catalogue id");
                            Response.End();
                        }
                        info.fCatalogueID = (int)set["fCatalogueID"];
                        info.fCatalogueName = (string)set["fCatalogueName"];
                        info.fTemplateID = (int)set["fTemplateID"];
                        info.fTemplateName = (string)set["fTemplateName"];
                        info.fTemplatePath = (string)set["fTemplatePath"];
                        info.fPhotoCount = (int)set["fPhotoCount"];
                    }
                }
            }
            return info;
        }

        int getSpeciesTaxonID(int id)
        {
            int nTaxonID = 0;
            String sql = "SELECT fTaxonomyID FROM TblSpecies WHERE fSpeciesID = " + id;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("Invalid species id");
                            Response.End();
                        }
                        if (set["fTaxonomyID"] == DBNull.Value)
                            return 0;

                        nTaxonID = (int)set["fTaxonomyID"];
                    }
                }
            }
            return nTaxonID;
        }

        bool FindTaxon(int nTaxonID, int nSelectedTaxon)
        {
            if (nTaxonID == 0)
                return false;

            String sql = "SELECT fTaxonomyParentID FROM TblTaxonomy WHERE fTaxonomyID = " + nTaxonID;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (!set.Read())
                        {
                            Response.Write("Invalid taxon id");
                            Response.End();
                        }

                        nTaxonID = int.Parse(set["fTaxonomyParentID"].ToString());
                        if (nTaxonID == nSelectedTaxon)
                            return true;
                        if (nTaxonID == 0)
                            return false;
                        if (FindTaxon(nTaxonID, nSelectedTaxon) == true)
                            return true;
                    }
                }
            }
            return false;
        }

        protected List<int> GetTaxonList(int id)
        {
            List<int> list = new List<int>();

            // add full list for now
            String query = "SELECT fTaxonID FROM TblCatalogueTaxon WHERE fCatalogueID = " + id;
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            list.Add((int)set["fTaxonID"]);
                        }
                    }
                }
            }
            return list;
        }


        protected List<int> GetSpeciesList(int id)
        {
            List<int> taxons = GetTaxonList(id);

            List<int> list = new List<int>();

            // add full list for now
            String query = "SELECT fSpeciesID FROM TblSpecies ORDER BY fSpeciesID";
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            int nSpecies = (int)set["fSpeciesID"];
                            int nTaxon = getSpeciesTaxonID(nSpecies);

                            bool bTaxonFound = false; 
                            foreach (int taxon in taxons)
                            {
                                if (FindTaxon(nTaxon, taxon))
                                {
                                    bTaxonFound = true;
                                    break;
                                }
                            }

                            if (bTaxonFound == true)
                                list.Add(nSpecies);
                        }
                    }
                }
            }
            return list;
        }
    }

    public class CatalogPage
    {
        public int index;
        public string path;
        public List<CatalogPhoto> photos = new List<CatalogPhoto>();
    }

    public class CatalogPhoto
    {
        public string tag;
        public string url;
    };



    public class CatalogInfo
    {
        public int fCatalogueID;
        public string fCatalogueName;
        public int fTemplateID;
        public string fTemplateName;
        public string fTemplatePath;
        public int fPhotoCount;
    }
}