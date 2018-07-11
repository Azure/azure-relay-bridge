// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Threading.Tasks;

    static class TaskEx
    {
        // Task.Delay is a little trick to get a completed task
        public static readonly Task CompletedTask = Task.Delay(0);

        public static void Fork(this Task thisTask, object source)
        {
            Fx.Assert(thisTask != null, "task is required!");
            thisTask.ContinueWith((t, s) => BridgeEventSource.Log.HandledExceptionAsError(s, t.Exception), source, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void Fork(this Task thisTask)
        {
            Fx.Assert(thisTask != null, "task is required!");
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="state"> 
        /// This parameter helps reduce allocations by passing state to the Funcs. e.g.:
        ///  await TaskEx.FromAsync(
        ///      (c, s) => ((Transaction)s).BeginCommit(c, s),
        ///      (a) => ((Transaction)a.AsyncState).EndCommit(a),
        ///      transaction);
        /// </param>
        public static Task FromAsync(Func<AsyncCallback, object, IAsyncResult> begin, Action<IAsyncResult> end, object state = null)
        {
            try
            {
                return Task.Factory.FromAsync(begin, end, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException(ex);
            }
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        public static Task FromAsync<TArg1>(
            Func<TArg1, AsyncCallback, object, IAsyncResult> begin,
            Action<IAsyncResult> end,
            TArg1 arg1,
            object state)
        {
            try
            {
                return Task.Factory.FromAsync(begin, end, arg1, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException(ex);
            }
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        public static Task FromAsync<TArg1, TArg2>(
            Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> begin,
            Action<IAsyncResult> end,
            TArg1 arg1,
            TArg2 arg2,
            object state)
        {
            try
            {
                return Task.Factory.FromAsync(begin, end, arg1, arg2, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException(ex);
            }
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        public static Task FromAsync<TArg1, TArg2, TArg3>(
            Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> begin,
            Action<IAsyncResult> end,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            object state)
        {
            try
            {
                return Task.Factory.FromAsync(begin, end, arg1, arg2, arg3, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException(ex);
            }
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        public static Task<TResult> FromAsync<TResult>(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, TResult> end, object state = null)
        {
            try
            {
                return Task<TResult>.Factory.FromAsync(begin, end, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException<TResult>(ex);
            }
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        public static Task<TResult> FromAsync<TArg1, TResult>(
            Func<TArg1, AsyncCallback, object, IAsyncResult> begin,
            Func<IAsyncResult, TResult> end,
            TArg1 arg1,
            object state)
        {
            try
            {
                return Task<TResult>.Factory.FromAsync(begin, end, arg1, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException<TResult>(ex);
            }
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        public static Task<TResult> FromAsync<TArg1, TArg2, TResult>(
            Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> begin,
            Func<IAsyncResult, TResult> end,
            TArg1 arg1,
            TArg2 arg2,
            object state)
        {
            try
            {
                return Task<TResult>.Factory.FromAsync(begin, end, arg1, arg2, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException<TResult>(ex);
            }
        }

        /// <summary>
        /// Create a Task based on Begin/End IAsyncResult pattern.
        /// This is a wrapper around Task.Factory.FromAsync with the additional guarantee that if the begin delegate
        /// throws an Exception it will result in a Task with that exception instead of the exception being thrown from this method.
        /// </summary>
        public static Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult>(
            Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> begin,
            Func<IAsyncResult, TResult> end,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            object state)
        {
            try
            {
                return Task<TResult>.Factory.FromAsync(begin, end, arg1, arg2, arg3, state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                return FromException<TResult>(ex);
            }
        }

        /// <summary>
        /// Creates a Task that has completed with a specified exception.
        /// Once the code has moved to .NET 4.6 just use Task.FromException;
        /// </summary>
        public static Task FromException(Exception exception)
        {
            return FromException<object>(exception);
        }

        /// <summary>
        /// Creates a Task&lt;TResult&gt; that's completed with a specified exception.
        /// Once the code has moved to .NET 4.6 just use Task.FromException&lt;TResult&gt;
        /// </summary>
        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            if (exception == null)
            {
                throw BridgeEventSource.Log.ArgumentNull(nameof(exception));
            }

            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetException(exception);
            return completionSource.Task;
        }
        
        public static IAsyncResult ToAsyncResult(this Task task, AsyncCallback callback, object state, bool executeSynchronously = false)
        {
            // Tasks ALWAYS report IAsyncResult.CompletedSynchronously = false.  This can lead to StackOverflow problems
            // when interoping with normal IAsyncResult patterns because if IAsyncResult.CompletedSynchronously == false then
            // it's supposed to be 'safe' to invoke continuations from the AsyncCallback.  This isn't necessarily true
            // with Tasks, so in order to break the stack overflow chain don't pass the TaskContinuationOptions.ExecuteSynchronously
            // flag.  However, this comes with a performance hit.  If we have a task that is not completed, it's safe to use 
            // the ExecuteSynchronously flag since we know the task had to complete asynchronously (as opposed to lying to us).
            var continuationOptions = task.IsCompleted || !executeSynchronously ? TaskContinuationOptions.None : TaskContinuationOptions.ExecuteSynchronously;

            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith(
                        t => callback(task),
                        continuationOptions);
                }

                return task;
            }

            var tcs = new TaskCompletionSource<object>(state);
            task.ContinueWith(
                (t, s) =>
                {
                    var tcsPtr = (TaskCompletionSource<object>)s;
                    if (t.IsFaulted)
                    {
                        tcsPtr.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcsPtr.TrySetCanceled();
                    }
                    else
                    {
                        tcsPtr.TrySetResult(null);
                    }

                    callback?.Invoke(tcsPtr.Task);
                },
                tcs,
                continuationOptions);

            return tcs.Task;
        }

        public static IAsyncResult ToAsyncResult<TResult>(this Task<TResult> task, AsyncCallback callback, object state, bool executeSynchronously = false)
        {
            // Tasks ALWAYS report IAsyncResult.CompletedSynchronously = false.  This can lead to StackOverflow problems
            // when interoping with normal IAsyncResult patterns because if IAsyncResult.CompletedSynchronously == false then
            // it's supposed to be 'safe' to invoke continuations from the AsyncCallback.  This isn't necessarily true
            // with Tasks, so in order to break the stack overflow chain don't pass the TaskContinuationOptions.ExecuteSynchronously
            // flag.  However, this comes with a performance hit.  If we have a task that is not completed, it's safe to use 
            // the ExecuteSynchronously flag since we know the task had to complete asynchronously (as opposed to lying to us).
            var continuationOptions = task.IsCompleted || !executeSynchronously ? TaskContinuationOptions.None : TaskContinuationOptions.ExecuteSynchronously;

            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith(
                        t => callback(task),
                        continuationOptions);
                }

                return task;
            }

            var tcs = new TaskCompletionSource<TResult>(state);
            task.ContinueWith(
                (t, s) =>
                {
                    var tcsPtr = (TaskCompletionSource<TResult>)s;
                    if (t.IsFaulted)
                    {
                        tcsPtr.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcsPtr.TrySetCanceled();
                    }
                    else
                    {
                        tcsPtr.TrySetResult(t.Result);
                    }

                    callback?.Invoke(tcsPtr.Task);
                },
                tcs,
                continuationOptions);

            return tcs.Task;
        }
    }
}
