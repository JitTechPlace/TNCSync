using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using TNCSync.Class.Info;
using Haley.Abstractions;

namespace TNCSync.Class.SP
{

    class UserSP : DBConnection
    {
        private IDialogService _ds;
        public UserInfo UserView(string username)
        {
            UserInfo userinfo = new UserInfo();
            SqlDataReader sdrreader = null;
            try
            {
                if(sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlCommand scmd = new SqlCommand("UserView", sqlcon);
                scmd.CommandType = CommandType.StoredProcedure;
                SqlParameter sparam = new SqlParameter();
                sparam = scmd.Parameters.Add("@username", SqlDbType.VarChar);
                sparam.Value = username;
                sdrreader = scmd.ExecuteReader();
                while (sdrreader.Read())
                {
                    userinfo.ID = int.Parse(sdrreader[0].ToString());
                    userinfo.CompanyId = int.Parse(sdrreader[1].ToString());
                    userinfo.UserName = sdrreader[2].ToString();
                    userinfo.LoginName = sdrreader[3].ToString();
                    userinfo.Password = sdrreader[4].ToString();
                    userinfo.Sex = sdrreader[5].ToString();
                    userinfo.DOB = DateTime.Parse(sdrreader[6].ToString());
                    userinfo.CreatedDate = DateTime.Parse(sdrreader[7].ToString());
                    userinfo.Email = sdrreader[8].ToString();
                    userinfo.Contect = sdrreader[9].ToString();
                    userinfo.IsActive = char.Parse(sdrreader[10].ToString());
                }
            }
            catch(Exception ex)
            {
                _ds.SendToast(ex.ToString(),"",Haley.Enums.NotificationIcon.Error);
            }
            finally
            {
                sdrreader.Close();
                sqlcon.Close();
            }
            return userinfo;
        }
    }
}
