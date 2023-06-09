using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TNCSync.Repositories;
using Haley.Abstractions;
using Haley.Enums;
using System.Data.SqlClient;
using System.Data;
using Haley.MVVM;
using Microsoft.Win32;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for DatabaseConfiguration.xaml
    /// </summary>
    public partial class DatabaseConfiguration : UserControl
    {
        public RegistryKey regkey;
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;

        public DatabaseConfiguration()
        {
            InitializeComponent();
            _ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            //Add server Name to combobox
            cbxserver.SelectedItem = string.Concat(Environment.MachineName, @"\SQLEXPRESS");
            cbxserver.Items.Add(".");
            cbxserver.Items.Add("(local)");
            cbxserver.Items.Add(@".\SQLEXPRESS");
            cbxserver.Items.Add(string.Format(@"{0}\SQLEXPRESS", Environment.MachineName));
            cbxserver.SelectedIndex = 3;
        }
        private IDialogService _ds;

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string _connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", cbxserver.Text, cbxdb.Text, ptxtusername.Text, ptxtpassword.Password);
            try
            {
                RepositoryBase rb = new RepositoryBase(_connectionString);
                if (rb.IsConnected)
                {
                    //MessageBox.Show("Test Connection Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                    _ds.Success("Test Connection Succeed", "Application Now connected to Current Company File ");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Test Connection Failed", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                _ds.Warning("Test Connection Failed", "Application not able to connect with Current Company File ");
            }
        }

        private void btnSavecon_Click(object sender, RoutedEventArgs e)
        {
            //Set connection string
            string _connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", cbxserver.Text, cbxdb.Text, ptxtusername.Text, ptxtpassword.Password);
            try
            {
                RepositoryBase rb = new RepositoryBase(_connectionString);
                if (rb.IsConnected)
                {
                    AppSetting setting = new AppSetting();
                    setting.SaveConnectionString("TNCSync_Connection", _connectionString);
                   // MessageBox.Show("Test Connection Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                    _ds.Success("Connection Saved","Now you can access QB Data");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Test Connection Failed", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                _ds.Warning("Unable to save the Connection", "Please check the connection details");
            }
        }

        private void cmbDatabae_DropDownOpened(object sender, EventArgs e)
        {
            cbxdb.Items.Clear();
            try
            {
                if (cbxAuth.Text.Equals("Windows Authentication"))
                {
                    SQLConnection.ConnectionString = @"Server = " + cbxserver.Text + "; Integrated Security = SSPI;";
                    con.ConnectionString = SQLConnection.ConnectionString;
                }
                else if (cbxAuth.Text.Equals("SQL Server Authentication"))
                {
                    SQLConnection.ConnectionString = @"Server = " + cbxserver.Text + "; User Id =" + ptxtusername.Text + "; Password=" + ptxtpassword.Password + ";";
                    con.ConnectionString = SQLConnection.ConnectionString;
                }
                con.Open();
                com.Connection = con;
                com.CommandText = "SELECT DB_NAME(database_id) AS[Database], database_id FROM sys.databases; ";
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    cbxdb.Items.Add(dr["Database"].ToString());
                }
                con.Close();
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message, "OOPSSss!", MessageBoxButton.OK, MessageBoxImage.Error);
                _ds.Error("Unable to Fetch Database", "Please check your Windows or Sql Authentication Details");
            }
        }

        private void cbxAuth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxAuth.SelectedItem.ToString().Contains("Windows Authentication"))
            {
                ptxtusername.IsEnabled = false;
                ptxtpassword.IsEnabled = false;
            }
            else if (cbxAuth.SelectedItem.ToString().Contains("SQL Server Authentication"))
            {
                ptxtusername.IsEnabled = true;
                ptxtpassword.IsEnabled = true;
            }
        }
    }
}
