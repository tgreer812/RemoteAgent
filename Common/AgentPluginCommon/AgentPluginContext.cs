using AgentCommon;
using AgentCommon.AgentPluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public class AgentPluginContext
    {
        public ILogger Logger { get; set; }
        public AgentPluginContext()
        {

        }
    }
}