using System.Net;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Rest
{
    /// <summary>
    /// Specifies an interface that can send HTTP Requests.
    /// </summary>
    public interface IRestRequest
    {
        NetworkCredential Credentials { get; set; }





        void AddHeader(string key, string value);
        void AddCookie(string key, string value);
        void AddSerializer(IMediaTypeSerializer serializer);

        /// <summary>
        /// Submit http request with no request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        RestResponse Submit(string verb, string uri, int timeOutInSeconds = -1);

        /// <summary>
        /// Submit http request with a request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        RestResponse Submit(string verb, string uri, string requestBody, int timeOutInSeconds = -1);

        /// <summary>
        /// Submit http request with typed request body.  The serializer that matches the content-type header will be used to serialize
        /// the requestBody input parameter into string text and submitted.  If no content-type header is specified default is application/json
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        RestResponse Submit<TRequest>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new();

        /// <summary>
        /// Submit http request with no request body and respond with a strongly typed response object.  The serializer specified for the accept header
        /// will be used.  If Accept header is not specified then default is application/json.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        RestResponse<TResponse> Submit<TResponse>(string verb, string uri, int timeOutInSeconds = -1)
            where TResponse : class, new();

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
        RestResponse<TResponse> Submit<TRequest, TResponse>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new()
            where TResponse : class, new();

        /// <summary>
        /// Submit http request with no request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<RestResponse> SubmitAsync(string verb, string uri, int timeOutInSeconds = -1);

        /// <summary>
        /// Submit http request with a request body and get a non-typed RestResponse with raw response data.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        Task<RestResponse> SubmitAsync(string verb, string uri, string requestBody, int timeOutInSeconds = -1);

        /// <summary>
        /// Submit http request with typed request body.  The serializer that matches the content-type header will be used to serialize
        /// the requestBody input parameter into string text and submitted.  If no content-type header is specified default is application/json
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        Task<RestResponse> SubmitAsync<TRequest>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new();

        /// <summary>
        /// Submit http request with no request body and respond with a strongly typed response object.  The serializer specified for the accept header
        /// will be used.  If Accept header is not specified then default is application/json.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="verb"></param>
        /// <param name="uri"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        Task<RestResponse<TResponse>> SubmitAsync<TResponse>(string verb, string uri, int timeOutInSeconds = -1)
            where TResponse : class, new();

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
        Task<RestResponse<TResponse>> SubmitAsync<TRequest, TResponse>(string verb, string uri, TRequest requestBody, int timeOutInSeconds = -1)
            where TRequest : class, new()
            where TResponse : class, new();
    }
}