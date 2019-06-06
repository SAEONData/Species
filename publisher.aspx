<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="publisher.aspx.cs" Inherits="species.publisher" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Click on the link below to download the catalogue:<br />
        <a href="<%=docURL%>"><%=docName%></a>
    </div>
    </form>
</body>
</html>
