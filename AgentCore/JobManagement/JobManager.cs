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
            Logger.LogDebug("Job added event received");
            Job job = _jobConverter.Convert(e);
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
                if ((i % 5 == 0) && i != 0)
                {
                    Logger.LogInfo("Adding mock job");

                    Dictionary<string, object> jobData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    jobData.Add("pluginName", "DirectoryListPlugin");
                    jobData.Add("pluginArguments", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "path", "C:\\" } });
                    
                    Job job = new Job("PluginJob",jobData);
                    this.AddJob(job);
                    
                }
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
            while (_jobs.Count > 0)
            {
                var job = _jobs.Dequeue();
                this.HandleJob(job);
            }
        }

        /*
        {
        "jobType": "PluginJob",
        "jobData": {
            "pluginName": "DirectoryListPlugin",
            "arguments": {
                "path": "C:\\"
            }
        }
         
         */
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

    
