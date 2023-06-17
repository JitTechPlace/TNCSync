using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TNCSync.Model;

namespace TNCSync.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields
        private string _loginName;
        private SecureString _password;
        private List<UserModel> _companyname;
        private string _errorMessage;
        private bool _isViewVisible = true;

       // private IUserRepository userRepository;

        //Properties
        public string Loginname
        {
            get
            {
                return _loginName;
            }
            set
            {
                _loginName = value;
                OnPropertyChanged(nameof(Loginname));
            }
        }

        public SecureString Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public List<UserModel> CompanyName
        {
            get
            {
                return _companyname;
            }
            set
            {
                _companyname = value;
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        //Commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }

        //Constructor
        public LoginViewModel()
        {
          //userRepository = new UserRepo
        }

    }
}
