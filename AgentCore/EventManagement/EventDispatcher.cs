using AgentCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.EventManagement
{
    [LoadPriority(1)]
    public class EventDispatcher : ICoreService, IEventDispatcher
    {
        // Thread-safe dictionaries for event handlers
        private readonly ConcurrentDictionary<string, List<EventHandler>> _syncEventHandlers = new ConcurrentDictionary<string, List<EventHandler>>();
        private readonly ConcurrentDictionary<string, List<Func<object, EventArgs, Task>>> _asyncEventHandlers = new ConcurrentDictionary<string, List<Func<object, EventArgs, Task>>>();
        private readonly object _lockObject = new object();
        
        private ILogger Logger { get; set; }
        
        internal EventDispatcher(ILogger logger)
        {
            Logger = logger;
        }

        public Task Start()
        {
            Logger.LogInfo("EventDispatcher is starting...");
            Logger.LogInfo("EventDispatcher started.");
            return Task.CompletedTask;
        }

        public Task<bool> Stop()
        {
            Logger.LogInfo("EventDispatcher is stopping...");

            // Clear all event handlers
            Logger.LogDebug("Clearing all event handlers...");
            _syncEventHandlers.Clear();
            _asyncEventHandlers.Clear();

            Logger.LogInfo("EventDispatcher stopped.");
            return Task.FromResult(true);
        }
        
        // Method to subscribe to a synchronous event
        public void Subscribe(string eventName, EventHandler handler)
        {
            lock (_lockObject)
            {
                var handlers = _syncEventHandlers.GetOrAdd(eventName, _ => new List<EventHandler>());
                if (!handlers.Contains(handler))
                {
                    Logger.LogDebug($"Adding sync subscriber to event {eventName}");
                    handlers.Add(handler);
                }
            }
        }

        // Method to subscribe to an asynchronous event
        public void Subscribe(string eventName, Func<object, EventArgs, Task> asyncHandler)
        {
            lock (_lockObject)
            {
                var handlers = _asyncEventHandlers.GetOrAdd(eventName, _ => new List<Func<object, EventArgs, Task>>());
                if (!handlers.Contains(asyncHandler))
                {
                    Logger.LogDebug($"Adding async subscriber to event {eventName}");
                    handlers.Add(asyncHandler);
                }
            }
        }

        // Method to unsubscribe from a synchronous event
        public void Unsubscribe(string eventName, EventHandler handler)
        {
            lock (_lockObject)
            {
                if (_syncEventHandlers.TryGetValue(eventName, out var handlers))
                {
                    Logger.LogDebug($"Removing sync subscriber from event {eventName}");
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                    {
                        _syncEventHandlers.TryRemove(eventName, out _);
                    }
                }
                else
                {
                    Logger.LogWarning($"Sync event {eventName} not found!");
                }
            }
        }

        // Method to unsubscribe from an asynchronous event
        public void Unsubscribe(string eventName, Func<object, EventArgs, Task> asyncHandler)
        {
            lock (_lockObject)
            {
                if (_asyncEventHandlers.TryGetValue(eventName, out var handlers))
                {
                    Logger.LogDebug($"Removing async subscriber from event {eventName}");
                    handlers.Remove(asyncHandler);
                    if (handlers.Count == 0)
                    {
                        _asyncEventHandlers.TryRemove(eventName, out _);
                    }
                }
                else
                {
                    Logger.LogWarning($"Async event {eventName} not found!");
                }
            }
        }

        // Method to publish an event synchronously
        public void Publish(string eventName, object sender, EventArgs e)
        {
            Logger.LogDebug($"Publishing sync event {eventName}");

            // Call synchronous handlers
            if (_syncEventHandlers.TryGetValue(eventName, out var syncHandlers))
            {
                List<EventHandler> handlersCopy;
                lock (_lockObject)
                {
                    handlersCopy = new List<EventHandler>(syncHandlers);
                }

                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        handler(sender, e);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error in sync event handler for {eventName}", ex);
                    }
                }
            }

            // Fire async handlers without awaiting (fire and forget for sync publish)
            if (_asyncEventHandlers.TryGetValue(eventName, out var asyncHandlers))
            {
                List<Func<object, EventArgs, Task>> asyncHandlersCopy;
                lock (_lockObject)
                {
                    asyncHandlersCopy = new List<Func<object, EventArgs, Task>>(asyncHandlers);
                }

                foreach (var asyncHandler in asyncHandlersCopy)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await asyncHandler(sender, e);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error in async event handler for {eventName}", ex);
                        }
                    });
                }
            }
        }

        // Method to publish an event asynchronously
        public async Task PublishAsync(string eventName, object sender, EventArgs e)
        {
            Logger.LogDebug($"Publishing async event {eventName}");

            var tasks = new List<Task>();

            // Call synchronous handlers
            if (_syncEventHandlers.TryGetValue(eventName, out var syncHandlers))
            {
                List<EventHandler> handlersCopy;
                lock (_lockObject)
                {
                    handlersCopy = new List<EventHandler>(syncHandlers);
                }

                foreach (var handler in handlersCopy)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            handler(sender, e);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error in sync event handler for {eventName}", ex);
                        }
                    }));
                }
            }

            // Call asynchronous handlers
            if (_asyncEventHandlers.TryGetValue(eventName, out var asyncHandlers))
            {
                List<Func<object, EventArgs, Task>> asyncHandlersCopy;
                lock (_lockObject)
                {
                    asyncHandlersCopy = new List<Func<object, EventArgs, Task>>(asyncHandlers);
                }

                foreach (var asyncHandler in asyncHandlersCopy)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await asyncHandler(sender, e);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error in async event handler for {eventName}", ex);
                        }
                    }));
                }
            }

            // Wait for all handlers to complete
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// A custom EventArgs class for JSON events
        /// </summary>
        public class JsonEventArgs : EventArgs
        {
            public string JsonData { get; set; }

            public JsonEventArgs(string jsonData)
            {
                JsonData = jsonData;
            }
        }
    }
}
