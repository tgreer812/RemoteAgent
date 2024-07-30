using System;
using System.Collections.Generic;
using System.Text.Json;
using AgentCore.EventManagement;
using static AgentCore.EventManagement.EventDispatcher;

namespace AgentCore.JobManagement
{
    internal class JobConverter : IJobConverter
    {
        public Job Convert(object jobData)
        {
            // Try to convert
            EventArgs args = jobData as EventArgs;

            if (args == null)
            {
                throw new ArgumentException("jobData is not of type EventArgs");
            }

            // Assuming the EventArgs contains a property with the JSON string
            string json = ExtractJsonFromEventArgs(args);

            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("EventArgs does not contain valid JSON data");
            }

            // Deserialize the JSON string to a dictionary
            Dictionary<string, object> jobDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (!jobDict.ContainsKey("jobType") || !jobDict.ContainsKey("jobData"))
            {
                throw new ArgumentException("JSON data does not contain required fields: jobType and jobData");
            }

            string jobType = jobDict["jobType"].ToString();
            Dictionary<string, object> jobDataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jobDict["jobData"].ToString());

            // Create and return the Job object
            Job job = new Job(jobType, jobDataDict);

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


