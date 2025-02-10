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
        public PluginResult Result { get; set; }

        public PluginCompletedEventArgs(PluginResult result)
        {
            Result = result;
        }
    }
}
