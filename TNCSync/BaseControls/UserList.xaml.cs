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
using TNCSync.View;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for UserList.xaml
    /// </summary>
    public partial class UserList : UserControl
    {
        public UserList()
        {
            InitializeComponent();
           // populateDatagrid();
        }

        private void syncusrList_Click(object sender, RoutedEventArgs e)
        {
            populateDatagrid();
        }

        private void populateDatagrid()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            //string sqlquery = "SELECT * FROM tblVendor";
            SqlCommand cmd = new SqlCommand("SELECT * FROM UserLogin", sqlconn);
            sqlconn.Open();
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            grdUsersLst.ItemsSource = table.DefaultView;
            sqlconn.Close();
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            AddNewUser addUser = new AddNewUser();
            addUser.ShowDialog();
        }
    }
}
