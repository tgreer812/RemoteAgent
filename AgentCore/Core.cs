﻿using System;
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
using System.Reflection;


namespace AgentCore
{
    public partial class Core
    {
        internal ILogger Logger { get; set; }
        internal bool IsRunning { get; set; }

        internal IPluginManager PluginManager { get; set; }
        internal IJobManager JobManager { get; set; }
        private IEventDispatcher EventManager { get; set; }

        internal ICommunicationManager CommunicationManager { get; set; }

        public static Core Instance { get; private set; }

        private Core() { }

        public void Run()
        {
            try { _run(); }
            catch (Exception ex) { Logger.LogError(ex.Message); }
        }

        private void _run()
        {
            if (IsRunning) { Logger.LogError("Core is already running!"); return; }

            Logger.LogInfo("Hello from Core!");
            IsRunning = true;

            StartCoreServices();

            Logger.LogDebug("Core is running...");
            while (IsRunning)
            {
                // sleep
                Task.Delay(1000).Wait();
                Logger.LogDebug("Core is looping...");
            }
        }

        private void StartCoreServices()
        {
            Logger.LogInfo("Starting Core services...");

            // Loop through this assembly and find all properties in this class that are of type ICoreService
            var coreServices = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => p.GetValue(this))
                .Where(instance => instance != null && typeof(ICoreService).IsAssignableFrom(instance.GetType()))
                .OrderByDescending(instance => instance.GetType().GetCustomAttribute<LoadPriorityAttribute>()?.LoadPriority ?? 0)
                .Cast<ICoreService>()
                .ToList();

            foreach (var coreServiceInstance in coreServices)
            {
                if (coreServiceInstance != null)
                {
                    Logger.LogInfo($"Starting {coreServiceInstance.GetType().Name}...");

                    // Launch the async Start method without awaiting it
                    var task = coreServiceInstance.Start();
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Logger.LogError($"Error starting {coreServiceInstance.GetType().Name}", t.Exception);
                        }
                        else
                        {
                            Logger.LogInfo($"{coreServiceInstance.GetType().Name} started successfully.");
                        }
                    }, TaskScheduler.Current);
                }
                else
                {
                    Logger.LogWarning($"Instance for {coreServiceInstance.GetType().Name} is not initialized.");
                }
            }
        }


        public void Stop()
        {
            if (!IsRunning) { Logger.LogError("Core is not running!"); return; }

            Logger.LogInfo("Core is stopping...");

            // Stop all plugins
            PluginManager.StopAllPlugins();

            // Stop all core services
            var coreServices = GetType().GetProperties()
                .Where(p => p.PropertyType.GetInterfaces().Contains(typeof(ICoreService)));

            foreach (var coreService in coreServices)
            {
                var instance = coreService.GetValue(this);
                if (instance != null)
                {
                    coreService.PropertyType.GetMethod("Stop").Invoke(instance, null);
                }
            }

            // Tell the core to stop running
            IsRunning = false;
        }
    
        public static IEventDispatcher GetEventDispatcher()
        {
            return Core.Instance.EventManager;
        }
    
    
    }
}

