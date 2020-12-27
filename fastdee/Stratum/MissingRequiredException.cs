using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Stratum
{
    public class MissingRequiredException : ApplicationException
    {
        public MissingRequiredException(string? message) : base(message) { }
    }
}
