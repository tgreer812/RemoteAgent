using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AgentCommon;
using AgentCore.CommunicationManagement;
using AgentCore.EventManagement;
using Moq;
using Xunit;

namespace RemoteAgent.Tests
{
    public class CommunicationManagerTests : IDisposable
    {
        private readonly Mock<IHttpClientWrapper> _mockHttpClient;
        private readonly Mock<ICommunicationConfiguration> _mockConfig;
        private readonly Mock<IEventDispatcher> _mockEventDispatcher;
        private readonly Mock<IMessageSerializer> _mockMessageSerializer;
        private readonly Mock<ILogger> _mockLogger;
        private readonly CommunicationManager _communicationManager;

        public CommunicationManagerTests()
        {
            _mockHttpClient = new Mock<IHttpClientWrapper>();
            _mockConfig = new Mock<ICommunicationConfiguration>();
            _mockEventDispatcher = new Mock<IEventDispatcher>();
            _mockMessageSerializer = new Mock<IMessageSerializer>();
            _mockLogger = new Mock<ILogger>();

            // Setup default configuration values
            _mockConfig.Setup(c => c.ServerAddress).Returns("http://test-server.com");
            _mockConfig.Setup(c => c.TaskingEndpoint).Returns("api/tasking");
            _mockConfig.Setup(c => c.JobEndpoint).Returns("api/job");
            _mockConfig.Setup(c => c.AgentHelloEndpoint).Returns("api/hello");
            _mockConfig.Setup(c => c.RequestTimeout).Returns(TimeSpan.FromSeconds(30));
            _mockConfig.Setup(c => c.HandshakeRetryDelay).Returns(TimeSpan.FromSeconds(5));
            _mockConfig.Setup(c => c.MaxHandshakeRetries).Returns(3);
            _mockConfig.Setup(c => c.AgentGuid).Returns(Guid.NewGuid());
            _mockConfig.Setup(c => c.AgentId).Returns((int?)null);

            _communicationManager = new CommunicationManager(
                _mockLogger.Object,
                _mockHttpClient.Object,
                _mockConfig.Object,
                _mockEventDispatcher.Object,
                _mockMessageSerializer.Object);
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert - Constructor should complete without exception
            Assert.NotNull(_communicationManager);
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CommunicationManager(null, _mockHttpClient.Object, _mockConfig.Object, _mockEventDispatcher.Object, _mockMessageSerializer.Object));
            
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CommunicationManager(_mockLogger.Object, null, _mockConfig.Object, _mockEventDispatcher.Object, _mockMessageSerializer.Object));
            
            Assert.Equal("httpClient", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CommunicationManager(_mockLogger.Object, _mockHttpClient.Object, null, _mockEventDispatcher.Object, _mockMessageSerializer.Object));
            
            Assert.Equal("config", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullMessageSerializer_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CommunicationManager(_mockLogger.Object, _mockHttpClient.Object, _mockConfig.Object, _mockEventDispatcher.Object, null));
            
            Assert.Equal("messageSerializer", exception.ParamName);
        }

        [Fact]
        public void IsRunning_WhenNotStarted_ShouldReturnFalse()
        {
            // Assert
            Assert.False(_communicationManager.IsRunning);
        }

        [Fact]
        public async Task Start_WithSuccessfulHandshake_ShouldSetIsRunningToTrue()
        {
            // Arrange
            var successResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"agentId\": 12345}")
            };
            _mockHttpClient.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                          .ReturnsAsync(successResponse);

            try
            {
                // Act
                await _communicationManager.Start();

                // Assert
                Assert.True(_communicationManager.IsRunning);
            }
            finally
            {
                // Cleanup - Stop the communication manager to prevent hanging
                await _communicationManager.Stop();
                _communicationManager.Dispose();
            }
        }

        [Fact]
        public void Dispose_ShouldDisposeHttpClient()
        {
            // Act
            _communicationManager.Dispose();

            // Assert
            _mockHttpClient.Verify(h => h.Dispose(), Times.Once);
        }

        public void Dispose()
        {
            // Ensure the communication manager is properly disposed to prevent hanging tests
            _communicationManager?.Dispose();
        }
    }
}
