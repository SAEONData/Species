Imports WormsReferenceExample.WormsReference
Public Class Form1

    Private Sub btnQueryTaxa_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnQueryTaxa.Click

        ' Use the following logic if you cannot rely on app.config settings. This typically occurs
        ' when accessing a web service from within a class library; any attempt to invoke the webservice 
        ' will result in an InvalidOperationException.
        '
        ' http://stackoverflow.com/questions/3703844/consume-a-soap-web-service-without-relying-on-the-app-config

        Dim binding = New ServiceModel.BasicHttpBinding()
        binding.Name = "AphiaNameServiceBinding"
        binding.CloseTimeout = TimeSpan.FromMinutes(1)
        binding.OpenTimeout = TimeSpan.FromMinutes(1)
        binding.ReceiveTimeout = TimeSpan.FromMinutes(10)
        binding.SendTimeout = TimeSpan.FromMinutes(1)
        binding.AllowCookies = False
        binding.BypassProxyOnLocal = False
        binding.HostNameComparisonMode = ServiceModel.HostNameComparisonMode.StrongWildcard
        binding.MaxBufferSize = 65536
        binding.MaxBufferPoolSize = 524288
        binding.MessageEncoding = ServiceModel.WSMessageEncoding.Text
        binding.TextEncoding = System.Text.Encoding.UTF8
        binding.TransferMode = ServiceModel.TransferMode.Buffered
        binding.UseDefaultWebProxy = True

        binding.ReaderQuotas.MaxDepth = 32
        binding.ReaderQuotas.MaxStringContentLength = 8192
        binding.ReaderQuotas.MaxArrayLength = 16384
        binding.ReaderQuotas.MaxBytesPerRead = 4096
        binding.ReaderQuotas.MaxNameTableCharCount = 16384

        binding.Security.Mode = ServiceModel.BasicHttpSecurityMode.None
        binding.Security.Transport.ClientCredentialType = ServiceModel.HttpClientCredentialType.None
        binding.Security.Transport.ProxyCredentialType = ServiceModel.HttpProxyCredentialType.None
        binding.Security.Transport.Realm = ""
        binding.Security.Message.ClientCredentialType = ServiceModel.BasicHttpMessageCredentialType.UserName
        binding.Security.Message.AlgorithmSuite = ServiceModel.Security.SecurityAlgorithmSuite.Default

        Dim endpointStr = "http://www.marinespecies.org/aphia.php?p=soap"
        Dim endpoint = New ServiceModel.EndpointAddress(endpointStr)
        Dim client As New AphiaNameServicePortTypeClient(binding, endpoint)

        'Declare dimensions
        Dim i As Integer, objRecords() As AphiaRecord, strUserInput As String
        'Get users input of a string
        strUserInput = Trim(InputBox("Please enter a taxa to Query", "WORMS Query"))
        'Use the users input to get the possible records from the WORMS website
        objRecords = client.getAphiaRecords(strUserInput, True, True, True, 1)
        'clear the listbox prior to filling it
        lstRecords.Items.Clear()
        'loop through retrieved records and 
        For i = 0 To objRecords.GetUpperBound(0)
            lstRecords.Items.Add(objRecords(i).scientificname)
        Next
        'inform the user that the query is complete
        MsgBox("Query Completed Successfully with " & objRecords.GetUpperBound(0) + 1 & "Records Retrieved", MsgBoxStyle.OkOnly, "WORMS Query")

    End Sub
End Class

