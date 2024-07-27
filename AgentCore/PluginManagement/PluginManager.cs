using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using AgentCommon.AgentPluginCommon;
using CorePlugins;
using AgentCommon;
using AgentCore.JobManagement;

namespace AgentCore.PluginManagement
{
    internal class PluginManager : ICoreService, IPluginManager
    {
        public ILogger Logger { get; set; }
        public Dictionary<string, IPlugin> LoadedPlugins { get; set; }

        public bool IsRunning { get; set; }
        public PluginManager(ILogger logger)
        {
            Logger = logger;
            LoadedPlugins = new Dictionary<string, IPlugin>();
        }

        public async Task Start()
        {
            if (IsRunning)
            {
                Logger.LogWarning("PluginManager is already running");
                return;
            }

            IsRunning = true;
            LoadCorePlugins();

            while (IsRunning)
            {
                await Task.Delay(1000);
                Logger.LogDebug("PluginManager is running...");
            }
        }

        public async Task<bool> Stop()
        {
            return await this.StopAllPlugins();
        }

        public void LoadCorePlugins()
        {
            // Load all plugins in the CorePlugins directory
            // by searching for the AgentPluginAttribute
            // and instantiating the class
            Assembly corePluginsAssembly = Assembly.LoadFrom("CorePlugins.dll");

            // Create a context object to pass to the plugins
            PluginContext context = new PluginContext();
            context.Logger = Logger;
            
            var pluginTypes = corePluginsAssembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(AgentPluginAttribute), false).Length > 0);

            foreach (var pluginType in pluginTypes)
            {
                Logger.LogInfo($"Loading plugin: {pluginType.Name}");

                IPlugin plugin = (IPlugin)Activator.CreateInstance(pluginType, context);
                if (plugin.Load())
                {
                    LoadedPlugins.Add(pluginType.Name, plugin);
                }
            }
        }

        public void LoadPlugin()
        {
            // Load a plugin from a file
            throw new NotImplementedException();
        }

        public void UnloadPlugin()
        {
            // Unload a plugin
            throw new NotImplementedException();
        }

        /*
         "pluginName": "DirectoryListPlugin",
            "pluginArguments": {
                "path": "C:\\"
            }
         */
        public void StartPlugin(IDictionary<string, object> args)
        {
            string pluginName = args["pluginName"].ToString();
            IDictionary<string, object> pluginArgsDict = (IDictionary<string, object>)args["pluginArguments"];

            // Check if the plugin is loaded
            if (!LoadedPlugins.ContainsKey(pluginName))
            {
                Logger.LogError($"Plugin {pluginName} is not loaded");
                return;
            }

            PluginArguments pluginArgs = new PluginArguments(pluginArgsDict);

            IPlugin plugin = LoadedPlugins[pluginName];
            plugin.Start(pluginArgs);
        }

        public async Task StopPlugin()
        {
            // Stop a specific plugin
            throw new NotImplementedException();
        }
    
        public async Task<bool> StopAllPlugins()
        {
            // Stop all plugins
            throw new NotImplementedException();
        }

        public void HandlePluginJob(Job job)
        {
            try
            {
                _handlePluginJob(job);
            } 
            catch (Exception ex)
            {
                Logger.LogError($"Failed to handle plugin job", ex);
            }
        }

        /// <summary>
        /// Not a safe method to call directly, as it does not catch exceptions
        /// </summary>
        /// <param name="job"></param>
        private void _handlePluginJob(Job job)
        {
            Logger.LogDebug($"Handling plugin job: {job.JobData}");
        }

        
    }
}
