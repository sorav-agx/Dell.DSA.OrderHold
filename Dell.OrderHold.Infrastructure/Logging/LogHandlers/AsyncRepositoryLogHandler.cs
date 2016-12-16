using Dell.OrderHold.Infrastructure.Threading;
using Dell.OrderHold.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dell.OrderHold.Infrastructure.Logging.LogHandlers
{
    public class AsyncRepositoryLogHandler : RepositoryLogHandler
    {
        private static FireAndForgetQueue _logExecutor = new FireAndForgetQueue();

        public AsyncRepositoryLogHandler(IDBLogItemRepository dbLogItemRepository, string source, string correlationId, string hostIpAddress, string hostName, string uri, params SeverityType[] typesToLog)
            : base(dbLogItemRepository, source, correlationId, hostIpAddress, hostName, uri, typesToLog)
        {
        }

        public override void Log(string source, string message, object description, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            Dictionary<string, string> newProperties = properties.ToDictionary();
            if (newProperties == null)
                newProperties = new Dictionary<string, string>();

            newProperties.Add("HostIpAddress", HostIpAddress);
            newProperties.Add("HostName", HostName);
            newProperties.Add("Uri", Uri);

            _logExecutor.QueueAction(new RepositoryActionItem(base.LogItemRepository, new LogItem()
            {
                Id = null,
                Source = source,
                Message = message,
                Description = JsonConvert.SerializeObject(description),
                Severity = severityType,
                CorrelationId = this.CorrelationId,
                DateCreated = DateTime.UtcNow,
                Properties = newProperties
            }));
        }
    }
}
