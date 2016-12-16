using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.IIS
{
    public interface ILogParseExaminer
    {
        void LogParseExecuted(LogParseExecutedEventArgs args);
    }
}
