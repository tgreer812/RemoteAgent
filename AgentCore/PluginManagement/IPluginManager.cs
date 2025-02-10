using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace AgentCore.PluginManagement
{
    internal interface IPluginManager
    {
        Task LoadPluginAsync(); // Updated to async

        Task LoadCorePluginsAsync(); // Updated to async

        Task<bool> StopPluginAsync(); // Updated to async

        Task<bool> StopAllPluginsAsync(); // Updated to async

        Task StartPluginAsync(JObject args); // Updated to async
    }
}
