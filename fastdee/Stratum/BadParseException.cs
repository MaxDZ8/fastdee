﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Stratum
{
    public class BadParseException : ApplicationException
    {
        public BadParseException(string? message) : base(message) { }
    }
}
