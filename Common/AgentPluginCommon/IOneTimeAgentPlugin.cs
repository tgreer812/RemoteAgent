using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public interface IOneTimeAgentPlugin : IAgentPlugin
    {
        void Execute(AgentPluginArguments args = null);
    }
}
