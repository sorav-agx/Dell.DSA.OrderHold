using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dell.OrderHold.Infrastructure.Instrumentation;
using Newtonsoft.Json;

namespace Dell.OrderHold.Infrastructure.Instrumentation
{
    public class InstrumentationHandler : IInstrumentationHandler
    {
        protected readonly IDBInstrumentationItemRepository _repository;
        private readonly string _correlationId;
        private readonly string _applicationId;
        private readonly string _serverName;
        private readonly string _clientIp;
        private readonly string _email;
        private readonly string _key;

        public InstrumentationHandler(IDBInstrumentationItemRepository repository, string correlationId, string applicationId, string serverName, string clientIp, string email, string key)
        {
            if(repository == null)
                throw new ArgumentNullException("repository");
            _repository = repository;
            _correlationId = correlationId;
            _applicationId = applicationId;
            _serverName = serverName;
            _clientIp = clientIp;
            _email = email;
            _key = key;
        }

        public void AddEntry(Logging.CallType callType, Logging.SeverityType severityType, bool isSuccessful, string serviceEndpoint, string operationName, DateTime? startTime, DateTime? endTime, DateTime? firstByteReceivedTime, object description)
        {
            var entry = new InstrumentationItem
            {   
                CallType = callType.ToString(),
                ApplicationId = _applicationId,
                ClientIp = _clientIp,
                CorrelationId = _correlationId,
                Description = JsonConvert.SerializeObject(description),
                StartTime = startTime,
                EndTime = endTime,
                ServerName = _serverName,
                OperationName = operationName,
                ServiceEndpoint = serviceEndpoint,
                Email = _email,
                Key = _key,
                IsSuccessful = isSuccessful,
                DurationInMilliseconds = (endTime.HasValue && startTime.HasValue) ? (endTime.Value - startTime.Value).TotalMilliseconds : 0
            };
            try
            {
                _repository.Create(entry);
            }
            catch (Exception) { } //Non critical. If instrumentation fails, we do not want it failing the original service.
        }
    }
}
