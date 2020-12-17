using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Stratum
{
    class NotificationSystem : IDisposable
    {
        public void Dispose() { }

        internal void TryMangle(string? method, object? evargs)
        {
            throw new NotImplementedException();
        }
    }
}
