using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class specie : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList();
            list.type = "Specie";
            list.table = "TblSpecies";
            list.idField = "fSpeciesID";
            list.fields.Add(new Field("fTaxonomyID", "Taxonomy", FieldType.Taxon, 80, 100));
            list.fields.Add(new Field("fSpeciesName", "Species Name", FieldType.String, 80, 100));
            list.fields.Add(new Field("fCommonName", "Common Name", FieldType.Date, 80, 100));
            list.fields.Add(new Field("fCommonName", "Common Name", FieldType.Date, 80, 100));
            list.InitList(Context);
        }

        public GenericList list;
    }
}