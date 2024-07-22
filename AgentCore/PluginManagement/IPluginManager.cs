using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.PluginManagement
{
    public interface IPluginManager : ICoreService
    {
        void LoadPlugin();
        
        void LoadCorePlugins();
        
        Task StopPlugin();
        
        Task<bool> StopAllPlugins();

        void StartPlugin();
    }
}
