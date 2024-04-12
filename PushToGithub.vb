Option Explicit

Sub SaveVbaScriptToGitHub()
    
    'Declare our variables related to our URL
    Dim base_url As String
    Dim username As String
    Dim repo_name As String
    Dim file_name As String
    Dim access_token As String
    Dim payload As String
    Dim full_url As String
    
    'Define our URL Components.
    
    base_url = "https://api.github.com/repos/"
    repo_name = "MacroToGithub/"
    username = "sknasirhussain/"
    access_token = "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"
    
    'Declare variables related to the HTTP Request.
    Dim xml_obj As MSXML2.XMLHTTP60
    
    'Declare variables related to the Visual Basic Editor
    Dim VBAEditor As VBIDE.VBE
    Dim VBProj As VBIDE.VBProject
    Dim VBComp As VBIDE.VBComponent
    Dim VBCodeMod As VBIDE.CodeModule
    Dim VBRawCode As String
    Dim RawCodeEncoded As String
    
    'Create a reference to the VB Editor, TURN OFF MACRO SECURITY!!!
    Set VBAEditor = Application.VBE
    
    For Each VBProj In VBAEditor.VBProjects
    
        If VBProj.Name <> "Add_Ins_Sheet" Then
        
            'Iterate through each VBComponent in the project
            For Each VBComp In VBProj.VBComponents
        
            
                'Reference a single component in our Project and then grab the code module.
                Set VBCodeMod = VBComp.CodeModule
            
                'Grab the raw code in the code module
                VBRawCode = VBCodeMod.Lines(StartLine:=1, Count:=VBCodeMod.CountOfLines)
            
                'Debug.Print VBRawCode
            
                'Base64 Encode the string
                RawCodeEncoded = EncodeBase64(text:=VBRawCode)
            
                'Print out the code
                'Debug.Print "Here is the encoded content: " + RawCodeEncoded
            
                'Define our XML HTTP Object
                Set xml_obj = New MSXML2.XMLHTTP60
                
                file_name = VBComp.Name & ".vb"
                
                'access_token = "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"
                
                'Build the Full Url
                full_url = base_url + username + repo_name + "contents/" + file_name
                
                'full_url = base_url + username + repo_name + "contents/" + file_name + "?ref=master"
                
                xml_obj.Open bstrMethod:="GET", bstrUrl:=full_url, varAsync:=True
                
                'Set the headers
                xml_obj.setRequestHeader "Authorization", "token " + access_token
                
                xml_obj.send
                
                While xml_obj.readyState <> 4
                    DoEvents
                Wend
                
                'Debug.Print "RESPONSE: " + xml_obj.responseText
                'Debug.Print "RESPONSE: " + xml_obj.responseBody
                'Debug.Print "RESPONSE2: " + xml_obj.statusText
                Dim statusText As String
                statusText = CStr(xml_obj.Status)
                
                If statusText = "200" Then 'if there is same file present
                    'Update get the sha and update existing file
                    'yet to implement
                    xml_obj.abort
                    'Set xml_obj = Nothing
                    Debug.Print "This is 200"
                    Dim sha As String
                    sha = GetFileSHA(full_url:=full_url, access_token:=access_token)   ' Function to retrieve SHA hash of the existing file
                    'Debug.Print "SHA: " + sha
                    'Define the payload
                    payload = "{""message"": ""Updating Module"", ""content"":"""
                    payload = payload + Application.Clean(RawCodeEncoded)
                    payload = payload & """, ""sha"": """ & sha & """}"
                    
                    'build the full url
                    full_url = full_url + "?ref=master"
                    
                    'Open a new request
                    xml_obj.Open bstrMethod:="PUT", bstrUrl:=full_url, varAsync:=True
                
                    'Set the headers
                    xml_obj.setRequestHeader "Accept", "application/vnd.github.v3+json"
                    xml_obj.setRequestHeader "Authorization", "token " + access_token
                    
                    'Send the request.
                    xml_obj.send varBody:=payload
            
                    'Wait till it is finished.
                    While xml_obj.readyState <> 4
                        DoEvents
                    Wend
            
                    Debug.Print "RESPONSE: " + xml_obj.responseText
                    'Print out some info
                    Debug.Print "FULL URL: " + full_url
                    'Debug.Print "STATUS TEXT: " + xml_obj.statusText
                    Debug.Print "PAYLOAD: " + payload
                    
                ElseIf statusText = "404" Then 'if there is no such file
                    'send a new request to put content
                    Debug.Print "This is 404"
                    xml_obj.abort
                    'Set xml_obj = Nothing
                    full_url = full_url + "?ref=master"
                    
                    'Open a new request
                    xml_obj.Open bstrMethod:="PUT", bstrUrl:=full_url, varAsync:=True
        
                    'Set the headers
                    xml_obj.setRequestHeader "Accept", "application/vnd.github.v3+json"
                    xml_obj.setRequestHeader "Authorization", "token " + access_token
        
                    'Define the payload
                    payload = "{""message"": ""Adding New Module"", ""content"":"""
                    payload = payload + Application.Clean(RawCodeEncoded)
                    payload = payload + """}"
                    
        
                    'Send the request.
                    xml_obj.send varBody:=payload
        
                    'Wait till it is finished.
                    While xml_obj.readyState <> 4
                      DoEvents
                    Wend
        
                    Debug.Print "RESPONSE: " + xml_obj.responseText
                    'Print out some info
                    Debug.Print "FULL URL: " + full_url
                    'Debug.Print "STATUS TEXT: " + xml_obj.statusText
                    Debug.Print "PAYLOAD: " + payload
                
                Else
                    Debug.Print "Unexpected status code: " + statusText
                    Debug.Print "RESPONSE: " + xml_obj.responseText
                     
                    
                End If
                
                
                xml_obj.abort
                Set xml_obj = Nothing
                
                
        '        'Open a new request
        '        xml_obj.Open bstrMethod:="PUT", bstrUrl:=full_url, varAsync:=True
        '
        '        'Set the headers
        '        xml_obj.setRequestHeader "Accept", "application/vnd.github.v3+json"
        '        xml_obj.setRequestHeader "Authorization", "token " + access_token
        '
        '        'Define the payload
        ''        Dim sha As String
        ''        sha = "20c0dc355589c2fc8059bd74365f030d519c95ea" ' Function to retrieve SHA hash of the existing file
        ''        Debug.Print "SHA: " + sha
        '        payload = "{""message"": ""Update TestModule2.vb"", ""content"":"""
        '        payload = payload + Application.Clean(RawCodeEncoded)
        '        payload = payload + """}"
        '        'payload = payload & """, ""sha"": """ & sha & """}"
        '
        '        'Send the request.
        '        xml_obj.send varBody:=payload
        '
        '        'Wait till it is finished.
        '        While xml_obj.readyState <> 4
        '            DoEvents
        '        Wend
        '
        '        Debug.Print "RESPONSE: " + xml_obj.responseText
        '        'Print out some info
        '        Debug.Print "FULL URL: " + full_url
        '        'Debug.Print "STATUS TEXT: " + xml_obj.statusText
        '        Debug.Print "PAYLOAD: " + payload
            Next VBComp
        End If
    Next VBProj
    
End Sub

Function EncodeBase64(text As String) As String
    'Define our variables.
    Dim arrData() As Byte
    Dim objXML As MSXML2.DOMDocument60
    Dim objNode As MSXML2.IXMLDOMElement
    
    'Convert our string to a Unicode String
    arrData = StrConv(text, vbFromUnicode)
    
    'Define our Dom Objects.
    Set objXML = New MSXML2.DOMDocument60
    Set objNode = objXML.createElement("b64")
    
    'Define the data Type.
    objNode.DataType = "bin.base64"
    
    'Assign the node value.
    objNode.nodeTypedValue = arrData
    
    'Return the Encoded Text.
    EncodeBase64 = Replace(objNode.text, vbLf, "")
    
    'Memory Cleanup
    Set objNode = Nothing
    Set objXML = Nothing
    
End Function

Function GetFileSHA(full_url As String, access_token As String) As String
    Dim xml_obj As MSXML2.XMLHTTP60
    Dim responseText As String
    
    ' Create a new HTTP request object
    Set xml_obj = New MSXML2.XMLHTTP60
    
    ' Open a request to retrieve the existing file
    xml_obj.Open "GET", full_url, False
    xml_obj.setRequestHeader "Authorization", "token " & access_token ' Ensure you have access_token defined
    
    ' Send the request
    xml_obj.send
    
    ' Store the response text
    responseText = xml_obj.responseText
    
    ' Parse the response text to extract the SHA hash
    Dim jsonResponse As Object
    Set jsonResponse = JSONConverter.ParseJson(responseText)
    
    ' Return the SHA hash
    GetFileSHA = jsonResponse("sha")
End Function