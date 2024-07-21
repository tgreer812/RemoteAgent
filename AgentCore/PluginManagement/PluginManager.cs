using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using AgentCommon.AgentPluginCommon;
using CorePlugins;
using AgentCommon;
using AgentCore.TaskManagement;

namespace AgentCore.PluginManagement
{
    internal class PluginManager
    {
        public ILogger Logger { get; set; }
        public Dictionary<string, IAgentPlugin> LoadedPlugins { get; set; }
        public PluginManager(ILogger logger)
        {
            Logger = logger;
            LoadedPlugins = new Dictionary<string, IAgentPlugin>();
        }

        public void LoadCorePlugins()
        {
            // Load all plugins in the CorePlugins directory
            // by searching for the AgentPluginAttribute
            // and instantiating the class
            Assembly corePluginsAssembly = Assembly.LoadFrom("CorePlugins.dll");

            // Create a context object to pass to the plugins
            AgentPluginContext context = new AgentPluginContext();
            context.Logger = Logger;
            
            var pluginTypes = corePluginsAssembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(AgentPluginAttribute), false).Length > 0);

            foreach (var pluginType in pluginTypes)
            {
                Logger.LogInfo($"Loading plugin: {pluginType.Name}");

                IAgentPlugin plugin = (IAgentPlugin)Activator.CreateInstance(pluginType, context);
                if (plugin.Load())
                {
                    LoadedPlugins.Add(pluginType.Name, plugin);
                }
            }
        }
    
        public void StopAll()
        {
            // Stop all plugins
            throw new NotImplementedException();
        }

        public void HandlePluginTask(PluginTask task)
        {
            // Check if the plugin is loaded
            if (!LoadedPlugins.ContainsKey(task.PluginName))
            {
                Logger.LogWarning($"Plugin {task.PluginName} is not loaded!");

                // TODO: Send a message to the server?
                return;
            }

            // Get the plugin
            IAgentPlugin plugin = LoadedPlugins[task.PluginName];

            // Check if the plugin is a long running plugin
            if (plugin is ILongRunningAgentPlugin longRunningPlugin)
            {
                // Handle long running plugin
                HandleLongRunningPlugin(longRunningPlugin, task);
            }
            // Check if the plugin is a one-time execution plugin
            else if (plugin is IOneTimeAgentPlugin oneTimePlugin)
            {
                // Handle one-time plugin
                HandleOneTimePlugin(oneTimePlugin, task);
            }
            else
            {
                Logger.LogWarning($"Plugin {task.PluginName} does not implement a recognized plugin interface!");
            }
        }

        private void HandleLongRunningPlugin(ILongRunningAgentPlugin plugin, PluginTask task)
        {
            if (task.OperationType == PluginOperationType.Stop)
            {
                // Implement handling of stopping long running plugins
                plugin.Stop();
                return;
            }
            // Implement handling of long running plugins
            plugin.Start(task.);
        }

        private void HandleOneTimePlugin(IOneTimeAgentPlugin plugin, PluginTask task)
        {
            // Implement handling of one-time execution plugins
            plugin.Execute(task.Arguments);
        }
    }
}
