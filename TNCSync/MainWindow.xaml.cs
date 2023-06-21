using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Haley.Abstractions;
using Haley.Enums;
using Haley.MVVM;

namespace TNCSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variable
        public static bool isDeleted = false;
        public static MainWindow objMW = new MainWindow();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
        }

        private IDialogService _ds;
        private bool blurOtherWindows;
        private bool hideIcon;

        bool EnableBackgroundBlur { get; set; }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogMode dialogMode = DialogMode.Confirmation;
            var result = _ds.ShowDialog("Logout", "Are you sure to logout?", Haley.Enums.NotificationIcon.Warning,dialogMode,hideIcon=true, blurOtherWindows = true);
            if (result.DialogResult == true)
            {
                this.Close();
                //AuthenticationWindow awnd = new AuthenticationWindow();
                //awnd.Show();
            }
            else
            {
                return;
            }
        }
    }
}
