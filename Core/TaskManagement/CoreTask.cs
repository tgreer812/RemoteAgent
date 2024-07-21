using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.TaskManagement
{
    public class CoreTask : ITask
    {        
        public CoreTask() 
        {

        }

        public TaskType TaskType()
        {
            return TaskManagement.TaskType.CoreTask;
        }
    }
}
