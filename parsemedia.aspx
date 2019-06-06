<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="parsemedia.aspx.cs" Inherits="species.parsemedia" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="Scripts/jquery-2.1.3.js"></script>
</head>
<body>
    <script type="text/javascript">
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
        <div id="inputFile" style="padding: 3px; border: 1px solid #f0f0f0"><%=trawlLog%></div>
        <input type="button" value="Parse Spreadsheet" onclick="parseXLS('inputFile', 'trawl');" />
        <br />
        <br />

        <strong>Catch Log:</strong><br />
        <div id="inputCatch" style="padding: 3px; border: 1px solid #f0f0f0"><%=catchLog%></div>
        <input type="button" value="Parse Spreadsheet" onclick="parseXLS('inputCatch', 'catch');" />
        <br />
        <br />

        <strong>Media:</strong><br />
        <table>
            <tr>
                <td>
                    URL: 
                </td>
                <td>
                    <input type="text" style="width: 500px" id="url" value="<%=url%>" /><br />
                </td>
            </tr>
            <tr>
                <td>
                    Survey: 
                </td>
                <td>
                    <input type="text" style="width: 500px" id="survey" value="<%=survey%>" />
                </td>
            </tr>
            <tr style="height: 40px;"></tr>
            <tr>
                <td>
                    Sample File Name:
                </td>
                <td>
                    <%=sampleFileName%>
                </td>
            </tr>
            <tr>
                <td>
                    Survey Field: 
                </td>
                <td>
                    <select style="width: 500px" id="fSurvey" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    Trawl Field: 
                </td>
                <td>
                    <select style="width: 500px" id="fTrawl" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    Depth Field: 
                </td>
                <td>
                    <select style="width: 500px" id="fDepth" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    Species Field: 
                </td>
                <td>
                    <select style="width: 500px" id="fSpecies" >
                    <% WritePartOptions(); %>
                    </select>
                </td>
            </tr>
        </table>
        <input type="button" value="Parse Media" onclick="parseMedia();" />


    
    </div>
</body>
</html>
