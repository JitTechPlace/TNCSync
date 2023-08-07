using System;
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
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Data;
using TNCSync.Sessions;
//using Interop.QBFC15;
using Interop.QBFC16;
using TNCSync.Class.DataBaseClass;
using Haley.Abstractions;
using Haley.MVVM;
using TNCSync.Class;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for SalesOrder.xaml
    /// </summary>
    public partial class SalesOrder : UserControl
    {
        private bool bError;
        private string path = null;
        private SQLControls sql = new SQLControls();
        private bool booSessionBegun;
        //private QBSessionManager sessionManager;
        //private IMsgSetRequest msgsetRequest;
        //private static DBTNCSDataContext db = new DBTNCSDataContext();
        //public IDialogService ds;
        ////SessionManager sessionManager;
        ////private short maxVersion;
        //private string payEntityRef = null;
        //private DateTime? txnDate = default;
        //private DateTime? timeCreated = default;
        //private string memo = null;
        //private decimal amount = 0m;
        //private string accountRef = null;
        //private string txnID = null;
        //private string addressBlock = null;

        public SalesOrder()
        {
            InitializeComponent();


            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
            PopulateTempleteCombobox();
            LoadSOCustomer(ClearAllControl.gblCompanyID);
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
                    MessageBox.Show(e.Message);
                }
            }
        }
        #endregion

        #region SOMethods
        public void GetSOTransaction(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                //Make sure we do not have any old requests still define
                //IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                IPurchaseOrderQuery purchaseOrderQuery;
                
                ISalesOrderQuery receiptQuery = msgsetRequest.AppendSalesOrderQueryRq();
                receiptQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                receiptQuery.ORTxnNoAccountQuery.TxnFilterNoAccount.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
                receiptQuery.IncludeLineItems.SetValue(true);
                receiptQuery.OwnerIDList.Add("0");
                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillSOInDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void GetSOTransactionNoDate(ref bool bErroe)
        {
            try
            {
                //Make sure we do not have any old requests still define
                //IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                // IMsgSetRequest msgsetRequest = Sessions.Sessions.GetLatestMsgSetRequestRet();
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                ISalesOrderQuery receiptQuery = msgsetRequest.AppendSalesOrderQueryRq();
                receiptQuery.IncludeLineItems.SetValue(true);
                receiptQuery.OwnerIDList.Add("0");
                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillSOInDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void FillSOInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
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

                //Make sure We are Processing the SOQueryRs and the SalesorderRetList responses in this list
                ISalesOrderRetList salesOrderRetList;  //salesOrderRetList
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtSalesOrderQueryRs & responseDetailType == ENObjectType.otSalesOrderRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    salesOrderRetList = (ISalesOrderRetList)response.Detail;
                }
                else
                {
                    // bill, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                // Parse the query response and add the Salesorder to the Salesorder list
                short count;
                short index;
                ISalesOrderRet salesOrderRet;
                count = (short)salesOrderRetList.Count;
                short Max_Returned;
                Max_Returned = (short)(1 + salesOrderRetList.Count);
                //we are done with the SOQueries if we have not received the MaxReturned
                if(count < Max_Returned)
                {
                    bDone = true;
                }
                db.tblSO_Delete(ClearAllControl.gblCompanyID);
                db.tblSOLine_Delete();
                var linkTxnDate = default(DateTime);
                var loopTo = (short)(count - 1);
                for( index = 0; index <= loopTo; index++)
                {
                    //Skip the Salesorder if it is repeat from the last query
                    salesOrderRet = salesOrderRetList.GetAt(index);
                    if(salesOrderRet is null | salesOrderRet.CustomerRef.ListID is null | salesOrderRet.TxnID is null)
                    {
                        bDone = true;
                        bError = true;
                        return;
                    }
                    if(index == 5)
                    {
                        index = index;
                    }

                    //Declare Variable to retrive Data
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


                    string TxnID = salesOrderRet.TxnID.GetValue();
                    string CustomerRefKey = string.Empty;
                    if (salesOrderRet.CustomerRef != null)
                    {
                        CustomerRefKey = salesOrderRet.CustomerRef.ListID.GetValue();
                    }
                    string customerName = string.Empty;
                    if (salesOrderRet.CustomerRef != null)
                    {
                        customerName = salesOrderRet.CustomerRef.FullName.GetValue();
                    }


                    string currencyRef = string.Empty;
                    // If Not (salesOrderRet.CurrencyRef Is Nothing) Then
                    // currencyRef = salesOrderRet.CurrencyRef.FullName.GetValue
                    // End If

                    string ExchangeRAte = string.Empty;
                    // If Not (salesOrderRet.ExchangeRate Is Nothing) Then
                    // ExchangeRAte = salesOrderRet.ExchangeRate.GetValue
                    // End If



                    string ClassRefKey = string.Empty;
                    if (salesOrderRet.ClassRef != null)
                    {
                        ClassRefKey = salesOrderRet.ClassRef.FullName.GetValue();
                    }

                    string ARAccountRefKey = string.Empty;
                    // If Not (salesOrderRet.ARAccountRef Is Nothing) Then
                    // ARAccountRefKey = salesOrderRet.ARAccountRef.FullName.GetValue

                    // End If
                    string TemplateRefKey = string.Empty;
                    if (salesOrderRet.TemplateRef != null)
                    {
                        TemplateRefKey = salesOrderRet.TemplateRef.ListID.GetValue();
                    }
                    var TxnDate = salesOrderRet.TxnDate.GetValue();



                    if (salesOrderRet.RefNumber != null)
                    {
                        RefNumber = salesOrderRet.RefNumber.GetValue();
                    }
                    if (RefNumber == "12" | RefNumber == "09613")
                    {
                        int m = 0;
                    }

                    // Bill Address


                    if (salesOrderRet.BillAddress !=null)
                    {


                        if (salesOrderRet.BillAddress.Addr1 != null)
                        {
                            BillAddress1 = salesOrderRet.BillAddress.Addr1.GetValue();
                        }


                        if (salesOrderRet.BillAddress.Addr2 != null)
                        {
                            BillAddress2 = salesOrderRet.BillAddress.Addr2.GetValue();
                        }

                        if (salesOrderRet.BillAddress.Addr3 != null)
                        {
                            BillAddress3 = salesOrderRet.BillAddress.Addr3.GetValue();
                        }

                        if (salesOrderRet.BillAddress.Addr4 != null)
                        {
                            BillAddress4 = salesOrderRet.BillAddress.Addr4.GetValue();
                        }

                        if (salesOrderRet.BillAddress.Addr5 != null)
                        {
                            BillAddress5 = salesOrderRet.BillAddress.Addr5.GetValue();
                        }

                        if (salesOrderRet.BillAddress.City != null)
                        {
                            BillAddressCity = salesOrderRet.BillAddress.City.GetValue();
                        }

                        if (salesOrderRet.BillAddress.State != null)
                        {
                            BillAddressState = salesOrderRet.BillAddress.State.GetValue();
                        }

                        if (salesOrderRet.BillAddress.PostalCode != null)
                        {
                            BillAddressPostalCode = salesOrderRet.BillAddress.PostalCode.GetValue();
                        }

                        if (salesOrderRet.BillAddress.Country != null)
                        {
                            BillAddressCountry = salesOrderRet.BillAddress.Country.GetValue();
                        }

                        if (salesOrderRet.BillAddress.Note != null)
                        {
                            BillAddressNote = salesOrderRet.BillAddress.Note.GetValue();
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
                    if (salesOrderRet.ShipAddress != null)
                    {

                        if (salesOrderRet.ShipAddress.Addr1 != null)
                        {
                            ShipAddress1 = salesOrderRet.ShipAddress.Addr1.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.Addr2 != null)
                        {
                            ShipAddress2 = salesOrderRet.ShipAddress.Addr2.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.Addr3 != null)
                        {
                            ShipAddress3 = salesOrderRet.ShipAddress.Addr3.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.Addr4 != null)
                        {
                            ShipAddress4 = salesOrderRet.ShipAddress.Addr4.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.Addr5 != null)
                        {
                            ShipAddress5 = salesOrderRet.ShipAddress.Addr5.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.City != null)
                        {
                            ShipAddressCity = salesOrderRet.ShipAddress.City.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.State != null)
                        {
                            ShipAddressState = salesOrderRet.ShipAddress.State.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.PostalCode != null)
                        {
                            ShipAddressPostalCode = salesOrderRet.ShipAddress.PostalCode.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.Country != null)
                        {
                            ShipAddressCountry = salesOrderRet.ShipAddress.Country.GetValue();
                        }

                        if (salesOrderRet.ShipAddress.Note != null)
                        {
                            ShipAddressNote = salesOrderRet.ShipAddress.Note.GetValue();
                        }
                    }
                    // End ship address

                    string IsPending = string.Empty;
                    // If Not (salesOrderRet.IsPending Is Nothing) Then
                    // IsPending = salesOrderRet.IsPending.GetValue
                    // If IsPending = True Then
                    // MsgBox("stop here")
                    // End If
                    // End If
                    string PONumber = string.Empty;
                    if (salesOrderRet.PONumber != null)
                    {
                        PONumber = salesOrderRet.PONumber.GetValue();
                    }

                    string TermsRefKey = string.Empty;
                    if (salesOrderRet.TermsRef != null)
                    {
                        TermsRefKey = salesOrderRet.TermsRef.FullName.GetValue();
                    }

                    string DueDate = string.Empty;
                    if (salesOrderRet.DueDate != null)
                    {
                        DueDate = salesOrderRet.DueDate.GetValue().ToString();
                    }

                    string SalesRefKey = string.Empty;
                    if (salesOrderRet.SalesRepRef != null)
                    {
                        SalesRefKey = salesOrderRet.SalesRepRef.FullName.GetValue();
                    }

                    string FOB = string.Empty;
                    if (salesOrderRet.FOB != null)
                    {
                        FOB = salesOrderRet.FOB.GetValue();
                    }

                    string ShipDate = string.Empty;
                    if (salesOrderRet.ShipDate != null)
                    {
                        ShipDate = salesOrderRet.ShipDate.GetValue().ToString();
                    }

                    string ShipMethodRefKey = string.Empty;
                    if (salesOrderRet.ShipMethodRef != null)
                    {
                        ShipMethodRefKey = salesOrderRet.ShipMethodRef.ListID.GetValue();
                    }

                    string ItemSalesTaxRefKey = string.Empty;
                    if (salesOrderRet.ItemSalesTaxRef != null)
                    {
                        ItemSalesTaxRefKey = salesOrderRet.ItemSalesTaxRef.ListID.GetValue();
                    }

                    string Memo = string.Empty;
                    if (salesOrderRet.Memo != null)
                    {
                        Memo = salesOrderRet.Memo.GetValue();
                    }

                    string CustomerMsgRefKey = string.Empty;
                    if (salesOrderRet.CustomerMsgRef != null)
                    {
                        CustomerMsgRefKey = salesOrderRet.CustomerMsgRef.ListID.GetValue();
                    }
                    string IsToBePrinted = string.Empty;
                    if (salesOrderRet.IsToBePrinted != null)
                    {
                        IsToBePrinted = salesOrderRet.IsToBePrinted.GetValue().ToString();
                    }
                    string IsToEmailed = string.Empty;
                    if (salesOrderRet.IsToBeEmailed != null)
                    {
                        IsToEmailed = salesOrderRet.IsToBeEmailed.GetValue().ToString();
                    }
                    string IsTaxIncluded = string.Empty;
                    if (salesOrderRet.IsTaxIncluded != null)
                    {
                        IsTaxIncluded = salesOrderRet.IsTaxIncluded.GetValue().ToString();
                    }
                    string CustomerSalesTaxCodeRefKey = string.Empty;
                    if (salesOrderRet.CustomerSalesTaxCodeRef != null)
                    {
                        CustomerSalesTaxCodeRefKey = salesOrderRet.CustomerSalesTaxCodeRef.ListID.GetValue();
                    }
                    string Other = string.Empty;
                    if (salesOrderRet.Other != null)
                    {
                        Other = salesOrderRet.Other.GetValue();
                    }
                    string Amount = "0";
                    if (salesOrderRet.TotalAmount != null)
                    {
                        Amount = salesOrderRet.TotalAmount.GetValue().ToString();
                    }
                    string AmountPaid = "0";
                    // If Not (salesOrderRet.AppliedAmount Is Nothing) Then
                    // AmountPaid = salesOrderRet.AppliedAmount.GetValue
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
                    if (salesOrderRet.Subtotal != null)
                    {
                        subTotal = salesOrderRet.Subtotal.GetValue().ToString();
                    }



                    if (salesOrderRet.DataExtRetList != null)
                    {
                        IDataExtRet extInvLst;
                        string dataExtaname = string.Empty;
                        string dataExtValue = string.Empty;
                        for (int g = 0, loopTo1 = salesOrderRet.DataExtRetList.Count - 1; g <= loopTo1; g++)
                        {
                            extInvLst = salesOrderRet.DataExtRetList.GetAt(g);
                            dataExtaname = extInvLst.DataExtName.GetValue();
                            dataExtValue = extInvLst.DataExtValue.GetValue();
                            if (dataExtaname == "Customer ID")
                            {
                                CustomField1 = dataExtValue;
                            }
                            else if (dataExtaname == "Rep Name")
                            {
                                CustomField2 = dataExtValue;
                            }
                            else if (dataExtaname == "Sales Man Mob:")
                            {
                                CustomField3 = dataExtValue;
                            }
                            else if (dataExtaname == "Customer TRN")
                            {
                                CustomField4 = dataExtValue;
                            }
                        }

                    }

                    if (salesOrderRet.SalesTaxTotal != null)
                    {
                        SalesTaxTotal = salesOrderRet.SalesTaxTotal.GetValue().ToString();
                    }
                    if (salesOrderRet.SalesTaxPercentage != null)
                    {
                        SalesTaxPercentage = salesOrderRet.SalesTaxPercentage.GetValue().ToString();
                    }
                    // 'Print(RefNumber)

                    // Insert data in database code left..........

                    db.tblSO_Insert(ClearAllControl.gblCompanyID, TxnID, CustomerRefKey, null, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, DateTime.Parse(DueDate), SalesRefKey, FOB, DateTime.Parse(ShipDate), ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, decimal.Parse(Amount), decimal.Parse(AmountPaid), CustomField1, CustomField2, CustomField3, currencyRef, CustomField5, QuotationRecNo, ExchangeRAte, InvoiceType, customerName, decimal.Parse(subTotal), decimal.Parse(SalesTaxTotal), decimal.Parse(SalesTaxPercentage));
                    if (salesOrderRet.LinkedTxnList != null)
                    {
                        for (int p = 0, loopTo2 = salesOrderRet.LinkedTxnList.Count - 1; p <= loopTo2; p++)
                        {
                            ILinkedTxn invoiceLinkTxn;
                            invoiceLinkTxn = salesOrderRet.LinkedTxnList.GetAt(p);
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


                    if (salesOrderRet.ORSalesOrderLineRetList != null)
                    {
                        for (int k = 0, loopTo3 = salesOrderRet.ORSalesOrderLineRetList.Count - 1; k <= loopTo3; k++)
                        {
                            IORSalesOrderLineRet InvoiceLineRetList;
                            InvoiceLineRetList = salesOrderRet.ORSalesOrderLineRetList.GetAt(k);

                            string lineQuantity = "0";
                            if (InvoiceLineRetList.SalesOrderLineRet.Quantity != null)
                            {
                                lineQuantity = InvoiceLineRetList.SalesOrderLineRet.Quantity.GetValue().ToString();
                            }

                            string lineAmount = "0";
                            if (InvoiceLineRetList.SalesOrderLineRet.Amount != null)
                            {
                                lineAmount = InvoiceLineRetList.SalesOrderLineRet.Amount.GetValue().ToString();
                            }


                            string lineDesc = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.Desc != null)
                            {
                                lineDesc = InvoiceLineRetList.SalesOrderLineRet.Desc.GetValue();
                            }
                            string lineItem = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.ItemRef != null)
                            {
                                lineItem = InvoiceLineRetList.SalesOrderLineRet.ItemRef.FullName.GetValue();
                            }
                            if (string.IsNullOrEmpty(lineItem))
                            {
                                goto caltchnullline;
                            }
                            string lineRate = "0";
                            if (InvoiceLineRetList.SalesOrderLineRet.ORRate.Rate != null)
                            {
                                lineRate = InvoiceLineRetList.SalesOrderLineRet.ORRate.Rate.GetValue().ToString();
                            }
                            string customefield = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.Other1 != null)
                            {
                                customefield = InvoiceLineRetList.SalesOrderLineRet.Other1.GetValue();
                            }
                            string customeFeild2 = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.Other2 != null)
                            {
                                customeFeild2 = InvoiceLineRetList.SalesOrderLineRet.Other2.GetValue();
                            }
                            string lineUOM = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.UnitOfMeasure != null)
                            {
                                lineUOM = InvoiceLineRetList.SalesOrderLineRet.UnitOfMeasure.GetValue();
                            }
                            // Dim lineQuantity As String = String.Empty
                            // If Not InvoiceLineRetList.InvoiceLineRet.Quantity Is Nothing Then
                            // lineQuantity = InvoiceLineRetList.InvoiceLineRet.Quantity.GetValue()
                            // End If
                            string lineUOMOrg = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.OverrideUOMSetRef != null)
                            {
                                lineUOMOrg = InvoiceLineRetList.SalesOrderLineRet.OverrideUOMSetRef.FullName.GetValue();
                            }
                            string lineTxnID = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.TxnLineID != null)
                            {
                                lineTxnID = InvoiceLineRetList.SalesOrderLineRet.TxnLineID.GetValue();
                            }
                            string txaCode = string.Empty;
                            if (InvoiceLineRetList.SalesOrderLineRet.SalesTaxCodeRef != null)
                            {
                                if (InvoiceLineRetList.SalesOrderLineRet.SalesTaxCodeRef.FullName != null)
                                {
                                    txaCode = InvoiceLineRetList.SalesOrderLineRet.SalesTaxCodeRef.FullName.GetValue();
                                }
                            }

                            double taxAmount = 0d;
                            if (InvoiceLineRetList.SalesOrderLineRet.TaxAmount != null)
                            {
                                taxAmount = InvoiceLineRetList.SalesOrderLineRet.TaxAmount.GetValue();
                            }


                            db.tblSOLine_Insert(TxnID, decimal.Parse(lineAmount), lineDesc, lineItem, lineRate, customefield, customeFeild2, lineUOM, lineQuantity, lineUOMOrg, lineTxnID, txaCode, (decimal?)taxAmount);
                        caltchnullline:
                            ;

                        }


                    }

                }
                return;

            }
            catch
            {
               // int w = 0;
                 
            }
        }

        #endregion

        #region Methods
        private void LoadSOCustomer(int cID)
        {
            cmbxSOCustomer.Items.Clear();
            sql.addparam("@CompanyID", cID);
            sql.execquery("SELECT distinct customerName from tblSO where companyID=@CompanyID");
            if(sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    cmbxSOCustomer.Items.Add(r["customerName"]);
                cmbxSOCustomer.SelectedIndex = -1;
            }

            //var col = new System.Windows.Forms.AutoCompleteStringCollection();
            //for (int i = 0, loopTo = sql.sqlds.Tables[0].Rows.Count - 1; i <= loopTo; i++)
            //    col.Add(sql.sqlds.Tables[0].Rows[i]["customerName"].ToString());
            //cmbxSOCustomer.SelectedItem = System.Windows.Forms.AutoCompleteSource.CustomSource;
            ////cmbxSOCustomer.ItemsSource = col;
            //cmbxSOCustomer.SelectedItem = System.Windows.Forms.AutoCompleteMode.Suggest;
        }

        private void LoadSONumber(string cust, int cID)
        {
            try
            {
                cmbxSONumber.SelectedIndex = -1;
                cmbxSONumber.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.addparam("@CustomerName", cust);
                sql.execquery("SELECT distinct RefNumber,TxnId  from tblSO where companyID=@CompanyID and ((@CustomerName is null) or (customerName=@CustomerName )) order by RefNumber");
                if(sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxSONumber.Items.Add(r["RefNumber"]);
                    cmbxSONumber.SelectedIndex = -1;
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void PopulateTempleteCombobox()
        {
            tmpltSOCmpx.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Sales Order' and Status='True'");
            if (sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    tmpltSOCmpx.Items.Add(r["TemplateName"]);
                tmpltSOCmpx.SelectedIndex = 0;
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

        private void cmbxSOCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSONumber(cmbxSOCustomer.Text, ClearAllControl.gblCompanyID);
        }

        private void tmpltSOCmpx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempPath(tmpltSOCmpx.Text);
        }

        private void btnSoSyn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                //sessionManager.openConnection();
                //Sessions.OpenConnectionBeginSession();
                GetSOTransaction(ref bError, dpFrmDate.DisplayDate, dpToDate.DisplayDate);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Sales Order Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Sales Order Synced successfully", Haley.Enums.NotificationIcon.Success);
                }
                LoadSOCustomer(ClearAllControl.gblCompanyID);
               //Sessions.Sessions.EndSessionCloseConnection();
            }
            catch
            {

            }
        }

        private void btnSoSyncAll_Click(object sender, RoutedEventArgs e)
        {
           // connectToQB();
            try
            {
                connectToQB();
                bError = false;
                //Sessions.Sessions.OpenConnectionBeginSession();
                GetSOTransactionNoDate(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Sales Order Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Sales Order Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }
                LoadSOCustomer(ClearAllControl.gblCompanyID);
                //Sessions.Sessions.EndSessionCloseConnection();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnSoPrint_Click(object sender, RoutedEventArgs e)
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
                _ReportDocument.Load(path + tmpltSOCmpx.Text + ".rpt");
                string startuppath = System.AppDomain.CurrentDomain.BaseDirectory + path+ tmpltSOCmpx.Text +".rpt";

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
                crParameterDiscreteValue.Value = cmbxSONumber.Text;
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
            catch(Exception ex)
            {
                ds.ShowDialog("TNC-Sync", ex.Message, Haley.Enums.NotificationIcon.Error);
            }
        }

        #endregion
    }
}
