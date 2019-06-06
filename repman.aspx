<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="repman.aspx.cs" Inherits="BLA.repman" %>
<% if (Request["type"] != "CSV" ) { %>
<html>
<head>
    <title>Charts</title>
    <link href="Content/bootstrap.css" rel="stylesheet" />
    <script src="Scripts/jquery-2.1.3.js"></script>
    <script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>

    <style>
        td {
            font-size: 14px;
            padding: 2px;
        }
    </style>
</head>
<body>

    <% if ((Request["species"] == null || Request["species"] == "") && (Request["items"] == null || Request["items"] == ""))
       { %>
        No species selected.
    <% } else { %>

        <% if (Request["type"] == "table") { %>

        <table class="table">
            <tr>
                <td>Date</td>
                <td>Time</td>
                <td>Common Name</td>
                <td>Scientific Name</td>
                <td>Flying</td>
                <td>Sitting</td>
                <td>Vessel</td>
                <td>Cruise</td>
                <td>Observers</td>
            </tr>
            <% WriteTable(); %>
        </table>

        <% } else { %>

        <script type="text/javascript">

            var mydata = <% WriteReportData(); %>;

            google.charts.load('current', { 'packages': ['annotationchart'] });
            google.charts.setOnLoadCallback(drawChart);


            function updateContainer() {
                var cy = $(window).height();
                $('#chart_div').height(cy - 20);
            }

            function drawChart() {


                updateContainer();
                $(window).resize(function() {
                    updateContainer();
                });



                var data = new google.visualization.DataTable();
                data.addColumn('date', 'Date');
                data.addColumn('number', 'Flying');
                data.addColumn('number', 'Sitting');

                for (var i=0; i<mydata.length; i++) {
                    var item = mydata[i];
                    var date = item.date.split('|');
                    var y = parseInt(date[0]);
                    var m = parseInt(date[1]);
                    var d = parseInt(date[2]);
                    var row = [];
                    row.push(new Date(y, m, d));
                    row.push(item.flying);
                    row.push(item.sitting);
                    data.addRows([ row ]);
                }

                var chart = new google.visualization.AnnotationChart(document.getElementById('chart_div'));

                var options = {
                    displayAnnotations: true
                };

                chart.draw(data, options);
            }
        </script>


        <div id="chart_div" style="width: 95%; height: 500px;"></div>
        <% }  %>

    <% }  %>
    
</body>
</html>

<% } else {

       WriteCSVFile();       
       
}  %>