//using System;
//using System.Data;
//using System.Linq;
//using System.Windows.Forms;
//using System.Xml.Linq;
//using Interop.QBFC15;
//using Microsoft.VisualBasic;
//using Microsoft.VisualBasic.CompilerServices;

//namespace TNCSync.Class
//{

//    static class QuickBookData
//    {
//        private static bool booSessionBegun;
//        private static QBSessionManager qbSessionManager;
//        private static IMsgSetRequest msgSetRequest;
//        private static DBTNCCHKDataContext db = new DBTNCCHKDataContext(ClearAllControl.gblConnectionString);

//        public static void OpenConnectionBeginSession()
//        {

//            booSessionBegun = false;
//            try
//            {

//                qbSessionManager = new QBSessionManager();


//                var companyresult = db.tblCompanySelect(Convert.ToInt16(ClearAllControl.gblCompanyID), null, null);
//                string qbfile = "";
//                string qbCompanyName = @"C:\Users\Public\Documents\Intuit\QuickBooks\Sample Company Files\QuickBooks Enterprise Solutions 13.0\Test for Integration.qbw";
//                foreach (tblCompanySelectResult company in companyresult)
//                {
//                    qbfile = company.QBFileName;
//                    qbCompanyName = company.QBCompanyName;
//                }
//                qbSessionManager.OpenConnection("124", qbCompanyName);
//                Dim qbfile As String = "C:\Users\Public\Documents\Intuit\QuickBooks\Sample Company Files\QuickBooks Enterprise Solutions 11.0\sample_product-based business.qbw"
//                if (!string.IsNullOrEmpty(qbfile))
//                {
//                    qbSessionManager.BeginSession(qbfile, ENOpenMode.omDontCare);
//                }
//                else
//                {
//                    MessageBox.Show("Please setup company first", "TNC-CHECK MANAGEMENT SYSTEM");
//                    return;
//                }


//                qbSessionManager.BeginSession("", ENOpenMode.omDontCare)
//                booSessionBegun = true;

//                Check to make sure the QuickBooks we're working with supports version 2 of the SDK
//                string[] strXMLVersions;
//                strXMLVersions = qbSessionManager.QBXMLVersionsForSession;

//                bool booSupports2dot0;
//                booSupports2dot0 = false;
//                long i;
//                var loopTo = (long)Information.UBound(strXMLVersions);
//                for (i = Information.LBound(strXMLVersions); i <= loopTo; i++)
//                {
//                    if (strXMLVersions[(int)i] == "2.0")
//                    {
//                        booSupports2dot0 = true;
//                        msgSetRequest = qbSessionManager.CreateMsgSetRequest("US", 8, 0);
//                        break;
//                    }
//                }

//                if (!booSupports2dot0)
//                {
//                    Interaction.MsgBox("This program only runs against QuickBooks installations which support the 2.0 qbXML spec.  Your version of QuickBooks does not support qbXML 2.0");
//                    Environment.Exit(0);
//                }
//                return;
//            }
//            catch
//            {

//                if (Information.Err().Number == int.MinValue + 0x00040416)
//                {
//                    Interaction.MsgBox("You must have QuickBooks running with the company" + Constants.vbCrLf + "file open to use this program.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040422)
//                {
//                    Interaction.MsgBox("This QuickBooks company file is open in single user mode and" + Constants.vbCrLf + "another application is already accessing it.  Please exit the" + Constants.vbCrLf + "other application and run this application again.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040401)
//                {
//                    Interaction.MsgBox("Could not access QuickBooks (Failure in attempt to connection)");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040403)
//                {
//                    Interaction.MsgBox("Could not open the specified QuickBooks company data file");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040407)
//                {
//                    Interaction.MsgBox("The installation of QuickBooks appears to be incomplete." + Constants.vbCrLf + "Please reinstall QuickBooks.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040408)
//                {
//                    Interaction.MsgBox("Could not start QuickBooks." + Constants.vbCrLf + "Try Again");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x0004040A)
//                {
//                    Interaction.MsgBox("QuickBooks company data file is already open and it " + Constants.vbCrLf + "is different from the one requested.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x0004041A)
//                {
//                    Interaction.MsgBox("This application does not have permission to access this QuickBooks company data file" + Constants.vbCrLf + "Grant access permission through the Integrated Application preferences.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040420)
//                {
//                    Interaction.MsgBox("The QuickBooks user has denied access.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040424)
//                {
//                    Interaction.MsgBox("QuickBooks did not finish its initialization." + Constants.vbCrLf + "Please try again later.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//                else if (Information.Err().Number == int.MinValue + 0x00040427)
//                {
//                    Interaction.MsgBox("Your QuickBooks application needs to be registered.");
//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }

//                else
//                {
//                    Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in OpenConnectionBeginSession");

//                    if (booSessionBegun)
//                    {
//                        qbSessionManager.EndSession();
//                    }

//                    qbSessionManager.CloseConnection();
//                    return;
//                    Environment.Exit(0);
//                }
//            }
//        }

//        public static void EndSessionCloseConnection()
//        {
//            try
//            {

//                if (booSessionBegun)
//                {
//                    qbSessionManager.EndSession();
//                    qbSessionManager.CloseConnection();
//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in EndSessionCloseConnection");

//            }
//        }

//        Fill vendor data in database
//        public static void getVendor(ref bool bError)
//        {
//            try
//            {

//                make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();

//                set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;

//                Add the CustomerQuery request

//                IVendorQuery vendortQuery;
//                vendortQuery = msgSetRequest.AppendVendorQueryRq;
//                vendortQuery.ORVendorListQuery.VendorListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
//                vendortQuery.ORVendorListQuery.VendorListFilter.Type.GetValue();
//                bool bDone = false;
//                while (!bDone)
//                {
//                    send the request to QB
//                   IMsgSetResponse msgSetResponse;
//                    msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//                    FillVendorInDatabase(ref msgSetResponse, ref bDone, ref bError);

//                }
//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//                bError = true;
//            }
//        }

//        public static void FillVendorInDatabase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//        {
//            try
//            {

//                check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }
//                Start parsing the response list
//               IResponseList responseList;
//                responseList = msgSetResponse.ResponseList;
//                IResponse response;
//                response = responseList.GetAt(0);
//                if (response is not null)
//                {
//                    if (response.StatusCode != "0")
//                    {
//                        If the status is bad, report it to the user
//                        Interaction.MsgBox("FillVendorListBox unexpexcted Error - " + response.StatusMessage);
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                }

//                first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IVendorRetList VendorRetList;
//                ENResponseType responseType;
//                ENObjectType responseDetailType;
//                responseType = response.Type.GetValue();
//                responseDetailType = response.Detail.Type.GetValue();
//                if (responseType == ENResponseType.rtVendorQueryRs & responseDetailType == ENObjectType.otVendorRetList)
//                {
//                    save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    VendorRetList = response.Detail;
//                }
//                else
//                {
//                    bail, we do not have the responses we were expecting
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                short count;
//                short index;
//                count = VendorRetList.Count;
//                IVendorRet VendorRet;

//                short MAX_RETURNED;
//                MAX_RETURNED = 1 + VendorRetList.Count;
//                we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//                {
//                    bDone = true;
//                }

//                var EditSequence = default(int);
//                var loopTo = (short)(count - 1);
//                for (index = 0; index <= loopTo; index++)
//                {
//                    VendorRet = VendorRetList.GetAt(index);

//                    if (VendorRet is null | VendorRet.Name is null | VendorRet.ListID is null)

//                    {
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                    Featch value of vendor

//                    string listID = VendorRet.ListID.GetValue;
//                    string Name = "";
//                    if (VendorRet.Name is not null)
//                    {
//                        Name = VendorRet.Name.GetValue;
//                    }

//                    string TimeCreated = "";
//                    if (VendorRet.TimeCreated is not null)
//                    {
//                        TimeCreated = VendorRet.TimeCreated.GetValue;
//                    }
//                    string TimeModified = "";
//                    if (VendorRet.TimeModified is not null)
//                    {
//                        TimeModified = VendorRet.TimeModified.GetValue;
//                    }
//                    if (VendorRet.EditSequence is not null)
//                    {
//                        EditSequence = VendorRet.EditSequence.GetValue;
//                    }
//                    Dim ArName As String = ""
//                     If Not(VendorRet.ArName Is Nothing) Then

//                    End If
//                    Dim FullName As String = ""
//                     If Not(VendorRet.FullName Is Nothing) Then

//                    End If
//                    string IsActive = "";
//                    if (VendorRet.IsActive is not null)
//                    {
//                        IsActive = VendorRet.IsActive.GetValue;
//                    }
//                    Dim Sublevel As Integer
//                     If Not(VendorRet.Sublevel Is Nothing) Then

//                    End If
//                    string CompanyName = "";
//                    if (VendorRet.CompanyName is not null)
//                    {
//                        CompanyName = VendorRet.CompanyName.GetValue;
//                    }
//                    string Salutation = "";
//                    if (VendorRet.Salutation is not null)
//                    {
//                        Salutation = VendorRet.Salutation.GetValue;
//                    }
//                    string FirstName = "";

//                    if (VendorRet.FirstName is not null)
//                    {
//                        FirstName = VendorRet.FirstName.GetValue;
//                    }
//                    string MiddleName = "";
//                    if (VendorRet.MiddleName is not null)
//                    {
//                        MiddleName = VendorRet.MiddleName.GetValue;
//                    }
//                    string LastName = "";
//                    if (VendorRet.LastName is not null)
//                    {
//                        LastName = VendorRet.LastName.GetValue;
//                    }
//                    string BillAddress1 = "";
//                    string BillAddress2 = "";
//                    Address part
//                    if (VendorRet.VendorAddress is not null)
//                    {

//                        if (VendorRet.VendorAddress.Addr1 is not null)
//                        {
//                            BillAddress1 = VendorRet.VendorAddress.Addr1.GetValue;
//                        }

//                        if (VendorRet.VendorAddress.Addr2 is not null)
//                        {
//                            BillAddress2 = VendorRet.VendorAddress.Addr2.GetValue;
//                        }
//                    }
//                    string Phone = "";
//                    if (VendorRet.Phone is not null)
//                    {
//                        Phone = VendorRet.Phone.GetValue;
//                    }
//                    string AltPhone = "";
//                    if (VendorRet.AltPhone is not null)
//                    {
//                        AltPhone = VendorRet.AltPhone.GetValue;
//                    }
//                    string Fax = "";
//                    if (VendorRet.Fax is not null)
//                    {
//                        Fax = VendorRet.Fax.GetValue;
//                    }
//                    string Email = "";
//                    if (VendorRet.Email is not null)
//                    {
//                        Email = VendorRet.Email.GetValue;
//                    }
//                    string Contact = "";
//                    if (VendorRet.Contact is not null)
//                    {
//                        Contact = VendorRet.Contact.GetValue;
//                    }
//                    string AltContact = "";
//                    if (VendorRet.AltContact is not null)
//                    {
//                        AltContact = VendorRet.AltContact.GetValue;
//                    }
//                    string Balance = "";
//                    if (VendorRet.Balance is not null)
//                    {
//                        Balance = VendorRet.Balance.GetValue;
//                    }
//                    Dim TotalBalance As String = ""
//                     If Not(VendorRet.TotalBalance Is Nothing) Then

//                    End If
//                    string AccountNumber = string.Empty;
//                    if (VendorRet.AccountNumber is not null)
//                    {
//                        AccountNumber = VendorRet.AccountNumber.GetValue;
//                    }
//                    string CreditLimit = "";
//                    if (VendorRet.CreditLimit is not null)
//                    {
//                        CreditLimit = VendorRet.CreditLimit.GetValue;
//                    }
//                    string PrintChequeAs = "";
//                    if (VendorRet.NameOnCheck is not null)
//                    {
//                        PrintChequeAs = VendorRet.NameOnCheck.GetValue;
//                    }
//                    Dim Cc As String = ""
//                     If Not(VendorRet.Cc Is Nothing) Then

//                    End If
//                    Dim ActName As String = ""
//                     If Not(VendorRet.ActName Is Nothing) Then

//                    End If
//                    Dim BankName As String = ""
//                     If Not(VendorRet.PrefillAccountRefList.Is Nothing) Then

//                   End If
//                   Dim ActNumber As String = ""
//                     If Not(VendorRet.Name Is Nothing) Then

//                    End If
//                    Dim Branch As String = ""
//                     If Not(VendorRet.Name Is Nothing) Then

//                    End If

//                    bool localFoundVendorInDatabase() { var arglistID = VendorRet.ListID.GetValue(); var ret = FoundVendorInDatabase(ref arglistID); return ret; }

//                    if (!localFoundVendorInDatabase())
//                    {
//                        Insert vendor data in database
//                        db.tblVendor_Insert(ClearAllControl.gblCompanyID, listID, Conversions.ToDate(TimeCreated), Conversions.ToDate(TimeModified), EditSequence, Name, Conversions.ToChar(IsActive), CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, Phone, AltPhone, Fax, Email, Contact, AltContact, Conversions.ToDecimal(Balance), AccountNumber, CreditLimit, PrintChequeAs);
//                    }

//                    else
//                    {
//                        Update vendor table for vendor if available
//                        db.tblVendor_Update(ClearAllControl.gblCompanyID, listID, Conversions.ToDate(TimeCreated), Conversions.ToDate(TimeModified), EditSequence, Name, Conversions.ToChar(IsActive), CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, Phone, AltPhone, Fax, Email, Contact, AltContact, Conversions.ToDecimal(Balance), AccountNumber, CreditLimit, PrintChequeAs);
//                    }

//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");


//                bDone = true;
//                bError = true;
//            }
//        }

//        private static bool FoundVendorInDatabase(ref string listID)
//        {
//            bool FoundVendorInDatabaseRet = default;

//            On Error GoTo Errs
//            FoundVendorInDatabaseRet = false;

//            try
//            {



//                var result = db.tblVendor_Select(ClearAllControl.gblCompanyID, listID, null, null, null, null, null, null, null);

//                int resultCount = result.Count();

//                if (!(resultCount == 0))
//                {
//                    FoundVendorInDatabaseRet = true;
//                    return FoundVendorInDatabaseRet;

//                }
//            }
//            catch (Exception ex)
//            {

//            }

//            return FoundVendorInDatabaseRet;



//        Errs:
//            MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, _

//            MsgBoxStyle.Critical, _

//            "Error in FoundCustomerInListBox")

//        }

//        Fill Other name in database
//        public static void GetOtherName(ref bool bError)
//        {
//            try
//            {

//                make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();

//                set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;

//                Add the CustomerQuery request

//                IOtherNameQuery OtherNameQuery;
//                OtherNameQuery = msgSetRequest.AppendOtherNameQueryRq;

//                OtherNameQuery.ORListQuery.ListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);

//                bool bDone = false;

//                while (!bDone)
//                {

//                    start looking for customer next in the list

//                    we will have one overlap

//                    vendortQuery.ORVendorListQuery.VendorListFilter.ORNameFilter.NameRangeFilter.FromName.SetValue(firstFullName)

//                    customerQuery.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameRangeFilter.FromName.SetValue(firstFullName)


//                    send the request to QB

//                   IMsgSetResponse msgSetResponse;
//                    msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//                    MsgBox(msgSetRequest.ToXMLString())

//                    FillOtherNameInDatabase(ref msgSetResponse, ref bDone, ref bError);

//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//                bError = true;
//            }
//        }

//        public static void FillOtherNameInDatabase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//        {
//            try
//            {

//                check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Start parsing the response list
//               IResponseList responseList;
//                responseList = msgSetResponse.ResponseList;

//                go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//                response = responseList.GetAt(0);
//                if (response is not null)
//                {
//                    if (response.StatusCode != "0")
//                    {
//                        If the status is bad, report it to the user
//                        Interaction.MsgBox("FillOtherNameListBox unexpexcted Error - " + response.StatusMessage);
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                }

//                first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IOtherNameRetList OtherNameRetList;
//                ENResponseType responseType;
//                ENObjectType responseDetailType;
//                responseType = response.Type.GetValue();
//                responseDetailType = response.Detail.Type.GetValue();
//                if (responseType == ENResponseType.rtOtherNameQueryRs & responseDetailType == ENObjectType.otOtherNameRetList)
//                {
//                    save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    OtherNameRetList = response.Detail;
//                }
//                else
//                {
//                    bail, we do not have the responses we were expecting
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Parse the query response and add the Customers to the Customer list box
//                short count;
//                short index;
//                IOtherNameRet otherNameRet;
//                count = OtherNameRetList.Count;
//                short MAX_RETURNED;
//                MAX_RETURNED = 1 + OtherNameRetList.Count;
//                we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//                {
//                    bDone = true;
//                }

//                var TimeCreated = default(string);
//                var TimeModified = default(string);
//                var EditSequence = default(int);
//                var loopTo = (short)(count - 1);
//                for (index = 0; index <= loopTo; index++)
//                {
//                    skip this customer if this is a repeat from the last query
//                    otherNameRet = OtherNameRetList.GetAt(index);
//                    if (otherNameRet is null | otherNameRet.Name is null | otherNameRet.ListID is null)

//                    {
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                    only the first customerRet should be repeating and then
//                    lets just check to make sure we do not have the customer
//                    just in case another app changed a customer right between our
//                     queries.
//                     Field value for other name

//                    string ListID = otherNameRet.ListID.GetValue;
//                    if (otherNameRet.TimeCreated is not null)
//                    {
//                        TimeCreated = otherNameRet.TimeCreated.GetValue;
//                    }
//                    if (otherNameRet.TimeModified is not null)
//                    {
//                        TimeModified = otherNameRet.TimeModified.GetValue;
//                    }
//                    if (otherNameRet.EditSequence is not null)
//                    {
//                        EditSequence = otherNameRet.EditSequence.GetValue;
//                    }
//                    string Name = string.Empty;
//                    if (otherNameRet.Name is not null)
//                    {
//                        Name = otherNameRet.Name.GetValue;
//                    }
//                    Dim ArName As String = String.Empty
//                     If Not(otherNameRet.ArName Is Nothing) Then

//                    End If
//                    Dim FullName As String = String.Empty
//                     If Not(otherNameRet.FullName Is Nothing) Then

//                    End If
//                    string IsActive = string.Empty;
//                    if (otherNameRet.IsActive is not null)
//                    {
//                        IsActive = otherNameRet.IsActive.GetValue;
//                    }
//                    string CompanyName = string.Empty;
//                    if (otherNameRet.CompanyName is not null)
//                    {
//                        CompanyName = otherNameRet.CompanyName.GetValue;
//                    }
//                    string Salutation = string.Empty;
//                    if (otherNameRet.Salutation is not null)
//                    {
//                        Salutation = otherNameRet.Salutation.GetValue;
//                    }
//                    string FirstName = string.Empty;
//                    if (otherNameRet.FirstName is not null)
//                    {
//                        FirstName = otherNameRet.FirstName.GetValue;
//                    }
//                    string MiddleName = string.Empty;
//                    if (otherNameRet.MiddleName is not null)
//                    {
//                        MiddleName = otherNameRet.MiddleName.GetValue;
//                    }
//                    string LastName = string.Empty;
//                    if (otherNameRet.LastName is not null)
//                    {
//                        LastName = otherNameRet.LastName.GetValue;
//                    }
//                    string BillAddress1 = string.Empty;
//                    If Not(otherNameRet.OtherNameAddress.Addr1 Is Nothing) Then
//                   BillAddress1 = otherNameRet.OtherNameAddress.Addr1.GetValue
//                     End If
//                    string BillAddress2 = string.Empty;
//                    If Not(otherNameRet.OtherNameAddress.Addr2 Is Nothing) Then
//                   BillAddress2 = otherNameRet.OtherNameAddress.Addr2.GetValue
//                     End If
//                    string Phone = string.Empty;
//                    if (otherNameRet.Phone is not null)
//                    {
//                        Phone = otherNameRet.Phone.GetValue;
//                    }
//                    string AltPhone = string.Empty;
//                    if (otherNameRet.AltPhone is not null)
//                    {
//                        AltPhone = otherNameRet.AltPhone.GetValue;
//                    }
//                    string Fax = string.Empty;
//                    if (otherNameRet.Fax is not null)
//                    {
//                        Fax = otherNameRet.Fax.GetValue;
//                    }
//                    string Email = string.Empty;
//                    if (otherNameRet.Email is not null)
//                    {
//                        Email = otherNameRet.Email.GetValue;
//                    }
//                    string Contact = string.Empty;

//                    if (otherNameRet.Contact is not null)
//                    {
//                        Contact = otherNameRet.Contact.GetValue;
//                    }
//                    string AltContact = string.Empty;
//                    if (otherNameRet.AltContact is not null)
//                    {
//                        AltContact = otherNameRet.AltContact.GetValue;
//                    }
//                    Dim Balance As String
//                     If Not(otherNameRet.Balance Is Nothing) Then
//                    Balance = otherNameRet.b
//                     End If

//                    string AccountNumber = string.Empty;
//                    if (otherNameRet.AccountNumber is not null)
//                    {
//                        AccountNumber = otherNameRet.AccountNumber.GetValue;
//                    }

//                    bool localFoundOtherNameInDatabase() { var arglistID1 = otherNameRet.ListID.GetValue(); var ret = FoundOtherNameInDatabase(ref arglistID1); return ret; }

//                    if (!localFoundOtherNameInDatabase())
//                    {
//                        we are saving the FullName and ListID pairs for each Customer

//                        this is good practice since the FullName can change but the

//                        ListID will not change for an Customer.


//                       db.tblOtherName_Insert(ClearAllControl.gblCompanyID, ListID, Conversions.ToDate(TimeCreated), Conversions.ToDate(TimeModified), EditSequence, Name, Conversions.ToChar(IsActive), CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, Phone, AltPhone, Fax, Email, Contact, AltContact, AccountNumber);
//                    }

//                    else
//                    {
//                        Update in other name database filed
//                        db.tblOtherName_Update(ClearAllControl.gblCompanyID, ListID, Conversions.ToDate(TimeCreated), Conversions.ToDate(TimeModified), EditSequence, Name, Conversions.ToChar(IsActive), CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, Phone, AltPhone, Fax, Email, Contact, AltContact, AccountNumber);
//                    }

//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//                bDone = true;
//                bError = true;
//            }
//        }

//        private static bool FoundOtherNameInDatabase(ref string listID)
//        {
//            bool FoundOtherNameInDatabaseRet = default;
//            try
//            {

//                FoundOtherNameInDatabaseRet = false;
//                coding for get data from othername
//               var result = db.tblOtherName_Select(ClearAllControl.gblCompanyID, listID, null, null, null, null, null, null, null);
//               int resultCount = result.Count();

//                if (!(resultCount == 0))
//                {
//                    FoundOtherNameInDatabaseRet = true;

//                }



//                return FoundOtherNameInDatabaseRet;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FoundCustomerInListBox");

//            }

//        }

//        fill chart of account
//        public static void GetChartOfAccount(ref bool bError)
//        {
//            try
//            {
//                make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//                set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//                Add the CustomerQuery request
//                IAccountQuery charterAcoountQuery;
//                charterAcoountQuery = msgSetRequest.AppendAccountQueryRq;
//                charterAcoountQuery.ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
//                bool bDone = false;
//                string firstFullName = "!";
//                while (!bDone)
//                {
//                    start looking for customer next in the list

//                    charterAcoountQuery.ORAccountListQuery.AccountListFilter.AccountTypeList.Add(ENAccountType.atBank)

//                   charterAcoountQuery.ORAccountListQuery.AccountListFilter.ORNameFilter.NameRangeFilter.FromName.SetValue(firstFullName);
//                    send the request to QB
//                   IMsgSetResponse msgSetResponse;
//                    msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//                    MsgBox(msgSetRequest.ToXMLString())
//                    FillChartOfAccountDataBase(ref msgSetResponse, ref bDone, ref bError);
//                }
//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//                bError = true;
//            }
//        }

//        public static void FillChartOfAccountDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//        {
//            try
//            {

//                check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Start parsing the response list
//               IResponseList responseList;
//                responseList = msgSetResponse.ResponseList;

//                go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//                response = responseList.GetAt(0);
//                if (response is not null)
//                {
//                    if (response.StatusCode != "0")
//                    {
//                        If the status is bad, report it to the user
//                        Interaction.MsgBox("FillChartOfAccountListBox unexpexcted Error - " + response.StatusMessage);
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                }

//                first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IAccountRetList CoARetList;
//                ENResponseType responseType;
//                ENObjectType responseDetailType;
//                responseType = response.Type.GetValue();
//                responseDetailType = response.Detail.Type.GetValue();
//                if (responseType == ENResponseType.rtAccountQueryRs & responseDetailType == ENObjectType.otAccountRetList)
//                {
//                    save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    CoARetList = response.Detail;
//                }
//                else
//                {
//                    bail, we do not have the responses we were expecting
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Parse the query response and add the Customers to the Customer list box
//                short count;
//                short index;
//                IAccountRet CoaRet;
//                count = CoARetList.Count;
//                short MAX_RETURNED;
//                MAX_RETURNED = 1 + CoARetList.Count;
//                we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//                {
//                    bDone = true;
//                }

//                var TimeModified = default(DateTime);
//                var EditSequence = default(int);
//                var Sublevel = default(int);
//                var balance = default(decimal);
//                var TotalBalance = default(decimal);
//                var PdcPostingAcct = default(int);
//                var DaysBefore = default(int);
//                var DaysAfter = default(int);
//                var loopTo = (short)(count - 1);
//                for (index = 0; index <= loopTo; index++)
//                {
//                    skip this customer if this is a repeat from the last query
//                    CoaRet = CoARetList.GetAt(index);
//                    if (CoaRet is null | CoaRet.FullName is null | CoaRet.ListID is null)
//                    {
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                    only the first customerRet should be repeating and then
//                    lets just check to make sure we do not have the customer
//                    just in case another app changed a customer right between our
//                     queries.
//                    bool localFoundAccountInDatabase() { var arglistID2 = CoaRet.ListID.GetValue(); var ret = FoundAccountInDatabase(ref arglistID2); return ret; }

//                    if (!localFoundAccountInDatabase())
//                    {
//                        Insert data in database code left..........string listID = CoaRet.ListID.GetValue();
//                        DateTime TimeCreated = CoaRet.TimeCreated.GetValue;

//                        if (CoaRet.TimeModified is not null)
//                        {
//                            TimeModified = CoaRet.TimeModified.GetValue;
//                        }

//                        if (CoaRet.EditSequence is not null)
//                        {
//                            EditSequence = CoaRet.EditSequence.GetValue;
//                        }


//                        string Name = string.Empty;
//                        if (CoaRet.Name is not null)
//                        {
//                            Name = CoaRet.Name.GetValue;
//                        }
//                        string FullName = string.Empty;
//                        if (CoaRet.FullName is not null)
//                        {
//                            FullName = CoaRet.FullName.GetValue;
//                        }
//                        string AccountName = string.Empty;
//                        if (CoaRet.FullName is not null & CoaRet.AccountNumber is not null)
//                        {

//                            AccountName = CoaRet.AccountNumber.GetValue + "·" + CoaRet.FullName.GetValue;
//                        }
//                        string Parent = string.Empty;
//                        If Not(CoaRet.ParentRef Is Nothing) Then
//                       Parent = CoaRet.ParentRef.GetValue
//                         End If
//                        string isActive = "Y";
//                        if (CoaRet.Sublevel is not null)
//                        {
//                            Sublevel = CoaRet.Sublevel.GetValue;
//                        }
//                        string AccountType = string.Empty;
//                        if (CoaRet.AccountType is not null)
//                        {
//                            AccountType = CoaRet.AccountType.GetValue;
//                        }
//                        string SpecialAccountType = string.Empty;
//                        if (CoaRet.SpecialAccountType is not null)
//                        {
//                            SpecialAccountType = CoaRet.SpecialAccountType.GetValue;
//                        }
//                        string AccountNumber = string.Empty;
//                        if (CoaRet.AccountNumber is not null)
//                        {
//                            AccountNumber = CoaRet.AccountNumber.GetValue;
//                        }
//                        if (CoaRet.Balance is not null)
//                        {
//                            balance = CoaRet.Balance.GetValue;
//                        }
//                        if (CoaRet.TotalBalance is not null)
//                        {
//                            TotalBalance = CoaRet.TotalBalance.GetValue;
//                        }
//                        string CashFlowClassification = string.Empty;
//                        if (CoaRet.CashFlowClassification is not null)
//                        {
//                            CashFlowClassification = CoaRet.CashFlowClassification.GetValue;
//                        }
//                        string ACTLEVELS = string.Empty;

//                        int SHOW_MENU = 0;

//                        string DESCRIPTION = string.Empty;
//                        if (CoaRet.Desc is not null)
//                        {
//                            DESCRIPTION = CoaRet.Desc.GetValue;
//                        }
//                        string NOTES = string.Empty;
//                        var BalanceDate = DateTime.Now;
//                        string ACTLEVELSwithNO = string.Empty;
//                        string AutoPosting = string.Empty;
//                        string PostingOn = string.Empty;


//                        db.tblAccounts_Insert(ClearAllControl.gblCompanyID, listID, TimeCreated, TimeModified, EditSequence, Name, FullName, AccountName, Parent, Conversions.ToChar(isActive), Sublevel, AccountType, SpecialAccountType, AccountNumber, balance, TotalBalance, CashFlowClassification, ACTLEVELS, SHOW_MENU, DESCRIPTION, NOTES, BalanceDate, ACTLEVELSwithNO, Conversions.ToChar(AutoPosting), Conversions.ToChar(PostingOn), PdcPostingAcct, DaysBefore, DaysAfter);

























//                    }

//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//                bDone = true;
//                bError = true;
//            }
//        }

//        private static bool FoundAccountInDatabase(ref string listID)
//        {
//            bool FoundAccountInDatabaseRet = default;
//            try
//            {

//                FoundAccountInDatabaseRet = false;

//               // go thru our list box and find the item which was modified
//                short i;
//                short numCustomers;
//                //check in database for existing customer

//               var result = db.tblAccounts_Select(listID, null, ClearAllControl.gblCompanyID);
//                int resultCount = result.Count();

//                if (!(resultCount == 0))
//                {
//                    FoundAccountInDatabaseRet = true;
//                }

//                return FoundAccountInDatabaseRet;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FoundIn Account database");

//            }

//        }
//        public static void getVendorCredit(ref bool bError)
//        {
//            try
//            {
//                make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//                set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//                Add the BillQuery request
//                IVendorCreditQuery vendorCreit;


//                vendorCreit = msgSetRequest.AppendVendorCreditQueryRq;
//                vendorCreit.IncludeLineItems.SetValue(true);
//                vendorCreit.IncludeLinkedTxns.SetValue(true);

//                bool bDone = false;
//                while (!bDone)
//                {
//                    start looking for customer next in the list


//                    send the request to QB

//                   IMsgSetResponse msgSetResponse;
//                    msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//                    MsgBox(msgSetRequest.ToXMLString())
//                    FillVendorCreitDetailsInDataBase(ref msgSetResponse, ref bDone, ref bError);
//                }
//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//                bError = true;
//            }
//        }
//        public static void FillVendorCreitDetailsInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//        {
//            try
//            {

//                check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Start parsing the response list
//               IResponseList responseList;
//                responseList = msgSetResponse.ResponseList;

//                go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//                response = responseList.GetAt(0);
//                if (response is not null)
//                {
//                    if (response.StatusCode != "0")
//                    {
//                        If the status is bad, report it to the user
//                        Interaction.MsgBox("FillChartOfAccountListBox unexpexcted Error - " + response.StatusMessage);
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                }

//                first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IVendorCreditRetList BillRetList;
//                ENResponseType responseType;
//                ENObjectType responseDetailType;
//                responseType = response.Type.GetValue();
//                responseDetailType = response.Detail.Type.GetValue();
//                if (responseType == ENResponseType.rtVendorCreditQueryRs & responseDetailType == ENObjectType.otVendorCreditRetList)
//                {
//                    save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    BillRetList = response.Detail;
//                }
//                else
//                {
//                    bail, we do not have the responses we were expecting
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Parse the query response and add the Customers to the Customer list box
//                short count;
//                short index;
//                IVendorCreditRet BillRet;
//                count = BillRetList.Count;
//                short MAX_RETURNED;
//                MAX_RETURNED = 1 + BillRetList.Count;
//                we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//                {
//                    bDone = true;
//                }
//                db.tblVendorCreit_Delete(ClearAllControl.gblCompanyID);

//                var EditSequence = default(int);
//                var TxnNumber = default(int);
//                var TxnDate = default(DateTime);
//                var loopTo = (short)(count - 1);
//                for (index = 0; index <= loopTo; index++)
//                {
//                    skip this customer if this is a repeat from the last query
//                    BillRet = BillRetList.GetAt(index);
//                    if (BillRet is null | BillRet.VendorRef.ListID is null | BillRet.TxnID is null)

//                    {
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                    only the first customerRet should be repeating and then
//                    lets just check to make sure we do not have the customer
//                    just in case another app changed a customer right between our
//                     queries.
//                    string TxnID = BillRet.TxnID.GetValue();
//                    if (BillRet.EditSequence is not null)
//                    {
//                        EditSequence = BillRet.EditSequence.GetValue;
//                    }
//                    if (BillRet.TxnNumber is not null)
//                    {
//                        TxnNumber = BillRet.TxnNumber.GetValue;
//                    }
//                    string vendorName = string.Empty;
//                    string VendorRef_Key = string.Empty;

//                    if (BillRet.VendorRef is not null)
//                    {

//                        if (BillRet.VendorRef.ListID is not null)
//                        {
//                            VendorRef_Key = BillRet.VendorRef.ListID.GetValue;
//                            vendorName = BillRet.VendorRef.FullName.GetValue;
//                        }
//                    }

//                    string APAccountRef_Key = string.Empty;
//                    if (BillRet.APAccountRef is not null)
//                    {
//                        if (BillRet.APAccountRef.ListID is not null)
//                        {
//                            APAccountRef_Key = BillRet.APAccountRef.ListID.GetValue;
//                        }
//                    }
//                    if (BillRet.TxnDate is not null)
//                    {
//                        TxnDate = BillRet.TxnDate.GetValue;
//                    }
//                    double Amount = 0.0d;
//                    If Not(BillRet.AmountDue Is Nothing) Then
//                   Amount = BillRet.AmountDue.GetValue
//                     End If

//                    if (BillRet.CreditAmount is not null)
//                    {
//                        Amount = BillRet.CreditAmount.GetValue;
//                    }
//                    double amountInHC = 0.0d;
//                    if (BillRet.CreditAmountInHomeCurrency is not null)
//                    {
//                        amountInHC = BillRet.CreditAmountInHomeCurrency.GetValue;
//                    }
//                    string RefNumber = string.Empty;
//                    if (BillRet.RefNumber is not null)
//                    {
//                        RefNumber = BillRet.RefNumber.GetValue;
//                    }
//                    string type = string.Empty;
//                    if (BillRet.Type is not null)
//                    {
//                        type = BillRet.Type.GetValue;

//                    }

//                    string Memo = string.Empty;
//                    if (BillRet.Memo is not null)
//                    {
//                        Memo = BillRet.Memo.GetValue;
//                    }

//                    string currencyRef = string.Empty;
//                    if (BillRet.CurrencyRef.ListID is not null)
//                    {
//                        currencyRef = BillRet.CurrencyRef.ListID.GetValue;
//                    }

//                    string exchangeRate = string.Empty;
//                    if (BillRet.ExchangeRate is not null)
//                    {
//                        exchangeRate = BillRet.ExchangeRate.GetValue;
//                    }
//                    double openAmount = 0.0d;
//                    if (BillRet.OpenAmount is not null)
//                    {
//                        openAmount = BillRet.OpenAmount.GetValue;
//                    }

//                    Insert data in database code left..........db.tblVendorCreit_Insert(VendorRef_Key, vendorName, APAccountRef_Key, (decimal?)Amount, (decimal?)amountInHC, currencyRef, EditSequence.ToString(), exchangeRate, Memo, (decimal?)openAmount, TxnDate, TxnNumber.ToString(), type, ClearAllControl.gblCompanyID, RefNumber);

//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//                bDone = true;
//                bError = true;
//            }

//        }
//        Get bill query data
//        public static void GetInvAdjustment(ref bool bError)
//        {
//            try
//            {
//                make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//                set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//                Add the BillQuery request
//                IInventoryAdjustmentQuery InvAdjQuery;


//                InvAdjQuery = msgSetRequest.AppendInventoryAdjustmentQueryRq;


//                InvAdjQuery.IncludeLineItems.SetValue(true);
//                bool bDone = false;
//                while (!bDone)
//                {
//                    start looking for customer next in the list


//                    send the request to QB

//                   IMsgSetResponse msgSetResponse;
//                    msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//                    MsgBox(msgSetRequest.ToXMLString())
//                    FillInvAdjInDataBase(ref msgSetResponse, ref bDone, ref bError);
//                }
//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//                bError = true;
//            }

//        }
//        public static void FillInvAdjInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//        {
//            try
//            {

//                check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Start parsing the response list
//               IResponseList responseList;
//                responseList = msgSetResponse.ResponseList;

//                go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//                response = responseList.GetAt(0);
//                if (response is not null)
//                {
//                    if (response.StatusCode != "0")
//                    {
//                        If the status is bad, report it to the user
//                        Interaction.MsgBox("FillChartOfAccountListBox unexpexcted Error - " + response.StatusMessage);
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                }

//                first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IInventoryAdjustmentRetList invAdjRetList;
//                ENResponseType responseType;
//                ENObjectType responseDetailType;
//                responseType = response.Type.GetValue();
//                responseDetailType = response.Detail.Type.GetValue();
//                if (responseType == ENResponseType.rtInventoryAdjustmentQueryRs & responseDetailType == ENObjectType.otInventoryAdjustmentRetList)
//                {
//                    save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    invAdjRetList = response.Detail;
//                }
//                else
//                {
//                    bail, we do not have the responses we were expecting
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Parse the query response and add the Customers to the Customer list box
//                short count;
//                short index;
//                IInventoryAdjustmentRet invAdjRet;
//                count = invAdjRetList.Count;
//                short MAX_RETURNED;
//                MAX_RETURNED = 1 + invAdjRetList.Count;
//                we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//                {
//                    bDone = true;
//                }
//                db.tblBillLinkedTxn_Delete();
//                db.tblBill_Delete(ClearAllControl.gblCompanyID);
//                var loopTo = (short)(count - 1);
//                for (index = 0; index <= loopTo; index++)
//                {
//                    skip this customer if this is a repeat from the last query
//                    invAdjRet = invAdjRetList.GetAt(index);
//                    if (invAdjRet is null | invAdjRet.VendorRef.ListID is null | invAdjRet.TxnID is null)

//                    {
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                    only the first customerRet should be repeating and then
//                    lets just check to make sure we do not have the customer
//                    just in case another app changed a customer right between our
//                     queries.

//                    string accountRef = string.Empty;
//                    if (invAdjRet.AccountRef is not null)
//                    {
//                        accountRef = invAdjRet.AccountRef.ListID.GetValue();
//                    }

//                    string classRef = string.Empty;
//                    if (invAdjRet.ClassRef is not null)
//                    {
//                        classRef = invAdjRet.ClassRef.ListID.GetValue;
//                    }

//                    string customerID = string.Empty;
//                    string customerName = string.Empty;
//                    if (invAdjRet.CustomerRef is not null)
//                    {
//                        customerID = invAdjRet.CustomerRef.ListID.GetValue;
//                        customerName = invAdjRet.CustomerRef.FullName.GetValue;
//                    }

//                    string memo = string.Empty;
//                    if (invAdjRet.Memo is not null)
//                    {
//                        memo = invAdjRet.Memo.GetValue;
//                    }

//                    string refNo = string.Empty;
//                    if (invAdjRet.RefNumber is not null)
//                    {
//                        refNo = invAdjRet.RefNumber.GetValue;
//                    }

//                    DateTime txnDate;
//                    if (invAdjRet.TxnDate is not null)
//                    {
//                        txnDate = invAdjRet.TxnDate.GetValue;
//                    }

//                    string txnNumber = string.Empty;
//                    if (invAdjRet.TxnNumber is not null)
//                    {
//                        txnNumber = invAdjRet.TxnNumber.GetValue;
//                    }

//                    string type = string.Empty;
//                    if (invAdjRet.Type is not null)
//                    {
//                        type = invAdjRet.Type.GetAsString;
//                    }




//                    IItemLineRet BillItemLine;
//                    int itemLineCount;
//                    If Not BillRet.ORItemLineRetList Is Nothing Then
//                     For itemLineCount = 0 To BillRet.ORItemLineRetList.Count
//                     BillItemLine = BillRet.ORItemLineRetList.GetAt(itemLineCount)
//                     Dim BAmount As Double
//                     If Not(BillItemLine.Amount Is Nothing) Then
//                    BAmount = BillItemLine.Amount.GetValue()
//                     End If
//                     Dim BBIllableStatus As String = String.Empty
//                     If Not(BillItemLine.BillableStatus Is Nothing) Then
//                    BBIllableStatus = BillItemLine.BillableStatus.GetValue()
//                     End If
//                     Dim BClassListId As String = String.Empty
//                     Dim BClassFullName As String = String.Empty
//                     Dim BClassTyepe As String = String.Empty
//                     If Not(BillItemLine.ClassRef Is Nothing) Then
//                    BClassListId = BillItemLine.ClassRef.ListID.GetValue
//                     BClassFullName = BillItemLine.ClassRef.FullName.GetValue
//                     BClassTyepe = BillItemLine.Type.GetValue
//                     End If
//                     Dim BCost As Double
//                     If Not BillItemLine.Cost Is Nothing Then
//                     BCost = BillItemLine.Cost.GetAsString()
//                     End If
//                     Dim BCustomerListId As String = String.Empty
//                     Dim BCustomerFullName As String = String.Empty
//                     Dim BCustomerType As String = String.Empty
//                     If Not(BillItemLine.CustomerRef Is Nothing) Then
//                    BClassListId = BillItemLine.CustomerRef.ListID.GetValue
//                     BCustomerFullName = BillItemLine.CustomerRef.FullName.GetValue
//                     BCustomerType = BillItemLine.CustomerRef.Type.GetValue()
//                     End If

//                     Dim BDesc As String = String.Empty
//                     If Not(BillItemLine.Desc Is Nothing) Then
//                    BDesc = BillItemLine.Desc.GetValue()
//                     End If

//                     Next
//                     End If


//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//                bDone = true;
//                bError = true;
//            }
//        }
//        public static void GetBill(ref bool bError)
//        {
//            try
//            {
//                make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//                set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//                Add the BillQuery request
//                IBillQuery BillNameQuery;


//                BillNameQuery = msgSetRequest.AppendBillQueryRq;
//                BillNameQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psAll);

//                BillNameQuery.IncludeLinkedTxns.SetValue(true);
//                bool bDone = false;
//                while (!bDone)
//                {
//                    start looking for customer next in the list


//                    send the request to QB

//                   IMsgSetResponse msgSetResponse;
//                    msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//                    MsgBox(msgSetRequest.ToXMLString())
//                    FillBillDetailsInDataBase(ref msgSetResponse, ref bDone, ref bError);
//                }
//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//                bError = true;
//            }
//        }

//        public static void FillBillDetailsInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//        {
//            try
//            {

//                check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Start parsing the response list
//               IResponseList responseList;
//                responseList = msgSetResponse.ResponseList;

//                go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//                response = responseList.GetAt(0);
//                if (response is not null)
//                {
//                    if (response.StatusCode != "0")
//                    {
//                        If the status is bad, report it to the user
//                        Interaction.MsgBox("FillChartOfAccountListBox unexpexcted Error - " + response.StatusMessage);
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                }

//                first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IBillRetList BillRetList;
//                ENResponseType responseType;
//                ENObjectType responseDetailType;
//                responseType = response.Type.GetValue();
//                responseDetailType = response.Detail.Type.GetValue();
//                if (responseType == ENResponseType.rtBillQueryRs & responseDetailType == ENObjectType.otBillRetList)
//                {
//                    save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    BillRetList = response.Detail;
//                }
//                else
//                {
//                    bail, we do not have the responses we were expecting
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Parse the query response and add the Customers to the Customer list box
//                short count;
//                short index;
//                IBillRet BillRet;
//                count = BillRetList.Count;
//                short MAX_RETURNED;
//                MAX_RETURNED = 1 + BillRetList.Count;
//                we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//                {
//                    bDone = true;
//                }
//                db.tblBillLinkedTxn_Delete();
//                db.tblBill_Delete(ClearAllControl.gblCompanyID);
//                var EditSequence = default(int);
//                var TxnNumber = default(int);
//                var TxnDate = default(DateTime);
//                var DueDate = default(DateTime);
//                var AmountDue = default(double);
//                var billLTDate = default(DateTime);
//                var loopTo = (short)(count - 1);
//                for (index = 0; index <= loopTo; index++)
//                {
//                    skip this customer if this is a repeat from the last query
//                    BillRet = BillRetList.GetAt(index);
//                    if (BillRet is null | BillRet.VendorRef.ListID is null | BillRet.TxnID is null)

//                    {
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                    only the first customerRet should be repeating and then
//                    lets just check to make sure we do not have the customer
//                    just in case another app changed a customer right between our
//                     queries.
//                    string TxnID = BillRet.TxnID.GetValue();
//                    string TimeCreated = BillRet.TimeCreated.GetValue;

//                    string TimeModified = string.Empty;
//                    if (BillRet.TimeModified is not null)
//                    {
//                        TimeModified = BillRet.TimeModified.GetValue;
//                    }
//                    if (BillRet.EditSequence is not null)
//                    {
//                        EditSequence = BillRet.EditSequence.GetValue;
//                    }
//                    if (BillRet.TxnNumber is not null)
//                    {
//                        TxnNumber = BillRet.TxnNumber.GetValue;
//                    }
//                    string CR_Flag = string.Empty;
//                    string VendorRef_Key = string.Empty;

//                    if (BillRet.VendorRef is not null)
//                    {

//                        if (BillRet.VendorRef.ListID is not null)
//                        {
//                            VendorRef_Key = BillRet.VendorRef.ListID.GetValue;
//                        }
//                    }

//                    string APAccountRef_Key = string.Empty;
//                    if (BillRet.APAccountRef is not null)
//                    {
//                        if (BillRet.APAccountRef.ListID is not null)
//                        {
//                            APAccountRef_Key = BillRet.APAccountRef.ListID.GetValue;
//                        }
//                    }
//                    if (BillRet.TxnDate is not null)
//                    {
//                        TxnDate = BillRet.TxnDate.GetValue;
//                    }
//                    if (BillRet.DueDate is not null)
//                    {
//                        DueDate = BillRet.DueDate.GetValue;
//                    }
//                    If Not(BillRet.AmountDue Is Nothing) Then
//                   Amount = BillRet.AmountDue.GetValue
//                     End If
//                    double Amount = 0.0d;
//                    if (BillRet.AmountDue is not null)
//                    {
//                        AmountDue = BillRet.AmountDue.GetValue;
//                    }
//                    string RefNumber = string.Empty;
//                    if (BillRet.RefNumber is not null)
//                    {
//                        RefNumber = BillRet.RefNumber.GetValue;
//                    }
//                    string TermsRef_Key = string.Empty;
//                    if (BillRet.TermsRef is not null)
//                    {
//                        if (BillRet.TermsRef.ListID is not null)
//                        {
//                            TermsRef_Key = BillRet.TermsRef.ListID.GetValue;
//                        }
//                    }

//                    string Memo = string.Empty;
//                    if (BillRet.Memo is not null)
//                    {
//                        Memo = BillRet.Memo.GetValue;
//                    }
//                    string BillNo = string.Empty;
//                    string Bill_Source = "QB";
//                    Insert data in database code left..........db.tblBill_Insert(ClearAllControl.gblCompanyID, TxnID, TimeCreated, TimeModified, EditSequence, TxnNumber, Conversions.ToChar(CR_Flag), VendorRef_Key, APAccountRef_Key, TxnDate, DueDate, (decimal?)Amount, (decimal?)AmountDue, RefNumber, TermsRef_Key, Memo, BillNo, Bill_Source);















//                    ILinkedTxn billLinkedTxn;
//                    if (BillRet.LinkedTxnList is not null)
//                    {
//                        int p = 0;
//                        var loopTo1 = BillRet.LinkedTxnList.Count - 1;
//                        for (p = 0; p <= loopTo1; p++)
//                        {
//                            billLinkedTxn = BillRet.LinkedTxnList.GetAt(p);
//                            string BillLTRefNum = string.Empty;
//                            if (billLinkedTxn.RefNumber is not null)
//                            {
//                                BillLTRefNum = billLinkedTxn.RefNumber.GetValue;
//                            }
//                            if (billLinkedTxn.TxnDate is not null)
//                            {
//                                billLTDate = billLinkedTxn.TxnDate.GetValue;
//                            }
//                            string billLTAmount = string.Empty;
//                            if (billLinkedTxn.Amount is not null)
//                            {
//                                billLTAmount = billLinkedTxn.Amount.GetValue;
//                            }
//                            db.tblBillLinkedTxn_Insert(RefNumber, BillLTRefNum, billLTDate, Conversions.ToDecimal(billLTAmount));
//                        }
//                    }

//                    IExpenseLineRet BillExpenceList;
//                    int j = 0;
//                    if (BillRet.ExpenseLineRetList is not null)
//                    {
//                        var loopTo2 = BillRet.ExpenseLineRetList.Count - 1;
//                        for (j = 0; j <= loopTo2; j++)
//                        {
//                            BillExpenceList = BillRet.ExpenseLineRetList.GetAt(j);
//                            string EListId = string.Empty;
//                            string EFullname = string.Empty;
//                            string EType = string.Empty;
//                            if (BillExpenceList.AccountRef is not null)
//                            {
//                                EListId = BillExpenceList.AccountRef.ListID.GetValue;
//                                EFullname = BillExpenceList.AccountRef.FullName.GetValue;
//                                EType = BillExpenceList.AccountRef.Type.GetValue;
//                            }
//                            double Eamount;
//                            if (BillExpenceList.Amount is not null)
//                            {
//                                Eamount = BillExpenceList.Amount.GetValue();
//                            }
//                            string EBIllableStatus = string.Empty;
//                            if (BillExpenceList.BillableStatus is not null)
//                            {
//                                EBIllableStatus = BillExpenceList.BillableStatus.GetValue();
//                            }
//                            string EClassListId = string.Empty;
//                            string EClassFullName = string.Empty;
//                            string EclassType = string.Empty;
//                            if (BillExpenceList.ClassRef is not null)
//                            {
//                                EClassListId = BillExpenceList.ClassRef.ListID.GetValue;
//                                EClassFullName = BillExpenceList.ClassRef.FullName.GetValue;
//                                EclassType = BillExpenceList.ClassRef.Type.GetValue;
//                            }
//                            string EcustomerListId = string.Empty;
//                            string EcustomerFullName = string.Empty;
//                            string EcustomerType = string.Empty;
//                            if (BillExpenceList.CustomerRef is not null)
//                            {
//                                EcustomerListId = BillExpenceList.CustomerRef.ListID.GetValue;
//                                EcustomerFullName = BillExpenceList.CustomerRef.FullName.GetValue;
//                                EcustomerType = BillExpenceList.CustomerRef.Type.GetValue;
//                            }
//                            string Ememo = string.Empty;
//                            if (BillExpenceList.Memo is not null)
//                            {
//                                Ememo = BillExpenceList.Memo.GetValue();
//                            }
//                            string EtxnLineId = string.Empty;
//                            if (BillExpenceList.TxnLineID is not null)
//                            {
//                                EtxnLineId = BillExpenceList.TxnLineID.GetValue();
//                            }
//                        }
//                    }
//                    'Dim BillLinkedTxn As ILinkedTxnList
//                    IItemLineRet BillItemLine;
//                    int itemLineCount;
//                    if (BillRet.ORItemLineRetList is not null)
//                    {
//                        var loopTo3 = BillRet.ORItemLineRetList.Count;
//                        for (itemLineCount = 0; itemLineCount <= loopTo3; itemLineCount++)
//                        {
//                            BillItemLine = BillRet.ORItemLineRetList.GetAt(itemLineCount);
//                            double BAmount;
//                            if (BillItemLine.Amount is not null)
//                            {
//                                BAmount = BillItemLine.Amount.GetValue();
//                            }
//                            string BBIllableStatus = string.Empty;
//                            if (BillItemLine.BillableStatus is not null)
//                            {
//                                BBIllableStatus = BillItemLine.BillableStatus.GetValue();
//                            }
//                            string BClassListId = string.Empty;
//                            string BClassFullName = string.Empty;
//                            string BClassTyepe = string.Empty;
//                            if (BillItemLine.ClassRef is not null)
//                            {
//                                BClassListId = BillItemLine.ClassRef.ListID.GetValue;
//                                BClassFullName = BillItemLine.ClassRef.FullName.GetValue;
//                                BClassTyepe = BillItemLine.Type.GetValue;
//                            }
//                            double BCost;
//                            if (BillItemLine.Cost is not null)
//                            {
//                                BCost = BillItemLine.Cost.GetAsString();
//                            }
//                            string BCustomerListId = string.Empty;
//                            string BCustomerFullName = string.Empty;
//                            string BCustomerType = string.Empty;
//                            if (BillItemLine.CustomerRef is not null)
//                            {
//                                BClassListId = BillItemLine.CustomerRef.ListID.GetValue;
//                                BCustomerFullName = BillItemLine.CustomerRef.FullName.GetValue;
//                                BCustomerType = BillItemLine.CustomerRef.Type.GetValue();
//                            }

//                            string BDesc = string.Empty;
//                            if (BillItemLine.Desc is not null)
//                            {
//                                BDesc = BillItemLine.Desc.GetValue();
//                            }

//                        }
//                    }


//                }

//                return;
//            }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//                bDone = true;
//                bError = true;
//            }
//        }
//        public static void GetJournalTransaction(ref bool bError)
//        {
//            On Error GoTo Errs
//             make sure we do not have any old requests still defined
//            msgSetRequest.ClearRequests();
//            set the OnError attribute to continueOnError
//            msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//            Add the BillQuery request
//            IJournalEntryQuery JournalQuery;
//            JournalQuery = msgSetRequest.AppendJournalEntryQueryRq;
//            CheckQuery.IncludeLinkedTxns.SetValue(True)
//            JournalQuery.IncludeLineItems.SetValue(true);
//            'Dim jrnlDate As Date
//             'jrnlDate = "01/01/2016"
//             'JournalQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(jrnlDate)
//             CheckQuery.IncludeLinkedTxns.SetValue(True)


//             CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//            bool bDone = false;
//            while (!bDone)
//            {
//                start looking for customer next in the list


//                send the request to QB

//               IMsgSetResponse msgSetResponse;
//                msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//                MsgBox(msgSetRequest.ToXMLString())
//                FillJournalDataBase(ref msgSetResponse, ref bDone, ref bError);
//            }
//            return;

//        Errs:
//            ;

//            Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//            bError = true;
//        }
//        public static void FillJournalDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//        {
//            try
//            {

//                check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Start parsing the response list
//               IResponseList responseList;
//                responseList = msgSetResponse.ResponseList;

//                go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//                response = responseList.GetAt(0);
//                if (response is not null)
//                {
//                    if (response.StatusCode != "0")
//                    {
//                        If the status is bad, report it to the user
//                        Interaction.MsgBox("Get Journal query unexpexcted Error - " + response.StatusMessage);
//                        bDone = true;
//                        bError = true;
//                        return;
//                    }
//                }

//                first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IJournalEntryRetList JournalList;
//                ENResponseType responseType;
//                ENObjectType responseDetailType;
//                responseType = response.Type.GetValue();
//                responseDetailType = response.Detail.Type.GetValue();
//                if (responseType == ENResponseType.rtJournalEntryQueryRs & responseDetailType == ENObjectType.otJournalEntryRetList)
//                {
//                    save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    JournalList = response.Detail;
//                }
//                else
//                {
//                    bail, we do not have the responses we were expecting
//                    bDone = true;
//                    bError = true;
//                    return;
//                }

//                Parse the query response and add the Customers to the Customer list box
//                short count;
//                short index;
//                IJournalEntryRet JournalRet;
//                count = JournalList.Count;
//                short MAX_RETURNED;
//                MAX_RETURNED = 1 + JournalList.Count;
//                we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//                {
//                    bDone = true;
//                }
//                db.tblJournalEntry_Delete();
//                db.tblJrLineItem_Delete();
//                var journalRefNUm = default(string);
//                var journalTxnDate = default(string);
//                var journalTxnId = default(string);
//                var journalTxnNumber = default(string);
//                var loopTo = (short)(count - 1);
//                for (index = 0; index <= loopTo; index++)
//                {
//                    if (index == 31)
//                    {
//                        int k = 0;
//                    }
//                    skip this customer if this is a repeat from the last query
//                    JournalRet = JournalList.GetAt(index);
//                    (JournalRet Is Nothing) Or _
//                    (JournalRet.RefNumber Is Nothing) Or _
//                    if (JournalRet.TxnID is null)
//                {
//                    bDone = true;
//                    bError = true;
//                    return;
//                }
//                If Not(JournalRet.CurrencyRef Is Nothing) Then
//               journalCurrency = JournalRet.CurrencyRef.FullName.GetValue()
//                     End If
//                    string journalCurrency = string.Empty;
//                if (JournalRet.RefNumber is not null)
//                {
//                    journalRefNUm = JournalRet.RefNumber.GetValue();
//                }
//                if (journalRefNUm == "Setup 15")
//                {
//                    int s = 0;
//                }
//                if (JournalRet.TxnDate is not null)
//                {
//                    journalTxnDate = JournalRet.TxnDate.GetValue();
//                }
//                if (JournalRet.TxnID is not null)
//                {
//                    journalTxnId = JournalRet.TxnID.GetValue();
//                }
//                if (JournalRet.TxnNumber is not null)
//                {
//                    journalTxnNumber = JournalRet.TxnNumber.GetValue();
//                }


//                db.tblJournalEntry_Insert(journalTxnId, journalTxnNumber, journalTxnDate, journalRefNUm, journalCurrency, ClearAllControl.gblCompanyID.ToString());

//                only the first customerRet should be repeating and then
//                lets just check to make sure we do not have the customer
//                just in case another app changed a customer right between our
//                     declare varibale to retrive data

//                    string jrItemCrFullName;
//                string jrItemCrTxnId;
//                string jrItemCrMemo;
//                decimal jrItemCrAmount;
//                string jrItemDrFullName;
//                string jrItemDrTxnId;
//                string txnType;
//                string jrItemDrMemo;
//                decimal jrItemDrAmount;
//                IORJournalLine orJournalitem;
//                string jrnlEntityREf;
//                int j = 0;
//                var loopTo1 = JournalRet.ORJournalLineList.Count - 1;
//                for (j = 0; j <= loopTo1; j++)
//                {
//                    jrItemCrFullName = "NULL";
//                    jrItemDrFullName = "NULL";
//                    jrItemCrAmount = 0m;
//                    jrItemDrAmount = 0m;
//                    jrItemCrMemo = "NULL";
//                    jrItemDrMemo = "NULL";
//                    jrItemCrTxnId = "NULL";
//                    jrItemDrTxnId = "NULL";
//                    jrnlEntityREf = "NULL";
//                    orJournalitem = JournalRet.ORJournalLineList.GetAt(j);

//                    if (orJournalitem.JournalCreditLine is not null)
//                    {
//                        if (orJournalitem.JournalCreditLine.AccountRef.FullName is not null)
//                        {
//                            jrItemCrFullName = orJournalitem.JournalCreditLine.AccountRef.FullName.GetValue();
//                        }

//                        if (orJournalitem.JournalCreditLine.TxnLineID is not null)
//                        {
//                            jrItemCrTxnId = orJournalitem.JournalCreditLine.TxnLineID.GetValue();
//                        }

//                        if (orJournalitem.JournalCreditLine.Memo is not null)
//                        {
//                            jrItemCrMemo = orJournalitem.JournalCreditLine.Memo.GetValue();
//                        }

//                        if (orJournalitem.JournalCreditLine.Amount is not null)
//                        {
//                            jrItemCrAmount = orJournalitem.JournalCreditLine.Amount.GetValue();
//                        }
//                        if (orJournalitem.JournalCreditLine.EntityRef is not null)
//                        {
//                            jrnlEntityREf = orJournalitem.JournalCreditLine.EntityRef.FullName.GetValue();
//                        }


//                    }
//                    txnType = "CR";
//                    if (!(jrItemCrTxnId == "NULL"))
//                    {
//                        db.tblJrLineItem_Insert(journalTxnNumber, txnType, jrItemCrTxnId, jrItemCrFullName, jrItemCrMemo, jrItemCrAmount, jrnlEntityREf);
//                    }

//                    if (orJournalitem.JournalDebitLine is not null)
//                    {
//                        if (orJournalitem.JournalDebitLine.AccountRef is not null)
//                        {
//                            jrItemDrFullName = orJournalitem.JournalDebitLine.AccountRef.FullName.GetValue();
//                        }

//                        if (orJournalitem.JournalDebitLine.TxnLineID is not null)
//                        {
//                            jrItemDrTxnId = orJournalitem.JournalDebitLine.TxnLineID.GetValue();
//                        }

//                        if (orJournalitem.JournalDebitLine.Memo is not null)
//                        {
//                            jrItemDrMemo = orJournalitem.JournalDebitLine.Memo.GetValue();
//                        }

//                        if (orJournalitem.JournalDebitLine.Amount is not null)
//                        {
//                            jrItemDrAmount = orJournalitem.JournalDebitLine.Amount.GetValue();
//                        }
//                        if (orJournalitem.JournalDebitLine.EntityRef is not null)
//                        {
//                            jrnlEntityREf = orJournalitem.JournalDebitLine.EntityRef.FullName.GetValue();
//                        }
//                    }
//                    if (!(jrItemDrTxnId == "NULL"))
//                    {
//                        txnType = "DR";
//                        db.tblJrLineItem_Insert(journalTxnNumber, txnType, jrItemDrTxnId, jrItemDrFullName, jrItemDrMemo, jrItemDrAmount, jrnlEntityREf);
//                    }

//                }
//            }

//                return;
//        }
//            catch
//            {

//                Interaction.MsgBox("HRESULT = :" + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//                bDone = true;
//                bError = true;
//            }
//}
//public static void billcheckTransaction(ref bool bError)
//{
//    On Error GoTo Errs
//             make sure we do not have any old requests still defined
//            msgSetRequest.ClearRequests();
//    set the OnError attribute to continueOnError
//            msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//    Add the BillQuery request
//            IBillPaymentCheckQuery checkBillQuery;
//    checkBillQuery = msgSetRequest.AppendBillPaymentCheckQueryRq;
//    checkBillQuery.IncludeLineItems.SetValue(true);
//    DateTime billTxnDate;
//    billTxnDate = DateAndTime.DateAdd(DateInterval.Day, -30, DateTime.Now);
//    'checkBillQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(billTxnDate)
//             'checkBillQuery.IncludeLinkedTxns.SetValue(True)
//            bool cBone = false;
//    while (!cBone)
//    {
//        IMsgSetResponse msgBillSetResponse;
//        msgBillSetResponse = qbSessionManager.DoRequests(msgSetRequest);
//        fillBillCheckINDB(ref msgBillSetResponse, ref cBone, ref bError);
//    }

//    return;

//Errs:
//    ;

//    Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//    bError = true;
//}
//public static void billcheckTransaction(ref bool bError, DateTime fromDate, DateTime toDate)
//{
//    On Error GoTo Errs
//             make sure we do not have any old requests still defined
//            msgSetRequest.ClearRequests();
//    set the OnError attribute to continueOnError
//            msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//    Add the BillQuery request
//            IBillPaymentCheckQuery checkBillQuery;
//    checkBillQuery = msgSetRequest.AppendBillPaymentCheckQueryRq;
//    checkBillQuery.IncludeLineItems.SetValue(true);
//    DateTime billTxnDate;
//    billTxnDate = DateAndTime.DateAdd(DateInterval.Day, -30, DateTime.Now);
//    checkBillQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
//    checkBillQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(toDate);
//    'checkBillQuery.IncludeLinkedTxns.SetValue(True)
//            bool cBone = false;
//    while (!cBone)
//    {
//        IMsgSetResponse msgBillSetResponse;
//        msgBillSetResponse = qbSessionManager.DoRequests(msgSetRequest);
//        fillBillCheckINDB(ref msgBillSetResponse, ref cBone, ref bError);
//    }

//    return;

//Errs:
//    ;

//    Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//    bError = true;
//}


//get all check payment for expense account

//        public static void GetCheckTransaction(ref bool bError)
//{
//    On Error GoTo Errs
//             make sure we do not have any old requests still defined
//            msgSetRequest.ClearRequests();
//    set the OnError attribute to continueOnError
//            msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//    Add the BillQuery request
//            ICheckQuery CheckQuery;
//    DateTime filterDate;
//    filterDate = DateAndTime.DateAdd(DateInterval.Day, -7, DateTime.Now);

//    CheckQuery = msgSetRequest.AppendCheckQueryRq;
//    CheckQuery.IncludeLinkedTxns.SetValue(true);
//    'CheckQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(filterDate)
//            CheckQuery.IncludeLineItems.SetValue(true);


//    CheckQuery.IncludeLinkedTxns.SetValue(True)


//             CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//            bool bDone = false;
//    while (!bDone)
//    {
//        start looking for customer next in the list


//        send the request to QB

//       IMsgSetResponse msgSetResponse;
//        msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//        MsgBox(msgSetRequest.ToXMLString())
//                FillExpenseAccountInDataBase(ref msgSetResponse, ref bDone, ref bError);
//    }
//    return;

//Errs:
//    ;

//    Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//    bError = true;
//}


//public static void GetCheckTransaction(ref bool bError, DateTime fromDate, DateTime toDate)
//{
//    On Error GoTo Errs
//             make sure we do not have any old requests still defined
//            msgSetRequest.ClearRequests();
//    set the OnError attribute to continueOnError
//            msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//    Add the BillQuery request
//            ICheckQuery CheckQuery;
//    DateTime filterDate;
//    filterDate = DateAndTime.DateAdd(DateInterval.Day, -7, DateTime.Now);

//    CheckQuery = msgSetRequest.AppendCheckQueryRq;
//    CheckQuery.IncludeLinkedTxns.SetValue(True)
//            CheckQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
//    CheckQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(toDate);
//    CheckQuery.IncludeLineItems.SetValue(true);


//    CheckQuery.IncludeLinkedTxns.SetValue(True)


//             CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//            bool bDone = false;
//    while (!bDone)
//    {
//        start looking for customer next in the list


//        send the request to QB

//       IMsgSetResponse msgSetResponse;
//        msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//        MsgBox(msgSetRequest.ToXMLString())
//                FillExpenseAccountInDataBase(ref msgSetResponse, ref bDone, ref bError);
//    }
//    return;

//Errs:
//    ;

//    Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//    bError = true;
//}

//public static void fillBillCheckINDB(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//    {
//        bDone = true;
//        bError = true;
//        return;
//    }


//    Start parsing the response list
//   IResponseList responseList;
//    responseList = msgSetResponse.ResponseList;

//    go thru each response and process the response.
//             this example will only have one response in the list
//             so we will look at index = 0
//            IResponse response;
//    response = responseList.GetAt(0);
//    if (response is not null)
//    {
//        if (response.StatusCode != "0")
//        {
//            If the status is bad, report it to the user
//                    Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
//            bDone = true;
//            bError = true;
//            return;
//        }
//    }

//    if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//    {
//        bDone = true;
//        bError = true;
//        return;
//    }


//    IBillPaymentCheckRetList checkBillRetList;
//    ENResponseType responseType;
//    ENObjectType responseDetailType;
//    responseType = response.Type.GetValue();
//    responseDetailType = response.Detail.Type.GetValue();
//    if (responseType == ENResponseType.rtBillPaymentCheckQueryRs & responseDetailType == ENObjectType.otBillPaymentCheckRetList)
//    {
//        save the response detail in the appropriate object type
//                 since we have first verified the type of the response object
//                checkBillRetList = response.Detail;
//    }
//    else
//    {
//        bail, we do not have the responses we were expecting
//                bDone = true;
//        bError = true;
//        return;
//    }

//    Parse the query response and add the Customers to the Customer list box
//            short count;
//    short index;
//    IBillPaymentCheckRet CheckBillRet;
//    count = checkBillRetList.Count;
//    short MAX_RETURNED;
//    MAX_RETURNED = 1 + checkBillRetList.Count;
//    we are done with the customerQueries if we have not received the MaxReturned
//            if (count < MAX_RETURNED)
//    {
//        bDone = true;
//    }
//    db.tblQBBillPayCheck_Delete();
//    db.tblQBBillPayCheckLInes_Delete();
//    var billLineAmount = default(decimal);
//    var balanceRemaining = default(decimal);
//    var discountAmount = default(decimal);
//    var BillLineTxnDate = default(DateTime);
//    var loopTo = (short)(count - 1);
//    for (index = 0; index <= loopTo; index++)
//    {

//        skip this customer if this is a repeat from the last query
//                CheckBillRet = checkBillRetList.GetAt(index);
//        if (CheckBillRet is null | CheckBillRet.APAccountRef is null | CheckBillRet.TxnID is null)

//        {
//            bDone = true;
//            bError = true;
//            return;
//        }
//        only the first customerRet should be repeating and then
//        lets just check to make sure we do not have the customer
//        just in case another app changed a customer right between our

//                 declare varibale to retrive data
//                string Amount = string.Empty;
//        if (CheckBillRet.Amount is not null)
//        {
//            Amount = CheckBillRet.Amount.GetValue;
//        }
//        string APName = null;
//        if (CheckBillRet.APAccountRef is not null)
//        {
//            APName = CheckBillRet.APAccountRef.FullName.GetValue;
//        }
//        string bankName = null;
//        if (CheckBillRet.BankAccountRef is not null)
//        {
//            bankName = CheckBillRet.BankAccountRef.FullName.GetValue;
//        }
//        string memo = null;
//        if (CheckBillRet.Memo is not null)
//        {
//            memo = CheckBillRet.Memo.GetValue;
//        }
//        string payeeFullName = null;
//        if (CheckBillRet.PayeeEntityRef is not null)
//        {
//            payeeFullName = CheckBillRet.PayeeEntityRef.FullName.GetValue;
//        }

//        string refNumber = null;
//        if (CheckBillRet.RefNumber is not null)
//        {
//            refNumber = CheckBillRet.RefNumber.GetValue;
//        }
//        DateTime txnDate = default;
//        if (CheckBillRet.TxnDate is not null)
//        {
//            txnDate = CheckBillRet.TxnDate.GetValue;
//        }
//        string txnNUmber = null;
//        if (CheckBillRet.TxnNumber is not null)
//        {
//            txnNUmber = CheckBillRet.TxnNumber.GetValue;
//        }
//        string type = null;
//        if (CheckBillRet.Type is not null)
//        {
//            type = CheckBillRet.Type.GetAsString;
//        }

//        db.tblQBBillPayCheck_Insert(txnNUmber, type, txnDate, refNumber, payeeFullName, memo, bankName, APName, Amount, ClearAllControl.gblCompanyID);

//        IAppliedToTxnRet appliedTxnRet;
//        if (CheckBillRet.AppliedToTxnRetList is not null)
//        {
//            for (int j = 0, loopTo1 = CheckBillRet.AppliedToTxnRetList.Count - 1; j <= loopTo1; j++)
//            {
//                appliedTxnRet = CheckBillRet.AppliedToTxnRetList.GetAt(j);
//                if (appliedTxnRet.Amount is not null)
//                {
//                    billLineAmount = appliedTxnRet.Amount.GetValue;
//                }
//                if (appliedTxnRet.BalanceRemaining is not null)
//                {
//                    balanceRemaining = appliedTxnRet.BalanceRemaining.GetValue;
//                }
//                if (appliedTxnRet.DiscountAmount is not null)
//                {
//                    discountAmount = appliedTxnRet.DiscountAmount.GetValue;
//                }

//                string billRefNumber = null;
//                if (appliedTxnRet.RefNumber is not null)
//                {
//                    billRefNumber = appliedTxnRet.RefNumber.GetValue;
//                }
//                if (appliedTxnRet.TxnDate is not null)
//                {
//                    BillLineTxnDate = appliedTxnRet.TxnDate.GetValue;
//                }
//                string billLineTxnId = null;
//                if (appliedTxnRet.TxnID is not null)
//                {
//                    billLineTxnId = appliedTxnRet.TxnID.GetValue;
//                }
//                string BillLineTxnType = null;
//                if (appliedTxnRet.TxnType is not null)
//                {
//                    BillLineTxnType = appliedTxnRet.TxnType.GetValue;
//                }
//                string billLineType = null;
//                if (appliedTxnRet.Type is not null)
//                {
//                    billLineType = appliedTxnRet.Type.GetValue;
//                }
//                db.tblQBBillPayCheckLInes_Insert(txnNUmber, billLineTxnId, BillLineTxnDate, billLineType, BillLineTxnType, billRefNumber, discountAmount, balanceRemaining, billLineAmount);
//            }
//        }
//    }
//}
//public static void FillExpenseAccountInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    try
//    {

//        check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        Start parsing the response list
//       IResponseList responseList;
//        responseList = msgSetResponse.ResponseList;

//        go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//        response = responseList.GetAt(0);
//        if (response is not null)
//        {
//            if (response.StatusCode != "0")
//            {
//                If the status is bad, report it to the user
//                        Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
//                bDone = true;
//                bError = true;
//                return;
//            }
//        }

//        first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                ICheckRetList checkRetList;
//        ENResponseType responseType;
//        ENObjectType responseDetailType;
//        responseType = response.Type.GetValue();
//        responseDetailType = response.Detail.Type.GetValue();
//        if (responseType == ENResponseType.rtCheckQueryRs & responseDetailType == ENObjectType.otCheckRetList)
//        {
//            save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    checkRetList = response.Detail;
//        }
//        else
//        {
//            bail, we do not have the responses we were expecting
//                    bDone = true;
//            bError = true;
//            return;
//        }

//        Parse the query response and add the Customers to the Customer list box
//                short count;
//        short index;
//        ICheckRet CheckRet;
//        count = checkRetList.Count;
//        short MAX_RETURNED;
//        MAX_RETURNED = 1 + checkRetList.Count;
//        we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//        {
//            bDone = true;
//        }
//        db.tblQbCheck_Delete();
//        db.tblQbCheckExpense_Delete();

//        var amount = default(decimal);
//        var amountInHomeCurrency = default(decimal);
//        var LineAmount = default(decimal);
//        var itemAmount = default(decimal);
//        var itemLineCustomerName = default(string);
//        var qty = default(int);
//        var loopTo = (short)(count - 1);
//        for (index = 0; index <= loopTo; index++)
//        {

//            skip this customer if this is a repeat from the last query
//                    CheckRet = checkRetList.GetAt(index);
//            if (CheckRet is null | CheckRet.AccountRef.ListID is null | CheckRet.TxnID is null)

//            {
//                bDone = true;
//                bError = true;
//                return;
//            }
//            only the first customerRet should be repeating and then
//            lets just check to make sure we do not have the customer
//            just in case another app changed a customer right between our

//                     declare varibale to retrive data
//                    string accountRef = string.Empty;
//            if (CheckRet.AccountRef is not null)
//            {
//                accountRef = CheckRet.AccountRef.ListID.GetValue;
//            }

//            string address = string.Empty;
//            If Not(CheckRet.Address.Addr1 Is Nothing) Then
//           address = CheckRet.Address.Addr1.GetValue
//                     End If
//                    string addressBlock = string.Empty;
//            if (CheckRet.AccountRef is not null)
//            {
//                addressBlock = CheckRet.AccountRef.FullName.GetValue;
//            }
//            'D:\SOURCECODE\QB Itegration\CHECK MANAGMENT\CHECKMANAGEMENT_PV - Al meera\CHECKMGMT\My Project\licenses.licx
//                    if (CheckRet.Amount is not null)
//            {
//                amount = CheckRet.Amount.GetValue;
//            }
//            if (CheckRet.AmountInHomeCurrency is not null)
//            {
//                amountInHomeCurrency = CheckRet.AmountInHomeCurrency.GetValue;
//            }
//            string currencyRef = string.Empty;
//            If Not(CheckRet.CurrencyRef.ListID Is Nothing) Then
//           currencyRef = CheckRet.CurrencyRef.ListID.GetValue
//                     End If
//                     Dim dataExtRetList As String = String.Empty
//                     If Not(CheckRet.DataExtRetList Is Nothing) Then
//                    dataExtRetList = CheckRet.DataExtRetList.
//                     End If
//                    string editSequence = string.Empty;
//            if (CheckRet.EditSequence is not null)
//            {
//                editSequence = CheckRet.EditSequence.GetValue;
//            }
//            string exchangeRate = string.Empty;
//            if (CheckRet.ExchangeRate is not null)
//            {
//                exchangeRate = CheckRet.ExchangeRate.GetValue;
//            }
//            string externalGuid = string.Empty;
//            if (CheckRet.ExternalGUID is not null)
//            {
//                externalGuid = CheckRet.ExternalGUID.GetValue;
//            }
//            string isTaxIncluded = string.Empty;
//            if (CheckRet.IsTaxIncluded is not null)
//            {
//                isTaxIncluded = CheckRet.IsTaxIncluded.GetValue;

//            }
//            string isToBePrinted = string.Empty;
//            if (CheckRet.IsToBePrinted is not null)
//            {
//                isToBePrinted = CheckRet.IsToBePrinted.GetValue;

//            }
//            Dim linkedTxnList As String = String.Empty
//                     If Not(CheckRet.LinkedTxnList Is Nothing) Then

//                    End If
//                    string memo = string.Empty;
//            if (CheckRet.Memo is not null)
//            {
//                memo = CheckRet.Memo.GetValue;
//            }
//            Dim orItemLineRetList As String = String.Empty
//                     If Not(CheckRet.ORItemLineRetList.Is Nothing) Then

//                   End If
//                    string payEntityRef = string.Empty;
//            string vendorListID = string.Empty;
//            if (CheckRet.PayeeEntityRef is not null)
//            {
//                payEntityRef = CheckRet.PayeeEntityRef.FullName.GetValue;
//                vendorListID = CheckRet.PayeeEntityRef.ListID.GetValue;
//            }
//            string refNumber = string.Empty;
//            if (CheckRet.RefNumber is not null)
//            {
//                refNumber = CheckRet.RefNumber.GetValue;
//            }
//            if (refNumber == "4545")
//            {
//                int g = 0;
//            }
//            string salesTaxCodeRef = string.Empty;
//            if (CheckRet.SalesTaxCodeRef is not null)
//            {
//                salesTaxCodeRef = CheckRet.SalesTaxCodeRef.ListID.GetValue;
//            }
//            string timeCreated = CheckRet.TimeCreated.GetValue;
//            string timeModified = string.Empty;
//            if (CheckRet.TimeModified is not null)
//            {
//                timeModified = CheckRet.TimeModified.GetValue;
//            }
//            DateTime txnDate = default;
//            if (CheckRet.TxnDate is not null)
//            {
//                txnDate = CheckRet.TxnDate.GetValue;
//            }
//            string txnID = CheckRet.TxnID.GetValue();
//            string txnNumber = string.Empty;
//            if (CheckRet.TxnNumber is not null)
//            {
//                txnNumber = CheckRet.TxnNumber.GetValue;
//            }

//            string type = string.Empty;
//            if (CheckRet.Type is not null)
//            {
//                type = CheckRet.Type.GetValue;
//            }
//            'Dim billcount = CheckRet.LinkedTxnList.Count

//                     ' If (Not FoundExpenseInDatabase(CheckRet.TxnID.GetValue())) Then
//                     Insert data in database code left..........db.tblQbCheck_Insert(ClearAllControl.gblCompanyID, accountRef, address, addressBlock, amount, amountInHomeCurrency, currencyRef, editSequence, exchangeRate, externalGuid, isTaxIncluded, isToBePrinted, memo, payEntityRef, refNumber, salesTaxCodeRef, timeCreated, timeModified, txnDate, txnID, txnNumber, type);

//            int j = 0;
//            IExpenseLineRet ExpenseLineRet;
//            if (CheckRet.ExpenseLineRetList is not null)
//            {
//                var loopTo1 = CheckRet.ExpenseLineRetList.Count - 1;
//                for (j = 0; j <= loopTo1; j++)
//                {
//                    ExpenseLineRet = CheckRet.ExpenseLineRetList.GetAt(j);
//                    string LineAccountRef = string.Empty;
//                    if (ExpenseLineRet.AccountRef is not null)
//                    {
//                        LineAccountRef = ExpenseLineRet.AccountRef.ListID.GetValue();
//                    }
//                    string LineFullName = string.Empty;
//                    if (ExpenseLineRet.AccountRef is not null)
//                    {
//                        LineFullName = ExpenseLineRet.AccountRef.FullName.GetValue();
//                    }
//                    if (ExpenseLineRet.Amount is not null)
//                    {
//                        LineAmount = ExpenseLineRet.Amount.GetValue();
//                    }
//                    string LineBillableStatus = string.Empty;
//                    if (ExpenseLineRet.BillableStatus is not null)
//                    {
//                        LineBillableStatus = ExpenseLineRet.BillableStatus.GetValue();
//                    }
//                    string LineClassListID = string.Empty;
//                    if (ExpenseLineRet.ClassRef is not null)
//                    {
//                        LineClassListID = ExpenseLineRet.ClassRef.ListID.GetValue;
//                    }
//                    string LineClassFullName = string.Empty;
//                    if (ExpenseLineRet.ClassRef is not null)
//                    {
//                        LineClassFullName = ExpenseLineRet.ClassRef.FullName.GetValue;
//                    }
//                    string LineCustomerRefListId = string.Empty;
//                    if (ExpenseLineRet.CustomerRef is not null)
//                    {
//                        LineCustomerRefListId = ExpenseLineRet.CustomerRef.ListID.GetValue;
//                    }
//                    string LineCustomerRefFullName = string.Empty;
//                    if (ExpenseLineRet.CustomerRef is not null)
//                    {
//                        LineCustomerRefFullName = ExpenseLineRet.CustomerRef.FullName.GetValue;
//                    }
//                    string LineMemo = string.Empty;
//                    if (ExpenseLineRet.Memo is not null)
//                    {
//                        LineMemo = ExpenseLineRet.Memo.GetValue();
//                    }
//                    string LineTxnLineID = string.Empty;
//                    if (ExpenseLineRet.TxnLineID is not null)
//                    {
//                        LineTxnLineID = ExpenseLineRet.TxnLineID.GetValue();
//                    }
//                    string LineType = string.Empty;
//                    if (ExpenseLineRet.Type is not null)
//                    {
//                        LineType = ExpenseLineRet.Type.GetValue();
//                    }
//                    db.tblQbCheckExpense_Insert(txnID, LineAccountRef, LineFullName, LineAmount, LineBillableStatus, LineClassListID, LineClassFullName, LineCustomerRefListId, LineCustomerRefFullName, LineMemo, LineTxnLineID, LineType, ClearAllControl.gblCompanyID);
//                }


//                int k = 0;

//                IORItemLineRet itemLineRet;
//                if (CheckRet.ORItemLineRetList is not null)
//                {
//                    var loopTo2 = CheckRet.ORItemLineRetList.Count - 1;
//                    for (k = 0; k <= loopTo2; k++)
//                    {
//                        itemLineRet = CheckRet.ORItemLineRetList.GetAt(k);
//                        if (itemLineRet.ItemLineRet.Amount is not null)
//                        {
//                            itemAmount = itemLineRet.ItemLineRet.Amount.GetValue();
//                        }
//                        if (itemLineRet.ItemLineRet.CustomerRef is not null)
//                        {
//                            itemLineCustomerName = itemLineRet.ItemLineRet.CustomerRef.FullName.GetValue();
//                        }
//                        string itemLineDesc = string.Empty;
//                        if (itemLineRet.ItemLineRet.Desc is not null)
//                        {
//                            itemLineDesc = itemLineRet.ItemLineRet.Desc.GetValue();
//                        }
//                        string itemLineFullName = string.Empty;
//                        if (itemLineRet.ItemLineRet.ItemRef is not null)
//                        {
//                            itemLineFullName = itemLineRet.ItemLineRet.ItemRef.FullName.GetValue();
//                        }
//                        string itemLineTxnId = string.Empty;
//                        if (itemLineRet.ItemLineRet.TxnLineID is not null)
//                        {
//                            itemLineTxnId = itemLineRet.ItemLineRet.TxnLineID.GetValue();
//                        }
//                        double itemLineCost = 0d;
//                        if (itemLineRet.ItemLineRet.Cost is not null)
//                        {
//                            itemLineCost = itemLineRet.ItemLineRet.Cost.GetValue();
//                        }
//                        if (itemLineRet.ItemLineRet.Quantity is not null)
//                        {
//                            qty = itemLineRet.ItemLineRet.Quantity.GetValue;
//                        }
//                        db.tblQbCheckItemLine_Insert(txnID, itemLineDesc, itemLineFullName, itemLineCustomerName, itemAmount, itemLineTxnId, (decimal?)itemLineCost, qty);
//                    }
//                    'End If

//                        }



//                Else
//                         'update command for check
//                         db.tblQbCheck_Update(gblCompanyID, accountRef, address, addressBlock, amount, amountInHomeCurrency, currencyRef, editSequence, exchangeRate, externalGuid, isTaxIncluded, isToBePrinted, memo, payEntityRef, refNumber, salesTaxCodeRef, timeCreated, timeModified, txnDate, txnID, txnNumber, type)
//                         db.tblQbCheckExpense_Delete()
//                         Dim j = 0
//                         Dim ExpenseLineRet As IExpenseLineRet
//                         If Not CheckRet.ExpenseLineRetList Is Nothing Then
//                         For j = 0 To CheckRet.ExpenseLineRetList.Count - 1
//                         ExpenseLineRet = CheckRet.ExpenseLineRetList.GetAt(j)
//                         Dim LineAccountRef As String = String.Empty
//                         If Not(ExpenseLineRet.AccountRef Is Nothing) Then
//                        LineAccountRef = ExpenseLineRet.AccountRef.ListID.GetValue()
//                         End If
//                         Dim LineFullName As String = String.Empty
//                         If Not(ExpenseLineRet.AccountRef Is Nothing) Then
//                        LineFullName = ExpenseLineRet.AccountRef.FullName.GetValue()
//                         End If
//                         Dim LineAmount As Decimal
//                         If Not(ExpenseLineRet.Amount Is Nothing) Then
//                        LineAmount = ExpenseLineRet.Amount.GetValue()
//                         End If
//                         Dim LineBillableStatus As String = String.Empty
//                         If Not(ExpenseLineRet.BillableStatus Is Nothing) Then
//                        LineBillableStatus = ExpenseLineRet.BillableStatus.GetValue()
//                         End If
//                         Dim LineClassListID As String = String.Empty
//                         If Not(ExpenseLineRet.ClassRef Is Nothing) Then
//                        LineClassListID = ExpenseLineRet.ClassRef.ListID.GetValue
//                         End If
//                         Dim LineClassFullName As String = String.Empty
//                         If Not(ExpenseLineRet.ClassRef Is Nothing) Then
//                        LineClassFullName = ExpenseLineRet.ClassRef.FullName.GetValue
//                         End If
//                         Dim LineCustomerRefListId As String = String.Empty
//                         If Not(ExpenseLineRet.CustomerRef Is Nothing) Then
//                        LineCustomerRefListId = ExpenseLineRet.CustomerRef.ListID.GetValue
//                         End If
//                         Dim LineCustomerRefFullName As String = String.Empty
//                         If Not(ExpenseLineRet.CustomerRef Is Nothing) Then
//                        LineCustomerRefFullName = ExpenseLineRet.CustomerRef.FullName.GetValue
//                         End If
//                         Dim LineMemo As String = String.Empty
//                         If Not(ExpenseLineRet.Memo Is Nothing) Then
//                        LineMemo = ExpenseLineRet.Memo.GetValue()
//                         End If
//                         Dim LineTxnLineID As String = String.Empty
//                         If Not(ExpenseLineRet.TxnLineID Is Nothing) Then
//                        LineTxnLineID = ExpenseLineRet.TxnLineID.GetValue()
//                         End If
//                         Dim LineType As String = String.Empty
//                         If Not(ExpenseLineRet.Type Is Nothing) Then
//                        LineType = ExpenseLineRet.Type.GetValue()
//                         End If
//                         db.tblQbCheckExpense_Insert(txnID, LineAccountRef, LineFullName, LineAmount, LineBillableStatus, LineClassListID, LineClassFullName, LineCustomerRefListId, LineCustomerRefFullName, LineMemo, LineTxnLineID, LineType, gblCompanyID)
//                         Next
//                         End If
//                         db.tblQbCheckItemLine_Delete()
//                         Dim k = 0

//                         Dim itemLineRet As IORItemLineRet
//                         If Not CheckRet.ORItemLineRetList Is Nothing Then
//                         For k = 0 To CheckRet.ORItemLineRetList.Count - 1
//                         itemLineRet = CheckRet.ORItemLineRetList.GetAt(k)

//                         Dim itemAmount As Decimal
//                         If Not(itemLineRet.ItemLineRet.Amount Is Nothing) Then
//                        itemAmount = itemLineRet.ItemLineRet.Amount.GetValue()
//                         End If
//                         Dim itemLineCustomerName As String
//                         If Not(itemLineRet.ItemLineRet.CustomerRef Is Nothing) Then
//                        itemLineCustomerName = itemLineRet.ItemLineRet.CustomerRef.FullName.GetValue()
//                         End If
//                         Dim itemLineDesc As String = String.Empty
//                         If Not(itemLineRet.ItemLineRet.Desc Is Nothing) Then
//                        itemLineDesc = itemLineRet.ItemLineRet.Desc.GetValue()
//                         End If
//                         Dim itemLineFullName As String = String.Empty
//                         If Not(itemLineRet.ItemLineRet.ItemRef Is Nothing) Then
//                        itemLineFullName = itemLineRet.ItemLineRet.ItemRef.FullName.GetValue()
//                         End If
//                         Dim itemLineTxnId As String = String.Empty
//                         If Not(itemLineRet.ItemLineRet.TxnLineID Is Nothing) Then
//                        itemLineTxnId = itemLineRet.ItemLineRet.TxnLineID.GetValue()
//                         End If
//                         Dim itemLineCost As Double = 0
//                         If Not(itemLineRet.ItemLineRet.Cost Is Nothing) Then
//                        itemLineCost = itemLineRet.ItemLineRet.Cost.GetValue()
//                         End If
//                         Dim qty As Integer
//                         If Not(itemLineRet.ItemLineRet.Quantity Is Nothing) Then
//                        qty = itemLineRet.ItemLineRet.Quantity.GetValue
//                         End If
//                         db.tblQbCheckItemLine_Insert(txnID, itemLineDesc, itemLineFullName, itemLineCustomerName, itemAmount, itemLineTxnId, itemLineCost, qty)
//                         Next
//                         End If


//                    }

//        }

//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = :" + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//        bDone = true;
//        bError = true;
//    }
//}

//private static bool FoundExpenseInDatabase(ref string txnID)
//{
//    bool FoundExpenseInDatabaseRet = default;
//    try
//    {

//        FoundExpenseInDatabaseRet = false;

//        go thru our list box and find the item which was modified
//                short i;
//        short numCustomers;
//        check in database for existing customer bill

//       var result = db.tblQBCheck_Select(ClearAllControl.gblCompanyID, null);
//        int resultCount = result.Count();

//        if (!(resultCount == 0))
//        {
//            FoundExpenseInDatabaseRet = true;
//        }

//        return FoundExpenseInDatabaseRet;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FoundIn Account database");

//    }

//}

//get all recepit data
//        public static void getRecivePayment(ref bool bError)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                IReceivePaymentQuery receivePayment;

//        receivePayment = msgSetRequest.AppendReceivePaymentQueryRq;
//        receivePayment.IncludeLineItems.SetValue(true);
//        DateTime txnReptDate;
//        ' txnReptDate = DateAdd(DateInterval.Day, -150, Date.Now)
//                 'receivePayment.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(txnReptDate)

//                 RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll)
//                 ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillReceivePaymentInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }

//}
//public static void getRecivePayment(ref bool bError, DateTime fromDate, DateTime toDate)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                IReceivePaymentQuery receivePayment;

//        receivePayment = msgSetRequest.AppendReceivePaymentQueryRq;
//        DateTime txnReptDate;
//        ' txnReptDate = DateAdd(DateInterval.Day, -150, Date.Now)
//                receivePayment.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
//        receivePayment.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(toDate);
//        receivePayment.IncludeLineItems.SetValue(true);
//        RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll)
//                 ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillReceivePaymentInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }

//}
//public static void GetSOTransaction(ref bool bError, DateTime fromDate, DateTime todate)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                IPurchaseOrderQuery purchasequery;

//        ISalesOrderQuery RecepitQuery;
//        RecepitQuery = msgSetRequest.AppendSalesOrderQueryRq;
//        RecepitQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
//        RecepitQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
//        RecepitQuery.IncludeLineItems.SetValue(true);
//        ' RecepitQuery.IncludeLinkedTxns.SetValue(True)
//                 ' RecepitQuery.IncludeRetElementLis()
//                RecepitQuery.OwnerIDList.Add("0");

//        'RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll)
//                 ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillSOInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }
//}

//public static void GetSOTransactionNoDate(ref bool bError)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                ISalesOrderQuery RecepitQuery;
//        RecepitQuery = msgSetRequest.AppendSalesOrderQueryRq;
//        'RecepitQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate)
//                 'RecepitQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate)

//                RecepitQuery.IncludeLineItems.SetValue(true);
//        ' RecepitQuery.IncludeLinkedTxns.SetValue(True)
//                 ' RecepitQuery.IncludeRetElementLis()
//                RecepitQuery.OwnerIDList.Add("0");

//        'RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll)
//                 ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillSOInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }
//}

//public static void FillSOInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    'On Error GoTo Errs
//            try
//    {
//        string RefNumber = string.Empty;
//        check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)
//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        Start parsing the response list
//       IResponseList responseList;
//        responseList = msgSetResponse.ResponseList;

//        go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//        response = responseList.GetAt(0);
//        if (response is not null)
//        {
//            if (response.StatusCode != "0")
//            {
//                If the status is bad, report it to the user
//                        Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
//                bDone = true;
//                bError = true;
//                return;
//            }
//        }

//        first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)
//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                ISalesOrderRetList InvoiceRetList;
//        ENResponseType responseType;
//        ENObjectType responseDetailType;
//        responseType = response.Type.GetValue();
//        responseDetailType = response.Detail.Type.GetValue();
//        'If (responseType = ENResponseType.rtInvoiceQueryRs) And (responseDetailType = ENObjectType.otInvoiceRetList) Then
//                if (responseType == ENResponseType.rtSalesOrderQueryRs & responseDetailType == ENObjectType.otSalesOrderRetList)
//        {
//            save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    InvoiceRetList = response.Detail;
//        }
//        else
//        {
//            bail, we do not have the responses we were expecting
//                    bDone = true;
//            bError = true;
//            return;
//        }

//        Parse the query response and add the Customers to the Customer list box
//                short count;
//        short index;
//        ISalesOrderRet InvoiceRet;
//        count = InvoiceRetList.Count;
//        short MAX_RETURNED;
//        MAX_RETURNED = 1 + InvoiceRetList.Count;
//        we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//        {
//            bDone = true;
//        }
//        db.tblSO_Delete(ClearAllControl.gblCompanyID);
//        db.tblSOLine_Delete();
//        'db.tblInvoiceApplTxn_Delete()
//                var linkTxnDate = default(DateTime);
//        var loopTo = (short)(count - 1);
//        for (index = 0; index <= loopTo; index++)
//        {
//            skip this customer if this is a repeat from the last query
//                    InvoiceRet = InvoiceRetList.GetAt(index);
//            if (InvoiceRet is null | InvoiceRet.CustomerRef.ListID is null | InvoiceRet.TxnID is null)
//            {
//                bDone = true;
//                bError = true;
//                return;
//            }
//            if (index == 5)
//            {
//                index = index;
//            }

//            'declare varibale to retrive data 
//                    string BillAddress1 = string.Empty;
//            string BillAddress2 = string.Empty;
//            string BillAddress3 = string.Empty;
//            string BillAddress4 = string.Empty;
//            string BillAddress5 = string.Empty;
//            string BillAddressCity = string.Empty;
//            string BillAddressState = string.Empty;
//            string BillAddressPostalCode = string.Empty;
//            string BillAddressCountry = string.Empty;
//            string BillAddressNote = string.Empty;


//            string TxnID = InvoiceRet.TxnID.GetValue;
//            string CustomerRefKey = string.Empty;
//            if (InvoiceRet.CustomerRef is not null)
//            {
//                CustomerRefKey = InvoiceRet.CustomerRef.ListID.GetValue;
//            }
//            string customerName = string.Empty;
//            if (InvoiceRet.CustomerRef is not null)
//            {
//                customerName = InvoiceRet.CustomerRef.FullName.GetValue;
//            }


//            string currencyRef = string.Empty;
//            If Not(InvoiceRet.CurrencyRef Is Nothing) Then
//           currencyRef = InvoiceRet.CurrencyRef.FullName.GetValue
//                     End If

//                    string ExchangeRAte = string.Empty;
//            If Not(InvoiceRet.ExchangeRate Is Nothing) Then
//           ExchangeRAte = InvoiceRet.ExchangeRate.GetValue
//                     End If



//                    string ClassRefKey = string.Empty;
//            if (InvoiceRet.ClassRef is not null)
//            {
//                ClassRefKey = InvoiceRet.ClassRef.FullName.GetValue;
//            }

//            string ARAccountRefKey = string.Empty;
//            If Not(InvoiceRet.ARAccountRef Is Nothing) Then
//           ARAccountRefKey = InvoiceRet.ARAccountRef.FullName.GetValue

//                     End If
//                    string TemplateRefKey = string.Empty;
//            if (InvoiceRet.TemplateRef is not null)
//            {
//                TemplateRefKey = InvoiceRet.TemplateRef.ListID.GetValue;
//            }
//            DateTime TxnDate = InvoiceRet.TxnDate.GetValue;



//            if (InvoiceRet.RefNumber is not null)
//            {
//                RefNumber = InvoiceRet.RefNumber.GetValue;
//            }
//            if (RefNumber == "12" | RefNumber == "09613")
//            {
//                int m = 0;
//            }

//            Bill Address


//                    if (InvoiceRet.BillAddress is not null)
//            {


//                if (InvoiceRet.BillAddress.Addr1 is not null)
//                {
//                    BillAddress1 = InvoiceRet.BillAddress.Addr1.GetValue;
//                }


//                if (InvoiceRet.BillAddress.Addr2 is not null)
//                {
//                    BillAddress2 = InvoiceRet.BillAddress.Addr2.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr3 is not null)
//                {
//                    BillAddress3 = InvoiceRet.BillAddress.Addr3.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr4 is not null)
//                {
//                    BillAddress4 = InvoiceRet.BillAddress.Addr4.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr5 is not null)
//                {
//                    BillAddress5 = InvoiceRet.BillAddress.Addr5.GetValue;
//                }

//                if (InvoiceRet.BillAddress.City is not null)
//                {
//                    BillAddressCity = InvoiceRet.BillAddress.City.GetValue;
//                }

//                if (InvoiceRet.BillAddress.State is not null)
//                {
//                    BillAddressState = InvoiceRet.BillAddress.State.GetValue;
//                }

//                if (InvoiceRet.BillAddress.PostalCode is not null)
//                {
//                    BillAddressPostalCode = InvoiceRet.BillAddress.PostalCode.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Country is not null)
//                {
//                    BillAddressCountry = InvoiceRet.BillAddress.Country.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Note is not null)
//                {
//                    BillAddressNote = InvoiceRet.BillAddress.Note.GetValue;
//                }
//            }
//            Ship address
//                    string ShipAddress1 = string.Empty;
//            string ShipAddress2 = string.Empty;
//            string ShipAddress3 = string.Empty;
//            string ShipAddress4 = string.Empty;
//            string ShipAddress5 = string.Empty;
//            string ShipAddressCity = string.Empty;
//            string ShipAddressState = string.Empty;
//            string ShipAddressPostalCode = string.Empty;
//            string ShipAddressCountry = string.Empty;
//            string ShipAddressNote = string.Empty;
//            if (InvoiceRet.ShipAddress is not null)
//            {

//                if (InvoiceRet.ShipAddress.Addr1 is not null)
//                {
//                    ShipAddress1 = InvoiceRet.ShipAddress.Addr1.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr2 is not null)
//                {
//                    ShipAddress2 = InvoiceRet.ShipAddress.Addr2.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr3 is not null)
//                {
//                    ShipAddress3 = InvoiceRet.ShipAddress.Addr3.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr4 is not null)
//                {
//                    ShipAddress4 = InvoiceRet.ShipAddress.Addr4.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr5 is not null)
//                {
//                    ShipAddress5 = InvoiceRet.ShipAddress.Addr5.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.City is not null)
//                {
//                    ShipAddressCity = InvoiceRet.ShipAddress.City.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.State is not null)
//                {
//                    ShipAddressState = InvoiceRet.ShipAddress.State.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.PostalCode is not null)
//                {
//                    ShipAddressPostalCode = InvoiceRet.ShipAddress.PostalCode.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Country is not null)
//                {
//                    ShipAddressCountry = InvoiceRet.ShipAddress.Country.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Note is not null)
//                {
//                    ShipAddressNote = InvoiceRet.ShipAddress.Note.GetValue;
//                }
//            }
//            End ship address

//                    string IsPending = string.Empty;
//            If Not(InvoiceRet.IsPending Is Nothing) Then
//           IsPending = InvoiceRet.IsPending.GetValue
//                     If IsPending = True Then
//                     MsgBox("stop here")
//                     End If
//                     End If
//                    string PONumber = string.Empty;
//            if (InvoiceRet.PONumber is not null)
//            {
//                PONumber = InvoiceRet.PONumber.GetValue;
//            }

//            string TermsRefKey = string.Empty;
//            if (InvoiceRet.TermsRef is not null)
//            {
//                TermsRefKey = InvoiceRet.TermsRef.FullName.GetValue;
//            }

//            string DueDate = string.Empty;
//            if (InvoiceRet.DueDate is not null)
//            {
//                DueDate = InvoiceRet.DueDate.GetValue;
//            }

//            string SalesRefKey = string.Empty;
//            if (InvoiceRet.SalesRepRef is not null)
//            {
//                SalesRefKey = InvoiceRet.SalesRepRef.FullName.GetValue;
//            }

//            string FOB = string.Empty;
//            if (InvoiceRet.FOB is not null)
//            {
//                FOB = InvoiceRet.FOB.GetValue;
//            }

//            string ShipDate = string.Empty;
//            if (InvoiceRet.ShipDate is not null)
//            {
//                ShipDate = InvoiceRet.ShipDate.GetValue;
//            }

//            string ShipMethodRefKey = string.Empty;
//            if (InvoiceRet.ShipMethodRef is not null)
//            {
//                ShipMethodRefKey = InvoiceRet.ShipMethodRef.ListID.GetValue;
//            }

//            string ItemSalesTaxRefKey = string.Empty;
//            if (InvoiceRet.ItemSalesTaxRef is not null)
//            {
//                ItemSalesTaxRefKey = InvoiceRet.ItemSalesTaxRef.ListID.GetValue;
//            }

//            string Memo = string.Empty;
//            if (InvoiceRet.Memo is not null)
//            {
//                Memo = InvoiceRet.Memo.GetValue;
//            }

//            string CustomerMsgRefKey = string.Empty;
//            if (InvoiceRet.CustomerMsgRef is not null)
//            {
//                CustomerMsgRefKey = InvoiceRet.CustomerMsgRef.ListID.GetValue;
//            }
//            string IsToBePrinted = string.Empty;
//            if (InvoiceRet.IsToBePrinted is not null)
//            {
//                IsToBePrinted = InvoiceRet.IsToBePrinted.GetValue;
//            }
//            string IsToEmailed = string.Empty;
//            if (InvoiceRet.IsToBeEmailed is not null)
//            {
//                IsToEmailed = InvoiceRet.IsToBeEmailed.GetValue;
//            }
//            string IsTaxIncluded = string.Empty;
//            if (InvoiceRet.IsTaxIncluded is not null)
//            {
//                IsTaxIncluded = InvoiceRet.IsTaxIncluded.GetValue;
//            }
//            string CustomerSalesTaxCodeRefKey = string.Empty;
//            if (InvoiceRet.CustomerSalesTaxCodeRef is not null)
//            {
//                CustomerSalesTaxCodeRefKey = InvoiceRet.CustomerSalesTaxCodeRef.ListID.GetValue;
//            }
//            string Other = string.Empty;
//            if (InvoiceRet.Other is not null)
//            {
//                Other = InvoiceRet.Other.GetValue;
//            }
//            string Amount = "0";
//            if (InvoiceRet.TotalAmount is not null)
//            {
//                Amount = InvoiceRet.TotalAmount.GetValue;
//            }
//            string AmountPaid = "0";
//            If Not(InvoiceRet.AppliedAmount Is Nothing) Then
//           AmountPaid = InvoiceRet.AppliedAmount.GetValue
//                     End If
//                    string CustomField1 = string.Empty;
//            string CustomField2 = string.Empty;
//            string CustomField3 = string.Empty;
//            string CustomField4 = string.Empty;
//            string CustomField5 = string.Empty;
//            string QuotationRecNo = string.Empty;
//            string GradeID = string.Empty;
//            string InvoiceType = string.Empty;
//            string subTotal = null;
//            string SalesTaxTotal = null;
//            string SalesTaxPercentage = null;
//            if (InvoiceRet.Subtotal is not null)
//            {
//                subTotal = InvoiceRet.Subtotal.GetValue;
//            }



//            if (InvoiceRet.DataExtRetList is not null)
//            {
//                IDataExtRet extInvLst;
//                string dataExtaname = string.Empty;
//                string dataExtValue = string.Empty;
//                for (int g = 0, loopTo1 = InvoiceRet.DataExtRetList.Count - 1; g <= loopTo1; g++)
//                {
//                    extInvLst = InvoiceRet.DataExtRetList.GetAt(g);
//                    dataExtaname = extInvLst.DataExtName.GetValue();
//                    dataExtValue = extInvLst.DataExtValue.GetValue();
//                    if (dataExtaname == "Customer ID")
//                    {
//                        CustomField1 = dataExtValue;
//                    }
//                    else if (dataExtaname == "Rep Name")
//                    {
//                        CustomField2 = dataExtValue;
//                    }
//                    else if (dataExtaname == "Sales Man Mob:")
//                    {
//                        CustomField3 = dataExtValue;
//                    }
//                    else if (dataExtaname == "Customer TRN")
//                    {
//                        CustomField4 = dataExtValue;
//                    }
//                }

//            }

//            if (InvoiceRet.SalesTaxTotal is not null)
//            {
//                SalesTaxTotal = InvoiceRet.SalesTaxTotal.GetValue;
//            }
//            if (InvoiceRet.SalesTaxPercentage is not null)
//            {
//                SalesTaxPercentage = InvoiceRet.SalesTaxPercentage.GetValue;
//            }
//            'Print(RefNumber)

//                     Insert data in database code left..........db.tblSO_Insert(ClearAllControl.gblCompanyID, TxnID, CustomerRefKey, null, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, Conversions.ToDate(DueDate), SalesRefKey, FOB, Conversions.ToDate(ShipDate), ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, Conversions.ToDecimal(Amount), Conversions.ToDecimal(AmountPaid), CustomField1, CustomField2, CustomField3, currencyRef, CustomField5, QuotationRecNo, ExchangeRAte, InvoiceType, customerName, Conversions.ToDecimal(subTotal), Conversions.ToDecimal(SalesTaxTotal), Conversions.ToDecimal(SalesTaxPercentage));
//            if (InvoiceRet.LinkedTxnList is not null)
//            {
//                for (int p = 0, loopTo2 = InvoiceRet.LinkedTxnList.Count - 1; p <= loopTo2; p++)
//                {
//                    ILinkedTxn invoiceLinkTxn;
//                    invoiceLinkTxn = InvoiceRet.LinkedTxnList.GetAt(p);
//                    string LinkRefNum = string.Empty;
//                    if (invoiceLinkTxn.RefNumber is not null)
//                    {
//                        LinkRefNum = invoiceLinkTxn.RefNumber.GetValue();
//                    }
//                    string linktXnAmount = string.Empty;
//                    if (invoiceLinkTxn.Amount is not null)
//                    {
//                        linktXnAmount = invoiceLinkTxn.Amount.GetValue();
//                    }
//                    if (invoiceLinkTxn.TxnDate is not null)
//                    {
//                        linkTxnDate = invoiceLinkTxn.TxnDate.GetValue;
//                    }
//                    db.tblInvoiceApplTxn_Insert(RefNumber, LinkRefNum, Conversions.ToString(linkTxnDate), linktXnAmount, TxnDate);
//                }
//            }


//            if (InvoiceRet.ORSalesOrderLineRetList is not null)
//            {
//                for (int k = 0, loopTo3 = InvoiceRet.ORSalesOrderLineRetList.Count - 1; k <= loopTo3; k++)
//                {
//                    IORSalesOrderLineRet InvoiceLineRetList;
//                    InvoiceLineRetList = InvoiceRet.ORSalesOrderLineRetList.GetAt(k);

//                    string lineQuantity = "0";
//                    if (InvoiceLineRetList.SalesOrderLineRet.Quantity is not null)
//                    {
//                        lineQuantity = InvoiceLineRetList.SalesOrderLineRet.Quantity.GetValue;
//                    }

//                    string lineAmount = "0";
//                    if (InvoiceLineRetList.SalesOrderLineRet.Amount is not null)
//                    {
//                        lineAmount = InvoiceLineRetList.SalesOrderLineRet.Amount.GetValue();
//                    }


//                    string lineDesc = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.Desc is not null)
//                    {
//                        lineDesc = InvoiceLineRetList.SalesOrderLineRet.Desc.GetValue();
//                    }
//                    string lineItem = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.ItemRef is not null)
//                    {
//                        lineItem = InvoiceLineRetList.SalesOrderLineRet.ItemRef.FullName.GetValue();
//                    }
//                    if (string.IsNullOrEmpty(lineItem))
//                    {
//                        goto caltchnullline;
//                    }
//                    string lineRate = "0";
//                    if (InvoiceLineRetList.SalesOrderLineRet.ORRate.Rate is not null)
//                    {
//                        lineRate = InvoiceLineRetList.SalesOrderLineRet.ORRate.Rate.GetValue();
//                    }
//                    string customefield = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.Other1 is not null)
//                    {
//                        customefield = InvoiceLineRetList.SalesOrderLineRet.Other1.GetValue();
//                    }
//                    string customeFeild2 = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.Other2 is not null)
//                    {
//                        customeFeild2 = InvoiceLineRetList.SalesOrderLineRet.Other2.GetValue();
//                    }
//                    string lineUOM = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.UnitOfMeasure is not null)
//                    {
//                        lineUOM = InvoiceLineRetList.SalesOrderLineRet.UnitOfMeasure.GetValue();
//                    }
//                    Dim lineQuantity As String = String.Empty
//                             If Not InvoiceLineRetList.InvoiceLineRet.Quantity Is Nothing Then
//                             lineQuantity = InvoiceLineRetList.InvoiceLineRet.Quantity.GetValue()
//                             End If
//                            string lineUOMOrg = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.OverrideUOMSetRef is not null)
//                    {
//                        lineUOMOrg = InvoiceLineRetList.SalesOrderLineRet.OverrideUOMSetRef.FullName.GetValue;
//                    }
//                    string lineTxnID = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.TxnLineID is not null)
//                    {
//                        lineTxnID = InvoiceLineRetList.SalesOrderLineRet.TxnLineID.GetValue();
//                    }
//                    string txaCode = string.Empty;
//                    if (InvoiceLineRetList.SalesOrderLineRet.SalesTaxCodeRef is not null)
//                    {
//                        if (InvoiceLineRetList.SalesOrderLineRet.SalesTaxCodeRef.FullName is not null)
//                        {
//                            txaCode = InvoiceLineRetList.SalesOrderLineRet.SalesTaxCodeRef.FullName.GetValue();
//                        }
//                    }

//                    double taxAmount = 0d;
//                    if (InvoiceLineRetList.SalesOrderLineRet.TaxAmount is not null)
//                    {
//                        taxAmount = InvoiceLineRetList.SalesOrderLineRet.TaxAmount.GetValue();
//                    }


//                    db.tblSOLine_Insert(TxnID, Conversions.ToDecimal(lineAmount), lineDesc, lineItem, lineRate, customefield, customeFeild2, lineUOM, Conversions.ToDecimal(lineQuantity), lineUOMOrg, lineTxnID, txaCode, (decimal?)taxAmount);
//                caltchnullline:
//                    ;

//                }


//            }



//        }

//        return;
//    Errs:
//        ;
//    }
//    catch
//    {

//        int w = 0;
//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + "-" + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");
//        bDone = true;
//        bError = true;
//    }
//}

//public static void GetPOTransaction(ref bool bError, DateTime fromDate, DateTime todate)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                IPurchaseOrderQuery ReceiptQuery;
//        ReceiptQuery = msgSetRequest.AppendPurchaseOrderQueryRq;
//        'ReceiptQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate)
//                 'ReceiptQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate)

//                ReceiptQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
//        ReceiptQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);

//        ReceiptQuery.IncludeLineItems.SetValue(true);
//        ReceiptQuery.IncludeLinkedTxns.SetValue(true);

//        ' RecepitQuery.IncludeRetElementLis()

//                 'RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll)
//                 ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillPOInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetPO");
//        bError = true;
//    }
//}

//public static void GetPOTransactionNoDate(ref bool bError)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                IPurchaseOrderQuery ReceiptQuery;
//        ReceiptQuery = msgSetRequest.AppendPurchaseOrderQueryRq;

//        'RecepitQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate)
//                 'RecepitQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate)

//                ReceiptQuery.IncludeLineItems.SetValue(true);
//        ReceiptQuery.IncludeLinkedTxns.SetValue(true);

//        ' RecepitQuery.IncludeRetElementLis()

//                 'RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll)
//                 ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillPOInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetPO");
//        bError = true;
//    }
//}


//public static void FillPOInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    'On Error GoTo Errs
//            try
//    {
//        string RefNumber = string.Empty;
//        check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)
//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        Start parsing the response list
//       IResponseList responseList;
//        responseList = msgSetResponse.ResponseList;

//        go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//        response = responseList.GetAt(0);
//        if (response is not null)
//        {
//            if (response.StatusCode != "0")
//            {
//                If the status is bad, report it to the user
//                        Interaction.MsgBox("Get Purchase Order query unexpexcted Error - " + response.StatusMessage);
//                bDone = true;
//                bError = true;
//                return;
//            }
//        }

//        first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)
//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IPurchaseOrderRetList PurchaseOrderRetList;
//        ENResponseType responseType;
//        ENObjectType responseDetailType;
//        responseType = response.Type.GetValue();
//        responseDetailType = response.Detail.Type.GetValue();
//        'If (responseType = ENResponseType.rtInvoiceQueryRs) And (responseDetailType = ENObjectType.otInvoiceRetList) Then
//                if (responseType == ENResponseType.rtPurchaseOrderQueryRs & responseDetailType == ENObjectType.otPurchaseOrderRetList)
//        {
//            save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    PurchaseOrderRetList = response.Detail;
//        }
//        else
//        {
//            bail, we do not have the responses we were expecting
//                    bDone = true;
//            bError = true;
//            return;
//        }

//        Parse the query response and add the Customers to the Customer list box
//                short count;
//        short index;
//        IPurchaseOrderRet PurchaseOrderRet;
//        count = PurchaseOrderRetList.Count;
//        short MAX_RETURNED;
//        MAX_RETURNED = 1 + PurchaseOrderRetList.Count;
//        we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//        {
//            bDone = true;
//        }
//        db.QBPurchaseOrder_Delete(ClearAllControl.gblCompanyID);
//        'db.tblInvoiceApplTxn_Delete()
//                var DueDate852 = default(DateTime);
//        var ExpectedDate853 = default(DateTime);
//        var LinkType876 = default(ENLinkType);
//        var ServiceDate895 = default(DateTime);
//        var ServiceDate933 = default(DateTime);
//        var loopTo = (short)(count - 1);
//        for (index = 0; index <= loopTo; index++)
//        {
//            skip this customer if this is a repeat from the last query
//                    PurchaseOrderRet = PurchaseOrderRetList.GetAt(index);
//            if (PurchaseOrderRet is null)
//            {
//                bDone = true;
//                bError = true;
//                return;
//            }

//            string TxnID803 = null;
//            DateTime TimeCreated804;
//            DateTime TimeModified805;
//            string EditSequence806 = null;
//            int TxnNumber807 = 0;
//            string VendorRefListID808 = null;
//            string VendorRefFullName809 = null;
//            string ClassRefListID810 = null;
//            string ClassRefFullName811 = null;
//            string InventorySiteRefListID812 = null;
//            string InventorySiteRefFullName813 = null;
//            string ShipToEntityRefListID814 = null;
//            string ShipToEntityRefFullName815 = null;
//            string TemplateRefListID816 = null;
//            string TemplateRefFullName817 = null;
//            DateTime TxnDate818;
//            string RefNumber819 = null;
//            string VendorAddressAddr1820 = null;
//            string VendorAddressAddr2821 = null;
//            string VendorAddressAddr3822 = null;
//            string VendorAddressAddr4823 = null;
//            string VendorAddressAddr5824 = null;
//            string VendorAddressCity825 = null;
//            string VendorAddressState826 = null;
//            string VendorAddressPostalCode827 = null;
//            string VendorAddressCountry828 = null;
//            string VendorAddressNote829 = null;
//            string VendorAddressBlockAddr1830 = null;
//            string VendorAddressBlockAddr2831 = null;
//            string VendorAddressBlockAddr3832 = null;
//            string VendorAddressBlockAddr4833 = null;
//            string VendorAddressBlockAddr5834 = null;
//            string ShipAddressAddr1835 = null;
//            string ShipAddressAddr2836 = null;
//            string ShipAddressAddr3837 = null;
//            string ShipAddressAddr4838 = null;
//            string ShipAddressAddr5839 = null;
//            string ShipAddressCity840 = null;
//            string ShipAddressState841 = null;
//            string ShipAddressPostalCode842 = null;
//            string ShipAddressCountry843 = null;
//            string ShipAddressNote844 = null;
//            string ShipAddressBlockAddr1845 = null;
//            string ShipAddressBlockAddr2846 = null;
//            string ShipAddressBlockAddr3847 = null;
//            string ShipAddressBlockAddr4848 = null;
//            string ShipAddressBlockAddr5849 = null;
//            string TermsRefListID850 = null;
//            string TermsRefFullName851 = null;
//            string ShipMethodRefListID854 = null;
//            string ShipMethodRefFullName855 = null;
//            string FOB856 = null;
//            double TotalAmount857 = 0d;
//            string CurrencyRefListID858 = null;
//            string CurrencyRefFullName859 = null;
//            string ExchangeRate860 = null;
//            double TotalAmountInHomeCurrency861 = 0d;
//            bool IsManuallyClosed862 = false;
//            bool IsFullyReceived863 = false;
//            string Memo864 = null;
//            string VendorMsg865 = null;
//            bool IsToBePrinted866 = false;
//            bool IsToBeEmailed867 = false;
//            string Other1868 = null;
//            string Other2869 = null;
//            string ExternalGUID870 = null;
//            string taxcode = null;



//            TxnID803 = PurchaseOrderRet.TxnID.GetValue();
//            Get value of TimeCreated
//                    TimeCreated804 = PurchaseOrderRet.TimeCreated.GetValue();
//            Get value of TimeModified
//                    TimeModified805 = PurchaseOrderRet.TimeModified.GetValue();
//            Get value of EditSequence
//                    EditSequence806 = PurchaseOrderRet.EditSequence.GetValue();
//            Get value of TxnNumber
//                    if (PurchaseOrderRet.TxnNumber is not null)
//            {
//                TxnNumber807 = PurchaseOrderRet.TxnNumber.GetValue();
//            }
//            if (PurchaseOrderRet.VendorRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.VendorRef.ListID is not null)
//                {
//                    VendorRefListID808 = PurchaseOrderRet.VendorRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.VendorRef.FullName is not null)
//                {
//                    VendorRefFullName809 = PurchaseOrderRet.VendorRef.FullName.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.ClassRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.ClassRef.ListID is not null)
//                {
//                    ClassRefListID810 = PurchaseOrderRet.ClassRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.ClassRef.FullName is not null)
//                {
//                    ClassRefFullName811 = PurchaseOrderRet.ClassRef.FullName.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.InventorySiteRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.InventorySiteRef.ListID is not null)
//                {
//                    InventorySiteRefListID812 = PurchaseOrderRet.InventorySiteRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.InventorySiteRef.FullName is not null)
//                {
//                    InventorySiteRefFullName813 = PurchaseOrderRet.InventorySiteRef.FullName.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.ShipToEntityRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.ShipToEntityRef.ListID is not null)
//                {
//                    ShipToEntityRefListID814 = PurchaseOrderRet.ShipToEntityRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.ShipToEntityRef.FullName is not null)
//                {
//                    ShipToEntityRefFullName815 = PurchaseOrderRet.ShipToEntityRef.FullName.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.TemplateRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.TemplateRef.ListID is not null)
//                {
//                    TemplateRefListID816 = PurchaseOrderRet.TemplateRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.TemplateRef.FullName is not null)
//                {
//                    TemplateRefFullName817 = PurchaseOrderRet.TemplateRef.FullName.GetValue();
//                }
//            }
//            Get value of TxnDate
//                    TxnDate818 = PurchaseOrderRet.TxnDate.GetValue();
//            Get value of RefNumber
//                    if (PurchaseOrderRet.RefNumber is not null)
//            {
//                RefNumber819 = PurchaseOrderRet.RefNumber.GetValue();
//            }
//            if (PurchaseOrderRet.VendorAddress is not null)
//            {
//                Get value of Addr1
//                        if (PurchaseOrderRet.VendorAddress.Addr1 is not null)
//                {
//                    VendorAddressAddr1820 = PurchaseOrderRet.VendorAddress.Addr1.GetValue();
//                }
//                Get value of Addr2
//                        if (PurchaseOrderRet.VendorAddress.Addr2 is not null)
//                {
//                    VendorAddressAddr2821 = PurchaseOrderRet.VendorAddress.Addr2.GetValue();
//                }
//                Get value of Addr3
//                        if (PurchaseOrderRet.VendorAddress.Addr3 is not null)
//                {
//                    VendorAddressAddr3822 = PurchaseOrderRet.VendorAddress.Addr3.GetValue();
//                }
//                Get value of Addr4
//                        if (PurchaseOrderRet.VendorAddress.Addr4 is not null)
//                {
//                    VendorAddressAddr4823 = PurchaseOrderRet.VendorAddress.Addr4.GetValue();
//                }
//                Get value of Addr5
//                        if (PurchaseOrderRet.VendorAddress.Addr5 is not null)
//                {
//                    VendorAddressAddr5824 = PurchaseOrderRet.VendorAddress.Addr5.GetValue();
//                }
//                Get value of City
//                        if (PurchaseOrderRet.VendorAddress.City is not null)
//                {
//                    VendorAddressCity825 = PurchaseOrderRet.VendorAddress.City.GetValue();
//                }
//                Get value of State
//                        if (PurchaseOrderRet.VendorAddress.State is not null)
//                {
//                    VendorAddressState826 = PurchaseOrderRet.VendorAddress.State.GetValue();
//                }
//                Get value of PostalCode
//                        if (PurchaseOrderRet.VendorAddress.PostalCode is not null)
//                {
//                    VendorAddressPostalCode827 = PurchaseOrderRet.VendorAddress.PostalCode.GetValue();
//                }
//                Get value of Country
//                        if (PurchaseOrderRet.VendorAddress.Country is not null)
//                {
//                    VendorAddressCountry828 = PurchaseOrderRet.VendorAddress.Country.GetValue();
//                }
//                Get value of Note
//                        if (PurchaseOrderRet.VendorAddress.Note is not null)
//                {
//                    VendorAddressNote829 = PurchaseOrderRet.VendorAddress.Note.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.VendorAddressBlock is not null)
//            {
//                Get value of Addr1
//                        if (PurchaseOrderRet.VendorAddressBlock.Addr1 is not null)
//                {
//                    VendorAddressBlockAddr1830 = PurchaseOrderRet.VendorAddressBlock.Addr1.GetValue();
//                }
//                Get value of Addr2
//                        if (PurchaseOrderRet.VendorAddressBlock.Addr2 is not null)
//                {
//                    VendorAddressBlockAddr2831 = PurchaseOrderRet.VendorAddressBlock.Addr2.GetValue();
//                }
//                Get value of Addr3
//                        if (PurchaseOrderRet.VendorAddressBlock.Addr3 is not null)
//                {
//                    VendorAddressBlockAddr3832 = PurchaseOrderRet.VendorAddressBlock.Addr3.GetValue();
//                }
//                Get value of Addr4
//                        if (PurchaseOrderRet.VendorAddressBlock.Addr4 is not null)
//                {
//                    VendorAddressBlockAddr4833 = PurchaseOrderRet.VendorAddressBlock.Addr4.GetValue();
//                }
//                Get value of Addr5
//                        if (PurchaseOrderRet.VendorAddressBlock.Addr5 is not null)
//                {
//                    VendorAddressBlockAddr5834 = PurchaseOrderRet.VendorAddressBlock.Addr5.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.ShipAddress is not null)
//            {

//                Get value of Addr1
//                        if (PurchaseOrderRet.ShipAddress.Addr1 is not null)
//                {
//                    ShipAddressAddr1835 = PurchaseOrderRet.ShipAddress.Addr1.GetValue();
//                }
//                Get value of Addr2
//                        if (PurchaseOrderRet.ShipAddress.Addr2 is not null)
//                {
//                    ShipAddressAddr2836 = PurchaseOrderRet.ShipAddress.Addr2.GetValue();
//                }
//                Get value of Addr3
//                        if (PurchaseOrderRet.ShipAddress.Addr3 is not null)
//                {
//                    ShipAddressAddr3837 = PurchaseOrderRet.ShipAddress.Addr3.GetValue();
//                }
//                Get value of Addr4
//                        if (PurchaseOrderRet.ShipAddress.Addr4 is not null)
//                {
//                    ShipAddressAddr4838 = PurchaseOrderRet.ShipAddress.Addr4.GetValue();
//                }
//                Get value of Addr5
//                        if (PurchaseOrderRet.ShipAddress.Addr5 is not null)
//                {
//                    ShipAddressAddr5839 = PurchaseOrderRet.ShipAddress.Addr5.GetValue();
//                }
//                Get value of City
//                        if (PurchaseOrderRet.ShipAddress.City is not null)
//                {
//                    ShipAddressCity840 = PurchaseOrderRet.ShipAddress.City.GetValue();
//                }
//                Get value of State
//                        if (PurchaseOrderRet.ShipAddress.State is not null)
//                {
//                    ShipAddressState841 = PurchaseOrderRet.ShipAddress.State.GetValue();
//                }
//                Get value of PostalCode
//                        if (PurchaseOrderRet.ShipAddress.PostalCode is not null)
//                {
//                    ShipAddressPostalCode842 = PurchaseOrderRet.ShipAddress.PostalCode.GetValue();
//                }
//                Get value of Country
//                        if (PurchaseOrderRet.ShipAddress.Country is not null)
//                {
//                    ShipAddressCountry843 = PurchaseOrderRet.ShipAddress.Country.GetValue();
//                }
//                Get value of Note
//                        if (PurchaseOrderRet.ShipAddress.Note is not null)
//                {
//                    ShipAddressNote844 = PurchaseOrderRet.ShipAddress.Note.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.ShipAddressBlock is not null)
//            {
//                Get value of Addr1
//                        if (PurchaseOrderRet.ShipAddressBlock.Addr1 is not null)
//                {
//                    ShipAddressBlockAddr1845 = PurchaseOrderRet.ShipAddressBlock.Addr1.GetValue();
//                }
//                Get value of Addr2
//                        if (PurchaseOrderRet.ShipAddressBlock.Addr2 is not null)
//                {
//                    ShipAddressBlockAddr2846 = PurchaseOrderRet.ShipAddressBlock.Addr2.GetValue();
//                }
//                Get value of Addr3
//                        if (PurchaseOrderRet.ShipAddressBlock.Addr3 is not null)
//                {
//                    ShipAddressBlockAddr3847 = PurchaseOrderRet.ShipAddressBlock.Addr3.GetValue();
//                }
//                Get value of Addr4
//                        if (PurchaseOrderRet.ShipAddressBlock.Addr4 is not null)
//                {
//                    ShipAddressBlockAddr4848 = PurchaseOrderRet.ShipAddressBlock.Addr4.GetValue();
//                }
//                Get value of Addr5
//                        if (PurchaseOrderRet.ShipAddressBlock.Addr5 is not null)
//                {
//                    ShipAddressBlockAddr5849 = PurchaseOrderRet.ShipAddressBlock.Addr5.GetValue();
//                }
//            }
//            if (PurchaseOrderRet.TermsRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.TermsRef.ListID is not null)
//                {
//                    TermsRefListID850 = PurchaseOrderRet.TermsRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.TermsRef.FullName is not null)
//                {
//                    TermsRefFullName851 = PurchaseOrderRet.TermsRef.FullName.GetValue();
//                }
//            }
//            Get value of DueDate
//                    if (PurchaseOrderRet.DueDate is not null)
//            {
//                DueDate852 = PurchaseOrderRet.DueDate.GetValue();
//            }
//            Get value of ExpectedDate
//                    if (PurchaseOrderRet.ExpectedDate is not null)
//            {
//                ExpectedDate853 = PurchaseOrderRet.ExpectedDate.GetValue();
//            }
//            if (PurchaseOrderRet.ShipMethodRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.ShipMethodRef.ListID is not null)
//                {
//                    ShipMethodRefListID854 = PurchaseOrderRet.ShipMethodRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.ShipMethodRef.FullName is not null)
//                {
//                    ShipMethodRefFullName855 = PurchaseOrderRet.ShipMethodRef.FullName.GetValue();
//                }
//            }
//            Get value of FOB
//                    if (PurchaseOrderRet.FOB is not null)
//            {
//                FOB856 = PurchaseOrderRet.FOB.GetValue();
//            }
//            Get value of TotalAmount
//                    if (PurchaseOrderRet.TotalAmount is not null)
//            {
//                TotalAmount857 = PurchaseOrderRet.TotalAmount.GetValue();
//            }
//            if (PurchaseOrderRet.CurrencyRef is not null)
//            {
//                Get value of ListID
//                        if (PurchaseOrderRet.CurrencyRef.ListID is not null)
//                {
//                    CurrencyRefListID858 = PurchaseOrderRet.CurrencyRef.ListID.GetValue();
//                }
//                Get value of FullName
//                        if (PurchaseOrderRet.CurrencyRef.FullName is not null)
//                {
//                    CurrencyRefFullName859 = PurchaseOrderRet.CurrencyRef.FullName.GetValue();
//                }
//            }
//            Get value of ExchangeRate
//                    if (PurchaseOrderRet.ExchangeRate is not null)
//            {
//                ExchangeRate860 = PurchaseOrderRet.ExchangeRate.GetValue();
//            }
//            Get value of TotalAmountInHomeCurrency
//                    if (PurchaseOrderRet.TotalAmountInHomeCurrency is not null)
//            {
//                TotalAmountInHomeCurrency861 = PurchaseOrderRet.TotalAmountInHomeCurrency.GetValue();
//            }
//            Get value of IsManuallyClosed
//                    if (PurchaseOrderRet.IsManuallyClosed is not null)
//            {
//                IsManuallyClosed862 = PurchaseOrderRet.IsManuallyClosed.GetValue();
//            }
//            Get value of IsFullyReceived
//                    if (PurchaseOrderRet.IsFullyReceived is not null)
//            {
//                IsFullyReceived863 = PurchaseOrderRet.IsFullyReceived.GetValue();
//            }
//            Get value of Memo
//                    if (PurchaseOrderRet.Memo is not null)
//            {
//                Memo864 = PurchaseOrderRet.Memo.GetValue();
//            }
//            Get value of VendorMsg
//                    if (PurchaseOrderRet.VendorMsg is not null)
//            {
//                VendorMsg865 = PurchaseOrderRet.VendorMsg.GetValue();
//            }
//            Get value of IsToBePrinted
//                    if (PurchaseOrderRet.IsToBePrinted is not null)
//            {
//                IsToBePrinted866 = PurchaseOrderRet.IsToBePrinted.GetValue();
//            }
//            Get value of IsToBeEmailed
//                    if (PurchaseOrderRet.IsToBeEmailed is not null)
//            {
//                IsToBeEmailed867 = PurchaseOrderRet.IsToBeEmailed.GetValue();
//            }
//            Get value of Other1
//                    if (PurchaseOrderRet.Other1 is not null)
//            {
//                Other1868 = PurchaseOrderRet.Other1.GetValue();
//            }
//            Get value of Other2
//                    if (PurchaseOrderRet.Other2 is not null)
//            {
//                Other2869 = PurchaseOrderRet.Other2.GetValue();
//            }
//            Get value of ExternalGUID
//                    if (PurchaseOrderRet.ExternalGUID is not null)
//            {
//                ExternalGUID870 = PurchaseOrderRet.ExternalGUID.GetValue();
//            }

//            'get value of linked Transaction List
//                    if (PurchaseOrderRet.LinkedTxnList is not null)
//            {

//                int i871 = 0;
//                var loopTo1 = PurchaseOrderRet.LinkedTxnList.Count - 1;
//                for (i871 = 0; i871 <= loopTo1; i871++)
//                {
//                    ILinkedTxn LinkedTxn;
//                    LinkedTxn = PurchaseOrderRet.LinkedTxnList.GetAt(i871);

//                    string TxnID872 = Conversions.ToString("" == "");
//                    ENTxnType TxnType873;
//                    DateTime TxnDate874;
//                    string RefNumber875 = Conversions.ToString("" == "");
//                    double Amount877 = Conversions.ToDouble(0 == 0);
//                    Get value of TxnID
//                            TxnID872 = LinkedTxn.TxnID.GetValue();
//                    Get value of TxnType
//                            TxnType873 = LinkedTxn.TxnType.GetValue();
//                    Get value of TxnDate
//                            TxnDate874 = LinkedTxn.TxnDate.GetValue();
//                    Get value of RefNumber
//                            if (LinkedTxn.RefNumber is not null)
//                    {
//                        RefNumber875 = LinkedTxn.RefNumber.GetValue();
//                    }
//                    Get value of LinkType
//                            if (LinkedTxn.LinkType is not null)
//                    {
//                        LinkType876 = LinkedTxn.LinkType.GetValue();
//                    }
//                    Get value of Amount
//                            Amount877 = LinkedTxn.Amount.GetValue();

//                    db.PurchaseOrderLinkedTxn_Insert(TxnID803, TxnID872, TxnType873, LinkType876, TxnDate874, RefNumber875, Amount877, TxnID803, ClearAllControl.gblCompanyID);
//                }

//            }
//            'For Line Item List
//                    if (PurchaseOrderRet.ORPurchaseOrderLineRetList is not null)
//            {

//                int i878 = 0;
//                var loopTo2 = PurchaseOrderRet.ORPurchaseOrderLineRetList.Count - 1;
//                for (i878 = 0; i878 <= loopTo2; i878++)
//                {

//                    IORPurchaseOrderLineRet ORPurchaseOrderLineRet879;

//                    ORPurchaseOrderLineRet879 = PurchaseOrderRet.ORPurchaseOrderLineRetList.GetAt(i878);

//                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet is not null)
//                    {
//                        string TxnLineID880 = null;
//                        string ItemRefListID881 = null;
//                        string ItemRefFullName882 = null;
//                        string ManufacturerPartNumber883 = null;
//                        string Desc884 = null;
//                        int Quantity885 = 0;
//                        string UnitOfMeasure886 = null;
//                        string OverrideUOMSetRefListID887 = null;
//                        string OverrideUOMSetRefFullName888 = null;
//                        double Rate889 = 0d;
//                        string ClassRefListID890 = null;
//                        string ClassRefFullName891 = null;
//                        double Amount892 = 0d;
//                        string CustomerRefListID893 = null;
//                        string CustomerRefFullName894 = null;
//                        int ReceivedQuantity896 = 0;
//                        int UnbilledQuantity897 = 0;
//                        bool IsBilled898 = false;
//                        bool IsManuallyClosed899 = false;
//                        string Other1900 = null;
//                        string Other2901 = null;

//                        Get value of TxnLineID

//                                TxnLineID880 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.TxnLineID.GetValue();
//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef is not null)
//                        {
//                            Get value of ListID
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.ListID is not null)
//                            {
//                                ItemRefListID881 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.ListID.GetValue();
//                            }
//                            Get value of FullName
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.FullName is not null)
//                            {
//                                ItemRefFullName882 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.FullName.GetValue();
//                            }
//                        }

//                        Get value of ManufacturerPartNumber
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ManufacturerPartNumber is not null)
//                        {
//                            ManufacturerPartNumber883 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ManufacturerPartNumber.GetValue();
//                        }
//                        Get value of Desc
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Desc is not null)
//                        {
//                            Desc884 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Desc.GetValue();
//                        }
//                        Get value of Quantity
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Quantity is not null)
//                        {
//                            Quantity885 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Quantity.GetValue();
//                        }
//                        Get value of UnitOfMeasure
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnitOfMeasure is not null)
//                        {
//                            UnitOfMeasure886 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnitOfMeasure.GetValue();
//                        }
//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef is not null)
//                        {
//                            Get value of ListID
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.ListID is not null)
//                            {
//                                OverrideUOMSetRefListID887 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.ListID.GetValue();
//                            }
//                            Get value of FullName
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.FullName is not null)
//                            {
//                                OverrideUOMSetRefFullName888 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.FullName.GetValue();
//                            }
//                        }
//                        Get value of Rate
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Rate is not null)
//                        {
//                            Rate889 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Rate.GetValue();
//                        }
//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef is not null)
//                        {
//                            Get value of ListID
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.ListID is not null)
//                            {
//                                ClassRefListID890 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.ListID.GetValue();
//                            }
//                            Get value of FullName
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.FullName is not null)
//                            {
//                                ClassRefFullName891 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.FullName.GetValue();
//                            }
//                        }

//                        Get value of Amount
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Amount is not null)
//                        {
//                            Amount892 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Amount.GetValue();
//                        }
//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef is not null)
//                        {
//                            Get value of ListID
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.ListID is not null)
//                            {
//                                CustomerRefListID893 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.ListID.GetValue();
//                            }
//                            Get value of FullName
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.FullName is not null)
//                            {
//                                CustomerRefFullName894 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.FullName.GetValue();
//                            }

//                        }
//                        Get value of ServiceDate
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ServiceDate is not null)
//                        {
//                            ServiceDate895 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ServiceDate.GetValue();
//                        }
//                        Get value of ReceivedQuantity
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ReceivedQuantity is not null)
//                        {
//                            ReceivedQuantity896 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ReceivedQuantity.GetValue();
//                        }
//                        Get value of UnbilledQuantity
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnbilledQuantity is not null)
//                        {
//                            UnbilledQuantity897 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnbilledQuantity.GetValue();
//                        }
//                        Get value of IsBilled
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsBilled is not null)
//                        {
//                            IsBilled898 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsBilled.GetValue();
//                        }
//                        Get value of IsManuallyClosed
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsManuallyClosed is not null)
//                        {
//                            IsManuallyClosed899 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsManuallyClosed.GetValue();
//                        }
//                        Get value of Other1
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other1 is not null)
//                        {
//                            Other1900 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other1.GetValue();
//                        }
//                        Get value of Other2
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other2 is not null)
//                        {
//                            Other2901 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other2.GetValue();
//                        }

//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.SalesTaxCodeRef is not null)
//                        {
//                            taxcode = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.SalesTaxCodeRef.FullName.GetValue();
//                        }

//                        PurchaseOrderLineRet.DataExtRetList loop
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.DataExtRetList is not null)
//                        {
//                            int i902 = 0;
//                            var loopTo3 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.DataExtRetList.Count - 1;
//                            for (i902 = 0; i902 <= loopTo3; i902++)
//                            {

//                                IDataExtRet DataExtRet;
//                                DataExtRet = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.DataExtRetList.GetAt(i902);
//                                Get value of OwnerID
//                                        string OwnerID903 = null;
//                                string DataExtName904 = null;
//                                ENDataExtType DataExtType905;
//                                string DataExtValue906 = null;
//                                if (DataExtRet.OwnerID is not null)
//                                {
//                                    OwnerID903 = DataExtRet.OwnerID.GetValue();
//                                }
//                                Get value of DataExtName
//                                        DataExtName904 = DataExtRet.DataExtName.GetValue();
//                                Get value of DataExtType
//                                        DataExtType905 = DataExtRet.DataExtType.GetValue();
//                                Get value of DataExtValue
//                                        DataExtValue906 = DataExtRet.DataExtValue.GetValue();
//                                db.PurchaseOrderDataExtRetList_Insert(OwnerID903, DataExtName904, DataExtType905, DataExtValue906, "PurchaseOrderLineRet", TxnID803, ClearAllControl.gblCompanyID);
//                            }

//                        }

//                        db.PurchaseOrderLineItem_Insert(TxnLineID880, ItemRefListID881, ItemRefFullName882, ManufacturerPartNumber883, Desc884, Quantity885.ToString(), UnitOfMeasure886, OverrideUOMSetRefListID887, OverrideUOMSetRefFullName888, (decimal?)Rate889, ClassRefListID890, ClassRefFullName891, (decimal?)Amount892, CustomerRefListID893, CustomerRefFullName894, ServiceDate895, ReceivedQuantity896, UnbilledQuantity897, IsBilled898, IsManuallyClosed899, Other1900, Other2901, TxnID803, ClearAllControl.gblCompanyID, taxcode);


//                    }
//                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet is not null)
//                    {

//                        Get value of TxnLineID
//                                string TxnLineID907 = null;
//                        string ItemGroupRefListID908 = null;
//                        string ItemGroupRefFullName909 = null;
//                        string Desc910 = null;
//                        int Quantity911 = 0;
//                        string UnitOfMeasure912 = null;
//                        string OverrideUOMSetRefListID913 = null;
//                        string OverrideUOMSetRefFullName914 = null;
//                        bool IsPrintItemsInGroup915 = false;
//                        double TotalAmount916 = 0d;

//                        TxnLineID907 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.TxnLineID.GetValue();
//                        Get value of ListID
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.ListID is not null)
//                        {
//                            ItemGroupRefListID908 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.ListID.GetValue();
//                        }
//                        Get value of FullName
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.FullName is not null)
//                        {
//                            ItemGroupRefFullName909 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.FullName.GetValue();
//                        }
//                        Get value of Desc
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Desc is not null)
//                        {
//                            Desc910 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Desc.GetValue();
//                        }
//                        Get value of Quantity
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Quantity is not null)
//                        {
//                            Quantity911 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Quantity.GetValue();
//                        }
//                        Get value of UnitOfMeasure
//                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.UnitOfMeasure is not null)
//                        {
//                            UnitOfMeasure912 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.UnitOfMeasure.GetValue();
//                        }
//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef is not null)
//                        {
//                            Get value of ListID
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.ListID is not null)
//                            {
//                                OverrideUOMSetRefListID913 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.ListID.GetValue();
//                            }
//                            Get value of FullName
//                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.FullName is not null)
//                            {
//                                OverrideUOMSetRefFullName914 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.FullName.GetValue();
//                            }
//                        }
//                        Get value of IsPrintItemsInGroup
//                                IsPrintItemsInGroup915 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.IsPrintItemsInGroup.GetValue();
//                        Get value of TotalAmount
//                                TotalAmount916 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.TotalAmount.GetValue();


//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.PurchaseOrderLineRetList is not null)
//                        {

//                            int i917 = 0;
//                            var loopTo4 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.PurchaseOrderLineRetList.Count - 1;
//                            for (i917 = 0; i917 <= loopTo4; i917++)
//                            {
//                                IPurchaseOrderLineRet PurchaseOrderLineRet;
//                                PurchaseOrderLineRet = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.PurchaseOrderLineRetList.GetAt(i917);

//                                string TxnLineID918 = Conversions.ToString("" == "");
//                                string ItemRefListID919 = null;
//                                string ItemRefFullName920 = null;
//                                string ManufacturerPartNumber921 = null;
//                                string Desc922 = null;
//                                int Quantity923 = 0;
//                                string UnitOfMeasure924 = null;
//                                string OverrideUOMSetRefListID925 = null;
//                                string OverrideUOMSetRefFullName926 = null;
//                                double Rate927 = 0d;
//                                string ClassRefListID928 = null;
//                                string ClassRefFullName929 = null;
//                                double Amount930 = 0d;
//                                string CustomerRefListID931 = null;
//                                string CustomerRefFullName932 = null;
//                                int ReceivedQuantity934 = 0;
//                                int UnbilledQuantity935 = 0;
//                                bool IsBilled936 = false;
//                                bool IsManuallyClosed937 = false;
//                                string Other1938 = null;
//                                string Other2939 = null;
//                                TxnLineID918 = PurchaseOrderLineRet.TxnLineID.GetValue();
//                                if (PurchaseOrderLineRet.ItemRef is not null)
//                                {
//                                    Get value of ListID
//                                            if (PurchaseOrderLineRet.ItemRef.ListID is not null)
//                                    {
//                                        ItemRefListID919 = PurchaseOrderLineRet.ItemRef.ListID.GetValue();
//                                    }
//                                    Get value of FullName
//                                            if (PurchaseOrderLineRet.ItemRef.FullName is not null)
//                                    {
//                                        ItemRefFullName920 = PurchaseOrderLineRet.ItemRef.FullName.GetValue();
//                                    }
//                                }
//                                Get value of ManufacturerPartNumber
//                                        if (PurchaseOrderLineRet.ManufacturerPartNumber is not null)
//                                {
//                                    ManufacturerPartNumber921 = PurchaseOrderLineRet.ManufacturerPartNumber.GetValue();
//                                }
//                                Get value of Desc
//                                        if (PurchaseOrderLineRet.Desc is not null)
//                                {
//                                    Desc922 = PurchaseOrderLineRet.Desc.GetValue();
//                                }
//                                Get value of Quantity
//                                        if (PurchaseOrderLineRet.Quantity is not null)
//                                {
//                                    Quantity923 = PurchaseOrderLineRet.Quantity.GetValue();
//                                }
//                                Get value of UnitOfMeasure
//                                        if (PurchaseOrderLineRet.UnitOfMeasure is not null)
//                                {
//                                    UnitOfMeasure924 = PurchaseOrderLineRet.UnitOfMeasure.GetValue();
//                                }
//                                if (PurchaseOrderLineRet.OverrideUOMSetRef is not null)
//                                {
//                                    Get value of ListID
//                                            if (PurchaseOrderLineRet.OverrideUOMSetRef.ListID is not null)
//                                    {
//                                        OverrideUOMSetRefListID925 = PurchaseOrderLineRet.OverrideUOMSetRef.ListID.GetValue();
//                                    }
//                                    Get value of FullName
//                                            if (PurchaseOrderLineRet.OverrideUOMSetRef.FullName is not null)
//                                    {
//                                        OverrideUOMSetRefFullName926 = PurchaseOrderLineRet.OverrideUOMSetRef.FullName.GetValue();
//                                    }

//                                }
//                                Get value of Rate
//                                        if (PurchaseOrderLineRet.Rate is not null)
//                                {
//                                    Rate927 = PurchaseOrderLineRet.Rate.GetValue();
//                                }
//                                if (PurchaseOrderLineRet.ClassRef is not null)
//                                {
//                                    Get value of ListID
//                                            if (PurchaseOrderLineRet.ClassRef.ListID is not null)
//                                    {
//                                        ClassRefListID928 = PurchaseOrderLineRet.ClassRef.ListID.GetValue();
//                                    }
//                                    Get value of FullName
//                                            if (PurchaseOrderLineRet.ClassRef.FullName is not null)
//                                    {
//                                        ClassRefFullName929 = PurchaseOrderLineRet.ClassRef.FullName.GetValue();
//                                    }

//                                }
//                                Get value of Amount
//                                        if (PurchaseOrderLineRet.Amount is not null)
//                                {
//                                    Amount930 = PurchaseOrderLineRet.Amount.GetValue();
//                                }
//                                if (PurchaseOrderLineRet.CustomerRef is not null)
//                                {
//                                    Get value of ListID
//                                            if (PurchaseOrderLineRet.CustomerRef.ListID is not null)
//                                    {
//                                        CustomerRefListID931 = PurchaseOrderLineRet.CustomerRef.ListID.GetValue();
//                                    }
//                                    Get value of FullName
//                                            if (PurchaseOrderLineRet.CustomerRef.FullName is not null)
//                                    {
//                                        CustomerRefFullName932 = PurchaseOrderLineRet.CustomerRef.FullName.GetValue();
//                                    }
//                                }
//                                Get value of ServiceDate
//                                        if (PurchaseOrderLineRet.ServiceDate is not null)
//                                {
//                                    ServiceDate933 = PurchaseOrderLineRet.ServiceDate.GetValue();
//                                }
//                                Get value of ReceivedQuantity
//                                        if (PurchaseOrderLineRet.ReceivedQuantity is not null)
//                                {
//                                    ReceivedQuantity934 = PurchaseOrderLineRet.ReceivedQuantity.GetValue();
//                                }
//                                Get value of UnbilledQuantity
//                                        if (PurchaseOrderLineRet.UnbilledQuantity is not null)
//                                {
//                                    UnbilledQuantity935 = PurchaseOrderLineRet.UnbilledQuantity.GetValue();
//                                }
//                                Get value of IsBilled
//                                        if (PurchaseOrderLineRet.IsBilled is not null)
//                                {
//                                    IsBilled936 = PurchaseOrderLineRet.IsBilled.GetValue();
//                                }
//                                Get value of IsManuallyClosed
//                                        if (PurchaseOrderLineRet.IsManuallyClosed is not null)
//                                {
//                                    IsManuallyClosed937 = PurchaseOrderLineRet.IsManuallyClosed.GetValue();
//                                }
//                                Get value of Other1
//                                        if (PurchaseOrderLineRet.Other1 is not null)
//                                {
//                                    Other1938 = PurchaseOrderLineRet.Other1.GetValue();
//                                }
//                                Get value of Other2
//                                        if (PurchaseOrderLineRet.Other2 is not null)
//                                {
//                                    Other2939 = PurchaseOrderLineRet.Other2.GetValue();
//                                }
//                                if (PurchaseOrderLineRet.DataExtRetList is not null)
//                                {

//                                    int i940 = 0;
//                                    var loopTo5 = PurchaseOrderLineRet.DataExtRetList.Count - 1;
//                                    for (i940 = 0; i940 <= loopTo5; i940++)
//                                    {

//                                        IDataExtRet DataExtRet;
//                                        DataExtRet = PurchaseOrderLineRet.DataExtRetList.GetAt(i940);
//                                        Get value of OwnerID
//                                                string OwnerID941 = null;
//                                        string DataExtName942 = null;
//                                        ENDataExtType DataExtType943;
//                                        string DataExtValue944 = null;
//                                        if (DataExtRet.OwnerID is not null)
//                                        {
//                                            OwnerID941 = DataExtRet.OwnerID.GetValue();
//                                        }
//                                        Get value of DataExtName
//                                                DataExtName942 = DataExtRet.DataExtName.GetValue();
//                                        Get value of DataExtType
//                                                DataExtType943 = DataExtRet.DataExtType.GetValue();
//                                        Get value of DataExtValue
//                                                DataExtValue944 = DataExtRet.DataExtValue.GetValue();
//                                        db.PurchaseOrderDataExtRetList_Insert(OwnerID941, DataExtName942, DataExtType943, DataExtValue944, "PurchaseOrderLineRet", TxnID803, ClearAllControl.gblCompanyID);
//                                    }
//                                    'groupLineItem Insert
//                                            db.PurchaseOrderLineItem_Insert(TxnLineID918, ItemRefListID919, ItemRefFullName920, ManufacturerPartNumber921, Desc922, Quantity923.ToString(), UnitOfMeasure924, OverrideUOMSetRefListID925, OverrideUOMSetRefFullName926, (decimal?)Rate927, ClassRefListID928, ClassRefFullName929, (decimal?)Amount930, CustomerRefListID931, CustomerRefFullName932, ServiceDate933, ReceivedQuantity934, UnbilledQuantity935, IsBilled936, IsManuallyClosed937, Other1938, Other2939, TxnID803, ClearAllControl.gblCompanyID, taxcode);

//                                }

//                            }

//                        }
//                        if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.DataExtRetList is not null)
//                        {

//                            int i945 = 0;
//                            var loopTo6 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.DataExtRetList.Count - 1;
//                            for (i945 = 0; i945 <= loopTo6; i945++)
//                            {

//                                IDataExtRet DataExtRet;
//                                DataExtRet = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.DataExtRetList.GetAt(i945);
//                                Get value of OwnerID
//                                        string OwnerID946 = null;
//                                if (DataExtRet.OwnerID is not null)
//                                {
//                                    OwnerID946 = DataExtRet.OwnerID.GetValue();
//                                }
//                                Get value of DataExtName
//                                        string DataExtName947 = null;
//                                DataExtName947 = DataExtRet.DataExtName.GetValue();
//                                Get value of DataExtType
//                                        ENDataExtType DataExtType948;
//                                DataExtType948 = DataExtRet.DataExtType.GetValue();
//                                Get value of DataExtValue
//                                        string DataExtValue949 = null;
//                                DataExtValue949 = DataExtRet.DataExtValue.GetValue();
//                                db.PurchaseOrderDataExtRetList_Insert(OwnerID946, DataExtName947, DataExtType948, DataExtValue949, "PurchaseOrderLineGroupRet", TxnID803, ClearAllControl.gblCompanyID);
//                            }

//                        }
//                        PurchaseOrderLineGroupRet
//                                db.PurchaseOrderLineGroupItem_Insert(TxnLineID907, ItemGroupRefListID908, ItemGroupRefFullName909, Desc910, Quantity911, UnitOfMeasure912, OverrideUOMSetRefListID913, OverrideUOMSetRefFullName914, IsPrintItemsInGroup915, (decimal?)TotalAmount916, TxnID803, ClearAllControl.gblCompanyID);
//                    }
//                }
//            }


//            if (PurchaseOrderRet.DataExtRetList is not null)
//            {

//                int i950 = 0;
//                var loopTo7 = PurchaseOrderRet.DataExtRetList.Count - 1;
//                for (i950 = 0; i950 <= loopTo7; i950++)
//                {

//                    IDataExtRet DataExtRet;
//                    DataExtRet = PurchaseOrderRet.DataExtRetList.GetAt(i950);
//                    string OwnerID951 = null;
//                    Get value of OwnerID
//                            if (DataExtRet.OwnerID is not null)
//                    {


//                        OwnerID951 = DataExtRet.OwnerID.GetValue();

//                    }
//                    Get value of DataExtName
//                            string DataExtName952 = null;
//                    DataExtName952 = DataExtRet.DataExtName.GetValue();
//                    Get value of DataExtType
//                            ENDataExtType DataExtType953;
//                    DataExtType953 = DataExtRet.DataExtType.GetValue();
//                    Get value of DataExtValue
//                            string DataExtValue954 = null;
//                    DataExtValue954 = DataExtRet.DataExtValue.GetValue();
//                    db.PurchaseOrderDataExtRetList_Insert(OwnerID951, DataExtName952, DataExtType953, DataExtValue954, "PurchaseOrderRet", TxnID803, ClearAllControl.gblCompanyID);

//                }

//            }

//            Main table insert
//                    db.QBPurchaseOrder_Insert(TxnID803, TimeCreated804, TimeModified805, EditSequence806, TxnNumber807, VendorRefListID808, VendorRefFullName809, ClassRefListID810, ClassRefFullName811, InventorySiteRefListID812, InventorySiteRefFullName813, ShipToEntityRefListID814, ShipToEntityRefFullName815, TemplateRefListID816, TemplateRefFullName817, TxnDate818, RefNumber819, VendorAddressAddr1820, VendorAddressAddr2821, VendorAddressAddr3822, VendorAddressAddr4823, VendorAddressAddr5824, VendorAddressCity825, VendorAddressState826, VendorAddressPostalCode827, VendorAddressCountry828, VendorAddressNote829, VendorAddressBlockAddr1830, VendorAddressBlockAddr2831, VendorAddressBlockAddr3832, VendorAddressBlockAddr4833, VendorAddressBlockAddr5834, ShipAddressAddr1835, ShipAddressAddr2836, ShipAddressAddr3837, ShipAddressAddr4838, ShipAddressAddr5839, ShipAddressCity840, ShipAddressState841, ShipAddressPostalCode842, ShipAddressCountry843, ShipAddressNote844, ShipAddressBlockAddr1845, ShipAddressBlockAddr2846, ShipAddressBlockAddr3847, ShipAddressBlockAddr4848, ShipAddressBlockAddr5849, TermsRefListID850, TermsRefFullName851, DueDate852, ExpectedDate853, ShipMethodRefListID854, ShipMethodRefFullName855, FOB856, (decimal?)TotalAmount857, CurrencyRefListID858, CurrencyRefFullName859, ExchangeRate860, (decimal?)TotalAmountInHomeCurrency861, IsManuallyClosed862, IsFullyReceived863, Memo864, VendorMsg865, IsToBePrinted866, IsToBeEmailed867, Other1868, Other2869, ExternalGUID870, ClearAllControl.gblCompanyID);

//        }

//        return;
//    Errs:
//        ;
//    }
//    catch
//    {

//        int w = 0;
//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + "-" + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");
//        bDone = true;
//        bError = true;
//    }
//}

//public static void GetCreditTransaction(ref bool bError, DateTime fromDate, DateTime todate)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                ICreditMemoQuery RecepitQuery;
//        RecepitQuery = msgSetRequest.AppendCreditMemoQueryRq;
//        RecepitQuery.IncludeLineItems.SetValue(true);
//        RecepitQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
//        RecepitQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
//        ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillCreditInDataBaseAll(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }


//}

//public static void GetCreditTransactionNoDate(ref bool bError)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                ICreditMemoQuery RecepitQuery;
//        RecepitQuery = msgSetRequest.AppendCreditMemoQueryRq;
//        RecepitQuery.IncludeLineItems.SetValue(true);
//        'RecepitQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate)
//                 'RecepitQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate)
//                 ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillCreditInDataBaseAll(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }


//}



//public static void FillCreditInDataBaseAll(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    try
//    {
//        string glvConnection;
//        string strIntConnection;
//        string sql;
//        var intDs = new DataSet();
//        var ds = new DataSet();
//        var dsCustomer = new DataSet();
//        var dsClinetCode = new DataSet();
//        check to make sure we have objects to access first
//                 and that there are responses in the list


//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        Start parsing the response list
//       IResponseList responseList;
//        responseList = msgSetResponse.ResponseList;

//        go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//        response = responseList.GetAt(0);
//        if (response is not null)
//        {
//            if (response.StatusCode != "0")
//            {
//                If the status is bad, report it to the user
//                        Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
//                bDone = true;
//                bError = true;
//                return;
//            }
//        }

//        first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                ICreditMemoRetList InvoiceRetList;
//        ENResponseType responseType;
//        ENObjectType responseDetailType;
//        responseType = response.Type.GetValue();
//        responseDetailType = response.Detail.Type.GetValue();
//        if (responseType == ENResponseType.rtCreditMemoQueryRs & responseDetailType == ENObjectType.otCreditMemoRetList)
//        {
//            save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    InvoiceRetList = response.Detail;
//        }
//        else
//        {
//            bail, we do not have the responses we were expecting
//                    bDone = true;
//            bError = true;
//            return;
//        }

//        Parse the query response and add the Customers to the Customer list box
//                short count;
//        short index;
//        ICreditMemoRet InvoiceRet;
//        count = InvoiceRetList.Count;
//        short MAX_RETURNED;
//        MAX_RETURNED = 1 + InvoiceRetList.Count;
//        we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//        {
//            bDone = true;
//        }

//        db.tblCredit_DeleteAll(ClearAllControl.gblCompanyID);
//        db.tblcreditLIne_Delete();

//        var SalesTaxPercentage = default(string);
//        var loopTo = (short)(count - 1);
//        for (index = 0; index <= loopTo; index++)
//        {
//            skip this customer if this is a repeat from the last query
//                    InvoiceRet = InvoiceRetList.GetAt(index);
//            if (InvoiceRet is null | InvoiceRet.CustomerRef.ListID is null | InvoiceRet.TxnID is null)

//            {
//                bDone = true;
//                bError = true;
//                return;
//            }


//            'declare varibale to retrive data 
//                    string BillAddress1 = string.Empty;
//            string BillAddress2 = string.Empty;
//            string BillAddress3 = string.Empty;
//            string BillAddress4 = string.Empty;
//            string BillAddress5 = string.Empty;
//            string BillAddressCity = string.Empty;
//            string BillAddressState = string.Empty;
//            string BillAddressPostalCode = string.Empty;
//            string BillAddressCountry = string.Empty;
//            string BillAddressNote = string.Empty;


//            string TxnID = InvoiceRet.TxnID.GetValue;
//            string CustomerRefKey = string.Empty;
//            if (InvoiceRet.CustomerRef is not null)
//            {
//                CustomerRefKey = InvoiceRet.CustomerRef.ListID.GetValue;
//            }
//            string customerName = string.Empty;
//            if (InvoiceRet.CustomerRef is not null)
//            {
//                customerName = InvoiceRet.CustomerRef.FullName.GetValue;
//            }

//            string ClassRefKey = string.Empty;
//            if (InvoiceRet.ClassRef is not null)
//            {
//                ClassRefKey = InvoiceRet.ClassRef.ListID.GetValue;
//            }

//            string ARAccountRefKey = string.Empty;
//            if (InvoiceRet.ARAccountRef is not null)
//            {
//                ARAccountRefKey = InvoiceRet.ARAccountRef.ListID.GetValue;

//            }
//            string TemplateRefKey = string.Empty;
//            if (InvoiceRet.TemplateRef is not null)
//            {
//                TemplateRefKey = InvoiceRet.TemplateRef.ListID.GetValue;
//            }
//            DateTime TxnDate = InvoiceRet.TxnDate.GetValue;

//            string RefNumber = string.Empty;
//            if (InvoiceRet.RefNumber is not null)
//            {
//                RefNumber = InvoiceRet.RefNumber.GetValue;
//            }

//            if (RefNumber == "301R000005")
//            {
//                index = index;
//            }
//            object discAcc;
//            object discAmt;

//            if (InvoiceRet.DiscountLineRet is not null)
//            {
//                discAcc = InvoiceRet.DiscountLineRet.AccountRef.ListID.GetValue();
//                discAmt = InvoiceRet.DiscountLineRet.ORDiscountLineRet.Amount.GetValue();
//            }
//            Bill Address


//                    if (InvoiceRet.BillAddress is not null)
//            {


//                if (InvoiceRet.BillAddress.Addr1 is not null)
//                {
//                    BillAddress1 = InvoiceRet.BillAddress.Addr1.GetValue;
//                }


//                if (InvoiceRet.BillAddress.Addr2 is not null)
//                {
//                    BillAddress2 = InvoiceRet.BillAddress.Addr2.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr3 is not null)
//                {
//                    BillAddress3 = InvoiceRet.BillAddress.Addr3.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr4 is not null)
//                {
//                    BillAddress4 = InvoiceRet.BillAddress.Addr4.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr5 is not null)
//                {
//                    BillAddress5 = InvoiceRet.BillAddress.Addr5.GetValue;
//                }

//                if (InvoiceRet.BillAddress.City is not null)
//                {
//                    BillAddressCity = InvoiceRet.BillAddress.City.GetValue;
//                }

//                if (InvoiceRet.BillAddress.State is not null)
//                {
//                    BillAddressState = InvoiceRet.BillAddress.State.GetValue;
//                }

//                if (InvoiceRet.BillAddress.PostalCode is not null)
//                {
//                    BillAddressPostalCode = InvoiceRet.BillAddress.PostalCode.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Country is not null)
//                {
//                    BillAddressCountry = InvoiceRet.BillAddress.Country.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Note is not null)
//                {
//                    BillAddressNote = InvoiceRet.BillAddress.Note.GetValue;
//                }
//            }
//            Ship address
//                    string ShipAddress1 = string.Empty;
//            string ShipAddress2 = string.Empty;
//            string ShipAddress3 = string.Empty;
//            string ShipAddress4 = string.Empty;
//            string ShipAddress5 = string.Empty;
//            string ShipAddressCity = string.Empty;
//            string ShipAddressState = string.Empty;
//            string ShipAddressPostalCode = string.Empty;
//            string ShipAddressCountry = string.Empty;
//            string ShipAddressNote = string.Empty;
//            if (InvoiceRet.ShipAddress is not null)
//            {

//                if (InvoiceRet.ShipAddress.Addr1 is not null)
//                {
//                    ShipAddress1 = InvoiceRet.ShipAddress.Addr1.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr2 is not null)
//                {
//                    ShipAddress2 = InvoiceRet.ShipAddress.Addr2.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr3 is not null)
//                {
//                    ShipAddress3 = InvoiceRet.ShipAddress.Addr3.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr4 is not null)
//                {
//                    ShipAddress4 = InvoiceRet.ShipAddress.Addr4.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr5 is not null)
//                {
//                    ShipAddress5 = InvoiceRet.ShipAddress.Addr5.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.City is not null)
//                {
//                    ShipAddressCity = InvoiceRet.ShipAddress.City.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.State is not null)
//                {
//                    ShipAddressState = InvoiceRet.ShipAddress.State.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.PostalCode is not null)
//                {
//                    ShipAddressPostalCode = InvoiceRet.ShipAddress.PostalCode.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Country is not null)
//                {
//                    ShipAddressCountry = InvoiceRet.ShipAddress.Country.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Note is not null)
//                {
//                    ShipAddressNote = InvoiceRet.ShipAddress.Note.GetValue;
//                }
//            }
//            End ship address

//                    string IsPending = string.Empty;
//            if (InvoiceRet.IsPending is not null)
//            {
//                IsPending = InvoiceRet.IsPending.GetValue;
//                if (Conversions.ToBoolean(IsPending) == true)
//                {
//                    Interaction.MsgBox("stop here");
//                }
//            }
//            string PONumber = string.Empty;
//            if (InvoiceRet.PONumber is not null)
//            {
//                PONumber = InvoiceRet.PONumber.GetValue;
//            }

//            string TermsRefKey = string.Empty;
//            if (InvoiceRet.TermsRef is not null)
//            {
//                TermsRefKey = InvoiceRet.TermsRef.FullName.GetValue;
//            }

//            string DueDate = string.Empty;
//            if (InvoiceRet.DueDate is not null)
//            {
//                DueDate = InvoiceRet.DueDate.GetValue;
//            }

//            string SalesRefKey = string.Empty;
//            if (InvoiceRet.SalesRepRef is not null)
//            {
//                SalesRefKey = InvoiceRet.SalesRepRef.ListID.GetValue;
//            }

//            string FOB = string.Empty;
//            if (InvoiceRet.FOB is not null)
//            {
//                FOB = InvoiceRet.FOB.GetValue;
//            }

//            string ShipDate = string.Empty;
//            if (InvoiceRet.ShipDate is not null)
//            {
//                ShipDate = InvoiceRet.ShipDate.GetValue;
//            }

//            string ShipMethodRefKey = string.Empty;
//            if (InvoiceRet.ShipMethodRef is not null)
//            {
//                ShipMethodRefKey = InvoiceRet.ShipMethodRef.ListID.GetValue;
//            }

//            string ItemSalesTaxRefKey = string.Empty;
//            if (InvoiceRet.ItemSalesTaxRef is not null)
//            {
//                ItemSalesTaxRefKey = InvoiceRet.ItemSalesTaxRef.ListID.GetValue;
//            }

//            string Memo = string.Empty;
//            if (InvoiceRet.Memo is not null)
//            {
//                Memo = InvoiceRet.Memo.GetValue;
//            }

//            string CustomerMsgRefKey = string.Empty;
//            if (InvoiceRet.CustomerMsgRef is not null)
//            {
//                CustomerMsgRefKey = InvoiceRet.CustomerMsgRef.ListID.GetValue;
//            }
//            string IsToBePrinted = string.Empty;
//            if (InvoiceRet.IsToBePrinted is not null)
//            {
//                IsToBePrinted = InvoiceRet.IsToBePrinted.GetValue;
//            }
//            string IsToEmailed = string.Empty;
//            if (InvoiceRet.IsToBeEmailed is not null)
//            {
//                IsToEmailed = InvoiceRet.IsToBeEmailed.GetValue;
//            }
//            string IsTaxIncluded = string.Empty;
//            if (InvoiceRet.IsTaxIncluded is not null)
//            {
//                IsTaxIncluded = InvoiceRet.IsTaxIncluded.GetValue;
//            }
//            string CustomerSalesTaxCodeRefKey = string.Empty;
//            if (InvoiceRet.CustomerSalesTaxCodeRef is not null)
//            {
//                CustomerSalesTaxCodeRefKey = InvoiceRet.CustomerSalesTaxCodeRef.ListID.GetValue;
//            }
//            string Other = string.Empty;
//            if (InvoiceRet.Other is not null)
//            {
//                Other = InvoiceRet.Other.GetValue;
//            }
//            string Amount = string.Empty;
//            if (InvoiceRet.TotalAmount is not null)
//            {
//                Amount = InvoiceRet.TotalAmount.GetValue;
//            }
//            string AmountPaid = string.Empty;
//            if (InvoiceRet.CreditRemaining is not null)
//            {
//                AmountPaid = InvoiceRet.CreditRemaining.GetValue;
//            }

//            string CustomField1 = string.Empty;
//            string CustomField2 = string.Empty;
//            string CustomField3 = string.Empty;
//            string CustomField4 = string.Empty;
//            string CustomField5 = string.Empty;
//            string QuotationRecNo = string.Empty;
//            string GradeID = string.Empty;
//            string InvoiceType = string.Empty;
//            decimal subTotal = 0m;
//            string SalesTaxTotal = string.Empty;
//            if (InvoiceRet.Subtotal is not null)
//            {
//                subTotal = InvoiceRet.Subtotal.GetValue;
//            }
//            if (InvoiceRet.EditSequence is not null)
//            {
//                SalesTaxPercentage = InvoiceRet.EditSequence.GetValue();
//            }
//            if (InvoiceRet.SalesTaxTotal is not null)
//            {
//                SalesTaxTotal = InvoiceRet.SalesTaxTotal.GetValue;
//            }

//            Insert data in database code left..........db.tblCredit_Insert(ClearAllControl.gblCompanyID, TxnID, CustomerRefKey, null, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, Conversions.ToChar(IsPending), PONumber, TermsRefKey, Conversions.ToDate(DueDate), SalesRefKey, FOB, ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, Conversions.ToChar(IsToBePrinted), Conversions.ToChar(IsToEmailed), Conversions.ToChar(IsTaxIncluded), CustomerSalesTaxCodeRefKey, Other, Conversions.ToDecimal(Amount), Conversions.ToDecimal(AmountPaid), CustomField1, CustomField2, CustomField3, CustomField4, CustomField5, QuotationRecNo, GradeID, Conversions.ToChar(InvoiceType), customerName, subTotal, Conversions.ToDecimal(SalesTaxTotal), Conversions.ToDecimal(SalesTaxPercentage));



//            if (InvoiceRet.ORCreditMemoLineRetList is not null)
//            {
//                for (int k = 0, loopTo1 = InvoiceRet.ORCreditMemoLineRetList.Count - 1; k <= loopTo1; k++)
//                {
//                    IORCreditMemoLineRet InvoiceLineRetList;
//                    InvoiceLineRetList = InvoiceRet.ORCreditMemoLineRetList.GetAt(k);

//                    string lineQuantity = "0";
//                    if (InvoiceLineRetList.CreditMemoLineRet.Quantity is not null)
//                    {
//                        lineQuantity = InvoiceLineRetList.CreditMemoLineRet.Quantity.GetValue;
//                    }

//                    string lineAmount = "0";
//                    if (InvoiceLineRetList.CreditMemoLineRet.Amount is not null)
//                    {
//                        lineAmount = InvoiceLineRetList.CreditMemoLineRet.Amount.GetValue();
//                    }


//                    string lineDesc = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.Desc is not null)
//                    {
//                        lineDesc = InvoiceLineRetList.CreditMemoLineRet.Desc.GetValue();
//                    }
//                    string lineItem = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.ItemRef is not null)
//                    {
//                        lineItem = InvoiceLineRetList.CreditMemoLineRet.ItemRef.FullName.GetValue();
//                    }
//                    if (string.IsNullOrEmpty(lineItem))
//                    {
//                        goto caltchnullline;
//                    }
//                    string lineRate = "0";
//                    if (InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate is not null)
//                    {
//                        lineRate = InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate.GetValue();
//                    }
//                    string customefield = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.Other1 is not null)
//                    {
//                        customefield = InvoiceLineRetList.CreditMemoLineRet.Other1.GetValue();
//                    }
//                    string customeFeild2 = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.Other2 is not null)
//                    {
//                        customeFeild2 = InvoiceLineRetList.CreditMemoLineRet.Other2.GetValue();
//                    }
//                    string lineUOM = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.UnitOfMeasure is not null)
//                    {
//                        lineUOM = InvoiceLineRetList.CreditMemoLineRet.UnitOfMeasure.GetValue();
//                    }
//                    Dim lineQuantity As String = String.Empty
//                             If Not InvoiceLineRetList.InvoiceLineRet.Quantity Is Nothing Then
//                             lineQuantity = InvoiceLineRetList.InvoiceLineRet.Quantity.GetValue()
//                             End If
//                            string lineUOMOrg = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.OverrideUOMSetRef is not null)
//                    {
//                        lineUOMOrg = InvoiceLineRetList.CreditMemoLineRet.OverrideUOMSetRef.FullName.GetValue;
//                    }
//                    string lineTxnID = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.TxnLineID is not null)
//                    {
//                        lineTxnID = InvoiceLineRetList.CreditMemoLineRet.TxnLineID.GetValue();
//                    }
//                    string txaCode = string.Empty;
//                    if (InvoiceLineRetList.CreditMemoLineRet.SalesTaxCodeRef is not null)
//                    {
//                        if (InvoiceLineRetList.CreditMemoLineRet.SalesTaxCodeRef.FullName is not null)
//                        {
//                            txaCode = InvoiceLineRetList.CreditMemoLineRet.SalesTaxCodeRef.FullName.GetValue();
//                        }
//                    }

//                    double taxAmount = 0d;
//                    if (InvoiceLineRetList.CreditMemoLineRet.TaxAmount is not null)
//                    {
//                        taxAmount = InvoiceLineRetList.CreditMemoLineRet.TaxAmount.GetValue();
//                    }


//                    db.tblCreditLine_Insert(TxnID, Conversions.ToDecimal(lineAmount), lineDesc, lineItem, lineRate, customefield, customeFeild2, lineUOM, Conversions.ToDecimal(lineQuantity), lineUOMOrg, lineTxnID, txaCode, (decimal?)taxAmount);
//                caltchnullline:
//                    ;

//                }


//            }



//        }

//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//        bDone = true;
//        bError = true;
//    }
//}


//public static void GetInvoiceTransaction(ref bool bError, DateTime fromDate, DateTime todate)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                IInvoiceQuery RecepitQuery;
//        RecepitQuery = msgSetRequest.AppendInvoiceQueryRq;
//        RecepitQuery.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
//        RecepitQuery.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
//        RecepitQuery.IncludeLineItems.SetValue(true);
//        RecepitQuery.IncludeLinkedTxns.SetValue(true);
//        ' RecepitQuery.IncludeRetElementLis()
//                RecepitQuery.OwnerIDList.Add("0");
//        RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll);
//        ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillInvoiceInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }
//}
//public static void GetInvoiceTransaction(ref bool bError)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                IInvoiceQuery RecepitQuery;
//        RecepitQuery = msgSetRequest.AppendInvoiceQueryRq;
//        'RecepitQuery.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate)
//                 'RecepitQuery.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate)
//                RecepitQuery.IncludeLineItems.SetValue(true);
//        RecepitQuery.IncludeLinkedTxns.SetValue(true);
//        RecepitQuery.OwnerIDList.Add("0");
//        RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll);
//        ' RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillInvoiceInDataBase(ref msgSetResponse, ref bDone, ref bError);
//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }
//}

//public static void FillInvoiceInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    'On Error GoTo Errs
//            try
//    {
//        string RefNumber = string.Empty;
//        check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        Start parsing the response list
//       IResponseList responseList;
//        responseList = msgSetResponse.ResponseList;

//        go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//        response = responseList.GetAt(0);
//        if (response is not null)
//        {
//            if (response.StatusCode != "0")
//            {
//                If the status is bad, report it to the user
//                        Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
//                bDone = true;
//                bError = true;
//                return;
//            }
//        }

//        first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IInvoiceRetList InvoiceRetList;
//        ENResponseType responseType;
//        ENObjectType responseDetailType;
//        responseType = response.Type.GetValue();
//        responseDetailType = response.Detail.Type.GetValue();
//        if (responseType == ENResponseType.rtInvoiceQueryRs & responseDetailType == ENObjectType.otInvoiceRetList)
//        {
//            save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    InvoiceRetList = response.Detail;
//        }
//        else
//        {
//            bail, we do not have the responses we were expecting
//                    bDone = true;
//            bError = true;
//            return;
//        }

//        Parse the query response and add the Customers to the Customer list box
//                short count;
//        short index;
//        IInvoiceRet InvoiceRet;
//        count = InvoiceRetList.Count;
//        short MAX_RETURNED;
//        MAX_RETURNED = 1 + InvoiceRetList.Count;
//        we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//        {
//            bDone = true;
//        }
//        db.tblInvoice_Delete(ClearAllControl.gblCompanyID);
//        db.tblInvoiceLIne_Delete();
//        db.tblInvoiceApplTxn_Delete();
//        var linkTxnDate = default(DateTime);
//        var ServDate = default(DateTime);
//        var loopTo = (short)(count - 1);
//        for (index = 0; index <= loopTo; index++)
//        {
//            skip this customer if this is a repeat from the last query
//                    InvoiceRet = InvoiceRetList.GetAt(index);
//            if (InvoiceRet is null | InvoiceRet.CustomerRef.ListID is null | InvoiceRet.TxnID is null)

//            {
//                bDone = true;
//                bError = true;
//                return;
//            }
//            if (index == 5)
//            {
//                index = index;
//            }

//            'declare varibale to retrive data 
//                    string BillAddress1 = string.Empty;
//            string BillAddress2 = string.Empty;
//            string BillAddress3 = string.Empty;
//            string BillAddress4 = string.Empty;
//            string BillAddress5 = string.Empty;
//            string BillAddressCity = string.Empty;
//            string BillAddressState = string.Empty;
//            string BillAddressPostalCode = string.Empty;
//            string BillAddressCountry = string.Empty;
//            string BillAddressNote = string.Empty;


//            string TxnID = InvoiceRet.TxnID.GetValue;
//            string CustomerRefKey = string.Empty;
//            if (InvoiceRet.CustomerRef is not null)
//            {
//                CustomerRefKey = InvoiceRet.CustomerRef.ListID.GetValue;
//            }
//            string customerName = string.Empty;
//            if (InvoiceRet.CustomerRef is not null)
//            {
//                customerName = InvoiceRet.CustomerRef.FullName.GetValue;
//            }


//            string currencyRef = string.Empty;
//            if (InvoiceRet.CurrencyRef is not null)
//            {
//                currencyRef = InvoiceRet.CurrencyRef.FullName.GetValue;
//            }

//            string ExchangeRAte = string.Empty;
//            if (InvoiceRet.ExchangeRate is not null)
//            {
//                ExchangeRAte = InvoiceRet.ExchangeRate.GetValue;
//            }



//            string ClassRefKey = string.Empty;
//            if (InvoiceRet.ClassRef is not null)
//            {
//                ClassRefKey = InvoiceRet.ClassRef.FullName.GetValue;
//            }

//            string ARAccountRefKey = string.Empty;
//            if (InvoiceRet.ARAccountRef is not null)
//            {
//                ARAccountRefKey = InvoiceRet.ARAccountRef.FullName.GetValue;

//            }

//            string TemplateRefKey = string.Empty;
//            if (InvoiceRet.TemplateRef is not null)
//            {
//                TemplateRefKey = InvoiceRet.TemplateRef.ListID.GetValue;
//            }
//            DateTime TxnDate = InvoiceRet.TxnDate.GetValue;



//            if (InvoiceRet.RefNumber is not null)
//            {
//                RefNumber = InvoiceRet.RefNumber.GetValue;
//            }
//            if (RefNumber == "12" | RefNumber == "09613")
//            {
//                int m = 0;
//            }

//            Bill Address


//                    if (InvoiceRet.BillAddress is not null)
//            {


//                if (InvoiceRet.BillAddress.Addr1 is not null)
//                {
//                    BillAddress1 = InvoiceRet.BillAddress.Addr1.GetValue;
//                }


//                if (InvoiceRet.BillAddress.Addr2 is not null)
//                {
//                    BillAddress2 = InvoiceRet.BillAddress.Addr2.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr3 is not null)
//                {
//                    BillAddress3 = InvoiceRet.BillAddress.Addr3.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr4 is not null)
//                {
//                    BillAddress4 = InvoiceRet.BillAddress.Addr4.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Addr5 is not null)
//                {
//                    BillAddress5 = InvoiceRet.BillAddress.Addr5.GetValue;
//                }

//                if (InvoiceRet.BillAddress.City is not null)
//                {
//                    BillAddressCity = InvoiceRet.BillAddress.City.GetValue;
//                }

//                if (InvoiceRet.BillAddress.State is not null)
//                {
//                    BillAddressState = InvoiceRet.BillAddress.State.GetValue;
//                }

//                if (InvoiceRet.BillAddress.PostalCode is not null)
//                {
//                    BillAddressPostalCode = InvoiceRet.BillAddress.PostalCode.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Country is not null)
//                {
//                    BillAddressCountry = InvoiceRet.BillAddress.Country.GetValue;
//                }

//                if (InvoiceRet.BillAddress.Note is not null)
//                {
//                    BillAddressNote = InvoiceRet.BillAddress.Note.GetValue;
//                }
//            }
//            Ship address
//                    string ShipAddress1 = string.Empty;
//            string ShipAddress2 = string.Empty;
//            string ShipAddress3 = string.Empty;
//            string ShipAddress4 = string.Empty;
//            string ShipAddress5 = string.Empty;
//            string ShipAddressCity = string.Empty;
//            string ShipAddressState = string.Empty;
//            string ShipAddressPostalCode = string.Empty;
//            string ShipAddressCountry = string.Empty;
//            string ShipAddressNote = string.Empty;
//            if (InvoiceRet.ShipAddress is not null)
//            {

//                if (InvoiceRet.ShipAddress.Addr1 is not null)
//                {
//                    ShipAddress1 = InvoiceRet.ShipAddress.Addr1.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr2 is not null)
//                {
//                    ShipAddress2 = InvoiceRet.ShipAddress.Addr2.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr3 is not null)
//                {
//                    ShipAddress3 = InvoiceRet.ShipAddress.Addr3.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr4 is not null)
//                {
//                    ShipAddress4 = InvoiceRet.ShipAddress.Addr4.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Addr5 is not null)
//                {
//                    ShipAddress5 = InvoiceRet.ShipAddress.Addr5.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.City is not null)
//                {
//                    ShipAddressCity = InvoiceRet.ShipAddress.City.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.State is not null)
//                {
//                    ShipAddressState = InvoiceRet.ShipAddress.State.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.PostalCode is not null)
//                {
//                    ShipAddressPostalCode = InvoiceRet.ShipAddress.PostalCode.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Country is not null)
//                {
//                    ShipAddressCountry = InvoiceRet.ShipAddress.Country.GetValue;
//                }

//                if (InvoiceRet.ShipAddress.Note is not null)
//                {
//                    ShipAddressNote = InvoiceRet.ShipAddress.Note.GetValue;
//                }
//            }
//            End ship address

//                    string IsPending = string.Empty;
//            if (InvoiceRet.IsPending is not null)
//            {
//                IsPending = InvoiceRet.IsPending.GetValue;
//                if (Conversions.ToBoolean(IsPending) == true)
//                {
//                    Interaction.MsgBox("stop here");
//                }
//            }
//            string PONumber = string.Empty;
//            if (InvoiceRet.PONumber is not null)
//            {
//                PONumber = InvoiceRet.PONumber.GetValue;
//            }

//            string TermsRefKey = string.Empty;
//            if (InvoiceRet.TermsRef is not null)
//            {
//                TermsRefKey = InvoiceRet.TermsRef.FullName.GetValue;
//            }

//            string DueDate = string.Empty;
//            if (InvoiceRet.DueDate is not null)
//            {
//                DueDate = InvoiceRet.DueDate.GetValue;
//            }

//            string SalesRefKey = string.Empty;
//            if (InvoiceRet.SalesRepRef is not null)
//            {
//                SalesRefKey = InvoiceRet.SalesRepRef.FullName.GetValue;
//            }

//            string FOB = string.Empty;
//            if (InvoiceRet.FOB is not null)
//            {
//                FOB = InvoiceRet.FOB.GetValue;
//            }

//            string ShipDate = string.Empty;
//            if (InvoiceRet.ShipDate is not null)
//            {
//                ShipDate = InvoiceRet.ShipDate.GetValue;
//            }

//            string ShipMethodRefKey = string.Empty;
//            if (InvoiceRet.ShipMethodRef is not null)
//            {
//                ShipMethodRefKey = InvoiceRet.ShipMethodRef.FullName.GetValue;
//            }

//            string ItemSalesTaxRefKey = string.Empty;
//            if (InvoiceRet.ItemSalesTaxRef is not null)
//            {
//                ItemSalesTaxRefKey = InvoiceRet.ItemSalesTaxRef.ListID.GetValue;
//            }

//            string Memo = string.Empty;
//            if (InvoiceRet.Memo is not null)
//            {
//                Memo = InvoiceRet.Memo.GetValue;
//            }

//            string CustomerMsgRefKey = string.Empty;
//            if (InvoiceRet.CustomerMsgRef is not null)
//            {
//                CustomerMsgRefKey = InvoiceRet.CustomerMsgRef.ListID.GetValue;
//            }
//            string IsToBePrinted = string.Empty;
//            if (InvoiceRet.IsToBePrinted is not null)
//            {
//                IsToBePrinted = InvoiceRet.IsToBePrinted.GetValue;
//            }
//            string IsToEmailed = string.Empty;
//            if (InvoiceRet.IsToBeEmailed is not null)
//            {
//                IsToEmailed = InvoiceRet.IsToBeEmailed.GetValue;
//            }
//            string IsTaxIncluded = string.Empty;
//            if (InvoiceRet.IsTaxIncluded is not null)
//            {
//                IsTaxIncluded = InvoiceRet.IsTaxIncluded.GetValue;
//            }
//            string CustomerSalesTaxCodeRefKey = string.Empty;
//            if (InvoiceRet.CustomerSalesTaxCodeRef is not null)
//            {
//                CustomerSalesTaxCodeRefKey = InvoiceRet.CustomerSalesTaxCodeRef.ListID.GetValue;
//            }
//            string Other = string.Empty;
//            if (InvoiceRet.Other is not null)
//            {
//                Other = InvoiceRet.Other.GetValue;
//            }
//            string Amount = "0";
//            if (InvoiceRet.BalanceRemaining is not null)
//            {
//                Amount = InvoiceRet.BalanceRemaining.GetValue;
//            }
//            string AmountPaid = "0";
//            if (InvoiceRet.AppliedAmount is not null)
//            {
//                AmountPaid = InvoiceRet.AppliedAmount.GetValue;
//            }
//            string CustomField1 = string.Empty;
//            string CustomField2 = string.Empty;
//            string CustomField3 = string.Empty;
//            string CustomField4 = string.Empty;
//            string CustomField5 = string.Empty;
//            string QuotationRecNo = string.Empty;
//            string GradeID = string.Empty;
//            string InvoiceType = string.Empty;
//            string subTotal = null;
//            string SalesTaxTotal = null;
//            string SalesTaxPercentage = null;
//            if (InvoiceRet.Subtotal is not null)
//            {
//                subTotal = InvoiceRet.Subtotal.GetValue;
//            }



//            if (InvoiceRet.DataExtRetList is not null)
//            {
//                IDataExtRet extInvLst;
//                string dataExtaname = string.Empty;
//                string dataExtValue = string.Empty;
//                for (int g = 0, loopTo1 = InvoiceRet.DataExtRetList.Count - 1; g <= loopTo1; g++)
//                {
//                    extInvLst = InvoiceRet.DataExtRetList.GetAt(g);
//                    dataExtaname = extInvLst.DataExtName.GetValue();
//                    dataExtValue = extInvLst.DataExtValue.GetValue();
//                    if (dataExtaname == "TRN")
//                    {
//                        CustomField1 = dataExtValue;
//                    }
//                    else if (dataExtaname == "CODE")
//                    {
//                        CustomField2 = dataExtValue;
//                    }
//                    else if (dataExtaname == "Sales Man Mob:")
//                    {
//                        CustomField3 = dataExtValue;
//                    }
//                    else if (dataExtaname == "Customer TRN")
//                    {
//                        CustomField4 = dataExtValue;
//                    }
//                }

//            }

//            if (InvoiceRet.SalesTaxTotal is not null)
//            {
//                SalesTaxTotal = InvoiceRet.SalesTaxTotal.GetValue;
//            }
//            if (InvoiceRet.SalesTaxPercentage is not null)
//            {
//                SalesTaxPercentage = InvoiceRet.SalesTaxPercentage.GetValue;
//            }
//            'Print(RefNumber)

//                     Insert data in database code left..........db.tblInvoice_Insert(ClearAllControl.gblCompanyID, TxnID, CustomerRefKey, currencyRef, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, Conversions.ToDate(DueDate), SalesRefKey, FOB, ShipDate, ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, Amount, AmountPaid, CustomField1, CustomField2, CustomField3, currencyRef, CustomField5, QuotationRecNo, ExchangeRAte, InvoiceType, customerName, subTotal, SalesTaxTotal, SalesTaxPercentage);
//            if (InvoiceRet.LinkedTxnList is not null)
//            {
//                for (int p = 0, loopTo2 = InvoiceRet.LinkedTxnList.Count - 1; p <= loopTo2; p++)
//                {
//                    ILinkedTxn invoiceLinkTxn;
//                    invoiceLinkTxn = InvoiceRet.LinkedTxnList.GetAt(p);
//                    string LinkRefNum = string.Empty;
//                    if (invoiceLinkTxn.RefNumber is not null)
//                    {
//                        LinkRefNum = invoiceLinkTxn.RefNumber.GetValue();
//                    }
//                    string linktXnAmount = string.Empty;
//                    if (invoiceLinkTxn.Amount is not null)
//                    {
//                        linktXnAmount = invoiceLinkTxn.Amount.GetValue();
//                    }
//                    if (invoiceLinkTxn.TxnDate is not null)
//                    {
//                        linkTxnDate = invoiceLinkTxn.TxnDate.GetValue;
//                    }
//                    db.tblInvoiceApplTxn_Insert(RefNumber, LinkRefNum, Conversions.ToString(linkTxnDate), linktXnAmount, TxnDate);
//                }
//            }


//            if (InvoiceRet.ORInvoiceLineRetList is not null)
//            {
//                for (int k = 0, loopTo3 = InvoiceRet.ORInvoiceLineRetList.Count - 1; k <= loopTo3; k++)
//                {
//                    IORInvoiceLineRet InvoiceLineRetList;
//                    InvoiceLineRetList = InvoiceRet.ORInvoiceLineRetList.GetAt(k);


//                    string lineItem = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.ItemRef is not null)
//                    {
//                        lineItem = InvoiceLineRetList.InvoiceLineRet.ItemRef.FullName.GetValue();
//                    }


//                    string lineDesc = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.Desc is not null)
//                    {
//                        lineDesc = InvoiceLineRetList.InvoiceLineRet.Desc.GetValue();
//                    }

//                    string lineQuantity = "0";
//                    string lineRate = "0";
//                    if (lineItem != "Subtotal")
//                    {

//                        if (InvoiceLineRetList.InvoiceLineRet.Quantity is not null)
//                        {
//                            lineQuantity = InvoiceLineRetList.InvoiceLineRet.Quantity.GetValue;
//                        }

//                        if (!string.IsNullOrEmpty(lineItem))
//                        {
//                            if (InvoiceLineRetList.InvoiceLineRet.ORRate.Rate is not null)
//                            {
//                                lineRate = InvoiceLineRetList.InvoiceLineRet.ORRate.Rate.GetValue();
//                            }
//                        }

//                    }
//                    string lineAmount = "0";
//                    if (InvoiceLineRetList.InvoiceLineRet.Amount is not null)
//                    {
//                        lineAmount = InvoiceLineRetList.InvoiceLineRet.Amount.GetValue();
//                    }




//                    string lineUOM = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.UnitOfMeasure is not null)
//                    {
//                        lineUOM = InvoiceLineRetList.InvoiceLineRet.UnitOfMeasure.GetValue();
//                    }
//                    Dim lineQuantity As String = String.Empty
//                             If Not InvoiceLineRetList.InvoiceLineRet.Quantity Is Nothing Then
//                             lineQuantity = InvoiceLineRetList.InvoiceLineRet.Quantity.GetValue()
//                             End If
//                            string lineUOMOrg = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.OverrideUOMSetRef is not null)
//                    {
//                        lineUOMOrg = InvoiceLineRetList.InvoiceLineRet.OverrideUOMSetRef.FullName.GetValue;
//                    }
//                    string lineTxnID = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.TxnLineID is not null)
//                    {
//                        lineTxnID = InvoiceLineRetList.InvoiceLineRet.TxnLineID.GetValue();
//                    }
//                    string txaCode = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.SalesTaxCodeRef is not null)
//                    {
//                        if (InvoiceLineRetList.InvoiceLineRet.SalesTaxCodeRef.FullName is not null)
//                        {
//                            txaCode = InvoiceLineRetList.InvoiceLineRet.SalesTaxCodeRef.FullName.GetValue();
//                        }
//                    }

//                    double taxAmount = 0d;
//                    if (InvoiceLineRetList.InvoiceLineRet.TaxAmount is not null)
//                    {
//                        taxAmount = InvoiceLineRetList.InvoiceLineRet.TaxAmount.GetValue();
//                    }

//                    string other1 = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.Other1 is not null)
//                    {
//                        other1 = InvoiceLineRetList.InvoiceLineRet.Other1.GetValue;
//                    }

//                    string other2 = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.Other2 is not null)
//                    {
//                        other2 = InvoiceLineRetList.InvoiceLineRet.Other2.GetValue;
//                    }
//                    if (InvoiceLineRetList.InvoiceLineRet.ServiceDate is not null)
//                    {
//                        ServDate = InvoiceLineRetList.InvoiceLineRet.ServiceDate.GetValue;
//                    }

//                    string customefield = string.Empty;
//                    string customeFeild2 = string.Empty;
//                    if (InvoiceLineRetList.InvoiceLineRet.DataExtRetList is not null)
//                    {
//                        IDataExtRet extInvLine;
//                        string dataExtanameLine = string.Empty;
//                        string dataExtValueLine = string.Empty;
//                        for (int p = 0, loopTo4 = InvoiceLineRetList.InvoiceLineRet.DataExtRetList.Count - 1; p <= loopTo4; p++)
//                        {
//                            extInvLine = InvoiceLineRetList.InvoiceLineRet.DataExtRetList.GetAt(p);
//                            dataExtanameLine = extInvLine.DataExtName.GetValue();
//                            dataExtValueLine = extInvLine.DataExtValue.GetValue();
//                            if (dataExtanameLine == "TEST")
//                            {
//                                customefield = dataExtValueLine;
//                            }
//                            else if (dataExtValueLine == "DN")
//                            {
//                                CustomField2 = dataExtValueLine;
//                            }
//                        }

//                    }


//                    db.tblInvoiceLine_Insert(TxnID, Conversions.ToDecimal(lineAmount), lineDesc, lineItem, lineRate, customefield, customeFeild2, lineUOM, Conversions.ToDecimal(lineQuantity), lineUOMOrg, lineTxnID, txaCode, (decimal?)taxAmount, other1, other2, ServDate);
//                }


//            }



//        }

//        return;
//    Errs:
//        ;
//    }
//    catch
//    {

//        int w = 0;
//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + "-" + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//        bDone = true;
//        bError = true;
//    }
//}

//public static void FillReceivePaymentInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    try
//    {

//        check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        Start parsing the response list
//       IResponseList responseList;
//        responseList = msgSetResponse.ResponseList;

//        go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//        response = responseList.GetAt(0);
//        if (response is not null)
//        {
//            if (response.StatusCode != "0")
//            {
//                If the status is bad, report it to the user
//                        Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
//                bDone = true;
//                bError = true;
//                return;
//            }
//        }

//        first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                IReceivePaymentRetList receivePaymentRetList;
//        ENResponseType responseType;
//        ENObjectType responseDetailType;
//        responseType = response.Type.GetValue();
//        responseDetailType = response.Detail.Type.GetValue();
//        if (responseType == ENResponseType.rtReceivePaymentQueryRs & responseDetailType == ENObjectType.otReceivePaymentRetList)
//        {
//            save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    receivePaymentRetList = response.Detail;
//        }
//        else
//        {
//            bail, we do not have the responses we were expecting
//                    bDone = true;
//            bError = true;
//            return;
//        }

//        Parse the query response and add the Customers to the Customer list box
//       var count = default(short);
//        short index;
//        IReceivePaymentRet receivePaymetRet;

//        short MAX_RETURNED;
//        MAX_RETURNED = 1 + receivePaymentRetList.Count;
//        we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//        {
//            bDone = true;
//        }
//        db.tblReceivePayment_Delete(ClearAllControl.gblCompanyID.ToString());
//        db.tblReceiveLine_delete();
//        count = receivePaymentRetList.Count;
//        var TimeCreated = default(DateTime);
//        var TimeModified = default(DateTime);
//        var linetxnDate = default(DateTime);
//        var loopTo = (short)(count - 1);
//        for (index = 0; index <= loopTo; index++)
//        {
//            if (index == 106)
//            {
//                int w = 0;
//            }

//            skip this customer if this is a repeat from the last query
//                    receivePaymetRet = receivePaymentRetList.GetAt(index);
//            if (receivePaymetRet is null | receivePaymetRet.CustomerRef.ListID is null | receivePaymetRet.TxnID is null)

//            {
//                bDone = true;
//                bError = true;
//                return;
//            }

//            string ARAccountRefListID = string.Empty;
//            string ARAccountFullName = string.Empty;
//            if (receivePaymetRet.ARAccountRef.ListID is not null)
//            {
//                ARAccountRefListID = receivePaymetRet.ARAccountRef.ListID.GetValue;
//                ARAccountFullName = receivePaymetRet.ARAccountRef.FullName.GetValue;
//            }

//            Dim linkTxn
//                     If Not(receivePaymetRet.AppliedToTxnRetList Is Nothing) Then
//                    linkTxn = receivePaymetRet.AppliedToTxnRetList.Append.RefNumber.GetValue
//                     End If
//                    string CurrencyRefListID = string.Empty;
//            string CurrencyRefFullName = string.Empty;
//            if (receivePaymetRet.CurrencyRef is not null)
//            {
//                CurrencyRefListID = receivePaymetRet.CurrencyRef.ListID.GetValue;
//                CurrencyRefFullName = receivePaymetRet.CurrencyRef.FullName.GetValue;
//            }
//            string CustomerRefListID = string.Empty;
//            string CustomerRefFullName = string.Empty;
//            if (receivePaymetRet.CustomerRef is not null)
//            {
//                CustomerRefListID = receivePaymetRet.CustomerRef.ListID.GetValue;
//                CustomerRefFullName = receivePaymetRet.CustomerRef.FullName.GetValue;
//            }
//            string DepositToAccountRefListID = string.Empty;
//            string DepositeToAccountRefFullName = string.Empty;
//            if (receivePaymetRet.DepositToAccountRef is not null)
//            {
//                DepositToAccountRefListID = receivePaymetRet.DepositToAccountRef.ListID.GetValue;
//                DepositeToAccountRefFullName = receivePaymetRet.DepositToAccountRef.FullName.GetValue;
//            }
//            string EditSequence = string.Empty;
//            if (receivePaymetRet.EditSequence is not null)
//            {
//                EditSequence = receivePaymetRet.EditSequence.GetValue();
//            }
//            string ExchangeRate = string.Empty;
//            if (receivePaymetRet.ExchangeRate is not null)
//            {
//                ExchangeRate = receivePaymetRet.ExchangeRate.GetValue();
//            }
//            string ExternalGUID = string.Empty;
//            if (receivePaymetRet.ExternalGUID is not null)
//            {
//                ExternalGUID = receivePaymetRet.ExternalGUID.GetValue();
//            }
//            string RefNumber = string.Empty;
//            if (receivePaymetRet.RefNumber is not null)
//            {
//                RefNumber = receivePaymetRet.RefNumber.GetValue();
//            }
//            if (RefNumber == "1799")
//            {
//                int k = 9;
//            }

//            string memo = string.Empty;
//            if (receivePaymetRet.Memo is not null)
//            {
//                memo = receivePaymetRet.Memo.GetValue();
//            }
//            string PaymentMethodRefListID = string.Empty;
//            string PaymentMethodRefFullName = string.Empty;
//            if (receivePaymetRet.PaymentMethodRef is not null)
//            {
//                PaymentMethodRefListID = receivePaymetRet.PaymentMethodRef.ListID.GetValue;
//                PaymentMethodRefFullName = receivePaymetRet.PaymentMethodRef.FullName.GetValue;
//            }
//            if (receivePaymetRet.TimeCreated is not null)
//            {
//                TimeCreated = receivePaymetRet.TimeCreated.GetValue;
//            }
//            if (receivePaymetRet.TimeModified is not null)
//            {
//                TimeModified = receivePaymetRet.TimeModified.GetValue();
//            }
//            decimal TotalAmount = 0m;
//            if (receivePaymetRet.TotalAmount is not null)
//            {
//                TotalAmount = receivePaymetRet.TotalAmount.GetValue();
//            }
//            decimal TotalAmountInHOmeCurrency = 0m;
//            if (receivePaymetRet.TotalAmountInHomeCurrency is not null)
//            {
//                TotalAmountInHOmeCurrency = receivePaymetRet.TotalAmountInHomeCurrency.GetValue();
//            }
//            string TxnDate = string.Empty;
//            if (receivePaymetRet.TxnDate is not null)
//            {
//                TxnDate = receivePaymetRet.TxnDate.GetValue();
//            }
//            string TxnId = string.Empty;
//            if (receivePaymetRet.TxnID is not null)
//            {
//                TxnId = receivePaymetRet.TxnID.GetValue();
//            }
//            string TxnNumber = string.Empty;
//            if (receivePaymetRet.TxnNumber is not null)
//            {
//                TxnNumber = receivePaymetRet.TxnNumber.GetValue();
//            }
//            string Type = string.Empty;
//            if (receivePaymetRet.Type is not null)
//            {
//                Type = receivePaymetRet.Type.GetValue();
//            }
//            decimal UnusedCredit = 0m;
//            if (receivePaymetRet.UnusedCredits is not null)
//            {
//                UnusedCredit = receivePaymetRet.UnusedCredits.GetValue();
//            }
//            string UnUsedPayment = string.Empty;
//            if (receivePaymetRet.UnusedPayment is not null)
//            {
//                UnUsedPayment = receivePaymetRet.UnusedPayment.GetValue();
//            }
//            string newmemo = string.Empty;
//            string checkDate = string.Empty;
//            string checkNumber = string.Empty;
//            string customerBankName = string.Empty;
//            string compaireDate = "10/04/2015";
//            object tempString;
//            DateTime tempDate = Conversions.ToDate(TxnDate);
//            int countStr;
//            string cashCustomerName;
//            string memoString = string.Empty;
//            tempString = Strings.Format(tempDate, "dd/MM/yyy").ToString();
//            if (PaymentMethodRefFullName == "Cheque")
//            {

//                tempString = Strings.Split(memo, ":");
//                countStr = CountCharacter(memo, ':');
//                if (countStr >= 2)
//                {
//                    checkDate = Conversions.ToString(tempString((object)1));
//                    checkNumber = Conversions.ToString(tempString((object)0));
//                    customerBankName = Conversions.ToString(tempString((object)2));

//                    for (int j = 3, loopTo1 = countStr; j <= loopTo1; j++)
//                        memoString = Conversions.ToString(Operators.AddObject(memoString + " ", tempString((object)j)));
//                }
//                tempString = Split(memoString, ":")
//                         countStr = CountCharacter(memo, ":")
//                         If countStr > 0 Then
//                         cashCustomerName = tempString(1)
//                         memoString = tempString(0)
//                         End If

//                        else
//                {
//                    memoString = memo;
//                    tempString = Strings.Split(memoString, ":");
//                    countStr = CountCharacter(memo, ':');
//                    if (countStr > 0)
//                    {
//                        cashCustomerName = Conversions.ToString(tempString((object)1));
//                        memoString = Conversions.ToString(tempString((object)0));
//                    }
//                }
//            }
//            MsgBox("Sc")
//                    else if (PaymentMethodRefFullName == "Check")
//            {
//                tempString = Strings.Split(memo, ":");
//                countStr = CountCharacter(memo, ':');
//                if (countStr > 2)
//                {
//                    checkDate = Conversions.ToString(tempString((object)1));
//                    checkNumber = Conversions.ToString(tempString((object)0));
//                    customerBankName = Conversions.ToString(tempString((object)2));

//                    for (int j = 3, loopTo2 = countStr; j <= loopTo2; j++)
//                        memoString = Conversions.ToString(Operators.AddObject(memoString + " ", tempString((object)j)));
//                }
//                tempString = Split(memoString, ":")
//                         countStr = CountCharacter(memo, ":")
//                         If countStr > 0 Then
//                         cashCustomerName = tempString(1)
//                         memoString = tempString(0)
//                         End If

//                        else
//                {
//                    memoString = memo;
//                    tempString = Strings.Split(memoString, ":");
//                    countStr = CountCharacter(memo, ':');
//                    if (countStr > 0)
//                    {
//                        cashCustomerName = Conversions.ToString(tempString((object)1));
//                        memoString = Conversions.ToString(tempString((object)0));
//                    }
//                }
//            }
//            else
//            {
//                memoString = memo;
//                tempString = Split(memoString, ":")
//                         countStr = CountCharacter(memo, ":")
//                         If countStr > 0 Then
//                         cashCustomerName = tempString(1)
//                         memoString = tempString(0)
//                         End If
//                         memoString = memoString
//                    }
//            Insert data in database code left..........db.tblInvoice_Insert(gblCompanyID, TxnID, CustomerRefKey, Nothing, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, DueDate, SalesRefKey, FOB, ShipDate, ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, Amount, AmountPaid, CustomField1, CustomField2, CustomField3, CustomField4, CustomField5, QuotationRecNo, GradeID, InvoiceType, customerName, subTotal, SalesTaxTotal, SalesTaxPercentage)

//                    db.tblReceivePayment_Insert(ARAccountRefListID, ARAccountFullName, CurrencyRefListID, CurrencyRefFullName, CustomerRefListID, CustomerRefFullName, DepositToAccountRefListID, DepositeToAccountRefFullName, EditSequence, ExchangeRate, ExternalGUID, memoString, PaymentMethodRefListID, PaymentMethodRefFullName, RefNumber, TimeCreated, TimeModified, TotalAmount, TotalAmountInHOmeCurrency, Conversions.ToDate(TxnDate), TxnId, TxnNumber, Type, UnusedCredit, Conversions.ToDecimal(UnUsedPayment), ClearAllControl.gblCompanyID.ToString(), checkDate, checkNumber, customerBankName);
//            cashCustomerName = string.Empty;
//            IAppliedToTxnRet appliedTxnRet;
//            ' If Not CheckBillRet.AppliedToTxnRetList Is Nothing Then

//                    if (receivePaymetRet.AppliedToTxnRetList is not null)
//            {
//                'appliedTxnRet = CheckBillRet.AppliedToTxnRetList.GetAt(j)
//                        for (int k = 0, loopTo3 = receivePaymetRet.AppliedToTxnRetList.Count - 1; k <= loopTo3; k++)
//                {
//                    IAppliedToTxnRet AppTxntRetList;
//                    AppTxntRetList = receivePaymetRet.AppliedToTxnRetList.GetAt(k);
//                    string lineRefNumber = string.Empty;
//                    if (AppTxntRetList.RefNumber is not null)
//                    {
//                        lineRefNumber = AppTxntRetList.RefNumber.GetValue;
//                    }
//                    if (AppTxntRetList.TxnDate is not null)
//                    {
//                        linetxnDate = AppTxntRetList.TxnDate.GetValue;
//                    }

//                    double lineAmount = 0d;
//                    if (AppTxntRetList.Amount is not null)
//                    {
//                        lineAmount = AppTxntRetList.Amount.GetValue;
//                    }

//                    double lineBalance = 0d;
//                    if (AppTxntRetList.BalanceRemaining is not null)
//                    {
//                        lineBalance = AppTxntRetList.BalanceRemaining.GetValue;
//                    }
//                    db.tblReceiveLine_insert(TxnId, lineRefNumber, linetxnDate, (decimal?)lineAmount, (decimal?)lineBalance);
//                }




//            }

//        }

//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//        bDone = true;
//        bError = true;
//    }
//}
//public static int CountCharacter(string value, char ch)
//{
//    int cnt = 0;
//    foreach (char c in value)
//    {
//        if (c == ch)
//            cnt += 1;
//    }
//    return cnt;
//}
//private static bool FoundInvoiceInDatabase(ref string txnID)
//{
//    bool FoundInvoiceInDatabaseRet = default;
//    try
//    {

//        FoundInvoiceInDatabaseRet = false;
//        check in database for existing customer bill

//       var result = db.tblInvoice_Select(ClearAllControl.gblCompanyID, txnID, default, null, default, null, default);

//        int resultCount = result.Count();

//        if (!(resultCount == 0))
//        {
//            FoundInvoiceInDatabaseRet = true;
//        }

//        return FoundInvoiceInDatabaseRet;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FoundIn Account database");

//    }

//}


//Get all customer list
//        public static void GetCustomerList(ref bool bError)
//{
//    try
//    {
//        make sure we do not have any old requests still defined
//                msgSetRequest.ClearRequests();
//        set the OnError attribute to continueOnError
//                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
//        Add the BillQuery request
//                ICustomerQuery CustomerQuery;
//        CustomerQuery = msgSetRequest.AppendCustomerQueryRq;
//        CustomerQuery.IncludeRetElementList.Add("ListID")
//                 CustomerQuery.IncludeRetElementList.Add("TimeCreated")
//                 CustomerQuery.IncludeRetElementList.Add("TimeModified")
//                 CustomerQuery.IncludeRetElementList.Add("EditSequence")
//                 CustomerQuery.IncludeRetElementList.Add("Name")
//                 CustomerQuery.IncludeRetElementList.Add("FullName")
//                 CustomerQuery.IncludeRetElementList.Add("IsActive ")
//                 CustomerQuery.IncludeRetElementList.Add("ClassRef")
//                 CustomerQuery.IncludeRetElementList.Add("Sublevel")
//                 CustomerQuery.IncludeRetElementList.Add("CompanyName")
//                 CustomerQuery.IncludeRetElementList.Add("BillAddress")
//                 CustomerQuery.IncludeRetElementList.Add("ShipAddress")
//                 CustomerQuery.IncludeRetElementList.Add("Phone")
//                 CustomerQuery.IncludeRetElementList.Add("Email")
//                 CustomerQuery.IncludeRetElementList.Add("Cc")

//                 CustomerQuery.IncludeRetElementList.Add("Balance")
//                 CustomerQuery.IncludeRetElementList.Add("TotalBalance")
//                 CustomerQuery.IncludeRetElementList.Add("Cc")


//                 CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//                bool bDone = false;
//        while (!bDone)
//        {
//            start looking for customer next in the list


//            send the request to QB

//           IMsgSetResponse msgSetResponse;
//            msgSetResponse = qbSessionManager.DoRequests(msgSetRequest);

//            MsgBox(msgSetRequest.ToXMLString())
//                    FillCustomerListInDataBase(ref msgSetResponse, ref bDone, ref bError);


//        }
//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
//        bError = true;
//    }
//}

//public static void FillCustomerListInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
//{
//    try
//    {

//        check to make sure we have objects to access first
//                 and that there are responses in the list
//                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        Start parsing the response list
//       IResponseList responseList;
//        responseList = msgSetResponse.ResponseList;

//        go thru each response and process the response.
//                 this example will only have one response in the list
//                 so we will look at index = 0
//                IResponse response;
//        response = responseList.GetAt(0);
//        if (response is not null)
//        {
//            if (response.StatusCode != "0")
//            {
//                If the status is bad, report it to the user
//                        Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
//                bDone = true;
//                bError = true;
//                return;
//            }
//        }

//        first make sure we have a response object to handle
//                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


//        {
//            bDone = true;
//            bError = true;
//            return;
//        }

//        make sure we are processing the CustomerQueryRs and
//                 the CustomerRetList responses in this response list
//                ICustomerRetList CustomerRetList;
//        ENResponseType responseType;
//        ENObjectType responseDetailType;
//        responseType = response.Type.GetValue();
//        responseDetailType = response.Detail.Type.GetValue();
//        if (responseType == ENResponseType.rtCustomerQueryRs & responseDetailType == ENObjectType.otCustomerRetList)
//        {
//            save the response detail in the appropriate object type
//                     since we have first verified the type of the response object
//                    CustomerRetList = response.Detail;
//        }
//        else
//        {
//            bail, we do not have the responses we were expecting
//                    bDone = true;
//            bError = true;
//            return;
//        }

//        Parse the query response and add the Customers to the Customer list box
//                short count;
//        short index;
//        ICustomerRet CustomerRet;
//        count = CustomerRetList.Count;
//        short MAX_RETURNED;
//        MAX_RETURNED = 1 + CustomerRetList.Count;
//        we are done with the customerQueries if we have not received the MaxReturned
//                if (count < MAX_RETURNED)
//        {
//            bDone = true;
//        }

//        var loopTo = (short)(count - 1);
//        for (index = 0; index <= loopTo; index++)
//        {
//            skip this customer if this is a repeat from the last query
//                    CustomerRet = CustomerRetList.GetAt(index);
//            if (CustomerRet is null | CustomerRet.ListID is null)
//            {
//                bDone = true;
//                bError = true;
//                return;
//            }


//            'declare varibale to retrive data            

//                    string ListID = CustomerRet.ListID.GetValue;
//            DateTime TimeCreated = default;
//            if (CustomerRet.TimeCreated is not null)
//            {
//                TimeCreated = CustomerRet.TimeCreated.GetValue;
//            }

//            DateTime TimeModified = default;
//            if (CustomerRet.TimeModified is not null)
//            {
//                TimeModified = CustomerRet.TimeModified.GetValue;
//            }
//            string EditSequence = string.Empty;
//            if (CustomerRet.EditSequence is not null)
//            {
//                EditSequence = CustomerRet.EditSequence.GetValue;
//            }
//            string Name = string.Empty;
//            if (CustomerRet.Name is not null)
//            {
//                Name = CustomerRet.Name.GetValue;
//            }
//            string ArName = string.Empty;

//            string FullName = string.Empty;
//            if (CustomerRet.FullName is not null)
//            {
//                FullName = CustomerRet.FullName.GetValue;
//            }
//            string Parent = string.Empty;
//            If Not(CustomerRet.ParentRef Is Nothing) Then
//           If Not(CustomerRet.ParentRef.ListID Is Nothing) Then
//          Parent = CustomerRet.ParentRef.ListID.GetValue
//                     End If
//                     End If
//                    string IsActive = string.Empty;
//            if (CustomerRet.IsActive is not null)
//            {
//                IsActive = CustomerRet.IsActive.GetValue;
//            }
//            string Sublevel = string.Empty;
//            if (CustomerRet.Sublevel is not null)
//            {
//                Sublevel = CustomerRet.Sublevel.GetValue;
//            }
//            string CompanyName = string.Empty;
//            if (CustomerRet.CompanyName is not null)
//            {
//                CompanyName = CustomerRet.CompanyName.GetValue;
//            }
//            string Salutation = string.Empty;
//            If Not(CustomerRet.Salutation Is Nothing) Then
//           Salutation = CustomerRet.Salutation.GetValue
//                     End If
//                    string FirstName = string.Empty;
//            If Not(CustomerRet.FirstName Is Nothing) Then
//           FirstName = CustomerRet.FirstName.GetValue
//                     End If
//                    string MiddleName = string.Empty;
//            If Not(CustomerRet.MiddleName Is Nothing) Then
//           MiddleName = CustomerRet.MiddleName.GetValue
//                     End If
//                    string LastName = string.Empty;
//            If Not(CustomerRet.LastName Is Nothing) Then
//           LastName = CustomerRet.LastName.GetValue
//                     End If
//                     bill address details
//                    string BillAddress1 = string.Empty;
//            string BillAddress2 = string.Empty;
//            string BillAddress3 = string.Empty;
//            string BillAddress4 = string.Empty;
//            string BillCityRefKey = string.Empty;
//            string BillCity = string.Empty;
//            string BillStateRefKey = string.Empty;
//            string BillState = string.Empty;
//            string BillPostalCode = string.Empty;
//            string BillCountryRefKey = string.Empty;
//            string BillCountry = string.Empty;


//            if (CustomerRet.BillAddress is not null)
//            {


//                if (CustomerRet.BillAddress.Addr1 is not null)
//                {
//                    BillAddress1 = CustomerRet.BillAddress.Addr1.GetValue;
//                }

//                if (CustomerRet.BillAddress.Addr2 is not null)
//                {
//                    BillAddress2 = CustomerRet.BillAddress.Addr2.GetValue;
//                }

//                if (CustomerRet.BillAddress.Addr3 is not null)
//                {
//                    BillAddress3 = CustomerRet.BillAddress.Addr3.GetValue;
//                }
//                if (CustomerRet.BillAddress.Addr4 is not null)
//                {
//                    BillAddress4 = CustomerRet.BillAddress.Addr4.GetValue;
//                }

//                If Not(CustomerRet.Is Nothing) Then
//              BillCityRefKey = CustomerRet.BillAddress.City.GetValue
//                         End If

//                        if (CustomerRet.BillAddress.City is not null)
//                {
//                    BillCity = CustomerRet.BillAddress.City.GetValue;
//                }

//                If Not(CustomerRet.BillAddress Is Nothing) Then
//               BillStateRefKey = CustomerRet.BillAddress.State.GetValue
//                         End If

//                        if (CustomerRet.BillAddress.State is not null)
//                {
//                    BillState = CustomerRet.BillAddress.State.GetValue;
//                }

//                if (CustomerRet.BillAddress.PostalCode is not null)
//                {
//                    BillPostalCode = CustomerRet.BillAddress.PostalCode.GetValue;
//                }

//                If Not(CustomerRet.BillAddress.Country Is Nothing) Then
//               BillCountryRefKey = CustomerRet.BillAddress.Country.GetValue
//                         End If

//                        if (CustomerRet.BillAddress.Country is not null)
//                {
//                    BillCountry = CustomerRet.BillAddress.Country.GetValue;
//                }
//            }

//            string Phone = string.Empty;
//            if (CustomerRet.Phone is not null)
//            {
//                Phone = CustomerRet.Phone.GetValue;
//            }
//            string AltPhone = string.Empty;
//            If Not(CustomerRet.AltPhone Is Nothing) Then
//           AltPhone = CustomerRet.AltPhone.GetValue
//                     End If
//                    string Fax = string.Empty;
//            If Not(CustomerRet.Fax Is Nothing) Then
//           Fax = CustomerRet.Fax.GetValue
//                     End If
//                    string Email = string.Empty;
//            if (CustomerRet.Email is not null)
//            {
//                Email = CustomerRet.Email.GetValue;
//            }
//            string Cc = string.Empty;
//            string Contact = string.Empty;
//            If Not(CustomerRet.Contact Is Nothing) Then
//           Contact = CustomerRet.Contact.GetValue
//                     End If
//                    string AltContact = string.Empty;
//            If Not(CustomerRet.AltContact Is Nothing) Then
//           AltContact = CustomerRet.AltContact.GetValue
//                     End If


//                     customer type ref
//                    string CustomerTypeName = string.Empty;
//            string CustomerTypeRef = string.Empty;
//            If Not(CustomerRet.CustomerTypeRef Is Nothing) Then
//           If Not(CustomerRet.CustomerTypeRef.ListID Is Nothing) Then
//          CustomerTypeRef = CustomerRet.CustomerTypeRef.ListID.GetValue
//                     End If

//                     If Not(CustomerRet.CustomerTypeRef.FullName Is Nothing) Then
//                    CustomerTypeName = CustomerRet.CustomerTypeRef.FullName.GetValue
//                     End If
//                     End If

//                    string TermsRef = string.Empty;
//            string TermsName = string.Empty;

//            If Not(CustomerRet.TermsRef Is Nothing) Then
//           If Not(CustomerRet.TermsRef.ListID Is Nothing) Then
//          TermsRef = CustomerRet.TermsRef.ListID.GetValue
//                     End If
//                     If Not(CustomerRet.TermsRef.FullName Is Nothing) Then
//                    TermsName = CustomerRet.TermsRef.FullName.GetValue
//                     End If
//                     End If


//                    string SalesRepRef = string.Empty;
//            string SalesRepName = string.Empty;
//            If Not(CustomerRet.SalesRepRef Is Nothing) Then
//           If Not(CustomerRet.SalesRepRef.ListID Is Nothing) Then
//          SalesRepRef = CustomerRet.SalesRepRef.ListID.GetValue
//                     End If
//                     If Not(CustomerRet.SalesRepRef.FullName Is Nothing) Then
//                    SalesRepName = CustomerRet.SalesRepRef.FullName.GetValue
//                     End If
//                     End If


//                    double Balance = 0.0d;
//            if (CustomerRet.Balance is not null)
//            {
//                Balance = CustomerRet.Balance.GetValue;
//            }
//            double TotalBalance = 0.0d;
//            if (CustomerRet.TotalBalance is not null)
//            {
//                TotalBalance = CustomerRet.TotalBalance.GetValue;
//            }

//            string SalesTaxCodeName = string.Empty;
//            string SalesTaxCodeRef = string.Empty;
//            If Not(CustomerRet.SalesTaxCodeRef Is Nothing) Then
//           If Not(CustomerRet.SalesTaxCodeRef.ListID Is Nothing) Then
//          SalesTaxCodeRef = CustomerRet.SalesTaxCodeRef.ListID.GetValue
//                     End If
//                     If Not(CustomerRet.SalesTaxCodeRef.FullName Is Nothing) Then
//                    SalesTaxCodeName = CustomerRet.SalesTaxCodeRef.FullName.GetValue
//                     End If

//                     End If


//                    string AccountNumber = string.Empty;
//            if (CustomerRet.AccountNumber is not null)
//            {
//                AccountNumber = CustomerRet.AccountNumber.GetValue;
//            }
//            string CreditLimit = string.Empty;
//            If Not(CustomerRet.CreditLimit Is Nothing) Then
//           CreditLimit = CustomerRet.CreditLimit.GetValue
//                     End If
//                    string JobStatus = string.Empty;
//            If Not(CustomerRet.JobStatus Is Nothing) Then
//           JobStatus = CustomerRet.JobStatus.GetValue
//                     End If
//                    DateTime JobStartDate = default;
//            If Not(CustomerRet.JobStartDate Is Nothing) Then
//           JobStartDate = CustomerRet.JobStartDate.GetValue
//                     End If
//                    DateTime JobProjectedEndDate = default;
//            If Not(CustomerRet.JobProjectedEndDate Is Nothing) Then
//           JobProjectedEndDate = CustomerRet.JobProjectedEndDate.GetValue
//                     End If
//                    DateTime JobEndDate = default;
//            If Not(CustomerRet.JobEndDate Is Nothing) Then
//           JobEndDate = CustomerRet.JobEndDate.GetValue
//                     End If
//                    string JobDesc = string.Empty;
//            If Not(CustomerRet.JobDesc Is Nothing) Then
//           JobDesc = CustomerRet.JobDesc.GetValue
//                     End If
//                    string JobTypeRef = string.Empty;
//            string JobTypeName = string.Empty;
//            If Not(CustomerRet.JobTypeRef Is Nothing) Then
//           If Not(CustomerRet.JobTypeRef.ListID Is Nothing) Then
//          JobTypeRef = CustomerRet.JobTypeRef.ListID.GetValue
//                     End If
//                     If Not(CustomerRet.JobTypeRef Is Nothing) Then
//                    JobTypeName = CustomerRet.JobTypeRef.FullName.GetValue
//                     End If

//                     End If


//                    string Other13 = string.Empty;
//            string Other14 = string.Empty;
//            string Other15 = string.Empty;
//            string Other16 = string.Empty;
//            int DisplayColor = 0;


//            Insert data in database code left..........db.tblCustomer_Insert(ClearAllControl.gblCompanyID, ListID, TimeCreated, TimeModified, EditSequence, Name, ArName, FullName, Parent, Conversions.ToChar(IsActive), Conversions.ToInteger(Sublevel), CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillCityRefKey, BillCity, BillStateRefKey, BillState, BillPostalCode, BillCountryRefKey, BillCountry, Phone, AltPhone, Fax, Email, Cc, Contact, AltContact, CustomerTypeRef, CustomerTypeName, TermsRef, TermsName, SalesRepRef, SalesRepName, (decimal?)Balance, (decimal?)TotalBalance, SalesTaxCodeRef, SalesTaxCodeName, AccountNumber, CreditLimit, JobStatus, Conversions.ToString(JobStartDate), Conversions.ToString(JobProjectedEndDate), Conversions.ToString(JobEndDate), JobDesc, JobTypeRef, JobTypeName, Other13, Other14, Other15, Other16, DisplayColor);








//        }

//        return;
//    }
//    catch
//    {

//        Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");

//        bDone = true;
//        bError = true;
//    }
//}

//Get Credit INformation
//         Public Sub GetCustomerCreditInfo(ByRef bError As Boolean)
//         On Error GoTo Errs
//         ' make sure we do not have any old requests still defined
//         msgSetRequest.ClearRequests()
//         ' set the OnError attribute to continueOnError
//         msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue
//         ' Add the BillQuery request
//         Dim CreditMemoQuery As ICreditMemoQuery
//         CreditMemoQuery = msgSetRequest.AppendCreditMemoQueryRq
//         CreditMemoQuery.ORTxnQuery.
//         Dim RecepitQuery As IInvoiceQuery
//         RecepitQuery = msgSetRequest.AppendInvoiceQueryRq
//         RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//         'CheckQuery.ORBillQuery.BillFilter.PaidStatus.SetValue(ENPaidStatus.psNotPaidOnly)
//         Dim bDone As Boolean = False
//         Do While (Not bDone)
//         ' start looking for customer next in the list

//         ' send the request to QB
//         Dim msgSetResponse As IMsgSetResponse
//         msgSetResponse = qbSessionManager.DoRequests(msgSetRequest)

//         'MsgBox(msgSetRequest.ToXMLString())
//         FillInvoiceInDataBase(msgSetResponse, bDone, bError)
//         Loop
//         Exit Sub

//         Errs:
//         MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, MsgBoxStyle.Critical, "Error in GetCustomers")
//         bError = True
//         End Sub

//         Public Sub FillInvoiceInDataBase(ByRef msgSetResponse As IMsgSetResponse, ByRef bDone As Boolean, ByRef bError As Boolean)
//         On Error GoTo Errs

//         ' check to make sure we have objects to access first
//         ' and that there are responses in the list
//         If (msgSetResponse Is Nothing) Or _
//         (msgSetResponse.ResponseList Is Nothing) Or _
//         (msgSetResponse.ResponseList.Count <= 0) Then
//         bDone = True
//         bError = True
//         Exit Sub
//         End If

//         ' Start parsing the response list
//         Dim responseList As IResponseList
//         responseList = msgSetResponse.ResponseList

//         ' go thru each response and process the response.
//         ' this example will only have one response in the list
//         ' so we will look at index=0
//         Dim response As IResponse
//         response = responseList.GetAt(0)
//         If(Not response Is Nothing) Then
//        If response.StatusCode <> "0" Then
//         'If the status is bad, report it to the user
//         MsgBox("Get check query unexpexcted Error - " & response.StatusMessage)
//         bDone = True
//         bError = True
//         Exit Sub
//         End If
//         End If

//         ' first make sure we have a response object to handle
//         If (response Is Nothing) Or _
//         (response.Type Is Nothing) Or _
//         (response.Detail Is Nothing) Or _
//         (response.Detail.Type Is Nothing) Then
//         bDone = True
//         bError = True
//         Exit Sub
//         End If

//         ' make sure we are processing the CustomerQueryRs and 
//         ' the CustomerRetList responses in this response list
//         Dim InvoiceRetList As IInvoiceRetList
//         Dim responseType As ENResponseType
//         Dim responseDetailType As ENObjectType
//         responseType = response.Type.GetValue()
//         responseDetailType = response.Detail.Type.GetValue()
//         If(responseType = ENResponseType.rtInvoiceQueryRs) And(responseDetailType = ENObjectType.otInvoiceRetList) Then
//         ' save the response detail in the appropriate object type
//         ' since we have first verified the type of the response object
//         InvoiceRetList = response.Detail
//         Else
//         ' bail, we do not have the responses we were expecting
//         bDone = True
//         bError = True
//         Exit Sub
//         End If

//         'Parse the query response and add the Customers to the Customer list box
//         Dim count As Short
//         Dim index As Short
//         Dim InvoiceRet As IInvoiceRet
//         count = InvoiceRetList.Count
//         Dim MAX_RETURNED As Short
//         MAX_RETURNED = 1 + InvoiceRetList.Count
//         ' we are done with the customerQueries if we have not received the MaxReturned
//         If (count < MAX_RETURNED) Then
//         bDone = True
//         End If

//         For index = 0 To count - 1
//         ' skip this customer if this is a repeat from the last query
//         InvoiceRet = InvoiceRetList.GetAt(index)
//         If(InvoiceRet Is Nothing) Or _
//        (InvoiceRet.CustomerRef.ListID Is Nothing) Or _
//        (InvoiceRet.TxnID Is Nothing) Then
//        bDone = True
//         bError = True
//         Exit Sub
//         End If


//         ''declare varibale to retrive data 
//         Dim BillAddress1 As String = String.Empty
//         Dim BillAddress2 As String = String.Empty
//         Dim BillAddress3 As String = String.Empty
//         Dim BillAddress4 As String = String.Empty
//         Dim BillAddress5 As String = String.Empty
//         Dim BillAddressCity As String = String.Empty
//         Dim BillAddressState As String = String.Empty
//         Dim BillAddressPostalCode As String = String.Empty
//         Dim BillAddressCountry As String = String.Empty
//         Dim BillAddressNote As String = String.Empty


//         Dim TxnID As String = InvoiceRet.TxnID.GetValue
//         Dim CustomerRefKey As String = String.Empty
//         If Not (InvoiceRet.CustomerRef Is Nothing) Then
//         CustomerRefKey = InvoiceRet.CustomerRef.ListID.GetValue
//         End If
//         Dim customerName As String = String.Empty
//         If Not (InvoiceRet.CustomerRef Is Nothing) Then
//         customerName = InvoiceRet.CustomerRef.FullName.GetValue
//         End If

//         Dim ClassRefKey As String = String.Empty
//         If Not (InvoiceRet.ClassRef Is Nothing) Then
//         ClassRefKey = InvoiceRet.ClassRef.ListID.GetValue
//         End If



//         Dim ARAccountRefKey As String = String.Empty
//         If Not (InvoiceRet.ARAccountRef Is Nothing) Then
//         ARAccountRefKey = InvoiceRet.ARAccountRef.ListID.GetValue

//         End If
//         Dim TemplateRefKey As String = String.Empty
//         If Not (InvoiceRet.TemplateRef Is Nothing) Then
//         TemplateRefKey = InvoiceRet.TemplateRef.ListID.GetValue
//         End If
//         Dim TxnDate As Date = InvoiceRet.TxnDate.GetValue

//         Dim RefNumber As String = String.Empty
//         If Not (InvoiceRet.RefNumber Is Nothing) Then
//         RefNumber = InvoiceRet.RefNumber.GetValue
//         End If

//         ' Bill Address


//         If Not (InvoiceRet.BillAddress Is Nothing) Then


//         If Not (InvoiceRet.BillAddress.Addr1 Is Nothing) Then
//         BillAddress1 = InvoiceRet.BillAddress.Addr1.GetValue
//         End If


//         If Not (InvoiceRet.BillAddress.Addr2 Is Nothing) Then
//         BillAddress2 = InvoiceRet.BillAddress.Addr2.GetValue
//         End If

//         If (Not InvoiceRet.BillAddress.Addr3 Is Nothing) Then
//         BillAddress3 = InvoiceRet.BillAddress.Addr3.GetValue
//         End If

//         If Not (InvoiceRet.BillAddress.Addr4 Is Nothing) Then
//         BillAddress4 = InvoiceRet.BillAddress.Addr4.GetValue
//         End If

//         If Not (InvoiceRet.BillAddress.Addr5 Is Nothing) Then
//         BillAddress5 = InvoiceRet.BillAddress.Addr5.GetValue
//         End If

//         If Not (InvoiceRet.BillAddress.City Is Nothing) Then
//         BillAddressCity = InvoiceRet.BillAddress.City.GetValue
//         End If

//         If Not (InvoiceRet.BillAddress.State Is Nothing) Then
//         BillAddressState = InvoiceRet.BillAddress.State.GetValue
//         End If

//         If Not (InvoiceRet.BillAddress.PostalCode Is Nothing) Then
//         BillAddressPostalCode = InvoiceRet.BillAddress.PostalCode.GetValue
//         End If

//         If Not (InvoiceRet.BillAddress.Country Is Nothing) Then
//         BillAddressCountry = InvoiceRet.BillAddress.Country.GetValue
//         End If

//         If Not (InvoiceRet.BillAddress.Note Is Nothing) Then
//         BillAddressNote = InvoiceRet.BillAddress.Note.GetValue
//         End If
//         End If
//         'Ship address
//         Dim ShipAddress1 As String = String.Empty
//         Dim ShipAddress2 As String = String.Empty
//         Dim ShipAddress3 As String = String.Empty
//         Dim ShipAddress4 As String = String.Empty
//         Dim ShipAddress5 As String = String.Empty
//         Dim ShipAddressCity As String = String.Empty
//         Dim ShipAddressState As String = String.Empty
//         Dim ShipAddressPostalCode As String = String.Empty
//         Dim ShipAddressCountry As String = String.Empty
//         Dim ShipAddressNote As String = String.Empty
//         If Not (InvoiceRet.ShipAddress Is Nothing) Then

//         If Not (InvoiceRet.ShipAddress.Addr1 Is Nothing) Then
//         ShipAddress1 = InvoiceRet.ShipAddress.Addr1.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.Addr2 Is Nothing) Then
//         ShipAddress2 = InvoiceRet.ShipAddress.Addr2.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.Addr3 Is Nothing) Then
//         ShipAddress3 = InvoiceRet.ShipAddress.Addr3.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.Addr4 Is Nothing) Then
//         ShipAddress4 = InvoiceRet.ShipAddress.Addr4.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.Addr5 Is Nothing) Then
//         ShipAddress5 = InvoiceRet.ShipAddress.Addr5.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.City Is Nothing) Then
//         ShipAddressCity = InvoiceRet.ShipAddress.City.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.State Is Nothing) Then
//         ShipAddressState = InvoiceRet.ShipAddress.State.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.PostalCode Is Nothing) Then
//         ShipAddressPostalCode = InvoiceRet.ShipAddress.PostalCode.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.Country Is Nothing) Then
//         ShipAddressCountry = InvoiceRet.ShipAddress.Country.GetValue
//         End If

//         If Not (InvoiceRet.ShipAddress.Note Is Nothing) Then
//         ShipAddressNote = InvoiceRet.ShipAddress.Note.GetValue
//         End If
//         End If
//         'End ship address

//         Dim IsPending As String = String.Empty
//         If Not (InvoiceRet.IsPending Is Nothing) Then
//         IsPending = InvoiceRet.IsPending.GetValue
//         End If
//         Dim PONumber As String = String.Empty
//         If Not (InvoiceRet.PONumber Is Nothing) Then
//         PONumber = InvoiceRet.PONumber.GetValue
//         End If

//         Dim TermsRefKey As String = String.Empty
//         If Not (InvoiceRet.TermsRef Is Nothing) Then
//         TermsRefKey = InvoiceRet.TermsRef.ListID.GetValue
//         End If

//         Dim DueDate As String = String.Empty
//         If Not (InvoiceRet.DueDate Is Nothing) Then
//         DueDate = InvoiceRet.DueDate.GetValue
//         End If

//         Dim SalesRefKey As String = String.Empty
//         If Not (InvoiceRet.SalesRepRef Is Nothing) Then
//         SalesRefKey = InvoiceRet.SalesRepRef.ListID.GetValue
//         End If

//         Dim FOB As String = String.Empty
//         If Not (InvoiceRet.FOB Is Nothing) Then
//         FOB = InvoiceRet.FOB.GetValue
//         End If

//         Dim ShipDate As String = String.Empty
//         If Not (InvoiceRet.ShipDate Is Nothing) Then
//         ShipDate = InvoiceRet.ShipDate.GetValue
//         End If

//         Dim ShipMethodRefKey As String = String.Empty
//         If Not (InvoiceRet.ShipMethodRef Is Nothing) Then
//         ShipMethodRefKey = InvoiceRet.ShipMethodRef.ListID.GetValue
//         End If

//         Dim ItemSalesTaxRefKey As String = String.Empty
//         If Not (InvoiceRet.ItemSalesTaxRef Is Nothing) Then
//         ItemSalesTaxRefKey = InvoiceRet.ItemSalesTaxRef.ListID.GetValue
//         End If

//         Dim Memo As String = String.Empty
//         If Not (InvoiceRet.Memo Is Nothing) Then
//         Memo = InvoiceRet.Memo.GetValue
//         End If

//         Dim CustomerMsgRefKey As String = String.Empty
//         If Not (InvoiceRet.CustomerMsgRef Is Nothing) Then
//         CustomerMsgRefKey = InvoiceRet.CustomerMsgRef.ListID.GetValue
//         End If
//         Dim IsToBePrinted As String = String.Empty
//         If Not (InvoiceRet.IsToBePrinted Is Nothing) Then
//         IsToBePrinted = InvoiceRet.IsToBePrinted.GetValue
//         End If
//         Dim IsToEmailed As String = String.Empty
//         If Not (InvoiceRet.IsToBeEmailed Is Nothing) Then
//         IsToEmailed = InvoiceRet.IsToBeEmailed.GetValue
//         End If
//         Dim IsTaxIncluded As String = String.Empty
//         If Not (InvoiceRet.IsTaxIncluded Is Nothing) Then
//         IsTaxIncluded = InvoiceRet.IsTaxIncluded.GetValue
//         End If
//         Dim CustomerSalesTaxCodeRefKey As String = String.Empty
//         If Not (InvoiceRet.CustomerSalesTaxCodeRef Is Nothing) Then
//         CustomerSalesTaxCodeRefKey = InvoiceRet.CustomerSalesTaxCodeRef.ListID.GetValue
//         End If
//         Dim Other As String = String.Empty
//         If Not (InvoiceRet.Other Is Nothing) Then
//         Other = InvoiceRet.Other.GetValue
//         End If
//         Dim Amount As String = String.Empty
//         If Not (InvoiceRet.AppliedAmount Is Nothing) Then
//         Amount = InvoiceRet.BalanceRemaining.GetValue
//         End If
//         Dim AmountPaid As String = String.Empty
//         If Not (InvoiceRet.AppliedAmount Is Nothing) Then
//         AmountPaid = InvoiceRet.AppliedAmount.GetValue
//         End If
//         Dim CustomField1 As String = String.Empty
//         Dim CustomField2 As String = String.Empty
//         Dim CustomField3 As String = String.Empty
//         Dim CustomField4 As String = String.Empty
//         Dim CustomField5 As String = String.Empty
//         Dim QuotationRecNo As String = String.Empty
//         Dim GradeID As String = String.Empty
//         Dim InvoiceType As String = String.Empty



//         'Insert data in database code left..........

//         db.tblInvoice_Insert(gblCompanyID, TxnID, CustomerRefKey, Nothing, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, DueDate, SalesRefKey, FOB, ShipDate, ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, Amount, AmountPaid, CustomField1, CustomField2, CustomField3, CustomField4, CustomField5, QuotationRecNo, GradeID, InvoiceType, customerName)


//         Next

//         Exit Sub
//         Errs:
//         MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, _
//         MsgBoxStyle.Critical, _
//         "Error in FillChartOfAccountListBox")
//         bDone = True
//         bError = True
//         End Sub

//         Private Function FoundInvoiceInDatabase(ByRef txnID As String) As Boolean
//         On Error GoTo Errs

//         FoundInvoiceInDatabase = False
//         'check in database for existing customer bill
//         Dim result As ISingleResult(Of tblInvoice_SelectResult) = db.tblInvoice_Select(gblCompanyID, txnID, Nothing, Nothing, Nothing, Nothing, Nothing)

//         Dim resultCount As Integer = result.Count

//         If Not resultCount = 0 Then
//         FoundInvoiceInDatabase = True
//         End If

//         Exit Function
//         Errs:
//         MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, _
//         MsgBoxStyle.Critical, _
//         "Error in FoundIn Account database")

//         End Function

//        public static void getItem(ref bool bError)
//{
//    try
//    {


//        string msg;
//        Create the session manager object
//                OpenConnectionBeginSession();

//        Create the message set request object
//       IMsgSetRequest requestSet;
//        requestSet = GetLatestMsgSetRequest(qbSessionManager);
//        requestSet.Attributes.OnError = ENRqOnError.roeContinue;
//        IItemQuery ItemQ;
//        ItemQ = requestSet.AppendItemQueryRq;
//        ItemQ.ORListQuery.ListFilter.ActiveStatus.SetValue(ENActiveStatus.asAll);
//        Perform the request
//       IMsgSetResponse responseSet;
//        responseSet = qbSessionManager.DoRequests(requestSet);

//        Interpret the response
//       IResponse response;
//        object statusCode, statusMessage, statusSeverity;
//        The response list contains only one response,
//                 which corresponds to our single request
//                response = responseSet.ResponseList.GetAt(0);
//        statusCode = response.StatusCode;
//        statusMessage = response.StatusMessage;
//        statusSeverity = response.StatusSeverity;

//        msg = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Status: Code = " + Conversions.ToString(statusCode) + ", Message = ", statusMessage), ", Severity = "), statusSeverity), Constants.vbCrLf));


//        IORItemRetList orItemRetList;
//        orItemRetList = response.Detail;
//        if (orItemRetList is not null)
//        {
//            object ndx;
//            var loopTo = orItemRetList.Count - 1;
//            for (ndx = 0; ndx <= loopTo; ndx++)
//            {
//                IORItemRet orItemRet;
//                orItemRet = orItemRetList.GetAt(ndx);
//                local variable
//                        string ListID = string.Empty;
//                DateTime? TimeCreated = default;
//                DateTime? TimeModified = default;
//                string EditSequence = string.Empty;
//                string Name = string.Empty;
//                string FullName = string.Empty;
//                string Description = string.Empty;
//                bool IsActive = false;
//                string ParentRefListID = string.Empty;
//                string ParentRefFullName = string.Empty;
//                int Sublevel = 0;
//                string Type = string.Empty;
//                string SalesTaxCodeRefListID = string.Empty;
//                string SalesTaxCodeRefFullName = string.Empty;
//                string SalesOrPurchaseDesc = string.Empty;
//                decimal SalesOrPurchasePrice = 0m;
//                decimal SalesOrPurchasePricePercent = 0m;
//                string SalesOrPurchaseAccountRefListID = string.Empty;
//                string SalesOrPurchaseAccountRefFullName = string.Empty;
//                string SalesAndPurchaseSalesDesc = string.Empty;
//                decimal SalesAndPurchaseSalesPrice = 0m;
//                string SalesAndPurchaseIncomeAccountRefListID = string.Empty;
//                string SalesAndPurchaseIncomeAccountRefFullName = string.Empty;
//                string SalesAndPurchasePurchaseDesc = string.Empty;
//                decimal SalesAndPurchasePurchaseCost = 0m;
//                string SalesAndPurchaseExpenseAccountRefListID = string.Empty;
//                string SalesAndPurchaseExpenseAccountRefFullName = string.Empty;
//                string SalesAndPurchasePrefVendorRefListID = string.Empty;
//                string SalesAndPurchasePrefVendorRefFullName = string.Empty;
//                string SalesDesc = string.Empty;
//                decimal SalesPrice = 0m;
//                string IncomeAccountRefListID = string.Empty;
//                string IncomeAccountRefFullName = string.Empty;
//                string PurchaseDesc = string.Empty;
//                decimal PurchaseCost = 0m;
//                string COGSAccountRefListID = string.Empty;
//                string COGSAccountRefFullName = string.Empty;
//                string PrefVendorRefListID = string.Empty;
//                string PrefVendorRefFullName = string.Empty;
//                string AssetAccountRefListID = string.Empty;
//                string AssetAccountRefFullName = string.Empty;
//                decimal ReorderBuildPoint = 0m;
//                DateTime? InventoryDate = default;
//                string DepositToAccountRefListID = string.Empty;
//                string DepositToAccountRefFullName = string.Empty;
//                string PaymentMethodRefListID = string.Empty;
//                string PaymentMethodRefFullName = string.Empty;
//                decimal TaxRate = 0m;
//                string TaxVendorRefListID = string.Empty;
//                string TaxVendorRefFullName = string.Empty;
//                string UnitOfMeasureSetRefListID = string.Empty;
//                string UnitOfMeasureSetRefFullName = string.Empty;
//                string QuantityOnHand = "0.00";


//                The ortype property returns an enum
//                of the elements that can be contained in the OR object
//                        switch (orItemRet.ortype.ToString)
//                        {

//                            case "orirItemServiceRet": // "orir" prefix comes from OR + Item + Ret
//                                {
//        IItemServiceRet ItemServiceRet;
//        ItemServiceRet = orItemRet.ItemServiceRet;

//        if (ItemServiceRet.ListID is not null)
//        {
//            ListID = ItemServiceRet.ListID.GetValue();
//        }
//        if (ItemServiceRet.TimeCreated is not null)
//        {
//            TimeCreated = ItemServiceRet.TimeCreated.GetValue;
//        }
//        if (ItemServiceRet.TimeModified is not null)
//        {
//            TimeModified = ItemServiceRet.TimeModified.GetValue;
//        }
//        if (ItemServiceRet.EditSequence is not null)
//        {
//            EditSequence = ItemServiceRet.EditSequence.GetValue;
//        }
//        if (ItemServiceRet.Name is not null)
//        {
//            Name = ItemServiceRet.Name.GetValue;
//        }
//        if (ItemServiceRet.FullName is not null)
//        {
//            FullName = ItemServiceRet.FullName.GetValue;
//        }


//        if (ItemServiceRet.ParentRef is not null)
//        {
//            if (ItemServiceRet.ParentRef.ListID is not null)
//            {
//                ParentRefListID = ItemServiceRet.ParentRef.ListID.GetValue;
//            }
//            if (ItemServiceRet.ParentRef.ListID is not null)
//            {
//                ParentRefFullName = ItemServiceRet.ParentRef.FullName.GetValue;
//            }
//        }

//        Sublevel = ItemServiceRet.Sublevel.GetValue();


//        if (ItemServiceRet.Type is not null)
//        {
//            Type = ItemServiceRet.Type.GetValue();
//        }
//        if (ItemServiceRet.SalesTaxCodeRef is not null)
//        {
//            if (ItemServiceRet.SalesTaxCodeRef.ListID is not null)
//            {
//                SalesTaxCodeRefListID = ItemServiceRet.SalesTaxCodeRef.ListID.GetValue;
//            }
//            if (ItemServiceRet.SalesTaxCodeRef.FullName is not null)
//            {
//                SalesTaxCodeRefFullName = ItemServiceRet.SalesTaxCodeRef.FullName.GetValue;
//            }
//        }
//        ORSalesPurchase start
//                                    if (ItemServiceRet.ORSalesPurchase is not null)
//        {
//            sale or purchase
//                                        if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase is not null)
//            {
//                if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase.Desc is not null)
//                {
//                    SalesOrPurchaseDesc = ItemServiceRet.ORSalesPurchase.SalesOrPurchase.Desc.GetValue;
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase.ORPrice is not null)
//                {
//                    if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase.ORPrice.Price is not null)
//                    {
//                        SalesOrPurchasePrice = ItemServiceRet.ORSalesPurchase.SalesOrPurchase.ORPrice.Price.GetValue();
//                    }
//                    if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase.ORPrice.PricePercent is not null)
//                    {
//                        SalesOrPurchasePricePercent = ItemServiceRet.ORSalesPurchase.SalesOrPurchase.ORPrice.PricePercent.GetValue;
//                    }
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase.AccountRef is not null)
//                {
//                    if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase.AccountRef.ListID is not null)
//                    {
//                        SalesOrPurchaseAccountRefListID = ItemServiceRet.ORSalesPurchase.SalesOrPurchase.AccountRef.ListID.GetValue;
//                    }
//                    if (ItemServiceRet.ORSalesPurchase.SalesOrPurchase.AccountRef.FullName is not null)
//                    {
//                        SalesOrPurchaseAccountRefFullName = ItemServiceRet.ORSalesPurchase.SalesOrPurchase.AccountRef.FullName.GetValue;
//                    }
//                }

//            }
//            Sale and purchase
//                                        if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase is not null)
//            {
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesDesc is not null)
//                {
//                    SalesAndPurchaseSalesDesc = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesDesc.GetValue;
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesPrice is not null)
//                {
//                    SalesAndPurchaseSalesPrice = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesPrice.GetValue;
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef is not null)
//                {
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID is not null)
//                    {
//                        SalesAndPurchaseIncomeAccountRefListID = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID.GetValue;
//                    }
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName is not null)
//                    {
//                        SalesAndPurchaseIncomeAccountRefFullName = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName.GetValue;
//                    }
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc is not null)
//                {
//                    SalesAndPurchasePurchaseDesc = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc.GetValue();
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost is not null)
//                {
//                    SalesAndPurchasePurchaseCost = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost.GetValue();
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef is not null)
//                {
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.ListID is not null)
//                    {
//                        SalesAndPurchaseExpenseAccountRefListID = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.ListID.GetValue();
//                    }
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.FullName is not null)
//                    {
//                        SalesAndPurchaseExpenseAccountRefFullName = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.FullName.GetValue();
//                    }
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef is not null)
//                {
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID is not null)
//                    {
//                        SalesAndPurchasePrefVendorRefListID = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID.GetValue();
//                    }
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName is not null)
//                    {
//                        SalesAndPurchasePrefVendorRefFullName = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName.GetValue();
//                    }
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesDesc is not null)
//                {
//                    SalesDesc = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesDesc.GetValue;
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesPrice is not null)
//                {
//                    SalesPrice = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.SalesPrice.GetValue;
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef is not null)
//                {
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID is not null)
//                    {
//                        IncomeAccountRefListID = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID.GetValue;
//                    }
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName is not null)
//                    {
//                        IncomeAccountRefFullName = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName.GetValue;
//                    }
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc is not null)
//                {
//                    PurchaseDesc = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc.GetValue;
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost is not null)
//                {
//                    PurchaseCost = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost.GetValue;
//                }
//                if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef is not null)
//                {
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID is not null)
//                    {
//                        PrefVendorRefListID = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID.GetValue;
//                    }
//                    if (ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName is not null)
//                    {
//                        PrefVendorRefFullName = ItemServiceRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName.GetValue;
//                    }
//                }


//                sale and purchase end

//                                        }

//            or salepurchase end

//                                    }

//        if (ItemServiceRet.UnitOfMeasureSetRef is not null)
//        {
//            if (ItemServiceRet.UnitOfMeasureSetRef.ListID is not null)
//            {
//                UnitOfMeasureSetRefListID = ItemServiceRet.UnitOfMeasureSetRef.ListID.GetValue;
//            }
//            if (ItemServiceRet.UnitOfMeasureSetRef.FullName is not null)
//            {
//                UnitOfMeasureSetRefFullName = ItemServiceRet.UnitOfMeasureSetRef.FullName.GetValue;
//            }


//        }

//        break;
//    }
//    Insert and update statement for service items


//                            case "orirItemInventoryRet":
//                                {
//        IItemInventoryRet ItemInventoryRet;
//        ItemInventoryRet = orItemRet.ItemInventoryRet;

//        if (ItemInventoryRet.ListID is not null)
//        {
//            ListID = ItemInventoryRet.ListID.GetValue();
//        }
//        if (ItemInventoryRet.TimeCreated is not null)
//        {
//            TimeCreated = ItemInventoryRet.TimeCreated.GetValue;
//        }
//        if (ItemInventoryRet.TimeModified is not null)
//        {
//            TimeModified = ItemInventoryRet.TimeModified.GetValue;
//        }
//        if (ItemInventoryRet.EditSequence is not null)
//        {
//            EditSequence = ItemInventoryRet.EditSequence.GetValue;
//        }
//        if (ItemInventoryRet.Name is not null)
//        {
//            Name = ItemInventoryRet.Name.GetValue;
//        }
//        if (Name == "140 707")
//        {
//            Name = Name;
//        }
//        if (ItemInventoryRet.FullName is not null)
//        {
//            FullName = ItemInventoryRet.FullName.GetValue;
//        }


//        if (ItemInventoryRet.ParentRef is not null)
//        {
//            if (ItemInventoryRet.ParentRef.ListID is not null)
//            {
//                ParentRefListID = ItemInventoryRet.ParentRef.ListID.GetValue;
//            }
//            if (ItemInventoryRet.ParentRef.ListID is not null)
//            {
//                ParentRefFullName = ItemInventoryRet.ParentRef.FullName.GetValue;
//            }
//        }
//        if (ItemInventoryRet.Sublevel is not null)
//        {
//            Sublevel = ItemInventoryRet.Sublevel.GetValue();
//        }



//        if (ItemInventoryRet.Type is not null)
//        {
//            Type = ItemInventoryRet.Type.GetValue();
//        }
//        if (ItemInventoryRet.SalesTaxCodeRef is not null)
//        {
//            if (ItemInventoryRet.SalesTaxCodeRef.ListID is not null)
//            {
//                SalesTaxCodeRefListID = ItemInventoryRet.SalesTaxCodeRef.ListID.GetValue;
//            }
//            if (ItemInventoryRet.SalesTaxCodeRef.FullName is not null)
//            {
//                SalesTaxCodeRefFullName = ItemInventoryRet.SalesTaxCodeRef.FullName.GetValue;
//            }
//        }
//        if (ItemInventoryRet.COGSAccountRef is not null)
//        {
//            if (ItemInventoryRet.COGSAccountRef.ListID is not null)
//            {
//                COGSAccountRefListID = ItemInventoryRet.COGSAccountRef.ListID.GetValue;
//            }
//            if (ItemInventoryRet.COGSAccountRef.FullName is not null)
//            {
//                COGSAccountRefFullName = ItemInventoryRet.COGSAccountRef.FullName.GetValue;
//            }

//        }
//        if (ItemInventoryRet.PrefVendorRef is not null)
//        {
//            if (ItemInventoryRet.PrefVendorRef.ListID is not null)
//            {
//                PrefVendorRefListID = ItemInventoryRet.PrefVendorRef.ListID.GetValue;
//            }
//            if (ItemInventoryRet.PrefVendorRef.FullName is not null)
//            {
//                PrefVendorRefFullName = ItemInventoryRet.PrefVendorRef.FullName.GetValue;
//            }
//        }
//        if (ItemInventoryRet.AssetAccountRef is not null)
//        {
//            if (ItemInventoryRet.AssetAccountRef.ListID is not null)
//            {
//                AssetAccountRefListID = ItemInventoryRet.AssetAccountRef.ListID.GetValue;
//            }
//            if (ItemInventoryRet.AssetAccountRef.FullName is not null)
//            {
//                AssetAccountRefFullName = ItemInventoryRet.AssetAccountRef.FullName.GetValue;
//            }
//        }
//        if (ItemInventoryRet.ReorderPoint is not null)
//        {
//            ReorderBuildPoint = ItemInventoryRet.ReorderPoint.GetValue;
//        }

//        if (ItemInventoryRet.UnitOfMeasureSetRef is not null)
//        {
//            if (ItemInventoryRet.UnitOfMeasureSetRef.ListID is not null)
//            {
//                UnitOfMeasureSetRefListID = ItemInventoryRet.UnitOfMeasureSetRef.ListID.GetValue;
//            }
//            if (ItemInventoryRet.UnitOfMeasureSetRef.FullName is not null)
//            {
//                UnitOfMeasureSetRefFullName = ItemInventoryRet.UnitOfMeasureSetRef.FullName.GetValue;
//            }

//        }
//        if (ItemInventoryRet.QuantityOnHand is not null)
//        {
//            QuantityOnHand = ItemInventoryRet.QuantityOnHand.GetValue;
//        }

//        break;
//    }




//                            case "orirItemNonInventoryRet":
//                                {

//        IItemNonInventoryRet ItemNonInventoryRet;
//        ItemNonInventoryRet = orItemRet.ItemNonInventoryRet;
//        if (ItemNonInventoryRet.ListID is not null)
//        {
//            ListID = ItemNonInventoryRet.ListID.GetValue();
//        }
//        if (ItemNonInventoryRet.TimeCreated is not null)
//        {
//            TimeCreated = ItemNonInventoryRet.TimeCreated.GetValue;
//        }
//        if (ItemNonInventoryRet.TimeModified is not null)
//        {
//            TimeModified = ItemNonInventoryRet.TimeModified.GetValue;
//        }
//        if (ItemNonInventoryRet.EditSequence is not null)
//        {
//            EditSequence = ItemNonInventoryRet.EditSequence.GetValue;
//        }
//        if (ItemNonInventoryRet.Name is not null)
//        {
//            Name = ItemNonInventoryRet.Name.GetValue;
//        }
//        if (ItemNonInventoryRet.FullName is not null)
//        {
//            FullName = ItemNonInventoryRet.FullName.GetValue;
//        }

//        if (ItemNonInventoryRet.ParentRef is not null)
//        {
//            if (ItemNonInventoryRet.ParentRef.ListID is not null)
//            {
//                ParentRefListID = ItemNonInventoryRet.ParentRef.ListID.GetValue;
//            }
//            if (ItemNonInventoryRet.ParentRef.ListID is not null)
//            {
//                ParentRefFullName = ItemNonInventoryRet.ParentRef.FullName.GetValue;
//            }
//        }

//        Sublevel = ItemNonInventoryRet.Sublevel.GetValue();


//        if (ItemNonInventoryRet.Type is not null)
//        {
//            Type = ItemNonInventoryRet.Type.GetValue();
//        }
//        if (ItemNonInventoryRet.SalesTaxCodeRef is not null)
//        {
//            if (ItemNonInventoryRet.SalesTaxCodeRef.ListID is not null)
//            {
//                SalesTaxCodeRefListID = ItemNonInventoryRet.SalesTaxCodeRef.ListID.GetValue;
//            }
//            if (ItemNonInventoryRet.SalesTaxCodeRef.FullName is not null)
//            {
//                SalesTaxCodeRefFullName = ItemNonInventoryRet.SalesTaxCodeRef.FullName.GetValue;
//            }
//        }
//        ORSalesPurchase start
//                                    if (ItemNonInventoryRet.ORSalesPurchase is not null)
//        {
//            sale or purchase
//                                        if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase is not null)
//            {
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.Desc is not null)
//                {
//                    SalesOrPurchaseDesc = ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.Desc.GetValue;
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.ORPrice is not null)
//                {
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.ORPrice.Price is not null)
//                    {
//                        SalesOrPurchasePrice = ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.ORPrice.Price.GetValue();
//                    }
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.ORPrice.PricePercent is not null)
//                    {
//                        SalesOrPurchasePricePercent = ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.ORPrice.PricePercent.GetValue;
//                    }
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.AccountRef is not null)
//                {
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.AccountRef.ListID is not null)
//                    {
//                        SalesOrPurchaseAccountRefListID = ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.AccountRef.ListID.GetValue;
//                    }
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.AccountRef.FullName is not null)
//                    {
//                        SalesOrPurchaseAccountRefFullName = ItemNonInventoryRet.ORSalesPurchase.SalesOrPurchase.AccountRef.FullName.GetValue;
//                    }
//                }

//            }
//            Sale and purchase
//                                        if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase is not null)
//            {
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesDesc is not null)
//                {
//                    SalesAndPurchaseSalesDesc = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesDesc.GetValue;
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesPrice is not null)
//                {
//                    SalesAndPurchaseSalesPrice = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesPrice.GetValue;
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef is not null)
//                {
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID is not null)
//                    {
//                        SalesAndPurchaseIncomeAccountRefListID = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID.GetValue;
//                    }
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName is not null)
//                    {
//                        SalesAndPurchaseIncomeAccountRefFullName = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName.GetValue;
//                    }
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc is not null)
//                {
//                    SalesAndPurchasePurchaseDesc = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc.GetValue();
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost is not null)
//                {
//                    SalesAndPurchasePurchaseCost = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost.GetValue();
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef is not null)
//                {
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.ListID is not null)
//                    {
//                        SalesAndPurchaseExpenseAccountRefListID = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.ListID.GetValue();
//                    }
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.FullName is not null)
//                    {
//                        SalesAndPurchaseExpenseAccountRefFullName = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.ExpenseAccountRef.FullName.GetValue();
//                    }
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef is not null)
//                {
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID is not null)
//                    {
//                        SalesAndPurchasePrefVendorRefListID = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID.GetValue();
//                    }
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName is not null)
//                    {
//                        SalesAndPurchasePrefVendorRefFullName = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName.GetValue();
//                    }
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesDesc is not null)
//                {
//                    SalesDesc = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesDesc.GetValue;
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesPrice is not null)
//                {
//                    SalesPrice = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.SalesPrice.GetValue;
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef is not null)
//                {
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID is not null)
//                    {
//                        IncomeAccountRefListID = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.ListID.GetValue;
//                    }
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName is not null)
//                    {
//                        IncomeAccountRefFullName = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.IncomeAccountRef.FullName.GetValue;
//                    }
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc is not null)
//                {
//                    PurchaseDesc = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseDesc.GetValue;
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost is not null)
//                {
//                    PurchaseCost = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PurchaseCost.GetValue;
//                }
//                if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef is not null)
//                {
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID is not null)
//                    {
//                        PrefVendorRefListID = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.ListID.GetValue;
//                    }
//                    if (ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName is not null)
//                    {
//                        PrefVendorRefFullName = ItemNonInventoryRet.ORSalesPurchase.SalesAndPurchase.PrefVendorRef.FullName.GetValue;
//                    }
//                }


//                sale and purchase end

//                                        }

//            or salepurchase end

//                                    }
//        if (ItemNonInventoryRet.UnitOfMeasureSetRef is not null)
//        {
//            if (ItemNonInventoryRet.UnitOfMeasureSetRef.ListID is not null)
//            {
//                UnitOfMeasureSetRefListID = ItemNonInventoryRet.UnitOfMeasureSetRef.ListID.GetValue;
//            }
//            if (ItemNonInventoryRet.UnitOfMeasureSetRef.FullName is not null)
//            {
//                UnitOfMeasureSetRefFullName = ItemNonInventoryRet.UnitOfMeasureSetRef.FullName.GetValue;
//            }
//        }

//        break;
//    }

//    default:
//                                {
//        break;
//    }
//    ...
//                                 Could have specific code for the other item types.



//                        }

//if (!FoundItemInDatabase(ref ListID))
//{
//    insert
//                            db.tblItemQuery_Insert(ListID, TimeCreated, TimeModified, EditSequence, Name, FullName, Description, IsActive, ParentRefListID, ParentRefFullName, Sublevel, Type, SalesTaxCodeRefListID, SalesTaxCodeRefFullName, SalesOrPurchaseDesc, SalesOrPurchasePrice, SalesOrPurchasePricePercent, SalesOrPurchaseAccountRefListID, SalesAndPurchaseExpenseAccountRefFullName, SalesAndPurchaseSalesDesc, SalesAndPurchaseSalesPrice, SalesAndPurchaseExpenseAccountRefListID, SalesAndPurchaseIncomeAccountRefFullName, SalesAndPurchasePurchaseDesc, SalesAndPurchasePurchaseCost, SalesAndPurchaseExpenseAccountRefFullName, SalesAndPurchaseExpenseAccountRefFullName, SalesAndPurchasePrefVendorRefListID, SalesAndPurchasePrefVendorRefFullName, SalesDesc, SalesPrice, IncomeAccountRefListID, IncomeAccountRefFullName, PurchaseDesc, PurchaseCost, COGSAccountRefListID, COGSAccountRefFullName, PrefVendorRefListID, PrefVendorRefFullName, AssetAccountRefListID, AssetAccountRefFullName, ReorderBuildPoint, InventoryDate, DepositToAccountRefListID, DepositToAccountRefFullName, PaymentMethodRefListID, PaymentMethodRefFullName, TaxRate, TaxVendorRefListID, TaxVendorRefFullName, UnitOfMeasureSetRefListID, UnitOfMeasureSetRefFullName, QuantityOnHand, ClearAllControl.gblCompanyID);











//}
//else
//{
//    update
//                            db.tblItemQuery_Update(ListID, TimeCreated, TimeModified, EditSequence, Name, FullName, Description, IsActive, ParentRefListID, ParentRefFullName, Sublevel, Type, SalesTaxCodeRefListID, SalesTaxCodeRefFullName, SalesOrPurchaseDesc, SalesOrPurchasePrice, SalesOrPurchasePricePercent, SalesOrPurchaseAccountRefListID, SalesAndPurchaseExpenseAccountRefFullName, SalesAndPurchaseSalesDesc, SalesAndPurchaseSalesPrice, SalesAndPurchaseExpenseAccountRefListID, SalesAndPurchaseIncomeAccountRefFullName, SalesAndPurchasePurchaseDesc, SalesAndPurchasePurchaseCost, SalesAndPurchaseExpenseAccountRefFullName, SalesAndPurchaseExpenseAccountRefFullName, SalesAndPurchasePrefVendorRefListID, SalesAndPurchasePrefVendorRefFullName, SalesDesc, SalesPrice, IncomeAccountRefListID, IncomeAccountRefFullName, PurchaseDesc, PurchaseCost, COGSAccountRefListID, COGSAccountRefFullName, PrefVendorRefListID, PrefVendorRefFullName, AssetAccountRefListID, AssetAccountRefFullName, ReorderBuildPoint, InventoryDate, DepositToAccountRefListID, DepositToAccountRefFullName, PaymentMethodRefListID, PaymentMethodRefFullName, TaxRate, TaxVendorRefListID, TaxVendorRefFullName, QuantityOnHand, ClearAllControl.gblCompanyID);











//}


//                    }

//                }
//                EndSessionCloseConnection();
//            }
//             qbSessionManager.EndSession()
//             qbSessionManager.CloseConnection()

//            catch (Exception ex)
//{
//    Interaction.MsgBox(ex.Message, MsgBoxStyle.Exclamation);
//    EndSessionCloseConnection();
//    bError = true;
//}

//        }

//        public static void getclassItem(ref bool Berror)
//{
//    try
//    {


//        string msg;
//        Create the session manager object
//                OpenConnectionBeginSession();

//        Create the message set request object
//       IMsgSetRequest requestSet;
//        requestSet = GetLatestMsgSetRequest(qbSessionManager);
//        requestSet.Attributes.OnError = ENRqOnError.roeContinue;

//        IClassQuery classitem;
//        classitem = requestSet.AppendClassQueryRq;
//        classitem.ORListQuery.ListFilter.ActiveStatus.SetValue(ENActiveStatus.asAll);
//        Perform the request
//       IMsgSetResponse responseSet;
//        responseSet = qbSessionManager.DoRequests(requestSet);

//        Interpret the response
//       IResponse response;
//        object statusCode, statusMessage, statusSeverity;
//        The response list contains only one response,
//                 which corresponds to our single request
//                response = responseSet.ResponseList.GetAt(0);
//        statusCode = response.StatusCode;
//        statusMessage = response.StatusMessage;
//        statusSeverity = response.StatusSeverity;

//        msg = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Status: Code = " + Conversions.ToString(statusCode) + ", Message = ", statusMessage), ", Severity = "), statusSeverity), Constants.vbCrLf));


//        IClassRetList classRetList;
//        classRetList = response.Detail;
//        if (classRetList is not null)
//        {
//            object ndx;
//            var loopTo = classRetList.Count - 1;
//            for (ndx = 0; ndx <= loopTo; ndx++)
//            {
//                IClassRet IclassRet;
//                IclassRet = classRetList.GetAt(ndx);

//                string ListID = string.Empty;
//                DateTime? TimeCreated = default;
//                DateTime? TimeModified = default;
//                string EditSequence = string.Empty;
//                string Name = string.Empty;
//                string FullName = string.Empty;
//                bool IsActive = false;
//                string ParentRefListID = string.Empty;
//                string ParentRefFullName = string.Empty;
//                int Sublevel = 0;

//                if (IclassRet.ListID is not null)
//                {
//                    ListID = IclassRet.ListID.GetValue();
//                }

//                if (IclassRet.TimeCreated is not null)
//                {
//                    TimeCreated = IclassRet.TimeCreated.GetValue();
//                }

//                if (IclassRet.TimeModified is not null)
//                {
//                    TimeModified = IclassRet.TimeModified.GetValue();
//                }

//                if (IclassRet.EditSequence is not null)
//                {
//                    EditSequence = IclassRet.EditSequence.GetValue();
//                }

//                if (IclassRet.Name is not null)
//                {
//                    Name = IclassRet.Name.GetValue();
//                }

//                if (IclassRet.FullName is not null)
//                {
//                    FullName = IclassRet.FullName.GetValue();
//                }

//                if (IclassRet.IsActive is not null)
//                {
//                    IsActive = IclassRet.IsActive.GetValue();
//                }


//                if (IclassRet.ParentRef is not null)
//                {
//                    if (IclassRet.ParentRef.ListID is not null)
//                    {
//                        ParentRefListID = IclassRet.ParentRef.ListID.GetValue();
//                    }
//                    if (IclassRet.ParentRef.FullName is not null)
//                    {
//                        ParentRefFullName = IclassRet.ParentRef.FullName.GetValue();
//                    }
//                }

//                if (IclassRet.Sublevel is not null)
//                {
//                    Sublevel = IclassRet.Sublevel.GetValue();
//                }
//                if (!FoundClassInDatabase(ref ListID))
//                {
//                    insert
//                            db.tblClass_Insert(ClearAllControl.gblCompanyID, ListID, TimeCreated, TimeModified, EditSequence, Name, FullName, IsActive, ParentRefListID, ParentRefFullName, Sublevel);
//                }
//                else
//                {
//                    update
//                            db.tblClass_Update(ClearAllControl.gblCompanyID, ListID, TimeCreated, TimeModified, EditSequence, Name, FullName, IsActive, ParentRefListID, ParentRefFullName, Sublevel);
//                }

//                Dim fullname As String = orItemRet.FullName.GetValue()
//                         The ortype property returns an enum
//                         of the elements that can be contained in the OR object

//                    }

//                }
//                 qbSessionManager.EndSession()
//                 qbSessionManager.CloseConnection()
//                EndSessionCloseConnection();
//            }

//            catch (Exception ex)
//{
//    Interaction.MsgBox(ex.Message, MsgBoxStyle.Exclamation);
//    EndSessionCloseConnection();
//    Berror = true;
//}

//        }

//        private static bool FoundClassInDatabase(ref string listID)
//{
//    bool FoundClassInDatabaseRet = default;

//    On Error GoTo Errs
//            FoundClassInDatabaseRet = false;

//    try
//    {


//        var result = db.tblClass_Select(ClearAllControl.gblCompanyID, listID);

//        int resultCount = result.Count();

//        if (!(resultCount == 0))
//        {
//            FoundClassInDatabaseRet = true;
//            return FoundClassInDatabaseRet;

//        }
//    }
//    catch (Exception ex)
//    {

//    }

//    return FoundClassInDatabaseRet;



//Errs:
//    MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, _

//    MsgBoxStyle.Critical, _

//    "Error in FoundCustomerInListBox")

//        }

//private static bool FoundItemInDatabase(ref string listID)
//{
//    bool FoundItemInDatabaseRet = default;

//    On Error GoTo Errs
//            FoundItemInDatabaseRet = false;

//    try
//    {


//        var result = db.tblItemQuery_Select(listID);

//        int resultCount = result.Count();

//        if (!(resultCount == 0))
//        {
//            FoundItemInDatabaseRet = true;
//            return FoundItemInDatabaseRet;

//        }
//    }
//    catch (Exception ex)
//    {

//    }

//    return FoundItemInDatabaseRet;


//Errs:
//    MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, _

//    MsgBoxStyle.Critical, _

//    "Error in FoundCustomerInListBox")

//        }

//public static IMsgSetRequest GetLatestMsgSetRequest(QBSessionManager SessionManager)
//{
//    IMsgSetRequest GetLatestMsgSetRequestRet = default;
//    double supportedVersion;
//    supportedVersion = Conversions.ToDouble(QBFCLatestVersion(SessionManager));
//    if (supportedVersion >= 6.0d)
//    {
//        GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 6, 0);
//    }
//    else if (supportedVersion >= 5.0d)
//    {
//        GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 5, 0);
//    }
//    else if (supportedVersion >= 4.0d)
//    {
//        GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 4, 0);
//    }
//    else if (supportedVersion >= 3.0d)
//    {
//        GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 3, 0);
//    }
//    else if (supportedVersion >= 2.0d)
//    {
//        GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 2, 0);
//    }
//    else if (supportedVersion == 1.1d)
//    {
//        GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 1, 1);
//    }
//    else
//    {
//        Interaction.MsgBox("You are apparently running QuickBooks 2002 Release 1, we strongly recommend that you use QuickBooks' online update feature to obtain the latest fixes and enhancements", Constants.vbExclamation);
//        GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 1, 0);
//    }

//    return GetLatestMsgSetRequestRet;
//}
//public static string QBFCLatestVersion(QBSessionManager SessionManager)
//{
//    string QBFCLatestVersionRet = default;
//    string[] strXMLVersions;
//    Should be able to use this, but there appears to be a bug that may cause 2.0 to be returned
//    when it should not.
//    strXMLVersions = SessionManager.QBXMLVersionsForSession

//            IMsgSetRequest msgset;
//    Use oldest version to ensure that we work with any QuickBooks(US)
//            msgset = SessionManager.CreateMsgSetRequest("US", 1, 0);
//    msgset.AppendHostQueryRq();
//    IMsgSetResponse QueryResponse;
//    QueryResponse = SessionManager.DoRequests(msgset);
//    IResponse response;

//    The response list contains only one response,
//             which corresponds to our single HostQuery request
//            response = QueryResponse.ResponseList.GetAt(0);
//    IHostRet HostResponse;
//    HostResponse = response.Detail;
//    IBSTRList supportedVersions;
//    supportedVersions = HostResponse.SupportedQBXMLVersionList;

//    long i;
//    double vers;
//    double LastVers;
//    LastVers = 0d;
//    var loopTo = supportedVersions.Count - 1;
//    for (i = 0L; i <= loopTo; i++)
//    {
//        vers = Val(supportedVersions.GetAt(i));
//        if (vers > LastVers)
//        {
//            LastVers = vers;
//            QBFCLatestVersionRet = supportedVersions.GetAt(i);
//        }
//    }

//    return QBFCLatestVersionRet;
//}
//    }
//}