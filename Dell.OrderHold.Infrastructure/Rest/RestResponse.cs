using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Rest
{
    public class RestResponse
    {
        public RestResponse()
        {
            this.Headers = new List<Header>();
            this.Cookies = new List<Cookie>();
        }
        public List<Header> Headers { get; set; }
        public List<Cookie> Cookies { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string RawResponse { get; set; }
        public Exception Exception { get; set; }
    }

    public class RestResponse<T> : RestResponse
    {
        public T Data { get; set; }
    }
}
