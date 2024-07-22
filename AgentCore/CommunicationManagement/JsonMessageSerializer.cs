using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentCommon;
using Newtonsoft.Json;

namespace AgentCore.CommunicationManagement
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        private ILogger Logger { get; set; }
        public JsonMessageSerializer(ILogger logger)
        {
            Logger = logger;
        }

        public string Serialize(ServerRequest request)
        {
            try
            {
                return JsonConvert.SerializeObject(request);
            } catch (Exception e)
            {
                Logger.LogError("Failed to serialize request", e);
                return null;
            }
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
