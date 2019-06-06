<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="mapframe.aspx.cs" Inherits="species.mapframe" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp"></script>
    <script>
        var map;

        function initialize() {
            var mapOptions = {
                zoom: 8,
                center: new google.maps.LatLng(-34.397, 150.644)
            };
            map = new google.maps.Map(document.getElementById('map-canvas'),
                mapOptions);
        }

        $(document).ready(function () {
            alert('init map');
            initialize();
        });

    </script>

</head>
<body>
    <form id="form1" runat="server">
        <div id="map-canvas"></div>
    </form>
</body>
</html>
