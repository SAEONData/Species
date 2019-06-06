<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="import.aspx.cs" Inherits="species.import" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="plupload/js/moxie.min.js"></script>
    <script src="plupload/js/plupload.min.js"></script>
    <style>
        .bottomspacer {
            padding-bottom: 10px;
        }
    </style>
    <script type="text/javascript">

        var rowGUID = 100;

        $(document).ready(function () {
            $('.addButton').off();

            

            var uploader = new plupload.Uploader({
                fileCount: 0,
                browse_button: 'btnAddItem1',
                container: 'container',
                url: 'UploadHandler.ashx?folder=import',
                filters: [
                    { title: "Microsoft Excel Files", extensions: "xls,xlsx" }
                ],
                init: {
                    FilesAdded: function (up, files) {
                        setTimeout(function () { up.start(); }, 100);
                    },
                    UploadComplete: function (up, files) {
                        $.each(files, function (i, file) {
                            // Do stuff with the file. There will only be one file as it uploaded straight after adding!
                        });
                    }
                }
            });

            uploader.init();

            uploader.bind('FilesAdded', function (up, files) {
                if (up.fileCount == null)
                    up.fileCount = 0;
                up.fileCount += files.length;
                console.log('start: file count = ' + up.fileCount);
                $.each(files, function (i, file) {
                    $('#filelist').append(
                        '<div id="' + file.id + '">' + file.name + ' (' + plupload.formatSize(file.size) + ') <b></b>' +
                        '</div>');
                });

                up.refresh();
            });

            uploader.bind('UploadProgress', function (up, file) {
                $('#' + file.id + " b").html(file.percent + "%");
            });

            uploader.bind('Error', function (up, err) {
                $('#filelist').append("<div>Error: " + err.code +
                    ", Message: " + err.message + (err.file ? ", File: " + err.file.name : "") +
                    "</div>");
                up.refresh(); // Reposition Flash/Silverlight
            });

            uploader.bind('FileUploaded', function (up, file) {
                up.fileCount--;
                $('#' + file.id + " b").html("Done");
                console.log('done: file count = ' + up.fileCount);
                if (up.fileCount == 0) {
                    var path = 'temp/import/' + file.name;
                    var url = 'import.aspx?mode=getfields&file=' + escape(path);
                    $('#file').val(path);
                    var lname = file.name.toLowerCase();
                    if (lname.indexOf('catch') > -1)
                        $('#importType').val('catch');
                    else
                        $('#importType').val('trawl');
                    showSurveyRows();
                    $.ajax(url).done(loadComboFields);
                    
                }
            });

            $('#importType').change(showSurveyRows);
            showSurveyRows();
            $('#panelOptions').hide();
            $('#panelIndicators').hide();
            $('#panelImport').hide();

            $('#btnAddIndicator').click(addIndicator);

            $('#btnImport').click(function () {
                var table = $('#indicatorRows');
                var indicators = '';
                table.find('tr').each(function () {
                    var row = $('#' + this.id);
                    var indic = row.attr('indic');
                    var field = row.attr('field');
                    if (indicators != '')
                        indicators += '~';
                    indicators += indic + '|' + field;
                });
                $('#indicators').val(indicators);
                form1.submit();
            });




        });

        function addIndicator() {
            var field = $('#selIndicatorField').val();
            var indic = $('#selIndicator').val();
            var text = $("#selIndicator option:selected").text();

            if (field == null || field == '') {
                alert("please select a field");
                return;
            }

            var code = '';
            code += '<tr id="importRow' + rowGUID + '" field="' + field + '" indic="' + indic + '" >';
            code += '<td>' + field + '</td>';
            code += '<td>' + text + '</td>';
            code += '<td>';
            code += '<button type="button" id="db' + rowGUID + '" class="btn btn-sm delrowbutton" style="position: relative; z-index: 1; width: 80px"><span class="glyphicon glyphicon glyphicon-minus" aria-hidden="true"></span>&nbsp;Remove</button>';
            code += '</td>';
            code += '</tr>';
            $('#indicatorRows').append($(code));


            $('.delrowbutton').off();
            $('.delrowbutton').click(function () {
                var id = this.id.substr(2);
                $('#importRow' + id).remove();
            });

            rowGUID++;

        }

        function loadComboFields(ret) {
            eval('items = ' + ret);
            $('.fieldSelector').empty();
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                $('.fieldSelector').append($('<option/>', { value: item, text: item }));
            }
            $(".fieldSelector").each(function () {
                $(this).val($(this).attr('title'));
            });
            $('#panelOptions').show();
            $('#panelIndicators').show();
            $('#panelImport').show();
            showSurveyRows();

        }

        function showSurveyRows() {
            var type = $('#importType').val();
            if (type == 'trawl') {
                $('.rowCatch').hide();
                $('.rowTrawl').show();
                $('#panelIndicators').show();
            }
            else {
                $('.rowTrawl').hide();
                $('.rowCatch').show();
                $('#panelIndicators').hide();
            }
        }



    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div id="container">

        <!-- panel with list of uploaded documents -->
        <div class="panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">Spreadsheet</h3>
            </div>
            <div class="panel-body">
                <div id="filelist" class="panel" style="border: 0px solid"></div>
                <button type="button" id="btnAddItem1" target="TblMedia" class="btn btn-sm addButton" style="position: relative; z-index: 1;"><span class="glyphicon glyphicon-floppy-open" aria-hidden="true"></span>&nbsp;Select Spreadsheet</button>
            </div>
        </div>

        <!-- panel of import options -->
        <form id="form1" name="form1">

            <div class="panel panel-default" id="panelOptions">
                <div class="panel-heading">
                    <h3 class="panel-title">Import Fields</h3>
                </div>
                <div class="panel-body">
                    <table>
                        <tr>
                            <td class="bottomspacer" style="width: 160px">Import Type:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control" name="importType" id="importType" style="width: 200px">
                                    <option value="trawl">Trawl Log</option>
                                    <option value="catch">Catch Log</option>
                                </select>
                            </td>
                        </tr>


                        <tr class="rowTrawl">
                            <td class="bottomspacer">Survey Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="fieldSurvey" name="fieldSurvey" title="Survey" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">Cruise Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="fieldCruise" name="fieldCruise" title="Cruise" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">Trawl No. Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="fieldTrawl" name="fieldTrawl" title="Trawl No." style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">Station Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="fieldStation" name="fieldStation" title="station" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">Date Fields:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <table>
                                    <tr>
                                        <td>Year:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>Month:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>Day:
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <select class="form-control fieldSelector" id="dateYear" name="dateYear" title="yy" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="dateMonth" name="dateMonth" title="mm" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="dateDay" name="dateDay" title="dd" style="width: 200px">
                                            </select>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">Duration Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="Duration" name="Duration" title="duration (min)" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">Start Coordinate:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <table>
                                    <tr>
                                        <td>Start lat hour:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>Start lat min:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>Start long hour:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>Start long min:
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <select class="form-control fieldSelector" id="sLatHour" name="sLatHour" title="Start lat hour" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="sLatMin" name="sLatMin" title="Start lat min" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="sLonHour" name="sLonHour" title="Start long hour" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="sLonMin" name="sLonMin" title="Start long min" style="width: 200px">
                                            </select>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">End Coordinate:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <table>
                                    <tr>
                                        <td>End lat hour:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>End lat min:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>End long hour:
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>End long min:
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <select class="form-control fieldSelector" id="eLatHour" name="eLatHour" title="End lat hour" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="eLatMin" name="eLatMin" title="End lat min" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="eLonHour" name="eLonHour" title="End long hour" style="width: 200px">
                                            </select>
                                        </td>
                                        <td style="width: 8px"></td>
                                        <td>
                                            <select class="form-control fieldSelector" id="eLonMin" name="eLonMin" title="End long min" style="width: 200px">
                                            </select>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr class="rowTrawl">
                            <td class="bottomspacer">Comments:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="Comments" name="Comments" title="Comments" style="width: 200px">
                                </select>
                            </td>
                        </tr>



                        <tr class="rowCatch">
                            <td class="bottomspacer">Cruise Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchCruise" name="catchCruise" title="Cruise #" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Trawl Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchTrawl" name="catchTrawl" title="Trawl #" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Station Field:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchStation" name="catchStation" title="Station #" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Common name:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchCommon" name="catchCommon" title="Common name" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Genus:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchGenus" name="catchGenus" title="Genus" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Species:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchSpecies" name="catchSpecies" title="species" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Abundance:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchAbundance" name="catchAbundance" title="Abundance (# individuals)" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Biomass (kg):&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchBiomass" name="catchBiomass" title="Biomass (kg)" style="width: 200px">
                                </select>
                            </td>
                        </tr>

                        <tr class="rowCatch">
                            <td class="bottomspacer">Description:&nbsp;
                            </td>
                            <td class="bottomspacer">
                                <select class="form-control fieldSelector" id="catchDescription" name="catchDescription" title="Description" style="width: 200px">
                                </select>
                            </td>
                        </tr>


                    </table>

                </div>
            </div>


        <!-- panel of import options -->

            <div class="panel panel-default" id="panelIndicators">
                <div class="panel-heading">
                    <h3 class="panel-title">Import Indicators</h3>
                </div>
                <div class="panel-body">
                    <table class="table">
                        <tr>
                            <td style="width: 200px">
                                Indicator Field:
                            </td>
                            <td style="width: 200px">
                                Indicator:
                            </td>
                            <td style="width: 200px">
                                &nbsp;
                            </td>

                        </tr>

                        <tr>
                            <td style="width: 215px">
                                <select class="form-control fieldSelector" id="selIndicatorField" title="" style="width: 200px">
                                </select>
                            </td>
                            <td style="width: 215px">
                                <select class="form-control" id="selIndicator" title="" style="width: 200px">
                                    <% WriteIndicators(); %>
                                </select>
                            </td>
                            <td style="width: 215px">
                                <button type="button" id="btnAddIndicator" class="btn btn-sm" style="position: relative; z-index: 1; width: 80px"><span class="glyphicon glyphicon glyphicon-plus" aria-hidden="true"></span>&nbsp;Add</button>
                            </td>

                        </tr>

                    </table>
                    <br />

                    <table class="table">
                        <tr>
                            <td style="width: 215px">
                                Indicator Field:
                            </td>
                            <td style="width: 215px">
                                Indicator:
                            </td>
                            <td style="width: 215px">
                                &nbsp;
                            </td>

                        </tr>

                        <tbody id="indicatorRows">

                        </tbody>

                    </table>

                </div>

            </div>

            <div class="panel panel-default" id="panelImport">
                <div class="panel-body">
                    <input type="hidden" id="mode" name="mode" value="importExcel" />
                    <input type="hidden" id="file" name="file" value="importExcel" />
                    <input type="hidden" id="indicators" name="indicators" value="" />

                    <button type="button" id="btnImport" class="btn btn-sm" style="position: relative; z-index: 1; width: 100px"><span class="glyphicon glyphicon glyphicon-save" aria-hidden="true"></span>&nbsp;Import</button>
                </div>

            </div>

        </form>





    </div>



</asp:Content>


