using System;

namespace AgentCore.CommunicationManagement
{
    /// <summary>
    /// Default implementation of ICommunicationConfiguration
    /// </summary>
    internal class DefaultCommunicationConfiguration : ICommunicationConfiguration
    {
        public string ServerAddress { get; }
        public string TaskingEndpoint { get; }
        public string JobEndpoint { get; }
        public string AgentHelloEndpoint { get; }
        public TimeSpan RequestTimeout { get; }
        public TimeSpan HandshakeRetryDelay { get; }
        public int MaxHandshakeRetries { get; }
        public Guid AgentGuid { get; }
        public int? AgentId { get; set; }
        public TimeSpan TaskPollingInterval { get; }

        public DefaultCommunicationConfiguration(AgentConfig? config = null)
        {
            // Use config values if provided, otherwise use defaults
            ServerAddress = config?.ServerAddress ?? "http://localhost:5148";
            TaskingEndpoint = "api/tasking";
            JobEndpoint = "api/job";
            AgentHelloEndpoint = "api/agent/hello";
            RequestTimeout = TimeSpan.FromSeconds(30);
            HandshakeRetryDelay = TimeSpan.FromSeconds(30);
            MaxHandshakeRetries = 5;
            TaskPollingInterval = TimeSpan.FromSeconds(config?.TaskPollingIntervalSeconds ?? 30);
            
            // Get AgentGuid from provided config if available, otherwise generate a new one
            try
            {
                string? configAgentGuid = config?.AgentGuid;
                AgentGuid = !string.IsNullOrEmpty(configAgentGuid) ? Guid.Parse(configAgentGuid) : Guid.NewGuid();
                AgentId = config?.AgentId;
            }
            catch
            {
                // Fallback if config is invalid
                AgentGuid = Guid.NewGuid();
                AgentId = null;
            }
        }

        public DefaultCommunicationConfiguration(
            string serverAddress,
            string taskingEndpoint = "api/tasking",
            string jobEndpoint = "api/job", 
            string agentHelloEndpoint = "api/agent/hello",
            TimeSpan? requestTimeout = null,
            TimeSpan? handshakeRetryDelay = null,
            int maxHandshakeRetries = 5,
            Guid? agentGuid = null,
            int? agentId = null,
            TimeSpan? taskPollingInterval = null)
        {
            ServerAddress = serverAddress ?? throw new ArgumentNullException(nameof(serverAddress));
            TaskingEndpoint = taskingEndpoint;
            JobEndpoint = jobEndpoint;
            AgentHelloEndpoint = agentHelloEndpoint;
            RequestTimeout = requestTimeout ?? TimeSpan.FromSeconds(30);
            HandshakeRetryDelay = handshakeRetryDelay ?? TimeSpan.FromSeconds(30);
            MaxHandshakeRetries = maxHandshakeRetries;
            AgentGuid = agentGuid ?? Guid.NewGuid();
            AgentId = agentId;
            TaskPollingInterval = taskPollingInterval ?? TimeSpan.FromSeconds(30);
        }
    }
}
