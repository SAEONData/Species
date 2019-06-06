using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class Catalouge : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.publishButtons = true;
            list.exportButton = false;
            list.type = "Catalogue";
            list.table = "TblCatalogue";
            list.idField = "fCatalogueID";
            list.fields.Add(new Field("fCatalogueName", "Name", FieldType.String, 80, 100));
            Field fUnit = new Field("fTemplateID", "Template", FieldType.Combo, 50, 50);
            fUnit.lookUpTable = "TblTemplate";
            fUnit.lookUpFieldID = "fTemplateID";
            fUnit.lookUpFieldName = "fTemplateName";
            list.fields.Add(fUnit);

            list.fields.Add(new Field("", "Taxonomy", FieldType.TaxonList, 0, 0, ""));


            list.InitList(Context);


        }

        public GenericList list;
    }
}