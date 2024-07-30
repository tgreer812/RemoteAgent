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

    internal class JobManager : ICoreService, IJobManager
    {
        private readonly Queue<Job> _jobs = new Queue<Job>();
        private readonly IJobConverter _jobConverter;
        private ILogger Logger { get; set; }

        public bool IsRunning { get; set; }
        internal JobManager(ILogger logger)
        {
            Logger = logger;    
            _jobConverter = new JobConverter();
        }

        private void OnJobAdded(object sender, EventArgs e)
        {
            if (sender == null || e == null) { throw new ArgumentNullException(); }

            Logger.LogDebug("Job added event received");
            Job job = _jobConverter.Convert(e);

            Logger.LogDebug("Adding job to queue...");
            AddJob(job);
        }

        public async Task Start()
        {
            if (IsRunning) { Logger.LogError("JobManager is already running!"); return; }
            
            Logger.LogInfo("JobManager is starting...");

            // Subscribe to job events
            Core.GetEventDispatcher().Subscribe("JobAdded", OnJobAdded);

            int i = 0;

            IsRunning = true;
            while (IsRunning)
            {
                Logger.LogDebug("JobManager is running...");
                await Task.Delay(1000);
                this.ProcessJobs();
                i++;
            }
        }

        public Task<bool> Stop()
        {
            Logger.LogInfo("JobManager is stopping...");
            IsRunning = false;

            Logger.LogWarning("Clearing job queue. Unhandled jobs will be lost.");
            _jobs.Clear();

            return Task.FromResult(true);
        }

        internal void AddJob(Job job)
        {
            _jobs.Enqueue(job);
        }

        internal void ProcessJobs()
        {
            Logger.LogDebug("Processing jobs...");
            Logger.LogDebug($"Jobs in queue: {_jobs.Count}");
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
                    Core.Instance.PluginManager.StartPlugin(job.JobData);
                    break;
                default:
                    throw new Exception("Unknown task type");
            }
        }
    }
}

    
