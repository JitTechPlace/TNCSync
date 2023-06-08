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

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for BillPayments.xaml
    /// </summary>
    public partial class BillPayments : UserControl
    {
        public BillPayments()
        {
            InitializeComponent();
        }

        private void btnsycall_Click(object sender, RoutedEventArgs e)
        {
            PopulateDataGrid();
        }

        private void PopulateDataGrid()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand("report_CheckPayment");
            //cmd.CommandText = "report_CheckPayment";
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sqlda = new SqlDataAdapter(cmd);
            sqlda.Fill(dt);
            grdBillPytDtl.DataContext = dt;

            //SqlDataReader sdr = cmd.ExecuteReader();
            //sdr.Read();
            //grdBillPytDtl.rea 


            //string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            //SqlConnection sqlconn = new SqlConnection(conn);
            ////string sqlquery = "SELECT * FROM tblVendor";
            //SqlCommand cmd = new SqlCommand("report_CheckPayment", sqlconn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //SqlDataReader rd = new SqlDataReader();
            //rd = cmd.ExecuteReader();
            //while (rd.Read())
            //{
            //    grdBillPytDtl.Columns.Add(rd[0].ToString());

            //}
            //rd.Close();
            //cmd.CommandText = "report_CheckPayment";
            //SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            //DataSet ds = new DataSet();
            //sdr.Fill(ds);
            //grdBillPytDtl.DataContext = ds.Tables[0];
            ////sqlconn.Open();
            ////SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            ////sdr.Fill(SqlCommand);
            ////grdBillPytDtl.ItemsSource = table.DefaultView;
            ////sqlconn.Close();
        }
    }
}
