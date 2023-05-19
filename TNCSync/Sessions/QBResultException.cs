using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync
{
    public class QBResultException : Exception
    {
        private readonly int _index;

        private QBResultException()
        {

        }
        public QBResultException(int index, string errorMsg) : base(errorMsg)
        {
            _index = index;
        }

        ///<summary>
        ///Readonly properties that provides the index of the offending IResponse object within the IResponseList object
        /// </summary>
        public int ErrorIndex
        {
            get
            {
                return _index;
            }
        }
    }
}
