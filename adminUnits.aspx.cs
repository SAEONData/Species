using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class adminUnits : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Unit";
            list.table = "TblUnit";
            list.idField = "fUnitID";
            list.fields.Add(new Field("fUnitName", "Unit Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fUnitABBR", "Abbreviation", FieldType.String, 50, 50));

            list.filterFields.Add("fUnitName");
            list.filterFields.Add("fUnitABBR");
            

            list.InitList(Context);
        }

        public GenericList list;
    }
}