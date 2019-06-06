<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="catalouge.aspx.cs" Inherits="species.Catalouge" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js"></script>
    <script>


        function publishCatalogue(id) {
            window.open('publisher.aspx?id=' + id, '_blank');
            /*
            ajaxindicatorstart('Working');
            $("#catDiv").load('publisher.aspx?id=' + id, function () {

                $('#dlgShowCatalog').modal();
                ajaxindicatorstop();
            });
            */
        }

        function initPublish() {
            $('.publishButton').off();
            $('.publishButton').click(function () {
                var id = this.id.substr(3);
                publishCatalogue(id);
            });



        }

        $(function () {
            global.reloadFunction = initPublish;
        });


    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="tableDiv"></div>
    <div id="dialogsDiv"></div>

    <!-- select species dialog -->
    <div id='dlgShowCatalog' class='modal'>
        <div class='modal-dialog'>
            <div class='modal-content'>
                <div class='modal-header'>
                    <button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span></button>
                    <h4 class='modal-title'>Catalogue</h4>
                </div>
                <div class='modal-body' id="catDiv">
                </div>

                <div class='modal-footer'>
                    <button type='button' class='btn btn-default' data-dismiss='modal'>Close</button>
                </div>
            </div>
        </div>
    </div>


</asp:Content>
