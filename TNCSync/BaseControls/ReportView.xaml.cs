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
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;


namespace TNCSync.BaseControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private string path = null;

        public ReportView()
        {
            InitializeComponent();

        }

        public void ShowReportView(ref ReportDocument _ReportDocument)
        {
            ReportDocument rptdoc = new ReportDocument();
            //rptdoc.Load(path + "" + ".rpt");
            //rptdoc.SetDatabaseLogon("","","","CMS");
            CRV.ViewerCore.ReportSource = _ReportDocument;
            //ShowReportView(ref _ReportDocument);
        }
    }
}
