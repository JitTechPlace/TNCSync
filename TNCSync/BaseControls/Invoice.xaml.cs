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
using SAPBusinessObjects.WPF.ViewerShared;
using SAPBusinessObjects.WPF.Viewer;
using TNCSync.Sessions;
//using Interop.QBFC15;
using TNCSync.Class.DataBaseClass;
using Haley.Abstractions;
using Haley.MVVM;
using TNCSync.Class;
using Microsoft.VisualBasic;
using System.Data;
using Interop.QBFC16;
using System.Data.SqlClient;
using System.Configuration;
using TNCSync.View;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for Invoice.xaml
    /// </summary>
    public partial class Invoice : UserControl
    {
        private bool bError;
        private string path;
        private SQLControls sql = new SQLControls();
        SessionManager sessionManager;
        private short maxVersion;
        private bool booSessionBegun = false;
        //private IMsgSetRequest msgSetRequest;
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService ds;
        Dictionary<string, string> comboSource = new Dictionary<string, string>();

        public Invoice()
        {
            InitializeComponent();
            
            /// connectToQB();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
            PopulateTempleteCombobox();
            LoadInvoiceCustomer(ClearAllControl.gblCompanyID);
        }

        #region CONNECTION TO QB
        private void connectToQB()
        {
            sessionManager = SessionManager.getInstance();
            booSessionBegun = true;
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


        #region Invoice Methods
        public void GetInvoiceTransaction(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                // make sure we do not have any old requests still defined
                IMsgSetRequest msgSetRequest = sessionManager.getMsgSetRequest();
                msgSetRequest.ClearRequests();
                // set the OnError attribute to continueOnError
                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                // Add the BillQuery request
                IInvoiceQuery RecepitQuery = msgSetRequest.AppendInvoiceQueryRq();
                RecepitQuery.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                RecepitQuery.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
                RecepitQuery.IncludeLineItems.SetValue(true);
                RecepitQuery.IncludeLinkedTxns.SetValue(true);

                RecepitQuery.OwnerIDList.Add("0");
                RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll);

                bool bDone = false;
                while (!bDone)
                {
                    // start looking for customer next in the list

                    // send the request to QB
                    IMsgSetResponse msgSetResponse = sessionManager.doRequest(true, ref msgSetRequest);

                    // MsgBox(msgSetRequest.ToXMLString())
                    FillInvoiceInDataBase(ref msgSetResponse, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                ds.SendToast("Not Responding", "Unable to connect with QuickBooks response", Haley.Enums.NotificationIcon.Error);
                bError = true;
            }
        }

        public void GetInvoiceTransactions(ref bool bError)
        {
            try
            {
                // make sure we do not have any old requests still defined
                IMsgSetRequest msgSetRequest = sessionManager.getMsgSetRequest();
                msgSetRequest.ClearRequests();
                // set the OnError attribute to continueOnError
                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                // Add the BillQuery request
                IInvoiceQuery RecepitQuery = msgSetRequest.AppendInvoiceQueryRq();
                RecepitQuery.IncludeLineItems.SetValue(true);
                RecepitQuery.IncludeLinkedTxns.SetValue(true);
                RecepitQuery.OwnerIDList.Add("0");
                RecepitQuery.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll);
                bool bDone = false;
                while (!bDone)
                {
                    // start looking for customer next in the list

                    // send the request to QB
                    IMsgSetResponse msgSetResponse = sessionManager.doRequest(true, ref msgSetRequest);

                    // MsgBox(msgSetRequest.ToXMLString())
                    FillInvoiceInDataBase(ref msgSetResponse, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                ds.SendToast("Not Responding", "Unable to connect with QuickBooks response", Haley.Enums.NotificationIcon.Error);
                bError = true;
            }
        }

        public void FillInvoiceInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
        {
            try
            {
                string RefNumber = string.Empty;
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

                // make sure we are processing the CustomerQueryRs and 
                // the CustomerRetList responses in this response list
                IInvoiceRetList InvoiceRetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtInvoiceQueryRs & responseDetailType == ENObjectType.otInvoiceRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    InvoiceRetList = (IInvoiceRetList)response.Detail;
                }
                else
                {
                    // bill, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                // Parse the query response and add the Customers to the Customer list box
                short count;
                short index;
                IInvoiceRet InvoiceRet;
                count = (short)InvoiceRetList.Count;
                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + InvoiceRetList.Count);
                // we are done with the customerQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }
                db.tblInvoice_Delete(ClearAllControl.gblCompanyID);
                db.tblInvoiceLIne_Delete();
                db.tblInvoiceApplTxn_Delete();
                var linkTxnDate = default(DateTime);
                var ServDate = default(DateTime);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    // skip this customer if this is a repeat from the last query
                    InvoiceRet = InvoiceRetList.GetAt(index);
                    if (InvoiceRet is null | InvoiceRet.CustomerRef.ListID is null | InvoiceRet.TxnID is null)

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


                    string TxnID = InvoiceRet.TxnID.GetValue();
                    string CustomerRefKey = string.Empty;
                    if (InvoiceRet.CustomerRef != null)
                    {
                        CustomerRefKey = InvoiceRet.CustomerRef.ListID.GetValue();
                    }
                    string customerName = string.Empty;
                    if (InvoiceRet.CustomerRef != null)
                    {
                        customerName = InvoiceRet.CustomerRef.FullName.GetValue();
                    }


                    string currencyRef = string.Empty;
                    if (InvoiceRet.CurrencyRef != null)
                    {
                        currencyRef = InvoiceRet.CurrencyRef.FullName.GetValue();
                    }

                    string ExchangeRAte = string.Empty;
                    if (InvoiceRet.ExchangeRate != null)
                    {
                        ExchangeRAte = InvoiceRet.ExchangeRate.GetValue().ToString();
                    }



                    string ClassRefKey = string.Empty;
                    if (InvoiceRet.ClassRef != null)
                    {
                        ClassRefKey = InvoiceRet.ClassRef.FullName.GetValue();
                    }

                    string ARAccountRefKey = string.Empty;
                    if (InvoiceRet.ARAccountRef != null)
                    {
                        ARAccountRefKey = InvoiceRet.ARAccountRef.FullName.GetValue();

                    }

                    string TemplateRefKey = string.Empty;
                    if (InvoiceRet.TemplateRef != null)
                    {
                        TemplateRefKey = InvoiceRet.TemplateRef.ListID.GetValue();
                    }
                    var TxnDate = InvoiceRet.TxnDate.GetValue();



                    if (InvoiceRet.RefNumber != null)
                    {
                        RefNumber = InvoiceRet.RefNumber.GetValue();
                    }
                    if (RefNumber == "12" | RefNumber == "09613")
                    {
                        int m = 0;
                    }

                    // Bill Address


                    if (InvoiceRet.BillAddress != null)
                    {


                        if (InvoiceRet.BillAddress.Addr1 != null)
                        {
                            BillAddress1 = InvoiceRet.BillAddress.Addr1.GetValue();
                        }


                        if (InvoiceRet.BillAddress.Addr2 != null)
                        {
                            BillAddress2 = InvoiceRet.BillAddress.Addr2.GetValue();
                        }

                        if (InvoiceRet.BillAddress.Addr3 != null)
                        {
                            BillAddress3 = InvoiceRet.BillAddress.Addr3.GetValue();
                        }

                        if (InvoiceRet.BillAddress.Addr4 != null)
                        {
                            BillAddress4 = InvoiceRet.BillAddress.Addr4.GetValue();
                        }

                        if (InvoiceRet.BillAddress.Addr5 != null)
                        {
                            BillAddress5 = InvoiceRet.BillAddress.Addr5.GetValue();
                        }

                        if (InvoiceRet.BillAddress.City != null)
                        {
                            BillAddressCity = InvoiceRet.BillAddress.City.GetValue();
                        }

                        if (InvoiceRet.BillAddress.State != null)
                        {
                            BillAddressState = InvoiceRet.BillAddress.State.GetValue();
                        }

                        if (InvoiceRet.BillAddress.PostalCode != null)
                        {
                            BillAddressPostalCode = InvoiceRet.BillAddress.PostalCode.GetValue();
                        }

                        if (InvoiceRet.BillAddress.Country != null)
                        {
                            BillAddressCountry = InvoiceRet.BillAddress.Country.GetValue();
                        }

                        if (InvoiceRet.BillAddress.Note != null)
                        {
                            BillAddressNote = InvoiceRet.BillAddress.Note.GetValue();
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
                    if (InvoiceRet.ShipAddress != null)
                    {

                        if (InvoiceRet.ShipAddress.Addr1 != null)
                        {
                            ShipAddress1 = InvoiceRet.ShipAddress.Addr1.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.Addr2 != null)
                        {
                            ShipAddress2 = InvoiceRet.ShipAddress.Addr2.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.Addr3 != null)
                        {
                            ShipAddress3 = InvoiceRet.ShipAddress.Addr3.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.Addr4 != null)
                        {
                            ShipAddress4 = InvoiceRet.ShipAddress.Addr4.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.Addr5 != null)
                        {
                            ShipAddress5 = InvoiceRet.ShipAddress.Addr5.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.City != null)
                        {
                            ShipAddressCity = InvoiceRet.ShipAddress.City.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.State != null)
                        {
                            ShipAddressState = InvoiceRet.ShipAddress.State.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.PostalCode != null)
                        {
                            ShipAddressPostalCode = InvoiceRet.ShipAddress.PostalCode.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.Country != null)
                        {
                            ShipAddressCountry = InvoiceRet.ShipAddress.Country.GetValue();
                        }

                        if (InvoiceRet.ShipAddress.Note != null)
                        {
                            ShipAddressNote = InvoiceRet.ShipAddress.Note.GetValue();
                        }
                    }
                    // End ship address

                    string IsPending = string.Empty;
                    if (InvoiceRet.IsPending != null)
                    {
                        IsPending = InvoiceRet.IsPending.GetValue().ToString();
                        if (bool.Parse(IsPending) == true)
                        {
                            Interaction.MsgBox("stop here");
                        }
                    }
                    string PONumber = string.Empty;
                    if (InvoiceRet.PONumber != null)
                    {
                        PONumber = InvoiceRet.PONumber.GetValue();
                    }

                    string TermsRefKey = string.Empty;
                    if (InvoiceRet.TermsRef != null)
                    {
                        TermsRefKey = InvoiceRet.TermsRef.FullName.GetValue();
                    }

                    string DueDate = string.Empty;
                    if (InvoiceRet.DueDate != null)
                    {
                        DueDate = InvoiceRet.DueDate.GetValue().ToString();
                    }

                    string SalesRefKey = string.Empty;
                    if (InvoiceRet.SalesRepRef != null)
                    {
                        SalesRefKey = InvoiceRet.SalesRepRef.FullName.GetValue();
                    }

                    string FOB = string.Empty;
                    if (InvoiceRet.FOB != null)
                    {
                        FOB = InvoiceRet.FOB.GetValue();
                    }

                    string ShipDate = string.Empty;
                    if (InvoiceRet.ShipDate != null)
                    {
                        ShipDate = InvoiceRet.ShipDate.GetValue().ToString();
                    }

                    string ShipMethodRefKey = string.Empty;
                    if (InvoiceRet.ShipMethodRef != null)
                    {
                        ShipMethodRefKey = InvoiceRet.ShipMethodRef.FullName.GetValue();
                    }

                    string ItemSalesTaxRefKey = string.Empty;
                    if (InvoiceRet.ItemSalesTaxRef != null)
                    {
                        ItemSalesTaxRefKey = InvoiceRet.ItemSalesTaxRef.ListID.GetValue();
                    }

                    string Memo = string.Empty;
                    if (InvoiceRet.Memo != null)
                    {
                        Memo = InvoiceRet.Memo.GetValue();
                    }

                    string CustomerMsgRefKey = string.Empty;
                    if (InvoiceRet.CustomerMsgRef != null)
                    {
                        CustomerMsgRefKey = InvoiceRet.CustomerMsgRef.ListID.GetValue();
                    }
                    string IsToBePrinted = string.Empty;
                    if (InvoiceRet.IsToBePrinted != null)
                    {
                        IsToBePrinted = InvoiceRet.IsToBePrinted.GetValue().ToString();
                    }
                    string IsToEmailed = string.Empty;
                    if (InvoiceRet.IsToBeEmailed != null)
                    {
                        IsToEmailed = InvoiceRet.IsToBeEmailed.GetValue().ToString();
                    }
                    string IsTaxIncluded = string.Empty;
                    if (InvoiceRet.IsTaxIncluded != null)
                    {
                        IsTaxIncluded = InvoiceRet.IsTaxIncluded.GetValue().ToString();
                    }
                    string CustomerSalesTaxCodeRefKey = string.Empty;
                    if (InvoiceRet.CustomerSalesTaxCodeRef != null)
                    {
                        CustomerSalesTaxCodeRefKey = InvoiceRet.CustomerSalesTaxCodeRef.ListID.GetValue();
                    }
                    string Other = string.Empty;
                    if (InvoiceRet.Other != null)
                    {
                        Other = InvoiceRet.Other.GetValue();
                    }
                    string Amount = "0";
                    if (InvoiceRet.BalanceRemaining != null)
                    {
                        Amount = InvoiceRet.BalanceRemaining.GetValue().ToString();
                    }
                    string AmountPaid = "0";
                    if (InvoiceRet.AppliedAmount != null)
                    {
                        AmountPaid = InvoiceRet.AppliedAmount.GetValue().ToString();
                    }
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
                    if (InvoiceRet.Subtotal != null)
                    {
                        subTotal = InvoiceRet.Subtotal.GetValue().ToString();
                    }



                    if (InvoiceRet.DataExtRetList != null)
                    {
                        IDataExtRet extInvLst;
                        string dataExtaname = string.Empty;
                        string dataExtValue = string.Empty;
                        for (int g = 0, loopTo1 = InvoiceRet.DataExtRetList.Count - 1; g <= loopTo1; g++)
                        {
                            extInvLst = InvoiceRet.DataExtRetList.GetAt(g);
                            dataExtaname = extInvLst.DataExtName.GetValue();
                            dataExtValue = extInvLst.DataExtValue.GetValue();
                            if (dataExtaname == "TRN")
                            {
                                CustomField1 = dataExtValue;
                            }
                            else if (dataExtaname == "CODE")
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

                    if (InvoiceRet.SalesTaxTotal != null)
                    {
                        SalesTaxTotal = InvoiceRet.SalesTaxTotal.GetValue().ToString();
                    }
                    if (InvoiceRet.SalesTaxPercentage != null)
                    {
                        SalesTaxPercentage = InvoiceRet.SalesTaxPercentage.GetValue().ToString();
                    }


                    db.tblInvoice_Insert(ClearAllControl.gblCompanyID, TxnID, CustomerRefKey, currencyRef, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, DateTime.Parse(DueDate), SalesRefKey, FOB, DateTime.Parse(ShipDate), ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, decimal.Parse(Amount), decimal.Parse(AmountPaid), CustomField1, CustomField2, CustomField3, currencyRef, CustomField5, QuotationRecNo, ExchangeRAte, InvoiceType, customerName, decimal.Parse(subTotal), decimal.Parse(SalesTaxTotal), decimal.Parse(SalesTaxPercentage));
                    if (InvoiceRet.LinkedTxnList != null)
                    {
                        for (int p = 0, loopTo2 = InvoiceRet.LinkedTxnList.Count - 1; p <= loopTo2; p++)
                        {
                            ILinkedTxn invoiceLinkTxn;
                            invoiceLinkTxn = InvoiceRet.LinkedTxnList.GetAt(p);
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


                    if (InvoiceRet.ORInvoiceLineRetList != null)
                    {
                        for (int k = 0, loopTo3 = InvoiceRet.ORInvoiceLineRetList.Count - 1; k <= loopTo3; k++)   ///Need to check here{Object ref not set to instance of an object}
                        {
                            IORInvoiceLineRet InvoiceLineRetList;
                            InvoiceLineRetList = InvoiceRet.ORInvoiceLineRetList.GetAt(k);

                            string lineItem = string.Empty;                 // Pls Check Here
                            //if (InvoiceLineRetList.InvoiceLineRet.ItemRef != null)       
                            //{
                            //   // lineItem = InvoiceLineRetList.InvoiceLineRet.ItemRef.FullName.GetValue();
                            //}
                            if (string.IsNullOrEmpty(lineItem))
                            {
                                goto caltchnullline;
                            }

                            string lineDesc = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.Desc != null)
                            {
                                lineDesc = InvoiceLineRetList.InvoiceLineRet.Desc.GetValue();
                            }

                            string lineQuantity = "0";
                            string lineRate = "0";
                            if (lineItem != "Subtotal")
                            {

                                if (InvoiceLineRetList.InvoiceLineRet.Quantity != null)
                                {
                                    lineQuantity = InvoiceLineRetList.InvoiceLineRet.Quantity.GetValue().ToString();
                                }

                                if (!string.IsNullOrEmpty(lineItem))
                                {
                                    if (InvoiceLineRetList.InvoiceLineRet.ORRate.Rate != null)
                                    {
                                        lineRate = InvoiceLineRetList.InvoiceLineRet.ORRate.Rate.GetValue().ToString();
                                    }
                                }

                            }
                            string lineAmount = "0";
                            if (InvoiceLineRetList.InvoiceLineRet.Amount != null)
                            {
                                lineAmount = InvoiceLineRetList.InvoiceLineRet.Amount.GetValue().ToString();
                            }




                            string lineUOM = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.UnitOfMeasure != null)
                            {
                                lineUOM = InvoiceLineRetList.InvoiceLineRet.UnitOfMeasure.GetValue();
                            }

                            string lineUOMOrg = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.OverrideUOMSetRef != null)
                            {
                                lineUOMOrg = InvoiceLineRetList.InvoiceLineRet.OverrideUOMSetRef.FullName.GetValue();
                            }
                            string lineTxnID = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.TxnLineID != null)
                            {
                                lineTxnID = InvoiceLineRetList.InvoiceLineRet.TxnLineID.GetValue();
                            }
                            string txaCode = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.SalesTaxCodeRef != null)
                            {
                                if (InvoiceLineRetList.InvoiceLineRet.SalesTaxCodeRef.FullName != null)
                                {
                                    txaCode = InvoiceLineRetList.InvoiceLineRet.SalesTaxCodeRef.FullName.GetValue();
                                }
                            }

                            double taxAmount = 0d;
                            if (InvoiceLineRetList.InvoiceLineRet.TaxAmount != null)
                            {
                                taxAmount = InvoiceLineRetList.InvoiceLineRet.TaxAmount.GetValue();
                            }

                            string other1 = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.Other1 != null)
                            {
                                other1 = InvoiceLineRetList.InvoiceLineRet.Other1.GetValue();
                            }

                            string other2 = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.Other2 != null)
                            {
                                other2 = InvoiceLineRetList.InvoiceLineRet.Other2.GetValue();
                            }
                            if (InvoiceLineRetList.InvoiceLineRet.ServiceDate != null)
                            {
                                ServDate = InvoiceLineRetList.InvoiceLineRet.ServiceDate.GetValue();
                            }

                            string customefield = string.Empty;
                            string customeFeild2 = string.Empty;
                            if (InvoiceLineRetList.InvoiceLineRet.DataExtRetList != null)
                            {
                                IDataExtRet extInvLine;
                                string dataExtanameLine = string.Empty;
                                string dataExtValueLine = string.Empty;
                                for (int p = 0, loopTo4 = InvoiceLineRetList.InvoiceLineRet.DataExtRetList.Count - 1; p <= loopTo4; p++)
                                {
                                    extInvLine = InvoiceLineRetList.InvoiceLineRet.DataExtRetList.GetAt(p);
                                    dataExtanameLine = extInvLine.DataExtName.GetValue();
                                    dataExtValueLine = extInvLine.DataExtValue.GetValue();
                                    if (dataExtanameLine == "TEST")
                                    {
                                        customefield = dataExtValueLine;
                                    }
                                    else if (dataExtValueLine == "DN")
                                    {
                                        CustomField2 = dataExtValueLine;
                                    }
                                }

                            }
                            db.tblInvoiceLine_Insert(TxnID, decimal.Parse(lineAmount), lineDesc, lineItem, lineRate, customefield, customeFeild2, lineUOM, decimal.Parse(lineQuantity), lineUOMOrg, lineTxnID, txaCode, (decimal?)taxAmount, other1, other2, ServDate);
                        caltchnullline:
                            ;
                        }
                    }
                }
                return;
            }
            catch
            {
                int w = 0;
                //Interaction.MsgBox("HRESULT = " + Information.Err().Number + "-" + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");
                bDone = true;
                bError = true;
            }
        }

        #endregion

        #region Methods
        private void LoadInvoiceCustomer(int cID)
        {
            try
            {
                cmbxInvoice.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.execquery("SELECT distinct customerName from tblInvoice where companyID=@CompanyID");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxInvoice.Items.Add(r["customerName"]);
                    cmbxInvoice.SelectedIndex = -1;
                }
                //Need to add the Autocomplete String Collection
            }
            catch
            {

            }
        }

        private void LoadInvoiceNumber(string cust, int cID)
        {
            try
            {
                cmbxInvoiceNum.SelectedIndex = -1;
                cmbxInvoiceNum.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.addparam("@CustomerName", cust);
                sql.execquery("SELECT distinct RefNumber,TxnId  from tblInvoice where companyID=@CompanyID and customerName=@CustomerName order by RefNumber");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxInvoiceNum.Items.Add(r["RefNumber"]);
                    cmbxInvoiceNum.SelectedIndex = -1;
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void PopulateTempleteCombobox()
        {
            cmbxTmptInvoice.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Sales Invoice' and Status='True' ");
            if (sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    cmbxTmptInvoice.Items.Add(r["TemplateName"]);
                cmbxTmptInvoice.SelectedIndex = 0;
            }
        }
        //tmpltCmbx.ItemsSource = table.DefaultView;
            //tmpltCmbx.DisplayMemberPath = "TemplateName";
            //tmpltCmbx.SelectedIndex = -1;
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
        private void cmbxInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadInvoiceNumber(cmbxInvoice.Text, ClearAllControl.gblCompanyID);
        }

        private void cmbxTmptInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempPath(cmbxTmptInvoice.Text);
        }

        private void btnsyncInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bError = false;
                connectToQB();
                //sessionManager.openConnection();
                GetInvoiceTransaction(ref bError, dpFrmDate.DisplayDate, dpToDate.DisplayDate);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Synced successfully", Haley.Enums.NotificationIcon.Success);
                }
                disconnectFromQB();
                LoadInvoiceCustomer(ClearAllControl.gblCompanyID);
            }
            catch(Exception ex)
            {

            }
        }

        private void btnsyncalInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bError = false;
                connectToQB();
                GetInvoiceTransactions(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Invoice Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Invoice Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }
                LoadInvoiceCustomer(ClearAllControl.gblCompanyID);
                disconnectFromQB();
            }
            catch
            {

            }
        }

        private void btnInvoicePrint_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{

            //    ReportDocument _ReportDocument = new ReportDocument();
            //    var crtableLogoninfos = new TableLogOnInfos();
            //    var crtableLogoninfo = new TableLogOnInfo();
            //    var crConnectionInfo = new ConnectionInfo();
            //    ParameterFieldDefinitions crParameterFieldDefinitions;
            //    ParameterFieldDefinition crParameterFieldDefinition;
            //    var crParameterValues = new ParameterValues();
            //    var crParameterDiscreteValue = new ParameterDiscreteValue();
            //    Tables CrTables;

            //    _ReportDocument.Load("C:\\Users\\Software\\Desktop\\ImageTechSolutions CMS Repot files\\TaxInvoice_New.rpt");
            //    //{
            //    //    //ref var withBlock = ref _ReportDocument;
            //    //    //string startuppatah = Application.StartupPath;
            //    //    _ReportDocument.Load(path + cmbxTmptInvoice.Text + ".rpt");
            //    //}

            //    _ReportDocument.ReportOptions.EnableSaveDataWithReport = false;
            //    // SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString: "TNCSync_Connection");
            //    SqlConnection sqlconn = new SqlConnection(ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ToString());
            //    crConnectionInfo.ServerName = sqlconn.DataSource;
            //    crConnectionInfo.DatabaseName = sqlconn.Database;
            //    crConnectionInfo.UserID = "sa";
            //    crConnectionInfo.Password = "p@ssw0rd";
            //    crConnectionInfo.AllowCustomConnection = false;
            //    crConnectionInfo.IntegratedSecurity = true;

            //    CrTables = _ReportDocument.Database.Tables;

            //    // _ReportDocument.SetParameterValue("@CompanyID", gblCompanyID)
            //    foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in CrTables)
            //    {
            //        crtableLogoninfo = CrTable.LogOnInfo;
            //        crtableLogoninfo.ConnectionInfo = crConnectionInfo;
            //        crtableLogoninfo.ReportName = _ReportDocument.Name;
            //        crtableLogoninfo.TableName = CrTable.Name;
            //        CrTable.ApplyLogOnInfo(crtableLogoninfo);
            //    }

            //    crParameterDiscreteValue.Value = cmbxInvoiceNum.Text;
            //    crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
            //    crParameterFieldDefinition = crParameterFieldDefinitions["@refNumber"];
            //    crParameterValues = crParameterFieldDefinition.CurrentValues;
            //    crParameterValues.Clear();
            //    crParameterValues.Add(crParameterDiscreteValue);
            //    crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

            //    var rpt = new ReportView();
            //    rpt.ShowReportView(ref _ReportDocument);
            //    //rpt.CRV();
            //    // If _ReportDocument.HasRecords Then
            //    //var frm = new ReportFormLinker();
            //    //frm.MdiParent = ParentForm;
            //    //frm.WindowState = FormWindowState.Maximized;
            //    //frm.ShowReportinViewer(ref _ReportDocument);
            //    //frm.Refresh();
            //    //frm.Show();
            //}

            //catch (NullReferenceException ex)
            //{
            //    ds.ShowDialog("TNC-Sync", ex.Message, Haley.Enums.NotificationIcon.Error);
            //}
        }

        #endregion
    }
}
