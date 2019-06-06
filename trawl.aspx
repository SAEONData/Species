<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="trawl.aspx.cs" Inherits="species.trawl" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body style="background-color: #f8f8ff">
    <form id="form1" runat="server">
    <div style="font-family: Arial; font-size: 14px">
        <a href="#slist">Resolve Species</a>
        <br />
        <br />
        <% Trawl(); %>
        <br />
        <br />
        <a name="slist"><strong>List of species found:</strong></a><br />
        <table border="1">
            <tr>
                <td>Name</td>
                <td>Search</td>
                <td>LSID</td>
            </tr>
        <% WriteSpecies(); %>
        </table>
        <br />
        <br />
        <% WriteSpeciesText();%>
    
    </div>
    </form>
</body>
</html>
