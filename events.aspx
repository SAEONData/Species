<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="events.aspx.cs" Inherits="species.events" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var surveyID = '<%=fSurveyID%>';
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js"></script>
    <script type="text/javascript">


        function loadFilters() {
            var filter = "";
            filter += 'survey=' + escape($('#selSurvey').val());
            filter += '|level2=' + escape($('#selectLevel2').val());
            filter += '|level3=' + escape($('#selectLevel3').val());
            filter += '|station=' + escape($('#selectStation').val());
            filter += '|date1=' + escape(getDatePickerDate('selDateFrom'));
            filter += '|date2=' + escape(getDatePickerDate('selDateTo'));
            loadTable(null, filter);
            return true;
        }

        function initFunc() {

        }


        $(function () {

            $('#selSurvey').change(function () {
                var id = $(this).val();
                document.location = 'events.aspx?sv=' + id;
            });

            $('#selectLevel3').change(loadFilters);
            $('#selectStation').change(loadFilters);

            $('#selDateFrom').datepicker().on('changeDate', function () {
                $('#selDateFrom').datepicker('hide');
                loadFilters();
            });

            $('#selDateTo').datepicker().on('changeDate', function () {
                $('#selDateTo').datepicker('hide');
                loadFilters();
            });

            $('#effStartTime').timepicker({ use24hours: true, format: 'HH:mm', showMeridian: false });


            setTimeout('initFunc();', 1000);
           


            function loadLevel3() {
                var survey = $('#selSurvey').val();
                var level2 = $('#selectLevel2').val();
                var url = 'ajaxSpecies.ashx?mode=loadLevel3&survey=' + survey + '&level2=' + level2;
                $.ajax(url).done(function (ret) {
                    eval('data = ' + ret);
                    clearCombo("selectLevel3");
                    addComboOption("selectLevel3", "", "All");
                    if (data.length == 0) {
                        enableControl('selectLevel3', false);
                    }
                    else {
                        enableControl('selectLevel3', true);
                        for (var i = 0; i < data.length; i++) {
                            var item = data[i];
                            addComboOption("selectLevel3", item, item);
                        }
                    }
                });
            }
            

            $('#selectLevel2').change(function () {
                loadLevel3();
                loadFilters();
            });


            global.onLoadEdit = function (id) {
                $('#indicatorFrame').attr('src', 'eventIndicators.aspx?survey=' + surveyID + '&id=' + id);
                $('#speciesFrame').attr('src', 'eventspecies.aspx?survey=' + surveyID + '&id=' + id);

            }

            global.onSaveFunc = getIndicators;
            loadLevel3();
        });

        function getIndicators() {
            return window.frames['indicatorFrame'].getIndicators();
        }

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table>
        <tr>
            <td>
                <strong>Survey:</strong>
            </td>
            <td style="width: 20px"></td>
            <% if (level2Name != "" ) { %>
            <td>
                <strong id="headerLevel2"><%=level2Name%>:</strong>
            </td>
            <td style="width: 20px"></td>
            <% } %>
            <% if (level3Name != "" ) { %>
            <td>
                <strong id="headerLevel3"><%=level3Name%>:</strong>
            </td>
            <td style="width: 20px"></td>
            <% } %>
            <td>
                <strong id="Strong3">Depth:</strong>
            </td>
            <td style="width: 20px"></td>
            <td>
                <strong id="Strong1">Date From:</strong>
            </td>
            <td style="width: 20px"></td>
            <td>
                <strong id="Strong2">Date To:</strong>
            </td>
        </tr>
        <tr>
            <td>
                <select class="form-control" id="selSurvey" style="width: 200px">
                    <% WriteSurveys(); %>
                </select>
            </td>
            <td style="width: 20px"></td>
            <% if (level2Name != "" ) { %>
            <td>
                <select class="form-control" id="selectLevel2" style="width: 150px">
                    <% WriteComboItems(level2Names, true); %>
                </select>
            </td>
            <td style="width: 20px"></td>
            <% } %>
            <% if (level3Name != "" ) { %>
            <td>
                <select disabled class="form-control" id="selectLevel3" style="width: 150px">
                    <option>All</option>
                </select>
            </td>
            <td style="width: 20px"></td>
            <% } %>
            <td>
                <select class="form-control" id="selectStation" style="width: 150px">
                    <option value="0">All</option>
                    <option value="1">0m - 100m</option>
                    <option value="2">100m - 200m</option>
                    <option value="3">200m - 300m</option>
                    <option value="4">300m - 400m</option>
                    <option value="5">400m - 500m</option>
                    <option value="6">500m - 600m</option>
                    <option value="7">600m - 700m</option>
                    <option value="8">700m - 800m</option>
                </select>
            </td>
            <td style="width: 20px"></td>
            <td>
                <div class="input-group date " id="selDateFrom" style="width: 150px">
                    <input type="text" class="form-control" placeholder="DD/MM/YYYY" />
                    <span class="input-group-addon"><span class="glyphicon-calendar glyphicon"></span>
                    </span>
                </div>
            </td>
            <td style="width: 20px"></td>
            <td>
                <div class="input-group date " id="selDateTo" style="width: 150px">
                    <input type="text" class="form-control" placeholder="DD/MM/YYYY" />
                    <span class="input-group-addon"><span class="glyphicon-calendar glyphicon"></span>
                    </span>
                </div>
            </td>
        </tr>
    </table>

    <br />

    <div id="tableDiv"></div>
    <div id="dialogsDiv"></div>

    <div id="editDialogTblEvent" class="modal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
                    <h4 class="modal-title" id="editDialogTitleTblEvent">Edit Event 2</h4>
                </div>
                <div class="modal-body">
                    <ul id="editTabs" class="nav nav-tabs" role="tablist">
                        <li role="presentation" class="active"><a href="#edit-general" role="tab" id="edit-general-tab" data-toggle="tab" aria-controls="edit-general" aria-expanded="false">General</a></li>
                        <!--<li role="presentation" class=""><a href="#edit-indicators" role="tab" id="edit-indicators-tab" data-toggle="tab" aria-controls="edit-indicators" aria-expanded="false">Indicators</a></li>-->
                        <li role="presentation" class=""><a href="#edit-species" role="tab" id="edit-species-tab" data-toggle="tab" aria-controls="edit-species" aria-expanded="false">Species</a></li>
                    </ul>

                    <div id="Div1" class="tab-content">
                        <div role="tabpanel" class="tab-pane active fade in" id="edit-general" aria-labelledby="edit-general-tab">
                            <input class="form-control" type="hidden" id="effSurveyID" value="2" /><br />
                            <strong>Voyage Number</strong><br />
                            <input class="form-control" type="text" id="effLevel2Name" /><br />
                            <strong>Trawl</strong><br />
                            <input class="form-control" type="text" id="effLevel3Name" /><br />
                            <strong>Event</strong><br />
                            <input class="form-control" type="text" id="effEventName" /><br />
                            <strong>Station</strong><br />
                            <select class="form-control" id="effStationID">
                                <% WriteComboItems(stations, true); %>
                            </select><br />
                            <strong>Date</strong><br />
                            <div class="input-group date datepicker" id="effStartDate">
                                <input type="text" class="form-control" />
                                <span class="input-group-addon"><span class="glyphicon-calendar glyphicon"></span>
                                </span>
                            </div>
                            <br />
                            <strong>Time</strong><br />
                            <form>
                            <div class="input-group">
                                <input type="text" class="form-control" placeholder='HH:mm' id="effStartTime" data-date-format="HH:mm" data-date-useseconds="false" data-date-pickDate="false" />
                                <span class="input-group-addon"><span class="glyphicon-time glyphicon"></span>
                                </span>
                            </div>
                            </form>
                            <br />
                            <strong>Duration</strong><br />
                            <input class="form-control" type="text" id="effDuration" /><br />
                            <iframe id="indicatorFrame" name="indicatorFrame" class="indicatorFrame" style="width: 100%; height: 130px; border: 0px solid #f0f0f0; overflow-y: hidden" scrolling="no"></iframe>
                        </div>

                        <div role="tabpanel" class="tab-pane" id="edit-indicators" aria-labelledby="edit-indicators-tab">
                        </div>

                        <div role="tabpanel" class="tab-pane" id="edit-species" aria-labelledby="edit-species-tab">
                            <br />
                            <iframe id="speciesFrame" name="speciesFrame" class="speciesFrame" style="width: 100%; height: 500px; border: 1px solid #f0f0f0"></iframe>
                        </div>



                    </div>

                </div>
                <div class="modal-footer">
                    <button id="buttonSave" type="button" target="TblEvent" class="btn btn-default">Save</button>
                    <button type="button" class="btn " data-dismiss="modal">Cancel</button>
                </div>
            </div>
            <!-- /.modal-content -->
        </div>
        <!-- /.modal-dialog -->
    </div>

</asp:Content>
