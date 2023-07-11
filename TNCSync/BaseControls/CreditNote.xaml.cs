using Haley.Abstractions;
using Haley.MVVM;
using System;
//using Interop.QBFC15;
using Interop.QBFC12;
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
using TNCSync.Class.DataBaseClass;
using TNCSync.Sessions;
using TNCSync.Class;

namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for CreditNote.xaml
    /// </summary>
    public partial class CreditNote : UserControl
    {
        private bool bError;
        private short maxVersion;
        private string path = null;
        private SQLControls sql = new SQLControls();
        private bool booSessionBegun;
        private static DBTNCSDataContext db = new DBTNCSDataContext();
        public IDialogService ds;
        SessionManager sessionManager;

        public CreditNote()
        {
            InitializeComponent();
            ds = ContainerStore.Singleton.DI.Resolve<IDialogService>();
            dpFrmDate.SelectedDate = DateTime.Now;
            dpToDate.SelectedDate = DateTime.Now;
        }

        #region CONNECTION TO QB
        private void connectToQB()
        {
            sessionManager = SessionManager.getInstance();
            maxVersion = sessionManager.QBsdkMajorVersion;
        }
        //private IMsgSetResponse processRequestFromQB(IMsgSetRequest requestSet)
        //{
        //    try
        //    {
        //        //MessageBox.Show(requestSet.ToXMLString());
        //        IMsgSetResponse responseSet = sessionManager.doRequest(true, ref requestSet);
        //        //MessageBox.Show(responseSet.ToXMLString());
        //        return responseSet;
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message);
        //        return null;
        //    }
        //}
        private void disconnectFromQB()
        {
            if (sessionManager != null)
            {
                try
                {
                    sessionManager.endSession();
                    sessionManager.closeConnection();
                    sessionManager = null;
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.Message);
                    ds.SendToast(e.Message, "TNC-Sync", Haley.Enums.NotificationIcon.Error);
                }
            }
        }
        #endregion

        #region CNMethods
        #endregion

        #region Methods
        #endregion

        #region Events
        #endregion
    }
}
