<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="dragmap.aspx.cs" Inherits="BLA.dragmap" %>

<!DOCTYPE html>
<html>
<head>
<meta name="viewport" content="initial-scale=1.0, user-scalable=no" />
<meta name="apple-mobile-web-app-capable" content="yes" />
<style type="text/css">
  html { height: 100% }
  body { height: 100%; margin: 0px; padding: 0px }
  #map_canvas { height: 100% }
</style>
<script type="text/javascript"
    <script src="<%=species.global.getGoogleMapsAPISource() %>"></script>
</script>
<script type="text/javascript">
    var map = null;
    var rectangle = null;
    var mapInit = false;
    var mouseDown = 0;
    var boxDragged = false;




    function initialize() {
        if (mapInit == true)
            return;
        mapInit = true;

        var x1 = parseFloat('<%=Request["x1"]%>');
        var y1 = parseFloat('<%=Request["y1"]%>');
        var x2 = parseFloat('<%=Request["x2"]%>');
        var y2 = parseFloat('<%=Request["y2"]%>');
        var cx = parseFloat('<%=Request["cx"]%>');
        var cy = parseFloat('<%=Request["cy"]%>');
        var z = parseInt('<%=Request["z"]%>');


        var center = new google.maps.LatLng(cy, cx);

        var mapOptions = {
            zoom: z,
            center: center
        };
        map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);


        var bounds = new google.maps.LatLngBounds(
          new google.maps.LatLng(y1, x1),
          new google.maps.LatLng(y2, x2)
        );

        rectangle = new google.maps.Rectangle({
            bounds: bounds,
            editable: true,
            draggable: true
        });

        rectangle.addListener('bounds_changed', showNewRect);

        map.addListener('bounds_changed', updateBounds);

        rectangle.setMap(map);

        mouseDown = 0;


        document.body.onmousedown = function () {
            mouseDown = 1;
            boxDragged = false;

        }

        document.body.onmouseup = function () {
            mouseDown = 0;
            if (boxDragged == true) {
                showNewRect(true);
            }
        }

    }


    function showNewRect(event, a, b, c) {
        boxDragged = true;
        if (mouseDown == 0) {
            var ne = rectangle.getBounds().getNorthEast();
            var sw = rectangle.getBounds().getSouthWest();
            var x1 = sw.lng();
            var y1 = sw.lat();
            var x2 = ne.lng();
            var y2 = ne.lat();
            if (parent.setCoords != null)
                parent.setCoords(x1, y1, x2, y2, false);
        }
    }

    function updateBounds() {
        var cp = map.getCenter();
        var z = map.getZoom();
        if (parent.updateBounds)
            parent.updateBounds(cp.lng(), cp.lat(), z);
    }


</script>
</head>
<body onload="initialize();"> 
  <div id="map_canvas" style="width:100%; height:100%"></div>
</body>
</html>