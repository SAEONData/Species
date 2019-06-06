<%@ Page Title="" Language="C#" MasterPageFile="~/Species.Master" AutoEventWireup="true" CodeBehind="parsemedia2.aspx.cs" Inherits="species.parsemedia2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/javascript">

        function downloadFile(ctrlID) {
            var xls = $('#' + ctrlID).html();
            window.open(xls + "?__ac_name=SpeciesDB&__ac_password=SpeciesDB", "_blank");
        }

        function parseXLS(ctrlID, type) {
            var xls = $('#' + ctrlID).html();
            var url = "isheet.aspx?url=" + escape(xls) + '&type=' + type;
            window.open(url, "_blank");
        }

        function parseMedia() {
            var url = "trawl.aspx";
            url += "?survey=" + escape($('#survey').val());
            url += "&url=" + escape($('#url').val());
            url += "&fsurvey=" + $('#fSurvey').val();
            url += "&ftrawl=" + $('#fTrawl').val();
            url += "&fdepth=" + $('#fDepth').val();
            url += "&fspecies=" + $('#fSpecies').val();
            window.open(url, "_blank");
        }
    </script>
    <div>
    
        Media Folder Parser:<br />
        <br />
        
        <strong>Trawl Log:</strong><br />
        <div id="inputFile" class="form-control"><%=trawlLog%></div>
        <input type="button" class="btn btn-default btn-sm" value="Download" onclick="downloadFile('inputFile', 'trawl');" />
        &nbsp;&nbsp;
        <input type="button" class="btn btn-default btn-sm" value="Parse Spreadsheet" onclick="parseXLS('inputFile', 'trawl');" />
        <br />
        <br />

        <strong>Catch Log:</strong><br />
        <div id="inputCatch" class="form-control"><%=catchLog%></div>
        <input type="button" class="btn btn-default btn-sm" value="Download" onclick="downloadFile('inputCatch', 'catch');" />
        &nbsp;&nbsp;
        <input type="button" class="btn btn-default btn-sm" value="Parse Spreadsheet" onclick="parseXLS('inputCatch', 'catch');" />
        <br />
        <br />

        <strong>Media:</strong><br />
        <table>
            <tr>
                <td>
                    URL: 
                </td>
                <td>
                    <input type="text" class="form-control" style="width: 500px" id="url" value="<%=url%>" /><br />
                </td>
            </tr>
            

            <tr>
                <td>
                    Survey: 
                </td>
                <td>
                    <input type="text" class="form-control" style="width: 500px" id="survey" value="<%=survey%>" />
                </td>
            </tr>
            <tr style="height: 40px;"></tr>
            <tr>
                <td>
                    Sample File Name:&nbsp;
                </td>
                <td>
                    <%=sampleFileName%>
                </td>
            </tr>
            <tr style="height: 10px;"></tr>
            <tr>
                <td>
                    Survey Field: 
                </td>
                <td>
                    <select class="form-control" style="width: 500px" id="fSurvey" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
            <tr style="height: 10px;"></tr>
            <tr>
                <td>
                    Trawl Field: 
                </td>
                <td>
                    <select class="form-control" style="width: 500px" id="fTrawl" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
            <tr style="height: 10px;"></tr>

            <tr>
                <td>
                    Depth Field: 
                </td>
                <td>
                    <select class="form-control" style="width: 500px" id="fDepth" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
            <tr style="height: 10px;"></tr>

            <tr>
                <td>
                    Species Field: 
                </td>
                <td>
                    <select class="form-control" style="width: 500px" id="fSpecies" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
        </table>
        <input class="btn btn-sm btn-default" type="button" value="Parse Media" onclick="parseMedia();" />
 
    </div>
</asp:Content>
