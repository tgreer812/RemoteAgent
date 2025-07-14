using System.Net;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace TestServer;

/// <summary>
/// Lightweight C# HTTP server for testing RemoteAgent without Python dependency
/// </summary>
public class Program
{
    private static readonly ConcurrentDictionary<int, AgentInfo> Agents = new();
    private static readonly ConcurrentDictionary<int, List<JobInfo>> PendingJobs = new();
    private static readonly ConcurrentDictionary<string, object> CompletedJobs = new();
    private static int _nextAgentId = 1;
    private static int _nextJobId = 1;
    private static HttpListener? _listener;
    
    public static async Task Main(string[] args)
    {
        var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 5148;
        var url = $"http://localhost:{port}/";
        
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);
        
        Console.WriteLine($"Starting test C2 server on {url}");
        Console.WriteLine("Endpoints:");
        Console.WriteLine("  POST /api/agent/hello - Agent registration");
        Console.WriteLine("  GET  /api/tasking/{agentId} - Get jobs for agent");
        Console.WriteLine("  POST /api/job - Receive job results");
        Console.WriteLine("  GET  /api/test - Health check");
        Console.WriteLine("  GET  /api/logs - Server state for testing");
        Console.WriteLine("\nPress Ctrl+C to stop the server");
        
        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\nShutting down server...");
            _listener?.Stop();
        };
        
        try
        {
            _listener.Start();
            await ProcessRequestsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
        finally
        {
            _listener?.Close();
            Console.WriteLine("Server stopped.");
        }
    }
    
    private static async Task ProcessRequestsAsync()
    {
        while (_listener?.IsListening == true)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (HttpListenerException)
            {
                // Listener was stopped
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
        }
    }
    
    private static async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        try
        {
            Console.WriteLine($"{request.HttpMethod} {request.Url?.PathAndQuery}");
            
            var path = request.Url?.AbsolutePath ?? "";
            
            switch (request.HttpMethod?.ToUpper())
            {
                case "POST" when path == "/api/agent/hello":
                    await HandleAgentHelloAsync(request, response);
                    break;
                    
                case "POST" when path == "/api/job":
                    await HandleJobResultAsync(request, response);
                    break;

                case "PUT" when path.StartsWith("/api/job/"):
                    // Handle PUT /api/job/{correlationId} - this is what the agent actually sends
                    var correlationIdStr = path.Split('/').LastOrDefault();
                    if (uint.TryParse(correlationIdStr, out var correlationId))
                    {
                        await HandleJobResultAsync(request, response, correlationId);
                    }
                    else
                    {
                        await SendErrorResponseAsync(response, 400, "Invalid correlation ID");
                    }
                    break;
                    
                case "GET" when path.StartsWith("/api/tasking/"):
                    var agentIdStr = path.Split('/').LastOrDefault();
                    if (int.TryParse(agentIdStr, out var agentId))
                    {
                        await HandleTaskingRequestAsync(agentId, response);
                    }
                    else
                    {
                        await SendErrorResponseAsync(response, 400, "Invalid agent ID");
                    }
                    break;
                    
                case "GET" when path == "/api/test":
                    await HandleTestEndpointAsync(response);
                    break;
                    
                case "GET" when path == "/api/logs":
                    await HandleLogsEndpointAsync(response);
                    break;
                    
                default:
                    await SendErrorResponseAsync(response, 404, "Not Found");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling request: {ex.Message}");
            await SendErrorResponseAsync(response, 500, ex.Message);
        }
        finally
        {
            response.Close();
        }
    }
    
    private static async Task HandleAgentHelloAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        try
        {
            var postData = await ReadRequestBodyAsync(request);
            
            // Assign new agent ID
            var agentId = Interlocked.Increment(ref _nextAgentId) - 1;
            
            // Store agent info
            var agentInfo = new AgentInfo
            {
                RegisteredAt = DateTime.UtcNow,
                Data = postData
            };
            Agents[agentId] = agentInfo;
            
            // Create a DirectoryListPlugin job for this agent
            var jobId = Interlocked.Increment(ref _nextJobId) - 1;
            
            // Use the well-known test directory for integration tests
            var testDirectory = Path.Combine(Path.GetTempPath(), "RemoteAgentE2ETest");
            
            var job = new JobInfo
            {
                JobId = jobId,
                JobType = "PluginJob", // Must be "PluginJob" for plugin jobs
                JobData = new Dictionary<string, object>
                {
                    ["pluginName"] = "DirectoryListPlugin",
                    ["pluginArguments"] = new Dictionary<string, object>
                    {
                        ["Path"] = testDirectory
                    }
                }
            };
            
            PendingJobs[agentId] = new List<JobInfo> { job };
            
            var responseObj = new { agentId };
            
            Console.WriteLine($"Registered new agent with ID: {agentId}");
            Console.WriteLine($"Created DirectoryList job {jobId} for agent {agentId}");
            
            await SendJsonResponseAsync(response, 200, responseObj);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in agent hello: {ex.Message}");
            await SendErrorResponseAsync(response, 500, ex.Message);
        }
    }
    
    private static async Task HandleTaskingRequestAsync(int agentId, HttpListenerResponse response)
    {
        try
        {
            if (!Agents.ContainsKey(agentId))
            {
                await SendErrorResponseAsync(response, 404, "Agent not found");
                return;
            }
            
            // Get pending jobs for this agent
            var jobs = PendingJobs.TryGetValue(agentId, out var pendingJobs) ? pendingJobs : new List<JobInfo>();
            
            if (jobs.Count > 0)
            {
                var responseObj = new { jobs };
                Console.WriteLine($"Sending {jobs.Count} job(s) to agent {agentId}");
                // Clear pending jobs after sending
                PendingJobs[agentId] = new List<JobInfo>();
            }
            else
            {
                var responseObj = new { jobs = new List<JobInfo>() };
                Console.WriteLine($"No pending jobs for agent {agentId}");
            }
            
            var finalResponse = new { jobs };
            await SendJsonResponseAsync(response, 200, finalResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in tasking request: {ex.Message}");
            await SendErrorResponseAsync(response, 500, ex.Message);
        }
    }
    
    private static async Task HandleJobResultAsync(HttpListenerRequest request, HttpListenerResponse response, uint? correlationId = null)
    {
        try
        {
            var postData = await ReadRequestBodyAsync(request);
            
            Console.WriteLine("=== JOB RESULT RECEIVED ===");
            Console.WriteLine($"Correlation ID from URL: {correlationId}");
            Console.WriteLine($"Data: {postData}");
            Console.WriteLine("===========================");
            
            object? resultData = null;
            if (!string.IsNullOrEmpty(postData))
            {
                resultData = JsonSerializer.Deserialize<object>(postData);
            }
            
            // Store the result using the correlation ID from URL if available
            var jobId = correlationId?.ToString() ?? "unknown";
            if (resultData is JsonElement element && element.TryGetProperty("JobId", out var jobIdProp))
            {
                jobId = jobIdProp.ToString();
            }
            
            CompletedJobs[jobId] = resultData;
            Console.WriteLine($"Stored completed job with ID: {jobId}");
            
            var responseObj = new { status = "success" };
            await SendJsonResponseAsync(response, 200, responseObj);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in job result: {ex.Message}");
            await SendErrorResponseAsync(response, 500, ex.Message);
        }
    }
    
    private static async Task HandleTestEndpointAsync(HttpListenerResponse response)
    {
        try
        {
            var responseObj = new
            {
                status = "ok",
                agents = Agents.Count,
                pending_jobs = PendingJobs.Values.Sum(jobs => jobs.Count),
                completed_jobs = CompletedJobs.Count
            };
            await SendJsonResponseAsync(response, 200, responseObj);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in test endpoint: {ex.Message}");
            await SendErrorResponseAsync(response, 500, ex.Message);
        }
    }
    
    private static async Task HandleLogsEndpointAsync(HttpListenerResponse response)
    {
        try
        {
            var responseObj = new
            {
                agents = Agents,
                pending_jobs = PendingJobs,
                completed_jobs = CompletedJobs
            };
            await SendJsonResponseAsync(response, 200, responseObj);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in logs endpoint: {ex.Message}");
            await SendErrorResponseAsync(response, 500, ex.Message);
        }
    }
    
    private static async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
    {
        if (request.ContentLength64 <= 0) return string.Empty;
        
        using var reader = new StreamReader(request.InputStream);
        return await reader.ReadToEndAsync();
    }
    
    private static async Task SendJsonResponseAsync(HttpListenerResponse response, int statusCode, object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        response.ContentLength64 = Encoding.UTF8.GetByteCount(json);
        
        await using var output = response.OutputStream;
        await output.WriteAsync(Encoding.UTF8.GetBytes(json));
    }
    
    private static async Task SendErrorResponseAsync(HttpListenerResponse response, int statusCode, string message)
    {
        var errorObj = new { error = message };
        await SendJsonResponseAsync(response, statusCode, errorObj);
    }
}

public class AgentInfo
{
    public DateTime RegisteredAt { get; set; }
    public string Data { get; set; } = string.Empty;
}

public class JobInfo
{
    public int JobId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public Dictionary<string, object> JobData { get; set; } = new();
}
