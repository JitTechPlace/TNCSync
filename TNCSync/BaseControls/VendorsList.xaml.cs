﻿using System;
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
        }

        private void syncVendorList_Click(object sender, RoutedEventArgs e)
        {
            populateDatagrid();
        }

        private void populateDatagrid()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            //string sqlquery = "SELECT * FROM tblVendor";
            SqlCommand cmd = new SqlCommand("SELECT * FROM tblVendor", sqlconn);
            sqlconn.Open();
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            grdVendorLst.ItemsSource = table.DefaultView;
            sqlconn.Close();
        }
    }
}
