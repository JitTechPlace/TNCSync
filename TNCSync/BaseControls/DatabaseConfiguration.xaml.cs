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
using System.Configuration;


namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for DatabaseConfiguration.xaml
    /// </summary>
    public partial class DatabaseConfiguration : UserControl
    {
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;

        public DatabaseConfiguration()
        {
            InitializeComponent();
            _ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            //Add Default server Name to combobox
            cbxserver.SelectedItem = string.Concat(Environment.MachineName, @"\SQLEXPRESS");
            cbxserver.Items.Add(string.Concat(Environment.MachineName, @"\SQLEXPRESS"));
            //cbxserver.Items.Add(".");
            //cbxserver.Items.Add("(local)");
            //cbxserver.SelectedIndex = 3;
            //cbxserver.Items.Add(@".\SQLEXPRESS");
            //cbxserver.Items.Add(@".\sql2012");
            //cbxAuth.SelectedItem = 
            //ptxtusername.Text = string.Concat
        }

        private IDialogService _ds;
        private bool hideIcon;
        private bool blurOtherWindows;

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            //string _connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", cbxserver.Text, cbxdb.Text, ptxtusername.Text, ptxtpassword.Password);
            if (cbxAuth.Text.Equals("Windows Authentication"))
            {
                string _connectionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security = TRUE;", cbxserver.Text, cbxdb.Text);
                try
                {
                    RepositoryBase rb = new RepositoryBase(_connectionString);
                    if (rb.IsConnected)
                    {
                        //MessageBox.Show("Test Connection Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                       // _ds.Success("Test Connection Succeed", "Application Now connected to Current Company File ");
                        DialogMode dialogMode = DialogMode.Confirmation;
                        var result = _ds.ShowDialog("Test Connection Succeed", "Application Now connected to Current Company File", Haley.Enums.NotificationIcon.Success, dialogMode, hideIcon = false, blurOtherWindows = true);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Test Connection Failed", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    //_ds.Warning("Test Connection Failed", "Application not able to connect with Current Company File ");
                    DialogMode dialogMode = DialogMode.Confirmation;
                    var result = _ds.ShowDialog("Test Connection Failed", ex.Message, Haley.Enums.NotificationIcon.Warning, dialogMode, hideIcon = false, blurOtherWindows = true);
                }
            }
            else if (cbxAuth.Text.Equals("SQL Server Authentication"))
            {
                string _connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", cbxserver.Text, cbxdb.Text, ptxtusername.Text, ptxtpassword.Password);
                try
                {
                    RepositoryBase rb = new RepositoryBase(_connectionString);
                    if (rb.IsConnected)
                    {
                        //MessageBox.Show("Test Connection Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                       // _ds.Success("Test Connection Succeed", "Application Now connected to Current Company File ");
                        DialogMode dialogMode = DialogMode.Confirmation;
                        var result = _ds.ShowDialog("Test Connection Succeed", "Application Now connected to Current Company File", Haley.Enums.NotificationIcon.Success, dialogMode, hideIcon = false, blurOtherWindows = true);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Test Connection Failed", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                   // _ds.Warning("Test Connection Failed", "Application not able to connect with Current Company File "); 
                    DialogMode dialogMode = DialogMode.Confirmation;
                    var result = _ds.ShowDialog("Test Connection Failed", ex.Message, Haley.Enums.NotificationIcon.Warning, dialogMode, hideIcon = false, blurOtherWindows = true);
                }
            }
            #region OldConnect
            //try
            //{
            //    RepositoryBase rb = new RepositoryBase(_connectionString);
            //    if (rb.IsConnected)
            //    {
            //        //MessageBox.Show("Test Connection Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            //        _ds.Success("Test Connection Succeed", "Application Now connected to Current Company File ");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show("Test Connection Failed", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
            //    _ds.Warning("Test Connection Failed", "Application not able to connect with Current Company File ");
            //}
            #endregion
        }
        /// <summary>
        /// It save the Correct connection in your App.config file for later 
        /// </summary>to delivre xml.
        /// <param name="sender"></param>that could be resolved version public token version=7..0.1
        /// <param name="e"></param>
        private void btnSavecon_Click(object sender, RoutedEventArgs e)
        {
            //Windows Authentication Save
            if (cbxAuth.Text.Equals("Windows Authentication"))
            {
                //Set connection string
                string _connectionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security = TRUE;", cbxserver.Text, cbxdb.Text);
                try
                {
                    RepositoryBase rb = new RepositoryBase(_connectionString);
                    if (rb.IsConnected)
                    {
                        AppSetting setting = new AppSetting();
                        setting.SaveConnectionString("TNCSync_Connection", _connectionString);
                        // MessageBox.Show("Test Connection Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                        //_ds.Success("Connection Saved", "Now you can access QB Data");
                        DialogMode dialogMode = DialogMode.Confirmation;
                        var result = _ds.ShowDialog("Connection Saved", "Now you can access QB Data", Haley.Enums.NotificationIcon.Success, dialogMode, hideIcon = false, blurOtherWindows = true);
                    }
                }
                catch (Exception ex)
                {
                    //_ds.Warning("Unable to save the Connection", "Please check the connection details");
                    DialogMode dialogMode = DialogMode.Confirmation;
                    var result = _ds.ShowDialog("Unable to save the Connection", ex.Message, Haley.Enums.NotificationIcon.Warning, dialogMode, hideIcon = false, blurOtherWindows = true);
                }
            }
            //SQL Server Authentication Save
            else if (cbxAuth.Text.Equals("SQL Server Authentication"))
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
                        setting.SaveConnectionString("TNCSync.Properties.Settings.CMSConnectionString", _connectionString);
                        DialogMode dialogMode = DialogMode.Confirmation;
                        var result = _ds.ShowDialog("Connection Saved", "Now you can access QB Data", Haley.Enums.NotificationIcon.Success, dialogMode, hideIcon = false, blurOtherWindows = true);
                    }
                }
                catch (Exception ex)
                {
                    DialogMode dialogMode = DialogMode.Confirmation;
                    var result = _ds.ShowDialog("Unable to save the Connection", ex.Message, Haley.Enums.NotificationIcon.Warning, dialogMode, hideIcon = false, blurOtherWindows = true);
                }
            }

            #region old SaveConnection
            //string _connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", cbxserver.Text, cbxdb.Text, ptxtusername.Text, ptxtpassword.Password);
            //try
            //{
            //    RepositoryBase rb = new RepositoryBase(_connectionString);
            //    if (rb.IsConnected)
            //    {
            //        AppSetting setting = new AppSetting();
            //        setting.SaveConnectionString("TNCSync_Connection", _connectionString);
            //       // MessageBox.Show("Test Connection Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            //        _ds.Success("Connection Saved","Now you can access QB Data");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show("Test Connection Failed", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
            //    _ds.Warning("Unable to save the Connection", "Please check the connection details");
            //}
            #endregion
        }
        /// <summary>
        /// If the above Authentication Details are correct it can load the Database that are available with those Connections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDatabae_DropDownOpened(object sender, EventArgs e)
        {
            cbxdb.Items.Clear();
            try
            {
                if (cbxAuth.Text.Equals("Windows Authentication"))
                {
                    SQLConnection.ConnectionString = @"Server = " + cbxserver.Text + "; Integrated Security = TRUE;";
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
                DialogMode dialogMode = DialogMode.Confirmation;
                var result = _ds.ShowDialog("Unable to Fetch Database", "Please check your Windows or Sql Authentication Details", Haley.Enums.NotificationIcon.Warning, dialogMode, hideIcon = false, blurOtherWindows = true);
            }
        }
        /// <summary>
        /// Enable and Disable the Authentication textbox Depending up on the selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void DBConfigure_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
