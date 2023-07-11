using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using TNCSync.ViewModel;

namespace TNCSync.View
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
        }

        private void PopulateDataGrid()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            //string sqlquery = "SELECT * FROM tblVendor";
            SqlCommand cmd = new SqlCommand("SELECT count(Loginname) FROM UserLogin WHERE IsActive = '1'", sqlconn);
            sqlconn.Open();
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            DataContext = table;
            sqlconn.Close();
            txtuser.Text = table.ToString();
        }

        private void Dashboard_UC_Loaded(object sender, RoutedEventArgs e)
        {
           // PopulateDataGrid();
        }
    }
}
