using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class eventMedia : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Species";
            list.table = "TblSpeciesEvent";
            list.idField = "fSpeciesEventID";

            Field fSpecies = new Field("fSpeciesID", "Species", FieldType.Combo, 50, 50);
            fSpecies.lookUpTable = "TblSpecies";
            fSpecies.lookUpFieldID = "fSpeciesID";
            fSpecies.lookUpFieldName = "fCommonName";
            list.fields.Add(fSpecies);
            list.showEditButton = false;
            list.showViewButton = false;


            list.InitList(Context);

        }

        public GenericList list;
    }
}