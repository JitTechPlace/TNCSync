using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNCSync.Model;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace TNCSync.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public void Add(UserModel userModel)
        {
            throw new NotImplementedException();
        }
        public bool AuthenticateUser(NetworkCredential credential)
        {
            bool validUser;
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select * from [UserLogin] where Loginname=@Loginname and [Password]=@Password";
                command.Parameters.Add("@Loginname", SqlDbType.NVarChar).Value = credential.UserName;
                command.Parameters.Add("@Password", SqlDbType.NVarChar).Value = credential.Password;
                validUser = command.ExecuteScalar() == null ? false : true;
            }
            return validUser;
        }
        public void Edit(UserModel userModel)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<UserModel> GetByAll()
        {
            throw new NotImplementedException();
        }
        public UserModel GetById(int id)
        {
            throw new NotImplementedException();
        }
        public UserModel GetByUsername(string username)
        {
            UserModel user = null;
            using(var connection = GetConnection())
            using(var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select [Loginname], [Password], tblCompany.CompanyName, tblCompany.email from [UserLogin] INNER JOIN [tblCompany] ON UserLogin.companyID = tblCompany.ID WHERE tblCompany.isActive = 'Y';";
                command.Parameters.Add("@Loginname", SqlDbType.NVarChar).Value = username;
                using(var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            //Id = reader[0].ToString(),
                            //LoginName = reader[1].ToString(),
                            //Password = string.Empty,
                            //CompanyName = reader[3].ToString(),
                            //Username = reader[4].ToString(),
                            //Email = reader[5].ToString(),
                        };
                    }
                }
            }
            return user;
        }
        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
