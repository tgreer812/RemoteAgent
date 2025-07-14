using System;

namespace AgentCore.CommunicationManagement
{
    /// <summary>
    /// Abstraction for configuration management
    /// </summary>
    public interface ICommunicationConfiguration
    {
        /// <summary>
        /// The base server address for communication
        /// </summary>
        string ServerAddress { get; }

        /// <summary>
        /// The endpoint for tasking requests
        /// </summary>
        string TaskingEndpoint { get; }

        /// <summary>
        /// The endpoint for job results
        /// </summary>
        string JobEndpoint { get; }

        /// <summary>
        /// The endpoint for agent hello/handshake
        /// </summary>
        string AgentHelloEndpoint { get; }

        /// <summary>
        /// The timeout for HTTP requests
        /// </summary>
        TimeSpan RequestTimeout { get; }

        /// <summary>
        /// The delay between handshake retry attempts
        /// </summary>
        TimeSpan HandshakeRetryDelay { get; }

        /// <summary>
        /// Maximum number of handshake retry attempts
        /// </summary>
        int MaxHandshakeRetries { get; }

        /// <summary>
        /// The agent's unique identifier
        /// </summary>
        Guid AgentGuid { get; }

        /// <summary>
        /// The agent ID assigned by the server (set during handshake)
        /// </summary>
        int? AgentId { get; set; }

        /// <summary>
        /// The interval between task polling requests
        /// </summary>
        TimeSpan TaskPollingInterval { get; }
    }
}
