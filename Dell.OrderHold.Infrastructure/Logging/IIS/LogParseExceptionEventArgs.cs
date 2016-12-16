using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.IIS
{
    public class LogParseExceptionEventArgs : EventArgs
    {
        internal bool IsParsingCanceled { get; private set; }
        public Exception Exception { get; private set; }

        public LogParseExceptionEventArgs(Exception ex)
        {
            this.Exception = ex;
        }

        public void CancelParsing()
        {
            this.IsParsingCanceled = true;
        }
    }
}
