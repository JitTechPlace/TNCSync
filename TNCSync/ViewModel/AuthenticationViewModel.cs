using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
using Haley.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using TNCSync.Enums;
using TNCSync.Model;

namespace TNCSync.ViewModel
{
    public class AuthenticationViewModel : BaseVM
    {
        private IDialogService ds;

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set { SetProp(ref _currentView, value); }
        }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { SetProp(ref _userName, value); }
        }

        private string _email;

        public string Email
        {
            get { return _email; }
            set { SetProp(ref _email, value); }
        }


        private string _company;

        public string Company
        {
            get { return _company; }
            set { SetProp(ref _company , value); }
        }

        private bool _isLoggedout;

        public bool IsLoggedOut
        {
            get { return _isLoggedout; }
            set { SetProp(ref  _isLoggedout, value); }
        }

        private bool _controlsEnabled;

        public bool ControlsEnabled
        {
            get { return _controlsEnabled; }
            set { SetProp(ref _controlsEnabled , value); }
        }

        private string  _errorRegister;

        public string  ErrorRegister
        {
            get { return _errorRegister; }
            set { SetProp(ref _errorRegister, value); }
        }


        private ICommand mCMDSwitchLogOnPages;
        public ICommand CMDSwitchLogOnPages
        {
            get
            {
                if (mCMDSwitchLogOnPages == null)
                { 
                    mCMDSwitchLogOnPages = new RelayCommand(param => SwithLogOnPages(param), GlobelHelpers.AlwaysExecute);
                }
                return mCMDSwitchLogOnPages;
            }
        }

        public void SwithLogOnPages(object InputValues)
        {
            LogOnPages iparam = (LogOnPages)InputValues;
            currentPage = iparam;
        }

        private LogOnPages _currentPage;
        public LogOnPages currentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged(); }
        }

        private int mMoveOn;
        public int MoveOn
        {
            get { return mMoveOn; }
            set
            {
                mMoveOn = value;
                OnPropertyChanged();
            }
        }



        #region Commands


        public ICommand CmdChangeView => new DelegateCommand<object>((o) => CurrentView = o);

        //public ICommand CmdCreateAccount => new DelegateCommand(_createAccount);

        //public ICommand CmdSignIn => new DelegateCommand<object>(_signIn);

       // public ICommand CmdResetPassword => new DelegateCommand<object>(_resetpassword);

        //public ICommand CmdGenerateOtp => new DelegateCommand<object>(_generateOTP);

        public ICommand CmdLogout => new DelegateCommand<object>(_logout);

        #endregion


        #region COmmand Methods

        //private void _signIn(object obj)
        //{
        //    //Signin
        //    var _cmp = Company;
        //    var _usn = UserName;
        //    if (obj is PlainPasswordBox pbox)
        //    {
        //        var _password = pbox.GetPassword();
        //        if (string.IsNullOrWhiteSpace(_password) || !_password.Equals("Admin"))
        //        {
        //            ds.SendToast("Authentication Error", "Credentials Mismatch or Missing", NotificationIcon.Error);
        //            return;
        //        }
        //    }
        //    //finally if its sucessfull
        //    InvokeVMClosed(this, new FrameClosingEventArgs(true, "Authenticated"));
        //}

        //private void _createAccount()
        //{
        //    //Create an account by the values
        //    //change the view
        //    CurrentView = ViewEnums.LoginPage;
        //}

        //private async void _generateOTP(object param)
        //{
        //    try
        //    {
        //        //Generate otp for the provided email id
        //        object[] values = (object[])param;
        //        string EmailID = (string)values[0];

        //        Email = EmailID;
        //        //Invoke otp and move to logonpage
        //       // Task<bool> TgenerateOTP = new Task<bool>(() => EmailHelper.GenerateOTP(EmailID));
        //      //  TgenerateOTP.Start();

        //        ControlsEnabled = false;

        //        //if(await TgenerateOTP == true) //  Waiting to see the result
        //        //{
        //        //    CmdChangeView.Execute(Controls.LoginHelperPage);
        //        //    ErrorRegister = string.Empty;
        //        //    ControlsEnabled = true;
        //        //}
        //        //else
        //        //{
        //        //    ErrorRegister = "Unable to generate OTP. Please try again or contact admin";
        //        //    ControlsEnabled = true;
        //        //}
        //    }
        //    catch(Exception)
        //    {
        //        ControlsEnabled = true;
        //    }
        //}

        //private async void _resetpassword(object param)
        //{

        //}

        public void _logout(object parameter)
        {
            MessageBox.Show("Do you want to logout from the current session?", "Logout", MessageBoxButton.OK, MessageBoxImage.Warning);
            // ds.SendToast("Do you with to logout from the current session?","",NotificationIcon.Warning);
            return;
        }

        #endregion

        public static AuthenticationViewModel Instance = new AuthenticationViewModel(); //Singleton
        public static void CreateInstance()
        {
            Instance = null;
            if (Instance == null) Instance = new AuthenticationViewModel();
        }


        public AuthenticationViewModel()
        {
            MoveOn = 1;
           currentPage = LogOnPages.login;
           CurrentView = ViewEnums.LoginPage;
            //ds = dialogService;
        }
    }
}
