using cyber_server_base.async_task.@base;
using cyber_server_base.async_task.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cyber_base.implement.async_task
{
    public class CancelableAsyncTask : BaseAsyncTask
    {
        private Func<CancellationTokenSource, AsyncTaskResult, Task<AsyncTaskResult>> _mainFunc;
        private Func<AsyncTaskResult, Task<AsyncTaskResult>> _callback;
        private Func<bool> _canExecute;

        private CancellationTokenSource _cancellationTokenSource;

        public Func<AsyncTaskResult, Task<AsyncTaskResult>> CallbackHandler => _callback;
        public Func<bool> CanExecute => _canExecute;
        public Func<CancellationTokenSource, AsyncTaskResult, Task<AsyncTaskResult>> MainFunc => _mainFunc;

        public CancelableAsyncTask(
            Func<CancellationTokenSource, AsyncTaskResult, Task<AsyncTaskResult>> mainFunc
            , CancellationTokenSource cancellationTokenSource
            , Func<bool> canExecute = null
            , Func<AsyncTaskResult, Task<AsyncTaskResult>> callback = null
            , string name = ""
            , ulong estimatedTime = 0
            , ulong delayTime = 0
            , int reportDelay = 1000)
            : base(name, estimatedTime, delayTime, reportDelay)
        {
            _mainFunc = mainFunc;
            _canExecute = canExecute;
            _callback = callback;
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected async override Task DoMainFunc()
        {
            await MainFunc.Invoke(_cancellationTokenSource, _result)
                .ContinueWith((task) =>
                {
                    HandleMainTaskException(task);
                });

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException("Task was aborted from user!");
            }
        }

        protected async override Task DoCallback()
        {
            if (CallbackHandler != null)
            {
                await CallbackHandler.Invoke(_result);
            }
        }

        protected override async Task DoWaitRestDelay(long rest)
        {
            await Task.Delay(Convert.ToInt32(rest)
                , _cancellationTokenSource.Token);
        }

        public override void Cancel()
        {
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

        protected async override Task DoDelayForReportTask()
        {
            await Task.Delay(_reportDelay
              , _cancellationTokenSource.Token);
        }

        protected override bool CanMainFuncExecute()
        {
            return CanExecute?.Invoke() ?? true;
        }
    }
}
