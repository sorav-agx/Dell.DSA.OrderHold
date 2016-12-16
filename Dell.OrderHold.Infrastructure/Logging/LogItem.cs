using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public class LogItem
    {
        public LogItem()
        {
            Properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public string Id { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public SeverityType Severity { get; set; }
        public string CorrelationId { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }

    public enum SeverityType
    {
        Info,
        Warning,
        Error,
        Critical,
        Trace
    }

    public enum CallType
    {
        UNKNOWN,
        WCF,
        REST
    }
}
