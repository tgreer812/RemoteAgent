using AgentCommon;
using AgentCore.EventManagement;
using AgentCore.PluginManagement;
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
        private readonly IJobConverter _jobConverter;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IPluginManager _pluginManager;
        private ILogger Logger { get; set; }

        public bool IsRunning { get; set; }
        
        internal JobManager(ILogger logger) : this(logger, null, null)
        {
        }

        internal JobManager(ILogger logger, IEventDispatcher eventDispatcher) : this(logger, eventDispatcher, null)
        {
        }

        internal JobManager(ILogger logger, IEventDispatcher eventDispatcher, IPluginManager pluginManager)
        {
            Logger = logger;    
            _jobConverter = new JobConverter();
            _eventDispatcher = eventDispatcher;
            _pluginManager = pluginManager;
        }

        private async Task OnJobAddedAsync(object sender, EventArgs e)
        {
            if (sender == null || e == null) 
            { 
                Logger.LogError("OnJobAddedAsync called with null parameters");
                return; 
            }

            try
            {
                Logger.LogDebug("Job added event received");
                Job job = _jobConverter.Convert(e);

                Logger.LogDebug($"Processing job {job.JobId} of type {job.JobType}");
                await ProcessJobAsync(job);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error processing job from event", ex);
            }
        }

        public Task Start()
        {
            if (IsRunning) 
            { 
                Logger.LogError("JobManager is already running!"); 
                return Task.CompletedTask; 
            }
            
            Logger.LogInfo("JobManager is starting...");

            // Subscribe to job events using async handler
            _eventDispatcher?.Subscribe("JobAdded", OnJobAddedAsync);

            IsRunning = true;
            Logger.LogInfo("JobManager started and listening for job events.");
            return Task.CompletedTask;
        }

        public Task<bool> Stop()
        {
            Logger.LogInfo("JobManager is stopping...");
            
            if (IsRunning)
            {
                // Unsubscribe from events
                _eventDispatcher?.Unsubscribe("JobAdded", OnJobAddedAsync);
                IsRunning = false;
            }

            return Task.FromResult(true);
        }

        private async Task ProcessJobAsync(Job job)
        {
            try
            {
                Logger.LogDebug($"Processing job {job.JobId} of type {job.JobType}");
                
                switch (job.JobType)
                {
                    case JobTypes.CoreJobType:
                        // Handle core task
                        await ProcessCoreJobAsync(job);
                        break;
                    case JobTypes.PluginJobType:
                        await ProcessPluginJobAsync(job);
                        break;
                    default:
                        throw new Exception($"Unknown job type: {job.JobType}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error processing job {job.JobId}", ex);
            }
        }

        private async Task ProcessCoreJobAsync(Job job)
        {
            Logger.LogInfo($"Processing core job {job.JobId}");
            // TODO: Implement core job processing
            await Task.CompletedTask;
            throw new NotImplementedException("Core job processing not implemented");
        }

        private async Task ProcessPluginJobAsync(Job job)
        {
            Logger.LogInfo($"Processing plugin job {job.JobId}");
            if (_pluginManager != null)
            {
                await _pluginManager.StartPluginAsync(job.JobId, job.JobData);
            }
            else
            {
                Logger.LogError("PluginManager not available to process plugin job");
            }
        }
    }
}

    
