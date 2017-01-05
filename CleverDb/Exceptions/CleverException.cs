using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverDb.Exceptions
{
    public class CleverException : ApplicationException
    {
        public string ExceptionDetails { get; set; }
        public override string Message
        {
            get
            {
                return ExceptionDetails;
            }
        }
    }
}
