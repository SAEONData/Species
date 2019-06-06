using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class stations : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Station";
            list.table = "TblStation";
            list.idField = "fStationID";
            list.fields.Add(new Field("fStationName", "Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fStartLat", "Latitude", FieldType.String, 80, 100, "-28.5"));
            list.fields.Add(new Field("fStartLng", "Longitude", FieldType.String, 80, 100, "25.0"));

            list.filterFields.Add("fStationName");
            list.customEditDialog = true;
            list.exportButton = true;
            list.showAllButton = true;


            list.listFilter = "fSystem != 1";
            list.InitList(Context);
        }

        public GenericList list;
    }
}