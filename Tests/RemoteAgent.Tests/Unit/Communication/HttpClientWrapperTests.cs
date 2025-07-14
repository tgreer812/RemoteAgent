using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AgentCore.CommunicationManagement;
using Xunit;

namespace RemoteAgent.Tests
{
    public class HttpClientWrapperTests : IDisposable
    {
        private readonly HttpClientWrapper _wrapper;
        private readonly HttpClient _httpClient;

        public HttpClientWrapperTests()
        {
            _httpClient = new HttpClient(new TestHttpMessageHandler());
            _wrapper = new HttpClientWrapper(_httpClient);
        }

        [Fact]
        public void Constructor_WithValidHttpClient_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_wrapper);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new HttpClientWrapper(null));
            Assert.Equal("httpClient", exception.ParamName);
        }

        [Fact]
        public async Task GetStringAsync_WithValidUrl_ShouldReturnResponse()
        {
            // Arrange
            var url = "http://test.com/api/test";

            // Act
            var response = await _wrapper.GetStringAsync(url);

            // Assert
            Assert.NotNull(response);
            Assert.Contains("GET", response);
        }

        [Fact]
        public async Task PostAsync_WithValidUrlAndContent_ShouldReturnResponse()
        {
            // Arrange
            var url = "http://test.com/api/test";
            var content = new StringContent("test data", Encoding.UTF8, "application/json");

            // Act
            var response = await _wrapper.PostAsync(url, content);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("POST", responseContent);
            Assert.Contains("test data", responseContent);
        }

        [Fact]
        public async Task PutAsync_WithValidUrlAndContent_ShouldReturnResponse()
        {
            // Arrange
            var url = "http://test.com/api/test";
            var content = new StringContent("test data", Encoding.UTF8, "application/json");

            // Act
            var response = await _wrapper.PutAsync(url, content);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("PUT", responseContent);
            Assert.Contains("test data", responseContent);
        }

        [Fact]
        public async Task Dispose_ShouldDisposeHttpClient()
        {
            // Act
            _wrapper.Dispose();

            // Assert - HttpClient should be disposed, calling it should throw
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _httpClient.GetAsync("http://test.com"));
        }

        public void Dispose()
        {
            _wrapper?.Dispose();
        }

        // Test HTTP message handler for mocking HTTP responses
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var content = string.Empty;
                if (request.Content != null)
                {
                    content = await request.Content.ReadAsStringAsync();
                }

                var responseContent = $"Method: {request.Method}, URL: {request.RequestUri}, Content: {content}";
                
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
