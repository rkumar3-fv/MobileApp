using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class ExceptionSupport
    {
        public static void LogAndIgnore(Task task)
        {}

        public static void ReportAndExit(Task task)
        {
            UncaughtTaskExceptionHandler(task.Exception);
            Process.GetCurrentProcess().Kill();
        }
        public static Action<object> UncaughtTaskExceptionHandler;
        public static Task HandleExceptions(this Task task)
        {
            return task.HandleExceptions(ReportAndExit);
        }
        public static Task HandleExceptions(this Task task, Action<Task> handler)
        {
            return task.ContinueWith(handler,
                                TaskContinuationOptions.OnlyOnFaulted |
                                TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}