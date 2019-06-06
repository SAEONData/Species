<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="speciesmap.aspx.cs" Inherits="species.speciesmap" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script src="Scripts/jquery-2.1.3.js"></script>
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp"></script>
    <style>
html, body, #map_canvas {
    width: 100%;
    height: 100%;
    margin: 0;
    padding: 0;
}
#map_canvas {
    height: 400px;
}
    </style>
    <script>
        var map = null;
        var markers = new Array();
        var tilerIndex = 0;
        var tilerMax = 4;
        var layerBathy = null;
        var layer200m = null;
        var layer500m = null;



        function DoAddShapeFileLayers(layers, opacity, version) {

            function prepareTileUrl(coord, zoom, a, b, c) {

                var tiler = (tilerIndex % tilerMax) + 1;
                tilerIndex++;

                var url = 'http://mtiler' + tiler + '.mapable.co.za/render.aspx';

                var proj = map.getProjection();
                var zfactor = Math.pow(2, zoom);
                var top = proj.fromPointToLatLng(new google.maps.Point(coord.x * 256 / zfactor, coord.y * 256 / zfactor));
                var bot = proj.fromPointToLatLng(new google.maps.Point((coord.x + 1) * 256 / zfactor, (coord.y + 1) * 256 / zfactor));
                var final = url + "?zoom=" + map.getZoom() + "&ver=1&map=" + this.mapfile + "&version=" + version + "&x1=" + top.lng().toFixed(6) + "&y2=" + top.lat().toFixed(6) + "&x2=" + bot.lng().toFixed(6) + "&y1=" + bot.lat().toFixed(6);
                return final;
            }

            var mapMPType = new google.maps.ImageMapType({
                getTileUrl: prepareTileUrl,
                tileSize: new google.maps.Size(256, 256),
                isPng: true,
                maxZoom: 18,
                mapfile: "&LAYER=" + layers,
                opacity: parseFloat(opacity) / 255.0,
                mapver: 1,
                name: 142,
                alt: ''
            });

            map.overlayMapTypes.insertAt(map.overlayMapTypes.length, mapMPType);
            return mapMPType;
        }


        function initialize() {
            var mapOptions = {
                zoom: 5,
                center: new google.maps.LatLng(-28.5, 25)
            };
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);

            layerBathy = DoAddShapeFileLayers(6498, 255, 1);
            layer200m = DoAddShapeFileLayers(6499, 255, 1);
            layer500m = DoAddShapeFileLayers(6500, 255, 1);

        }

        $(document).ready(function () {
            initialize();
        });

    </script>

</head>
<body>
    <form id="form1" runat="server">
        <div id="map_canvas" ></div>
    </form>
</body>
</html>
