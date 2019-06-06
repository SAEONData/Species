<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="mediacap.aspx.cs" Inherits="species.mediacap" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form action="parsemedia.aspx" method="get">
    <div>
        Enter folder URL:<br />
        <input type="text" name="url" style="Width:1153px" value="http://media.dirisa.org/inventory/upload/egagasini/species/Invertebrate%20Trawl%20Species%20Database/trawl-surveys-final/africana-v270-jan-2011"></input>
        <br />
        <br />
        <input id="Submit1" type="submit" value="submit" /></div>
    </form>
</body>
</html>
