using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AgentCommon;
using AgentCore.EventManagement;

namespace RemoteAgent.Tests
{
    public class EventDispatcherTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly EventDispatcher _eventDispatcher;

        public EventDispatcherTests()
        {
            _mockLogger = new Mock<ILogger>();
            _eventDispatcher = new EventDispatcher(_mockLogger.Object);
        }

        [Fact]
        public async Task Start_ShouldCompleteSuccessfully()
        {
            // Act
            await _eventDispatcher.Start();

            // Assert
            _mockLogger.Verify(l => l.LogInfo("EventDispatcher is starting..."), Times.Once);
            _mockLogger.Verify(l => l.LogInfo("EventDispatcher started."), Times.Once);
        }

        [Fact]
        public async Task Stop_ShouldReturnTrueAndClearHandlers()
        {
            // Act
            var result = await _eventDispatcher.Stop();

            // Assert
            Assert.True(result);
            _mockLogger.Verify(l => l.LogInfo("EventDispatcher is stopping..."), Times.Once);
            _mockLogger.Verify(l => l.LogInfo("EventDispatcher stopped."), Times.Once);
        }

        [Fact]
        public void Subscribe_SyncHandler_ShouldAddHandler()
        {
            // Arrange
            var handlerCalled = false;
            void Handler(object sender, EventArgs e) => handlerCalled = true;

            // Act
            _eventDispatcher.Subscribe("test", Handler);
            _eventDispatcher.Publish("test", this, EventArgs.Empty);

            // Assert
            Assert.True(handlerCalled);
        }

        [Fact]
        public async Task Subscribe_AsyncHandler_ShouldAddHandler()
        {
            // Arrange
            var handlerCalled = false;
            Task AsyncHandler(object sender, EventArgs e)
            {
                handlerCalled = true;
                return Task.CompletedTask;
            }

            // Act
            _eventDispatcher.Subscribe("test", AsyncHandler);
            await _eventDispatcher.PublishAsync("test", this, EventArgs.Empty);

            // Assert
            Assert.True(handlerCalled);
        }

        [Fact]
        public void Unsubscribe_SyncHandler_ShouldRemoveHandler()
        {
            // Arrange
            var handlerCallCount = 0;
            void Handler(object sender, EventArgs e) => handlerCallCount++;

            _eventDispatcher.Subscribe("test", Handler);
            _eventDispatcher.Publish("test", this, EventArgs.Empty);
            Assert.Equal(1, handlerCallCount);

            // Act
            _eventDispatcher.Unsubscribe("test", Handler);
            _eventDispatcher.Publish("test", this, EventArgs.Empty);

            // Assert
            Assert.Equal(1, handlerCallCount); // Should not increase
        }

        [Fact]
        public async Task Unsubscribe_AsyncHandler_ShouldRemoveHandler()
        {
            // Arrange
            var handlerCallCount = 0;
            Task AsyncHandler(object sender, EventArgs e)
            {
                handlerCallCount++;
                return Task.CompletedTask;
            }

            _eventDispatcher.Subscribe("test", AsyncHandler);
            await _eventDispatcher.PublishAsync("test", this, EventArgs.Empty);
            Assert.Equal(1, handlerCallCount);

            // Act
            _eventDispatcher.Unsubscribe("test", AsyncHandler);
            await _eventDispatcher.PublishAsync("test", this, EventArgs.Empty);

            // Assert
            Assert.Equal(1, handlerCallCount); // Should not increase
        }

        [Fact]
        public void Publish_WithMultipleHandlers_ShouldCallAllHandlers()
        {
            // Arrange
            var handler1Called = false;
            var handler2Called = false;
            void Handler1(object sender, EventArgs e) => handler1Called = true;
            void Handler2(object sender, EventArgs e) => handler2Called = true;

            // Act
            _eventDispatcher.Subscribe("test", Handler1);
            _eventDispatcher.Subscribe("test", Handler2);
            _eventDispatcher.Publish("test", this, EventArgs.Empty);

            // Assert
            Assert.True(handler1Called);
            Assert.True(handler2Called);
        }

        [Fact]
        public async Task PublishAsync_WithMultipleHandlers_ShouldCallAllHandlers()
        {
            // Arrange
            var handler1Called = false;
            var handler2Called = false;
            Task AsyncHandler1(object sender, EventArgs e)
            {
                handler1Called = true;
                return Task.CompletedTask;
            }
            Task AsyncHandler2(object sender, EventArgs e)
            {
                handler2Called = true;
                return Task.CompletedTask;
            }

            // Act
            _eventDispatcher.Subscribe("test", AsyncHandler1);
            _eventDispatcher.Subscribe("test", AsyncHandler2);
            await _eventDispatcher.PublishAsync("test", this, EventArgs.Empty);

            // Assert
            Assert.True(handler1Called);
            Assert.True(handler2Called);
        }

        [Fact]
        public void Publish_WithException_ShouldLogErrorAndContinue()
        {
            // Arrange
            var handler2Called = false;
            void ThrowingHandler(object sender, EventArgs e) => throw new Exception("Test exception");
            void Handler2(object sender, EventArgs e) => handler2Called = true;

            // Act
            _eventDispatcher.Subscribe("test", ThrowingHandler);
            _eventDispatcher.Subscribe("test", Handler2);
            _eventDispatcher.Publish("test", this, EventArgs.Empty);

            // Assert
            Assert.True(handler2Called); // Should still be called despite first handler throwing
            _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task PublishAsync_WithException_ShouldLogErrorAndContinue()
        {
            // Arrange
            var handler2Called = false;
            Task ThrowingAsyncHandler(object sender, EventArgs e) => throw new Exception("Test exception");
            Task AsyncHandler2(object sender, EventArgs e)
            {
                handler2Called = true;
                return Task.CompletedTask;
            }

            // Act
            _eventDispatcher.Subscribe("test", ThrowingAsyncHandler);
            _eventDispatcher.Subscribe("test", AsyncHandler2);
            await _eventDispatcher.PublishAsync("test", this, EventArgs.Empty);

            // Assert
            Assert.True(handler2Called); // Should still be called despite first handler throwing
            _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void Subscribe_DuplicateHandler_ShouldNotAddTwice()
        {
            // Arrange
            var handlerCallCount = 0;
            void Handler(object sender, EventArgs e) => handlerCallCount++;

            // Act
            _eventDispatcher.Subscribe("test", Handler);
            _eventDispatcher.Subscribe("test", Handler); // Try to add the same handler again
            _eventDispatcher.Publish("test", this, EventArgs.Empty);

            // Assert
            Assert.Equal(1, handlerCallCount); // Should only be called once
        }

        [Fact]
        public async Task Publish_MixedSyncAndAsyncHandlers_ShouldCallBoth()
        {
            // Arrange
            var syncHandlerCalled = false;
            var asyncHandlerCalled = false;
            
            void SyncHandler(object sender, EventArgs e) => syncHandlerCalled = true;
            Task AsyncHandler(object sender, EventArgs e)
            {
                asyncHandlerCalled = true;
                return Task.CompletedTask;
            }

            // Act
            _eventDispatcher.Subscribe("test", SyncHandler);
            _eventDispatcher.Subscribe("test", AsyncHandler);
            _eventDispatcher.Publish("test", this, EventArgs.Empty);

            // Give async handler time to complete
            await Task.Delay(100);

            // Assert
            Assert.True(syncHandlerCalled);
            Assert.True(asyncHandlerCalled);
        }
    }
}
