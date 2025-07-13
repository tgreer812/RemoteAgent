using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AgentCommon.AgentPluginCommon
{
    public class PluginArguments
    {
        private readonly Dictionary<string, object> _arguments;

        public PluginArguments()
        {
            _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public PluginArguments(Dictionary<string, object> arguments)
        {
            _arguments = new Dictionary<string, object>(arguments, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates PluginArguments from a JSON string
        /// </summary>
        public static PluginArguments FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new PluginArguments();

            var jObject = JObject.Parse(json);
            var arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var property in jObject.Properties())
            {
                arguments[property.Name] = property.Value.ToObject<object>();
            }
            
            return new PluginArguments(arguments);
        }

        /// <summary>
        /// Creates PluginArguments from a JObject (for backward compatibility)
        /// </summary>
        public static PluginArguments FromJObject(JObject jObject)
        {
            if (jObject == null)
                return new PluginArguments();

            var arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var property in jObject.Properties())
            {
                arguments[property.Name] = property.Value.ToObject<object>();
            }
            
            return new PluginArguments(arguments);
        }

        public T GetArgument<T>(string key, T defaultValue = default)
        {
            if (_arguments.TryGetValue(key, out var value))
            {
                try
                {
                    // Handle direct type match
                    if (value is T directMatch)
                        return directMatch;

                    // Handle conversion through JSON for complex types
                    if (typeof(T).IsClass && typeof(T) != typeof(string))
                    {
                        var json = JsonConvert.SerializeObject(value);
                        return JsonConvert.DeserializeObject<T>(json);
                    }

                    // Handle primitive type conversion
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception)
                {
                    // Log or handle the exception as needed
                }
            }

            return defaultValue;
        }

        public void AddArgument<T>(string key, T value)
        {
            _arguments[key] = value;
        }

        public bool HasArgument(string key)
        {
            return _arguments.ContainsKey(key);
        }

        public bool TryGetArgument<T>(string key, out T value)
        {
            value = default;
            
            if (_arguments.TryGetValue(key, out var objValue))
            {
                try
                {
                    value = GetArgument<T>(key);
                    return true;
                }
                catch (Exception)
                {
                    // Conversion failed
                }
            }

            return false;
        }

        public void RemoveArgument(string key)
        {
            _arguments.Remove(key);
        }

        public IEnumerable<string> GetArgumentKeys()
        {
            return _arguments.Keys;
        }

        /// <summary>
        /// Serializes the arguments to a JSON string
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(_arguments, Formatting.None);
        }

        /// <summary>
        /// Serializes the arguments to a formatted JSON string
        /// </summary>
        public string ToJson(Formatting formatting)
        {
            return JsonConvert.SerializeObject(_arguments, formatting);
        }

        /// <summary>
        /// Converts to JObject for backward compatibility
        /// </summary>
        public JObject ToJObject()
        {
            var json = ToJson();
            return JObject.Parse(json);
        }
    }
}
