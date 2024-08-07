using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AgentCore.EventManagement;
using static AgentCore.EventManagement.EventDispatcher;
using Newtonsoft.Json.Linq;

namespace AgentCore.JobManagement
{
    internal class JobConverter : IJobConverter
    {
        public Job Convert(object jobEventArgs)
        {
            // Try to convert
            EventArgs args = jobEventArgs as EventArgs;

            if (args == null)
            {
                throw new ArgumentException("jobData is not of type EventArgs");
            }

            // Assuming the EventArgs contains a property with the JSON string
            string jsonString = ExtractJsonFromEventArgs(args);

            if (string.IsNullOrEmpty(jsonString))
            {
                throw new ArgumentException("EventArgs does not contain valid JSON data");
            }

            // Deserialize the JSON string to JObjects
            var jsonObject = JObject.Parse(jsonString);

            if (!jsonObject.ContainsKey("jobType") || !jsonObject.ContainsKey("jobData"))
            {
                throw new ArgumentException("JSON data does not contain required fields: jobType and jobData");
            }

            string jobId = jsonObject["jobId"].ToString();
            string jobType = jsonObject["jobType"].ToString();
            JObject jobData = (JObject)jsonObject["jobData"];

            // Create and return the Job object
            Job job = new Job(jobId, jobType, jobData);

            return job;
        }

        private string ExtractJsonFromEventArgs(EventArgs args)
        {
            // Implement logic to extract JSON string from EventArgs
            // This example assumes a custom EventArgs class with a JsonData property
            if (args is JsonEventArgs customArgs)
            {
                return customArgs.JsonData;
            }

            return null;
        }
    }

    
}


