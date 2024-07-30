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
        public int StatusCode { get; private set; }
        public string Content { get; private set; }
        public bool IsSuccessStatusCode { get; private set; }

        public ServerResponse(int statusCode, string content)
        {
            StatusCode = statusCode;
            Content = content;
            IsSuccessStatusCode = statusCode >= 200 && statusCode < 300;
        }
    }

}
