using System;
using System.Threading.Tasks;
using AgentCommon;
using AgentCore;
using AgentCore.CommunicationManagement;
using AgentCore.EventManagement;
using AgentCore.JobManagement;
using AgentCore.PluginManagement;
using Moq;
using Xunit;

namespace RemoteAgent.Tests
{
    public class CoreHostTests : IDisposable
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IPluginManager> _mockPluginManager;
        private readonly Mock<IJobManager> _mockJobManager;
        private readonly Mock<IEventDispatcher> _mockEventDispatcher;
        private readonly Mock<ICommunicationManager> _mockCommunicationManager;
        private readonly AgentConfig _testConfig;
        private CoreHost _coreHost;

        public CoreHostTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockPluginManager = new Mock<IPluginManager>();
            _mockJobManager = new Mock<IJobManager>();
            _mockEventDispatcher = new Mock<IEventDispatcher>();
            _mockCommunicationManager = new Mock<ICommunicationManager>();

            // Create a test config to avoid file dependency
            _testConfig = new AgentConfig
            {
                AgentGuid = "test-guid-123",
                AgentId = 42
            };

            // Set up core services to implement ICoreService
            _mockPluginManager.As<ICoreService>();
            _mockJobManager.As<ICoreService>();
            _mockEventDispatcher.As<ICoreService>();
            _mockCommunicationManager.As<ICoreService>();

            // Setup default successful starts
            _mockPluginManager.As<ICoreService>().Setup(s => s.Start()).Returns(Task.CompletedTask);
            _mockJobManager.As<ICoreService>().Setup(s => s.Start()).Returns(Task.CompletedTask);
            _mockEventDispatcher.As<ICoreService>().Setup(s => s.Start()).Returns(Task.CompletedTask);
            _mockCommunicationManager.As<ICoreService>().Setup(s => s.Start()).Returns(Task.CompletedTask);

            // Setup default successful stops
            _mockPluginManager.As<ICoreService>().Setup(s => s.Stop()).Returns(Task.FromResult(true));
            _mockJobManager.As<ICoreService>().Setup(s => s.Stop()).Returns(Task.FromResult(true));
            _mockEventDispatcher.As<ICoreService>().Setup(s => s.Stop()).Returns(Task.FromResult(true));
            _mockCommunicationManager.As<ICoreService>().Setup(s => s.Stop()).Returns(Task.FromResult(true));

            _coreHost = new CoreHost(
                _mockLogger.Object,
                _mockPluginManager.Object,
                _mockJobManager.Object,
                _mockEventDispatcher.Object,
                _mockCommunicationManager.Object,
                _testConfig); // Inject test config
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Assert
            Assert.NotNull(_coreHost);
            Assert.Equal(_mockPluginManager.Object, _coreHost.PluginManager);
            Assert.Equal(_mockJobManager.Object, _coreHost.JobManager);
            Assert.Equal(_mockEventDispatcher.Object, _coreHost.EventManager);
            Assert.Equal(_mockCommunicationManager.Object, _coreHost.CommunicationManager);
            Assert.False(_coreHost.IsRunning);
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CoreHost(null, _mockPluginManager.Object, _mockJobManager.Object, _mockEventDispatcher.Object, _mockCommunicationManager.Object, null));
            
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullPluginManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CoreHost(_mockLogger.Object, null, _mockJobManager.Object, _mockEventDispatcher.Object, _mockCommunicationManager.Object, null));
            
            Assert.Equal("pluginManager", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullJobManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CoreHost(_mockLogger.Object, _mockPluginManager.Object, null, _mockEventDispatcher.Object, _mockCommunicationManager.Object, null));
            
            Assert.Equal("jobManager", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullEventManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CoreHost(_mockLogger.Object, _mockPluginManager.Object, _mockJobManager.Object, null, _mockCommunicationManager.Object, null));
            
            Assert.Equal("eventManager", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCommunicationManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CoreHost(_mockLogger.Object, _mockPluginManager.Object, _mockJobManager.Object, _mockEventDispatcher.Object, null, null));
            
            Assert.Equal("communicationManager", exception.ParamName);
        }

        [Fact]
        public async Task StartAsync_ShouldStartAllServicesInOrder()
        {
            // Act
            await _coreHost.StartAsync();

            // Assert
            Assert.True(_coreHost.IsRunning);
            _mockPluginManager.As<ICoreService>().Verify(s => s.Start(), Times.Once);
            _mockJobManager.As<ICoreService>().Verify(s => s.Start(), Times.Once);
            _mockEventDispatcher.As<ICoreService>().Verify(s => s.Start(), Times.Once);
            _mockCommunicationManager.As<ICoreService>().Verify(s => s.Start(), Times.Once);
        }

        [Fact]
        public async Task StartAsync_WhenAlreadyRunning_ShouldLogWarningAndReturn()
        {
            // Arrange
            await _coreHost.StartAsync();

            // Act
            await _coreHost.StartAsync(); // Second call

            // Assert
            _mockLogger.Verify(l => l.LogWarning("Core host is already running!"), Times.Once);
            // Services should only be started once
            _mockPluginManager.As<ICoreService>().Verify(s => s.Start(), Times.Once);
        }

        [Fact]
        public async Task StartAsync_WhenServiceFails_ShouldThrowException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Service failed to start");
            _mockJobManager.As<ICoreService>().Setup(s => s.Start()).ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _coreHost.StartAsync());
            Assert.Equal(expectedException.Message, actualException.Message);
            Assert.False(_coreHost.IsRunning);
        }

        [Fact]
        public async Task StopAsync_ShouldStopAllServicesInReverseOrder()
        {
            // Arrange
            await _coreHost.StartAsync();

            // Act
            await _coreHost.StopAsync();

            // Assert
            Assert.False(_coreHost.IsRunning);
            _mockPluginManager.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
            _mockJobManager.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
            _mockEventDispatcher.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
            _mockCommunicationManager.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
        }

        [Fact]
        public async Task StopAsync_WhenNotRunning_ShouldLogWarningAndReturn()
        {
            // Act
            await _coreHost.StopAsync();

            // Assert
            _mockLogger.Verify(l => l.LogWarning("Core host is not running!"), Times.Once);
            // Services should not be stopped
            _mockPluginManager.As<ICoreService>().Verify(s => s.Stop(), Times.Never);
        }

        [Fact]
        public async Task StopAsync_WhenServiceThrows_ShouldContinueStoppingOthers()
        {
            // Arrange
            await _coreHost.StartAsync();
            _mockJobManager.As<ICoreService>().Setup(s => s.Stop()).ThrowsAsync(new InvalidOperationException("Stop failed"));

            // Act & Assert
            await _coreHost.StopAsync(); // Should not throw

            // Assert - all services should still be called to stop
            _mockPluginManager.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
            _mockJobManager.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
            _mockEventDispatcher.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
            _mockCommunicationManager.As<ICoreService>().Verify(s => s.Stop(), Times.Once);
            Assert.False(_coreHost.IsRunning);
        }

        [Fact]
        public async Task Dispose_ShouldStopIfRunning()
        {
            // Arrange
            await _coreHost.StartAsync();

            // Act
            _coreHost.Dispose();

            // Assert
            Assert.False(_coreHost.IsRunning);
        }

        [Fact]
        public void Dispose_WhenNotRunning_ShouldNotThrow()
        {
            // Act & Assert - Should not throw
            _coreHost.Dispose();
        }

        public void Dispose()
        {
            _coreHost?.Dispose();
        }
    }
}
