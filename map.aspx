<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="map.aspx.cs" Inherits="species.map" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        body {
        }

        #map {
            width: 100%;
            height: 400px;
            border: 1px solid #f0f0f0;
            border-radius: 5px;
        }

        label {
            font-weight: normal;
            font-size: 14px;
        }

        .modal-dialog {
            width: 730px;
        }


    </style>

    <script src="<%=species.global.getGoogleMapsAPISource() %>"></script>
    
    <script src="Scripts/markerclusterer.js"></script>
    <script src="Scripts/StyledMarker.js"></script>

    <script type="text/javascript">
        var global = {
            minClusterSize: 2,
            filter: '',
            reportName: 'report.csv',
            seperator: ','
        };

        global.species = [];
        global.transects = [];
        global.stations = [];
        global.loggedin = <%=(loggedIn ? "true" : "false")%>;


        var map = null;
        var markerCluster = null;
        var data = <%=getDataPoints()%>

        var features = [];


        var markers = [];

        var initPrintCout = 0;



        function initPrintMap(pmap, lpanel) {
            initPrintCout++;
            if (initPrintCout % 2 == 0) {
                return;
            }

            var t = map.getMapTypeId();
            var c = map.getCenter();
            var lat = parseFloat(c.lat());
            var lng = parseFloat(c.lng());
            var z = map.getZoom();


            var frame = window.frames["viewPrintIFrame"];

            frame.initMarkers(global.data);

            frame.zoomTo(lat, lng, z, t);

            // frame.doPrint();
        }

        function printFrame() {
            debugger;
            var frame = window.frames["viewPrintIFrame"];
            frame.doPrint();
        }

        //Complete path to OpenLayers WMS layer
 
        function WMSGetTileUrl(tile, zoom) {
            var projection = map.getProjection();
            var zpow = Math.pow(2, zoom);
            var ul = new google.maps.Point(tile.x * 256.0 / zpow, (tile.y + 1) * 256.0 / zpow);
            var lr = new google.maps.Point((tile.x + 1) * 256.0 / zpow, (tile.y) * 256.0 / zpow);
            var ulw = projection.fromPointToLatLng(ul);
            var lrw = projection.fromPointToLatLng(lr);
            //The user will enter the address to the public WMS layer here.  The data must be in WGS84
            var baseURL = "http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?";
            var version = "1.3.0";
            var request = "GetMap";
            var format = "image%2Fpng"; //type of image returned  or image/jpeg
            //The layer ID.  Can be found when using the layers properties tool in ArcMap or from the WMS settings 
            var layers = "SPECIES:-100m";
            //projection to display. This is the projection of google map. Don't change unless you know what you are doing.  
            //Different from other WMS servers that the projection information is called by crs, instead of srs
            var crs = "EPSG:4326"; 
            //With the 1.3.0 version the coordinates are read in LatLon, as opposed to LonLat in previous versions
            var bbox = ulw.lat() + "," + ulw.lng() + "," + lrw.lat() + "," + lrw.lng();
            var service = "WMS";
            //the size of the tile, must be 256x256
            var width = "256";
            var height = "256";
            //Some WMS come with named styles.  The user can set to default.
            var styles = "";
            //Establish the baseURL.  Several elements, including &EXCEPTIONS=INIMAGE and &Service are unique to openLayers addresses.
            var url = baseURL + "Layers=" + layers + "&version=" + version + "&EXCEPTIONS=INIMAGE" + "&Service=" + service + "&request=" + request + "&Styles=" + styles + "&format=" + format + "&CRS=" + crs + "&BBOX=" + bbox + "&width=" + width + "&height=" + height + "&transparent=true";
            return url;
        }

        function createWMSLegend(server, layer) {
            return server + '?service=WMS&REQUEST=GetLegendGraphic&VERSION=1.1.1&LAYER=' + layer + '&FORMAT=image/png';
        }

        function createWMSLayer(server, layers) {
            //Define custom WMS tiled layer
            return new google.maps.ImageMapType({
                getTileUrl: function (coord, zoom) {
                    var proj = map.getProjection();
                    var zfactor = Math.pow(2, zoom);
                    // get Long Lat coordinates
                    var top = proj.fromPointToLatLng(new google.maps.Point(coord.x * 256 / zfactor, coord.y * 256 / zfactor));
                    var bot = proj.fromPointToLatLng(new google.maps.Point((coord.x + 1) * 256 / zfactor, (coord.y + 1) * 256 / zfactor));

                    //corrections for the slight shift of the SLP (mapserver)
                    var deltaX = 0.0013;
                    var deltaY = 0.00058;

                    //create the Bounding box string
                    var bbox =     (top.lng() + deltaX) + "," +
                                   (bot.lat() + deltaY) + "," +
                                   (bot.lng() + deltaX) + "," +
                                   (top.lat() + deltaY);

                    //base WMS URL
                    var url = server; //"http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?service=WMS";
                    url += "&REQUEST=GetMap"; //WMS operation
                    url += "&VERSION=1.1.1";  //WMS version  
                    url += "&LAYERS=" + layers; //"SPECIES:-100m"; //WMS layers
                    url += "&FORMAT=image/png" ; //WMS format
                    url += "&BGCOLOR=0xFFFFFF";  
                    url += "&TRANSPARENT=TRUE";
                    url += "&SRS=EPSG:4326";     //set WGS84 
                    url += "&BBOX=" + bbox;      // set bounding box
                    url += "&WIDTH=256";         //tile size in google
                    url += "&HEIGHT=256";
                    return url;                 // return URL for the tile

                },
                tileSize: new google.maps.Size(256, 256),
                isPng: true
            });



        }
            

        function initialize() {


            var center = new google.maps.LatLng(-30, 24);

            map = new google.maps.Map(document.getElementById('map'), {
                zoom: 5,
                center: center,
                minZoom: 3,
                mapTypeId: google.maps.MapTypeId.HYBRID
            });

            bathy100 = createWMSLayer('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?service=WMS', 'SPECIES:-100m');
            map.overlayMapTypes.push(bathy100);

            bathy200 = createWMSLayer('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?service=WMS', 'SPECIES:-200m');
            map.overlayMapTypes.push(bathy200);
 
            bathy500 = createWMSLayer('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?service=WMS', 'SPECIES:-500m');
            map.overlayMapTypes.push(bathy500);
        
            bathy1000 = createWMSLayer('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?service=WMS', 'SPECIES:-1000m');
            map.overlayMapTypes.push(bathy1000);


                        
            initMarkers(data);

            setTimeout("loadInitSpecies();", 500);
        }
        google.maps.event.addDomListener(window, 'load', initialize);

        function initMarkers(data) {


            var species = [];

            var showLines = $('#STL').is(':checked');


            global.data = data;
            for (var i=0; i<features.length; i++)
                features[i].setMap(null);
            if (markerCluster != null)
                markerCluster.removeMarkers(markers);
            markers = [];
            features = [];

            var viewType = $('#viewType').val();
            var viewCount = $('#cbViewCount').val();

            $('#cbClusterPoints').attr('disabled', viewType == 'area');

            var cluster = $('#cbClusterPoints').is(':checked');

            var minX = 1e100;
            var minY = 1e100;
            var maxX = -1e100;
            var maxY = -1e100;

            for (var i = 0; i < data.length; i++) {
                var point = data[i];
                minX = Math.min(minX, point.x);
                minY = Math.min(minY, point.y);
                maxX = Math.max(maxX, point.x);
                maxY = Math.max(maxY, point.y);

                if (viewType == 'point') {

                    var count = 1;
                    if (cluster == true) {
                        count = 0;
                        if (viewCount == 'all' || viewCount == 'flying')
                            count += point.flying;
                        if (viewCount == 'all' || viewCount == 'sitting')
                            count += point.sitting;
                    }



                    for (var c=0; c<count; c++) {

                        var latLng = new google.maps.LatLng(point.y, point.x);

                        /*
                        var image = {
                            url: 'https://chart.googleapis.com/chart?chst=d_map_pin_letter&chld=.|FF00FF|000000',
                            // This marker is 20 pixels wide by 32 pixels high.
                            size: new google.maps.Size(21, 34),
                            // The origin for this image is (0, 0).
                            origin: new google.maps.Point(0, 0),
                            // The anchor for this image is the base of the flagpole at (0, 32).
                            anchor: new google.maps.Point(0, 17)
                        };
                        **/

                        var marker = new google.maps.Marker({
                            title: point.name,
                            position: latLng
                            /*, icon: image */
                        });

                        if (cluster == true) {
                            markers.push(marker);
                        }
                        else {
                            marker.transect = point.transect;
                            marker.addListener('click', function () {
                                feature = this;
                                viewIFrameDialog('View Transect', 'trinfo.aspx?incmedia=true&id=' + feature.transect, 1000, 600);
                            });

                            features.push(marker);
                            marker.setMap(map);
                        }
                    }
                }
                else {

                    var latLng = new google.maps.LatLng(point.y, point.x);

                    if (showLines) {

                        var size = 0.025;

                        debugger;



                        var path = [
                            { lat: point.y1, lng: point.x1 },
                            { lat: point.y2, lng: point.x2 }
                        ];

                        var line = new google.maps.Polygon({
                            title: point.name,
                            path: path,
                            geodesic: true,
                            fillColor:'#' + Math.random().toString(16).slice(2, 8).toUpperCase(),
                            fillOpacity: 0.35,
                            strokeColor:  '#' + Math.random().toString(16).slice(2, 8).toUpperCase(),
                            strokeOpacity: 0.8,
                            strokeWeight: 2
                        });

                        line.transect = point.transect;


                        line.addListener('click', function () {
                            feature = this;
                            viewIFrameDialog('View Transect', 'trinfo.aspx?incmedia=true&id=' + feature.transect, 1000, 600);
                        });

                        line.setMap(map);

                        features.push(line);
                    }


                    var colors = '';
                    var title = '';

                    if (point.obs.length == 0) {
                        var sp = species[-1];
                        if (sp == null) {
                            species[-1] = {
                                title: 'None observed',
                                color: '',
                                sitting: 0,
                                flying: 1
                            }
                        }
                        else {
                            sp.flying++;
                        }
                    }

                    for (var o=0; o<point.obs.length; o++) {
                        if (colors != '') {
                            colors += ',';
                            title += "\n";
                        }

                        var obs = point.obs[o];

                        colors += obs.color.replace('#', '');
                        title += obs.sname + ': ' + (obs.sitting + obs.flying);

                        
                        
                        
                        var sp = species[obs.species];
                        if (sp == null) {
                            species[obs.species] = {
                                title: obs.sname,
                                color: obs.color,
                                sitting: obs.sitting,
                                flying: obs.flying
                            }
                        }
                        else {
                            sp.sitting += obs.sitting;
                            sp.flying += obs.flying;
                        }
                        
                    }


                    var image = {
                        url: showLines ? 'renderbox.aspx' : 'renderbox.aspx?c=' + colors + '&v=2',
                        size: new google.maps.Size(10, 10),
                        origin: new google.maps.Point(0, 0),
                        anchor: new google.maps.Point(5, 5)
                    };

                    var marker = new google.maps.Marker({
                        title: title,
                        position: latLng,
                        icon: image
                    });

                    if (cluster == true) {
                        markers.push(marker);
                    }
                    else {
                        marker.transect = point.transect;
                        marker.addListener('click', function () {
                            feature = this;
                            viewIFrameDialog('View Transect', 'trinfo.aspx?incmedia=true&id=' + feature.transect, 1000, 600);
                        });

                        features.push(marker);
                        marker.setMap(map);

                    }



                }

            }

            if (minX < 1e10) {
                var bounds = new google.maps.LatLngBounds();
                bounds.extend(new google.maps.LatLng(minY, minX));
                bounds.extend(new google.maps.LatLng(maxY, maxX));
                map.fitBounds(bounds);
                if (map.getZoom() > 10)
                    map.setZoom(10);

            }

            updateLegend(species);


            if (cluster == true)  {
            
                var options = { minimumClusterSize: 1 };
                markerCluster = new MarkerClusterer(map, markers, options);
            }
        }

        function updateLegend(species) {
            var code = '';
            for (s in species) {
                var spec = species[s];
                if (code != "")
                    code += '<br />';

                var cnt = spec.sitting + spec.flying;
                code += "<img width=10 height=10 src='renderbox.aspx?c=" + spec.color.replace('#', '') + "' />";

                if (s > 0)
                    code += '&nbsp;' + spec.title + ' (' + cnt + ')';
                else
                    code += '&nbsp;' + spec.title;
            }


            $('#legendPanel').html(code);
        }

        

        $(document).ready(function () {
            updateContainer();
            $(window).resize(function() {
                updateContainer();
            });


            

            



        });



        function selectTransects() {
            dlgSelectTransect(transectSelectFunc);
        }

        function selectStations() {
            dlgSelectStations(stationSelectFunc);
        }

        function doPrint() {
            $('#viewPrintIFrame').attr('src', 'mapprint.aspx');
            $('#dlgViewPrint').modal();

        }

        function toggleLegend() {
            var btn = $('#legendButton');
            if (btn.val() == 'Hide Legend') {
                $('#smcol2').hide();
                $('#smcol3').hide();
                btn.val('Show Legend');
            }
            else {
                $('#smcol2').show();
                $('#smcol3').show();
                btn.val('Hide Legend');
            }

            google.maps.event.trigger(map, "resize");
        }

        function loadInitSpecies() {

            var vessel = parseInt('<%=vesselID%>');

            var trm = '<%=Request["trm"]%>';
            if (trm == 'cruise' || trm == 'vessel' || trm == 'transect') {
                $('#selectByMode').val(trm);
                $('#selectByMode').trigger('change');
            }

            if (vessel != -1) {
                $('#xtVessel').val(vessel);
                selectListVessel(vesselLoadInit);
            }

            
            var species = '<%=Request["species"]%>';
            if (species != '') {
                masterGlobal.saveFunc = speciesSelectFunc;
                $('#lbAvailable').val(species);
                moveListItems('>');
            }
        }

        function vesselLoadInit() {
            var trm = '<%=Request["trm"]%>';

            var cruise = parseInt('<%=cruiseID%>');
            $('#xtCruise').val(cruise);

            if (trm == 'vessel') {
                masterGlobal.saveFunc = transectSelectFunc;
                moveListItemsXT('>');
            } 
            else if (trm == 'cruise') {
                masterGlobal.saveFunc = transectSelectFunc;
                moveListItemsXT('>');
            }
            else
                $('#xtCruise').trigger('change');
        }



        function updateContainer() {
            var cy = $(window).height();
            $('#map').height(Math.max(550, cy - 397));
            var os = 72;

            $('#legendPanel').height(Math.max(550 - os, cy - 397 - os));
            if (map != null)
                google.maps.event.trigger(map, "resize");

        }

        function showClusters(cluster) {
            global.minClusterSize = cluster ? 2 : 100000;
            initMarkers(global.data);
        }

        $(document).ready(function () {

            var p = $("[data-toggle=popover]").popover({
                html: true,
                content: function () {
                    setTimeout('initJS();', 500);
                    return $('#popover-content').html();
                }
            });
            /*
                        p.on("show.bs.popover", function (e) {
                            p.data("bs.popover").tip().css({ "max-width": "1000px" });
                        });
                        */

        });

        function speciesSelectFunc(species) {
            global.species = species;
            reloadData();
        }

        function transectSelectFunc(transects) {
            global.transects = transects;
            reloadData();
        }

        function stationSelectFunc(stations) {
            global.stations = stations;
            reloadData();
        }

        var myFilter = '';

        function filterFunction(filter) {
            global.filter = filter;
            myFilter = filter;
            reloadData();

        }

        function reloadData() {
            var url = 'ajax.aspx?mode=loadspecies&species=' + global.species + '&trm=' + $('#selectByMode').val() + '&items=' + global.transects + '&stations=' + global.stations + global.filter;
            $.ajax(url).done(function (code) {
                var data = $.parseJSON(code);
                initMarkers(data);
            })

        }

        function exportCSV() {
            /*
            if (global.loggedin == false) {
                error("Please sign in or register to be able to download data.");
                return;
            }
            */
            getItemName("Save Report", "Report Name", global.reportName, global.seperator, function (name, seperator) {
                var lcase = name.toLowerCase();
                if (lcase.indexOf('.csv') == -1)
                    name += '.csv';
                global.reportName = name;
                global.seperator = seperator;
                $('#saveframe').attr('src', "savecsv.aspx?url=" + escape('repman.aspx?species=' + global.species + '&trm=' + $('#selectByMode').val() + '&items=' + global.transects + myFilter + '&type=CSV&seperator=' + seperator) + "&name=" + escape(name));
            });
        }






    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">






    <% if (pageTitle != "") { %>
    <h4><%=pageTitle%></h4>
    <% } %>
            <input style="width: 120px" type="button" class="btn btn-sm btn-default" onclick="selectTransects();"  value="Select Survey" />
            &nbsp;
            <input style="width: 120px" type="button" class="btn btn-sm btn-default" onclick="selectStations();"  value="Select Stations" />
            &nbsp;
            <input style="width: 120px" type="button" class="btn btn-sm btn-default" onclick="showSpeciesSelect(speciesSelectFunc);"  value="Select Species" />
            &nbsp;
            <input style="width: 120px" type="button" class="btn btn-sm btn-default" onclick="filterDialog(filterFunction);"  value="Filters" />
            &nbsp;
            <input style="width: 120px" type="button" class="btn btn-sm btn-default" onclick="doPrint();"  value="Print" />
            &nbsp;
            <input style="width: 120px" type="button" class="btn btn-sm btn-default" onclick="exportCSV();"  value="Export to .CSV" />
            &nbsp;

            <input style="width: 120px" type="button" class="btn btn-sm btn-default" onclick="toggleLegend();" id="legendButton"  value="Hide Legend" />

            <% { %>
            &nbsp;
            <label><input type="checkbox" id="STL" onclick="initMarkers(global.data);" />Show Transect Lines</label>
            <% } %>


    <div class="hidden">
        <select class="form-control" id="cbViewCount" onchange="initMarkers(global.data);" style="display:none">
            <option value="all">View All</option>
            <option value="flying">View Flying</option>
            <option value="sitting">View Sitting</option>
        </select>
        <select class="form-control" onchange="initMarkers(global.data);" id="viewType">
            <option selected value="area">View Areas</option>
            <option value="point">View Points</option>
        </select>
        <label><input type="checkbox" id="cbClusterPoints" onclick="showClusters(this.checked);" /> Cluster Points</label>
    </div>
    <div style="height: 10px"></div>
    <div id="printerDiv">
    <table style="width: 100%">
        <tr>
            <td style="width: 70%; vertical-align: top" >
                <div id="map"></div>
            </td>
            <td id="smcol2">&nbsp;&nbsp;&nbsp;</td>
            <td id="smcol3" style="width: 30%; vertical-align: top">
                <div class="panel panel-default">
                  <div class="panel-heading">Legend (no. of records)</div>
                  <div class="panel-body">
                    <div id="legendPanel" style="overflow-y: auto;"></div>
                  </div>
                </div>
            </td>
        </tr>
    </table>
    <table>
        <tr>
            <td>
                <label style="cursor: pointer">
                    Bathy Lines:
                </label>
            </td>
            <td style="width: 20px"></td>

            <td>
                <label style="cursor: pointer">
                    <input type="checkbox" id="cb100" checked="checked" />
                    100m
                    <img id="bl100" width="16" alt="bathy lines 100m" src="" />
                </label>
            </td>
            <td style="width: 50px"></td>
            <td>
                <label style="cursor: pointer">
                    <input type="checkbox" id="cb200" checked="checked" />
                    200m
                    <img id="bl200" width="16" alt="bathy lines 200m" src="" />
                </label>
            </td>
            <td style="width: 50px"></td>
            <td>
                <label style="cursor: pointer">
                    <input type="checkbox" id="cb500" checked="checked" />
                    500m
                    <img id="bl500" width="16" alt="bathy lines 500m" src="" />
                </label>
            </td>
            <td style="width: 50px"></td>
            <td>
                <label style="cursor: pointer">
                    <input type="checkbox" id="cb1000" checked="checked" />
                    1000m
                    <img id="bl1000" width="16" alt="bathy lines 1000m" src="" />
                </label>
            </td>
        </tr>
    </table>

    <script type="text/javascript">

        $(document).ready(function () {
            $('#bl100').attr('src', createWMSLegend('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?', 'SPECIES:-100m'));
            $('#bl200').attr('src', createWMSLegend('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?', 'SPECIES:-200m'));
            $('#bl500').attr('src', createWMSLegend('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?', 'SPECIES:-500m'));
            $('#bl1000').attr('src', createWMSLegend('http://app01.saeon.ac.za:8087/geoserver/SPECIES/wms?', 'SPECIES:-1000m'));

            $('#cb100').click(function() { bathy100.setOpacity($('#cb100').is(':checked') ? 1.0 : 0.0); })
            $('#cb200').click(function() { bathy200.setOpacity($('#cb200').is(':checked') ? 1.0 : 0.0); })
            $('#cb500').click(function() { bathy500.setOpacity($('#cb500').is(':checked') ? 1.0 : 0.0); })
            $('#cb1000').click(function() { bathy1000.setOpacity($('#cb1000').is(':checked') ? 1.0 : 0.0); })
        });



     </script>

    </div>
    
        <!-- dialog view print-->
        <div id='dlgViewPrint' class='modal'>
            <div class='modal-dialog' style="width: 1100px">
                <div class='modal-content'>
                    <div class='modal-header'>
                        <button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span></button>
                        <h4 class='modal-title' id="viewPrintCaption">Select Species</h4>
                    </div>
                    <div class='modal-body'>
                        <iframe style="width: 100%; height: 600px; border: 0px solid white;" name="viewPrintIFrame" id="viewPrintIFrame"></iframe>
                    </div>
                    <div class='modal-footer'>
                        <button type='button' class='btn btn-sm btn-default' style="width: 80px" onclick="printFrame();">Print</button>
                        <button type='button' class='btn btn-sm btn-default' data-dismiss='modal' style="width: 80px">Close</button>
                    </div>
                </div>
            </div>
        </div>

    
    
</asp:Content>
