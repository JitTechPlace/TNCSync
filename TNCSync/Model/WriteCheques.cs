using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.Model
{
    public class WriteCheques
    {
        public string refNumber { get; set; }

        public string payEntityRef { get; set; }

        public string txnDate { get; set; }

        public string timeCreated { get; set; }

        public int amount { get; set; }

        public string txnID { get; set; }

        public string accountRef { get; set; }


        //public string ChequesFullInfo
        //{
        //    get 
        //    {
        //         return $"{refNumber} {payEntityRef} {txnDate} {timeCreated} {amount} {txnID} {accountRef}";
        //    }
        //}

    }
}
