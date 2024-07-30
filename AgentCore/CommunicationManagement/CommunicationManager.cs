using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AgentCore.JobManagement;
using static AgentCore.EventManagement.EventDispatcher;

namespace AgentCore.CommunicationManagement
{
    public class CommunicationManager : ICoreService, ICommunicationManager
    {
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
        }

        private async Task PollForTasking()
        {
            await Task.Delay(PollingInterval);

            // Send a request to the server for tasking
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(this.ServerAddress);

            string response = null;
            await client.GetStringAsync(this.ServerAddress + TASKING_ENDPOINT).ContinueWith((task) =>
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

            // Convert response to an EventArgs object
            // and publish an event
            JsonEventArgs args = new JsonEventArgs(response);
            Core.GetEventDispatcher().Publish("JobAdded", this, args);
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
