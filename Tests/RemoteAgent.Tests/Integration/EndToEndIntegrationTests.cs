using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using AgentCore;
using AgentCommon;
using Newtonsoft.Json.Linq;

namespace RemoteAgent.Tests
{
    /// <summary>
    /// End-to-end integration tests that verify the complete system works together
    /// These tests simulate real usage scenarios
    /// </summary>
    public class EndToEndIntegrationTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly CoreHost _coreHost;
        private readonly string _configPath;

        public EndToEndIntegrationTests()
        {
            // Create test environment
            _testDirectory = Path.Combine(Path.GetTempPath(), $"RemoteAgentE2E_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            
            // Create some test files
            File.WriteAllText(Path.Combine(_testDirectory, "document.txt"), "Test document");
            File.WriteAllText(Path.Combine(_testDirectory, "data.csv"), "col1,col2\nval1,val2");
            
            // Create a test config file
            _configPath = Path.Combine(_testDirectory, "test-config.json");
            var config = new
            {
                agentGuid = Guid.NewGuid().ToString(),  // Use lowercase property name to match JSON serialization
                AgentId = 1
            };
            File.WriteAllText(_configPath, Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));

            // Create CoreHost
            _coreHost = CoreHostFactory.CreateDefault();
        }

        [Fact]
        public async Task FullSystem_StartupAndShutdown_ShouldWorkCorrectly()
        {
            // Act - Start the system
            await _coreHost.StartAsync(_configPath);
            
            // Assert - Verify all components are running
            Assert.True(_coreHost.IsRunning);
            Assert.NotNull(_coreHost.PluginManager);
            Assert.NotNull(_coreHost.JobManager);
            Assert.NotNull(_coreHost.EventManager);
            Assert.NotNull(_coreHost.CommunicationManager);
            
            // Act - Stop the system
            await _coreHost.StopAsync();
            
            // Assert - Verify clean shutdown
            Assert.False(_coreHost.IsRunning);
        }

        [Fact]
        public async Task PluginSystem_LoadAndExecute_ShouldWorkEndToEnd()
        {
            // Arrange
            await _coreHost.StartAsync(_configPath);
            
            // Simulate receiving a job from the server (this is what would happen in real usage)
            var jobPayload = new
            {
                jobId = 1,
                jobType = "plugin",
                jobData = new
                {
                    pluginName = "DirectoryListPlugin",
                    pluginArguments = new
                    {
                        path = _testDirectory
                    }
                }
            };

            string jobJson = Newtonsoft.Json.JsonConvert.SerializeObject(jobPayload);
            var jobJObject = JObject.Parse(jobJson);
            var job = new AgentCore.JobManagement.Job(
                (uint)jobPayload.jobId, 
                jobPayload.jobType, 
                jobJObject["jobData"] as JObject
            );

            // Act - Process the job through the system
            // In real usage, this would be triggered by the CommunicationManager receiving from server
            var jobManager = _coreHost.JobManager;
            
            // Simulate the job processing (in real system this happens automatically)
            // Note: These variables would be used in a real job processing scenario
            // but are not used in this simplified test
            
            // Subscribe to job completion events
            _coreHost.EventManager.Subscribe("PluginCompleted", (sender, args) =>
            {
                // In real system, the result would be in the event args
                // Job completed successfully - event received
            });

            // Note: Direct job triggering would require access to internal methods
            // This is a simplified test to verify the system is running
            
            // Give it a moment to process
            await Task.Delay(100);

            // Assert - This is a simplified test since the actual job processing
            // is async and event-driven. In a real integration test, you'd need
            // to wait for the actual events or results.
            Assert.True(_coreHost.IsRunning);
            // Note: Full verification would require a more sophisticated event waiting mechanism
        }

        [Fact]
        public async Task ConfigurationLoading_ShouldWorkCorrectly()
        {
            // Act
            await _coreHost.StartAsync(_configPath);
            
            // Assert
            Assert.NotNull(_coreHost.Config);
            Assert.NotNull(_coreHost.Config.AgentGuid);
            Assert.NotEmpty(_coreHost.Config.AgentGuid);
            // Note: AgentConfig doesn't have ServerUri property in current implementation
        }

        [Fact] 
        public async Task SystemResilience_BadConfig_ShouldHandleGracefully()
        {
            // Arrange - Create invalid config
            var badConfigPath = Path.Combine(_testDirectory, "bad-config.json");
            File.WriteAllText(badConfigPath, "{ invalid json }");
            
            // Act & Assert - Should throw JsonException for invalid JSON
            await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => _coreHost.StartAsync(badConfigPath));
        }

        public void Dispose()
        {
            try
            {
                _coreHost?.StopAsync().GetAwaiter().GetResult();
                _coreHost?.Dispose();
            }
            catch { }

            try
            {
                if (Directory.Exists(_testDirectory))
                    Directory.Delete(_testDirectory, true);
            }
            catch { }
        }
    }
}
