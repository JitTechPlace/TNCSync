using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TNCSync.Class;
using TNCSync.Class.DataBaseClass;

namespace TNCSync.View
{
    /// <summary>
    /// Interaction logic for AddNewCompany.xaml
    /// </summary>
    public partial class AddNewCompany : Window
    {
        private static DBTNCSDataContext db = new DBTNCSDataContext(ClearAllControl.gblConnectionString);
        private string u_companyCode = null;
        private int u_companyID = 0;
        private string s_companyName = null;
        private object Picture;

        public AddNewCompany()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private byte[] ImageToStream(string fileName)
        {
            var stream = new MemoryStream();
        tryagain:
            ;

            try
            {
                var image = new Bitmap(fileName);
                image.Save(stream, ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                goto tryagain;
            }

            return stream.ToArray();
        }

        private void addQBFile_Click(object sender, RoutedEventArgs e)
        {
            var folder = new OpenFileDialog();
            var result = folder.ShowDialog();
            if ((int)result == 1)
            {
                // System.IO.File.Copy(folder.FileName, TextEdit1.Text)
                qbfilename.Text = folder.FileName;

            }
        }

        private void saveCompany_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(cmpnyimgbox.Source != null)
                {
                    try
                    {
                       // Picture = new object(ClearAllControl.ConvertImageToByteArray(cmpnyimgbox));
                    }
                    catch(Exception ex)
                    {

                    }
                }

                int? argiNewComapnyId;
               // db.tblCompanyInsert(txtcpycode.Text, txtcpyname.Text, txtcpytype.Text, txtemail.Text, (BinaryWriter)logo, qbcmpyname.Text, qbfilename.Text,IsActive, ref argiNewComapnyId);
            }
            catch
            {

            }
        }

        //private bool validateCompanyInfo()
        //{
        //    bool blnResult = false;

        //}
    }
}
