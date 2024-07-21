using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace CorePlugins.DirectoryListPlugin
{
    [AgentPlugin("DirectoryListPlugin")]
    internal class DirectoryListPlugin : PluginBase, IOneTimeAgentPlugin
    {
        public DirectoryListPlugin(AgentPluginContext context) : base(context)
        {
        }   

        public override bool Load(AgentPluginArguments agentPluginArguments = null)
        {
            Logger.LogInfo("DirectoryListPlugin loaded");
            return true;
        }

        public void Execute(AgentPluginArguments args = null)
        {
            throw new NotImplementedException();
        }
    }
}
