using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.IIS
{
    public class IISLog
    {
        public DateTime Date { get; set; }
        public string ServerIpAddress { get; set; }
        public string HttpMethod { get; set; }
        public string BaseUri { get; set; }
        public string Querystring { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string ClientIpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Referer { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public int SubStatusCode { get; set; }
        public string Win32Status { get; set; }
        public int TimeTakenInMilliseconds { get; set; }
        public Dictionary<string, string> AdditionalValues { get; set; }
    }
}
