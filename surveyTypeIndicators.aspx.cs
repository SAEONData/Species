using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class surveyTypeIndicators : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {
            string survey = Request["st"];
            if (survey == null || survey == "")
                survey = Session["fSurveyTypeID"].ToString();
            fSurveyTypeID = int.Parse(survey);
            Session["fSurveyTypeID"] = survey;

            list = new GenericList(Context);
            list.type = "Indicator";
            list.table = "TblIndicator";
            list.idField = "fIndicatorID";
            list.fields.Add(new Field("fSurveyTypeID", "SurveyType", FieldType.Constant, 0, 0, fSurveyTypeID.ToString()));

            list.fields.Add(new Field("fIndicatorName", "Indicator Name", FieldType.String, 80, 100));
            Field fUnit = new Field("fUnitID", "Unit", FieldType.Combo, 50, 50);
            fUnit.lookUpTable = "TblUnit";
            fUnit.lookUpFieldID = "fUnitID";
            fUnit.lookUpFieldName = "fUnitName";
            list.fields.Add(fUnit);

            list.topAddButton = false;

            list.listFilter = "fSurveyTypeID = " + fSurveyTypeID.ToString();
            list.InitList(Context);
        }

        int fSurveyTypeID = 0;
        public GenericList list;
    }
}