using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using TNCSync.Class.DataBaseClass;
using System.Configuration;

namespace TNCSync.Class
{
    public class SQLControls
    {
        private SqlConnection sqlcon = new SqlConnection(ClearAllControl.gblConnectionString);
        private SqlCommand sqlcmd;
        public SqlDataAdapter sqlda = new SqlDataAdapter();
        public DBTNCSDataContext dbclass = new DBTNCSDataContext();
        public List<SqlParameter> param = new List<SqlParameter>();
        public int recordcount;
        public string exception;
        public DataSet sqlds;

        public void execquery(string query)
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            //using (var sqlcon = new SqlConnection(ClearAllControl.gblConnectionString))
            //{
            sqlconn.Open();
            sqlcmd = new SqlCommand(query, sqlconn);
            param.ForEach(x => sqlcmd.Parameters.Add(x));
            param.Clear();
            sqlds = new DataSet();
            sqlda = new SqlDataAdapter(sqlcmd);
            recordcount = sqlda.Fill(sqlds);
            // }
        }

        public void addparam(string name, object value)
        {
            var newparam = new SqlParameter(name, value);
            param.Add(newparam);
        }
    }
}
