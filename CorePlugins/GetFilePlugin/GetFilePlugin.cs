using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace CorePlugins.GetFilePlugin
{
    [AgentPlugin("GetFilePlugin")]
    internal class GetFilePlugin : PluginBase, IOneTimeAgentPlugin
    {
        public GetFilePlugin(AgentPluginContext context) : base(context)
        {
        }

        public override bool Load(AgentPluginArguments agentPluginArguments = null)
        {
            Logger.LogInfo("GetFilePlugin loaded");
            return true;
        }

        public void Execute(AgentPluginArguments args = null)
        {
            throw new NotImplementedException();
        }
    }
}
