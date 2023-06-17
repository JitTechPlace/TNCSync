using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TNCSync.Enums;
using TNCSync.Model;
using TNCSync.Repositories;

namespace TNCSync.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

       // private IUserRepository userRepository;

        ///Properties
        public UserAccountModel CurrentUserAccount
        {
            get { return _currentUserAccount; }
            set { _currentUserAccount = value; OnPropertyChanged(nameof(CurrentUserAccount)); }
        }

        public ViewModelBase CurrentChildView
        {
            get { return _currentChildView; }
            set { _currentChildView = value; OnPropertyChanged(nameof(CurrentChildView)); }
        }

        public string Caption
        {
            get { return _caption; }
            set { _caption = value; OnPropertyChanged(nameof(Caption)); }
        }

        public IconChar Icon
        {
            get { return _icon; }
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        //Swith Modules
        private PageId _pageId;
        public PageId PageID
        {
            get { return _pageId; }
            set { SetProperty(ref _pageId, value); }
        }

        public void changePage(PageId newPage)
        {
            PageID = newPage;
            //CurrentChildView = new ViewModel();
            //Caption = "Dashboard";
            //Icon = IconChar.Home;
        }

        //Commands
        public ICommand CMDChangePage => new RelayCommand<PageId>(changePage);
        //public ICommand ShowHomwViewCommand { get; }
        //public ICommand ShowCustomerViewCommand { get; }
        //public ICommand ShowVendorViewCommand { get; }
        //public ICommand ShowCoAViewCommand { get; }
        //public ICommand ShowJvViewCommand { get; }

        public MainViewModel()
        {
            //userRepository = new UserRepository();
            CurrentUserAccount = new UserAccountModel();

            //SwithPage
            PageID = PageId.Dashboard;

            #region Initialize Commands
            //ShowHomwViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            //ShowCustomerViewCommand = new ViewModelCommand(ExecuteShowCustomerViewCommand);
            //ShowVendorViewCommand = new ViewModelCommand(ExecuteShowVendorViewCommand);
            //ShowCoAViewCommand = new ViewModelCommand(ExecuteShowCoaViewCommand);
            //ShowJvViewCommand = new ViewModelCommand(ExecuteShowJvViewCommand);

            //Default View
            // ExecuteShowHomeViewCommand(null);
            ///LoadCurrentusrData();
           #endregion

        }

        //private void ExecuteShowCustomerViewCommand(object obj)
        //{
        //    CurrentChildView = new CustomerViewModel();
        //    Caption = "Customers";
        //    Icon = IconChar.UserAlt;
        //}
        //private void ExecuteShowHomeViewCommand(object obj)
        //{
        //    CurrentChildView = new HomeViewModel();
        //    Caption = "Dashboard";
        //    Icon = IconChar.Home;
        //}
        //private void ExecuteShowVendorViewCommand(object obj)
        //{
        //    CurrentChildView = new VendorViewModel();
        //    Caption = "Vendor";
        //    Icon = IconChar.UserGroup;
        //}
        //private void ExecuteShowCoaViewCommand(object obj)
        //{
        //    CurrentChildView = new COAViewModel();
        //    Caption = "Chart of Account";
        //    Icon = IconChar.Box;
        //}
        //private void ExecuteShowJvViewCommand(object obj)
        //{
        //    CurrentChildView = new JournalVoucherViewModel();
        //    Caption = "Journal Voucher";
        //    Icon = IconChar.ShoppingBag;
        //}

        //private void LoadCurrentusrData()
        //{
        //    var user = userRepository.GetByUsername(Thread.CurrentPrincipal.Identity.Name);
        //    if(user != null)
        //    {
        //        CurrentUserAccount.UserName = user.Username;
        //        CurrentUserAccount.DisplayName = $"{user.Username} { user.LoginName}";
        //        CurrentUserAccount.ProfilePicture = null;
        //    }
        //    else
        //    {
        //        CurrentUserAccount.DisplayName = "user not Logged in";
        //        //Hide Child View
        //    }
        //}
    }
}
