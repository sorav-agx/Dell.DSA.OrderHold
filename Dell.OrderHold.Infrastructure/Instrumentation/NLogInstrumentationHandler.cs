using System;
using Dell.OrderHold.Infrastructure.Logging;
using Newtonsoft.Json;
using NLog;

namespace Dell.OrderHold.Infrastructure.Instrumentation
{
    public class NLogInstrumentationHandler : IInstrumentationHandler
    {
        #region Fields

        private readonly string _correlationId;
        private readonly string _applicationId;
        private readonly string _serverName;
        private readonly string _clientIp;
        private readonly string _email;
        private readonly string _key;
        private readonly string _dsaReqestResponseLogCorrelationId;

        #endregion

        #region Properties

        public string DsaRequestResponseLogCorrelationId 
        {
            get { return _dsaReqestResponseLogCorrelationId; }
        }

        #endregion

        #region Constructors
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="applicationId"></param>
        /// <param name="serverName"></param>
        /// <param name="clientIp"></param>
        /// <param name="email"></param>
        /// <param name="key"></param>
        /// <param name="dsaReqestResponseLogCorrelationId"></param>
        public NLogInstrumentationHandler(string correlationId, string applicationId, string serverName, string clientIp, string email, string key, string dsaReqestResponseLogCorrelationId = null)
        {
            _correlationId = correlationId;
            _applicationId = applicationId;
            _serverName = serverName;
            _clientIp = clientIp;
            _email = email;
            _key = key;
            _dsaReqestResponseLogCorrelationId = dsaReqestResponseLogCorrelationId;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callType"></param>
        /// <param name="severityType"></param>
        /// <param name="isSuccessful"></param>
        /// <param name="serviceEndpoint"></param>
        /// <param name="operationName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="firstByteReceivedTime"></param>
        /// <param name="description"></param>
        public void AddEntry(Logging.CallType callType, Logging.SeverityType severityType, bool isSuccessful, string serviceEndpoint, string operationName, DateTime? startTime, DateTime? endTime, DateTime? firstByteReceivedTime, object description)
        {
            // Logs data to Instrumentation files.
            LogToInstrumentation(callType, severityType, isSuccessful, serviceEndpoint, operationName, startTime, endTime, firstByteReceivedTime, description);

            // Logs data to RequestResponse files.
            LogToRequestResponse(callType, severityType, isSuccessful, serviceEndpoint, operationName, startTime, endTime, firstByteReceivedTime, description);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callType"></param>
        /// <param name="severityType"></param>
        /// <param name="isSuccessful"></param>
        /// <param name="serviceEndpoint"></param>
        /// <param name="operationName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="firstByteReceivedTime"></param>
        /// <param name="description"></param>
        private void LogToInstrumentation(Logging.CallType callType, Logging.SeverityType severityType, bool isSuccessful, string serviceEndpoint, string operationName, DateTime? startTime, DateTime? endTime, DateTime? firstByteReceivedTime, object description)
        {
            var logger = NLog.LogManager.GetLogger("InstrumentationLogger");

            var logLevel = LogLevel.Error;

            switch (severityType)
            {
                case SeverityType.Error:
                    logLevel = LogLevel.Error;
                    break;
                case SeverityType.Critical:
                    logLevel = LogLevel.Fatal;
                    break;
                case SeverityType.Info:
                    logLevel = LogLevel.Info;
                    break;
                case SeverityType.Trace:
                    logLevel = LogLevel.Trace;
                    break;
                case SeverityType.Warning:
                    logLevel = LogLevel.Warn;
                    break;
            }

            var info = new LogEventInfo(logLevel, logger.Name, null, parameters: null, message: "This is a test");

            info.Properties.Add("applicationId", _applicationId);
            info.Properties.Add("callType", callType.ToString());
            info.Properties.Add("clientIp", _clientIp);
            info.Properties.Add("correlationId", _correlationId);
            //info.Properties.Add("email", _email);
            info.Properties.Add("endTime", endTime);
            info.Properties.Add("key", _key);
            info.Properties.Add("operationName", operationName);
            info.Properties.Add("serverName", _serverName);
            info.Properties.Add("serviceEndpoint", serviceEndpoint);
            info.Properties.Add("startTime", startTime);
            info.Properties.Add("isSuccessful", isSuccessful ? 1 : 0);

            if (endTime.HasValue && startTime.HasValue)
            {
                TimeSpan ts = endTime.Value - startTime.Value;
                info.Properties.Add("totalDuration", ts.TotalMilliseconds);
            }

            if (startTime.HasValue && firstByteReceivedTime.HasValue)
            {
                TimeSpan ts = firstByteReceivedTime.Value - startTime.Value;
                info.Properties.Add("firstByteReceivedDuration", ts.TotalMilliseconds);
            }

            if (!string.IsNullOrWhiteSpace(_dsaReqestResponseLogCorrelationId))
            {
                info.Properties.Add("dsaLogCorrelationId", _dsaReqestResponseLogCorrelationId);
            }

            // NLog.log
            logger.Log(info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callType"></param>
        /// <param name="severityType"></param>
        /// <param name="isSuccessful"></param>
        /// <param name="serviceEndpoint"></param>
        /// <param name="operationName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="firstByteReceivedTime"></param>
        /// <param name="description"></param>
        private void LogToRequestResponse(Logging.CallType callType, Logging.SeverityType severityType, bool isSuccessful, string serviceEndpoint, string operationName, DateTime? startTime, DateTime? endTime, DateTime? firstByteReceivedTime, object description)
        {
            var logger = NLog.LogManager.GetLogger("RequestResponseLogger");

            // Do not log when 'EnableDsaLog' is not true and for data related to Payments.
            if (!string.IsNullOrWhiteSpace(_dsaReqestResponseLogCorrelationId) && !string.IsNullOrEmpty(serviceEndpoint) && !serviceEndpoint.ToUpper().Contains("PAYMENTS"))
            {
                var logLevel = LogLevel.Error;

                switch (severityType)
                {
                    case SeverityType.Error:
                        logLevel = LogLevel.Error;
                        break;
                    case SeverityType.Critical:
                        logLevel = LogLevel.Fatal;
                        break;
                    case SeverityType.Info:
                        logLevel = LogLevel.Info;
                        break;
                    case SeverityType.Trace:
                        logLevel = LogLevel.Trace;
                        break;
                    case SeverityType.Warning:
                        logLevel = LogLevel.Warn;
                        break;
                }

                var info = new LogEventInfo(logLevel, logger.Name, null, parameters: null, message: "This is a test");

                info.Properties.Add("applicationId", _applicationId);
                info.Properties.Add("callType", callType.ToString());
                info.Properties.Add("clientIp", _clientIp);
                info.Properties.Add("correlationId", _correlationId);
                //info.Properties.Add("email", _email);
                info.Properties.Add("endTime", endTime);
                info.Properties.Add("key", _key);
                info.Properties.Add("operationName", operationName);
                info.Properties.Add("serverName", _serverName);
                info.Properties.Add("serviceEndpoint", serviceEndpoint);
                info.Properties.Add("startTime", startTime);
                info.Properties.Add("isSuccessful", isSuccessful ? 1 : 0);

                if (endTime.HasValue && startTime.HasValue)
                {
                    var ts = endTime.Value - startTime.Value;
                    info.Properties.Add("durationInMilliseconds", ts.TotalMilliseconds);
                }

                if (!string.IsNullOrWhiteSpace(_dsaReqestResponseLogCorrelationId))
                {
                    info.Properties.Add("dsaLogCorrelationId", _dsaReqestResponseLogCorrelationId);
                }

                if (description != null)
                {
                    string strDescription;
                    try
                    {
                        strDescription = JsonConvert.SerializeObject(description);
                    }
                    catch
                    {
                        strDescription = description.ToString();
                    }
                    
                    info.Properties.Add("description", strDescription);
                }

                // NLog.log
                logger.Log(info);
            }
        }

        #endregion
    }
}