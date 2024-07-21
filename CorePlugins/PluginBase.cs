using AgentCommon;
using AgentCommon.AgentPluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlugins
{
    public class PluginBase : IAgentPlugin
    {
        public ILogger Logger { get; set; }
        public AgentPluginContext Context { get; set; }

        public PluginBase(AgentPluginContext context) 
        { 
            // Pull out the logger for convenience
            Logger = context.Logger;
            Context = context;
        }

        public virtual bool Load(AgentPluginArguments agentPluginArguments = null)
        {
            throw new NotImplementedException();
        }
    }
}
