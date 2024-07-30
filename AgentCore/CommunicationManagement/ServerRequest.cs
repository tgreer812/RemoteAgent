using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.CommunicationManagement
{
    public class ServerRequest
    {        
        public string RequestType { get; private set; }
        public object Payload { get; private set; }

        public ServerRequest(string requestType, object payload)
        {
            RequestType = requestType;
            Payload = payload;
        }

    }
}
