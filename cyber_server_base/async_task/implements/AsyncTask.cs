using cyber_server_base.async_task.@base;
using cyber_server_base.async_task.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server_base.async_task.implements
{
    public class AsyncTask : BaseAsyncTask
    {
        private Func<AsyncTaskResult, Task<AsyncTaskResult>> _mainFunc;
        private Func<AsyncTaskResult, Task<AsyncTaskResult>> _callback;
        private Func<bool> _canExecute;

        public Func<AsyncTaskResult, Task<AsyncTaskResult>> CallbackHandler => _callback;
        public Func<bool> CanExecute => _canExecute;
        public Func<AsyncTaskResult, Task<AsyncTaskResult>> MainFunc => _mainFunc;

        public AsyncTask(
            Func<AsyncTaskResult, Task<AsyncTaskResult>> mainFunc
            , Func<bool> canExecute = null
            , Func<AsyncTaskResult, Task<AsyncTaskResult>> callback = null
            , string name = ""
            , ulong estimatedTime = 0
            , ulong delayTime = 0
            , int reportDelay = 1000) : base(name, estimatedTime, delayTime, reportDelay)
        {
            _mainFunc = mainFunc;
            _canExecute = canExecute;
            _callback = callback;
        }

        protected async override Task DoMainFunc()
        {

            await MainFunc.Invoke(_result)
                .ContinueWith((task) =>
                {
                    HandleMainTaskException(task);
                });
        }

        protected async override Task DoCallback()
        {
            if (CallbackHandler != null)
            {
                await CallbackHandler.Invoke(_result);
            }
        }

        protected async override Task DoWaitRestDelay(long rest)
        {
            await Task.Delay(0);
        }

        public override void Cancel()
        {
        }

        protected async override Task DoDelayForReportTask()
        {
            await Task.Delay(_reportDelay);
        }

        protected override bool CanMainFuncExecute()
        {
            return CanExecute?.Invoke() ?? true;
        }
    }
}
