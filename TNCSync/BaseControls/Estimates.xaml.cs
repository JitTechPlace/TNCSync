using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Haley.Abstractions;
using Haley.MVVM;
using Interop.QBFC16;
using System;
using System.Collections.Generic;
using System.Data;
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
using TNCSync.Sessions;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for Estimates.xaml
    /// </summary>
    public partial class Estimates : UserControl
    {
        private bool bError;
        private string path = null;
        private SQLControls sql = new SQLControls();
        private bool booSessionBegun;

        public Estimates()
        {
            InitializeComponent();

            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
            PopulateTempleteCombobox();
            LoadEstimateCustomer(ClearAllControl.gblCompanyID);

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

        #region Estimates Methods
        public void GetEstimateTransaction(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                IEstimateQuery estimateQuery = msgsetRequest.AppendEstimateQueryRq();
                estimateQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                estimateQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
                estimateQuery.IncludeLineItems.SetValue(true);
                estimateQuery.OwnerIDList.Add("0");
                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillEstInDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void GetEstimateTransactions(ref bool bError)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                IEstimateQuery estimateQuery = msgsetRequest.AppendEstimateQueryRq();
                estimateQuery.IncludeLineItems.SetValue(true);
                estimateQuery.OwnerIDList.Add("0");
                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillEstInDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void FillEstInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
        {
            try
            {
                string RefNumber = string.Empty;
                //Check to make sure we have objects to access first and then there are response in the list
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
                        //ds.ShowDialog("","Get check query unexpexcted Error - " + response.StatusMessage);
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

                // make sure we are processing the EstimationQueryRs and 
                // the EstimationRetList responses in this response list
                IEstimateRetList EstimationRetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtEstimateQueryRs & responseDetailType == ENObjectType.otEstimateRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    EstimationRetList = (IEstimateRetList)response.Detail;
                }
                else
                {
                    // bill, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }
                // Parse the query response and add the Estimation to the Estimation list box
                short count;
                short index;
                IEstimateRet EstimationRet;
                count = (short)EstimationRetList.Count;
                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + EstimationRetList.Count);
                // we are done with the EstimationQuery if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }
                db.tblEstimate_Delete(ClearAllControl.gblCompanyID);
                db.tblEstimateLine_Delete();
                var linkTxnDate = default(DateTime);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    // skip this customer if this is a repeat from the last query
                    EstimationRet = EstimationRetList.GetAt(index);
                    if (EstimationRet is null | EstimationRet.CustomerRef.ListID is null | EstimationRet.TxnID is null)
                    {
                        bDone = true;
                        bError = true;
                        return;
                    }
                    if (index == 5)
                    {
                        index = index;
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


                    string TxnID = EstimationRet.TxnID.GetValue();
                    string CustomerRefKey = string.Empty;
                    if (EstimationRet.CustomerRef != null)
                    {
                        CustomerRefKey = EstimationRet.CustomerRef.ListID.GetValue();
                    }
                    string customerName = string.Empty;
                    if (EstimationRet.CustomerRef != null)
                    {
                        customerName = EstimationRet.CustomerRef.FullName.GetValue();
                    }


                    string currencyRef = string.Empty;
                    // If Not (InvoiceRet.CurrencyRef Is Nothing) Then
                    // currencyRef = InvoiceRet.CurrencyRef.FullName.GetValue
                    // End If

                    string ExchangeRAte = string.Empty;
                    // If Not (InvoiceRet.ExchangeRate Is Nothing) Then
                    // ExchangeRAte = InvoiceRet.ExchangeRate.GetValue
                    // End If



                    string ClassRefKey = string.Empty;
                    if (EstimationRet.ClassRef != null)
                    {
                        ClassRefKey = EstimationRet.ClassRef.FullName.GetValue();
                    }

                    string ARAccountRefKey = string.Empty;
                    // If Not (InvoiceRet.ARAccountRef Is Nothing) Then
                    // ARAccountRefKey = InvoiceRet.ARAccountRef.FullName.GetValue

                    // End If
                    string TemplateRefKey = string.Empty;
                    if (EstimationRet.TemplateRef != null)
                    {
                        TemplateRefKey = EstimationRet.TemplateRef.ListID.GetValue();
                    }
                    var TxnDate = EstimationRet.TxnDate.GetValue();



                    if (EstimationRet.RefNumber != null)
                    {
                        RefNumber = EstimationRet.RefNumber.GetValue();
                    }
                    if (RefNumber == "12" | RefNumber == "09613")
                    {
                        int m = 0;
                    }

                    // Bill Address


                    if (EstimationRet.BillAddress != null)
                    {


                        if (EstimationRet.BillAddress.Addr1 != null)
                        {
                            BillAddress1 = EstimationRet.BillAddress.Addr1.GetValue();
                        }


                        if (EstimationRet.BillAddress.Addr2 != null)
                        {
                            BillAddress2 = EstimationRet.BillAddress.Addr2.GetValue();
                        }

                        if (EstimationRet.BillAddress.Addr3 != null)
                        {
                            BillAddress3 = EstimationRet.BillAddress.Addr3.GetValue();
                        }

                        if (EstimationRet.BillAddress.Addr4 != null)
                        {
                            BillAddress4 = EstimationRet.BillAddress.Addr4.GetValue();
                        }

                        if (EstimationRet.BillAddress.Addr5 != null)
                        {
                            BillAddress5 = EstimationRet.BillAddress.Addr5.GetValue();
                        }

                        if (EstimationRet.BillAddress.City != null)
                        {
                            BillAddressCity = EstimationRet.BillAddress.City.GetValue();
                        }

                        if (EstimationRet.BillAddress.State != null)
                        {
                            BillAddressState = EstimationRet.BillAddress.State.GetValue();
                        }

                        if (EstimationRet.BillAddress.PostalCode != null)
                        {
                            BillAddressPostalCode = EstimationRet.BillAddress.PostalCode.GetValue();
                        }

                        if (EstimationRet.BillAddress.Country != null)
                        {
                            BillAddressCountry = EstimationRet.BillAddress.Country.GetValue();
                        }

                        if (EstimationRet.BillAddress.Note != null)
                        {
                            BillAddressNote = EstimationRet.BillAddress.Note.GetValue();
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
                    if (EstimationRet.ShipAddress != null)
                    {

                        if (EstimationRet.ShipAddress.Addr1 != null)
                        {
                            ShipAddress1 = EstimationRet.ShipAddress.Addr1.GetValue();
                        }

                        if (EstimationRet.ShipAddress.Addr2 != null)
                        {
                            ShipAddress2 = EstimationRet.ShipAddress.Addr2.GetValue();
                        }

                        if (EstimationRet.ShipAddress.Addr3 != null)
                        {
                            ShipAddress3 = EstimationRet.ShipAddress.Addr3.GetValue();
                        }

                        if (EstimationRet.ShipAddress.Addr4 != null)
                        {
                            ShipAddress4 = EstimationRet.ShipAddress.Addr4.GetValue();
                        }

                        if (EstimationRet.ShipAddress.Addr5 != null)
                        {
                            ShipAddress5 = EstimationRet.ShipAddress.Addr5.GetValue();
                        }

                        if (EstimationRet.ShipAddress.City != null)
                        {
                            ShipAddressCity = EstimationRet.ShipAddress.City.GetValue();
                        }

                        if (EstimationRet.ShipAddress.State != null)
                        {
                            ShipAddressState = EstimationRet.ShipAddress.State.GetValue();
                        }

                        if (EstimationRet.ShipAddress.PostalCode != null)
                        {
                            ShipAddressPostalCode = EstimationRet.ShipAddress.PostalCode.GetValue();
                        }

                        if (EstimationRet.ShipAddress.Country != null)
                        {
                            ShipAddressCountry = EstimationRet.ShipAddress.Country.GetValue();
                        }

                        if (EstimationRet.ShipAddress.Note != null)
                        {
                            ShipAddressNote = EstimationRet.ShipAddress.Note.GetValue();
                        }
                    }
                    // End ship address

                    string IsPending = string.Empty;
                    // If Not (InvoiceRet.IsPending Is Nothing) Then
                    // IsPending = InvoiceRet.IsPending.GetValue
                    // If IsPending = True Then
                    // MsgBox("stop here")
                    // End If
                    // End If
                    string PONumber = string.Empty;
                    if (EstimationRet.PONumber != null)
                    {
                        PONumber = EstimationRet.PONumber.GetValue();
                    }

                    string TermsRefKey = string.Empty;
                    if (EstimationRet.TermsRef != null)
                    {
                        TermsRefKey = EstimationRet.TermsRef.FullName.GetValue();
                    }

                    // Dim DueDate As String = String.Empty
                    // If Not (EstimationRet.DueDate Is Nothing) Then
                    // DueDate = EstimationRet.DueDate.GetValue
                    // End If

                    string SalesRefKey = string.Empty;
                    if (EstimationRet.SalesRepRef != null)
                    {
                        SalesRefKey = EstimationRet.SalesRepRef.FullName.GetValue();
                    }

                    string FOB = string.Empty;
                    if (EstimationRet.FOB != null)
                    {
                        FOB = EstimationRet.FOB.GetValue();
                    }

                    // Dim ShipDate As String = String.Empty
                    // If Not (EstimationRet.ShipDate Is Nothing) Then
                    // ShipDate = EstimationRet.ShipDate.GetValue
                    // End If

                    string ShipMethodRefKey = string.Empty;
                    // If Not (EstimationRet.ShipMethodRef Is Nothing) Then
                    // ShipMethodRefKey = EstimationRet.ShipMethodRef.ListID.GetValue
                    // End If

                    string ItemSalesTaxRefKey = string.Empty;
                    if (EstimationRet.ItemSalesTaxRef != null)
                    {
                        ItemSalesTaxRefKey = EstimationRet.ItemSalesTaxRef.ListID.GetValue();
                    }

                    string Memo = string.Empty;
                    if (EstimationRet.Memo != null)
                    {
                        Memo = EstimationRet.Memo.GetValue();
                    }

                    string CustomerMsgRefKey = string.Empty;
                    if (EstimationRet.CustomerMsgRef != null)
                    {
                        CustomerMsgRefKey = EstimationRet.CustomerMsgRef.ListID.GetValue();
                    }
                    string IsToBePrinted = string.Empty;
                    // If Not (EstimationRet.IsToBePrinted Is Nothing) Then
                    // IsToBePrinted = EstimationRet.IsToBePrinted.GetValue
                    // End If
                    string IsToEmailed = string.Empty;
                    if (EstimationRet.IsToBeEmailed != null)
                    {
                        IsToEmailed = EstimationRet.IsToBeEmailed.GetValue().ToString();
                    }
                    string IsTaxIncluded = string.Empty;
                    if (EstimationRet.IsTaxIncluded != null)
                    {
                        IsTaxIncluded = EstimationRet.IsTaxIncluded.GetValue().ToString();
                    }
                    string CustomerSalesTaxCodeRefKey = string.Empty;
                    if (EstimationRet.CustomerSalesTaxCodeRef != null)
                    {
                        CustomerSalesTaxCodeRefKey = EstimationRet.CustomerSalesTaxCodeRef.ListID.GetValue();
                    }
                    string Other = string.Empty;
                    if (EstimationRet.Other != null)
                    {
                        Other = EstimationRet.Other.GetValue();
                    }
                    string Amount = "0";
                    if (EstimationRet.TotalAmount != null)
                    {
                        Amount = EstimationRet.TotalAmount.GetValue().ToString();
                    }
                    string AmountPaid = "0";
                    // If Not (InvoiceRet.AppliedAmount Is Nothing) Then
                    // AmountPaid = InvoiceRet.AppliedAmount.GetValue
                    // End If
                    string CustomField1 = string.Empty;
                    string CustomField2 = string.Empty;
                    string CustomField3 = string.Empty;
                    string CustomField4 = string.Empty;
                    string CustomField5 = string.Empty;
                    string QuotationRecNo = string.Empty;
                    string GradeID = string.Empty;
                    string InvoiceType = string.Empty;
                    string subTotal = null;
                    string SalesTaxTotal = null;
                    string SalesTaxPercentage = null;
                    if (EstimationRet.Subtotal != null)
                    {
                        subTotal = EstimationRet.Subtotal.GetValue().ToString();
                    }



                    if (EstimationRet.DataExtRetList != null)
                    {
                        IDataExtRet extInvLst;
                        string dataExtaname = string.Empty;
                        string dataExtValue = string.Empty;
                        for (int g = 0, loopTo1 = EstimationRet.DataExtRetList.Count - 1; g <= loopTo1; g++)
                        {
                            extInvLst = EstimationRet.DataExtRetList.GetAt(g);
                            dataExtaname = extInvLst.DataExtName.GetValue();
                            dataExtValue = extInvLst.DataExtValue.GetValue();
                            if (dataExtaname == "TRN")
                            {
                                CustomField1 = dataExtValue;
                            }
                            else if (dataExtaname == "CONTACT")
                            {
                                CustomField2 = dataExtValue;
                            }
                            else if (dataExtaname == "Vehicle")
                            {
                                CustomField3 = dataExtValue;
                            }
                            else if (dataExtaname == "Customer TRN")
                            {
                                CustomField4 = dataExtValue;
                            }
                        }

                    }

                    if (EstimationRet.SalesTaxTotal != null)
                    {
                        SalesTaxTotal = EstimationRet.SalesTaxTotal.GetValue().ToString();
                    }
                    if (EstimationRet.SalesTaxPercentage != null)
                    {
                        SalesTaxPercentage = EstimationRet.SalesTaxPercentage.GetValue().ToString();
                    }


                    // Insert Code For Estimation
                    db.tblEstimate_Insert(ClearAllControl.gblCompanyID, TxnID, CustomerRefKey, (string)null, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, default(DateTime?), SalesRefKey, FOB, default(DateTime?), ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, decimal.Parse(Amount), decimal.Parse(AmountPaid), CustomField1, CustomField2, CustomField3, currencyRef, CustomField5, QuotationRecNo, ExchangeRAte, InvoiceType, customerName, decimal.Parse(subTotal), decimal.Parse(SalesTaxTotal), decimal.Parse(SalesTaxPercentage));
                    if (EstimationRet.LinkedTxnList != null)
                    {
                        for (int p = 0, loopTo2 = EstimationRet.LinkedTxnList.Count - 1; p <= loopTo2; p++)
                        {
                            ILinkedTxn invoiceLinkTxn;
                            invoiceLinkTxn = EstimationRet.LinkedTxnList.GetAt(p);
                            string LinkRefNum = string.Empty;
                            if (invoiceLinkTxn.RefNumber != null)
                            {
                                LinkRefNum = invoiceLinkTxn.RefNumber.GetValue();
                            }
                            string linktXnAmount = string.Empty;
                            if (invoiceLinkTxn.Amount != null)
                            {
                                linktXnAmount = invoiceLinkTxn.Amount.GetValue().ToString();
                            }
                            if (invoiceLinkTxn.TxnDate != null)
                            {
                                linkTxnDate = invoiceLinkTxn.TxnDate.GetValue();
                            }
                            db.tblInvoiceApplTxn_Insert(RefNumber, LinkRefNum, linkTxnDate, linktXnAmount, TxnDate);
                        }
                    }
                    if (EstimationRet.OREstimateLineRetList != null)
                    {
                        for (int k = 0, loopTo3 = EstimationRet.OREstimateLineRetList.Count - 1; k <= loopTo3; k++)
                        {
                            IOREstimateLineRet InvoiceLineRetList;
                            InvoiceLineRetList = EstimationRet.OREstimateLineRetList.GetAt(k);

                            string lineQuantity = "0";
                            // If Not InvoiceLineRetList.EstimateLineRet.Quantity Is Nothing Then
                            // lineQuantity = InvoiceLineRetList.EstimateLineRet.Quantity.GetValue
                            // End If

                            string lineAmount = "0";
                            if (InvoiceLineRetList.EstimateLineRet.Amount != null)
                            {
                                lineAmount = InvoiceLineRetList.EstimateLineRet.Amount.GetValue().ToString();
                            }

                            string lineDesc = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.Desc != null)
                            {
                                lineDesc = InvoiceLineRetList.EstimateLineRet.Desc.GetValue();
                            }
                            string lineItem = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.ItemRef != null)
                            {
                                lineItem = InvoiceLineRetList.EstimateLineRet.ItemRef.FullName.GetValue();
                            }
                            if (string.IsNullOrEmpty(lineItem))
                            {
                                goto caltchnullline;
                            }
                            string lineRate = "0";

                            string customefield = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.Other1 != null)
                            {
                                customefield = InvoiceLineRetList.EstimateLineRet.Other1.GetValue();
                            }
                            string customeFeild2 = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.Other2 != null)
                            {
                                customeFeild2 = InvoiceLineRetList.EstimateLineRet.Other2.GetValue();
                            }
                            string lineUOM = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.UnitOfMeasure != null)
                            {
                                lineUOM = InvoiceLineRetList.EstimateLineRet.UnitOfMeasure.GetValue();
                            }

                            string lineUOMOrg = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.OverrideUOMSetRef != null)
                            {
                                lineUOMOrg = InvoiceLineRetList.EstimateLineRet.OverrideUOMSetRef.FullName.GetValue();
                            }
                            string lineTxnID = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.TxnLineID != null)
                            {
                                lineTxnID = InvoiceLineRetList.EstimateLineRet.TxnLineID.GetValue();
                            }
                            string txaCode = string.Empty;
                            if (InvoiceLineRetList.EstimateLineRet.SalesTaxCodeRef != null)
                            {
                                if (InvoiceLineRetList.EstimateLineRet.SalesTaxCodeRef.FullName != null)
                                {
                                    txaCode = InvoiceLineRetList.EstimateLineRet.SalesTaxCodeRef.FullName.GetValue();
                                }
                            }

                            double taxAmount = 0d;
                            if (InvoiceLineRetList.EstimateLineRet.TaxAmount != null)
                            {
                                taxAmount = InvoiceLineRetList.EstimateLineRet.TaxAmount.GetValue();
                            }
                            db.tblEstimateLine_Insert(TxnID, decimal.Parse(lineAmount), lineDesc, lineItem, lineRate, customefield, customeFeild2, lineUOM, lineQuantity, lineUOMOrg, lineTxnID, txaCode, (decimal?)taxAmount);
                        caltchnullline:
                            ;
                        }
                    }
                }
                return;
            Errs:
                ;
            }
            catch
            {

            }
        }
        #endregion

        #region Methods
        private void LoadEstimateCustomer(int cID)
        {
            try
            {
                cmbxEstimateCust.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.execquery("SELECT distinct customerName from tblEstimate where companyID=@CompanyID");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxEstimateCust.Items.Add(r["customerName"]);
                    cmbxEstimateCust.SelectedIndex = -1;
                }
            }
            catch
            {

            }
        }

        private void LoadEstimateNumber(string cust, int cID)
        {
            try
            {
                cmbxEstimateNum.SelectedIndex = -1;
                cmbxEstimateNum.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.addparam("@CustomerName", cust);
                sql.execquery("SELECT distinct RefNumber,TxnId  from tblEstimate where companyID=@CompanyID and ((@CustomerName is null) or (customerName=@CustomerName )) order by RefNumber");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxEstimateNum.Items.Add(r["RefNumber"]);
                    cmbxEstimateNum.SelectedIndex = -1;
                }
            }

            catch (Exception ex)
            {

            }
        }

        private void PopulateTempleteCombobox()
        {
            cmbxTmptEstimate.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Quotation Sales' and Status='True'");
            if (sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    cmbxTmptEstimate.Items.Add(r["TemplateName"]);
                cmbxTmptEstimate.SelectedIndex = 0;
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

        private void cmbxEstimateCust_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadEstimateNumber(cmbxEstimateCust.Text, ClearAllControl.gblCompanyID);
        }

        private void cmbxTmptEstimate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempPath(cmbxTmptEstimate.Text);
        }

        private void btnsyncQuotation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                GetEstimateTransaction(ref bError, dpFrmDate.DisplayDate, dpToDate.DisplayDate);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Estimation Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Estimation Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }
                disconnectFromQB();
                LoadEstimateCustomer(ClearAllControl.gblCompanyID);
                
            }
            catch
            {

            }
        }

        private void btnsyncalQuotation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                //Sessions.Sessions.OpenConnectionBeginSession();
                GetEstimateTransactions(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Estimation Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Estimation Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }
                LoadEstimateCustomer(ClearAllControl.gblCompanyID);
                disconnectFromQB();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnQuotaionPrint_Click(object sender, RoutedEventArgs e)
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

                //ref var withBlock = ref _ReportDocument;
                //string startuppath = Application.
                _ReportDocument.Load(path + cmbxTmptEstimate.Text + ".rpt");
                string startuppath = System.AppDomain.CurrentDomain.BaseDirectory + path + cmbxTmptEstimate.Text + ".rpt";

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

                // 'crParameterDiscreteValue.Value = txtInvoiceNo.Text
                crParameterDiscreteValue.Value = cmbxEstimateNum.Text;
                crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
                crParameterFieldDefinition = crParameterFieldDefinitions["@refNumber"];
                crParameterValues = crParameterFieldDefinition.CurrentValues;
                crParameterValues.Clear();
                crParameterValues.Add(crParameterDiscreteValue);
                crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);


                var rpt = new ReportView();
                rpt.ShowReportView(ref _ReportDocument);                //rpt.
                // If _ReportDocument.HasRecords Then  //Need to check
                // var frm = new ReportFormLinker();
                //frm.MdiParent = ParentForm;
                //frm.WindowState = FormWindowState.Maximized;
                //frm.ShowReportinViewer(ref _ReportDocument);
                //frm.Refresh();
                //frm.Show();
            }
            catch (Exception ex)
            {
                ds.ShowDialog("TNC-Sync", ex.Message, Haley.Enums.NotificationIcon.Error);
            }
        }
        #endregion
    }
}
