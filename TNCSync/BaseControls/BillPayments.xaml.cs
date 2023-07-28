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
using Haley.MVVM;
//using Interop.QBFC15;
using Interop.QBFC16;
using Microsoft.VisualBasic.CompilerServices;
//using Interop.QBXMLRP2;
using TNCSync.Class;
using TNCSync.Class.DataBaseClass;
using TNCSync.Model;
using TNCSync.Sessions;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for BillPayments.xaml
    /// </summary>
    public partial class BillPayments : UserControl
    {
        private bool bError;
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        List<BillPaymentCheque> BpCheques = new List<BillPaymentCheque>();
        private SQLControls sql = new SQLControls();
        DataTable table = new DataTable();
        SqlDataAdapter sda = new SqlDataAdapter();
        SessionManager sessionManager;
        private short maxVersion;
        private string path = null;
        //private string payeeFullName;

        public BillPayments()
        {
            InitializeComponent();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpfrom.SelectedDate = DateTime.Now.Date;
            dpTo.SelectedDate = DateTime.Now.Date;
            //PopulateDataGrid();
            //LoadPayeeCombobox();
            PopulateTempleteCombobox();
            PopulateDataGridInitial();
        }

        public IDialogService ds;

        #region Connect to QuickBooks
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

        #region BillPayment Methods
        public void GetBillPaymentTransactionDate(ref bool bError, DateTime fromDate, DateTime todate)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;
                IBillPaymentCheckQuery billPaymentCheckQuery = msgsetRequest.AppendBillPaymentCheckQueryRq();
                billPaymentCheckQuery.IncludeLineItems.SetValue(true);
                DateTime billTxnDate;
                billTxnDate = Microsoft.VisualBasic.DateAndTime.DateAdd(Microsoft.VisualBasic.DateInterval.Day, -30, DateTime.Now);
                billPaymentCheckQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(fromDate);
                billPaymentCheckQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(todate);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillBillCheckinDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void GetBillPaymentTransaction(ref bool bError)
        {
            try
            {
                //Make sure we do not have any old requests still define
                IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
                msgsetRequest.ClearRequests();
                //Set the On error attribute to continueonerror
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;

                IBillPaymentCheckQuery billPaymentCheckQuery = msgsetRequest.AppendBillPaymentCheckQueryRq();
                billPaymentCheckQuery.IncludeLineItems.SetValue(true);
                DateTime billTxnDate;
                billTxnDate = Microsoft.VisualBasic.DateAndTime.DateAdd(Microsoft.VisualBasic.DateInterval.Day, -30, DateTime.Now);

                bool bDone = false;
                while (!bDone)
                {
                    // send the request to QB
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);

                    FillBillCheckinDataBase(ref responseSet, ref bDone, ref bError);
                }
                return;
            }
            catch
            {
                bError = true;
            }
        }

        public void FillBillCheckinDataBase(ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
        {
            try
            {
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
                         ds.ShowDialog("TNCSync","FillBillpaymentListBox unexpexcted Error - " + response.StatusMessage,Haley.Enums.NotificationIcon.Warning);
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

                //Make sure We are Processing the BillpaymentQueryRs and the SalesorderRetList responses in this list
                IBillPaymentCheckRetList billPaymentCheckRetList;  //BillpaymentRetlist
                ENResponseType responseType;
                ENObjectType responseDetailType;
                responseType = (ENResponseType)response.Type.GetValue();
                responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
                if (responseType == ENResponseType.rtBillPaymentCheckQueryRs & responseDetailType == ENObjectType.otBillPaymentCheckRetList)
                {
                    // save the response detail in the appropriate object type
                    // since we have first verified the type of the response object
                    billPaymentCheckRetList = (IBillPaymentCheckRetList)response.Detail;
                }
                else
                {
                    // bill, we do not have the responses we were expecting
                    bDone = true;
                    bError = true;
                    return;
                }

                //Parse the query response and add the BillPayment to the BPlist
                short count;
                short index;
                IBillPaymentCheckRet billPaymentCheckRet;
                count = (short)billPaymentCheckRetList.Count;
                short Max_Returned;
                Max_Returned = (short)(1 + billPaymentCheckRetList.Count);
                //we are done with the Billpayment Queries if we have not received the MaxReturned
                if (count < Max_Returned)
                {
                    bDone = true;
                }
                db.tblQBBillPayCheck_Delete();
                db.tblQBBillPayCheckLInes_Delete();
                var billLineAmount = default(decimal);
                var balanceRemaining = default(decimal);
                var discountAmount = default(decimal);
                var BillLineTxnDate = default(DateTime);
                var loopTo = (short)(count - 1);
                for(index =0; index<= loopTo; index++)
                {
                    //Skip this bill if this is a repeat from the last query
                    billPaymentCheckRet = billPaymentCheckRetList.GetAt(index);
                    if(billPaymentCheckRet is null | billPaymentCheckRet.APAccountRef is null | billPaymentCheckRet.TxnID is null)
                    {
                        bDone = true;
                        bError = true;
                        return;
                    }
                    //Only the first BPCheckRet should be repeating and then lets just check to make sure we do not have the BP
                    //Just in case another app chenged a BP right between out

                    //Declare Variable to retrive data
                    string Amount = string.Empty;
                    if (billPaymentCheckRet.Amount != null)
                    {
                        Amount = billPaymentCheckRet.Amount.GetValue().ToString();
                    }
                    string APName = null;
                    if (billPaymentCheckRet.APAccountRef != null)
                    {
                        APName = billPaymentCheckRet.APAccountRef.FullName.GetValue();
                    }
                    string bankName = null;
                    if (billPaymentCheckRet.BankAccountRef != null)
                    {
                        bankName = billPaymentCheckRet.BankAccountRef.FullName.GetValue();
                    }
                    string memo = null;
                    if (billPaymentCheckRet.Memo != null)
                    {
                        memo = billPaymentCheckRet.Memo.GetValue();
                    }
                    string payeeFullName = null;
                    if (billPaymentCheckRet.PayeeEntityRef != null)
                    {
                        payeeFullName = billPaymentCheckRet.PayeeEntityRef.FullName.GetValue();
                    }

                    string refNumber = null;
                    if (billPaymentCheckRet.RefNumber != null)
                    {
                        refNumber = billPaymentCheckRet.RefNumber.GetValue();
                    }
                    DateTime txnDate = default;
                    if (billPaymentCheckRet.TxnDate != null)
                    {
                        txnDate = billPaymentCheckRet.TxnDate.GetValue();
                    }
                    string txnNUmber = null;
                    if (billPaymentCheckRet.TxnNumber != null)
                    {
                        txnNUmber = billPaymentCheckRet.TxnNumber.GetValue().ToString();
                    }
                    string type = null;
                    if (billPaymentCheckRet.Type != null)
                    {
                        type = billPaymentCheckRet.Type.GetAsString();
                    }

                    db.tblQBBillPayCheck_Insert(txnNUmber, type, txnDate, refNumber, payeeFullName, memo, bankName, APName, Amount, ClearAllControl.gblCompanyID);

                    IAppliedToTxnRet appliedTxnRet;
                    if (billPaymentCheckRet.AppliedToTxnRetList != null)
                    {
                        for (int j = 0, loopTo1 = billPaymentCheckRet.AppliedToTxnRetList.Count - 1; j <= loopTo1; j++)
                        {
                            appliedTxnRet = billPaymentCheckRet.AppliedToTxnRetList.GetAt(j);
                            if (appliedTxnRet.Amount != null)
                            {
                                billLineAmount = (decimal)appliedTxnRet.Amount.GetValue();
                            }
                            if (appliedTxnRet.BalanceRemaining != null)
                            {
                                balanceRemaining = (decimal)appliedTxnRet.BalanceRemaining.GetValue();
                            }
                            if (appliedTxnRet.DiscountAmount != null)
                            {
                                discountAmount = (decimal)appliedTxnRet.DiscountAmount.GetValue();
                            }

                            string billRefNumber = null;
                            if (appliedTxnRet.RefNumber != null)
                            {
                                billRefNumber = appliedTxnRet.RefNumber.GetValue();
                            }
                            if (appliedTxnRet.TxnDate != null)
                            {
                                BillLineTxnDate = appliedTxnRet.TxnDate.GetValue();
                            }
                            string billLineTxnId = null;
                            if (appliedTxnRet.TxnID != null)
                            {
                                billLineTxnId = appliedTxnRet.TxnID.GetValue();
                            }
                            string BillLineTxnType = null;
                            if (appliedTxnRet.TxnType != null)
                            {
                                BillLineTxnType = ((int)appliedTxnRet.TxnType.GetValue()).ToString();
                            }
                            string billLineType = null;
                            if (appliedTxnRet.Type != null)
                            {
                                billLineType = appliedTxnRet.Type.GetValue().ToString();
                            }
                            db.tblQBBillPayCheckLInes_Insert(txnNUmber, billLineTxnId, BillLineTxnDate, billLineType, BillLineTxnType, billRefNumber, discountAmount, balanceRemaining, billLineAmount);
                        }
                    }
                }
                return;
            }
            catch
            {
                int w = 0;
                ds.ShowDialog("TNCSync", "Error to Fill Billpayment", Haley.Enums.NotificationIcon.Error);
                bDone = true;
                bError = true;
            }
        }

        #endregion

        #region Methods
        private void LoadPayeeName(int cID)
        {
            try
            {
                cbxpayeeName.Items.Clear();
                sql.addparam("@CompanyID", cID);
                sql.execquery("SELECT distinct payeeFullName from tblQBBillPayCheck where companyID=@CompanyID");
                if (sql.recordcount > 0)
                {
                    foreach (DataRow r in sql.sqlds.Tables[0].Rows)
                        cbxpayeeName.Items.Add(r["payeeFullName"]);
                    cbxpayeeName.SelectedIndex = -1;
                }

                ///Need to check when it was working or not
                var col = new System.Windows.Forms.AutoCompleteStringCollection();
                for (int i = 0, loopTo = sql.sqlds.Tables[0].Rows.Count - 1; i <= loopTo; i++)
                    col.Add(sql.sqlds.Tables[0].Rows[i]["payeeFullName"].ToString());
                cbxpayeeName.SelectedItem = System.Windows.Forms.AutoCompleteSource.CustomSource;
                cbxpayeeName.ItemsSource = col;
                cbxpayeeName.SelectedItem = System.Windows.Forms.AutoCompleteMode.Suggest;

            }
            catch
            {

            }
        }


        private void PopulateDataGrid()
        {
            var result = db.tblQBBillPayCheckP_Select_TNCS();
            grdBillPytDtl.ItemsSource = result.ToList();

            // string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            // SqlConnection sqlconn = new SqlConnection(conn);
            // SqlCommand cmd = new SqlCommand("tblQBBillPayCheckP_Select", sqlconn);
            // cmd.CommandType = CommandType.StoredProcedure;
            // SqlParameter param = new SqlParameter();
            // param.ParameterName = "@payeeFullName";
            // //sda.SelectCommand = cmd;
            // //cmd.Parameters.Add(param);
            //// sda.Fill(table);
            // grdBillPytDtl.ItemsSource = table.DefaultView;
        }

        private void PopulateDataGridInitial()
        {
            try
            {
                var result = db.tblQBBillPayCheck_Select_Initial(ClearAllControl.gblCompanyID);
                grdBillPytDtl.Columns.Clear();
                grdBillPytDtl.ItemsSource = result.ToList();
            }
            catch
            {

            }
        }

        private void LoadPayeeCombobox()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            sqlconn.Open();
            SqlCommand cmd = new SqlCommand("SELECT distinct payeeFullName FROM tblQBBillPayCheck", sqlconn);
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            cbxpayeeName.ItemsSource = table.DefaultView;
            cbxpayeeName.DisplayMemberPath = "payeeFullName";
        }
        private void UpdateBinding()
        {
            grdBillPytDtl.ItemsSource = BpCheques;
            grdBillPytDtl.DisplayMemberPath = "BillPaymentCheque";
        }

        private void PopulateTempleteCombobox()
        {
            tmpltCmbx.Items.Clear();
            sql.execquery("Select TemplateName from Templates where TemplateType='Bill Payment Voucher' and Status='True' ");
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
        #endregion


        #region Events
        private void btnsycall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                GetBillPaymentTransaction(ref bError);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "BillPayment Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "BillPayment Sync Successfully", Haley.Enums.NotificationIcon.Success);
                }
                PopulateDataGridInitial();
                LoadPayeeName(ClearAllControl.gblCompanyID);
            }
            catch
            {

            }
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectToQB();
                bError = false;
                GetBillPaymentTransactionDate(ref bError, dpfrom.DisplayDate, dpTo.DisplayDate);
                if (bError)
                {
                    ds.ShowDialog("TNC-Sync", "Sync Failed", Haley.Enums.NotificationIcon.Error);
                }
                else
                {
                    ds.ShowDialog("TNC-Sync", "Synced successfully", Haley.Enums.NotificationIcon.Success);
                }
                PopulateDataGridInitial();
                LoadPayeeName(ClearAllControl.gblCompanyID);
            }
            catch
            {

            }
        }

        private void srchBtn_Click(object sender, RoutedEventArgs e)
        {
            BillPaymentCheques_DA Bpcda = new BillPaymentCheques_DA();
            BpCheques = Bpcda.GetCheques(cbxpayeeName.Text);
            UpdateBinding();
        }

        private void btnBillPrint_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    for (int i = 0, loopTo = grdBillPytDtl.Columns.Count - 1; i <= loopTo; i++)
            //    {
            //        if (Operators.ConditionalCompareObjectEqual(grdBillPytDtl.Columns[i].Value, 1, false))
            //        {
            //            var _ReportDocument = new ReportDocument();
            //            var crtableLogoninfos = new TableLogOnInfos();
            //            var crtableLogoninfo = new TableLogOnInfo();
            //            var crConnectionInfo = new ConnectionInfo();
            //            ParameterFieldDefinitions crParameterFieldDefinitions;
            //            ParameterFieldDefinition crParameterFieldDefinition;
            //            var crParameterValues = new ParameterValues();
            //            var crParameterDiscreteValue = new ParameterDiscreteValue();
            //            Tables CrTables;
            //            string extra = null;

            //            {
            //                ref var withBlock = ref _ReportDocument;
            //                string startuppatah = Application.StartupPath;
            //                if (chkIsAccountPayee.Checked)
            //                {
            //                    extra = " AP";
            //                    if (chkWithOutDate.Checked)
            //                    {
            //                        extra = " APWD";
            //                    }
            //                }
            //                else if (chkWithOutDate.Checked)
            //                {
            //                    extra = " WD";
            //                }
            //                else
            //                {
            //                    extra = "";
            //                }

            //                withBlock.Load(path + "BP " + tempname.Text + extra + ".rpt");

            //            }

            //            _ReportDocument.ReportOptions.EnableSaveDataWithReport = false;
            //            crConnectionInfo.ServerName = ClearAllControl.gblSQLServerName;
            //            crConnectionInfo.DatabaseName = ClearAllControl.gblDatabaseName;
            //            crConnectionInfo.UserID = ClearAllControl.gblSQLServerUserName;
            //            crConnectionInfo.Password = ClearAllControl.gblSQLServerPassword;
            //            crConnectionInfo.AllowCustomConnection = false;
            //            crConnectionInfo.IntegratedSecurity = false;

            //            CrTables = _ReportDocument.Database.Tables;

            //            // _ReportDocument.SetParameterValue("@CompanyID", gblCompanyID)
            //            foreach (Table CrTable in CrTables)
            //            {
            //                crtableLogoninfo = CrTable.LogOnInfo;
            //                crtableLogoninfo.ConnectionInfo = crConnectionInfo;
            //                crtableLogoninfo.ReportName = _ReportDocument.Name;
            //                crtableLogoninfo.TableName = CrTable.Name;
            //                CrTable.ApplyLogOnInfo(crtableLogoninfo);
            //            }

            //            crParameterDiscreteValue.Value = dgvCheck.Rows[i].Cells["cTxnID"].Value;
            //            crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
            //            crParameterFieldDefinition = crParameterFieldDefinitions["@txnNumber"];
            //            crParameterValues = crParameterFieldDefinition.CurrentValues;
            //            crParameterValues.Clear();
            //            crParameterValues.Add(crParameterDiscreteValue);
            //            crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

            //            if (chkNameOnCheck.Checked)
            //            {
            //                crParameterDiscreteValue.Value = txtNameOnCheck.EditValue;
            //            }
            //            else
            //            {
            //                string payeeName;

            //                payeeName = Strings.Trim(Conversions.ToString(dgvCheck.Rows[i].Cells["payeename"].Value));
            //                crParameterDiscreteValue.Value = payeeName;
            //                // 'crParameterDiscreteValue.Value = payeename
            //            }
            //            crParameterFieldDefinitions = _ReportDocument.DataDefinition.ParameterFields;
            //            crParameterFieldDefinition = crParameterFieldDefinitions["@payeename"];
            //            crParameterValues = crParameterFieldDefinition.CurrentValues;
            //            crParameterValues.Clear();
            //            crParameterValues.Add(crParameterDiscreteValue);
            //            crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

            //            var rpt = new ReportView();
            //            rpt.ShowReportView(ref _ReportDocument);
            //            return;
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    ds.ShowDialog(ex.Message, "TNC-Sync", Haley.Enums.NotificationIcon.Error);
            //}
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            cbxpayeeName.Text = "";
            dpfrom.SelectedDate = DateTime.Now.Date;
            dpTo.SelectedDate = DateTime.Now.Date;
            PopulateDataGridInitial();
        }
        #endregion



        public static void billPaymenr(ref bool bError)
        {
            
        }
    }
}
