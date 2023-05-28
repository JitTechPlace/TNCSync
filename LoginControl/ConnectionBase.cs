using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace LoginControl
{
    public abstract class ConnectionBase
    {
        private readonly string _connectionString;

        public ConnectionBase()
        {
            _connectionString = "Server=JITENDHRA\\SQLEXPRESS; Database=CMS; User Id=sa; Password=p@ssw0rd; Integrated Security=false";
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
