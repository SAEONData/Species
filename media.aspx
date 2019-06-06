<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="media.aspx.cs" Inherits="species.media" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js"></script>
    <script src="plupload/js/moxie.min.js"></script>
    <script src="plupload/js/plupload.min.js"></script>

    <script>

        function reloadTable() {
            var rank = $('#selRank').val();
            var taxon = $('#selTaxon').val();

            var filter = "";
            filter += 'rank=' + escape($('#selRank').val());
            filter += '|taxon=' + escape($('#selTaxon').val());
            filter += '|find=' + escape($('#textFilter').val());
            filter += '|untagged=' + document.getElementById('cbUntagged').checked;
            loadTable(initUploaders, filter);
        }

        function initUploader(ctrl) {
            $('.addButton').off();


            var uploader = new plupload.Uploader({
                fileCount: 0,
                browse_button: ctrl,
                container: 'container',
                url: 'UploadHandler.ashx?folder=biome',
                filters: [
                    { title: "Image files", extensions: "jpg,gif,png" },
                    { title: "Zip files", extensions: "zip" }
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
                $('#' + file.id + " b").html("100%");
                console.log('done: file count = ' + up.fileCount);
                if (up.fileCount == 0) {
                    $('#filelist').html('');
                    $('#filelist').hide();
                    reloadTable();
                }
            });

            $(".addWebFolder").off();
            $(".addWebFolder").click(function () {
                $('#webFolderPath').val("");
                $('#webFolderSpeciesID').val(0);
                $('#webFolderSpecies').val("");
                $('#dlgSelectWebFolder').modal();
            });

            function selectSpeciesRF(id, name) {
                $('#webFolderSpeciesID').val(id);
                $('#webFolderSpecies').val(name);
            }

            $('.webFolderSpeciesSB').off();
            $('.webFolderSpeciesSB').click(function () {
                selectSpecies($('#webFolderSpecies').val(), selectSpeciesRF);
            });

            $('#btnWebFolderOK').off();
            $('#btnWebFolderOK').click(function () {
                // http://media.dirisa.org/inventory/upload/egagasini/species/Africana%20V270%20Trawl%20Photos/jsonContent?type=Image
                var url = $('#webFolderPath').val().trim();
                if (url == "") {
                    alert('Please enter an URL to the Portal Folder path');
                    return;
                }
                if (url.indexOf('jsonContent') == -1) {
                    if (url.substr(url.length - 1, 1) != '/')
                        url += '/';
                    url += 'jsonContent?type=Image';
                }

                var speciesID = parseInt($('#webFolderSpeciesID').val());
                if ($('#webFolderSpeciesID').val() == "")
                    speciesID = 0;
                var doc = 'loadPortalFolder.aspx?url=' + escape(url) + '&species=' + speciesID;
                ajaxindicatorstart('Loading portal data');
                $.ajax(doc).done(function (ret) {
                    reloadTable();
                    ajaxindicatorstop();
                    $('#dlgSelectWebFolder').modal('toggle');
                });
                


            });






            $(".textFilter").off();
            $(".textFilter").click(function () {
                reloadTable();
                return false;
            });


        }

        function initUploaders() {
            setTimeout("initUploader('btnAddItem1');", 500);
            setTimeout("initUploader('btnAddItem2');", 500);
        }

        function loadSpeciesTable() {
            var species = $('#selSpecies').val();
            reloadTable();
        }

        $(function () {
            global.reloadFunction = initUploaders;
            $('#cbUntagged').change(function () {
                var untagged = this.checked;
                $('#selSpecies').attr('disabled', untagged);
                if (untagged == true)
                    reloadTable();
                else
                    loadSpeciesTable();
            });

            $('#selSpecies').change(loadSpeciesTable);
        });

        function loadTaxonBox(code) {
            $('#selTaxon').html('<option selected value=0>* All *</option>');
            for (var i = 0; i < code.length; i++) {
                var item = code[i];
                $('#selTaxon').append(new Option(item.name, item.id));
            }
            filterTaxon();
        }

        function loadTaxon() {
            var rank = $('#selRank').val();
            var url = 'ajaxSpecies.ashx?mode=loadTaxon&rank=' + rank;
            $.getJSON(url, null, loadTaxonBox);
        }

        function filterTaxon() {
            var taxon = $('#selTaxon').val();
            if (global.prevTaxon == null) {
                global.prevTaxon = 0;
            }
            if (global.prevTaxon != taxon) {
                global.prevTaxon = taxon;
                reloadTable();
            }
        }


        $(document).ready(function () {
            $('#selRank').change(loadTaxon);
            $('#selTaxon').change(filterTaxon);
            loadTaxon();
        });



    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="Div1">
        <table style="border: 0px solid black">
            <tr>
                <td style="width: 140px"><strong>Rank:</strong></td>
                <td style="width: 20px"></td>
                <td style="width: 140px"><strong>Filter:</strong></td>
                <td style="width: 20px"></td>
                <td style="width: 140px"><strong>Find:</strong></td>
                <td rowspan="2" style="vertical-align: bottom">&nbsp;&nbsp;<label><input type="checkbox" name="checkbox" value="value" id="cbUntagged">
                    Untagged images only</label>
                </td>


            </tr>
            <tr>
                <td>
                    <select class="form-control" id="selRank" style="width: 140px">
                        <% WriteRanks(); %>
                    </select>
                </td>
                <td></td>
                <td>
                    <select class="form-control" id="selTaxon" style="width: 140px">
                    </select>
                </td>
                <td></td>
                <td>
                    <form>
                    <div class="input-group" style="width: 200px">
                        <input type="text" class="form-control" placeholder="Search" id="textFilter" name="textFilter" value="" />
                        <div class="input-group-btn textFilter">
                            <button class="btn btn-default" type="submit"><i class="glyphicon glyphicon-search"></i></button>
                        </div>
                    </div>
                    </form>
                </td>
            </tr>
        </table>
    </div>
    <!--
    <div id="filterDiv">
        <table border="0">
            <tr>
                <td style="width:240px"><strong>Species:</strong></td>
                <td style="width:20px"></td>
                <td rowspan="2" style="vertical-align: bottom">
                    
                </td>
            </tr>
            <tr>
                <td>
                    <select class="form-control" id="selSpecies" style="width: 240px">
                        <option value="0">All</option>
                        <% WriteSpecies(); %>
                    </select>
                </td>
                <td></td>
                
            </tr>
        </table>
    </div>
-->
    <br />

    <div id="tableDiv"></div>
    <div id="dialogsDiv"></div>

    <div id="container">
        <div id="filelist" class="panel"></div>
    </div>



    <div id='dlgSelectWebFolder' class='modal'>
        <div class='modal-dialog'>
            <div class='modal-content'>
                <div class='modal-header'>
                    <button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span></button>
                    <h4 class='modal-title'>Select Webfolder</h4>
                </div>
                <div class='modal-body'>
                    <strong>Folder URL:</strong><br />
                    <input name="usr" id="webFolderPath" type="text" class="form-control" />
                    <br />
                    <strong>Species:</strong><br />
                    <div class='input-group'>
                        <input type="hidden" id="webFolderSpeciesID" name="webFolderSpeciesID" />
                        <input type='text' class='form-control' placeholder='Search' id='webFolderSpecies' name='find' />
                        <div class='input-group-btn webFolderSpeciesSB'>
                            <button class='btn btn-default' type='submit'><i class='glyphicon glyphicon-search'></i></button>
                        </div>
                    </div>
                </div>

                <div class='modal-footer'>
                    <button type='button' class='btn btn-default' id="btnWebFolderOK" style="width: 80px">Ok</button>
                    <button type='button' class='btn' data-dismiss='modal' style="width: 80px">Cancel</button>
                </div>
            </div>
        </div>
    </div>


</asp:Content>
