<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="speciesmedia.aspx.cs" Inherits="species.speciesmedia" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <% WriteMedia(); %>
    
    </div>
        <script type="text/javascript">
            function updateCat(id, checked) {
                var url = 'ajaxSpecies.ashx?mode=updateCatFlag&species=' + id + '&selected=' + checked;
                $.ajax(url)
            }

        </script>
    </form>
</body>
</html>
