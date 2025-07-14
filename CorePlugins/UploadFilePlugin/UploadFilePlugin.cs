using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace CorePlugins.UploadFilePlugin
{
    [AgentPlugin("UploadFilePlugin")]
    internal class UploadFilePlugin : PluginBase
    {
        public UploadFilePlugin(PluginContext context) : base(context)
        {
        }

        public override async Task<bool> LoadAsync(PluginArguments? agentPluginArguments = null)
        {
            Logger.LogInfo("UploadFilePlugin loaded");
            return await Task.FromResult(true);
        }

        public override async Task<PluginResult> StartAsync(PluginArguments? args = null, CancellationToken cancellationToken = default)
        {
            var arguments = new UploadFileArguments(args ?? new PluginArguments());

            if (string.IsNullOrEmpty(arguments.FilePath))
            {
                Logger.LogError("FilePath argument is required");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = "FilePath argument is required" };
            }

            if (string.IsNullOrEmpty(arguments.Content))
            {
                Logger.LogError("Content argument is required");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = "Content argument is required" };
            }

            try
            {
                Logger.LogInfo($"Uploading file to: {arguments.FilePath}");

                // Create directory if it doesn't exist and CreateDirectories is true
                var directory = Path.GetDirectoryName(arguments.FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    if (arguments.CreateDirectories)
                    {
                        Directory.CreateDirectory(directory);
                        Logger.LogInfo($"Created directory: {directory}");
                    }
                    else
                    {
                        Logger.LogError($"Directory does not exist: {directory}");
                        return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Directory does not exist: {directory}" };
                    }
                }

                byte[] fileBytes;
                
                // Decode content based on encoding
                switch (arguments.Encoding.ToLowerInvariant())
                {
                    case "base64":
                        fileBytes = Convert.FromBase64String(arguments.Content);
                        break;
                    case "utf8":
                    case "text":
                        fileBytes = Encoding.UTF8.GetBytes(arguments.Content);
                        break;
                    default:
                        Logger.LogError($"Unsupported encoding: {arguments.Encoding}");
                        return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Unsupported encoding: {arguments.Encoding}" };
                }

                // Write file
                await File.WriteAllBytesAsync(arguments.FilePath, fileBytes, cancellationToken);
                
                // Get file info for result
                var fileInfo = new FileInfo(arguments.FilePath);
                
                var result = new Dictionary<string, object>
                {
                    ["FilePath"] = arguments.FilePath,
                    ["FileSize"] = fileInfo.Length,
                    ["Created"] = fileInfo.CreationTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["LastModified"] = fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["BytesWritten"] = fileBytes.Length
                };

                Logger.LogInfo($"Successfully uploaded file: {arguments.FilePath} ({fileBytes.Length} bytes)");
                return new PluginResult() { Status = PluginStatus.Success, OutputData = result };
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError($"Access denied writing file: {arguments.FilePath} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Access denied: {ex.Message}" };
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.LogError($"Directory not found: {arguments.FilePath} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Directory not found: {ex.Message}" };
            }
            catch (FormatException ex)
            {
                Logger.LogError($"Invalid content format for encoding {arguments.Encoding}: {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Invalid content format: {ex.Message}" };
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error uploading file: {arguments.FilePath} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = ex.Message };
            }
        }
    }
}
