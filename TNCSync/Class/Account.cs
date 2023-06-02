using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Interop.QBFC15;

namespace TNCSync.Class
{
   public class Account
    {
        public void Accountfetch()
        {
            bool sessionBegun = false;
            bool connactionOpen = false;
            QBSessionManager sessionManager = null;

            try
            {
                //Crete the session manager
                sessionManager = new QBSessionManager();

                //Create the message set request to hold our request
                IMsgSetRequest reqMsgSet = sessionManager.CreateMsgSetRequest("US", 23, 0);
                reqMsgSet.Attributes.OnError = ENRqOnError.roeContinue;
                BuildAccountFetchRq(reqMsgSet);
                //Connect to QB and begin a session
                sessionManager.OpenConnection("", "TNCSync");
                connactionOpen = true;
                sessionManager.BeginSession("", ENOpenMode.omDontCare);
                sessionBegun = true;
                //Send the request and get the response from QB
                IMsgSetResponse respMsgSet = sessionManager.DoRequests(reqMsgSet);
                //End the session and close the connection to QB
                sessionManager.EndSession();
                sessionBegun = false;
                sessionManager.CloseConnection();
                connactionOpen = false;
                WalkAccountfetchRs(respMsgSet);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                if (sessionBegun)
                {
                    sessionManager.EndSession();
                }
                if (connactionOpen)
                {
                    sessionManager.CloseConnection();
                }
            }
        }

        public void BuildAccountFetchRq(IMsgSetRequest requrequestMsgSet)
        {
            IAccountQuery accountQryRq = requrequestMsgSet.AppendAccountQueryRq();
            //Set attributes
            //Set field value for metadata
            //accountQryRq.metaData.SetValue("IQBENmetaDataType");
        }

        public void WalkAccountfetchRs(IMsgSetResponse responseMsgSet)
        {

        }
    }
}
