using Dell.OrderHold.Infrastructure.Logging;
using System;
using System.Collections.Generic;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public interface ILogHandler
    {
        bool CanLog(SeverityType severityType);
        void Info(string message, object description, params KeyValuePair<string, string>[] properties);
        void Warning(string message, object description, params KeyValuePair<string, string>[] properties);
        void Error(string message, object description, params KeyValuePair<string, string>[] properties);
        void Critical(string message, object description, params KeyValuePair<string, string>[] properties);
        void Trace(string message, object description, params KeyValuePair<string, string>[] properties);
        void Log(string source, string message, object description, SeverityType severityType, params KeyValuePair<string, string>[] properties);
        void LogException(string message, Exception exception, SeverityType severityType, params KeyValuePair<string, string>[] properties);
    }
}
