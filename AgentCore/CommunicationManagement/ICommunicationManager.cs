using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.CommunicationManagement
{
    public interface ICommunicationManager
    {
        /// <summary>
        /// Indicates whether the communication manager is currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the communication manager and initiates handshake with server
        /// </summary>
        /// <returns>Task representing the start operation</returns>
        Task Start();

        /// <summary>
        /// Stops the communication manager and cleans up resources
        /// </summary>
        /// <returns>Task with boolean indicating success</returns>
        Task<bool> Stop();

        /// <summary>
        /// Requests tasking from the server immediately
        /// </summary>
        /// <returns>Task representing the tasking request operation</returns>
        Task RequestTaskingAsync();

        /// <summary>
        /// Sends a plugin result back to the server
        /// </summary>
        /// <param name="correlationId">The job ID this result corresponds to</param>
        /// <param name="result">The plugin result to send</param>
        /// <returns>Task with boolean indicating success</returns>
        Task<bool> SendPluginResultAsync(uint correlationId, object result);
    }
}
