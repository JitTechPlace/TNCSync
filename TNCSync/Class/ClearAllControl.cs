﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.Class
{
     public class ClearAllControl
    {
        public static long gblUserID = 0L;
        public static string gblSQLServerName = "";
        public static string gblDatabaseName = "";
        public static string gblSQLServerUserName = "";
        public static string gblSQLServerPassword = "";
        public static string gblDatabase = "";

        public static int gblCompanyID = 0;

        public static string gblCompanyName = "TNC";
        public static string gblConnectionString = "";
        public static string gblUserName = "";

        public static bool isKeyExit = false;
        public static string keyType = "";

        public static string regMAcAddress = "";

        public static string gblPOTxnID = "";

        public static string gblLoginName = " ";

        public static byte[] ConvertImageToByteArray(Image p_Image)
        {
            byte[] bytBuffer;
            MemoryStream memStream = null;
            try
            {
                memStream = new MemoryStream();
                p_Image.Save(memStream, ImageFormat.Png);
                bytBuffer = memStream.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (memStream !=null)
                {
                    memStream.Close();
                }
            }
            return bytBuffer;
        }

        public static Image ConvertByteArrayToImage(byte[] imageArray)
        {
            Image img;
            MemoryStream memStream = null;
            try
            {
                memStream = new MemoryStream(imageArray, 0, imageArray.Length);
                memStream.Write(imageArray, 0, imageArray.Length);
                img = Image.FromStream(memStream, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (memStream !=null)
                {
                    memStream.Close();
                }

            }
            return img;

        }
    }
}
