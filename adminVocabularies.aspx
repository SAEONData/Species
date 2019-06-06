<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="adminVocabularies.aspx.cs" Inherits="species.adminVocabularies" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="tableDiv"></div>
    <div id="dialogsDiv"></div>
</asp:Content>
