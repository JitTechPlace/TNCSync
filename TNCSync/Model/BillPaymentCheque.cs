using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.Model
{
    public class BillPaymentCheque
    {
        public string refNumber { get; set; }

        public string payeeFullName { get; set; }

        public string txnDate { get; set; }

        public int amount { get; set; }

        public string txtNumber { get; set; }

        public string voucherNumber { get; set; }

        public string bankName { get; set; }
    }
}
