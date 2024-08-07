using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.JobManagement
{
    public class Job
    {
        public string JobId { get; set; }
        public string JobType { get; set; }
        public JObject JobData { get; set; }

        public Job(string jobId, string jobType, JObject jobData)
        {
            JobId = jobId;
            JobType = jobType;
            JobData = jobData;
        }
        
    }
}
