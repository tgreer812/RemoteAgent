using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public interface IPlugin
    {
        PluginContext Context { get; set; }
        bool Load(PluginArguments agentPluginArguments = null);

        bool Unload(PluginArguments agentPluginArguments = null);

        bool Start(PluginArguments agentPluginArguments = null);

        bool Stop(PluginArguments agentPluginArguments = null);

        
    }
}
