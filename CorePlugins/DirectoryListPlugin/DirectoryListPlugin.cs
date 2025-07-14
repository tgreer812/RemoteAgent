using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;
using Newtonsoft.Json;

namespace CorePlugins.DirectoryListPlugin
{
    [AgentPlugin("DirectoryListPlugin")]
    internal class DirectoryListPlugin : PluginBase
    {

        public DirectoryListPlugin(PluginContext context) : base(context)
        {
        }

        public override async Task<bool> LoadAsync(PluginArguments? agentPluginArguments = null)
        {
            Logger.LogInfo("DirectoryListPlugin loaded");
            return await Task.FromResult(true); // Use Task.FromResult for async-compatible return
        }

        public override async Task<PluginResult> StartAsync(PluginArguments? args = null, CancellationToken cancellationToken = default)
        {
            DirectoryListArguments arguments = new DirectoryListArguments(args ?? new PluginArguments());

            if (string.IsNullOrEmpty(arguments.Path))
            {
                Logger.LogError("Path argument is required");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = "Path argument is required" }; // Return PluginResult for failure
            }

            if (!Directory.Exists(arguments.Path))
            {
                Logger.LogError("Path does not exist: " + arguments.Path);
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Path does not exist: {arguments.Path}" }; // Return PluginResult for failure
            }

            try
            {
                // Use Task.Run to offload potentially blocking IO operation to a background thread
                // Get both files and directories
                string[] files = await Task.Run(() => Directory.GetFiles(arguments.Path), cancellationToken);
                string[] directories = await Task.Run(() => Directory.GetDirectories(arguments.Path), cancellationToken);
                
                // Combine and sort all entries
                var allEntries = files.Concat(directories).OrderBy(entry => entry).ToArray();
                
                string output = $@"Directory listing for {arguments.Path}:"
                                 + Environment.NewLine
                                 + string.Join(Environment.NewLine, allEntries);

                // Return PluginResult with success and output data
                return new PluginResult()
                {
                    Status = PluginStatus.Success,
                    OutputData = output // Store the directory listing in OutputData
                };
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Directory listing operation was cancelled.");
                return new PluginResult() { Status = PluginStatus.Cancelled, ErrorMessage = "Directory listing was cancelled." }; // Return PluginResult for cancellation
            }
            catch (Exception e)
            {
                Logger.LogError($"Error listing directory: {e.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Error listing directory: {e.Message}" }; // Return PluginResult for failure
            }
        }

        public override async Task<bool> StopAsync(PluginArguments? agentPluginArguments = null)
        {
            Logger.LogInfo("DirectoryListPlugin stopped (no specific stop logic).");
            return await Task.FromResult(true); // Use Task.FromResult for async-compatible return, no specific stop action
        }

        public override async Task<bool> UnloadAsync(PluginArguments? agentPluginArguments = null)
        {
            Logger.LogInfo("DirectoryListPlugin unloaded");
            return await Task.FromResult(true); // Use Task.FromResult for async-compatible return
        }
    }

}