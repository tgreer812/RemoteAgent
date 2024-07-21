using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.TaskManagement
{
    public enum TaskType
    {
        CoreTask,
        PluginTask
    }

    public class TaskManager
    {
        public ILogger Logger { get; set; }
        public Queue<CoreTask> AgentTasks { get; set; }

        public TaskManager(ILogger logger)
        {
            Logger = logger;
            AgentTasks = new Queue<CoreTask>();
        }

        public void AddTask(CoreTask task)
        {
            AgentTasks.Enqueue(task);
        }

        public void HandleAllTasks()
        {
            while (AgentTasks.Count > 0)
            {
                CoreTask task = AgentTasks.Dequeue();
                this.HandleTask(task);
            }
        }

        private void HandleTask(ITask task)
        {
            switch (task.TaskType())
            {
                case TaskType.CoreTask:
                    // Handle core task
                    HandleCoreTask((CoreTask)task);
                    break;
                case TaskType.PluginTask:
                    HandlePluginTask((PluginTask)task);
                    break;
                default:
                    throw new Exception("Unknown task type");
            }
        }

        private void HandleCoreTask(CoreTask task)
        {
            throw new NotImplementedException();
        }

        private void HandlePluginTask(PluginTask task)
        {
            Core.PluginManager.HandlePluginTask(task);
        }
    }
}

    
