using System.Security.Policy;
using Dell.OrderHold.Infrastructure.Logging;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace Dell.OrderHold.Infrastructure.Logging.LogHandlers
{
    public class NLogHandler : ILogHandler
    {
        private readonly string _correlationId;
        private readonly Logger _logger;
        private readonly bool _throwExceptions = false;
        private readonly string _ipAddress;
        private readonly string _environment;
        private readonly string _uri;
        private readonly string _verb;
        private readonly string _hostIpAddress;

        public NLogHandler(Logger logger, string ipAddress, string hostIpAddress, string environment, string uri, string verb, bool throwExceptions = false)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentNullException("ipAddress");
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException("environment");
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException("uri");
            if (string.IsNullOrWhiteSpace(verb))
                throw new ArgumentNullException("verb");
            if (string.IsNullOrWhiteSpace(hostIpAddress))
                throw new ArgumentNullException("hostIpAddress");
            _verb = verb;
            _uri = uri;
            _ipAddress = ipAddress;
            _environment = environment;
            _logger = logger;
            _throwExceptions = throwExceptions;
            _hostIpAddress = hostIpAddress;

        }

        public NLogHandler(string correlationId, string ipAddress, string hostIpAddress, string environment, string uri, string verb, Logger logger, bool throwExceptions = false)
            : this(logger, ipAddress, hostIpAddress, environment, uri, verb, throwExceptions)
        {
            if (correlationId == null) throw new ArgumentNullException("correlationId");
            _correlationId = correlationId;
        }

        public bool CanLog(SeverityType severityType)
        {
            switch (severityType)
            {
                case SeverityType.Info:
                    return _logger.IsInfoEnabled;
                case SeverityType.Warning:
                    return _logger.IsWarnEnabled;
                case SeverityType.Error:
                    return _logger.IsErrorEnabled;
                case SeverityType.Critical:
                    return _logger.IsFatalEnabled;
                case SeverityType.Trace:
                    return _logger.IsTraceEnabled;
            }
            return true;
        }

        public void Info(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            this.Log(this._logger.Name, message, description, Logging.SeverityType.Info, properties);
        }

        public void Warning(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            this.Log(this._logger.Name, message, description, Logging.SeverityType.Warning, properties);
        }

        public void Error(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            this.Log(this._logger.Name, message, description, Logging.SeverityType.Error, properties);
        }

        public void Critical(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            this.Log(this._logger.Name, message, description, Logging.SeverityType.Critical, properties);
        }

        public void Trace(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            this.Log(this._logger.Name, message, description, Logging.SeverityType.Trace, properties);
        }

        public void Log(string source, string message, object description, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            var logLevel = LogLevel.Info;

            switch (severityType)
            {
                case Logging.SeverityType.Info:
                    logLevel = LogLevel.Info;
                    break;
                case Logging.SeverityType.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Logging.SeverityType.Critical:
                    logLevel = LogLevel.Fatal;
                    break;
                case Logging.SeverityType.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Logging.SeverityType.Trace:
                    logLevel = LogLevel.Trace;
                    break;
            }

            var logEventInfo = new LogEventInfo(
                level: logLevel,
                loggerName: _logger.Name,
                formatProvider: null,
                parameters: null,
                message: message);
            logEventInfo.Properties["source"] = source;

            string strDescription = string.Empty;
            if (description != null)
            {
                try
                {
                    strDescription = JsonConvert.SerializeObject(description);
                }
                catch
                {
                    strDescription = description.ToString();
                }
            }

            logEventInfo.Properties["description"] = strDescription;
            logEventInfo.Properties["correlationId"] = _correlationId;

            logEventInfo.Properties["verb"] = _verb;
            logEventInfo.Properties["ipAddress"] = _ipAddress;
            logEventInfo.Properties["uri"] = _uri;
            logEventInfo.Properties["environment"] = _environment;
            logEventInfo.Properties["hostIpAddress"] = _hostIpAddress;

            try
            {
                _logger.Log(logEventInfo);
            }
            catch (Exception)
            {
                if (_throwExceptions)
                    throw;
            }
        }


        public void LogException(string message, Exception exception, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            var logLevel = LogLevel.Info;

            switch (severityType)
            {
                case Logging.SeverityType.Info:
                    logLevel = LogLevel.Info;
                    break;
                case Logging.SeverityType.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Logging.SeverityType.Critical:
                    logLevel = LogLevel.Fatal;
                    break;
                case Logging.SeverityType.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Logging.SeverityType.Trace:
                    logLevel = LogLevel.Trace;
                    break;
            }

            var logEventInfo = new LogEventInfo(
                    level: logLevel,
                    loggerName: _logger.Name,
                    formatProvider: null,
                    message: message,
                    parameters: null,
                    exception: exception);
            logEventInfo.Properties["source"] = _logger.Name;
            logEventInfo.Properties["description"] = JsonConvert.SerializeObject("");
            logEventInfo.Properties["correlationId"] = _correlationId;

            logEventInfo.Properties["verb"] = _verb;
            logEventInfo.Properties["ipAddress"] = _ipAddress;
            logEventInfo.Properties["uri"] = _uri;
            logEventInfo.Properties["environment"] = _environment;
            logEventInfo.Properties["hostIpAddress"] = _hostIpAddress;

            if (properties != null)
                foreach (var property in properties)
                    try
                    {
                        logEventInfo.Properties[property.Key] = property.Value;
                    }
                    catch { }

            try
            {
                _logger.Log(logEventInfo);
            }
            catch (Exception)
            {
                if (_throwExceptions)
                    throw;
            }
        }
    }
}
