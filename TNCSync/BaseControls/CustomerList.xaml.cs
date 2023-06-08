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
using System.Xml;
using TNCSync.Sessions;
using Interop.QBFC15;
using Interop.QBXMLRP2;
using Haley.Abstractions;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for CustomerList.xaml
    /// </summary>
    public partial class CustomerList : UserControl
    {
        public CustomerList()
        {
            InitializeComponent();
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


		#region Request Building
		private IMsgSetRequest buildDataCountQuery(string request)
		{
			IMsgSetRequest requestMsgSet = sessionManager.getMsgSetRequest();
			requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;
			switch (request)
			{
				case "CustomerQueryRq":
					ICustomerQuery custQuery = requestMsgSet.AppendCustomerQueryRq();
					custQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
					break;
				case "ItemQueryRq":
					IItemQuery itemQuery = requestMsgSet.AppendItemQueryRq();
					itemQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
					break;
				case "TermsQueryRq":
					ITermsQuery termsQuery = requestMsgSet.AppendTermsQueryRq();
					termsQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
					break;
				case "SalesTaxCodeQueryRq":
					ISalesTaxCodeQuery salesTaxQuery = requestMsgSet.AppendSalesTaxCodeQueryRq();
					salesTaxQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
					break;
				case "CustomerMsgQueryRq":
					ICustomerMsgQuery custMsgQuery = requestMsgSet.AppendCustomerMsgQueryRq();
					custMsgQuery.metaData.SetValue(ENmetaData.mdMetaDataOnly);
					break;
				default:
					break;
			}
			return requestMsgSet;
		}

		private IMsgSetRequest buildCustomerQueryRq(string[] includeRetElement, string fullName)
		{
			IMsgSetRequest requestMsgSet = sessionManager.getMsgSetRequest();
			requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;
			ICustomerQuery custQuery = requestMsgSet.AppendCustomerQueryRq();
			if (fullName != null)
			{
				custQuery.ORCustomerListQuery.FullNameList.Add(fullName);
			}
			for (int x = 0; x < includeRetElement.Length; x++)
			{
				custQuery.IncludeRetElementList.Add(includeRetElement[x]);
			}
			return requestMsgSet;
		}
		#endregion

		#region Response Parsing
		private int getCount(string request)
		{
			IMsgSetResponse responseMsgSet = processRequestFromQB(buildDataCountQuery(request));
			int count = parseRsForCount(responseMsgSet);
			return count;
		}
		private int parseRsForCount(IMsgSetResponse responseMsgSet)
		{
			int ret = -1;
			try
			{
				IResponse response = responseMsgSet.ResponseList.GetAt(0);
				ret = response.retCount;
			}
			catch (Exception e)
			{
				MessageBox.Show("Error encountered: " + e.Message);
				ret = -1;
			}
			return ret;
		}

		private string[] parseCustomerQueryRs(IMsgSetResponse responseMsgSet, int count)
		{
			/*
             <?xml version="1.0" ?> 
             <QBXML>
             <QBXMLMsgsRs>
             <CustomerQueryRs requestID="1" statusCode="0" statusSeverity="Info" statusMessage="Status OK">
                 <CustomerRet>
                     <FullName>Abercrombie, Kristy</FullName> 
                 </CustomerRet>
             </CustomerQueryRs>
             </QBXMLMsgsRs>
             </QBXML>    
            */
			string[] retVal = new string[count];
			IResponse response = responseMsgSet.ResponseList.GetAt(0);
			int statusCode = response.StatusCode;
			if (statusCode == 0)
			{
				ICustomerRetList custRetList = response.Detail as ICustomerRetList;
				for (int i = 0; i < count; i++)
				{
					string fullName = null;
					if (custRetList.GetAt(i).FullName != null)
					{
						fullName = custRetList.GetAt(i).FullName.GetValue().ToString();
						if (fullName != null)
						{
							retVal[i] = fullName;
						}
					}
					IAddress billAddress = null;
					if (custRetList.GetAt(i).BillAddress != null)
					{
						billAddress = custRetList.GetAt(i).BillAddress;
						string addr1 = "", addr2 = "", addr3 = "", addr4 = "", addr5 = "";
						string city = "", state = "", postalcode = "";
						if (billAddress != null)
						{
							if (billAddress.Addr1 != null) addr1 = billAddress.Addr1.GetValue().ToString();
							if (billAddress.Addr1 != null) addr1 = billAddress.Addr1.GetValue().ToString();
							if (billAddress.Addr2 != null) addr2 = billAddress.Addr2.GetValue().ToString();
							if (billAddress.Addr3 != null) addr3 = billAddress.Addr3.GetValue().ToString();
							if (billAddress.Addr4 != null) addr4 = billAddress.Addr4.GetValue().ToString();
							if (billAddress.Addr5 != null) addr5 = billAddress.Addr5.GetValue().ToString();
							if (billAddress.City != null) city = billAddress.City.GetValue().ToString();
							if (billAddress.State != null) state = billAddress.State.GetValue().ToString();
							if (billAddress.PostalCode != null) postalcode = billAddress.PostalCode.GetValue().ToString();

							retVal[i] = addr1 + "\r\n" + addr2 + "\r\n"
								+ addr3 + "\r\n"
								+ city + "\r\n" + state + "\r\n" + postalcode;
						}
					}
					//IAddress shipAddress = null;
					//if (custRetList.GetAt(i).ShipAddress != null)
					//{
					//    shipAddress = custRetList.GetAt(i).ShipAddress;
					//    string addr1 = "", addr2 = "", addr3 = "", addr4 = "", addr5 = "";
					//    string city = "", state = "", postalcode = "";
					//    if (shipAddress != null)
					//    {
					//        if (shipAddress.Addr1 != null) addr1 = shipAddress.Addr1.GetValue().ToString();
					//        if (shipAddress.Addr1 != null) addr1 = shipAddress.Addr1.GetValue().ToString();
					//        if (shipAddress.Addr2 != null) addr2 = shipAddress.Addr2.GetValue().ToString();
					//        if (shipAddress.Addr3 != null) addr3 = shipAddress.Addr3.GetValue().ToString();
					//        if (shipAddress.Addr4 != null) addr4 = shipAddress.Addr4.GetValue().ToString();
					//        if (shipAddress.Addr5 != null) addr5 = shipAddress.Addr5.GetValue().ToString();
					//        if (shipAddress.City != null) city = shipAddress.City.GetValue().ToString();
					//        if (shipAddress.State != null) state = shipAddress.State.GetValue().ToString();
					//        if (shipAddress.PostalCode != null) postalcode = shipAddress.PostalCode.GetValue().ToString();

					//        // RESUME HERE
					//        retVal[i] = addr1 + "\r\n" + addr2 + "\r\n"
					//            + addr3 + "\r\n"
					//            + city + "\r\n" + state + "\r\n" + postalcode;
					//    }
					//}
					string currencyRef = null;
					if (custRetList.GetAt(i).CurrencyRef != null)
					{
						currencyRef = custRetList.GetAt(i).CurrencyRef.FullName.GetValue().ToString();
						if (currencyRef != null)
						{
							retVal[i] = currencyRef;
						}
					}
				}
			}
			return retVal;
		}
		#endregion

		private void Addcust_Click(object sender, RoutedEventArgs e)
		{
			//step1: verify that Name is not empty
			String name = CustName.Text.Trim();
			if (name.Length == 0)
			{
				MessageBox.Show("Please enter a value for Name.", "Input Validation");
				return;
			}

			//step2: create the qbXML request
			XmlDocument inputXMLDoc = new XmlDocument();
			inputXMLDoc.AppendChild(inputXMLDoc.CreateXmlDeclaration("1.0", null, null));
			inputXMLDoc.AppendChild(inputXMLDoc.CreateProcessingInstruction("qbxml", "version=\"2.0\""));
			XmlElement qbXML = inputXMLDoc.CreateElement("QBXML");
			inputXMLDoc.AppendChild(qbXML);
			XmlElement qbXMLMsgsRq = inputXMLDoc.CreateElement("QBXMLMsgsRq");
			qbXML.AppendChild(qbXMLMsgsRq);
			qbXMLMsgsRq.SetAttribute("onError", "stopOnError");
			XmlElement custAddRq = inputXMLDoc.CreateElement("CustomerAddRq");
			qbXMLMsgsRq.AppendChild(custAddRq);
			custAddRq.SetAttribute("requestID", "1");
			XmlElement custAdd = inputXMLDoc.CreateElement("CustomerAdd");
			custAddRq.AppendChild(custAdd);
			custAdd.AppendChild(inputXMLDoc.CreateElement("Name")).InnerText = name;
			if (Phone.Text.Length > 0)
			{
				custAdd.AppendChild(inputXMLDoc.CreateElement("Phone")).InnerText = Phone.Text;
			}

			string input = inputXMLDoc.OuterXml;
			//step3: do the qbXMLRP request
			RequestProcessor2 rp = null;
			string ticket = null;
			string response = null;
			try
			{
				rp = new RequestProcessor2();
				rp.OpenConnection("", "IDN CustomerAdd");
				ticket = rp.BeginSession("", QBFileMode.qbFileOpenDoNotCare);
				response = rp.ProcessRequest(ticket, input);

			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show("COM Error Description = " + ex.Message, "COM error");
				return;
			}
			finally
			{
				if (ticket != null)
				{
					rp.EndSession(ticket);
				}
				if (rp != null)
				{
					rp.CloseConnection();
				}
			};

			//step4: parse the XML response and show a message
			XmlDocument outputXMLDoc = new XmlDocument();
			outputXMLDoc.LoadXml(response);
			XmlNodeList qbXMLMsgsRsNodeList = outputXMLDoc.GetElementsByTagName("CustomerAddRs");

			if (qbXMLMsgsRsNodeList.Count == 1) //it's always true, since we added a single Customer
			{
				System.Text.StringBuilder popupMessage = new System.Text.StringBuilder();

				XmlAttributeCollection rsAttributes = qbXMLMsgsRsNodeList.Item(0).Attributes;
				//get the status Code, info and Severity
				string retStatusCode = rsAttributes.GetNamedItem("statusCode").Value;
				string retStatusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
				string retStatusMessage = rsAttributes.GetNamedItem("statusMessage").Value;
				popupMessage.AppendFormat("statusCode = {0}, statusSeverity = {1}, statusMessage = {2}",
					retStatusCode, retStatusSeverity, retStatusMessage);

				//get the CustomerRet node for detailed info

				//a CustomerAddRs contains max one childNode for "CustomerRet"
				XmlNodeList custAddRsNodeList = qbXMLMsgsRsNodeList.Item(0).ChildNodes;
				if (custAddRsNodeList.Count == 1 && custAddRsNodeList.Item(0).Name.Equals("CustomerRet"))
				{
					XmlNodeList custRetNodeList = custAddRsNodeList.Item(0).ChildNodes;

					foreach (XmlNode custRetNode in custRetNodeList)
					{
						if (custRetNode.Name.Equals("ListID"))
						{
							popupMessage.AppendFormat("\r\nCustomer ListID = {0}", custRetNode.InnerText);
						}
						else if (custRetNode.Name.Equals("Name"))
						{
							popupMessage.AppendFormat("\r\nCustomer Name = {0}", custRetNode.InnerText);
						}
						else if (custRetNode.Name.Equals("FullName"))
						{
							popupMessage.AppendFormat("\r\nCustomer FullName = {0}", custRetNode.InnerText);
						}
					}
				} // End of customerRet

				MessageBox.Show(popupMessage.ToString(), "QuickBooks response");
				//ds.SendToast(popupMessage.ToString(), "QuickBooks response",Haley.Enums.NotificationIcon.Success);
			} //End of customerAddRs
		}

		private void Searchcust_Click(object sender, RoutedEventArgs e)
		{
			string request = "CustomerQueryRq";
			int count = getCount(request);
			IMsgSetResponse responseMsgSet = processRequestFromQB(buildCustomerQueryRq(new string[] { "FullName" }, null));
			string[] customerList = parseCustomerQueryRs(responseMsgSet, count);
			disconnectFromQB();
			//Custgrid(this.data)

			////Step-1 Verify that name is not empty
			String name = CustName.Text.Trim();
			if (name.Length == 0)
			{
				MessageBox.Show("Please enter a value for Name.", "Input Validation");
				return;
			}
			//Step-2: Create the QBXML request
			XmlDocument getXMLDoc = new XmlDocument();
			getXMLDoc.AppendChild(getXMLDoc.CreateXmlDeclaration("1.0", null, null));
			getXMLDoc.AppendChild(getXMLDoc.CreateProcessingInstruction("qbxml", "version=\"2.0\""));
			XmlElement qbXML = getXMLDoc.GetElementById("QBXML");
			getXMLDoc.AppendChild(qbXML);
			XmlElement qbXMLMsgRq = getXMLDoc.GetElementById("QBXMLMsgsRq");
			qbXML.AppendChild(qbXMLMsgRq);
			qbXMLMsgRq.SetAttribute("onErroe", "stopOnError");
			XmlElement custSrcRq = getXMLDoc.GetElementById("CustomerSrcRq");
			qbXMLMsgRq.AppendChild(custSrcRq);
			custSrcRq.GetAttribute("requestID", "1");
			XmlElement custSrc = getXMLDoc.GetElementById("CustomerSearch");
			custSrcRq.AppendChild(custSrc);
			custSrc.AppendChild(getXMLDoc.GetElementById("Name")).InnerText = name;
			if (Phone.Text.Length > 0)
			{
				custSrc.AppendChild(getXMLDoc.GetElementById("Phone")).InnerText = Phone.Text;
			}
			string get = getXMLDoc.OuterXml;
			////step3: do the qbXMLRP request
			RequestProcessor2 rp = null;
			string ticket = null;
			string response = null;
			try
			{
				rp = new RequestProcessor2();
				rp.OpenConnection("", "IDN CustomerSearch ");
				ticket = rp.BeginSession("", QBFileMode.qbFileOpenDoNotCare);
				response = rp.ProcessRequest(ticket, get);

			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show("COM Error Description = " + ex.Message, "COM error");
				return;
			}
			finally
			{
				if (ticket != null)
				{
					rp.EndSession(ticket);
				}
				if (rp != null)
				{
					rp.CloseConnection();
				}
			};
			////step4: parse the XML response and show a message
			XmlDocument outputXMLDoc = new XmlDocument();
			outputXMLDoc.LoadXml(response);
			XmlNodeList qbXMLMsgsRsNodeList = outputXMLDoc.GetElementsByTagName("CustomerSrcRq");

			if (qbXMLMsgsRsNodeList.Count == 1) //it's always true, since we added a single Customer
			{
				System.Text.StringBuilder popupMessage = new System.Text.StringBuilder();

				XmlAttributeCollection rsAttributes = qbXMLMsgsRsNodeList.Item(0).Attributes;
				//get the status Code, info and Severity
				string retStatusCode = rsAttributes.GetNamedItem("statusCode").Value;
				string retStatusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
				string retStatusMessage = rsAttributes.GetNamedItem("statusMessage").Value;
				popupMessage.AppendFormat("statusCode = {0}, statusSeverity = {1}, statusMessage = {2}",
					retStatusCode, retStatusSeverity, retStatusMessage);

				//get the CustomerRet node for detailed info

				//a CustomerAddRs contains max one childNode for "CustomerRet"
				XmlNodeList custAddRsNodeList = qbXMLMsgsRsNodeList.Item(0).ChildNodes;
				if (custAddRsNodeList.Count == 1 && custAddRsNodeList.Item(0).Name.Equals("CustomerRet"))
				{
					XmlNodeList custRetNodeList = custAddRsNodeList.Item(0).ChildNodes;

					foreach (XmlNode custRetNode in custRetNodeList)
					{
						if (custRetNode.Name.Equals("ListID"))
						{
							popupMessage.AppendFormat("\r\nCustomer ListID = {0}", custRetNode.InnerText);
						}
						else if (custRetNode.Name.Equals("Name"))
						{
							popupMessage.AppendFormat("\r\nCustomer Name = {0}", custRetNode.InnerText);
						}
						else if (custRetNode.Name.Equals("FullName"))
						{
							popupMessage.AppendFormat("\r\nCustomer FullName = {0}", custRetNode.InnerText);
						}
					}
				} // End of customerRet

				MessageBox.Show(popupMessage.ToString(), "QuickBooks response");
			} //End of customerAddRs
		}

        private void btnShowall_Click(object sender, RoutedEventArgs e)
        {
			populateDatagrid();
        }

		private void populateDatagrid()
		{
			string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
			SqlConnection sqlconn = new SqlConnection(conn);
			//string sqlquery = "SELECT * FROM tblVendor";
			SqlCommand cmd = new SqlCommand("SELECT * FROM tblCustomer", sqlconn);
			sqlconn.Open();
			SqlDataAdapter sdr = new SqlDataAdapter(cmd);
			DataTable table = new DataTable();
			sdr.Fill(table);
			Custgrid.ItemsSource = table.DefaultView;
			sqlconn.Close();
		}
	}
}
