using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace TNCSync
{
    public class AppSetting
    {
        Configuration Config;

        public AppSetting()
        {
            Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        //Get Connection string from APP.Config file
        public string GetConnectionString(string key)
        {
            return Config.ConnectionStrings.ConnectionStrings[key].ConnectionString;
        }


        //Save connection string to App.config file
        public void SaveConnectionString(string key, string value)
        {
            Config.ConnectionStrings.ConnectionStrings[key].ConnectionString = value;
            Config.ConnectionStrings.ConnectionStrings[key].ProviderName = "System.Data.SqlClient";
            Config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
