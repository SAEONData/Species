<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="photobox.aspx.cs" Inherits="species.photobox" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="Content/bootstrap.css" rel="stylesheet" />
    <link href="Content/bootstrap-theme.css" rel="stylesheet" />
    <link href="Content/datetimepicker.min.css" rel="stylesheet" />
    <link href="Content/bootstrap-modal.css" rel="stylesheet" />
    <link href="Content/jquery.Jcrop.min.css" rel="stylesheet" />

    <script src="Scripts/jquery-2.1.3.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="Scripts/moment.js"></script>

    <script src="Scripts/jquery.Jcrop.js"></script>
    <script type="text/javascript">

        var global = {};
        global.species = '<%=Request["species"] %>';
        global.id = '<%=Request["id"]%>';
        global.tagCount = 0;
        global.tags = [];
        global.forcedSelect = false;
        global.selectedRectangle = null;

        function speciesTagSaved() {
            alert('Tag saved');
        }


        function onSelect(rect) {
            if (global.forcedSelect == true) {
                global.forcedSelect = false;
                return;
            }
            global.rect = rect;

            if (global.species != '') {
                var tag = {
                    rect: global.rect,
                    id: global.species,
                    deleted: false
                };
                global.tags.push(tag);
                saveTags(speciesTagSaved);
            }
            else {
                global.jcrop.release();

                if (parent.selectSpecies)
                    parent.selectSpecies("", onSepectedSelected);
                else
                    onSepectedSelected(8, 'tag' + global.tagCount);
            }
        }

        function onSepectedSelected(id, name) {
            var index = global.tagCount++;
            var tag = {
                name: name,
                rect: global.rect,
                id: id,
                deleted: false
            };
            global.tags.push(tag);

            addTagPill(tag, index);

            reloadTriggers();
        }

        function addTagPill(tag, index) {
            var code = "<li id='li" + index + "' role='presentation'><a class='taglabel' id='tn" + index + "'>" + tag.name + "<i id='db" + index + "' class=' glyphicon glyphicon-remove pull-right' style='cursor: pointer'></i></a></li>";
            $('#tags').append(code);
        }

        function reloadTriggers() {
            $('.taglabel').off();

            $('.taglabel').mouseover(function () {
                var id = parseInt(this.id.substr(2));
                var tag = global.tags[id];
                var r = tag.rect;
                var rect = [r.x, r.y, r.x2, r.y2];
                global.forcedSelect = true;
                global.jcrop.setSelect(rect);
            });

            $('.taglabel').mouseout(function () {
                global.jcrop.release();
            });

            $('.glyphicon-remove').off();
            $('.glyphicon-remove').click(function () {
                var id = this.id.substr(2);
                var tag = global.tags[id];
                tag.deleted = true;
                var liid = '#li' + id;
                $(liid).remove();
                global.jcrop.release();
            });


        }

        function onChange(a, b, c, d) {
            //            debugger;
        }


        jQuery(function ($) {

            var options = {
                boxWidth: 580,
                boxHeight: 413,
                onSelect: onSelect,
                onChange: onChange
            };

            $('#target').Jcrop(options, function () {
                global.jcrop = this;
                if (global.selectedRectangle != null) {
                    global.forcedSelect = true;
                    global.jcrop.setSelect(global.selectedRectangle);
                }
            });

            loadTags(initTags);
        });

        function loadTags(rf) {
            var url = 'ajaxSpecies.ashx?mode=loadTags&id=' + global.id;
            jQuery.getJSON(url, null, rf);
        }

        function initTags(tags) {
            var lastTag = null;

            global.tagCount = 0;
            global.tags = tags;
            for (var i = 0; i < global.tags.length; i++) {
                lastTag = global.tags[i];
                addTagPill(lastTag, i);
                global.tagCount++;
            }
            reloadTriggers();

            if (global.species != '' && lastTag != null) {
                var tag = lastTag;
                var r = tag.rect;
                var rect = [r.x, r.y, r.x2, r.y2];
                global.selectedRectangle = rect;
            }
        }

        function saveTags(rf) {
            var code = '';
            for (var i = 0; i < global.tags.length; i++) {
                var tag = global.tags[i];
                if (tag.deleted == false) {
                    if (code != '')
                        code += '|';
                    code += tag.id + ',';
                    code += parseInt(tag.rect.x) + ',';
                    code += parseInt(tag.rect.y) + ',';
                    code += parseInt(tag.rect.x2) + ',';
                    code += parseInt(tag.rect.y2);
                }
            }

            var url = 'ajaxSpecies.ashx?mode=save&id=' + global.id + '&params=' + code;
            $.ajax(url, rf);
        }

    </script>

    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <% if (Request["species"] == null || Request["species"] == "" ) { %>
        Drag an area on the photo to add a species.
        <% } %>

        <div>
            <img id="target" src="ic.aspx?url=<%=Server.UrlEncode(Request["photo"])%>" />
        </div>

        <% if (Request["species"] == null || Request["species"] == "" ) { %>
            <ul id="tags" class="nav nav-pills nav-stacked">
            </ul>
        <% } %>

        <!--        
        <a href="#" onclick="saveTags(null); return false;">Save</a>
        -->

    </form>
</body>
</html>
