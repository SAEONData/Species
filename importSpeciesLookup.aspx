<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="importSpeciesLookup.aspx.cs" Inherits="species.importSpeciesLookup" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <form>
    Input Database<br />
    <input type="text" class="form-control" value="C:\Users\johan\Desktop\mydata.xls" name="spf" />
    <br />
    <br />
    <input type="submit" />
    </form>
    
   
</asp:Content>
