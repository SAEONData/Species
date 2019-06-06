using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class adminSurveyTypes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            list = new GenericList(Context);
            list.type = "Survey Type";
            list.table = "TblSurveyType";
            list.idField = "fSurveyTypeID";
            list.fields.Add(new Field("fSurveyTypeName", "Survey Type", FieldType.String, 80, 100));
            list.fields.Add(new Field("fSurveyLevel2Name", "Level 1 Name", FieldType.String, 50, 50));
            list.fields.Add(new Field("fSurveyLevel3Name", "Level 2 Name", FieldType.String, 50, 50));
//            list.fields.Add(new Field("", "Indicators", FieldType.IFrame, 0, 0));

            list.filterFields.Add("fSurveyTypeName");

            list.InitList(Context);
        }

        public GenericList list;
    }
}