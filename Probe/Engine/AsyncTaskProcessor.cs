using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Probe.Engine
{
    public class AsyncTaskProcessor
    {
        private BackgroundWorker taskProcessor = new BackgroundWorker();

        private Queue<IAsyncTask> queue = new Queue<IAsyncTask>();
        private IAsyncTask _currentAsyncTask;
        private bool _working;
        private Object _lock = new Object();

        public bool Busy { get { return queue.Count != 0 || _working; } }

        public AsyncTaskProcessor()
        {
            taskProcessor.DoWork += TaskProcessorDoWork;
            taskProcessor.RunWorkerCompleted += TaskProcessorRunWorkerCompleted;
            taskProcessor.RunWorkerAsync();
            _working = false;
        }

        void TaskProcessorRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _currentAsyncTask.OnComplete();
            _currentAsyncTask = null;
            if (!taskProcessor.IsBusy) taskProcessor.RunWorkerAsync();
        }

        void TaskProcessorDoWork(object sender, DoWorkEventArgs e)
        {
            _working = false;
            while (queue.Count == 0)
            {
                Thread.Sleep(500);
            }
            _working = true;
            lock (_lock)
            {
                _currentAsyncTask = queue.Dequeue();
            }
            _currentAsyncTask.Processor = this;
            _currentAsyncTask.Run();
        }

        public void Add(IAsyncTask task)
        {
            lock (_lock)
            {
                queue.Enqueue(task);
            }
        }

        public void AddUnique(IAsyncTask task)
        {
            lock (_lock)
            {
                if (!TaskOfTypeEqueued(task.GetType())) Add(task);
            }
        }

        private bool TaskOfTypeEqueued(Type type)
        {
            lock (_lock)
            {
                foreach (IAsyncTask task in queue)
                {
                    if (type.Equals(task)) return true;
                }
                return type.Equals(_currentAsyncTask);
            }
        }
    }
}
