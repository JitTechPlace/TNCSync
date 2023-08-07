using Haley.Abstractions;
using Haley.MVVM;
using System;
//using Interop.QBFC15;
using Interop.QBFC16;
using System.Collections.Generic;
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
using TNCSync.Class.DataBaseClass;
using TNCSync.Sessions;
using TNCSync.Class;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for CreditNote.xaml
    /// </summary>
    public partial class CreditNote : UserControl
    {
        private bool bError;
        private string path = null;
        private SQLControls sql = new SQLControls();
        private bool booSessionBegun;

        public CreditNote()
        {
            InitializeComponent();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
            LoadCNCustomer(ClearAllControl.gblCompanyID);
            PopulateTempleteCombobox();
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
                    //MessageBox.Show(e.Message);
                    ds.SendToast(e.Message, "TNC-Sync", Haley.Enums.NotificationIcon.Error);
                }
            }
        }
        #endregion

        #region CNMethods
        public void GetCreditTransaction(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                ICreditMemoQuery creditMemoQuery = msgsetRequest.AppendCreditMemoQueryRq();
                creditMemoQuery.IncludeLineItems.SetValue(true);
                creditMemoQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                creditMemoQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillCreditTransInDataBaseAll(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void GetCreditTransactions(ref bool bError)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                // Add the BillQuery request
                ICreditMemoQuery creditMemoQuery = msgsetRequest.AppendCreditMemoQueryRq();
                creditMemoQuery.IncludeLineItems.SetValue(true);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillCreditTransInDataBaseAll(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void FillCreditTransInDataBaseAll(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
        {
            try
            {
                string glvConnection;
                string strIntConnection;
                string sql;
                var intDs = new DataSet();
                var dst = new DataSet();
                var dsCustomer = new DataSet();
                var dsClinetCode = new DataSet();
                // check to make sure we have objects to access first
                // and that there are responses in the list

                if (msgSetResponse is null | msgSetResponse.ResponseList is null | msgSetResponse.ResponseList.Count <= 0)

                {
                    bDone = true;
                    bError = true;
                    return;
                }
                //Start Parsing the response List
                IResponseList responseList;
                responseList = msgSetResponse.ResponseList;
                IResponse response;
                response = responseList.GetAt(0);
                if (response != null)
                {
                    if (response.StatusCode != double.Parse("0"))
                    {
                        // If the status is bad, report it to the user
                        ds.ShowDialog("","Get check query unexpexcted Error - " + response.StatusMessage, Haley.Enums.NotificationIcon.Info);
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

                //Make sure We are Processing the CreditMemoQueryRs and the CreditMemoRetList responses in this list
                ICreditMemoRetList creditMemoRetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtCreditMemoQueryRs & responseDetailType == ENObjectType.otCreditMemoRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    creditMemoRetList = (ICreditMemoRetList)response.Detail;
                }
                else
                {
                    // bail, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                // Parse the query response and add the CreditNote to the CreditNote list
                short count;
                short index;
                ICreditMemoRet creditMemoRet;
                count = (short)creditMemoRetList.Count;
                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + creditMemoRetList.Count);
                // we are done with the customerQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }

                db.tblCredit_DeleteAll(ClearAllControl.gblCompanyID);
                db.tblcreditLIne_Delete();

                var SalesTaxPercentage = default(string);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    // skip this customer if this is a repeat from the last query
                    creditMemoRet = creditMemoRetList.GetAt(index);
                    if (creditMemoRet is null | creditMemoRet.CustomerRef.ListID is null | creditMemoRet.TxnID is null)

                    {
                        bDone = true;
                        bError = true;
                        return;
                    }


                    // 'declare varibale to retrive data 
                    string BillAddress1 = string.Empty;
                    string BillAddress2 = string.Empty;
                    string BillAddress3 = string.Empty;
                    string BillAddress4 = string.Empty;
                    string BillAddress5 = string.Empty;
                    string BillAddressCity = string.Empty;
                    string BillAddressState = string.Empty;
                    string BillAddressPostalCode = string.Empty;
                    string BillAddressCountry = string.Empty;
                    string BillAddressNote = string.Empty;


                    string TxnID = creditMemoRet.TxnID.GetValue();
                    string CustomerRefKey = string.Empty;
                    if (creditMemoRet.CustomerRef != null)
                    {
                        CustomerRefKey = creditMemoRet.CustomerRef.ListID.GetValue();
                    }
                    string customerName = string.Empty;
                    if (creditMemoRet.CustomerRef != null)
                    {
                        customerName = creditMemoRet.CustomerRef.FullName.GetValue();
                    }

                    string ClassRefKey = string.Empty;
                    if (creditMemoRet.ClassRef != null)
                    {
                        ClassRefKey = creditMemoRet.ClassRef.ListID.GetValue();
                    }

                    string ARAccountRefKey = string.Empty;
                    if (creditMemoRet.ARAccountRef != null)
                    {
                        ARAccountRefKey = creditMemoRet.ARAccountRef.ListID.GetValue();

                    }
                    string TemplateRefKey = string.Empty;
                    if (creditMemoRet.TemplateRef != null)
                    {
                        TemplateRefKey = creditMemoRet.TemplateRef.ListID.GetValue();
                    }
                    var TxnDate = creditMemoRet.TxnDate.GetValue();

                    string RefNumber = string.Empty;
                    if (creditMemoRet.RefNumber != null)
                    {
                        RefNumber = creditMemoRet.RefNumber.GetValue();
                    }

                    if (RefNumber == "301R000005")
                    {
                        index = index;
                    }
                    object discAcc;
                    object discAmt;

                    if (creditMemoRet.DiscountLineRet != null)
                    {
                        discAcc = creditMemoRet.DiscountLineRet.AccountRef.ListID.GetValue();
                        discAmt = creditMemoRet.DiscountLineRet.ORDiscountLineRet.Amount.GetValue();
                    }
                    // Bill Address


                    if (creditMemoRet.BillAddress != null)
                    {


                        if (creditMemoRet.BillAddress.Addr1 != null)
                        {
                            BillAddress1 = creditMemoRet.BillAddress.Addr1.GetValue();
                        }


                        if (creditMemoRet.BillAddress.Addr2 != null)
                        {
                            BillAddress2 = creditMemoRet.BillAddress.Addr2.GetValue();
                        }

                        if (creditMemoRet.BillAddress.Addr3 != null)
                        {
                            BillAddress3 = creditMemoRet.BillAddress.Addr3.GetValue();
                        }

                        if (creditMemoRet.BillAddress.Addr4 != null)
                        {
                            BillAddress4 = creditMemoRet.BillAddress.Addr4.GetValue();
                        }

                        if (creditMemoRet.BillAddress.Addr5 != null)
                        {
                            BillAddress5 = creditMemoRet.BillAddress.Addr5.GetValue();
                        }

                        if (creditMemoRet.BillAddress.City != null)
                        {
                            BillAddressCity = creditMemoRet.BillAddress.City.GetValue();
                        }

                        if (creditMemoRet.BillAddress.State != null)
                        {
                            BillAddressState = creditMemoRet.BillAddress.State.GetValue();
                        }

                        if (creditMemoRet.BillAddress.PostalCode != null)
                        {
                            BillAddressPostalCode = creditMemoRet.BillAddress.PostalCode.GetValue();
                        }

                        if (creditMemoRet.BillAddress.Country != null)
                        {
                            BillAddressCountry = creditMemoRet.BillAddress.Country.GetValue();
                        }

                        if (creditMemoRet.BillAddress.Note != null)
                        {
                            BillAddressNote = creditMemoRet.BillAddress.Note.GetValue();
                        }
                    }
                    // Ship address
                    string ShipAddress1 = string.Empty;
                    string ShipAddress2 = string.Empty;
                    string ShipAddress3 = string.Empty;
                    string ShipAddress4 = string.Empty;
                    string ShipAddress5 = string.Empty;
                    string ShipAddressCity = string.Empty;
                    string ShipAddressState = string.Empty;
                    string ShipAddressPostalCode = string.Empty;
                    string ShipAddressCountry = string.Empty;
                    string ShipAddressNote = string.Empty;
                    if (creditMemoRet.ShipAddress != null)
                    {

                        if (creditMemoRet.ShipAddress.Addr1 != null)
                        {
                            ShipAddress1 = creditMemoRet.ShipAddress.Addr1.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.Addr2 != null)
                        {
                            ShipAddress2 = creditMemoRet.ShipAddress.Addr2.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.Addr3 != null)
                        {
                            ShipAddress3 = creditMemoRet.ShipAddress.Addr3.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.Addr4 != null)
                        {
                            ShipAddress4 = creditMemoRet.ShipAddress.Addr4.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.Addr5 != null)
                        {
                            ShipAddress5 = creditMemoRet.ShipAddress.Addr5.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.City != null)
                        {
                            ShipAddressCity = creditMemoRet.ShipAddress.City.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.State != null)
                        {
                            ShipAddressState = creditMemoRet.ShipAddress.State.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.PostalCode != null)
                        {
                            ShipAddressPostalCode = creditMemoRet.ShipAddress.PostalCode.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.Country != null)
                        {
                            ShipAddressCountry = creditMemoRet.ShipAddress.Country.GetValue();
                        }

                        if (creditMemoRet.ShipAddress.Note != null)
                        {
                            ShipAddressNote = creditMemoRet.ShipAddress.Note.GetValue();
                        }
                    }
                    // End ship address

                    string IsPending = string.Empty;
                    if (creditMemoRet.IsPending != null)
                    {
                        IsPending = creditMemoRet.IsPending.GetValue().ToString();
                        if (bool.Parse(IsPending) == true)
                        {
                            //Interaction.MsgBox("stop here");
                            ds.ShowDialog("", "stop here", Haley.Enums.NotificationIcon.Info);
                        }
                    }
                    string PONumber = string.Empty;
                    if (creditMemoRet.PONumber != null)
                    {
                        PONumber = creditMemoRet.PONumber.GetValue();
                    }

                    string TermsRefKey = string.Empty;
                    if (creditMemoRet.TermsRef != null)
                    {
                        TermsRefKey = creditMemoRet.TermsRef.FullName.GetValue();
                    }

                    string DueDate = string.Empty;
                    if (creditMemoRet.DueDate != null)
                    {
                        DueDate = creditMemoRet.DueDate.GetValue().ToString();
                    }

                    string SalesRefKey = string.Empty;
                    if (creditMemoRet.SalesRepRef != null)
                    {
                        SalesRefKey = creditMemoRet.SalesRepRef.ListID.GetValue();
                    }

                    string FOB = string.Empty;
                    if (creditMemoRet.FOB != null)
                    {
                        FOB = creditMemoRet.FOB.GetValue();
                    }

                    string ShipDate = string.Empty;
                    if (creditMemoRet.ShipDate != null)
                    {
                        ShipDate = creditMemoRet.ShipDate.GetValue().ToString();
                    }

                    string ShipMethodRefKey = string.Empty;
                    if (creditMemoRet.ShipMethodRef != null)
                    {
                        ShipMethodRefKey = creditMemoRet.ShipMethodRef.ListID.GetValue();
                    }

                    string ItemSalesTaxRefKey = string.Empty;
                    if (creditMemoRet.ItemSalesTaxRef != null)
                    {
                        ItemSalesTaxRefKey = creditMemoRet.ItemSalesTaxRef.ListID.GetValue();
                    }

                    string Memo = string.Empty;
                    if (creditMemoRet.Memo != null)
                    {
                        Memo = creditMemoRet.Memo.GetValue();
                    }

                    string CustomerMsgRefKey = string.Empty;
                    if (creditMemoRet.CustomerMsgRef != null)
                    {
                        CustomerMsgRefKey = creditMemoRet.CustomerMsgRef.ListID.GetValue();
                    }
                    string IsToBePrinted = string.Empty;
                    if (creditMemoRet.IsToBePrinted != null)
                    {
                        IsToBePrinted = creditMemoRet.IsToBePrinted.GetValue().ToString();
                    }
                    string IsToEmailed = string.Empty;
                    if (creditMemoRet.IsToBeEmailed != null)
                    {
                        IsToEmailed = creditMemoRet.IsToBeEmailed.GetValue().ToString();
                    }
                    string IsTaxIncluded = string.Empty;
                    if (creditMemoRet.IsTaxIncluded != null)
                    {
                        IsTaxIncluded = creditMemoRet.IsTaxIncluded.GetValue().ToString();
                    }
                    string CustomerSalesTaxCodeRefKey = string.Empty;
                    if (creditMemoRet.CustomerSalesTaxCodeRef != null)
                    {
                        CustomerSalesTaxCodeRefKey = creditMemoRet.CustomerSalesTaxCodeRef.ListID.GetValue();
                    }
                    string Other = string.Empty;
                    if (creditMemoRet.Other != null)
                    {
                        Other = creditMemoRet.Other.GetValue();
                    }
                    string Amount = string.Empty;
                    if (creditMemoRet.TotalAmount != null)
                    {
                        Amount = creditMemoRet.TotalAmount.GetValue().ToString();
                    }
                    string AmountPaid = string.Empty;
                    if (creditMemoRet.CreditRemaining != null)
                    {
                        AmountPaid = creditMemoRet.CreditRemaining.GetValue().ToString();
                    }

                    string CustomField1 = string.Empty;
                    string CustomField2 = string.Empty;
                    string CustomField3 = string.Empty;
                    string CustomField4 = string.Empty;
                    string CustomField5 = string.Empty;
                    string QuotationRecNo = string.Empty;
                    string GradeID = string.Empty;
                    string InvoiceType = string.Empty;
                    decimal subTotal = 0m;
                    string SalesTaxTotal = string.Empty;
                    if (creditMemoRet.Subtotal != null)
                    {
                        subTotal = (decimal)creditMemoRet.Subtotal.GetValue();
                    }
                    if (creditMemoRet.EditSequence != null)
                    {
                        SalesTaxPercentage = creditMemoRet.EditSequence.GetValue();
                    }
                    if (creditMemoRet.SalesTaxTotal != null)
                    {
                        SalesTaxTotal = creditMemoRet.SalesTaxTotal.GetValue().ToString();
                    }

                    // Insert data in database code left..........

                    db.tblCredit_Insert(ClearAllControl.gblCompanyID, TxnID, CustomerRefKey, null, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, DateTime.Parse(DueDate), SalesRefKey, FOB, ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, decimal.Parse(Amount), decimal.Parse(AmountPaid), CustomField1, CustomField2, CustomField3, CustomField4, CustomField5, QuotationRecNo, GradeID, InvoiceType, customerName, subTotal, decimal.Parse(SalesTaxTotal), decimal.Parse(SalesTaxPercentage));



                    if (creditMemoRet.ORCreditMemoLineRetList != null)
                    {
                        for (int k = 0, loopTo1 = creditMemoRet.ORCreditMemoLineRetList.Count - 1; k <= loopTo1; k++)
                        {
                            IORCreditMemoLineRet InvoiceLineRetList;
                            InvoiceLineRetList = creditMemoRet.ORCreditMemoLineRetList.GetAt(k);

                            string lineQuantity = "0";
                            if (InvoiceLineRetList.CreditMemoLineRet.Quantity != null)
                            {
                                lineQuantity = InvoiceLineRetList.CreditMemoLineRet.Quantity.GetValue().ToString();
                            }

                            string lineAmount = "0";
                            if (InvoiceLineRetList.CreditMemoLineRet.Amount != null)
                            {
                                lineAmount = InvoiceLineRetList.CreditMemoLineRet.Amount.GetValue().ToString();
                            }


                            string lineDesc = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.Desc != null)
                            {
                                lineDesc = InvoiceLineRetList.CreditMemoLineRet.Desc.GetValue();
                            }
                            string lineItem = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.ItemRef != null)
                            {
                                lineItem = InvoiceLineRetList.CreditMemoLineRet.ItemRef.FullName.GetValue();
                            }
                            if (string.IsNullOrEmpty(lineItem))
                            {
                                goto caltchnullline;
                            }
                            string lineRate = "0";

                            //if (InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate != null)               //Need to check
                            //{
                            //    if (InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate != null)
                            //    {
                            //        lineRate = InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate.GetValue().ToString();
                            //    }
                            //}
                            if (InvoiceLineRetList.CreditMemoLineRet.ORRate != null)
                            {
                                if (InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate != null)
                                {
                                    //Get value of Rate
                                    if (InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate != null)
                                    {
                                        double Rate6403 = (double)InvoiceLineRetList.CreditMemoLineRet.ORRate.Rate.GetValue();
                                    }
                                }
                                if (InvoiceLineRetList.CreditMemoLineRet.ORRate.RatePercent != null)
                                {
                                    //Get value of RatePercent
                                    if (InvoiceLineRetList.CreditMemoLineRet.ORRate.RatePercent != null)
                                    {
                                        double RatePercent6404 = (double)InvoiceLineRetList.CreditMemoLineRet.ORRate.RatePercent.GetValue();
                                    }
                                }
                            }



                            string customefield = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.Other1 != null)
                            {
                                customefield = InvoiceLineRetList.CreditMemoLineRet.Other1.GetValue();
                            }
                            string customeFeild2 = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.Other2 != null)
                            {
                                customeFeild2 = InvoiceLineRetList.CreditMemoLineRet.Other2.GetValue();
                            }
                            string lineUOM = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.UnitOfMeasure != null)
                            {
                                lineUOM = InvoiceLineRetList.CreditMemoLineRet.UnitOfMeasure.GetValue();
                            }

                            string lineUOMOrg = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.OverrideUOMSetRef != null)
                            {
                                lineUOMOrg = InvoiceLineRetList.CreditMemoLineRet.OverrideUOMSetRef.FullName.GetValue();
                            }
                            string lineTxnID = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.TxnLineID != null)
                            {
                                lineTxnID = InvoiceLineRetList.CreditMemoLineRet.TxnLineID.GetValue();
                            }
                            string txaCode = string.Empty;
                            if (InvoiceLineRetList.CreditMemoLineRet.SalesTaxCodeRef != null)
                            {
                                if (InvoiceLineRetList.CreditMemoLineRet.SalesTaxCodeRef.FullName != null)
                                {
                                    txaCode = InvoiceLineRetList.CreditMemoLineRet.SalesTaxCodeRef.FullName.GetValue();
                                }
                            }

                            double taxAmount = 0d;
                            if (InvoiceLineRetList.CreditMemoLineRet.TaxAmount != null)
                            {
                                taxAmount = InvoiceLineRetList.CreditMemoLineRet.TaxAmount.GetValue();
                            }


                            db.tblCreditLine_Insert(TxnID, decimal.Parse(lineAmount), lineDesc, lineItem, lineRate, customefield, customeFeild2, lineUOM, decimal.Parse(lineQuantity), lineUOMOrg, lineTxnID, txaCode, (decimal?)taxAmount);
                        caltchnullline:
                            ;
                        }
                    }
                }

                return;
            }
            catch
            {
                ds.ShowDialog("", "Error to Fill CreditNote", Haley.Enums.NotificationIcon.Error);
                bDone = true;
                bError = true;
            }
        }
        #endregion

        #region Methods
        private void LoadCNCustomer(int cID)
        {
            try
            {
                cmbxccustname.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.execquery("SELECT distinct customerName from tblCredit where companyID=@CompanyID");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxccustname.Items.Add(r["customerName"]);
                    cmbxccustname.SelectedIndex = -1;
                }

                //var col = new System.Windows.Forms.AutoCompleteStringCollection();
                //for (int i = 0, loopTo = sql.sqlds.Tables[0].Rows.Count - 1; i <= loopTo; i++)
                //    col.Add(sql.sqlds.Tables[0].Rows[i]["customerName"].ToString());
                //cmbxccustname.SelectedItem = System.Windows.Forms.AutoCompleteSource.CustomSource;
                //cmbxccustname.ItemsSource = col;
                //cmbxccustname.SelectedItem = System.Windows.Forms.AutoCompleteMode.Suggest;
            }
            catch
            {

            }
        }

        private void LoadCNNumber(string cust, int cID)
        {
            try
            {
                cmbxcnotenum.SelectedIndex = -1;
                cmbxcnotenum.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.addparam("@CustomerName", cust);
                sql.execquery("SELECT distinct RefNumber,TxnId  from tblCredit where companyID=@CompanyID and customerName=@CustomerName order by RefNumber");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxcnotenum.Items.Add(r["RefNumber"]);
                    cmbxcnotenum.SelectedIndex = -1;
                }
            }
            catch
            {

            }
        }

        private void PopulateTempleteCombobox()
        {
            cmbxtemp.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Credit Note' and Status='True'");
            if (sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    cmbxtemp.Items.Add(r["TemplateName"]);
                cmbxtemp.SelectedIndex = 0;
            }
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
        #endregion

        #region Events

        private void cmbxccustname_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCNNumber(cmbxccustname.Text, ClearAllControl.gblCompanyID);
        }

        private void cmbxtemp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempPath(cmbxtemp.Text);
        }

        private void btnCreditSyn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                GetCreditTransaction(ref bError, dpFrmDate.DisplayDate, dpToDate.DisplayDate);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Credit Memo Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Credit Memo Synced successfully", Haley.Enums.NotificationIcon.Success);
                }
                LoadCNCustomer(ClearAllControl.gblCompanyID);
                disconnectFromQB();
            }
            catch
            {

            }
        }

        private void btnCreditSynAl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                //Sessions.Sessions.OpenConnectionBeginSession();
                GetCreditTransactions(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Credit Memo Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Credit Memo Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }
                disconnectFromQB();
                LoadCNCustomer(ClearAllControl.gblCompanyID);
            }
            catch
            {

            }
        }

        private void btnCreditPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var _ReportDocument = new ReportDocument();
                var crtableLogoninfos = new TableLogOnInfos();
                var crtableLogoninfo = new TableLogOnInfo();
                var crConnectionInfo = new ConnectionInfo();
                ParameterFieldDefinitions crParameterFieldDefinitions;
                ParameterFieldDefinition crParameterFieldDefinition;
                var crParameterValues = new ParameterValues();
                var crParameterDiscreteValue = new ParameterDiscreteValue();
                Tables CrTables;

                _ReportDocument.Load(path + cmbxtemp.Text + ".rpt");
                string startuppath = System.AppDomain.CurrentDomain.BaseDirectory + path + cmbxtemp.Text + ".rpt";

                //ref var withBlock = ref _ReportDocument;
                //string startuppatah = Application.StartupPath;
                //withBlock.Load(path + tempname.Text + ".rpt");

                _ReportDocument.ReportOptions.EnableSaveDataWithReport = false;
                crConnectionInfo.ServerName = ClearAllControl.gblSQLServerName;
                crConnectionInfo.DatabaseName = ClearAllControl.gblDatabaseName;
                crConnectionInfo.UserID = ClearAllControl.gblSQLServerUserName;
                crConnectionInfo.Password = ClearAllControl.gblSQLServerPassword;
                crConnectionInfo.AllowCustomConnection = false;
                crConnectionInfo.IntegratedSecurity = false;

                CrTables = _ReportDocument.Database.Tables;

                // _ReportDocument.SetParameterValue("@CompanyID", gblCompanyID)
                foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in CrTables)
                {
                    crtableLogoninfo = CrTable.LogOnInfo;
                    crtableLogoninfo.ConnectionInfo = crConnectionInfo;
                    crtableLogoninfo.ReportName = _ReportDocument.Name;
                    crtableLogoninfo.TableName = CrTable.Name;
                    CrTable.ApplyLogOnInfo(crtableLogoninfo);
                }

                crParameterDiscreteValue.Value = cmbxcnotenum.Text;
                crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
                crParameterFieldDefinition = crParameterFieldDefinitions["@refNumber"];
                crParameterValues = crParameterFieldDefinition.CurrentValues;
                crParameterValues.Clear();
                crParameterValues.Add(crParameterDiscreteValue);
                crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);


                var rpt = new ReportView();
                rpt.ShowReportView(ref _ReportDocument);
            }

            catch (Exception ex)
            {
                ds.ShowDialog("TNC-Sync", ex.Message, Haley.Enums.NotificationIcon.Error);
            }

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
