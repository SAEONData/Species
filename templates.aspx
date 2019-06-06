<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="templates.aspx.cs" Inherits="species.templates" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js"></script>
    <script src="plupload/js/moxie.min.js"></script>
    <script src="plupload/js/plupload.min.js"></script>

    <script>

        function initUploader(ctrl) {
            $('.addButton').off();


            var uploader = new plupload.Uploader({
                fileCount: 0,
                browse_button: ctrl,
                container: 'container',
                url: 'UploadHandler.ashx?folder=templates',
                filters: [
                    { title: "Word documents", extensions: "doc,docx" }
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
                    loadTable(initUploaders);
                }
            });
        }

        function initUploaders() {
            setTimeout("initUploader('btnAddItem1');", 500);
            setTimeout("initUploader('btnAddItem2');", 500);
        }

        $(function () {
            global.reloadFunction = initUploaders;
            //            initUploaders();
        });


    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    A template provides the layout for the catalogues.<br />
    <div id="tableDiv"></div>
    <div id="dialogsDiv"></div>

    <div id="container">
        <div id="filelist" class="panel"></div>
    </div>

</asp:Content>
