<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="mediaimp.aspx.cs" Inherits="species.mediaimp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <form action="parsemedia2.aspx" method="get">
    <div>
        Enter folder URL:<br />
        <input type="text" class="form-control" name="url" value="http://media.dirisa.org/inventory/upload/egagasini/species/Invertebrate%20Trawl%20Species%20Database/trawl-surveys-final/africana-v270-jan-2011"></input>
        <br />
        <input id="Submit1" class="btn btn-sm btn-default" type="submit" value="submit" /></div>
    </form>
</asp:Content>
