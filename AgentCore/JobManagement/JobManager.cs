using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.JobManagement
{
    public enum TaskType
    {
        CoreTask,
        PluginTask
    }

    internal class JobManager : IJobManager
    {
        private readonly Queue<Job> _jobs = new Queue<Job>();
        private readonly IJobConverter _jobConverter;
        private ILogger Logger { get; set; }

        internal JobManager(ILogger logger, IJobConverter jobConverter)
        {
            Logger = logger;    
            _jobConverter = jobConverter;
        }

        public void Start()
        {
            Logger.LogInfo("JobManager is starting...");
        }

        public void Stop()
        {
            Logger.LogInfo("JobManager is stopping...");
        }

        internal void AddJob(Job job)
        {
            _jobs.Enqueue(job);
        }

        internal void ProcessJobs()
        {
            while (_jobs.Count > 0)
            {
                var job = _jobs.Dequeue();
                this.HandleJob(job);
            }
        }

        private void HandleJob(Job job)
        {
            switch (job.JobType)
            {
                case JobTypes.CoreJobType:
                    // Handle core task
                    throw new NotImplementedException();
                case JobTypes.PluginJobType:
                    // TODO: parse job data and start plugin
                    Core.PluginManager.StartPlugin();
                    break;
                default:
                    throw new Exception("Unknown task type");
            }
        }
    }
}

    
