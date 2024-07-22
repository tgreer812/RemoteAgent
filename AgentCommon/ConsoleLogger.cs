using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void LogInfo(string message)
        {
            Log($"INFO: {message}");
        }

        public void LogError(string message, Exception exception = null)
        {
            Log($"ERROR: {message}");
            Log(exception.ToString());
            /*if (exception != null)
            {
                Log(exception.Message);
                if (exception.StackTrace != null)
                {
                    exception.StackTrace.Split('\n').ToList().ForEach(Log);
                }
            }*/
        }

        public void LogException(Exception exception)
        {
            exception.StackTrace.Split('\n').ToList().ForEach(Log);
        }

        public void LogWarning(string message)
        {
            Log($"WARNING: {message}");
        }

        public void LogDebug(string message)
        {
            #if DEBUG
                Log($"DEBUG: {message}");
            #endif
        }
    }
}
