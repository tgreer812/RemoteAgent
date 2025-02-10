using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public interface IPlugin
    {
        PluginContext Context { get; set; }

        /// <summary>
        /// Asynchronously loads the plugin.
        /// </summary>
        /// <param name="agentPluginArguments">Optional arguments for loading.</param>
        /// <returns>A Task representing the asynchronous operation, returning true if loaded successfully, false otherwise.</returns>
        Task<bool> LoadAsync(PluginArguments agentPluginArguments = null);

        /// <summary>
        /// Asynchronously unloads the plugin.
        /// </summary>
        /// <param name="agentPluginArguments">Optional arguments for unloading.</param>
        /// <returns>A Task representing the asynchronous operation, returning true if unloaded successfully, false otherwise.</returns>
        Task<bool> UnloadAsync(PluginArguments agentPluginArguments = null);

        /// <summary>
        /// Asynchronously starts the plugin's main execution logic.
        /// </summary>
        /// <param name="agentPluginArguments">Arguments to pass to the plugin's start operation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to stop the plugin's operation.</param>
        /// <returns>A Task that represents the asynchronous operation and returns a PluginResult upon completion.</returns>
        Task<PluginResult> StartAsync(PluginArguments agentPluginArguments = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously stops the plugin's execution if it's running.
        /// </summary>
        /// <param name="agentPluginArguments">Optional arguments for stopping.</param>
        /// <returns>A Task representing the asynchronous operation, returning true if stopped successfully, false otherwise.</returns>
        Task<bool> StopAsync(PluginArguments agentPluginArguments = null);
    }
}
