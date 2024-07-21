using AgentCore.TaskManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AgentCommon;
using System.Text.Json;

namespace AgentCore.CommunicationManagement
{
    public class ServerResponse
    {
        private string RawResponse { get; set; }
        private ILogger Logger { get; set; }
        private CoreTask Task { get; set; }
        private bool IsAgentTask { get; set; }
        private bool IsValid { get; set; }
        public ServerResponse(string response, ILogger logger)
        {
            Logger = logger;
            RawResponse = response;

            this.DeserializeResponse(RawResponse);
        }

        private void DeserializeResponse(string response)
        {
            JsonDocument jsonObject = null;

            // Check if the response is a valid JSON object
            try
            {
                jsonObject = JsonDocument.Parse(response);
            }
            catch (Exception e)
            {
                Logger.LogError("Invalid response format", e);
                Logger.LogException(e);
                return; // Exit the method if the JSON is invalid
            }

            // Check if the response is a valid server response
            string responseType = string.Empty;
            if (jsonObject.RootElement.TryGetProperty("ResponseType", out JsonElement responseTypeElement))
            {
                responseType = responseTypeElement.GetString();
            }
            else
            {
                Logger.LogError("Invalid response format: ResponseType not found");
                return; // Exit the method if the ResponseType is missing
            }

            // Deserialize as the proper type
            if (jsonObject.RootElement.TryGetProperty("Response", out JsonElement responseBody))
            {
                switch (responseType)
                {
                    case "AgentTask":
                        this.IsAgentTask = this.DeserializeAsAgentTask(responseBody);
                        break;
                    default:
                        Logger.LogWarning("Invalid response type");
                        break;
                }
            }
            else
            {
                Logger.LogError("Invalid response format: Response not found");
            }
        }

        // Example DeserializeAsAgentTask method for context
        private bool DeserializeAsAgentTask(JsonElement responseBody)
        {
            // Deserialize the responseBody to AgentTask
            // Implement the actual deserialization logic here
            string rawTaskBody = responseBody.ToString();
            return true;
        }

        public bool IsValidResponse()
        {
            return this.IsValid;
        }

        public bool IsAgentTaskResponse()
        {
            return this.IsAgentTask;
        }

        public CoreTask GetAgentTask()
        {
            if (this.IsAgentTask)
            {
                return this.Task;
            }
            else
            {
                return null;
            }
        }
    }
}
