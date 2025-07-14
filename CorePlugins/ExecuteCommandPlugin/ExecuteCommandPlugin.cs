using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace CorePlugins.ExecuteCommandPlugin
{
    [AgentPlugin("ExecuteCommandPlugin")]
    internal class ExecuteCommandPlugin : PluginBase
    {
        public ExecuteCommandPlugin(PluginContext context) : base(context)
        {
        }

        public override async Task<bool> LoadAsync(PluginArguments? agentPluginArguments = null)
        {
            Logger.LogInfo("ExecuteCommandPlugin loaded");
            return await Task.FromResult(true);
        }

        public override async Task<PluginResult> StartAsync(PluginArguments? args = null, CancellationToken cancellationToken = default)
        {
            var arguments = new ExecuteCommandArguments(args ?? new PluginArguments());

            if (string.IsNullOrEmpty(arguments.Command))
            {
                Logger.LogError("Command argument is required");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = "Command argument is required" };
            }

            try
            {
                Logger.LogInfo($"Executing command: {arguments.Command} {arguments.Arguments}");

                var processStartInfo = new ProcessStartInfo();
                
                if (arguments.UseShell)
                {
                    // Use shell execution (cmd.exe on Windows, /bin/sh on Unix)
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        processStartInfo.FileName = "cmd.exe";
                        processStartInfo.Arguments = $"/c {arguments.Command} {arguments.Arguments}";
                    }
                    else
                    {
                        processStartInfo.FileName = "/bin/sh";
                        processStartInfo.Arguments = $"-c \"{arguments.Command} {arguments.Arguments}\"";
                    }
                }
                else
                {
                    // Direct execution
                    processStartInfo.FileName = arguments.Command;
                    processStartInfo.Arguments = arguments.Arguments;
                }

                if (!string.IsNullOrEmpty(arguments.WorkingDirectory) && Directory.Exists(arguments.WorkingDirectory))
                {
                    processStartInfo.WorkingDirectory = arguments.WorkingDirectory;
                }

                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                
                if (arguments.CaptureOutput)
                {
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.RedirectStandardError = true;
                }

                var stopwatch = Stopwatch.StartNew();
                string? standardOutput = null;
                string? standardError = null;
                int exitCode = -1;

                using var process = new Process { StartInfo = processStartInfo };
                
                if (arguments.CaptureOutput)
                {
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            outputBuilder.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            errorBuilder.AppendLine(e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to complete or timeout
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(arguments.TimeoutSeconds), cancellationToken);
                    var processTask = Task.Run(() => process.WaitForExit(), cancellationToken);

                    var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch { /* Process might have already exited */ }
                        
                        stopwatch.Stop();
                        Logger.LogError($"Command timed out after {arguments.TimeoutSeconds} seconds");
                        return new PluginResult() 
                        { 
                            Status = PluginStatus.Failed, 
                            ErrorMessage = $"Command timed out after {arguments.TimeoutSeconds} seconds" 
                        };
                    }

                    exitCode = process.ExitCode;
                    standardOutput = outputBuilder.ToString();
                    standardError = errorBuilder.ToString();
                }
                else
                {
                    process.Start();
                    
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(arguments.TimeoutSeconds), cancellationToken);
                    var processTask = Task.Run(() => process.WaitForExit(), cancellationToken);

                    var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch { /* Process might have already exited */ }
                        
                        stopwatch.Stop();
                        Logger.LogError($"Command timed out after {arguments.TimeoutSeconds} seconds");
                        return new PluginResult() 
                        { 
                            Status = PluginStatus.Failed, 
                            ErrorMessage = $"Command timed out after {arguments.TimeoutSeconds} seconds" 
                        };
                    }

                    exitCode = process.ExitCode;
                }

                stopwatch.Stop();

                var result = new Dictionary<string, object>
                {
                    ["Command"] = arguments.Command,
                    ["Arguments"] = arguments.Arguments,
                    ["ExitCode"] = exitCode,
                    ["ExecutionTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["WorkingDirectory"] = processStartInfo.WorkingDirectory ?? Environment.CurrentDirectory,
                    ["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };

                if (arguments.CaptureOutput)
                {
                    result["StandardOutput"] = standardOutput ?? "";
                    result["StandardError"] = standardError ?? "";
                }

                var logMessage = $"Command completed with exit code {exitCode} in {stopwatch.ElapsedMilliseconds}ms";
                if (exitCode == 0)
                {
                    Logger.LogInfo(logMessage);
                }
                else
                {
                    Logger.LogError($"{logMessage} (non-zero exit code)");
                }

                return new PluginResult() { Status = PluginStatus.Success, OutputData = result };
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2) // File not found
            {
                Logger.LogError($"Command not found: {arguments.Command} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Command not found: {arguments.Command}" };
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError($"Access denied executing command: {arguments.Command} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = $"Access denied: {ex.Message}" };
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error executing command: {arguments.Command} - {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = ex.Message };
            }
        }
    }
}
