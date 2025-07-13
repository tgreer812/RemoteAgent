using AgentCommon.AgentPluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.PluginManagement
{
    internal class PluginCompletedEventArgs : EventArgs
    {
        public uint CorrelationId { get; set; }
        public PluginResult Result { get; set; }

        public PluginCompletedEventArgs(uint correlationId, PluginResult result)
        {
            CorrelationId = correlationId;
            Result = result;
        }
    }
}
