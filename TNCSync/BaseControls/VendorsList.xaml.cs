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
using TNCSync.ViewModel;
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
            List<Vendor> vendors = new List<Vendor>();
            vendors.Add(new Vendor()
            {
                Name = "Jitendhra",
                CompanyName = "TNC",
                Address = "Dubai",
                Phone = 0551533823,
                Email = "jitendhra@tnc-me.com",
                Contact = "Admin"
            });
            vendors.Add(new Vendor()
            {
                Name = "Vaibhav",
                CompanyName = "TNC",
                Address = "Dubai",
                Phone = 0551533823,
                Email = "Vaibhav@tnc-me.com",
                Contact = "Admin"
            });
            grdVendorLst.ItemsSource = vendors;
        }

        private void syncVendorList_Click(object sender, RoutedEventArgs e)
        {
            //grdVendorLst.ItemsSource = Vendor();
            //grdVendorLst.AutoGenerateColumns = false;
            //populateDatagrid();
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
           
        }
        public class Vendor
        {
            public string Name { get; set; }
            public string CompanyName { get; set; }
            public string Address { get; set; }
            public int Phone { get; set; }
            public string Email { get; set; }
            public string Contact { get; set; }

        }

        //private List<Vendor> LoadCollectionData()
        //{
        //    List<Vendor> vendors = new List<Vendor>();
        //    vendors.Add(new Vendor()
        //    {
        //        Name = "Jitendhra",
        //        CompanyName = "TNC",
        //        Address = "Dubai",
        //        Phone = 0551533823,
        //        Email = "jitendhra@tnc-me.com",
        //        Contact = "Admin"
        //    });
        //    vendors.Add(new Vendor()
        //    {
        //        Name = "Vaibhav",
        //        CompanyName = "TNC",
        //        Address = "Dubai",
        //        Phone = 0551533823,
        //        Email = "Vaibhav@tnc-me.com",
        //        Contact = "Admin"
        //    });
        //    return vendors;
        //}
    }
}
