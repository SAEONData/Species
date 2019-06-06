using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class survey : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Survey";
            list.table = "TblSurvey";
            list.idField = "fSurveyID";
            list.fields.Add(new Field("fSurveyLabel", "Name", FieldType.String, 80, 100));
            Field fUnit = new Field("fSurveyTypeRef", "Type", FieldType.Combo, 50, 50);
            fUnit.lookUpTable = "TblSurveyType";
            fUnit.lookUpFieldID = "fSurveyTypeID";
            fUnit.lookUpFieldName = "fSurveyTypeName";
            list.fields.Add(fUnit);
            list.fields.Add(new Field("fSurveyDesc", "Description", FieldType.Paragraph, 80, 100));

            list.filterFields.Add("fSurveyLabel");
            list.filterFields.Add("fSurveyDesc");

            list.listFilter = "fSystem != 1";
            list.InitList(Context);
        }

        public GenericList list;
    }
}