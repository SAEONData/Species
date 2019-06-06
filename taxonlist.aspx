<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="taxonlist.aspx.cs" Inherits="species.taxonlist" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Content/bootstrap.css" rel="stylesheet" />
    <link href="Content/bootstrap-theme.css" rel="stylesheet" />
    <link href="Content/datetimepicker.min.css" rel="stylesheet" />
    <link href="Content/bootstrap-modal.css" rel="stylesheet" />

    <script src="Scripts/jquery-2.1.3.js"></script>
    
    <script src="Scripts/bootstrap.min.js"></script>

    <script type="text/javascript">

        var global = {};
        global.id = '<%=Request["id"]%>';
        global.viewMode = '<%=Request["view"]%>';
        global.index = 10;
        global.taxons = [];
        

        function findTaxon(id) {
            for (var i = 0; i < global.taxons.length; i++) {
                if (global.taxons[i].id == id)
                    return global.taxons[i];
            }
            return null;
        }

        function reloadTriggers() {
            $('.glyphicon-remove').off();
            $('.glyphicon-remove').click(function () {
                var id = this.id.substr(2);
                taxon = findTaxon(id);
                taxon.deleted = true;
                var liid = '#li' + id;
                $(liid).remove();
            });

            if (global.viewMode == 'true') {
                $('.glyphicon').removeClass('glyphicon-remove');
            }
        }



        function addTaxonGo(id, name) {
            var taxon = { id: id, name: name, deleted: false };
            global.taxons.push(taxon);

            var code = "<li id='li" + id + "' role='presentation'><a class='taglabel' id='tn" + id + "'>" + name + "<i id='db" + id + "' class=' glyphicon glyphicon-remove pull-right' style='cursor: pointer'></i></a></li>";
            $('#pills').append(code);

            reloadTriggers();
        }

        function addTaxon() {
            if (parent.selectTaxonomy != null) {
                parent.selectTaxonomy(addTaxonGo);
            }
            else {
                var id = global.index;
                addTaxonGo(id, 'Phylum -> Antropoda: ' + id);
                global.index++;
            }
        }

        $(document).ready(function () {


            $("#btnAddItem1").click(addTaxon);

            var url = 'ajaxSpecies.ashx?mode=loadCatTAxon&id=' + global.id;
            jQuery.getJSON(url, null, initTaxons);


        });

        function getTaxons() {
            var list = '';
            for (var i = 0; i < global.taxons.length; i++) {
                var taxon = global.taxons[i];
                if (taxon.deleted == false) {
                    if (list != '')
                        list += ',';
                    list += taxon.id;
                }
            }
            return list;
        }

        function initTaxons(data) {
            for (var i = 0; i < data.length; i++) {
                var item = data[i];
                addTaxonGo(item.id, item.name);
            }
        }



    </script>




</head>
<body>
    <form id="form1" runat="server">
    <div>
        <% if (Request["view"] == "false") { %>
        <div id="" style="width:100%; height: 200px; border: 1px solid #f0f0f0; overflow-y: scroll ">
        <% } %>

            <ul id="pills" class="nav nav-pills nav-stacked">
            </ul>

        <% if (Request["view"] == "false") { %>
        </div>
        <br />
        <button type='button' id='btnAddItem1' class='btn btn-sm addButton'><span class='glyphicon glyphicon-plus' aria-hidden='true' /> Add Taxonomy</button>
        <% } %>
    
    </div>




    </form>
</body>
</html>
