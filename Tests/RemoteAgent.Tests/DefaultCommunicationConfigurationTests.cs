using System;
using AgentCore.CommunicationManagement;
using AgentCore;
using Xunit;

namespace RemoteAgent.Tests
{
    public class DefaultCommunicationConfigurationTests
    {
        [Fact]
        public void Constructor_WithValidConfig_ShouldSetProperties()
        {
            // Arrange
            var agentConfig = new AgentConfig();

            // Act
            var config = new DefaultCommunicationConfiguration(agentConfig);

            // Assert
            Assert.Equal("http://localhost:5148", config.ServerAddress);
            Assert.Equal("api/tasking", config.TaskingEndpoint);
            Assert.Equal("api/job", config.JobEndpoint);
            Assert.Equal("api/agent/hello", config.AgentHelloEndpoint);
            Assert.Equal(TimeSpan.FromSeconds(30), config.RequestTimeout);
            Assert.Equal(TimeSpan.FromSeconds(30), config.HandshakeRetryDelay);
            Assert.Equal(5, config.MaxHandshakeRetries);
            Assert.NotEqual(Guid.Empty, config.AgentGuid);
        }

        [Fact]
        public void Constructor_WithNullConfig_ShouldUseDefaults()
        {
            // Act
            var config = new DefaultCommunicationConfiguration(null);

            // Assert
            Assert.Equal("http://localhost:5148", config.ServerAddress);
            Assert.Equal("api/tasking", config.TaskingEndpoint);
            Assert.Equal("api/job", config.JobEndpoint);
            Assert.Equal("api/agent/hello", config.AgentHelloEndpoint);
            Assert.Equal(TimeSpan.FromSeconds(30), config.RequestTimeout);
            Assert.Equal(TimeSpan.FromSeconds(30), config.HandshakeRetryDelay);
            Assert.Equal(5, config.MaxHandshakeRetries);
            Assert.NotEqual(Guid.Empty, config.AgentGuid);
        }

        [Fact]
        public void AgentId_ShouldBeSettable()
        {
            // Arrange
            var config = new DefaultCommunicationConfiguration();
            var agentId = 12345;

            // Act
            config.AgentId = agentId;

            // Assert
            Assert.Equal(agentId, config.AgentId);
        }

        [Fact]
        public void AgentId_InitialValue_ShouldBeNull()
        {
            // Arrange & Act
            var config = new DefaultCommunicationConfiguration();

            // Assert
            Assert.Null(config.AgentId);
        }

        [Fact]
        public void AgentGuid_ShouldBeConsistentAcrossAccesses()
        {
            // Arrange
            var config = new DefaultCommunicationConfiguration();

            // Act
            var guid1 = config.AgentGuid;
            var guid2 = config.AgentGuid;

            // Assert
            Assert.Equal(guid1, guid2);
            Assert.NotEqual(Guid.Empty, guid1);
        }

        [Fact]
        public void Constructor_WithDefaultParameters_ShouldUseExpectedDefaults()
        {
            // Act
            var config = new DefaultCommunicationConfiguration();

            // Assert
            Assert.Equal("http://localhost:5148", config.ServerAddress);
            Assert.Equal("api/tasking", config.TaskingEndpoint);
            Assert.Equal("api/job", config.JobEndpoint);
            Assert.Equal("api/agent/hello", config.AgentHelloEndpoint);
            Assert.Equal(TimeSpan.FromSeconds(30), config.RequestTimeout);
            Assert.Equal(TimeSpan.FromSeconds(30), config.HandshakeRetryDelay);
            Assert.Equal(5, config.MaxHandshakeRetries);
        }
    }
}
