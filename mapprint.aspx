<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="mapprint.aspx.cs" Inherits="BLA.mapprint" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style>
        body {
            
        }

        #map {
            width: 100%;
            height: 700px;
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

        .newspaper {
            -webkit-column-count: 3; /* Chrome, Safari, Opera */
            -moz-column-count: 3; /* Firefox */
            column-count: 3;
        }

@media print {  
  @page {
    size: 342.8571429mm 514.2857143mm; /* landscape */
    /* you can also specify margins here: */
    margin: 20mm;
  }
}

    </style>



    <script src="Scripts/jquery-2.1.3.js"></script>
    <script src="<%=species.global.getGoogleMapsAPISource() %>"></script>

    <script>
        var global = {
            minClusterSize: 2,
            filter: ''
        };

        global.species = [];
        global.transects = [];


        var map = null;
        var markerCluster = null;
        var data = [];


        var features = [];


        var markers = [];

            
        var initCount = 0;


        function initialize() {


            var center = new google.maps.LatLng(-2, 24);

            map = new google.maps.Map(document.getElementById('map'), {
                zoom: 2,
                center: center,
                mapTypeId: google.maps.MapTypeId.HYBRID
            });


                        
            // initMarkers(data);

            
            parent.initPrintMap(map);


        }
        google.maps.event.addDomListener(window, 'load', initialize);


        function zoomTo(lat, lng, z, t) {
            if (t != null)
                map.setMapTypeId(t);
            map.setCenter(new google.maps.LatLng(lat, lng));
            map.setZoom(z);

        }


        function doPrint() {
            window.print();
        }

        function initMarkers(data) {

            var species = [];

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

            for (var i = 0; i < data.length; i++) {
                var point = data[i];

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
                        url: 'renderbox.aspx?c=' + colors,
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

                    /*


                    var size = 0.025;


                    var path = [
                        { lat: point.y - size, lng: point.x - size },
                        { lat: point.y - size, lng: point.x + size },
                        { lat: point.y + size, lng: point.x + size },
                        { lat: point.y + size, lng: point.x - size },
                        { lat: point.y - size, lng: point.x - size },
                    ];

                    var line = new google.maps.Polygon({
                        title: point.name,
                        path: path,
                        geodesic: true,
                        fillColor:'#700000',
                        fillOpacity: 0.35,
                        strokeColor:  '#ff0000',
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
                    */
                }

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

            var vessel = -1;

            var trm = '';
            if (trm == 'cruise' || trm == 'vessel' || trm == 'transect') {
                $('#selectByMode').val(trm);
                $('#selectByMode').trigger('change');
            }

            if (vessel != -1) {
                $('#xtVessel').val(vessel);
                // selectListVessel(vesselLoadInit);
            }

            
            var species = '';
            if (species != '') {
                masterGlobal.saveFunc = speciesSelectFunc;
                $('#lbAvailable').val(species);
                moveListItems('>');
            }
        }

        function vesselLoadInit() {
            var trm = '';

            var cruise = parseInt('');
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

        }

        function showClusters(cluster) {
            global.minClusterSize = cluster ? 2 : 100000;
            initMarkers(global.data);
        }

        $(document).ready(function () {

        });

        function speciesSelectFunc(species) {
            global.species = species;
            reloadData();
        }

        function transectSelectFunc(transects) {
            global.transects = transects;
            reloadData();
        }

        function filterFunction(filter) {
            global.filter = filter;
            reloadData();

        }

        function reloadData() {
            var url = 'ajax.aspx?mode=loadspecies&species=' + global.species + '&trm=' + $('#selectByMode').val() + '&items=' + global.transects + global.filter;
            $.ajax(url).done(function (code) {
                var data = $.parseJSON(code);
                initMarkers(data);
            })

        }







    </script>


</head>
<body>
    <form id="form1" runat="server">

        <div id="map"></div>
        <br />
        <div class="panel-heading">Legend</div>
        <div id="legendPanel" class="newspaper"></div>

    </form>
</body>
</html>
