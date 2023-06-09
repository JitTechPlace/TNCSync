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
using Haley.Abstractions;
using Haley.Enums;
using Haley.MVVM;
using TNCSync.Class.Info;
using TNCSync.Class.SP;

namespace TNCSync.Controls
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
     
        public LoginPage()
        {
            InitializeComponent();
            _ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            FillCombobox();
        }
        private IDialogService _ds;
        #region Methods
        //private void Login()
        //{
        //    if (ptboxEmail.Text.Trim() == string.Empty)
        //    {
        //        _ds.SendToast("Please Enter your email or Username", "", NotificationIcon.Info);
        //        ptboxEmail.Focus();
        //    }
        //    else if (ptboxPass.Text.Trim() == string.Empty)
        //    {
        //        _ds.SendToast("Please Enter Password", "", NotificationIcon.Info);
        //        ptboxPass.Focus();
        //    }
        //    else
        //    {
        //        UserSP spUser = new UserSP();
        //        UserInfo infouser = new UserInfo();
        //        infouser = spUser.UserView(ptboxEmail.Text.Trim());
        //        if (infouser.LoginName != ptboxEmail.Text || infouser.Password != ptboxPass.Text)
        //        {
        //            _ds.SendToast("Invalid Credentials", "", NotificationIcon.Warning);
        //            //Clear();
        //        }
        //        else
        //        {

        //        }
        //    }
        //}


        #endregion

        public void FillCombobox() //need to work some time it not connecting to DB for fetch comapny name
        {
            string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(conn);
            sqlconn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM tblCompany", sqlconn);
            SqlDataAdapter sdr = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            sdr.Fill(table);
            cmpyCmbx.ItemsSource = table.DefaultView;
            cmpyCmbx.DisplayMemberPath = "CompanyName";
            cmpyCmbx.SelectedIndex = -1;
        }


        #region Events
        private void pbtnSignin_Click(object sender, RoutedEventArgs e)
        {
            string userName = (string)ptboxEmail.Text;
            string password = (string)ptboxPass.Password;
            string companyName = (string)cmpyCmbx.Text;
            DataTable table = new DataTable();
            SqlDataAdapter sda = new SqlDataAdapter();

            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(companyName))
            {
                _ds.SendToast("Authentication Error", "Please Fill All the Credential details to Login", NotificationIcon.Error);
                return;
            }
            else
            {
                try
                {
                    string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
                    SqlConnection sqlconn = new SqlConnection(conn);
                    SqlCommand cmd = new SqlCommand("UserLogin_SelectAll", sqlconn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Loginname", ptboxEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", ptboxPass.Password);
                    cmd.Parameters.AddWithValue("@CompanyName", cmpyCmbx.Text);
                    sda.SelectCommand = cmd;
                    sda.Fill(table);
                    cmd.Dispose();
                    AuthenticationWindow aw = new AuthenticationWindow();
                    aw.Close();

                    MainWindow mw = new MainWindow();
                    mw.Show();

                }
                catch (Exception ex)
                {
                    _ds.SendToast("Credentials Mismatch or Empty", "", NotificationIcon.Warning);
                    return;
                }
            }

        }
        #endregion

    }
}
