<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="species._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var dbType = '<%=list.type%>';
    </script>
    <script src="Scripts/generic.js?v=2"></script>
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp"></script>

    <script type="text/javascript">
        
        var currentSpecies = 0;

        global.onLoadView = function (id) {
            currentSpecies = id;
            $('#speciesMediaTab').load('speciesmedia.aspx?id=' + id + '&mode=view&df=1', function () {
                $('#myTab a:first').tab('show') // Select first tab
                $('#speciesmap').attr('src', 'speciesmap.aspx?id=' + id);
            });
            if (map != null)
                loadSpeciesMap();
        }

        function loadTaxonBox(code) {
            $('#selTaxon').html('<option selected value=0>* All *</option>');
            for (var i = 0; i < code.length; i++) {
                var item = code[i];
                $('#selTaxon').append(new Option(item.name, item.id));
            }
            filterTaxon();
        }

        function loadTaxon() {
            var rank = $('#selRank').val();
            var url = 'ajaxSpecies.ashx?mode=loadTaxon&rank=' + rank;
            $.getJSON(url, null, loadTaxonBox);
        }

        function filterTaxon() {
            debugger;
            var taxon = $('#selTaxon').val();
            if (global.prevTaxon == null) {
                global.prevTaxon = 0;
            }
            if (global.prevTaxon != taxon) {
                global.prevTaxon = taxon;
                loadTable(null, taxon);
            }
        }


        $(document).ready(function () {
            $('#selRank').change(loadTaxon);
            $('#selTaxon').change(filterTaxon);

            $('a').on('shown', function (e) {
                console.log(e.target); // activated tab
                console.log(e.relatedTarget); // previous tab
            });
            loadTaxon();
        });


        var map = null;
        var markers = new Array();
        var tilerIndex = 0;
        var tilerMax = 4;
        var layerBathy = null;
        var layer200m = null;
        var layer500m = null;

        function createMap() {
            if (map == null) {
                setTimeout("initialize();", 300);
                
            }
        }

        function loadSpeciesMap() {
            var url = 'ajaxSpecies.ashx?mode=loadSpeciesstations&id=' + currentSpecies;
            $.getJSON(url, null, initMarkers)
                .fail(function () {
                    alert('failed to load stations')
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


        function initialize() {
            var mapOptions = {
                zoom: 5,
                center: new google.maps.LatLng(-28.5, 25)
            };
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);

            layerBathy = DoAddShapeFileLayers(6498, 255, 1);
            layer200m = DoAddShapeFileLayers(6499, 255, 1);
            layer500m = DoAddShapeFileLayers(6500, 255, 1);

            loadSpeciesMap();
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
                draggable: true,
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

        function initMarkers(code) {
            clearMarkers();
            for (var i = 0; i < code.length; i++) {
                var station = code[i];
                addMarker(station.id, station.name, station.lat, station.lon);
            }
        }

        $(document).ready(function () {
        });


    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="filterDiv">
        <table>
            <tr>
                <td style="width: 140px"><strong>Rank:</strong></td>
                <td style="width: 20px"></td>
                <td style="width: 140px"><strong>Filter:</strong></td>
                <td style="width: 20px"></td>
                <td style="width: 140px"><strong>Find:</strong></td>

            </tr>
            <tr>
                <td>
                    <select class="form-control" id="selRank" style="width: 140px">
                        <% WriteRanks(); %>
                    </select>
                </td>
                <td></td>
                <td>
                    <select class="form-control" id="selTaxon" style="width: 140px">
                    </select>
                </td>
                <td></td>
                <td>
                    <form>
                    <div class="input-group" style="width: 200px">
                       <input type="text" class="form-control" placeholder="Search" id="textFilter" name="textFilter" value="" />
                       <div class="input-group-btn textFilter">
                           <button class="btn btn-default" type="submit"><i class="glyphicon glyphicon-search"></i></button>
                       </div>
                    </div>
                    </form>
                </td>
            </tr>
        </table>
    </div>
    <br />
    <div id="tableDiv"></div>
    <div id="dialogsDiv"></div>

    <div id="viewDialogTblSpecies" class="modal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
                    <h4 class="modal-title">View Species</h4>
                </div>
                <div class="modal-body">
                    <ul id="myTab" class="nav nav-tabs" role="tablist">
                        <li role="presentation" class="active"><a href="#general" id="general-tab" role="tab" data-toggle="tab" aria-controls="general" aria-expanded="false">General</a></li>
                        <li role="presentation" class=""><a href="#view-references" role="tab" id="view-references-tab" data-toggle="tab" aria-controls="view-references" aria-expanded="false">More</a></li>
                        <li role="presentation" class=""><a href="#profile" role="tab" id="profile-tab" data-toggle="tab" aria-controls="profile" aria-expanded="true">Media</a></li>
                        <li role="presentation" class=""><a href="#map" onclick="createMap();" role="tab" id="map-tab" data-toggle="tab" aria-controls="map" aria-expanded="true">Map</a></li>
                    </ul>
                    <div id="myTabContent" class="tab-content">
                        <div role="tabpanel" class="tab-pane active fade in" id="general" aria-labelledby="general-tab">
                            <br />
                            <strong>Common Name</strong><br />
                            <div id="vffCommonName"></div>
                            <br />
                            <strong>Species Name</strong><br />
                            <div id="vffSpeciesName"></div>
                            <br />
                            <strong>Features</strong><br />
                            <div id="vffFeatures"></div>
                            <br />
                            <strong>Colour</strong><br />
                            <div id="vffColour"></div>
                            <br />
                            <strong>Size</strong><br />
                            <div id="vffSize"></div>
                            <br />
                            <strong>Distribution</strong><br />
                            <div id="vffDistribution"></div>
                            <br />
                        </div>
                        <div role="tabpanel" class="tab-pane fade" id="view-references" aria-labelledby="view-references-tab">
                            <br />
                            <strong>References</strong><br />
                            <div id="vffReferences"></div>
                            <br />
                            <strong>Similar</strong><br />
                            <div id="vffSimilar"></div>
                            <br />
                            <strong>Notes</strong><br />
                            <div id="vffNotes"></div>
                            <br />
                        </div>

                        <div role="tabpanel" class="tab-pane fade" id="profile" aria-labelledby="profile-tab">
                            <br />
                            <div id="speciesMediaTab" style="width: 100%; height: 410px; overflow-y: auto; border: 1px solid #f0f0f0; padding: 4px 4px 4px 4px">.</div>
                        </div>

                        <div role="tabpanel" class="tab-pane fade" id="map" aria-labelledby="map-tab" >
                            <br />
                            <div id="map_canvas" style="width: 100%; height: 410px; border: 1px solid #f0f0f0;" >.</div>
                        </div>

                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>
            </div>
            <!-- /.modal-content -->
        </div>
        <!-- /.modal-dialog -->
    </div>

</asp:Content>
