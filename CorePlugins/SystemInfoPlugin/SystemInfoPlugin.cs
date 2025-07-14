using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace CorePlugins.SystemInfoPlugin
{
    [AgentPlugin("SystemInfoPlugin")]
    internal class SystemInfoPlugin : PluginBase
    {
        public SystemInfoPlugin(PluginContext context) : base(context)
        {
        }

        public override async Task<bool> LoadAsync(PluginArguments? agentPluginArguments = null)
        {
            Logger.LogInfo("SystemInfoPlugin loaded");
            return await Task.FromResult(true);
        }

        public override async Task<PluginResult> StartAsync(PluginArguments? args = null, CancellationToken cancellationToken = default)
        {
            var arguments = new SystemInfoArguments(args ?? new PluginArguments());

            try
            {
                Logger.LogInfo("Gathering system information");

                var result = new Dictionary<string, object>();

                // Basic system info
                result["MachineName"] = Environment.MachineName;
                result["OSVersion"] = Environment.OSVersion.ToString();
                result["Platform"] = Environment.OSVersion.Platform.ToString();
                result["Is64BitOperatingSystem"] = Environment.Is64BitOperatingSystem;
                result["Is64BitProcess"] = Environment.Is64BitProcess;
                result["ProcessorCount"] = Environment.ProcessorCount;
                result["UserName"] = Environment.UserName;
                result["UserDomainName"] = Environment.UserDomainName;
                result["WorkingSet"] = Environment.WorkingSet;
                result["SystemPageSize"] = Environment.SystemPageSize;
                
                // Runtime info
                result["RuntimeVersion"] = Environment.Version.ToString();
                result["Framework"] = RuntimeInformation.FrameworkDescription;
                result["Architecture"] = RuntimeInformation.OSArchitecture.ToString();
                result["RuntimeIdentifier"] = RuntimeInformation.RuntimeIdentifier;

                // Memory info
                var gc = GC.GetTotalMemory(false);
                result["ManagedMemoryUsage"] = gc;

                // Drive info
                var drives = await Task.Run(() => 
                {
                    try
                    {
                        return System.IO.DriveInfo.GetDrives()
                            .Where(d => d.IsReady)
                            .Select(d => new Dictionary<string, object>
                            {
                                ["Name"] = d.Name,
                                ["DriveType"] = d.DriveType.ToString(),
                                ["FileSystem"] = d.DriveFormat,
                                ["TotalSize"] = d.TotalSize,
                                ["AvailableSpace"] = d.AvailableFreeSpace,
                                ["UsedSpace"] = d.TotalSize - d.AvailableFreeSpace
                            }).ToList();
                    }
                    catch
                    {
                        return new List<Dictionary<string, object>>();
                    }
                }, cancellationToken);
                result["Drives"] = drives;

                // Environment variables (if requested)
                if (arguments.IncludeEnvironmentVariables)
                {
                    var envVars = new Dictionary<string, string>();
                    foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
                    {
                        envVars[entry.Key?.ToString() ?? ""] = entry.Value?.ToString() ?? "";
                    }
                    result["EnvironmentVariables"] = envVars;
                }

                // Network info (if requested)
                if (arguments.IncludeNetworkInfo)
                {
                    var networkInterfaces = await Task.Run(() =>
                    {
                        try
                        {
                            return NetworkInterface.GetAllNetworkInterfaces()
                                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                                .Select(ni => new Dictionary<string, object>
                                {
                                    ["Name"] = ni.Name,
                                    ["Description"] = ni.Description,
                                    ["NetworkInterfaceType"] = ni.NetworkInterfaceType.ToString(),
                                    ["Speed"] = ni.Speed,
                                    ["PhysicalAddress"] = ni.GetPhysicalAddress().ToString(),
                                    ["IPAddresses"] = ni.GetIPProperties().UnicastAddresses
                                        .Select(addr => addr.Address.ToString()).ToList()
                                }).ToList();
                        }
                        catch
                        {
                            return new List<Dictionary<string, object>>();
                        }
                    }, cancellationToken);
                    result["NetworkInterfaces"] = networkInterfaces;
                }

                // Timestamp
                result["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                Logger.LogInfo("Successfully gathered system information");
                return new PluginResult() { Status = PluginStatus.Success, OutputData = result };
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error gathering system information: {ex.Message}");
                return new PluginResult() { Status = PluginStatus.Failed, ErrorMessage = ex.Message };
            }
        }
    }
}
