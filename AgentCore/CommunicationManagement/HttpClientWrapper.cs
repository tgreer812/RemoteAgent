using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgentCore.CommunicationManagement
{
    /// <summary>
    /// Default implementation of IHttpClientWrapper using HttpClient
    /// </summary>
    internal class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public HttpClientWrapper() : this(new HttpClient())
        {
        }

        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetStringAsync(string requestUri)
        {
            ThrowIfDisposed();
            return await _httpClient.GetStringAsync(requestUri);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            ThrowIfDisposed();
            return await _httpClient.PostAsync(requestUri, content);
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            ThrowIfDisposed();
            return await _httpClient.PutAsync(requestUri, content);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HttpClientWrapper));
            }
        }
    }
}
