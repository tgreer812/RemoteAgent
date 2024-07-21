using AgentCommon;
using AgentCommon.AgentPluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public class PluginContext
    {
        public ILogger Logger { get; set; }
        public PluginContext()
        {

        }
    }
}