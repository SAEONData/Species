<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="survey.aspx.cs" Inherits="species.survey" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js"></script>
    <script>
        function parseMediaFolder() {
            document.location = 'mediaimp.aspx';
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <button type="button" id="btnAddItem1" target="TblSurvey" class="btn btn-sm " onclick="parseMediaFolder();"><span class="glyphicon glyphicon-folder-open" aria-hidden="true"></span>&nbsp;&nbsp;Parse Media Folder</button>
    <br /><br />
    <div id="tableDiv"></div>
    <div id="dialogsDiv"></div>
</asp:Content>
