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
    public class VendorsList_DA
    {
        public List<VendorList> GetVendorList(string Name)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(Helper.CnnVal("TNCSync_Connection")))
            {
                var output = connection.Query<VendorList>("dbo.tblVendor_Select_TNCS_Param @Name", new { Name = Name }).ToList();
                return output;
            }
        }
    }
}
