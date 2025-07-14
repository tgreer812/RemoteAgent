using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace CorePlugins.DownloadFilePlugin
{
    [AgentPlugin("DownloadFilePlugin")]
    internal class DownloadFilePlugin : PluginBase
    {
        public DownloadFilePlugin(PluginContext context) : base(context)
        {
        }

        public override async Task<bool> LoadAsync(PluginArguments? agentPluginArguments = null)
        {
            Logger.LogInfo("DownloadFilePlugin loaded");
            return await Task.FromResult(true);
        }

        public override async Task<PluginResult> StartAsync(PluginArguments? args = null, CancellationToken cancellationToken = default)
        {
            var arguments = new DownloadFileArguments(args ?? new PluginArguments());

            if (string.IsNullOrEmpty(arguments.FilePath))
            {
                Logger.LogError("FilePath argument is required");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = "FilePath argument is required" };
            }

            if (!File.Exists(arguments.FilePath))
            {
                Logger.LogError($"File does not exist: {arguments.FilePath}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"File does not exist: {arguments.FilePath}" };
            }

            try
            {
                Logger.LogInfo($"Reading file: {arguments.FilePath}");
                
                // Read file content as base64 to handle binary files
                byte[] fileBytes = await File.ReadAllBytesAsync(arguments.FilePath, cancellationToken);
                string base64Content = Convert.ToBase64String(fileBytes);
                
                // Get file info
                var fileInfo = new FileInfo(arguments.FilePath);
                
                var result = new Dictionary<string, object>
                {
                    ["FileName"] = fileInfo.Name,
                    ["FilePath"] = arguments.FilePath,
                    ["FileSize"] = fileInfo.Length,
                    ["LastModified"] = fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["Content"] = base64Content,
                    ["Encoding"] = "base64"
                };

                Logger.LogInfo($"Successfully read file: {arguments.FilePath} ({fileInfo.Length} bytes)");
                return new PluginResult() { Status = PluginStatus.Success, OutputData = result };
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError($"Access denied reading file: {arguments.FilePath} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Access denied: {ex.Message}" };
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error reading file: {arguments.FilePath} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = ex.Message };
            }
        }
    }
}
