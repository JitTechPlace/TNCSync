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
using TNCSync.Class;
using TNCSync.Class.DataBaseClass;
using TNCSync.Sessions;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
//using Interop.QBFC15;
using Interop.QBFC16;
using System.Data;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for JournalVoucher.xaml
    /// </summary>
    public partial class JournalVoucher : UserControl
    {
        private bool bError;
        private string path = null;
        private SQLControls sql = new SQLControls();
        private bool booSessionBegun;
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService ds;
        SessionManager sessionManager;
        private short maxVersion;
        private string payEntityRef = null;
        private DateTime? txnDate = default;
        private DateTime? timeCreated = default;
        private string memo = null;
        private decimal amount = 0m;
        private string accountRef = null;
        private string txnID = null;
        private string addressBlock = null;

        public JournalVoucher()
        {
            InitializeComponent();

            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            PopulateTempleteCombobox();
            LoadJVNumber();
        }

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


        #region JV Methods
        public void GetJournalTransaction(ref bool bError)
        {
            try
            {
                // make sure we do not have any old requests still defined
                IMsgSetRequest msgSetRequest = sessionManager.getMsgSetRequest();
                msgSetRequest.ClearRequests();
                // set the OnError attribute to continueOnError
                msgSetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                // Add the BillQuery request
                IJournalEntryQuery JournalQuery;
                JournalQuery = msgSetRequest.AppendJournalEntryQueryRq();
                // CheckQuery.IncludeLinkedTxns.SetValue(True)
                JournalQuery.IncludeLineItems.SetValue(true);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse msgSetResponse = sessionManager.doRequest(true, ref msgSetRequest);

                    FillJournalDataBase(ref msgSetResponse, ref bDone, ref bError);

                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void FillJournalDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
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
                if (response !=null)
                {
                    if (response.StatusCode != double.Parse("0"))
                    {
                        // If the status is bad, report it to the user
                       // Interaction.MsgBox("Get Journal query unexpexcted Error - " + response.StatusMessage);
                        ds.ShowDialog("", "Get Journal query unexpexcted Error -" + response.StatusMessage, Haley.Enums.NotificationIcon.Error);
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
                IJournalEntryRetList JournalList;
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtJournalEntryQueryRs & responseDetailType == ENObjectType.otJournalEntryRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    JournalList = (IJournalEntryRetList)response.Detail;
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
                IJournalEntryRet JournalRet;
                count = (short)JournalList.Count;
                short MAX_RETURNED;
                MAX_RETURNED = (short)(1 + JournalList.Count);
                // we are done with the customerQueries if we have not received the MaxReturned
                if (count < MAX_RETURNED)
                {
                    bDone = true;
                }
                db.tblJournalEntry_Delete();
                db.tblJrLineItem_Delete();
                var journalRefNUm = default(string);
                var journalTxnDate = default(string);
                var journalTxnId = default(string);
                var journalTxnNumber = default(string);
                var loopTo = (short)(count - 1);
                for (index = 0; index <= loopTo; index++)
                {
                    if (index == 31)
                    {
                        int k = 0;
                    }
                    // skip this customer if this is a repeat from the last query
                    JournalRet = JournalList.GetAt(index);
                    // (JournalRet Is Nothing) Or _
                    // (JournalRet.RefNumber Is Nothing) Or _
                    if (JournalRet.TxnID is null)
                    {
                        bDone = true;
                        bError = true;
                        return;
                    }
                    // If Not (JournalRet.CurrencyRef Is Nothing) Then
                    // journalCurrency = JournalRet.CurrencyRef.FullName.GetValue()
                    // End If
                    string journalCurrency = string.Empty;
                    if (JournalRet.RefNumber != null)
                    {
                        journalRefNUm = JournalRet.RefNumber.GetValue();
                    }
                    if (journalRefNUm == "Setup 15")
                    {
                        int s = 0;
                    }
                    if (JournalRet.TxnDate != null)
                    {
                        journalTxnDate = JournalRet.TxnDate.GetValue().ToString();
                    }
                    if (JournalRet.TxnID != null)
                    {
                        journalTxnId = JournalRet.TxnID.GetValue();
                    }
                    if (JournalRet.TxnNumber != null)
                    {
                        journalTxnNumber = JournalRet.TxnNumber.GetValue().ToString();
                    }


                    db.tblJournalEntry_Insert(journalTxnId, journalTxnNumber, journalTxnDate, journalRefNUm, journalCurrency, ClearAllControl.gblCompanyID.ToString());

                    // only the first customerRet should be repeating and then
                    // lets just check to make sure we do not have the customer
                    // just in case another app changed a customer right between our
                    // declare varibale to retrive data 

                    string jrItemCrFullName;
                    string jrItemCrTxnId;
                    string jrItemCrMemo;
                    decimal jrItemCrAmount;
                    string jrItemDrFullName;
                    string jrItemDrTxnId;
                    string txnType;
                    string jrItemDrMemo;
                    decimal jrItemDrAmount;
                    IORJournalLine orJournalitem;
                    string jrnlEntityREf;
                    int j = 0;
                    var loopTo1 = JournalRet.ORJournalLineList.Count - 1;
                    for (j = 0; j <= loopTo1; j++)
                    {
                        jrItemCrFullName = "NULL";
                        jrItemDrFullName = "NULL";
                        jrItemCrAmount = 0m;
                        jrItemDrAmount = 0m;
                        jrItemCrMemo = "NULL";
                        jrItemDrMemo = "NULL";
                        jrItemCrTxnId = "NULL";
                        jrItemDrTxnId = "NULL";
                        jrnlEntityREf = "NULL";
                        orJournalitem = JournalRet.ORJournalLineList.GetAt(j);

                        if (orJournalitem.JournalCreditLine != null)
                        {
                            if (orJournalitem.JournalCreditLine.AccountRef.FullName != null)
                            {
                                jrItemCrFullName = orJournalitem.JournalCreditLine.AccountRef.FullName.GetValue();
                            }

                            if (orJournalitem.JournalCreditLine.TxnLineID != null)
                            {
                                jrItemCrTxnId = orJournalitem.JournalCreditLine.TxnLineID.GetValue();
                            }

                            if (orJournalitem.JournalCreditLine.Memo != null)
                            {
                                jrItemCrMemo = orJournalitem.JournalCreditLine.Memo.GetValue();
                            }

                            if (orJournalitem.JournalCreditLine.Amount != null)
                            {
                                jrItemCrAmount = (decimal)orJournalitem.JournalCreditLine.Amount.GetValue();
                            }
                            if (orJournalitem.JournalCreditLine.EntityRef != null)
                            {
                                jrnlEntityREf = orJournalitem.JournalCreditLine.EntityRef.FullName.GetValue();
                            }


                        }
                        txnType = "CR";
                        if (!(jrItemCrTxnId == "NULL"))
                        {
                            db.tblJrLineItem_Insert(journalTxnNumber, txnType, jrItemCrTxnId, jrItemCrFullName, jrItemCrMemo, jrItemCrAmount, jrnlEntityREf);
                        }

                        if (orJournalitem.JournalDebitLine != null)
                        {
                            if (orJournalitem.JournalDebitLine.AccountRef != null)
                            {
                                jrItemDrFullName = orJournalitem.JournalDebitLine.AccountRef.FullName.GetValue();
                            }

                            if (orJournalitem.JournalDebitLine.TxnLineID != null)
                            {
                                jrItemDrTxnId = orJournalitem.JournalDebitLine.TxnLineID.GetValue();
                            }

                            if (orJournalitem.JournalDebitLine.Memo != null)
                            {
                                jrItemDrMemo = orJournalitem.JournalDebitLine.Memo.GetValue();
                            }

                            if (orJournalitem.JournalDebitLine.Amount != null)
                            {
                                jrItemDrAmount = (decimal)orJournalitem.JournalDebitLine.Amount.GetValue();
                            }
                            if (orJournalitem.JournalDebitLine.EntityRef != null)
                            {
                                jrnlEntityREf = orJournalitem.JournalDebitLine.EntityRef.FullName.GetValue();
                            }
                        }
                        if (!(jrItemDrTxnId == "NULL"))
                        {
                            txnType = "DR";
                            db.tblJrLineItem_Insert(journalTxnNumber, txnType, jrItemDrTxnId, jrItemDrFullName, jrItemDrMemo, jrItemDrAmount, jrnlEntityREf);
                        }

                    }
                }
                return;
            }
            catch
            {
                //Interaction.MsgBox("HRESULT = :" + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in FillChartOfAccountListBox");
                ds.ShowDialog("", "Error to Fill JV Table DB", Haley.Enums.NotificationIcon.Error);
                bDone = true;
                bError = true;
            }
        }
        #endregion

        #region Methods
        private void LoadJVNumber()
        {
            try
            {
                cmbxJVNumber.SelectedIndex = -1;
                cmbxJVNumber.Items.Clear();
                //sql.addparam("@CopmanyID", cID);
                //sql.addparam("@JVDate", jvDate);
                //sql.addparam("@voucherName", Vouc);
                sql.execquery("select jRefNumber from tblJournalEntry");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cmbxJVNumber.Items.Add(r["jRefNumber"]);
                    cmbxJVNumber.SelectedIndex = -1;
                }
                //object tempVoucherID;
                //if (!string.IsNullOrEmpty(cmbxJVNumber.Text))
                //{
                //    tempVoucherID = cmbxJVNumber.Text;
                //}
                //else
                //{
                //    tempVoucherID = null;
                //}
                //{
                //    var withBlock = cmbxJVNumber.Text;
                //    cmbxJVNumber.DataContext = db.tblJournalEntry_Select(ClearAllControl.gblCompanyID.ToString(), tempVoucherID.ToString());
                //    cmbxJVNumber.DisplayMemberPath = "jRefNumber";
                //    cmbxJVNumber.SelectedValuePath = "jRefNumber";
                //    //cmbxJVNumber.Clear();
                //   // cmbxJVNumber.Text.Add("jRefNumber", "Journal Reference Number", 50);

                //}
            }


            catch (Exception ex)
            {

            }
        }

        private void PopulateTempleteCombobox()
        {
            tmpltJVCmbx.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Journal' and Status='True' ");
            if (sql.recordcount > 0)
            {
                foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                    tmpltJVCmbx.Items.Add(r["TemplateName"]);
                tmpltJVCmbx.SelectedIndex = 0;
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
        private void tmpltJVCmbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempPath(tmpltJVCmbx.Text);
        }

        private void btnJVSynAl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                GetJournalTransaction(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "JV Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "JV Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }

                // populateGrid()
                LoadJVNumber();
            }
            catch (Exception ex)
            {


            }
        }

        private void btnJVPrint_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            //if (string.IsNullOrEmpty(VoucherNumLkpEdit.Text))
            //{
            //    Interaction.MsgBox("Enter Voucher Number...", Constants.vbInformation);
            //    return;
            //}
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

                //ref var withBlock = ref _ReportDocument{ }
                //string startuppatah = Application.StartupPath;
                _ReportDocument.Load(path + tmpltJVCmbx.Text + ".rpt");
                //{
                //    ref var withBlock = ref _ReportDocument;
                //    //string startuppatah = Application.StartupPath;
                //    withBlock.Load(path + tmpltJVCmbx.Text + ".rpt");

                //    // 'If chkAd.Checked = True Then
                //    // '.Load(Application.StartupPath & "\Reports\JournalVoucherU.rpt")
                //    // 'ElseIf rdMBP.Checked = True Then
                //    // '.Load(Application.StartupPath & "\Reports\JournalVoucherMBP.rpt")
                //    // 'ElseIf rdMOnarqi.Checked = True Then
                //    // '.Load(Application.StartupPath & "\Reports\JournalVoucherMON.rpt")
                //    // 'Else
                //    // '.Load(Application.StartupPath & "\Reports\JournalVoucher.rpt")
                //    // 'End If

                //}

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


                crParameterDiscreteValue.Value = ClearAllControl.gblCompanyID;
                crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
                crParameterFieldDefinition = crParameterFieldDefinitions["@CopmanyID"];
                crParameterValues = crParameterFieldDefinition.CurrentValues;
                crParameterValues.Clear();
                crParameterValues.Add(crParameterDiscreteValue);
                crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

                //crParameterDiscreteValue.Value = tempdate;
                //crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
                //crParameterFieldDefinition = crParameterFieldDefinitions["@JVDate"];
                //crParameterValues = crParameterFieldDefinition.CurrentValues;
                //crParameterValues.Clear();
                //crParameterValues.Add(crParameterDiscreteValue);
                //crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

                crParameterDiscreteValue.Value = cmbxJVNumber.Text;
                crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
                crParameterFieldDefinition = crParameterFieldDefinitions["@voucherName"];
                crParameterValues = crParameterFieldDefinition.CurrentValues;
                crParameterValues.Clear();
                crParameterValues.Add(crParameterDiscreteValue);
                crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);



                // If _ReportDocument.HasRecords Then
                var rpt = new ReportView();
                rpt.ShowReportView(ref _ReportDocument);
            }

            catch (Exception ex)
            {
                ds.ShowDialog(ex.Message, "TNC-Sync", Haley.Enums.NotificationIcon.Error);
            }
        }
        #endregion
    }
}
