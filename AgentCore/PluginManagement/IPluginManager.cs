using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.PluginManagement
{
    internal interface IPluginManager : ICoreService
    {
        void LoadPlugin();
        
        void LoadCorePlugins();
        
        void StopPlugin();
        
        void StopAllPlugins();

        void StartPlugin();
    }
}
