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
    internal class DirectoryListPlugin : PluginBase
    {
        public DirectoryListPlugin(PluginContext context) : base(context)
        {
        }   

        public override bool Load(PluginArguments agentPluginArguments = null)
        {
            Logger.LogInfo("DirectoryListPlugin loaded");
            return true;
        }

        public void Execute(PluginArguments args = null)
        {
            throw new NotImplementedException();
        }
    }
}
