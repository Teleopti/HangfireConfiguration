using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hangfire.Configuration.Test
{
    public class ConcurrencyRunner
    {
        private class task
        {
            public Thread Thread;
            public Exception Exception;
        }

        private readonly List<task> _tasks = new List<task>();
        private Action _lastSyncAction;
        private Action _lastAsyncAction;

        public ConcurrencyRunner InSync(Action action)
        {
            _lastSyncAction = action;
            _lastAsyncAction = null;
            action.Invoke();
            return this;
        }

        public ConcurrencyRunner InParallel(Action action)
        {
            _lastSyncAction = null;
            _lastAsyncAction = action;
            addTask(_lastAsyncAction);
            return this;
        }

        public ConcurrencyRunner Times(int times)
        {
            if (_lastAsyncAction != null)
                (times - 1).Times(() => addTask(_lastAsyncAction));
            if (_lastSyncAction != null)
                (times - 1).Times(_lastSyncAction);
            return this;
        }

        private void addTask(Action action)
        {
            var task = new task();
            task.Thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    task.Exception = e;
                }
            });
            task.Thread.Start();
            _tasks.Add(task);
        }

        public void Wait()
        {
            _tasks.ForEach(t => t.Thread.Join());
            var exceptions = _tasks
                .Where(t => t.Exception != null)
                .Select(t => t.Exception)
                .ToArray();
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        public void Wait(TimeSpan timeout)
        {
	        var finished = _tasks.All(t => t.Thread.Join(timeout));
	        if (!finished)
		        throw new TimeoutException("BLIP");
	        var exceptions = _tasks
		        .Where(t => t.Exception != null)
		        .Select(t => t.Exception)
		        .ToArray();
	        if (exceptions.Any())
		        throw new AggregateException(exceptions);
        }
        
//
//        public void WaitForException<T>() where T : Exception
//        {
//            try
//            {
//                Wait();
//            }
//            catch (Exception e)
//            {
//                var matched = e.AllExceptions().OfType<T>().FirstOrDefault();
//                if (matched != null)
//                    throw matched;
//                throw;
//            }
//        }
    }
}