using cyber_server_base.async_task.@base;
using cyber_server_base.async_task.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cyber_base.implement.async_task
{
    public enum MultiAsyncTaskReportType
    {
        EstimatedTime = 0,
        SubTasks = 1,
    }

    public class MultiAsyncTask : BaseAsyncTask
    {
        private List<BaseAsyncTask> _mainFuncs;
        private CancellationTokenSource _cancellationTokenSource;
        private List<AsyncTaskResult> _results;
        private Func<List<AsyncTaskResult>, AsyncTaskResult, Task<AsyncTaskResult>> _callback;
        private BaseAsyncTask _currentExecuteTask;
        private MultiAsyncTaskReportType _rpType;

        protected BaseAsyncTask CurrentExecuteTask
        {
            get
            {
                return _currentExecuteTask;
            }
            set
            {
                var oldTask = _currentExecuteTask;
                _currentExecuteTask = value;
                if (oldTask != value)
                {
                    CurrentTaskChanged?.Invoke(this, oldTask, value);
                }
            }
        }
        public Func<List<AsyncTaskResult>, AsyncTaskResult, Task<AsyncTaskResult>> CallbackHandler => _callback;
        public new List<AsyncTaskResult> Result => _results;
        public event CurrentTaskChangedHandler CurrentTaskChanged;

        public MultiAsyncTask(
            List<BaseAsyncTask> mainFunc
            , CancellationTokenSource cancellationTokenSource
            , Func<List<AsyncTaskResult>, AsyncTaskResult, Task<AsyncTaskResult>> callback = null
            , string name = ""
            , ulong delayTime = 0
            , int reportDelay = 1000
            , MultiAsyncTaskReportType reportType = MultiAsyncTaskReportType.EstimatedTime)
            : base(name, 0, delayTime, reportDelay)
        {
            _estimatedTime = 0;
            foreach (var ele in mainFunc)
            {
                _estimatedTime += ele.EstimatedTime;
            }
            _mainFuncs = mainFunc;
            _cancellationTokenSource = cancellationTokenSource;
            _results = new List<AsyncTaskResult>();
            _callback = callback;
            _rpType = reportType;
        }

        public int TaskCount => _mainFuncs.Count;

        public override void Cancel()
        {
            if (CurrentExecuteTask != null
                && CurrentExecuteTask.IsCompleted == false
                && CurrentExecuteTask.IsCanceled == false)
            {
                CurrentExecuteTask.Cancel();
            }
            try
            {
                lock (_cancellationTokenSource)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected async override Task DoCallback()
        {
            if (CallbackHandler != null)
            {
                await CallbackHandler.Invoke(_results, _result);
            }
        }

        protected async override Task DoDelayForReportTask()
        {
            await Task.Delay(_reportDelay
            , _cancellationTokenSource.Token);
        }

        protected async override Task DoMainFunc()
        {
            int countTaskFinished = 0;
            foreach (var ele in _mainFuncs)
            {
                // Cập nhật cờ executeable trước khi thay đổi current execute task
                ele.CanThisTaskExecuteable();

                CurrentExecuteTask = ele;
                await ele.Execute()
                    .ContinueWith((task) =>
                {
                    if (task.IsFaulted)
                    {
                    }
                    else if (task.Result.IsCanceled)
                    {
                    }
                });

                _results.Add(ele.Result);

                if (_rpType == MultiAsyncTaskReportType.SubTasks)
                {
                    CurrentProgress = Math.Round((double)countTaskFinished
                        / (double)_mainFuncs.Count, 2) * 100;
                }

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Task was aborted by user!");
                }
            }
        }

        protected override void DoReportTask()
        {
            if (_rpType == MultiAsyncTaskReportType.EstimatedTime)
            {
                base.DoReportTask();
            }
        }

        protected async override Task DoWaitRestDelay(long rest)
        {
            await Task.Delay(Convert.ToInt32(rest)
                , _cancellationTokenSource.Token);
        }

        protected override bool CanMainFuncExecute()
        {
            return true;
        }
    }

    public delegate void CurrentTaskChangedHandler(object sender, BaseAsyncTask oldTask, BaseAsyncTask newTask);
}
