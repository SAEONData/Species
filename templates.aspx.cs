using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class templates : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Template";
            list.table = "TblTemplate";
            list.idField = "fTemplateID";
            list.exportButton = false;
            list.fields.Add(new Field("fTemplateName", "Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fPhotoCount", "Photo Count", FieldType.String, 80, 100));
            list.fields.Add(new Field("fTemplatePath", "Path", FieldType.ReadOnly, 4, 0));
            list.InitList(Context);
        }

        public GenericList list;
    }
}