using System;
using System.Threading;
using System.Threading.Tasks;
using AgentCore;
using AgentCommon;
using System.IO;

// Simple test to verify shutdown behavior works correctly
class ShutdownTest
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing graceful shutdown...");
        
        var coreHost = CoreHostFactory.CreateDefault();
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AgentConfig.json");
        
        // Start the core host
        await coreHost.StartAsync(configPath);
        Console.WriteLine("CoreHost started successfully.");
        
        // Wait a few seconds to let background tasks start
        await Task.Delay(3000);
        Console.WriteLine("Initiating shutdown...");
        
        // Test graceful shutdown
        await coreHost.StopAsync();
        Console.WriteLine("CoreHost stopped successfully.");
        
        coreHost.Dispose();
        Console.WriteLine("Test completed - shutdown works correctly!");
    }
}
