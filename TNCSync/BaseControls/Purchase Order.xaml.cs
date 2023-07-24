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
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
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
    /// Interaction logic for Purchase_Order.xaml
    /// </summary>
    public partial class Purchase_Order : UserControl
    {
        private bool bError;
        private string path = null;
        private SQLControls sql = new SQLControls();
        SessionManager sessionManager;
        private short maxVersion;
        private bool booSessionBegun = false;
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService ds;

        public Purchase_Order()
        {
            InitializeComponent();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
          // PopulateTempleteCombobox();

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


        #region POMethods

        public void GetPOTransaction(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                //Add the billquery Request
                IPurchaseOrderQuery receiptQuery = msgsetRequest.AppendPurchaseOrderQueryRq();

                receiptQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                receiptQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);

                receiptQuery.IncludeLineItems.SetValue(true);
                receiptQuery.IncludeLinkedTxns.SetValue(true);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillPOInDatabase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                ds.SendToast("Not Responding", "Unable to connect with QuickBooks response", Haley.Enums.NotificationIcon.Error);
                bError = true;
            }
        }

        public void GetPOTransactionNoDate(ref bool bError)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                //Add the billquery Request
                IPurchaseOrderQuery receiptQuery = msgsetRequest.AppendPurchaseOrderQueryRq();

                receiptQuery.IncludeLineItems.SetValue(true);
                receiptQuery.IncludeLinkedTxns.SetValue(true);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillPOInDatabase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                ds.SendToast("Not Responding", "Unable to connect with QuickBooks response", Haley.Enums.NotificationIcon.Error);
                bError = true;
            }
        }

        public void FillPOInDatabase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
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
                // make sure we are processing the POQueryRs and 
                // the IPurchaseOrderRetList responses in this response list
                IPurchaseOrderRetList purchaseOrderRetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtPurchaseOrderQueryRs & responseDetailType == ENObjectType.otPurchaseOrderRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    purchaseOrderRetList = (IPurchaseOrderRetList)response.Detail;
                }
                else
                {
                    // bill, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }
                // Parse the query response and add the PO to the PO list box
                short count;
                short index;
                IPurchaseOrderRet PurchaseOrderRet;
                count = (short)purchaseOrderRetList.Count;
                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + purchaseOrderRetList.Count);
                // we are done with the POrQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }
                db.QBPurchaseOrder_Delete(ClearAllControl.gblCompanyID);
                // 'db.tblInvoiceApplTxn_Delete()
                var DueDate852 = default(DateTime);
                var ExpectedDate853 = default(DateTime);
                var LinkType876 = default(ENLinkType);
                var ServiceDate895 = default(DateTime);
                var ServiceDate933 = default(DateTime);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    // skip this customer if this is a repeat from the last query
                    PurchaseOrderRet = purchaseOrderRetList.GetAt(index);
                    if (PurchaseOrderRet is null)
                    {
                        bDone = true;
                        bError = true;
                        return;
                    }

                    string TxnID803 = null;
                    DateTime TimeCreated804;
                    DateTime TimeModified805;
                    string EditSequence806 = null;
                    int TxnNumber807 = 0;
                    string VendorRefListID808 = null;
                    string VendorRefFullName809 = null;
                    string ClassRefListID810 = null;
                    string ClassRefFullName811 = null;
                    string InventorySiteRefListID812 = null;
                    string InventorySiteRefFullName813 = null;
                    string ShipToEntityRefListID814 = null;
                    string ShipToEntityRefFullName815 = null;
                    string TemplateRefListID816 = null;
                    string TemplateRefFullName817 = null;
                    DateTime TxnDate818;
                    string RefNumber819 = null;
                    string VendorAddressAddr1820 = null;
                    string VendorAddressAddr2821 = null;
                    string VendorAddressAddr3822 = null;
                    string VendorAddressAddr4823 = null;
                    string VendorAddressAddr5824 = null;
                    string VendorAddressCity825 = null;
                    string VendorAddressState826 = null;
                    string VendorAddressPostalCode827 = null;
                    string VendorAddressCountry828 = null;
                    string VendorAddressNote829 = null;
                    string VendorAddressBlockAddr1830 = null;
                    string VendorAddressBlockAddr2831 = null;
                    string VendorAddressBlockAddr3832 = null;
                    string VendorAddressBlockAddr4833 = null;
                    string VendorAddressBlockAddr5834 = null;
                    string ShipAddressAddr1835 = null;
                    string ShipAddressAddr2836 = null;
                    string ShipAddressAddr3837 = null;
                    string ShipAddressAddr4838 = null;
                    string ShipAddressAddr5839 = null;
                    string ShipAddressCity840 = null;
                    string ShipAddressState841 = null;
                    string ShipAddressPostalCode842 = null;
                    string ShipAddressCountry843 = null;
                    string ShipAddressNote844 = null;
                    string ShipAddressBlockAddr1845 = null;
                    string ShipAddressBlockAddr2846 = null;
                    string ShipAddressBlockAddr3847 = null;
                    string ShipAddressBlockAddr4848 = null;
                    string ShipAddressBlockAddr5849 = null;
                    string TermsRefListID850 = null;
                    string TermsRefFullName851 = null;
                    string ShipMethodRefListID854 = null;
                    string ShipMethodRefFullName855 = null;
                    string FOB856 = null;
                    double TotalAmount857 = 0d;
                    string CurrencyRefListID858 = null;
                    string CurrencyRefFullName859 = null;
                    string ExchangeRate860 = null;
                    double TotalAmountInHomeCurrency861 = 0d;
                    bool IsManuallyClosed862 = false;
                    bool IsFullyReceived863 = false;
                    string Memo864 = null;
                    string VendorMsg865 = null;
                    bool IsToBePrinted866 = false;
                    bool IsToBeEmailed867 = false;
                    string Other1868 = null;
                    string Other2869 = null;
                    string ExternalGUID870 = null;
                    string taxcode = null;



                    TxnID803 = PurchaseOrderRet.TxnID.GetValue();
                    // Get value of TimeCreated
                    TimeCreated804 = PurchaseOrderRet.TimeCreated.GetValue();
                    // Get value of TimeModified
                    TimeModified805 = PurchaseOrderRet.TimeModified.GetValue();
                    // Get value of EditSequence
                    EditSequence806 = PurchaseOrderRet.EditSequence.GetValue();
                    // Get value of TxnNumber
                    if (PurchaseOrderRet.TxnNumber != null)
                    {
                        TxnNumber807 = PurchaseOrderRet.TxnNumber.GetValue();
                    }
                    if (PurchaseOrderRet.VendorRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.VendorRef.ListID != null)
                        {
                            VendorRefListID808 = PurchaseOrderRet.VendorRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.VendorRef.FullName != null)
                        {
                            VendorRefFullName809 = PurchaseOrderRet.VendorRef.FullName.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.ClassRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.ClassRef.ListID != null)
                        {
                            ClassRefListID810 = PurchaseOrderRet.ClassRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.ClassRef.FullName != null)
                        {
                            ClassRefFullName811 = PurchaseOrderRet.ClassRef.FullName.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.InventorySiteRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.InventorySiteRef.ListID != null)
                        {
                            InventorySiteRefListID812 = PurchaseOrderRet.InventorySiteRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.InventorySiteRef.FullName != null)
                        {
                            InventorySiteRefFullName813 = PurchaseOrderRet.InventorySiteRef.FullName.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.ShipToEntityRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.ShipToEntityRef.ListID != null)
                        {
                            ShipToEntityRefListID814 = PurchaseOrderRet.ShipToEntityRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.ShipToEntityRef.FullName != null)
                        {
                            ShipToEntityRefFullName815 = PurchaseOrderRet.ShipToEntityRef.FullName.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.TemplateRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.TemplateRef.ListID != null)
                        {
                            TemplateRefListID816 = PurchaseOrderRet.TemplateRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.TemplateRef.FullName != null)
                        {
                            TemplateRefFullName817 = PurchaseOrderRet.TemplateRef.FullName.GetValue();
                        }
                    }
                    // Get value of TxnDate
                    TxnDate818 = PurchaseOrderRet.TxnDate.GetValue();
                    // Get value of RefNumber
                    if (PurchaseOrderRet.RefNumber != null)
                    {
                        RefNumber819 = PurchaseOrderRet.RefNumber.GetValue();
                    }
                    if (PurchaseOrderRet.VendorAddress != null)
                    {
                        // Get value of Addr1
                        if (PurchaseOrderRet.VendorAddress.Addr1 != null)
                        {
                            VendorAddressAddr1820 = PurchaseOrderRet.VendorAddress.Addr1.GetValue();
                        }
                        // Get value of Addr2
                        if (PurchaseOrderRet.VendorAddress.Addr2 != null)
                        {
                            VendorAddressAddr2821 = PurchaseOrderRet.VendorAddress.Addr2.GetValue();
                        }
                        // Get value of Addr3
                        if (PurchaseOrderRet.VendorAddress.Addr3 != null)
                        {
                            VendorAddressAddr3822 = PurchaseOrderRet.VendorAddress.Addr3.GetValue();
                        }
                        // Get value of Addr4
                        if (PurchaseOrderRet.VendorAddress.Addr4 != null)
                        {
                            VendorAddressAddr4823 = PurchaseOrderRet.VendorAddress.Addr4.GetValue();
                        }
                        // Get value of Addr5
                        if (PurchaseOrderRet.VendorAddress.Addr5 != null)
                        {
                            VendorAddressAddr5824 = PurchaseOrderRet.VendorAddress.Addr5.GetValue();
                        }
                        // Get value of City
                        if (PurchaseOrderRet.VendorAddress.City != null)
                        {
                            VendorAddressCity825 = PurchaseOrderRet.VendorAddress.City.GetValue();
                        }
                        // Get value of State
                        if (PurchaseOrderRet.VendorAddress.State != null)
                        {
                            VendorAddressState826 = PurchaseOrderRet.VendorAddress.State.GetValue();
                        }
                        // Get value of PostalCode
                        if (PurchaseOrderRet.VendorAddress.PostalCode != null)
                        {
                            VendorAddressPostalCode827 = PurchaseOrderRet.VendorAddress.PostalCode.GetValue();
                        }
                        // Get value of Country
                        if (PurchaseOrderRet.VendorAddress.Country != null)
                        {
                            VendorAddressCountry828 = PurchaseOrderRet.VendorAddress.Country.GetValue();
                        }
                        // Get value of Note
                        if (PurchaseOrderRet.VendorAddress.Note != null)
                        {
                            VendorAddressNote829 = PurchaseOrderRet.VendorAddress.Note.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.VendorAddressBlock != null)
                    {
                        // Get value of Addr1
                        if (PurchaseOrderRet.VendorAddressBlock.Addr1 != null)
                        {
                            VendorAddressBlockAddr1830 = PurchaseOrderRet.VendorAddressBlock.Addr1.GetValue();
                        }
                        // Get value of Addr2
                        if (PurchaseOrderRet.VendorAddressBlock.Addr2 != null)
                        {
                            VendorAddressBlockAddr2831 = PurchaseOrderRet.VendorAddressBlock.Addr2.GetValue();
                        }
                        // Get value of Addr3
                        if (PurchaseOrderRet.VendorAddressBlock.Addr3 != null)
                        {
                            VendorAddressBlockAddr3832 = PurchaseOrderRet.VendorAddressBlock.Addr3.GetValue();
                        }
                        // Get value of Addr4
                        if (PurchaseOrderRet.VendorAddressBlock.Addr4 != null)
                        {
                            VendorAddressBlockAddr4833 = PurchaseOrderRet.VendorAddressBlock.Addr4.GetValue();
                        }
                        // Get value of Addr5
                        if (PurchaseOrderRet.VendorAddressBlock.Addr5 != null)
                        {
                            VendorAddressBlockAddr5834 = PurchaseOrderRet.VendorAddressBlock.Addr5.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.ShipAddress != null)
                    {

                        // Get value of Addr1
                        if (PurchaseOrderRet.ShipAddress.Addr1 != null)
                        {
                            ShipAddressAddr1835 = PurchaseOrderRet.ShipAddress.Addr1.GetValue();
                        }
                        // Get value of Addr2
                        if (PurchaseOrderRet.ShipAddress.Addr2 != null)
                        {
                            ShipAddressAddr2836 = PurchaseOrderRet.ShipAddress.Addr2.GetValue();
                        }
                        // Get value of Addr3
                        if (PurchaseOrderRet.ShipAddress.Addr3 != null)
                        {
                            ShipAddressAddr3837 = PurchaseOrderRet.ShipAddress.Addr3.GetValue();
                        }
                        // Get value of Addr4
                        if (PurchaseOrderRet.ShipAddress.Addr4 != null)
                        {
                            ShipAddressAddr4838 = PurchaseOrderRet.ShipAddress.Addr4.GetValue();
                        }
                        // Get value of Addr5
                        if (PurchaseOrderRet.ShipAddress.Addr5 != null)
                        {
                            ShipAddressAddr5839 = PurchaseOrderRet.ShipAddress.Addr5.GetValue();
                        }
                        // Get value of City
                        if (PurchaseOrderRet.ShipAddress.City != null)
                        {
                            ShipAddressCity840 = PurchaseOrderRet.ShipAddress.City.GetValue();
                        }
                        // Get value of State
                        if (PurchaseOrderRet.ShipAddress.State != null)
                        {
                            ShipAddressState841 = PurchaseOrderRet.ShipAddress.State.GetValue();
                        }
                        // Get value of PostalCode
                        if (PurchaseOrderRet.ShipAddress.PostalCode != null)
                        {
                            ShipAddressPostalCode842 = PurchaseOrderRet.ShipAddress.PostalCode.GetValue();
                        }
                        // Get value of Country
                        if (PurchaseOrderRet.ShipAddress.Country != null)
                        {
                            ShipAddressCountry843 = PurchaseOrderRet.ShipAddress.Country.GetValue();
                        }
                        // Get value of Note
                        if (PurchaseOrderRet.ShipAddress.Note != null)
                        {
                            ShipAddressNote844 = PurchaseOrderRet.ShipAddress.Note.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.ShipAddressBlock != null)
                    {
                        // Get value of Addr1
                        if (PurchaseOrderRet.ShipAddressBlock.Addr1 != null)
                        {
                            ShipAddressBlockAddr1845 = PurchaseOrderRet.ShipAddressBlock.Addr1.GetValue();
                        }
                        // Get value of Addr2
                        if (PurchaseOrderRet.ShipAddressBlock.Addr2 != null)
                        {
                            ShipAddressBlockAddr2846 = PurchaseOrderRet.ShipAddressBlock.Addr2.GetValue();
                        }
                        // Get value of Addr3
                        if (PurchaseOrderRet.ShipAddressBlock.Addr3 != null)
                        {
                            ShipAddressBlockAddr3847 = PurchaseOrderRet.ShipAddressBlock.Addr3.GetValue();
                        }
                        // Get value of Addr4
                        if (PurchaseOrderRet.ShipAddressBlock.Addr4 != null)
                        {
                            ShipAddressBlockAddr4848 = PurchaseOrderRet.ShipAddressBlock.Addr4.GetValue();
                        }
                        // Get value of Addr5
                        if (PurchaseOrderRet.ShipAddressBlock.Addr5 != null)
                        {
                            ShipAddressBlockAddr5849 = PurchaseOrderRet.ShipAddressBlock.Addr5.GetValue();
                        }
                    }
                    if (PurchaseOrderRet.TermsRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.TermsRef.ListID != null)
                        {
                            TermsRefListID850 = PurchaseOrderRet.TermsRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.TermsRef.FullName != null)
                        {
                            TermsRefFullName851 = PurchaseOrderRet.TermsRef.FullName.GetValue();
                        }
                    }
                    // Get value of DueDate
                    if (PurchaseOrderRet.DueDate != null)
                    {
                        DueDate852 = PurchaseOrderRet.DueDate.GetValue();
                    }
                    // Get value of ExpectedDate
                    if (PurchaseOrderRet.ExpectedDate != null)
                    {
                        ExpectedDate853 = PurchaseOrderRet.ExpectedDate.GetValue();
                    }
                    if (PurchaseOrderRet.ShipMethodRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.ShipMethodRef.ListID != null)
                        {
                            ShipMethodRefListID854 = PurchaseOrderRet.ShipMethodRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.ShipMethodRef.FullName != null)
                        {
                            ShipMethodRefFullName855 = PurchaseOrderRet.ShipMethodRef.FullName.GetValue();
                        }
                    }
                    // Get value of FOB
                    if (PurchaseOrderRet.FOB != null)
                    {
                        FOB856 = PurchaseOrderRet.FOB.GetValue();
                    }
                    // Get value of TotalAmount
                    if (PurchaseOrderRet.TotalAmount != null)
                    {
                        TotalAmount857 = PurchaseOrderRet.TotalAmount.GetValue();
                    }
                    if (PurchaseOrderRet.CurrencyRef != null)
                    {
                        // Get value of ListID
                        if (PurchaseOrderRet.CurrencyRef.ListID != null)
                        {
                            CurrencyRefListID858 = PurchaseOrderRet.CurrencyRef.ListID.GetValue();
                        }
                        // Get value of FullName
                        if (PurchaseOrderRet.CurrencyRef.FullName != null)
                        {
                            CurrencyRefFullName859 = PurchaseOrderRet.CurrencyRef.FullName.GetValue();
                        }
                    }
                    // Get value of ExchangeRate
                    if (PurchaseOrderRet.ExchangeRate != null)
                    {
                        ExchangeRate860 = PurchaseOrderRet.ExchangeRate.GetValue().ToString();
                    }
                    // Get value of TotalAmountInHomeCurrency
                    if (PurchaseOrderRet.TotalAmountInHomeCurrency != null)
                    {
                        TotalAmountInHomeCurrency861 = PurchaseOrderRet.TotalAmountInHomeCurrency.GetValue();
                    }
                    // Get value of IsManuallyClosed
                    if (PurchaseOrderRet.IsManuallyClosed != null)
                    {
                        IsManuallyClosed862 = PurchaseOrderRet.IsManuallyClosed.GetValue();
                    }
                    // Get value of IsFullyReceived
                    if (PurchaseOrderRet.IsFullyReceived != null)
                    {
                        IsFullyReceived863 = PurchaseOrderRet.IsFullyReceived.GetValue();
                    }
                    // Get value of Memo
                    if (PurchaseOrderRet.Memo != null)
                    {
                        Memo864 = PurchaseOrderRet.Memo.GetValue();
                    }
                    // Get value of VendorMsg
                    if (PurchaseOrderRet.VendorMsg != null)
                    {
                        VendorMsg865 = PurchaseOrderRet.VendorMsg.GetValue();
                    }
                    // Get value of IsToBePrinted
                    if (PurchaseOrderRet.IsToBePrinted != null)
                    {
                        IsToBePrinted866 = PurchaseOrderRet.IsToBePrinted.GetValue();
                    }
                    // Get value of IsToBeEmailed
                    if (PurchaseOrderRet.IsToBeEmailed != null)
                    {
                        IsToBeEmailed867 = PurchaseOrderRet.IsToBeEmailed.GetValue();
                    }
                    // Get value of Other1
                    if (PurchaseOrderRet.Other1 != null)
                    {
                        Other1868 = PurchaseOrderRet.Other1.GetValue();
                    }
                    // Get value of Other2
                    if (PurchaseOrderRet.Other2 != null)
                    {
                        Other2869 = PurchaseOrderRet.Other2.GetValue();
                    }
                    // Get value of ExternalGUID
                    if (PurchaseOrderRet.ExternalGUID != null)
                    {
                        ExternalGUID870 = PurchaseOrderRet.ExternalGUID.GetValue();
                    }

                    // 'get value of linked Transaction List
                    if (PurchaseOrderRet.LinkedTxnList != null)
                    {

                        int i871 = 0;
                        var loopTo1 = PurchaseOrderRet.LinkedTxnList.Count - 1;
                        for (i871 = 0; i871 <= loopTo1; i871++)
                        {
                            ILinkedTxn LinkedTxn;
                            LinkedTxn = PurchaseOrderRet.LinkedTxnList.GetAt(i871);

                            //string TxnID872 = Conversions.ToString("" == "");
                            string TxnID872 = ("" == "").ToString();
                            ENTxnType TxnType873;
                            DateTime TxnDate874;
                            string RefNumber875 = ("" == "").ToString();
                            double Amount877 = double.Parse("0 == 0");
                            // Get value of TxnID
                            TxnID872 = LinkedTxn.TxnID.GetValue();
                            // Get value of TxnType
                            TxnType873 = LinkedTxn.TxnType.GetValue();
                            // Get value of TxnDate
                            TxnDate874 = LinkedTxn.TxnDate.GetValue();
                            // Get value of RefNumber
                            if (LinkedTxn.RefNumber != null)
                            {
                                RefNumber875 = LinkedTxn.RefNumber.GetValue();
                            }
                            // Get value of LinkType
                            if (LinkedTxn.LinkType != null)
                            {
                                LinkType876 = LinkedTxn.LinkType.GetValue();
                            }
                            // Get value of Amount
                            Amount877 = LinkedTxn.Amount.GetValue();

                            db.PurchaseOrderLinkedTxn_Insert(TxnID803, TxnID872, ((int)TxnType873).ToString(), ((int)LinkType876).ToString(), TxnDate874, RefNumber875, (decimal?)Amount877, TxnID803, ClearAllControl.gblCompanyID);
                        }

                    }
                    // 'For Line Item List
                    if (PurchaseOrderRet.ORPurchaseOrderLineRetList != null)
                    {

                        int i878 = 0;
                        var loopTo2 = PurchaseOrderRet.ORPurchaseOrderLineRetList.Count - 1;
                        for (i878 = 0; i878 <= loopTo2; i878++)
                        {

                            IORPurchaseOrderLineRet ORPurchaseOrderLineRet879;

                            ORPurchaseOrderLineRet879 = PurchaseOrderRet.ORPurchaseOrderLineRetList.GetAt(i878);

                            if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet != null)
                            {
                                string TxnLineID880 = null;
                                string ItemRefListID881 = null;
                                string ItemRefFullName882 = null;
                                string ManufacturerPartNumber883 = null;
                                string Desc884 = null;
                                int Quantity885 = 0;
                                string UnitOfMeasure886 = null;
                                string OverrideUOMSetRefListID887 = null;
                                string OverrideUOMSetRefFullName888 = null;
                                double Rate889 = 0d;
                                string ClassRefListID890 = null;
                                string ClassRefFullName891 = null;
                                double Amount892 = 0d;
                                string CustomerRefListID893 = null;
                                string CustomerRefFullName894 = null;
                                int ReceivedQuantity896 = 0;
                                int UnbilledQuantity897 = 0;
                                bool IsBilled898 = false;
                                bool IsManuallyClosed899 = false;
                                string Other1900 = null;
                                string Other2901 = null;

                                // Get value of TxnLineID

                                TxnLineID880 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.TxnLineID.GetValue();
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef != null)
                                {
                                    // Get value of ListID
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.ListID != null)
                                    {
                                        ItemRefListID881 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.ListID.GetValue();
                                    }
                                    // Get value of FullName
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.FullName != null)
                                    {
                                        ItemRefFullName882 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ItemRef.FullName.GetValue();
                                    }
                                }

                                // Get value of ManufacturerPartNumber
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ManufacturerPartNumber != null)
                                {
                                    ManufacturerPartNumber883 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ManufacturerPartNumber.GetValue();
                                }
                                // Get value of Desc
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Desc != null)
                                {
                                    Desc884 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Desc.GetValue();
                                }
                                // Get value of Quantity
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Quantity != null)
                                {
                                    Quantity885 = (int)Math.Round(ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Quantity.GetValue());
                                }
                                // Get value of UnitOfMeasure
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnitOfMeasure != null)
                                {
                                    UnitOfMeasure886 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnitOfMeasure.GetValue();
                                }
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef != null)
                                {
                                    // Get value of ListID
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.ListID != null)
                                    {
                                        OverrideUOMSetRefListID887 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.ListID.GetValue();
                                    }
                                    // Get value of FullName
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.FullName != null)
                                    {
                                        OverrideUOMSetRefFullName888 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.OverrideUOMSetRef.FullName.GetValue();
                                    }
                                }
                                // Get value of Rate
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Rate != null)
                                {
                                    Rate889 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Rate.GetValue();
                                }
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef != null)
                                {
                                    // Get value of ListID
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.ListID != null)
                                    {
                                        ClassRefListID890 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.ListID.GetValue();
                                    }
                                    // Get value of FullName
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.FullName != null)
                                    {
                                        ClassRefFullName891 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ClassRef.FullName.GetValue();
                                    }
                                }

                                // Get value of Amount
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Amount != null)
                                {
                                    Amount892 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Amount.GetValue();
                                }
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef != null)
                                {
                                    // Get value of ListID
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.ListID != null)
                                    {
                                        CustomerRefListID893 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.ListID.GetValue();
                                    }
                                    // Get value of FullName
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.FullName != null)
                                    {
                                        CustomerRefFullName894 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.CustomerRef.FullName.GetValue();
                                    }

                                }
                                // Get value of ServiceDate
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ServiceDate != null)
                                {
                                    ServiceDate895 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ServiceDate.GetValue();
                                }
                                // Get value of ReceivedQuantity
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ReceivedQuantity != null)
                                {
                                    ReceivedQuantity896 = (int)Math.Round(ORPurchaseOrderLineRet879.PurchaseOrderLineRet.ReceivedQuantity.GetValue());
                                }
                                // Get value of UnbilledQuantity
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnbilledQuantity != null)
                                {
                                    UnbilledQuantity897 = (int)Math.Round(ORPurchaseOrderLineRet879.PurchaseOrderLineRet.UnbilledQuantity.GetValue());
                                }
                                // Get value of IsBilled
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsBilled != null)
                                {
                                    IsBilled898 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsBilled.GetValue();
                                }
                                // Get value of IsManuallyClosed
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsManuallyClosed != null)
                                {
                                    IsManuallyClosed899 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.IsManuallyClosed.GetValue();
                                }
                                // Get value of Other1
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other1 != null)
                                {
                                    Other1900 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other1.GetValue();
                                }
                                // Get value of Other2
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other2 != null)
                                {
                                    Other2901 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.Other2.GetValue();
                                }

                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.SalesTaxCodeRef != null)
                                {
                                    taxcode = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.SalesTaxCodeRef.FullName.GetValue();
                                }

                                // PurchaseOrderLineRet.DataExtRetList loop
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineRet.DataExtRetList != null)
                                {
                                    int i902 = 0;
                                    var loopTo3 = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.DataExtRetList.Count - 1;
                                    for (i902 = 0; i902 <= loopTo3; i902++)
                                    {

                                        IDataExtRet DataExtRet;
                                        DataExtRet = ORPurchaseOrderLineRet879.PurchaseOrderLineRet.DataExtRetList.GetAt(i902);
                                        // Get value of OwnerID
                                        string OwnerID903 = null;
                                        string DataExtName904 = null;
                                        ENDataExtType DataExtType905;
                                        string DataExtValue906 = null;
                                        if (DataExtRet.OwnerID != null)
                                        {
                                            OwnerID903 = DataExtRet.OwnerID.GetValue();
                                        }
                                        // Get value of DataExtName
                                        DataExtName904 = DataExtRet.DataExtName.GetValue();
                                        // Get value of DataExtType
                                        DataExtType905 = DataExtRet.DataExtType.GetValue();
                                        // Get value of DataExtValue
                                        DataExtValue906 = DataExtRet.DataExtValue.GetValue();
                                        db.PurchaseOrderDataExtRetList_Insert(OwnerID903, DataExtName904, ((int)DataExtType905).ToString(), DataExtValue906, "PurchaseOrderLineRet", TxnID803, ClearAllControl.gblCompanyID);
                                    }

                                }

                                db.PurchaseOrderLineItem_Insert(TxnLineID880, ItemRefListID881, ItemRefFullName882, ManufacturerPartNumber883, Desc884, Quantity885.ToString(), UnitOfMeasure886, OverrideUOMSetRefListID887, OverrideUOMSetRefFullName888, (decimal?)Rate889, ClassRefListID890, ClassRefFullName891, (decimal?)Amount892, CustomerRefListID893, CustomerRefFullName894, ServiceDate895, ReceivedQuantity896, UnbilledQuantity897, IsBilled898, IsManuallyClosed899, Other1900, Other2901, TxnID803, ClearAllControl.gblCompanyID, taxcode);


                            }
                            if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet != null)
                            {

                                // Get value of TxnLineID
                                string TxnLineID907 = null;
                                string ItemGroupRefListID908 = null;
                                string ItemGroupRefFullName909 = null;
                                string Desc910 = null;
                                int Quantity911 = 0;
                                string UnitOfMeasure912 = null;
                                string OverrideUOMSetRefListID913 = null;
                                string OverrideUOMSetRefFullName914 = null;
                                bool IsPrintItemsInGroup915 = false;
                                double TotalAmount916 = 0d;

                                TxnLineID907 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.TxnLineID.GetValue();
                                // Get value of ListID
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.ListID != null)
                                {
                                    ItemGroupRefListID908 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.ListID.GetValue();
                                }
                                // Get value of FullName
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.FullName != null)
                                {
                                    ItemGroupRefFullName909 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.ItemGroupRef.FullName.GetValue();
                                }
                                // Get value of Desc
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Desc != null)
                                {
                                    Desc910 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Desc.GetValue();
                                }
                                // Get value of Quantity
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Quantity != null)
                                {
                                    Quantity911 = (int)Math.Round(ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.Quantity.GetValue());
                                }
                                // Get value of UnitOfMeasure
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.UnitOfMeasure != null)
                                {
                                    UnitOfMeasure912 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.UnitOfMeasure.GetValue();
                                }
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef != null)
                                {
                                    // Get value of ListID
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.ListID != null)
                                    {
                                        OverrideUOMSetRefListID913 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.ListID.GetValue();
                                    }
                                    // Get value of FullName
                                    if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.FullName != null)
                                    {
                                        OverrideUOMSetRefFullName914 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.OverrideUOMSetRef.FullName.GetValue();
                                    }
                                }
                                // Get value of IsPrintItemsInGroup
                                IsPrintItemsInGroup915 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.IsPrintItemsInGroup.GetValue();
                                // Get value of TotalAmount
                                TotalAmount916 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.TotalAmount.GetValue();


                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.PurchaseOrderLineRetList != null)
                                {

                                    int i917 = 0;
                                    var loopTo4 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.PurchaseOrderLineRetList.Count - 1;
                                    for (i917 = 0; i917 <= loopTo4; i917++)
                                    {
                                        IPurchaseOrderLineRet PurchaseOrderLineRet;
                                        PurchaseOrderLineRet = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.PurchaseOrderLineRetList.GetAt(i917);

                                        string TxnLineID918 = ("" == "").ToString();
                                        string ItemRefListID919 = null;
                                        string ItemRefFullName920 = null;
                                        string ManufacturerPartNumber921 = null;
                                        string Desc922 = null;
                                        int Quantity923 = 0;
                                        string UnitOfMeasure924 = null;
                                        string OverrideUOMSetRefListID925 = null;
                                        string OverrideUOMSetRefFullName926 = null;
                                        double Rate927 = 0d;
                                        string ClassRefListID928 = null;
                                        string ClassRefFullName929 = null;
                                        double Amount930 = 0d;
                                        string CustomerRefListID931 = null;
                                        string CustomerRefFullName932 = null;
                                        int ReceivedQuantity934 = 0;
                                        int UnbilledQuantity935 = 0;
                                        bool IsBilled936 = false;
                                        bool IsManuallyClosed937 = false;
                                        string Other1938 = null;
                                        string Other2939 = null;
                                        TxnLineID918 = PurchaseOrderLineRet.TxnLineID.GetValue();
                                        if (PurchaseOrderLineRet.ItemRef != null)
                                        {
                                            // Get value of ListID
                                            if (PurchaseOrderLineRet.ItemRef.ListID != null)
                                            {
                                                ItemRefListID919 = PurchaseOrderLineRet.ItemRef.ListID.GetValue();
                                            }
                                            // Get value of FullName
                                            if (PurchaseOrderLineRet.ItemRef.FullName != null)
                                            {
                                                ItemRefFullName920 = PurchaseOrderLineRet.ItemRef.FullName.GetValue();
                                            }
                                        }
                                        // Get value of ManufacturerPartNumber
                                        if (PurchaseOrderLineRet.ManufacturerPartNumber != null)
                                        {
                                            ManufacturerPartNumber921 = PurchaseOrderLineRet.ManufacturerPartNumber.GetValue();
                                        }
                                        // Get value of Desc
                                        if (PurchaseOrderLineRet.Desc != null)
                                        {
                                            Desc922 = PurchaseOrderLineRet.Desc.GetValue();
                                        }
                                        // Get value of Quantity
                                        if (PurchaseOrderLineRet.Quantity != null)
                                        {
                                            Quantity923 = (int)Math.Round(PurchaseOrderLineRet.Quantity.GetValue());
                                        }
                                        // Get value of UnitOfMeasure
                                        if (PurchaseOrderLineRet.UnitOfMeasure != null)
                                        {
                                            UnitOfMeasure924 = PurchaseOrderLineRet.UnitOfMeasure.GetValue();
                                        }
                                        if (PurchaseOrderLineRet.OverrideUOMSetRef != null)
                                        {
                                            // Get value of ListID
                                            if (PurchaseOrderLineRet.OverrideUOMSetRef.ListID != null)
                                            {
                                                OverrideUOMSetRefListID925 = PurchaseOrderLineRet.OverrideUOMSetRef.ListID.GetValue();
                                            }
                                            // Get value of FullName
                                            if (PurchaseOrderLineRet.OverrideUOMSetRef.FullName != null)
                                            {
                                                OverrideUOMSetRefFullName926 = PurchaseOrderLineRet.OverrideUOMSetRef.FullName.GetValue();
                                            }

                                        }
                                        // Get value of Rate
                                        if (PurchaseOrderLineRet.Rate != null)
                                        {
                                            Rate927 = PurchaseOrderLineRet.Rate.GetValue();
                                        }
                                        if (PurchaseOrderLineRet.ClassRef != null)
                                        {
                                            // Get value of ListID
                                            if (PurchaseOrderLineRet.ClassRef.ListID != null)
                                            {
                                                ClassRefListID928 = PurchaseOrderLineRet.ClassRef.ListID.GetValue();
                                            }
                                            // Get value of FullName
                                            if (PurchaseOrderLineRet.ClassRef.FullName != null)
                                            {
                                                ClassRefFullName929 = PurchaseOrderLineRet.ClassRef.FullName.GetValue();
                                            }

                                        }
                                        // Get value of Amount
                                        if (PurchaseOrderLineRet.Amount != null)
                                        {
                                            Amount930 = PurchaseOrderLineRet.Amount.GetValue();
                                        }
                                        if (PurchaseOrderLineRet.CustomerRef != null)
                                        {
                                            // Get value of ListID
                                            if (PurchaseOrderLineRet.CustomerRef.ListID != null)
                                            {
                                                CustomerRefListID931 = PurchaseOrderLineRet.CustomerRef.ListID.GetValue();
                                            }
                                            // Get value of FullName
                                            if (PurchaseOrderLineRet.CustomerRef.FullName != null)
                                            {
                                                CustomerRefFullName932 = PurchaseOrderLineRet.CustomerRef.FullName.GetValue();
                                            }
                                        }
                                        // Get value of ServiceDate
                                        if (PurchaseOrderLineRet.ServiceDate != null)
                                        {
                                            ServiceDate933 = PurchaseOrderLineRet.ServiceDate.GetValue();
                                        }
                                        // Get value of ReceivedQuantity
                                        if (PurchaseOrderLineRet.ReceivedQuantity != null)
                                        {
                                            ReceivedQuantity934 = (int)Math.Round(PurchaseOrderLineRet.ReceivedQuantity.GetValue());
                                        }
                                        // Get value of UnbilledQuantity
                                        if (PurchaseOrderLineRet.UnbilledQuantity != null)
                                        {
                                            UnbilledQuantity935 = (int)Math.Round(PurchaseOrderLineRet.UnbilledQuantity.GetValue());
                                        }
                                        // Get value of IsBilled
                                        if (PurchaseOrderLineRet.IsBilled != null)
                                        {
                                            IsBilled936 = PurchaseOrderLineRet.IsBilled.GetValue();
                                        }
                                        // Get value of IsManuallyClosed
                                        if (PurchaseOrderLineRet.IsManuallyClosed != null)
                                        {
                                            IsManuallyClosed937 = PurchaseOrderLineRet.IsManuallyClosed.GetValue();
                                        }
                                        // Get value of Other1
                                        if (PurchaseOrderLineRet.Other1 != null)
                                        {
                                            Other1938 = PurchaseOrderLineRet.Other1.GetValue();
                                        }
                                        // Get value of Other2
                                        if (PurchaseOrderLineRet.Other2 != null)
                                        {
                                            Other2939 = PurchaseOrderLineRet.Other2.GetValue();
                                        }
                                        if (PurchaseOrderLineRet.DataExtRetList != null)
                                        {

                                            int i940 = 0;
                                            var loopTo5 = PurchaseOrderLineRet.DataExtRetList.Count - 1;
                                            for (i940 = 0; i940 <= loopTo5; i940++)
                                            {

                                                IDataExtRet DataExtRet;
                                                DataExtRet = PurchaseOrderLineRet.DataExtRetList.GetAt(i940);
                                                // Get value of OwnerID
                                                string OwnerID941 = null;
                                                string DataExtName942 = null;
                                                ENDataExtType DataExtType943;
                                                string DataExtValue944 = null;
                                                if (DataExtRet.OwnerID != null)
                                                {
                                                    OwnerID941 = DataExtRet.OwnerID.GetValue();
                                                }
                                                // Get value of DataExtName
                                                DataExtName942 = DataExtRet.DataExtName.GetValue();
                                                // Get value of DataExtType
                                                DataExtType943 = DataExtRet.DataExtType.GetValue();
                                                // Get value of DataExtValue
                                                DataExtValue944 = DataExtRet.DataExtValue.GetValue();
                                                db.PurchaseOrderDataExtRetList_Insert(OwnerID941, DataExtName942, ((int)DataExtType943).ToString(), DataExtValue944, "PurchaseOrderLineRet", TxnID803, ClearAllControl.gblCompanyID);
                                            }
                                            // 'groupLineItem Insert
                                            db.PurchaseOrderLineItem_Insert(TxnLineID918, ItemRefListID919, ItemRefFullName920, ManufacturerPartNumber921, Desc922, Quantity923.ToString(), UnitOfMeasure924, OverrideUOMSetRefListID925, OverrideUOMSetRefFullName926, (decimal?)Rate927, ClassRefListID928, ClassRefFullName929, (decimal?)Amount930, CustomerRefListID931, CustomerRefFullName932, ServiceDate933, ReceivedQuantity934, UnbilledQuantity935, IsBilled936, IsManuallyClosed937, Other1938, Other2939, TxnID803, ClearAllControl.gblCompanyID, taxcode);

                                        }

                                    }

                                }
                                if (ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.DataExtRetList != null)
                                {

                                    int i945 = 0;
                                    var loopTo6 = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.DataExtRetList.Count - 1;
                                    for (i945 = 0; i945 <= loopTo6; i945++)
                                    {

                                        IDataExtRet DataExtRet;
                                        DataExtRet = ORPurchaseOrderLineRet879.PurchaseOrderLineGroupRet.DataExtRetList.GetAt(i945);
                                        // Get value of OwnerID
                                        string OwnerID946 = null;
                                        if (DataExtRet.OwnerID != null)
                                        {
                                            OwnerID946 = DataExtRet.OwnerID.GetValue();
                                        }
                                        // Get value of DataExtName
                                        string DataExtName947 = null;
                                        DataExtName947 = DataExtRet.DataExtName.GetValue();
                                        // Get value of DataExtType
                                        ENDataExtType DataExtType948;
                                        DataExtType948 = DataExtRet.DataExtType.GetValue();
                                        // Get value of DataExtValue
                                        string DataExtValue949 = null;
                                        DataExtValue949 = DataExtRet.DataExtValue.GetValue();
                                        db.PurchaseOrderDataExtRetList_Insert(OwnerID946, DataExtName947, ((int)DataExtType948).ToString(), DataExtValue949, "PurchaseOrderLineGroupRet", TxnID803, ClearAllControl.gblCompanyID);
                                    }

                                }
                                // PurchaseOrderLineGroupRet
                                db.PurchaseOrderLineGroupItem_Insert(TxnLineID907, ItemGroupRefListID908, ItemGroupRefFullName909, Desc910, Quantity911, UnitOfMeasure912, OverrideUOMSetRefListID913, OverrideUOMSetRefFullName914, IsPrintItemsInGroup915, (decimal?)TotalAmount916, TxnID803, ClearAllControl.gblCompanyID);
                            }
                        }
                    }


                    if (PurchaseOrderRet.DataExtRetList != null)
                    {

                        int i950 = 0;
                        var loopTo7 = PurchaseOrderRet.DataExtRetList.Count - 1;
                        for (i950 = 0; i950 <= loopTo7; i950++)
                        {

                            IDataExtRet DataExtRet;
                            DataExtRet = PurchaseOrderRet.DataExtRetList.GetAt(i950);
                            string OwnerID951 = null;
                            // Get value of OwnerID
                            if (DataExtRet.OwnerID != null)
                            {


                                OwnerID951 = DataExtRet.OwnerID.GetValue();

                            }
                            // Get value of DataExtName
                            string DataExtName952 = null;
                            DataExtName952 = DataExtRet.DataExtName.GetValue();
                            // Get value of DataExtType
                            ENDataExtType DataExtType953;
                            DataExtType953 = DataExtRet.DataExtType.GetValue();
                            // Get value of DataExtValue
                            string DataExtValue954 = null;
                            DataExtValue954 = DataExtRet.DataExtValue.GetValue();
                            db.PurchaseOrderDataExtRetList_Insert(OwnerID951, DataExtName952, ((int)DataExtType953).ToString(), DataExtValue954, "PurchaseOrderRet", TxnID803, ClearAllControl.gblCompanyID);

                        }

                    }

                    // Main table insert
                    db.QBPurchaseOrder_Insert(TxnID803, TimeCreated804, TimeModified805, EditSequence806, TxnNumber807, VendorRefListID808, VendorRefFullName809, ClassRefListID810, ClassRefFullName811, InventorySiteRefListID812, InventorySiteRefFullName813, ShipToEntityRefListID814, ShipToEntityRefFullName815, TemplateRefListID816, TemplateRefFullName817, TxnDate818, RefNumber819, VendorAddressAddr1820, VendorAddressAddr2821, VendorAddressAddr3822, VendorAddressAddr4823, VendorAddressAddr5824, VendorAddressCity825, VendorAddressState826, VendorAddressPostalCode827, VendorAddressCountry828, VendorAddressNote829, VendorAddressBlockAddr1830, VendorAddressBlockAddr2831, VendorAddressBlockAddr3832, VendorAddressBlockAddr4833, VendorAddressBlockAddr5834, ShipAddressAddr1835, ShipAddressAddr2836, ShipAddressAddr3837, ShipAddressAddr4838, ShipAddressAddr5839, ShipAddressCity840, ShipAddressState841, ShipAddressPostalCode842, ShipAddressCountry843, ShipAddressNote844, ShipAddressBlockAddr1845, ShipAddressBlockAddr2846, ShipAddressBlockAddr3847, ShipAddressBlockAddr4848, ShipAddressBlockAddr5849, TermsRefListID850, TermsRefFullName851, DueDate852, ExpectedDate853, ShipMethodRefListID854, ShipMethodRefFullName855, FOB856, (decimal?)TotalAmount857, CurrencyRefListID858, CurrencyRefFullName859, ExchangeRate860, (decimal?)TotalAmountInHomeCurrency861, IsManuallyClosed862, IsFullyReceived863, Memo864, VendorMsg865, IsToBePrinted866, IsToBeEmailed867, Other1868, Other2869, ExternalGUID870, ClearAllControl.gblCompanyID);

                }

                return;
            Errs:
                ;
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

        private void PopulateTempleteCombobox()
        {
            tmpltPoCmbx.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Purchase Order' and Status='True' ");
            if (sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    tmpltPoCmbx.Items.Add(r["TemplateName"]);
                tmpltPoCmbx.SelectedIndex = 0;
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

        private void LoadVendors(int cID)
        {
            try
            {
                cmbxPoVendor.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.execquery("SELECT distinct VendorRefFullName from QBPurchaseOrder where companyID=@CompanyID");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxPoVendor.Items.Add(r["VendorRefFullName"]);
                    cmbxPoVendor.SelectedIndex = -1;
                }

                //Need to add the AutoCompleteString Colleection
                //var col = new AutoCompleteStringCollection();
            }
            catch (Exception ex)
            {

            }
        }

        private void loadPONumber(string vend, int cID)
        {
            try
            {
                cmbxPoVendor.SelectedIndex = -1;
                cmbxPoVendor.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.addparam("@VendorName", vend);
                sql.execquery("SELECT distinct RefNumber,TxnId  from QBPurchaseOrder where companyID=@CompanyID and VendorRefFullName=@VendorName order by RefNumber");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxPoVendor.Items.Add(r["RefNumber"]);
                    cmbxPoVendor.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion


        #region Events

        private void btnPoSynAl_Click(object sender, RoutedEventArgs e)
        {
            //PopulateTempleteCombobox();
            try
            {
                bError = false;
                connectToQB();
                //sessionManager.openConnection();
                GetPOTransactionNoDate(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Purchase Order Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Purchase Order Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }
                LoadVendors(ClearAllControl.gblCompanyID);
                //sessionManager.closeConnection();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnPoSyn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bError = false;
                connectToQB();
                //sessionManager.openConnection();
                GetPOTransaction(ref bError, dpFrmDate.DisplayDate, dpToDate.DisplayDate);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Synced successfully", Haley.Enums.NotificationIcon.Success);
                }
                LoadVendors(ClearAllControl.gblCompanyID);
                //sessionManager.closeConnection();
            }
            catch (Exception ex)
            {

            }
        }


        private void tmpltPoCmbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempPath(tmpltPoCmbx.Text);
        }


        private void cmbxPoVendor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadPONumber(cmbxPoVendor.Text, ClearAllControl.gblCompanyID);
        }

        private void btnPoPrint_Click(object sender, RoutedEventArgs e)
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
                {
                    ref var withBlock = ref _ReportDocument;
                    //string startuppatah = Application.GetResourceStream();
                    withBlock.Load(path + tmpltPoCmbx.Text + ".rpt");
                }

                _ReportDocument.ReportOptions.EnableSaveDataWithReport = false;
                crConnectionInfo.ServerName = ClearAllControl.gblSQLServerName;
                crConnectionInfo.DatabaseName = ClearAllControl.gblDatabaseName;
                crConnectionInfo.UserID = ClearAllControl.gblSQLServerUserName;
                crConnectionInfo.Password = ClearAllControl.gblSQLServerPassword;
                crConnectionInfo.AllowCustomConnection = false;
                crConnectionInfo.IntegratedSecurity = false;

                CrTables = _ReportDocument.Database.Tables;

                foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in CrTables)
                {
                    crtableLogoninfo = CrTable.LogOnInfo;
                    crtableLogoninfo.ConnectionInfo = crConnectionInfo;
                    crtableLogoninfo.ReportName = _ReportDocument.Name;
                    crtableLogoninfo.TableName = CrTable.Name;
                    CrTable.ApplyLogOnInfo(crtableLogoninfo);
                }

                crParameterDiscreteValue.Value = tmpltPoCmbx.Text;
                crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
                crParameterFieldDefinition = crParameterFieldDefinitions["@PONumber"];
                crParameterValues = crParameterFieldDefinition.CurrentValues;
                crParameterValues.Clear();
                crParameterValues.Add(crParameterDiscreteValue);
                crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

                crParameterDiscreteValue.Value = ClearAllControl.gblCompanyID;
                crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
                crParameterFieldDefinition = crParameterFieldDefinitions["@companyID"];
                crParameterValues = crParameterFieldDefinition.CurrentValues;
                crParameterValues.Clear();
                crParameterValues.Add(crParameterDiscreteValue);
                crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

                //IF _ReporDocument.HasRecords Then
                var rpt = new ReportView();
                rpt.ShowReportView(ref _ReportDocument);

            }
            catch (Exception ex)
            {
                ds.ShowDialog(ex.Message, "TNC-Sync", Haley.Enums.NotificationIcon.Error);
            }
        }


        #endregion

        private void PO_Loaded(object sender, RoutedEventArgs e)
        {
            //PopulateTempleteCombobox();
        }
    }
}
