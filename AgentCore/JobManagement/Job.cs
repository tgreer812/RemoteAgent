using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.JobManagement
{
    public class Job
    {
        public string JobType { get; set; }
        public Dictionary<string, object> JobData { get; set; }

        public Job(string jobType, Dictionary<string, object> jobData)
        {
            JobType = jobType;
            JobData = jobData;
        }
        
    }
}
