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
    public class BillPaymentCheques_DA
    {
        public List<BillPaymentCheque> GetCheques(string payeeFullName)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(Helper.CnnVal("TNCSync_Connection")))
            {
                // var output = connection.Query<WriteCheques>($"SELECT * FROM tblQbCheck where payEntityRef ='{ payEntityRef }'").ToList();
                var output = connection.Query<BillPaymentCheque>("dbo.tblQBBillPayCheckP_Select @payeeFullName", new { payeeFullName = payeeFullName }).ToList();
                return output;
            }
        }
    }
}
