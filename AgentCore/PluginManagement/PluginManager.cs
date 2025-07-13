using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;
using AgentCore.EventManagement;
using AgentCore.JobManagement;
using CorePlugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AgentCore.PluginManagement
{
    internal class PluginManager : ICoreService, IPluginManager
    {
        public ILogger Logger { get; set; }
        public Dictionary<string, IPlugin> LoadedPlugins { get; set; }
        private readonly IEventDispatcher _eventDispatcher;

        public bool IsRunning { get; set; }
        public PluginManager(ILogger logger, IEventDispatcher eventDispatcher = null)
        {
            Logger = logger;
            _eventDispatcher = eventDispatcher;
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
            await LoadCorePluginsAsync(); // Use the async version
            Logger.LogInfo("PluginManager started successfully");
        }

        public async Task<bool> Stop()
        {
            return await StopAllPluginsAsync(); // Use the async version
        }

        public async Task LoadCorePluginsAsync() // Made LoadCorePlugins Async
        {
            // Load all plugins in the CorePlugins directory
            // by searching for the AgentPluginAttribute
            // and instantiating the class
            var pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CorePlugins.dll");
            Assembly corePluginsAssembly = Assembly.LoadFrom(pluginPath);

            // Create a context object to pass to the plugins
            PluginContext context = new PluginContext();
            context.Logger = Logger;

            var pluginTypes = corePluginsAssembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(AgentPluginAttribute), false).Length > 0);

            foreach (var pluginType in pluginTypes)
            {
                Logger.LogInfo($"Loading plugin: {pluginType.Name}");

                IPlugin plugin = (IPlugin)Activator.CreateInstance(pluginType, context);
                bool loaded = await plugin.LoadAsync(); // Call LoadAsync and await
                if (loaded)
                {
                    LoadedPlugins.Add(pluginType.Name, plugin);
                }
                else
                {
                    Logger.LogError($"Plugin {pluginType.Name} failed to load."); // Log failure to load
                }
            }
        }

        public Task LoadPluginAsync() // Made LoadPlugin Async
        {
            // Load a plugin from a file
            throw new NotImplementedException();
        }

        public Task UnloadPluginAsync() // Made UnloadPlugin Async
        {
            // Unload a plugin
            throw new NotImplementedException();
        }

        public async Task StartPluginAsync(uint correlationId, JObject args)
        {
            string pluginName = args["pluginName"].ToString();
            JObject jObjectPluginArgs = (JObject)args["pluginArguments"];

            // Check if the plugin is loaded
            if (!LoadedPlugins.ContainsKey(pluginName))
            {
                Logger.LogError($"Plugin {pluginName} is not loaded");
                return;
            }

            PluginArguments pluginArgs = PluginArguments.FromJObject(jObjectPluginArgs);

            IPlugin plugin = LoadedPlugins[pluginName];

            try
            {
                PluginResult pluginResult = await plugin.StartAsync(pluginArgs, CancellationToken.None); // Call StartAsync and await, pass CancellationToken.None for now

                if (pluginResult != null) // Check for null PluginResult
                {
                    Logger.LogInfo($"Plugin {pluginName} StartAsync completed with status: {pluginResult.Status}");
                    if (pluginResult.Status == PluginStatus.Failed)
                    {
                        Logger.LogError($"Plugin {pluginName} reported failure: {pluginResult.ErrorMessage}");
                    }
                    // You can process pluginResult.OutputData here if needed
                    if (pluginResult.OutputData != null)
                    {
                        Logger.LogDebug($"Plugin {pluginName} Output Data: {JsonConvert.SerializeObject(pluginResult.OutputData)}"); // Example of logging OutputData
                    }
                }
                else
                {
                    // This shouldn't be possible
                    throw new Exception($"Plugin {pluginName} StartAsync returned null PluginResult.");
                }

                // Publish plugin completion event if event dispatcher is available
                _eventDispatcher?.Publish("PluginCompleted", this, new PluginCompletedEventArgs(correlationId, pluginResult));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to start plugin {pluginName}", ex);
            }
        }

        public Task<bool> StopPluginAsync() // Made StopPlugin Async
        {
            // Stop a specific plugin
            throw new NotImplementedException();
        }

        public async Task<bool> StopAllPluginsAsync() // Made StopAllPlugins Async
        {
            // Stop all plugins
            // Iterate through loaded plugins and call StopAsync on each
            foreach (var pluginPair in LoadedPlugins)
            {
                string pluginName = pluginPair.Key;
                IPlugin plugin = pluginPair.Value;
                try
                {
                    bool stopped = await plugin.StopAsync(); // Call StopAsync and await
                    if (stopped)
                    {
                        Logger.LogInfo($"Plugin {pluginName} stopped successfully.");
                    }
                    else
                    {
                        Logger.LogWarning($"Plugin {pluginName} StopAsync returned false or did not stop.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error stopping plugin {pluginName}", ex);
                    // Continue stopping other plugins instead of returning false
                }
            }
            
            IsRunning = false;
            Logger.LogInfo("PluginManager stopped successfully");
            return true; // All plugins (attempted to) stop
        }

        public void HandlePluginJob(Job job)
        {
            try
            {
                _handlePluginJobAsync(job).ConfigureAwait(false).GetAwaiter().GetResult(); // Block to call async method from sync context (HandlePluginJob) - Consider making HandlePluginJob Async if possible in your architecture.
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
        private async Task _handlePluginJobAsync(Job job) // Made _handlePluginJob Async
        {
            Logger.LogDebug($"Handling plugin job: {job.JobData}");
            // Placeholder for actual job handling logic - You'll need to determine which plugin to invoke based on Job data
            // and then call the appropriate plugin's StartAsync or other relevant method.
            // Example (Illustrative and needs to be adapted to your actual Job structure and plugin dispatching):
            if (job.JobData is JObject jobDataJson)
            {
                string pluginName = jobDataJson["pluginName"]?.ToString(); // Assuming JobData contains pluginName
                if (!string.IsNullOrEmpty(pluginName) && LoadedPlugins.ContainsKey(pluginName))
                {
                    IPlugin plugin = LoadedPlugins[pluginName];
                    PluginArguments pluginArgs = PluginArguments.FromJObject(jobDataJson["pluginArguments"] as JObject); // Assuming JobData also contains pluginArguments
                    PluginResult pluginResult = await plugin.StartAsync(pluginArgs, CancellationToken.None); // Start the plugin's job

                    if (pluginResult != null)
                    {
                        Logger.LogInfo($"Plugin Job for {pluginName} completed with status: {pluginResult.Status}");
                        if (pluginResult.Status == PluginStatus.Failed)
                        {
                            Logger.LogError($"Plugin Job for {pluginName} reported failure: {pluginResult.ErrorMessage}");
                        }
                        // Process pluginResult.OutputData if needed
                        if (pluginResult.OutputData != null)
                        {
                            Logger.LogDebug($"Plugin Job for {pluginName} Output Data: {JsonConvert.SerializeObject(pluginResult.OutputData)}");
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"Plugin Job for {pluginName} StartAsync returned null PluginResult.");
                    }

                }
                else
                {
                    Logger.LogError($"Could not determine plugin name from job data or plugin not loaded.");
                }
            }
            else
            {
                Logger.LogWarning($"Job data is not in expected JObject format for plugin job handling.");
            }
        }
    }
}