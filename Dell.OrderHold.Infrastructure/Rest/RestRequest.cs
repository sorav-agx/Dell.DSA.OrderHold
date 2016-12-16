using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Dell.OrderHold.Infrastructure.Instrumentation;
using System.Threading.Tasks;
using Dell.OrderHold.Infrastructure.Logging;

namespace Dell.OrderHold.Infrastructure.Rest
{
    /// <summary>
    /// Class used to easily make REST (or Http) requests.  Comes, by default, with JSON serializer installed.  If a media type
    /// is being used that is NOT JSON a serializer will need to be created and added via AddSerializer(...).
    /// </summary>
    public class RestRequest : IRestRequest
    {
        private readonly List<Header> _requestHeaders = new List<Header>();
        private readonly List<Cookie> _requestCookies = new List<Cookie>();
        private readonly List<IMediaTypeSerializer> _serializers = new List<IMediaTypeSerializer>();
        private readonly IHttpUtility _httpUtility;

        public NetworkCredential Credentials { get; set; }

        private IInstrumentationHandler InstrumentationHandler { get; set; }

        /// <summary>
        /// When this constructor is used the http utility that is used will be the default Dell.DSA.Common.Infrastructure.Rest.HttpUtility.
        /// </summary>
        public RestRequest()
            : this(new Dell.OrderHold.Infrastructure.Rest.HttpUtility())
        {
        }

        /// <summary>
        /// When this constructor is used the http utility that is used will be the default Dell.DSA.Common.Infrastructure.Rest.HttpUtility.
        /// </summary>
        public RestRequest(IInstrumentationHandler handler)
            : this(new Dell.OrderHold.Infrastructure.Rest.HttpUtility())
        {
            InstrumentationHandler =  handler;
        }

        /// <summary>
        /// When this constructor is used the http utility that is injected will be used to submit the underlying
        /// http request for this RestRequest class.
        /// </summary>
        /// <param name="httpUtility"></param>
        /// <param name="handler"></param>
        public RestRequest(IHttpUtility httpUtility, IInstrumentationHandler handler = null)
        {
            if (httpUtility == null)
                throw new ArgumentNullException("httpUtility");

            _serializers.Add(new JsonNetSerializer());
            _httpUtility = httpUtility;
            InstrumentationHandler = handler;
        }

        public void AddHeader(string key, string value)
        {
            if (value == null)
                value = "";
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            var existingHeader = _requestHeaders.FirstOrDefault(d => d.Key.ToLower().Equals(key.ToLower()));

            if (existingHeader == null)
                _requestHeaders.Add(new Header(key, value));
            else
            {
                existingHeader.Value += ";" + value;
            }
        }

        public void AddCookie(string key, string value)
        {
            if (value == null)
                value = "";
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            var cookie = _requestCookies.FirstOrDefault(d => d.Key.ToLower().Equals(key.ToLower()));
            if (cookie != null)
                throw new ArgumentException(string.Format("cookie with key {0} already exists in request.", key));

            _requestCookies.Add(new Cookie(key, value));
        }

        public void AddSerializer(IMediaTypeSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var existingSerializer = _serializers.FirstOrDefault(d => d.GetType().FullName == serializer.GetType().FullName);
            if (existingSerializer != null)
                throw new ArgumentException("serializer already exists in collection.");

            this._serializers.Add(serializer);
        }

        #region " SYNC "
        /// <summary>
        /// Submit http request with no request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="timeOutInSeconds"></param>
        /// <returns></returns>
        public RestResponse Submit(string verb, string uri, int timeOutInSeconds = -1)
        {
            return this.Submit(verb, uri, null, timeOutInSeconds);
        }
        /// <summary>
        /// Submit http request with a request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <param name="timeOutInSeconds"></param>
        /// <returns></returns>
        public RestResponse Submit(string verb, string uri, string requestBody, int timeOutInSeconds = -1)
        {
            if (string.IsNullOrWhiteSpace(verb))
                throw new ArgumentNullException("verb");
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException("uri");

            string responseFromServer = null;
            Stream dataStream = null;
            StreamReader reader = null;

            var startTime = DateTime.UtcNow;
            RestResponse response = new RestResponse();
            var httpResponse = _httpUtility.SubmitWebRequest(verb, uri, timeOutInSeconds, requestBody, _requestHeaders, _requestCookies, this.Credentials);
            DateTime? firstByteReceivedTime = null;

            try
            {
                dataStream = httpResponse.GetResponseStream();
                firstByteReceivedTime = DateTime.UtcNow;
                if (dataStream != null)
                {
                    reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }
            }
            catch (WebException wEx)
            {
                response.Exception = wEx;

                try
                {
                    dataStream = wEx.Response.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                catch { }
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            
            response.RawResponse = responseFromServer;

            if (httpResponse != null)
            {
                response.StatusCode = httpResponse.StatusCode;
                if (httpResponse.Headers != null)
                    foreach (string headerKey in httpResponse.Headers.AllKeys)
                        response.Headers.Add(new Header(headerKey, httpResponse.Headers[headerKey]));

                if (httpResponse.Cookies != null)
                    for (int i = 0; i < httpResponse.Cookies.Count; i++)
                        response.Cookies.Add(new Cookie(httpResponse.Cookies[i].Name, httpResponse.Cookies[i].Value));
            }
            else
                response.StatusCode = HttpStatusCode.BadGateway;

            // Log data.
            LogData(verb, uri, startTime, firstByteReceivedTime, requestBody, _requestHeaders, response);

            return response;
        }
        /// <summary>
        /// Submit http request with typed request body.  The serializer that matches the content-type header will be used to serialize
        /// the requestBody input parameter into string text and submitted.  If no content-type header is specified default is application/json
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        public RestResponse Submit<TRequest>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new()
        {
            var serializer = GetContentTypeSerializer();
            if (serializer == null)
                throw new ArgumentException("Unable to determine serializer for content-type header specified.");

            return this.Submit(verb, uri, serializer.Serialize(requestBody), timeOutInSeconds);
        }
        /// <summary>
        /// Submit http request with no request body and respond with a strongly typed response object.  The serializer specified for the accept header
        /// will be used.  If Accept header is not specified then default is application/json.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public RestResponse<TResponse> Submit<TResponse>(string verb, string uri, int timeOutInSeconds = -1)
            where TResponse : class, new()
        {
            var serializer = GetAcceptSerializer();
            if (serializer == null)
                throw new ArgumentException("Unable to determine serializer for accept header specified.");

            var r = this.Submit(verb, uri, timeOutInSeconds);
            RestResponse<TResponse> response = new RestResponse<TResponse>();

            response.Cookies = r.Cookies;
            response.Exception = r.Exception;
            response.Headers = r.Headers;
            response.RawResponse = r.RawResponse;
            response.StatusCode = r.StatusCode;

            try
            {
                if (!string.IsNullOrWhiteSpace(r.RawResponse))
                    response.Data = serializer.DeSerialize<TResponse>(response.RawResponse);
            }
            catch { }

            return response;
        }
        /// <summary>
        /// Submit http request with strongly typed request body and get a strongly typed response object.  The serializer for content-type header will be used
        /// to serialize the requestBody for submitting the request.  The serializer for Accept header will be used to deserialize the object from the http response.
        /// If no Accept or Content-Type header is specified application/json will be used.
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        public RestResponse<TResponse> Submit<TRequest, TResponse>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            var serializer = GetAcceptSerializer();
            if (serializer == null)
                throw new ArgumentException("Unable to determine serializer for accept header specified.");

            var httpResponse = this.Submit<TRequest>(verb, uri, requestBody, timeOutInSeconds);
            RestResponse<TResponse> response = new RestResponse<TResponse>();

            response.Cookies = httpResponse.Cookies;
            response.Exception = httpResponse.Exception;
            response.Headers = httpResponse.Headers;
            response.RawResponse = httpResponse.RawResponse;
            response.StatusCode = httpResponse.StatusCode;

            try
            {
                if (!string.IsNullOrWhiteSpace(httpResponse.RawResponse))
                    response.Data = serializer.DeSerialize<TResponse>(httpResponse.RawResponse);
            }
            catch { }

            return response;
        }

        #endregion

        #region " ASYNC "

        /// <summary>
        /// Submit http request with no request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<RestResponse> SubmitAsync(string verb, string uri, int timeOutInSeconds = -1)
        {
            return await this.SubmitAsync(verb, uri, null, timeOutInSeconds);
        }
        /// <summary>
        /// Submit http request with a request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        public async Task<RestResponse> SubmitAsync(string verb, string uri, string requestBody, int timeOutInSeconds = -1)
        {
            if (string.IsNullOrWhiteSpace(verb))
                throw new ArgumentNullException("verb");
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException("uri");

            string responseFromServer = null;
            Stream dataStream = null;
            StreamReader reader = null;

            var startTime = DateTime.UtcNow;
            RestResponse response = new RestResponse();
            var httpResponse = await _httpUtility.SubmitWebRequestAsync(verb, uri, timeOutInSeconds, requestBody, _requestHeaders, _requestCookies, this.Credentials);
            DateTime? firstByteReceivedTime = null;

            try
            {
                dataStream = httpResponse.GetResponseStream();
                firstByteReceivedTime = DateTime.UtcNow;

                if (dataStream != null)
                {
                    reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }
            }
            catch (WebException wEx)
            {
                response.Exception = wEx;

                try
                {
                    dataStream = wEx.Response.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                catch { }
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }

            response.RawResponse = responseFromServer;

            if (httpResponse != null)
            {
                response.StatusCode = httpResponse.StatusCode;
                if (httpResponse.Headers != null)
                    foreach (string headerKey in httpResponse.Headers.AllKeys)
                        response.Headers.Add(new Header(headerKey, httpResponse.Headers[headerKey]));

                if (httpResponse.Cookies != null)
                    for (int i = 0; i < httpResponse.Cookies.Count; i++)
                        response.Cookies.Add(new Cookie(httpResponse.Cookies[i].Name, httpResponse.Cookies[i].Value));
            }
            else
                response.StatusCode = HttpStatusCode.BadGateway;

            // Log Data.
            LogData(verb, uri, startTime, firstByteReceivedTime, requestBody, _requestHeaders, response);

            return response;
        }
        /// <summary>
        /// Submit http request with typed request body.  The serializer that matches the content-type header will be used to serialize
        /// the requestBody input parameter into string text and submitted.  If no content-type header is specified default is application/json
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        public async Task<RestResponse> SubmitAsync<TRequest>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new()
        {
            var serializer = GetContentTypeSerializer();
            if (serializer == null)
                throw new ArgumentException("Unable to determine serializer for content-type header specified.");

            return await this.SubmitAsync(verb, uri, serializer.Serialize(requestBody), timeOutInSeconds);
        }
        /// <summary>
        /// Submit http request with no request body and respond with a strongly typed response object.  The serializer specified for the accept header
        /// will be used.  If Accept header is not specified then default is application/json.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public async Task<RestResponse<TResponse>> SubmitAsync<TResponse>(string verb, string uri, int timeOutInSeconds = -1)
            where TResponse : class, new()
        {
            var serializer = GetAcceptSerializer();
            if (serializer == null)
                throw new ArgumentException("Unable to determine serializer for accept header specified.");

            var r = await this.SubmitAsync(verb, uri, timeOutInSeconds);
            RestResponse<TResponse> response = new RestResponse<TResponse>();

            response.Cookies = r.Cookies;
            response.Exception = r.Exception;
            response.Headers = r.Headers;
            response.RawResponse = r.RawResponse;
            response.StatusCode = r.StatusCode;

            try
            {
                if (!string.IsNullOrWhiteSpace(r.RawResponse))
                    response.Data = serializer.DeSerialize<TResponse>(response.RawResponse);
            }
            catch { }

            return response;
        }
        /// <summary>
        /// Submit http request with strongly typed request body and get a strongly typed response object.  The serializer for content-type header will be used
        /// to serialize the requestBody for submitting the request.  The serializer for Accept header will be used to deserialize the object from the http response.
        /// If no Accept or Content-Type header is specified application/json will be used.
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        public async Task<RestResponse<TResponse>> SubmitAsync<TRequest, TResponse>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            var serializer = GetAcceptSerializer();
            if (serializer == null)
                throw new ArgumentException("Unable to determine serializer for accept header specified.");

            var httpResponse = await this.SubmitAsync<TRequest>(verb, uri, requestBody, timeOutInSeconds);
            RestResponse<TResponse> response = new RestResponse<TResponse>();

            response.Cookies = httpResponse.Cookies;
            response.Exception = httpResponse.Exception;
            response.Headers = httpResponse.Headers;
            response.RawResponse = httpResponse.RawResponse;
            response.StatusCode = httpResponse.StatusCode;

            try
            {
                if (!string.IsNullOrWhiteSpace(httpResponse.RawResponse))
                    response.Data = serializer.DeSerialize<TResponse>(httpResponse.RawResponse);
            }
            catch { }

            return response;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IMediaTypeSerializer GetAcceptSerializer()
        {
            var acceptValue = _requestHeaders.GetHeaderValue("accept");
            if (string.IsNullOrWhiteSpace(acceptValue))
                acceptValue = "application/json";

            return GetSerializer(acceptValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IMediaTypeSerializer GetContentTypeSerializer()
        {
            var acceptValue = _requestHeaders.GetHeaderValue("content-type");
            if (string.IsNullOrWhiteSpace(acceptValue))
                acceptValue = "application/json";

            return GetSerializer(acceptValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        private IMediaTypeSerializer GetSerializer(string mediaType)
        {
            IMediaTypeSerializer s = null;

            foreach (var serializer in _serializers)
                if (serializer.MediaTypes != null)
                    foreach (var type in serializer.MediaTypes)
                        if (type.ToLower().Equals(mediaType.ToLower()))
                        {
                            if (s != null)
                                throw new Exception("multiple serializers were matched for given request.  Make sure no two serializers exist with same media type.");
                            else
                            {
                                s = serializer;
                                continue;
                            }
                        }
            return s;

        }

        /// <summary>
        /// Logs data to required log files.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="startTime"></param>
        /// <param name="firstByteReceivedTime"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="response"></param>
        private void LogData(string verb, string uri, DateTime startTime, DateTime? firstByteReceivedTime, string requestBody, List<Header> requestHeaders, RestResponse response)
        {
            try
            {
                // Logs data to Instrumentation files.
                var instrumentationHandler = InstrumentationHandler as NLogInstrumentationHandler;

                if (instrumentationHandler != null)
                {
                    var endTime = DateTime.UtcNow;
                    var description = BuildDescription(verb, uri, requestBody, requestHeaders, response);
                    var severityType = response.Exception == null ? SeverityType.Info : SeverityType.Error;

                    instrumentationHandler.AddEntry(CallType.REST, severityType, response.StatusCode == HttpStatusCode.OK, uri, verb,
                        startTime, endTime, firstByteReceivedTime, description.ToString());
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private static StringBuilder BuildDescription(string verb, string uri, string requestBody, List<Header> requestHeaders,
            RestResponse response)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.Append(@"""Uri"" : " + (string.IsNullOrEmpty(uri) ? @""" """ : "\"" + uri + "\""));
            builder.Append(", \n");
            builder.Append(@"""Http Verb"" : " +
                           (string.IsNullOrEmpty(verb) ? @""" """ : "\"" + verb + "\""));
            builder.Append(", \n");
            builder.Append(@"""RequestHeaders"": ");

            if (requestHeaders.Count == 0)
                builder.Append(@"""  "" ");
            else
            {
                builder.Append(@""" ");
                foreach (var header in requestHeaders)
                {
                    builder.Append(" " + header.Key + ": " + header.Value + " ");
                }
                builder.Append(@""" ");
            }
            builder.Append(", \n");
            builder.Append(@"""RequestBody"" : " +
                           (string.IsNullOrEmpty(requestBody) ? @""" """ : requestBody));
            builder.Append(", \n");
            builder.Append(@"""Response StatusCode"" : " + "\"" + response.StatusCode + "\"");
            builder.Append(", \n");
            builder.Append(@"""ResponseHeaders"" : ");
            if (response.Headers.Count == 0)
                builder.Append(@"""  "" ");
            else
            {
                builder.Append(@""" ");
                foreach (var header in response.Headers)
                {
                    builder.Append(" " + header.Key + " : " + header.Value + " ");
                }
                builder.Append(@""" ");
            }

            if (response.Exception != null)
            {
                builder.Append(", \n");
                builder.Append(@"""Exception"" : " + @""" " + response.Exception.ToString() + @""" ");
            }
            builder.Append(", \n");
            builder.Append(@"""RawResponse"" : " + response.RawResponse);
            return builder;
        }
    }
}
