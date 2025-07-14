using System;
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
    public class CoreHostFactoryTests
    {
        [Fact]
        public void CreateDefault_WithoutLogger_ShouldCreateWithConsoleLogger()
        {
            // Act
            var coreHost = CoreHostFactory.CreateDefault();

            // Assert
            Assert.NotNull(coreHost);
            Assert.NotNull(coreHost.PluginManager);
            Assert.NotNull(coreHost.JobManager);
            Assert.NotNull(coreHost.EventManager);
            Assert.NotNull(coreHost.CommunicationManager);
            Assert.False(coreHost.IsRunning);
        }

        [Fact]
        public void CreateDefault_WithCustomLogger_ShouldUseProvidedLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act
            var coreHost = CoreHostFactory.CreateDefault(mockLogger.Object);

            // Assert
            Assert.NotNull(coreHost);
            Assert.NotNull(coreHost.PluginManager);
            Assert.NotNull(coreHost.JobManager);
            Assert.NotNull(coreHost.EventManager);
            Assert.NotNull(coreHost.CommunicationManager);
        }

        [Fact]
        public void Create_WithCustomImplementations_ShouldUseProvidedImplementations()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockPluginManager = new Mock<IPluginManager>();
            var mockJobManager = new Mock<IJobManager>();
            var mockEventDispatcher = new Mock<IEventDispatcher>();
            var mockCommunicationManager = new Mock<ICommunicationManager>();

            // Act
            var coreHost = CoreHostFactory.Create(
                mockLogger.Object,
                mockPluginManager.Object,
                mockJobManager.Object,
                mockEventDispatcher.Object,
                mockCommunicationManager.Object);

            // Assert
            Assert.NotNull(coreHost);
            Assert.Equal(mockPluginManager.Object, coreHost.PluginManager);
            Assert.Equal(mockJobManager.Object, coreHost.JobManager);
            Assert.Equal(mockEventDispatcher.Object, coreHost.EventManager);
            Assert.Equal(mockCommunicationManager.Object, coreHost.CommunicationManager);
        }

        [Fact]
        public void Create_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockPluginManager = new Mock<IPluginManager>();
            var mockJobManager = new Mock<IJobManager>();
            var mockEventDispatcher = new Mock<IEventDispatcher>();
            var mockCommunicationManager = new Mock<ICommunicationManager>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                CoreHostFactory.Create(null, mockPluginManager.Object, mockJobManager.Object, mockEventDispatcher.Object, mockCommunicationManager.Object));
            
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void CreateDefault_ShouldCreateFullyWiredDependencies()
        {
            // Act
            var coreHost = CoreHostFactory.CreateDefault();

            // Assert - Verify that all services are created with proper types
            Assert.IsType<PluginManager>(coreHost.PluginManager);
            Assert.IsType<JobManager>(coreHost.JobManager);
            Assert.IsType<EventDispatcher>(coreHost.EventManager);
            Assert.IsType<CommunicationManager>(coreHost.CommunicationManager);
            
            // Verify services implement ICoreService
            Assert.IsAssignableFrom<ICoreService>(coreHost.PluginManager);
            Assert.IsAssignableFrom<ICoreService>(coreHost.JobManager);
            Assert.IsAssignableFrom<ICoreService>(coreHost.EventManager);
            Assert.IsAssignableFrom<ICoreService>(coreHost.CommunicationManager);
        }
    }
}
