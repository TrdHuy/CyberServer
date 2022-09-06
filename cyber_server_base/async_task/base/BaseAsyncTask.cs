using cyber_server_base.async_task.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cyber_server_base.async_task.@base
{
    public abstract class BaseAsyncTask : IAsyncTask, IDisposable
    {
        private bool _isExecuteableFlagUpdated = false;
        private bool _isDisposed = false;
        private bool _isCompleted;
        private bool _isFaulted;
        private bool _isCompletedCallback;
        private bool _isCanceled;
        private bool _isExecuting;
        protected AsyncTaskResult _result { get; set; }
        protected string _name { get; set; }
        protected ulong _delayTime { get; set; } = 0;
        protected ulong _estimatedTime { get; set; } = 0;
        protected double _progress { get; set; } = 0d;
        protected int _reportDelay { get; set; } = 0;
        protected bool _isEnableReport { get; set; } = true;

        public ulong DelayTime => _delayTime;
        public AsyncTaskResult Result => _result;
        public bool IsCompletedCallback
        {
            get => _isCompletedCallback;
            protected set => _isCompletedCallback = value;
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                var oldVal = _isExecuting;
                _isExecuting = value;

                if (oldVal != value)
                {
                    OnExecutingChanged?.Invoke(this, oldVal, value);
                }
            }
        }
        public bool IsFaulted
        {
            get => _isFaulted;
            private set
            {
                var oldVal = _isFaulted;
                _isFaulted = value;

                if (oldVal != value)
                {
                    OnFaultedChanged?.Invoke(this, oldVal, value);
                }
            }
        }
        public bool IsCompleted
        {
            get => _isCompleted;
            private set
            {
                var oldVal = _isCompleted;
                _isCompleted = value;

                if (oldVal != value)
                {
                    OnCompletedChanged?.Invoke(this, oldVal, value);
                }
            }
        }
        public bool IsCanceled
        {
            get => _isCanceled;
            private set
            {
                var oldVal = _isCanceled;
                _isCanceled = value;

                if (oldVal != value)
                {
                    OnCanceledChanged?.Invoke(this, oldVal, value);
                }
            }
        }
        public string Name { get => _name; set => _name = value; }
        public ulong EstimatedTime => _estimatedTime;
        public double CurrentProgress
        {
            get => _progress;
            protected set
            {
                var oldVal = _progress;
                _progress = value;

                if (oldVal != value)
                {
                    ProgressChanged?.Invoke(this, value);
                }
            }
        }

        public bool IsExecuteable { get; private set; }

        public BaseAsyncTask(
             string name = ""
            , ulong estimatedTime = 0
            , ulong delayTime = 0
            , int reportDelay = 1000)
        {
            _name = name;
            _estimatedTime = estimatedTime;
            _delayTime = delayTime;
            _result = new AsyncTaskResult(null, MessageAsyncTaskResult.Non);
            _reportDelay = reportDelay;
        }

        public event IsCompletedChangedHandler OnCompletedChanged;
        public event IsCanceldChangedHandler OnCanceledChanged;
        public event IsFaultedChangedHandler OnFaultedChanged;
        public event IsExecutingChangedHandler OnExecutingChanged;
        public event ProgressChangedHandler ProgressChanged;

        public bool CanThisTaskExecuteable()
        {
            if (!_isExecuteableFlagUpdated)
                IsExecuteable = CanMainFuncExecute();
            _isExecuteableFlagUpdated = true;
            return IsExecuteable;
        }

        public async Task<BaseAsyncTask> Execute(bool isAsyncCallback = false)
        {
            if (_isDisposed) throw new InvalidOperationException("Can not exectue a disposed task");

            var asynTaskExecuteWatcher = Stopwatch.StartNew();
            if (IsCanceled || IsCompleted || IsFaulted) return this;

            // Cập nhật cờ executeable
            CanThisTaskExecuteable();

            if (IsExecuteable)
            {
                // Cập nhật cờ executing
                IsExecuting = true;

                // Thực hiện report tiến độ nếu cần thiết
                if (_isEnableReport)
                {
                    DoReportTask();
                }
                try
                {
                    // do main task
                    await Task.Run(DoMainFunc)
                    .ContinueWith((t) =>
                    {
                        // stop clock
                        asynTaskExecuteWatcher.Stop();

                        // handle task exception
                        // only handle operation cancel exception
                        // exception will be thrown here
                        HandleMainTaskException(t);
                    });
                    long restLoadingTime = (long)DelayTime - asynTaskExecuteWatcher.ElapsedMilliseconds;
                    if (restLoadingTime > 0)
                    {
                        // delay the task if its execution duration smaller than inited delay time
                        await DoWaitRestDelay(restLoadingTime);
                    }

                    // set completed flag
                    IsCompleted = true;

                    // default set message result if it was not handled
                    if (_result.MesResult == MessageAsyncTaskResult.Non)
                    {
                        _result.MesResult = MessageAsyncTaskResult.Finished;
                    }
                    // update progress when it completed
                    CurrentProgress = 100;

                    if (isAsyncCallback)
                    {
                        await Task.Run(DoCallback);
                    }
                    else
                    {
                        await DoCallback();
                    }
                    IsCompletedCallback = true;
                }
                catch (OperationCanceledException oce)
                {
                    // when the task was canceled by user
                    // the callback will be triggered
                    _result.Messsage = oce.ToString();
                    _result.MesResult = MessageAsyncTaskResult.Aborted;
                    IsCompleted = false;
                    IsFaulted = false;
                    IsCanceled = true;
                    if (isAsyncCallback)
                    {
                        await Task.Run(DoCallback);
                    }
                    else
                    {
                        await DoCallback();
                    }
                    IsCompletedCallback = true;
                }
                catch (Exception ex)
                {
                    // when the task was canceled due to function exception
                    // callback will not be triggered
                    _result.Messsage = ex.ToString();
                    _result.MesResult = MessageAsyncTaskResult.Faulted;
                    IsCompleted = false;
                    IsFaulted = true;
                    IsCanceled = false;
                    IsCompletedCallback = false;
                }
                finally
                {
                    // Cập nhật cờ executing khi thực hiện toàn bộ
                    // công việc chính và callback
                    IsExecuting = false;
                }
            }
            else
            {
                _result.MesResult = MessageAsyncTaskResult.DoneWithoutExecuted;
                // set completed flag
                IsCompleted = true;

                if (isAsyncCallback)
                {
                    await Task.Run(DoCallback);
                }
                else
                {
                    await DoCallback();
                }
                IsCompletedCallback = true;
            }
            return this;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            foreach (Delegate d in OnExecutingChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                OnExecutingChanged -= (IsExecutingChangedHandler)d;
            }
            foreach (Delegate d in OnCompletedChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                OnCompletedChanged -= (IsCompletedChangedHandler)d;
            }
            foreach (Delegate d in OnCanceledChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                OnCanceledChanged -= (IsCanceldChangedHandler)d;
            }
            foreach (Delegate d in OnFaultedChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                OnFaultedChanged -= (IsFaultedChangedHandler)d;
            }
            foreach (Delegate d in ProgressChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                ProgressChanged -= (ProgressChangedHandler)d;
            }

            if (IsExecuting)
            {
                Cancel();
            }
            _isDisposed = true;
        }

        protected abstract bool CanMainFuncExecute();
        protected abstract Task DoMainFunc();
        protected abstract Task DoCallback();
        protected abstract Task DoWaitRestDelay(long rest);

        protected virtual void DoReportTask()
        {
            Task.Run(async () =>
            {
                var reportWatch = Stopwatch.StartNew();

                if (EstimatedTime == 0)
                {
                    CurrentProgress = 100;
                    return;
                }

                try
                {
                    while (IsCompleted == false
                    && IsCanceled == false
                    && CurrentProgress < 100)
                    {
                        var per = Math.Round
                            ((double)(reportWatch.ElapsedMilliseconds
                            / (double)EstimatedTime), 2);
                        per = per > 1 ? 1 : per;
                        CurrentProgress = per * 100;
                        await DoDelayForReportTask();
                    }
                }
                catch
                {

                }
                finally
                {
                    reportWatch.Stop();
                }
            });
        }

        protected abstract Task DoDelayForReportTask();

        protected void HandleMainTaskException(Task task)
        {
            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                }
                throw task.Exception ?? new AggregateException();
            }
            else if (task.IsCanceled)
            {
                throw new OperationCanceledException("Task was aborted from user!");
            }
        }

        public abstract void Cancel();

        public static string GetCurrentThreadInformation()
        {
            var taskName = "Main Task(Task #" + Task.CurrentId.ToString() + ")";
            String msg = "";
            Thread thread = Thread.CurrentThread;
            msg = String.Format("{0} thread information\n", taskName) +
                  String.Format("   Background: {0}\n", thread.IsBackground) +
                  String.Format("   Thread Pool: {0}\n", thread.IsThreadPoolThread) +
                  String.Format("   Thread ID: {0}\n", thread.ManagedThreadId);
            return msg;
        }
    }
}
