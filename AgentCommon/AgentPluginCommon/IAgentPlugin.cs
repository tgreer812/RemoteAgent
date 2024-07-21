using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public interface IAgentPlugin
    {
        AgentPluginContext Context { get; set; }
        bool Load(AgentPluginArguments agentPluginArguments = null);
    }
}
