<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="specie.aspx.cs" Inherits="species.specie" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <link href='Content/bootstrap.min.css' rel='stylesheet' />
    <link href='Content/bootstrap-theme.css' rel='stylesheet' />
    <link href='generic.css' rel='stylesheet' />
    <script type='text/javascript' src='scripts/jquery-2.1.1.min.js'></script>
    <script type='text/javascript' src='scripts/bootstrap.min.js'></script>
    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script type='text/javascript' src='generic.js'></script>
    <script type="text/javascript">
    </script>
</head>
<body>

    <form id="form1" runat="server">
    <div>
        <div id="tableDiv"></div>
        <div id="dialogsDiv"></div>
    </div>
    </form>



</body>
</html>
