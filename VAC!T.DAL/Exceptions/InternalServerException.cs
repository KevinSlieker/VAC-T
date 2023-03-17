using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAC_T.DAL.Exceptions
{
    public class InternalServerException : Exception
    {
        public InternalServerException(string? reason) : base(reason)  {  }
    }
}
