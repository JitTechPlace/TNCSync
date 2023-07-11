using Haley.Abstractions;
using Haley.MVVM;
using Interop.QBFC15;
//using Interop.QBFC12;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TNCSync.Class;
using TNCSync.Class.DataBaseClass;
using TNCSync.Class.SP;
using TNCSync.Sessions;
using TNCSync.ViewModel;
////using TNCSync.Class.QuickBookData;
///
namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for VendorsList.xaml
    /// </summary>
    public partial class VendorsList : UserControl
    {
        private bool bError;
        private short maxVersion;
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService ds;
        private string listID = null;
        private string QvendorName = null;
        private string QCompanyName = null;
        private string BillAddress1 = null;
        private string Phone = null;
        private string Fax = null;
        private string Email = null;
        private string Contact = null;

        public VendorsList()
        {
            InitializeComponent();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
           
        }
        SessionManager sessionManager;

        #region CONNECTION TO QB
        private void connectToQB()
        {
            sessionManager = SessionManager.getInstance();
            maxVersion = sessionManager.QBsdkMajorVersion;
        }
        //private IMsgSetResponse processRequestFromQB(IMsgSetRequest requestSet)
        //{
        //    try
        //    {
        //        //MessageBox.Show(requestSet.ToXMLString());
        //        IMsgSetResponse responseSet = sessionManager.doRequest(true, ref requestSet);
        //        //MessageBox.Show(responseSet.ToXMLString());
        //        return responseSet;
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message);
        //        return null;
        //    }
        //}
        private void disconnectFromQB()
        {
            if (sessionManager != null)
            {
                try
                {
                    sessionManager.endSession();
                    sessionManager.closeConnection();
                    sessionManager = null;
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.Message);
                    ds.SendToast(e.Message,"TNC-Sync", Haley.Enums.NotificationIcon.Error);
                }
            }
        }
        #endregion

        private void syncVendorList_Click(object sender, RoutedEventArgs e)
        {
            connectToQB();
            bError = false;
            getVendor(ref bError);
            if (!bError)
            {
                ds.SendToast("Synchronized ", "Vendors List has been synchronized successfully", Haley.Enums.NotificationIcon.Success);
                disconnectFromQB();
                populateDatagrid();
            }
            else
            {
                disconnectFromQB();
                //populateDatagrid();
            }
        }


        private void populateDatagrid()
        {
            try
            {
                //var query = from tblVendor in db.tblVendor_Select(ClearAllControl.gblCompanyID, listID, Name, QCompanyName, BillAddress1, Phone, Fax, Email, Contact)select tblVendor;
                var result = db.tblVendor_Select(ClearAllControl.gblCompanyID, listID, QvendorName, QCompanyName, BillAddress1, Phone, Fax, Email, Contact);
                grdVendorLst.ItemsSource = result.ToList();
            }
            catch (Exception ex)
            {

            }
            #region old
            //string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            //SqlConnection sqlconn = new SqlConnection(conn);
            //SqlCommand cmd = new SqlCommand("tblVendor_Select_TNCS", sqlconn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //DataTable table = new DataTable();
            //SqlDataAdapter sda = new SqlDataAdapter();
            //sda.SelectCommand = cmd;
            ////cmd.Parameters.Add(param);
            //sda.Fill(table);
            //grdVendorLst.ItemsSource = table.DefaultView;
            #endregion
        }

        public void getVendor(ref bool bError)
        {
            try
            {

                //IMsgSetRequest msgsetRequest = null;
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                // Add the CustomerQuery request

                IVendorQuery vendortQuery;
                vendortQuery = msgsetRequest.AppendVendorQueryRq();
                vendortQuery.ORVendorListQuery.VendorListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
                vendortQuery.ORVendorListQuery.VendorListFilter.Type.GetValue();
                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillVendorInDatabase(ref responseSet, ref bDone, ref bError);

                }
                return;
            }
            catch
            {
                ds.SendToast("Not Responding", "Unable to connect with QuickBook,Pls Check QuickBook Company File Opened", Haley.Enums.NotificationIcon.Error);
                // Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in GetCustomers");
                bError = true;
            }
        }

        public static void FillVendorInDatabase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
        {
            try
            {

                // check to make sure we have objects to access first
                // and that there are responses in the list
                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

                {
                    bDone = true;
                    bError = true;
                    return;
                }
                // Start parsing the response list
                IResponseList responseList;
                responseList = msgSetResponse.ResponseList;
                IResponse response;
                response = responseList.GetAt(0);
                if (response !=null)
                {
                    if (response.StatusCode != double.Parse("0"))
                    {
                        // If the status is bad, report it to the user
                       // ds.ShowDialog("","FillVendorListBox unexpexcted Error - " + response.StatusMessage,Haley.Enums.NotificationIcon.Error);
                        bDone = true;
                        bError = true;
                        return;
                    }
                }

                // first make sure we have a response object to handle
                if (response is null | response.Type is null | response.Detail is null | response.Detail.Type is null)


                {
                    bDone = true;
                    bError = true;
                    return;
                }

                // make sure we are processing the VendorQueryRs and 
                // the IVendorRetList responses in this response list
                IVendorRetList VendorRetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtVendorQueryRs & responseDetailType == ENObjectType.otVendorRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    VendorRetList = (IVendorRetList)response.Detail;
                }
                else
                {
                    // bill, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                short count;
                short index;
                count = (short)VendorRetList.Count;
                IVendorRet VendorRet;

                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + VendorRetList.Count);
                // we are done with the customerQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }

                var EditSequence = default(int);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    VendorRet = VendorRetList.GetAt(index);

                    if (VendorRet is null | VendorRet.Name is null | VendorRet.ListID is null)

                    {
                        bDone = true;
                        bError = true;
                        return;
                    }
                    // Featch value of vendor

                    string listID = VendorRet.ListID.GetValue();
                    string Name = "";
                    if (VendorRet.Name != null)
                    {
                        Name = VendorRet.Name.GetValue();
                    }

                    string TimeCreated = "";
                    if (VendorRet.TimeCreated != null)
                    {
                        TimeCreated = VendorRet.TimeCreated.GetValue().ToString();
                    }
                    string TimeModified = "";
                    if (VendorRet.TimeModified != null)
                    {
                        TimeModified = VendorRet.TimeModified.GetValue().ToString();
                    }
                    if (VendorRet.EditSequence != null)
                    {
                        EditSequence = int.Parse(VendorRet.EditSequence.GetValue());
                    }
                    string IsActive = "";
                    if (VendorRet.IsActive != null)
                    {
                        IsActive = VendorRet.IsActive.GetValue().ToString();
                    }
                    string CompanyName = "";
                    if (VendorRet.CompanyName != null)
                    {
                        CompanyName = VendorRet.CompanyName.GetValue();
                    }
                    string Salutation = "";
                    if (VendorRet.Salutation != null)
                    {
                        Salutation = VendorRet.Salutation.GetValue();
                    }
                    string FirstName = "";

                    if (VendorRet.FirstName != null)
                    {
                        FirstName = VendorRet.FirstName.GetValue();
                    }
                    string MiddleName = "";
                    if (VendorRet.MiddleName != null)
                    {
                        MiddleName = VendorRet.MiddleName.GetValue();
                    }
                    string LastName = "";
                    if (VendorRet.LastName != null)
                    {
                        LastName = VendorRet.LastName.GetValue();
                    }
                    string BillAddress1 = "";
                    string BillAddress2 = "";
                    // Address part
                    if (VendorRet.VendorAddress != null)
                    {

                        if (VendorRet.VendorAddress.Addr1 != null)
                        {
                            BillAddress1 = VendorRet.VendorAddress.Addr1.GetValue();
                        }

                        if (VendorRet.VendorAddress.Addr2 != null)
                        {
                            BillAddress2 = VendorRet.VendorAddress.Addr2.GetValue();
                        }
                    }
                    string Phone = "";
                    if (VendorRet.Phone != null)
                    {
                        Phone = VendorRet.Phone.GetValue();
                    }
                    string AltPhone = "";
                    if (VendorRet.AltPhone != null)
                    {
                        AltPhone = VendorRet.AltPhone.GetValue();
                    }
                    string Fax = "";
                    if (VendorRet.Fax != null)
                    {
                        Fax = VendorRet.Fax.GetValue();
                    }
                    string Email = "";
                    if (VendorRet.Email != null)
                    {
                        Email = VendorRet.Email.GetValue();
                    }
                    string Contact = "";
                    if (VendorRet.Contact != null)
                    {
                        Contact = VendorRet.Contact.GetValue();
                    }
                    string AltContact = "";
                    if (VendorRet.AltContact != null)
                    {
                        AltContact = VendorRet.AltContact.GetValue();
                    }
                    string Balance = "";
                    if (VendorRet.Balance != null)
                    {
                        Balance = VendorRet.Balance.GetValue().ToString();
                    }
                    string AccountNumber = string.Empty;
                    if (VendorRet.AccountNumber != null)
                    {
                        AccountNumber = VendorRet.AccountNumber.GetValue();
                    }
                    string CreditLimit = "";
                    if (VendorRet.CreditLimit != null)
                    {
                        CreditLimit = VendorRet.CreditLimit.GetValue().ToString();
                    }
                    string PrintChequeAs = "";
                    if (VendorRet.NameOnCheck != null)
                    {
                        PrintChequeAs = VendorRet.NameOnCheck.GetValue();
                    }

                    bool localFoundVendorInDatabase() { string arglistID = VendorRet.ListID.GetValue(); var ret = FoundVendorInDatabase(ref arglistID); return ret; }

                    if (!localFoundVendorInDatabase())
                    {
                        // Insert vendor data in database
                        db.tblVendor_Insert(ClearAllControl.gblCompanyID, listID, DateTime.Parse(TimeCreated), DateTime.Parse(TimeModified), EditSequence, Name, IsActive, CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, Phone, AltPhone, Fax, Email, Contact, AltContact, decimal.Parse(Balance), AccountNumber, CreditLimit, PrintChequeAs);
                    }

                    else
                    {
                        // Update vendor table for vendor if available
                        db.tblVendor_Update(ClearAllControl.gblCompanyID, listID, DateTime.Parse(TimeCreated), DateTime.Parse(TimeModified), EditSequence, Name, IsActive, CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, Phone, AltPhone, Fax, Email, Contact, AltContact,decimal.Parse(Balance), AccountNumber, CreditLimit, PrintChequeAs);

                    }

                }

                return;
            }
            catch
            {

                //Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");


                bDone = true;
                bError = true;
            }
        }

        private static bool FoundVendorInDatabase(ref string listID)
        {
            bool FoundVendorInDatabaseRet = default;

            // On Error GoTo Errs
            FoundVendorInDatabaseRet = false;

            try
            {

                var result = db.tblVendor_Select(ClearAllControl.gblCompanyID, listID, null, null, null, null, null, null, null);

                int resultCount = result.Count();

                if (!(resultCount == 0))
                {
                    FoundVendorInDatabaseRet = true;
                    return FoundVendorInDatabaseRet;

                }
            }
            catch (Exception ex)
            {

            }

            return FoundVendorInDatabaseRet;



            // Errs:
            // MsgBox("HRESULT = " & Err.Number & " (" & Hex(Err.Number) & ") " & vbCrLf & vbCrLf & Err.Description, _
            // MsgBoxStyle.Critical, _
            // "Error in FoundCustomerInListBox")

        }

        private void VendorList_Loaded(object sender, RoutedEventArgs e)
        {
          // populateDatagrid();
        }
    }
}
