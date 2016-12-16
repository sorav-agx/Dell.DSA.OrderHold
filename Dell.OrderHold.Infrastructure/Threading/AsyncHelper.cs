using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Threading
{
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
  TaskFactory(CancellationToken.None,
              TaskCreationOptions.None,
              TaskContinuationOptions.None,
              TaskScheduler.Default);

        /// <summary>
        /// Example: return AsyncHelper.RunSync<Something>(() => manager.GetSomething(someId));
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        /// <summary>
        /// Example: AsyncHelper.RunSync(() => manager.DoSomething());
        /// </summary>
        /// <param name="func"></param>
        public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }

    }
}
