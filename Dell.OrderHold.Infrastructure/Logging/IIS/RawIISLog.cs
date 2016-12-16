using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.IIS
{
    public class RawIISLog
    {
        public RawIISLog()
        {
            this.AdditionalValues = new Dictionary<string, string>();
        }
        public string RawText { get; set; }

        public string Date { get; set; }
        public string Time { get; set; }
        public string ServerIpAddress { get; set; }
        public string HttpMethod { get; set; }
        public string BaseUri { get; set; }
        public string UriQuerystring { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string ClientIpAddress { get; set; }
        public string UserAgentHeader { get; set; }
        public string RefererHeader { get; set; }
        public string HttpStatusCode { get; set; }
        public string HttpSubStatus { get; set; }
        public string Win32Status { get; set; }
        public string TimeTaken { get; set; }
        public Dictionary<string, string> AdditionalValues { get; set; }

        public IISLog ToParsedLog()
        {
            return new IISLog()
            {
                Date = DateTime.Parse(Date + " " + Time),
                ServerIpAddress = ServerIpAddress,
                HttpMethod = HttpMethod,
                BaseUri = BaseUri,
                Querystring = UriQuerystring,
                Port = string.IsNullOrWhiteSpace(Port) ? 0 : int.Parse(Port),
                Username = Username,
                UserAgent = UserAgentHeader,
                ClientIpAddress = ClientIpAddress,
                Referer = RefererHeader,
                StatusCode = (System.Net.HttpStatusCode)int.Parse(HttpStatusCode),
                SubStatusCode = int.Parse(HttpSubStatus),
                Win32Status = Win32Status,
                TimeTakenInMilliseconds = int.Parse(TimeTaken),
                AdditionalValues = AdditionalValues
            };
        }
    }
}
