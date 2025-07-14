using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AgentCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteAgent.Tests.Integration
{
    /// <summary>
    /// True end-to-end integration test that spins up a real C# test server and tests
    /// the complete agent workflow over HTTP without Python dependency
    /// </summary>
    public class HttpEndToEndIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _testDirectory;
        private readonly string _configPath;
        private readonly CoreHost _coreHost;
        private Process _serverProcess;
        private readonly HttpClient _httpClient;
        private readonly int _serverPort = 5148;
        private readonly string _serverUrl;

        public HttpEndToEndIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _serverUrl = $"http://localhost:{_serverPort}";
            _httpClient = new HttpClient();

            // Create test environment - use a well-known location that the C# server can use
            _testDirectory = Path.Combine(Path.GetTempPath(), "RemoteAgentE2ETest");
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
            Directory.CreateDirectory(_testDirectory);
            
            // Create some test files for the DirectoryListPlugin to find
            File.WriteAllText(Path.Combine(_testDirectory, "test1.txt"), "Test file 1");
            File.WriteAllText(Path.Combine(_testDirectory, "test2.txt"), "Test file 2");
            Directory.CreateDirectory(Path.Combine(_testDirectory, "subfolder"));
            File.WriteAllText(Path.Combine(_testDirectory, "subfolder", "test3.txt"), "Test file 3");
            
            // Create test config with just the properties that are actually used
            _configPath = Path.Combine(_testDirectory, "test-config.json");
            var config = new
            {
                agentGuid = Guid.NewGuid().ToString(),
                AgentId = 1
            };
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(config, Formatting.Indented));

            // Create CoreHost
            _coreHost = CoreHostFactory.CreateDefault();
        }

        [Fact]
        public async Task FullHttpWorkflow_AgentRegistrationAndJobExecution_ShouldWorkEndToEnd()
        {
            // Arrange - Start the C2 server
            await StartC2Server();
            
            // Wait for server to be ready
            await WaitForServerReady();
            
            // Act - Start the agent
            _output.WriteLine("Starting agent...");
            await _coreHost.StartAsync(_configPath);
            
            // Wait a moment for the agent to register and receive tasking
            _output.WriteLine("Waiting for agent to register and receive tasking...");
            await Task.Delay(5000); // Give it time to complete the workflow
            
            // Verify the server received communication from agent
            var serverLogs = await GetServerLogs();
            _output.WriteLine($"Server logs: {serverLogs}");
            
            var logsData = JObject.Parse(serverLogs);
            
            // Check that we have at least one agent registered
            var agents = logsData["agents"] as JObject;
            Assert.NotNull(agents);
            Assert.True(agents.Count > 0, "No agents registered with server");
            
            // Check that we have completed jobs
            var completedJobs = logsData["completed_jobs"] as JObject;
            Assert.NotNull(completedJobs);
            Assert.True(completedJobs.Count > 0, "No jobs completed");
            
            // Stop the agent cleanly
            await _coreHost.StopAsync();
            Assert.False(_coreHost.IsRunning);
        }

        [Fact]
        public async Task AgentResilience_ServerDownThenUp_ShouldRecoverAndContinue()
        {
            // Arrange - Start agent without server (should handle gracefully)
            _output.WriteLine("Starting agent with no server running...");
            await _coreHost.StartAsync(_configPath);
            
            // Wait a moment - agent should be trying to connect but failing gracefully
            await Task.Delay(2000);
            Assert.True(_coreHost.IsRunning); // Agent should still be running despite connection failures
            
            // Act - Now start the server
            _output.WriteLine("Starting server after agent is already running...");
            await StartC2Server();
            await WaitForServerReady();
            
            // Wait for agent to connect and complete workflow
            _output.WriteLine("Waiting for agent to connect to newly started server...");
            await Task.Delay(5000);
            
            // Assert - Verify the workflow completed
            var serverLogs = await GetServerLogs();
            _output.WriteLine($"Server logs: {serverLogs}");
            
            var logsData = JObject.Parse(serverLogs);
            var agents = logsData["agents"] as JObject;
            var completedJobs = logsData["completed_jobs"] as JObject;
            
            Assert.NotNull(agents);
            Assert.True(agents.Count > 0, "Agent should have registered");
            Assert.NotNull(completedJobs);
            Assert.True(completedJobs.Count > 0, "Jobs should have completed");
            
            await _coreHost.StopAsync();
        }

        [Fact]
        public async Task DirectoryListJob_WithRealFiles_ShouldReturnCorrectResults()
        {
            // Arrange
            await StartC2Server();
            await WaitForServerReady();
            
            // Start agent
            await _coreHost.StartAsync(_configPath);
            
            // Wait for job execution
            await Task.Delay(5000);
            
            // Get the job results from server
            var serverLogs = await GetServerLogs();
            _output.WriteLine($"Full server output:\n{serverLogs}");
            
            var logsData = JObject.Parse(serverLogs);
            var completedJobs = logsData["completed_jobs"] as JObject;
            Assert.NotNull(completedJobs);
            Assert.True(completedJobs.Count > 0, "Should have completed jobs");
            
            // Get the first completed job result
            var firstJobResult = completedJobs.First?.First as JObject;
            Assert.NotNull(firstJobResult);
            
            var outputData = firstJobResult["OutputData"]?.ToString();
            Assert.NotNull(outputData);
            
            // Verify the job result contains our test files
            Assert.Contains("test1.txt", outputData);
            Assert.Contains("test2.txt", outputData);
            Assert.Contains("subfolder", outputData);
            
            // Verify it shows success - plugin result uses Status field with value 1 for success
            var status = firstJobResult["Status"]?.Value<int>();
            Assert.True(status == 1, $"Expected Status=1 (success), but got {status}");
            
            await _coreHost.StopAsync();
        }

        private async Task StartC2Server()
        {
            try
            {
                // Find the C# test server executable
                var currentDir = Directory.GetCurrentDirectory();
                var testsDir = currentDir;
                
                // Navigate up to find the Tests directory 
                while (!Directory.Exists(Path.Combine(testsDir, "TestServer.CSharp")) && 
                       Directory.GetParent(testsDir) != null)
                {
                    testsDir = Directory.GetParent(testsDir).FullName;
                }
                
                var serverProjectDir = Path.Combine(testsDir, "TestServer.CSharp");
                if (!Directory.Exists(serverProjectDir))
                {
                    throw new DirectoryNotFoundException($"Could not find TestServer.CSharp directory. Searched from {currentDir} up to {testsDir}");
                }

                _output.WriteLine($"Starting C# test server from: {serverProjectDir}");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{serverProjectDir}\" -- {_serverPort}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = serverProjectDir
                };

                _serverProcess = Process.Start(startInfo);
                if (_serverProcess == null)
                {
                    throw new InvalidOperationException("Failed to start server process");
                }

                _output.WriteLine($"Started C# test server process with PID: {_serverProcess.Id}");
                
                // Capture server output for debugging
                var outputTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!_serverProcess.StandardOutput.EndOfStream)
                        {
                            var line = await _serverProcess.StandardOutput.ReadLineAsync();
                            if (line != null)
                                _output.WriteLine($"[C# SERVER] {line}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"[C# SERVER ERROR] {ex.Message}");
                    }
                });
                
                // Also capture error output
                var errorTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!_serverProcess.StandardError.EndOfStream)
                        {
                            var line = await _serverProcess.StandardError.ReadLineAsync();
                            if (line != null)
                                _output.WriteLine($"[C# SERVER ERROR] {line}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"[C# SERVER STDERR] {ex.Message}");
                    }
                });
                
                // Give the server a moment to start
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to start C# test server: {ex.Message}");
                throw;
            }
        }

        private async Task WaitForServerReady()
        {
            _output.WriteLine("Waiting for C# test server to be ready...");
            
            for (int i = 0; i < 30; i++) // Wait up to 30 seconds
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{_serverUrl}/api/test");
                    if (response.IsSuccessStatusCode)
                    {
                        _output.WriteLine("C# test server is ready!");
                        return;
                    }
                }
                catch (HttpRequestException)
                {
                    // Server not ready yet
                }
                
                await Task.Delay(1000);
                _output.WriteLine($"Waiting for C# test server... attempt {i + 1}/30");
            }
            
            throw new TimeoutException("C# test server did not become ready within 30 seconds");
        }

        private async Task<string> GetServerLogs()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_serverUrl}/api/logs");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    return $"Failed to get logs: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Error getting server logs: {ex.Message}";
            }
        }

        public void Dispose()
        {
            try
            {
                _coreHost?.StopAsync().GetAwaiter().GetResult();
                _coreHost?.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error stopping CoreHost: {ex.Message}");
            }

            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _output.WriteLine("Stopping C# test server process...");
                    _serverProcess.Kill();
                    _serverProcess.WaitForExit(5000);
                    _serverProcess.Dispose();
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error stopping C# test server: {ex.Message}");
            }

            try
            {
                _httpClient?.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error disposing HttpClient: {ex.Message}");
            }

            try
            {
                if (Directory.Exists(_testDirectory))
                    Directory.Delete(_testDirectory, true);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error cleaning up test directory: {ex.Message}");
            }
        }
    }
}
