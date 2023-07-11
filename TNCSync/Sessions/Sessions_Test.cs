using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Haley.Abstractions;
using Haley.MVVM;
using Interop.QBFC15;
//using Interop.QBFC12;
using Microsoft.VisualBasic;
using TNCSync.Class;
using TNCSync.Class.DataBaseClass;


namespace TNCSync.Sessions
{
     public class Sessions
    {
        private bool booSessionBegun;
        private QBSessionManager qbSessionManager;
        private IMsgSetRequest msgSetRequest;
        private DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService _ds;

        public void OpenConnectionBeginSession()
        {

            booSessionBegun = false;
           // _ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            try
            {

                qbSessionManager = new QBSessionManager();

                var companyresult = db.tblCompanySelect(Convert.ToInt16(ClearAllControl.gblCompanyID), null, null);
                string qbfile = "";
                string qbCompanyName = @"C:\Users\Public\Documents\Intuit\QuickBooks\Sample Company Files\QuickBooks Enterprise Solutions 13.0\Test for Integration.qbw";
                foreach (tblCompanySelectResult company in companyresult)
                {
                    qbfile = company.QBFileName;
                    qbCompanyName = company.QBCompanyName;
                }
                qbSessionManager.OpenConnection("124", qbCompanyName);
                // Dim qbfile As String = "C:\Users\Public\Documents\Intuit\QuickBooks\Sample Company Files\QuickBooks Enterprise Solutions 11.0\sample_product-based business.qbw"
                if (!string.IsNullOrEmpty(qbfile))
                {
                    qbSessionManager.BeginSession(qbfile, ENOpenMode.omDontCare);
                }
                else
                {
                   MessageBox.Show("Please setup company first", "TNC-CHECK MANAGEMENT SYSTEM");
                   // _ds.ShowDialog("TNC-Sync", "Please setup company first", Haley.Enums.NotificationIcon.Error);
                    return;
                }


                // qbSessionManager.BeginSession("", ENOpenMode.omDontCare)
                booSessionBegun = true;

                // Check to make sure the QuickBooks we're working with supports version 2 of the SDK
                string[] strXMLVersions;
                strXMLVersions = qbSessionManager.QBXMLVersionsForSession;

                bool booSupports2dot0;
                booSupports2dot0 = false;
                long i;
                var loopTo = (long)Information.UBound(strXMLVersions);
                for (i = Information.LBound(strXMLVersions); i <= loopTo; i++)
                {
                    if (strXMLVersions[(int)i] == "2.0")
                    {
                        booSupports2dot0 = true;
                        msgSetRequest = qbSessionManager.CreateMsgSetRequest("US", 15, 0);
                        break;
                    }
                }

                if (!booSupports2dot0)
                {
                    Interaction.MsgBox("This program only runs against QuickBooks installations which support the 2.0 qbXML spec.  Your version of QuickBooks does not support qbXML 2.0");
                    //_ds.ShowDialog("TNC-Sync", "This program only runs against QuickBooks installations which support the 2.0 qbXML spec.  Your version of QuickBooks does not support qbXML 2.0", Haley.Enums.NotificationIcon.Error);
                    Environment.Exit(0);
                }
                return;
            }
            catch
            {

                if (Information.Err().Number == int.MinValue + 0x00040416)
                {
                    Interaction.MsgBox("You must have QuickBooks running with the company" + Constants.vbCrLf + "file open to use this program.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040422)
                {
                    Interaction.MsgBox("This QuickBooks company file is open in single user mode and" + Constants.vbCrLf + "another application is already accessing it.  Please exit the" + Constants.vbCrLf + "other application and run this application again.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040401)
                {
                    Interaction.MsgBox("Could not access QuickBooks (Failure in attempt to connection)");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040403)
                {
                    Interaction.MsgBox("Could not open the specified QuickBooks company data file");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040407)
                {
                    Interaction.MsgBox("The installation of QuickBooks appears to be incomplete." + Constants.vbCrLf + "Please reinstall QuickBooks.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040408)
                {
                    Interaction.MsgBox("Could not start QuickBooks." + Constants.vbCrLf + "Try Again");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x0004040A)
                {
                    Interaction.MsgBox("QuickBooks company data file is already open and it " + Constants.vbCrLf + "is different from the one requested.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x0004041A)
                {
                    Interaction.MsgBox("This application does not have permission to access this QuickBooks company data file" + Constants.vbCrLf + "Grant access permission through the Integrated Application preferences.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040420)
                {
                    Interaction.MsgBox("The QuickBooks user has denied access.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040424)
                {
                    Interaction.MsgBox("QuickBooks did not finish its initialization." + Constants.vbCrLf + "Please try again later.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
                else if (Information.Err().Number == int.MinValue + 0x00040427)
                {
                    Interaction.MsgBox("Your QuickBooks application needs to be registered.");
                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }

                else
                {
                    Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in OpenConnectionBeginSession");

                    if (booSessionBegun)
                    {
                        qbSessionManager.EndSession();
                    }

                    qbSessionManager.CloseConnection();
                    return;
                    Environment.Exit(0);
                }
            }
        }

        public void EndSessionCloseConnection()
        {
            try
            {

                if (booSessionBegun)
                {
                    qbSessionManager.EndSession();
                    qbSessionManager.CloseConnection();
                }

                return;
            }
            catch
            {

                Interaction.MsgBox("HRESULT = " + Information.Err().Number + " (" + Conversion.Hex(Information.Err().Number) + ") " + Constants.vbCrLf + Constants.vbCrLf + Information.Err().Description, MsgBoxStyle.Critical, "Error in EndSessionCloseConnection");

            }
        }


        public IMsgSetRequest GetLatestMsgSetRequest(QBSessionManager SessionManager)
        {
            IMsgSetRequest GetLatestMsgSetRequestRet = default;
            double supportedVersion;
            supportedVersion = double.Parse(QBFCLatestVersion(SessionManager));
            if (supportedVersion >= 6.0d)
            {
                GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 6, 0);
            }
            else if (supportedVersion >= 5.0d)
            {
                GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 5, 0);
            }
            else if (supportedVersion >= 4.0d)
            {
                GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 4, 0);
            }
            else if (supportedVersion >= 3.0d)
            {
                GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 3, 0);
            }
            else if (supportedVersion >= 2.0d)
            {
                GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 2, 0);
            }
            else if (supportedVersion == 1.1d)
            {
                GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 1, 1);
            }
            else
            {
                Interaction.MsgBox("You are apparently running QuickBooks 2002 Release 1, we strongly recommend that you use QuickBooks' online update feature to obtain the latest fixes and enhancements", Constants.vbExclamation);
                GetLatestMsgSetRequestRet = SessionManager.CreateMsgSetRequest("US", 1, 0);
            }

            return GetLatestMsgSetRequestRet;
        }
        public string QBFCLatestVersion(QBSessionManager SessionManager)
        {
            string QBFCLatestVersionRet = default;
            string[] strXMLVersions;
            // Should be able to use this, but there appears to be a bug that may cause 2.0 to be returned
            // when it should not.
            // strXMLVersions = SessionManager.QBXMLVersionsForSession

            IMsgSetRequest msgset;
            // Use oldest version to ensure that we work with any QuickBooks (US)
            msgset = SessionManager.CreateMsgSetRequest("US", 1, 0);
            msgset.AppendHostQueryRq();
            IMsgSetResponse QueryResponse;
            QueryResponse = SessionManager.DoRequests(msgset);
            IResponse response;

            // The response list contains only one response,
            // which corresponds to our single HostQuery request
            response = QueryResponse.ResponseList.GetAt(0);
            IHostRet HostResponse;
            HostResponse = (IHostRet)response.Detail;
            IBSTRList supportedVersions;
            supportedVersions = HostResponse.SupportedQBXMLVersionList;

            long i;
            double vers;
            double LastVers;
            LastVers = 0d;
            var loopTo = (long)(supportedVersions.Count - 1);
            for (i = 0L; i <= loopTo; i++)
            {
                vers = Conversion.Val(supportedVersions.GetAt((int)i));
                if (vers > LastVers)
                {
                    LastVers = vers;
                    QBFCLatestVersionRet = supportedVersions.GetAt((int)i);
                }
            }

            return QBFCLatestVersionRet;
        }
    }
}
