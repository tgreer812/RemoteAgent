using AgentCommon;
using AgentCore.PluginManagement;
using AgentCommon.AgentPluginCommon;

//using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static AgentCore.EventManagement.EventDispatcher;

namespace AgentCore.CommunicationManagement
{
    public class CommunicationManager : ICoreService, ICommunicationManager
    {
        private HttpClient Client { get; set; }
        internal bool IsRunning { get; private set; }
        private int PollingInterval { get; set; }
        private ILogger Logger { get; set; }
        private string ServerAddress { get; set; }
        private const string TASKING_ENDPOINT = "api/tasking";
        private const string JOB_ENDPOINT = "api/job";
        private const string AGENT_HELLO_ENDPOINT = "api/agents/hello";
        internal CommunicationManager(ILogger logger)
        {
            // TODO: Refactor this so that endpoints, ports, etc. are read from configuration/constants files
            Logger = logger;
            IsRunning = false;
            PollingInterval = 4000; // 4 seconds
            ServerAddress = "http://localhost:5148";
            this.Client = new HttpClient();
        }

        private async Task PollForTasking()
        {
            await Task.Delay(PollingInterval);

            // Send a request to the server for tasking
            string response = null;
            await this.Client.GetStringAsync($"{this.ServerAddress}/{TASKING_ENDPOINT}/{Core.Instance.Config.AgentId}").ContinueWith((task) =>
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

        private async Task<bool> SendHelloMessage()
        {
            Logger.LogInfo("Sending agent hello message to server...");

            var helloPayload = new
            {
                
                agentGuid = Core.Instance.Config.AgentGuid // Include AgentGuid from config
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(helloPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await Client.PostAsync($"{ServerAddress}/{AGENT_HELLO_ENDPOINT}", content);
            }
            catch (HttpRequestException)
            {
                //Logger.LogError($"Error sending hello message: HTTP Request Error - {ex.Message}");
                return false; // Handshake failed due to HTTP error
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error sending hello message: Unexpected Error - {ex.Message}");
                return false; // Handshake failed due to unexpected error
            }

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var helloResponseJson = JsonDocument.Parse(responseContent);
                    JsonElement root = helloResponseJson.RootElement;

                    if (root.TryGetProperty("agentId", out JsonElement agentIdElement) && agentIdElement.TryGetInt32(out int agentId))
                    {
                        Core.Instance.Config.AgentId = agentId; // Store the agentId received from the server
                        Logger.LogInfo($"Agent handshake successful. Assigned AgentId: {agentId}");
                        return true; // Handshake successful
                    }
                    else
                    {
                        Logger.LogError("Error during hello handshake: Server response did not contain 'agentId' or it was not a valid integer.");
                        Logger.LogDebug($"Server Response Content: {responseContent}"); // Log response for debugging
                        return false; // Handshake failed - missing agentId
                    }
                }
                catch (JsonException jEx)
                {
                    Logger.LogError($"Error parsing hello response JSON: {jEx.Message}");
                    Logger.LogDebug($"Server Response Content: {responseContent}"); // Log response for debugging
                    return false; // Handshake failed - invalid JSON response
                }
            }
            else
            {
                Logger.LogError($"Error during hello handshake: HTTP Status Code - {response.StatusCode}");
                string errorContent = await response.Content.ReadAsStringAsync();
                Logger.LogDebug($"Server Error Response Content: {errorContent}"); // Log error response for debugging
                return false; // Handshake failed - HTTP error status
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

            bool handshakeSuccessful = false;
            int retryCount = 0;
            IsRunning = true;

            while (!handshakeSuccessful && IsRunning) // Loop while handshake fails AND the manager should be running
            {
                retryCount++;
                Logger.LogInfo($"Attempting Hello handshake (Attempt #{retryCount})...");

                handshakeSuccessful = await SendHelloMessage();

                if (handshakeSuccessful)
                {
                    Logger.LogInfo("Hello handshake succeeded after retry.");
                    break; // Exit the retry loop if handshake is successful
                }
                else
                {
                    Logger.LogWarning($"Hello handshake failed (Attempt #{retryCount}). Retrying in 30 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(30)); // Wait 30 seconds before retrying
                }
            }

            if (!handshakeSuccessful)
            {
                Logger.LogError("Communication manager failed to start - Hello handshake unsuccessful after multiple retries.");
                return; // Stop starting if handshake ultimately fails after retries
            }

            // Subscribe to PluginCompleted event to send results back to server
            Core.GetEventDispatcher().Subscribe("PluginCompleted", OnPluginCompleted);

            while (IsRunning)
            {
                Logger.LogDebug("Communication manager is running");
                await PollForTasking();
            }

            Logger.LogInfo("Communication manager stopped."); // Add log for when the manager stops running (if it ever gets out of the while loop in normal operation)
        }

        private void OnPluginCompleted(object sender, EventArgs e)
        {
            try
            {
                PluginResult pluginResult = ((PluginCompletedEventArgs)e).Result;

                this.Client.PutAsync($"{this.ServerAddress}/{TASKING_ENDPOINT}/{Core.Instance.Config.AgentId}", new StringContent(JsonSerializer.Serialize(pluginResult), Encoding.UTF8, "application/json"))
                    .ContinueWith((task) =>
                    {
                        if (task.IsFaulted)
                        {
                            Logger.LogError("Error sending plugin result: " + task.Exception.Message);
                        }
                    }); 
            }
            catch (InvalidCastException ex)
            {
                Logger.LogError($"Error casting EventArgs to PluginCompletedEventArgs: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error handling PluginCompleted event: {ex.Message}");
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
