using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Threading
{
    public class ActionItemEventArgs : EventArgs
    {
        public ActionItemEventArgs(string actionName, string actionDescription, Exception exception)
        {
            this.ActionName = actionName;
            this.ActionDescription = actionDescription;
            this.Exception = exception;
        }
        public string ActionName { get; private set; }
        public string ActionDescription { get; set; }
        public Exception Exception { get; private set; }
        public void CancelAction()
        {
            this.IsCancelled = true;
        }
        internal bool IsCancelled { get; private set; }
    }
}
