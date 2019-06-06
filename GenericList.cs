using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace species
{
    public delegate String FileSourceDelegate(int id);
    public delegate String RowDescriptorDelegate(SqlDataReader set);


    public enum FieldType
    {
        String,
        Integer,
        Float,
        Combo,
        Link,
        Paragraph,
        Date,
        Time,
        Taxon,
        Photo,
        ReadOnly,
        Constant,
        TaxonList,
        IFrame,
        Image,
        FileName,
        Hidden
    };

    public class Field
    {
        public Field(String name, String header, FieldType type, int length, int headerWidth, String value = "", FileSourceDelegate fileFunc = null)
        {
            this.name = name;
            this.header = header;
            this.type = type;
            this.length = length;
            this.headerWidth = headerWidth;
            this.value = value;
            this.fileFunc = fileFunc;
        }

        public String name;
        public String header;
        public FieldType type;
        public int length;
        public int headerWidth;
        public int height = 370;

        public String value = "";

        public String lookUpTable = "";
        public String lookUpFieldID = "";
        public String lookUpFieldName = "";
        public String lookUpFilter = "";

        public FileSourceDelegate fileFunc = null;
    };


    public delegate bool GenericFilter(int id, string filter);
    public delegate void SaveExtraData(int id, HttpRequest req, HttpResponse res);


    public class GenericList
    {
        public GenericList(HttpContext context)
        {
            this.context = context;
        }
        HttpContext context;
        public string type = "";
        public string table = "";
        public string idField = "";


        public bool customViewDialog = false;
        public bool customEditDialog = false;
        public bool staticList = false;
        public bool pagination = true;

        public List<String> filterFields = new List<string>();
        public bool showFilterFields = true;

        public GenericFilter filterDelegate = null;
        public SaveExtraData dataSavedEvent = null;
        public RowDescriptorDelegate rowDescriptorDelegate = null;
        public bool speciesQuery = false;



        public bool publishButtons = false;
        public string listFilter = "";
        public string orderField = "";
        public bool topAddButton = true;
        public bool exportButton = false;
        public bool showAllButton = false;

        public bool showViewButton = true;
        public bool showEditButton = true;
        public bool showDeleteButton = true;





        public bool webFolderButton = false;

        public int rowsPerPage = 20;
        



        public List<Field> fields = new List<Field>();

        public void WriteLine(HttpResponse res, String text)
        {
            res.Write(text + "\r\n");
        }

        public void InitList(HttpContext context)
        {
            HttpRequest req = context.Request;
            HttpResponse res = context.Response;

            String mode = req["mode"];
            if (mode == "loadrec")
            {
                loadRecord(res, int.Parse(req["id"]));
                res.End();
            }

            if (mode == "save")
            {
                saveRecord(req, res);
                res.End();
            }

            if (mode == "delete")
            {
                deleteRecord(req, res);
                res.End();
            }

            if (mode == "loadlist" || mode == "exportcsv")
            {
                int page = 0;
                if (req["page"] != null)
                    page = int.Parse(req["page"]) - 1;
                WriteTable(context, true, req["filter"], req["filterText"], page, mode);
                res.End();
            }

            if (mode == "loaddlg")
            {
                WriteDialogs(res, true);
                res.End();
            }


        }

        public String CSVString(string intext)
        {
            if (intext == null)
                return "";
            if (intext.IndexOf(',') != -1 || intext.IndexOf('"') != -1)
            {
                intext = intext.Replace("\"", "\"\"");
                return "\"" + intext + "\"";
            }
            return intext;
        }


        public void WriteTable(HttpContext context, bool editable, string filter, string filterText, int page, string mode)
        {
            HttpResponse res = context.Response;

            if (mode == "loadlist")
            {
                if (filterFields.Count > 0 && showFilterFields)
                {
                    if (filterText == null)
                        filterText = "";
                    String find = context.Server.HtmlEncode(filterText);
                    find = find.Replace("'", "\\'");

                    WriteLine(res, "<div class='input-group' style='width: 50%'>");
                    WriteLine(res, "   <input type='text' class='form-control' placeholder='Search' id='textFilter' name='textFilter' value='" + find + "' />");
                    WriteLine(res, "   <div class='input-group-btn textFilter'>");
                    WriteLine(res, "       <button class='btn btn-default' type='submit'><i class='glyphicon glyphicon-search'></i></button>");
                    WriteLine(res, "   </div>");
                    WriteLine(res, "</div>");
                    WriteLine(res, "<br />");
                }

                if (staticList == false && topAddButton == true)
                {
                    WriteLine(res, "<button type='button' id='btnAddItem1' target='" + table + "' class='btn btn-sm addButton'><span class='glyphicon glyphicon-plus' aria-hidden='true' /> Add " + type + "</button>");
                    if (webFolderButton == true)
                        WriteLine(res, "&nbsp;<button type='button' id='btnAddWebFolder' class='btn btn-sm addWebFolder'><span class='glyphicon glyphicon-plus' aria-hidden='true' /> Add Portal Folder</button>");
                    if (exportButton == true)
                        WriteLine(res, "&nbsp;<button type='button' id='btnExcelExport' class='btn btn-sm btnExcelExport'><span class='glyphicon glyphicon glyphicon-save' aria-hidden='true' /> Export to CSV</button>");
                    if (showAllButton == true)
                        WriteLine(res, "&nbsp;<button type='button' id='btnShowAll' class='btn btn-sm btnShowAll'><span class='glyphicon glyphicon glyphicon-screenshot' aria-hidden='true' /> Show All</button>");

                    WriteLine(res, "<br /><br />");
                }
                else
                {
                    if (exportButton == true)
                        WriteLine(res, "&nbsp;<button type='button' id='btnExcelExport' class='btn btn-sm btnExcelExport'><span class='glyphicon glyphicon-save' aria-hidden='true' /> Export to CSV</button>");
                }


                WriteLine(res, "<table class='table'>");
                // write headers
                WriteLine(res, "<tr>");
                foreach (Field field in fields)
                {
                    if (field.headerWidth > 0 && field.type != FieldType.Constant)
                    {
                        WriteLine(res, "<td>");
                        WriteLine(res, "<h5><strong>" + field.header + "</strong></h5>");
                        WriteLine(res, "</td>");
                    }
                }
                WriteLine(res, "<td>");
                WriteLine(res, "<h5><strong>...</strong></h5>");
                WriteLine(res, "</td>");
                WriteLine(res, "</tr>");
            }

            // write data
            String fieldList = "[" + idField + "]";
            foreach (Field field in fields)
            {
                if (field.name != "")
                    fieldList += ", [" + field.name + "]";
            }

            // full query
            

            String query = "";
            if (speciesQuery == true)
            {
                query += "WITH SpeciesTaxon as (";
                query += "	SELECT P.fTaxonomyID, P.fTaxonomyParentID, CAST(P.fTaxonomyName AS VarChar(Max)) as fTaxonomy";
                query += "	FROM TblTaxonomy P";
                query += "	WHERE P.fTaxonomyParentID = 0";
                query += "	UNION ALL";
                query += "	SELECT P1.fTaxonomyID, P1.fTaxonomyParentID, CAST(P1.fTaxonomyName AS VarChar(Max)) + ', ' + M.fTaxonomy";
                query += "	FROM TblTaxonomy P1  ";
                query += "	INNER JOIN SpeciesTaxon M";
                query += "	ON M.fTaxonomyID = P1.fTaxonomyParentID";
                query += ")";
            }

            query += "SELECT " + fieldList + " FROM " + table;
            if (speciesQuery == true)
            {
                query += " INNER JOIN SpeciesTaxon ON SpeciesTaxon.fTaxonomyID = TblSpecies.fTaxonomyID ";
            }

            if (listFilter != "")
            {
                query += " WHERE " + listFilter;
                if (filterText != null && filterText != "")
                {
                    String find = filterText.Replace("'", "''");
                    for (int i = 0; i < filterFields.Count; i++)
                    {
                        String field = filterFields[i];
                        if (i == 0)
                            query += String.Format(" AND ({0} LIKE '%{1}%'", field, find);
                        else
                            query += String.Format(" OR {0} LIKE '%{1}%'", field, find);
                    }
                    query += ")";
                }
            }
            else
            {
                if (filterText != null && filterText != "")
                {
                    String find = filterText.Replace("'", "''");
                    for (int i = 0; i < filterFields.Count; i++)
                    {
                        String field = filterFields[i];
                        if (i == 0)
                            query += String.Format(" WHERE {0} LIKE '%{1}%'", field, find);
                        else
                            query += String.Format(" OR {0} LIKE '%{1}%'", field, find);
                    }
                }
            }

            if (orderField != "")
                query += " ORDER BY " + orderField;

//            res.Write(query);
//            res.End();


            int rowCount = 0;

            // count the rows
            if (pagination)
            {
                using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader set = command.ExecuteReader())
                        {
                            while (set.Read())
                            {
                                int id = int.Parse(set[idField].ToString());
                                bool writeRecord = true;
                                if (filterDelegate != null)
                                {
                                    if (filterDelegate.Invoke(id, filter) == false)
                                        writeRecord = false;
                                }
                                if (writeRecord)
                                    rowCount++;
                            }
                        }
                    }
                }
            }

            int rowIndex = 0;



            // write out the records
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader set = command.ExecuteReader())
                    {

                        if (mode == "exportcsv")
                        {
                            bool firstField = true;
                            foreach (Field field in fields)
                            {
                                if (firstField == true)
                                    firstField = false;
                                else
                                    res.Write(",");
                                res.Write(CSVString(field.header == "" ? field.name : field.header));
                            }
                            res.Write("\r\n");
                        }
                        

                        while (set.Read())
                        {
                            int id = int.Parse(set[idField].ToString());
                            bool writeRecord = true;
                            bool firstTextField = false;

                            if (filterDelegate != null)
                            {
                                if (filterDelegate.Invoke(id, filter) == false)
                                    writeRecord = false;
                            }


                            if (writeRecord)
                            {
                                if (mode == "loadlist")
                                {
                                    if (rowIndex >= page * rowsPerPage && rowIndex < page * rowsPerPage + rowsPerPage)
                                    {
                                        WriteLine(res, "<tr>");
                                        foreach (Field field in fields)
                                        {
                                            String value = "";

                                            try
                                            {
                                                if (field.fileFunc != null)
                                                    value = field.fileFunc.Invoke(id);
                                                else
                                                {
                                                    if (field.name != null && field.name != "")
                                                        value = GetFieldText(field, set[field.name].ToString());
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                res.Write(e.Message);
                                                res.Write("<br><br> on field: " + field.name);
                                                res.End();
                                            }

                                            if (field.type == FieldType.FileName)
                                            {
                                                WriteLine(res, "<td>");

                                                WriteLine(res, Path.GetFileName(value));
                                                WriteLine(res, "</td>");
                                            }

                                            else if (field.type == FieldType.Image)
                                            {
                                                WriteLine(res, "<td>");
                                                WriteLine(res, "<h5><img width='200px' maxHeight='200' src='ic.aspx?url=" + context.Server.UrlEncode(value) + "&maxHeight=200' ></img></h5>");
                                                WriteLine(res, "</td>");
                                            }
                                            else
                                            {
                                                if (field.headerWidth > 0 && field.type != FieldType.Constant)
                                                {
                                                    WriteLine(res, "<td>");

                                                    if (field.name.ToLower().Contains("species") == true)
                                                        WriteLine(res, "<i>" + value + "</i>");
                                                    else
                                                        WriteLine(res, "<h5>" + value + "</h5>");
                                                    if (firstTextField == false)
                                                    {
                                                        firstTextField = true;
                                                        if (rowDescriptorDelegate != null)
                                                        {
                                                            String text = rowDescriptorDelegate.Invoke(set);
                                                            if (text != "")
                                                                WriteLine(res, "<h5>" + text + "</h5>");

                                                        }
                                                    }
                                                    WriteLine(res, "</td>");
                                                }
                                            }
                                        }
                                        WriteLine(res, "<td>");
                                        WriteLine(res, "<h5>");

                                        if (showViewButton == true)
                                            WriteLine(res, "<button type='button' target='" + table + "' class='btn btn-sm viewButton fixedButton' id='viw" + id + "' href=''><span class='glyphicon glyphicon-eye-open' aria-hidden='true' /> View</button>&nbsp;");
                                        if (staticList == false)
                                        {
                                            if (showEditButton == true)
                                                WriteLine(res, "<button type='button' target='" + table + "' class='btn btn-sm editButton fixedButton' id='edt" + id + "' href=''><span class='glyphicon glyphicon-edit' aria-hidden='true' /> Edit</button>&nbsp;");
                                            if (showDeleteButton == true) 
                                                WriteLine(res, "<button type='button' target='" + table + "' class='btn btn-sm deleteButton fixedButton' id='del" + id + "' href=''><span class='glyphicon glyphicon-remove' aria-hidden='true' /> Delete</button>");
                                        }
                                        if (publishButtons)
                                            WriteLine(res, "&nbsp;&nbsp;<button type='button' class='btn btn-sm publishButton fixedButton' style='width:100px' id='pub" + id + "' href=''><span class='glyphicon glyphicon-print' aria-hidden='true' /> Publish</button>");
                                        WriteLine(res, "</h5>");
                                        WriteLine(res, "</td>");
                                        WriteLine(res, "</tr>");
                                    }
                                }
                                else
                                {
                                    bool firstField = true;
                                    foreach (Field field in fields)
                                    {
                                        if (firstField == true)
                                            firstField = false;
                                        else
                                            res.Write(",");
                                        String value = "";

                                        try
                                        {
                                            if (field.fileFunc != null)
                                                value = field.fileFunc.Invoke(id);
                                            else
                                            {
                                                if (field.name != null && field.name != "")
                                                    value = GetFieldText(field, set[field.name].ToString());
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            res.Write(e.Message);
                                            res.Write("<br><br> on field: " + field.name);
                                            res.End();
                                        }

                                        res.Write(CSVString(value));
                                    }
                                    res.Write("\r\n");
                                }

                                rowIndex++;
                            }
                        }
                    }
                }
            }

            if (mode == "loadlist")
            {

                WriteLine(res, "</tr>");
                WriteLine(res, "</table>");

                if (pagination == true)
                {
                    WriteLine(res, "<div id='page-selection'></div>");
                }

                if (staticList == false)
                {
                    WriteLine(res, "<button type='button' id='btnAddItem2' target='" + table + "' class='btn btn-sm addButton'><span class='glyphicon glyphicon-plus' aria-hidden='true' /> Add " + type + "</button>");
                    if (webFolderButton == true)
                        WriteLine(res, "&nbsp;<button type='button' id='btnAddWebFolder' class='btn btn-sm addWebFolder'><span class='glyphicon glyphicon-plus' aria-hidden='true' /> Add Portal Folder</button><br /><br>");
                }

                // init bootpag
                if (pagination == true)
                {
                    int curPage = page + 1;
                    int pages = (int)(Math.Ceiling((double)rowCount / (double)rowsPerPage));

                    WriteLine(res, "<script>");
                    WriteLine(res, "$(document).ready(function () {");
                    WriteLine(res, "    $('#page-selection').bootpag({");
                    WriteLine(res, "        page: " + curPage + ",");
                    WriteLine(res, "        total: " + pages + ",");
                    WriteLine(res, "        leaps: true,");
                    WriteLine(res, "        maxVisible: 8");

                    WriteLine(res, "    }).on('page', function(event, num){");
                    WriteLine(res, "        loadPage(num); // some ajax content loading...");
                    WriteLine(res, "    });");
                    WriteLine(res, "});");
                    WriteLine(res, "</script>");
                }
            }

        }

        public void WriteDialogs(HttpResponse res, bool editable)
        {
            // view dialog
            if (customViewDialog == false)
            {
                WriteLine(res, "<div id='viewDialog" + table + "' class='modal'>");
                WriteLine(res, "  <div class='modal-dialog'>");
                WriteLine(res, "    <div class='modal-content'>");
                WriteLine(res, "      <div class='modal-header'>");
                WriteLine(res, "        <button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span></button>");
                WriteLine(res, "        <h4 class='modal-title'>View " + type + "</h4>");
                WriteLine(res, "      </div>");
                WriteLine(res, "      <div class='modal-body'>");
                WriteLine(res, "        <p>");
                foreach (Field field in fields)
                {
                    WriteLine(res, "        <strong>" + field.header + "</strong><br>");
                    if (field.type == FieldType.TaxonList)
                        WriteTaxonViewBox(res, field);
                    else
                    {
                        if (field.type != FieldType.Constant)
                        {
                            WriteLine(res, "        <div id='vf" + field.name + "'>" + field.value + "</div><br>");
                        }
                    }
                }
                WriteLine(res, "        </p>");
                WriteLine(res, "      </div>");
                WriteLine(res, "      <div class='modal-footer'>");
                WriteLine(res, "        <button type='button' class='btn btn-default' data-dismiss='modal'>Close</button>");
                WriteLine(res, "      </div>");
                WriteLine(res, "    </div><!-- /.modal-content -->");
                WriteLine(res, "  </div><!-- /.modal-dialog -->");
                WriteLine(res, "</div><!-- /.modal -->");
            }

            // edit dialog
            if (customEditDialog == false)
            {
                WriteLine(res, "<div id='editDialog" + table + "' class='modal'>");
                WriteLine(res, "  <div class='modal-dialog'>");
                WriteLine(res, "    <div class='modal-content'>");
                WriteLine(res, "      <div class='modal-header'>");
                WriteLine(res, "        <button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span></button>");
                WriteLine(res, "        <h4 class='modal-title' id='editDialogTitle" + table + "'>Edit " + type + "</h4>");
                WriteLine(res, "      </div>");
                WriteLine(res, "      <div class='modal-body'>");
                foreach (Field field in fields)
                {
                    if (field.type == FieldType.Constant)
                    {
                        WriteLine(res, "        <input class='form-control' type='hidden' id='ef" + field.name + "'></input><br>");
                    }
                    else if (field.type == FieldType.IFrame)
                    {
                        WriteFrameBox(res, field);
                    }
                    else
                    {
                        WriteLine(res, "        <strong>" + field.header + "</strong><br>");

                        if (field.type == FieldType.Photo)
                        {
                            WritePhotoBox(res, field);
                        }
                        else if (field.type == FieldType.TaxonList)
                        {
                            WriteTaxonListBox(res, field);
                        }
                        else if (field.type == FieldType.Date)
                        {
                            WriteDateBox(res, field);
                        }
                        else if (field.type == FieldType.Time)
                        {
                            WriteTimeBox(res, field);
                        }
                        else if (field.type == FieldType.Taxon)
                        {
                            WriteTaxonBox(res, field);
                        }
                        else if (field.type == FieldType.Combo)
                        {
                            WriteComboBox(res, field);
                        }
                        else if (field.type == FieldType.Paragraph)
                        {
                            WriteLine(res, "        <textarea class='form-control' id='ef" + field.name + "' rows=3></textarea><br>");
                        }
                        else if (field.type == FieldType.ReadOnly)
                        {
                            WriteLine(res, "        <input class='form-control' readonly type='text' id='ef" + field.name + "'></input><br>");
                        }
                        else
                        {
                            WriteLine(res, "        <input class='form-control' type='text' id='ef" + field.name + "'></input><br>");
                        }
                    }
                }
                WriteLine(res, "        </p>");
                WriteLine(res, "      </div>");
                WriteLine(res, "      <div class='modal-footer'>");
                WriteLine(res, "        <button id='buttonSave' target='" + table + "' type='button' class='btn btn-default'>Save</button>");
                WriteLine(res, "        <button type='button' class='btn ' data-dismiss='modal'>Cancel</button>");
                WriteLine(res, "      </div>");
                WriteLine(res, "    </div><!-- /.modal-content -->");
                WriteLine(res, "  </div><!-- /.modal-dialog -->");
                WriteLine(res, "</div><!-- /.modal -->");
            }


            // delete dialog
            WriteLine(res, "<div id='deleteDialog" + table + "' class='modal'>");
            WriteLine(res, "  <div class='modal-dialog'>");
            WriteLine(res, "    <div class='modal-content'>");
            WriteLine(res, "      <div class='modal-header'>");
            WriteLine(res, "        <button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span></button>");
            WriteLine(res, "        <h4 class='modal-title'>Delete " + type + "</h4>");
            WriteLine(res, "      </div>");
            WriteLine(res, "      <div class='modal-body'>");
            WriteLine(res, "      Are you sure?");
            WriteLine(res, "      </div>");
            WriteLine(res, "      <div class='modal-footer'>");
            WriteLine(res, "        <button id='buttonDelete' type='button' target='" + table + "' class='btn btn-default'>Delete</button>");
            WriteLine(res, "        <button type='button' class='btn ' data-dismiss='modal'>Cancel</button>");
            WriteLine(res, "      </div>");
            WriteLine(res, "    </div><!-- /.modal-content -->");
            WriteLine(res, "  </div><!-- /.modal-dialog -->");
            WriteLine(res, "</div><!-- /.modal -->");


            // taxon dialog
            WriteLine(res, "<div id='taxonDialog' class='modal'>");
            WriteLine(res, "  <div class='modal-dialog modal-wide'>");
            WriteLine(res, "    <div class='modal-content'>");
            WriteLine(res, "      <div class='modal-header'>");
            WriteLine(res, "        <button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span></button>");
            WriteLine(res, "        <h4 class='modal-title'>Select Taxonomy</h4>");
            WriteLine(res, "      </div>");
            WriteLine(res, "      <div class='modal-body'>");
            WriteLine(res, "      <strong>Taxonomy</strong>");
            WriteLine(res, "      <div class='input-group'>");
            WriteLine(res, "          <input type='text' class='form-control' placeholder='Search' id='find' name='find' />");
            WriteLine(res, "          <div class='input-group-btn'>");
            WriteLine(res, "              <button class='btn btn-default findTaxonButton' type='submit'><i class='glyphicon glyphicon-search'></i></button>");
            WriteLine(res, "          </div>");
            WriteLine(res, "      </div>");
            WriteLine(res, "      <br />");
            WriteLine(res, "      <table class='table'>");
            WriteLine(res, "          <tr>");
            WriteLine(res, "              <td class='col-md-4'>");
            WriteLine(res, "                  Results:");
            WriteLine(res, "              </td>");
            WriteLine(res, "              <td class='col-md-8'>");
            WriteLine(res, "                  Details:");
            WriteLine(res, "              </td>");
            WriteLine(res, "          </tr>");
            WriteLine(res, "          <tr>");
            WriteLine(res, "              <td class='col-md-4'>");
            WriteLine(res, "                  <div id='results' style='height: 430px; overflow-y: scroll; border: 1px solid #f0f0f0; padding: 4px 4px 4px 4px'></div>");
            WriteLine(res, "              </td>");
            WriteLine(res, "              <td class='col-md-8'>");
            WriteLine(res, "                  <div id='details' style='border: 1px solid #f0f0f0; padding: 4px 4px 4px 4px'></div>");
            WriteLine(res, "              </td>");
            WriteLine(res, "          </tr>");
            WriteLine(res, "      </table>");
            WriteLine(res, "      </div>");
            WriteLine(res, "      <div class='modal-footer'>");
            WriteLine(res, "        <button id='buttonTaxonSelect' type='button' class='btn btn-default'>Select</button>");
            WriteLine(res, "        <button type='buttonTaxonCancel' class='btn' data-dismiss='modal'>Cancel</button>");
            WriteLine(res, "      </div>");
            WriteLine(res, "    </div><!-- /.modal-content -->");
            WriteLine(res, "  </div><!-- /.modal-dialog -->");
            WriteLine(res, "</div><!-- /.modal -->");


        }

        public String GetFieldText(Field field, String val)
        {
            String text = val;

            if (field.type == FieldType.Photo)
            {
                text = String.Format("<img src='ic.aspx?url={0}&maxWidth=300' width=300></src>", context.Server.UrlEncode(val));
            }

            if (field.type == FieldType.Link)
            {
                text = String.Format("<a target='_blank' href='{0}'>{0}</a>", val);
            }

            if (field.type == FieldType.Taxon)
            {
                int id = int.Parse(val);
                String query = String.Format("SELECT fTaxonomyName FROM TblTaxonomy WHERE fTaxonomyID = {0}", id);
                using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader set = command.ExecuteReader())
                        {
                            set.Read();
                            text = set["fTaxonomyName"].ToString();
                        }
                    }
                }

            }

            if (field.type == FieldType.Combo)
            {
                int id = int.Parse(val);
                String query = String.Format("SELECT {0} FROM {1} WHERE {2} = {3}", field.lookUpFieldName, field.lookUpTable, field.lookUpFieldID, id);
                

                using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader set = command.ExecuteReader())
                        {
                            set.Read();
                            text = set[field.lookUpFieldName].ToString();
                        }
                    }
                }
            }

            return text;

        }

        public void WriteTaxonViewBox(HttpResponse res, Field field)
        {
            WriteLine(res, "        <iframe id='' name='' class='taxonList' style='width: 100%; height: 210px; border: 0px solid #f0f0f0;'>");
            WriteLine(res, "        </iframe>");
        }

        public void WriteFrameBox(HttpResponse res, Field field)
        {
            WriteLine(res, "        <iframe id='ef" + field.header + "' name='ef" + field.header + "' class='ef" + field.header + "' style='width: 100%; height: " + field.height + "px; border: 0px solid #f0f0f0;'>");
            WriteLine(res, "        </iframe>");
        }

        public void WriteTaxonListBox(HttpResponse res, Field field)
        {
            WriteLine(res, "        <iframe id='taxonList' name='taxonList' class='taxonList' style='width: 100%; height: 250px; border: 0px solid #f0f0f0;'>");
            WriteLine(res, "        </iframe>");
        }


        public void WritePhotoBox(HttpResponse res, Field field)
        {
            WriteLine(res, "        <iframe id='ef" + field.name + "' name='photobox' class='photobox' style='width: 100%; height: 560px; border: 1px solid #f0f0f0;'>");
            WriteLine(res, "        </iframe>");
        }

        public void WriteTimeBox(HttpResponse res, Field field)
        {
            WriteLine(res, " <input type='text' name='start_time' class='timepicker' placeholder='h:mm PM' data-default-time='false'>");
        }

        public void WriteDateBox(HttpResponse res, Field field)
        {
            WriteLine(res, "        <div class='input-group date datepicker' id='ef" + field.name + "'>");
            WriteLine(res, "        <input type='text' class='form-control' />");
            WriteLine(res, "        <span class='input-group-addon'><span class='glyphicon glyphicon-calendar'></span>");
            WriteLine(res, "        </span>");
            WriteLine(res, "        </div><br>");
        }

        public void WriteTaxonBox(HttpResponse res, Field field)
        {
            WriteLine(res, "        <div class='input-group'>");
            WriteLine(res, "            <input type='text' class='form-control' placeholder='Search' id='ef" + field.name + "' />");
            WriteLine(res, "            <div class='input-group-btn'>");
            WriteLine(res, "                <button class='btn btn-default taxonButton' id='eb" + field.name + "' type='submit'><i class='glyphicon glyphicon-search'></i></button>");
            WriteLine(res, "            </div>");
            WriteLine(res, "        </div>");
            WriteLine(res, "        <br />");
        }

        public void WriteComboBox(HttpResponse res, Field field)
        {
            WriteLine(res, "        <select class='form-control' id='ef" + field.name + "'>");
            String query = String.Format("SELECT {0},{1} FROM {2}", field.lookUpFieldID, field.lookUpFieldName, field.lookUpTable);
            if (field.lookUpFilter != null && field.lookUpFilter != "")
                query += " WHERE " + field.lookUpFilter;
            query += " ORDER BY " + field.lookUpFieldName;

            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            WriteLine(res, String.Format("<option value='{0}'>{1}</option>", set[field.lookUpFieldID].ToString(), set[field.lookUpFieldName].ToString()));
                        }
                    }
                }
            }
            WriteLine(res, "        </select><br>");
        }

        public void loadRecord(HttpResponse res, int id)
        {
            if (id == 0)
            {
                WriteLine(res, "[");
                bool firstField = true;
                foreach (Field field in fields)
                {
                    if (firstField == true)
                        firstField = false;
                    else
                        res.Write(",");
                    res.Write("{");
                    res.Write("name: '" + field.name + "', ");
                    res.Write("value: '" + field.value + "',");
                    res.Write("text: ''");
                    res.Write("}");
                }
                WriteLine(res, "]");
            }
            else
            {
                // write data
                String fieldList = "[" + idField + "]";
                foreach (Field field in fields)
                {
                    if (field.name != "")
                        fieldList += ", [" + field.name + "]";
                }
                String query = "SELECT " + fieldList + " FROM " + table + " WHERE [" + idField + "] = " + id;
                using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader set = command.ExecuteReader())
                        {
                            set.Read();
                            WriteLine(res, "[");
                            bool firstField = true;
                            foreach (Field field in fields)
                            {
                                String value = "";
                                if (field.name != "")
                                    value = set[field.name].ToString();

                                if (firstField == true)
                                    firstField = false;
                                else
                                    res.Write(",");
                                res.Write("{");
                                res.Write("name: '" + field.name + "', ");
                                if (field.type == FieldType.Taxon)
                                {
                                    res.Write("value: " + Json.Encode(GetFieldText(field, value) + " (" + set["fWoRMID"].ToString() + ")") + ", ");
                                }
                                else
                                {
                                    res.Write("value: " + Json.Encode(value) + ", ");
                                }
                                res.Write("text: " + Json.Encode(GetFieldText(field, value)));
                                res.Write("}");
                            }
                            WriteLine(res, "]");
                        }
                    }
                }
            }
        }

        public void deleteRecord(HttpRequest req, HttpResponse res)
        {
            int id = int.Parse(req["id"]);
            String command = "";
            command = String.Format("DELETE FROM {0} WHERE {1} = {2}", table, idField, id);
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand myCommand = new SqlCommand(command, connection))
                {
                    myCommand.ExecuteNonQuery();
                }
            }
        }

        public void saveRecord(HttpRequest req, HttpResponse res)
        {
            int id = int.Parse(req["id"]);
            String command = "";

            for (int i = 0; i < fields.Count; i++)
            {
                Field field = fields[i];
                field.value = req[field.name];

                if (field.type == FieldType.Taxon)
                    field.value = getTaxonID(res, field.value);
            }


            if (id == 0)
            {
                // insert
                command = "INSERT INTO " + table + "(";
                for (int i = 0; i < fields.Count; i++)
                {
                    Field field = fields[i];
                    if (field.name != "")
                    {
                        if (i > 0)
                            command += ", ";
                        command += field.name;
                    }
                }
                command += ") VALUES (";
                for (int i = 0; i < fields.Count; i++)
                {
                    Field field = fields[i];
                    if (field.name != "")
                    {
                        if (i > 0)
                            command += ", ";
                        command += "@" + field.name;
                    }
                }
                command += ")";
            }
            else
            {
                // update
                command = "UPDATE " + table + " SET ";
                bool firstField = true;
                for (int i = 0; i < fields.Count; i++)
                {
                    Field field = fields[i];
                    if (field.name != "" && field.type != FieldType.Photo && field.type != FieldType.Hidden && field.type != FieldType.ReadOnly)
                    {
                        if (firstField == true)
                            firstField = false;
                        else
                            command += ", ";
                        command += field.name + "=@" + field.name;
                    }
                }
                command += " WHERE " + idField + "=" + id;
            }

            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand myCommand = new SqlCommand(command, connection))
                {
                    foreach (Field field in fields)
                    {
                        if (field.name != "" && field.type != FieldType.Photo && field.type != FieldType.Hidden && field.type != FieldType.ReadOnly)
                            myCommand.Parameters.AddWithValue("@" + field.name, field.value);
                    }

                    myCommand.ExecuteNonQuery();
                    if (id == 0)
                    {
                        id = GetMaxID(table, idField);
                    }
                }
            }

            foreach (Field field in fields)
            {
                if (field.type == FieldType.TaxonList)
                {
                    // remove old
                    ExecuteSQL(String.Format("DELETE FROM TblCatalogueTaxon WHERE fCatalogueID = {0}", id));
                    // add new
                    String[] taxons = req["taxons"].Split(',');
                    foreach (String taxon in taxons)
                        ExecuteSQL(String.Format("INSERT INTO TblCatalogueTaxon (fCatalogueID, fTaxonID) VALUES ({0}, {1})", id, taxon));
                }
            }

            if (dataSavedEvent != null)
                dataSavedEvent.Invoke(id, req, res);




        }

        public void ExecuteSQL(String sql)
        {
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception err)
                    {
                        context.Response.Write(err.Message + "<br><br>");
                        context.Response.Write(sql + "<br><br>");


                    }
                }
            }
        }



        public int GetMaxID(String table, String field)
        {
            int nMaxID = 0;

            String sql = String.Format("SELECT MAX({0}) FROM {1}", field, table);
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        set.Read();
                        nMaxID = (int)set[0];
                    }
                }
            }

            return nMaxID;
        }

        int AddRank(SqlConnection con, String rank)
        {
            int nRankID = 0;
            String query = "SELECT fTaxonRankID FROM TblTaxonRank WHERE fTaxonRankName = @fTaxonRankName";
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@fTaxonRankName", rank);
                using (SqlDataReader set = command.ExecuteReader())
                {
                    if (set.Read())
                        nRankID = (int)set["fTaxonRankID"];
                }
            }

            if (nRankID == 0)
            {
                String sql = "INSERT INTO TblTaxonRank (fTaxonRankName) VALUES (@fTaxonRankName)";
                using (SqlCommand myCommand = new SqlCommand(sql, con))
                {
                    myCommand.Parameters.AddWithValue("@fTaxonRankName", rank);
                    myCommand.ExecuteNonQuery();
                }

                nRankID = AddRank(con, rank);
            }

            return nRankID;
        }

        int AddTaxon(SqlConnection con, String rank, String value, int nParentID)
        {
            int nRankID = AddRank(con, rank);

            int nTaxonID = 0;
            String query = "SELECT fTaxonomyID FROM TblTaxonomy WHERE fTaxonRankID = @fTaxonRankID AND fTaxonomyName = @fTaxonomyName AND fTaxonomyParentID=@fTaxonomyParentID";
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@fTaxonRankID", nRankID);
                command.Parameters.AddWithValue("@fTaxonomyName", value);
                command.Parameters.AddWithValue("@fTaxonomyParentID", nParentID);
                using (SqlDataReader set = command.ExecuteReader())
                {
                    if (set.Read())
                        nTaxonID = (int)set["fTaxonomyID"];
                }
            }

            if (nTaxonID == 0)
            {
                String sql = "INSERT INTO TblTaxonomy (fTaxonomyParentID,fTaxonRankID,fTaxonomyName) VALUES (@fTaxonomyParentID,@fTaxonRankID,@fTaxonomyName)";
                using (SqlCommand myCommand = new SqlCommand(sql, con))
                {
                    myCommand.Parameters.AddWithValue("@fTaxonRankID", nRankID);
                    myCommand.Parameters.AddWithValue("@fTaxonomyName", value);
                    myCommand.Parameters.AddWithValue("@fTaxonomyParentID", nParentID);
                    myCommand.ExecuteNonQuery();
                }

                nTaxonID = AddTaxon(con, rank, value, nParentID);
            }


            return nTaxonID;
        }

        public string getTaxonID(int AphiaID, String name)
        {
            int nTaxonomyID = 0;

            Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();
            using (SqlConnection con = new SqlConnection(DataSources.dbConSpecies))
            {
                con.Open();
                Worms.Classification wclass = client.getAphiaClassificationByID(AphiaID);
                if (wclass != null)
                {
                    while (wclass != null)
                    {
                        if (wclass.rank != null)
                            nTaxonomyID = AddTaxon(con, wclass.rank, wclass.scientificname, nTaxonomyID);
                        wclass = wclass.child;
                    }


                }

                nTaxonomyID = AddTaxon(con, "SaeonID", name, nTaxonomyID);
            }

            return nTaxonomyID.ToString();

        }

        string getTaxonID(HttpResponse res, String value)
        {
            String name = value;

            int s = name.IndexOf('(');
            int e = name.IndexOf(')');
            int AphiaID = int.Parse(name.Substring(s + 1, e - s - 1));

            Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();
            Worms.AphiaRecord record = client.getAphiaRecordByID(AphiaID);
            if (record == null)
            {
                res.Write("Error: failed to get record");
                res.End();
            }

            return getTaxonID(AphiaID, name);
        }

    }
}