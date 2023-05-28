using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNCSync.Model;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using LoginControl.Controls;

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
            UserModel user = null;
            using (var connection = GetConnection())
            using (var command = new SqlCommand("dbo.getLoginDetails", connection))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Loginname", SqlDbType.NVarChar).Value = credential.UserName;
                command.Parameters.Add("@Password", SqlDbType.NVarChar).Value = credential.Password;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            LoginName = reader[1].ToString(),
                            Password = reader[2].ToString(),
                            CompanyName = reader[3].ToString(),
                        };
                    }
                }
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
            using(var command = new SqlCommand("dbo.getLoginDetails",connection))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Loginname", SqlDbType.NVarChar).Value = username;
                using(var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            Id = reader[0].ToString(),
                            LoginName = reader[1].ToString(),
                            Password = string.Empty,
                            CompanyName = reader[3].ToString(),
                            Username = reader[4].ToString(),
                            Email = reader[5].ToString(),
                        };
                    }
                }
            }
            return user;
        }

        private void populateCompany()
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand("dbo.getCompanyName",connection))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader sdr = command.ExecuteReader();
            }
        }
        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
