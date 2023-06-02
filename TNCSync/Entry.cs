using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
using TNCSync.ViewModel;
using TNCSync.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.IOC;


namespace TNCSync
{
    public static class Entry
    {
        public static void Initialize(IContainerFactory containerfactory)
        {
            if(containerfactory.Services is IBaseContainer bsContainer)
            {
                // Currently handling only haley
                bsContainer.Register<AuthenticationViewModel>();  //Resolve this
            }

            //UIRegistrations
            containerfactory.Controls.Register<AuthenticationViewModel, LoginPage>(ViewEnums.LoginPage);
            containerfactory.Controls.Register<AuthenticationViewModel, LoginHelperPage>(ViewEnums.HelperPage);
            containerfactory.Controls.Register<AuthenticationViewModel, SignupPage>(ViewEnums.SignUpPage);

            //WindowRegistrations
            containerfactory.Windows.Register<AuthenticationViewModel, AuthenticationWindow>("authmainWindow");

        }
    }
}
