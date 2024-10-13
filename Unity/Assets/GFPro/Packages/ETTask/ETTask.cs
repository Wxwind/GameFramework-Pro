using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace GFPro
{
    [AsyncMethodBuilder(typeof(ETAsyncTaskMethodBuilder))]
    public class ETTask : ICriticalNotifyCompletion
    {
        public static Action<Exception> ExceptionHandler;

        public static ETTaskCompleted CompletedTask => new();

        private static readonly ConcurrentQueue<ETTask> queue = new();

        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        public static ETTask Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask();
            }

            if (!queue.TryDequeue(out var task))
            {
                return new ETTask() { fromPool = true };
            }

            return task;
        }

        private void Recycle()
        {
            if (!fromPool)
            {
                return;
            }

            state = AwaiterStatus.Pending;
            callback = null;
            // 太多了
            if (queue.Count > 1000)
            {
                return;
            }

            queue.Enqueue(this);
        }

        private bool          fromPool;
        private AwaiterStatus state;
        private object        callback; // Action or ExceptionDispatchInfo

        private ETTask()
        {
        }

        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [DebuggerHidden]
        public ETTask GetAwaiter()
        {
            return this;
        }


        public bool IsCompleted
        {
            [DebuggerHidden] get => state != AwaiterStatus.Pending;
        }

        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (state != AwaiterStatus.Pending)
            {
                action?.Invoke();
                return;
            }

            callback = action;
        }

        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            UnsafeOnCompleted(action);
        }

        [DebuggerHidden]
        public void GetResult()
        {
            switch (state)
            {
                case AwaiterStatus.Succeeded:
                    Recycle();
                    break;
                case AwaiterStatus.Faulted:
                    var c = callback as ExceptionDispatchInfo;
                    callback = null;
                    Recycle();
                    c?.Throw();
                    break;
                default:
                    throw new NotSupportedException("ETTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        [DebuggerHidden]
        public void SetResult()
        {
            if (state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            state = AwaiterStatus.Succeeded;

            var c = callback as Action;
            callback = null;
            c?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            state = AwaiterStatus.Faulted;

            var c = callback as Action;
            callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }

    [AsyncMethodBuilder(typeof(ETAsyncTaskMethodBuilder<>))]
    public class ETTask<T> : ICriticalNotifyCompletion
    {
        private static readonly ConcurrentQueue<ETTask<T>> queue = new();

        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        public static ETTask<T> Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask<T>();
            }

            if (!queue.TryDequeue(out var task))
            {
                return new ETTask<T>() { fromPool = true };
            }

            return task;
        }

        private void Recycle()
        {
            if (!fromPool)
            {
                return;
            }

            callback = null;
            value = default;
            state = AwaiterStatus.Pending;
            // 太多了
            if (queue.Count > 1000)
            {
                return;
            }

            queue.Enqueue(this);
        }

        private bool          fromPool;
        private AwaiterStatus state;
        private T             value;
        private object        callback; // Action or ExceptionDispatchInfo

        private ETTask()
        {
        }

        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [DebuggerHidden]
        public ETTask<T> GetAwaiter()
        {
            return this;
        }

        [DebuggerHidden]
        public T GetResult()
        {
            switch (state)
            {
                case AwaiterStatus.Succeeded:
                    var v = value;
                    Recycle();
                    return v;
                case AwaiterStatus.Faulted:
                    var c = callback as ExceptionDispatchInfo;
                    callback = null;
                    Recycle();
                    c?.Throw();
                    return default;
                default:
                    throw new NotSupportedException("ETask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }


        public bool IsCompleted
        {
            [DebuggerHidden] get => state != AwaiterStatus.Pending;
        }

        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (state != AwaiterStatus.Pending)
            {
                action?.Invoke();
                return;
            }

            callback = action;
        }

        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            UnsafeOnCompleted(action);
        }

        [DebuggerHidden]
        public void SetResult(T result)
        {
            if (state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            state = AwaiterStatus.Succeeded;

            value = result;

            var c = callback as Action;
            callback = null;
            c?.Invoke();
        }

        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            state = AwaiterStatus.Faulted;

            var c = callback as Action;
            callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }
}