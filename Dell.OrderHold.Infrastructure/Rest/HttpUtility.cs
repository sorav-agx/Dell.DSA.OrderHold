using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NLog.Internal;


namespace Dell.OrderHold.Infrastructure.Rest
{
    public class HttpUtility : IHttpUtility
    {
        private bool _keepAlive;
        public HttpUtility(bool keepAlive = true)
        {
            _keepAlive = keepAlive;
        }

        /// <summary>
        /// Returns the Timeout value from Client's config (if present) or default value
        /// </summary>
        /// <param name="timeOutInSeconds"></param>
        /// <returns></returns>
        private int SafeSetTimeOutFromClientConfig(int timeOutInSeconds)
        {
            if (timeOutInSeconds != -1)
            {
                //TimeOut value is explicitly set from the client , honour that
                return timeOutInSeconds;
            }
            else
            {
                //Try getting the config from Client's config file. If not , set the default value
                return TryGetIntegerFromConfig("RestTimeOutInSeconds", 100);

            }


        }

        /// <summary>
        /// Tries the get integer from configuration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        private static int TryGetIntegerFromConfig(string key, int defaultValue)
        {
            try
            {
                int timeOutInSecsFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings[key]);
                if (timeOutInSecsFromConfig > 0)
                {
                    return timeOutInSecsFromConfig;
                }
                else
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }
        public HttpWebResponse SubmitWebRequest(string verb, string uri, int timeOutInSeconds = -1, string requestObj = null, IEnumerable<Rest.Header> requestHeaders = null, IEnumerable<Rest.Cookie> requestCookies = null, NetworkCredential credentials = null)
        {
            if (requestHeaders == null)
                requestHeaders = new List<Rest.Header>();
            if (requestCookies == null)
                requestCookies = new List<Rest.Cookie>();

            verb = verb.ToUpper();
            if ((verb == "GET" || verb == "DELETE") && requestObj != null)
                throw new Exception(string.Format("Unable to submit a {0} request with a content body.", verb));

            var acceptValue = requestHeaders.GetHeaderValue("accept");
            var contentTypeValue = requestHeaders.GetHeaderValue("content-type");

            if (string.IsNullOrWhiteSpace(acceptValue))
                acceptValue = "application/json";
            if (string.IsNullOrWhiteSpace(contentTypeValue))
                contentTypeValue = "application/json";

            //If timeout is not set, read the timeOut value from client's config if present or set to default
            timeOutInSeconds = SafeSetTimeOutFromClientConfig(timeOutInSeconds);

            Stream dataStream = null;
            HttpWebResponse response = null;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = verb;
            request.ContentType = contentTypeValue;
            request.Accept = acceptValue;
            request.ContentLength = 0;
            request.Timeout = timeOutInSeconds * 1000;
            request.KeepAlive = _keepAlive;

            if (credentials != null)
                request.Credentials = credentials;

            if (requestHeaders != null)
                foreach (var header in requestHeaders.Where(d => !d.Key.ToLower().Equals("accept") && !d.Key.ToLower().Equals("content-type")))
                    request.Headers.Add(header.Key, header.Value);
            if (requestCookies != null)
            {
                request.CookieContainer = new CookieContainer();
                foreach (var cookie in requestCookies)
                {
                    request.CookieContainer.Add(new System.Net.Cookie()
                    {
                        Name = cookie.Key,
                        Value = cookie.Value,
                        Domain = new Uri(uri).Authority
                    });
                }
            }

            if (requestObj != null)
            {
                string postData = requestObj;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wEx)
            {
                response = (HttpWebResponse)wEx.Response;
            }

            return response;
        }

        public async Task<HttpWebResponse> SubmitWebRequestAsync(string verb, string uri, int timeOutInSeconds = -1, string requestObj = null, IEnumerable<Rest.Header> requestHeaders = null, IEnumerable<Rest.Cookie> requestCookies = null, NetworkCredential credentials = null)
        {
            if (requestHeaders == null)
                requestHeaders = new List<Rest.Header>();
            if (requestCookies == null)
                requestCookies = new List<Rest.Cookie>();

            verb = verb.ToUpper();
            if ((verb == "GET" || verb == "DELETE") && requestObj != null)
                throw new Exception(string.Format("Unable to submit a {0} request with a content body.", verb));

            var acceptValue = requestHeaders.GetHeaderValue("accept");
            var contentTypeValue = requestHeaders.GetHeaderValue("content-type");

            if (string.IsNullOrWhiteSpace(acceptValue))
                acceptValue = "application/json";
            if (string.IsNullOrWhiteSpace(contentTypeValue))
                contentTypeValue = "application/json";

            //If timeout is not set, read the timeOut value from client's config if present or set to default
            timeOutInSeconds = SafeSetTimeOutFromClientConfig(timeOutInSeconds);

            Stream dataStream = null;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = verb;
            request.ContentType = contentTypeValue;
            request.Accept = acceptValue;
            request.ContentLength = 0;
            request.Timeout = timeOutInSeconds * 1000;
            request.KeepAlive = _keepAlive;

            if (credentials != null)
                request.Credentials = credentials;

            if (requestHeaders != null)
                foreach (var header in requestHeaders.Where(d => !d.Key.ToLower().Equals("accept") && !d.Key.ToLower().Equals("content-type")))
                    request.Headers.Add(header.Key, header.Value);
            if (requestCookies != null)
            {
                request.CookieContainer = new CookieContainer();
                foreach (var cookie in requestCookies)
                {
                    request.CookieContainer.Add(new System.Net.Cookie()
                    {
                        Name = cookie.Key,
                        Value = cookie.Value,
                        Domain = new Uri(uri).Authority
                    });
                }
            }

            if (requestObj != null)
            {
                string postData = requestObj;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            WebResponse httpResponse = null;
            try
            {
                httpResponse = await request.GetResponseAsync();
            }
            catch (WebException wEx)
            {
                httpResponse = wEx.Response;
            }

            return httpResponse as HttpWebResponse;
        }
    }
}
