using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Interop.QBFC15;
using Interop.QBFC16;

namespace TNCSync.Sessions
{
    static class Defaults
    {
        #region Connection and Session Variables
        //The default connection type used when creating a connection with QuickBooks
        //This may be manually overridden with in the initalize methods parameters.
        public const ENConnectionType CONNECTION_TYPE = ENConnectionType.ctLocalQBD;

        //The Default Connection mode used when opening a session with quickbooks.
        //A different value can be manually provided with in the SessionManager. beginsession method
        public const ENOpenMode SESSION_MODE = ENOpenMode.omDontCare;

        //The Edition of Quickbooks for which this is designed to be used.
        public const ENEdition EDITION = ENEdition.edUS;

        //The full path to the company file that should be used when creatinf a session with quickbooks. this is only when running in unattended Mode.
        public const string QBFILE = "";
        #endregion

        #region Logging Variables
        //Product Version String
        public const string PRODUCT_VERSION = "";

        //The maximum size to which a log file can grow before it is remanmed xxxOLD and a new file is created
        public const int MAX_LOG_FILESIZE = 100 * 1024;

        //The default sizze of the log file
        public const int DEFAULT_LOG_FILESIZE = 50 * 1024;

        //The default Logging level
        public const ENLogLevel DEFAULT_LOG_LEVEL = ENLogLevel.CRITICAL;
        #endregion


        #region Automatically Generated(During the Wizard)variables

        //the application name used when opening a connection with quickbooks
        public const string APPNAME = "TNCSync";

        //The Application ID, obtained from Intuti for you specific application, used when opening a connection with Quickbooks
        public const string APPID = "";

        // The ID used ti uniquely identify the QBFC application when subscribing to events with in the QuickBooks
        public static readonly Guid SUBSCRIBER_ID = new Guid("");

        //The Registration Key for your application within the Microsoft Registry
        public const string REG_KEY = "Software\\IDN\\TNCSync";

        #endregion
    }
}
