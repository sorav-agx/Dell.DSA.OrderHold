using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dell.OrderHold.Infrastructure.Threading;

namespace Dell.OrderHold.Infrastructure.Instrumentation
{
    public class GenericActionItem : IActionItem
    {
        private readonly Action _action;
        public GenericActionItem(string name, string description, Action action)
        {
            Name = name;
            Description = description;
            _action = action;
        }
        public void HandleException(ActionItemEventArgs args)
        {
            //Do nothing. Instrumentation and Log use same DB so logging would fail as well.
        }

        public void Execute()
        {
            _action();
        }

        public string Description { get; private set; }

        public string Name { get; private set; }

        public bool IsCancelled
        {
            get ; set ;
        }
    }
}
