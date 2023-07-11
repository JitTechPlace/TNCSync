using Haley.Abstractions;
using Haley.MVVM;
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
using Interop.QBFC15;
//using Interop.QBFC12;
using TNCSync.Class;
using TNCSync.Class.DataBaseClass;
using TNCSync.Sessions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for Receipt.xaml
    /// </summary>
    public partial class Receipt : UserControl
    {
        private bool bError;
        private string path = null;
        private SQLControls sql = new SQLControls();
        private bool booSessionBegun;
        public Receipt()
        {
            InitializeComponent();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
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

        #region ReceiptsMethod
        public void GetReceivePayments(ref bool bError)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgSetRequest = sessionManager.getMsgSetRequest();
                msgSetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                //Add the Bill query request
                IReceivePaymentQuery receivePayment = msgSetRequest.AppendReceivePaymentQueryRq();
                receivePayment.IncludeLineItems.SetValue(true);
                DateTime txnReptDate;

                bool bDone = false;
                while (!bDone)
                {
                    // start looking for customer next in the list

                    // send the request to QB
                    IMsgSetResponse msgSetResponse = sessionManager.doRequest(true, ref msgSetRequest);

                    // MsgBox(msgSetRequest.ToXMLString())
                    FillReceivePaymentInDataBase(ref msgSetResponse, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void GetReceivePayment(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgSetRequest = sessionManager.getMsgSetRequest();
                msgSetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                //Add the Bill query request
                IReceivePaymentQuery receivePayment = msgSetRequest.AppendReceivePaymentQueryRq();
                receivePayment.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                receivePayment.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);
                receivePayment.IncludeLineItems.SetValue(true);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgSetRequest);

                    FillReceivePaymentInDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void FillReceivePaymentInDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
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
                IReceivePaymentRetList receivePaymentRetList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtReceivePaymentQueryRs & responseDetailType == ENObjectType.otReceivePaymentRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    receivePaymentRetList = (IReceivePaymentRetList)response.Detail;
                }
                else
                {
                    // bail, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                // Parse the query response and add the Customers to the Customer list box
                var count = default(short);
                short index;
                IReceivePaymentRet receivePaymetRet;

                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + receivePaymentRetList.Count);
                // we are done with the customerQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }
                db.tblReceivePayment_Delete(ClearAllControl.gblCompanyID.ToString());
                db.tblReceiveLine_delete();
                count = (short)receivePaymentRetList.Count;
                var TimeCreated = default(DateTime);
                var TimeModified = default(DateTime);
                var linetxnDate = default(DateTime);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    if (index == 106)
                    {
                        int w = 0;
                    }

                    // skip this customer if this is a repeat from the last query
                    receivePaymetRet = receivePaymentRetList.GetAt(index);
                    if (receivePaymetRet is null | receivePaymetRet.CustomerRef.ListID is null | receivePaymetRet.TxnID is null)

                    {
                        bDone = true;
                        bError = true;
                        return;
                    }

                    string ARAccountRefListID = string.Empty;
                    string ARAccountFullName = string.Empty;
                    if (receivePaymetRet.ARAccountRef.ListID != null)
                    {
                        ARAccountRefListID = receivePaymetRet.ARAccountRef.ListID.GetValue();
                        ARAccountFullName = receivePaymetRet.ARAccountRef.FullName.GetValue();
                    }

                    string CurrencyRefListID = string.Empty;
                    string CurrencyRefFullName = string.Empty;
                    if (receivePaymetRet.CurrencyRef != null)
                    {
                        CurrencyRefListID = receivePaymetRet.CurrencyRef.ListID.GetValue();
                        CurrencyRefFullName = receivePaymetRet.CurrencyRef.FullName.GetValue();
                    }
                    string CustomerRefListID = string.Empty;
                    string CustomerRefFullName = string.Empty;
                    if (receivePaymetRet.CustomerRef != null)
                    {
                        CustomerRefListID = receivePaymetRet.CustomerRef.ListID.GetValue();
                        CustomerRefFullName = receivePaymetRet.CustomerRef.FullName.GetValue();
                    }
                    string DepositToAccountRefListID = string.Empty;
                    string DepositeToAccountRefFullName = string.Empty;
                    if (receivePaymetRet.DepositToAccountRef != null)
                    {
                        DepositToAccountRefListID = receivePaymetRet.DepositToAccountRef.ListID.GetValue();
                        DepositeToAccountRefFullName = receivePaymetRet.DepositToAccountRef.FullName.GetValue();
                    }
                    string EditSequence = string.Empty;
                    if (receivePaymetRet.EditSequence != null)
                    {
                        EditSequence = receivePaymetRet.EditSequence.GetValue();
                    }
                    string ExchangeRate = string.Empty;
                    if (receivePaymetRet.ExchangeRate != null)
                    {
                        ExchangeRate = receivePaymetRet.ExchangeRate.GetValue().ToString();
                    }
                    string ExternalGUID = string.Empty;
                    if (receivePaymetRet.ExternalGUID != null)
                    {
                        ExternalGUID = receivePaymetRet.ExternalGUID.GetValue();
                    }
                    //string RefNumber = string.Empty;
                    if (receivePaymetRet.RefNumber != null)
                    {
                        RefNumber = receivePaymetRet.RefNumber.GetValue();
                    }
                    if (RefNumber == "1799")
                    {
                        int k = 9;
                    }

                    string memo = string.Empty;
                    if (receivePaymetRet.Memo != null)
                    {
                        memo = receivePaymetRet.Memo.GetValue();
                    }
                    string PaymentMethodRefListID = string.Empty;
                    string PaymentMethodRefFullName = string.Empty;
                    if (receivePaymetRet.PaymentMethodRef != null)
                    {
                        PaymentMethodRefListID = receivePaymetRet.PaymentMethodRef.ListID.GetValue();
                        PaymentMethodRefFullName = receivePaymetRet.PaymentMethodRef.FullName.GetValue();
                    }
                    if (receivePaymetRet.TimeCreated != null)
                    {
                        TimeCreated = receivePaymetRet.TimeCreated.GetValue();
                    }
                    if (receivePaymetRet.TimeModified != null)
                    {
                        TimeModified = receivePaymetRet.TimeModified.GetValue();
                    }
                    decimal TotalAmount = 0m;
                    if (receivePaymetRet.TotalAmount != null)
                    {
                        TotalAmount = (decimal)receivePaymetRet.TotalAmount.GetValue();
                    }
                    decimal TotalAmountInHOmeCurrency = 0m;
                    if (receivePaymetRet.TotalAmountInHomeCurrency != null)
                    {
                        TotalAmountInHOmeCurrency = (decimal)receivePaymetRet.TotalAmountInHomeCurrency.GetValue();
                    }
                    string TxnDate = string.Empty;
                    if (receivePaymetRet.TxnDate != null)
                    {
                        TxnDate = receivePaymetRet.TxnDate.GetValue().ToString();
                    }
                    string TxnId = string.Empty;
                    if (receivePaymetRet.TxnID != null)
                    {
                        TxnId = receivePaymetRet.TxnID.GetValue();
                    }
                    string TxnNumber = string.Empty;
                    if (receivePaymetRet.TxnNumber != null)
                    {
                        TxnNumber = receivePaymetRet.TxnNumber.GetValue().ToString();
                    }
                    string Type = string.Empty;
                    if (receivePaymetRet.Type != null)
                    {
                        Type = receivePaymetRet.Type.GetValue().ToString();
                    }
                    decimal UnusedCredit = 0m;
                    if (receivePaymetRet.UnusedCredits != null)
                    {
                        UnusedCredit = (decimal)receivePaymetRet.UnusedCredits.GetValue();
                    }
                    string UnUsedPayment = string.Empty;
                    if (receivePaymetRet.UnusedPayment != null)
                    {
                        UnUsedPayment = receivePaymetRet.UnusedPayment.GetValue().ToString();
                    }
                    string newmemo = string.Empty;
                    string checkDate = string.Empty;
                    string checkNumber = string.Empty;
                    string customerBankName = string.Empty;
                    string compaireDate = "10/04/2015";
                    object tempString;
                    DateTime tempDate = DateTime.Parse(TxnDate);
                    int countStr;
                    string cashCustomerName;
                    string memoString = string.Empty;
                    tempString = (tempDate, "dd/MM/yyy").ToString();
                    //if (PaymentMethodRefFullName == "Cheque")
                    //{

                    //    tempString = Strings.Split(memo, ":");
                    //    countStr = CountCharacter(memo, ':');
                    //    if (countStr >= 2)
                    //    {
                    //        checkDate = Conversions.ToString(tempString((object)1));
                    //        checkDate = tempString((object)1).Tostring();
                    //        checkNumber = Conversions.ToString(tempString((object)0));
                    //        customerBankName = Conversions.ToString(tempString((object)2));

                    //        for (int j = 3, loopTo1 = countStr; j <= loopTo1; j++)
                    //            memoString = Operators.AddObject(memoString + " ", tempString((object)j)).ToString();
                    //    }

                    //    else
                    //    {
                    //        memoString = memo;
                    //        tempString = Strings.Split(memoString, ":");
                    //        countStr = CountCharacter(memo, ':');
                    //        if (countStr > 0)
                    //        {
                    //            cashCustomerName = Conversions.ToString(tempString((object)1));
                    //            memoString = Conversions.ToString(tempString((object)0));
                    //        }
                    //    }
                    //}
                    //else if (PaymentMethodRefFullName == "Check")
                    //{
                    //    tempString = Strings.Split(memo, ":");
                    //    countStr = CountCharacter(memo, ':');
                    //    if (countStr > 2)
                    //    {
                    //        checkDate = Conversions.ToString(tempString((object)1));
                    //        checkNumber = Conversions.ToString(tempString((object)0));
                    //        customerBankName = Conversions.ToString(tempString((object)2));

                    //        for (int j = 3, loopTo2 = countStr; j <= loopTo2; j++)
                    //            memoString = Conversions.ToString(Operators.AddObject(memoString + " ", tempString((object)j)));
                    //    }
                    //    else
                    //    {
                    //        memoString = memo;
                    //        tempString = Strings.Split(memoString, ":");
                    //        countStr = CountCharacter(memo, ':');
                    //        if (countStr > 0)
                    //        {
                    //            cashCustomerName = Conversions.ToString(tempString((object)1));
                    //            memoString = Conversions.ToString(tempString((object)0));
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    memoString = memo;
                    //    // tempString = Split(memoString, ":")
                    //    // countStr = CountCharacter(memo, ":")
                    //    // If countStr > 0 Then
                    //    // cashCustomerName = tempString(1)
                    //    // memoString = tempString(0)
                    //    // End If
                    //    // memoString = memoString
                    //}
                    // Insert data in database code left..........

                    // db.tblInvoice_Insert(gblCompanyID, TxnID, CustomerRefKey, Nothing, ClassRefKey, ARAccountRefKey, TemplateRefKey, TxnDate, RefNumber, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillAddress5, BillAddressCity, BillAddressState, BillAddressPostalCode, BillAddressCountry, BillAddressNote, ShipAddress1, ShipAddress2, ShipAddress3, ShipAddress4, ShipAddress5, ShipAddressCity, ShipAddressState, ShipAddressPostalCode, ShipAddressCountry, ShipAddressNote, IsPending, PONumber, TermsRefKey, DueDate, SalesRefKey, FOB, ShipDate, ShipMethodRefKey, ItemSalesTaxRefKey, Memo, CustomerMsgRefKey, IsToBePrinted, IsToEmailed, IsTaxIncluded, CustomerSalesTaxCodeRefKey, Other, Amount, AmountPaid, CustomField1, CustomField2, CustomField3, CustomField4, CustomField5, QuotationRecNo, GradeID, InvoiceType, customerName, subTotal, SalesTaxTotal, SalesTaxPercentage)

                    db.tblReceivePayment_Insert(ARAccountRefListID, ARAccountFullName, CurrencyRefListID, CurrencyRefFullName, CustomerRefListID, CustomerRefFullName, DepositToAccountRefListID, DepositeToAccountRefFullName, EditSequence, ExchangeRate, ExternalGUID, memoString, PaymentMethodRefListID, PaymentMethodRefFullName, RefNumber, TimeCreated, TimeModified, TotalAmount, TotalAmountInHOmeCurrency, DateTime.Parse(TxnDate), TxnId, TxnNumber, Type, UnusedCredit, decimal.Parse(UnUsedPayment), ClearAllControl.gblCompanyID.ToString(), checkDate, checkNumber, customerBankName);
                    cashCustomerName = string.Empty;
                    IAppliedToTxnRet appliedTxnRet;
                    // ' If Not CheckBillRet.AppliedToTxnRetList Is Nothing Then

                    if (receivePaymetRet.AppliedToTxnRetList != null)
                    {
                        // 'appliedTxnRet = CheckBillRet.AppliedToTxnRetList.GetAt(j)
                        for (int k = 0, loopTo3 = receivePaymetRet.AppliedToTxnRetList.Count - 1; k <= loopTo3; k++)
                        {
                            IAppliedToTxnRet AppTxntRetList;
                            AppTxntRetList = receivePaymetRet.AppliedToTxnRetList.GetAt(k);
                            string lineRefNumber = string.Empty;
                            if (AppTxntRetList.RefNumber != null)
                            {
                                lineRefNumber = AppTxntRetList.RefNumber.GetValue();
                            }
                            if (AppTxntRetList.TxnDate != null)
                            {
                                linetxnDate = AppTxntRetList.TxnDate.GetValue();
                            }

                            double lineAmount = 0d;
                            if (AppTxntRetList.Amount != null)
                            {
                                lineAmount = AppTxntRetList.Amount.GetValue();
                            }

                            double lineBalance = 0d;
                            if (AppTxntRetList.BalanceRemaining != null)
                            {
                                lineBalance = AppTxntRetList.BalanceRemaining.GetValue();
                            }
                            db.tblReceiveLine_insert(TxnId, lineRefNumber, linetxnDate, (decimal?)lineAmount, (decimal?)lineBalance);
                        }

                    }

                }

                return;
            }
            catch
            {
                ds.ShowDialog("", "Error to Fill Receipt Table Database", Haley.Enums.NotificationIcon.Error);
                bDone = true;
                bError = true;
            }
        }


        public int CountCharacter(string value, char ch)
        {
            int cnt = 0;
            foreach (char c in value)
            {
                if (c == ch)
                    cnt += 1;
            }
            return cnt;
        }
        #endregion

        #region Methods
        #endregion


        #region Events

        #endregion


    }
}
