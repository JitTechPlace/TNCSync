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
//using Interop.QBFC15;
using Interop.QBFC16;
//using Interop.QBXMLRP2;
using TNCSync.Class;
using TNCSync.Model;
using TNCSync.Sessions;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for BillPayments.xaml
    /// </summary>
    public partial class BillPayments : UserControl
    {
        List<BillPaymentCheque> BpCheques = new List<BillPaymentCheque>();
        DataTable table = new DataTable();
        SqlDataAdapter sda = new SqlDataAdapter();
        public BillPayments()
        {
            InitializeComponent();
            dpfrom.SelectedDate = DateTime.Now.Date;
            dpTo.SelectedDate = DateTime.Now.Date;
            //PopulateDataGrid();
            //LoadPayeeCombobox();
            //PopulateTempleteCombobox();
        }
        //QBConnect qbConnect = new QBConnect();
        SessionManager sessionManager;
        private short maxVersion;
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

        #region Load Common Controls
        //private void loadPayeeName()
        //{
        //    string request = "PayeeNameQueryRq";
        //    connectToQB();
        //    int count = getCount(request);
        //    IMsgSetResponse responseMsgSet = processRequestFromQB(buildCustomerQueryRq(new string[] { "Fullname" }, null));
        //    string[] customerlist = parseCustomerQueryRs(responseMsgSet, count);
        //    disconnectFromQB();
        //    fillComboBox(this.cbxpayeeName, customerlist);
        //}

        #endregion

        #region RequestBuilding
        //private IMsgSetRequest buildDataCountQuery(string request)
        //{
        //    IMsgSetRequest requestMsgSet = sessionManager.getMsgSetRequest();
        //    requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;
        //    switch (request)
        //    {
        //        case "CustomerQueryRq":
        //            ICustomerQuery custQuery = requestMsgSet.AppendCustomerQueryRq();
        //            custQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
        //            break;
        //        case "ItemQueryRq":
        //            IItemQuery itemQuery = requestMsgSet.AppendItemQueryRq();
        //            itemQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
        //            break;
        //        case "TermsQueryRq":
        //            ITermsQuery termsQuery = requestMsgSet.AppendTermsQueryRq();
        //            termsQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
        //            break;
        //        case "SalesTaxCodeQueryRq":
        //            ISalesTaxCodeQuery salesTaxQuery = requestMsgSet.AppendSalesTaxCodeQueryRq();
        //            salesTaxQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
        //            break;
        //        case "CustomerMsgQueryRq":
        //            ICustomerMsgQuery custMsgQuery = requestMsgSet.AppendCustomerMsgQueryRq();
        //            custMsgQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
        //            break;
        //        default:
        //            break;
        //    }
        //    return requestMsgSet;
        //}

        //private IMsgSetRequest buildCustomerQueryRq(string[] includeRetElement, string fullName)
        //{
        //    IMsgSetRequest requestMsgSet = sessionManager.getMsgSetRequest();
        //    requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;
        //    ICustomerQuery custQuery = requestMsgSet.AppendCustomerQueryRq();
        //    if (fullName != null)
        //    {
        //        custQuery.ORCustomerListQuery.FullNameList.Add(fullName);
        //    }
        //    for (int x = 0; x < includeRetElement.Length; x++)
        //    {
        //        custQuery.IncludeRetElementList.Add(includeRetElement[x]);
        //    }
        //    return requestMsgSet;
        //}
        #endregion



        #region Response Parsing
        //private int getCount(string request)
        //{
        //    IMsgSetResponse responseMsgSet = processRequestFromQB(buildDataCountQuery(request));
        //    int count = parseRsForCount(responseMsgSet);
        //    return count;
        //}

        //private int parseRsForCount(IMsgSetResponse responseMsgSet)
        //{
        //    int ret = -1;
        //    try
        //    {
        //        IResponse response = responseMsgSet.ResponseList.GetAt(0);
        //        ret = response.retCount;
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show("Error encountered: " + e.Message);
        //        ret = -1;
        //    }
        //    return ret;
        //}

        //private string[] parseCustomerQueryRs(IMsgSetResponse responseMsgSet, int count)
        //{
        //    /*
        //     <?xml version="1.0" ?> 
        //     <QBXML>
        //     <QBXMLMsgsRs>
        //     <CustomerQueryRs requestID="1" statusCode="0" statusSeverity="Info" statusMessage="Status OK">
        //         <CustomerRet>
        //             <FullName>Abercrombie, Kristy</FullName> 
        //         </CustomerRet>
        //     </CustomerQueryRs>
        //     </QBXMLMsgsRs>
        //     </QBXML>    
        //    */
        //    string[] retVal = new string[count];
        //    IResponse response = responseMsgSet.ResponseList.GetAt(0);
        //    int statusCode = response.StatusCode;
        //    if (statusCode == 0)
        //    {
        //        ICustomerRetList custRetList = response.Detail as ICustomerRetList;
        //        for (int i = 0; i < count; i++)
        //        {
        //            string fullName = null;
        //            if (custRetList.GetAt(i).FullName != null)
        //            {
        //                fullName = custRetList.GetAt(i).FullName.GetValue().ToString();
        //                if (fullName != null)
        //                {
        //                    retVal[i] = fullName;
        //                }
        //            }
        //            IAddress billAddress = null;
        //            if (custRetList.GetAt(i).BillAddress != null)
        //            {
        //                billAddress = custRetList.GetAt(i).BillAddress;
        //                string addr1 = "", addr2 = "", addr3 = "", addr4 = "", addr5 = "";
        //                string city = "", state = "", postalcode = "";
        //                if (billAddress != null)
        //                {
        //                    if (billAddress.Addr1 != null) addr1 = billAddress.Addr1.GetValue().ToString();
        //                    if (billAddress.Addr1 != null) addr1 = billAddress.Addr1.GetValue().ToString();
        //                    if (billAddress.Addr2 != null) addr2 = billAddress.Addr2.GetValue().ToString();
        //                    if (billAddress.Addr3 != null) addr3 = billAddress.Addr3.GetValue().ToString();
        //                    if (billAddress.Addr4 != null) addr4 = billAddress.Addr4.GetValue().ToString();
        //                    if (billAddress.Addr5 != null) addr5 = billAddress.Addr5.GetValue().ToString();
        //                    if (billAddress.City != null) city = billAddress.City.GetValue().ToString();
        //                    if (billAddress.State != null) state = billAddress.State.GetValue().ToString();
        //                    if (billAddress.PostalCode != null) postalcode = billAddress.PostalCode.GetValue().ToString();

        //                    retVal[i] = addr1 + "\r\n" + addr2 + "\r\n"
        //                        + addr3 + "\r\n"
        //                        + city + "\r\n" + state + "\r\n" + postalcode;
        //                }
        //            }
        //            //IAddress shipAddress = null;
        //            //if (custRetList.GetAt(i).ShipAddress != null)
        //            //{
        //            //    shipAddress = custRetList.GetAt(i).ShipAddress;
        //            //    string addr1 = "", addr2 = "", addr3 = "", addr4 = "", addr5 = "";
        //            //    string city = "", state = "", postalcode = "";
        //            //    if (shipAddress != null)
        //            //    {
        //            //        if (shipAddress.Addr1 != null) addr1 = shipAddress.Addr1.GetValue().ToString();
        //            //        if (shipAddress.Addr1 != null) addr1 = shipAddress.Addr1.GetValue().ToString();
        //            //        if (shipAddress.Addr2 != null) addr2 = shipAddress.Addr2.GetValue().ToString();
        //            //        if (shipAddress.Addr3 != null) addr3 = shipAddress.Addr3.GetValue().ToString();
        //            //        if (shipAddress.Addr4 != null) addr4 = shipAddress.Addr4.GetValue().ToString();
        //            //        if (shipAddress.Addr5 != null) addr5 = shipAddress.Addr5.GetValue().ToString();
        //            //        if (shipAddress.City != null) city = shipAddress.City.GetValue().ToString();
        //            //        if (shipAddress.State != null) state = shipAddress.State.GetValue().ToString();
        //            //        if (shipAddress.PostalCode != null) postalcode = shipAddress.PostalCode.GetValue().ToString();

        //            //        // RESUME HERE
        //            //        retVal[i] = addr1 + "\r\n" + addr2 + "\r\n"
        //            //            + addr3 + "\r\n"
        //            //            + city + "\r\n" + state + "\r\n" + postalcode;
        //            //    }
        //            //}
        //            string currencyRef = null;
        //            if (custRetList.GetAt(i).CurrencyRef != null)
        //            {
        //                currencyRef = custRetList.GetAt(i).CurrencyRef.FullName.GetValue().ToString();
        //                if (currencyRef != null)
        //                {
        //                    retVal[i] = currencyRef;
        //                }
        //            }
        //        }
        //    }
        //    return retVal;
        //}
        #endregion

        #region Methods
        //private void fillComboBox(ComboBox cbxpayeeName, string[] values)
        //{
        //    for(int i=0; i< values.Length; i++)
        //    {
        //        if (values[i] != null) cbxpayeeName.Items.Add(values[i]);
        //    }
        //}
        #endregion

        private void btnsycall_Click(object sender, RoutedEventArgs e)
        {
            PopulateDataGrid();
            LoadPayeeCombobox();
            PopulateTempleteCombobox();
        }

        private void PopulateDataGrid()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            SqlCommand cmd = new SqlCommand("tblQBBillPayCheckP_Select_TNCS", sqlconn);
            cmd.CommandType = CommandType.StoredProcedure;
            //SqlParameter param = new SqlParameter();
            //param.ParameterName = "@payeeFullName";
            sda.SelectCommand = cmd;
            //cmd.Parameters.Add(param);
            sda.Fill(table);
            grdBillPytDtl.ItemsSource = table.DefaultView;
        }

        public static void billPaymenr(ref bool bError)
        {
            
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

        private void srchBtn_Click(object sender, RoutedEventArgs e)
        {
            BillPaymentCheques_DA Bpcda = new BillPaymentCheques_DA();
            BpCheques = Bpcda.GetCheques(cbxpayeeName.Text);
            UpdateBinding();
        }
        private void UpdateBinding()
        {
            grdBillPytDtl.ItemsSource = BpCheques;
            grdBillPytDtl.DisplayMemberPath = "BillPaymentCheque";
        }

        private void PopulateTempleteCombobox()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            sqlconn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Templates where TemplateType ='Check Payment Voucher'", sqlconn);
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            tmpltCmbx.ItemsSource = table.DefaultView;
            tmpltCmbx.DisplayMemberPath = "TemplateName";
            tmpltCmbx.SelectedIndex = -1;
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
