using BLA;
using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class importSpeciesLookup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["spf"] != null && Request["spf"] != "")
            {
                ImportSpreadsheet(Request["spf"]);
            }
        }

        bool IsValidDescriber(String text)
        {
            if (text.Trim() == "" || text == "NULL")
                return false;

            return true;
        }

        string FormatDescriber(String text)
        {
            if (text == "NULL")
                return "";
            return text;
        }

        public void ImportSpreadsheet(String path)
        {
            const int fSpeciesID = 1;
            const int fTaxonomyID = 2;
            const int fLSID = 3;
            const int fWoRMID = 4;
            const int fSpeciesName = 5;
            const int fCommonName = 6;
            const int fFeatures = 7;
            const int fColour = 8;
            const int fSize = 9;
            const int fDistribution = 10;
            const int fHabitat = 11;
            const int fSimilar = 12;
            const int fReferences = 13;
            const int fNotes = 14;
            const int fOrangeToRed = 15;

            Dictionary<int, SpeciesLookup> speciesLookups = new Dictionary<int, SpeciesLookup>();

            
            Dictionary<int, Row> rows = ReadSpreadsheet(path);
            foreach (int r in rows.Keys)
            {
                Row row = rows[r];
                if (r > 1)
                {
                    int SpeciesID = int.Parse(row.cells[fSpeciesID].Trim());
                    int TaxonomyID = int.Parse(row.cells[fTaxonomyID].Trim());
                    int LSID = int.Parse(row.cells[fLSID].Trim());
                    int WoRMID = int.Parse(row.cells[fWoRMID].Trim());
                    String SpeciesName = row.cells[fSpeciesName];
                    String CommonName = row.cells[fCommonName];
                    String Features = row.cells[fFeatures];
                    String Colour = row.cells[fColour];
                    String Size = row.cells[fSize];
                    String Distribution = row.cells[fDistribution];
                    String Habitat = row.cells[fHabitat];
                    String Similar = row.cells[fSimilar];
                    String References = row.cells[fReferences];
                    String Notes = row.cells[fNotes];
                    String OrangeToRed = row.cells[fOrangeToRed];

                    String query = String.Format("SELECT * FROM TblSpeciesLookup WHERE fWoRMID = {0}", WoRMID);
                    using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader set = command.ExecuteReader())
                            {
                                SuperSQL sql = new SuperSQL(connection, null, Response);

                                if (set.Read())
                                {
                                    Response.Write("mod<br>");
                                    if (IsValidDescriber(CommonName))
                                    {
                                        String[] alts = set["fCommonNameAlts"].ToString().ToLower().Split('|');
                                        List<String> alternatives = alts.ToList();
                                        if (alternatives.IndexOf(CommonName) == -1)
                                        {
                                            String altNames = set["fCommonNameAlts"].ToString();
                                            if (altNames != "")
                                                altNames += "|";
                                            altNames += CommonName;
                                            sql.add("fCommonNameAlts", altNames);
                                        }
                                    }

                                    if (IsValidDescriber(SpeciesName))
                                    {
                                        String[] alts = set["fScienceNameAlts"].ToString().ToLower().Split('|');
                                        List<String> alternatives = alts.ToList();
                                        if (alternatives.IndexOf(SpeciesName) == -1)
                                        {
                                            String altNames = set["fScienceNameAlts"].ToString();
                                            if (altNames != "")
                                                altNames += "|";
                                            altNames += SpeciesName;
                                            sql.add("fScienceNameAlts", altNames);
                                        }
                                    }

                                    if (IsValidDescriber(Features))
                                        sql.add("fFeatures", Features);
                                    if (IsValidDescriber(Colour))
                                        sql.add("fColour", Colour);
                                    if (IsValidDescriber(Size))
                                        sql.add("fSize", Size);
                                    if (IsValidDescriber(Distribution))
                                        sql.add("fDistribution", Distribution);
                                    if (IsValidDescriber(Habitat))
                                        sql.add("fHabitat", Habitat);
                                    if (IsValidDescriber(Similar))
                                        sql.add("fSimilar", Similar);
                                    if (IsValidDescriber(References))
                                        sql.add("fReferences", References);
                                    if (IsValidDescriber(Notes))
                                        sql.add("fNotes", Notes);
                                    if (IsValidDescriber(OrangeToRed))
                                        sql.add("fFAFFCode", OrangeToRed);
                                    set.Close();

                                    sql.modify("TblSpeciesLookup", "fWoRMID = " + WoRMID);
                                }
                                else
                                {
                                    Response.Write("add<br>");
                                    set.Close();
                                    sql.add("fWoRMID", WoRMID);
                                    sql.add("fSpeciesLookupName", SpeciesName);
                                    sql.add("fCommonName", CommonName);
                                    sql.add("fScienceNameAlts", SpeciesName);
                                    sql.add("fCommonNameAlts", CommonName);
                                    sql.add("fFeatures", FormatDescriber(Features));
                                    sql.add("fColour", FormatDescriber(Colour));
                                    sql.add("fSize", FormatDescriber(Size));
                                    sql.add("fDistribution", FormatDescriber(Distribution));
                                    sql.add("fSimilar", FormatDescriber(Similar));
                                    sql.add("fReferences", FormatDescriber(References));
                                    sql.add("fNotes", FormatDescriber(Notes));
                                    sql.add("fFAFFCode", FormatDescriber(OrangeToRed));
                                    sql.insert("TblSpeciesLookup");
                                }
                            }





                        }

                    }



                }
            }
        }

        Dictionary<int, Row> ReadSpreadsheet(String path)
        {
            Dictionary<int, Row> rows = new Dictionary<int, Row>();
            string fileName = fileName = System.IO.Path.GetFileName(path);
            string fileExtension = System.IO.Path.GetExtension(fileName);
            FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = null;
            if (fileExtension.Equals(".xls"))
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            else if (fileExtension.Equals(".xlsx"))
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelReader.IsFirstRowAsColumnNames = false;
            DataSet result = excelReader.AsDataSet();
            int tableCount = result.Tables.Count;
            tableCount = 1; // only use the first worksheet which is "Primary Data"
            for (int i = 0; i < tableCount; i++)
            {
                DataTable Sheets = result.Tables[i];
                int rowIndex = 1;
                foreach (DataRow row in Sheets.Rows)
                {
                    rows[rowIndex] = new Row();
                    int colIndex = 1;
                    foreach(object cel in row.ItemArray)
                    {
                        rows[rowIndex].cells[colIndex] = cel.ToString();
                        colIndex++;
                    }
                    rowIndex++;
                }
            }
            return rows;
        }
    }

    class SpeciesLookup
    {
        public String SpeciesID;
        public String TaxonomyID;
        public String LSID;
        public String WoRMID;
        public String SpeciesName;
        public String CommonName;
        public String Features;
        public String Colour;
        public String Size;
        public String Distribution;
        public String Habitat;
        public String Similar;
        public String References;
        public String Notes;
        public String OrangeToRed;
        public String AltNames;
    }


    class Row
    {
        public Dictionary<int, string> cells = new Dictionary<int, string>();
    }



    


}