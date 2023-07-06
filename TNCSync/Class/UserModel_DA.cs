using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNCSync.Model;
using Dapper;
using System.Data;

namespace TNCSync.Class
{
    public class UserModel_DA
    {
        public List<UserModel> GetUser(string CompanyName)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(Helper.CnnVal("TNCSync_Connection")))
            {
                    var output = connection.Query<UserModel>("dbo.UserLogin_SelectAll_TNCS @CompanyName, @Password", new { CompanyName = CompanyName }).ToList();
                return output;
            }
        }
    }
}
