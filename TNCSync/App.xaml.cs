using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
using TNCSync;
using TNCSync.Controls;
using Haley.IOC;


namespace TNCSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IDialogService _ds;

        protected override void OnStartup(StartupEventArgs e)
        {
            _ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            Entry.Initialize(ContainerStore.Singleton.GetFactory());
            base.OnStartup(e);

        }
        private void Application_Startup(object sender, StartupEventArgs e)  //Need to work on[Auth window will close while main window opened]
        {
            var _actualmainwindow = new MainWindow();  //Remember to dispose at the end.
            var _authwindow = new AuthenticationWindow();
           
            //if (_authwindow == null)
            //{
            //    _actualmainwindow.Show();
            //}
            //else
            //{
            //    _authwindow.Show();
            //}

            while (!CredentialHolder.Singleton.IsAuthenticated)
            {
                var _result = ContainerStore.Singleton.Windows.ShowDialog("authmainWindow");
                break;
            }
        }
    }
}
