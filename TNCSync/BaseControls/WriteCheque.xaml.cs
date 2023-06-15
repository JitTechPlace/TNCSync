using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TNCSync.Class;
using TNCSync.Model;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for WriteCheque.xaml
    /// </summary>
    public partial class WriteCheque : UserControl
    {
        List<WriteCheques> cheques = new List<WriteCheques>();
        DataTable table = new DataTable();
        SqlDataAdapter sda = new SqlDataAdapter();
        public WriteCheque()
        {
            InitializeComponent();
            UpdateBinding();
        }

        private void LoadGrid()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            SqlCommand cmd = new SqlCommand("WriteCheque_TNCS", sqlconn);
            cmd.CommandType = CommandType.StoredProcedure;
            sda.SelectCommand = cmd;
            sda.Fill(table);
            grdChequeList.ItemsSource = table.DefaultView;
            //grdChequeList.DisplayMemberPath = "TemplateName";
            //grdChequeList.SelectedIndex = -1;

            //Load the Payee Combobox
            //payeeCmbx.ItemsSource = table.DefaultView;
            //payeeCmbx.DisplayMemberPath = "Payee Name";
        }

        private void LoadPayeeCombobox()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            sqlconn.Open();
            SqlCommand cmd = new SqlCommand("SELECT distinct payEntityRef FROM tblQbCheck", sqlconn);
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            payeeCmbx.ItemsSource = table.DefaultView;
            payeeCmbx.DisplayMemberPath = "payEntityRef";
        }


        private void PopulateTempleteCombobox()
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            sqlconn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Templates where TemplateType ='Check Payment Voucher'", sqlconn);
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            tmpltCmbx.ItemsSource = table.DefaultView;
            tmpltCmbx.DisplayMemberPath = "TemplateName";
            tmpltCmbx.SelectedIndex = -1;
        }

        private void chequeDGSyncall_Click(object sender, RoutedEventArgs e)
        {
            LoadGrid();
            LoadPayeeCombobox();
            PopulateTempleteCombobox();
        }

        private void srchbtn_Click(object sender, RoutedEventArgs e)
        {
            WriteCheques_DA Wcda = new WriteCheques_DA();
            cheques = Wcda.GetCheques(payeeCmbx.Text);
            UpdateBinding();
            ////get the selected value from the combobox
            //string selectedValue = payeeCmbx.SelectedItem as string;
            ////Clear any existing filters
            //grdChequeList.Items.Filter = null;

            ////Apply new filter based on the selected value
            //ICollectionView view = CollectionViewSource.GetDefaultView(grdChequeList.ItemsSource);
            //if (selectedValue != null)
            //{
            //    //view.Filter = (item) =>
            //    //{
            //    //    return((string)item).
            //    //};
            //}

            ////DataRowView payee = (DataRowView)payeeCmbx.SelectedItem;
            ////grdChequeList.ItemsSource = table.DefaultView;
        }

        private void UpdateBinding()
        {
            grdChequeList.ItemsSource = cheques;
            grdChequeList.DisplayMemberPath = "WriteCheques";
        }
    }
}
