using Dell.OrderHold.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Pulled code from http://stackoverflow.com/questions/1181561/how-to-effectively-log-asynchronously
namespace Dell.OrderHold.Infrastructure.Threading
{
    public class FireAndForgetQueue
    {
        private static FireAndForgetQueue _sharedExecutor = null;
        private static int SharedQueueThreshold { get; set; }

        private readonly bool _isSharingThread = false;
        private readonly int _queueThreshold = 2000;

        private Queue<IActionItem> queue = new Queue<IActionItem>();
        private ManualResetEvent hasNewItems = new ManualResetEvent(false);
        private ManualResetEvent terminate = new ManualResetEvent(false);
        private ManualResetEvent waiting = new ManualResetEvent(false);
        private Thread thread;

        /// <summary>
        /// Creates a new long running thread and new queue to process queued actions.
        /// </summary>
        /// <param name="queueThreshold"></param>
        public FireAndForgetQueue(int queueThreshold = 2000)
        {
            this._queueThreshold = queueThreshold;
            thread = new Thread(new ThreadStart(ProcessQueue));
            thread.IsBackground = true;
            // this is performed from a bg thread, to ensure the queue is serviced from a single thread
            thread.Start();
        }

        static FireAndForgetQueue()
        {
            SharedQueueThreshold = 5000;
        }

        /// <summary>
        /// Will use a single long running shared thread to process queued actions.  Default shared thread queueThreshold=2000.
        /// </summary>
        /// <param name="useSharedThread"></param>
        public FireAndForgetQueue(bool useSharedThread)
        {
            if (useSharedThread)
            {
                this._isSharingThread = true;
                if (_sharedExecutor == null)
                    _sharedExecutor = new FireAndForgetQueue(SharedQueueThreshold);
            }
            else
            {
                thread = new Thread(new ThreadStart(ProcessQueue));
                thread.IsBackground = true;
                // this is performed from a bg thread, to ensure the queue is serviced from a single thread
                thread.Start();
            }
        }

        private void ProcessQueue()
        {
            while (true)
            {
                waiting.Set();
                int i = ManualResetEvent.WaitAny(new WaitHandle[] { hasNewItems, terminate });
                // terminate was signaled 
                if (i == 1) return;
                hasNewItems.Reset();
                waiting.Reset();

                Queue<IActionItem> queueCopy = new Queue<IActionItem>();
                lock (queue)
                {
                    queueCopy = new Queue<IActionItem>(queue);
                    queue.Clear();
                }

                foreach (var action in queueCopy)
                {
                    try
                    {
                        action.Execute();
                    }
                    catch(Exception ex)
                    {
                        try
                        {
                            ActionItemEventArgs args = new ActionItemEventArgs(action.Name, action.Description, ex);
                            action.HandleException(args);
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Fire and forget. Queue action to be executed.
        /// </summary>
        /// <typeparam name="TInput1"></typeparam>
        /// <param name="action"></param>
        /// <param name="input1"></param>
        public void QueueAction(IActionItem action)
        {
            if (this._isSharingThread)
            {
                _sharedExecutor.QueueAction(action);
            }
            else
            {
                if (queue.Count < _queueThreshold) // very important.  Will only queue this many logs before it stops.
                {
                    lock (queue)
                    {
                        queue.Enqueue(action);
                    }
                }
                hasNewItems.Set();
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
    }
}
