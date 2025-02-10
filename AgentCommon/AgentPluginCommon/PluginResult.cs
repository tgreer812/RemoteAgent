using AgentCommon.AgentPluginCommon;

namespace AgentCommon.AgentPluginCommon
{
    /// <summary>
    /// Represents the result of a plugin operation, typically returned by the Start method of an IPlugin.
    /// </summary>
    public class PluginResult
    {
        /// <summary>
        /// Gets or sets the status of the plugin result. Indicates whether the plugin operation was successful, failed, etc.
        /// </summary>
        public PluginStatus Status { get; set; }

        /// <summary>
        /// Gets or sets an optional object containing output data from the plugin operation.
        /// This can be used to return structured data (e.g., lists, dictionaries, custom objects) from the plugin.
        /// The content and type of OutputData will be specific to each plugin.
        /// </summary>
        public object OutputData { get; set; }

        /// <summary>
        /// Gets or sets an optional error message.
        /// This should be populated if the Status is Failed or indicates an error condition.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Default constructor for PluginResult.
        /// Initializes Status to PluginStatus.Unknown by default.
        /// </summary>
        public PluginResult()
        {
            Status = PluginStatus.Unknown; // Default status if not explicitly set
        }
    }

    /// <summary>
    /// Enumerates the possible statuses of a plugin operation result.
    /// </summary>
    public enum PluginStatus
    {
        /// <summary>
        /// Represents an unknown or undefined plugin status.
        /// This might be used as an initial status or when the status cannot be determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates that the plugin operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Indicates that the plugin operation failed.
        /// Check the ErrorMessage property of PluginResult for details.
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates that the plugin operation is currently in progress and has not yet completed.
        /// This status is typically used for long-running plugins.
        /// </summary>
        InProgress,

        /// <summary>
        /// Indicates that the plugin operation was cancelled before completion.
        /// This status is typically used when a CancellationToken is triggered.
        /// </summary>
        Cancelled,
    }
}