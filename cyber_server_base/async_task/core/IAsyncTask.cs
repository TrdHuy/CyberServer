using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server_base.async_task.core
{
    public interface IAsyncTask
    {
        /// <summary>
        /// Tên của Task
        /// </summary
        string Name { get; }

        /// <summary>
        /// Thời gian dự kiến hoàn thành task
        /// </summary
        ulong EstimatedTime { get; }

        /// <summary>
        /// Tiến độ hiện tại
        /// Bằng khoảng thời gian từ lúc bắt đầu thực hiện 
        /// chia phần trăm cho Thời gian dự kiến
        /// </summary>
        double CurrentProgress { get; }

        /// <summary>
        /// Thời gian delay (theo millisecon) khi thực hiện async action
        /// </summary
        ulong DelayTime { get; }

        /// <summary>
        /// Kết quả trả về sau khi triển khai async action
        /// </summary
        AsyncTaskResult Result { get; }

        /// <summary>
        /// Task chính có thực hiện được không
        /// </summary>
        bool IsExecuteable { get; }

        /// <summary>
        /// Task chính đã được thực hiện xong chưa
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Callback đã được thực hiện xong chưa
        /// </summary>
        bool IsCompletedCallback { get; }

        /// <summary>
        /// Kiểm tra Task chính có bị hủy hay không
        /// </summary>
        bool IsCanceled { get; }

        /// <summary>
        /// Kiểm tra Task có đang chạy hay không
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// Kiểm tra Task chính có bị lỗi hay không
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        /// Trigger a notify when IsCompleted changed
        /// </summary>
        event IsCompletedChangedHandler OnCompletedChanged;

        /// <summary>
        /// Trigger a notify when Iscanceld changed
        /// </summary>
        event IsCanceldChangedHandler OnCanceledChanged;

        /// <summary>
        /// Trigger a notify when Iscanceld changed
        /// </summary>
        event IsFaultedChangedHandler OnFaultedChanged;

        /// <summary>
        /// Trigger a notify when IsExecuting changed
        /// </summary>
        event IsExecutingChangedHandler OnExecutingChanged;
    }

    public delegate void IsCompletedChangedHandler(object sender, bool oldValue, bool newValue);
    public delegate void IsCanceldChangedHandler(object sender, bool oldValue, bool newValue);
    public delegate void IsFaultedChangedHandler(object sender, bool oldValue, bool newValue);
    public delegate void IsExecutingChangedHandler(object sender, bool oldValue, bool newValue);
    public delegate void ProgressChangedHandler(object sender, double currentProgress);
}
