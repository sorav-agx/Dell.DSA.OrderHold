using Dell.OrderHold.Infrastructure.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Dell.OrderHold.Infrastructure.Logging.LogHandlers
{
    public class RepositoryLogHandler : ILogHandler
    {
        protected readonly IDBLogItemRepository LogItemRepository;
        protected readonly string CorrelationId;
        protected readonly string Source;
        protected readonly IEnumerable<SeverityType> TypesToLog;
        protected readonly string HostIpAddress;
        protected readonly string HostName;
        protected readonly string Uri;
        /// <summary>
        /// When typesToLog is specified it will only log those severity types. If typesToLog is not specified then only INFO and TRACE will
        /// not be logged.
        /// </summary>
        /// <param name="dbLogItemRepository"></param>
        /// <param name="source"></param>
        /// <param name="correlationId"></param>
        /// <param name="typesToLog"></param>
        public RepositoryLogHandler(IDBLogItemRepository dbLogItemRepository, 
            string source, 
            string correlationId, 
            string hostIpAddress, 
            string hostName,
            string uri, 
            params SeverityType[] typesToLog)
        {
            if (dbLogItemRepository == null)
                throw new ArgumentNullException("DBLogItemRepository");
            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException("correlationId");
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException("source");
            if (string.IsNullOrWhiteSpace(hostIpAddress))
                throw new ArgumentNullException("hostIpAddress");
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentNullException("hostName");
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException("uri");

            Source = source;
            LogItemRepository = dbLogItemRepository;
            CorrelationId = correlationId;
            TypesToLog = typesToLog;
            this.HostName = hostName;
            this.HostIpAddress = hostIpAddress;
            this.Uri = uri;
        }

        public bool CanLog(SeverityType type)
        {
            if ((TypesToLog == null || !TypesToLog.Any()) && (type == SeverityType.Info || type == SeverityType.Trace))
                return false;
            else if (TypesToLog == null || !TypesToLog.Any())
                return true;

            return TypesToLog.Any(d => d == type);
        }

        public void Info(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            Log(Source, message, description, SeverityType.Info, properties);
        }

        public void Warning(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            Log(Source, message, description, SeverityType.Warning, properties);
        }

        public void Error(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            Log(Source, message, description, SeverityType.Error, properties);
        }

        public void Critical(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            Log(Source, message, description, SeverityType.Critical, properties);
        }

        public void Trace(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            Log(Source, message, description, SeverityType.Trace, properties);
        }

        public virtual void Log(string source, string message, object description, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            if (!CanLog(severityType))
                return;

            Dictionary<string, string> newProperties = properties.ToDictionary();
            if (newProperties == null)
                newProperties = new Dictionary<string, string>();

            newProperties.Add("HostIpAddress", HostIpAddress);
            newProperties.Add("HostName", HostName);
            newProperties.Add("Uri", Uri);

            LogItemRepository.Create(new LogItem()
            {
                CorrelationId = CorrelationId,
                DateCreated = DateTime.UtcNow,
                Description = JsonConvert.SerializeObject(description),
                Message = message,
                Severity = severityType,
                Properties = newProperties,
                Source = source
            });
        }

        public void LogException(string message, Exception exception, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            Log(Source, message, exception.ToString(), severityType, properties);
        }
    }
}
