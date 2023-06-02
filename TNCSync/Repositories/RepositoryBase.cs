using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;

namespace TNCSync.Repositories
{
    public class RepositoryBase
    {
        SqlConnection TNCSync_Connection;
        //private readonly string _connectionString;

        public RepositoryBase(string connectionString)
        {
            TNCSync_Connection = new SqlConnection(connectionString);
            //connectionString = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString; //Check App.config

        }
        public bool IsConnected
        {
            get
            {
                if (TNCSync_Connection.State == System.Data.ConnectionState.Closed)
                    TNCSync_Connection.Open();
                return true;
            }
        }
        //protected SqlConnection GetConnection()
        //{
        //    return new SqlConnection(_connectionString);
        //}
    }
}
