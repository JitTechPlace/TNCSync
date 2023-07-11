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
using System.IO;
using Microsoft.Win32;
using TNCSync.Class;
using TNCSync.Model;
using TNCSync.Class.DataBaseClass;
using SimpleAES;

namespace TNCSync.Controls
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
        List<UserModel> user = new List<UserModel>();
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        private string companyChoice = "";
        public bool binForDifferentUser;

        public LoginPage()
        {
            InitializeComponent();
            string userName = (string)ptboxEmail.Text;
            string password = (string)ptboxPass.Password;
            string companyName = (string)cmpyCmbx.Text;
            _ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            //cmpyCmbx.Visibility = Visibility.Hidden;
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
        //          ds_SentToast("Invalid Credentials", "" , NotificationIcon.Warning));
        //          Clear();
        //        }
        //    }
        //}


        #endregion

        public void FillCombobox() //need to work some time it not connecting to DB for fetch comapny name
        {
            //UserModel_DA Umda = new UserModel_DA();
            //user = Umda.GetUser(cmpyCmbx.Text);
            try
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
            catch
            {
                _ds.SendToast("Unable To Load Company", "Please check with Data Connection", NotificationIcon.Error);
            }
        }

        #region Events
        private void pbtnSignin_Click(object sender, RoutedEventArgs e)
        {
            string userName = (string)ptboxEmail.Text;
            string password = (string)ptboxPass.Password;
            string companyName = (string)cmpyCmbx.Text;
            MainWindow mw = new MainWindow();
            AuthenticationWindow aw = new AuthenticationWindow();
            DataTable table = new DataTable();
            SqlDataAdapter sda = new SqlDataAdapter();

            if (userName == "Superadmin" & password == "Version01")
            {
                mw.Show();
                if (mw.Visibility == Visibility.Visible)
                {
                    aw.Visibility = Visibility.Hidden;
                }
                else
                {
                    aw.Visibility = Visibility.Visible;
                }

                //cmpyCmbx.Visibility = Visibility.Visible;
            }
            else
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(companyName))
                {
                    _ds.SendToast("Authentication Error", "Please Fill All the Credential details to Login", NotificationIcon.Error);
                    return;
                }
                else
                {
                    try
                    {
                        if(!(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(companyName)))
                        {
                            // 'MsgBox(gblUserID)
                            // to get user information
                            var UserInfo = db.UserLogin_SelectAll(0, ClearAllControl.gblUserID, null, null, bool.Parse("1"));
                            var rec = UserInfo.First();

                            if (rec != null)
                            {
                                ClearAllControl.gblCompanyID = rec.companyID;
                                ClearAllControl.gblCompanyName = rec.CompanyName;
                                ClearAllControl.gblUserName = rec.UserName;
                                // gblDecimalPlaces = 2
                            }
                            //string conn = ConfigurationManager.ConnectionStrings["TNCSync_Connection"].ConnectionString;
                            //SqlConnection sqlconn = new SqlConnection(conn);
                            //SqlCommand cmd = new SqlCommand("UserLogin_SelectAll_TNCS", sqlconn);
                            //cmd.CommandType = CommandType.StoredProcedure;
                            //cmd.Parameters.AddWithValue("@Email", ptboxEmail.Text);
                            //cmd.Parameters.AddWithValue("@Password", ptboxPass.Password);
                            //cmd.Parameters.AddWithValue("@CompanyName", cmpyCmbx.Text);
                            //sda.SelectCommand = cmd;
                            //sda.Fill(table);
                            //cmd.Dispose();
                            mw.Show();
                            //aw.Close();
                        }
                        else
                        {
                            _ds.SendToast("Authentication Error", "Please Fill All the Credential details to Login", NotificationIcon.Error);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _ds.SendToast("Credentials Mismatch or Empty", "", NotificationIcon.Warning);
                        return;
                    }
                    aw.Close();
                }
            }
            #region Old Login
            //try
            //{
            //    var validUser = default(int);
            //    if(ptboxEmail.Text == "Superadmin" & ptboxPass.Password == "Version01")
            //    {
            //        companyChoice = cmpyCmbx.SelectedValue.ToString();
            //        int? argvalidUser = validUser;
            //        db.sp_userlogin(int.Parse(companyChoice), ptboxEmail.Text, ptboxPass.Password, ref argvalidUser);
            //        validUser = (int)argvalidUser;
            //        if(validUser == -1 | string.IsNullOrEmpty(ptboxEmail.Text) & string.IsNullOrEmpty(ptboxPass.Password) & string.IsNullOrEmpty(cmpyCmbx.Text))
            //        {
            //            _ds.ShowDialog("Login Error", "Either Login ID or Password is Wrong", NotificationIcon.Warning);
            //        }
            //        else
            //        {
            //            ClearAllControl.gblCompanyID = validUser;
            //            var UserInfo = db.UserLogin_SelectAll(0, ClearAllControl.gblUserID, null, null, bool.Parse("1"));
            //            var rec = UserInfo.First();

            //            if(rec != null)
            //            {
            //                ClearAllControl.gblCompanyID = rec.companyID;
            //                ClearAllControl.gblCompanyName = rec.CompanyName;
            //                ClearAllControl.gblUserName = rec.UserName;
            //            }

            //            if(binForDifferentUser == false)
            //            {
            //                MainWindow mainwnd = new MainWindow();
            //                mainwnd.Show();

            //            }

            //        }
            //    }
            //    else
            //    {
            //        ClearAllControl.gblUserName = ptboxEmail.Text.Trim();
            //        if(binForDifferentUser == false)
            //        {
            //            AuthenticationWindow authwind = new AuthenticationWindow();
            //            authwind.Show();
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    _ds.ShowDialog("TNC_sync","Authentication Failed", NotificationIcon.Error);
            //    //_ds.ShowDialog("TNC_sync",ex.Message, NotificationIcon.Error);
            //}

            #endregion

        }
        #endregion

        private void cmpnycbx_Checked(object sender, RoutedEventArgs e)
        {
            FillCombobox();
        }

        private void UpdateBinding()
        {

        }
    }
}
