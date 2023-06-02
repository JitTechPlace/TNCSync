using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.Class.Info
{
    class VendorListInfo
    {
        private int _companyId;
        private string _listId;
        private string _name;
        private string _companyname;
        private string _billaddress1;
        private string _billaddress2;
        private string _phone;
        private string _email;
        private string _contact;

        public int CompanyId
        {
            get { return _companyId; }
            set { _companyId = value; }
        }

        public string ListId
        {
            get { return _listId; }
            set { _listId = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string CommpanyName
        {
            get { return _companyname; }
            set { _companyname = value; }
        }

        public string BillingAddress1
        {
            get { return _billaddress1; }
            set { _billaddress1 = value; }
        }

        public string BillingAddress2
        {
            get { return _billaddress2; }
            set { _billaddress2 = value; }
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Contact
        {
            get { return _contact; }
            set { _contact = value; }
        }
    }
}
