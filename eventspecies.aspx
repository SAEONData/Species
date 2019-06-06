<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eventspecies.aspx.cs" Inherits="species.eventMedia" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Content/bootstrap.css" rel="stylesheet" />
    <link href="Content/bootstrap-theme.css" rel="stylesheet" />
    <link href="Content/datetimepicker.min.css" rel="stylesheet" />
    <link href="Content/bootstrap-modal.css" rel="stylesheet" />
    <link href="Content/generic.css" rel="stylesheet" />
    <script src="Scripts/jquery-2.1.3.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="Scripts/moment.js"></script>
    <script src="Scripts/dateformat.js"></script>
    <script src="Scripts/datetimepicker.js"></script>

    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js"></script>


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
