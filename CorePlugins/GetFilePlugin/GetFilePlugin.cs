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
    internal class GetFilePlugin : PluginBase
    {
        public GetFilePlugin(PluginContext context) : base(context)
        {
        }

        public override bool Load(PluginArguments agentPluginArguments = null)
        {
            Logger.LogInfo("GetFilePlugin loaded");
            return true;
        }

        public void Execute(PluginArguments args = null)
        {
            throw new NotImplementedException();
        }
    }
}
