using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AgentCommon;
using AgentCore.JobManagement;
using AgentCore.EventManagement;
using Newtonsoft.Json.Linq;

namespace RemoteAgent.Tests
{
    public class JobManagerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IEventDispatcher> _mockEventDispatcher;
        private readonly JobManager _jobManager;

        public JobManagerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockEventDispatcher = new Mock<IEventDispatcher>();
            _jobManager = new JobManager(_mockLogger.Object, _mockEventDispatcher.Object);
        }

        [Fact]
        public async Task Start_ShouldSetIsRunningToTrue()
        {
            // Act
            await _jobManager.Start();

            // Assert
            Assert.True(_jobManager.IsRunning);
            _mockLogger.Verify(l => l.LogInfo("JobManager is starting..."), Times.Once);
            _mockLogger.Verify(l => l.LogInfo("JobManager started and listening for job events."), Times.Once);
        }

        [Fact]
        public async Task Start_WhenAlreadyRunning_ShouldLogErrorAndReturn()
        {
            // Arrange
            await _jobManager.Start();

            // Act
            await _jobManager.Start(); // Try to start again

            // Assert
            _mockLogger.Verify(l => l.LogError("JobManager is already running!", null), Times.Once);
        }

        [Fact]
        public async Task Stop_ShouldSetIsRunningToFalse()
        {
            // Arrange
            await _jobManager.Start();

            // Act
            var result = await _jobManager.Stop();

            // Assert
            Assert.True(result);
            Assert.False(_jobManager.IsRunning);
            _mockLogger.Verify(l => l.LogInfo("JobManager is stopping..."), Times.Once);
        }

        [Fact]
        public async Task Stop_WhenNotRunning_ShouldStillReturnTrue()
        {
            // Act
            var result = await _jobManager.Stop();

            // Assert
            Assert.True(result);
            Assert.False(_jobManager.IsRunning);
        }
    }

    public class JobTests
    {
        [Fact]
        public void Job_Constructor_ShouldSetProperties()
        {
            // Arrange
            uint jobId = 123;
            string jobType = JobTypes.PluginJobType;
            var jobData = new JObject { ["test"] = "value" };

            // Act
            var job = new Job(jobId, jobType, jobData);

            // Assert
            Assert.Equal(jobId, job.JobId);
            Assert.Equal(jobType, job.JobType);
            Assert.Equal(jobData, job.JobData);
        }
    }

    public class JobConverterTests
    {
        private readonly JobConverter _jobConverter;

        public JobConverterTests()
        {
            _jobConverter = new JobConverter();
        }

        [Fact]
        public void Convert_ValidJsonEventArgs_ShouldReturnJob()
        {
            // Arrange
            var jsonData = @"{
                ""jobId"": ""123"",
                ""jobType"": ""PluginJob"",
                ""jobData"": {
                    ""pluginName"": ""TestPlugin"",
                    ""pluginArguments"": {
                        ""path"": ""C:\\test""
                    }
                }
            }";
            var eventArgs = new EventDispatcher.JsonEventArgs(jsonData);

            // Act
            var job = _jobConverter.Convert(eventArgs);

            // Assert
            Assert.Equal(123u, job.JobId);
            Assert.Equal("PluginJob", job.JobType);
            Assert.NotNull(job.JobData);
            Assert.Equal("TestPlugin", job.JobData["pluginName"].ToString());
        }

        [Fact]
        public void Convert_NullEventArgs_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _jobConverter.Convert(null));
        }

        [Fact]
        public void Convert_NonEventArgs_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _jobConverter.Convert("not an EventArgs"));
        }

        [Fact]
        public void Convert_EventArgsWithoutJsonData_ShouldThrowArgumentException()
        {
            // Arrange
            var eventArgs = new EventArgs();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _jobConverter.Convert(eventArgs));
        }

        [Fact]
        public void Convert_InvalidJson_ShouldThrowArgumentException()
        {
            // Arrange
            var eventArgs = new EventDispatcher.JsonEventArgs("invalid json");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _jobConverter.Convert(eventArgs));
        }

        [Fact]
        public void Convert_JsonMissingRequiredFields_ShouldThrowArgumentException()
        {
            // Arrange
            var jsonData = @"{""someField"": ""value""}"; // Missing jobType and jobData
            var eventArgs = new EventDispatcher.JsonEventArgs(jsonData);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _jobConverter.Convert(eventArgs));
        }
    }

    public class JobTypesTests
    {
        [Fact]
        public void JobTypes_ShouldHaveCorrectConstants()
        {
            // Assert
            Assert.Equal("PluginJob", JobTypes.PluginJobType);
            Assert.Equal("CoreJob", JobTypes.CoreJobType);
        }
    }
}
