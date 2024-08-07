using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.PluginManagement
{
    internal interface IPluginManager
    {
        void LoadPlugin();
        
        void LoadCorePlugins();
        
        Task StopPlugin();
        
        Task<bool> StopAllPlugins();

        void StartPlugin(JObject args);
    }
}
