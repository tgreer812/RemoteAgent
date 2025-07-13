using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgentCore.CommunicationManagement
{
    /// <summary>
    /// Abstraction for HTTP client operations to enable testing
    /// </summary>
    public interface IHttpClientWrapper : IDisposable
    {
        /// <summary>
        /// Sends a GET request to the specified URI
        /// </summary>
        /// <param name="requestUri">The URI to send the request to</param>
        /// <returns>Task with the response content as string</returns>
        Task<string> GetStringAsync(string requestUri);

        /// <summary>
        /// Sends a POST request to the specified URI
        /// </summary>
        /// <param name="requestUri">The URI to send the request to</param>
        /// <param name="content">The HTTP content to send</param>
        /// <returns>Task with the HTTP response message</returns>
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);

        /// <summary>
        /// Sends a PUT request to the specified URI
        /// </summary>
        /// <param name="requestUri">The URI to send the request to</param>
        /// <param name="content">The HTTP content to send</param>
        /// <returns>Task with the HTTP response message</returns>
        Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content);
    }
}
