using System;
using System.Diagnostics;
using Dell.OrderHold.Infrastructure.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dell.OrderHold.Infrastructure.Logging.LogHandlers
{
    /// <summary>
    /// Do not use this right now, SCOM logging is being revisted.  
    /// A LogHandler implementation which logs to the Windows Event Log.  These event log entries are monitored by SCOM (allegedly).
    /// </summary>
    public class SCOMLogHandler : ILogHandler
    {
        private readonly string _source;
        private readonly string _ipAddress;
        private readonly string _environment;
        private readonly string _uri;
        private readonly string _verb;
        private readonly string _logSource = "Dell Actionable Events";
        private readonly bool _throwExceptions = false;
        private readonly string _correlationId;

        public SCOMLogHandler(string correlationId, string source, string ipAddress, string environment, string uri, string verb, bool throwExceptions = false)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            if (environment == null) throw new ArgumentNullException("environment");
            if (uri == null) throw new ArgumentNullException("uri");
            if (verb == null) throw new ArgumentNullException("verb");
            if (correlationId == null) throw new ArgumentNullException("correlationId");

            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException("source");

            if (!EventLog.SourceExists(_logSource))
            {
                if (_throwExceptions)
                    throw new ArgumentException(_logSource + " source does not exist in the event log.");
            }

            _correlationId = correlationId;
            _source = source;
            _ipAddress = ipAddress;
            _environment = environment;
            _uri = uri;
            _verb = verb;
            _throwExceptions = throwExceptions;
        }

        public bool CanLog(SeverityType severityType)
        {
            switch (severityType)
            {
                case SeverityType.Trace:
                case SeverityType.Info:
                case SeverityType.Warning:
                    return false;
                case SeverityType.Error:
                case SeverityType.Critical:
                    return true;
            }
            return true;
        }

        public void Info(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            // Info is not a valid SCOM actionable event
            //this.Log(this._source, message, description, Common.Logging.SeverityType.Info, properties);
        }

        public void Warning(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            // Warning is not a valid SCOM actionable event
            //this.Log(this._source, message, description, Common.Logging.SeverityType.Warning, properties);
        }

        public void Error(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            this.Log(this._source, message, description, Logging.SeverityType.Error, properties);
        }

        public void Critical(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            this.Log(this._source, message, description, Logging.SeverityType.Critical, properties);
        }

        public void Log(string source, string message, object description, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            int messageId = 0;
            if (!int.TryParse(message, out messageId))
            {
                if (_throwExceptions)
                    throw new ArgumentNullException("source must be of type System.Int32");
                else
                    return;
            }

            EventLogEntryType logType = EventLogEntryType.Information;

            switch (severityType)
            {
                case Logging.SeverityType.Critical:
                case Logging.SeverityType.Error:
                    logType = EventLogEntryType.Error;
                    break;
                case SeverityType.Trace:
                case Logging.SeverityType.Info:
                    logType = EventLogEntryType.Information;
                    return;
                case Logging.SeverityType.Warning:
                    logType = EventLogEntryType.Warning;
                    return;
                default:
                    throw new ArgumentException("severityType unknown");
            }

            var scomMessage = new SCOMUtility().BuildMessage(JsonConvert.SerializeObject(description), _ipAddress, _environment, _uri, "n/a", "n/a", _correlationId, _verb, properties);

            try
            {
                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, _logSource);

                EventLog.WriteEntry(source, scomMessage,
                    logType, messageId);

            }
            catch (Exception ex)
            {
                if (_throwExceptions)
                    throw ex;
            }
        }

        public void LogException(string message, Exception exception, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            Log(_source, message, exception, severityType, properties);
        }

        public void Trace(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            // Trace is not a valid SCOM actionable event
            //this.Log(this._source, message, description, Common.Logging.SeverityType.Trace, properties);
        }
    }
}
