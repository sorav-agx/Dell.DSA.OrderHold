using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dell.OrderHold.Infrastructure.Logging;

namespace Dell.OrderHold.Infrastructure.Logging.LogHandlers
{
    public class AggregateLogHandler : ILogHandler
    {
        private readonly List<ILogHandler> _logHandlers;
        private readonly bool _throwExceptions = false;

        public AggregateLogHandler(bool throwExceptions = false, params ILogHandler[] logHandlers)
        {
            if (logHandlers == null || logHandlers.Length == 0)
                throw new ArgumentNullException("logHandlers");

            this._logHandlers = logHandlers.ToList();
            this._throwExceptions = throwExceptions;
        }

        public bool CanLog(SeverityType severityType)
        {
            return _logHandlers.Any(x => x.CanLog(severityType));
        }

        public void Info(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            foreach (var logHandler in this._logHandlers)
                if (logHandler != null)
                {
                    try
                    {
                        logHandler.Info(message, description, properties);
                    }
                    catch
                    {
                        if (_throwExceptions)
                            throw;
                    }
                }
        }

        public void Warning(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            foreach (var logHandler in this._logHandlers)
                if (logHandler != null)
                {
                    try
                    {
                        logHandler.Warning(message, description, properties);
                    }
                    catch
                    {
                        if (_throwExceptions)
                            throw;
                    }
                }
        }

        public void Error(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            foreach (var logHandler in this._logHandlers)
                if (logHandler != null)
                {
                    try
                    {
                        logHandler.Error(message, description, properties);
                    }
                    catch
                    {
                        if (_throwExceptions)
                            throw;
                    }
                }
        }

        public void Critical(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            foreach (var logHandler in this._logHandlers)
                if (logHandler != null)
                {
                    try
                    {
                        logHandler.Critical(message, description, properties);
                    }
                    catch
                    {
                        if (_throwExceptions)
                            throw;
                    }
                }
        }

        public void Log(string source, string message, object description, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            foreach (var logHandler in this._logHandlers)
                if (logHandler != null)
                {
                    try
                    {
                        logHandler.Log(source, message, description, severityType, properties);
                    }
                    catch
                    {
                        if (_throwExceptions)
                            throw;
                    }
                }
        }

        public void LogException(string message, Exception exception, SeverityType severityType, params KeyValuePair<string, string>[] properties)
        {
            foreach (var logHandler in this._logHandlers)
                if (logHandler != null)
                {
                    try
                    {
                        logHandler.LogException(message, exception, severityType, properties);
                    }
                    catch
                    {
                        if (_throwExceptions)
                            throw;
                    }
                }
        }

        public void Trace(string message, object description, params KeyValuePair<string, string>[] properties)
        {
            foreach (var logHandler in this._logHandlers)
                if (logHandler != null)
                {
                    try
                    {
                        logHandler.Trace(message, description, properties);
                    }
                    catch
                    {
                        if (_throwExceptions)
                            throw;
                    }
                }
        }
    }
}
