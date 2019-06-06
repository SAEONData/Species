using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BLA
{
    public partial class renderbox : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int cx = 10;
            int cy = 10;

            int range = cy - 2;
            int l = 0;



            String colors = Request["c"];
            String[] cols = null;



            if (colors != null && colors != "")
            {
                cols = colors.Split(',');

                List<String> colorList = cols.ToList();
                while (colorList.Count > range)
                    colorList.Remove(colorList[colorList.Count - 1]);
                cols = colorList.ToArray();

                l = range / cols.Length;
            }

            

            Color lineColor = Color.FromArgb(200, 0, 0, 0);


            using (Bitmap bmp = new Bitmap(cx, cy))
            {
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    if (cols != null)
                    {
                        for (int i=0; i<cols.Length; i++)
                        {
                            Color fillColor = ColorTranslator.FromHtml("#" + cols[i]);
                            Brush br = new SolidBrush(fillColor);
                            gr.FillRectangle(br, 0, 1 + i*l, cx, l);
                        }
                    }


                    Pen pen = new Pen(lineColor, 1.0f);
                    gr.DrawRectangle(pen, 0, 0, cx-1, cy-1);




                }

                Response.ContentType = "image/png";

                // write to response stream
                bmp.Save(Response.OutputStream, ImageFormat.Png);
            }
        }
    }
}