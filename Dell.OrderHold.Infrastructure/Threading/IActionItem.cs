using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Threading
{
    public interface IActionItem
    {
        void HandleException(ActionItemEventArgs args);
        void Execute();
        string Description { get; }
        string Name { get; }
        bool IsCancelled { get; set; }
    }
}
