using System;

namespace Dell.OrderHold.Infrastructure.Instrumentation
{
    public interface IInstrumentationHandler
    {
        void AddEntry(Logging.CallType callType, Logging.SeverityType severityType, bool isSuccessful, string serviceEndpoint, string operationName, DateTime? startTime, DateTime? endTime,
            DateTime? firstByteReceivedTime, object description);
    }
}