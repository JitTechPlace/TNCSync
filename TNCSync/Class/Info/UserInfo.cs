using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.Class.Info
{
    class UserInfo
    {
        private int _id;
        private int _companyId;
        private string _userName;
        private string _loginName;
        private string _password;
        private string _sex;
        private DateTime _dob;
        private DateTime _createdDate;
        private string _email;
        private string _contact;
        private char _isactive;

        public int ID 
        {
            get { return _id; }
            set { _id = value; }
        }

        public int CompanyId
        {
            get { return _companyId; }
            set { _companyId = value; }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public string LoginName
        {
            get { return _loginName; }
            set { _loginName = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string Sex
        {
            get { return _sex; }
            set { _sex = value; }
        }

        public DateTime DOB
        {
            get { return _dob; }
            set { _dob = value; }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Contect
        {
            get { return _contact; }
            set { _contact = value; }
        }

        public char IsActive
        {
            get { return _isactive; }
            set { _isactive = value; }
        }
    }
}
