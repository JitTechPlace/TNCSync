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

namespace LoginControl.ViewModel
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

        public ICommand CmdChangeView => new DelegateCommand<object>((o) => CurrentView = o);

        public ICommand CmdCreateAccount => new DelegateCommand(_createAccount);

        public ICommand CmdSignIn => new DelegateCommand<object>(_signIn);

        private void _signIn(object obj)
        {
            //Signin
            var _usn = UserName;
            if(obj is PlainPasswordBox pbox)
            {
                var _password = pbox.GetPassword();
                if(string.IsNullOrWhiteSpace(_password) || !_password.Equals("Admin"))
                {
                    ds.SendToast("Authentication Error", "Password Mismatch", NotificationIcon.Error);
                    return;
                }
            }
            //finally if its sucessfull
            InvokeVMClosed(this, new FrameClosingEventArgs(true, "Authenticated"));
        }

        private void _createAccount()
        {
            //Create an account by the values
            //change the view
            CurrentView = ViewEnums.LoginPage;
        }



        public AuthenticationViewModel(IDialogService dialogService)
        {
            CurrentView = ViewEnums.LoginPage;
            ds = dialogService;
        }
    }
}
