using CrystalDecisions.CrystalReports.Engine;
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
using System.Windows.Shapes;
using SAPBusinessObjects.WPF.Viewer;

namespace TNCSync.View
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        public Report()
        {
            InitializeComponent();



            //crViewer.ReportSource = new ReportDocument();
            //crViewer.ReportSource.Load("path_to_your_report.rpt");
        }

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //   // ReportDocument _ReportDocument = new ReportDocument();
        //    //CRV.ViewerCore.ReportSource = _ReportDocument;
        //}

        //public void ShowReportView(ref ReportDocument _ReportDocument)
        //{
        //    CRV.ViewerCore.ReportSource = _ReportDocument;
        //    //ShowReportView(ref _ReportDocument);
        //}
    }
}
