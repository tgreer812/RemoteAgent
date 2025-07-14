using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AgentCore.PluginManagement;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace RemoteAgent.Tests
{
    /// <summary>
    /// Integration tests for the PluginManager that test actual plugin loading
    /// These test the real plugin loading mechanism without mocks
    /// </summary>
    public class PluginManagerIntegrationTests : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly ConsoleLogger _logger;
        private readonly ITestOutputHelper _output;

        public PluginManagerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new ConsoleLogger();
            _pluginManager = new PluginManager(_logger);
        }

        [Fact]
        public void DebugPluginPath_ShouldShowCurrentDirectoryAndPluginPath()
        {
            // Debug - Check what directory we're running from
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var pluginPath = Path.Combine(baseDirectory, "CorePlugins.dll");
            var pluginExists = File.Exists(pluginPath);
            
            _output.WriteLine($"Base Directory: {baseDirectory}");
            _output.WriteLine($"Plugin Path: {pluginPath}");
            _output.WriteLine($"Plugin Exists: {pluginExists}");
            
            // List all DLL files in the base directory
            if (Directory.Exists(baseDirectory))
            {
                var dllFiles = Directory.GetFiles(baseDirectory, "*.dll");
                _output.WriteLine($"DLL files in base directory: {string.Join(", ", dllFiles.Select(Path.GetFileName))}");
            }
            
            Assert.True(pluginExists, $"CorePlugins.dll should exist at {pluginPath}");
        }

        [Fact]
        public async Task LoadCorePluginsAsync_ShouldLoadDirectoryListPlugin()
        {
            // Act
            await _pluginManager.LoadCorePluginsAsync();

            // Assert - Access the public property directly
            var loadedPlugins = _pluginManager.LoadedPlugins;

            Assert.NotNull(loadedPlugins);
            Assert.True(loadedPlugins.ContainsKey("DirectoryListPlugin"));
            
            var plugin = loadedPlugins["DirectoryListPlugin"];
            Assert.NotNull(plugin);
        }

        [Fact]
        public async Task LoadedPlugin_ShouldImplementIPluginInterface()
        {
            // Arrange
            await _pluginManager.LoadCorePluginsAsync();
            
            // Act - Get the loaded plugin
            var loadedPlugins = _pluginManager.LoadedPlugins;
            var plugin = loadedPlugins["DirectoryListPlugin"];

            // Assert
            Assert.True(plugin is IPlugin);
            
            // Test the plugin interface methods
            var loadResult = await plugin.LoadAsync();
            Assert.True(loadResult);
            
            var stopResult = await plugin.StopAsync();
            Assert.True(stopResult);
            
            var unloadResult = await plugin.UnloadAsync();
            Assert.True(unloadResult);
        }

        [Fact]
        public async Task DirectoryListPlugin_WithTestArguments_ShouldExecuteCorrectly()
        {
            // Arrange
            await _pluginManager.LoadCorePluginsAsync();
            
            var loadedPlugins = _pluginManager.LoadedPlugins;
            var plugin = loadedPlugins["DirectoryListPlugin"];

            // Create test arguments pointing to a known directory
            var args = new PluginArguments();
            args.AddArgument("path", Environment.GetFolderPath(Environment.SpecialFolder.Windows)); // Should always exist

            // Act
            var result = await plugin.StartAsync(args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PluginStatus.Success, result.Status);
            Assert.NotNull(result.OutputData);
            Assert.Contains("Directory listing for", result.OutputData.ToString());
        }

        [Fact]
        public async Task DiagnosePluginLoading_ShouldShowWhatIsLoaded()
        {
            // Act
            await _pluginManager.LoadCorePluginsAsync();

            // Debug - Access the public property directly
            var loadedPlugins = _pluginManager.LoadedPlugins;

            _output.WriteLine($"LoadedPlugins is null: {loadedPlugins == null}");
            if (loadedPlugins != null)
            {
                _output.WriteLine($"Number of loaded plugins: {loadedPlugins.Count}");
                foreach (var kvp in loadedPlugins)
                {
                    _output.WriteLine($"Plugin: {kvp.Key} -> {kvp.Value?.GetType().FullName}");
                }
            }

            // Check what types are found in the assembly
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var pluginPath = Path.Combine(baseDirectory, "CorePlugins.dll");
            if (File.Exists(pluginPath))
            {
                var assembly = System.Reflection.Assembly.LoadFrom(pluginPath);
                var allTypes = assembly.GetTypes();
                _output.WriteLine($"All types in CorePlugins.dll: {allTypes.Length}");
                foreach (var type in allTypes)
                {
                    var hasAttribute = type.GetCustomAttributes(typeof(AgentCommon.AgentPluginCommon.AgentPluginAttribute), false).Length > 0;
                    _output.WriteLine($"Type: {type.FullName}, HasAgentPluginAttribute: {hasAttribute}");
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _pluginManager?.Stop().GetAwaiter().GetResult();
            }
            catch { }
        }
    }
}
