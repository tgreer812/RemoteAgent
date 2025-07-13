using System;
using AgentCommon;
using AgentCore.CommunicationManagement;
using AgentCore.EventManagement;
using AgentCore.JobManagement;
using AgentCore.PluginManagement;

namespace AgentCore
{
    /// <summary>
    /// Service factory for creating CoreHost instances with proper dependency injection
    /// </summary>
    public static class CoreHostFactory
    {
        /// <summary>
        /// Create a CoreHost with default implementations
        /// </summary>
        public static CoreHost CreateDefault(ILogger logger = null, AgentConfig config = null)
        {
            if (logger == null)
                logger = new ConsoleLogger();
            
            // Create dependencies with proper constructor parameters
            var eventDispatcher = new EventDispatcher(logger);
            var pluginManager = new PluginManager(logger, eventDispatcher);
            var jobManager = new JobManager(logger, eventDispatcher, pluginManager);
            
            // Create communication manager with all required dependencies
            var httpClient = new HttpClientWrapper();
            var communicationConfig = new DefaultCommunicationConfiguration(config);
            var messageSerializer = new JsonMessageSerializer(logger);
            var communicationManager = new CommunicationManager(
                logger, 
                httpClient, 
                communicationConfig, 
                eventDispatcher, 
                messageSerializer,
                config);
            
            return new CoreHost(
                logger,
                pluginManager,
                jobManager,
                eventDispatcher,
                communicationManager,
                config);
        }

        /// <summary>
        /// Create a CoreHost with custom implementations (for testing)
        /// </summary>
        public static CoreHost Create(
            ILogger logger,
            IPluginManager pluginManager,
            IJobManager jobManager,
            IEventDispatcher eventDispatcher,
            ICommunicationManager communicationManager,
            AgentConfig config = null)
        {
            return new CoreHost(
                logger,
                pluginManager,
                jobManager,
                eventDispatcher,
                communicationManager,
                config);
        }
    }
}
