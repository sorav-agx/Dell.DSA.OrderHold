using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Threading
{
    public class PollingHandler
    {
        private ManualResetEvent hasNewItems = new ManualResetEvent(false);
        private ManualResetEvent terminate = new ManualResetEvent(false);
        private ManualResetEvent waiting = new ManualResetEvent(false);
        private Thread thread;
        private readonly int _secondsToWait = 1;

        public bool IsSuspended { get; private set; }

        private List<IActionItem> _actions = new List<IActionItem>();
        public PollingHandler(int secondsToWait = 1)
        {
            _secondsToWait = secondsToWait;
            thread = new Thread(new ThreadStart(Start));
            thread.IsBackground = true;
            // this is performed from a bg thread, to ensure the queue is serviced from a single thread
            thread.Start();
        }

        public void AddPollingAction(IActionItem pollingActionItem)
        {
            if (pollingActionItem == null)
                throw new ArgumentNullException("actionHandlerItem");

            if (pollingActionItem == null)
                throw new ArgumentNullException("pollingActionItem");
            if (string.IsNullOrWhiteSpace(pollingActionItem.Name))
                throw new ArgumentNullException("actionHandlerItem.Name");
            if (string.IsNullOrWhiteSpace(pollingActionItem.Description))
                throw new ArgumentNullException("actionHandlerItem.Description");

            lock (_actions)
            {
                _actions.Add(pollingActionItem);
            } 
            hasNewItems.Set();
        }

        private void Start()
        {             
            while (true)
            {
                waiting.Set();
                int i = ManualResetEvent.WaitAny(new WaitHandle[] { hasNewItems, terminate });
                // terminate was signaled 
                if (i == 1)
                    return;
                hasNewItems.Reset();
                waiting.Reset();

                if (!IsSuspended)
                {
                    List<IActionItem> actions = new List<IActionItem>();
                    lock (_actions)
                    {
                        actions = new List<IActionItem>();
                        if (_actions != null)
                            actions.AddRange(_actions.Where(d => !d.IsCancelled));
                    }

                    foreach (var action in actions.Where(d => !d.IsCancelled))
                    {
                        try
                        {
                            action.Execute();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                ActionItemEventArgs args = new ActionItemEventArgs(action.Name, action.Description, ex);
                                action.HandleException(args);

                                action.IsCancelled = args.IsCancelled;
                            }
                            catch { }
                        }
                    }
                }

                System.Threading.Thread.Sleep(_secondsToWait * 1000);
            }
        }
        public void Flush()
        {
            waiting.WaitOne();
        }

        public void Dispose()
        {
            terminate.Set();
            thread.Join();
        }

        public void Suspend()
        {
            this.IsSuspended = true;
        }
        public void UnSuspend()
        {
            this.IsSuspended = false;
        }
    }
}
