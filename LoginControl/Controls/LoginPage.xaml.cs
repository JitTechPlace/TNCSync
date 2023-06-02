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
using Haley.Abstractions;
using Haley.Enums;
using TNCSync.Class.Info;
using TNCSync.Class.SP;

namespace LoginControl.Controls
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
        #region Methods
        public LoginPage()
        {
            InitializeComponent();
        }
        private IDialogService _ds;

        private void Login()
        {
            if(ptboxEmail.Text.Trim() == string.Empty)
            {
                _ds.SendToast("Please Enter your email or Username", "", NotificationIcon.Info);
                ptboxEmail.Focus();
            }
            else if(ptboxPass.Text.Trim() == string.Empty)
            {
                _ds.SendToast("Please Enter Password", "", NotificationIcon.Info);
                ptboxPass.Focus();
            }
            else
            {
                UserSP spUser = new UserSP();
                UserInfo infouser = new UserInfo();
                infouser = spUser.UserView(ptboxEmail.Text.Trim());
                if(infouser.LoginName != ptboxEmail.Text || infouser.Password != ptboxPass.Text)
                {
                    _ds.SendToast("Invalid Credentials", "", NotificationIcon.Warning);
                    //Clear();
                }
                else
                {
                    if()
                }
            }
        }


        #endregion


        #region Events
        private void pbtnSignin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login();
            }
            catch (Exception ex)
            {
                _ds.SendToast("Credentials Mismatch or Empty", "", NotificationIcon.Warning);
                return;
            }
        }
        #endregion

    }
}
