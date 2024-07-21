using AgentCommon.AgentPluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.TaskManagement
{
    

    internal class PluginTask : ITask
    { 
        internal string PluginName { get; set; }
        internal PluginOperationType OperationType { get; set; }
        internal AgentPluginArguments ITask.Arguments { get; set; }

        internal PluginTask()
        {

        }

        public TaskType TaskType()
        {
            throw new NotImplementedException();
        }

        TaskType ITask.TaskType()
        {
            throw new NotImplementedException();
        }
    }
}
