using AgentCommon;
using AgentCore.PluginManagement;
using AgentCommon.AgentPluginCommon;
using AgentCore.EventManagement;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static AgentCore.EventManagement.EventDispatcher;

namespace AgentCore.CommunicationManagement
{
    /// <summary>
    /// Manages communication with the remote server using event-driven architecture
    /// </summary>
    public class CommunicationManager : ICoreService, ICommunicationManager, IDisposable
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly ICommunicationConfiguration _config;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IMessageSerializer _messageSerializer;
        private readonly ILogger _logger;
        private readonly AgentConfig _agentConfig;
        
        private bool _isRunning;
        private bool _disposed;

        public bool IsRunning => _isRunning;

        internal CommunicationManager(ILogger logger) 
            : this(logger, new HttpClientWrapper(), new DefaultCommunicationConfiguration(), null, new JsonMessageSerializer(logger), null)
        {
        }

        internal CommunicationManager(
            ILogger logger,
            IHttpClientWrapper httpClient,
            ICommunicationConfiguration config,
            IEventDispatcher eventDispatcher,
            IMessageSerializer messageSerializer,
            AgentConfig agentConfig = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _eventDispatcher = eventDispatcher;
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
            _agentConfig = agentConfig;
        }

        public Task Start()
        {
            if (_isRunning)
            {
                _logger.LogWarning("Communication manager is already running");
                return Task.CompletedTask;
            }

            _logger.LogInfo("Communication manager is starting...");
            _isRunning = true;

            try
            {
                // Subscribe to events for communication triggers first
                SubscribeToEvents();

                // Start handshake in background - don't block startup
                _ = Task.Run(async () =>
                {
                    try
                    {
                        bool handshakeSuccessful = await PerformHandshakeWithRetry();
                        if (handshakeSuccessful)
                        {
                            _logger.LogInfo("Background handshake completed successfully");
                        }
                        else
                        {
                            _logger.LogWarning("Background handshake failed after all retries");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error in background handshake", ex);
                    }
                });

                _logger.LogInfo("Communication manager started successfully");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error starting communication manager", ex);
                _isRunning = false;
                throw;
            }
        }

        public Task<bool> Stop()
        {
            if (!_isRunning)
            {
                _logger.LogInfo("Communication manager is already stopped");
                return Task.FromResult(true);
            }

            _logger.LogInfo("Communication manager is stopping...");
            
            try
            {
                // Unsubscribe from events
                UnsubscribeFromEvents();
                
                _isRunning = false;
                _logger.LogInfo("Communication manager stopped successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error stopping communication manager", ex);
                return Task.FromResult(false);
            }
        }

        public async Task RequestTaskingAsync()
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Cannot request tasking - communication manager is not running");
                return;
            }

            if (!_config.AgentId.HasValue)
            {
                _logger.LogWarning("Cannot request tasking - agent ID not set");
                return;
            }

            try
            {
                _logger.LogDebug("Requesting tasking from server...");
                
                string taskingUrl = $"{_config.ServerAddress}/{_config.TaskingEndpoint}/{_config.AgentId.Value}";
                string response = await _httpClient.GetStringAsync(taskingUrl);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogDebug("No tasking response received");
                    return;
                }

                ProcessTaskingResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error requesting tasking from server", ex);
            }
        }

        public async Task<bool> SendPluginResultAsync(uint correlationId, object result)
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Cannot send plugin result - communication manager is not running");
                return false;
            }

            try
            {
                _logger.LogDebug($"Sending plugin result for correlation ID: {correlationId}");
                
                string jsonPayload = JsonSerializer.Serialize(result);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                string resultUrl = $"{_config.ServerAddress}/{_config.JobEndpoint}/{correlationId}";
                var response = await _httpClient.PutAsync(resultUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug($"Plugin result sent successfully for correlation ID: {correlationId}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to send plugin result. HTTP Status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending plugin result for correlation ID: {correlationId}", ex);
                return false;
            }
        }

        private async Task<bool> PerformHandshakeWithRetry()
        {
            int retryCount = 0;
            
            while (retryCount <= _config.MaxHandshakeRetries && _isRunning)
            {
                retryCount++;
                _logger.LogInfo($"Attempting handshake with server (Attempt #{retryCount})...");

                try
                {
                    bool success = await SendHelloMessageAsync();
                    if (success)
                    {
                        _logger.LogInfo("Handshake completed successfully");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Handshake attempt #{retryCount} failed", ex);
                }

                if (retryCount <= _config.MaxHandshakeRetries)
                {
                    _logger.LogWarning($"Handshake failed (Attempt #{retryCount}). Retrying in {_config.HandshakeRetryDelay.TotalSeconds} seconds...");
                    await Task.Delay(_config.HandshakeRetryDelay);
                }
            }

            _logger.LogError($"Handshake failed after {_config.MaxHandshakeRetries} attempts");
            return false;
        }

        private async Task<bool> SendHelloMessageAsync()
        {
            _logger.LogInfo("Sending agent hello message to server...");

            var helloPayload = new
            {
                agentGuid = _config.AgentGuid
            };

            string jsonPayload = JsonSerializer.Serialize(helloPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                string helloUrl = $"{_config.ServerAddress}/{_config.AgentHelloEndpoint}";
                var response = await _httpClient.PostAsync(helloUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return ProcessHelloResponse(responseContent);
                }
                else
                {
                    _logger.LogError($"Hello handshake failed. HTTP Status: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Server error response: {errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP error during hello handshake: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error during hello handshake", ex);
                return false;
            }
        }

        private bool ProcessHelloResponse(string responseContent)
        {
            try
            {
                var helloResponseJson = JsonDocument.Parse(responseContent);
                JsonElement root = helloResponseJson.RootElement;

                if (root.TryGetProperty("agentId", out JsonElement agentIdElement) && 
                    agentIdElement.TryGetInt32(out int agentId))
                {
                    _config.AgentId = agentId;
                    
                    // Also update the AgentConfig if available
                    if (_agentConfig != null)
                    {
                        _agentConfig.AgentId = agentId;
                    }
                    
                    _logger.LogInfo($"Agent handshake successful. Assigned AgentId: {agentId}");
                    return true;
                }
                else
                {
                    _logger.LogError("Server response did not contain valid 'agentId'");
                    _logger.LogDebug($"Server response: {responseContent}");
                    return false;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError("Error parsing hello response JSON", ex);
                _logger.LogDebug($"Server response: {responseContent}");
                return false;
            }
        }

        private void ProcessTaskingResponse(string response)
        {
            try
            {
                var json = JObject.Parse(response);
                var jobs = json["jobs"];

                if (jobs != null)
                {
                    var jobCount = 0;
                    foreach (var job in jobs)
                    {
                        var args = new JsonEventArgs(job.ToString());
                        _eventDispatcher?.Publish("JobAdded", this, args);
                        jobCount++;
                    }
                    
                    _logger.LogDebug($"Processed {jobCount} jobs from tasking response");
                }
                else
                {
                    _logger.LogDebug("No jobs found in tasking response");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing tasking response", ex);
            }
        }

        private void SubscribeToEvents()
        {
            try
            {
                // Subscribe to plugin completion events
                _eventDispatcher?.Subscribe("PluginCompleted", OnPluginCompleted);
                
                // Subscribe to tasking request events
                _eventDispatcher?.Subscribe("RequestTasking", OnTaskingRequested);
                
                _logger.LogDebug("Subscribed to communication events");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error subscribing to events", ex);
            }
        }

        private void UnsubscribeFromEvents()
        {
            try
            {
                _eventDispatcher?.Unsubscribe("PluginCompleted", OnPluginCompleted);
                _eventDispatcher?.Unsubscribe("RequestTasking", OnTaskingRequested);
                
                _logger.LogDebug("Unsubscribed from communication events");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error unsubscribing from events", ex);
            }
        }

        private void OnPluginCompleted(object sender, EventArgs e)
        {
            try
            {
                if (e is PluginCompletedEventArgs pluginArgs)
                {
                    var result = pluginArgs.Result;
                    _ = Task.Run(async () => await SendPluginResultAsync(result.CorrelationId, result));
                }
                else
                {
                    _logger.LogError("Invalid event args type for PluginCompleted event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error handling PluginCompleted event", ex);
            }
        }

        private void OnTaskingRequested(object sender, EventArgs e)
        {
            try
            {
                _ = Task.Run(async () => await RequestTaskingAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error handling RequestTasking event", ex);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_isRunning)
                {
                    _ = Task.Run(async () => await Stop());
                }
                
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
