using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace TNCSync.Class
{
    class Common
    {
        #region Variables
        private static string _strPath;


        public static string strPath
        {
            get { return _strPath; }
            set { _strPath = value; }
        }
        #endregion
    }
}
