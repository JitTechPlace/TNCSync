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
    public class WriteCheques_DA
    {
        public List<WriteCheques> GetCheques(string payEntityRef)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(Helper.CnnVal("TNCSync_Connection")))
            {
                 var output = connection.Query<WriteCheques>($"SELECT * FROM tblQbCheck where payEntityRef ='{ payEntityRef }'").ToList();
                //var output = connection.Query<WriteCheques>("dbo.tblQBCheck_Select @payEntityRef", new { payEntityRef = payEntityRef }).ToList();
                return output;
            }
        }
    }
}
