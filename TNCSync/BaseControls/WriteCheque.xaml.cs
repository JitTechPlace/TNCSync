using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TNCSync.Class;
using TNCSync.Model;
using Interop.QBFC16;
using Haley.MVVM;
using TNCSync.Class.DataBaseClass;
using Haley.Abstractions;
using TNCSync.Sessions;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for WriteCheque.xaml
    /// </summary>
    public partial class WriteCheque : UserControl
    {
        private bool bError;
        private string path = null;
        private string txnID;
        private SQLControls sql = new SQLControls();
        List<WriteCheques> cheques = new List<WriteCheques>();
        DataTable table = new DataTable();
        SqlDataAdapter sda = new SqlDataAdapter();
        public WriteCheque()
        {
            InitializeComponent();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
            PopulateTempleteCombobox();
           // UpdateBinding();
        }

        private static DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService ds;
        SessionManager sessionManager;
        private short maxVersion;

        #region CONNECTION TO QB
        private void connectToQB()
        {
            sessionManager = SessionManager.getInstance();
            maxVersion = sessionManager.QBsdkMajorVersion;
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

        #region WriteCheck Methods
        public void GetCheckTransactionDate(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                //Add checkQueryRequest
                ICheckQuery checkQuery = msgsetRequest.AppendCheckQueryRq();
                DateTime filterDate;
                filterDate = Microsoft.VisualBasic.DateAndTime.DateAdd(Microsoft.VisualBasic.DateInterval.Day, -7, DateTime.Now);
                checkQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                checkQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
                checkQuery.IncludeLineItems.SetValue(true);

                bool bDone = false;
                while (!bDone)
                {
                    // start looking for customer next in the list

                    // send the request to QB
                    IMsgSetResponse msgSetResponse = sessionManager.doRequest(true, ref msgsetRequest);

                    // MsgBox(msgSetRequest.ToXMLString())
                    FillExpenseAccountInDataBase(ref msgSetResponse, ref bDone, ref bError);
                }
                return;
            }
            catch
            {

            }
        }

        public void GetCheckTransaction(ref bool bError)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                //Add checkQueryRequest
                ICheckQuery checkQuery = msgsetRequest.AppendCheckQueryRq();
                DateTime filterDate;
                filterDate = Microsoft.VisualBasic.DateAndTime.DateAdd(Microsoft.VisualBasic.DateInterval.Day, -7, DateTime.Now);
                checkQuery.IncludeLineItems.SetValue(true);

                bool bDone = false;
                while (!bDone)
                {
                    // start looking for customer next in the list

                    // send the request to QB
                    IMsgSetResponse msgSetResponse = sessionManager.doRequest(true, ref msgsetRequest);

                    // MsgBox(msgSetRequest.ToXMLString())
                    FillExpenseAccountInDataBase(ref msgSetResponse, ref bDone, ref bError);
                }
                return;
            }
            catch
            {

            }
        }

        public void FillExpenseAccountInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
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
                if (response != null)
                {
                    if (response.StatusCode != double.Parse("0"))
                    {
                        // If the status is bad, report it to the user
                        //Interaction.MsgBox("Get check query unexpexcted Error - " + response.StatusMessage);
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

                // make sure we are processing the ChechQueryRs and 
                // the CheckRetList responses in this response list
                ICheckRetList checkRetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if(responseType == ENResponseType.rtCheckQueryRs & responseDetailType == ENObjectType.otCheckRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    checkRetList = (ICheckRetList)response.Detail;

                }
                else
                {
                    // bill, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                // Parse the query response and add the Check to the Check list box
                short count;
                short index;
                ICheckRet checkRet;
                count = (short)checkRetList.Count;
                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + checkRetList.Count);
                //we are done with the checkQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }
                db.tblQbCheck_Delete();
                db.tblQbCheckExpense_Delete();

                var amount = default(decimal);
                var amountInHomeCurrency = default(decimal);
                var LineAmount = default(decimal);
                var itemAmount = default(decimal);
                var itemLineCustomerName = default(string);
                var qty = default(int);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    //skip the customer if this is repeating from the last Query
                    checkRet = checkRetList.GetAt(index);
                    if(checkRet is null | checkRet.AccountRef.ListID is null | checkRet.TxnID is null)
                    {
                        bDone = true;
                        bError = true;
                        return;
                    }
                    //Only the first customerRet should be repeating and then lets just check to make sure we do not have the customer
                    //Just in case another app changed a customer right between our

                    //Declate a variable to retrive date
                    string accountRef = string.Empty;
                    if (checkRet.AccountRef != null)
                    {
                        accountRef = checkRet.AccountRef.ListID.GetValue();
                    }

                    string address = string.Empty;
                    // If Not (checkRet.Address.Addr1 Is Nothing) Then
                    // address = checkRet.Address.Addr1.GetValue
                    // End If
                    string addressBlock = string.Empty;
                    if (checkRet.AccountRef != null)
                    {
                        addressBlock = checkRet.AccountRef.FullName.GetValue();
                    }
                    // 'D:\SOURCECODE\QB Itegration\CHECK MANAGMENT\CHECKMANAGEMENT_PV - Al meera\CHECKMGMT\My Project\licenses.licx
                    if (checkRet.Amount != null)
                    {
                        amount = (decimal)checkRet.Amount.GetValue();
                    }
                    if (checkRet.AmountInHomeCurrency != null)
                    {
                        amountInHomeCurrency = (decimal)checkRet.AmountInHomeCurrency.GetValue();
                    }
                    string currencyRef = string.Empty;
                    // If Not (checkRet.CurrencyRef.ListID Is Nothing) Then
                    // currencyRef = checkRet.CurrencyRef.ListID.GetValue
                    // End If
                    // Dim dataExtRetList As String = String.Empty
                    // If Not (checkRet.DataExtRetList Is Nothing) Then
                    // dataExtRetList=checkRet.DataExtRetList.
                    // End If
                    string editSequence = string.Empty;
                    if (checkRet.EditSequence != null)
                    {
                        editSequence = checkRet.EditSequence.GetValue();
                    }
                    string exchangeRate = string.Empty;
                    if (checkRet.ExchangeRate != null)
                    {
                        exchangeRate = checkRet.ExchangeRate.GetValue().ToString();
                    }
                    string externalGuid = string.Empty;
                    if (checkRet.ExternalGUID != null)
                    {
                        externalGuid = checkRet.ExternalGUID.GetValue();
                    }
                    string isTaxIncluded = string.Empty;
                    if (checkRet.IsTaxIncluded != null)
                    {
                        isTaxIncluded = checkRet.IsTaxIncluded.GetValue().ToString();

                    }
                    string isToBePrinted = string.Empty;
                    if (checkRet.IsToBePrinted != null)
                    {
                        isToBePrinted = checkRet.IsToBePrinted.GetValue().ToString();

                    }
                    // Dim linkedTxnList As String = String.Empty
                    // If Not (checkRet.LinkedTxnList Is Nothing) Then

                    // End If
                    string memo = string.Empty;
                    if (checkRet.Memo != null)
                    {
                        memo = checkRet.Memo.GetValue();
                    }
                    // Dim orItemLineRetList As String = String.Empty
                    // If Not (checkRet.ORItemLineRetList. Is Nothing) Then

                    // End If
                    string payEntityRef = string.Empty;
                    string vendorListID = string.Empty;
                    if (checkRet.PayeeEntityRef != null)
                    {
                        payEntityRef = checkRet.PayeeEntityRef.FullName.GetValue();
                        vendorListID = checkRet.PayeeEntityRef.ListID.GetValue();
                    }
                    string refNumber = string.Empty;
                    if (checkRet.RefNumber != null)
                    {
                        refNumber = checkRet.RefNumber.GetValue();
                    }
                    if (refNumber == "4545")
                    {
                        int g = 0;
                    }
                    string salesTaxCodeRef = string.Empty;
                    if (checkRet.SalesTaxCodeRef != null)
                    {
                        salesTaxCodeRef = checkRet.SalesTaxCodeRef.ListID.GetValue();
                    }
                    string timeCreated = checkRet.TimeCreated.GetValue().ToString();
                    string timeModified = string.Empty;
                    if (checkRet.TimeModified != null)
                    {
                        timeModified = checkRet.TimeModified.GetValue().ToString();
                    }
                    DateTime txnDate = default;
                    if (checkRet.TxnDate != null)
                    {
                        txnDate = checkRet.TxnDate.GetValue();
                    }
                    string txnID = checkRet.TxnID.GetValue();
                    string txnNumber = string.Empty;
                    if (checkRet.TxnNumber != null)
                    {
                        txnNumber = checkRet.TxnNumber.GetValue().ToString();
                    }

                    string type = string.Empty;
                    if (checkRet.Type != null)
                    {
                        type = checkRet.Type.GetValue().ToString();
                    }
                    // 'Dim billcount = checkRet.LinkedTxnList.Count

                    // ' If (Not FoundExpenseInDatabase(checkRet.TxnID.GetValue())) Then
                    // Insert data in database code left..........

                    db.tblQbCheck_Insert(ClearAllControl.gblCompanyID, accountRef, address, addressBlock, amount, amountInHomeCurrency, currencyRef, editSequence, exchangeRate, externalGuid, isTaxIncluded, isToBePrinted, memo, payEntityRef, refNumber, salesTaxCodeRef, timeCreated, timeModified, txnDate, txnID, txnNumber, type);

                    int j = 0;
                    IExpenseLineRet ExpenseLineRet;
                    if (checkRet.ExpenseLineRetList != null)
                    {
                        var loopTo1 = checkRet.ExpenseLineRetList.Count - 1;
                        for (j = 0; j <= loopTo1; j++)
                        {
                            ExpenseLineRet = checkRet.ExpenseLineRetList.GetAt(j);
                            string LineAccountRef = string.Empty;
                            if (ExpenseLineRet.AccountRef != null)
                            {
                                LineAccountRef = ExpenseLineRet.AccountRef.ListID.GetValue();
                            }
                            string LineFullName = string.Empty;
                            if (ExpenseLineRet.AccountRef != null)
                            {
                                LineFullName = ExpenseLineRet.AccountRef.FullName.GetValue();
                            }
                            if (ExpenseLineRet.Amount != null)
                            {
                                LineAmount = (decimal)ExpenseLineRet.Amount.GetValue();
                            }
                            string LineBillableStatus = string.Empty;
                            if (ExpenseLineRet.BillableStatus != null)
                            {
                                LineBillableStatus = ((int)ExpenseLineRet.BillableStatus.GetValue()).ToString();
                            }
                            string LineClassListID = string.Empty;
                            if (ExpenseLineRet.ClassRef != null)
                            {
                                LineClassListID = ExpenseLineRet.ClassRef.ListID.GetValue();
                            }
                            string LineClassFullName = string.Empty;
                            if (ExpenseLineRet.ClassRef != null)
                            {
                                LineClassFullName = ExpenseLineRet.ClassRef.FullName.GetValue();
                            }
                            string LineCustomerRefListId = string.Empty;
                            if (ExpenseLineRet.CustomerRef != null)
                            {
                                LineCustomerRefListId = ExpenseLineRet.CustomerRef.ListID.GetValue();
                            }
                            string LineCustomerRefFullName = string.Empty;
                            if (ExpenseLineRet.CustomerRef != null)
                            {
                                LineCustomerRefFullName = ExpenseLineRet.CustomerRef.FullName.GetValue();
                            }
                            string LineMemo = string.Empty;
                            if (ExpenseLineRet.Memo != null)
                            {
                                LineMemo = ExpenseLineRet.Memo.GetValue();
                            }
                            string LineTxnLineID = string.Empty;
                            if (ExpenseLineRet.TxnLineID != null)
                            {
                                LineTxnLineID = ExpenseLineRet.TxnLineID.GetValue();
                            }
                            string LineType = string.Empty;
                            if (ExpenseLineRet.Type != null)
                            {
                                LineType = ExpenseLineRet.Type.GetValue().ToString();
                            }
                            db.tblQbCheckExpense_Insert(txnID, LineAccountRef, LineFullName, LineAmount, LineBillableStatus, LineClassListID, LineClassFullName, LineCustomerRefListId, LineCustomerRefFullName, LineMemo, LineTxnLineID, LineType, ClearAllControl.gblCompanyID);
                        }


                        int k = 0;

                        IORItemLineRet itemLineRet;
                        if (checkRet.ORItemLineRetList != null)
                        {
                            var loopTo2 = checkRet.ORItemLineRetList.Count - 1;
                            for (k = 0; k <= loopTo2; k++)
                            {
                                itemLineRet = checkRet.ORItemLineRetList.GetAt(k);
                                if (itemLineRet.ItemLineRet.Amount != null)
                                {
                                    itemAmount = (decimal)itemLineRet.ItemLineRet.Amount.GetValue();
                                }
                                if (itemLineRet.ItemLineRet.CustomerRef != null)
                                {
                                    itemLineCustomerName = itemLineRet.ItemLineRet.CustomerRef.FullName.GetValue();
                                }
                                string itemLineDesc = string.Empty;
                                if (itemLineRet.ItemLineRet.Desc != null)
                                {
                                    itemLineDesc = itemLineRet.ItemLineRet.Desc.GetValue();
                                }
                                string itemLineFullName = string.Empty;
                                if (itemLineRet.ItemLineRet.ItemRef != null)
                                {
                                    itemLineFullName = itemLineRet.ItemLineRet.ItemRef.FullName.GetValue();
                                }
                                string itemLineTxnId = string.Empty;
                                if (itemLineRet.ItemLineRet.TxnLineID != null)
                                {
                                    itemLineTxnId = itemLineRet.ItemLineRet.TxnLineID.GetValue();
                                }
                                double itemLineCost = 0d;
                                if (itemLineRet.ItemLineRet.Cost != null)
                                {
                                    itemLineCost = itemLineRet.ItemLineRet.Cost.GetValue();
                                }
                                if (itemLineRet.ItemLineRet.Quantity != null)
                                {
                                    qty = (int)Math.Round(itemLineRet.ItemLineRet.Quantity.GetValue());
                                }
                                db.tblQbCheckItemLine_Insert(txnID, itemLineDesc, itemLineFullName, itemLineCustomerName, itemAmount, itemLineTxnId, (decimal?)itemLineCost, qty);
                            }
                        }
                    }
                }
                return;
            }
            catch
            {
                //Interaction.MsgBox("HRESULT = " + Information.Err().Number + "-" + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");
                bDone = true;
                bError = true;
            }
        }

        public void FillBillCheckInDB(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
        {
            try
            {

            }
            catch
            {

            }
        }

        #endregion

        #region Methods
        private void LoadPayeeCombobox(int cID)
        {
            try
            {
                payeeCmbx.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.execquery("SELECT distinct payEntityRef from tblQbCheck where companyID=@CompanyID");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        payeeCmbx.Items.Add(r["payEntityRef"]);
                    payeeCmbx.SelectedIndex = -1;
                }

                ///Need to check when it was working or not
                var col = new System.Windows.Forms.AutoCompleteStringCollection();
                for (int i = 0, loopTo = sql.sqlds.Tables[0].Rows.Count - 1; i <= loopTo; i++)
                    col.Add(sql.sqlds.Tables[0].Rows[i]["payeeFullName"].ToString());
                payeeCmbx.SelectedItem = System.Windows.Forms.AutoCompleteSource.CustomSource;
                payeeCmbx.ItemsSource = col;
                payeeCmbx.SelectedItem = System.Windows.Forms.AutoCompleteMode.Suggest;
            }
            catch (Exception ex)
            {

            }

            #region Old
            //string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            //SqlConnection sqlconn = new SqlConnection(conn);
            //sqlconn.Open();
            //SqlCommand cmd = new SqlCommand("SELECT distinct payEntityRef FROM tblQbCheck", sqlconn);
            //SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            //DataTable table = new DataTable();
            //sdr.Fill(table);
            //payeeCmbx.ItemsSource = table.DefaultView;
            //payeeCmbx.DisplayMemberPath = "payEntityRef";
            #endregion
        }

        private void LoadGrid()
        {
            try
            {
                var result = db.tblQBCheck_Select_TNCS();
                grdChequeList.Columns.Clear();
                grdChequeList.ItemsSource = result.ToList();
            }
            catch
            {

            }

            #region Old
            //string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            //SqlConnection sqlconn = new SqlConnection(conn);
            //SqlCommand cmd = new SqlCommand("WriteCheque_TNCS", sqlconn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //sda.SelectCommand = cmd;
            //sda.Fill(table);
            //grdChequeList.ItemsSource = table.DefaultView;
            ////grdChequeList.DisplayMemberPath = "TemplateName";
            ////grdChequeList.SelectedIndex = -1;

            ////Load the Payee Combobox
            ////payeeCmbx.ItemsSource = table.DefaultView;
            ////payeeCmbx.DisplayMemberPath = "Payee Name";
            #endregion
        }

        private void PopulateTempleteCombobox()
        {
            tmpltCmbx.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Check Payment Voucher' and Status='True' ");
            if (sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    tmpltCmbx.Items.Add(r["TemplateName"]);
                tmpltCmbx.SelectedIndex = 0;
            }


            #region Old
            //string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            //SqlConnection sqlconn = new SqlConnection(conn);
            //sqlconn.Open();
            //SqlCommand cmd = new SqlCommand("SELECT * FROM Templates where TemplateType ='Check Payment Voucher'", sqlconn);
            //SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            //DataTable table = new DataTable();
            //sdr.Fill(table);
            //tmpltCmbx.ItemsSource = table.DefaultView;
            //tmpltCmbx.DisplayMemberPath = "TemplateName";
            //tmpltCmbx.SelectedIndex = -1;
            #endregion
        }

        private void GetTempPath(string Paths)
        {
            sql.addparam("@name", Paths);
            sql.execquery("Select TemplatePath from Templates where TemplateName = @name ");
            if (sql.recordcount > 0)
            {
                path = sql.sqlds.Tables[0].Rows[0]["Templatepath"].ToString();
            }
        }

        private void srchbtn_Click(object sender, RoutedEventArgs e)
        {
            WriteCheques_DA Wcda = new WriteCheques_DA();
            cheques = Wcda.GetCheques(payeeCmbx.Text);
            UpdateBinding();

            #region Old
            ////get the selected value from the combobox
            //string selectedValue = payeeCmbx.SelectedItem as string;
            ////Clear any existing filters
            //grdChequeList.Items.Filter = null;

            ////Apply new filter based on the selected value
            //ICollectionView view = CollectionViewSource.GetDefaultView(grdChequeList.ItemsSource);
            //if (selectedValue != null)
            //{
            //    //view.Filter = (item) =>
            //    //{
            //    //    return((string)item).
            //    //};
            //}

            ////DataRowView payee = (DataRowView)payeeCmbx.SelectedItem;
            ////grdChequeList.ItemsSource = table.DefaultView;
            #endregion
        }

        private void UpdateBinding()
        {
            grdChequeList.ItemsSource = cheques;
            grdChequeList.DisplayMemberPath = "WriteCheques";
        }
        #endregion


        #region Events
        private void chequeDGSyncall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bError = false;
                connectToQB();
                GetCheckTransaction(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "WriteCheck Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "WriteCheck Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }

                LoadPayeeCombobox(ClearAllControl.gblCompanyID);
                LoadGrid();
            }
            catch
            {

            }
        }

        private void chequeDGSync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bError = false;
                connectToQB();
                GetCheckTransactionDate(ref  bError, dpFrmDate.DisplayDate,  dpToDate.DisplayDate);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "WriteCheck Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "WriteCheck Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }

                LoadPayeeCombobox(ClearAllControl.gblCompanyID);
                LoadGrid();

            }
            catch
            {

            }
        }

        private void tmpltCmbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempPath(tmpltCmbx.Text);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            payeeCmbx.Text = "";
            LoadPayeeCombobox(ClearAllControl.gblCompanyID);
            LoadGrid();
        }
        #endregion
    }
}
