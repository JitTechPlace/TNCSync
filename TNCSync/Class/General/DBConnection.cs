using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace TNCSync.Class
{
    public class DBConnection
    {
        protected SqlConnection sqlcon;

        public DBConnection()
        {
            string path = Common.strPath + "";
            string strServer = "JITENDHRA\\SQLEXPRESS";
            if (File.Exists(path + "\\sys.txt"))
            {
                strServer = File.ReadAllText(path + "\\sys.txt"); //getting IP of server
            }

            sqlcon = new SqlConnection(@"Data Source=" + strServer + ";AttachDbFilename=" + path + ";Integrated Security=True;Connect Timeout=120;User Instance=True");
        }
    }
}
