using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.IIS
{
    public class LogParseExecutedEventArgs : EventArgs
    {
        internal bool IsParsingCanceled = false;
        public RawIISLog RawIISLog { get; private set; }

        public LogParseExecutedEventArgs(RawIISLog log)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            this.RawIISLog = log;
        }

        public void CancelParsing()
        {
            IsParsingCanceled = true;
        }
    }
}
