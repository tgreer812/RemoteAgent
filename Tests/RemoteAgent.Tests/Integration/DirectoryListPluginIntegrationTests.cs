using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using AgentCore;
using AgentCore.PluginManagement;
using AgentCore.JobManagement;
using AgentCore.EventManagement;
using AgentCore.CommunicationManagement;
using AgentCommon;
using AgentCommon.AgentPluginCommon;
using Newtonsoft.Json.Linq;

namespace RemoteAgent.Tests
{
    /// <summary>
    /// Integration tests that verify DirectoryListPlugin works end-to-end
    /// These tests use real components (not mocks) to verify actual behavior
    /// </summary>
    public class DirectoryListPluginIntegrationTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testFile1;
        private readonly string _testFile2;
        private readonly CoreHost _coreHost;

        public DirectoryListPluginIntegrationTests()
        {
            // Create a temporary test directory with some files
            _testDirectory = Path.Combine(Path.GetTempPath(), $"RemoteAgentTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            
            _testFile1 = Path.Combine(_testDirectory, "test1.txt");
            _testFile2 = Path.Combine(_testDirectory, "test2.log");
            File.WriteAllText(_testFile1, "Test content 1");
            File.WriteAllText(_testFile2, "Test content 2");

            // Create a real CoreHost with real components (no mocks)
            _coreHost = CoreHostFactory.CreateDefault();
        }

        [Fact]
        public async Task DirectoryListPlugin_WithValidPath_ShouldReturnFileList()
        {
            // Arrange - Start the core host so plugins are loaded
            await _coreHost.StartAsync();
            
            // Create plugin arguments for directory listing
            var pluginArgs = new PluginArguments();
            pluginArgs.AddArgument("path", _testDirectory);

            // Act
            var plugin = GetLoadedPlugin("DirectoryListPlugin");
            var result = await plugin.StartAsync(pluginArgs);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PluginStatus.Success, result.Status);
            Assert.NotNull(result.OutputData);
            var outputText = result.OutputData.ToString();
            Assert.Contains(Path.GetFileName(_testFile1), outputText);
            Assert.Contains(Path.GetFileName(_testFile2), outputText);
            Assert.Contains(_testDirectory, outputText);
        }

        [Fact]
        public async Task DirectoryListPlugin_WithInvalidPath_ShouldReturnFailure()
        {
            // Arrange
            await _coreHost.StartAsync();
            
            var pluginArgs = new PluginArguments();
            pluginArgs.AddArgument("path", @"C:\ThisDirectoryDoesNotExist\Invalid\Path");
            
            // Act
            var plugin = GetLoadedPlugin("DirectoryListPlugin");
            var result = await plugin.StartAsync(pluginArgs);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PluginStatus.Failed, result.Status);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("Path does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task DirectoryListPlugin_WithEmptyPath_ShouldReturnFailure()
        {
            // Arrange
            await _coreHost.StartAsync();
            
            var pluginArgs = new PluginArguments();
            // Don't add path argument (or add empty one)
            
            // Act
            var plugin = GetLoadedPlugin("DirectoryListPlugin");
            var result = await plugin.StartAsync(pluginArgs);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PluginStatus.Failed, result.Status);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("Path argument is required", result.ErrorMessage);
        }

        [Fact]
        public async Task PluginManager_ShouldLoadDirectoryListPluginOnStartup()
        {
            // Arrange & Act
            await _coreHost.StartAsync();

            // Assert
            var pluginManager = _coreHost.PluginManager;
            
            // Access the LoadedPlugins public property directly by casting to PluginManager
            var pluginManagerConcrete = pluginManager as PluginManager;
            Assert.NotNull(pluginManagerConcrete);
            var loadedPlugins = pluginManagerConcrete.LoadedPlugins;
            
            Assert.NotNull(loadedPlugins);
            Assert.True(loadedPlugins.ContainsKey("DirectoryListPlugin"));
        }

        /// <summary>
        /// Helper method to get a loaded plugin from the PluginManager
        /// </summary>
        private IPlugin GetLoadedPlugin(string pluginName)
        {
            var pluginManager = _coreHost.PluginManager;
            
            // Cast to PluginManager to access LoadedPlugins public property
            var pluginManagerConcrete = pluginManager as PluginManager;
            if (pluginManagerConcrete?.LoadedPlugins != null && 
                pluginManagerConcrete.LoadedPlugins.TryGetValue(pluginName, out var plugin))
            {
                return plugin;
            }
            
            throw new InvalidOperationException($"Plugin '{pluginName}' not found in loaded plugins");
        }

        public void Dispose()
        {
            // Clean up test files and directory
            try
            {
                if (File.Exists(_testFile1)) File.Delete(_testFile1);
                if (File.Exists(_testFile2)) File.Delete(_testFile2);
                if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }

            // Clean up CoreHost
            _coreHost?.StopAsync().GetAwaiter().GetResult();
            _coreHost?.Dispose();
        }
    }
}
