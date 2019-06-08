using System;
namespace FreedomVoice.iOS.Core
{
    public interface ILogger
    {
        void Debug(string fileName, string methodName, string debugInfo);
    }

    internal class Logger : ILogger
    {
        public void Debug(string fileName, string methodName, string debugInfo)
        {
            Console.WriteLine($"{fileName}.{methodName}: {debugInfo}");
        }
    }
}
