using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync
{
    public class QBNoResponseException : Exception
    {
        private readonly int _index;

        private QBNoResponseException() { }

        ///<summary>
        ///Describe an error in which no IResponse object is found in the IResponseList
        /// </summary>
        /// <param name="index">Index within the IResponseList that was not found</param>
        /// <param name="errorMsg">Textual error message</param>
        public QBNoResponseException(int index, string errorMsg) : base(errorMsg)
        {
            _index = index;
        }


        ///<summary>
        ///Read-only property that provied the index of the offending IResponse object within the IResponseList object
        ///</summary>
        public int ErroeIndex
        {
            get
            {
                return _index;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
