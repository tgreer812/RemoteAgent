using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public interface ILongRunningAgentPlugin : IAgentPlugin
    {
        void Start(AgentPluginArguments args=null);
        void Stop(AgentPluginArguments args=null);
    }
}
