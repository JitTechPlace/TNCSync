using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace TNCSync.Sessions
{
    /// <summary>
    ///     Permissible Logging request Priorities
    /// </summary>
    public enum ENLogLevel
    {
        NONE,           //No logginf of any type is done
        CRITICAL,       //Only Exception Logging is undertaken
        ERROR,          //basic logging of error and XML traces
        VERBOSE,       //Full logging of any calls to the logging framework
    }


    /// <summary>
    ///     Summary Description for Logger
    /// </summary>
    public class Logger
    {
        private string _productName = Defaults.APPNAME;
        private string _productVersion = Defaults.PRODUCT_VERSION;
        private static Logger _logger = null;
        private string _logFileName = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Intuit\\QBSDK\\log\\" + Defaults.APPNAME.Replace(" ", "") + "Log.txt";
        private string _oldLogFileName = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Intuit\\QBSDK\\log\\" + Defaults.APPNAME.Replace(" ", "") + "LogOld.txt";
        private int _maxLogFileSize = Defaults.MAX_LOG_FILESIZE;
        private ENLogLevel _logLevel = Defaults.DEFAULT_LOG_LEVEL;
        private StreamWriter _str;
        private const string MaxlogSizekey = "MaxLogFileSize";
        private const string LogLevelKey = "LogLevel";

        #region Public Interface
        //Sets the New logging level
        public ENLogLevel LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
                setLogLevel(_logLevel);
            }
        }

        //Gets the log Filename(Set is not available)
        public string LogFile
        {
            get
            {
                return _logFileName;
            }
        }

        ///Private as this is a singleton. Access through the getlogger() method
        private Logger()
        {
            InitializeLog();
        }

        ///Obtain the logging object
        ///<return> the logging object</return>
        public static Logger getInstance()
        {
            if (_logger == null)
            {
                _logger = new Logger();
            }
            return _logger;
        }

        //Clears the present log file
        public void ClearLog()
        {
            if (File.Exists(_logFileName))
            {
                _str = File.CreateText(_logFileName);
                _str.Close();
            }
        }

        //Initiates Viewing of the log file
        public void viewLog()
        {
            try
            {
                System.Diagnostics.Process.Start(_logFileName);
            }
            catch (Exception eLog)
            {
                if (eLog.Message.IndexOf("The system cannot find the file specified") >= 0)
                {
                    try
                    {
                        StreamWriter SW = File.CreateText(_oldLogFileName);
                        SW.WriteLine("Application Log");
                        SW.WriteLine("-------------------");
                        SW.Close();
                        InitializeLog();
                        System.Diagnostics.Process.Start(_logFileName);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to execute Logger.ViewLog():\n" + eLog.Message);
                    }
                }
                else
                    MessageBox.Show("Unable to execute Logger.ViewLog():\n" + eLog.Message);
            }
        }

        /// <summary>
        ///Attempts to log a Critical message. the Actual logging may or may not occur as a function of the current Log Level.
        ///NOTE: Please use the logCritical(method, text) method wherever possible
        /// </summary>
        /// <param name="logText"></param>
        public void logCritical(string logText)
        {
            logCritical("", logText);
        }

        ///<summary>
        ///Attempts to log a critical message. the actual logging may or may not occur as a function of the current Log Level.
        ///</summary>
        ///<param name="method">The location from which the log message was requested</param>
        ///<param name="logText">The message to log</param>
        public void logCritical(string method, string logText)
        {
            if (_logLevel == ENLogLevel.NONE)
                return;

            log("[CRITICAL]", method, logText);
        }

        ///<summary>
        ///Attempts to log an Error message. The actual logging may or may not occur as a function of the current Log Level.
        ///NOTE: Please use the logError(method, text) method wherever possible
        ///</summary>
        ///<param name="logText">The message to log</param>
        public void logError(string logText)
        {
            logError("", logText);
        }

        ///<summary>
        ///Attempts to log an Error message. The actual logging may or may not occur as a function of the current Log level
        ///</summary>
        ///<param name="method">The location from which the log message was requested</param>
        ///<param name="logText">The message to log</param>
        public void logError(string method, string logText)
        {
            if (_logLevel == ENLogLevel.NONE || _logLevel == ENLogLevel.VERBOSE)
                return;
            log("[ERROE]", method, logText);
        }

        ///<summary>
        ///Attempts to log an Informational message. The actual logging may or may not occur as a function of the current Log Level.
        ///NOTE: Please use the logInfo(method, text) method wherever possible
        ///</summary>
        ///<param name="logText">The message to log</param>
        public void logInfo(string logText)
        {
            logInfo("", logText);
        }

        ///<summary>
        ///Attempts to log an Informational message. The actual logging may or may not occur as a function of the current Log Level.
        ///</summary>
        ///<param name="method"> The location from which the log message was required</param>
        ///<param name="logText">The message to log</param>
        public void logInfo(string method, string logText)
        {
            if (_logLevel != ENLogLevel.VERBOSE)
                return;
            log("[INFO}", method, logText);
        }
        #endregion
        #region Private Utility Functions
        ///<summary>
        ///Main logging Method
        ///</summary>
        ///<param name="type">The type of log messge: [INFO], [ERROR], [CRITICAL]</param>
        ///<param name="method">The method where logging was requested(Allows Tracking)</param>
        ///<param name="logText"> The text of the log message</param>
        private void log(string type, string method, string logText)
        {
            try
            {
                this.checkLogFileSize();
                _str = File.AppendText(_logFileName);
                _str.WriteLine(now() + ":" + type + method + ":" + logText);
                _str.Close();
            }
            catch (Exception eLog)
            {
                if (eLog.Message.IndexOf("Could not find file") >= 0)
                {
                    try
                    {
                        StreamWriter SW = File.CreateText(_logFileName);
                        SW.WriteLine("Application Log");
                        SW.WriteLine("------------");
                        SW.Close();
                        this.InitializeLog();
                    }
                    catch
                    {
                        MessageBox.Show("[Logger.Log] Unable to create save log entry:\n" + eLog.Message + "\n\nLog text:\n" + logText);
                    }
                }
            }
        }

        ///<summary>
        /// Provides the data and time string
        /// </summary>
        /// <returns> the current date and time in UTC: "YYYYMMDD. HH:MM:SS"</returns>
        private string now()
        {
            string yr = null;
            string mon = null;
            string day = null;
            string hr = null;
            string min = null;
            string sec = null;

            System.DateTime dtnow = System.DateTime.Now.ToUniversalTime();
            yr = dtnow.Year.ToString();
            mon = dtnow.Month.ToString();
            if (mon.Length < 2)
            {
                mon = "0" + mon;
            }
            day = dtnow.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }
            hr = dtnow.Hour.ToString();
            if (hr.Length < 2)
            {
                hr = "0" + hr;
            }
            min = dtnow.Minute.ToString();
            if (min.Length < 2)
            {
                min = "0" + min;
            }
            sec = dtnow.Second.ToString();
            if (sec.Length < 2)
            {
                sec = "0" + sec;
            }
            string stamp = yr + mon + day + ".";
            stamp = stamp + hr + ":" + min + ":" + sec + "UTC" + "\t";
            return stamp;
        }

        ///<summary>
        ///Creates a string representing a long version of the time
        /// </summary>
        /// <returns>The long version of the time=</returns>
        private string nowLong()
        {
            System.DateTime dtNow = System.DateTime.Now.ToUniversalTime();
            return dtNow.ToLongDateString() + " - " + dtNow.ToShortDateString() + " UTC ";
        }

        ///<summary>
        ///Ensures that there is sufficient space in the log file
        /// </summary>
        private void checkLogFileSize()
        {
            //Check the size of the QWCLog.txt
            FileInfo fi = new FileInfo(_logFileName);
            long currentLogFileSize = fi.Length;
            int temp = getMaxLogFileSize();

            if(temp == 0)
            {
                if(_logLevel != ENLogLevel.NONE)
                {
                    setLogLevel(ENLogLevel.NONE);
                }
                return;
            }
            if (temp < 0)
            {
                if (_logLevel != ENLogLevel.CRITICAL)
                {
                    setLogLevel(ENLogLevel.CRITICAL);
                }
                return;
            }
            if(currentLogFileSize> _maxLogFileSize)
            {
                //Delete QWCLogOld.txt if any
                try
                {
                    File.Delete(_oldLogFileName);
                }
                catch
                {
                    //Do Nothing
                }

                //rename QWCLog.txt to QWCLogOld.txt
                try
                {
                    File.Move(_logFileName, _oldLogFileName);
                }
                catch
                {
                    //Do Nothing
                }

                //Create a new QWCLog.txt
                try
                {
                    this.InitializeLog();
                }
                catch
                {
                    //Do nothing
                }
            }
        }
        private void InitializeLog()
        {
            string dir = Path.GetDirectoryName(_logFileName);

            //Create directory and file as required
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(_logFileName))
            {
                _str = File.AppendText(_logFileName);
            }
            else
            {
                _str = File.CreateText(_logFileName);
            }

            //Add some initialization string to the log file
            _str.WriteLine("\r\n\r\nLog File Initialized at" + nowLong());
            _str.WriteLine("Timestamp format used: YYYYMMDD. HH:MM:SS UTC");
            string messaging = "";

            //Initialize the looging level and the maximum size
            _logLevel = getLogLevel();
            _maxLogFileSize = getMaxLogFileSize(true);

            _str.WriteLine("\"" + _productName + "\" version: " + _productVersion + ", has been initialized with its logging" + "status to _logLevel = " + _logLevel.ToString() + ".\n" + messaging);
            _str.WriteLine("");
            _str.Close();

            if(_maxLogFileSize <= 1024)
            {
                //If the logging size is pathetically small, it is equivalent to _logLevel = NONE
                if(_logLevel != ENLogLevel.NONE)
                {
                    log("[INFO]", "Logger.checkLogFileSize()", "Log file size only" + _maxLogFileSize + "bytes. setting LogLevel to NONE for no logging.");
                    setLogLevel(ENLogLevel.NONE);
                }
            }
        }

        ///<summary>
        ///Sets the maximum log file size in the registry
        /// </summary>
        /// <param name="fileSize">value to set</param>
        /// <returns>true if the maximum file size was successful saved in the registry</returns>
        private bool setMaxLogFileSize(int fileSize)
        {
            try
            {
                RegistryKey rkQBWC = Registry.CurrentUser.CreateSubKey(Defaults.REG_KEY);
                rkQBWC.SetValue(MaxlogSizekey, fileSize, RegistryValueKind.DWord);
                rkQBWC.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        ///<summary>
        ///Obtains the maximum file size from the registry. If an entry is not found then the default value is saved to the registry.
        ///If an error is detected in any of this, the default value is returned
        /// </summary>
        /// <param name="bStoredValue">True if the registry value should be forcibly set after being retrived(handled default case) </param>
        /// <returns>The maximum file size</returns>
        private int getMaxLogFileSize()
        {
            return getMaxLogFileSize(false);
        }

        ///<summary>
        ///Obtains the maximum file size from the registry. If an entry is not found then the default values is saved to the registry.
        ///If an error is detected in any of this, the default value is returned
        /// </summary>
        ///  /// <param name="bStoredValue">True if the registry value should be forcibly set after being retrived(handled default case) </param>
        /// <returns>The maximum file size</returns>
        private int getMaxLogFileSize(bool bStoreValue)
        {
            int retValue = Defaults.DEFAULT_LOG_FILESIZE;

            try
            {
                //Attempt to get the MaxFileSize from the registry. If the key cannot be found there, Create a new key, setting the value to the default value.
                RegistryKey rkQBWC = Registry.CurrentUser.OpenSubKey(Defaults.REG_KEY, false);
                retValue = (Int32)rkQBWC.GetValue(MaxlogSizekey, Defaults.DEFAULT_LOG_FILESIZE);
                rkQBWC.Close();

                //because the default value will be returned if a no registry entry is found(and well never know about the lack of an enty), set the value in the registry if so commanded
                if (bStoreValue)
                    setMaxLogFileSize(retValue);
            }
            catch
            {
                if (bStoreValue)
                    setMaxLogFileSize(retValue);
            }
            return retValue;
        }

        ///<summary>
        ///Set the logging level in the registry
        /// </summary>
        ///  /// <param name="val">The maximum file size </param>
        /// <returns>true if the logging level was saved in the registry</returns>
        private bool setLogLevel(ENLogLevel val)
        {
            try
            {
                RegistryKey rkQBWC = Registry.CurrentUser.CreateSubKey(Defaults.REG_KEY);
                rkQBWC.SetValue(LogLevelKey, val.ToString(), RegistryValueKind.String);
                rkQBWC.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        ///<summary>
        ///Obtains the logging level from the registry. If an entry is not found then the default level is saved to the registry.
        ///If an erroe is detected in any of this, the default level is returned
        /// </summary>
        /// <returns>The current logging level</returns>
        private ENLogLevel getLogLevel()
        {
            ENLogLevel retValue = Defaults.DEFAULT_LOG_LEVEL;

            try
            {
                //Attempt to get the LogLevel from the registry. if the key cannot be found there, Create a new key, setting the value to the default value.
                RegistryKey rkQBWC = Registry.CurrentUser.OpenSubKey(Defaults.REG_KEY, false);

                if(rkQBWC == null)
                {
                    setLogLevel(retValue);
                    return retValue;
                }

                string retVal = (string)rkQBWC.GetValue(LogLevelKey);
                rkQBWC.Close();

                switch (retVal)
                {
                    case "NONE":
                        retValue = ENLogLevel.NONE;
                        break;
                    case "CRITICAL":
                        retValue = ENLogLevel.CRITICAL;
                        break;
                    case "ERROE":
                        retValue = ENLogLevel.ERROR;
                        break;
                    case "VERBOSE":
                        retValue = ENLogLevel.VERBOSE;
                        break;
                    default:
                        retValue = Defaults.DEFAULT_LOG_LEVEL;
                        setLogLevel(retValue);
                        break;
                }
            }
            catch
            {
                // Dont do anything
            }
            return retValue;
        }
        #endregion

    }
}
