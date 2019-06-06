using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class adminVocabularies : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Vocabulary";
            list.table = "TblVocabulary";
            list.idField = "fVocabularyID";
            list.fields.Add(new Field("fVocabularyName", "Name", FieldType.String, 80, 100));
            Field fUnit = new Field("fVocabularyInterfaceID", "Type", FieldType.Combo, 50, 50);
            fUnit.lookUpTable = "TblVocabularyInterface";
            fUnit.lookUpFieldID = "fVocabularyInterfaceID";
            fUnit.lookUpFieldName = "fVocabularyInterfaceName";
            list.fields.Add(fUnit);
            list.fields.Add(new Field("fVocabularyURL", "URL", FieldType.Link, 80, 100));

            list.filterFields.Add("fVocabularyName");
            list.filterFields.Add("fVocabularyURL");


            list.InitList(Context);
        }

        public GenericList list;
    }
}