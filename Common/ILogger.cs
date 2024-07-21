using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon
{
    public interface ILogger
    {
        void Log(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception=null);
        void LogException(Exception exception);
        void LogDebug(string message);
    }
}
