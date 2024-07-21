using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentCommon;
using AgentCore.PluginManagement;
using AgentCore.TaskManagement;


namespace AgentCore
{
    public static class Core
    {
        internal static ILogger Logger { get; set; }
        public static bool IsRunning { get; set; }

        internal static PluginManager PluginManager { get; set; }
        internal static TaskManager TaskManager { get; set; }

        
        public static void Run(ILogger logger)
        {
            if (IsRunning) { logger.LogError("Core is already running!"); return; }

            logger.LogInfo("Hello from Core!");
            IsRunning = true;
            Logger = logger;

            PluginManager = new PluginManager(logger);
            PluginManager.LoadCorePlugins();

            TaskManager = new TaskManager(logger);

            while (IsRunning)
            {
                Logger.LogDebug("Core is running...");
                System.Threading.Thread.Sleep(1000);

            }
        }

        public static void Stop()
        {
            if (!IsRunning) { Logger.LogError("Core is not running!"); return; }

            Logger.LogInfo("Core is stopping...");

            PluginManager.StopAll();


        }

        private static ILogger _logger = new ConsoleLogger();
    }
}
