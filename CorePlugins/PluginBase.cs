using AgentCommon;
using AgentCommon.AgentPluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlugins
{
    public abstract class PluginBase : IPlugin
    {
        public ILogger Logger { get; set; }
        public PluginContext Context { get; set; }

        public PluginBase(PluginContext context) 
        { 
            // Pull out the logger for convenience
            Logger = context.Logger;
            Context = context;
        }

        public virtual bool Load(PluginArguments agentPluginArguments = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool Unload(PluginArguments agentPluginArguments = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool Start(PluginArguments agentPluginArguments = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool Stop(PluginArguments agentPluginArguments = null)
        {
            throw new NotImplementedException();
        }
    }
}
