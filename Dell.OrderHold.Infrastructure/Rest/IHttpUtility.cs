using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Rest
{
    public interface IHttpUtility
    {
        HttpWebResponse SubmitWebRequest(string verb, string uri, int timeOutInSeconds = -1, string requestObj = null, IEnumerable<Rest.Header> requestHeaders = null, IEnumerable<Rest.Cookie> requestCookies = null, NetworkCredential credentials = null);
        Task<HttpWebResponse> SubmitWebRequestAsync(string verb, string uri, int timeOutInSeconds = -1, string requestObj = null, IEnumerable<Rest.Header> requestHeaders = null, IEnumerable<Rest.Cookie> requestCookies = null, NetworkCredential credentials = null);
    }
}
