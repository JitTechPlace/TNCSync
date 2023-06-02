using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync
{
    internal class CredentialHolder
    {
        internal bool IsAuthenticated { get; set; }

        #region Constructor
        public static CredentialHolder Singleton = new CredentialHolder();
        public static CredentialHolder getSingleton()
        {
            if (Singleton == null) Singleton = new CredentialHolder();
            return Singleton;
        }

        public static void Clear()
        {
            Singleton = new CredentialHolder();
        }
        private CredentialHolder() { }
        #endregion
    }

    //Temp Added--31-05-2023
    public static class SQLConnection
    {
        public static string ConnectionString { get; set; }
    }
}
