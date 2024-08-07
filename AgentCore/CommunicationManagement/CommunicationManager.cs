using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AgentCore.JobManagement;
using static AgentCore.EventManagement.EventDispatcher;
//using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace AgentCore.CommunicationManagement
{
    public class CommunicationManager : ICoreService, ICommunicationManager
    {
        private HttpClient Client { get; set; }
        internal bool IsRunning { get; private set; }
        private int PollingInterval { get; set; }
        private ILogger Logger { get; set; }
        private string ServerAddress { get; set; }
        private const string TASKING_ENDPOINT = "/api/tasking";
        internal CommunicationManager(ILogger logger)
        {
            Logger = logger;
            IsRunning = false;
            PollingInterval = 10000; // 10 seconds
            ServerAddress = "http://localhost:5000";
            this.Client = new HttpClient();
        }

        internal CommunicationManager(ILogger logger, HttpClient client)
        {
            Logger = logger;
            IsRunning = false;
            PollingInterval = 4000; // 10 seconds
            ServerAddress = "http://localhost:5000";
            this.Client = client;
            this.Client.BaseAddress = new Uri(this.ServerAddress);
        }

        private async Task PollForTasking()
        {
            await Task.Delay(PollingInterval);

            // Send a request to the server for tasking
            string response = null;
            await this.Client.GetStringAsync(this.ServerAddress + TASKING_ENDPOINT).ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    Logger.LogError("Error polling for tasking: " + task.Exception.Message);
                    return;
                }

                response = task.Result;
            });

            if (response == null)
            {
                Logger.LogDebug("No tasking response received");
                return;
            }

            // Response should be a JSON string with a key 'jobs' and value that is an array of jobs
            // so parse the JSON string and convert it to a list of JsonEventArgs objects
            // then publish an event for each JsonEventArgs object
            var json = JObject.Parse(response);
            var jobs = json["jobs"];

            foreach (var job in jobs)
            {
                JsonEventArgs args = new JsonEventArgs(job.ToString());
                Core.GetEventDispatcher().Publish("JobAdded", this, args);
            }
        }

        public async Task Start()
        {
            if (IsRunning)
            {
                Logger.LogWarning("Communication manager is already running");
                return;
            }

            Logger.LogInfo("Communication manager is starting...");
            IsRunning = true;
            while (IsRunning)
            {
                Logger.LogDebug("Communication manager is running");
                await PollForTasking();

                // TODO: delete this
                //Core.GetEventDispatcher().Publish("JobAdded", this, new EventArgs());
            }
        }

        public async Task<bool> Stop()
        {
            IsRunning = false;
            Logger.LogInfo("Communication manager is stopping...");
            return await Task.FromResult(true);
        }
    }
}
