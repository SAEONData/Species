<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eventIndicators.aspx.cs" Inherits="species.eventIndicators" %>

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

    <script type="text/javascript">
        $(function () {
            $('#savebutton').click(function () {
                var ic = getIndicators();
                alert(ic);
            });

            
        });

        function getIndicators() {
            var text = '';
            for (var i = 0; i < indicators.length; i++) {
                var indicator = indicators[i];
                var val = $('#ff' + indicator.id).val();
                text += '&ff' + indicator.id + "=" + escape(val);
            }
            return text;
        }

    </script>


</head>
<body >
    <form id="form1" runat="server">
    <div>
        <% WriteForm(); %>
    </div>

    </form>
</body>
</html>
