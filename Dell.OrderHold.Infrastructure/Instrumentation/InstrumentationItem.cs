using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Instrumentation
{
    public class InstrumentationItem
    {
        public Guid Id { get; set; }
        public string CorrelationId { get; set; }
        public string ApplicationId { get; set; }
        public string ServiceEndpoint { get; set; }
        public string OperationName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ServerName { get; set; }
        public string ClientIp { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public double DurationInMilliseconds { get; set; }
        public bool IsSuccessful { get; set; }
        public string CallType { get; set; }
    }
}
