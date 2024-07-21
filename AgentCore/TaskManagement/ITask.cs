using AgentCommon.AgentPluginCommon;
using AgentCore.TaskManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.TaskManagement
{
    internal enum PluginOperationType
    {
        Start,
        Stop,
        Execute,
        Unload
    }

    internal interface ITask
    {
        TaskType TaskType();
        AgentPluginArguments Arguments { get; set; }
    }

    internal static class TaskManagementHelper
    {
        // helper method to get enum PluginOperationType from string
        internal static PluginOperationType GetPluginOperationType(string operationType)
        {
            PluginOperationType result;
            if (Enum.TryParse(operationType, out result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException("Invalid operation type");
            }
        }
    }
}
