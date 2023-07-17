﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TNCSync.Sessions;
using Interop.QBFC15;
//using Interop.QBFC12;
using Interop.QBXMLRP2;
using Haley.Abstractions;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Haley.MVVM;
using TNCSync.Class.DataBaseClass;
using TNCSync.Class;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for CustomerList.xaml
    /// </summary>
    public partial class CustomerList : UserControl
    {
		private bool bError;
        public CustomerList()
        {
            InitializeComponent();
			ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
			populateDatagrid();
		}

        //QBConnect qbConnect = new QBConnect();
        SessionManager sessionManager;
        private short maxVersion;
		private static DBTNCSDataContext db = new DBTNCSDataContext();
		public IDialogService ds;

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

        #region Customer

		private void Addcust_Click(object sender, RoutedEventArgs e)
		{
			//step1: verify that Name is not empty
			String name = CustName.Text.Trim();
			if (name.Length == 0)
			{
				ds.ShowDialog("Please enter a value for Name.", "Input Validation", Haley.Enums.NotificationIcon.Warning);
				//MessageBox.Show("Please enter a value for Name.", "Input Validation");
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
				ds.SendToast("COM Error Description = " + ex.Message, "COM error");
				//MessageBox.Show("COM Error Description = " + ex.Message, "COM error");
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

				//MessageBox.Show(popupMessage.ToString(), "QuickBooks response");
				ds.SendToast(popupMessage.ToString(), "QuickBooks response",Haley.Enums.NotificationIcon.Success);
			} //End of customerAddRs
		}

		private void Searchcust_Click(object sender, RoutedEventArgs e)
        {
		
		}
		public void GetCustomerList(ref bool bError)
        {
			try
			{
                //IMsgSetRequest msgsetRequest = null;
				IMsgSetRequest msgsetRequest = sessionManager.getMsgSetRequest();
				msgsetRequest.ClearRequests();
                msgsetRequest.Attributes.OnError = ENRqOnError.roeContinue;
				ICustomerQuery customerQuery = msgsetRequest.AppendCustomerQueryRq();
                bool bDone = false;
                while (!bDone)
                {
                    IMsgSetResponse responseSet = sessionManager.doRequest(true, ref msgsetRequest);
                    CustomerDatatoDatabase(ref responseSet, ref bDone, ref bError);
                }
            }
            catch
            {
				ds.SendToast("Not Responding", "Unable to connect with QuickBooks response", Haley.Enums.NotificationIcon.Error);
			}
        }

		public void CustomerDatatoDatabase( ref IMsgSetResponse msgSetResponse, ref bool bDone, ref bool bError)
        {
            try 
			{
				if (msgSetResponse is null || msgSetResponse.ResponseList is null || msgSetResponse.ResponseList.Count < 0)
				{
					bDone = true;
					bError = true;
					return;
				}

				//ResponseParsing List
				IResponseList responseList;
				responseList = msgSetResponse.ResponseList;

				IResponse response;
				response = responseList.GetAt(0);
				if (response != null)
				{
					if (response.StatusCode != 0)
					{
						MessageBox.Show("Error", "Unexpected Error " + response.StatusMessage);
						bDone = true;
						bError = true;
						return;
					}
				}

				//make sure we have a response object to handle
				if (response == null || response.Type == null || response.Detail == null || response.Detail.Type == null)
				{
					bDone = true;
					bError = true;
					return;
				}

				//make sure we are processing the CustomerQueryRs and CustomerRetList in the response list
				ICustomerRetList customerRetList;
				ENResponseType responseType;
				ENObjectType responseDetailType;
				responseType = (ENResponseType)response.Type.GetValue();
				responseDetailType = (ENObjectType)response.Detail.Type.GetValue();
				if (responseType == ENResponseType.rtCustomerQueryRs && responseDetailType == ENObjectType.otCustomerRetList)
				{
					customerRetList = response.Detail as ICustomerRetList;
				}
				else
				{
					bDone = true;
					bError = true;
					return;
				}
				//Parse the query response and add the customer to the customer list box
				short count;
				short index;
				ICustomerRet customerRet;
				count = (short)customerRetList.Count;
				short Max_RETURNED;
				Max_RETURNED = (short)(1 + customerRetList.Count);
				//We are done with the Customer Quereis if we have not received the Maxreturned
				if (count < Max_RETURNED)
				{
					bDone = true;
				}

				var loopTo = (short)(count - 1);
				for (index = 0; index <= loopTo; index++)
				{
					//Skip the value if it is repeating from the last query
					customerRet = customerRetList.GetAt(index);
					if (customerRet == null || customerRet.ListID == null)
					{
						bDone = true;
						bError = true;
						return;
					}

					//Declare variable to Retrive data
					string ListID = customerRet.ListID.GetValue();
					DateTime TimeCreated = default;
					if (customerRet.TimeCreated != null)
					{
						TimeCreated = customerRet.TimeCreated.GetValue();
					}
					DateTime TimeModified = default;
					if (customerRet.TimeModified != null)
					{
						TimeModified = customerRet.TimeModified.GetValue();
					}
					string EditSequence = string.Empty;
					if (customerRet.EditSequence != null)
					{
						EditSequence = customerRet.EditSequence.GetValue();
					}
					string Name = string.Empty;
					if (customerRet.Name != null)
					{
						Name = customerRet.Name.GetValue();
					}
					string ArName = string.Empty;
					string FullName = string.Empty;
					if (customerRet.FullName != null)
					{
						FullName = customerRet.FullName.GetValue();
					}
					string Parent = string.Empty;
					string IsActive = string.Empty;
					if (customerRet.IsActive != null)
					{
						IsActive = customerRet.IsActive.GetValue().ToString();
					}
					string Sublevel = string.Empty;
					if (customerRet.Sublevel != null)
					{
						Sublevel = customerRet.Sublevel.GetValue().ToString();
					}
					string CompanyName = string.Empty;
					if (customerRet.CompanyName != null)
					{
						CompanyName = customerRet.CompanyName.GetValue();
					}
					string Salutation = string.Empty;
					string FirstName = string.Empty;
					string MiddleName = string.Empty;
					string LastName = string.Empty;
					// bill address details
					string BillAddress1 = string.Empty;
					string BillAddress2 = string.Empty;
					string BillAddress3 = string.Empty;
					string BillAddress4 = string.Empty;
					string BillCityRefKey = string.Empty;
					string BillCity = string.Empty;
					string BillStateRefKey = string.Empty;
					string BillState = string.Empty;
					string BillPostalCode = string.Empty;
					string BillCountryRefKey = string.Empty;
					string BillCountry = string.Empty;

					if (customerRet.BillAddress != null)
					{


						if (customerRet.BillAddress.Addr1 != null)
						{
							BillAddress1 = customerRet.BillAddress.Addr1.GetValue();
						}

						if (customerRet.BillAddress.Addr2 != null)
						{
							BillAddress2 = customerRet.BillAddress.Addr2.GetValue();
						}

						if (customerRet.BillAddress.Addr3 != null)
						{
							BillAddress3 = customerRet.BillAddress.Addr3.GetValue();
						}
						if (customerRet.BillAddress.Addr4 != null)
						{
							BillAddress4 = customerRet.BillAddress.Addr4.GetValue();
						}
						if (customerRet.BillAddress.City != null)
						{
							BillCity = customerRet.BillAddress.City.GetValue();
						}
						if (customerRet.BillAddress.State != null)
						{
							BillState = customerRet.BillAddress.State.GetValue();
						}

						if (customerRet.BillAddress.PostalCode != null)
						{
							BillPostalCode = customerRet.BillAddress.PostalCode.GetValue();
						}
						if (customerRet.BillAddress.Country != null)
						{
							BillCountry = customerRet.BillAddress.Country.GetValue();
						}
					}

					string Phone = string.Empty;
					if (customerRet.Phone != null)
					{
						Phone = customerRet.Phone.GetValue();
					}
					string AltPhone = string.Empty;
					string Fax = string.Empty;
					string Email = string.Empty;
					if (customerRet.Email != null)
					{
						Email = customerRet.Email.GetValue();
					}
					string Cc = string.Empty;
					string Contact = string.Empty;
					string AltContact = string.Empty;
					// customer type ref
					string CustomerTypeName = string.Empty;
					string CustomerTypeRef = string.Empty;
					string TermsRef = string.Empty;
					string TermsName = string.Empty;
					string SalesRepRef = string.Empty;
					string SalesRepName = string.Empty;
					double Balance = 0.0d;
					if (customerRet.Balance != null)
					{
						Balance = customerRet.Balance.GetValue();
					}
					double TotalBalance = 0.0d;
					if (customerRet.TotalBalance != null)
					{
						TotalBalance = customerRet.TotalBalance.GetValue();
					}

					string SalesTaxCodeName = string.Empty;
					string SalesTaxCodeRef = string.Empty;
					string AccountNumber = string.Empty;
					if (customerRet.AccountNumber != null)
					{
						AccountNumber = customerRet.AccountNumber.GetValue();
					}
					string CreditLimit = string.Empty;
					string JobStatus = string.Empty;
					DateTime JobStartDate = default;
					DateTime JobProjectedEndDate = default;
					DateTime JobEndDate = default;
					string JobDesc = string.Empty;
					string JobTypeRef = string.Empty;
					string JobTypeName = string.Empty;
					string Other13 = string.Empty;
					string Other14 = string.Empty;
					string Other15 = string.Empty;
					string Other16 = string.Empty;
					//string TaxRegistrationNumber = string.Empty;    //Need to add in Database table as well
					int DisplayColor = 0;
					//Insert into Database
					db.tblCustomer_Insert(ClearAllControl.gblCompanyID, ListID, TimeCreated, TimeModified, EditSequence, Name, ArName, FullName, Parent, IsActive, int.Parse(Sublevel), CompanyName, Salutation, FirstName, MiddleName, LastName, BillAddress1, BillAddress2, BillAddress3, BillAddress4, BillCityRefKey, BillCity, BillStateRefKey, BillState, BillPostalCode, BillCountryRefKey, BillCountry, Phone, AltPhone, Fax, Email, Cc, Contact, AltContact, CustomerTypeRef, CustomerTypeName, TermsRef, TermsName, SalesRepRef, SalesRepName, (decimal?)Balance, (decimal?)TotalBalance, SalesTaxCodeRef, SalesTaxCodeName, AccountNumber, CreditLimit, JobStatus, Convert.ToString(JobStartDate), Convert.ToString(JobProjectedEndDate), Convert.ToString(JobEndDate), JobDesc, JobTypeRef, JobTypeName, Other13, Other14, Other15, Other16,DisplayColor);
				}

				return;
			}
			catch
            {
				return;
            }
		}

		public class Customer
        {
			public string ListID { get; set; }
			public string Name { get; set; }
			public string Companyname { get; set; }
			public string Email { get; set; }
        }

        private void btnShowall_Click(object sender, RoutedEventArgs e)
        {
			connectToQB();
			//GetCustomerList();
			bError = false;
			GetCustomerList(ref bError);
			if (!bError)
			{
				ds.SendToast("Synchronized ", "Customer List has been synchronized successfully", Haley.Enums.NotificationIcon.Success);
				disconnectFromQB();
				populateDatagrid();
			}
			else
			{
				disconnectFromQB();
				//populateDatagrid();
			}
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

		#endregion


		#region Invoice

		private void syncalInvoice_Click(object sender, RoutedEventArgs e)
		{
			PopulateTempleteCombobox(); 
		}

		private void PopulateTempleteCombobox()
		{
			//string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
			//SqlConnection sqlconn = new SqlConnection(conn);
			//sqlconn.Open();
			//SqlCommand cmd = new SqlCommand("SELECT * FROM Templates where TemplateType ='Sales Invoice' and Status='True'", sqlconn);
			//SqlDataAdapter sdr = new SqlDataAdapter(cmd);
			//DataTable table = new DataTable();
			//sdr.Fill(table);
			//cmbxTmptInvoice.ItemsSource = table.DefaultView;
			//cmbxTmptInvoice.DisplayMemberPath = "TemplateName";
			//cmbxTmptInvoice.SelectedIndex = -1;
		}

        #endregion

    }
}
