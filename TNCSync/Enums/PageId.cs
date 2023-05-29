using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.Enums
{
    public enum PageId
    {
        Dashboard,
        Vendor,
        Customer,
        JournalVoucher,
        ChartofAccount,
        Cheque,
        Manage
    }

    public enum LogFileType
    {
        userInfo,
        AppSettings,
        tempSettings
    }

    public enum LogOnPages
    {
        register,
        login,
        resetpassword,
        OTP
    }
}
