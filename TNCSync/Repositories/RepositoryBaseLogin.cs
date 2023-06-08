using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;

namespace TNCSync.Repositories
{
    public class RepositoryBaseLogin
    {
        SqlConnection TNCSync_Connection;
        private readonly string _Connstring;

        public RepositoryBaseLogin()
        {
            _Connstring = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString; //Check App.config

            //TNCSync_Connection = new SqlConnection(_Connstring);

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
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_Connstring);
        }

    }
}
