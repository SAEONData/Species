<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="stations.aspx.cs" Inherits="species.stations" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp"></script>
    <script type="text/javascript" src="wms.js"></script>
    <script>
        var dbType = '<%=list.type%>';
        var map = null;
        var markers = new Array();
        var tilerIndex = 0;
        var tilerMax = 4;
        var layerBathy = null;
        var layer200m = null;
        var layer500m = null;
        var editMap = null;
        


        function getLayer(name) {
            switch (name) {
                case 'bathy':
                    return layerBathy;
                case '200m':
                    return layer200m;
                case '500m':
                    return layer500m;
                default:
                    return null;
            }
        }

        function postEditFunc(id, code) {
            eval('data = ' + code);
            var lat = 0;
            var lng = 0;
            for (var i = 0; i < data.length; i++) {
                var item = data[i];
                if (item.name == "fStartLat")
                    lat = parseFloat(item.value);
                if (item.name == "fStartLng")
                    lng = parseFloat(item.value);
            }

            global.ll = new google.maps.LatLng(lat, lng);
            setTimeout(createEditMap, 200);
        }


        function createEditMap() {

            var ll = global.ll;


            var mapOptions = { zoom: 7, center: ll };
            var editMap = new google.maps.Map(document.getElementById('stationEditMap'), mapOptions);

            var marker = new google.maps.Marker({
                position: ll,
                draggable: true,
                map: editMap,
            });

            google.maps.event.addListener(marker, 'dragend', function (event) {
                $('#effStartLat').val(event.latLng.lat());
                $('#effStartLng').val(event.latLng.lng());
            });


        }




        function initialize() {
            var mapOptions = {
                zoom: 5,
                center: new google.maps.LatLng(-28.5, 25)
            };
            map = new google.maps.Map(document.getElementById('map-canvas'), mapOptions);

            layerBathy = DoAddShapeFileLayers(6498, 255, 1);
            layer200m = DoAddShapeFileLayers(6499, 255, 1);
            layer500m = DoAddShapeFileLayers(6500, 255, 1);



        }

        function clearMarkers() {
            for (var i = 0; i < markers.length; i++) {
                var marker = markers[i];
                marker.setMap(null);
            }
            markers = new Array();
        }

        function findMarker(id) {
            for (var i = 0; i < markers.length; i++) {
                var marker = markers[i];
                if (marker.id == id)
                    return marker;
            }
            return null;
        }

        function addMarker(id, name, lat, lon) {
            var ll = new google.maps.LatLng(lat, lon);

            var marker = new google.maps.Marker({
                position: ll,
                draggable: false,
                map: map,
                id: id,
                title: name
            });

            google.maps.event.addListener(marker, 'click', function () {
                alert(marker.title);
            });

            google.maps.event.addListener(marker, 'dragend', function (event) {
                var id = marker.id;
                var lat = event.latLng.lat();
                var lng = event.latLng.lng();
                var url = 'ajaxSpecies.ashx?mode=movestation&id=' + id + '&lat=' + lat + '&lon=' + lng;
                $.getJSON(url, null, reloadTable);
            });

            markers.push(marker);
        }

        function reloadTable() {
            loadTable(null, null);
        }

        function initMarkers(code) {
            clearMarkers();
            for (var i = 0; i < code.length; i++) {
                var station = code[i];
                addMarker(station.id, station.name, station.lat, station.lon);
            }
        }

        function initMap() {
            if (map == null) {
                setTimeout("initMap();", 200);
                return;
            }

            var url = 'ajaxSpecies.ashx?mode=loadstations';
            $.getJSON(url, null, initMarkers)
                .fail(function () {
                    alert('failed to load stations')
                });

            $(".viewButton").off();
            $(".viewButton").click(function (event) {
                event.stopPropagation();
                var id = getRowID(this.id);
                var marker = findMarker(id);

                map.setZoom(17);
                map.panTo(marker.position);

            });


            $('.btnShowAll').off();
            $('.btnShowAll').click(function () {
                map.setCenter(new google.maps.LatLng(-28.5, 25));
                map.setZoom(5);
            });



        }

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



        $(document).ready(function () {
            global.reloadFunction = initMap;
            global.postEdit = postEditFunc;

            $('.layercb').change(function () {
                var layer = getLayer(this.name);
                if (layer.getOpacity() == 1)
                    layer.setOpacity(0);
                else
                    layer.setOpacity(1);
            });

            initialize();
        });

    </script>

    <script src="Scripts/generic.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table style="width: 100%">
        <tr>
            <td style="width: 55%; vertical-align: top">
                <div id="tableDiv"></div>
            </td>

            <td style="width: 8px">&nbsp;</td>

            <td style="width: 45%; vertical-align: top">
                <label style="cursor: pointer; font-weight: 300">
                    <input type="checkbox" name="bathy" class="layercb" checked value="value" id="cbUntagged" />
                    Bathy Lines</label>&nbsp;&nbsp;
                <label style="cursor: pointer; font-weight: 300">
                    <input type="checkbox" name="200m" class="layercb" checked value="value" id="Checkbox1" />
                    200 m</label>&nbsp;&nbsp;
                <label style="cursor: pointer; font-weight: 300">
                    <input type="checkbox" name="500m" class="layercb" checked value="value" id="Checkbox2" />
                    500 m</label>&nbsp;&nbsp;
                <div id="map-canvas" style="width: 100%; height: 500px; border: 1px solid #f0f0f0;"></div>
            </td>
        </tr>

    </table>
    <div id="dialogsDiv">


    </div>

    <div class="modal">
        <div id="editDialogTblStation" class="modal">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
                        <h4 class="modal-title" id="editDialogTitleTblStation">Edit Station 2</h4>
                    </div>
                    <div class="modal-body">
                        <strong>Name</strong><br>
                        <input class="form-control" type="text" id="effStationName"><br>
                        <strong>Latitude</strong><br>
                        <input class="form-control" type="text" id="effStartLat"><br>
                        <strong>Longitude</strong><br>
                        <input class="form-control" type="text" id="effStartLng"><br>
                        <div style="width: 100%; height: 250px; border: 1px solid purple" id="stationEditMap"></div>
                    </div>
                    <div class="modal-footer">
                        <button id="buttonSave" target="TblStation" type="button" class="btn btn-default">Save</button>
                        <button type="button" class="btn " data-dismiss="modal">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
    </div>
</asp:Content>
