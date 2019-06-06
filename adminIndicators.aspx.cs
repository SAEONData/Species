using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class adminIndicators : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string fSurveyType = Request["id"];
            if (fSurveyType == null)
            {
                if (Session["fSurveyTypeID"] == null)
                    fSurveyType = "2";
                else
                    fSurveyType = Session["fSurveyTypeID"].ToString();
            }
            fSurveyTypeID = int.Parse(fSurveyType);


            list = new GenericList(Context);
            list.type = "Indicator";
            list.table = "TblIndicator";
            list.idField = "fIndicatorID";
            list.fields.Add(new Field("fSurveyTypeID", "fSurveyTypeID", FieldType.Constant, 0, 0, fSurveyTypeID.ToString()));
            list.fields.Add(new Field("fIndicatorName", "Indicator Name", FieldType.String, 80, 100));
            Field fUnit = new Field("fUnitID", "Unit", FieldType.Combo, 50, 50);
            fUnit.lookUpTable = "TblUnit";
            fUnit.lookUpFieldID = "fUnitID";
            fUnit.lookUpFieldName = "fUnitName";
            list.fields.Add(fUnit);

//            list.listFilter = "fSurveyTypeID = " + fSurveyType;
            list.InitList(Context);
        }

        public int fSurveyTypeID = 0;
        public GenericList list;
    }
}