using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentCommon;
using Newtonsoft.Json;

namespace AgentCore.CommunicationManagement
{
    public class JsonMessageDeserializer : IMessageDeserializer
    {
        private ILogger Logger { get; set; }
        public JsonMessageDeserializer(ILogger logger)
        {
            Logger = logger;
        }
        public ServerResponse Deserialize(string message)
        {
            try
            {
                return JsonConvert.DeserializeObject<ServerResponse>(message);
            } catch (Exception e)
            {
                Logger.LogError("Failed to deserialize message", e);
                return null;
            }
        }
    }
}
