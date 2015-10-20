using System;
using System.Threading.Tasks;

namespace FreedomVoice.iOS.Utilities
{
    /// <summary>
    /// Class containing helper extension methods
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Attaches a continuation on a task on the UI Thread
        /// </summary>
        /// <param name="task"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Task ContinueOnCurrentThread(this Task task, Action<Task> callback)
        {
            return task.ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Attaches a continuation on a task on the UI Thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Task<T> ContinueOnCurrentThread<T>(this Task<T> task, Func<Task<T>, T> callback)
        {
            return task.ContinueWith<T>(callback, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// A quick helper to be able to chain 2 tasks together
        /// </summary>
        public static Task ContinueWith(this Task task, Task continuation)
        {
            return task.ContinueWith(t => continuation).Unwrap();
        }

        /// <summary>
        /// A quick helper to be able to chain 2 tasks together
        /// </summary>
        public static Task<T> ContinueWith<T>(this Task task, Task<T> continuation)
        {
            return task.ContinueWith(t => continuation).Unwrap();
        }
    }
}