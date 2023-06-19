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
using Haley.Abstractions;
using Interop.QBFC15;
using SAPBusinessObjects.WPF.Viewer;
using TNCSync.Class;
using TNCSync.Class.DataBaseClass;
using TNCSync.Sessions;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for ChartofAccount.xaml
    /// </summary>
    public partial class ChartofAccount : UserControl
    {
        private bool bError;
        public ChartofAccount()
        {
            InitializeComponent();
        }
        //QBConnect qbConnect = new QBConnect();
        SessionManager sessionManager;
        private short maxVersion;
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService ds;

        #region CONNECTION TO QB
        private void connectToQB()
        {
            sessionManager = SessionManager.getInstance();
            maxVersion = sessionManager.QBsdkMajorVersion;
        }
        private IMsgSetResponse processRequestFromQB(IMsgSetRequest requestSet)
        {
            try
            {
                //MessageBox.Show(requestSet.ToXMLString());
                IMsgSetResponse responseSet = sessionManager.doRequest(true, ref requestSet);
                //MessageBox.Show(responseSet.ToXMLString());
                return responseSet;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
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
                    MessageBox.Show(e.Message);
                }
            }
        }
        #endregion

        private void syncCoaList_Click(object sender, RoutedEventArgs e)
        {
            connectToQB();
            bError = false;
            GetChartofAccount(ref bError);
            populateDatagrid();
        }

        private void populateDatagrid()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            //string sqlquery = "SELECT * FROM tblVendor";
            SqlCommand cmd = new SqlCommand("SELECT Name, FullName,AccountName FROM tblAccounts", sqlconn);
            sqlconn.Open();
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            grdCoaLst.ItemsSource = table.DefaultView;
            sqlconn.Close();
        }

        public void GetChartofAccount(ref bool bError)
        {
            try
            {
                //Make sure we do not have any old requests still defined
                IMsgSetRequest msgSetRequest = sessionManager.getMsgSetRequest();
                msgSetRequest.ClearRequests();
                //set the OnError attribute to continueOnError
                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                //Add the CustomerQuery request
                IAccountQuery chartAccountQuery = msgSetRequest.AppendAccountQueryRq();
                chartAccountQuery.ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
                bool bDone = false;
                string firstFullName = "!";
                while (!bDone)
                {
                    //start looking for customer next in the list
                    chartAccountQuery.ORAccountListQuery.AccountListFilter.ORNameFilter.NameRangeFilter.FromName.SetValue(firstFullName);
                    //send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgSetRequest);
                    // msgSetResponse = sessionManager.DoRequests(msgSetRequest);

                    FillChartofAccountDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                ds.ShowDialog("HRESULT = ", "Error in FoundIn Account database");
                bError = true;
            }
        }

        public void FillChartofAccountDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
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

                // go thru each response and process the response.
                // this example will only have one response in the list
                // so we will look at index=0
                IResponse response;
                response = responseList.GetAt(0);
                if (response == null)
                {
                    if (response.StatusCode != 0)
                    {
                        // If the status is bad, report it to the user
                        MessageBox.Show("FillChartOfAccountListBox unexpexcted Error - " + response.StatusMessage);
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

                // make sure we are processing the CustomerQueryRs and 
                // the CustomerRetList responses in this response list
                IAccountRetList CoARetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtAccountQueryRs & responseDetailType == ENObjectType.otAccountRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    CoARetList = response.Detail as IAccountRetList;
                }
                else
                {
                    // bail, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                // Parse the query response and add the Customers to the Customer list box
                short count;
                short index;
                IAccountRet CoaRet;
                count = (short)CoARetList.Count;
                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + CoARetList.Count);
                // we are done with the customerQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }

                var TimeModified = default(DateTime);
                var EditSequence = default(int);
                var Sublevel = default(int);
                var balance = default(decimal);
                var TotalBalance = default(decimal);
                var PdcPostingAcct = default(int);
                var DaysBefore = default(int);
                var DaysAfter = default(int);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    // skip this customer if this is a repeat from the last query
                    CoaRet = CoARetList.GetAt(index);
                    if (CoaRet is null | CoaRet.FullName is null | CoaRet.ListID is null)
                    {
                        bDone = true;
                        bError = true;
                        return;
                    }
                    // only the first customerRet should be repeating and then
                    // lets just check to make sure we do not have the customer
                    // just in case another app changed a customer right between our
                    // queries.
                    bool localFoundAccountInDatabase()
                    {
                        var arglistID2 = CoaRet.ListID.GetValue();
                        var ret = FoundAccountInDatabase(ref arglistID2); 
                        return ret;
                    }

                    if (!localFoundAccountInDatabase())
                    {
                        // Insert data in database code left..........
                        string listID = CoaRet.ListID.GetValue();
                        DateTime TimeCreated = CoaRet.TimeCreated.GetValue();

                        if (CoaRet.TimeModified != null)
                        {
                            TimeModified = CoaRet.TimeModified.GetValue();
                        }

                        if (CoaRet.EditSequence != null)
                        {
                            EditSequence = int.Parse(CoaRet.EditSequence.GetValue().ToString());
                        }


                        string Name = string.Empty;
                        if (CoaRet.Name != null)
                        {
                            Name = CoaRet.Name.GetValue();
                        }
                        string FullName = string.Empty;
                        if (CoaRet.FullName != null)
                        {
                            FullName = CoaRet.FullName.GetValue();
                        }
                        string AccountName = string.Empty;
                        if (CoaRet.FullName != null & CoaRet.AccountNumber != null)
                        {

                            AccountName = CoaRet.AccountNumber.GetValue() + "·" + CoaRet.FullName.GetValue();
                        }
                        string Parent = string.Empty;
                        // If Not (CoaRet.ParentRef Is Nothing) Then
                        // Parent = CoaRet.ParentRef.GetValue
                        // End If
                        string isActive = "Y";
                        if (CoaRet.Sublevel != null)
                        {
                            Sublevel = CoaRet.Sublevel.GetValue();
                        }
                        string AccountType = string.Empty;
                        if (CoaRet.AccountType != null)
                        {
                            AccountType = CoaRet.AccountType.GetValue().ToString();
                        }
                        string SpecialAccountType = string.Empty;
                        if (CoaRet.SpecialAccountType != null)
                        {
                            SpecialAccountType = CoaRet.SpecialAccountType.GetValue().ToString();
                        }
                        string AccountNumber = string.Empty;
                        if (CoaRet.AccountNumber != null)
                        {
                            AccountNumber = CoaRet.AccountNumber.GetValue();
                        }
                        if (CoaRet.Balance != null)
                        {
                            balance = (decimal)CoaRet.Balance.GetValue();
                        }
                        if (CoaRet.TotalBalance != null)
                        {
                            TotalBalance = (decimal)CoaRet.TotalBalance.GetValue(); 
                        }
                        string CashFlowClassification = string.Empty;
                        if (CoaRet.CashFlowClassification != null)
                        {
                            CashFlowClassification = CoaRet.CashFlowClassification.GetValue().ToString();
                        }
                        string ACTLEVELS = string.Empty;

                        int SHOW_MENU = 0;

                        string DESCRIPTION = string.Empty;
                        if (CoaRet.Desc != null)
                        {
                            DESCRIPTION = CoaRet.Desc.GetValue();
                        }
                        string NOTES = string.Empty;
                        var BalanceDate = DateTime.Now;
                        string ACTLEVELSwithNO = string.Empty;
                        string AutoPosting = string.Empty;
                        string PostingOn = string.Empty;


                        db.tblAccounts_Insert(ClearAllControl.gblCompanyID, listID, TimeCreated, TimeModified, EditSequence, Name, FullName, AccountName, Parent, isActive, Sublevel, AccountType, SpecialAccountType, AccountNumber, balance, TotalBalance, CashFlowClassification, ACTLEVELS, SHOW_MENU, DESCRIPTION, NOTES, BalanceDate, ACTLEVELSwithNO, AutoPosting, PostingOn, PdcPostingAcct, DaysBefore, DaysAfter);

                    }

                }

                return;
            }
            catch
            {

                //Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");
                ds.ShowDialog("HRESULT = ", "Error in FillChartOfAccountListBox");
                bDone = true;
                bError = true;
            }
        }
        private bool FoundAccountInDatabase(ref string listID)
        {
            bool FoundAccountInDatabaseRet = default;

            FoundAccountInDatabaseRet = false;
            try
            {

                // go thru our list box and find the item which was modified
                //short i;
                //short numCustomers;
                // check in database for existing customer
                var result = db.tblAccounts_Select(listID, null, ClearAllControl.gblCompanyID);
                int resultCount = result.Count();

                if (!(resultCount == 0))
                {
                    FoundAccountInDatabaseRet = true;
                    return FoundAccountInDatabaseRet;
                }
            }
            catch
            {

                //Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FoundIn Account database");
                ds.ShowDialog("HRESULT = ", "Error in FoundIn Account database");
            }
            return FoundAccountInDatabaseRet;

        }
    }
}
