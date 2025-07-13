using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AgentCore;
using AgentCommon;

namespace ConsoleStart
{
    internal class Program
    {
        private static CoreHost? _coreHost;
        private static readonly ManualResetEventSlim _shutdownEvent = new ManualResetEventSlim(false);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Remote Agent Console Host - Starting...");
            
            // Setup graceful shutdown
            Console.CancelKeyPress += OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            try
            {
                // Create the core host using the new factory
                _coreHost = CoreHostFactory.CreateDefault();
                
                // Start the core host - look for config in the executable directory
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AgentConfig.json");
                await _coreHost.StartAsync(configPath);
                
                Console.WriteLine("Remote Agent is running. Press Ctrl+C to stop.");
                
                // Wait for shutdown signal
                _shutdownEvent.Wait();
                
                Console.WriteLine("Shutdown signal received. Stopping...");
                
                // Stop the core host
                if (_coreHost != null)
                {
                    await _coreHost.StopAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                // Final cleanup - dispose the host
                if (_coreHost != null)
                {
                    try
                    {
                        _coreHost.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during cleanup: {ex.Message}");
                    }
                }
                
                Console.WriteLine("Remote Agent stopped.");
            }
        }

        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // Prevent immediate termination
            Console.WriteLine("\nGraceful shutdown requested...");
            
            // Simply signal shutdown - let the main method handle StopAsync
            _shutdownEvent.Set();
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            _shutdownEvent.Set();
        }
    }
}
