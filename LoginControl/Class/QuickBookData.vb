Imports Interop.QBFC12
Imports System.Data.Linq
Imports System.Data.Linq.Mapping
Imports System.Linq

Module QuickBookData
    Dim booSessionBegun As Boolean
    Dim qbSessionManager As QBSessionManager
    Dim msgSetRequest As IMsgSetRequest
    Dim db As New DBTNCCHKDataContext(gblConnectionString)

    'OPen QB Connection
    Public Sub OpenConnectionBeginSession()

        booSessionBegun = False

        On Error GoTo Errs

        qbSessionManager = New QBSessionManager()


        Dim companyresult As ISingleResult(Of tblCompanySelectResult) = db.tblCompanySelect(Convert.ToInt16(gblCompanyID), Nothing, Nothing)
        Dim qbfile As String = ""
        Dim qbCompanyName As String = "C:\Users\Public\Documents\Intuit\QuickBooks\Sample Company Files\QuickBooks Enterprise Solutions 13.0\Test for Integration.qbw"
        For Each company As tblCompanySelectResult In companyresult
            qbfile = company.QBFileName
            qbCompanyName = company.QBCompanyName
        Next
        qbSessionManager.OpenConnection("124", qbCompanyName)
        ' Dim qbfile As String = "C:\Users\Public\Documents\Intuit\QuickBooks\Sample Company Files\QuickBooks Enterprise Solutions 11.0\sample_product-based business.qbw"
        If Not qbfile = "" Then
            qbSessionManager.BeginSession(qbfile, ENOpenMode.omDontCare)
        Else
            MessageBox.Show("Please setup company first", "TNC-CHECK MANAGEMENT SYSTEM")
            Exit Sub
        End If


        ' qbSessionManager.BeginSession("", ENOpenMode.omDontCare)
        booSessionBegun = True

        'Check to make sure the QuickBooks we're working with supports version 2 of the SDK
        Dim strXMLVersions() As String
        strXMLVersions = qbSessionManager.QBXMLVersionsForSession

        Dim booSupports2dot0 As Boolean
        booSupports2dot0 = False
        Dim i As Long
        For i = LBound(strXMLVersions) To UBound(strXMLVersions)
            If (strXMLVersions(i) = "2.0") Then
                booSupports2dot0 = True
                msgSetRequest = qbSessionManager.CreateMsgSetRequest("US", 8, 0)
                Exit For
            End If
        Next

        If Not booSupports2dot0 Then
            MsgBox("This program only runs against QuickBooks installations which support the 2.0 qbXML spec.  Your version of QuickBooks does not support qbXML 2.0")
            End
        End If
        Exit Sub

Errs:
        If Err.Number = &H80040416 Then
            MsgBox("You must have QuickBooks running with the company" & vbCrLf & "file open to use this program.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040422 Then
            MsgBox("This QuickBooks company file is open in single user mode and" & vbCrLf & "another application is already accessing it.  Please exit the" & vbCrLf & "other application and run this application again.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040401 Then
            MsgBox("Could not access QuickBooks (Failure in attempt to connection)")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040403 Then
            MsgBox("Could not open the specified QuickBooks company data file")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040407 Then
            MsgBox("The installation of QuickBooks appears to be incomplete." & vbCrLf & "Please reinstall QuickBooks.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040408 Then
            MsgBox("Could not start QuickBooks." & vbCrLf & "Try Again")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H8004040A Then
            MsgBox("QuickBooks company data file is already open and it " & vbCrLf & "is different from the one requested.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H8004041A Then
            MsgBox("This application does not have permission to access this QuickBooks company data file" & vbCrLf & "Grant access permission through the Integrated Application preferences.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040420 Then
            MsgBox("The QuickBooks user has denied access.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040424 Then
            MsgBox("QuickBooks did not finish its initialization." & vbCrLf & "Please try again later.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End
        ElseIf Err.Number = &H80040427 Then
            MsgBox("Your QuickBooks application needs to be registered.")
            qbSessionManager.CloseConnection()
            Exit Sub
            End

        Else
            MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, MsgBoxStyle.Critical, "Error in OpenConnectionBeginSession")

            If booSessionBegun Then
                qbSessionManager.EndSession()
            End If

            qbSessionManager.CloseConnection()
            Exit Sub
            End
        End If
    End Sub

    'End QB Connection
    Public Sub EndSessionCloseConnection()
        On Error GoTo Errs

        If booSessionBegun Then
            qbSessionManager.EndSession()
            qbSessionManager.CloseConnection()
        End If

        Exit Sub
Errs:
        MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description,
                    MsgBoxStyle.Critical,
                    "Error in EndSessionCloseConnection")
    End Sub

#Region 'Fill vendor data In database
    Public Sub getVendor(ByRef bError As Boolean)
        On Error GoTo Errs

        ' make sure we do not have any old requests still defined
        msgSetRequest.ClearRequests()

        ' set the OnError attribute to continueOnError
        msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue

        ' Add the CustomerQuery request

        Dim vendortQuery As IVendorQuery
        vendortQuery = msgSetRequest.AppendVendorQueryRq
        vendortQuery.ORVendorListQuery.VendorListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly)
        vendortQuery.ORVendorListQuery.VendorListFilter.Type.GetValue()
        Dim bDone As Boolean = False
        Do While (Not bDone)
            ' send the request to QB
            Dim msgSetResponse As IMsgSetResponse
            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest)

            FillVendorInDatabase(msgSetResponse, bDone, bError)

        Loop
        Exit Sub


Errs:
        MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, MsgBoxStyle.Critical, "Error in GetCustomers")
        bError = True
    End Sub

    Public Sub FillVendorInDatabase(ByRef msgSetResponse As IMsgSetResponse, ByRef bDone As Boolean, ByRef bError As Boolean)
        On Error GoTo Errs

        ' check to make sure we have objects to access first
        ' and that there are responses in the list
        If (msgSetResponse Is Nothing) Or
            (msgSetResponse.ResponseList Is Nothing) Or
            (msgSetResponse.ResponseList.Count <= 0) Then
            bDone = True
            bError = True
            Exit Sub
        End If
        ' Start parsing the response list
        Dim responseList As IResponseList
        responseList = msgSetResponse.ResponseList
        Dim response As IResponse
        response = responseList.GetAt(0)
        If (Not response Is Nothing) Then
            If response.StatusCode <> "0" Then
                'If the status is bad, report it to the user
                MsgBox("FillVendorListBox unexpexcted Error - " & response.StatusMessage)
                bDone = True
                bError = True
                Exit Sub
            End If
        End If

        ' first make sure we have a response object to handle
        If (response Is Nothing) Or
            (response.Type Is Nothing) Or
            (response.Detail Is Nothing) Or
            (response.Detail.Type Is Nothing) Then
            bDone = True
            bError = True
            Exit Sub
        End If

        ' make sure we are processing the CustomerQueryRs and 
        ' the CustomerRetList responses in this response list
        Dim VendorRetList As IVendorRetList
        Dim responseType As ENResponseType
        Dim responseDetailType As ENObjectType
        responseType = response.Type.GetValue()
        responseDetailType = response.Detail.Type.GetValue()
        If (responseType = ENResponseType.rtVendorQueryRs) And (responseDetailType = ENObjectType.otVendorRetList) Then
            ' save the response detail in the appropriate object type
            ' since we have first verified the type of the response object
            VendorRetList = response.Detail
        Else
            ' bail, we do not have the responses we were expecting
            bDone = True
            bError = True
            Exit Sub
        End If

        Dim count As Short
        Dim index As Short
        count = VendorRetList.Count
        Dim VendorRet As IVendorRet

        Dim MAX_RETURNED As Short
        MAX_RETURNED = 1 + VendorRetList.Count
        ' we are done with the customerQueries if we have not received the MaxReturned
        If (count < MAX_RETURNED) Then
            bDone = True
        End If

        For index = 0 To count - 1
            VendorRet = VendorRetList.GetAt(index)

            If (VendorRet Is Nothing) Or
                    (VendorRet.Name Is Nothing) Or
                    (VendorRet.ListID Is Nothing) Then
                bDone = True
                bError = True
                Exit Sub
            End If
            'Featch value of vendor

            Dim listID As String = VendorRet.ListID.GetValue
            Dim Name As String = ""
            If Not (VendorRet.Name Is Nothing) Then
                Name = VendorRet.Name.GetValue
            End If

            Dim TimeCreated As String = ""
            If Not (VendorRet.TimeCreated Is Nothing) Then
                TimeCreated = VendorRet.TimeCreated.GetValue
            End If
            Dim TimeModified As String = ""
            If Not (VendorRet.TimeModified Is Nothing) Then
                TimeModified = VendorRet.TimeModified.GetValue
            End If
            Dim EditSequence As Integer
            If Not (VendorRet.EditSequence Is Nothing) Then
                EditSequence = VendorRet.EditSequence.GetValue
            End If
            'Dim ArName As String = ""
            'If Not (VendorRet.ArName Is Nothing) Then

            'End If
            'Dim FullName As String = ""
            'If Not (VendorRet.FullName Is Nothing) Then

            'End If
            Dim IsActive As String = ""
            If Not (VendorRet.IsActive Is Nothing) Then
                IsActive = VendorRet.IsActive.GetValue
            End If
            'Dim Sublevel As Integer
            'If Not (VendorRet.Sublevel Is Nothing) Then

            'End If
            Dim CompanyName As String = ""
            If Not (VendorRet.CompanyName Is Nothing) Then
                CompanyName = VendorRet.CompanyName.GetValue
            End If
            Dim Salutation As String = ""
            If Not (VendorRet.Salutation Is Nothing) Then
                Salutation = VendorRet.Salutation.GetValue
            End If
            Dim FirstName As String = ""

            If Not (VendorRet.FirstName Is Nothing) Then
                FirstName = VendorRet.FirstName.GetValue
            End If
            Dim MiddleName As String = ""
            If Not (VendorRet.MiddleName Is Nothing) Then
                MiddleName = VendorRet.MiddleName.GetValue
            End If
            Dim LastName As String = ""
            If Not (VendorRet.LastName Is Nothing) Then
                LastName = VendorRet.LastName.GetValue
            End If
            Dim BillAddress1 As String = ""
            Dim BillAddress2 As String = ""
            'Address part
            If Not (VendorRet.VendorAddress Is Nothing) Then

                If Not (VendorRet.VendorAddress.Addr1 Is Nothing) Then
                    BillAddress1 = VendorRet.VendorAddress.Addr1.GetValue
                End If

                If Not (VendorRet.VendorAddress.Addr2 Is Nothing) Then
                    BillAddress2 = VendorRet.VendorAddress.Addr2.GetValue
                End If
            End If
            Dim Phone As String = ""
            If Not (VendorRet.Phone Is Nothing) Then
                Phone = VendorRet.Phone.GetValue
            End If
            Dim AltPhone As String = ""
            If Not (VendorRet.AltPhone Is Nothing) Then
                AltPhone = VendorRet.AltPhone.GetValue
            End If
            Dim Fax As String = ""
            If Not (VendorRet.Fax Is Nothing) Then
                Fax = VendorRet.Fax.GetValue
            End If
            Dim Email As String = ""
            If Not (VendorRet.Email Is Nothing) Then
                Email = VendorRet.Email.GetValue
            End If
            Dim Contact As String = ""
            If Not (VendorRet.Contact Is Nothing) Then
                Contact = VendorRet.Contact.GetValue
            End If
            Dim AltContact As String = ""
            If Not (VendorRet.AltContact Is Nothing) Then
                AltContact = VendorRet.AltContact.GetValue
            End If
            Dim Balance As String = ""
            If Not (VendorRet.Balance Is Nothing) Then
                Balance = VendorRet.Balance.GetValue
            End If
            'Dim TotalBalance As String = ""
            'If Not (VendorRet.TotalBalance Is Nothing) Then

            'End If
            Dim AccountNumber As String = String.Empty
            If Not (VendorRet.AccountNumber Is Nothing) Then
                AccountNumber = VendorRet.AccountNumber.GetValue
            End If
            Dim CreditLimit As String = ""
            If Not (VendorRet.CreditLimit Is Nothing) Then
                CreditLimit = VendorRet.CreditLimit.GetValue
            End If
            Dim PrintChequeAs As String = ""
            If Not (VendorRet.NameOnCheck Is Nothing) Then
                PrintChequeAs = VendorRet.NameOnCheck.GetValue
            End If
            'Dim Cc As String = ""
            'If Not (VendorRet.Cc Is Nothing) Then

            'End If
            'Dim ActName As String = ""
            'If Not (VendorRet.ActName Is Nothing) Then

            'End If
            'Dim BankName As String = ""
            'If Not (VendorRet.PrefillAccountRefList. Is Nothing) Then

            'End If
            'Dim ActNumber As String = ""
            'If Not (VendorRet.Name Is Nothing) Then

            'End If
            'Dim Branch As String = ""
            'If Not (VendorRet.Name Is Nothing) Then

            'End If

            If (Not FoundVendorInDatabase(VendorRet.ListID.GetValue())) Then
                'Insert vendor data in database
                db.tblVendor_Insert(gblCompanyID, listID,
                                    TimeCreated,
                                    TimeModified,
                                    EditSequence,
                                    Name,
                                    IsActive,
                                    CompanyName,
                                    Salutation,
                                    FirstName,
                                    MiddleName,
                                    LastName,
                                    BillAddress1,
                                    BillAddress2,
                                    Phone,
                                    AltPhone,
                                    Fax,
                                    Email,
                                    Contact,
                                    AltContact,
                                    Balance,
                                    AccountNumber,
                                    CreditLimit,
                                    PrintChequeAs)

            Else
                'Update vendor table for vendor if available
                db.tblVendor_Update(gblCompanyID, listID,
                                    TimeCreated,
                                    TimeModified,
                                    EditSequence,
                                    Name,
                                    IsActive,
                                    CompanyName,
                                    Salutation,
                                    FirstName,
                                    MiddleName,
                                    LastName,
                                    BillAddress1,
                                    BillAddress2,
                                    Phone,
                                    AltPhone,
                                    Fax,
                                    Email,
                                    Contact,
                                    AltContact,
                                    Balance,
                                    AccountNumber,
                                    CreditLimit,
                                    PrintChequeAs)

            End If

        Next

        Exit Sub


Errs:
        MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description,
                    MsgBoxStyle.Critical,
                    "Error in FillChartOfAccountListBox")

        bDone = True
        bError = True
    End Sub

    Private Function FoundVendorInDatabase(ByRef listID As String) As Boolean

        ' On Error GoTo Errs
        FoundVendorInDatabase = False

        Try



            Dim result As ISingleResult(Of tblVendor_SelectResult) = db.tblVendor_Select(gblCompanyID, listID, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)

            Dim resultCount As Integer = result.Count

            If Not resultCount = 0 Then
                FoundVendorInDatabase = True
                Exit Function

            End If
        Catch ex As Exception

        End Try



        'Errs:
        '        MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, _
        '                    MsgBoxStyle.Critical, _
        '                    "Error in FoundCustomerInListBox")

    End Function
#End Region


    Function QBFCLatestVersion(ByVal SessionManager As QBSessionManager) As String
        Dim strXMLVersions() As String
        'Should be able to use this, but there appears to be a bug that may cause 2.0 to be returned
        'when it should not.
        'strXMLVersions = SessionManager.QBXMLVersionsForSession

        Dim msgset As IMsgSetRequest
        'Use oldest version to ensure that we work with any QuickBooks (US)
        msgset = SessionManager.CreateMsgSetRequest("US", 1, 0)
        msgset.AppendHostQueryRq()
        Dim QueryResponse As IMsgSetResponse
        QueryResponse = SessionManager.DoRequests(msgset)
        Dim response As IResponse

        ' The response list contains only one response,
        ' which corresponds to our single HostQuery request
        response = QueryResponse.ResponseList.GetAt(0)
        Dim HostResponse As IHostRet
        HostResponse = response.Detail
        Dim supportedVersions As IBSTRList
        supportedVersions = HostResponse.SupportedQBXMLVersionList

        Dim i As Long
        Dim vers As Double
        Dim LastVers As Double
        LastVers = 0
        For i = 0 To supportedVersions.Count - 1
            vers = Val(supportedVersions.GetAt(i))
            If (vers > LastVers) Then
                LastVers = vers
                QBFCLatestVersion = supportedVersions.GetAt(i)
            End If
        Next i
    End Function
End Module
