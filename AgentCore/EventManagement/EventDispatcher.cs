using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.EventManagement
{
    public class EventDispatcher : IEventDispatcher
    {
        // Define a delegate for event handlers
        public delegate void EventHandler(object sender, EventArgs e);

        // Dictionary to hold events and their subscribers
        private readonly Dictionary<string, List<EventHandler>> _eventHandlers = new Dictionary<string, List<EventHandler>>();
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

            // Unsubscribe all event handlers
            Logger.LogDebug("Unsubscribing all event handlers...");
            foreach (var eventName in _eventHandlers.Keys)
            {
                foreach (var handler in _eventHandlers[eventName])
                {
                    Unsubscribe(eventName, handler);
                }
            }

            Logger.LogInfo("EventDispatcher stopped.");
            return Task.FromResult(true);

        }
        
        // Method to subscribe to an event
        public void Subscribe(string eventName, EventHandler handler)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                Logger.LogDebug($"Creating new event {eventName}");
                _eventHandlers[eventName] = new List<EventHandler>();
            }

            Logger.LogDebug($"Adding subscriber to event {eventName}");
            _eventHandlers[eventName].Add(handler);
        }

        // Method to unsubscribe from an event
        public void Unsubscribe(string eventName, EventHandler handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                Logger.LogDebug($"Removing subscriber from event {eventName}");
                _eventHandlers[eventName].Remove(handler);
                if (_eventHandlers[eventName].Count == 0)
                {
                    _eventHandlers.Remove(eventName);
                }
            } 
            else
            {
                Logger.LogWarning($"Event {eventName} not found!");
            }
        }

        // Method to publish an event
        public void Publish(string eventName, object sender, EventArgs e)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                Logger.LogDebug($"Publishing event {eventName}");
                foreach (var handler in _eventHandlers[eventName])
                {
                    handler(sender, e);
                }
            }
        }
    }
}
