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
using TNCSync.Class.SP;
////using TNCSync.Class.QuickBookData;
///
namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for VendorsList.xaml
    /// </summary>
    public partial class VendorsList : UserControl
    {
        public VendorsList()
        {
            InitializeComponent();
        }

        private void syncVendorList_Click(object sender, RoutedEventArgs e)
        {
            populateDatagrid();
            // VendorsList =

            //// string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            //SqlConnection sqlconn = new SqlConnection(conn);
            ////string sqlquery = "SELECT * FROM tblVendor";
            //sqlconn.Open();
            //SqlCommand cmd = new SqlCommand("SELECT * FROM tblVendor",conn);
            //SqlDataReader reader = cmd.ExecuteReader();
            //DataTable table = new DataTable();
            //table.Load(reader);
            //grdVendorLst.dataso
            ////DataTable dt = new DataTable();
            ////SqlDataAdapter sdr = new SqlDataAdapter(sqlquery, sqlconn);
            ////sdr.Fill(dt);
            ////grdVendorLst.DataContext = dt;
            ////sqlconn.Close();
            //////Account acc = new Account();
            //////acc.Accountfetch();
        }

        private void populateDatagrid()
        {
            UserSP spUser = new UserSP();
            grdVendorLst.DataContext = spUser.UserViewOnGridView();
        }
    }
}
