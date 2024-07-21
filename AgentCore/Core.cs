using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentCommon;
using AgentCore.PluginManagement;
using AgentCore.JobManagement;
using AgentCore.EventManagement;
using AgentCore.CommunicationManagement;


namespace AgentCore
{
    public static class Core
    {
        internal static ILogger Logger { get; set; }
        internal static bool IsRunning { get; set; }

        internal static IPluginManager PluginManager { get; set; }
        internal static IJobManager JobManager { get; set; }
        public static IEventDispatcher EventManager { get; set; }

        
        public static void Run(ILogger logger)
        {
            if (IsRunning) { logger.LogError("Core is already running!"); return; }

            logger.LogInfo("Hello from Core!");
            IsRunning = true;
            Logger = logger;

            StartCoreServices();

            Logger.LogDebug("Core is running...");
            while (IsRunning)
            {
                // sleep
                Thread.Sleep(1000);
                Logger.LogDebug("Core is looping...");
            }
        }

        private static void StartCoreServices()
        {
            Logger.LogInfo("Starting Core services...");

            // Loop through this assembly and find all properties in this class that are of type ICoreService
            var coreServices = typeof(Core).GetProperties()
                .Where(p => p.PropertyType.GetInterfaces().Contains(typeof(ICoreService)));

            foreach (var coreService in coreServices)
            {
                var instance = coreService.GetValue(null);
                if (instance != null)
                {
                    Logger.LogInfo($"Starting {coreService.Name}...");
                    coreService.PropertyType.GetMethod("Start").Invoke(instance, null);
                }
                else
                {
                    Logger.LogWarning($"Instance for {coreService.Name} is not initialized.");
                }
            }
        }

        public static void Stop()
        {
            if (!IsRunning) { Logger.LogError("Core is not running!"); return; }

            Logger.LogInfo("Core is stopping...");

            // Stop all plugins
            PluginManager.StopAllPlugins();

            // TODO: Stop all core services
            
            // Tell the core to stop running
            IsRunning = false;
        }

        private static ILogger _logger = new ConsoleLogger();
    }
}
