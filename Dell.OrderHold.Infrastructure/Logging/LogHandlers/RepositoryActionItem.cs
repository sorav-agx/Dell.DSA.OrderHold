using Dell.OrderHold.Infrastructure.Threading;
using Dell.OrderHold.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.LogHandlers
{
    public class RepositoryActionItem : IActionItem
    {
        private readonly LogItem _logItem;
        private readonly IDBLogItemRepository _logRepository;

        public RepositoryActionItem(IDBLogItemRepository logRepository, LogItem log)
        {
            _logItem = log;
            _logRepository = logRepository;
        }

        public void HandleException(ActionItemEventArgs args)
        {
            // log failed to execute
        }

        public void Execute()
        {
            _logRepository.Create(_logItem);
        }

        public string Description
        {
            get { return "Logs a single log item assigned to the class."; }
        }

        public string Name
        {
            get { return "LogItemLogger"; }
        }

        public bool IsCancelled { get; set; }
    }
}
