using System;
using System.Collections.Generic;
using System.Threading;

namespace CustomThreadPoolLib
{
    public class PoolEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int ThreadCount { get; set; }
        public int QueueLength { get; set; }
    }

    public class MyThreadPool : IDisposable
    {
        private readonly Queue<Action> _taskQueue = new Queue<Action>();
        private readonly List<Worker> _workers = new List<Worker>();
        private readonly int _minThreads;
        private readonly int _maxThreads;
        private readonly int _idleTimeoutMs;
        private bool _stop;
        private readonly object _lock = new object();

        public event EventHandler<PoolEventArgs> OnPoolChanged;
        public event EventHandler<PoolEventArgs> OnTaskStarted;

        public int CurrentThreadCount => _workers.Count;
        public int QueueLength => _taskQueue.Count;

        public MyThreadPool(int minThreads, int maxThreads, int idleTimeoutMs = 5000)
        {
            _minThreads = minThreads;
            _maxThreads = maxThreads;
            _idleTimeoutMs = idleTimeoutMs;

            for (int i = 0; i < _minThreads; i++) CreateWorker();
        }

        private void RaiseEvent(EventHandler<PoolEventArgs> handler, string msg)
        {
            handler?.Invoke(this, new PoolEventArgs { Message = msg, ThreadCount = _workers.Count, QueueLength = _taskQueue.Count });
        }

        public void Enqueue(Action task)
        {
            lock (_lock)
            {
                _taskQueue.Enqueue(task);
                Monitor.Pulse(_lock);

                if (_taskQueue.Count > 0 && _workers.Count < _maxThreads)
                {
                    CreateWorker();
                    RaiseEvent(OnPoolChanged, "Масштабирование UP");
                }
            }
        }

        private void CreateWorker()
        {
            var worker = new Worker(this);
            _workers.Add(worker);
            worker.Start();
        }

        private class Worker
        {
            private readonly MyThreadPool _pool;
            private readonly Thread _thread;
            public DateTime LastActive { get; private set; }
            public bool IsRunningTask { get; private set; }

            public Worker(MyThreadPool pool)
            {
                _pool = pool;
                _thread = new Thread(WorkLoop) { IsBackground = true };
                LastActive = DateTime.Now;
            }

            public void Start() => _thread.Start();

            private void WorkLoop()
            {
                try
                {
                    while (true)
                    {
                        Action task = null;
                        lock (_pool._lock)
                        {
                            while (_pool._taskQueue.Count == 0 && !_pool._stop)
                            {
                                if (!Monitor.Wait(_pool._lock, _pool._idleTimeoutMs))
                                {
                                    if (_pool._workers.Count > _pool._minThreads)
                                    {
                                        _pool._workers.Remove(this);
                                        _pool.RaiseEvent(_pool.OnPoolChanged, "Сжатие DOWN");
                                        return;
                                    }
                                }
                            }
                            if (_pool._stop) return;
                            if (_pool._taskQueue.Count > 0) task = _pool._taskQueue.Dequeue();
                        }

                        if (task != null)
                        {
                            IsRunningTask = true;
                            _pool.RaiseEvent(_pool.OnTaskStarted, "Задача запущена");
                            try { task(); }
                            catch { }
                            finally
                            {
                                IsRunningTask = false;
                                LastActive = DateTime.Now;
                            }
                        }
                    }
                }
                catch (ThreadAbortException) { }
            }

            public bool IsStuck(int timeoutMs) => IsRunningTask && (DateTime.Now - LastActive).TotalMilliseconds > timeoutMs;
            public void ForceStop() => _thread.Interrupt();
        }

        public void HealthCheck(int hangTimeoutMs)
        {
            lock (_lock)
            {
                for (int i = _workers.Count - 1; i >= 0; i--)
                {
                    if (_workers[i].IsStuck(hangTimeoutMs))
                    {
                        _workers[i].ForceStop();
                        _workers.RemoveAt(i);
                        CreateWorker();
                        RaiseEvent(OnPoolChanged, "Замена зависшего потока");
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _stop = true;
                Monitor.PulseAll(_lock);
            }
        }
    }
}