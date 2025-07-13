using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// Modern, testable Core host that eliminates singleton pattern and provides proper async lifecycle management
    /// </summary>
    public class CoreHost : IDisposable
    {
        private readonly ILogger _logger;
        private readonly List<ICoreService> _coreServices;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _runTask;
        private bool _disposed;

        public bool IsRunning { get; private set; }
        public AgentConfig Config { get; private set; }
        
        // Expose services through properties for backward compatibility
        public IPluginManager PluginManager { get; }
        public IJobManager JobManager { get; }
        public IEventDispatcher EventManager { get; }
        public ICommunicationManager CommunicationManager { get; }

        public CoreHost(
            ILogger logger,
            IPluginManager pluginManager,
            IJobManager jobManager,
            IEventDispatcher eventManager,
            ICommunicationManager communicationManager,
            AgentConfig config = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            PluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
            JobManager = jobManager ?? throw new ArgumentNullException(nameof(jobManager));
            EventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            CommunicationManager = communicationManager ?? throw new ArgumentNullException(nameof(communicationManager));
            Config = config; // Allow injected config for testing

            _cancellationTokenSource = new CancellationTokenSource();
            
            // Collect all core services for lifecycle management
            _coreServices = new List<ICoreService>
            {
                pluginManager as ICoreService,
                jobManager as ICoreService,
                eventManager as ICoreService,
                communicationManager as ICoreService
            }.Where(s => s != null).ToList();
        }

        /// <summary>
        /// Start the core host asynchronously
        /// </summary>
        public async Task StartAsync(string configPath = "AgentConfig.json")
        {
            if (IsRunning)
            {
                _logger.LogWarning("Core host is already running!");
                return;
            }

            try
            {
                _logger.LogInfo("Starting Core Host...");
                
                // Load configuration only if not already injected
                if (Config == null)
                {
                    Config = AgentConfig.LoadFromFile(configPath);
                }
                
                // Start all core services in priority order
                await StartCoreServicesAsync();
                
                IsRunning = true;
                
                // Start the main loop
                _runTask = RunMainLoopAsync(_cancellationTokenSource.Token);
                
                _logger.LogInfo("Core Host started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start Core Host: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop the core host asynchronously
        /// </summary>
        public async Task StopAsync()
        {
            if (!IsRunning)
            {
                _logger.LogWarning("Core host is not running!");
                return;
            }

            try
            {
                _logger.LogInfo("Stopping Core Host...");
                
                // Signal cancellation
                _cancellationTokenSource.Cancel();
                
                // Wait for main loop to complete
                if (_runTask != null)
                {
                    await _runTask;
                }
                
                // Stop all core services in reverse order
                await StopCoreServicesAsync();
                
                IsRunning = false;
                _logger.LogInfo("Core Host stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping Core Host: {ex.Message}");
                throw;
            }
        }

        private async Task StartCoreServicesAsync()
        {
            _logger.LogInfo("Starting Core services...");

            // Order services by LoadPriority attribute (highest first)
            var orderedServices = _coreServices
                .OrderByDescending(service => service.GetType().GetCustomAttribute<LoadPriorityAttribute>()?.LoadPriority ?? 0)
                .ToList();

            foreach (var service in orderedServices)
            {
                try
                {
                    _logger.LogInfo($"Starting {service.GetType().Name}...");
                    await service.Start();
                    _logger.LogInfo($"{service.GetType().Name} started successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to start {service.GetType().Name}: {ex.Message}");
                    throw;
                }
            }
        }

        private async Task StopCoreServicesAsync()
        {
            _logger.LogInfo("Stopping Core services...");

            // Stop services in reverse order
            var reversedServices = _coreServices
                .OrderBy(service => service.GetType().GetCustomAttribute<LoadPriorityAttribute>()?.LoadPriority ?? 0)
                .ToList();

            foreach (var service in reversedServices)
            {
                try
                {
                    _logger.LogInfo($"Stopping {service.GetType().Name}...");
                    await service.Stop();
                    _logger.LogInfo($"{service.GetType().Name} stopped successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error stopping {service.GetType().Name}: {ex.Message}");
                    // Continue stopping other services
                }
            }
        }

        private async Task RunMainLoopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Core Host main loop started");
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Core Host heartbeat");
                    
                    // Use proper async delay with cancellation support
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when shutting down
                _logger.LogDebug("Core Host main loop cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in Core Host main loop: {ex.Message}");
                throw;
            }
            
            _logger.LogInfo("Core Host main loop stopped");
        }

        public void Dispose()
        {
            if (_disposed) return;

            _logger?.LogDebug("Disposing Core Host");
            
            // Stop if still running
            if (IsRunning)
            {
                try
                {
                    StopAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error during disposal: {ex.Message}");
                }
            }

            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
    }
}
